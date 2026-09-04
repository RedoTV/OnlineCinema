import { useContext, useEffect, useMemo, useRef, useState } from 'react';
import { Navigate } from 'react-router-dom';
import { api } from '../api/axios';
import { AuthContext } from '../context/AuthContext';

const TABS = [['dashboard', 'Обзор'], ['catalog', 'Каталог'], ['actors', 'Актёры'], ['genres', 'Жанры'], ['moderation', 'Модерация']];
const blankMovie = { title: '', description: '', releaseYear: '', duration: '', genreIds: [], actorIds: [] };
const blankSeries = { title: '', description: '', releaseYear: '', genreIds: [], actorIds: [] };
const input = 'w-full border-2 border-black bg-white px-3 py-2 outline-none focus:ring-2 focus:ring-black focus:ring-offset-2';
const primary = 'border-2 border-black bg-black px-4 py-2 font-bold text-white transition hover:bg-white hover:text-black disabled:cursor-not-allowed disabled:opacity-40';
const secondary = 'border-2 border-black bg-white px-4 py-2 font-bold text-black transition hover:bg-black hover:text-white disabled:opacity-40';

const posterSrc = (kind, id, stamp = '') => `/api/Media/poster/${kind === 'series' ? 'series/' : ''}${id}${stamp ? `?v=${stamp}` : ''}`;
const errorText = error => error?.response?.data?.title || error?.response?.data?.message || error?.message || 'Операция не выполнена';

