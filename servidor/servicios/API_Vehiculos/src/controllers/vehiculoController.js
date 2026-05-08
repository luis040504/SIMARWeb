const Vehiculo = require('../models/Vehiculo');

const vehiculoController = {
    
    async getAll(req, res) {
        try {
            const { search } = req.query;
            let vehiculos;
            
            if (search && search.trim() !== '') {
                vehiculos = await Vehiculo.search(search);
            } else {
                vehiculos = await Vehiculo.findAll();
            }
            
            res.json({
                success: true,
                data: vehiculos,
                count: vehiculos.length
            });
        } catch (error) {
            console.error('Error en getAll:', error);
            res.status(500).json({ success: false, message: 'Error interno del servidor' });
        }
    },

    async getById(req, res) {
        try {
            const { id } = req.params;
            
            // Validar que id sea número
            if (isNaN(id) || id <= 0) {
                return res.status(400).json({ success: false, message: 'ID inválido' });
            }
            
            const vehiculo = await Vehiculo.findById(id);
            
            if (!vehiculo) {
                return res.status(404).json({ success: false, message: 'Vehículo no encontrado' });
            }
            
            res.json({ success: true, data: vehiculo });
        } catch (error) {
            console.error('Error en getById:', error);
            res.status(500).json({ success: false, message: 'Error interno del servidor' });
        }
    },

    async getTiposDesecho(req, res) {
        try {
            const tipos = await Vehiculo.getTiposDesecho();
            res.json({ success: true, data: tipos });
        } catch (error) {
            console.error('Error en getTiposDesecho:', error);
            res.status(500).json({ success: false, message: 'Error al obtener tipos de desecho' });
        }
    },

    async getTiposGasolina(req, res) {
        try {
            const tipos = await Vehiculo.getTiposGasolina();
            res.json({ success: true, data: tipos });
        } catch (error) {
            console.error('Error en getTiposGasolina:', error);
            res.status(500).json({ success: false, message: 'Error al obtener tipos de gasolina' });
        }
    },

    async create(req, res) {
        try {
            const vehiculo = await Vehiculo.create(req.body);
            res.status(201).json({ success: true, data: vehiculo });
        } catch (error) {
            console.error('Error en create:', error);
            
            // Manejo específico de errores de unicidad
            if (error.message === 'PLACAS_DUPLICADAS') {
                return res.status(409).json({ 
                    success: false, 
                    message: 'Las placas ya están registradas en otro vehículo' 
                });
            }
            
            if (error.message === 'NUMERO_ECONOMICO_DUPLICADO') {
                return res.status(409).json({ 
                    success: false, 
                    message: 'El número económico ya está registrado en otro vehículo' 
                });
            }
            
            // Error genérico de base de datos
            res.status(500).json({ success: false, message: 'Error al crear el vehículo' });
        }
    },

    async update(req, res) {
        try {
            const { id } = req.params;
            
            // Validar que id sea número
            if (isNaN(id) || id <= 0) {
                return res.status(400).json({ success: false, message: 'ID inválido' });
            }
            
            const vehiculo = await Vehiculo.update(id, req.body);
            
            if (!vehiculo) {
                return res.status(404).json({ success: false, message: 'Vehículo no encontrado' });
            }
            
            res.json({ success: true, data: vehiculo });
        } catch (error) {
            console.error('Error en update:', error);
            
            if (error.message === 'PLACAS_DUPLICADAS') {
                return res.status(409).json({ 
                    success: false, 
                    message: 'Las placas ya están registradas en otro vehículo' 
                });
            }
            
            if (error.message === 'NUMERO_ECONOMICO_DUPLICADO') {
                return res.status(409).json({ 
                    success: false, 
                    message: 'El número económico ya está registrado en otro vehículo' 
                });
            }
            
            res.status(500).json({ success: false, message: 'Error al actualizar el vehículo' });
        }
    },

    async delete(req, res) {
        try {
            const { id } = req.params;
            
            // Validar que id sea número
            if (isNaN(id) || id <= 0) {
                return res.status(400).json({ success: false, message: 'ID inválido' });
            }
            
            const deleted = await Vehiculo.delete(id);
            
            if (!deleted) {
                return res.status(404).json({ success: false, message: 'Vehículo no encontrado' });
            }
            
            res.json({ success: true, message: 'Vehículo eliminado correctamente' });
        } catch (error) {
            console.error('Error en delete:', error);
            res.status(500).json({ success: false, message: 'Error al eliminar el vehículo' });
        }
    }
};

module.exports = vehiculoController;