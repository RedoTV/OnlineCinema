import React, { useState, useContext } from 'react';
import { api } from '../api/axios';
import { AuthContext } from '../context/AuthContext';
import { useNavigate, Link } from 'react-router-dom';

export const RegisterPage = () => {
  const [formData, setFormData] = useState({
    username: '', email: '', password: '', firstName: '', lastName: ''
  });
  const { login } = useContext(AuthContext);
  const navigate = useNavigate();
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      const response = await api.post('/Auth/register', formData);
      if (response.data.success) {
        login(response.data.token);
        navigate('/');
      } else {
        setError(response.data.message || 'Ошибка регистрации');
      }
    } catch {
      // Убрали err
      setError('Ошибка соединения с сервером. Попробуйте позже.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="max-w-md mx-auto border-2 border-black p-8 mt-10 mb-10">
      <h2 className="text-2xl font-black uppercase mb-6 text-center">РЕГИСТРАЦИЯ</h2>

      {error && (
        <div className="mb-4 p-3 border border-red-600 bg-red-50 text-red-600 font-bold text-sm uppercase text-center">
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        {['username', 'email', 'password', 'firstName', 'lastName'].map((field) => (
          <div key={field}>
            <label className="block font-bold text-sm mb-1 uppercase">
              {field === 'username' ? 'Логин' :
                field === 'firstName' ? 'Имя' :
                  field === 'lastName' ? 'Фамилия' : field}
            </label>
            <input
              type={field === 'password' ? 'password' : field === 'email' ? 'email' : 'text'}
              name={field}
              required={['username', 'email', 'password'].includes(field)}
              className="w-full p-2 border-2 border-black outline-none focus:ring-2 focus:ring-black transition-all"
              value={formData[field]}
              onChange={handleChange}
              disabled={isLoading}
            />
          </div>
        ))}
        <button
          type="submit"
          disabled={isLoading}
          className="mt-4 bg-black text-white font-bold py-3 hover:bg-gray-800 transition-colors uppercase disabled:bg-gray-400 disabled:cursor-not-allowed"
        >
          {isLoading ? 'Загрузка...' : 'Создать аккаунт'}
        </button>
      </form>
      <div className="mt-4 text-center text-sm font-medium">
        Есть аккаунт? <Link to="/login" className="underline decoration-2 hover:text-gray-600">Войти</Link>
      </div>
    </div>
  );
};
