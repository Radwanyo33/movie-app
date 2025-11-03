import React from 'react';
import './Header.css';

const Header = ({ onAddMovie, isAdmin, onLoginClick, onLogout, searchTerm, onSearchChange, onSearchSubmit }) => {
return (
<header className="app-header">
<nav className="navbar">
<div className="nav-brand">
<h1 className="app-title">🎬 LMDB</h1>
</div>


            <div className="nav-search">
                <div className="search-container">
                    <input 
                        type="text" 
                        placeholder="Search by name, year, genre, rating, language....." 
                        value={searchTerm}
                        onChange={(e) => onSearchChange(e.target.value)}
                        onKeyPress={(e) => e.key === 'Enter' && onSearchSubmit()}
                        className="search-input" 
                    />
                    <button className="search-button" onClick={onSearchSubmit}>
                        Search
                    </button>
                </div>
            </div>
            
            <div className="nav-actions">
                {isAdmin ? (
                    <>
                        <button className="add-movie-button" onClick={onAddMovie}>
                            + Add New Movie
                        </button>
                        <button className="logout-button" onClick={onLogout}>
                            🔓 Logout
                        </button>
                    </>
                ) : (
                    <button className="login-header-button" onClick={onLoginClick}>
                        🔐 Admin Login
                    </button>
                )}
            </div>
        </nav>
    </header>
);
};

export default Header;