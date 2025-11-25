import React, { useState } from 'react';
import { jwtDecode } from 'jwt-decode';
import { AuthContext } from './AuthContext'; // Импортируем контекст из отдельного файла

export const AuthProvider = ({ children }) => {
  // Lazy initialization: читаем токен только один раз при старте приложения
  const [user, setUser] = useState(() => {
    const token = localStorage.getItem('token');
    if (token) {
      try {
        return jwtDecode(token);
      } catch {
        localStorage.removeItem('token');
        return null;
      }
    }
    return null;
  });

  const login = (token) => {
    localStorage.setItem('token', token);
    try {
      const decoded = jwtDecode(token);
      setUser(decoded);
    } catch {
      // Если токен битый, ничего не делаем
    }
  };

  const logout = () => {
    localStorage.removeItem('token');
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, login, logout, isAuthenticated: !!user }}>
      {children}
    </AuthContext.Provider>
  );
};