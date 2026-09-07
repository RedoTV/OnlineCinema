import React, { useEffect, useState } from 'react';
import { api } from '../api/axios';
import { Link } from 'react-router-dom';
import { PosterImage } from '../components/PosterImage';

const MiniRow = ({ title, items, to, itemTo }) => (
  <section className="mb-10">
    <div className="flex items-end justify-between mb-3">
      <h2 className="text-2xl font-black uppercase border-l-8 border-black pl-4 leading-none">{title}</h2>
      <Link to={to} className="text-sm font-bold uppercase hover:underline decoration-2 underline-offset-4">Все →</Link>
    </div>
    <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
      {items.map((item, i) => (
        <Link key={item.id} to={itemTo(item)} className="group block border-2 border-black hover:bg-black hover:text-white transition-colors">
          <div className="relative aspect-[2/3] w-full overflow-hidden border-b-2 border-black group-hover:border-white">
            <PosterImage
              src={item.posterUrl ? `/api/Media/poster/${item.kind === 'series' ? 'series/' : ''}${item.id}?size=preview` : null}
              alt={item.title}
              eager={i < 4}
            />
          </div>
          <div className="p-3">
            <h3 className="font-black text-sm leading-tight mb-1 uppercase">{item.title}</h3>
            <div className="flex justify-between text-xs font-medium">
              <span>{item.releaseYear}</span>
              {item.averageRating > 0 && <span>★ {Number(item.averageRating).toFixed(1)}</span>}
            </div>
          </div>
        </Link>
      ))}
    </div>
  </section>
);

export const HomePage = () => {
  const [movies, setMovies] = useState([]);
  const [series, setSeries] = useState([]);

  useEffect(() => {
    api.get('/Movies')
      .then((response) => setMovies(response.data))
      .catch(() => {});
  }, []);

  useEffect(() => {
    api.get('/Series')
      .then((response) => setSeries(response.data))
      .catch(() => {});
  }, []);

  const topMovies = movies.slice(0, 6);
  const topSeries = series.slice(0, 6);

  return (
    <div>
      <h1 className="text-3xl font-black uppercase mb-6 border-l-8 border-black pl-4 leading-none">Онлайн-кинотеатр</h1>

      {/* Мини-раздел фильмов */}
      {topMovies.length > 0 && (
        <MiniRow
          title="Фильмы"
          items={topMovies.map(m => ({ ...m, kind: 'movie' }))}
          to="/movies"
          itemTo={(m) => `/movie/${m.id}`}
        />
      )}

      {/* Мини-раздел сериалов */}
      {topSeries.length > 0 && (
        <MiniRow
          title="Сериалы"
          items={topSeries.map(s => ({ ...s, kind: 'series' }))}
          to="/series"
          itemTo={(s) => `/series/${s.id}`}
        />
      )}

      {/* Новости и статьи */}
      <section className="mb-10 border-2 border-black p-5 bg-neutral-50">
        <div className="flex items-end justify-between mb-4">
          <h2 className="text-2xl font-black uppercase border-l-8 border-black pl-4 leading-none">Новости и статьи</h2>
          <Link to="/news" className="text-sm font-bold uppercase hover:underline decoration-2 underline-offset-4">Все →</Link>
        </div>
        <p className="text-sm text-gray-600 italic">
          Обзоры, анонсы и мнения о кино. Читайте свежие материалы, а также делитесь своими — они проходят проверку модератором.
        </p>
      </section>
    </div>
  );
};
