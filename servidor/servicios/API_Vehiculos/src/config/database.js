const mysql = require('mysql2');
require('dotenv').config();


const pool = mysql.createPool({
    host: process.env.DB_HOST || 'mysql_vehiculos',
    port: process.env.DB_PORT || 3306,
    user: process.env.DB_USER || 'root',
    password: process.env.DB_PASSWORD || 'Simar123!',
    database: process.env.DB_NAME || 'simar_vehiculos_db',
    waitForConnections: true,
    connectionLimit: 10,
    queueLimit: 0,
    
    enableKeepAlive: true,
    keepAliveInitialDelay: 0
});

const promisePool = pool.promise();


const testConnection = async () => {
    try {
        const connection = await promisePool.getConnection();
        console.log(' MySQL conectado correctamente');
        connection.release();
        return true;
    } catch (error) {
        console.error(' Error conectando a MySQL:', error.message);
        return false;
    }
};

module.exports = { promisePool, testConnection };