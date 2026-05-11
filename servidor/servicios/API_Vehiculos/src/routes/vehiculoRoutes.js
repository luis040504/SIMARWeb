const express = require('express');
const router = express.Router();
const vehiculoController = require('../controllers/vehiculoController');

// Obtener todos los vehículos (soporta búsqueda con ?search=...)
router.get('/', vehiculoController.getAll);

// Obtener catálogos
router.get('/tipos-desecho', vehiculoController.getTiposDesecho);
router.get('/tipos-gasolina', vehiculoController.getTiposGasolina);

// Obtener vehículo por ID
router.get('/:id', vehiculoController.getById);

// Crear nuevo vehículo
router.post('/', vehiculoController.create);

// Actualizar vehículo
router.put('/:id', vehiculoController.update);

// Eliminar vehículo (soft delete)
router.delete('/:id', vehiculoController.delete);

module.exports = router;