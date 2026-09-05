import React, { useEffect, useState, useContext } from 'react';
import { api } from '../api/axios';
import { Link } from 'react-router-dom';
import { AuthContext } from '../context/AuthContext';
import { RecommendationRow } from '../components/RecommendationRow';
import { ActivityFeed } from '../components/ActivityFeed';
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

export const HomePage = () => {
  const { user } = useContext(AuthContext);
  const [movies, setMovies] = useState([]);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [retry, setRetry] = useState(0);
  // Поиск бьёт в API только после паузы ввода, а не на каждую клавишу.
  const debouncedSearch = useDebouncedValue(search, 350);

  // sub в JWT = userId. Идёт в analytics для рекомендаций
  const userId = user ? Number(user.sub) : null;
  const username = user?.username;

  // Сброс loading/error живёт в обработчиках, а не в эффекте:
  // синхронный setState в теле эффекта запрещён (каскадные рендеры).
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

    // Устаревший ответ поиска не затирает свежий.
    return () => controller.abort();
  }, [debouncedSearch, retry]);

  return (
    <div>
      {/* Поиск */}
      <div className="mb-8">
        <input
          type="text"
          placeholder="Поиск фильма по названию..."
          className="w-full p-3 border-2 border-black outline-none focus:ring-2 focus:ring-black transition-all font-medium placeholder-gray-500"
          value={search}
          onChange={handleSearchChange}
        />
      </div>

      {/* Персонализация + live-активность из analytics */}
      <RecommendationRow userId={userId} username={username} />
      <ActivityFeed userId={userId} />

      {/* Каталог */}
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
                <h3 className="font-bold text-lg leading-tight mb-1 uppercase">{movie.title}</h3>
                <div className="flex justify-between text-sm font-medium">
                  <span>{movie.releaseYear}</span>
                  <span>★ {movie.averageRating?.toFixed(1)}</span>
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
