import { useState, useEffect, useContext } from 'react';
import { api } from '../api/axios';
import { AuthContext } from '../context/AuthContext';

// Компонент комментариев: работает и для фильмов, и для сериалов
export const CommentSection = ({ movieId, seriesId }) => {
  const { user } = useContext(AuthContext);
  const [comments, setComments] = useState([]);
  const [text, setText] = useState('');
  const [sort, setSort] = useState('oldest');
  const [replyTo, setReplyTo] = useState(null);
  const [status, setStatus] = useState('');

  const loadComments = () => {
    const params = new URLSearchParams();
    if (movieId) params.append('movieId', movieId);
    if (seriesId) params.append('seriesId', seriesId);
    params.append('sort', sort);
    api.get(`/Comments?${params}`)
      .then(res => setComments(res.data))
      .catch(() => setStatus('Не удалось загрузить комментарии'));
  };

  useEffect(() => { loadComments(); }, [movieId, seriesId, sort]);

  const submit = async (parentId) => {
    if (!text.trim()) return;
    try {
      await api.post('/Comments', { movieId: movieId || null, seriesId: seriesId || null, parentId, text });
      setText('');
      setReplyTo(null);
      loadComments();
    } catch {
      setStatus('Ошибка при отправке');
    }
  };

  const toggleLike = async (commentId) => {
    await api.post(`/Comments/${commentId}/like`);
    loadComments();
  };

  const renderComment = (c) => (
    <div key={c.id} className="mt-3">
      <div className={`border-2 border-black p-3 ${c.isHidden ? 'opacity-50' : ''}`}>
        <div className="flex justify-between items-start">
          <span className="font-bold">{c.username}</span>
          <span className="text-xs text-gray-500">{new Date(c.createdAt).toLocaleString('ru-RU')}</span>
        </div>
        <p className="mt-1 whitespace-pre-line">{c.text}</p>
        <div className="flex gap-4 mt-2 text-sm font-bold">
          <button onClick={() => toggleLike(c.id)} className={c.likedByMe ? 'text-red-600' : ''}>
            {c.likedByMe ? '♥' : '♡'} {c.likeCount}
          </button>
          {user && <button onClick={() => setReplyTo(c)} className="hover:underline">ОТВЕТИТЬ</button>}
        </div>
      </div>
      {c.replies?.length > 0 && (
        <div className="ml-6 border-l-2 border-gray-300 pl-3">
          {c.replies.map(renderComment)}
        </div>
      )}
    </div>
  );

  return (
    <div className="mt-8">
      <h2 className="text-xl font-bold border-b-2 border-black mb-3 pb-1">КОММЕНТАРИИ</h2>

      <div className="flex gap-2 mb-4">
        {['oldest', 'newest', 'popular'].map(s => (
          <button key={s} onClick={() => setSort(s)}
            className={`px-3 py-1 border-2 border-black text-sm font-bold ${sort === s ? 'bg-black text-white' : ''}`}>
            {s === 'oldest' ? 'Сначала старые' : s === 'newest' ? 'Новые' : 'Популярные'}
          </button>
        ))}
      </div>

      {user ? (
        <div className="border-2 border-black p-3 mb-4">
          {replyTo && (
            <div className="text-sm font-bold mb-1">Ответ {replyTo.username}: <button onClick={() => setReplyTo(null)} className="text-red-600">отмена</button></div>
          )}
          <textarea
            value={text}
            onChange={e => setText(e.target.value)}
            placeholder="Написать комментарий..."
            className="w-full p-2 border-2 border-black outline-none"
            rows={2}
          />
          <button onClick={() => submit(replyTo?.id)} className="mt-2 border-2 border-black px-4 py-1 font-bold hover:bg-black hover:text-white">
            ОТПРАВИТЬ
          </button>
        </div>
      ) : (
        <p className="text-sm italic text-gray-500 mb-4">Войдите, чтобы оставить комментарий.</p>
      )}

      {status && <div className="text-red-600 text-sm font-bold mb-2">{status}</div>}

      {comments.map(renderComment)}
      {comments.length === 0 && <p className="italic text-gray-500">Комментариев пока нет.</p>}
    </div>
  );
};
