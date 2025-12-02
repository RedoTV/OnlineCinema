import React, { useEffect, useState, useContext } from 'react';
import { api } from '../api/axios';
import { AuthContext } from '../context/AuthContext';

// Словарь для русских названий статусов
const statusLabel = {
  Planned: 'В планах',
  Watched: 'Просмотрено',
  Favorite: 'Избранное',
};

const allStatuses = ['Planned', 'Watched', 'Favorite'];

export const ProfilePage = () => {
  const { user, logout } = useContext(AuthContext);
  const [myMovies, setMyMovies] = useState([]);
  const [filterStatus, setFilterStatus] = useState('');

  useEffect(() => {
    const fetchMyMovies = async () => {
      try {
        const params = filterStatus ? `?status=${filterStatus}` : '';
        const res = await api.get(`/UserActions/my-movies${params}`);
        setMyMovies(res.data);
      } catch (error) {
        console.error(error);
      }
    };
    if (user) fetchMyMovies();
  }, [user, filterStatus]);

  const updateStatus = async (movieId, status) => {
    await api.post('/UserActions/status', { movieId, status });
    const params = filterStatus ? `?status=${filterStatus}` : '';
    const res = await api.get(`/UserActions/my-movies${params}`);
    setMyMovies(res.data);
  };

  if (!user) return <div className="text-xl font-bold">Требуется вход в систему.</div>;

  return (
    <div>
      <div className="flex justify-between items-end mb-8 border-b-4 border-black pb-4">
        <div>
          <div className="text-sm text-gray-500 font-bold mb-1">ПОЛЬЗОВАТЕЛЬ</div>
          <h2 className="text-3xl font-black uppercase">{user.username}</h2>
        </div>
        <button onClick={logout} className="text-red-600 font-bold border-2 border-red-600 px-4 py-2 hover:bg-red-600 hover:text-white transition-colors">
          ВЫЙТИ
        </button>
      </div>

      {/* Фильтры в виде табов, показываем русские подписи */}
      <div className="flex flex-wrap gap-4 mb-8">
        {['', ...allStatuses].map(status => (
          <button
            key={status}
            onClick={() => setFilterStatus(status)}
            className={`px-6 py-2 border-2 border-black font-bold uppercase transition-all ${filterStatus === status
              ? 'bg-black text-white'
              : 'hover:bg-gray-100'
              }`}
          >
            {status === '' ? 'ВСЕ' : statusLabel[status]}
          </button>
        ))}
      </div>

      {/* Список фильмов — строгий список с управлением статусом */}
      <div className="space-y-4">
        {myMovies.length === 0 ? (
          <p className="text-lg italic text-gray-500">Список пуст</p>
        ) : (
          myMovies.map(m => (
            <div key={m.movieId} className="flex items-center border-2 border-black p-2 hover:bg-gray-50 transition-colors">
              <img
                src={m.posterUrl ? `http://localhost:5000${m.posterUrl}` : '/placeholder.jpg'}
                className="w-12 h-16 object-cover border border-black mr-4"
                alt="poster"
              />
              <div className="flex-1">
                <h4 className="font-bold text-lg">{m.movieTitle}</h4>
                <span className="text-sm font-medium border border-black px-2 py-0.5 bg-white inline-block mt-1 mr-3">
                  {statusLabel[m.status] ?? m.status}
                </span>
                <select
                  className="text-sm border-2 border-black px-2 py-0.5"
                  value={m.status}
                  onChange={(e) => updateStatus(m.movieId, e.target.value)}
                  style={{ marginLeft: '10px' }}
                >
                  {allStatuses.map(s => <option key={s} value={s}>{statusLabel[s]}</option>)}
                </select>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
};