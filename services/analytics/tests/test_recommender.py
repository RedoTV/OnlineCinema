"""Юнит-тесты математики рекомендаций (без обращения к БД)."""

import pandas as pd

from app.recommender import _predict_from_matrix


def _synth_ratings():
    # 4 юзера × оценки на части фильмов
    data = {
        "UserId": [1, 1, 1, 2, 2, 2, 3, 3, 4, 4],
        "MovieId": [10, 11, 12, 10, 11, 13, 12, 13, 10, 14],
        "RatingValue": [9, 8, 7, 9, 9, 6, 7, 8, 8, 5],
    }
    return pd.DataFrame(data)


def test_cold_start_no_ratings():
    # юзер 99 ничего не смотрел — _predict вернёт пусто, уйдёт на content-cold start
    assert _predict_from_matrix(99, _synth_ratings()) == []


def test_svd_returns_movie_ids_without_seen():
    ratings = _synth_ratings()
    preds = _predict_from_matrix(1, ratings)
    assert len(preds) > 0
    # юзер 1 точно не должен увидеть уже оценённые 10/11/12
    seen = {10, 11, 12}
    assert all(p["movieId"] not in seen for p in preds)
    # отсортированы по убыванию
    scores = [p["score"] for p in preds]
    assert scores == sorted(scores, reverse=True)


def test_ratings_empty_matrix_branch():
    assert _predict_from_matrix(1, pd.DataFrame(columns=["UserId", "MovieId", "RatingValue"])) == []
    assert _predict_from_matrix(1, pd.DataFrame({"UserId": [], "MovieId": [], "RatingValue": []})) == []
