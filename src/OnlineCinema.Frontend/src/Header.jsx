import { useContext } from 'react';
import { Link } from 'react-router-dom';
import { AuthContext } from './context/AuthContext';

const getRole = (user) => user?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || user?.role;

export const Header = () => {
    const { user, logout } = useContext(AuthContext);
    const role = getRole(user);
    const isStaff = role === 'Admin' || role === 'Moderator';

    return (
        <header className="w-full border-b-2 border-black bg-white py-4 px-6 mb-8 sticky top-0 z-50">
            <div className="max-w-5xl mx-auto flex flex-wrap justify-between items-center gap-3">
                <nav className="flex flex-wrap gap-6 font-bold text-lg">
                    <Link to="/" className="hover:underline decoration-2 underline-offset-4">КАТАЛОГ</Link>
                    <Link to="/series" className="hover:underline decoration-2 underline-offset-4">СЕРИАЛЫ</Link>
                    <Link to="/actors" className="hover:underline decoration-2 underline-offset-4">АКТЕРЫ</Link>
                    <Link to="/news" className="hover:underline decoration-2 underline-offset-4">НОВОСТИ</Link>
                    {isStaff && (
                        <Link to="/analytics" className="hover:underline decoration-2 underline-offset-4">АНАЛИТИКА</Link>
                    )}
                    {user && (
                        <Link to="/profile" className="hover:underline decoration-2 underline-offset-4">ЛИЧНЫЙ КАБИНЕТ</Link>
                    )}
                    {role === 'Admin' && (
                        <Link to="/admin" className="hover:underline decoration-2 underline-offset-4">АДМИН</Link>
                    )}
                </nav>
                {user ? (
                    <div className="flex items-center gap-3">
                        <span className="font-bold">{user.username}</span>
                        <button onClick={logout} className="text-sm font-bold border border-black px-3 py-1 hover:bg-black hover:text-white transition-colors">
                            ВЫЙТИ
                        </button>
                    </div>
                ) : (
                    <Link to="/login" className="text-sm font-bold border border-black px-3 py-1 hover:bg-black hover:text-white transition-colors">
                        ВХОД
                    </Link>
                )}
            </div>
        </header>
    );
};
