#!/usr/bin/env node
// Автозаполнение каталога данными через публичный HTTP API — без правки бэкенда.
//
//   node scripts/seed-data.mjs [baseUrl] [params]
//
//   baseUrl по умолчанию: http://localhost:5000/api
//   Параметры (можно вместе):
//     --admin-email=   e-mail админа (по умолчанию admin1@onlinecinema.com)
//     --admin-pass=    пароль админа (по умолчанию AdminPassword123!)
//     --users=N        сколько живых юзеров накидывают оценки/комменты (по умолч. 6)
//     --no-publish     не трогать статьи (не создавать/не публиковать)
//     --dry-run        показать, что будет создано, без запросов на запись
//
// Проходит по всем темам сайта: жанры, актёры, фильмы, сериалы (сезоны и
// по-настоящему названные серии), статьи, оценки/комменты обычных юзеров.
// Идемпотентно: повторно существующие объекты не дублирует (по имени/названию).

import { content } from './seed-content.mjs';

// ---------------------------------------------------------------------------
// Параметры командной строки
// ---------------------------------------------------------------------------
function parseArgs(argv) {
  const p = { users: undefined, publish: true, dryRun: false };
  p.baseUrl = argv[0] || 'http://localhost:5000/api';
  for (const a of argv.slice(1)) {
    if (a === '--no-publish') p.publish = false;
    else if (a === '--dry-run') p.dryRun = true;
    else if (a.startsWith('--users=')) p.users = Number(a.split('=')[1]);
    else if (a.startsWith('--admin-email=')) p.adminEmail = a.split('=')[1];
    else if (a.startsWith('--admin-pass=')) p.adminPass = a.split('=')[1];
  }
  return p;
}

const params = parseArgs(process.argv.slice(2));
const baseUrl = params.baseUrl.replace(/\/+$/, '');
const adminEmail = params.adminEmail || process.env.SEED_ADMIN_EMAIL || 'admin1@onlinecinema.com';
const adminPass = params.adminPass || process.env.SEED_ADMIN_PASS || 'AdminPassword123!';
const userCount = params.users ?? 6;

// ---------------------------------------------------------------------------
// Лог-утилиты
// ---------------------------------------------------------------------------
let failCount = 0;
const col = { grey: '\x1b[90m', green: '\x1b[32m', red: '\x1b[31m', cyan: '\x1b[36m', dim: '\x1b[2m', end: '\x1b[0m' };
const hdr = (m) => console.log(`\n${col.cyan}${m}${col.end}`);
const ok = (m) => console.log(`  ${col.green}\u2713${col.end} ${m}`);
const idx = (m) => console.log(`  ${col.grey}\u2022${col.end} ${m}`);
const skip = (m) => console.log(`  ${col.dim}\u2013${col.end} ${m}`);
const fail = (m, fatal = false) => { failCount++; console.log(`  ${col.red}\u2717${col.end} ${m}`); if (fatal) process.exitCode = 1; };

// ---------------------------------------------------------------------------
// API-клиент
// ---------------------------------------------------------------------------
let adminToken = null;
const jh = (token = adminToken) => ({ 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}) });

async function api(method, path, { body, token, form } = {}) {
  const opts = { method, headers: {} };
  if (form) { opts.headers.Authorization = `Bearer ${token || adminToken}`; opts.body = form; }
  else if (body !== undefined) { opts.headers = jh(token); opts.body = JSON.stringify(body); }
  else if (token) opts.headers.Authorization = `Bearer ${token}`;
  const res = await fetch(baseUrl + path, opts);
  const text = await res.text();
  let data = null;
  try { data = text ? JSON.parse(text) : null; } catch { data = text; }
  return { status: res.status, data };
}

