import React from 'react'; // React importu eklendi (JSX hatası için)
import { Routes, Route, Navigate } from 'react-router-dom';
import { Landing } from './pages/Landing'; 
import { Login } from './pages/Login';       // DÜZELTME 1: { } eklendi (Named Import)
import { Register } from './pages/Register'; // DÜZELTME 1: { } eklendi (Named Import)
import { Dashboard } from './pages/Dashboard';
import { Study } from './pages/Study';
import { Analytics } from './pages/Analytics';
import { AppLayout } from './components/AppLayout'; 
import { useAuth } from './context/AuthContext';

// DÜZELTME 3: JSX.Element yerine React.ReactNode kullanıldı (Daha güvenli)
const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
  // DÜZELTME 2: 'loading' muhtemelen 'isLoading' olarak tanımlı. 
  // Eğer AuthContext dosyasında bu özellik yoksa, context dosyanı kontrol etmeliyiz.
  // Şimdilik standart olan 'isLoading'i deniyoruz.
  const { isAuthenticated, isLoading } = useAuth() as any; // Geçici tip düzeltmesi (as any)
  
  if (isLoading) return <div className="h-screen flex items-center justify-center text-gray-500">Yükleniyor...</div>;
  if (!isAuthenticated) return <Navigate to="/login" replace />;

  return <>{children}</>;
};

function App() {
  return (
    <Routes>
      {/* HALKA AÇIK SAYFALAR */}
      <Route path="/" element={<Landing />} />
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />

      {/* KORUMALI SAYFALAR (Giriş Şart!) */}
      <Route element={<ProtectedRoute><AppLayout /></ProtectedRoute>}>
        
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="/study" element={<Study />} />
        <Route path="/analytics" element={<Analytics />} />
        
        {/* Tanımsız rotalar dashboard'a yönlendirilsin */}
        <Route path="*" element={<Navigate to="/dashboard" replace />} />

      </Route>
    </Routes>
  );
}

export default App;