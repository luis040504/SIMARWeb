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
                Manifiesto: {
                    type: 'object',
                    properties: {
                        id:             { type: 'integer', example: 1 },
                        folio:          { type: 'string', example: 'MAN-2024-0001' },
                        fecha:          { type: 'string', format: 'date', example: '2024-05-10' },
                        estado:         { type: 'string', enum: ['borrador', 'generado', 'firmado', 'cancelado'] },
                        generadorNombre:{ type: 'string', example: 'Empresa Generadora S.A.' },
                        generadorRfc:   { type: 'string', example: 'EGE010101AAA' },
                        transportistaId:{ type: 'integer', example: 3 },
                        destinoFinal:   { type: 'string', example: 'CICOPLAFEST S.A.' },
                        residuos:       { type: 'string', example: 'RP-ACE-001,RP-BAT-001' },
                        cantidadTotal:  { type: 'number', example: 250.5 },
                        unidad:         { type: 'string', example: 'kg' },
                        observaciones:  { type: 'string', nullable: true },
                        pdfFirmaUrl:    { type: 'string', nullable: true },
                        createdAt:      { type: 'string', format: 'date-time' },
                        updatedAt:      { type: 'string', format: 'date-time' },
                    }
                },
                Error: {
                    type: 'object',
                    properties: {
                        error: { type: 'string' },
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
                        { in: 'query', name: 'estado', schema: { type: 'string' }, description: 'Filtrar por estado' },
                        { in: 'query', name: 'page', schema: { type: 'integer', default: 1 } },
                        { in: 'query', name: 'limit', schema: { type: 'integer', default: 20 } },
                    ],
                    responses: {
                        200: { description: 'Lista de manifiestos', content: { 'application/json': { schema: { type: 'array', items: { $ref: '#/components/schemas/Manifiesto' } } } } }
                    }
                },
                post: {
                    summary: 'Crear manifiesto',
                    tags: ['Manifiestos'],
                    requestBody: { required: true, content: { 'application/json': { schema: { $ref: '#/components/schemas/Manifiesto' } } } },
                    responses: {
                        201: { description: 'Manifiesto creado' },
                        400: { description: 'Datos inválidos', content: { 'application/json': { schema: { $ref: '#/components/schemas/Error' } } } }
                    }
                }
            },
            '/{id}': {
                get: {
                    summary: 'Obtener manifiesto por ID',
                    tags: ['Manifiestos'],
                    parameters: [{ in: 'path', name: 'id', required: true, schema: { type: 'integer' } }],
                    responses: {
                        200: { description: 'Manifiesto encontrado', content: { 'application/json': { schema: { $ref: '#/components/schemas/Manifiesto' } } } },
                        404: { description: 'No encontrado' }
                    }
                },
                put: {
                    summary: 'Actualizar manifiesto',
                    tags: ['Manifiestos'],
                    parameters: [{ in: 'path', name: 'id', required: true, schema: { type: 'integer' } }],
                    requestBody: { required: true, content: { 'application/json': { schema: { $ref: '#/components/schemas/Manifiesto' } } } },
                    responses: { 200: { description: 'Actualizado' }, 404: { description: 'No encontrado' } }
                },
                delete: {
                    summary: 'Eliminar manifiesto',
                    tags: ['Manifiestos'],
                    parameters: [{ in: 'path', name: 'id', required: true, schema: { type: 'integer' } }],
                    responses: { 204: { description: 'Eliminado' }, 404: { description: 'No encontrado' } }
                }
            },
            '/{id}/estado': {
                patch: {
                    summary: 'Actualizar estado del manifiesto',
                    tags: ['Estado / Firma'],
                    parameters: [{ in: 'path', name: 'id', required: true, schema: { type: 'integer' } }],
                    requestBody: { required: true, content: { 'application/json': { schema: { type: 'object', properties: { estado: { type: 'string', enum: ['borrador', 'generado', 'firmado', 'cancelado'] } } } } } },
                    responses: { 200: { description: 'Estado actualizado' }, 404: { description: 'No encontrado' } }
                }
            },
            '/{id}/firma': {
                post: {
                    summary: 'Subir PDF firmado',
                    tags: ['Estado / Firma'],
                    parameters: [{ in: 'path', name: 'id', required: true, schema: { type: 'integer' } }],
                    requestBody: { required: true, content: { 'multipart/form-data': { schema: { type: 'object', properties: { pdf: { type: 'string', format: 'binary' } } } } } },
                    responses: { 200: { description: 'PDF guardado' }, 400: { description: 'Solo se permiten archivos PDF' } }
                },
                get: {
                    summary: 'Obtener PDF firmado',
                    tags: ['Estado / Firma'],
                    parameters: [{ in: 'path', name: 'id', required: true, schema: { type: 'integer' } }],
                    responses: { 200: { description: 'Archivo PDF', content: { 'application/pdf': {} } }, 404: { description: 'No encontrado' } }
                }
            }
        }
    },
    apis: []
};

module.exports = swaggerJsdoc(options);