export function AdminPage() {
  const { user } = useContext(AuthContext);
  const role = user?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || user?.role;
  const [tab, setTab] = useState('dashboard');
  const [data, setData] = useState({ movies: [], series: [], actors: [], genres: [], comments: [], ratings: [], dashboard: {} });
  const [message, setMessage] = useState(null);
  const [busy, setBusy] = useState(false);
  const [catalogType, setCatalogType] = useState('all');
  const [catalogSearch, setCatalogSearch] = useState('');
  const [mediaFilter, setMediaFilter] = useState('all');
  const [editorType, setEditorType] = useState('movie');
  const [editing, setEditing] = useState(null);
  const [movie, setMovie] = useState(blankMovie);
  const [series, setSeries] = useState(blankSeries);
  const [posterFile, setPosterFile] = useState(null);
  const [videoFile, setVideoFile] = useState(null);
  const [posterVersion, setPosterVersion] = useState(Date.now());
  const [expandedSeries, setExpandedSeries] = useState(null);
  const [seasonDraft, setSeasonDraft] = useState({ seasonNumber: 1, title: '', releaseYear: '' });
  const [episodeDrafts, setEpisodeDrafts] = useState({});
  const [actor, setActor] = useState({ firstName: '', lastName: '', birthDate: '', biography: '' });
  const [genre, setGenre] = useState({ name: '' });
  const [editingActor, setEditingActor] = useState(null);
  const [editingGenre, setEditingGenre] = useState(null);
  const [peopleSearch, setPeopleSearch] = useState('');
  const [genreSearch, setGenreSearch] = useState('');
  const [moderationView, setModerationView] = useState('comments');

  const notify = (text, type = 'ok') => { setMessage({ text, type }); window.setTimeout(() => setMessage(null), 3500); };
  const load = async () => {
    try {
      const requests = [api.get('/Movies'), api.get('/Series'), api.get('/Actors'), api.get('/Genres'), api.get('/Admin/dashboard'), api.get('/Admin/comments'), api.get('/Admin/ratings')];
      const [movies, shows, actors, genres, dashboard, comments, ratings] = await Promise.all(requests);
      setData({ movies: movies.data, series: shows.data, actors: actors.data, genres: genres.data, dashboard: dashboard.data, comments: comments.data, ratings: ratings.data });
    } catch (error) { notify(error.response?.status === 403 ? 'Недостаточно прав администратора' : errorText(error), 'error'); }
  };
  useEffect(() => { if (role === 'Admin') load(); }, [role]);

  const resetEditor = (type = editorType) => {
    setEditing(null); setEditorType(type); setMovie(blankMovie); setSeries(blankSeries); setPosterFile(null); setVideoFile(null);
  };
  const run = async (task, success, reload = true) => {
    setBusy(true);
    try { const result = await task(); if (reload) await load(); notify(success); return result; }
    catch (error) { notify(errorText(error), 'error'); throw error; }
    finally { setBusy(false); }
  };
  const uploadFile = async (url, file, onProgress) => {
    const body = new FormData(); body.append('file', file);
    return api.post(url, body, { headers: { 'Content-Type': 'multipart/form-data' }, onUploadProgress: e => onProgress?.(e.total ? Math.round(e.loaded * 100 / e.total) : 0) });
  };

  const saveContent = async e => {
    e.preventDefault();
    const kind = editorType;
    const model = kind === 'movie' ? movie : series;
    const body = kind === 'movie'
      ? { title: model.title.trim(), description: model.description || null, releaseYear: Number(model.releaseYear) || null, duration: Number(model.duration) || null, genreIds: model.genreIds, actorIds: model.actorIds }
      : { title: model.title.trim(), description: model.description || null, releaseYear: Number(model.releaseYear) || null, genreIds: model.genreIds, actorIds: model.actorIds };
    await run(async () => {
      const endpoint = kind === 'movie' ? '/Movies' : '/Series';
      const response = editing ? await api.put(`${endpoint}/${editing}`, body) : await api.post(endpoint, body);
      const id = editing || response.data.id;
      if (posterFile) await uploadFile(`${endpoint}/${id}/upload-poster`, posterFile);
      if (kind === 'movie' && videoFile) await uploadFile(`/Movies/${id}/upload-video`, videoFile);
      return response;
    }, `${kind === 'movie' ? 'Фильм' : 'Сериал'} и выбранные файлы сохранены`);
    setPosterVersion(Date.now()); resetEditor(kind);
  };

  const content = useMemo(() => [
    ...data.movies.map(x => ({ ...x, kind: 'movie', typeLabel: 'Фильм', seasonsTotal: null, episodesTotal: null, hasVideo: Boolean(x.videoUrl) })),
    ...data.series.map(x => ({ ...x, kind: 'series', typeLabel: 'Сериал', seasonsTotal: x.seasonsCount ?? x.seasons?.length ?? 0, episodesTotal: x.seasons?.reduce((n, s) => n + (s.episodes?.length || 0), 0) ?? 0, hasVideo: x.seasons?.some(s => s.episodes?.some(ep => ep.videoUrl)) ?? false })),
  ].filter(x => (catalogType === 'all' || x.kind === catalogType)
    && (!catalogSearch || `${x.title} ${x.description || ''} ${x.releaseYear || ''}`.toLowerCase().includes(catalogSearch.toLowerCase()))
    && (mediaFilter === 'all' || (mediaFilter === 'poster' && x.posterUrl) || (mediaFilter === 'no-poster' && !x.posterUrl) || (mediaFilter === 'video' && x.hasVideo) || (mediaFilter === 'no-video' && !x.hasVideo))), [data.movies, data.series, catalogType, catalogSearch, mediaFilter]);

  if (!user) return <Navigate to="/login" replace />;
  if (role !== 'Admin') return <section className="border-2 border-black p-8"><h1 className="text-3xl font-black">403</h1><p>Эта страница доступна только администратору.</p></section>;

  const editContent = async row => {
    setEditorType(row.kind); setEditing(row.id); setPosterFile(null); setVideoFile(null);
    if (row.kind === 'movie') {
      const full = (await api.get(`/Movies/${row.id}`)).data;
      setMovie({ title: full.title || '', description: full.description || '', releaseYear: full.releaseYear || '', duration: full.duration || '', genreIds: full.genres?.map(x => x.id) || [], actorIds: full.actors?.map(x => x.id) || [] });
    } else {
      const full = (await api.get(`/Series/${row.id}`)).data;
      setSeries({ title: full.title || '', description: full.description || '', releaseYear: full.releaseYear || '', genreIds: full.genres?.map(x => x.id) || [], actorIds: full.actors?.map(x => x.id) || [] });
    }
    document.getElementById('content-editor')?.scrollIntoView({ behavior: 'smooth' });
  };
  const deleteContent = async row => run(() => api.delete(`/${row.kind === 'movie' ? 'Movies' : 'Series'}/${row.id}`), 'Материал удалён');
  const openSeries = async id => {
    if (expandedSeries?.id === id) return setExpandedSeries(null);
    const result = await api.get(`/Series/${id}`); setExpandedSeries(result.data);
  };
  const refreshExpanded = async id => setExpandedSeries((await api.get(`/Series/${id}`)).data);

  return <section className="relative left-1/2 w-[min(94vw,1500px)] -translate-x-1/2 space-y-6">
    <header className="flex flex-wrap items-end justify-between gap-4 border-b-4 border-black pb-5"><div><p className="text-sm font-bold uppercase tracking-[.25em]">Быстрое наполнение каталога</p><h1 className="text-4xl font-black">ADMIN WORKSPACE</h1></div><span className="border-2 border-black px-3 py-2 text-sm font-bold">{user.username}</span></header>
    <nav className="flex flex-wrap gap-2" aria-label="Разделы администратора">{TABS.map(([id, title]) => <button key={id} onClick={() => setTab(id)} className={tab === id ? primary : secondary}>{title}</button>)}</nav>
    {message && <div role="status" className={`border-2 border-black p-3 font-bold ${message.type === 'error' ? 'bg-black text-white' : 'bg-neutral-100 text-black'}`}>{message.text}</div>}

    {tab === 'dashboard' && <Dashboard data={data.dashboard} onOpen={setTab}/>}
    {tab === 'catalog' && <>
      <div className="grid gap-3 border-2 border-black p-4 lg:grid-cols-[1fr_auto_auto_auto]">
        <input className={input} value={catalogSearch} onChange={e => setCatalogSearch(e.target.value)} placeholder="Поиск по названию, описанию или году…" />
        <Segment value={catalogType} onChange={setCatalogType} items={[['all','Все'],['movie','Фильмы'],['series','Сериалы']]}/>
        <select className={input} value={mediaFilter} onChange={e => setMediaFilter(e.target.value)}><option value="all">Любые файлы</option><option value="poster">Есть постер</option><option value="no-poster">Нет постера</option><option value="video">Есть видео</option><option value="no-video">Нет видео</option></select>
        <button className={primary} onClick={() => { resetEditor(catalogType === 'series' ? 'series' : 'movie'); document.getElementById('content-editor')?.scrollIntoView({behavior:'smooth'}); }}>+ ДОБАВИТЬ</button>
      </div>
      <div className="flex items-center justify-between"><strong>{content.length} материалов</strong><span className="text-sm">Фильмы и сериалы находятся в одном месте</span></div>
      <ContentGrid rows={content} stamp={posterVersion} busy={busy} onEdit={editContent} onDelete={deleteContent} onSeries={openSeries}/>
      {expandedSeries && <SeriesBuilder series={expandedSeries} busy={busy} seasonDraft={seasonDraft} setSeasonDraft={setSeasonDraft} episodeDrafts={episodeDrafts} setEpisodeDrafts={setEpisodeDrafts}
        onClose={() => setExpandedSeries(null)} onAddSeason={async () => { await run(() => api.post(`/Series/${expandedSeries.id}/seasons`, { ...seasonDraft, seasonNumber:Number(seasonDraft.seasonNumber), releaseYear:Number(seasonDraft.releaseYear)||null }), 'Сезон добавлен', false); await refreshExpanded(expandedSeries.id); setSeasonDraft({seasonNumber:(expandedSeries.seasons?.length||0)+2,title:'',releaseYear:''}); }}
        onAddEpisode={async seasonId => { const d = episodeDrafts[seasonId] || {episodeNumber:1,title:'',description:'',duration:''}; await run(() => api.post(`/Series/seasons/${seasonId}/episodes`, {...d,episodeNumber:Number(d.episodeNumber),duration:Number(d.duration)||null}), 'Эпизод добавлен', false); await refreshExpanded(expandedSeries.id); setEpisodeDrafts(x=>({...x,[seasonId]:{episodeNumber:Number(d.episodeNumber)+1,title:'',description:'',duration:''}})); }}
        onUpload={async (episodeId,file,setProgress) => { await run(()=>uploadFile(`/Series/episodes/${episodeId}/upload-video`,file,setProgress),'Видео эпизода загружено',false); await refreshExpanded(expandedSeries.id); }}/>}
      <ContentEditor id="content-editor" type={editorType} setType={x => resetEditor(x)} editing={editing} model={editorType === 'movie' ? movie : series} setModel={editorType === 'movie' ? setMovie : setSeries} genres={data.genres} actors={data.actors} posterFile={posterFile} setPosterFile={setPosterFile} videoFile={videoFile} setVideoFile={setVideoFile} stamp={posterVersion} onSubmit={saveContent} onCancel={() => resetEditor(editorType)} busy={busy}/>
    </>}
    {tab === 'actors' && <PeoplePanel rows={data.actors.filter(x=>`${x.firstName} ${x.lastName}`.toLowerCase().includes(peopleSearch.toLowerCase()))} search={peopleSearch} setSearch={setPeopleSearch} actor={actor} setActor={setActor} editing={editingActor} busy={busy} onSave={async e=>{e.preventDefault();await run(()=>editingActor?api.put(`/Actors/${editingActor}`,actor):api.post('/Actors',actor),'Актёр сохранён');setActor({firstName:'',lastName:'',birthDate:'',biography:''});setEditingActor(null)}} onEdit={x=>{setActor({firstName:x.firstName,lastName:x.lastName,birthDate:x.birthDate||'',biography:x.biography||''});setEditingActor(x.id)}} onDelete={id=>run(()=>api.delete(`/Actors/${id}`),'Актёр удалён')}/>}
    {tab === 'genres' && <GenresPanel rows={data.genres.filter(x=>x.name.toLowerCase().includes(genreSearch.toLowerCase()))} search={genreSearch} setSearch={setGenreSearch} genre={genre} setGenre={setGenre} editing={editingGenre} busy={busy} onSave={async e=>{e.preventDefault();await run(()=>editingGenre?api.put(`/Genres/${editingGenre}`,genre):api.post('/Genres',genre),'Жанр сохранён');setGenre({name:''});setEditingGenre(null)}} onEdit={x=>{setGenre({name:x.name});setEditingGenre(x.id)}} onDelete={id=>run(()=>api.delete(`/Genres/${id}`),'Жанр удалён')}/>}
    {tab === 'moderation' && <ModerationPanel view={moderationView} setView={setModerationView} comments={data.comments} ratings={data.ratings} onToggle={x=>run(()=>api.post(`/Comments/${x.id}/hide?hide=${!x.isHidden}`),x.isHidden?'Комментарий опубликован':'Комментарий скрыт')} onDelete={id=>run(()=>api.delete(`/Comments/${id}`),'Комментарий удалён')}/>}
  </section>;
}

