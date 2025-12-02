import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './context/AuthProvider';
import { Header } from './Header';
import { HomePage } from './pages/HomePage';
import { MoviePage } from './pages/MoviePage';
import { ProfilePage } from './pages/ProfilePage';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Header />
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
