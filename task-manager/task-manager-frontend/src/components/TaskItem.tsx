import React from 'react';
import type { TaskItem as Task } from '../api';

interface Props {
  task: Task;
  onToggle: (task: Task) => Promise<void> | void;
  onDelete: (task: Task) => Promise<void> | void;
}

export const TaskItem: React.FC<Props> = ({ task, onToggle, onDelete }) => {
  return (
    <li className="list-group-item d-flex justify-content-between align-items-center">
      <div className="form-check">
        <input
          className="form-check-input"
          type="checkbox"
          checked={task.isCompleted}
          id={`task-${task.id}`}
          onChange={() => onToggle(task)}
        />
        <label
          className={`form-check-label ms-2 ${task.isCompleted ? 'text-decoration-line-through text-muted' : ''}`}
          htmlFor={`task-${task.id}`}
        >
          {task.description}
        </label>
      </div>
      <button className="btn btn-sm btn-outline-danger" onClick={() => onDelete(task)}>
        Delete
      </button>
    </li>
  );
};


