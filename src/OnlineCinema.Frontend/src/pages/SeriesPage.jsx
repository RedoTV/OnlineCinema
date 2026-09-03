import React, { useEffect, useState, useContext } from 'react';
import { useParams, Link } from 'react-router-dom';
import { api } from '../api/axios';
import { CommentSection } from '../components/CommentSection';
import { VideoPlayer } from '../components/VideoPlayer';
import { AuthContext } from '../context/AuthContext';

// Страница сериалов: список (без id) или детальная (с id) со сезонами/эпизодами
export const SeriesPage = () => {
  const { id } = useParams();
  const { user } = useContext(AuthContext);
  const [seriesList, setSeriesList] = useState([]);
  const [series, setSeries] = useState(null);
  const [search, setSearch] = useState('');
  const [streamUrl, setStreamUrl] = useState(null);
  const [activeEpisode, setActiveEpisode] = useState(null);
  const [initialPosition, setInitialPosition] = useState(0);

  // список
  useEffect(() => {
    if (id) return;
    const params = search ? `?search=${search}` : '';
    api.get(`/Series${params}`)
      .then(res => setSeriesList(res.data))
      .catch(() => {});
  }, [search, id]);

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
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
          {seriesList.map(s => (
            <Link to={`/series/${s.id}`} key={s.id} className="group block border-2 border-black hover:bg-black hover:text-white transition-colors">
              <div className="aspect-[2/3] w-full overflow-hidden border-b-2 border-black group-hover:border-white">
                <img src={`/api/Media/poster/series/${s.id}`} alt={s.title} className="w-full h-full object-cover" />
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
          />
          <div className="text-white p-2 font-bold">С{activeEpisode.seasonNumber}E{activeEpisode.episodeNumber} — {activeEpisode.title}</div>
        </div>
      )}

      <p className="text-lg leading-relaxed mb-6">{series.description}</p>

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
