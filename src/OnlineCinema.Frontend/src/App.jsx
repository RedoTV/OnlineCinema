import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import { AuthProvider } from './context/AuthProvider';
import { HomePage } from './pages/HomePage';
import { MoviePage } from './pages/MoviePage';
import { ProfilePage } from './pages/ProfilePage';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <header className="w-full border-b-2 border-black bg-white py-4 px-6 mb-8 sticky top-0 z-50">
          <div className="max-w-5xl mx-auto flex justify-between items-center">
            <nav className="flex gap-6 font-bold text-lg">
              <Link to="/" className="hover:underline decoration-2 underline-offset-4">КАТАЛОГ</Link>
              <Link to="/profile" className="hover:underline decoration-2 underline-offset-4">ЛИЧНЫЙ КАБИНЕТ</Link>
            </nav>
            {/* Можно добавить условие: если не залогинен, показывать кнопку ВХОД */}
            <Link to="/login" className="text-sm font-bold border border-black px-3 py-1 hover:bg-black hover:text-white transition-colors">
              ВХОД
            </Link>
          </div>
        </header>

        <main className="max-w-5xl mx-auto px-6 pb-12 min-h-[80vh]">
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/movie/:id" element={<MoviePage />} />
            <Route path="/profile" element={<ProfilePage />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
          </Routes>
        </main>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;