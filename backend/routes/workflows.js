const express = require('express');
const router = express.Router();
const n8nService = require('../services/n8nService');
const syncService = require('../services/syncService');
const { pool } = require('../db');

// GET /api/workflows: Fetch all workflows from database
router.get('/workflows', async (req, res) => {
  try {
    const result = await pool.query('SELECT * FROM workflows ORDER BY created_at DESC');

    // If database is empty, sync from n8n
    if (result.rows.length === 0) {
      console.log('Database is empty, syncing workflows from n8n...');
      try {
        const syncResult = await syncService.syncWorkflowsFromN8n();
        console.log(`Synced ${syncResult.count} workflows from n8n`);
        // Fetch again after sync
        const updatedResult = await pool.query('SELECT * FROM workflows ORDER BY created_at DESC');
        return res.json(updatedResult.rows);
      } catch (syncError) {
        console.error('Failed to sync workflows from n8n:', syncError);
        // Continue with empty result but log the error
      }
    }

    res.json(result.rows);
  } catch (error) {
    console.error(error);
    res.status(500).json({ error: 'Failed to fetch workflows' });
  }
});

// POST /api/workflows: Create new workflow (save to DB and n8n)
router.post('/workflows', async (req, res) => {
  const { name, description, workflow_data } = req.body;
  try {
    // Prepare correct payload for n8n API
    const n8nPayload = {
      name: name,
      nodes: workflow_data?.nodes || [],
      connections: workflow_data?.connections || {},
      settings: workflow_data?.settings || { executionOrder: 'v1' }
    };
    console.log('🔍 DEBUG: Prepared n8n payload:', JSON.stringify(n8nPayload, null, 2));

    const n8nWorkflow = await n8nService.createWorkflow(n8nPayload);
    const result = await pool.query(
      'INSERT INTO workflows (name, description, status, n8n_id, workflow_data) VALUES ($1, $2, $3, $4, $5) RETURNING *',
      [name, description, n8nWorkflow.active ? 'active' : 'inactive', n8nWorkflow.id, JSON.stringify(workflow_data)]
    );
    res.status(201).json(result.rows[0]);
  } catch (error) {
    console.error('❌ Failed to create workflow:', error);
    res.status(500).json({ error: 'Failed to create workflow' });
  }
});

// PUT /api/workflows/:id: Update workflow (update in DB and n8n)
router.put('/workflows/:id', async (req, res) => {
  const { id } = req.params;
  const { name, description, workflow_data } = req.body;
  try {
    const n8nWorkflow = await n8nService.updateWorkflow(id, { name, ...workflow_data });
    const result = await pool.query(
      'UPDATE workflows SET name = $1, description = $2, workflow_data = $3, updated_at = CURRENT_TIMESTAMP WHERE n8n_id = $4 RETURNING *',
      [name, description, JSON.stringify(workflow_data), id]
    );
    if (result.rows.length === 0) {
      return res.status(404).json({ error: 'Workflow not found' });
    }
    res.json(result.rows[0]);
  } catch (error) {
    console.error(error);
    res.status(500).json({ error: 'Failed to update workflow' });
  }
});

// DELETE /api/workflows/:id: Delete workflow (delete from DB and n8n)
router.delete('/workflows/:id', async (req, res) => {
  const { id } = req.params;
  try {
    await n8nService.deleteWorkflow(id);
    await pool.query('DELETE FROM workflows WHERE n8n_id = $1', [id]);
    res.json({ message: 'Workflow deleted' });
  } catch (error) {
    console.error(error);
    res.status(500).json({ error: 'Failed to delete workflow' });
  }
});

// PUT /api/workflows/:id/toggle: Toggle workflow active status
router.put('/workflows/:id/toggle', async (req, res) => {
  const { id } = req.params;
  try {
    const n8nWorkflow = await n8nService.toggleWorkflow(id);
    const status = n8nWorkflow.active ? 'active' : 'inactive';
    const result = await pool.query(
      'UPDATE workflows SET status = $1, updated_at = CURRENT_TIMESTAMP WHERE n8n_id = $2 RETURNING *',
      [status, id]
    );
    if (result.rows.length === 0) {
      return res.status(404).json({ error: 'Workflow not found' });
    }
    res.json(result.rows[0]);
  } catch (error) {
    console.error(error);
    res.status(500).json({ error: 'Failed to toggle workflow' });
  }
});

// POST /api/workflows/:id/execute: Execute workflow and log to executions table
router.post('/workflows/:id/execute', async (req, res) => {
  const { id } = req.params;
  const data = req.body;
  try {
    const execution = await n8nService.executeWorkflow(id, data);
    // Get workflow_id from DB
    const workflowResult = await pool.query('SELECT id FROM workflows WHERE n8n_id = $1', [id]);
    if (workflowResult.rows.length === 0) {
      return res.status(404).json({ error: 'Workflow not found' });
    }
    const workflowId = workflowResult.rows[0].id;
    const execResult = await pool.query(
      'INSERT INTO executions (workflow_id, status, started_at, output) VALUES ($1, $2, $3, $4) RETURNING *',
      [workflowId, execution.status, execution.startedAt, JSON.stringify(execution)]
    );
    res.status(201).json(execResult.rows[0]);
  } catch (error) {
    console.error(error);
    res.status(500).json({ error: 'Failed to execute workflow' });
  }
});

// POST /api/sync: Manually trigger synchronization
router.post('/sync', async (req, res) => {
  try {
    const syncResult = await syncService.syncWorkflowsFromN8n();
    res.json({
      message: 'Synchronization completed',
      syncedCount: syncResult.count,
      workflows: syncResult.syncedWorkflows
    });
  } catch (error) {
    console.error('Failed to sync workflows:', error);
    res.status(500).json({ error: 'Failed to synchronize workflows' });
  }
});

// GET /api/sync/status: Get synchronization status
router.get('/sync/status', async (req, res) => {
  try {
    const status = await syncService.getSyncStatus();
    res.json(status);
  } catch (error) {
    console.error('Failed to get sync status:', error);
    res.status(500).json({ error: 'Failed to get synchronization status' });
  }
});

module.exports = router;