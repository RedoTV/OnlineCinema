import { useState } from 'react';

// Постер каталога: ленивая загрузка вне вьюпорта, пульс-плейсхолдер
// вместо дёргания несуществующего /placeholder.jpg, тихий фолбэк
// «НЕТ ПОСТЕРА» при 404 без битой иконки.
export const PosterImage = ({ src, alt, eager = false, className = 'w-full h-full object-cover' }) => {
  const [failed, setFailed] = useState(false);
  const [loaded, setLoaded] = useState(false);

  if (!src || failed) {
    return (
      <div className="flex h-full w-full items-center justify-center bg-neutral-200 text-center text-xs font-bold leading-tight text-neutral-500">
        НЕТ
        <br />
        ПОСТЕРА
      </div>
    );
  }

  return (
    <>
      {!loaded && <div className="absolute inset-0 animate-pulse bg-neutral-200" aria-hidden="true" />}
      <img
        src={src}
        alt={alt}
        loading={eager ? 'eager' : 'lazy'}
        decoding="async"
        fetchPriority={eager ? 'high' : 'low'}
        draggable={false}
        onLoad={() => setLoaded(true)}
        onError={() => setFailed(true)}
        className={`${className}${loaded ? '' : ' invisible'}`}
      />
    </>
  );
};