import React, { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { api } from '../api/axios';
import { PosterImage } from '../components/PosterImage';

// Портрет актёра (или инлайн-фолбэк, если фото не загружено).
const ActorPhoto = ({ actorId, name }) => {
  const [failed, setFailed] = useState(false);
  if (failed) {
    return (
      <div className="flex h-full w-full items-center justify-center bg-neutral-900 text-white text-center p-4">
        <span className="text-sm font-bold tracking-widest uppercase leading-tight">{name}</span>
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

export const ActorPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [actor, setActor] = useState(null);
  const [credits, setCredits] = useState([]);
  const [creditsLoading, setCreditsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    let alive = true;
    const controller = new AbortController();

    Promise.all([
      api.get(`/Actors/${id}`, { signal: controller.signal }),
      api.get(`/Actors/${id}/credits`, { signal: controller.signal }),
    ])
      .then(([actorRes, creditsRes]) => {
        if (!alive) return;
        setActor(actorRes.data);
        setCredits(creditsRes.data || []);
      })
      .catch((err) => {
        if (err?.code === 'ERR_CANCELED') return;
        if (alive) setError('Не удалось загрузить актёра');
      })
      .finally(() => {
        if (alive) setCreditsLoading(false);
      });

    return () => {
      alive = false;
      controller.abort();
    };
  }, [id]);

  if (error) {
    return (
      <div className="border-2 border-red-600 p-8 text-center">
        <p className="font-bold text-red-600">{error}</p>
      </div>
    );
  }

  if (!actor) {
    return <div className="text-xl font-bold text-center mt-10 uppercase">Загрузка...</div>;
  }

  const fullName = `${actor.firstName} ${actor.lastName}`;

  return (
    <div className="max-w-[1400px] mx-auto">
      {/* Шапка */}
      <header className="grid md:grid-cols-[280px_1fr] gap-10 mb-12">
        <div className="aspect-[2/3] border-2 border-black overflow-hidden bg-neutral-900 shadow-[8px_8px_0_#000]">
          <ActorPhoto actorId={actor.id} name={fullName} />
        </div>
        <div className="flex flex-col justify-center">
          <p className="text-sm font-bold uppercase tracking-[.25em] text-neutral-500 mb-2">Актёр</p>
          <h1 className="text-5xl md:text-6xl font-black uppercase leading-none tracking-[-0.03em] mb-4">{fullName}</h1>
          {actor.birthDate && (
            <p className="text-lg font-medium text-neutral-600 mb-6">
              Дата рождения: <span className="font-bold">{new Date(actor.birthDate).toLocaleDateString('ru-RU')}</span>
            </p>
          )}
        </div>
      </header>

      {/* Биография */}
      {actor.biography && (
        <section className="mb-12">
          <h2 className="text-2xl font-black uppercase mb-4 border-b-2 border-black pb-2">БИОГРАФИЯ</h2>
          <p className="text-lg leading-relaxed text-justify font-medium whitespace-pre-line text-gray-800">{actor.biography}</p>
        </section>
      )}

      {/* Фильмография */}
      <section className="mb-12">
        <h2 className="text-2xl font-black uppercase mb-6 border-b-2 border-black pb-2">ФИЛЬМОГРАФИЯ</h2>

        {creditsLoading ? (
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4">
            {Array.from({ length: 6 }).map((_, i) => (
              <div key={i} className="aspect-[2/3] border-2 border-black animate-pulse bg-neutral-200" />
            ))}
          </div>
        ) : credits.length === 0 ? (
          <div className="border-2 border-dashed border-neutral-300 p-8 text-center font-medium text-neutral-500">
            Пока нет работ в каталоге.
          </div>
        ) : (
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4">
            {credits.map((credit) => (
              <Link key={`${credit.type}-${credit.id}`} to={credit.type === 'movie' ? `/movie/${credit.id}` : `/series/${credit.id}`} className="group block">
                <div className="aspect-[2/3] border-2 border-black overflow-hidden bg-neutral-900 mb-2 group-hover:shadow-[4px_4px_0_#000] transition-shadow">
                  <PosterImage
                    src={credit.posterUrl ? `/api/Media/poster/${credit.type === 'series' ? 'series/' : ''}${credit.id}?size=preview` : null}
                    alt={credit.title}
                  />
                </div>
                <div className="text-center">
                  <div className="font-bold text-sm leading-tight uppercase group-hover:underline">{credit.title}</div>
                  <div className="text-xs text-gray-500 flex justify-center gap-2">
                    {credit.releaseYear && <span>{credit.releaseYear}</span>}
                    {credit.averageRating > 0 && <span>★ {Number(credit.averageRating).toFixed(1)}</span>}
                    <span className="uppercase font-semibold">{credit.type === 'movie' ? 'Фильм' : 'Сериал'}</span>
                  </div>
                </div>
              </Link>
            ))}
          </div>
        )}
      </section>

      <button
        onClick={() => navigate(-1)}
        className="inline-block border-2 border-black px-4 py-2 font-bold uppercase hover:bg-black hover:text-white transition-colors"
      >
        ← Назад
      </button>
    </div>
  );
};
