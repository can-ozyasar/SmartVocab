import React from 'react'
import ReactDOM from 'react-dom/client'
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { Analytics } from './pages/Analytics.tsx' 
import { Login } from './pages/Login.tsx'
import { Register } from './pages/Register.tsx'
import { Dashboard } from './pages/Dashboard.tsx' 
import { AppLayout } from './components/AppLayout.tsx' 
import { LabSession } from './pages/LabSession.tsx';
import { AuthProvider } from './context/AuthContext.tsx'
import { Labs } from './pages/Labs.tsx' 
import { Profile } from './pages/Profile.tsx'
import { Study } from './pages/Study.tsx'; 
import { SettingsProvider } from './context/SettingsContext.tsx';
import App from './App.tsx'
import { Toaster } from 'sonner';
import { Words } from './pages/Words.tsx'
import './index.css'

const queryClient = new QueryClient();

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <AuthProvider>
      <SettingsProvider> 
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <Toaster position="top-center" richColors closeButton />
          <Routes>
            {/* Halka Açık Sayfalar */}
            <Route path="/" element={<App />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />

            {/* Korumalı Sayfalar (Layout İçinde) */}
            <Route element={<AppLayout />}>
              <Route path="/dashboard" element={<Dashboard />} />
              <Route path="/words" element={<Words />} />
              <Route path="/study" element={<Study />} /> {/* <-- GÜNCELLENDİ */}
              <Route path="/analytics" element={<Analytics />} /> {/* <-- 2. ROTA EKLENDİ */}
              <Route path="/profile" element={<Profile />} />
              <Route path="/labs" element={<Labs />} /> {/* <-- YENİ ROTA */}
            </Route>
            <Route path="/lab-session" element={<LabSession />} />
          </Routes>
        </BrowserRouter>
      </QueryClientProvider>
      </SettingsProvider> 
    </AuthProvider>
  </React.StrictMode>,
)