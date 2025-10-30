import React, { useEffect, useMemo, useState } from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import { api, TaskItem } from './api';
import { TaskInput } from './components/TaskInput';
import { TaskList } from './components/TaskList';

type Filter = 'all' | 'active' | 'completed';

function App() {
  const [tasks, setTasks] = useState<TaskItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<Filter>('all');

  // Read projectId from query string
  const projectId = useMemo(() => {
    const p = new URLSearchParams(window.location.search).get('projectId');
    return p ? parseInt(p, 10) : undefined;
  }, []);

  useEffect(() => {
    const fetchTasks = async () => {
      try {
        if (!projectId) {
          setTasks([]);
          return;
        }
        const res = await api.get<TaskItem[]>(`/api/projects/${projectId}/tasks`);
        setTasks(res.data);
      } finally {
        setLoading(false);
      }
    };
    fetchTasks();
  }, [projectId]);

  const filtered = useMemo(() => {
    if (filter === 'active') return tasks.filter(t => !t.isCompleted);
    if (filter === 'completed') return tasks.filter(t => t.isCompleted);
    return tasks;
  }, [tasks, filter]);

  const addTask = async (description: string) => {
    if (!projectId) return;
    const res = await api.post<TaskItem>(`/api/projects/${projectId}/tasks`, { description });
    setTasks(prev => [...prev, res.data]);
    // Persist to localStorage for enhancement
    localStorage.setItem('tasks', JSON.stringify([...tasks, res.data]));
  };

  const toggleTask = async (task: TaskItem) => {
    if (!projectId) return;
    const res = await api.put<TaskItem>(`/api/projects/${projectId}/tasks/${task.id}`, {
      description: task.description,
      isCompleted: !task.isCompleted,
    });
    setTasks(prev => prev.map(t => (t.id === task.id ? res.data : t)));
  };

  const deleteTask = async (task: TaskItem) => {
    if (!projectId) return;
    await api.delete(`/api/projects/${projectId}/tasks/${task.id}`);
    setTasks(prev => prev.filter(t => t.id !== task.id));
  };

  return (
    <div className="container py-4">
  <h1 className="mb-4">Task Manager {projectId ? `(Project ${projectId})` : ''}</h1>

      <TaskInput onAdd={addTask} />

      <div className="btn-group mb-3" role="group">
        <button className={`btn btn-outline-secondary ${filter === 'all' ? 'active' : ''}`} onClick={() => setFilter('all')}>All</button>
        <button className={`btn btn-outline-secondary ${filter === 'active' ? 'active' : ''}`} onClick={() => setFilter('active')}>Active</button>
        <button className={`btn btn-outline-secondary ${filter === 'completed' ? 'active' : ''}`} onClick={() => setFilter('completed')}>Completed</button>
      </div>

      {loading ? (
        <p>Loading...</p>
      ) : (
        <TaskList tasks={filtered} onToggle={toggleTask} onDelete={deleteTask} />
      )}
    </div>
  );
}

export default App;