function Dashboard({ data, onOpen }) {
  const labels = { movies:'Фильмы', series:'Сериалы', actors:'Актёры', genres:'Жанры', users:'Пользователи', comments:'Комментарии', hiddenComments:'Скрыты', ratings:'Оценки', averageRating:'Средняя оценка' };
  return <><div className="grid grid-cols-2 gap-3 md:grid-cols-3 xl:grid-cols-5">{Object.entries(data).map(([key,value])=><article className="border-2 border-black p-5" key={key}><p className="text-xs font-bold uppercase text-neutral-500">{labels[key]||key}</p><strong className="text-3xl">{typeof value==='number'&&!Number.isInteger(value)?value.toFixed(1):value}</strong></article>)}</div><div className="grid gap-3 md:grid-cols-3"><button className={`${secondary} p-6 text-left`} onClick={()=>onOpen('catalog')}><b className="block text-xl">+ Контент</b><span>Фильм, сериал, сезон или эпизод</span></button><button className={`${secondary} p-6 text-left`} onClick={()=>onOpen('actors')}><b className="block text-xl">+ Актёр</b><span>Быстро пополнить справочник</span></button><button className={`${secondary} p-6 text-left`} onClick={()=>onOpen('moderation')}><b className="block text-xl">Модерация</b><span>Комментарии и оценки</span></button></div></>;
}

