"""Контракт событий .NET -> analytics: новый и старый форматы обязаны сходиться."""

from app.events import normalize_event


def test_movie_rated_new_format_content_id():
    # Текущий бэкенд: UserActionService шлёт {contentType, contentId}
    norm = normalize_event("movie.rated", {
        "userId": 7, "contentType": "movie", "contentId": 1, "grade": 8,
    })
    assert norm["user_id"] == 7
    assert norm["stats"] == {"content_type": "movie", "content_id": 1,
                             "views": 0, "watch_seconds": 0, "grade": 8}
    assert norm["event"]["kind"] == "rated"
    assert norm["event"]["ref_type"] == "movie"
    assert norm["event"]["ref_id"] == 1


def test_series_rated_new_format_content_id():
    norm = normalize_event("series.rated", {
        "userId": 7, "contentType": "series", "contentId": 3, "grade": 9,
    })
    assert norm["stats"]["content_type"] == "series"
    assert norm["stats"]["content_id"] == 3
    assert norm["event"]["ref_id"] == 3


def test_movie_rated_legacy_format_movie_id():
    # Старые сообщения в очереди: {movieId}
    norm = normalize_event("movie.rated", {"userId": 7, "movieId": 1, "grade": 8})
    assert norm["stats"]["content_id"] == 1
    assert norm["event"]["ref_id"] == 1


def test_rated_without_id_has_no_aggregates():
    # Позорный кейс из прода: series/0 в content_stats — такого больше нет
    norm = normalize_event("series.rated", {
        "userId": 7, "contentType": "series", "contentId": None, "grade": 8,
    })
    assert norm["stats"] is None
    assert norm["event"] is None


def test_episode_watched_defers_series_lookup():
    norm = normalize_event("episode.watched", {
        "userId": 7, "episodeId": 42, "watchSeconds": 120,
    })
    assert norm["episode_id"] == 42
    assert norm["stats"] is None  # content_id доищется в БД по Seasons
    assert norm["event"]["ref_type"] == "series"


def test_movie_watched_legacy():
    norm = normalize_event("movie.watched", {
        "userId": 7, "movieId": 1, "watchSeconds": 5000,
    })
    assert norm["stats"] == {"content_type": "movie", "content_id": 1,
                             "views": 1, "watch_seconds": 5000, "grade": None}
    assert norm["event"]["kind"] == "watched"


def test_status_changed_watched_and_favorite():
    norm = normalize_event("user.status_changed", {
        "userId": 7, "movieId": 1, "state": "Watched",
    })
    assert norm["stats"]["views"] == 1
    assert norm["event"]["kind"] == "watched"

    fav = normalize_event("user.status_changed", {
        "userId": 7, "movieId": 1, "state": "Favorite",
    })
    assert fav["stats"] is None
    assert fav["event"]["kind"] == "favorite"


def test_status_changed_unknown_state_ignored():
    norm = normalize_event("user.status_changed", {
        "userId": 7, "movieId": 1, "state": "SomethingElse",
    })
    assert norm["stats"] is None
    assert norm["event"] is None


def test_comment_and_register():
    c = normalize_event("comment.created", {"userId": 7, "movieId": 1})
    assert c["event"] == {"kind": "comment", "ref_type": "movie", "ref_id": 1, "note": ""}
    r = normalize_event("user.registered", {"userId": 7, "username": "x"})
    assert r["event"]["kind"] == "registered"


def test_unknown_routing_key_keeps_user():
    norm = normalize_event("something.else", {"userId": 7})
    assert norm["user_id"] == 7
    assert norm["stats"] is None
