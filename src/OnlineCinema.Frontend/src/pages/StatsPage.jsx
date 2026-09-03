import React, { useEffect, useState } from 'react';
import { api } from '../api/axios';
import { Link } from 'react-router-dom';

// Статистика (v2.0 пока читает из основной БД, агрегации SQL)
export const StatsPage = () => {
  const [topRated, setTopRated] = useState([]);
  const [mostWatched, setMostWatched] = useState([]);
  const [genres, setGenres] = useState([]);

  useEffect(() => {
    api.get('/Stats/top-rated?count=10').then(r => setTopRated(r.data)).catch(() => {});
    api.get('/Stats/most-watched?count=10').then(r => setMostWatched(r.data)).catch(() => {});
    api.get('/Stats/genres').then(r => setGenres(r.data)).catch(() => {});
  }, []);

  const maxGenreCount = Math.max(...genres.map(g => g.movieCount), 1);

  return (
    <div>
      <h1 className="text-4xl font-black uppercase mb-6 border-l-8 border-black pl-4">СТАТИСТИКА</h1>

      <div className="grid md:grid-cols-2 gap-8">
        <section>
          <h2 className="text-xl font-bold border-b-2 border-black mb-3 pb-1">ТОП ПО ОЦЕНКАМ</h2>
          <table className="w-full text-sm">
            <tbody>
              {topRated.map((m, i) => (
                <tr key={m.id} className="border-b border-gray-200">
                  <td className="py-1 pr-2 font-bold">{i + 1}</td>
                  <td className="py-1"><Link to={`/movie/${m.id}`} className="hover:underline">{m.title}</Link></td>
                  <td className="py-1 text-right">★ {m.avgRating.toFixed(1)} <span className="text-gray-500">({m.ratingCount})</span></td>
                </tr>
              ))}
              {topRated.length === 0 && <tr><td className="py-2 italic text-gray-500">Оценок пока нет</td></tr>}
            </tbody>
          </table>
        </section>

        <section>
          <h2 className="text-xl font-bold border-b-2 border-black mb-3 pb-1">ЧАЩЕ ВСЕГО СМОТРЯТ</h2>
          <table className="w-full text-sm">
            <tbody>
              {mostWatched.map((m, i) => (
                <tr key={i} className="border-b border-gray-200">
                  <td className="py-1 pr-2 font-bold">{i + 1}</td>
                  <td className="py-1"><Link to={`/movie/${m.movieId}`} className="hover:underline">{m.title}</Link></td>
                  <td className="py-1 text-right font-medium">{m.views} просмотров</td>
                </tr>
              ))}
              {mostWatched.length === 0 && <tr><td className="py-2 italic text-gray-500">Нет данных по просмотрам</td></tr>}
            </tbody>
          </table>
        </section>
      </div>

      <section className="mt-8">
        <h2 className="text-xl font-bold border-b-2 border-black mb-3 pb-1">ЖАНРЫ</h2>
        <div className="space-y-2">
          {genres.map(g => (
            <div key={g.genre} className="flex items-center gap-3">
              <span className="w-32 font-medium">{g.genre}</span>
              <div className="flex-1 border-2 border-black">
                <div className="bg-black text-white text-right text-xs pr-1"
                  style={{ width: `${(g.movieCount / maxGenreCount) * 100}%`, transition: 'width .5s' }}>
                  {g.movieCount}
                </div>
              </div>
            </div>
          ))}
          {genres.length === 0 && <p className="italic text-gray-500">Нет жанров</p>}
        </div>
      </section>
    </div>
  );
};
