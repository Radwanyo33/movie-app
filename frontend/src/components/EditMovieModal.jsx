import React, { useState, useEffect } from 'react';
import './MovieForm.css'; // Reuse the same styles

const EditMovieModal = ({ isOpen, onClose, movie, onMovieUpdated }) => {
    const [formData, setFormData] = useState({
        Name: '',
        Release_Year: '',
        Language: '',
        Genre: [],
        Rating: '',
        Description: '',
        Cast: [],
        Image_url: '',
        Watch_url: ''
    });

    const [currentGenre, setCurrentGenre] = useState('');
    const [currentCast, setCurrentCast] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    useEffect(() => {
        if (movie) {
            setFormData({
                Name: movie.name || movie.Name || '',
                Release_Year: movie.release_Year || movie.Release_Year || '',
                Language: movie.language || movie.Language || '',
                Genre: Array.isArray(movie.genre) ? movie.genre : 
                       Array.isArray(movie.Genre) ? movie.Genre : [],
                Rating: movie.rating || movie.Rating || '',
                Description: movie.description || movie.Description || '',
                Cast: Array.isArray(movie.cast) ? movie.cast : 
                      Array.isArray(movie.Cast) ? movie.Cast : [],
                Image_url: movie.image_url || movie.Image_url || '',
                Watch_url: movie.watch_url || movie.Watch_url || ''
            });
        }
    }, [movie]);

    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: value
        }));
    };

    const addGenre = () => {
        if (currentGenre && !formData.Genre.includes(currentGenre)) {
            setFormData(prev => ({
                ...prev,
                Genre: [...prev.Genre, currentGenre]
            }));
            setCurrentGenre('');
        }
    };

    const removeGenre = (genreToRemove) => {
        setFormData(prev => ({
            ...prev,
            Genre: prev.Genre.filter(genre => genre !== genreToRemove)
        }));
    };

    const addCast = () => {
        if (currentCast && !formData.Cast.includes(currentCast)) {
            setFormData(prev => ({
                ...prev,
                Cast: [...prev.Cast, currentCast]
            }));
            setCurrentCast('');
        }
    };

    const removeCast = (castToRemove) => {
        setFormData(prev => ({
            ...prev,
            Cast: prev.Cast.filter(cast => cast !== castToRemove)
        }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);
        setError('');

        try {
            const response = await fetch(`http://localhost:5000/api/movies/${movie.id}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(formData),
                credentials: 'include'
            });

            const result = await response.json();

            if (response.ok) {
                await onMovieUpdated(result);
                onClose();
            } else {
                setError(result.message || 'Failed to update movie');
            }
        } catch (err) {
            setError('Network error. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    if (!isOpen) return null;

    return (
        <div className="modal-overlay">
            <div className="modal-content large-modal">
                <button className="close-button" onClick={onClose}>×</button>
                <h2>Edit Movie: {movie?.name || movie?.Name}</h2>
                
                {error && <div className="error-message">{error}</div>}
                
                <form onSubmit={handleSubmit} className="movie-form">
                    <div className="form-row">
                        <div className="form-group">
                            <label>Movie Name *</label>
                            <input
                                type="text"
                                name="Name"
                                value={formData.Name}
                                onChange={handleInputChange}
                                required
                                disabled={loading}
                            />
                        </div>
                        <div className="form-group">
                            <label>Release Year *</label>
                            <input
                                type="text"
                                name="Release_Year"
                                value={formData.Release_Year}
                                onChange={handleInputChange}
                                required
                                disabled={loading}
                            />
                        </div>
                    </div>

                    <div className="form-row">
                        <div className="form-group">
                            <label>Language *</label>
                            <input
                                type="text"
                                name="Language"
                                value={formData.Language}
                                onChange={handleInputChange}
                                required
                                disabled={loading}
                            />
                        </div>
                        <div className="form-group">
                            <label>Rating</label>
                            <input
                                type="text"
                                name="Rating"
                                value={formData.Rating}
                                onChange={handleInputChange}
                                placeholder="e.g., 8.5/10"
                                disabled={loading}
                            />
                        </div>
                    </div>

                    <div className="form-group">
                        <label>Current Image URL</label>
                        <input
                            type="text"
                            name="Image_url"
                            value={formData.Image_url}
                            onChange={handleInputChange}
                            placeholder="Image URL"
                            disabled={loading}
                        />
                    </div>

                    <div className="form-group">
                        <label>Genres</label>
                        <div className="array-input">
                            <input
                                type="text"
                                value={currentGenre}
                                onChange={(e) => setCurrentGenre(e.target.value)}
                                placeholder="Add genre"
                                onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addGenre())}
                                disabled={loading}
                            />
                            <button type="button" onClick={addGenre} disabled={loading}>Add</button>
                        </div>
                        <div className="array-items">
                            {formData.Genre.map((genre, index) => (
                                <span key={index} className="array-item">
                                    {genre}
                                    <button type="button" onClick={() => removeGenre(genre)} disabled={loading}>×</button>
                                </span>
                            ))}
                        </div>
                    </div>

                    <div className="form-group">
                        <label>Cast</label>
                        <div className="array-input">
                            <input
                                type="text"
                                value={currentCast}
                                onChange={(e) => setCurrentCast(e.target.value)}
                                placeholder="Add cast member"
                                onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addCast())}
                                disabled={loading}
                            />
                            <button type="button" onClick={addCast} disabled={loading}>Add</button>
                        </div>
                        <div className="array-items">
                            {formData.Cast.map((cast, index) => (
                                <span key={index} className="array-item">
                                    {cast}
                                    <button type="button" onClick={() => removeCast(cast)} disabled={loading}>×</button>
                                </span>
                            ))}
                        </div>
                    </div>

                    <div className="form-group">
                        <label>Watch URL</label>
                        <input
                            type="url"
                            name="Watch_url"
                            value={formData.Watch_url}
                            onChange={handleInputChange}
                            placeholder="https://example.com/watch"
                            disabled={loading}
                        />
                    </div>

                    <div className="form-group">
                        <label>Description</label>
                        <textarea
                            name="Description"
                            value={formData.Description}
                            onChange={handleInputChange}
                            rows="4"
                            placeholder="Movie description..."
                            disabled={loading}
                        />
                    </div>

                    <button type="submit" className="submit-button" disabled={loading}>
                        {loading ? 'Updating Movie...' : 'Update Movie'}
                    </button>
                </form>
            </div>
        </div>
    );
};

export default EditMovieModal;