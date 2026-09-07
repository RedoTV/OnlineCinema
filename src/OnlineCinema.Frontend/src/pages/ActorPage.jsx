import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
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
  const [actor, setActor] = useState(null);
  const [credits, setCredits] = useState([]);
  const [error, setError] = useState('');

  useEffect(() => {
    let alive = true;
    const controller = new AbortController();

    Promise.all([
      api.get(`/Actors/${id}`, { signal: controller.signal }),
      api.get(`/Actors`, { signal: controller.signal }),
    ])
      .then(([actorRes, allRes]) => {
        if (!alive) return;
        setActor(actorRes.data);
        // Находим работы актёра по всем актёрам; в идеале бэкенд отдаёт кредиты напрямую.
        setCredits(allRes.data || []);
      })
      .catch((err) => {
        if (err?.code === 'ERR_CANCELED') return;
        setError('Не удалось загрузить актёра');
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
        <p className="text-sm italic text-gray-500 mb-4">
          Список работ обновляется по мере наполнения каталога.
        </p>
        {/* Здесь должен быть список фильмов/сериалов актёра. Бэкенд пока не отдаёт кредиты,
            поэтому показываем пустую заглушку, если данных нет. */}
        {credits.length === 0 && (
          <div className="border-2 border-dashed border-neutral-300 p-8 text-center font-medium text-neutral-500">
            Фильмография появится, когда у этого актёра будут работы в каталоге.
          </div>
        )}
      </section>

      <Link to="/actors" className="inline-block border-2 border-black px-4 py-2 font-bold uppercase hover:bg-black hover:text-white transition-colors">
        ← Все актёры
      </Link>
    </div>
  );
};
