import { getImageUrl } from '../services/movieApi'; 
import { useState } from 'react';
import { movieApi } from '../services/movieApi';
import './MoviesCards.css';

export const MoviesCards = ({ data, isAdmin, onEdit, onDelete }) => {
    const {
        id,
        name: Name,
        release_Year: Release_Year,
        language: Language,
        genre: Genre,
        rating: Rating,
        description: Description,
        cast: Cast,
        image_url: Image_url,
        watch_url: Watch_url
    } = data;

    const [isDeleting, setIsDeleting] = useState(false);

    const handleDelete = async () => {
        if (!window.confirm(`Are you sure you want to delete "${Name}"?`)) {
            return;
        }

        setIsDeleting(true);
        console.log('Deleting movie ID:', id);
        
        try {
            // Use movieApi service instead of hardcoded URL
            const result = await movieApi.deleteMovie(id);

            console.log('Delete response:', result);
            
            // Check for success in both response formats
            if (result.success || result.message === 'Movie deleted successfully') {
                console.log('Delete successful, calling onDelete callback');
                // Notify parent to update the state
                onDelete(id);
            } else {
                console.error('Delete failed:', result);
                alert(result.message || 'Failed to delete movie');
            }
        } catch (error) {
            console.error('Delete error:', error);
            alert(error.message || 'Network error. Please try again.');
        } finally {
            setIsDeleting(false);
        }
    };

    return (
        <li className="movies-item" key={id}>
            {/* Admin Controls */}
            {isAdmin && (
                <div className="admin-controls">
                    <button 
                        className="edit-button" 
                        onClick={() => onEdit(data)}
                        disabled={isDeleting}
                    >
                        ✏️ Edit
                    </button>
                    <button 
                        className="delete-button" 
                        onClick={handleDelete}
                        disabled={isDeleting}
                    >
                        {isDeleting ? 'Deleting...' : '🗑️ Delete'}
                    </button>
                </div>
            )}

            <div className="image-container">
                <img src={getImageUrl(Image_url) || '/images/placeholder.jpg'} alt={Name} className="image" />
            </div>

            <div className="card-content">
                <h2>Name: {Name}</h2>
                <p><strong>Release: </strong>{Release_Year}</p>
                <p><strong>Language:</strong> {Language}</p>
                <p><strong>Genre:</strong> {Array.isArray(Genre) ? Genre.join(", ") : Genre || 'N/A'} </p>
                <p><strong>Ratings: </strong>{Rating}</p>
                <p><strong>Description:</strong> {Description}</p>
                <p><strong>Cast: </strong>{Array.isArray(Cast) ? Cast.join(", ") : Cast || 'N/A'}</p>
            </div>
            <div className="button-container">
                <a href={Watch_url} target="_blank" rel="noopener noreferrer">
                    <button className="button">
                    Watch Now
                    </button>
                </a> <br />
            </div>
        </li>
    );
}