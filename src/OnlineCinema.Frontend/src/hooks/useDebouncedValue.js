import { useEffect, useState } from 'react';

// Возвращает значение с задержкой: пока пользователь печатает,
// API не дёргается на каждое нажатие клавиши.
export const useDebouncedValue = (value, delay = 300) => {
  const [debounced, setDebounced] = useState(value);

  useEffect(() => {
    const timer = window.setTimeout(() => setDebounced(value), delay);
    return () => window.clearTimeout(timer);
  }, [value, delay]);

  return debounced;
};