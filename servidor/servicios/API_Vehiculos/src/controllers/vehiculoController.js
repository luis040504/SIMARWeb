const Vehiculo = require('../models/Vehiculo');
const multer = require('multer');

// Configurar multer para recibir la foto en memoria (como buffer)
const storage = multer.memoryStorage();
const upload = multer({ 
    storage: storage,
    limits: { fileSize: 20 * 1024 * 1024 }, // 5MB max
    fileFilter: (req, file, cb) => {
        if (file.mimetype.startsWith('image/')) {
            cb(null, true);
        } else {
            cb(new Error('Solo se permiten imágenes'), false);
        }
    }
});

const vehiculoController = {
    
    // Middleware para upload de foto
    uploadFoto: upload.single('foto'),

    async getAll(req, res) {
        try {
            const { search } = req.query;
            let vehiculos;
            
            if (search && search.trim() !== '') {
                vehiculos = await Vehiculo.search(search);
            } else {
                vehiculos = await Vehiculo.findAll();
            }
            
            // Convertir foto a base64 para enviar al cliente
            const vehiculosConFoto = vehiculos.map(v => ({
                ...v,
                foto: v.foto ? v.foto.toString('base64') : null,
                fotoContentType: v.foto ? 'image/jpeg' : null
            }));
            
            res.json({
                success: true,
                data: vehiculosConFoto,
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
            
            if (isNaN(id) || id <= 0) {
                return res.status(400).json({ success: false, message: 'ID inválido' });
            }
            
            const vehiculo = await Vehiculo.findById(id);
            
            if (!vehiculo) {
                return res.status(404).json({ success: false, message: 'Vehículo no encontrado' });
            }
            
            // Convertir foto a base64 si existe
            if (vehiculo.foto) {
                vehiculo.foto = vehiculo.foto.toString('base64');
                vehiculo.fotoContentType = 'image/jpeg';
            }
            
            res.json({ success: true, data: vehiculo });
        } catch (error) {
            console.error('Error en getById:', error);
            res.status(500).json({ success: false, message: 'Error interno del servidor' });
        }
    },

    // Endpoint para sincronizar catálogo de residuos (llamado por el API de catálogo o job)
    async sincronizarCatalogo(req, res) {
        try {
            const { wasteTypes } = req.body;
            
            if (!wasteTypes || !Array.isArray(wasteTypes)) {
                return res.status(400).json({ success: false, message: 'Se requiere un array de tipos de residuo' });
            }
            
            await Vehiculo.sincronizarCatalogoResiduos(wasteTypes);
            
            res.json({ success: true, message: 'Catálogo sincronizado correctamente' });
        } catch (error) {
            console.error('Error en sincronizarCatalogo:', error);
            res.status(500).json({ success: false, message: 'Error al sincronizar catálogo' });
        }
    },

    async getTiposResiduoDisponibles(req, res) {
        try {
            const tipos = await Vehiculo.getTiposResiduoDisponibles();
            res.json({ success: true, data: tipos });
        } catch (error) {
            console.error('Error en getTiposResiduoDisponibles:', error);
            res.status(500).json({ success: false, message: 'Error al obtener tipos de residuo' });
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
            // Procesar la foto (req.file viene de multer)
            const fotoBuffer = req.file ? req.file.buffer : null;
            
            // Parsear tipos_residuo_ids si viene como string JSON
            let tiposResiduoIds = req.body.tipos_residuo_ids;
            if (typeof tiposResiduoIds === 'string') {
                tiposResiduoIds = JSON.parse(tiposResiduoIds);
            }
            
            const vehiculoData = {
                ...req.body,
                tipos_residuo_ids: tiposResiduoIds,
                foto: fotoBuffer
            };
            
            const vehiculo = await Vehiculo.create(vehiculoData);
            
            // Convertir foto a base64 para respuesta
            if (vehiculo.foto) {
                vehiculo.foto = vehiculo.foto.toString('base64');
            }
            
            res.status(201).json({ success: true, data: vehiculo });
        } catch (error) {
            console.error('Error en create:', error);
            
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
            
            res.status(500).json({ success: false, message: 'Error al crear el vehículo' });
        }
    },

    async update(req, res) {
    try {
        const { id } = req.params;
        
        console.log('=== UPDATE REQUEST ===');
        console.log('Body recibido:', req.body);
        console.log('File:', req.file ? `Presente - ${req.file.originalname} (${req.file.size} bytes)` : 'No');
        
        if (isNaN(id) || id <= 0) {
            return res.status(400).json({ success: false, message: 'ID inválido' });
        }
        
        // Procesar la foto
        const fotoBuffer = req.file ? req.file.buffer : undefined;
        
        // Procesar tipos de residuo - IMPORTANTE: buscar en ambos formatos
        let tiposResiduoIds = req.body.TiposResiduoIds || req.body.tipos_residuo_ids || req.body.tiposResiduoIds;
        
        if (typeof tiposResiduoIds === 'string') {
            // Si es un string, puede ser JSON o valores separados por comas
            if (tiposResiduoIds.startsWith('[')) {
                try {
                    tiposResiduoIds = JSON.parse(tiposResiduoIds);
                } catch (e) {
                    tiposResiduoIds = tiposResiduoIds.split(',').map(Number).filter(n => !isNaN(n));
                }
            } else {
                tiposResiduoIds = [parseInt(tiposResiduoIds)].filter(n => !isNaN(n));
            }
        }
        
        if (!Array.isArray(tiposResiduoIds)) {
            tiposResiduoIds = [];
        }
        
        // Mapear campos - aceptar tanto mayúsculas como minúsculas
        const vehiculoData = {
            numero_economico: req.body.NumeroEconomico || req.body.numero_economico || null,
            marca: req.body.Marca || req.body.marca,
            modelo: req.body.Modelo || req.body.modelo,
            anio: req.body.Anio || req.body.anio || null,
            color: req.body.Color || req.body.color || null,
            placas: req.body.Placas || req.body.placas,
            peso_toneladas: req.body.PesoToneladas || req.body.peso_toneladas || null,
            licencia_requerida: req.body.LicenciaRequerida || req.body.licencia_requerida,
            tipo_gasolina: req.body.TipoGasolina || req.body.tipo_gasolina,
            descripcion: req.body.Descripcion || req.body.descripcion || null,
            tipos_residuo_ids: tiposResiduoIds
        };
        
        // Convertir a números donde sea necesario
        if (vehiculoData.anio) vehiculoData.anio = parseInt(vehiculoData.anio);
        if (vehiculoData.peso_toneladas) vehiculoData.peso_toneladas = parseFloat(vehiculoData.peso_toneladas);
        
        if (fotoBuffer !== undefined) {
            vehiculoData.foto = fotoBuffer;
        }
        
        console.log('Datos procesados para update:', {
            ...vehiculoData,
            foto: vehiculoData.foto ? `Buffer de ${vehiculoData.foto.length} bytes` : 'Sin foto'
        });
        
        // Validar campos requeridos
        if (!vehiculoData.marca || !vehiculoData.modelo || !vehiculoData.placas || 
            !vehiculoData.licencia_requerida || !vehiculoData.tipo_gasolina) {
            console.log('Faltan campos requeridos:', {
                marca: !!vehiculoData.marca,
                modelo: !!vehiculoData.modelo,
                placas: !!vehiculoData.placas,
                licencia_requerida: !!vehiculoData.licencia_requerida,
                tipo_gasolina: !!vehiculoData.tipo_gasolina
            });
            return res.status(400).json({ 
                success: false, 
                message: 'Faltan campos requeridos: Marca, Modelo, Placas, LicenciaRequerida, TipoGasolina' 
            });
        }
        
        const vehiculo = await Vehiculo.update(id, vehiculoData);
        
        if (!vehiculo) {
            return res.status(404).json({ success: false, message: 'Vehículo no encontrado' });
        }
        
        if (vehiculo.foto) {
            vehiculo.foto = vehiculo.foto.toString('base64');
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
        
        res.status(500).json({ 
            success: false, 
            message: 'Error al actualizar el vehículo: ' + error.message
        });
    }
},

    async delete(req, res) {
        try {
            const { id } = req.params;
            
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

module.exports = { vehiculoController, upload };