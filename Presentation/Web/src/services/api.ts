import { getToken } from './keycloak';

class ApiClient {
  private baseUrl: string;

  constructor(baseUrl: string = 'http://localhost:8090/api') {
    this.baseUrl = baseUrl;
  }

  private async getHeaders(): Promise<HeadersInit> {
    const token = getToken();
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
    };

    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    return headers;
  }

  private async handleResponse<T>(response: Response): Promise<T> {
    if (!response.ok) {
      if (response.status === 401) {
        // Unauthorized - redirect to login
        window.location.reload();
        throw new Error('Unauthorized');
      }
      
      const errorText = await response.text();
      throw new Error(`HTTP ${response.status}: ${errorText}`);
    }

    const contentType = response.headers.get('content-type');
    if (contentType && contentType.includes('application/json')) {
      return response.json();
    }
    
    return response.text() as unknown as T;
  }

  async get<T>(endpoint: string): Promise<T> {
    const headers = await this.getHeaders();
    const response = await fetch(`${this.baseUrl}${endpoint}`, {
      method: 'GET',
      headers,
    });

    return this.handleResponse<T>(response);
  }

  async post<T>(endpoint: string, data?: any): Promise<T> {
    const headers = await this.getHeaders();
    const response = await fetch(`${this.baseUrl}${endpoint}`, {
      method: 'POST',
      headers,
      body: data ? JSON.stringify(data) : undefined,
    });

    return this.handleResponse<T>(response);
  }

  async put<T>(endpoint: string, data?: any): Promise<T> {
    const headers = await this.getHeaders();
    const response = await fetch(`${this.baseUrl}${endpoint}`, {
      method: 'PUT',
      headers,
      body: data ? JSON.stringify(data) : undefined,
    });

    return this.handleResponse<T>(response);
  }

  async delete<T>(endpoint: string): Promise<T> {
    const headers = await this.getHeaders();
    const response = await fetch(`${this.baseUrl}${endpoint}`, {
      method: 'DELETE',
      headers,
    });

    return this.handleResponse<T>(response);
  }
}

// Create a singleton instance
export const apiClient = new ApiClient();

// API endpoints
export const environmentsApi = {
  getAll: () => apiClient.get<any[]>('/environments'),
  getById: (id: string) => apiClient.get<any>(`/environments/${id}`),
  create: (data: any) => apiClient.post<any>('/environments', data),
  update: (id: string, data: any) => apiClient.put<any>(`/environments/${id}`, data),
  delete: (id: string) => apiClient.delete<void>(`/environments/${id}`),
};

export const installationsApi = {
  getAll: () => apiClient.get<any[]>('/installations'),
  getById: (id: string) => apiClient.get<any>(`/installations/${id}`),
  create: (data: any) => apiClient.post<any>('/installations', data),
  update: (id: string, data: any) => apiClient.put<any>(`/installations/${id}`, data),
  delete: (id: string) => apiClient.delete<void>(`/installations/${id}`),
  getByEnvironment: (environmentId: string) => apiClient.get<any[]>(`/installations/environment/${environmentId}`),
};

export const usersApi = {
  getProfile: () => apiClient.get<any>('/users/profile'),
  updateProfile: (data: any) => apiClient.put<any>('/users/profile', data),
  getAll: () => apiClient.get<any[]>('/users'),
  invite: (data: any) => apiClient.post<any>('/users/invite', data),
};
