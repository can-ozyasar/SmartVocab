import React, { forwardRef } from 'react';
import { ChevronDown } from 'lucide-react';

interface SelectOption {
  value: string | number;
  label: string;
}

interface SelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  label: string;
  options: SelectOption[];
  error?: string;
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(
  ({ label, options, error, className, ...props }, ref) => {
    return (
      <div className="flex flex-col gap-1.5 w-full">
        {/* DÜZELTME 1: font-semibold yerine font-medium yaptık (Input ile aynı oldu) */}
        <label className="text-sm font-medium text-gray-700 dark:text-gray-300 ml-1">
          {label}
        </label>
        
        <div className="relative">
          <select
            ref={ref}
            className={`
              /* DÜZELTME 2: py-3.5 yerine py-3 (Input ile aynı yükseklik) */
              /* DÜZELTME 3: font-medium yerine font-normal (Yazı kalınlığı eşitlendi) */
              w-full px-4 py-3 pr-10 rounded-xl border outline-none transition-all duration-200 appearance-none
              
              /* Renkleri Input ile birebir eşitledik */
              bg-white text-gray-900 border-gray-200
              placeholder:text-gray-400
              
              dark:bg-slate-800 dark:text-white dark:border-slate-700
              
              focus:border-brand-primary focus:ring-4 focus:ring-brand-primary/10
              dark:focus:border-brand-primary dark:focus:ring-brand-primary/20

              hover:border-gray-300 dark:hover:border-white/20
              cursor-pointer

              ${error ? 'border-brand-danger bg-red-50 text-brand-danger' : ''}
              ${className}
            `}
            {...props}
          >
            {options.map((opt) => (
              <option key={opt.value} value={opt.value} className="text-gray-900 bg-white dark:bg-slate-800 dark:text-white">
                {opt.label}
              </option>
            ))}
          </select>

          {/* İkon rengini de eşitledik */}
          <div className="absolute right-3 top-1/2 -translate-y-1/2 pointer-events-none text-gray-400">
            <ChevronDown size={20} strokeWidth={2} />
          </div>
        </div>

        {error && <span className="text-xs text-brand-danger font-medium ml-1">{error}</span>}
      </div>
    );
  }
);

Select.displayName = 'Select';