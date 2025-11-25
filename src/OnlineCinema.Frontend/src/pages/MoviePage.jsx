import React, { useEffect, useState, useContext } from 'react';
import { useParams } from 'react-router-dom';
import { api } from '../api/axios';
import { AuthContext } from '../context/AuthContext';

export const MoviePage = () => {
  const { id } = useParams();
  const [movie, setMovie] = useState(null);
  const { user } = useContext(AuthContext);

  // Состояние для отображения результата действия (успех/ошибка)
  const [actionStatus, setActionStatus] = useState({ message: '', type: '' });

  useEffect(() => {
    api.get(`/Movies/${id}`)
      .then(res => setMovie(res.data))
      .catch(() => setActionStatus({ message: 'Не удалось загрузить фильм', type: 'error' }));
  }, [id]);

  const handleSetStatus = async (status) => {
    // Сбрасываем предыдущий статус
    setActionStatus({ message: 'Сохранение...', type: 'loading' });

    try {
      await api.post('/UserActions/status', { movieId: Number(id), status });
      setActionStatus({ message: 'Статус успешно обновлен', type: 'success' });

      // Убираем сообщение через 3 секунды
      setTimeout(() => setActionStatus({ message: '', type: '' }), 3000);
    } catch {
      // Убрали 'e'
      setActionStatus({ message: 'Ошибка при обновлении статуса', type: 'error' });
    }
  };

  if (!movie) return <div className="text-xl font-bold text-center mt-10 uppercase">Загрузка данных...</div>;

  return (
    <div>
      <h1 className="text-4xl font-black uppercase mb-6 border-l-8 border-black pl-4 leading-none">{movie.title}</h1>

      <div className="mb-8 border-2 border-black p-1 bg-black">
        <video controls className="w-full aspect-video bg-black outline-none">
          <source src={`http://localhost:5000${movie.videoUrl}`} type="video/mp4" />
          Ваш браузер не поддерживает видео.
        </video>
      </div>

      <div className="grid md:grid-cols-3 gap-8">
        <div className="md:col-span-2">
          <h2 className="text-xl font-bold border-b-2 border-black mb-3 pb-1">ОПИСАНИЕ</h2>
          <p className="text-lg leading-relaxed text-justify whitespace-pre-line font-medium">
            {movie.description}
          </p>
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

            {/* Блок уведомления о результате */}
            {actionStatus.message && (
              <div className={`mt-4 p-2 text-sm font-bold text-center border-2 uppercase animate-pulse
                                ${actionStatus.type === 'error' ? 'border-red-600 text-red-600' :
                  actionStatus.type === 'success' ? 'border-green-600 text-green-600' :
                    'border-gray-400 text-gray-500'}`}
              >
                {actionStatus.message}
              </div>
            )}
          </div>
        )}
      </div>
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
