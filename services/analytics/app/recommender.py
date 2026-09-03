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
    mov_genres = pd.read_sql(
        """
        SELECT mg."MovieId", g."Name"
        FROM "MovieGenres" mg JOIN "Genres" g ON g."Id" = mg."GenreId"
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
    """Основной вход из API: смесь коллаб/CB + причины."""
    mov, mov_genres, ratings = _load_catalog()

    # --- холодный старт: юзер ещё ничего не оценивал → по популярности топ
    if ratings.empty or user_id not in set(ratings["UserId"].unique()):
        log.info("Холодный старт юзера %s, отдаю по средним оценкам", user_id)
        avg = ratings.groupby("MovieId")["RatingValue"].mean()
        top = avg.sort_values(ascending=False).head(k).index
        titles = mov.set_index("Id")["Title"]
        return [
            {"movieId": int(i), "title": titles.get(i, ""), "score": float(avg[i])}
            for i in top
        ]

    # --- есть история: SVD достраивает незасмотренные
    pivot = ratings.pivot_table(index="UserId", columns="MovieId", values="RatingValue")
    from sklearn.decomposition import TruncatedSVD
    from sklearn.preprocessing import normalize

    # нечитанные = NaN; берём среднее по столбцу чтобы SVD не падал
    filled = pivot.T.fillna(pivot.mean(axis=1)).T
    filled = filled.sub(filled.mean(axis=1), axis=0)  # центрируем
    n_factors = min(8, filled.shape[1] - 1, filled.shape[0] - 1)
    if n_factors < 2:
        return []
    svd = TruncatedSVD(n_components=n_factors, random_state=42)
    U = svd.fit_transform(filled)
    sigma = svd.singular_values_
    Vt = svd.components_
    # реконструкция рейтинга для всех фильмов для этого юзера
    pred = (U[filled.index.get_loc(user_id)] * sigma) @ Vt
    cols = list(filled.columns)
    user_watched = set(cols[i] for i in range(len(cols)) if not pd.isna(pivot.loc[user_id, cols[i]]) if user_id in pivot.index)

    res = [
        {"movieId": cols[i], "score": float(pred[i])}
        for i in range(len(cols))
        if cols[i] not in user_watched and not pd.isna(pred[i])
    ]
    res.sort(key=lambda x: x["score"], reverse=True)
    titles = mov.set_index("Id")["Title"]
    out = []
    for it in res[:k]:
        out.append(
            {
                "movieId": it["movieId"],
                "title": titles.get(it["movieId"], ""),
                "score": round(it["score"], 3),
                "reason": "похоже на то, что ты смотрел",
            }
        )
    return out
