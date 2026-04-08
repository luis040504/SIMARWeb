const Vehiculo = require('../models/Vehiculo');

const vehiculoController = {
    
    async getAll(req, res) {
        try {
            const { search } = req.query;
            let vehiculos;
            
            if (search) {
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
            res.status(500).json({ success: false, message: error.message });
        }
    },

    
    async getById(req, res) {
        try {
            const { id } = req.params;
            const vehiculo = await Vehiculo.findById(id);
            
            if (!vehiculo) {
                return res.status(404).json({ success: false, message: 'Vehículo no encontrado' });
            }
            
            res.json({ success: true, data: vehiculo });
        } catch (error) {
            res.status(500).json({ success: false, message: error.message });
        }
    },

    
    async getTiposDesecho(req, res) {
        try {
            const tipos = await Vehiculo.getTiposDesecho();
            res.json({ success: true, data: tipos });
        } catch (error) {
            res.status(500).json({ success: false, message: error.message });
        }
    },

    
    async getTiposGasolina(req, res) {
        try {
            const tipos = await Vehiculo.getTiposGasolina();
            res.json({ success: true, data: tipos });
        } catch (error) {
            res.status(500).json({ success: false, message: error.message });
        }
    },

   
    async create(req, res) {
        try {
            const vehiculo = await Vehiculo.create(req.body);
            res.status(201).json({ success: true, data: vehiculo });
        } catch (error) {
            res.status(500).json({ success: false, message: error.message });
        }
    },

   
    async update(req, res) {
        try {
            const { id } = req.params;
            const vehiculo = await Vehiculo.update(id, req.body);
            
            if (!vehiculo) {
                return res.status(404).json({ success: false, message: 'Vehículo no encontrado' });
            }
            
            res.json({ success: true, data: vehiculo });
        } catch (error) {
            res.status(500).json({ success: false, message: error.message });
        }
    },

    
    async delete(req, res) {
        try {
            const { id } = req.params;
            const deleted = await Vehiculo.delete(id);
            
            if (!deleted) {
                return res.status(404).json({ success: false, message: 'Vehículo no encontrado' });
            }
            
            res.json({ success: true, message: 'Vehículo eliminado correctamente' });
        } catch (error) {
            res.status(500).json({ success: false, message: error.message });
        }
    }
};

module.exports = vehiculoController;