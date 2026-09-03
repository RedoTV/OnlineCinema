import axios from 'axios';

const API_URL = import.meta.env.PROD ? '/api' : 'http://localhost:5000/api';
// analytics-сервис (FastAPI). В проде идём через nginx префикс /analytics
const ANALYTICS_URL = import.meta.env.PROD ? '/analytics' : 'http://localhost:8000';

export const api = axios.create({
    baseURL: API_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

// отдельный клиент под аналитику, чтобы не путать базы
export const analyticsApi = axios.create({
    baseURL: ANALYTICS_URL,
    headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});