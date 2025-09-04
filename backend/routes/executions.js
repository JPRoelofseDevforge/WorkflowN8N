const express = require('express');
const router = express.Router();
const { pool } = require('../db');

// GET /api/executions: Fetch execution history
router.get('/executions', async (req, res) => {
  try {
    const result = await pool.query('SELECT * FROM executions ORDER BY started_at DESC');
    res.json(result.rows);
  } catch (error) {
    console.error(error);
    res.status(500).json({ error: 'Failed to fetch executions' });
  }
});

// GET /api/executions/:workflowId: Fetch executions for specific workflow
router.get('/executions/:workflowId', async (req, res) => {
  const { workflowId } = req.params;
  try {
    const result = await pool.query('SELECT * FROM executions WHERE workflow_id = $1 ORDER BY started_at DESC', [workflowId]);
    res.json(result.rows);
  } catch (error) {
    console.error(error);
    res.status(500).json({ error: 'Failed to fetch executions for workflow' });
  }
});

module.exports = router;