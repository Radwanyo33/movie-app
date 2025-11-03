import React, { useState } from 'react';
import './MovieForm.css';

const MovieForm = ({ isOpen, onClose, onMovieAdded }) => {
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
const [imagePreview, setImagePreview] = useState('');
const [uploadingImage, setUploadingImage] = useState(false);

// Image upload function
const handleImageUpload = async (file) => {
setUploadingImage(true);
try {
const formData = new FormData();
formData.append('file', file);
  const response = await fetch('http://localhost:5000/api/upload/image', {
    method: 'POST',
    body: formData,
  });

  const result = await response.json();
  
  if (result.success) {
    setFormData(prev => ({
      ...prev,
      Image_url: result.imagePath
    }));
    return { success: true };
  } else {
    return { success: false, message: result.message };
  }
} catch (error) {
  return { success: false, message: 'Image upload failed' };
} finally {
  setUploadingImage(false);
}
};

const handleImageChange = (e) => {
const file = e.target.files[0];
if (file) {
// Create preview
const previewUrl = URL.createObjectURL(file);
setImagePreview(previewUrl);
  // Upload image
  handleImageUpload(file).then(result => {
    if (!result.success) {
      setError(result.message);
      setImagePreview('');
    }
  });
}
};

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
  const result = await onMovieAdded(formData);
  
  if (result.success) {
    onClose();
    setFormData({
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
    setImagePreview('');
  } else {
    setError(result.message || 'Failed to add movie');
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
<h2>Add New Movie</h2>
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

      {/* Image Upload Section */}
      <div className="form-group">
        <label>Movie Poster</label>
        <div className="image-upload-container">
          <input
            type="file"
            accept="image/*"
            onChange={handleImageChange}
            className="image-upload-input"
            disabled={uploadingImage || loading}
          />
          {uploadingImage && <div className="uploading-text">Uploading image...</div>}
          
          {imagePreview && (
            <div className="image-preview">
              <img src={imagePreview} alt="Preview" className="preview-image" />
            </div>
          )}
          
          {formData.Image_url && !imagePreview && (
            <div className="current-image">
              <p>Current image: {formData.Image_url}</p>
            </div>
          )}
        </div>
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
        {loading ? 'Adding Movie...' : 'Add Movie'}
      </button>
    </form>
  </div>
</div>
);
};

export default MovieForm;