const express = require('express');
const cors = require('cors');
const helmet = require('helmet');
const rateLimit = require('express-rate-limit');
require('dotenv').config();

const vehiculoRoutes = require('./routes/vehiculoRoutes');
const errorHandler = require('./middlewares/errorHandler');

const app = express();

// Middlewares de seguridad
app.use(helmet());
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// Rate limiting
const limiter = rateLimit({
    windowMs: 15 * 60 * 1000, // 15 minutos
    max: 100, // 100 solicitudes por ventana
    message: 'Demasiadas solicitudes desde esta IP'
});
app.use('/api/vehiculos', limiter);

// Rutas
app.use('/api/vehiculos', vehiculoRoutes);

// Health check
app.get('/health', (req, res) => {
    res.status(200).json({ status: 'OK', service: 'vehiculos-api' });
});

// Manejador de errores
app.use(errorHandler);

module.exports = app;