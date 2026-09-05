import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { analyticsApi } from '../api/axios';

// «Подборка для вас» — берём рекомендации у Python/ML сервиса.
export const RecommendationRow = ({ userId, username }) => {
  const [recs, setRecs] = useState([]);
  const [hidden, setHidden] = useState(true);

  useEffect(() => {
    if (!userId) return;
    analyticsApi.get(`/recommendations/${userId}?k=8`)
      .then(res => {
        if (res.data && res.data.length) { setRecs(res.data); setHidden(false); }
      })
      .catch(() => {}); // сервис может быть не поднят — не страшно
  }, [userId]);

  if (hidden || recs.length === 0) return null;

  return (
    <section className="mb-10">
      <h2 className="text-2xl font-black uppercase mb-1 border-l-8 border-black pl-4">
        Для тебя{username ? `, ${username}` : ''}
      </h2>
      <p className="text-sm text-gray-600 italic mb-3">порекомендовано ML-сервисом (SVD + жанры)</p>
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {recs.map(r => (
          <div key={r.movieId} className="border-2 border-black hover:bg-black hover:text-white transition-colors">
            <Link to={`/movie/${r.movieId}`} className="block p-3">
              <div className="font-bold leading-tight mb-1 uppercase text-sm">{r.title}</div>
              <div className="text-xs flex justify-between gap-2">
                <span>★ {Number(r.score).toFixed(1)}</span>
                {r.reason && <span className="italic text-right">{r.reason}</span>}
              </div>
            </Link>
          </div>
        ))}
      </div>
    </section>
  );
};