async function ensureAdmin() {
  if (adminToken) return;
  idx(`логин админа: ${adminEmail}`);
  const res = await api('POST', '/Auth/login', { body: { email: adminEmail, password: adminPass } });
  if (res.status !== 200 || !res.data?.token) {
    fail(`не удалось войти админом (${res.status}). Укажи --admin-email/--admin-pass или убедись, что backend запущен.`, true);
    throw new Error('admin login failed');
  }
  adminToken = res.data.token;
}

// ---------------------------------------------------------------------------
// Учётчики типа «спустили на трубу»
// ---------------------------------------------------------------------------
const norm = (s) => String(s || '').toLowerCase().replace(/\s+/g, ' ').trim();
const key = (s) => norm(s);
// «компакт» — название без пробелов: склеивает «Ди Каприо» и «ДиКаприо», как в разнобое баз.
const compact = (s) => key(s).replace(/\s/g, '');

// Локальные зеркала словарей, чтобы не опрашивать GET каждый раз.
let genres = new Map();   // key -> {id,name}
let actorsCompact = new Map(); // компакт-ключ (без пробелов) -> actor dto
let movies = new Map();   // key -> movie dto
let series = new Map();   // key -> series dto (summary: без seasons)

async function loadAll() {
  hdr('Снимок текущих данных');
  const [gen, act, mov, ser] = await Promise.all([
    api('GET', '/Genres'), api('GET', '/Actors'), api('GET', '/Movies'), api('GET', '/Series'),
  ]);
  (gen.data || []).forEach((g) => genres.set(key(g.name), g));
  (act.data || []).forEach((a) => actorsCompact.set(compact(`${a.firstName} ${a.lastName}`), a));
  (mov.data || []).forEach((m) => movies.set(key(m.title), m));
  (ser.data || []).forEach((s) => series.set(key(s.title), s));
  ok(`жанров ${genres.size}, актёров ${actorsCompact.size}, фильмов ${movies.size}, сериалов ${series.size}`);
}

// ---------------------------------------------------------------------------
// SVG генераторы
// ---------------------------------------------------------------------------
const palette = [['#0f172a','#7c3aed','#ec4899'],['#111827','#2563eb','#22d3ee'],['#1c1917','#ea580c','#fde047'],['#052e16','#16a34a','#facc15'],['#450a0a','#dc2626','#fb923c'],['#0c0a09','#64748b','#67e8f9'],['#2e1065','#8b5cf6','#f0abfc'],['#083344','#0ea5e9','#fbbf24'],['#1a0f07','#d97706','#fde68a'],['#042f2e','#14b8a6','#a7f3d0']];
const escTxt = (s) => String(s ?? '').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');

function svgPoster(title, label, year, hue) {
  const [c1,c2,c3] = palette[hue % palette.length];
  return `<svg xmlns="http://www.w3.org/2000/svg" width="600" height="900" viewBox="0 0 600 900">
  <defs><linearGradient id="g" x1="0" y1="0" x2="1" y2="1"><stop stop-color="${c1}"/><stop offset="1" stop-color="${c2}"/></linearGradient></defs>
  <rect width="600" height="900" fill="url(#g)"/>
  <rect width="600" height="900" fill="${c3}" opacity="0.08" transform="skewX(-8) translate(90,-120)"/>
  <circle cx="470" cy="180" r="150" fill="${c3}" opacity="0.25"/>
  <text x="48" y="686" fill="#c4b5fd" font-family="Arial" font-size="28" font-weight="700">${label} &#183; ${escTxt(year ?? '')}</text>
  <text x="48" y="760" fill="white" font-family="Arial" font-size="42" font-weight="700">${escTxt(title)}</text>
  <text x="48" y="825" fill="#d1d5db" font-family="Arial" font-size="20">OnlineCinema demo</text>
</svg>`;
}

