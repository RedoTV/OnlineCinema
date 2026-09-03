import React, { useEffect, useState } from 'react';
import { analyticsApi } from '../api/axios';
import { Link } from 'react-router-dom';

// Дашборд аналитики (v3.0). Тянет агрегаты из Python-сервиса.
const fmt = (s) => {
  const num = Number(s) || 0;
  if (num >= 1e6) return (num / 1e6).toFixed(1) + 'M';
  if (num >= 1e3) return (num / 1e3).toFixed(1) + 'k';
  return Math.round(num).toString();
};

export const AnalyticsDashboard = () => {
  const [ready, setReady] = useState(false);
  const [error, setError] = useState('');
  const [trending, setTrending] = useState([]);
  const [topRated, setTopRated] = useState([]);
  const [genreMix, setGenreMix] = useState([]);
  const [window, setWindow] = useState('168');

  const load = () => {
    analyticsApi.get(`/stats/trending?window=${window === '168' ? '7d' : window === '24' ? '24h' : '30d'}`)
      .then(r => setTrending(r.data)).catch(() => {});
    analyticsApi.get('/stats/top-rated?k=8').then(r => setTopRated(r.data)).catch(() => {});
    analyticsApi.get('/stats/genres').then(r => setGenreMix(r.data)).catch(() => {});
  };

  useEffect(() => {
    analyticsApi.get('/health').then(() => setReady(true)).catch(() => setError('Analytics-сервис недоступен (v3.0). Подними через docker compose --profile full up'));
    load();
  }, [window]);

  const maxGenre = Math.max(...genreMix.map(g => g.cnt), 1);

  return (
    <div>
      <h1 className="text-4xl font-black uppercase mb-2 border-l-8 border-black pl-4">АНАЛИТИКА</h1>
      <p className="text-sm text-gray-600 mb-6 italic">агрегируется из событий RabbitMQ в реальном времени (Python/FastAPI)</p>

      {error && <div className="text-red-600 font-bold border-2 border-red-600 p-3 mb-4">{error}</div>}

      {ready && (
        <div className="mb-6 flex gap-2">
          {[{ k: '24', l: '24 часа' }, { k: '168', l: 'Неделя' }, { k: '720', l: 'Месяц' }].map(w => (
            <button key={w.k} onClick={() => setWindow(w.k)}
              className={`px-3 py-1 border-2 border-black font-bold text-sm ${window === w.k ? 'bg-black text-white' : ''}`}>
              {w.l}
            </button>
          ))}
        </div>
      )}

      <div className="grid md:grid-cols-2 gap-8">
        <section>
          <h2 className="text-xl font-bold border-b-2 border-black mb-3 pb-1">ТРЕНДЫ (по просмотрам+времени)</h2>
          <ol className="space-y-1 text-sm">
            {trending.map((t, i) => (
              <li key={i} className="flex justify-between border-b border-gray-200 py-1">
                <span>
                  <span className="font-bold mr-2">{i + 1}.</span>
                  <Link to={`/movie/${t.content_id}`} className="hover:underline">
                    {t.content_type === 'movie' ? 'Фильм' : 'Сериал'} #{t.content_id}
                  </Link>
                </span>
                <span>👁 {fmt(t.views)} · {fmt(t.watch_seconds)}с</span>
              </li>
            ))}
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
    </div>
  );
};
