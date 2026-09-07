import { useEffect, useMemo, useState, useContext } from 'react';
import { api } from '../api/axios';
import { Link } from 'react-router-dom';
import { AuthContext } from '../context/AuthContext';

/* ============================================================================
   Аналитика платформы.
   Дизайн-принципы:
   - "Топ по просмотрам" и "Топ по оценкам" — ДВА РАЗНЫХ рейтинга, не смешиваются.
   - Вместо голой пятёрки — полноэкранная, сортируемая таблица по всему каталогу.
   - Фильтры: период, тип, жанр, поиск, сортировка — внутри одной страницы (без
     лишних запросов): весь датасет приходит одним запросом, фильтры локальные.
   - Хронология просмотров и жанровая разбивка — чтобы было видно не только "топ".
   ============================================================================ */

const PERIODS = [
  { key: '7', label: '7 дней', days: 7 },
  { key: '30', label: '30 дней', days: 30 },
  { key: '90', label: '90 дней', days: 90 },
  { key: 'all', label: 'Всё время', days: 0 },
];

const TYPE_LABEL = { all: 'Всё', movie: 'Фильмы', series: 'Сериалы' };

const nFmt = (v) => (Number(v) || 0).toLocaleString('ru-RU');
const sFmt = (v) => {
  const n = Number(v) || 0;
  if (n >= 1_000_000) return (n / 1e6).toFixed(1).replace('.', ',') + 'M';
  if (n >= 1000) return (n / 1e3).toFixed(1).replace('.', ',') + 'k';
  return nFmt(n);
};
const durFmt = (sec) => {
  const s = Number(sec) || 0;
  const h = s / 3600;
  if (h === 0) return '0 мин';
  return h >= 10 ? `${h.toFixed(0)} ч` : h >= 1 ? `${h.toFixed(1).replace('.', ',')} ч` : `${Math.round(s / 60)} мин`;
};
const dayLabel = (iso) => {
  const b = String(iso || '').split('-').slice(1, 3).join('.');
  return b || iso;
};

const hrefOf = (type, id) => (type === 'movie' ? `/movie/${id}` : `/series/${id}`);
const rankClass = ['bg-zinc-900', 'bg-zinc-600', 'bg-zinc-400'];

/* Коль?? нет — простая полоска. */
function BarRow({ title, type, id, rank, value, txt, sub, max, accent }) {
  const pct = max > 0 ? Math.min(100, (Math.max(0, value) / max) * 100) : 0;
  return (
    <Link to={hrefOf(type, id)} className="group relative block overflow-hidden border-b border-black/10 py-1.5 px-0.5 hover:bg-neutral-50">
      <div className="absolute inset-y-0 left-0 group-hover:bg-black/5 transition-colors" />
      <div className="absolute inset-y-0 left-0 opacity-90 group-hover:opacity-100" style={{ width: `${pct}%`, background: accent }} />
      <div className="relative flex items-center gap-2">
        {rank && <span className={`w-6 h-6 flex-none grid place-items-center text-xs font-black text-white ${rankClass[rank - 1] || 'bg-neutral-300'}`}>{rank}</span>}
        <span className="truncate font-semibold group-hover:underline">{title}</span>
        <span className="ml-auto flex-none font-black text-sm tabular-nums">{txt}</span>
        {sub != null && <span className="hidden sm:flex items-center flex-none text-xs text-gray-500">{sub}</span>}
      </div>
    </Link>
  );
}

function Panel({ title, children, hint }) {
  return (
    <section>
      <div className="flex items-baseline justify-between border-b-4 border-black pb-1.5 mb-2">
        <h3 className="font-black uppercase text-lg">{title}</h3>
        {hint && <span className="text-xs text-gray-400 normal-case font-medium">{hint}</span>}
      </div>
      {children}
    </section>
  );
}

const emptyBlock = (msg) => (
  <p className="border border-dashed border-gray-300 p-5 text-sm italic text-gray-500">{msg}</p>
);