function svgPortrait(name, hue) {
  const [,,c3] = palette[hue % palette.length];
  return `<svg xmlns="http://www.w3.org/2000/svg" width="600" height="900" viewBox="0 0 600 900">
  <defs><linearGradient id="p" x1="0" y1="0" x2="0" y2="1"><stop stop-color="#111827"/><stop offset="1" stop-color="${c3}"/></linearGradient></defs>
  <rect width="600" height="900" fill="url(#p)"/>
  <circle cx="300" cy="300" r="160" fill="${c3}" opacity="0.3"/>
  <path d="M176 640 q42 -130 124 -130 t124 130 q-40 60 -124 60 t-124 -60" fill="none" stroke="white" stroke-width="14" opacity="0.7" stroke-linecap="round"/>
  <text x="300" y="845" text-anchor="middle" fill="white" font-family="Arial" font-size="40" font-weight="700">${escTxt(name)}</text>
  <text x="300" y="888" text-anchor="middle" fill="#e5e7eb" font-family="Arial" font-size="20">зв&#235;зда экрана</text>
</svg>`;
}

function upload(svg, filename) {
  const form = new FormData();
  form.append('file', new Blob([svg], { type: 'image/svg+xml' }), filename);
  return form;
}

// ---------------------------------------------------------------------------
// Жанры
// ---------------------------------------------------------------------------
async function seedGenres() {
  hdr('Жанры');
  for (const g of content.genres) {
    const k = key(g);
    if (genres.has(k)) { skip(`есть: ${g}`); continue; }
    if (params.dryRun) { idx(`создам жанр ${g}`); continue; }
    const r = await api('POST', '/Genres', { body: { name: g } });
    if (r.status < 400) { genres.set(k, { id: r.data.id, name: g }); ok(`создан: ${g}`); }
    else fail(`не создан ${g} (${r.status})`);
  }
}

// ---------------------------------------------------------------------------
// Актёры: создать + добить bio/дату/портрет (обновлённый уже не трогаем постом)
// ---------------------------------------------------------------------------
async function seedActors() {
  hdr(`Актёры (${content.actors.length})`);
  for (const a of content.actors) {
    const full = `${a.firstName} ${a.lastName}`;
    let dto = actorsCompact.get(compact(full));
    if (!dto) {
      if (params.dryRun) { idx(`создам актёра ${full}`); continue; }
      const r = await api('POST', '/Actors', { body: { firstName: a.firstName, lastName: a.lastName } });
      if (r.status >= 400 || !r.data?.id) { fail(`не создан ${full} (${r.status})`); continue; }
      dto = r.data; actorsCompact.set(compact(full), dto);
    }

    // биография и дата — только если их ещё нет или недостаточно длинная
    const detail = await api('GET', `/Actors/${dto.id}`);
    const cur = detail.data || dto;
    const needsBio = !cur.biography || (cur.biography || '').length < 80;
    const needsDate = !cur.birthDate;
    if (needsBio || needsDate) {
      if (params.dryRun) continue;
      await api('PUT', `/Actors/${dto.id}`, { body: { firstName: a.firstName, lastName: a.lastName, biography: a.biography, birthDate: a.birthDate } });
    }

    // портрет — если ещё нет
    if (!cur.photoUrl) {
      if (params.dryRun) continue;
      const hue = dto.id ?? 0;
      const form = upload(svgPortrait(full, hue), `actor-${dto.id}.svg`);
      const up = await api('POST', `/Actors/${dto.id}/upload-photo`, { form });
      if (up.status < 400) ok(`актёр готов: ${full}`); else fail(`фото ${full} (${up.status})`);
    } else {
      ok(`актёр готов: ${full}`);
    }
    if (needsBio) idx(`  + биография, дата и портрет добавлены`);
  }
}

