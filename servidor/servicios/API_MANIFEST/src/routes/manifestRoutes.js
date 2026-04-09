const express = require('express');
const router = express.Router();
const path = require('path');
const multer = require('multer');
const manifestController = require('../controllers/manifestController');

// ─── Configuración Multer para PDFs ──────────────────────────────────────────
const storage = multer.diskStorage({
    destination: (req, file, cb) => {
        cb(null, path.join(__dirname, '..', '..', 'uploads', 'manifiestos'));
    },
    filename: (req, file, cb) => {
        const id = req.params.id;
        const timestamp = Date.now();
        cb(null, `manifiesto_${id}_firmado_${timestamp}.pdf`);
    }
});

const fileFilter = (req, file, cb) => {
    if (file.mimetype === 'application/pdf') {
        cb(null, true);
    } else {
        cb(new Error('Solo se permiten archivos PDF'), false);
    }
};

const upload = multer({
    storage,
    fileFilter,
    limits: { fileSize: 10 * 1024 * 1024 } // 10 MB máximo
});

// ─── Rutas ────────────────────────────────────────────────────────────────────

// CRUD principal
router.get('/',    manifestController.getAll);
router.get('/:id', manifestController.getById);
router.post('/',   manifestController.create);
router.put('/:id', manifestController.update);
router.delete('/:id', manifestController.delete);

// Estado / firma
router.patch('/:id/estado', manifestController.updateStatus);
router.post('/:id/firma',   upload.single('pdf'), manifestController.uploadFirma);
router.get('/:id/firma',    manifestController.getFirma);

module.exports = router;
