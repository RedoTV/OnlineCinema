import React, { useEffect, useState, useContext } from 'react';
import { useParams, Link } from 'react-router-dom';
import { api } from '../api/axios';
import { AuthContext } from '../context/AuthContext';
import { CommentSection } from '../components/CommentSection';
import { VideoPlayer } from '../components/VideoPlayer';
import { PosterImage } from '../components/PosterImage';

// Портрет актёра с фолбэком на инициалы.
const ActorThumb = ({ actor }) => {
  const [failed, setFailed] = useState(false);
  const name = `${actor.firstName} ${actor.lastName}`;
  if (failed) {
    return (
      <div className="flex h-full w-full items-center justify-center bg-neutral-200 text-center p-2">
        <span className="text-[10px] font-bold uppercase leading-tight">{name}</span>
      </div>
    );
  }
  return (
    <img
      src={`/api/Media/poster/actor/${actor.id}`}
      alt={name}
      className="h-full w-full object-cover"
      onError={() => setFailed(true)}
    />
  );
};

export const MoviePage = () => {
  const { id } = useParams();
  const [movie, setMovie] = useState(null);
  const [streamUrl, setStreamUrl] = useState(null);
  const [initialPosition, setInitialPosition] = useState(0);
  const { user } = useContext(AuthContext);

  const [actionStatus, setActionStatus] = useState({ message: '', type: '' });
  const [myRating, setMyRating] = useState(10);

  useEffect(() => {
    api.get(`/Movies/${id}`)
      .then(res => setMovie(res.data))
      .catch(() => setActionStatus({ message: 'Не удалось загрузить фильм', type: 'error' }));

    api.get(`/Streaming/movie/${id}`)
      .then(res => setStreamUrl(res.data.url))
      .catch(() => {});

    if (user)
      api.get(`/Playback/progress?movieId=${id}`)
        .then(res => {
          if (res.data.positionSeconds > 30) setInitialPosition(res.data.positionSeconds);
        })
        .catch(() => {});
  }, [id, user]);

  const handleSetStatus = async (status) => {
    setActionStatus({ message: 'Сохранение...', type: 'loading' });
    try {
      await api.post('/UserActions/status', { movieId: Number(id), status });
      setActionStatus({ message: 'Статус успешно обновлен', type: 'success' });
      window.setTimeout(() => setActionStatus({ message: '', type: '' }), 3000);
    } catch {
      setActionStatus({ message: 'Ошибка при обновлении статуса', type: 'error' });
    }
  };

  const handleSetRatingValue = async () => {
    setActionStatus({ message: 'Сохранение...', type: 'loading' });
    try {
      await api.post('/UserActions/rating', { movieId: Number(id), rating: Number(myRating) });
      const fresh = await api.get(`/Movies/${id}`);
      setMovie(fresh.data);
      setActionStatus({ message: 'Оценка сохранена', type: 'success' });
      window.setTimeout(() => setActionStatus({ message: '', type: '' }), 2500);
    } catch {
      setActionStatus({ message: 'Ошибка при сохранении оценки', type: 'error' });
    }
  };

  if (!movie)
    return <div className="text-xl font-bold text-center mt-10 uppercase">Загрузка данных...</div>;

  const posterSrc = movie.posterUrl ? `/api/Media/poster/${movie.id}?size=preview` : null;
  const rating = movie.averageRating != null && movie.averageRating > 0
    ? Number(movie.averageRating).toFixed(1)
    : null;

  return (
    <div className="max-w-[1400px] mx-auto">
      {/* ПЛЕЕР */}
      <div className="mb-8 border-2 border-black p-1 bg-black">
        <VideoPlayer streamUrl={streamUrl} movieId={Number(id)} initialPosition={initialPosition} />
      </div>

      {/* ШАПКА: постер слева, инфо справа */}
      <header className="grid md:grid-cols-[260px_1fr] gap-10 mb-12">
        <div className="aspect-[2/3] border-2 border-black overflow-hidden bg-neutral-900 shadow-[8px_8px_0_#000]">
          <PosterImage src={posterSrc} alt={movie.title} eager />
        </div>

        <div className="flex flex-col justify-between gap-6">
          <div>
            <h1 className="text-4xl md:text-5xl font-black uppercase leading-none tracking-[-0.03em] mb-3">{movie.title}</h1>

            <div className="flex flex-wrap items-center gap-x-6 gap-y-2 text-sm uppercase tracking-widest text-neutral-500 mb-5">
              {movie.releaseYear && <span>{movie.releaseYear}</span>}
              {movie.duration && <span>{movie.duration} мин</span>}
              {movie.genres?.length > 0 && <span>{movie.genres.map(g => g.name).join(' · ')}</span>}
            </div>

            {/* Средняя оценка */}
            <div className="flex items-center gap-4 mb-6">
              {rating ? (
                <>
                  <span className="text-5xl font-black text-black leading-none">{rating}</span>
                  <span className="text-6xl text-amber-400 leading-none">★</span>
                </>
              ) : (
                <span className="text-sm font-bold uppercase text-neutral-400">Оценок пока нет</span>
              )}
            </div>
          </div>

          {/* Кнопки действий */}
          {user && (
            <div className="space-y-2 max-w-sm">
              <ActionButton onClick={() => handleSetStatus('Planned')}>+ В планах</ActionButton>
              <ActionButton onClick={() => handleSetStatus('Watched')}>✓ Просмотрено</ActionButton>
              <ActionButton onClick={() => handleSetStatus('Favorite')}>♥ В избранное</ActionButton>
            </div>
          )}
        </div>
      </header>

      {/* Описание */}
      {movie.description && (
        <section className="mb-12">
          <h2 className="text-2xl font-black uppercase mb-4 border-b-2 border-black pb-2">ОПИСАНИЕ</h2>
          <p className="text-lg leading-relaxed text-justify font-medium whitespace-pre-line text-gray-800">{movie.description}</p>
        </section>
      )}

      {/* Оценка пользователя */}
      {user && (
        <section className="mb-12 border-2 border-black p-5 max-w-md">
          <h2 className="font-black uppercase mb-3">Ваша оценка</h2>
          <div className="flex items-center gap-3">
            <select
              value={myRating}
              onChange={(e) => setMyRating(e.target.value)}
              className="border-2 border-black px-3 py-2 font-bold outline-none focus:ring-2 focus:ring-black"
            >
              {[...Array(10)].map((_, i) => (
                <option key={i + 1} value={i + 1}>{i + 1}</option>
              ))}
            </select>
            <button onClick={handleSetRatingValue} className="border-2 border-black bg-black text-white px-4 py-2 font-bold uppercase hover:bg-white hover:text-black transition-colors">
              Поставить
            </button>
          </div>
          {actionStatus.message && (
            <div className={`mt-3 p-2 text-sm font-bold text-center border-2 uppercase
              ${actionStatus.type === 'error' ? 'border-red-600 text-red-600' :
                actionStatus.type === 'success' ? 'border-green-600 text-green-600' :
                  'border-gray-400 text-gray-500'}`}>
              {actionStatus.message}
            </div>
          )}
        </section>
      )}

      {/* Актёры */}
      {movie.actors?.length > 0 && (
        <section className="mb-12">
          <h2 className="text-2xl font-black uppercase mb-6 border-b-2 border-black pb-2">АКТЁРЫ</h2>
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4">
            {movie.actors.map((actor) => (
              <Link key={actor.id} to={`/actor/${actor.id}`} className="group block">
                <div className="aspect-[2/3] border-2 border-black overflow-hidden bg-neutral-900 mb-2 group-hover:shadow-[4px_4px_0_#000] transition-shadow">
                  <ActorThumb actor={actor} />
                </div>
                <div className="text-center">
                  <div className="font-bold text-sm leading-tight uppercase group-hover:underline">{actor.firstName} {actor.lastName}</div>
                </div>
              </Link>
            ))}
          </div>
        </section>
      )}

      <CommentSection movieId={Number(id)} />
    </div>
  );
};

const ActionButton = ({ onClick, children }) => (
  <button
    onClick={onClick}
    className="w-full py-3 px-4 border-2 border-black font-bold hover:bg-black hover:text-white transition-all text-left uppercase flex justify-between items-center group active:bg-gray-800"
  >
    {children}
    <span className="group-hover:translate-x-1 transition-transform text-xl leading-none">→</span>
  </button>
);
