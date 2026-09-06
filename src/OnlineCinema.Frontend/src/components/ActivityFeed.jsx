import { useEffect, useState } from 'react';
import { api } from '../api/axios';
import { Link } from 'react-router-dom';

const KIND_LABEL = {
  watched: 'посмотрел',
  rated: 'оценил',
  comment: 'прокомментировал',
};

const fmtAgo = (iso) => {
  const d = new Date(iso);
  const s = Math.floor((Date.now() - d.getTime()) / 1000);
  if (s < 60) return 'только что';
  if (s < 3600) return `${Math.floor(s / 60)} мин назад`;
  if (s < 86400) return `${Math.floor(s / 3600)} ч назад`;
  return d.toLocaleDateString('ru-RU');
};

export const ActivityFeed = ({ userId }) => {
  const [items, setItems] = useState([]);

  useEffect(() => {
    const qs = userId ? `userId=${userId}&limit=12` : 'limit=15';
    api.get(`/analytics/activity?${qs}`)
      .then(res => setItems(res.data || []))
      .catch(() => setItems([]));
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
              {e.refType && e.refId ? (
                <Link to={e.refType === 'movie' ? `/movie/${e.refId}` : `/series/${e.refId}`} className="underline">
                  «{e.title || `#${e.refId}`}»
                </Link>
              ) : null}
              {e.note ? <> · <span className="text-gray-600">{e.note}</span></> : null}
            </span>
            <span className="text-gray-500 whitespace-nowrap">{fmtAgo(e.happenedAt)}</span>
          </div>
        ))}
      </div>
    </section>
  );
};