function ContentGrid({ rows, stamp, busy, onEdit, onDelete, onSeries }) {
  if (!rows.length) return <Empty text="Ничего не найдено. Измените фильтры или добавьте материал."/>;
  return <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">{rows.map(row=><article key={`${row.kind}-${row.id}`} className="grid grid-cols-[96px_1fr] gap-4 border-2 border-black p-3">
    <Poster kind={row.kind} id={row.id} hasPoster={row.posterUrl} stamp={stamp}/><div className="min-w-0"><div className="mb-2 flex flex-wrap items-center gap-2"><Badge>{row.typeLabel}</Badge><Badge>{row.posterUrl?'ПОСТЕР ✓':'БЕЗ ПОСТЕРА'}</Badge><Badge>{row.hasVideo?'ВИДЕО ✓':'БЕЗ ВИДЕО'}</Badge></div><h3 className="truncate text-xl font-black">{row.title}</h3><p className="text-sm font-bold">{row.releaseYear||'Год не указан'} · {row.kind==='movie' ? `${row.duration||'—'} мин` : `${row.seasonsTotal} сез. / ${row.episodesTotal} эп.`}</p><p className="mt-2 line-clamp-2 min-h-10 text-sm">{row.description||'Описание не заполнено'}</p><div className="mt-3 flex flex-wrap gap-2"><button disabled={busy} className={secondary} onClick={()=>onEdit(row)}>Изменить</button>{row.kind==='series'&&<button disabled={busy} className={secondary} onClick={()=>onSeries(row.id)}>Сезоны</button>}<Confirm disabled={busy} onConfirm={()=>onDelete(row)}/></div></div>
  </article>)}</div>;
}