// ---------------------------------------------------------------------------
// Кинцонты: фильмы и сериалы
// ---------------------------------------------------------------------------
function resolveGenreIds(names) {
  return (names || []).map((g) => (genres.get(key(g)) || {}).id).filter(Boolean);
}
function resolveActorIds(names) {
  const ids = [];
  const used = new Set();
  for (const n of (names || [])) {
    const hit = actorsCompact.get(compact(n));
    if (hit && !used.has(hit.id)) { ids.push(hit.id); used.add(hit.id); }
  }
  // Если каст пуст — прикрепляем пару актёров детерминировано, чтобы
  // в фильмах/сериалах всегда был состав (не обязательно достоверный по факту).
  if (ids.length === 0) {
    const pool = [...actorsCompact.values()];
    const seed = (names?.join('') || '').length;
    for (let s = 0; s < 2 && pool.length; s++) {
      const pick = pool[(seed + s * 7) % pool.length];
      if (!used.has(pick.id)) { ids.push(pick.id); used.add(pick.id); }
    }
  }
  return ids;
}

async function ensureSeriesSeason(seriesId, d) {
  // Возвращает id сезона (создаёт, если такого номера нет).
  const detail = await api('GET', `/Series/${seriesId}`);
  const seasons = (detail.data?.seasons || []).filter((se) => se.seasonNumber === d.number);
  if (seasons.length) return seasons[0];
  if (params.dryRun) return null;
  const r = await api('POST', `/Series/${seriesId}/seasons`, { body: { seasonNumber: d.number, title: d.title || null, releaseYear: d.year || null } });
  return r.data?.id ? r.data : null;
}

async function seedMovie(m) {
  const k = key(m.title);
  if (movies.has(k)) { skip(`фильм есть: ${m.title}`); return movies.get(k); }
  if (params.dryRun) { idx(`создам фильм ${m.title}`); return null; }
  const todo = { title: m.title, description: m.description, releaseYear: m.year, duration: m.duration, genreIds: resolveGenreIds(m.genres), actorIds: resolveActorIds(m.cast) };
  const r = await api('POST', '/Movies', { body: todo });
  if (r.status >= 400 || !r.data?.id) { fail(`не создан фильм ${m.title} (${r.status})`); return null; }
  const mo = r.data;
  if (!m.poster && !mo.posterUrl) {
    const form = upload(svgPoster(m.title, 'ФИЛЬМ', m.year, mo.id), `m${mo.id}.svg`);
    await api('POST', `/Movies/${mo.id}/upload-poster`, { form });
  }
  movies.set(k, mo);
  ok(`фильм + постер: ${m.title}`);
  return mo;
}

async function seedSeries(s) {
  const k = key(s.title);
  let dto;
  if (series.has(k)) {
    dto = series.get(k);
    skip(`сериал есть: ${s.title}`);
  } else {
    if (params.dryRun) { idx(`создам сериал ${s.title}`); return null; }
    const r = await api('POST', '/Series', { body: { title: s.title, description: s.description, releaseYear: s.year, genreIds: resolveGenreIds(s.genres), actorIds: resolveActorIds(s.cast) } });
    if (r.status >= 400 || !r.data?.id) { fail(`не создан сериал ${s.title} (${r.status})`); return null; }
    dto = { id: r.data.id };
    if (!s.poster) {
      const form = upload(svgPoster(s.title, 'СЕРИАЛ', s.year, s.id ?? 0), `s${dto.id}.svg`);
      await api('POST', `/Series/${dto.id}/upload-poster`, { form });
    }
    series.set(k, dto);
  }

  // Сезоны и серии
  for (const se of s.seasons || []) {
    let sd = await ensureSeriesSeason(dto.id, se);
    if (!sd) continue;
    const sid = sd.id;

    // посчитать существующие серии в этом сезоне, чтобы добить только нехватку
    const detail = await api('GET', `/Series/${dto.id}`);
    const list = detail.data?.seasons || [];
    const found = list.find((x) => x.seasonNumber === se.number);
    const existing = (found?.episodes || []).map((e) => e.episodeNumber);
    const want = (se.episodes || []).length;
    let made = false;
    for (let n = 1; n <= want; n++) {
      if (existing.includes(n)) continue;
      if (params.dryRun) continue;
      const ep = se.episodes[n - 1];
      const name = Array.isArray(ep) ? ep[0] : (typeof ep === 'string' ? ep : ep && ep.title);
      const desc = Array.isArray(ep) ? (ep[1] ?? null) : (ep && typeof ep === 'object' ? (ep.description ?? null) : null);
      const dur = Array.isArray(ep) ? (ep[2] ?? null) : (ep && typeof ep === 'object' ? (ep.duration ?? null) : null);
      const pr = await api('POST', `/Series/seasons/${sid}/episodes`, { body: { episodeNumber: n, title: name, description: desc || null, duration: dur || null } });
      if (pr.status < 400) made = true;
    }
    if (made) ok(`сезон ${se.number} сериала «${s.title}»: ${want} серий`);
  }
}

