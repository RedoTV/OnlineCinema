import React, { useEffect, useState, useContext } from 'react';
import { useParams, Link } from 'react-router-dom';
import { api } from '../api/axios';
import { CommentSection } from '../components/CommentSection';
import { VideoPlayer } from '../components/VideoPlayer';
import { PosterImage } from '../components/PosterImage';
import { useDebouncedValue } from '../hooks/useDebouncedValue';
import { AuthContext } from '../context/AuthContext';

const SKELETON_COUNT = 8;

const SeriesListSkeleton = () => (
  <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6" aria-hidden="true">
    {Array.from({ length: SKELETON_COUNT }).map((_, i) => (
      <div key={i} className="border-2 border-black">
        <div className="aspect-[2/3] w-full animate-pulse bg-neutral-200" />
        <div className="space-y-2 p-3">
          <div className="h-4 animate-pulse bg-neutral-200" />
          <div className="h-3 w-2/3 animate-pulse bg-neutral-200" />
        </div>
      </div>
    ))}
  </div>
);

// Страница сериалов: список (без id) или детальная (с id) со сезонами/эпизодами
export const SeriesPage = () => {
  const { id } = useParams();
  const { user } = useContext(AuthContext);
  const [seriesList, setSeriesList] = useState([]);
  const [series, setSeries] = useState(null);
  const [search, setSearch] = useState('');
  const [listLoading, setListLoading] = useState(false);
  const [listError, setListError] = useState('');
  const [listRetry, setListRetry] = useState(0);
  // Поиск бьёт в API только после паузы ввода, а не на каждую клавишу.
  const debouncedSearch = useDebouncedValue(search, 350);
  const [streamUrl, setStreamUrl] = useState(null);
  const [activeEpisode, setActiveEpisode] = useState(null);
  const [initialPosition, setInitialPosition] = useState(0);
  const [myRating, setMyRating] = useState(8);

  const handleRate = async () => {
    try {
      await api.post('/UserActions/rating', { movieId: null, seriesId: Number(id), rating: Number(myRating) });
      const sd = await api.get(`/Series/${id}`);
      setSeries(sd.data);
    } catch (e) {
      console.error('rating failed', e);
    }
  };

  // список
  useEffect(() => {
    if (id) return;
    const controller = new AbortController();
    setListLoading(true);
    setListError('');

    const params = debouncedSearch ? `?search=${encodeURIComponent(debouncedSearch)}` : '';
    api.get(`/Series${params}`, { signal: controller.signal })
      .then(res => setSeriesList(res.data))
      .catch((err) => {
        if (err?.code === 'ERR_CANCELED') return;
        console.error('series list failed', err);
        setListError('Не удалось загрузить сериалы');
      })
      .finally(() => {
        if (!controller.signal.aborted) setListLoading(false);
      });

    // Устаревший ответ поиска не затирает свежий.
    return () => controller.abort();
  }, [debouncedSearch, listRetry, id]);

  // детальная
  useEffect(() => {
    if (!id) return;
    api.get(`/Series/${id}`)
      .then(res => setSeries(res.data))
      .catch(() => {});
  }, [id]);

  const playEpisode = async (episode) => {
    setActiveEpisode(episode);
    setInitialPosition(0);
    setStreamUrl(null);

    const [streamRes, progRes] = await Promise.all([
      api.get(`/Streaming/episode/${episode.id}`).catch(() => null),
      user ? api.get(`/Playback/progress?episodeId=${episode.id}`).catch(() => null) : null,
    ]);

    setStreamUrl(streamRes?.data?.url);
    const pos = progRes?.data?.positionSeconds || 0;
    if (pos > 30) setInitialPosition(pos);
  };

  // следующая серия по порядку сезонов (авто-транзишн после конца эпизода)
  const autoNext = () => {
    const flat = [];
    (series?.seasons || []).forEach(s => {
      (s.episodes || []).forEach(ep => flat.push({ ...ep, seasonNumber: s.seasonNumber }));
    });
    const idx = flat.findIndex(e => e.id === activeEpisode?.id);
    const next = flat[idx + 1];
    if (next) playEpisode(next);
  };

  // ---- СПИСОК ----
  if (!id) {
    return (
      <div>
        <h1 className="text-4xl font-black uppercase mb-6 border-l-8 border-black pl-4">СЕРИАЛЫ</h1>
        <input
          type="text"
          placeholder="Поиск сериала..."
          className="w-full p-3 border-2 border-black outline-none mb-6"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        {listError && (
          <div className="mb-6 border-2 border-red-600 p-4 text-center">
            <p className="font-bold text-red-600">{listError}</p>
            <button
              onClick={() => setListRetry((n) => n + 1)}
              className="mt-2 border-2 border-black px-4 py-1 font-bold hover:bg-black hover:text-white"
            >
              Повторить
            </button>
          </div>
        )}
        {listLoading && seriesList.length === 0 ? (
          <SeriesListSkeleton />
        ) : (
          <div className={`grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6${listLoading ? ' opacity-60' : ''}`}>
            {seriesList.map((s, i) => (
              <Link to={`/series/${s.id}`} key={s.id} className="group block border-2 border-black hover:bg-black hover:text-white transition-colors">
                <div className="relative aspect-[2/3] w-full overflow-hidden border-b-2 border-black group-hover:border-white">
                  <PosterImage
                    src={s.posterUrl ? `/api/Media/poster/series/${s.id}` : null}
                    alt={s.title}
                    eager={i < 4}
                  />
                </div>
                <div className="p-3">
                  <h3 className="font-bold text-lg leading-tight uppercase">{s.title}</h3>
                  <div className="flex justify-between text-sm font-medium">
                    <span>{s.releaseYear}</span>
                    <span>★ {s.averageRating?.toFixed(1)} · {s.seasonsCount} сез.</span>
                  </div>
                </div>
              </Link>
            ))}
          </div>
        )}
        {!listLoading && !listError && seriesList.length === 0 && (
          <p className="mt-6 text-center text-lg italic text-gray-500">Ничего не найдено</p>
        )}
      </div>
    );
  }

  // ---- ДЕТАЛЬНАЯ ----
  if (!series) return <div className="text-xl font-bold text-center mt-10 uppercase">Загрузка...</div>;

  return (
    <div>
      <h1 className="text-4xl font-black uppercase mb-6 border-l-8 border-black pl-4">{series.title}</h1>

      {activeEpisode && (
        <div className="mb-6 border-2 border-black p-1 bg-black">
          <VideoPlayer
            key={activeEpisode.id}
            streamUrl={streamUrl}
            episodeId={activeEpisode.id}
            initialPosition={initialPosition}
            onEnded={autoNext}
          />
          <div className="text-white p-2 font-bold">С{activeEpisode.seasonNumber}E{activeEpisode.episodeNumber} — {activeEpisode.title}</div>
        </div>
      )}

      <p className="text-lg leading-relaxed mb-6">{series.description}</p>

      {user && (
        <div className="flex items-center gap-3 mb-6 border-2 border-black p-2 w-fit">
          <span className="font-bold">Ваша оценка сериала:</span>
          <select
            value={myRating}
            onChange={(e) => setMyRating(e.target.value)}
            className="border-2 border-black px-2 py-0.5"
          >
            {[...Array(10)].map((_, i) => <option key={i + 1} value={i + 1}>{i + 1}</option>)}
          </select>
          <button onClick={handleRate} className="border-2 border-black px-3 py-0.5 font-bold hover:bg-black hover:text-white">
            Поставить
          </button>
          <span className="text-sm text-gray-600">★ {series.averageRating?.toFixed(1)}</span>
        </div>
      )}

      <div className="space-y-6">
        {series.seasons?.map(season => (
          <div key={season.id} className="border-2 border-black">
            <div className="bg-black text-white px-4 py-2 font-bold uppercase">
              Сезон {season.seasonNumber}{season.title ? ` — ${season.title}` : ''}
            </div>
            <div className="divide-y divide-black">
              {season.episodes?.map(ep => (
                <button
                  key={ep.id}
                  onClick={() => playEpisode({ ...ep, seasonNumber: season.seasonNumber })}
                  className="w-full flex justify-between items-center px-4 py-2 hover:bg-gray-100 text-left"
                >
                  <span className="font-medium">
                    <span className="font-bold mr-2">Э{ep.episodeNumber}</span> {ep.title}
                  </span>
                  <span className="text-sm text-gray-500">{ep.duration ? `${ep.duration} мин` : ''}</span>
                </button>
              ))}
            </div>
          </div>
        ))}
      </div>

      <CommentSection seriesId={Number(id)} />
    </div>
  );
};