function ContentEditor({ id, type, setType, editing, model, setModel, genres, actors, posterFile, setPosterFile, videoFile, setVideoFile, stamp, onSubmit, onCancel, busy }) {
  const preview = useObjectUrl(posterFile);
  return <form id={id} onSubmit={onSubmit} className="scroll-mt-4 border-4 border-black p-5"><div className="mb-5 flex flex-wrap items-center justify-between gap-3"><div><p className="text-xs font-bold uppercase tracking-widest">{editing?'Редактирование':'Новый материал'}</p><h2 className="text-2xl font-black">{type==='movie'?'ФИЛЬМ':'СЕРИАЛ'}</h2></div><Segment value={type} onChange={setType} disabled={Boolean(editing)} items={[['movie','Фильм'],['series','Сериал']]}/></div><div className="grid gap-6 lg:grid-cols-[220px_1fr]">
    <div><div className="aspect-[2/3] overflow-hidden border-2 border-black bg-neutral-100">{preview?<img className="h-full w-full object-cover" src={preview} alt="Предпросмотр"/>:editing?<Poster kind={type} id={editing} hasPoster stamp={stamp}/>:<div className="flex h-full items-center justify-center p-4 text-center font-bold">ВЫБЕРИТЕ ПОСТЕР</div>}</div><FilePicker label={editing?'Заменить постер':'Выбрать постер'} accept="image/jpeg,image/png,image/webp,image/svg+xml" file={posterFile} onChange={setPosterFile}/><p className="mt-2 text-xs">JPG, PNG, WEBP или SVG. Рекомендуется 2:3.</p></div>
    <div className="grid content-start gap-4 md:grid-cols-2"><Field label="Название *"><input className={input} required value={model.title} onChange={e=>setModel({...model,title:e.target.value})}/></Field><Field label="Год выхода"><input className={input} min="1888" max="2100" type="number" value={model.releaseYear} onChange={e=>setModel({...model,releaseYear:e.target.value})}/></Field>{type==='movie'&&<Field label="Длительность, минут"><input className={input} min="1" type="number" value={model.duration} onChange={e=>setModel({...model,duration:e.target.value})}/></Field>}<Field label="Описание" wide><textarea rows="5" className={input} value={model.description} onChange={e=>setModel({...model,description:e.target.value})}/></Field><SearchMulti label="Жанры" rows={genres} value={model.genreIds} text={x=>x.name} onChange={ids=>setModel({...model,genreIds:ids})}/><SearchMulti label="Актёры" rows={actors} value={model.actorIds} text={x=>`${x.firstName} ${x.lastName}`} onChange={ids=>setModel({...model,actorIds:ids})}/>{type==='movie'&&<div className="md:col-span-2 border-2 border-black p-4"><h3 className="font-black">ВИДЕО ФИЛЬМА</h3><p className="mb-2 text-sm">MP4, WebM или другой браузерный видеоформат. Загрузка начнётся после сохранения карточки.</p><FilePicker label={editing?'Заменить видео':'Выбрать видео'} accept="video/*" file={videoFile} onChange={setVideoFile}/></div>}</div>
  </div><div className="mt-5 flex flex-wrap justify-end gap-2 border-t-2 border-black pt-4"><button type="button" className={secondary} onClick={onCancel}>Сбросить</button><button disabled={busy} className={primary} type="submit">{busy?'СОХРАНЕНИЕ…':'СОХРАНИТЬ ВСЁ'}</button></div></form>;
}

