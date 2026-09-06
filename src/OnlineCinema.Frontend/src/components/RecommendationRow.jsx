import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api/axios';

export const RecommendationRow = ({ userId, username }) => {
  const [picks, setPicks] = useState([]);

  useEffect(() => {
    if (!userId) return;
    api.get(`/analytics/picks/${userId}?count=8`)
      .then(res => setPicks(res.data || []))
      .catch(() => setPicks([]));
  }, [userId]);

  if (!picks.length) return null;

  return (
    <section className="mb-10">
      <h2 className="text-2xl font-black uppercase mb-1 border-l-8 border-black pl-4">
        Для тебя{username ? `, ${username}` : ''}
      </h2>
      <p className="text-sm text-gray-600 italic mb-3">по вашим любимым жанрам и новинкам (.NET)</p>
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {picks.map(p => (
          <div key={p.movieId} className="border-2 border-black hover:bg-black hover:text-white transition-colors">
            <Link to={`/movie/${p.movieId}`} className="block p-3">
              <div className="font-bold leading-tight mb-1 uppercase text-sm">{p.title}</div>
              <div className="text-xs flex justify-between gap-2">
                <span>{p.rating != null ? `★ ${Number(p.rating).toFixed(1)}` : ''}</span>
                {p.reason && <span className="italic text-right">{p.reason}</span>}
              </div>
            </Link>
          </div>
        ))}
      </div>
    </section>
  );
};
