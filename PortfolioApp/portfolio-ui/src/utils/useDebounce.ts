import { useState, useEffect } from "react";

/**
 * Возвращает значение с задержкой `delay` мс после последнего изменения.
 */
export function useDebounce<T>(value: T, delay = 400) {
  const [debounced, setDebounced] = useState(value);

  useEffect(() => {
    const id = setTimeout(() => setDebounced(value), delay);
    return () => clearTimeout(id);          // очистка таймера
  }, [value, delay]);

  return debounced;
}