function SeriesBuilder({ series, busy, seasonDraft, setSeasonDraft, episodeDrafts, setEpisodeDrafts, onClose, onAddSeason, onAddEpisode, onUpload }) {
  return <section className="border-4 border-black p-5"><div className="mb-4 flex items-start justify-between"><div><p className="text-xs font-bold uppercase tracking-widest">Структура сериала</p><h2 className="text-2xl font-black">{series.title}</h2></div><button className={secondary} onClick={onClose}>Закрыть</button></div><div className="mb-5 grid gap-2 border-2 border-black p-3 md:grid-cols-[120px_1fr_140px_auto]"><input className={input} type="number" min="1" value={seasonDraft.seasonNumber} onChange={e=>setSeasonDraft({...seasonDraft,seasonNumber:e.target.value})} placeholder="№ сезона"/><input className={input} value={seasonDraft.title} onChange={e=>setSeasonDraft({...seasonDraft,title:e.target.value})} placeholder="Название сезона"/><input className={input} type="number" value={seasonDraft.releaseYear} onChange={e=>setSeasonDraft({...seasonDraft,releaseYear:e.target.value})} placeholder="Год"/><button disabled={busy} className={primary} onClick={onAddSeason}>+ СЕЗОН</button></div><div className="space-y-4">{series.seasons?.map(season=>{const draft=episodeDrafts[season.id]||{episodeNumber:(season.episodes?.length||0)+1,title:'',description:'',duration:''};return <article className="border-2 border-black" key={season.id}><header className="flex flex-wrap justify-between gap-2 bg-black p-3 text-white"><strong>Сезон {season.seasonNumber}{season.title?` — ${season.title}`:''}</strong><span>{season.episodes?.length||0} эпизодов</span></header><div className="divide-y-2 divide-black">{season.episodes?.map(ep=><EpisodeRow key={ep.id} episode={ep} busy={busy} onUpload={onUpload}/>)}</div><div className="grid gap-2 bg-neutral-100 p-3 lg:grid-cols-[90px_1fr_110px_auto]"><input className={input} type="number" min="1" value={draft.episodeNumber} onChange={e=>setEpisodeDrafts(x=>({...x,[season.id]:{...draft,episodeNumber:e.target.value}}))}/><input className={input} value={draft.title} onChange={e=>setEpisodeDrafts(x=>({...x,[season.id]:{...draft,title:e.target.value}}))} placeholder="Название нового эпизода"/><input className={input} type="number" value={draft.duration} onChange={e=>setEpisodeDrafts(x=>({...x,[season.id]:{...draft,duration:e.target.value}}))} placeholder="Минуты"/><button disabled={busy||!draft.title} className={primary} onClick={()=>onAddEpisode(season.id)}>+ ЭПИЗОД</button></div></article>})}</div></section>;
}

