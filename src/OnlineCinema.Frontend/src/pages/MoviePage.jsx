import React, { useEffect, useState, useContext } from 'react';
import { useParams } from 'react-router-dom';
import { api } from '../api/axios';
import { AuthContext } from '../context/AuthContext';
import { CommentSection } from '../components/CommentSection';
import { VideoPlayer } from '../components/VideoPlayer';

export const MoviePage = () => {
  const { id } = useParams();
  const [movie, setMovie] = useState(null);
  const [streamUrl, setStreamUrl] = useState(null);
  const [initialPosition, setInitialPosition] = useState(0);
  const { user } = useContext(AuthContext);

  const [actionStatus, setActionStatus] = useState({ message: '', type: '' });

  // Оценка пользователя (по умолчанию 10)
  const [myRating, setMyRating] = useState(10);

  useEffect(() => {
    api.get(`/Movies/${id}`)
      .then(res => setMovie(res.data))
      .catch(() => setActionStatus({ message: 'Не удалось загрузить фильм', type: 'error' }));

    // presigned URL для стриминга, чтобы не гонять байты через бэкенд
    api.get(`/Streaming/movie/${id}`)
      .then(res => setStreamUrl(res.data.url))
      .catch(() => {});

    // резюм с места остановки
    if (user)
      api.get(`/Playback/progress?movieId=${id}`)
        .then(res => {
          if (res.data.positionSeconds > 30) setInitialPosition(res.data.positionSeconds);
        })
        .catch(() => {});
  }, [id]);

  const handleSetStatus = async (status) => {
    setActionStatus({ message: 'Сохранение...', type: 'loading' });

    try {
      await api.post('/UserActions/status', { movieId: Number(id), status });
      setActionStatus({ message: 'Статус успешно обновлен', type: 'success' });
      setTimeout(() => setActionStatus({ message: '', type: '' }), 3000);
    } catch {
      setActionStatus({ message: 'Ошибка при обновлении статуса', type: 'error' });
    }
  };

  const handleSetRatingValue = async () => {
    setActionStatus({ message: 'Сохранение...', type: 'loading' });
    try {
      await api.post('/UserActions/rating', { movieId: Number(id), rating: Number(myRating) });
      setActionStatus({ message: 'Оценка сохранена', type: 'success' });
      setTimeout(() => setActionStatus({ message: '', type: '' }), 2500);
    } catch {
      setActionStatus({ message: 'Ошибка при сохранении оценки', type: 'error' });
    }
  };

  if (!movie)
    return <div className="text-xl font-bold text-center mt-10 uppercase">Загрузка данных...</div>;

  return (
    <div>
      <h1 className="text-4xl font-black uppercase mb-6 border-l-8 border-black pl-4 leading-none">{movie.title}</h1>

      <div className="mb-8 border-2 border-black p-1 bg-black">
        <VideoPlayer streamUrl={streamUrl} movieId={Number(id)} initialPosition={initialPosition} />
      </div>

      <div className="grid md:grid-cols-3 gap-8">
        <div className="md:col-span-2">
          <h2 className="text-xl font-bold border-b-2 border-black mb-3 pb-1">ОПИСАНИЕ</h2>
          <p className="text-lg leading-relaxed text-justify whitespace-pre-line font-medium">{movie.description}</p>

          <h2 className="text-xl font-bold border-b-2 border-black mb-3 pb-1 mt-6">АКТЁРЫ</h2>
          <ul className="flex flex-wrap gap-x-4 gap-y-1 mb-8">
            {movie.actors?.map(a => (
              <li key={a.id} className="font-medium">{a.firstName} {a.lastName}</li>
            ))}
          </ul>

          {user && (
            <div className="mt-6 border-2 border-black p-3">
              <div className="font-bold mb-2">Ваша оценка</div>
              <div className="flex items-center gap-3">
                <select
                  value={myRating}
                  onChange={(e) => setMyRating(e.target.value)}
                  className="border-2 border-black px-2 py-1"
                >
                  {[...Array(10)].map((_, i) => (
                    <option key={i + 1} value={i + 1}>{i + 1}</option>
                  ))}
                </select>
                <button onClick={handleSetRatingValue} className="border-2 border-black px-3 py-1 font-bold hover:bg-black hover:text-white">
                  Поставить
                </button>
              </div>
            </div>
          )}
        </div>
        {/* Панель действий */}
        {user && (
          <div>
            <h2 className="text-xl font-bold border-b-2 border-black mb-3 pb-1">ДЕЙСТВИЯ</h2>
            <div className="flex flex-col gap-3">
              <ActionButton onClick={() => handleSetStatus('Planned')}>В ПЛАНАХ</ActionButton>
              <ActionButton onClick={() => handleSetStatus('Watched')}>ПРОСМОТРЕНО</ActionButton>
              <ActionButton onClick={() => handleSetStatus('Favorite')}>ИЗБРАННОЕ</ActionButton>
            </div>
            {actionStatus.message && (
              <div className={`mt-4 p-2 text-sm font-bold text-center border-2 uppercase animate-pulse
                ${actionStatus.type === 'error' ? 'border-red-600 text-red-600' :
                  actionStatus.type === 'success' ? 'border-green-600 text-green-600' :
                    'border-gray-400 text-gray-500'}`}>
                {actionStatus.message}
              </div>
            )}
          </div>
        )}
      </div>

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
