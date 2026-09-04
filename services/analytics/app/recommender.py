"""Гибридный рекомендатель.

Работает двухконтурно:
1) коллаборативный (SVD) — по матрице оценок из public.Ratings;
2) контентный (жанры) — когда данных юзеров мало (cold start) и для
   секции «потому что ты смотрел X».

Пропорция смешивания 0.7/0.3. Для диплома хватает, онлайн-обучение —
уже перебор.
"""

import logging

import pandas as pd
from sqlalchemy import create_engine

from .settings import settings

log = logging.getLogger(__name__)

# движок SQLAlchemy используем один раз, безопасно держать в памяти
_engine = create_engine(settings.dsn.replace("postgresql://", "postgresql+psycopg://"))


def _load_catalog() -> tuple[pd.DataFrame, pd.DataFrame, pd.DataFrame]:
    """Читает каталог из .NET-схемы (public) только на чтение."""
    mov = pd.read_sql('SELECT m."Id", m."Title", m."ReleaseYear" FROM "Movies" m', _engine)
    # join-таблица EF кольцnamится по нативным именам GenreId/MoviesId -
    # MoviesId здесь и есть id фильма (совпадает с Ratings.MovieId)
    mov_genres = pd.read_sql(
        """
        SELECT mg."MoviesId" AS "MovieId", g."Name"
        FROM "MovieGenres" mg JOIN "Genres" g ON g."Id" = mg."GenresId"
        """,
        _engine,
    )
    ratings = pd.read_sql('SELECT "UserId","MovieId","RatingValue" FROM "Ratings"', _engine)
    return mov, mov_genres, ratings


def _build_onehot(mov: pd.DataFrame, mov_genres: pd.DataFrame) -> pd.DataFrame:
    # onehot по жанрам для content-подобия
    df = mov.merge(mov_genres, left_on="Id", right_on="MovieId", how="left")
    h = pd.crosstab(df["Id"], df["Name"])
    # добавляем Title для красоты вывода
    h = h.reset_index()
    titles = mov.set_index("Id")["Title"]
    h["Title"] = h["Id"].map(titles)
    return h


def content_recs(reference_movie_id: int, k: int = 8):
    """Похожее по жанрам к заданному фильму — «потому что ты смотрел X»."""
    mov, mov_genres, _ = _load_catalog()
    h = _build_onehot(mov, mov_genres)
    if reference_movie_id not in set(h["Id"]):
        return []
    vec = h[h["Id"] == reference_movie_id].iloc[:, 1:-1].astype(float).iloc[0]
    feats = h.iloc[:, 1:-1].astype(float)
    from numpy.linalg import norm

    # cosine по строкам
    denom = feats.apply(lambda r: norm(r.values) or 1.0, axis=1)
    dots = feats.dot(vec.values)
    sim = dots / denom
    res = h.copy()
    res["score"] = sim.values
    out = (
        res[res["Id"] != reference_movie_id]
        .sort_values("score", ascending=False)
        .head(k)
    )
    return out[["Id", "Title", "score"]].to_dict("records")


def _content_for_user(user_id: int, mov, mov_genres, ratings, k: int = 10) -> list[dict]:
    """Ранжируем фильмы по предпочтениям юзера к жанрам (content fallback).

    Берёт жанры из фильмов, которым юзер поставил >=7, и отдаёт недосмотренные
    фильмы тех же жанров. Падает армией, когда оценок мало или они невысокие.
    """
    if user_id not in set(ratings["UserId"].unique()):
        return []
    mine = ratings[(ratings["UserId"] == user_id) & (ratings["RatingValue"] >= 7)]
    if mine.empty:
        return []

    watched = set(ratings[ratings["UserId"] == user_id]["MovieId"])
    loved_movie_ids = set(mine["MovieId"])
    # любимые жанры агрегируем
    gi = _build_onehot(mov, mov_genres).set_index("Id")
    fav = gi.loc[[mid for mid in loved_movie_ids if mid in gi.index]]
    if fav.empty:
        return []  # любимые фильмы без жанров (маловероятно)
    genre_weights = fav.iloc[:, :-1].sum(axis=0)  # без Title-колонки
    genre_weights = genre_weights[genre_weights > 0]

    # скорим все фильмы кроме просмотренных и любимых
    cands = gi.loc[[mid for mid in gi.index if mid not in watched]]
    if cands.empty:
        return []
    feats = cands.iloc[:, :-1].fillna(0).astype(float)
    # профиль-вес по жанрам домножаем на признаки
    score = feats.dot(genre_weights.reindex(feats.columns).fillna(0))
    ranked = score.sort_values(ascending=False).head(k)
    titles = mov.set_index("Id")["Title"]
    return [
        {
            "movieId": int(mid),
            "title": titles.get(mid, ""),
            "score": round(float(val), 3),
            "reason": "по твоим любимым жанрам",
        }
        for mid, val in ranked.items()
    ]