function EpisodeRow({ episode, busy, onUpload }) { const [progress,setProgress]=useState(0); return <div className="grid items-center gap-3 p-3 md:grid-cols-[60px_1fr_auto]"><strong>#{episode.episodeNumber}</strong><div><b>{episode.title}</b><p className="text-sm">{episode.duration||'—'} мин · {episode.videoUrl?'Видео готово':'Видео отсутствует'}{progress>0&&progress<100?` · ${progress}%`:''}</p></div><label className={`${secondary} cursor-pointer text-center`}>{episode.videoUrl?'Заменить видео':'Загрузить видео'}<input disabled={busy} className="hidden" type="file" accept="video/*" onChange={e=>{const f=e.target.files?.[0];if(f)onUpload(episode.id,f,setProgress);e.target.value='';}}/></label></div>; }

function SearchMulti({ label, rows, value, onChange, text }) {
  const [query,setQuery]=useState(''); const [open,setOpen]=useState(false); const root=useRef(null);
  useEffect(()=>{const close=e=>{if(!root.current?.contains(e.target))setOpen(false)};document.addEventListener('mousedown',close);return()=>document.removeEventListener('mousedown',close)},[]);
  const selected=rows.filter(x=>value.includes(x.id)); const filtered=rows.filter(x=>text(x).toLowerCase().includes(query.toLowerCase())).slice(0,50);
  const toggle=id=>onChange(value.includes(id)?value.filter(x=>x!==id):[...value,id]);
  return <div ref={root} className="relative"><span className="text-sm font-bold">{label} ({value.length})</span><button className={`${input} mt-1 flex min-h-11 items-center justify-between text-left`} type="button" onClick={()=>setOpen(!open)}><span className="truncate">{selected.length?selected.map(text).join(', '):`Выберите: ${label.toLowerCase()}`}</span><b>⌄</b></button>{open&&<div className="absolute z-20 mt-1 w-full border-2 border-black bg-white p-2 shadow-[6px_6px_0_#000]"><input autoFocus className={input} value={query} onChange={e=>setQuery(e.target.value)} placeholder="Фильтр…"/><div className="mt-2 max-h-52 overflow-auto">{filtered.map(x=><label key={x.id} className="flex cursor-pointer gap-2 border-b border-neutral-300 p-2 hover:bg-neutral-100"><input type="checkbox" checked={value.includes(x.id)} onChange={()=>toggle(x.id)}/><span>{text(x)}</span></label>)}</div>{value.length>0&&<button type="button" className="mt-2 text-sm font-bold underline" onClick={()=>onChange([])}>Очистить выбор</button>}</div>}<div className="mt-2 flex flex-wrap gap-1">{selected.slice(0,5).map(x=><button type="button" key={x.id} onClick={()=>toggle(x.id)} className="border border-black px-2 py-1 text-xs">{text(x)} ×</button>)}{selected.length>5&&<Badge>+{selected.length-5}</Badge>}</div></div>;
}

function PeoplePanel({ rows, search, setSearch, actor, setActor, editing, busy, onSave, onEdit, onDelete }) { return <div className="grid gap-5 lg:grid-cols-[360px_1fr]"><form onSubmit={onSave} className="h-fit border-2 border-black p-4"><h2 className="mb-3 text-xl font-black">{editing?'ИЗМЕНИТЬ АКТЁРА':'НОВЫЙ АКТЁР'}</h2><div className="space-y-3"><input className={input} required placeholder="Имя" value={actor.firstName} onChange={e=>setActor({...actor,firstName:e.target.value})}/><input className={input} required placeholder="Фамилия" value={actor.lastName} onChange={e=>setActor({...actor,lastName:e.target.value})}/><input className={input} type="date" value={actor.birthDate||''} onChange={e=>setActor({...actor,birthDate:e.target.value})}/><textarea className={input} rows="5" placeholder="Биография" value={actor.biography||''} onChange={e=>setActor({...actor,biography:e.target.value})}/><button disabled={busy} className={primary}>Сохранить</button></div></form><div><input className={`${input} mb-3`} placeholder="Поиск актёра…" value={search} onChange={e=>setSearch(e.target.value)}/><SimpleTable rows={rows} columns={[['firstName','Имя'],['lastName','Фамилия'],['birthDate','Дата рождения']]} onEdit={onEdit} onDelete={onDelete}/></div></div>; }
function GenresPanel({ rows, search, setSearch, genre, setGenre, editing, busy, onSave, onEdit, onDelete }) { return <div className="grid gap-5 lg:grid-cols-[360px_1fr]"><form onSubmit={onSave} className="h-fit border-2 border-black p-4"><h2 className="mb-3 text-xl font-black">{editing?'ИЗМЕНИТЬ ЖАНР':'НОВЫЙ ЖАНР'}</h2><input className={input} required value={genre.name} onChange={e=>setGenre({name:e.target.value})} placeholder="Название"/><button disabled={busy} className={`${primary} mt-3`}>Сохранить</button></form><div><input className={`${input} mb-3`} value={search} onChange={e=>setSearch(e.target.value)} placeholder="Поиск жанра…"/><SimpleTable rows={rows} columns={[["name","Название"]]} onEdit={onEdit} onDelete={onDelete}/></div></div>; }

function ModerationPanel({ view, setView, comments, ratings, onToggle, onDelete }) { const [q,setQ]=useState(''); const rows=(view==='comments'?comments:ratings).filter(x=>JSON.stringify(x).toLowerCase().includes(q.toLowerCase())); return <><div className="grid gap-3 md:grid-cols-[auto_1fr]"><Segment value={view} onChange={setView} items={[["comments",`Комментарии (${comments.length})`],["ratings",`Оценки (${ratings.length})`]]}/><input className={input} value={q} onChange={e=>setQ(e.target.value)} placeholder="Фильтр по пользователю или материалу…"/></div>{view==='comments'?<SimpleTable rows={rows} columns={[["username","Пользователь"],["target","Материал"],["text","Текст"],["isHidden","Статус",x=>x.isHidden?'СКРЫТ':'ОПУБЛИКОВАН']]} customActions={x=><><button className={secondary} onClick={()=>onToggle(x)}>{x.isHidden?'Опубликовать':'Скрыть'}</button><Confirm onConfirm={()=>onDelete(x.id)}/></>}/>:<SimpleTable rows={rows} columns={[["username","Пользователь"],["contentType","Тип"],["target","Материал"],["ratingValue","Оценка"]]}/>}</>; }

function SimpleTable({ rows, columns, onEdit, onDelete, customActions }) { return <div className="overflow-x-auto border-2 border-black"><table className="w-full text-left text-sm"><thead className="bg-black text-white"><tr>{columns.map(c=><th className="p-3" key={c[0]}>{c[1]}</th>)}{(onEdit||onDelete||customActions)&&<th className="p-3">Действия</th>}</tr></thead><tbody>{rows.map(row=><tr className="border-t-2 border-black" key={row.id}>{columns.map(c=><td className="p-3 align-top" key={c[0]}>{c[2]?c[2](row):row[c[0]]}</td>)}{(onEdit||onDelete||customActions)&&<td className="p-3"><div className="flex gap-2">{customActions?customActions(row):<>{onEdit&&<button className={secondary} onClick={()=>onEdit(row)}>Изменить</button>}{onDelete&&<Confirm onConfirm={()=>onDelete(row.id)}/>}</>}</div></td>}</tr>)}</tbody></table>{!rows.length&&<Empty text="Ничего не найдено"/>}</div>; }
function Poster({ kind, id, hasPoster = true, stamp }) { const [failed,setFailed]=useState(false); useEffect(()=>setFailed(false),[kind,id,stamp]); return <div className="flex h-full min-h-36 items-center justify-center overflow-hidden bg-neutral-100 text-center text-xs font-bold">{hasPoster&&!failed?<img className="h-full w-full object-cover" src={posterSrc(kind,id,stamp)} alt="Постер" onError={()=>setFailed(true)}/>:<span className="p-2">НЕТ<br/>ПОСТЕРА</span>}</div>; }
function FilePicker({ label, accept, file, onChange }) { return <label className={`${secondary} mt-2 block cursor-pointer text-center`}>{file?file.name:label}<input className="hidden" type="file" accept={accept} onChange={e=>onChange(e.target.files?.[0]||null)}/></label>; }
function Segment({ value, onChange, items, disabled=false }) { return <div className="flex flex-wrap">{items.map(([id,label])=><button disabled={disabled} type="button" key={id} onClick={()=>onChange(id)} className={`border-2 border-black px-3 py-2 font-bold ${value===id?'bg-black text-white':'bg-white text-black'} disabled:opacity-50`}>{label}</button>)}</div>; }
function Field({ label, wide, children }) { return <label className={`text-sm font-bold ${wide?'md:col-span-2':''}`}>{label}<div className="mt-1">{children}</div></label>; }
function Badge({ children }) { return <span className="border border-black px-2 py-1 text-[10px] font-black">{children}</span>; }
function Empty({ text }) { return <div className="p-8 text-center font-bold text-neutral-500">{text}</div>; }
function Confirm({ onConfirm, disabled }) { return <button disabled={disabled} className={secondary} onClick={()=>window.confirm('Удалить без возможности восстановления?')&&onConfirm()}>Удалить</button>; }
function useObjectUrl(file) { const [url,setUrl]=useState(''); useEffect(()=>{if(!file){setUrl('');return;}const next=URL.createObjectURL(file);setUrl(next);return()=>URL.revokeObjectURL(next)},[file]);return url; }
