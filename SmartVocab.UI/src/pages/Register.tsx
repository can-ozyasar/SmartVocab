import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Input } from '../components/Input';
import { Select } from '../components/Select'; // <-- Yeni bileşen
import { Button } from '../components/Button';
import { useForm, Controller } from 'react-hook-form'; 
import axiosClient from '../api/axiosClient';
import { UserPlus } from 'lucide-react';

interface RegisterFormInputs {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
  dailyGoalMinutes: string; // Select'ten string gelir, gönderirken sayı yapacağız
  nativeLanguage: string;
}

export const Register = () => {
  const navigate = useNavigate();
  const [serverError, setServerError] = useState('');
  
const { 
    register, 
    handleSubmit, 
    watch, 
    control, // <-- Control nesnesini de alıyoruz (Controller için gerekli)
    formState: { errors, isSubmitting } 
  } = useForm<RegisterFormInputs>({
    defaultValues: {
      dailyGoalMinutes: "15",
      nativeLanguage: "Turkish"
    }
  });

  const password = watch('password');

  const onSubmit = async (data: RegisterFormInputs) => {
    setServerError('');
    
    try {
      const payload = {
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        password: data.password,
        // String gelen değeri Integer'a çeviriyoruz
        dailyGoalMinutes: parseInt(data.dailyGoalMinutes),
        nativeLanguage: data.nativeLanguage
      };

      await axiosClient.post('/user/register', payload);
      navigate('/login');
      
    } catch (err: any) {
      setServerError(err.response?.data?.Error || 'Kayıt işlemi başarısız.');
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-100 dark:bg-neutral-950 px-4 transition-colors duration-300 py-10">
      <div className="max-w-md w-full bg-white dark:bg-neutral-900 rounded-3xl shadow-2xl p-8 border border-white/50 dark:border-white/5 transition-all duration-300">
        
        <div className="text-center mb-8">
          <div className="w-16 h-16 bg-brand-primary/10 rounded-2xl flex items-center justify-center mx-auto mb-4 text-brand-primary">
            <UserPlus size={32} />
          </div>
          <h2 className="text-3xl font-bold text-gray-900 dark:text-white">Aramıza Katıl</h2>
          <p className="text-gray-500 dark:text-gray-400 mt-2">Profilini oluştur ve hedeflerini belirle.</p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          
          {serverError && (
            <div className="p-3 bg-red-50 text-brand-danger text-sm rounded-lg border border-red-100 font-bold">
              ⚠️ {serverError}
            </div>
          )}

          <div className="grid grid-cols-2 gap-4">
            <Input 
              label="Ad" 
              placeholder="Ahmet"
              error={errors.firstName?.message}
              {...register('firstName', { required: 'Ad zorunludur' })}
            />
            <Input 
              label="Soyad" 
              placeholder="Yılmaz"
              error={errors.lastName?.message}
              {...register('lastName', { required: 'Soyad zorunludur' })}
            />
          </div>

          <Input 
            label="E-Posta" 
            type="email" 
            placeholder="ahmet@ornek.com"
            error={errors.email?.message}
            {...register('email', { 
              required: 'E-Posta zorunludur',
              pattern: {
                value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                message: "Geçersiz e-posta adresi"
              }
            })}
          />

          {/* YENİ ALANLAR: Controller ile Custom Select Kullanımı */}
          <div className="grid grid-cols-2 gap-4">
            
            {/* Günlük Hedef Seçimi */}
            <Controller
              name="dailyGoalMinutes"
              control={control}
              render={({ field }) => (
                <Select
                  label="Günlük Hedef"
                  options={[
                    { value: "5", label: "5 Dakika (Hafif)" },
                    { value: "15", label: "15 Dakika (Normal)" },
                    { value: "30", label: "30 Dakika (Ciddi)" },
                    { value: "60", label: "60 Dakika (Yoğun)" },
                  ]}
                  value={field.value} // Formdaki değeri bileşene ver
                  onChange={field.onChange} // Bileşendeki değişimi forma bildir
                  error={errors.dailyGoalMinutes?.message}
                />
              )}
            />
            
            {/* Anadil Seçimi */}
            <Controller
              name="nativeLanguage"
              control={control}
              render={({ field }) => (
                <Select
                  label="Anadil"
                  options={[
                    { value: "Turkish", label: "Türkçe" },
                    { value: "English", label: "English" },
                    { value: "German", label: "Deutsch" },
                    { value: "Spanish", label: "Español" },
                  ]}
                  value={field.value}
                  onChange={field.onChange}
                  error={errors.nativeLanguage?.message}
                />
              )}
            />
          </div>

          <Input 
            label="Şifre" 
            type="password" 
            placeholder="••••••"
            error={errors.password?.message}
            {...register('password', { 
              required: 'Şifre zorunludur',
              minLength: { value: 6, message: "En az 6 karakter" }
            })}
          />

          <Input 
            label="Şifre Tekrar" 
            type="password" 
            placeholder="••••••"
            error={errors.confirmPassword?.message}
            {...register('confirmPassword', { 
              required: 'Tekrar zorunludur',
              validate: (val: string) => { // <-- TypeScript düzeltmesi burada
                if (!val) return "Şifreyi tekrar girin";
                if (watch('password') != val) return "Şifreler eşleşmiyor";
              }
            })}
          />

          <Button type="submit" isLoading={isSubmitting} className="mt-4 shadow-lg shadow-brand-primary/30">
            Hesap Oluştur
          </Button>

        </form>

        <div className="mt-8 text-center text-sm text-gray-500 dark:text-gray-400">
          Zaten hesabın var mı?{' '}
          <Link to="/login" className="text-brand-primary font-bold hover:underline">
            Giriş Yap
          </Link>
        </div>
      </div>
    </div>
  );
};