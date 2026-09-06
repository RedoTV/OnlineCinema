import React, { useEffect, useState } from 'react';
import { api } from '../api/axios';
import { Link } from 'react-router-dom';

const fmt = (s) => {
  const num = Number(s) || 0;
  if (num >= 1e6) return (num / 1e6).toFixed(1) + 'M';
  if (num >= 1e3) return (num / 1e3).toFixed(1) + 'k';
  return Math.round(num).toString();
};

const DAYS = { '24': 1, '168': 7, '720': 30 };

export const AnalyticsDashboard = () => {
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [trending, setTrending] = useState([]);
  const [topRated, setTopRated] = useState([]);
  const [genreMix, setGenreMix] = useState([]);
  const [window, setWindow] = useState('168');
  const [retry, setRetry] = useState(0);

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

  useEffect(() => {
    const controller = new AbortController();
    const signal = controller.signal;

    Promise.all([
      api.get(`/analytics/trending?days=${DAYS[window]}&count=12`, { signal }).then(r => setTrending(r.data)),
      api.get('/analytics/top-rated?count=8', { signal }).then(r => setTopRated(r.data)),
      api.get('/analytics/genres', { signal }).then(r => setGenreMix(r.data)),
    ])
      .then(() => setError(''))
      .catch((err) => {
        if (err?.code === 'ERR_CANCELED') return;
        setError('Не удалось загрузить аналитику — повтори попытку');
      })
      .finally(() => {
        if (!signal.aborted) setLoading(false);
      });

    return () => controller.abort();
  }, [window, retry]);

  const maxGenre = Math.max(...genreMix.map(g => g.movieCount), 1);

  return (
    <div>
      <h1 className="text-4xl font-black uppercase mb-2 border-l-8 border-black pl-4">АНАЛИТИКА</h1>
      <p className="text-sm text-gray-600 mb-6 italic">агрегируется прямо из базы каталога (.NET)</p>

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

      <div className="mb-6 flex gap-2">
        {[{ k: '24', l: '24 часа' }, { k: '168', l: 'Неделя' }, { k: '720', l: 'Месяц' }].map(w => (
          <button key={w.k} onClick={() => handleWindowChange(w.k)}
            className={`px-3 py-1 border-2 border-black font-bold text-sm ${window === w.k ? 'bg-black text-white' : ''}`}>
            {w.l}
          </button>
        ))}
      </div>

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
          <h2 className="text-xl font-bold border-b-2 border-black mb-3 pb-1">ТРЕНДЫ (по просмотрам+оценкам)</h2>
          <ol className="space-y-1 text-sm">
            {trending.map((t, i) => {
              const bits = [];
              if (Number(t.views) > 0) bits.push(`👁 ${fmt(t.views)}`);
              if (Number(t.ratingCount) > 0) bits.push(`★ ${Number(t.avgRating || 0).toFixed(1)} (${t.ratingCount})`);
              return (
              <li key={i} className="flex justify-between border-b border-gray-200 py-1">
                <span>
                  <span className="font-bold mr-2">{i + 1}.</span>
                  <Link to={t.contentType === 'movie' ? `/movie/${t.contentId}` : `/series/${t.contentId}`} className="hover:underline">
                    {t.title || `${t.contentType === 'movie' ? 'Фильм' : 'Сериал'} #${t.contentId}`}
                  </Link>
                </span>
                <span>{bits.join(' · ')}</span>
              </li>
              );
            })}
            {trending.length === 0 && <li className="italic text-gray-500">За последние дни активности нет.</li>}
          </ol>
        </section>

        <section>
          <h2 className="text-xl font-bold border-b-2 border-black mb-3 pb-1">ОБЩИЙ ТОП ПО ОЦЕНКАМ</h2>
          <table className="w-full text-sm">
            <tbody>
              {topRated.map((m, i) => (
                <tr key={m.id} className="border-b border-gray-200">
                  <td className="py-1 pr-2 font-bold">{i + 1}</td>
                  <td><Link to={`/movie/${m.id}`} className="hover:underline">{m.title}</Link></td>
                  <td className="text-right">★ {Number(m.avgRating).toFixed(1)} ({m.ratingCount})</td>
                </tr>
              ))}
              {topRated.length === 0 && <tr><td className="italic text-gray-500">Оценок нет</td></tr>}
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
                <div className="bg-blue-800 text-white text-right text-xs px-1" style={{ width: `${(g.movieCount / maxGenre) * 100}%` }}>{g.movieCount}</div>
              </div>
            </div>
          ))}
          {genreMix.length === 0 && <p className="italic text-gray-500">Пусто</p>}
        </div>
      </section>
      </>
      )}
    </div>
  );
};
