import React, { useEffect, useState, useContext } from 'react';
import { api } from '../api/axios';
import { AuthContext } from '../context/AuthContext';

export const NewsPage = () => {
  const { user } = useContext(AuthContext);
  const [articles, setArticles] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ title: '', content: '' });
  const [status, setStatus] = useState({ message: '', type: '' });

  useEffect(() => {
    let alive = true;
    api.get('/Articles')
      .then(res => { if (alive) setArticles(res.data); })
      .catch(() => { if (alive) setError('Не удалось загрузить статьи'); })
      .finally(() => { if (alive) setLoading(false); });
    return () => { alive = false; };
  }, []);

  const submit = async (e) => {
    e.preventDefault();
    if (!form.title.trim() || !form.content.trim()) return;
    setStatus({ message: 'Отправка...', type: 'loading' });
    try {
      await api.post('/Articles', { title: form.title.trim(), content: form.content.trim() });
      setForm({ title: '', content: '' });
      setShowForm(false);
      setStatus({ message: 'Статья отправлена на модерацию', type: 'success' });
      window.setTimeout(() => setStatus({ message: '', type: '' }), 3500);
    } catch {
      setStatus({ message: 'Ошибка при отправке', type: 'error' });
    }
  };

  return (
    <div>
      <div className="flex items-end justify-between mb-6">
        <h1 className="text-4xl font-black uppercase border-l-8 border-black pl-4">НОВОСТИ И СТАТЬИ</h1>
        {user && (
          <button onClick={() => setShowForm(s => !s)} className="border-2 border-black px-4 py-2 font-bold uppercase hover:bg-black hover:text-white transition-colors">
            + Написать
          </button>
        )}
      </div>

      <p className="text-sm text-gray-600 italic mb-8">Обзоры, анонсы и мнения о фильмах и сериалах. Каждая статья проходит проверку модератором перед публикацией.</p>

      {status.message && (
        <div className={`mb-6 border-2 p-3 font-bold ${status.type === 'error' ? 'border-red-600 text-red-600' : status.type === 'success' ? 'border-green-600 text-green-600' : 'border-gray-400 text-gray-500'}`}>
          {status.message}
        </div>
      )}

      {showForm && user && (
        <form onSubmit={submit} className="mb-8 border-2 border-black p-5">
          <h2 className="font-black uppercase mb-4">Новая статья</h2>
          <input
            className="w-full border-2 border-black px-3 py-2 mb-3 outline-none focus:ring-2 focus:ring-black font-medium"
            placeholder="Заголовок"
            value={form.title}
            onChange={(e) => setForm({ ...form, title: e.target.value })}
          />
          <textarea
            className="w-full border-2 border-black px-3 py-2 mb-4 outline-none focus:ring-2 focus:ring-black font-medium"
            rows="6"
            placeholder="Текст статьи..."
            value={form.content}
            onChange={(e) => setForm({ ...form, content: e.target.value })}
          />
          <div className="flex gap-3">
            <button type="submit" className="border-2 border-black bg-black text-white px-4 py-2 font-bold uppercase hover:bg-white hover:text-black transition-colors">
              Отправить
            </button>
            <button type="button" onClick={() => setShowForm(false)} className="border-2 border-black px-4 py-2 font-bold uppercase hover:bg-black hover:text-white transition-colors">
              Отмена
            </button>
          </div>
        </form>
      )}

      {loading ? (
        <div className="space-y-4">
          {Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="border-2 border-black p-5 animate-pulse">
              <div className="h-5 w-2/3 bg-neutral-200 mb-3" />
              <div className="h-4 w-full bg-neutral-200 mb-2" />
              <div className="h-4 w-5/6 bg-neutral-200" />
            </div>
          ))}
        </div>
      ) : error ? (
        <div className="border-2 border-red-600 p-6 text-center font-bold text-red-600">{error}</div>
      ) : articles.length === 0 ? (
        <p className="text-center text-lg italic text-gray-500">Статей пока нет</p>
      ) : (
        <div className="space-y-4">
          {articles.map((a) => (
            <article key={a.id} className="border-2 border-black p-5 hover:shadow-[6px_6px_0_#000] transition-shadow">
              <h2 className="text-2xl font-black uppercase leading-tight mb-2">{a.title}</h2>
              <div className="text-sm text-gray-500 mb-3">
                {a.authorUsername} · {new Date(a.createdAt).toLocaleDateString('ru-RU')}
              </div>
              <p className="text-lg leading-relaxed whitespace-pre-line">{a.content}</p>
            </article>
          ))}
        </div>
      )}
    </div>
  );
};
