import React, { useEffect, useState } from 'react';
import { api } from '../api/axios';
import { Link } from 'react-router-dom';
import { PosterImage } from '../components/PosterImage';
import { useDebouncedValue } from '../hooks/useDebouncedValue';

const SKELETON_COUNT = 8;

const CatalogSkeleton = () => (
  <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6" aria-hidden="true">
    {Array.from({ length: SKELETON_COUNT }).map((_, i) => (
      <div key={i} className="border-2 border-black">
        <div className="aspect-[2/3] w-full animate-pulse bg-neutral-200" />
        <div className="space-y-2 p-3">
          <div className="h-4 animate-pulse bg-neutral-200" />
          <div className="h-3 w-2/3 animate-pulse bg-neutral-200" />
        </div>
      </div>
    ))}
  </div>
);

const MiniRow = ({ title, items, to }) => (
  <section className="mb-10">
    <div className="flex items-end justify-between mb-3">
      <h2 className="text-2xl font-black uppercase border-l-8 border-black pl-4 leading-none">{title}</h2>
      <Link to={to} className="text-sm font-bold uppercase hover:underline decoration-2 underline-offset-4">Все →</Link>
    </div>
    <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
      {items.map((item, i) => (
        <Link key={item.id} to={`${to}/${item.id}`} className="group block border-2 border-black hover:bg-black hover:text-white transition-colors">
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
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [retry, setRetry] = useState(0);
  const debouncedSearch = useDebouncedValue(search, 350);

  const handleSearchChange = (e) => {
    setSearch(e.target.value);
    setError('');
    setLoading(true);
  };

  const handleRetry = () => {
    setError('');
    setLoading(true);
    setRetry((n) => n + 1);
  };

  // Фильмы (с учётом поиска) + сериалы (топ для мини-раздела) — одним заходом.
  useEffect(() => {
    const controller = new AbortController();
    const params = new URLSearchParams();
    if (debouncedSearch) params.append('search', debouncedSearch);

    api.get(`/Movies?${params.toString()}`, { signal: controller.signal })
      .then((response) => setMovies(response.data))
      .catch((err) => {
        if (err?.code === 'ERR_CANCELED') return;
        console.error('Error:', err);
        setError('Не удалось загрузить каталог');
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false);
      });

    return () => controller.abort();
  }, [debouncedSearch, retry]);

  useEffect(() => {
    api.get('/Series')
      .then((response) => setSeries(response.data))
      .catch(() => {});
  }, []);

  const topMovies = movies.slice(0, 6);
  const topSeries = series.slice(0, 6);

  return (
    <div>
      {/* Поиск */}
      <div className="mb-10">
        <input
          type="text"
          placeholder="Поиск фильма по названию..."
          className="w-full p-3 border-2 border-black outline-none focus:ring-2 focus:ring-black transition-all font-medium placeholder-gray-500"
          value={search}
          onChange={handleSearchChange}
        />
      </div>

      {/* Мини-раздел фильмов */}
      {!search && topMovies.length > 0 && (
        <MiniRow title="Фильмы" items={topMovies.map(m => ({ ...m, kind: 'movie' }))} to="/" />
      )}

      {/* Мини-раздел сериалов */}
      {!search && topSeries.length > 0 && (
        <MiniRow title="Сериалы" items={topSeries.map(s => ({ ...s, kind: 'series' }))} to="/series" />
      )}

      {/* Новости и статьи */}
      {!search && (
        <section className="mb-10 border-2 border-black p-5 bg-neutral-50">
          <div className="flex items-end justify-between mb-4">
            <h2 className="text-2xl font-black uppercase border-l-8 border-black pl-4 leading-none">Новости и статьи</h2>
            <Link to="/news" className="text-sm font-bold uppercase hover:underline decoration-2 underline-offset-4">Все →</Link>
          </div>
          <p className="text-sm text-gray-600 italic">
            Обзоры, анонсы и мнения о кино. Читайте свежие материалы, а также делитесь своими — они проходят проверку модератором.
          </p>
        </section>
      )}

      {/* Полный каталог фильмов */}
      <h2 className="text-2xl font-black uppercase mb-3 border-l-8 border-black pl-4 leading-none">Каталог</h2>

      {error && (
        <div className="mb-6 border-2 border-red-600 p-4 text-center">
          <p className="font-bold text-red-600">{error}</p>
          <button
            onClick={handleRetry}
            className="mt-2 border-2 border-black px-4 py-1 font-bold hover:bg-black hover:text-white"
          >
            Повторить
          </button>
        </div>
      )}
      {loading && movies.length === 0 ? (
        <CatalogSkeleton />
      ) : (
        <div className={`grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6${loading ? ' opacity-60' : ''}`}>
          {movies.map((movie, i) => (
            <Link to={`/movie/${movie.id}`} key={movie.id} className="group block border-2 border-black hover:bg-black hover:text-white transition-colors duration-200">
              <div className="relative aspect-[2/3] w-full overflow-hidden border-b-2 border-black group-hover:border-white">
                <PosterImage
                  src={movie.posterUrl ? `/api/Media/poster/${movie.id}?size=preview` : null}
                  alt={movie.title}
                  eager={i < 4}
                />
              </div>
              <div className="p-3">
                <h3 className="font-black text-lg leading-tight mb-1 uppercase">{movie.title}</h3>
                <div className="flex justify-between text-sm font-medium">
                  <span>{movie.releaseYear}</span>
                  {movie.averageRating > 0 && <span>★ {Number(movie.averageRating).toFixed(1)}</span>}
                </div>
              </div>
            </Link>
          ))}
        </div>
      )}
      {!loading && !error && movies.length === 0 && (
        <p className="mt-6 text-center text-lg italic text-gray-500">Ничего не найдено</p>
      )}
    </div>
  );
};
