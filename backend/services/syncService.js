const n8nService = require('./n8nService');
const { pool } = require('../db');

class SyncService {
  // Fetch all workflows from n8n and save/update them in the database
  async syncWorkflowsFromN8n() {
    try {
      const n8nWorkflows = await n8nService.getWorkflows();
      const syncedWorkflows = [];

      for (const n8nWorkflow of n8nWorkflows) {
        const existingWorkflow = await pool.query(
          'SELECT * FROM workflows WHERE n8n_id = $1',
          [n8nWorkflow.id]
        );

        if (existingWorkflow.rows.length === 0) {
          // Insert new workflow
          const result = await pool.query(
            'INSERT INTO workflows (name, description, status, n8n_id, workflow_data) VALUES ($1, $2, $3, $4, $5) RETURNING *',
            [
              n8nWorkflow.name,
              n8nWorkflow.description || '',
              n8nWorkflow.active ? 'active' : 'inactive',
              n8nWorkflow.id,
              JSON.stringify(n8nWorkflow)
            ]
          );
          syncedWorkflows.push(result.rows[0]);
        } else {
          // Update existing workflow
          const result = await pool.query(
            'UPDATE workflows SET name = $1, description = $2, status = $3, workflow_data = $4, updated_at = CURRENT_TIMESTAMP WHERE n8n_id = $5 RETURNING *',
            [
              n8nWorkflow.name,
              n8nWorkflow.description || '',
              n8nWorkflow.active ? 'active' : 'inactive',
              JSON.stringify(n8nWorkflow),
              n8nWorkflow.id
            ]
          );
          syncedWorkflows.push(result.rows[0]);
        }
      }

      return { success: true, syncedWorkflows, count: syncedWorkflows.length };
    } catch (error) {
      console.error('Failed to sync workflows from n8n:', error);
      throw new Error('Failed to sync workflows from n8n');
    }
  }

  // Ensure a workflow exists in n8n (for cases where DB has it but n8n doesn't)
  async syncWorkflowToN8n(workflow) {
    try {
      // Check if workflow exists in n8n
      let n8nWorkflow;
      try {
        n8nWorkflow = await n8nService.getWorkflow(workflow.n8n_id);
      } catch (error) {
        // Workflow doesn't exist in n8n, create it
        const workflowData = JSON.parse(workflow.workflow_data);
        n8nWorkflow = await n8nService.createWorkflow({
          name: workflow.name,
          ...workflowData
        });

        // Update DB with new n8n_id if it changed
        if (n8nWorkflow.id !== workflow.n8n_id) {
          await pool.query(
            'UPDATE workflows SET n8n_id = $1, updated_at = CURRENT_TIMESTAMP WHERE id = $2',
            [n8nWorkflow.id, workflow.id]
          );
        }
      }

      return { success: true, n8nWorkflow };
    } catch (error) {
      console.error('Failed to sync workflow to n8n:', error);
      throw new Error('Failed to sync workflow to n8n');
    }
  }

  // Check if database is in sync with n8n
  async getSyncStatus() {
    try {
      // Get counts
      const dbResult = await pool.query('SELECT COUNT(*) as count FROM workflows');
      const dbCount = parseInt(dbResult.rows[0].count);

      const n8nWorkflows = await n8nService.getWorkflows();
      const n8nCount = n8nWorkflows.length;

      // Check for discrepancies
      const discrepancies = [];
      for (const n8nWorkflow of n8nWorkflows) {
        const dbWorkflow = await pool.query(
          'SELECT * FROM workflows WHERE n8n_id = $1',
          [n8nWorkflow.id]
        );

        if (dbWorkflow.rows.length === 0) {
          discrepancies.push({
            type: 'missing_in_db',
            n8n_id: n8nWorkflow.id,
            name: n8nWorkflow.name
          });
        } else {
          // Check if data matches
          const dbData = JSON.parse(dbWorkflow.rows[0].workflow_data);
          if (dbWorkflow.rows[0].name !== n8nWorkflow.name ||
              dbWorkflow.rows[0].status !== (n8nWorkflow.active ? 'active' : 'inactive')) {
            discrepancies.push({
              type: 'data_mismatch',
              n8n_id: n8nWorkflow.id,
              name: n8nWorkflow.name,
              db_name: dbWorkflow.rows[0].name,
              db_status: dbWorkflow.rows[0].status,
              n8n_status: n8nWorkflow.active ? 'active' : 'inactive'
            });
          }
        }
      }

      // Check for workflows in DB but not in n8n
      const allDbWorkflows = await pool.query('SELECT * FROM workflows');
      for (const dbWorkflow of allDbWorkflows.rows) {
        const existsInN8n = n8nWorkflows.some(n8n => n8n.id === dbWorkflow.n8n_id);
        if (!existsInN8n) {
          discrepancies.push({
            type: 'missing_in_n8n',
            db_id: dbWorkflow.id,
            n8n_id: dbWorkflow.n8n_id,
            name: dbWorkflow.name
          });
        }
      }

      const isInSync = discrepancies.length === 0;

      return {
        isInSync,
        dbCount,
        n8nCount,
        discrepancies
      };
    } catch (error) {
      console.error('Failed to get sync status:', error);
      throw new Error('Failed to get sync status');
    }
  }
}

module.exports = new SyncService();