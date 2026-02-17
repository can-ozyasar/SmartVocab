import React, { createContext, useContext, useState, useEffect } from 'react';
import axiosClient from '../api/axiosClient';

// Kullanıcı Tipi (Backend'den gelen veri)
interface User {
  id: string;
  email: string;
  firstName: string;
}

// Context Tipi
interface AuthContextType {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  login: (token: string, user: User) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(localStorage.getItem('token'));

  // Sayfa yenilenince Token varsa kullanıcıyı hatırla
  useEffect(() => {
    if (token) {
      // Normalde burada /api/user/me gibi bir endpoint ile kullanıcı verisini tazelemek gerekir.
      // Şimdilik token varsa "Giriş yapılmış" sayıyoruz.
      axiosClient.defaults.headers.Authorization = `Bearer ${token}`;
    }
  }, [token]);

  const login = (newToken: string, newUser: User) => {
    localStorage.setItem('token', newToken); // Tarayıcı hafızasına yaz
    setToken(newToken);
    setUser(newUser);
    axiosClient.defaults.headers.Authorization = `Bearer ${newToken}`;
  };

  const logout = () => {
    localStorage.removeItem('token');
    setToken(null);
    setUser(null);
    delete axiosClient.defaults.headers.Authorization;
  };

  return (
    <AuthContext.Provider value={{ user, token, isAuthenticated: !!token, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

// Hook: Diğer sayfalarda "useAuth()" diyerek verilere ulaşacağız.
export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within an AuthProvider');
  return context;
};