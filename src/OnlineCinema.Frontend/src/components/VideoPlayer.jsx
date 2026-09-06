import { useEffect, useRef, useContext } from 'react';
import Plyr from 'plyr';
import 'plyr/dist/plyr.css';
import { api } from '../api/axios';
import { AuthContext } from '../context/AuthContext';

// Обёртка над Plyr. Юзает presigned URL для стрима, шлёт прогресс на бэкенд.
//
// Один смысловой «просмотр» на один контент в рамках монтирования плеера:
// срабатывает ТОЛЬКО ОДИН раз — когда зритель либо реально досмотрел
// (конец или >=85%), либо честно посмотрел MEANINGFUL_SECONDS подряд.
// Никаких запросов «на каждую секунду»: максимум один POST /Watch/report.
//
// Считаются ВСЕ зрители, включая гостей без аккаунта: их различает viewerKey
// (устойчивый id браузера в localStorage). У залогиненных ключ тоже шлётся —
// он нужен для дедупа на бэкенде.
const MEANINGFUL_SECONDS = 180;
const COMPLETION_RATIO = 0.85;
// Хартбит прогресса — только для «продолжить просмотр», не для аналитики.
const PROGRESS_INTERVAL_MS = 10000;

// Устойчивый id браузера для анонимной аналитики. Один на вкладку-вкладки,
// живёт в localStorage; у гостей — единственный способ их посчитать.
const getViewerKey = () => {
  try {
    let key = localStorage.getItem('viewerKey');
    if (!key) {
      key = (crypto?.randomUUID && crypto.randomUUID())
        || `${Date.now().toString(36)}-${Math.random().toString(36).slice(2, 14)}`;
      localStorage.setItem('viewerKey', key);
    }
    return key;
  } catch {
    return null;
  }
};

export const VideoPlayer = ({ streamUrl, movieId, episodeId, initialPosition = 0, onEnded }) => {
  const videoRef = useRef(null);
  const playerRef = useRef(null);
  const { user } = useContext(AuthContext);
  const lastSent = useRef(0);
  // Монотонный максимум просмотренного (перемотка назад не уменьшает).
  const maxTime = useRef(0);
  // Один смысловой «просмотр» на один контент в рамках этого монтирования.
  const watchReported = useRef(false);
  // Свежие ссылки без пересоздания плеера (иначе сбросится воспроизведение).
  // Обновляем в эффекте, а не в теле рендера — иначе React 19 ругается.
  const userRef = useRef(user);
  const onEndedRef = useRef(onEnded);
  const initialPositionRef = useRef(initialPosition);
  useEffect(() => {
    userRef.current = user;
    onEndedRef.current = onEnded;
    initialPositionRef.current = initialPosition;
  });

  useEffect(() => {
    if (!streamUrl || !videoRef.current) return;

    // Новый контент — новые счётчики.
    lastSent.current = 0;
    maxTime.current = 0;
    watchReported.current = false;

    // Plyr — лишь обвязка контролов; само видео — родной <video>.
    // Если Plyr не завёлся, слушатели ниже всё равно работают на нативе.
    let player = null;
    try {
      player = new Plyr(videoRef.current, {
        controls: ['play-large', 'play', 'progress', 'current-time', 'duration', 'mute', 'volume', 'settings', 'pip', 'fullscreen'],
        settings: ['speed'],
        speed: { selected: 1, options: [0.5, 0.75, 1, 1.25, 1.5, 2] },
      });
    } catch (e) {
      console.warn('[player] plyr init failed, falling back to native video', e);
    }
    playerRef.current = player;

    const v = videoRef.current;

    // Если метаданные уже готовы — ставим позицию сразу.
    const applyInitialPosition = () => {
      const pos = initialPositionRef.current;
      if (pos > 0) {
        try {
          if (v.currentTime < pos - 1 || v.currentTime > pos + 1) v.currentTime = pos;
        } catch {
          // длительность ещё неизвестна — повторим на loadedmetadata
        }
      }
    };
    const onMeta = () => applyInitialPosition();
    applyInitialPosition();

    const sendProgress = () => {
      if (!userRef.current) return;
      const now = Date.now();
      // не спамим, шлём раз в PROGRESS_INTERVAL_MS (только для "продолжить просмотр")
      if (now - lastSent.current < PROGRESS_INTERVAL_MS) return;
      lastSent.current = now;

      api.post('/Playback/progress', {
        movieId: movieId || null,
        episodeId: episodeId || null,
        positionSeconds: v.currentTime,
        durationSeconds: Number.isFinite(v.duration) ? v.duration : 0,
      }).catch(() => {
        // резюм — не критично, молча пропускаем
      });
    };

    // Одиночный, «смысловой» эвент реального просмотра: шлём ровно один раз
    // за контент — и для залогиненных, и для гостей (viewerKey в помощь).
    // Ошибки НЕ глотаем молча — пишем в консоль, чтобы было видно,
    // если отчёты не долетают.
    const reportWatch = () => {
      if (watchReported.current) return;
      watchReported.current = true;
      api.post('/Watch/report', {
        movieId: movieId || null,
        episodeId: episodeId || null,
        watchedSeconds: maxTime.current || v.currentTime || 0,
        viewerKey: getViewerKey(),
      }).catch((err) => {
        const status = err?.response?.status;
        console.warn(`[watch] report failed${status ? ` (HTTP ${status})` : ''}`, err?.message || err);
      });
    };

    const maybeReport = (ended) => {
      if (watchReported.current) return;
      // Досмотрел до конца — это точно состоявшийся просмотр.
      if (ended) {
        reportWatch();
        return;
      }
      // Честно посмотрел несколько минут подряд — уже живой интерес,
      // даже если до конца фильма далеко.
      if (maxTime.current >= MEANINGFUL_SECONDS) {
        reportWatch();
        return;
      }
      // Пересёк порог досмотра (~85% от известной длительности).
      const dur = Number(v.duration) || 0;
      if (Number.isFinite(dur) && dur > 0 && v.currentTime >= dur * COMPLETION_RATIO) {
        reportWatch();
      }
    };

    const onTime = () => {
      sendProgress();
      if (v.currentTime > maxTime.current) maxTime.current = v.currentTime;
      maybeReport(false);
    };
    const onEnd = () => {
      if (userRef.current) {
        api.post('/Playback/progress', {
          movieId: movieId || null,
          episodeId: episodeId || null,
          positionSeconds: Number.isFinite(v.duration) ? v.duration : maxTime.current,
          durationSeconds: Number.isFinite(v.duration) ? v.duration : 0,
        }).catch(() => {
          // резюм — не критично
        });
      }
      maybeReport(true);
      if (onEndedRef.current) onEndedRef.current();
    };

    v.addEventListener('timeupdate', onTime);
    v.addEventListener('ended', onEnd);
    v.addEventListener('loadedmetadata', onMeta);

    return () => {
      v.removeEventListener('timeupdate', onTime);
      v.removeEventListener('ended', onEnd);
      v.removeEventListener('loadedmetadata', onMeta);
      if (player) {
        try {
          player.destroy();
        } catch {
          // уже уничтожен — не критично
        }
      }
      playerRef.current = null;
    };
  }, [streamUrl]);

  if (!streamUrl) return <div className="text-white p-4">Видео не загружено</div>;

  return (
    <video ref={videoRef} className="w-full" playsInline controls preload="metadata">
      <source src={streamUrl} type="video/mp4" />
    </video>
  );
};
