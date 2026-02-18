
import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm, Controller } from 'react-hook-form';
import axiosClient from '../api/axiosClient';
import { Plus, Search, Filter, BookOpen, Volume2, Save, X, Trash2, Edit2, Loader2 } from 'lucide-react';
import { Button } from '../components/Button';
import { Input } from '../components/Input';
import { Select } from '../components/Select';
import { ConfirmModal } from '../components/ConfirmModal'; 
import { toast } from 'sonner'; 


// --- ENUM & TİP TANIMLAMALARI ---
const WordType = { Noun: 0, Verb: 1, Adjective: 2, Adverb: 3, PhrasalVerb: 4, Idiom: 5 } as const;
type WordType = typeof WordType[keyof typeof WordType];

const CEFRLevel = { A1: 0, A2: 1, B1: 2, B2: 3, C1: 4, C2: 5 } as const;
type CEFRLevel = typeof CEFRLevel[keyof typeof CEFRLevel];

interface WordDto {
  id: string;
  text: string;
  meaning: string;
  exampleSentence?: string;
  type: WordType;
  level: CEFRLevel;
}

interface WordForm {
  id?: string;
  text: string;
  meaning: string;
  exampleSentence?: string;
  type: number;
  level: number;
}

export const Words = () => {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedLevel, setSelectedLevel] = useState<string | null>(null);
  
  // Modal State'leri
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingWord, setEditingWord] = useState<WordDto | null>(null);
  
  // Silme İşlemi State'leri
  const [wordToDelete, setWordToDelete] = useState<WordDto | null>(null); // Silinecek kelimeyi tutar

  // --- 1. KELİMELERİ ÇEK ---
  const { data: words, isLoading } = useQuery<WordDto[]>({
    queryKey: ['words'],
    queryFn: async () => (await axiosClient.get('/word')).data
  });

  // --- FORM YÖNETİMİ ---
  const { register, handleSubmit, reset, control, setValue, formState: { errors, isSubmitting } } = useForm<WordForm>();

  const openModal = (word?: WordDto) => {
    if (word) {
      setEditingWord(word);
      setValue('text', word.text);
      setValue('meaning', word.meaning);
      setValue('exampleSentence', word.exampleSentence || '');
      setValue('type', word.type);
      setValue('level', word.level);
    } else {
      setEditingWord(null);
      reset({ text: '', meaning: '', exampleSentence: '', type: 0, level: 0 });
    }
    setIsModalOpen(true);
  };

  // --- API İŞLEMLERİ ---
  const saveMutation = useMutation({
    mutationFn: async (data: WordForm) => {
      if (editingWord) {
        await axiosClient.put('/word', { ...data, Id: editingWord.id });
      } else {
        await axiosClient.post('/word', data);
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['words'] });
      setIsModalOpen(false);
      reset();
      // Şık Bildirim:
      toast.success(editingWord ? "Kelime güncellendi!" : "Kelime başarıyla eklendi!", {
      });
    },
    onError: () => {
      toast.error("İşlem başarısız oldu.");
    }
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: string) => {
      await axiosClient.delete(`/word/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['words'] });
      setWordToDelete(null); // Modalı kapat
      toast.success("Kelime silindi.", { icon: '🗑️' });
    },
    onError: () => {
      toast.error("Silme işlemi başarısız.");
    }
  });

  // --- FİLTRELEME ---
  const filteredWords = words?.filter(word => {
    const matchesSearch = word.text.toLowerCase().includes(searchTerm.toLowerCase()) || 
                          word.meaning.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesLevel = selectedLevel ? word.level.toString() === selectedLevel : true;
    return matchesSearch && matchesLevel;
  });

  // --- UI ---
  const speak = (text: string, e: React.MouseEvent) => {
    e.stopPropagation();
    const utterance = new SpeechSynthesisUtterance(text);
    utterance.lang = 'en-US';
    window.speechSynthesis.speak(utterance);
  };

  const getLevelBadgeColor = (level: CEFRLevel) => {
    const colors = ["bg-green-100 text-green-700", "bg-emerald-100 text-emerald-700", "bg-blue-100 text-blue-700", "bg-indigo-100 text-indigo-700", "bg-purple-100 text-purple-700", "bg-pink-100 text-pink-700"];
    return colors[level] || "bg-gray-100 text-gray-700";
  };

  const getTypeLabel = (type: WordType) => ["İsim", "Fiil", "Sıfat", "Zarf", "Phrasal Verb", "Deyim"][type] || "Kelime";
  const getLevelLabel = (level: CEFRLevel) => ["A1", "A2", "B1", "B2", "C1", "C2"][level] || "?";
  
  // Options (Select için)
  const typeOptions = [{ value: 0, label: 'İsim (Noun)' }, { value: 1, label: 'Fiil (Verb)' }, { value: 2, label: 'Sıfat (Adj)' }, { value: 3, label: 'Zarf (Adv)' }, { value: 4, label: 'Phrasal Verb' }, { value: 5, label: 'Deyim (Idiom)' }];
  const levelOptions = [{ value: 0, label: 'A1 (Başlangıç)' }, { value: 1, label: 'A2 (Temel)' }, { value: 2, label: 'B1 (Orta)' }, { value: 3, label: 'B2 (İyi)' }, { value: 4, label: 'C1 (İleri)' }, { value: 5, label: 'C2 (Uzman)' }];

  return (
    <div className="max-w-6xl mx-auto pb-20 animate-fade-in relative">
      
      {/* HEADER */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-8">
        <div>
          <h2 className="text-3xl font-bold text-gray-900 dark:text-white">Sözlüğüm</h2>
          <p className="text-gray-500 mt-1">Kelime hazineni yönet ve genişlet.</p>
        </div>
        <Button onClick={() => openModal()} className="w-auto px-6 shadow-lg shadow-brand-primary/20">
          <Plus size={20} className="mr-2" />
          Yeni Kelime Ekle
        </Button>
      </div>

      {/* SEARCH BAR */}
      <div className="bg-white dark:bg-slate-900 p-4 rounded-2xl border border-gray-100 dark:border-white/5 shadow-sm mb-6 flex flex-col md:flex-row gap-4">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" size={20} />
          <input type="text" placeholder="Ara..." value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-10 pr-4 py-3 bg-gray-50 dark:bg-slate-800 border border-transparent focus:bg-white dark:focus:bg-slate-950 focus:border-brand-primary rounded-xl outline-none transition-all"
          />
        </div>
        <div className="flex items-center gap-2 overflow-x-auto pb-2 md:pb-0 no-scrollbar">
          <Filter size={18} className="text-gray-400 mr-1 min-w-[18px]" />
          {["A1", "A2", "B1", "B2", "C1", "C2"].map((lvl, index) => (
            <button key={lvl} onClick={() => setSelectedLevel(selectedLevel === index.toString() ? null : index.toString())}
              className={`px-4 py-2 rounded-xl text-sm font-medium transition-all whitespace-nowrap
                ${selectedLevel === index.toString() ? 'bg-brand-primary text-white shadow-md' : 'bg-gray-50 dark:bg-slate-800 text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-slate-700'}`}
            >
              {lvl}
            </button>
          ))}
          {selectedLevel && <button onClick={() => setSelectedLevel(null)} className="text-xs text-red-500 hover:underline px-2">Temizle</button>}
        </div>
      </div>

      {/* LİSTE */}
      {isLoading ? (
        <div className="text-center p-20 text-gray-400 animate-pulse">Yükleniyor...</div>
      ) : filteredWords && filteredWords.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredWords.map((word) => (
            <div key={word.id} className="group bg-white dark:bg-slate-900 p-6 rounded-[2rem] border border-gray-100 dark:border-white/5 shadow-sm hover:shadow-md hover:border-brand-primary/30 transition-all duration-300 relative">
              <div className="flex justify-between items-start mb-4">
                <div>
                  <h3 className="text-2xl font-bold text-gray-900 dark:text-white mb-1 group-hover:text-brand-primary transition-colors">{word.text}</h3>
                  <span className={`inline-block px-2 py-0.5 rounded-md text-[10px] font-bold uppercase tracking-wider ${getLevelBadgeColor(word.level)}`}>
                    {getLevelLabel(word.level)} • {getTypeLabel(word.type)}
                  </span>
                </div>
                <button onClick={(e) => speak(word.text, e)} className="p-2 rounded-full bg-gray-50 dark:bg-slate-800 text-gray-400 hover:text-brand-primary transition-all">
                  <Volume2 size={18} />
                </button>
              </div>
              <p className="text-gray-600 dark:text-gray-300 font-medium border-l-2 border-brand-primary/20 pl-3">{word.meaning}</p>
              {word.exampleSentence && <p className="mt-3 text-sm text-gray-400 italic bg-gray-50 dark:bg-slate-800/50 p-3 rounded-xl">"{word.exampleSentence}"</p>}

              {/* ACTION BUTTONS */}
              <div className="absolute top-4 right-14 flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity duration-200">
                <button onClick={() => openModal(word)} className="p-2 text-gray-400 hover:text-blue-500 hover:bg-blue-50 dark:hover:bg-blue-900/20 rounded-lg transition-colors">
                    <Edit2 size={16} />
                </button>
                <button 
                    onClick={() => setWordToDelete(word)} // <-- SİLME MODALINI AÇAR
                    className="p-2 text-gray-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg transition-colors" 
                >
                    <Trash2 size={16} />
                </button>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="flex flex-col items-center justify-center p-20 text-center">
          <div className="w-24 h-24 bg-gray-50 dark:bg-slate-900 rounded-full flex items-center justify-center mb-6 text-gray-300">
            <BookOpen size={48} />
          </div>
          <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-2">Sözlük Boş</h3>
          <p className="text-gray-500 mb-8">Henüz kelime yok.</p>
          <Button variant="outline" onClick={() => openModal()}>İlk Kelimeni Ekle</Button>
        </div>
      )}

      {/* --- CONFIRM DELETE MODAL --- */}
      <ConfirmModal 
        isOpen={!!wordToDelete} // wordToDelete doluysa açılır
        title="Kelimeyi Sil"
        message={`"${wordToDelete?.text}" kelimesini silmek istediğinize emin misiniz? Bu işlem geri alınamaz.`}
        onConfirm={() => wordToDelete && deleteMutation.mutate(wordToDelete.id)}
        onCancel={() => setWordToDelete(null)}
        isLoading={deleteMutation.isPending}
      />

      {/* --- ADD/EDIT MODAL --- */}
      {isModalOpen && (
        <div className="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm animate-fade-in">
          <div className="bg-white dark:bg-slate-900 w-full max-w-lg rounded-[2rem] shadow-2xl border border-gray-100 dark:border-white/10 p-8 relative animate-scale-up">
            <button onClick={() => setIsModalOpen(false)} className="absolute top-6 right-6 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200">
              <X size={24} />
            </button>

            <h3 className="text-2xl font-bold text-gray-900 dark:text-white mb-6 flex items-center gap-3">
              <div className="p-2 bg-brand-primary/10 rounded-xl text-brand-primary">
                  {editingWord ? <Edit2 size={24} /> : <Plus size={24} />}
              </div>
              {editingWord ? "Kelimeyi Düzenle" : "Yeni Kelime"}
            </h3>

            <form onSubmit={handleSubmit((data) => saveMutation.mutate(data))} className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                 <Input label="Kelime" placeholder="Word" autoFocus {...register('text', { required: 'Gerekli' })} error={errors.text?.message as string} />
                 <Input label="Anlamı" placeholder="Meaning" {...register('meaning', { required: 'Gerekli' })} error={errors.meaning?.message as string} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">Örnek Cümle</label>
                <textarea {...register('exampleSentence')} rows={2} className="w-full p-3 bg-gray-50 dark:bg-slate-800 border border-gray-200 dark:border-slate-700 rounded-xl outline-none focus:border-brand-primary transition-all resize-none dark:text-white"></textarea>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <Controller name="type" control={control} rules={{ required: "Seçiniz" }}
                  render={({ field }) => <Select label="Tür" options={typeOptions} value={field.value} onChange={field.onChange} error={errors.type?.message} />} />
                <Controller name="level" control={control} rules={{ required: "Seçiniz" }}
                  render={({ field }) => <Select label="Seviye" options={levelOptions} value={field.value} onChange={field.onChange} error={errors.level?.message} />} />
              </div>

              <div className="pt-4 flex justify-end gap-3">
                <Button type="button" variant="outline" onClick={() => setIsModalOpen(false)} className="w-auto">İptal</Button>
                <Button type="submit" isLoading={isSubmitting || saveMutation.isPending} className="w-auto px-8">
                   <Save size={18} className="mr-2" /> {editingWord ? "Güncelle" : "Kaydet"}
                </Button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};