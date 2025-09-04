const express = require('express');
const cors = require('cors');
require('dotenv').config({ path: './backend/.env' });
const { testConnection } = require('./db');
const { initDb } = require('./initDb');
const syncService = require('./services/syncService');
const workflowRouter = require('./routes/workflows');
const executionRouter = require('./routes/executions');

const app = express();
const port = process.env.PORT || 3001;

app.use(cors({
  origin: ['http://localhost:5173', 'http://localhost:5174']
}));

app.use(express.json());

app.use('/api', workflowRouter);
app.use('/api', executionRouter);

app.get('/health', (req, res) => {
  res.json({ status: 'OK' });
});

app.get('/api/health/db', async (req, res) => {
  const connected = await testConnection();
  res.json({ status: connected ? 'OK' : 'ERROR' });
});

app.post('/api/init-db', async (req, res) => {
  const success = await initDb();
  res.json({ success });
});

// Error handling middleware
app.use((err, req, res, next) => {
  console.error(err.stack);
  res.status(500).send('Something broke!');
});

app.listen(port, async () => {
  console.log(`Server running on port ${port}`);
  await testConnection();
  await initDb();

  // Optional initial sync on startup
  if (process.env.INITIAL_SYNC === 'true') {
    console.log('Performing initial synchronization with n8n...');
    try {
      const syncResult = await syncService.syncWorkflowsFromN8n();
      console.log(`Initial sync completed: ${syncResult.count} workflows synced`);
    } catch (error) {
      console.error('Initial sync failed:', error);
    }
  }
});