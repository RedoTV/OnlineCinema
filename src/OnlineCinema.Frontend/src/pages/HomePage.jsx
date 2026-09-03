import React, { useEffect, useState, useContext } from 'react';
import { api } from '../api/axios';
import { Link } from 'react-router-dom';
import { AuthContext } from '../context/AuthContext';
import { RecommendationRow } from '../components/RecommendationRow';
import { ActivityFeed } from '../components/ActivityFeed';

export const HomePage = () => {
  const { user } = useContext(AuthContext);
  const [movies, setMovies] = useState([]);
  const [search, setSearch] = useState('');

  // sub в JWT = userId. Идёт в analytics для рекомендаций
  const userId = user ? Number(user.sub) : null;
  const username = user?.username;

  useEffect(() => {
    const fetchMovies = async () => {
      try {
        const params = new URLSearchParams();
        if (search) params.append('search', search);
        const response = await api.get(`/Movies?${params.toString()}`);
        setMovies(response.data);
      } catch (error) {
        console.error("Error:", error);
      }
    };
    fetchMovies();
  }, [search]);

  return (
    <div>
      {/* Поиск */}
      <div className="mb-8">
        <input
          type="text"
          placeholder="Поиск фильма по названию..."
          className="w-full p-3 border-2 border-black outline-none focus:ring-2 focus:ring-black transition-all font-medium placeholder-gray-500"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      {/* v3.0: персонализация + live-активность из analytics */}
      <RecommendationRow userId={userId} username={username} />
      <ActivityFeed userId={userId} />

      {/* Каталог */}
      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
        {movies.map(movie => (
          <Link to={`/movie/${movie.id}`} key={movie.id} className="group block border-2 border-black hover:bg-black hover:text-white transition-colors duration-200">
            <div className="aspect-[2/3] w-full overflow-hidden border-b-2 border-black group-hover:border-white">
              <img
                src={movie.posterUrl ? `/api/Media/poster/${movie.id}` : '/placeholder.jpg'}
                alt={movie.title}
                className="w-full h-full object-cover"
              />
            </div>
            <div className="p-3">
              <h3 className="font-bold text-lg leading-tight mb-1 uppercase">{movie.title}</h3>
              <div className="flex justify-between text-sm font-medium">
                <span>{movie.releaseYear}</span>
                <span>★ {movie.averageRating?.toFixed(1)}</span>
              </div>
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
};
