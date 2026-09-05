import React, { useEffect, useState } from 'react';
import { analyticsApi } from '../api/axios';
import { Link } from 'react-router-dom';

// Дашборд аналитики. Тянет агрегаты из Python-сервиса.
const fmt = (s) => {
  const num = Number(s) || 0;
  if (num >= 1e6) return (num / 1e6).toFixed(1) + 'M';
  if (num >= 1e3) return (num / 1e3).toFixed(1) + 'k';
  return Math.round(num).toString();
};

export const AnalyticsDashboard = () => {
  const [ready, setReady] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [trending, setTrending] = useState([]);
  const [topRated, setTopRated] = useState([]);
  const [genreMix, setGenreMix] = useState([]);
  const [window, setWindow] = useState('168');
  const [retry, setRetry] = useState(0);

  const windowParam = window === '168' ? '7d' : window === '24' ? '24h' : '30d';

  const handleWindowChange = (k) => {
    setWindow(k);
    setError('');
    setLoading(true);
  };

  const handleRetry = () => {
    setError('');
    setLoading(true);
    setRetry((n) => n + 1);
  };

  // Все три виджета грузятся параллельно; ошибка любого видна,
  // а не тонет в тихом catch — иначе пустые секции выглядят как «нет данных».
  useEffect(() => {
    const controller = new AbortController();
    const signal = controller.signal;

    analyticsApi.get('/health', { signal })
      .then(() => setReady(true))
      .catch((err) => {
        if (err?.code === 'ERR_CANCELED') return;
        setError('Analytics-сервис недоступен. Подними через docker compose up -d');
        setLoading(false);
      });

    Promise.allSettled([
      analyticsApi.get(`/stats/trending?window=${windowParam}`, { signal }),
      analyticsApi.get('/stats/top-rated?k=8', { signal }),
      analyticsApi.get('/stats/genres', { signal }),
    ]).then(([t, top, genres]) => {
      if (signal.aborted) return;
      if (t.status === 'fulfilled') setTrending(t.value.data);
      if (top.status === 'fulfilled') setTopRated(top.value.data);
      if (genres.status === 'fulfilled') setGenreMix(genres.value.data);
      const failed = [t, top, genres].filter(r => r.status === 'rejected');
      if (failed.length > 0) setError('Часть виджетов не загрузилась — повтори попытку');
      setLoading(false);
    });

    return () => controller.abort();
  }, [windowParam, retry]);

  const maxGenre = Math.max(...genreMix.map(g => g.cnt), 1);

  return (
    <div>
      <h1 className="text-4xl font-black uppercase mb-2 border-l-8 border-black pl-4">АНАЛИТИКА</h1>
      <p className="text-sm text-gray-600 mb-6 italic">агрегируется из событий RabbitMQ в реальном времени (Python/FastAPI)</p>

      {error && (
        <div className="border-2 border-red-600 p-4 mb-4 text-center">
          <p className="text-red-600 font-bold">{error}</p>
          <button
            onClick={handleRetry}
            className="mt-2 border-2 border-black px-4 py-1 font-bold hover:bg-black hover:text-white"
          >
            Повторить
          </button>
        </div>
      )}

      {ready && (
        <div className="mb-6 flex gap-2">
          {[{ k: '24', l: '24 часа' }, { k: '168', l: 'Неделя' }, { k: '720', l: 'Месяц' }].map(w => (
            <button key={w.k} onClick={() => handleWindowChange(w.k)}
              className={`px-3 py-1 border-2 border-black font-bold text-sm ${window === w.k ? 'bg-black text-white' : ''}`}>
              {w.l}
            </button>
          ))}
        </div>
      )}

      {loading && (
        <div className="grid md:grid-cols-2 gap-8" aria-hidden="true">
          {[0, 1].map(i => (
            <div key={i} className="space-y-2">
              <div className="h-6 w-1/2 animate-pulse bg-neutral-200" />
              {Array.from({ length: 5 }).map((_, j) => (
                <div key={j} className="h-4 animate-pulse bg-neutral-200" />
              ))}
            </div>
          ))}
        </div>
      )}

      {!loading && (
      <>
      <div className="grid md:grid-cols-2 gap-8">
        <section>
          <h2 className="text-xl font-bold border-b-2 border-black mb-3 pb-1">ТРЕНДЫ (по просмотрам+времени)</h2>
          <ol className="space-y-1 text-sm">
            {trending.map((t, i) => {
              // Нулевые метрики прячем, а не показываем «👁 0 · 0с»:
              // строка из одних оценок иначе выглядит сломанной.
              const bits = [];
              if (Number(t.views) > 0) bits.push(`👁 ${fmt(t.views)}`);
              if (Number(t.watch_seconds) > 0) bits.push(`${fmt(t.watch_seconds)}с`);
              if (Number(t.avg_grade) > 0) bits.push(`★ ${Number(t.avg_grade).toFixed(1)}`);
              return (
              <li key={i} className="flex justify-between border-b border-gray-200 py-1">
                <span>
                  <span className="font-bold mr-2">{i + 1}.</span>
                  <Link to={t.content_type === 'movie' ? `/movie/${t.content_id}` : `/series/${t.content_id}`} className="hover:underline">
                    {t.title || `${t.content_type === 'movie' ? 'Фильм' : 'Сериал'} #${t.content_id}`}
                  </Link>
                </span>
                <span>{bits.join(' · ')}</span>
              </li>
              );
            })}
            {ready && trending.length === 0 && <li className="italic text-gray-500">Событий пока нет — посмотри что-нибудь 😉</li>}
          </ol>
        </section>

        <section>
          <h2 className="text-xl font-bold border-b-2 border-black mb-3 pb-1">ОБЩИЙ ТОП ПО ОЦЕНКАМ</h2>
          <table className="w-full text-sm">
            <tbody>
              {topRated.map((m, i) => (
                <tr key={m.Id} className="border-b border-gray-200">
                  <td className="py-1 pr-2 font-bold">{i + 1}</td>
                  <td><Link to={`/movie/${m.Id}`} className="hover:underline">{m.Title}</Link></td>
                  <td className="text-right">★ {Number(m.avg_rating).toFixed(1)} ({m.n_ratings})</td>
                </tr>
              ))}
              {ready && topRated.length === 0 && <tr><td className="italic text-gray-500">Оценок нет</td></tr>}
            </tbody>
          </table>
        </section>
      </div>

      <section className="mt-8">
        <h2 className="text-xl font-bold border-b-2 border-black mb-3 pb-1">ЖАНРОВАЯ РАЗБИВКА</h2>
        <div className="space-y-2">
          {genreMix.map(g => (
            <div key={g.genre} className="flex items-center gap-3">
              <span className="w-32 font-medium">{g.genre}</span>
              <div className="flex-1 border-2 border-black">
                <div className="bg-blue-800 text-white text-right text-xs px-1" style={{ width: `${(g.cnt / maxGenre) * 100}%` }}>{g.cnt}</div>
              </div>
            </div>
          ))}
          {ready && genreMix.length === 0 && <p className="italic text-gray-500">Пусто</p>}
        </div>
      </section>
      </>
      )}
    </div>
  );
};