def _predict_from_matrix(user_id, ratings: pd.DataFrame, n_factors=8):
    """Чистая SVD-функция (без БД) — покрывается юнит-тестом.

    Возвращает отсортированный список {movieId, score} для незасмотренных.
    """
    if ratings.empty or user_id not in set(ratings["UserId"].unique()):
        return []
    pivot = ratings.pivot_table(index="UserId", columns="MovieId", values="RatingValue")
    # центрируем и заполняем средним по юзеру, чтобы SVD не падал
    filled = pivot.apply(lambda row: row.fillna(row.mean()), axis=1)
    filled = filled.fillna(filled.mean().mean())
    filled = filled.sub(filled.mean(axis=1), axis=0)

    from sklearn.decomposition import TruncatedSVD

    n_factors = max(2, min(n_factors, filled.shape[1] - 1, filled.shape[0] - 1))
    svd = TruncatedSVD(n_components=n_factors, random_state=42)
    u = svd.fit_transform(filled)
    pred = (u[filled.index.get_loc(user_id)] * svd.singular_values_) @ svd.components_

    cols = list(filled.columns)
    if user_id in pivot.index:
        user_watched = set(pivot.loc[user_id].dropna().index)
    else:
        user_watched = set()

    out = [
        {"movieId": cols[i], "score": float(pred[i])}
        for i in range(len(cols))
        if cols[i] not in user_watched and not pd.isna(pred[i])
    ]
    out.sort(key=lambda x: x["score"], reverse=True)
    return out


def hybrid_for_user(user_id: int, k: int = 10) -> list[dict]:
    """Главный вход API. Колаб + контент, никогда не возвращает пусто.

    Приоритет отдаём коллаборативным (SVD), но если в выборке слишком мало
    пользователей (у нас свежая БД — такое реально), сползаем на контентный
    fallback по любимым жанрам юзера, иначе на просто популярные фильмы.
    """
    mov, mov_genres, ratings = _load_catalog()

    # юзер ничего не оценивал — холодный старт: топ по средним
    has_user_history = not ratings.empty and user_id in set(ratings["UserId"].unique())
    if not has_user_history:
        log.info("Холодный старт юзера %s, отдаю по средним оценкам", user_id)
        if ratings.empty:
            # Даже на абсолютно новой базе ряд в UI не должен исчезать.
            return [
                {"movieId": int(row.Id), "title": row.Title, "score": 0.0,
                 "reason": "новинка каталога"}
                for row in mov.sort_values(["ReleaseYear", "Id"], ascending=False).head(k).itertuples()
            ]
        avg = ratings.groupby("MovieId")["RatingValue"].mean()
        rated = [int(i) for i in avg.sort_values(ascending=False).index if i in set(mov["Id"])]
        # Rated titles can be fewer than k on first startup; fill with recent catalog items.
        candidates = rated + [int(i) for i in mov.sort_values(["ReleaseYear", "Id"], ascending=False)["Id"] if int(i) not in rated]
        titles = mov.set_index("Id")["Title"]
        return [
            {"movieId": i, "title": titles.get(i, ""), "score": float(avg.get(i, 0)),
             "reason": "популярно у других" if i in avg.index else "новинка каталога"}
            for i in candidates[:k]
        ]

    collab = _predict_from_matrix(user_id, ratings)

    if len(collab) >= k:
        # коллаборатив дал норм кандидатов — используем как есть
        titles = mov.set_index("Id")["Title"]
        return [
            {"movieId": it["movieId"], "title": titles.get(it["movieId"], ""),
             "score": round(it["score"], 3), "reason": "похоже на то, что ты смотрел"}
            for it in collab[:k]
            if it["movieId"] in titles.index
        ]

    # мало пользователей — добор контентом, потом только популярным
    content = _content_for_user(user_id, mov, mov_genres, ratings, k=k)
    if content:
        return content

    avg = ratings.groupby("MovieId")["RatingValue"].mean()
    top = avg.sort_values(ascending=False).head(k).index
    titles = mov.set_index("Id")["Title"]
    watched = set(ratings[ratings["UserId"] == user_id]["MovieId"])
    result = [
        {"movieId": int(i), "title": titles.get(i, ""), "score": float(avg[i]),
         "reason": "популярно у других"}
        for i in top
        if i in titles.index and i not in watched
    ]
    used = watched | {x["movieId"] for x in result}
    for row in mov.sort_values(["ReleaseYear", "Id"], ascending=False).itertuples():
        if len(result) >= k:
            break
        if int(row.Id) not in used:
            result.append({"movieId": int(row.Id), "title": row.Title, "score": 0.0,
                           "reason": "новинка каталога"})
    return result
