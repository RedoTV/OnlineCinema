#!/usr/bin/env node
// Smok-e-тесты API. Запуск: node scripts/smoke-test.mjs [baseUrl]
// baseUrl по умолчанию http://localhost:5000/api
//
// Проходит весь критичный путь: рега -> логин -> каталог ->
// сериалы(структура) -> оценки -> статусы -> комменты -> лайки -> статистика.
// Честно говоря чаще всего запускаю из корня после docker-compose up.

const baseUrl = process.argv[2] || 'http://localhost:5000/api';

let passed = 0;
let failed = 0;
const errors = [];

function check(name, cond, extra) {
  if (cond) {
    passed++;
    console.log(`  \x1b[32m✓\x1b[0m ${name}`);
  } else {
    failed++;
    const msg = extra ? ` (${extra})` : '';
    console.log(`  \x1b[31m✗\x1b[0m ${name}${msg}`);
    errors.push(name + msg);
  }
}

async function req(method, path, body, token) {
  const headers = { 'Content-Type': 'application/json' };
  if (token) headers['Authorization'] = `Bearer ${token}`;
  const res = await fetch(baseUrl + path, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined,
  });
  const data = await res.json().catch(() => null);
  return { status: res.status, data };
}

async function main() {
  console.log(`Смоук против ${baseUrl}\n`);

  // уникальный пользователь, чтобы не упасть на повторе
  const suffix = Date.now().toString().slice(-6);
  const uname = `tester_${suffix}`;
  const email = `${uname}@test.local`;

  console.log('1. Авторизация');
  const reg = await req('POST', '/Auth/register', {
    username: uname, email, password: 'Passw0rd!', firstName: 'Тест', lastName: 'Юзер'
  });
  check('register returns token', reg.status === 200 && reg.data?.token, `status=${reg.status}`);

  const login = await req('POST', '/Auth/login', { email, password: 'Passw0rd!' });
  const token = login.data?.token;
  check('login works', !!token, 'no token');

  console.log('2. Каталог');
  const movies = await req('GET', '/Movies');
  check('movies list', movies.status === 200 && Array.isArray(movies.data));
  const firstMovie = movies.data?.[0];

  const series = await req('GET', '/Series');
  check('series list', series.status === 200 && Array.isArray(series.data));
  const firstSeries = series.data?.find(s => s.seasonsCount > 0);

  console.log('3. Детали и структура');
  if (firstMovie) {
    const detail = await req('GET', `/Movies/${firstMovie.id}`);
    check('movie detail', detail.status === 200 && detail.data?.title);
  }
  if (firstSeries) {
    const sd = await req('GET', `/Series/${firstSeries.id}`);
    const eps = sd.data?.seasons?.[0]?.episodes;
    check('series has season/episodes', Array.isArray(eps) && eps.length > 0, 'no episodes');
    global.__firstEpisode = eps?.[0];
  }

  console.log('4. Действия юзера (нужен кинчик)');
  if (firstMovie && token) {
    const rate = await req('POST', '/UserActions/rating', { movieId: firstMovie.id, rating: 8 }, token);
    check('rate movie', rate.status === 200);

    const stat = await req('POST', '/UserActions/status', { movieId: firstMovie.id, status: 'Watched' }, token);
    check('set status', stat.status === 200);

    const my = await req('GET', '/UserActions/my-movies', null, token);
    check('my movies non-empty', my.status === 200 && my.data?.length > 0);
  } else {
    console.log('  (пропускаю, нет фильмов в каталоге)');
  }

  console.log('5. Комментарии');
  if (firstMovie && token) {
    const add = await req('POST', '/Comments', { movieId: firstMovie.id, text: 'Тестовый комментарий от смок-теста' }, token);
    check('add comment', add.status === 200, `status=${add.status}`);
    const cid = add.data?.id;

    if (cid) {
      // like
      const like = await req('POST', `/Comments/${cid}/like`, null, token);
      check('like comment', like.status === 200);

      // список
      const list = await req('GET', `/Comments?movieId=${firstMovie.id}&sort=newest`, null, token);
      check('comments listed', list.status === 200 && list.data?.length > 0);

      // ответ
      const reply = await req('POST', '/Comments', { movieId: firstMovie.id, parentId: cid, text: 'ответ' }, token);
      check('reply works', reply.status === 200);
    }
  }

  console.log('6. Статистика');
  const top = await req('GET', '/Stats/top-rated?count=5');
  check('stats top-rated', top.status === 200 && Array.isArray(top.data));
  const genres = await req('GET', '/Stats/genres');
  check('stats genres', genres.status === 200 && genres.data?.length > 0);

  console.log('7. Стриминг (если есть видео в хранилище)');
  if (firstMovie) {
    const stream = await req('GET', `/Streaming/movie/${firstMovie.id}`);
    check('stream endpoint responds', stream.status === 200 || stream.status === 404);
  }

  console.log('\n------------------------------------');
  console.log(`Итог: ${passed} ок, ${failed} не ок`);
  if (errors.length) {
    console.log('Падения:');
    errors.forEach(e => console.log('  - ' + e));
    process.exitCode = 1;
  }
}

main().catch(e => {
  console.error('Скрипт упал:', e.message);
  process.exitCode = 1;
});
