const errorHandler = (err, req, res, next) => {
    console.error(err.stack);

    // Error de multer (tipo de archivo inválido)
    if (err.code === 'LIMIT_FILE_SIZE') {
        return res.status(400).json({ success: false, message: 'El archivo excede el límite de 10 MB' });
    }
    if (err.message === 'Solo se permiten archivos PDF') {
        return res.status(400).json({ success: false, message: err.message });
    }

    res.status(err.status || 500).json({
        success: false,
        message: err.status ? err.message : 'Error interno del servidor'
    });
};

module.exports = errorHandler;
