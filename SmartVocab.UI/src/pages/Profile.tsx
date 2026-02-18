import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { useMutation, useQuery } from '@tanstack/react-query';
import axiosClient from '../api/axiosClient';
import { useAuth } from '../context/AuthContext';
import { Button } from '../components/Button';
import { Input } from '../components/Input';
import { User, Lock, Save, LogOut, ShieldAlert, CheckCircle, Target, Globe } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

// Veri Tipleri
interface ProfileData {
  firstName: string;
  lastName: string;
  email?: string;
  dailyGoalMinutes: number;
  nativeLanguage: string;
}

export const Profile = () => {
  const { logout } = useAuth();
  const navigate = useNavigate();
  const [successMsg, setSuccessMsg] = useState<string | null>(null);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  // --- 1. VERİ ÇEKME ---
  const { data: profileData, isLoading: isProfileLoading } = useQuery<ProfileData>({
    queryKey: ['user-profile'],
    queryFn: async () => (await axiosClient.get('/user/profile')).data,
  });

  // --- FORM 1: PROFİL ---
  const { 
    register: registerProfile, 
    handleSubmit: handleProfileSubmit, 
    reset: resetProfile, 
    watch,
    formState: { errors: profileErrors, isSubmitting: isProfileSubmitting } 
  } = useForm<ProfileData>({
    defaultValues: { dailyGoalMinutes: 15, nativeLanguage: 'Turkish' }
  });

  useEffect(() => {
    if (profileData) {
      resetProfile({
        firstName: profileData.firstName,
        lastName: profileData.lastName,
        dailyGoalMinutes: profileData.dailyGoalMinutes || 15,
        nativeLanguage: profileData.nativeLanguage || 'Turkish',
        email: profileData.email
      });
    }
  }, [profileData, resetProfile]);

  const currentGoal = watch('dailyGoalMinutes');

  const profileMutation = useMutation({
    mutationFn: async (data: ProfileData) => {
      await axiosClient.put('/user/profile', {
        firstName: data.firstName,
        lastName: data.lastName,
        dailyGoalMinutes: Number(data.dailyGoalMinutes),
        nativeLanguage: data.nativeLanguage
      });
    },
    onSuccess: () => {
      setSuccessMsg("Profil bilgileri güncellendi.");
      setTimeout(() => setSuccessMsg(null), 3000);
    },
    onError: (err: any) => {
      setErrorMsg(err.response?.data?.error || "Güncelleme başarısız.");
      setTimeout(() => setErrorMsg(null), 5000);
    }
  });

  // --- FORM 2: ŞİFRE DEĞİŞTİRME (DÜZELTİLDİ) ---
  const { 
    register: registerPass, 
    handleSubmit: handlePassSubmit, 
    reset: resetPass, 
    watch: watchPass, // Şifreleri karşılaştırmak için izliyoruz
    formState: { errors: passErrors, isSubmitting: isPassSubmitting } 
  } = useForm();

  // Yeni şifreyi anlık takip et (Tekrar şifresiyle kıyaslamak için)
  const newPasswordValue = watchPass('newPassword');

// ... (yukarıdaki kodlar aynı kalsın)

  const passwordMutation = useMutation({
    mutationFn: async (data: any) => {
      // DÜZELTME: Backend DTO'su ile %100 eşleşen isimlendirme
      await axiosClient.post('/user/change-password', {
        OldPassword: data.currentPassword,       // Backend: OldPassword
        NewPassword: data.newPassword,           // Backend: NewPassword
        ConfirmNewPassword: data.confirmPassword // Backend: ConfirmNewPassword
      });
    },
    onSuccess: () => {
      setSuccessMsg("Şifreniz başarıyla değiştirildi.");
      resetPass(); // Formu temizle
      setTimeout(() => setSuccessMsg(null), 3000);
    },
    onError: (err: any) => {
      console.error("Şifre Değiştirme Hatası:", err);
      // Backend'den gelen hatayı yakala
      const serverError = err.response?.data?.errors 
        ? Object.values(err.response.data.errors).flat().join(', ') 
        : (err.response?.data?.Error || "Şifre değiştirilemedi. Mevcut şifrenizi kontrol edin.");
      
      setErrorMsg(serverError);
      setTimeout(() => setErrorMsg(null), 5000);
    }
  });

  // ... (aşağıdaki kodlar aynı kalsın)

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  if (isProfileLoading) return <div className="text-center p-20 animate-pulse text-gray-400">Profil Yükleniyor...</div>;

  return (
    <div className="max-w-5xl mx-auto pb-10 animate-fade-in">
      
      {/* HEADER */}
      <div className="mb-8 flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h2 className="text-3xl font-bold text-gray-900 dark:text-white">Hesap Ayarları</h2>
          <p className="text-gray-500 mt-1">Öğrenme hedeflerin ve kimlik bilgilerin.</p>
        </div>
        <button 
          onClick={handleLogout}
          className="flex items-center gap-2 px-5 py-2.5 text-red-600 bg-red-50 dark:bg-red-900/10 rounded-xl hover:bg-red-100 transition-colors font-medium text-sm border border-red-100 dark:border-transparent"
        >
          <LogOut size={18} />
          Oturumu Kapat
        </button>
      </div>

      {/* MESAJLAR */}
      {successMsg && (
        <div className="mb-6 p-4 bg-green-50 dark:bg-green-900/20 text-green-700 dark:text-green-300 rounded-xl flex items-center gap-3 border border-green-100 dark:border-green-900/30">
          <CheckCircle size={20} />
          {successMsg}
        </div>
      )}
      {errorMsg && (
        <div className="mb-6 p-4 bg-red-50 dark:bg-red-900/20 text-red-700 dark:text-red-300 rounded-xl flex items-center gap-3 border border-red-100 dark:border-red-900/30">
          <ShieldAlert size={20} />
          {errorMsg}
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        {/* SOL KOLON */}
        <div className="space-y-6">
          <div className="bg-white dark:bg-neutral-900 p-8 rounded-[2rem] border border-gray-100 dark:border-white/5 shadow-sm text-center">
            <div className="w-24 h-24 bg-gradient-to-tr from-brand-primary to-purple-600 rounded-full mx-auto flex items-center justify-center text-white text-3xl font-bold mb-4 shadow-xl shadow-brand-primary/20">
              {profileData?.firstName?.charAt(0) || 'U'}
            </div>
            <h3 className="text-xl font-bold text-gray-900 dark:text-white">{profileData?.firstName} {profileData?.lastName}</h3>
            <p className="text-gray-500 text-sm mb-4">{profileData?.email}</p>
            <span className="inline-flex items-center px-3 py-1 bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400 text-xs font-bold rounded-full border border-blue-100 dark:border-blue-900/30">
              Hedef: {profileData?.dailyGoalMinutes}dk/gün
            </span>
          </div>
        </div>

        {/* SAĞ KOLON */}
        <div className="lg:col-span-2 space-y-8">
          
          {/* PROFİL AYARLARI */}
          <div className="bg-white dark:bg-neutral-900 p-8 rounded-[2rem] border border-gray-100 dark:border-white/5 shadow-sm">
            <div className="flex items-center gap-3 mb-6">
              <div className="p-2.5 bg-gray-100 dark:bg-neutral-800 rounded-xl text-gray-600 dark:text-gray-300">
                <User size={20} />
              </div>
              <h3 className="text-lg font-bold text-gray-900 dark:text-white">Kimlik & Hedefler</h3>
            </div>
            
            <form onSubmit={handleProfileSubmit((data) => profileMutation.mutate(data))} className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                <Input label="İsim" {...registerProfile('firstName', { required: 'Gerekli' })} error={profileErrors.firstName?.message as string} />
                <Input label="Soyisim" {...registerProfile('lastName', { required: 'Gerekli' })} error={profileErrors.lastName?.message as string} />
              </div>

              <div className="p-5 bg-gray-50 dark:bg-neutral-800/50 rounded-2xl border border-gray-100 dark:border-white/5">
                <div className="flex items-center justify-between mb-4">
                  <div className="flex items-center gap-2 text-gray-700 dark:text-gray-300 font-medium">
                    <Target size={18} className="text-brand-primary" /> Günlük Hedef
                  </div>
                  <span className="text-2xl font-bold text-brand-primary">{currentGoal} dk</span>
                </div>
                <input type="range" min="5" max="120" step="5" className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer accent-brand-primary" {...registerProfile('dailyGoalMinutes')} />
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                <div className="space-y-1">
                  <label className="text-sm font-medium text-gray-700 dark:text-gray-300 flex items-center gap-2"><Globe size={14} /> Ana Dil</label>
                  <select {...registerProfile('nativeLanguage')} className="w-full p-3 bg-white dark:bg-neutral-950 border border-gray-200 dark:border-neutral-800 rounded-xl outline-none">
                    <option value="Turkish">Türkçe</option>
                    <option value="English">English</option>
                  </select>
                </div>
                <Input label="E-posta" disabled className="opacity-60 bg-gray-100 dark:bg-neutral-800" {...registerProfile('email')} />
              </div>
              
              <div className="flex justify-end pt-2">
                <Button type="submit" isLoading={isProfileSubmitting || profileMutation.isPending} className="w-auto px-8">
                  <Save size={18} className="mr-2" /> Kaydet
                </Button>
              </div>
            </form>
          </div>

          {/* GÜVENLİK AYARLARI (ŞİFRE) */}
          <div className="bg-white dark:bg-neutral-900 p-8 rounded-[2rem] border border-gray-100 dark:border-white/5 shadow-sm">
            <div className="flex items-center gap-3 mb-6">
              <div className="p-2.5 bg-orange-50 dark:bg-orange-900/20 rounded-xl text-orange-600">
                <Lock size={20} />
              </div>
              <h3 className="text-lg font-bold text-gray-900 dark:text-white">Şifre Değiştir</h3>
            </div>

            <form onSubmit={handlePassSubmit((data) => passwordMutation.mutate(data))} className="space-y-5">
              <Input 
                label="Mevcut Şifre" 
                type="password" 
                {...registerPass('currentPassword', { required: 'Mevcut şifrenizi giriniz' })}
                error={passErrors.currentPassword?.message as string}
              />
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                <Input 
                  label="Yeni Şifre" 
                  type="password" 
                  {...registerPass('newPassword', { 
                    required: 'Yeni şifre gerekli', 
                    minLength: { value: 6, message: 'En az 6 karakter olmalı' } 
                  })}
                  error={passErrors.newPassword?.message as string}
                />
                
                {/* DÜZELTME: Front-end Validasyonu (Eşleşme Kontrolü) */}
                <Input 
                  label="Yeni Şifre (Tekrar)" 
                  type="password" 
                  {...registerPass('confirmPassword', { 
                    required: 'Tekrar gerekli',
                    validate: (value) => value === newPasswordValue || "Şifreler eşleşmiyor!" 
                  })}
                  error={passErrors.confirmPassword?.message as string}
                />
              </div>

              <div className="flex justify-end pt-2">
                <Button variant="outline" type="submit" isLoading={isPassSubmitting || passwordMutation.isPending} className="w-auto px-8 border-gray-200 dark:border-neutral-700">
                  Şifreyi Güncelle
                </Button>
              </div>
            </form>
          </div>

        </div>
      </div>
    </div>
  );
};