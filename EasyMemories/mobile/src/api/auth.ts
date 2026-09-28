import { apiClient } from './client';
import { AuthResponse } from '../types';

export function register(email: string, password: string, displayName: string) {
  return apiClient
    .post<AuthResponse>('/auth/register', { email, password, displayName })
    .then((res) => res.data);
}

export function login(email: string, password: string) {
  return apiClient.post<AuthResponse>('/auth/login', { email, password }).then((res) => res.data);
}
