const express = require('express');
const cors = require('cors');
const helmet = require('helmet');
const rateLimit = require('express-rate-limit');
const path = require('path');
require('dotenv').config();

const manifestRoutes = require('./routes/manifestRoutes');
const errorHandler = require('./middleware/errorHandler');

const app = express();

// Middlewares de seguridad
app.use(helmet({ crossOriginResourcePolicy: { policy: 'cross-origin' } }));
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// Servir PDFs firmados como archivos estáticos (cross-origin permitido)
app.use('/uploads', express.static(path.join(__dirname, '..', 'uploads')));

// Rate limiting
const limiter = rateLimit({
    windowMs: 15 * 60 * 1000,
    max: 200,
    message: 'Demasiadas solicitudes desde esta IP'
});
app.use('/api/manifiestos', limiter);

// Rutas
app.use('/api/manifiestos', manifestRoutes);

// Health check
app.get('/health', (req, res) => {
    res.status(200).json({ status: 'OK', service: 'manifiestos-api' });
});

// Manejador de errores
app.use(errorHandler);

module.exports = app;
