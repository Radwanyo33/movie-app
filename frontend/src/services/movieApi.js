// Dynamic API URL based on environment
const getApiBaseUrl = () => {
  // In production, use your Render backend URL
  if (process.env.NODE_ENV === 'production') {
    return 'https://movie-app-backend.onrender.com/api';
  }
  return 'http://localhost:5000/api';
};

const API_BASE_URL = getApiBaseUrl();

// Generic API request function
async function apiRequest(endpoint, options = {}) {
  const url = `${API_BASE_URL}${endpoint}`;

  try {
    const response = await fetch(url, {
      headers: {
        'Content-Type': 'application/json',
        ...options.headers,
      },
      credentials: 'include', // Include cookies for session/auth
      ...options,
    });

    if (!response.ok) {
      throw new Error(`API error: ${response.status} ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    console.error('API request failed:', error);
    throw error;
  }
}

// Movie API functions
export const movieApi = {
  // Get all movies
  getAllMovies: async () => {
    return await apiRequest('/movies');
  },

  // Search movies
  searchMovies: async (searchTerm) => {
    return await apiRequest(`/movies/search?q=${encodeURIComponent(searchTerm)}`);
  },

  // Add new movie (regular JSON)
  addMovie: async (movieData) => {
    return await apiRequest('/movies', {
      method: 'POST',
      body: JSON.stringify(movieData),
    });
  },

  // Add new movie with image upload
  addMovieWithImage: async (formData) => {
    const response = await fetch(`${API_BASE_URL}/movies/with-image`, {
      method: 'POST',
      body: formData,
      credentials: 'include',
      // Don't set Content-Type header for FormData - browser will set it automatically
    });

    if (!response.ok) {
      throw new Error(`API error: ${response.status} ${response.statusText}`);
    }

    return await response.json();
  },

  // Update movie
  updateMovie: async (id, movieData) => {
    return await apiRequest(`/movies/${id}`, {
      method: 'PUT',
      body: JSON.stringify(movieData),
    });
  },

  // Delete movie
  deleteMovie: async (id) => {
    const response = await fetch(`${API_BASE_URL}/movies/${id}`, {
      method: 'DELETE',
      credentials: 'include'
    });
    
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    
    try {
      return await response.json();
    } catch (error) {
      return { success: true, message: 'Movie deleted successfully' };
    }
  },

  // Get movie by ID
  getMovieById: async (id) => {
    return await apiRequest(`/movies/${id}`);
  },

  // Upload image separately
  uploadImage: async (file) => {
    const formData = new FormData();
    formData.append('file', file);
    
    const response = await fetch(`${API_BASE_URL}/upload/image`, {
      method: 'POST',
      body: formData,
      credentials: 'include',
    });

    if (!response.ok) {
      throw new Error(`API error: ${response.status} ${response.statusText}`);
    }

    return await response.json();
  }
};

// Update image URL helper
export const getImageUrl = (imagePath) => {
  if (!imagePath) return '';
  
  if (imagePath.startsWith('http')) {
    return imagePath;
  }
  
  if (imagePath.startsWith('/uploads/')) {
    const baseUrl = process.env.NODE_ENV === 'production'
      ? 'https://movie-app-backend.onrender.com'
      : 'http://localhost:5000';
    return `${baseUrl}${imagePath}`;
  }
  
  // For frontend images
  return imagePath;
};

// Auth API functions
export const authApi = {
  // Admin login
  login: async (email, password) => {
    return await apiRequest('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    });
  },

  // Admin register (if needed)
  register: async (email, password) => {
    return await apiRequest('/auth/register', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    });
  },

  checkAuth: async () => {
    return await apiRequest('/auth/check-auth');
  },

  // Admin Logout
  logout: async () => {
    return await apiRequest('/auth/logout', {
      method: 'POST',
    });
  }
};