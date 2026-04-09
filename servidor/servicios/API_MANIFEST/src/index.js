const app = require('./app');
const { testConnection } = require('./config/database');

const PORT = process.env.PORT || 8007;

const startServer = async () => {
    const dbConnected = await testConnection();

    if (!dbConnected) {
        console.error('No se pudo conectar a la base de datos. Saliendo...');
        process.exit(1);
    }

    app.listen(PORT, () => {
        console.log(`Manifiestos API corriendo en puerto ${PORT}`);
        console.log(`Health check: http://localhost:${PORT}/health`);
    });
};

startServer();
