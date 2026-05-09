const express = require('express');
const cors = require('cors');
const helmet = require('helmet');
const rateLimit = require('express-rate-limit');
const swaggerUi = require('swagger-ui-express');
const swaggerSpec = require('./config/swagger');
require('dotenv').config();

const manifestRoutes = require('./routes/manifestRoutes');
const errorHandler = require('./middleware/errorHandler');

const app = express();

// Middlewares de seguridad
app.use(helmet());
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// Rate limiting
const limiter = rateLimit({
    windowMs: 15 * 60 * 1000,
    max: 200,
    message: 'Demasiadas solicitudes desde esta IP'
});
app.use('/api/manifiestos', limiter);

// Swagger UI
app.use('/api/manifiestos/docs', swaggerUi.serve, swaggerUi.setup(swaggerSpec));

// Rutas
app.use('/api/manifiestos', manifestRoutes);

// Health check
app.get('/health', (req, res) => {
    res.status(200).json({ status: 'OK', service: 'manifiestos-api' });
});

// Manejador de errores
app.use(errorHandler);

module.exports = app;
