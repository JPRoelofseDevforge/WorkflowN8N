import axios, { AxiosInstance, AxiosResponse } from 'axios';

// Auth interfaces
export interface User {
  id: number;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  permissions: string[];
  preferences: {
    theme: 'light' | 'dark';
    notifications: boolean;
    language: string;
  };
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}

export interface LoginRequest {
  Username: string;
  Password: string;
}

export interface RegisterRequest {
  Username: string;
  Email: string;
  FirstName: string;
  LastName: string;
  Password: string;
}

export interface RefreshTokenRequest {
  RefreshToken: string;
}

// n8n Workflow interface based on n8n API
export interface N8nWorkflow {
  id: string;
  name: string;
  active: boolean;
  nodes: any[];
  connections: any;
  staticData: any;
  settings: any;
  createdAt: string;
  updatedAt: string;
  tags?: string[];
}

// n8n API response interfaces
interface N8nWorkflowsResponse {
  data: N8nWorkflow[];
}

interface N8nWorkflowResponse {
  data: N8nWorkflow;
}

interface N8nExecutionResponse {
  data: {
    id: string;
    workflowId: string;
    status: string;
    startedAt: string;
    finishedAt?: string;
  };
}

class N8nApiService {
  private api: AxiosInstance;
  private baseURL: string;
  private isRefreshing = false;
  private failedQueue: Array<{
    resolve: (token: string) => void;
    reject: (error: any) => void;
  }> = [];

