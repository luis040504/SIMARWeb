const swaggerJsdoc = require('swagger-jsdoc');

const options = {
    definition: {
        openapi: '3.0.0',
        info: {
            title: 'SIMAR — Manifiestos de Residuos',
            version: '1.0.0',
            description: 'API para gestión de manifiestos de residuos peligrosos y de manejo especial.',
        },
        servers: [{ url: '/api/manifiestos', description: 'Servidor local' }],
        components: {
            schemas: {
                ResiduoPeligroso: {
                    type: 'object',
                    properties: {
                        nombre_residuo:    { type: 'string', example: 'Aceites gastados' },
                        es_corrosivo:      { type: 'boolean', example: false },
                        es_reactivo:       { type: 'boolean', example: false },
                        es_explosivo:      { type: 'boolean', example: false },
                        es_toxico:         { type: 'boolean', example: true },
                        es_inflamable:     { type: 'boolean', example: false },
                        es_biologico:      { type: 'boolean', example: false },
                        es_mutagenico:     { type: 'boolean', example: false },
                        tipo_envase:       { type: 'string', example: 'Tambor metálico' },
                        capacidad_envase:  { type: 'number', example: 200 },
                        cantidad_kg:       { type: 'number', example: 150.5 },
                        tiene_etiqueta:    { type: 'boolean', nullable: true, example: true }
                    }
                },
                ResiduoEspecial: {
                    type: 'object',
                    properties: {
                        clave_residuo:  { type: 'string', example: 'RME-CAR-001' },
                        nombre_residuo: { type: 'string', example: 'Cartón y papel' },
                        tipo_envase:    { type: 'string', example: 'Caja' },
                        capacidad:      { type: 'number', example: 50 },
                        peso:           { type: 'number', example: 30 },
                        unidad:         { type: 'string', example: 'kg' }
                    }
                },
                ManifiestoInput: {
                    type: 'object',
                    required: ['id_cliente', 'tipo'],
                    properties: {
                        id_cliente:   { type: 'integer', example: 1 },
                        tipo:         { type: 'string', enum: ['peligroso', 'especial'], example: 'peligroso' },
                        estado:       { type: 'string', enum: ['borrador', 'en_transito', 'completado'], example: 'borrador' },
                        // Generador
                        numero_registro_ambiental:   { type: 'string', example: 'SEMARNAT-2024-001' },
                        razon_social:                { type: 'string', example: 'Empresa Generadora S.A. de C.V.' },
                        domicilio:                   { type: 'string', example: 'Av. Industria 100' },
                        calle:                       { type: 'string', example: 'Av. Industria' },
                        numero_exterior:             { type: 'string', example: '100' },
                        numero_interior:             { type: 'string', example: 'A' },
                        colonia:                     { type: 'string', example: 'Zona Industrial' },
                        estado_generador:            { type: 'string', example: 'Jalisco' },
                        codigo_postal:               { type: 'string', example: '44100' },
                        municipio:                   { type: 'string', example: 'Guadalajara' },
                        telefono:                    { type: 'string', example: '3312345678' },
                        correo:                      { type: 'string', example: 'contacto@empresa.com' },
                        fecha_manifiesto:            { type: 'string', format: 'date', example: '2026-04-27' },
                        hora_manifiesto:             { type: 'string', example: '09:00' },
                        observaciones_generador:     { type: 'string', nullable: true },
                        instrucciones_manejo_seguro: { type: 'string', nullable: true },
                        nombre_responsable_generador:{ type: 'string', example: 'Juan Pérez' },
                        fecha_firma_generador:       { type: 'string', format: 'date', nullable: true, example: '2026-04-27' },
                        // Transportista
                        numero_autorizacion_transportista: { type: 'string', example: 'SCT-2024-001' },
                        numero_permiso_sct:                { type: 'string', example: 'TP-01234' },
                        razon_social_transportista:        { type: 'string', example: 'Transportes Seguros S.A.' },
                        domicilio_transportista:           { type: 'string', example: 'Calle Logística 50' },
                        calle_transportista:               { type: 'string', example: 'Calle Logística' },
                        numero_exterior_transportista:     { type: 'string', example: '50' },
                        numero_interior_transportista:     { type: 'string', nullable: true },
                        colonia_transportista:             { type: 'string', example: 'Parque Industrial' },
                        estado_transportista:              { type: 'string', example: 'Jalisco' },
                        codigo_postal_transportista:       { type: 'string', example: '45130' },
                        municipio_transportista:           { type: 'string', example: 'Tlajomulco' },
                        telefono_transportista:            { type: 'string', example: '3398765432' },
                        correo_transportista:              { type: 'string', example: 'ops@transportes.com' },
                        tipo_vehiculo:                     { type: 'string', example: 'Camión de carga' },
                        placa:                             { type: 'string', example: 'JAL-1234-B' },
                        licencia_conductor:                { type: 'string', example: 'JALISCO-LIC-001' },
                        ruta_transporte:                   { type: 'string', example: 'Guadalajara - Monterrey' },
                        observaciones_transportista:       { type: 'string', nullable: true },
                        nombre_responsable_transportista:  { type: 'string', example: 'Carlos López' },
                        fecha_recepcion_transportista:     { type: 'string', format: 'date', nullable: true, example: '2026-04-27' },
                        hora_recepcion_transportista:      { type: 'string', nullable: true, example: '12:15' },
                        fecha_firma_transportista:         { type: 'string', format: 'date', nullable: true, example: '2026-04-27' },
                        // Destinatario
                        numero_autorizacion_destinatario:  { type: 'string', example: 'SEMARNAT-DEST-001' },
                        razon_social_destinatario:         { type: 'string', example: 'CICOPLAFEST S.A.' },
                        domicilio_destinatario:            { type: 'string', example: 'Blvd. Reciclaje 200' },
                        calle_destinatario:                { type: 'string', example: 'Blvd. Reciclaje' },
                        numero_exterior_destinatario:      { type: 'string', example: '200' },
                        numero_interior_destinatario:      { type: 'string', nullable: true },
                        colonia_destinatario:              { type: 'string', example: 'Industrial Norte' },
                        estado_destinatario:               { type: 'string', example: 'Nuevo León' },
                        codigo_postal_destinatario:        { type: 'string', example: '64000' },
                        municipio_destinatario:            { type: 'string', example: 'Monterrey' },
                        telefono_destinatario:             { type: 'string', example: '8112345678' },
                        correo_destinatario:               { type: 'string', example: 'recepcion@cicoplafest.com' },
                        tipo_disposicion:                  { type: 'string', example: 'Reciclaje' },
                        fecha_destinatario:                { type: 'string', format: 'date', nullable: true, example: '2026-04-27' },
                        hora_destinatario:                 { type: 'string', nullable: true, example: '14:30' },
                        persona_recibe:                    { type: 'string', nullable: true, example: 'María González' },
                        fecha_firma_destinatario:          { type: 'string', format: 'date', nullable: true, example: '2026-04-27' },
                        nombre_responsable_destinatario:   { type: 'string', nullable: true },
                        observaciones_destinatario:        { type: 'string', nullable: true },
                        // Residuos
                        residuos_peligrosos: {
                            type: 'array',
                            items: { $ref: '#/components/schemas/ResiduoPeligroso' }
                        },
                        residuos_especiales: {
                            type: 'array',
                            items: { $ref: '#/components/schemas/ResiduoEspecial' }
                        }
                    }
                },
                Error: {
                    type: 'object',
                    properties: {
                        success: { type: 'boolean', example: false },
                        message: { type: 'string' }
                    }
                }
            }
        },
        paths: {
            '/': {
                get: {
                    summary: 'Listar manifiestos',
                    tags: ['Manifiestos'],
                    parameters: [
                        { in: 'query', name: 'numero',       schema: { type: 'string' }, description: 'Filtrar por número de manifiesto' },
                        { in: 'query', name: 'razon_social', schema: { type: 'string' }, description: 'Filtrar por razón social del generador' },
                        { in: 'query', name: 'tipo',         schema: { type: 'string', enum: ['peligroso', 'especial'] } },
                        { in: 'query', name: 'estado',       schema: { type: 'string', enum: ['borrador', 'en_transito', 'completado'] } },
                        { in: 'query', name: 'fecha_desde',  schema: { type: 'string', format: 'date' } },
                        { in: 'query', name: 'fecha_hasta',  schema: { type: 'string', format: 'date' } },
                    ],
                    responses: {
                        200: { description: 'Lista de manifiestos', content: { 'application/json': { schema: { type: 'object', properties: { success: { type: 'boolean' }, count: { type: 'integer' }, data: { type: 'array', items: { $ref: '#/components/schemas/ManifiestoInput' } } } } } } }
                    }
                },
                post: {
                    summary: 'Crear manifiesto',
                    tags: ['Manifiestos'],
                    requestBody: { required: true, content: { 'application/json': { schema: { $ref: '#/components/schemas/ManifiestoInput' } } } },
                    responses: {
                        201: { description: 'Manifiesto creado', content: { 'application/json': { schema: { type: 'object', properties: { success: { type: 'boolean' }, data: { $ref: '#/components/schemas/ManifiestoInput' } } } } } },
                        400: { description: 'Datos inválidos', content: { 'application/json': { schema: { $ref: '#/components/schemas/Error' } } } }
                    }
                }
            },
            '/{id}': {
                get: {
                    summary: 'Obtener manifiesto por ID',
                    tags: ['Manifiestos'],
                    parameters: [{ in: 'path', name: 'id', required: true, schema: { type: 'integer' }, example: 1 }],
                    responses: {
                        200: { description: 'Manifiesto encontrado', content: { 'application/json': { schema: { type: 'object', properties: { success: { type: 'boolean' }, data: { $ref: '#/components/schemas/ManifiestoInput' } } } } } },
                        404: { description: 'No encontrado', content: { 'application/json': { schema: { $ref: '#/components/schemas/Error' } } } }
                    }
                },
                put: {
                    summary: 'Actualizar manifiesto',
                    tags: ['Manifiestos'],
                    parameters: [{ in: 'path', name: 'id', required: true, schema: { type: 'integer' }, example: 1 }],
                    requestBody: { required: true, content: { 'application/json': { schema: { $ref: '#/components/schemas/ManifiestoInput' } } } },
                    responses: {
                        200: { description: 'Manifiesto actualizado', content: { 'application/json': { schema: { type: 'object', properties: { success: { type: 'boolean' }, data: { $ref: '#/components/schemas/ManifiestoInput' } } } } } },
                        404: { description: 'No encontrado', content: { 'application/json': { schema: { $ref: '#/components/schemas/Error' } } } }
                    }
                },
                delete: {
                    summary: 'Eliminar manifiesto (soft delete)',
                    tags: ['Manifiestos'],
                    parameters: [{ in: 'path', name: 'id', required: true, schema: { type: 'integer' }, example: 1 }],
                    responses: {
                        200: { description: 'Manifiesto eliminado', content: { 'application/json': { schema: { type: 'object', properties: { success: { type: 'boolean' }, message: { type: 'string' } } } } } },
                        404: { description: 'No encontrado', content: { 'application/json': { schema: { $ref: '#/components/schemas/Error' } } } }
                    }
                }
            },
            '/{id}/estado': {
                patch: {
                    summary: 'Cambiar estado del manifiesto',
                    tags: ['Estado / Firma'],
                    parameters: [{ in: 'path', name: 'id', required: true, schema: { type: 'integer' }, example: 1 }],
                    requestBody: {
                        required: true,
                        content: {
                            'application/json': {
                                schema: {
                                    type: 'object',
                                    required: ['estado'],
                                    properties: {
                                        estado:      { type: 'string', enum: ['borrador', 'en_transito', 'completado'], example: 'en_transito' },
                                        fecha_firma: { type: 'string', format: 'date', nullable: true }
                                    }
                                }
                            }
                        }
                    },
                    responses: {
                        200: { description: 'Estado actualizado' },
                        400: { description: 'Estado inválido', content: { 'application/json': { schema: { $ref: '#/components/schemas/Error' } } } },
                        404: { description: 'No encontrado', content: { 'application/json': { schema: { $ref: '#/components/schemas/Error' } } } }
                    }
                }
            },
            '/{id}/firma': {
                post: {
                    summary: 'Subir PDF firmado',
                    tags: ['Estado / Firma'],
                    parameters: [{ in: 'path', name: 'id', required: true, schema: { type: 'integer' }, example: 1 }],
                    requestBody: {
                        required: true,
                        content: { 'multipart/form-data': { schema: { type: 'object', required: ['pdf'], properties: { pdf: { type: 'string', format: 'binary', description: 'Archivo PDF firmado (máx. 10 MB)' } } } } }
                    },
                    responses: {
                        200: { description: 'PDF guardado correctamente' },
                        400: { description: 'Solo se permiten archivos PDF', content: { 'application/json': { schema: { $ref: '#/components/schemas/Error' } } } },
                        404: { description: 'Manifiesto no encontrado', content: { 'application/json': { schema: { $ref: '#/components/schemas/Error' } } } }
                    }
                },
                get: {
                    summary: 'Descargar PDF firmado',
                    tags: ['Estado / Firma'],
                    parameters: [{ in: 'path', name: 'id', required: true, schema: { type: 'integer' }, example: 1 }],
                    responses: {
                        200: { description: 'Archivo PDF', content: { 'application/pdf': {} } },
                        404: { description: 'No encontrado', content: { 'application/json': { schema: { $ref: '#/components/schemas/Error' } } } }
                    }
                }
            }
        }
    },
    apis: []
};

module.exports = swaggerJsdoc(options);
