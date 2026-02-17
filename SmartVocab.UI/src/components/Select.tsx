import React, { useState, useRef, useEffect } from 'react';
import { ChevronDown, Check } from 'lucide-react';

interface SelectOption {
  value: string | number;
  label: string;
}

interface SelectProps {
  label: string;
  options: SelectOption[];
  error?: string;
  value?: string | number;
  onChange?: (value: string | number) => void;
  placeholder?: string;
}

export const Select: React.FC<SelectProps> = ({ 
  label, 
  options, 
  error, 
  value, 
  onChange,
  placeholder = "Seçiniz"
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  // Seçili olan seçeneğin "Label"ını bul (Göstermek için)
  const selectedOption = options.find(opt => opt.value === value);

  // Dışarı tıklayınca menüyü kapatma mantığı
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleSelect = (optionValue: string | number) => {
    if (onChange) {
      onChange(optionValue);
    }
    setIsOpen(false);
  };

  return (
    <div className="flex flex-col gap-1.5 w-full relative" ref={containerRef}>
      <label className="text-sm font-medium text-gray-700 dark:text-gray-300 ml-1">
        {label}
      </label>
      
      {/* Tetikleyici Buton (Input gibi görünen kısım) */}
      <div 
        onClick={() => setIsOpen(!isOpen)}
        className={`
          w-full px-4 py-3 rounded-xl border outline-none transition-all duration-200 cursor-pointer flex items-center justify-between
          bg-white dark:bg-slate-800 
          text-gray-900 dark:text-white
          
          ${isOpen 
            ? 'border-brand-primary ring-4 ring-brand-primary/10' 
            : 'border-gray-200 dark:border-slate-700 hover:border-gray-300 dark:hover:border-slate-600'
          }
          
          ${error ? 'border-brand-danger bg-red-50 text-brand-danger' : ''}
        `}
      >
        <span className={selectedOption ? 'font-medium' : 'text-gray-400 font-normal'}>
          {selectedOption ? selectedOption.label : placeholder}
        </span>
        
        <ChevronDown 
          size={20} 
          className={`text-gray-400 transition-transform duration-200 ${isOpen ? 'rotate-180 text-brand-primary' : ''}`} 
        />
      </div>

      {/* Açılır Menü (Dropdown) */}
      {isOpen && (
        <div className="absolute top-full left-0 right-0 mt-2 bg-white dark:bg-slate-800 border border-gray-100 dark:border-slate-700 rounded-xl shadow-xl z-50 overflow-hidden animate-in fade-in zoom-in-95 duration-100">
          <ul className="max-h-60 overflow-auto py-1">
            {options.map((opt) => (
              <li 
                key={opt.value}
                onClick={() => handleSelect(opt.value)}
                className={`
                  px-4 py-3 cursor-pointer text-sm font-medium flex items-center justify-between transition-colors
                  ${value === opt.value 
                    ? 'bg-brand-primary/10 text-brand-primary' 
                    : 'text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-slate-700'
                  }
                `}
              >
                {opt.label}
                {value === opt.value && <Check size={16} />}
              </li>
            ))}
          </ul>
        </div>
      )}

      {error && <span className="text-xs text-brand-danger font-bold ml-1">{error}</span>}
    </div>
  );
};