import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

export const Register: React.FC = () => {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    try {
      await register(email, password);
      navigate('/dashboard');
    } catch (err) {
      const anyErr = err as any;
      const serverMsg = anyErr?.response?.data?.message || anyErr?.response?.data?.title || anyErr?.message;
      setError(serverMsg ? `Registration failed: ${serverMsg}` : 'Registration failed');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container py-4">
      <h2>Register</h2>
      <form className="mt-3" onSubmit={onSubmit}>
        <div className="mb-3">
          <label className="form-label">Email</label>
          <input className="form-control" value={email} onChange={e => setEmail(e.target.value)} />
        </div>
        <div className="mb-3">
          <label className="form-label">Password</label>
          <input type="password" className="form-control" value={password} onChange={e => setPassword(e.target.value)} />
        </div>
        {error && <div className="alert alert-danger">{error}</div>}
        <button className="btn btn-primary" disabled={loading}>{loading ? 'Creating...' : 'Create account'}</button>
      </form>
      <p className="mt-3">Already have an account? <Link to="/login">Login</Link></p>
    </div>
  );
};


