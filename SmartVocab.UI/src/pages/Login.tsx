import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Input } from '../components/Input';
import { Button } from '../components/Button';
import { useAuth } from '../context/AuthContext';
import axiosClient from '../api/axiosClient';

import { BookOpen } from 'lucide-react'; // Logo ikonu

export const Login = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError('');

    try {
      // Backend'e istek at
      const response = await axiosClient.post('/user/login', { email, password });
      
      // Başarılıysa Context'i güncelle
      // (Backend'den Token ve User bilgisi dönmeli. Eğer User dönmüyorsa Token'ı decode edebiliriz)
      // Şimdilik user objesini manuel oluşturuyoruz, sonra düzelteceğiz.
      const { token } = response.data;
      
      login(token, { id: '1', email, firstName: 'User' }); // Geçici user verisi
      
      // Dashboard'a yönlendir
      navigate('/dashboard');
      
    } catch (err: any) {
      setError(err.response?.data?.error || 'Giriş yapılamadı. Bilgilerini kontrol et.');
    } finally {
      setIsLoading(false);
    }
  };

  
  // ... (Importlar ve fonksiyon başı aynı kalsın) ...

  return (
    // Arka plan: Aydınlıkta gri (gray-50), Karanlıkta koyu lacivert (brand-dark)
    <div className="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-brand-dark px-4 transition-colors duration-300">
      
      {/* Kart: Aydınlıkta beyaz, Karanlıkta koyu gri (slate-900) */}
      <div className="max-w-md w-full bg-white dark:bg-slate-900 rounded-3xl shadow-xl p-8 border border-gray-100 dark:border-slate-800 transition-colors duration-300">
        
        {/* Header */}
        <div className="text-center mb-8">
          <div className="w-16 h-16 bg-brand-primary/10 rounded-2xl flex items-center justify-center mx-auto mb-4 text-brand-primary">
            <BookOpen size={32} />
          </div>
          {/* Başlık: Siyah -> Beyaz */}
          <h2 className="text-3xl font-bold text-gray-900 dark:text-white">Tekrar Hoşgeldin</h2>
          <p className="text-gray-500 dark:text-gray-400 mt-2">Kaldığın yerden devam etmeye hazır mısın?</p>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit} className="space-y-6">
          {error && (
            <div className="p-4 bg-red-50 dark:bg-red-900/20 text-brand-danger dark:text-red-400 text-sm rounded-xl border border-red-100 dark:border-red-800">
              {error}
            </div>
          )}

          <Input 
            label="E-Posta Adresi" 
            type="email" 
            placeholder="ornek@email.com"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
          
          <Input 
            label="Şifre" 
            type="password" 
            placeholder="••••••••"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />

          <div className="flex items-center justify-between text-sm">
            <label className="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" className="w-4 h-4 rounded text-brand-primary focus:ring-brand-primary border-gray-300 dark:border-slate-600 dark:bg-slate-800" />
              <span className="text-gray-600 dark:text-gray-400">Beni Hatırla</span>
            </label>
            <a href="#" className="text-brand-primary font-medium hover:underline">Şifremi Unuttum?</a>
          </div>

          <Button type="submit" isLoading={isLoading}>
            Giriş Yap
          </Button>
        </form>

        {/* Footer */}
        <div className="mt-8 text-center text-sm text-gray-500 dark:text-gray-400">
          Hesabın yok mu?{' '}
          <Link to="/register" className="text-brand-primary font-bold hover:underline">
            Hemen Kayıt Ol
          </Link>
        </div>
      </div>
    </div>
  );
};



///DAHA KARANLIK BİR TASARIM İÇİN
// // ... Importlar aynı ...

//   return (
//     // AYDINLIK: Arka planı gray-100 yaptık (Daha belirgin gri)
//     // KOYU: Arka planı neutral-950 yaptık (Premium Siyah-Gri karışımı)
//     <div className="min-h-screen flex items-center justify-center bg-gray-100 dark:bg-neutral-950 px-4 transition-colors duration-300">
      
//       {/* KART: Shadow-2xl ile havada duruyor hissi verdik */}
//       <div className="max-w-md w-full bg-white dark:bg-neutral-900 rounded-3xl shadow-2xl p-8 border border-white/50 dark:border-white/5 transition-all duration-300">
        
//         <div className="text-center mb-8">
//           {/* Logo kutusunu daha yumuşak yaptık */}
//           <div className="w-16 h-16 bg-blue-50 dark:bg-blue-900/20 rounded-2xl flex items-center justify-center mx-auto mb-4 text-brand-primary transition-colors">
//             <BookOpen size={32} />
//           </div>
//           <h2 className="text-3xl font-bold text-gray-900 dark:text-white tracking-tight">Tekrar Hoşgeldin</h2>
//           <p className="text-gray-500 dark:text-gray-400 mt-2 font-medium">Kaldığın yerden devam etmeye hazır mısın?</p>
//         </div>

//         <form onSubmit={handleSubmit} className="space-y-5">
//           {error && (
//             <div className="p-4 bg-red-50 dark:bg-red-900/20 text-brand-danger dark:text-red-400 text-sm font-medium rounded-xl border border-red-100 dark:border-red-800 flex items-center gap-2">
//                <span>⚠️</span> {error}
//             </div>
//           )}

//           <Input 
//             label="E-Posta Adresi" 
//             type="email" 
//             placeholder="ornek@email.com"
//             value={email}
//             onChange={(e) => setEmail(e.target.value)}
//             required
//           />
          
//           <Input 
//             label="Şifre" 
//             type="password" 
//             placeholder="••••••••"
//             value={password}
//             onChange={(e) => setPassword(e.target.value)}
//             required
//           />

//           <div className="flex items-center justify-between text-sm">
//             <label className="flex items-center gap-2 cursor-pointer group">
//               <input type="checkbox" className="w-4 h-4 rounded text-brand-primary focus:ring-brand-primary border-gray-300 cursor-pointer" />
//               <span className="text-gray-600 dark:text-gray-400 group-hover:text-gray-900 transition-colors font-medium">Beni Hatırla</span>
//             </label>
//             <a href="#" className="text-brand-primary font-semibold hover:text-blue-700 transition-colors">Şifremi Unuttum?</a>
//           </div>

//           {/* Butona gölge ekledik */}
//           <Button type="submit" isLoading={isLoading} className="shadow-lg shadow-blue-500/30 hover:shadow-blue-500/50">
//             Giriş Yap
//           </Button>
//         </form>

//         <div className="mt-8 text-center text-sm text-gray-500 dark:text-gray-400 font-medium">
//           Hesabın yok mu?{' '}
//           <Link to="/register" className="text-brand-primary font-bold hover:underline">
//             Hemen Kayıt Ol
//           </Link>
//         </div>
//       </div>
//     </div>
//   );
// };