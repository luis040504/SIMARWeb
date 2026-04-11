const path = require('path');
const fs = require('fs');
const Manifest = require('../models/Manifest');

const manifestController = {

    // GET /api/manifiestos
    async getAll(req, res) {
        try {
            const filters = {
                numero:       req.query.numero,
                razon_social: req.query.razon_social,
                tipo:         req.query.tipo,
                estado:       req.query.estado,
                fecha_desde:  req.query.fecha_desde,
                fecha_hasta:  req.query.fecha_hasta
            };
            const data = await Manifest.findAll(filters);
            res.json({ success: true, data, count: data.length });
        } catch (err) {
            res.status(500).json({ success: false, message: 'Error interno del servidor' });
        }
    },

    // GET /api/manifiestos/:id
    async getById(req, res) {
        try {
            const manifest = await Manifest.findById(req.params.id);
            if (!manifest) return res.status(404).json({ success: false, message: 'Manifiesto no encontrado' });
            res.json({ success: true, data: manifest });
        } catch (err) {
            res.status(500).json({ success: false, message: 'Error interno del servidor' });
        }
    },

    // POST /api/manifiestos
    async create(req, res) {
        try {
            const manifest = await Manifest.create(req.body);
            res.status(201).json({ success: true, data: manifest });
        } catch (err) {
            res.status(500).json({ success: false, message: 'Error interno del servidor' });
        }
    },

    // PUT /api/manifiestos/:id
    async update(req, res) {
        try {
            const manifest = await Manifest.update(req.params.id, req.body);
            if (!manifest) return res.status(404).json({ success: false, message: 'Manifiesto no encontrado' });
            res.json({ success: true, data: manifest });
        } catch (err) {
            res.status(500).json({ success: false, message: 'Error interno del servidor' });
        }
    },

    // PATCH /api/manifiestos/:id/estado
    async updateStatus(req, res) {
        try {
            const { estado, fecha_firma } = req.body;
            const estadosValidos = ['borrador', 'en_transito', 'completado'];

            if (!estadosValidos.includes(estado)) {
                return res.status(400).json({ success: false, message: 'Estado inválido' });
            }

            const manifest = await Manifest.updateStatus(req.params.id, estado, { fecha_firma });
            if (!manifest) return res.status(404).json({ success: false, message: 'Manifiesto no encontrado' });
            res.json({ success: true, data: manifest });
        } catch (err) {
            res.status(500).json({ success: false, message: 'Error interno del servidor' });
        }
    },

    // DELETE /api/manifiestos/:id
    async delete(req, res) {
        try {
            const deleted = await Manifest.delete(req.params.id);
            if (!deleted) return res.status(404).json({ success: false, message: 'Manifiesto no encontrado' });
            res.json({ success: true, message: 'Manifiesto eliminado correctamente' });
        } catch (err) {
            res.status(500).json({ success: false, message: 'Error interno del servidor' });
        }
    },

    // POST /api/manifiestos/:id/firma  — sube el PDF firmado
    async uploadFirma(req, res) {
        try {
            if (!req.file) {
                return res.status(400).json({ success: false, message: 'No se recibió ningún archivo PDF' });
            }

            const manifest = await Manifest.setFirmaFile(req.params.id, req.file.filename);
            if (!manifest) return res.status(404).json({ success: false, message: 'Manifiesto no encontrado' });

            res.json({
                success: true,
                message: 'PDF firmado subido correctamente',
                data: {
                    id: manifest.id,
                    nombre_archivo_firmado: manifest.nombre_archivo_firmado,
                    estado: manifest.estado,
                    url_firma: `/api/manifiestos/${manifest.id}/firma`
                }
            });
        } catch (err) {
            res.status(500).json({ success: false, message: 'Error interno del servidor' });
        }
    },

    // GET /api/manifiestos/:id/firma  — sirve el PDF inline para visualizarlo
    async getFirma(req, res) {
        try {
            const filename = await Manifest.getFirmaFilename(req.params.id);
            if (!filename) {
                return res.status(404).json({ success: false, message: 'No hay PDF firmado para este manifiesto' });
            }

            const filePath = path.join(__dirname, '..', '..', 'uploads', 'manifiestos', filename);

            if (!fs.existsSync(filePath)) {
                return res.status(404).json({ success: false, message: 'Archivo no encontrado en el servidor' });
            }

            res.setHeader('Content-Type', 'application/pdf');
            res.setHeader('Content-Disposition', `inline; filename="${filename}"`);
            res.sendFile(filePath);
        } catch (err) {
            res.status(500).json({ success: false, message: 'Error interno del servidor' });
        }
    }
};

module.exports = manifestController;
