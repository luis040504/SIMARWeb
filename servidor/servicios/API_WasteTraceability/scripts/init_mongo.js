db = db.getSiblingDB('simar_trazabilidad_db');

// Colección: eventos_trazabilidad
db.createCollection('eventos_trazabilidad', {
    validator: {
        $jsonSchema: {
            bsonType: 'object',
            required: ['servicioId', 'tipoEvento', 'fechaEvento', 'usuario'],
            properties: {
                servicioId: { 
                    bsonType: 'string',
                    description: 'ID del servicio relacionado - requerido'
                },
                tipoEvento: { 
                    bsonType: 'string',
                    enum: ['SERVICIO_CREADO', 'RECOLECCION_CONFIRMADA', 'TRANSPORTE_INICIADO', 'LLEGADA_REGISTRADA', 'ESTADO_CAMBIADO'],
                    description: 'Tipo de evento - requerido'
                },
                fechaEvento: { 
                    bsonType: 'date',
                    description: 'Fecha y hora del evento - requerido'
                },
                usuario: { 
                    bsonType: 'string',
                    description: 'Usuario que realizó la acción - requerido'
                },
                estadoAnterior: { 
                    bsonType: 'string',
                    description: 'Estado antes del cambio'
                },
                estadoNuevo: { 
                    bsonType: 'string',
                    description: 'Estado después del cambio'
                },
                ubicacion: { 
                    bsonType: 'object',
                    properties: {
                        latitud: { bsonType: 'double' },
                        longitud: { bsonType: 'double' },
                        direccion: { bsonType: 'string' }
                    }
                },
                observaciones: { 
                    bsonType: 'string',
                    description: 'Observaciones del evento'
                },
                metadata: {
                    bsonType: 'object',
                    description: 'Información adicional del evento'
                },
                activo: { 
                    bsonType: 'bool',
                    description: 'Registro activo'
                },
                createdAt: { 
                    bsonType: 'date',
                    description: 'Fecha de creación'
                }
            }
        }
    }
});

// Índices
db.eventos_trazabilidad.createIndex({ servicioId: 1 });
db.eventos_trazabilidad.createIndex({ fechaEvento: -1 });
db.eventos_trazabilidad.createIndex({ tipoEvento: 1 });
db.eventos_trazabilidad.createIndex({ usuario: 1 });
db.eventos_trazabilidad.createIndex({ servicioId: 1, fechaEvento: -1 });

const now = new Date();
const yesterday = new Date(now);
yesterday.setDate(now.getDate() - 1);
const lastWeek = new Date(now);
lastWeek.setDate(now.getDate() - 7);

const eventos = [
    {
        servicioId: "65f1a2b3c4d5e6f7a8b9c0d1",
        tipoEvento: "SERVICIO_CREADO",
        fechaEvento: lastWeek,
        usuario: "admin@simar.com",
        estadoNuevo: "Asignado",
        observaciones: "Servicio creado para Industrias ABC",
        metadata: {
            cliente: "Industrias ABC",
            contrato: "CON-2024-001"
        },
        activo: true,
        createdAt: lastWeek
    },
    {
        servicioId: "65f1a2b3c4d5e6f7a8b9c0d1",
        tipoEvento: "RECOLECCION_CONFIRMADA",
        fechaEvento: yesterday,
        usuario: "carlos.lopez@simar.com",
        estadoAnterior: "Asignado",
        estadoNuevo: "Recolectado",
        ubicacion: {
            latitud: 19.4326,
            longitud: -99.1332,
            direccion: "Av. Industrial 123, Parque Industrial"
        },
        observaciones: "Recolección completada - 500.5 kg de plásticos",
        metadata: {
            vehiculo: "Kenworth T680 - ABC-123",
            conductor: "Juan Pérez"
        },
        activo: true,
        createdAt: yesterday
    },
    {
        servicioId: "65f1a2b3c4d5e6f7a8b9c0d1",
        tipoEvento: "TRANSPORTE_INICIADO",
        fechaEvento: now,
        usuario: "juan.perez@simar.com",
        estadoAnterior: "Recolectado",
        estadoNuevo: "En curso",
        ubicacion: {
            latitud: 19.4326,
            longitud: -99.1332,
            direccion: "Av. Industrial 123, Parque Industrial"
        },
        observaciones: "Inicio de transporte hacia planta de tratamiento",
        metadata: {
            vehiculo: "Kenworth T680 - ABC-123",
            destino: "Planta de Tratamiento Central"
        },
        activo: true,
        createdAt: now
    }
];

db.eventos_trazabilidad.insertMany(eventos);

print('✅ Base de datos simar_trazabilidad_db creada exitosamente');
print('✅ Colección eventos_trazabilidad inicializada con ' + eventos.length + ' registros');