import React, { useState } from 'react';

interface Props {
  onAdd: (description: string) => Promise<void> | void;
}

export const TaskInput: React.FC<Props> = ({ onAdd }) => {
  const [value, setValue] = useState('');
  const [loading, setLoading] = useState(false);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    const trimmed = value.trim();
    if (!trimmed) return;
    setLoading(true);
    try {
      await onAdd(trimmed);
      setValue('');
    } finally {
      setLoading(false);
    }
  };

  return (
    <form className="d-flex gap-2 mb-3" onSubmit={submit}>
      <input
        className="form-control"
        placeholder="Add a task..."
        value={value}
        onChange={(e) => setValue(e.target.value)}
        disabled={loading}
      />
      <button className="btn btn-primary" type="submit" disabled={loading}>
        {loading ? 'Adding...' : 'Add'}
      </button>
    </form>
  );
};


