import React, { useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { api, TaskDto } from '../api';

export const ProjectPage: React.FC = () => {
  const { id } = useParams();
  const projectId = Number(id);
  const [tasks, setTasks] = useState<TaskDto[]>([]);
  const [title, setTitle] = useState('');
  const [dueDate, setDueDate] = useState<string>('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filter, setFilter] = useState<'all' | 'active' | 'completed'>('all');
  const [editingId, setEditingId] = useState<number | null>(null);
  const [editingTitle, setEditingTitle] = useState<string>('');

  const cacheKey = useMemo(() => `pm_tasks_${projectId}`, [projectId]);

  // Load cached tasks quickly while we fetch fresh data
  useEffect(() => {
    try {
      const cached = localStorage.getItem(cacheKey);
      if (cached) {
        const parsed: TaskDto[] = JSON.parse(cached);
        if (Array.isArray(parsed)) setTasks(parsed);
      }
    } catch {}
  }, [cacheKey]);

  useEffect(() => {
    const fetch = async () => {
      setLoading(true); setError(null);
      try {
        const r = await api.get<TaskDto[]>(`/api/projects/${projectId}/tasks`);
        setTasks(r.data);
        try { localStorage.setItem(cacheKey, JSON.stringify(r.data)); } catch {}
      } catch (e: any) {
        setError('Failed to load tasks');
      } finally {
        setLoading(false);
      }
    };
    fetch();
  }, [projectId, cacheKey]);

  const addTask = async (e: React.FormEvent) => {
    e.preventDefault();
    const payload: any = { title };
    if (dueDate) payload.dueDate = new Date(dueDate).toISOString();
    const res = await api.post<TaskDto>(`/api/projects/${projectId}/tasks`, payload);
  setTasks(prev => { const next = [...prev, res.data]; try { localStorage.setItem(cacheKey, JSON.stringify(next)); } catch {}; return next; });
    setTitle('');
    setDueDate('');
  };

  const toggle = async (task: TaskDto) => {
    const res = await api.put<TaskDto>(`/api/projects/${projectId}/tasks/${task.id}`, { id: task.id, title: task.title, isCompleted: !task.isCompleted, dueDate: task.dueDate });
    setTasks(prev => { const next = prev.map(t => t.id === task.id ? res.data : t); try { localStorage.setItem(cacheKey, JSON.stringify(next)); } catch {}; return next; });
  };

  const del = async (task: TaskDto) => {
    await api.delete(`/api/projects/${projectId}/tasks/${task.id}`);
    setTasks(prev => { const next = prev.filter(t => t.id !== task.id); try { localStorage.setItem(cacheKey, JSON.stringify(next)); } catch {}; return next; });
  };

  const startEdit = (task: TaskDto) => { setEditingId(task.id); setEditingTitle(task.title); };
  const saveEdit = async (task: TaskDto) => {
    const res = await api.put<TaskDto>(`/api/projects/${projectId}/tasks/${task.id}`, { id: task.id, title: editingTitle, isCompleted: task.isCompleted, dueDate: task.dueDate });
    setTasks(prev => { const next = prev.map(t => t.id === task.id ? res.data : t); try { localStorage.setItem(cacheKey, JSON.stringify(next)); } catch {}; return next; });
    setEditingId(null);
    setEditingTitle('');
  };

  const filtered = useMemo(() => {
    if (filter === 'active') return tasks.filter(t => !t.isCompleted);
    if (filter === 'completed') return tasks.filter(t => t.isCompleted);
    return tasks;
  }, [tasks, filter]);

  return (
    <div className="container py-4">
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h2>Project #{projectId}</h2>
        <Link to="/dashboard" className="btn btn-outline-secondary">Back</Link>
      </div>

      {error ? <div className="alert alert-danger">{error}</div> : null}
      {loading ? <p>Loading...</p> : (
        <>
          <form className="row g-2 mb-3" onSubmit={addTask}>
            <div className="col-md-6">
              <input className="form-control" placeholder="Task title" value={title} onChange={e => setTitle(e.target.value)} required minLength={1} maxLength={200} />
            </div>
            <div className="col-md-3">
              <input className="form-control" type="date" value={dueDate} onChange={e => setDueDate(e.target.value)} />
            </div>
            <div className="col-md-3 d-grid">
              <button className="btn btn-primary" type="submit">Add</button>
            </div>
          </form>

          <div className="btn-group mb-3" role="group">
            <button className={`btn btn-outline-secondary ${filter==='all'?'active':''}`} onClick={() => setFilter('all')}>All</button>
            <button className={`btn btn-outline-secondary ${filter==='active'?'active':''}`} onClick={() => setFilter('active')}>Active</button>
            <button className={`btn btn-outline-secondary ${filter==='completed'?'active':''}`} onClick={() => setFilter('completed')}>Completed</button>
          </div>

          <ul className="list-group">
            {filtered.map(t => (
              <li key={t.id} className="list-group-item d-flex justify-content-between align-items-center">
                <div className="d-flex align-items-center">
                  <input className="form-check-input me-2" type="checkbox" checked={t.isCompleted} onChange={() => toggle(t)} />
                  {editingId === t.id ? (
                    <input className="form-control" value={editingTitle} onChange={e => setEditingTitle(e.target.value)} />
                  ) : (
                    <span className={t.isCompleted ? 'text-decoration-line-through text-muted' : ''}>{t.title}</span>
                  )}
                  {t.dueDate ? <span className="badge bg-light text-dark ms-2">Due {new Date(t.dueDate).toLocaleDateString()}</span> : null}
                </div>
                <div className="d-flex gap-2">
                  {editingId === t.id ? (
                    <button className="btn btn-sm btn-success" onClick={() => saveEdit(t)}>Save</button>
                  ) : (
                    <button className="btn btn-sm btn-outline-secondary" onClick={() => startEdit(t)}>Edit</button>
                  )}
                  <button className="btn btn-sm btn-outline-danger" onClick={() => del(t)}>Delete</button>
                </div>
              </li>
            ))}
          </ul>
        </>
      )}
    </div>
  );
};


