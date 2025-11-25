import React, { useState, useContext } from 'react';
import { api } from '../api/axios';
import { AuthContext } from '../context/AuthContext';
import { useNavigate, Link } from 'react-router-dom';

export const LoginPage = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const { login } = useContext(AuthContext);
  const navigate = useNavigate();
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(''); // Сбрасываем старую ошибку перед новым запросом
    try {
      const response = await api.post('/Auth/login', { email, password });
      if (response.data.success) {
        login(response.data.token);
        navigate('/profile');
      } else {
        setError(response.data.message || 'Неверный логин или пароль');
      }
    } catch {
      // Убрали переменную (err), раз не используем её для логов
      setError('Ошибка соединения с сервером');
    }
  };

  return (
    <div className="max-w-md mx-auto border-2 border-black p-8 mt-10">
      <h2 className="text-2xl font-black uppercase mb-6 text-center">ВХОД В СИСТЕМУ</h2>

      {/* Блок ошибки - красный текст в рамке */}
      {error && (
        <div className="mb-4 p-3 border border-red-600 bg-red-50 text-red-600 font-bold text-sm uppercase text-center">
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        <div>
          <label className="block font-bold text-sm mb-1 uppercase">Email</label>
          <input
            type="email"
            required
            className="w-full p-2 border-2 border-black outline-none focus:ring-2 focus:ring-black transition-all"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
        </div>
        <div>
          <label className="block font-bold text-sm mb-1 uppercase">Пароль</label>
          <input
            type="password"
            required
            className="w-full p-2 border-2 border-black outline-none focus:ring-2 focus:ring-black transition-all"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
        </div>
        <button
          type="submit"
          className="mt-4 bg-black text-white font-bold py-3 hover:bg-gray-800 transition-colors uppercase active:translate-y-0.5"
        >
          Войти
        </button>
      </form>
      <div className="mt-4 text-center text-sm font-medium">
        Нет аккаунта? <Link to="/register" className="underline decoration-2 hover:text-gray-600">Зарегистрироваться</Link>
      </div>
    </div>
  );
};
