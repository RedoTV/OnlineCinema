import { useContext, useEffect, useMemo, useState } from 'react';
import { Navigate } from 'react-router-dom';
import { api } from '../api/axios';
import { AuthContext } from '../context/AuthContext';

const TABS = [
  ['dashboard', 'Обзор'], ['movies', 'Фильмы'], ['series', 'Сериалы'],
  ['actors', 'Актёры'], ['genres', 'Жанры'], ['comments', 'Комментарии'], ['ratings', 'Оценки'],
];
const emptyMovie = { title: '', description: '', releaseYear: '', duration: '', genreIds: [], actorIds: [] };
const emptySeries = { title: '', description: '', releaseYear: '', genreIds: [], actorIds: [] };
const input = 'w-full border border-black bg-white px-3 py-2 outline-none focus:ring-2 focus:ring-black';
const button = 'border border-black bg-black text-white px-4 py-2 font-bold hover:bg-white hover:text-black transition-colors disabled:opacity-40';

function ConfirmButton({ label = 'Удалить', onConfirm }) {
  return <button className="border border-black px-3 py-1 text-sm font-bold hover:bg-black hover:text-white" onClick={() => {
    if (window.confirm('Действие нельзя отменить. Продолжить?')) onConfirm();
  }}>{label}</button>;
}

function EntityTable({ rows, columns, onEdit, onDelete, uploadLabel, onUpload }) {
  return <div className="overflow-x-auto border border-black">
    <table className="w-full text-left text-sm">
      <thead className="bg-black text-white"><tr>{columns.map(c => <th className="p-3" key={c.key}>{c.label}</th>)}<th className="p-3">Действия</th></tr></thead>
      <tbody>{rows.map(row => <tr className="border-t border-black" key={row.id}>
        {columns.map(c => <td className="p-3 align-top" key={c.key}>{c.render ? c.render(row) : row[c.key]}</td>)}
        <td className="p-3"><div className="flex flex-wrap gap-2">
          {onEdit && <button className="border border-black px-3 py-1 font-bold" onClick={() => onEdit(row)}>Изменить</button>}
          {onUpload && <label className="cursor-pointer border border-black px-3 py-1 font-bold">{uploadLabel}<input className="hidden" type="file" accept="image/*" onChange={e => e.target.files[0] && onUpload(row.id, e.target.files[0])}/></label>}
          {onDelete && <ConfirmButton onConfirm={() => onDelete(row.id)}/>} 
        </div></td>
      </tr>)}</tbody>
    </table>
    {!rows.length && <p className="p-6 text-center text-neutral-500">Пока ничего нет</p>}
  </div>;
}

