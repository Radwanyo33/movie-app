import { getImageUrl } from '../services/movieApi'; 
import { useState } from 'react';

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
            // Use direct URL instead of movieApi service
            const response = await fetch(`http://localhost:5000/api/movies/${id}`, {
                method: 'DELETE',
                credentials: 'include'
            });

            console.log('Delete response status:', response.status);
            
            if (response.ok) {
                console.log('Delete successful, calling onDelete callback');
                // Notify parent to update the state
                onDelete(id);
            } else {
                const errorData = await response.json();
                console.error('Delete failed:', errorData);
                alert(errorData.message || 'Failed to delete movie');
            }
        } catch (error) {
            console.error('Delete error:', error);
            alert('Network error. Please try again.');
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