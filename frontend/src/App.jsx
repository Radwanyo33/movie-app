import React, { useState, useEffect } from "react";
import "./index.css";
import ImdbMovies from "./components/ImdbMovies";
import Header from "./components/Header";
import LoginModal from "./components/LoginModal";
import MovieForm from "./components/MovieForm";
import EditMovieModal from "./components/EditMovieModal"; // Add this import
import { movieApi, authApi } from "./services/movieApi";

export const App = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const [isLoginModalOpen, setIsLoginModalOpen] = useState(false);
  const [isMovieFormOpen, setIsMovieFormOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false); // Add edit modal state
  const [editingMovie, setEditingMovie] = useState(null); // Add editing movie state
  const [isAdmin, setIsAdmin] = useState(false);
  const [movies, setMovies] = useState([]);
  const [filteredMovies, setFilteredMovies] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  // Check authentication status on app load
  useEffect(() => {
    checkAuthStatus();
    fetchMovies();
  }, []);

  // Check if user is already logged in
  const checkAuthStatus = async () => {
    try {
      const result = await authApi.checkAuth();
      if (result.isAdmin) {
        setIsAdmin(true);
      }
    } catch (error) {
      console.error('Error checking auth status:', error);
      setIsAdmin(false);
    }
  };

  const fetchMovies = async (searchQuery = '') => {
    setLoading(true);
    setError('');
    try {
      let data;
      if (searchQuery) {
        data = await movieApi.searchMovies(searchQuery);
      } else {
        data = await movieApi.getAllMovies();
      }
      
      console.log('Fetched movies:', data);
      
      // Force new array references to ensure re-render
      setMovies([...data]);
      setFilteredMovies([...data]);
    } catch (error) {
      console.error('Error fetching movies:', error);
      setError('Failed to load movies. Please try again.');
    
      // Fallback to local data if API is not available
      try {
        const localData = await import('./api/seriesData.json');
        setMovies([...localData.default]);
        setFilteredMovies([...localData.default]);
      } catch (fallbackError) {
        console.error('Fallback also failed:', fallbackError);
      }
    } finally {
      setLoading(false);
    }
  };

  const handleMovieAdded = async (movieData) => {
    console.log('Adding movie:', movieData);
    try {
      const result = await movieApi.addMovie(movieData);
      console.log('Add movie response:', result);

      if (result) {
        // Clear search term to show all movies including the new one
        setSearchTerm('');
        await fetchMovies(''); // Fetch all movies
        return { success: true };
      }
      return { success: false, message: 'Failed to add movie' };
    } catch (error) {
      console.error('Error adding movie:', error);
      return { success: false, message: 'Failed to add movie. Please try again.' };
    }
  };

  // Handle movie edit
  const handleMovieEdit = (movie) => {
    setEditingMovie(movie);
    setIsEditModalOpen(true);
  };

  // Handle movie update
  const handleMovieUpdated = async (updatedMovie) => {
    try {
      await fetchMovies(searchTerm); // Refresh the list
      return { success: true };
    } catch (error) {
      console.error('Error updating movie:', error);
      return { success: false, message: 'Failed to update movie' };
    }
  };

  // Handle movie delete
  // Handle movie delete
  const handleMovieDelete = async (movieId) => {
    console.log('Parent component received delete for ID:', movieId);
    
    // Update both movies and filteredMovies states
    setMovies(prevMovies => {
        const updatedMovies = prevMovies.filter(movie => movie.id !== movieId);
        console.log('Movies after deletion:', updatedMovies.length, 'remaining');
        return updatedMovies;
    });
    
    setFilteredMovies(prevFiltered => {
        const updatedFiltered = prevFiltered.filter(movie => movie.id !== movieId);
        console.log('Filtered movies after deletion:', updatedFiltered.length, 'remaining');
        return updatedFiltered;
    });
  };

  const handleSearchChange = (term) => {
    setSearchTerm(term);
  };

  const handleSearchSubmit = () => {
    fetchMovies(searchTerm);
  };

  const handleLogin = async (email, password) => {
    try {
      const result = await authApi.login(email, password);
      if (result.success) {
        setIsAdmin(true);
        setIsLoginModalOpen(false);
        return { success: true };
      } else {
        return { success: false, message: result.message };
      }
    } catch (error) {
      return { success: false, message: 'Login failed. Please try again.' };
    }
  };

  const handleLogout = async () => {
    try {
      await authApi.logout();
      setIsAdmin(false);
    } catch (error) {
      console.error('Logout error:', error);
      setIsAdmin(false); // Still logout locally even if API call fails
    }
  };

  return (
    <div className="container">
      <Header
        onAddMovie={() => setIsMovieFormOpen(true)}
        isAdmin={isAdmin}
        onLoginClick={() => setIsLoginModalOpen(true)}
        onLogout={handleLogout}
        searchTerm={searchTerm}
        onSearchChange={handleSearchChange}
        onSearchSubmit={handleSearchSubmit}
      />

      {loading && (
        <div className="loading-message">Loading movies...</div>
      )}
      
      {error && (
        <div className="error-message">{error}</div>
      )}
      
      <ImdbMovies 
        movies={filteredMovies} 
        isAdmin={isAdmin}
        onMovieEdit={handleMovieEdit}
        onMovieDelete={handleMovieDelete}
      />
      
      <LoginModal
        isOpen={isLoginModalOpen}
        onClose={() => setIsLoginModalOpen(false)}
        onLogin={handleLogin}
      />
      
      <MovieForm
        isOpen={isMovieFormOpen}
        onClose={() => setIsMovieFormOpen(false)}
        onMovieAdded={handleMovieAdded}
      />

      {/* Add Edit Movie Modal */}
      <EditMovieModal
        isOpen={isEditModalOpen}
        onClose={() => {
          setIsEditModalOpen(false);
          setEditingMovie(null);
        }}
        movie={editingMovie}
        onMovieUpdated={handleMovieUpdated}
      />
    </div>
  );
}