import { createContext, useContext, useState, useEffect } from 'react';
import type { ReactNode } from 'react';
import authService from '../api/client/authService';
import type { UserDTO } from '../api/models/UserDTO';
import type { LoginRequestDTO } from '../api/models/LoginRequestDTO';
import type { RegisterRequestDTO } from '../api/models/RegisterRequestDTO';

interface AuthContextType {
  user: UserDTO | null;
  isAuthenticated: boolean;
  loading: boolean;
  login: (loginData: LoginRequestDTO) => Promise<void>;
  register: (registerData: RegisterRequestDTO) => Promise<void>;
  logout: () => void;
  error: string | null;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<UserDTO | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    // check if user is already logged in
    const storedUser = authService.getCurrentUser();
    if (storedUser) {
      setUser(storedUser);
    }
    setLoading(false);
  }, []);

  const login = async (loginData: LoginRequestDTO) => {
    setLoading(true);
    setError(null);
    try {
      const response = await authService.login(loginData);
      setUser(response.user || null);
    } catch (err: unknown) {
      setError(
        typeof err === 'object' && err && 'detail' in err
          ? (err.detail as string)
          : 'Failed to login'
      );
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const register = async (registerData: RegisterRequestDTO) => {
    setLoading(true);
    setError(null);
    try {
      await authService.register(registerData);
    } catch (err: unknown) {
      setError(
        typeof err === 'object' && err && 'detail' in err
          ? (err.detail as string)
          : 'Failed to register'
      );
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const logout = () => {
    authService.logout();
    setUser(null);
  };

  const value = {
    user,
    isAuthenticated: !!user,
    loading,
    login,
    register,
    logout,
    error,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
