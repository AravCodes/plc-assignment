import React, { useEffect, useState } from 'react';
import { api, ProjectDto, Paginated } from '../api';
import { useAuth } from '../auth/AuthContext';
import { Link } from 'react-router-dom';

export const Dashboard: React.FC = () => {
  const { logout } = useAuth();
  const [projects, setProjects] = useState<ProjectDto[]>([]);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');

  useEffect(() => {
    api.get<Paginated<ProjectDto>>('/api/projects', { params: { page: 1, pageSize: 20 } })
      .then(r => setProjects(r.data.data));
  }, []);

  const addProject = async (e: React.FormEvent) => {
    e.preventDefault();
    const res = await api.post<ProjectDto>('/api/projects', { title, description });
    setProjects(prev => [...prev, res.data]);
    setTitle('');
    setDescription('');
  };

  return (
    <div className="container py-4">
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h2>Projects</h2>
        <button className="btn btn-outline-secondary" onClick={logout}>Logout</button>
      </div>

      <form className="mb-4" onSubmit={addProject}>
        <div className="row g-2">
          <div className="col-md-4">
            <input className="form-control" placeholder="Title" value={title} onChange={e => setTitle(e.target.value)} />
          </div>
          <div className="col-md-6">
            <input className="form-control" placeholder="Description" value={description} onChange={e => setDescription(e.target.value)} />
          </div>
          <div className="col-md-2 d-grid">
            <button className="btn btn-primary" type="submit">Add</button>
          </div>
        </div>
      </form>

      <ul className="list-group">
        {projects.map(p => (
          <li key={p.id} className="list-group-item d-flex justify-content-between align-items-center">
            <div>
              <strong>{p.title}</strong>
              {p.description ? <span className="text-muted ms-2">{p.description}</span> : null}
              {p.createdAt ? <div className="small text-muted">Created {new Date(p.createdAt).toLocaleString()}</div> : null}
            </div>
            <Link className="btn btn-sm btn-outline-primary" to={`/project/${p.id}`}>Open</Link>
          </li>
        ))}
      </ul>
    </div>
  );
};