  constructor(baseURL: string) {
    this.baseURL = baseURL;

    this.api = axios.create({
      baseURL: this.baseURL,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Set Authorization header if token exists
    const accessToken = localStorage.getItem('accessToken');
    if (accessToken) {
      this.api.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
    }

    // Add response interceptor for error handling and token refresh
    this.api.interceptors.response.use(
      (response) => response,
      async (error) => {
        const originalRequest = error.config;

        if (error.response?.status === 401 && !originalRequest._retry) {
          if (this.isRefreshing) {
            // If already refreshing, queue the request
            return new Promise((resolve, reject) => {
              this.failedQueue.push({ resolve, reject });
            }).then(token => {
              originalRequest.headers['Authorization'] = `Bearer ${token}`;
              return this.api(originalRequest);
            }).catch(err => {
              throw err;
            });
          }

          originalRequest._retry = true;
          this.isRefreshing = true;

          try {
            const refreshToken = localStorage.getItem('refreshToken');
            if (!refreshToken) {
              throw new Error('No refresh token available');
            }

            const response = await axios.post(`${this.baseURL}/auth/refresh`, {
              RefreshToken: refreshToken,
            });

            const { AccessToken } = response.data;
            localStorage.setItem('accessToken', AccessToken);
            this.api.defaults.headers.common['Authorization'] = `Bearer ${AccessToken}`;

            // Process queued requests
            this.failedQueue.forEach(({ resolve }) => {
              resolve(AccessToken);
            });
            this.failedQueue = [];

            // Retry original request
            originalRequest.headers['Authorization'] = `Bearer ${AccessToken}`;
            return this.api(originalRequest);
          } catch (refreshError) {
            // Refresh failed, reject all queued requests
            this.failedQueue.forEach(({ reject }) => {
              reject(refreshError);
            });
            this.failedQueue = [];

            // Clear tokens and redirect to login
            localStorage.removeItem('accessToken');
            localStorage.removeItem('refreshToken');
            localStorage.removeItem('user');
            delete this.api.defaults.headers.common['Authorization'];

            // You might want to trigger a logout or redirect here
            throw refreshError;
          } finally {
            this.isRefreshing = false;
          }
        }

        console.error('API Error:', error.response?.data || error.message);
        throw error;
      }
    );
  }

  // Get all workflows
  async getWorkflows(): Promise<N8nWorkflow[]> {
    try {
      const response: AxiosResponse<any[]> = await this.api.get('/workflows');
      // Map backend response to N8nWorkflow format
      return response.data.map((wf: any) => ({
        id: wf.n8nId,
        name: wf.name,
        active: wf.isActive,
        nodes: [], // C# API doesn't store workflow data
        connections: {},
        staticData: null,
        settings: {},
        createdAt: wf.createdAt,
        updatedAt: wf.createdAt, // No separate updatedAt in C# API
        tags: []
      }));
    } catch (error) {
      console.error('Failed to fetch workflows:', error);
      throw new Error('Failed to fetch workflows from backend');
    }
  }

  // Get a specific workflow
  async getWorkflow(id: string): Promise<N8nWorkflow> {
    try {
      // Backend doesn't have single workflow endpoint, so fetch all and filter
      const workflows = await this.getWorkflows();
      const workflow = workflows.find(wf => wf.id === id);
      if (!workflow) {
        throw new Error(`Workflow ${id} not found`);
      }
      return workflow;
    } catch (error) {
      console.error(`Failed to fetch workflow ${id}:`, error);
      throw new Error(`Failed to fetch workflow ${id} from backend`);
    }
  }

  // Create a new workflow
  async createWorkflow(workflow: Partial<N8nWorkflow>): Promise<N8nWorkflow> {
    try {
      const payload = {
        name: workflow.name,
        description: workflow.name,
        isActive: workflow.active
      };
      const response: AxiosResponse<any> = await this.api.post('/workflows', payload);
      // Map response back to N8nWorkflow
      return {
        id: response.data.n8nId,
        name: response.data.name,
        active: response.data.isActive,
        nodes: [],
        connections: {},
        staticData: null,
        settings: {},
        createdAt: response.data.createdAt,
        updatedAt: response.data.createdAt,
        tags: []
      };
    } catch (error) {
      console.error('Failed to create workflow:', error);
      throw new Error('Failed to create workflow in backend');
    }
  }

  // Update a workflow
  async updateWorkflow(id: string, workflow: Partial<N8nWorkflow>): Promise<N8nWorkflow> {
    try {
      const payload = {
        name: workflow.name,
        description: workflow.name // C# API uses name as description if not provided
      };
      const response: AxiosResponse<any> = await this.api.put(`/workflows/${id}`, payload);
      // Map response back to N8nWorkflow
      return {
        id: response.data.n8nId,
        name: response.data.name,
        active: response.data.isActive,
        nodes: [],
        connections: {},
        staticData: null,
        settings: {},
        createdAt: response.data.createdAt,
        updatedAt: response.data.createdAt,
        tags: []
      };
    } catch (error) {
      console.error(`Failed to update workflow ${id}:`, error);
      throw new Error(`Failed to update workflow ${id} in backend`);
    }
  }

  // Delete a workflow
  async deleteWorkflow(id: string): Promise<void> {
    try {
      await this.api.delete(`/workflows/${id}`);
    } catch (error) {
      console.error(`Failed to delete workflow ${id}:`, error);
      throw new Error(`Failed to delete workflow ${id} from backend`);
    }
  }

  // Execute a workflow
  async executeWorkflow(id: string, data?: any): Promise<N8nExecutionResponse['data']> {
    try {
      const response: AxiosResponse<any> = await this.api.post(`/workflows/${id}/execute`, data);
      // Map backend response to expected format
      return {
        id: response.data.executionId.toString(),
        workflowId: id, // Since we passed id as n8nId
        status: "Running", // C# doesn't return status immediately
        startedAt: new Date().toISOString(),
        finishedAt: undefined
      };
    } catch (error) {
      console.error(`Failed to execute workflow ${id}:`, error);
      throw new Error(`Failed to execute workflow ${id}`);
    }
  }

  // Activate a workflow
  async activateWorkflow(id: string): Promise<N8nWorkflow> {
    try {
      const response: AxiosResponse<any> = await this.api.put(`/workflows/${id}/toggle`);
      // Map response back to N8nWorkflow
      return {
        id: response.data.n8nId,
        name: response.data.name,
        active: response.data.isActive,
        nodes: [],
        connections: {},
        staticData: null,
        settings: {},
        createdAt: response.data.createdAt,
        updatedAt: response.data.createdAt,
        tags: []
      };
    } catch (error) {
      console.error(`Failed to activate workflow ${id}:`, error);
      throw new Error(`Failed to activate workflow ${id}`);
    }
  }

  // Deactivate a workflow
  async deactivateWorkflow(id: string): Promise<N8nWorkflow> {
    try {
      const response: AxiosResponse<any> = await this.api.put(`/workflows/${id}/toggle`);
      // Map response back to N8nWorkflow
      return {
        id: response.data.n8n_id,
        name: response.data.name,
        active: response.data.status === 'active',
        nodes: response.data.workflow_data?.nodes || [],
        connections: response.data.workflow_data?.connections || {},
        staticData: response.data.workflow_data?.staticData || null,
        settings: response.data.workflow_data?.settings || {},
        createdAt: response.data.created_at,
        updatedAt: response.data.updated_at,
        tags: response.data.workflow_data?.tags || []
      };
    } catch (error) {
      console.error(`Failed to deactivate workflow ${id}:`, error);
      throw new Error(`Failed to deactivate workflow ${id}`);
    }
  }

  // Authentication methods
  async login(request: LoginRequest): Promise<AuthResponse> {
    try {
      console.log('🔐 n8nApi: Making login request to:', this.baseURL + '/auth/login');
      const response: AxiosResponse<any> = await this.api.post('/auth/login', request);
      console.log('🔐 n8nApi: Login response received:', {
        status: response.status,
        hasData: !!response.data,
        dataKeys: response.data ? Object.keys(response.data) : [],
        hasAccessToken: !!response.data?.accessToken,
        hasRefreshToken: !!response.data?.refreshToken,
        hasUser: !!response.data?.user
      });

      const { accessToken, refreshToken, user } = response.data;
      console.log('🔐 n8nApi: Extracted values:', {
        accessToken: accessToken ? accessToken.substring(0, 20) + '...' : null,
        refreshToken: refreshToken ? refreshToken.substring(0, 20) + '...' : null,
        user: user ? { id: user.id, username: user.username, roles: user.roles } : null
      });

      return {
        accessToken,
        refreshToken,
        user,
      };
    } catch (error) {
      console.error('❌ n8nApi: Login failed:', error);
      throw new Error('Login failed');
    }
  }

  async register(request: RegisterRequest): Promise<AuthResponse> {
    try {
      console.log('👤 n8nApi: Making register request to:', this.baseURL + '/auth/register');
      const response: AxiosResponse<any> = await this.api.post('/auth/register', request);
      console.log('👤 n8nApi: Register response received:', {
        status: response.status,
        hasData: !!response.data,
        dataKeys: response.data ? Object.keys(response.data) : []
      });

      const { accessToken, refreshToken, user } = response.data;
      console.log('👤 n8nApi: Extracted register values:', {
        accessToken: accessToken ? accessToken.substring(0, 20) + '...' : null,
        refreshToken: refreshToken ? refreshToken.substring(0, 20) + '...' : null,
        user: user ? { id: user.id, username: user.username } : null
      });

      return {
        accessToken,
        refreshToken,
        user,
      };
    } catch (error) {
      console.error('❌ n8nApi: Registration failed:', error);
      throw new Error('Registration failed');
    }
  }

  async logout(): Promise<void> {
    try {
      await this.api.post('/auth/logout');
    } catch (error) {
      console.error('Logout failed:', error);
      throw new Error('Logout failed');
    }
  }

  async refreshToken(request: RefreshTokenRequest): Promise<{ accessToken: string }> {
    try {
      const response: AxiosResponse<any> = await this.api.post('/auth/refresh', request);
      const { AccessToken } = response.data;
      return { accessToken: AccessToken };
    } catch (error) {
      console.error('Token refresh failed:', error);
      throw new Error('Token refresh failed');
    }
  }

  // Set authorization header
  setAuthToken(token: string | null): void {
    if (token) {
      this.api.defaults.headers.common['Authorization'] = `Bearer ${token}`;
    } else {
      delete this.api.defaults.headers.common['Authorization'];
    }
  }

  // Test connection to backend
  async testConnection(): Promise<boolean> {
    try {
      await this.api.get('/workflows');
      return true;
    } catch (error) {
      console.error('Failed to connect to backend:', error);
      return false;
    }
  }

  // Get n8n web editor URL for a workflow
  getN8nEditorUrl(workflowId: string): string {
    // Use the n8n web URL from backend configuration
    const n8nWebUrl = 'https://n8n-945bdc38.azurewebsites.net';
    return `${n8nWebUrl}/workflow/${workflowId}`;
  }

  // Open workflow in n8n editor
  openWorkflowInN8n(workflowId: string): void {
    const editorUrl = this.getN8nEditorUrl(workflowId);
    console.log('🎨 Opening n8n editor:', editorUrl);
    window.open(editorUrl, '_blank');
  }
}

// Create and export a default instance
const n8nApi = new N8nApiService(
  import.meta.env.VITE_API_BASE_URL || 'http://localhost:3001/api'
);

export default n8nApi;
export { N8nApiService };