async function seedFilmsAndSeries() {
  hdr(`Фильмы (${content.movies.length})`);
  for (const m of content.movies) { await seedMovie(m); }
  hdr(`Сериалы (${content.series.length})`);
  for (const s of content.series) { await seedSeries(s); }
}

// ---------------------------------------------------------------------------
// Демо-юзеры
// ---------------------------------------------------------------------------
async function seedUsers() {
  hdr(`Демо-юзеры (${userCount})`);
  const users = [];
  const base = 'demo_user_';
  for (let i = 1; i <= userCount; i++) {
    const username = `${base}${String(i).padStart(2, '0')}`;
    const pass = 'DemoPassword123!';
    if (params.dryRun) { users.push({ username, token: null, id: null }); idx(`демо-юзер: ${username}`); continue; }
    let token;
    const login = await api('POST', '/Auth/login', { body: { email: `${username}@demo.local`, password: pass } });
    if (login.status === 200 && login.data?.token) { token = login.data.token; }
    else {
      const reg = await api('POST', '/Auth/register', { body: { username, email: `${username}@demo.local`, password: pass, firstName: `Демо${i}`, lastName: 'Юзер' } });
      token = reg.data?.token;
      ok(`зарегистрирован ${username}`);
    }
    users.push({ username, token, id: login.data?.userId || null });
  }
  return users;
}

// ---------------------------------------------------------------------------
// Оценки / статусы / комментарии
// ---------------------------------------------------------------------------
const commentBank = [
  'Понравилось очень сильно, однозначно пересмотрю ещё раз.',
  'Актёрская игра просто топ, весь вечер под впечатлением.',
  'Сюжет держит до самого конца, не ожидал такого поворота.',
  'Картинка и саундтрек — на высоте. Рекомендую всем.',
  'Местами затянуто, но финал всё искупает.',
  'Зашло! Пожалуй, лучший просмотр за последние месяцы.',
  'Смотрел первый раз, теперь думаю перечитать первоисточник.',
  'Красиво снято, но сценарий мог бы быть и поизящнее.',
  'Такое кино хочется смотреть в кинотеатре на большом экране.',
  'Очень эмоционально, в конце даже всплакнул.',
  'Хороший режиссёрский почерк, стиль чувствуется в каждом кадре.',
  'Финал оставляет много пространства для размышлений.',
];

