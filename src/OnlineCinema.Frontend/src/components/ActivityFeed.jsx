import { useEffect, useState } from 'react';
import { analyticsApi } from '../api/axios';
import { Link } from 'react-router-dom';

const KIND_LABEL = {
  watched: 'посмотрел',
  rated: 'оценил',
  favorite: 'добавил в избранное',
  comment: 'прокомментировал',
  registered: 'зарегистрировался',
};

const fmtAgo = (iso) => {
  const d = new Date(iso);
  const s = Math.floor((Date.now() - d.getTime()) / 1000);
  if (s < 60) return 'только что';
  if (s < 3600) return `${Math.floor(s / 60)} мин назад`;
  if (s < 86400) return `${Math.floor(s / 3600)} ч назад`;
  return d.toLocaleDateString('ru-RU');
};

// Живая лента действий (из analytics user_events).
export const ActivityFeed = ({ userId }) => {
  const [items, setItems] = useState([]);

  useEffect(() => {
    const qs = userId ? `?user_id=${userId}&limit=12` : '?limit=15';
    analyticsApi.get(`/activity${qs}`)
      .then(res => setItems(res.data))
      .catch(() => {});
  }, [userId]);

  return (
    <section className="mb-10">
      <h2 className="text-2xl font-black uppercase mb-3 border-l-8 border-black pl-4">
        {userId ? 'МОЯ АКТИВНОСТЬ' : 'ЧТО СМОТРЯТ'}
      </h2>
      <div className="border-2 border-black divide-y divide-black text-sm">
        {items.length === 0 && <div className="px-3 py-4 italic text-gray-500">Пока пусто — начни смотреть и оценивать</div>}
        {items.map((e, i) => (
          <div key={i} className="px-3 py-2 flex justify-between gap-2">
            <span>
              <span className="font-bold">{e.actor || 'кто-то'}</span>{' '}
              {KIND_LABEL[e.kind] || e.kind}{' '}
              {e.ref_type && e.ref_id ? (
                <Link to={e.ref_type === 'movie' ? `/movie/${e.ref_id}` : `/series/${e.ref_id}`} className="underline">
                  #{e.ref_id}
                </Link>
              ) : null}
              {e.note ? <> · <span className="text-gray-600">{e.note}</span></> : null}
            </span>
            <span className="text-gray-500 whitespace-nowrap">{fmtAgo(e.happened_at)}</span>
          </div>
        ))}
      </div>
    </section>
  );
};
