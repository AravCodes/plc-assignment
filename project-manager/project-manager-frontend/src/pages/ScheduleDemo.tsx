import React, { useState } from 'react';
import { useParams } from 'react-router-dom';
import { api } from '../api';

interface InTask { title: string; duration: number; }
interface OutTask { title: string; start: string; end: string; }

export const ScheduleDemo: React.FC = () => {
  const { id } = useParams();
  const projectId = Number(id);
  const [tasks, setTasks] = useState<InTask[]>([{ title: 'UI Design', duration: 3 }]);
  const [startDate, setStartDate] = useState<string>(new Date().toISOString().slice(0,10));
  const [loading, setLoading] = useState(false);
  const [schedule, setSchedule] = useState<OutTask[] | null>(null);

  const addRow = () => setTasks(prev => [...prev, { title: '', duration: 1 }]);
  const update = (i: number, field: keyof InTask, value: string) => {
    const clone = [...tasks];
    // @ts-expect-error narrow later
    clone[i][field] = field === 'duration' ? Number(value) : value;
    setTasks(clone);
  };

  const generate = async () => {
    setLoading(true);
    try {
      const res = await api.post(`/api/v1/projects/${projectId}/schedule`, { tasks, startDate });
      setSchedule(res.data.schedule);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container py-4">
      <h3>Smart Scheduler</h3>
      <div className="table-responsive">
        <table className="table">
          <thead>
            <tr><th>Title</th><th>Duration (days)</th></tr>
          </thead>
          <tbody>
            {tasks.map((t, i) => (
              <tr key={i}>
                <td><input className="form-control" value={t.title} onChange={e => update(i, 'title', e.target.value)} /></td>
                <td style={{maxWidth:120}}><input type="number" min={1} className="form-control" value={t.duration} onChange={e => update(i, 'duration', e.target.value)} /></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <button className="btn btn-sm btn-outline-secondary me-2" onClick={addRow}>Add Row</button>
      <input type="date" className="form-control d-inline-block" style={{width: 200}} value={startDate} onChange={e => setStartDate(e.target.value)} />
      <button className="btn btn-primary ms-2" onClick={generate} disabled={loading}>{loading ? 'Generating...' : 'Generate Schedule'}</button>

      {schedule && (
        <div className="mt-4">
          <h5>Result</h5>
          <table className="table">
            <thead><tr><th>Title</th><th>Start</th><th>End</th></tr></thead>
            <tbody>
              {schedule.map((s, i) => (
                <tr key={i}><td>{s.title}</td><td>{s.start}</td><td>{s.end}</td></tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};


