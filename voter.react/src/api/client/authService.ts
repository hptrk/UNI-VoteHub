import apiClient from './apiClient';
import type { LoginRequestDTO } from '../models/LoginRequestDTO';
import type { LoginResponseDTO } from '../models/LoginResponseDTO';
import type { RegisterRequestDTO } from '../models/RegisterRequestDTO';
import type { RegisterResponseDTO } from '../models/RegisterResponseDTO';

export const authService = {
  login: async (loginRequest: LoginRequestDTO): Promise<LoginResponseDTO> => {
    const response = await apiClient.post<LoginResponseDTO>(
      '/auth/login',
      loginRequest
    );

    // JWT TOKEN and user in local storage
    if (response.token) {
      localStorage.setItem('token', response.token);
      localStorage.setItem('user', JSON.stringify(response.user));
    }

    return response;
  },

  register: async (
    registerRequest: RegisterRequestDTO
  ): Promise<RegisterResponseDTO> => {
    const response = await apiClient.post<RegisterResponseDTO>(
      '/auth/register',
      registerRequest
    );
    return response;
  },

  logout: (): void => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },

  isAuthenticated: (): boolean => {
    return !!localStorage.getItem('token');
  },

  getCurrentUser: (): any => {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  },
};

export default authService;
