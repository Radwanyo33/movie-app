import React, { useState } from 'react';
import './LoginModal.css';

const LoginModal = ({ isOpen, onClose, onLogin }) => {
const [email, setEmail] = useState('');
const [password, setPassword] = useState('');
const [error, setError] = useState('');
const [loading, setLoading] = useState(false);

const handleSubmit = async (e) => {
e.preventDefault();
setError('');
setLoading(true);

try {
  const result = await onLogin(email, password);
  
  if (result.success) {
    setEmail('');
    setPassword('');
  } else {
    setError(result.message || 'Login failed');
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
<div className="modal-content">
<button className="close-button" onClick={onClose}>×</button>
<h2>Admin Login</h2>
<form onSubmit={handleSubmit}>
<div className="form-group">
<label>Email:</label>
<input
type="email"
value={email}
onChange={(e) => setEmail(e.target.value)}
required
disabled={loading}
/>
</div>
<div className="form-group">
<label>Password:</label>
<input
type="password"
value={password}
onChange={(e) => setPassword(e.target.value)}
required
disabled={loading}
/>
</div>
{error && <div className="error-message">{error}</div>}
<button type="submit" className="login-button" disabled={loading}>
{loading ? 'Logging in...' : 'Login'}
</button>
</form>
</div>
</div>
);
};

export default LoginModal;