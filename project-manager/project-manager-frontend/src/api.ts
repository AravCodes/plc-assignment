import axios from 'axios';

// Default to backend dev port if env var isn't set
const baseURL = process.env.REACT_APP_API_URL || 'http://localhost:5067';

export const api = axios.create({ baseURL });

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers = config.headers || {};
    config.headers['Authorization'] = `Bearer ${token}`;
  }
  return config;
});

export interface ProjectDto {
  id: number;
  title: string;
  description?: string | null;
  createdAt?: string;
}

export interface TaskDto {
  id: number;
  title: string;
  isCompleted: boolean;
  dueDate?: string | null;
}

export interface Paginated<T> {
  page: number;
  pageSize: number;
  total: number;
  data: T[];
}