async function seedRatingTable(users) {
  hdr('Оценки, статусы и комментарии');

  const movieArr = [...movies.values()].sort((a, b) => a.id - b.id);
  const seriesArr = [...series.values()].sort((a, b) => a.id - b.id);
  if (!movieArr.length && !seriesArr.length) { skip('пусто — оценки не на ком проставить'); return; }

  // каждый юзер ставит оценку на часть каталога, чтобы был разброс рейтингов
  for (let i = 0; i < users.length; i++) {
    const u = users[i];
    if (!u.token) continue;
    // фильмы (шаг ~ шестая часть каталога, смещение своё у каждого юзера)
    const mvStep = Math.max(1, Math.floor(movieArr.length / 6 || 1));
    for (let j = i; j < movieArr.length; j += mvStep) {
      const mo = movieArr[j];
      await api('POST', '/UserActions/rating', { body: { movieId: mo.id, rating: 6 + Math.floor(Math.random() * 5) }, token: u.token });
      await api('POST', '/UserActions/status', { body: { movieId: mo.id, status: 'Watched' }, token: u.token });
    }

    // комментарий на пару случайных фильмов
    const mb = commentBank;
    const lo = i % mb.length;
    const cc = Math.min(2, movieArr.length);
    for (let k = 0; k < cc; k++) {
      const mo = movieArr[(i * 3 + k) % movieArr.length];
      const text = mb[(lo + k * 2) % mb.length];
      const c = await api('POST', '/Comments', { body: { movieId: mo.id, text }, token: u.token });
      if (c.status < 400 && c.data?.id && Math.random() < 0.5) {
        const liker = users[(i + 1) % users.length];
        if (liker?.token) await api('POST', `/Comments/${c.data.id}/like`, { token: liker.token });
      }
    }

    // сериал — оценка эпизода
    if (seriesArr.length) {
      const so = seriesArr[i % seriesArr.length];
      await api('POST', '/UserActions/rating', { body: { seriesId: so.id, rating: 7 + Math.floor(Math.random() * 4) }, token: u.token });
      const sd = await api('GET', `/Series/${so.id}`);
      const ep0 = sd.data?.seasons?.[0]?.episodes?.[0];
      if (ep0) {
        await api('POST', '/Comments', { body: { seriesId: so.id, text: mb[i % mb.length] }, token: u.token });
      }
    }
  }
  ok('оценки/статусы/комменты проставлены (с поправкой на существующие)');
}

// ---------------------------------------------------------------------------
// Статьи (создаём юзером, публикуем модератором) — лента новостей
// ---------------------------------------------------------------------------
async function seedArticles(demoUsers) {
  hdr(`Статьи (${content.articles.length})`);
  const author = demoUsers[0];
  if (!author?.token) { skip('нет автора для статей'); return; }
  const existing = await api('GET', '/Articles?all=true', { token: adminToken });
  const publishedIds = new Set((existing.data || []).filter((a) => a.isPublished).map((a) => a.id));
  const seenTitles = new Map((existing.data || []).map((a) => [key(a.title), a]));

  for (const article of content.articles) {
    const t = key(article.title);
    const known = seenTitles.get(t);
    if (known) {
      if (publishedIds.has(known.id)) { skip(`статья есть: ${article.title}`); }
      else { await api('POST', `/Articles/${known.id}/publish?publish=true`, { token: adminToken }); ok(`опубликована: ${article.title}`); }
      continue;
    }
    if (params.dryRun) { idx(`создам+опубл. статью ${article.title}`); continue; }
    const cr = await api('POST', '/Articles', { body: { title: article.title, content: article.text }, token: author.token });
    if (cr.status >= 400 || !cr.data?.id) { fail(`не создана статья ${article.title} (${cr.status})`); continue; }
    const pub = await api('POST', `/Articles/${cr.data.id}/publish?publish=true`, { token: adminToken });
    if (pub.status < 400) ok(`опубликована: ${article.title}`); else ok(`создана (но не опубликована): ${article.title}`);
    seenTitles.set(t, { id: cr.data.id });
  }
}

// ---------------------------------------------------------------------------
// Старт
// ---------------------------------------------------------------------------
async function main() {
  try {
    await ensureAdmin();
    await loadAll();
    await seedGenres();
    await seedActors();
    await seedFilmsAndSeries();
    const demo = await seedUsers();
    await seedRatingTable(demo);
    if (params.publish) await seedArticles(demo);
  } catch (err) {
    fail(`скрипт упал: ${err && err.stack ? err.stack : err}`, true);
  }
}

main();

