const express = require('express');
const router = express.Router();
const { vehiculoController, upload } = require('../controllers/vehiculoController');

// === PRIMERO: Rutas específicas SIN parámetros ===
router.get('/tipos-gasolina', vehiculoController.getTiposGasolina);
router.get('/tipos-residuo', vehiculoController.getTiposResiduoDisponibles);

// === SEGUNDO: Ruta de sincronización ===
router.post('/sincronizar-catalogo', vehiculoController.sincronizarCatalogo);

// === TERCERO: Rutas con parámetros ===
router.get('/', vehiculoController.getAll);
router.get('/:id', vehiculoController.getById);

// === CUARTO: Rutas POST/PUT/DELETE ===
// CORRECCIÓN: Usar upload.single('foto') en lugar de solo upload
router.post('/', upload.single('foto'), vehiculoController.create);
router.put('/:id', upload.single('foto'), vehiculoController.update);
router.delete('/:id', vehiculoController.delete);

module.exports = router;