/* ===== мини-гистограмма (SVG без библиотек) ===== */
function Histogram({ points, unit = 'просм.', limit = 0 }) {
  if (!points || points.length === 0) return emptyBlock('За период нет событий — данные появятся после реальных просмотров.');
  const shown = limit > 0 ? points.slice(-limit) : points;
  const max = Math.max(...shown.map((p) => p.v), 1);
  const wPer = 30;
  const W = shown.length * wPer + 20;
  const H = 120;
  const base = H - 18;
  return (
    <div className="overflow-x-auto">
      <svg viewBox={`0 0 ${Math.max(W, 240)} ${H}`} width="100%" height={H}>
        <line x1="0" y1={base} x2={Math.max(W, 240)} y2={base} stroke="#d4d4d8" />
        {shown.map((p, i) => {
          const bh = p.v > 0 ? Math.max(3, (p.v / max) * (base - 10)) : 1;
          const x = i * wPer + 8;
          return (
            <g key={i}>
              <title>{`${p.l} — ${p.v} ${unit}`}</title>
              <rect x={x} y={base - bh} width={wPer - 12} height={bh}
                fill={p.v > 0 ? '#18181b' : '#e4e4e7'} rx="1" />
              <text x={x + (wPer - 12) / 2} y={H - 5} textAnchor="middle" fontSize="8.5" fill="#71717a">{p.l}</text>
            </g>
          );
        })}
      </svg>
    </div>
  );
}

