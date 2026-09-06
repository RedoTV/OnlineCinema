import { useEffect, useRef, useContext } from 'react';
import Plyr from 'plyr';
import 'plyr/dist/plyr.css';
import { api } from '../api/axios';
import { AuthContext } from '../context/AuthContext';

// Обёртка над Plyr. Юзает presigned URL для стрима, шлёт прогресс на бэкенд.
export const VideoPlayer = ({ streamUrl, movieId, episodeId, initialPosition = 0, onEnded }) => {
  const videoRef = useRef(null);
  const playerRef = useRef(null);
  const { user } = useContext(AuthContext);
  const lastSent = useRef(0);
  // Один смысловой «просмотр» на один контент в рамках этой вкладки.
  const watchReported = useRef(false);

  useEffect(() => {
    if (!streamUrl || !videoRef.current) return;

    const player = new Plyr(videoRef.current, {
      controls: ['play-large', 'play', 'progress', 'current-time', 'duration', 'mute', 'volume', 'settings', 'pip', 'fullscreen'],
      settings: ['speed'],
      speed: { selected: 1, options: [0.5, 0.75, 1, 1.25, 1.5, 2] },
    });
    playerRef.current = player;

    // Plyr оборачивает video, юзаем его родной элемент для событий
    const v = videoRef.current;
    if (initialPosition > 0) {
      v.currentTime = initialPosition;
    }

    const sendProgress = () => {
      if (!user) return;
      const now = Date.now();
      // не спамим, шлём раз в 10 сек (только для "продолжить просмотр")
      if (now - lastSent.current < 10000) return;
      lastSent.current = now;

      api.post('/Playback/progress', {
        movieId: movieId || null,
        episodeId: episodeId || null,
        positionSeconds: v.currentTime,
        durationSeconds: v.duration || 0,
      }).catch(() => {});
    };

    // Одиночный, «смысловой» эвент реального просмотра: шлём ровно один раз
    // за контент (никаких запросов на каждую секунду). Логика порога — тут.
    const reportWatch = () => {
      if (!user || watchReported.current) return;
      watchReported.current = true;
      api.post('/Watch/report', {
        movieId: movieId || null,
        episodeId: episodeId || null,
        watchedSeconds: v.currentTime || v.duration || 0,
      }).catch(() => {});
    };

    const onTime = () => {
      sendProgress();
      // Пересекли реальный порог досмотра (~85%)? Если да — фиксируем просмотр.
      const dur = v.duration || 0;
      if (dur > 0 && v.currentTime >= dur * 0.85) reportWatch();
    };
    const onEnd = () => {
      if (user) {
        api.post('/Playback/progress', {
          movieId: movieId || null,
          episodeId: episodeId || null,
          positionSeconds: v.duration || 0,
          durationSeconds: v.duration || 0,
        }).catch(() => {});
      }
      // Доходишь до конца — это точно состоявшийся просмотр.
      reportWatch();
      if (onEnded) onEnded();
    };

    v.addEventListener('timeupdate', onTime);
    v.addEventListener('ended', onEnd);

    return () => {
      v.removeEventListener('timeupdate', onTime);
      v.removeEventListener('ended', onEnd);
      player.destroy();
    };
  }, [streamUrl]);

  if (!streamUrl) return <div className="text-white p-4">Видео не загружено</div>;

  return (
    <video ref={videoRef} className="w-full" playsInline controls crossOrigin="anonymous">
      <source src={streamUrl} type="video/mp4" />
    </video>
  );
};
