const axios = require('axios');

class N8nService {
  constructor() {
    this.baseURL = process.env.N8N_BASE_URL;
    this.apiKey = process.env.N8N_API_KEY;

    this.api = axios.create({
      baseURL: this.baseURL,
      headers: {
        'Content-Type': 'application/json',
        ...(this.apiKey && { 'X-N8N-API-KEY': this.apiKey }),
      },
    });

    // Add response interceptor for error handling
    this.api.interceptors.response.use(
      (response) => response,
      (error) => {
        console.error('n8n API Error:', error.response?.data || error.message);
        throw error;
      }
    );
  }

  // Get all workflows
  async getWorkflows() {
    try {
      const response = await this.api.get('/workflows');
      return response.data.data;
    } catch (error) {
      console.error('Failed to fetch workflows:', error);
      throw new Error('Failed to fetch workflows from n8n');
    }
  }

  // Get a specific workflow
  async getWorkflow(id) {
    try {
      const response = await this.api.get(`/workflows/${id}`);
      return response.data.data;
    } catch (error) {
      console.error(`Failed to fetch workflow ${id}:`, error);
      throw new Error(`Failed to fetch workflow ${id} from n8n`);
    }
  }

  // Create a new workflow
  async createWorkflow(workflow) {
    try {
      console.log('🔍 DEBUG: Payload being sent to n8n /workflows:', JSON.stringify(workflow, null, 2));
      const response = await this.api.post('/workflows', workflow);
      console.log('🔍 DEBUG: n8n response:', JSON.stringify(response.data, null, 2));
      return response.data.data;
    } catch (error) {
      console.error('❌ Failed to create workflow:', error);
      console.error('❌ Error response:', error.response?.data);
      throw new Error('Failed to create workflow in n8n');
    }
  }

  // Update a workflow
  async updateWorkflow(id, workflow) {
    try {
      const response = await this.api.put(`/workflows/${id}`, workflow);
      return response.data.data;
    } catch (error) {
      console.error(`Failed to update workflow ${id}:`, error);
      throw new Error(`Failed to update workflow ${id} in n8n`);
    }
  }

  // Delete a workflow
  async deleteWorkflow(id) {
    try {
      await this.api.delete(`/workflows/${id}`);
    } catch (error) {
      console.error(`Failed to delete workflow ${id}:`, error);
      throw new Error(`Failed to delete workflow ${id} from n8n`);
    }
  }

  // Toggle workflow active status
  async toggleWorkflow(id) {
    try {
      const workflow = await this.getWorkflow(id);
      if (workflow.active) {
        const response = await this.api.post(`/workflows/${id}/deactivate`);
        return response.data.data;
      } else {
        const response = await this.api.post(`/workflows/${id}/activate`);
        return response.data.data;
      }
    } catch (error) {
      console.error(`Failed to toggle workflow ${id}:`, error);
      throw new Error(`Failed to toggle workflow ${id}`);
    }
  }

  // Execute a workflow
  async executeWorkflow(id, data) {
    try {
      const response = await this.api.post(`/workflows/${id}/execute`, data);
      return response.data.data;
    } catch (error) {
      console.error(`Failed to execute workflow ${id}:`, error);
      throw new Error(`Failed to execute workflow ${id}`);
    }
  }
}

module.exports = new N8nService();