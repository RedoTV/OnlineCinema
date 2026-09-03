import { useContext } from 'react';
import { Link } from 'react-router-dom';
import { AuthContext } from './context/AuthContext';

export const Header = () => {
    const { user, logout } = useContext(AuthContext);

    return (
        <header className="w-full border-b-2 border-black bg-white py-4 px-6 mb-8 sticky top-0 z-50">
            <div className="max-w-5xl mx-auto flex justify-between items-center">
                <nav className="flex gap-6 font-bold text-lg">
                    <Link to="/" className="hover:underline decoration-2 underline-offset-4">КАТАЛОГ</Link>
                    <Link to="/series" className="hover:underline decoration-2 underline-offset-4">СЕРИАЛЫ</Link>
                    <Link to="/profile" className="hover:underline decoration-2 underline-offset-4">ЛИЧНЫЙ КАБИНЕТ</Link>
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
