import axios from 'axios';

const baseURL = process.env.REACT_APP_API_URL || 'http://localhost:5091';

export const api = axios.create({
  baseURL,
});

export interface TaskItem {
  id: number;
  projectId?: number;
  description: string;
  isCompleted: boolean;
}