/* ===== сама страница ===== */
export const AnalyticsDashboard = () => {
  const { user } = useContext(AuthContext);
  const role = user?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || user?.role;
  const [period, setPeriod] = useState('30');
  const [type, setType] = useState('all');
  const [genreId, setGenreId] = useState('');
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [nudge, setNudge] = useState(0);

  const days = (PERIODS.find((p) => p.key === period) || PERIODS[0]).days;

  // Один запрос отдаёт весь датасет (каталог + сводку). Настройка периода/жанра/типа
  // прогоняется на клиенте — отличные от подёргиваний вкладки.
  useEffect(() => {
    const ctrl = new AbortController();
    api.get(`/analytics/dashboard?days=${days}`, { signal: ctrl.signal })
      .then((r) => { if (!ctrl.signal.aborted) { setData(r.data); setError(''); } })
      .catch((e) => {
        if (e?.code === 'ERR_CANCELED' || ctrl.signal.aborted) return;
        setError('Не удалось загрузить аналитику.');
      })
      .finally(() => { if (!ctrl.signal.aborted) setLoading(false); });
    return () => ctrl.abort();
  }, [days, nudge]);

  // Жанры живут внутри ответа дашборда — источник и для фильтра, и для селекта.
  const allGenres = useMemo(() => (data?.genres || []), [data]);

  // --- фильтрация / сортировка локально ---
  const visible = useMemo(() => {
    let rows = data?.content || [];

    if (type !== 'all') rows = rows.filter((c) => c.contentType === type);
    if (genreId) {
      const g = allGenres.find((x) => String(x.genreId) === genreId);
      if (g) rows = rows.filter((c) => (c.genres || []).includes(g.name));
    }
    return rows;
  }, [data, type, genreId, allGenres]);

  // --- топы строятся только над уже отфильтрованной выборкой ---
  const [search, setSearch] = useState('');
  const [sort, setSort] = useState('popular');
  const [page, setPage] = useState(0);
  const PAGE = 12;

  const searched = useMemo(() => {
    if (!search.trim()) return visible;
    const q = search.trim().toLowerCase();
    return visible.filter((c) =>
      (c.title || '').toLowerCase().includes(q) ||
      (c.genres || []).some((gg) => gg.toLowerCase().includes(q)));
  }, [visible, search]);

  const sorted = useMemo(() => {
    const cmp = {
      popular: (a, b) => (b.watches - a.watches) || (b.watchesInRange - a.watchesInRange) || ((b.avgRating ?? -1) - (a.avgRating ?? -1)),
      topRating: (a, b) => ((b.avgRating ?? -1) - (a.avgRating ?? -1)) || (b.ratingCount - a.ratingCount),
      ratingCount: (a, b) => b.ratingCount - a.ratingCount,
      watchToday: (a, b) => (b.watchesInRange - a.watchesInRange) || (b.watches - a.watches),
      title: (a, b) => (a.title || '').localeCompare(b.title || ''),
    }[sort] || ((a, b) => b.watches - a.watches);
    return [...searched].sort(cmp);
  }, [searched, sort]);

  // сброс страницы таблицы при смене фильтров (из кликов ниже в UI)
  const pages = Math.max(1, Math.ceil(sorted.length / PAGE));
  const curPage = Math.min(page, pages - 1);

  // метрика «сколько смотрим»: число за окно, либо по всем, если окно пустое
  const watchCount = (c) => {
    const r = Number(c.watchesInRange) || 0;
    return r > 0 ? r : Number(c.watches) || 0;
  };

  // Топ-10 по просмотрам И Топ-10 по оценкам считаются независимо,
  // каждый над отфильтрованной выборкой (`visible`), отдельно от сортировки таблицы.
  const byWatches = useMemo(() => {
    const rows = visible.filter((c) => watchCount(c) > 0)
      .sort((a, b) => watchCount(b) - watchCount(a))
      .slice(0, 10);
    return { rows, has: rows.length > 0 };
  }, [visible]);

  const watchesMax = byWatches.rows.length
    ? Math.max(...byWatches.rows.map((r) => Math.max(watchCount(r), 1)))
    : 1;

  const byRating = useMemo(() => {
    const rows = visible
      .filter((c) => (c.ratingCount || 0) > 0)
      .sort((a, b) => ((b.avgRating ?? -1) - (a.avgRating ?? -1)) || (b.ratingCount - a.ratingCount))
      .slice(0, 10);
    return { rows, has: rows.length > 0 };
  }, [visible]);

  // --- производные для сводки ---
  // Пока данные не пришли (или ответ неполный) у нас есть дефолты нулевых метрик,
  // чтобы блоки ниже не падали при ошибке загрузки.
  const overview = data?.overview || {
    movies: 0, series: 0,
    activeViewers: 0, watchEvents: 0, watchSeconds: 0,
    rangeViewers: 0, rangeWatchEvents: 0, rangeWatchSeconds: 0,
    rangeRatings: 0, totalRatings: 0,
  };
  const timeline = data?.activity || [];

  // нормализация жанровых полос по своим максимумам (библиотека vs просмотры)
  const genreView = useMemo(() => {
    const stats = allGenres.map((g) => ({
      ...g,
      titles: (g.movies || 0) + (g.series || 0),
      views: g.watchEvents || 0,
    }));
    const maxTitles = Math.max(1, ...stats.map((g) => g.titles));
    const maxViews = Math.max(1, ...stats.map((g) => g.views));
    return { stats, maxTitles, maxViews };
  }, [allGenres]);

  if (role !== 'Admin' && role !== 'Moderator') {
    return (
      <div className="border-2 border-black p-8 text-center">
        <h1 className="text-3xl font-black uppercase mb-2">403</h1>
        <p className="text-lg font-medium">Аналитика доступна только администратору или модератору.</p>
      </div>
    );
  }

  if (loading && !data) {
    return (
      <div className="space-y-6">
        <div className="animate-pulse"><div className="h-10 w-2/3 bg-neutral-200 border" /><div className="h-4 w-1/2 bg-neutral-200 mt-2" /></div>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">{[0, 1, 2, 3].map((i) => <div key={i} className="h-24 animate-pulse bg-neutral-100 border border-black/10" />)}</div>
        <div className="grid md:grid-cols-2 gap-5">{[0, 1].map((i) => <div key={i} className="h-64 animate-pulse bg-neutral-100 border border-black/10" />)}</div>
      </div>
    );
  }

  return (
    <div className="space-y-7">
      <header>
        <h1 className="text-4xl font-black uppercase border-l-8 border-black pl-4">Аналитика</h1>
        <p className="text-sm text-zinc-500 italic mt-2">Общедоступная статистика платформы. Просмотры считаются по фактам реального (устойчивого) воспроизведения — фильма целиком или эпизода, — а не по «вольтеру». Оценки учитывают количество голосов, поэтому рейтинг по оценкам уместен даже если просмотров мало.</p>
      </header>

      {error && (
        <div className="border-2 border-red-600 bg-red-50 text-red-700 font-semibold p-3 flex items-center justify-between">
          <span>{error}</span>
          <button onClick={() => { setNudge((x) => x + 1); setLoading(true); }} className="border-2 border-red-600 px-3 py-1 hover:bg-red-600 hover:text-white">Повторить</button>
        </div>
      )}

      {/* Фильтры-панель */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-3 border-2 border-black p-3">
        <div>
          <div className="text-[11px] uppercase font-bold text-zinc-400 mb-1">Период</div>
          <div className="inline-flex flex-wrap border-2 border-black">
            {PERIODS.map((p) => (
              <button key={p.key} onClick={() => setPeriod(p.key)}
                className={`px-2.5 py-1 text-xs font-semibold ${period === p.key ? 'bg-black text-white' : 'hover:bg-neutral-100'}`}>
                {p.label}
              </button>
            ))}
            <span className="px-2 self-center text-xs text-zinc-400">окно анализа</span>
          </div>
        </div>
        <div>
          <div className="text-[11px] uppercase font-bold text-zinc-400 mb-1">Тип контента</div>
          <div className="inline-flex border-2 border-black">
            {Object.entries(TYPE_LABEL).map(([k, l]) => (
              <button key={k} onClick={() => setType(k)}
                className={`px-3 py-1 text-xs font-semibold ${type === k ? 'bg-black text-white' : 'hover:bg-neutral-100'}`}>
                {l}
              </button>
            ))}
          </div>
        </div>
        <div>
          <div className="text-[11px] uppercase font-bold text-zinc-400 mb-1">Жанр</div>
          <select value={genreId} onChange={(e) => setGenreId(e.target.value)}
            className="block w-full border-2 border-black px-2 py-1 bg-white text-sm">
            <option value="">Все жанры</option>
            {allGenres.map((g) => <option key={g.genreId} value={g.genreId}>{g.name}</option>)}
          </select>
        </div>
      </div>

      {/* Карточки KPI за выбранный период */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
        <div className="border-2 border-black p-3">
          <div className="text-[11px] uppercase font-bold text-zinc-400">В каталоге</div>
          <div className="text-3xl font-black">
            {nFmt(overview.movies)} <span className="text-lg text-zinc-400 font-semibold">фильм</span>
            <br /><span className="text-xl text-zinc-600 font-black">{nFmt(overview.series)} сериал</span>
          </div>
        </div>
        <div className="border-2 border-black p-3">
          <div className="text-[11px] uppercase font-bold text-zinc-400">Активность за период</div>
          <div className="text-3xl font-black">{nFmt(overview.rangeViewers)}</div>
          <div className="text-xs text-zinc-500">зрителей · {nFmt(overview.rangeWatchEvents)} просмотров · {durFmt(overview.rangeWatchSeconds)}</div>
        </div>
        <div className="border-2 border-black p-3">
          <div className="text-[11px] uppercase font-bold text-zinc-400">Всего просмотров</div>
          <div className="text-3xl font-black">{sFmt(overview.watchEvents)}</div>
          <div className="text-xs text-zinc-500">{nFmt(overview.activeViewers)} зрителей · {durFmt(overview.watchSeconds)} времени</div>
        </div>
        <div className="border-2 border-black p-3">
          <div className="text-[11px] uppercase font-bold text-zinc-400">Оценок за период</div>
          <div className="text-3xl font-black">{nFmt(overview.rangeRatings)}</div>
          <div className="text-xs text-zinc-500">из {nFmt(overview.totalRatings)} всего</div>
        </div>
      </div>

      {/* Раздельные топы: просмотры vs оценки */}
      <div className="grid md:grid-cols-2 gap-6">
        <Panel title="Топ по просмотрам" hint={`по «${PERIODS.find((p) => p.key === period).label.toLowerCase()}» за окно анализа`}>
            {!byWatches.has
              ? emptyBlock('Просмотров пока нет — топ сформируется, как только зрители реально досмотрят контент в этом окне.')
              : byWatches.rows.map((c, i) => (
                  <BarRow key={'w' + c.contentType + c.contentId} rank={i + 1} title={c.title}
                    type={c.contentType} id={c.contentId}
                    value={watchCount(c)} max={watchesMax}
                    txt={sFmt(watchCount(c))}
                    sub={`${c.ratingCount ? '★ ' + Number(c.avgRating).toFixed(1) : 'без оценки'} · ${c.contentType === 'movie' ? 'Ф' : 'С'} ${c.releaseYear || '—'}`}
                    accent="linear-gradient(90deg,#a5b4fc,#1d4ed8)" />
                ))}
        </Panel>
        <Panel title="Топ по оценкам" hint="средняя оценка (нужен хотя бы один голос)">
          {!byRating.has
            ? emptyBlock('Оценок пока нет — рейтинг появится после первых голосов в этой выборке.')
            : byRating.rows.map((c, i) => (
              <BarRow key={'r' + c.contentType + c.contentId} rank={i + 1} title={c.title}
                type={c.contentType} id={c.contentId}
                value={Number(c.avgRating)} max={10}
                txt={'★ ' + Number(c.avgRating).toFixed(1)}
                sub={`${nFmt(c.ratingCount)} оцен. · ${sFmt(watchCount(c))} просм. · ${c.contentType === 'movie' ? 'Ф' : 'С'} ${c.releaseYear || '—'}`}
                accent="linear-gradient(90deg,#86efac,#047857)" />
            ))}
        </Panel>
      </div>

      {/* Разбивки: динамика по дням + жанры */}
      <div className="grid md:grid-cols-2 gap-6">
        <Panel title="Просмотры по дням" hint="за выбранный период">
          <Histogram points={timeline.map((t) => ({ l: dayLabel(t.label), v: t.watches }))} />
        </Panel>

        <Panel title="Жанровый охват" hint="длина полосы = доля просмотров жанра (по шкале максимального жанра)">
          {genreView.stats.length === 0
            ? emptyBlock('Жанры не распределены по каталогу.')
            : (
              <div className="space-y-2">
                {genreView.stats.map((g) => (
                  <div key={g.genreId} className="grid grid-cols-[104px_1fr] items-center gap-2 text-sm">
                    <span className="flex items-center justify-between gap-1 truncate text-right">
                      <span className="truncate font-medium">{g.name}</span>
                      <span className="text-zinc-400 text-xs whitespace-nowrap">{nFmt(g.titles)}</span>
                    </span>
                    <div className="flex items-center gap-2">
                      <div className="flex-1 h-2.5 bg-zinc-100 border border-black/15 overflow-hidden" title={`${g.views} просм. · ${g.titles} позиций`}>
                        <div className="h-full transition-all"
                          style={{ width: `${g.views > 0 ? Math.max(4, (g.views / genreView.maxViews) * 100) : 0}%`, background: g.views > 0 ? '#27272a' : '#e4e4e7' }} />
                      </div>
                      <span className="w-16 flex-none text-right text-[11px] text-zinc-500 tabular-nums">{sFmt(g.views)}</span>
                    </div>
                  </div>
                ))}
              </div>
            )}
        </Panel>
      </div>

      {/* Полная таблица содержимого (весь каталог, а не только топ-5) */}
      <Panel title="Весь каталог по параметрам" hint={`${nFmt(sorted.length)} позиций · длинные списки прокручиваются/сортируются`}>
        <div className="flex flex-wrap items-center gap-2 mb-3">
          <input value={search} onChange={(e) => setSearch(e.target.value)}
            placeholder="Поиск по названию или жанру…"
            className="border-2 border-black px-3 py-1.5 text-sm min-w-[220px] flex-1 sm:flex-none" />
          <div className="inline-flex border-2 border-black flex-wrap">
            {[['popular', 'Популярные'], ['topRating', 'Топ-оценка'], ['ratingCount', 'По числу оценок'], ['watchToday', 'Хит недели'], ['title', 'По алфавиту']].map(([k, l]) => (
              <button key={k} onClick={() => { setSort(k); setPage(0); }}
                className={`px-2 py-1 text-xs font-semibold ${sort === k ? 'bg-black text-white' : 'hover:bg-neutral-100'}`}>
                {l}
              </button>
            ))}
          </div>
        </div>

        {sorted.length === 0
          ? emptyBlock('По выбранным фильтрам ничего не нашлось. Попробуй изменить жанр или включить другой тип.')
          : (
          <>
            <div className="overflow-x-auto border-2 border-black">
              <table className="w-full text-sm min-w-[640px]">
                <thead>
                  <tr className="bg-zinc-900 text-white text-left text-[11px] uppercase tracking-wide">
                    <th className="px-2 py-2">#</th>
                    <th className="px-2 py-2">Название</th>
                    <th className="px-2 py-2">Тип</th>
                    <th className="px-2 py-2 text-right">Оценка</th>
                    <th className="px-2 py-2 text-right">Просм. (всего)</th>
                    <th className="px-2 py-2 text-right">Просм. (период)</th>
                    <th className="px-2 py-2 text-right hidden md:table-cell">Время</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-black/10">
                  {sorted.slice(curPage * PAGE, curPage * PAGE + PAGE).map((c, i) => (
                    <tr key={`${c.contentType}-${c.contentId}`} className="hover:bg-neutral-50">
                      <td className="px-2 py-2 text-zinc-400 tabular-nums">{curPage * PAGE + i + 1}</td>
                      <td className="px-2 py-2 font-semibold"><Link to={hrefOf(c.contentType, c.contentId)} className="hover:underline">{c.title}</Link></td>
                      <td className="px-2 py-2"><Chip t={c.contentType} /></td>
                      <td className="px-2 py-2 text-right">
                        {c.ratingCount
                          ? <><b>★ {Number(c.avgRating).toFixed(1)}</b> <span className="text-zinc-400 text-xs">({nFmt(c.ratingCount)})</span></>
                          : <span className="text-zinc-300">—</span>}
                      </td>
                      <td className="px-2 py-2 text-right tabular-nums">{nFmt(c.watches)}</td>
                      <td className="px-2 py-2 text-right tabular-nums font-semibold">{c.watchesInRange ? nFmt(c.watchesInRange) : <span className="text-zinc-300">0</span>}</td>
                      <td className="px-2 py-2 text-right text-zinc-500 hidden md:table-cell tabular-nums">{durFmt(c.watchSecondsInRange)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <div className="flex items-center justify-between mt-3 gap-2">
              <button onClick={() => setPage((p) => Math.max(0, p - 1))} disabled={curPage === 0}
                className="border-2 border-black px-3 py-1 text-sm font-semibold disabled:opacity-30 hover:bg-black hover:text-white disabled:hover:bg-white disabled:hover:text-black">
                ← Назад
              </button>
              <span className="text-sm text-zinc-400">стр. {curPage + 1} / {pages}</span>
              <button onClick={() => setPage((p) => p + 1)} disabled={(curPage + 1) * PAGE >= sorted.length}
                className="border-2 border-black px-3 py-1 text-sm font-semibold disabled:opacity-30 hover:bg-black hover:text-white disabled:hover:bg-white disabled:hover:text-black">
                Вперёд →
              </button>
            </div>
          </>
          )}
      </Panel>
    </div>
  );
};

function Chip({ t }) {
  const isMovie = t === 'movie';
  return (
    <span className={`inline-block px-1.5 py-0.5 text-[10px] font-bold uppercase ${isMovie ? 'bg-indigo-100 text-indigo-900' : 'bg-amber-100 text-amber-800'}`}>
      {isMovie ? 'Фильм' : 'Сериал'}
    </span>
  );
}
