const fs = require('fs');
const path = require('path');
const { pool } = require('./db');

async function initDb() {
  try {
    const schemaPath = path.join(__dirname, 'schema.sql');
    const schemaSQL = fs.readFileSync(schemaPath, 'utf8');

    await pool.query(schemaSQL);
    console.log('Database schema initialized successfully');
    return true;
  } catch (err) {
    console.error('Failed to initialize database schema:', err.message);
    return false;
  }
}

module.exports = { initDb };