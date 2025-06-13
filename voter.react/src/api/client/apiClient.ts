import type { ProblemDetails } from '../models/ProblemDetails';

const API_BASE_URL = 'https://localhost:7012/api';

interface FetchOptions extends RequestInit {
  params?: Record<string, string>;
}

// FETCH HELPER
async function fetchApi<T>(
  endpoint: string,
  options: FetchOptions = {}
): Promise<T> {
  // authorization token to header if available
  const token = localStorage.getItem('token');
  const headers = new Headers(options.headers || {});

  // default headers
  if (!headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }

  if (token) {
    headers.set('Authorization', `Bearer ${token}`);
  }

  // build URL with query parameters if they exist
  let url = `${API_BASE_URL}${endpoint}`;
  if (options.params) {
    const searchParams = new URLSearchParams();
    Object.entries(options.params).forEach(([key, value]) => {
      searchParams.append(key, value);
    });
    url += `?${searchParams.toString()}`;
  }

  // create request options
  const fetchOptions: RequestInit = {
    ...options,
    headers,
  };

  try {
    const response = await fetch(url, fetchOptions);

    // 401 Unauthorized responses
    if (response.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
      throw new Error('Unauthorized');
    }

    // response body
    const data = await (response.headers
      .get('Content-Type')
      ?.includes('application/json')
      ? response.json()
      : response.text());

    // API errors
    if (!response.ok) {
      const problemDetails = data as ProblemDetails;
      throw problemDetails;
    }

    return data as T;
  } catch (error) {
    if ((error as ProblemDetails).status) {
      throw error;
    }
    throw { detail: (error as Error).message, status: 500 } as ProblemDetails;
  }
}

const apiClient = {
  get: <T>(endpoint: string, params?: Record<string, string>): Promise<T> => {
    return fetchApi<T>(endpoint, { method: 'GET', params });
  },
  post: <T>(endpoint: string, data?: unknown): Promise<T> => {
    return fetchApi<T>(endpoint, {
      method: 'POST',
      body: data ? JSON.stringify(data) : undefined,
    });
  },

  put: <T>(endpoint: string, data?: unknown): Promise<T> => {
    return fetchApi<T>(endpoint, {
      method: 'PUT',
      body: data ? JSON.stringify(data) : undefined,
    });
  },

  patch: <T>(endpoint: string, data?: unknown): Promise<T> => {
    return fetchApi<T>(endpoint, {
      method: 'PATCH',
      body: data ? JSON.stringify(data) : undefined,
    });
  },

  delete: <T>(endpoint: string): Promise<T> => {
    return fetchApi<T>(endpoint, { method: 'DELETE' });
  },
};

export default apiClient;
