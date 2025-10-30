import React from 'react';
import { TaskItem as Task } from '../api';
import { TaskItem } from './TaskItem';

interface Props {
  tasks: Task[];
  onToggle: (task: Task) => Promise<void> | void;
  onDelete: (task: Task) => Promise<void> | void;
}

export const TaskList: React.FC<Props> = ({ tasks, onToggle, onDelete }) => {
  if (!tasks.length) {
    return <p className="text-muted">No tasks yet. Add your first one!</p>;
  }

  return (
    <ul className="list-group">
      {tasks.map((t) => (
        <TaskItem key={t.id} task={t} onToggle={onToggle} onDelete={onDelete} />)
      )}
    </ul>
  );
};