export function AdminPage() {
  const { user } = useContext(AuthContext);
  const role = user?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || user?.role;
  const [tab, setTab] = useState('dashboard');
  const [data, setData] = useState({ movies: [], series: [], actors: [], genres: [], comments: [], ratings: [], dashboard: {} });
  const [movie, setMovie] = useState(emptyMovie);
  const [series, setSeries] = useState(emptySeries);
  const [actor, setActor] = useState({ firstName: '', lastName: '', birthDate: '', biography: '' });
  const [genre, setGenre] = useState({ name: '' });
  const [editing, setEditing] = useState(null);
  const [message, setMessage] = useState('');

  const load = async () => {
    try {
      const [movies, shows, actors, genres, dashboard, comments, ratings] = await Promise.all([
        api.get('/Movies'), api.get('/Series'), api.get('/Actors'), api.get('/Genres'),
        api.get('/Admin/dashboard'), api.get('/Admin/comments'), api.get('/Admin/ratings'),
      ]);
      setData({ movies: movies.data, series: shows.data, actors: actors.data, genres: genres.data,
        dashboard: dashboard.data, comments: comments.data, ratings: ratings.data });
    } catch (error) { setMessage(error.response?.status === 403 ? 'Недостаточно прав администратора.' : 'Не удалось загрузить данные.'); }
  };
  useEffect(() => { if (role === 'Admin') load(); }, [role]);
  const notify = async (text) => { setMessage(text); await load(); window.setTimeout(() => setMessage(''), 2500); };
  const multi = e => Array.from(e.target.selectedOptions, x => Number(x.value));
  const options = useMemo(() => ({ genres: data.genres, actors: data.actors }), [data]);

  if (!user) return <Navigate to="/login" replace />;
  if (role !== 'Admin') return <section className="border-2 border-black p-8"><h1 className="text-3xl font-black">403</h1><p>Эта страница доступна только администратору.</p></section>;

  const saveMovie = async e => { e.preventDefault(); const body = { ...movie, releaseYear: Number(movie.releaseYear) || null, duration: Number(movie.duration) || null };
    editing ? await api.put(`/Movies/${editing}`, body) : await api.post('/Movies', body); setMovie(emptyMovie); setEditing(null); notify('Фильм сохранён'); };
  const saveSeries = async e => { e.preventDefault(); const body = { ...series, releaseYear: Number(series.releaseYear) || null };
    editing ? await api.put(`/Series/${editing}`, body) : await api.post('/Series', body); setSeries(emptySeries); setEditing(null); notify('Сериал сохранён'); };
  const upload = async (kind, id, file) => { const body = new FormData(); body.append('file', file); await api.post(`/${kind}/${id}/upload-poster`, body, { headers: { 'Content-Type': 'multipart/form-data' } }); notify('Постер загружен'); };

  return <section className="space-y-6">
    <div className="flex flex-wrap items-end justify-between gap-4 border-b-4 border-black pb-5"><div><p className="text-sm font-bold uppercase tracking-[.25em]">Управление каталогом</p><h1 className="text-4xl font-black">ADMIN PANEL</h1></div><span className="border border-black px-3 py-2 text-sm">{user.username}</span></div>
    <div className="flex flex-wrap gap-2">{TABS.map(([id, title]) => <button key={id} onClick={() => { setTab(id); setEditing(null); }} className={`border border-black px-4 py-2 font-bold ${tab === id ? 'bg-black text-white' : 'bg-white text-black'}`}>{title}</button>)}</div>
    {message && <div className="border-2 border-black bg-neutral-100 p-3 font-bold">{message}</div>}

    {tab === 'dashboard' && <div className="grid grid-cols-2 gap-3 md:grid-cols-4">{Object.entries(data.dashboard).map(([key, value]) => <article className="border-2 border-black p-5" key={key}><p className="text-xs uppercase text-neutral-500">{key}</p><strong className="text-3xl">{value}</strong></article>)}</div>}

    {tab === 'movies' && <><Editor title={editing ? 'Редактировать фильм' : 'Новый фильм'} onSubmit={saveMovie} onCancel={() => { setMovie(emptyMovie); setEditing(null); }}>
      <input className={input} placeholder="Название" value={movie.title} onChange={e => setMovie({...movie, title:e.target.value})} required/><input className={input} type="number" placeholder="Год" value={movie.releaseYear} onChange={e => setMovie({...movie, releaseYear:e.target.value})}/><input className={input} type="number" placeholder="Длительность, мин" value={movie.duration} onChange={e => setMovie({...movie, duration:e.target.value})}/><textarea className={`${input} md:col-span-3`} placeholder="Описание" value={movie.description} onChange={e => setMovie({...movie, description:e.target.value})}/><SelectMany label="Жанры" rows={options.genres} value={movie.genreIds} onChange={e => setMovie({...movie, genreIds:multi(e)})}/><SelectMany label="Актёры" rows={options.actors} value={movie.actorIds} text={x => `${x.firstName} ${x.lastName}`} onChange={e => setMovie({...movie, actorIds:multi(e)})}/></Editor>
      <EntityTable rows={data.movies} columns={[{key:'title',label:'Название'},{key:'releaseYear',label:'Год'},{key:'averageRating',label:'Рейтинг'}]} onEdit={x => { setMovie({...emptyMovie,...x,genreIds:x.genres.map(g=>g.id),actorIds:x.actors.map(a=>a.id)}); setEditing(x.id); }} onDelete={async id => { await api.delete(`/Movies/${id}`); notify('Фильм удалён'); }} uploadLabel="Постер" onUpload={(id,f)=>upload('Movies',id,f)}/></>}

    {tab === 'series' && <><Editor title={editing ? 'Редактировать сериал' : 'Новый сериал'} onSubmit={saveSeries} onCancel={() => { setSeries(emptySeries); setEditing(null); }}><input className={input} placeholder="Название" value={series.title} onChange={e => setSeries({...series,title:e.target.value})} required/><input className={input} type="number" placeholder="Год" value={series.releaseYear} onChange={e => setSeries({...series,releaseYear:e.target.value})}/><textarea className={`${input} md:col-span-2`} placeholder="Описание" value={series.description} onChange={e => setSeries({...series,description:e.target.value})}/><SelectMany label="Жанры" rows={options.genres} value={series.genreIds} onChange={e => setSeries({...series,genreIds:multi(e)})}/><SelectMany label="Актёры" rows={options.actors} value={series.actorIds} text={x=>`${x.firstName} ${x.lastName}`} onChange={e=>setSeries({...series,actorIds:multi(e)})}/></Editor>
      <EntityTable rows={data.series} columns={[{key:'title',label:'Название'},{key:'releaseYear',label:'Год'},{key:'seasons',label:'Сезоны',render:x=>x.seasons?.length||0}]} onEdit={x=>{setSeries({...emptySeries,...x,genreIds:x.genres?.map(g=>g.id)||[],actorIds:x.actors?.map(a=>a.id)||[]});setEditing(x.id)}} onDelete={async id=>{await api.delete(`/Series/${id}`);notify('Сериал удалён')}} uploadLabel="Постер" onUpload={(id,f)=>upload('Series',id,f)}/></>}

    {tab === 'actors' && <><Editor title={editing ? 'Редактировать актёра' : 'Новый актёр'} onSubmit={async e=>{e.preventDefault(); editing?await api.put(`/Actors/${editing}`,actor):await api.post('/Actors',actor);setActor({firstName:'',lastName:'',birthDate:'',biography:''});setEditing(null);notify('Актёр сохранён')}} onCancel={()=>setEditing(null)}><input className={input} placeholder="Имя" required value={actor.firstName} onChange={e=>setActor({...actor,firstName:e.target.value})}/><input className={input} placeholder="Фамилия" required value={actor.lastName} onChange={e=>setActor({...actor,lastName:e.target.value})}/><input className={input} type="date" value={actor.birthDate||''} onChange={e=>setActor({...actor,birthDate:e.target.value||null})}/><textarea className={input} placeholder="Биография" value={actor.biography||''} onChange={e=>setActor({...actor,biography:e.target.value})}/></Editor><EntityTable rows={data.actors} columns={[{key:'firstName',label:'Имя'},{key:'lastName',label:'Фамилия'},{key:'birthDate',label:'Дата рождения'}]} onEdit={x=>{setActor(x);setEditing(x.id)}} onDelete={async id=>{await api.delete(`/Actors/${id}`);notify('Актёр удалён')}}/></>}

    {tab === 'genres' && <><Editor title={editing ? 'Изменить жанр' : 'Новый жанр'} onSubmit={async e=>{e.preventDefault();editing?await api.put(`/Genres/${editing}`,genre):await api.post('/Genres',genre);setGenre({name:''});setEditing(null);notify('Жанр сохранён')}} onCancel={()=>setEditing(null)}><input className={input} placeholder="Название" required value={genre.name} onChange={e=>setGenre({name:e.target.value})}/></Editor><EntityTable rows={data.genres} columns={[{key:'name',label:'Название'}]} onEdit={x=>{setGenre(x);setEditing(x.id)}} onDelete={async id=>{await api.delete(`/Genres/${id}`);notify('Жанр удалён')}}/></>}

    {tab === 'comments' && <EntityTable rows={data.comments} columns={[{key:'username',label:'Пользователь'},{key:'target',label:'Материал'},{key:'text',label:'Текст'},{key:'isHidden',label:'Статус',render:x=>x.isHidden?'Скрыт':'Опубликован'}]} onEdit={async x=>{await api.post(`/Comments/${x.id}/hide?hide=${!x.isHidden}`);notify(x.isHidden?'Комментарий одобрен':'Комментарий скрыт')}} onDelete={async id=>{await api.delete(`/Comments/${id}`);notify('Комментарий удалён')}}/>}
    {tab === 'ratings' && <EntityTable rows={data.ratings} columns={[{key:'username',label:'Пользователь'},{key:'contentType',label:'Тип'},{key:'target',label:'Материал'},{key:'ratingValue',label:'Оценка'}]}/>} 
  </section>;
}

function Editor({ title, children, onSubmit, onCancel }) { return <form onSubmit={onSubmit} className="border-2 border-black p-5"><div className="mb-4 flex justify-between"><h2 className="text-xl font-black">{title}</h2>{onCancel&&<button type="button" onClick={onCancel}>Сбросить</button>}</div><div className="grid gap-3 md:grid-cols-3">{children}</div><button className={`${button} mt-4`} type="submit">Сохранить</button></form>; }
function SelectMany({ label, rows, value, onChange, text=x=>x.name }) { return <label className="text-sm font-bold">{label}<select className={`${input} mt-1 h-28`} multiple value={value} onChange={onChange}>{rows.map(x=><option key={x.id} value={x.id}>{text(x)}</option>)}</select></label>; }
