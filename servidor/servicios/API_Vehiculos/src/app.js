const express = require('express');
const cors = require('cors');
const helmet = require('helmet');
const rateLimit = require('express-rate-limit');
require('dotenv').config();

const vehiculoRoutes = require('./routes/vehiculoRoutes');
const errorHandler = require('./middleware/errorHandler');
const { testConnection } = require('./config/database');

const app = express();

// Middlewares de seguridad
app.use(helmet());
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// Rate limiting
const limiter = rateLimit({
    windowMs: 15 * 60 * 1000,
    max: 100,
    message: 'Demasiadas solicitudes desde esta IP'
});
app.use('/api/vehiculos', limiter);

// Rutas
app.use('/api/vehiculos', vehiculoRoutes);

// Health check mejorado
app.get('/health', async (req, res) => {
    const dbConnected = await testConnection();
    res.status(200).json({ 
        status: dbConnected ? 'healthy' : 'degraded',
        service: 'vehiculos-api',
        database: dbConnected ? 'connected' : 'disconnected',
        version: '1.0.0'
    });
});

// Manejador de errores
app.use(errorHandler);

module.exports = app;