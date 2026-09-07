import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api/axios';
import { useDebouncedValue } from '../hooks/useDebouncedValue';

const ActorPhoto = ({ actorId, name }) => {
  const [failed, setFailed] = useState(false);
  if (failed) {
    return (
      <div className="flex h-full w-full items-center justify-center bg-neutral-200 text-center p-3">
        <span className="text-xs font-bold uppercase leading-tight">{name}</span>
      </div>
    );
  }
  return (
    <img
      src={`/api/Media/poster/actor/${actorId}`}
      alt={name}
      className="h-full w-full object-cover"
      onError={() => setFailed(true)}
    />
  );
};

export const ActorsPage = () => {
  const [actors, setActors] = useState([]);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);
  const debouncedSearch = useDebouncedValue(search, 300);

  useEffect(() => {
    let alive = true;
    const controller = new AbortController();
    api.get('/Actors', { signal: controller.signal })
      .then(res => { if (alive) setActors(res.data); })
      .catch(() => {})
      .finally(() => { if (alive) setLoading(false); });
    return () => { alive = false; controller.abort(); };
  }, []);

  const filtered = actors.filter(a =>
    `${a.firstName} ${a.lastName}`.toLowerCase().includes(debouncedSearch.toLowerCase())
  );

  return (
    <div>
      <h1 className="text-4xl font-black uppercase mb-6 border-l-8 border-black pl-4">АКТЁРЫ</h1>
      <input
        type="text"
        placeholder="Поиск актёра..."
        className="w-full p-3 border-2 border-black outline-none focus:ring-2 focus:ring-black mb-6 font-medium"
        value={search}
        onChange={(e) => setSearch(e.target.value)}
      />

      {loading ? (
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
          {Array.from({ length: 10 }).map((_, i) => (
            <div key={i} className="aspect-[2/3] border-2 border-black animate-pulse bg-neutral-200" />
          ))}
        </div>
      ) : (
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
          {filtered.map((actor) => (
            <Link key={actor.id} to={`/actor/${actor.id}`} className="group block">
              <div className="aspect-[2/3] border-2 border-black overflow-hidden bg-neutral-900 mb-2 group-hover:shadow-[4px_4px_0_#000] transition-shadow">
                <ActorPhoto actorId={actor.id} name={`${actor.firstName} ${actor.lastName}`} />
              </div>
              <div className="text-center font-bold text-sm leading-tight uppercase group-hover:underline">
                {actor.firstName} {actor.lastName}
              </div>
            </Link>
          ))}
        </div>
      )}

      {!loading && filtered.length === 0 && (
        <p className="text-center text-lg italic text-gray-500 mt-6">Никого не найдено</p>
      )}
    </div>
  );
};
