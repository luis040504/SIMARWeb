// scripts/syncWasteCatalog.js
const axios = require('axios');

const CATALOG_API_URL = process.env.CATALOG_API_URL || 'http://simar_catalog_api:8010';
const VEHICULOS_API_URL = process.env.VEHICULOS_API_URL || 'http://simar_vehiculos_api:8003';
const CATALOG_API_KEY = process.env.CATALOG_API_KEY || 'tu-api-key-aqui';

async function syncWasteCatalog() {
    try {
        console.log('🔄 Sincronizando catálogo de residuos...');
        
        // Obtener todos los tipos de residuo del catálogo
        const response = await axios.get(`${CATALOG_API_URL}/api/catalog`, {
            headers: { 'X-Api-Key': CATALOG_API_KEY },
            params: { pageSize: 500 } // Obtener muchos a la vez
        });
        
        const wasteTypes = response.data.items || response.data;
        
        // Enviar al microservicio de vehículos
        await axios.post(`${VEHICULOS_API_URL}/api/vehiculos/sincronizar-catalogo`, {
            wasteTypes: wasteTypes.map(w => ({
                code: w.code,
                name: w.name,
                type: w.type,
                description: w.description
            }))
        });
        
        console.log(`✅ Catálogo sincronizado: ${wasteTypes.length} tipos de residuo`);
    } catch (error) {
        console.error('❌ Error sincronizando catálogo:', error.message);
    }
}

// Ejecutar si se llama directamente
if (require.main === module) {
    syncWasteCatalog();
}

module.exports = syncWasteCatalog;