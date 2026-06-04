// ============================================
// BASE DE DATOS: RECOLECCIONES - SIMAR
// ============================================

db = db.getSiblingDB('simar_recolecciones_db');

// ============================================
// COLECCIÓN: RECOLECCIONES (CON MÚLTIPLES RESIDUOS)
// ============================================
db.createCollection('recolecciones', {
    validator: {
        $jsonSchema: {
            bsonType: 'object',
            required: ['idContrato', 'cliente', 'fecha', 'direccion', 'vehiculos', 'estado', 'tiposResiduo'],
            properties: {
                idContrato: {
                    bsonType: 'int',
                    description: 'ID del contrato asociado - requerido'
                },
                cliente: {
                    bsonType: 'string',
                    description: 'Nombre del cliente - requerido'
                },
                fecha: {
                    bsonType: 'date',
                    description: 'Fecha y hora de la recolección - requerido'
                },
                direccion: {
                    bsonType: 'string',
                    description: 'Dirección completa - requerido'
                },
                vehiculos: {
                    bsonType: 'array',
                    minItems: 1,
                    items: {
                        bsonType: 'object',
                        required: ['vehiculo', 'chofer'],
                        properties: {
                            vehiculo: {
                                bsonType: 'string',
                                description: 'Identificador del vehículo'
                            },
                            chofer: {
                                bsonType: 'string',
                                description: 'Nombre del chofer'
                            },
                            tecnicos: {
                                bsonType: 'array',
                                maxItems: 3,
                                items: {
                                    bsonType: 'string'
                                },
                                description: 'Lista de técnicos (máximo 3)'
                            }
                        }
                    }
                },
                estado: {
                    bsonType: 'string',
                    enum: ['Programada', 'En ruta', 'Completada', 'Cancelada'],
                    description: 'Estado de la recolección - requerido'
                },
                tiposResiduo: {
                    bsonType: 'array',
                    minItems: 1,
                    items: {
                        bsonType: 'object',
                        required: ['wasteTypeId', 'wasteTypeCode', 'wasteTypeName', 'cantidadEstimada', 'unidad'],
                        properties: {
                            wasteTypeId: {
                                bsonType: 'int',
                                description: 'ID del tipo de residuo en el catálogo'
                            },
                            wasteTypeCode: {
                                bsonType: 'string',
                                description: 'Código del residuo (ej: RP-RPBI-001)'
                            },
                            wasteTypeName: {
                                bsonType: 'string',
                                description: 'Nombre del residuo'
                            },
                            wasteType: {
                                bsonType: 'string',
                                enum: ['peligroso', 'especial'],
                                description: 'Tipo de residuo (peligroso/especial)'
                            },
                            cantidadEstimada: {
                                bsonType: 'double',
                                description: 'Cantidad estimada en la unidad especificada'
                            },
                            unidad: {
                                bsonType: 'string',
                                description: 'Unidad de medida (kg, ton, lt, m3, pza)'
                            }
                        }
                    },
                    description: 'Lista de tipos de residuo a recolectar'
                },
                observaciones: {
                    bsonType: 'string',
                    description: 'Observaciones adicionales'
                },
                activo: {
                    bsonType: 'bool',
                    description: 'Registro activo o eliminado'
                },
                createdAt: {
                    bsonType: 'date',
                    description: 'Fecha de creación'
                },
                updatedAt: {
                    bsonType: 'date',
                    description: 'Fecha de última actualización'
                }
            }
        }
    }
});

// ============================================
// ÍNDICES ACTUALIZADOS
// ============================================
db.recolecciones.createIndex({ cliente: 1 });
db.recolecciones.createIndex({ fecha: -1 });
db.recolecciones.createIndex({ estado: 1 });
db.recolecciones.createIndex({ idContrato: 1 });
db.recolecciones.createIndex({ "vehiculos.vehiculo": 1 });
db.recolecciones.createIndex({ "vehiculos.chofer": 1 });
db.recolecciones.createIndex({ "tiposResiduo.wasteTypeId": 1 });
db.recolecciones.createIndex({ "tiposResiduo.wasteTypeCode": 1 });
db.recolecciones.createIndex({ cliente: 'text', direccion: 'text' });

// ============================================
// DATOS DE PRUEBA ACTUALIZADOS
// ============================================

const now = new Date();
const tomorrow = new Date(now);
tomorrow.setDate(now.getDate() + 1);
const nextWeek = new Date(now);
nextWeek.setDate(now.getDate() + 7);

const recolecciones = [
    {
        idContrato: 1,
        cliente: 'Hospital Ángeles',
        fecha: tomorrow,
        direccion: 'Av. Paseo de la Reforma 123, Col. Juárez, CDMX',
        vehiculos: [
            {
                vehiculo: 'Kenworth T680 - ABC-1234',
                chofer: 'Carlos Hernández',
                tecnicos: ['Miguel Rodríguez', 'Ana García']
            }
        ],
        estado: 'Programada',
        tiposResiduo: [
            {
                wasteTypeId: 2,
                wasteTypeCode: 'RP-RPBI-002',
                wasteTypeName: 'Residuos no anatómicos',
                wasteType: 'peligroso',
                cantidadEstimada: 1.5,
                unidad: 'kg'
            },
            {
                wasteTypeId: 1,
                wasteTypeCode: 'RP-RPBI-001',
                wasteTypeName: 'Objetos punzocortantes',
                wasteType: 'peligroso',
                cantidadEstimada: 0.5,
                unidad: 'kg'
            }
        ],
        observaciones: 'Recolección de residuos peligrosos biológico-infecciosos',
        activo: true,
        createdAt: now,
        updatedAt: now
    },
    {
        idContrato: 1,
        cliente: 'Hospital Ángeles',
        fecha: nextWeek,
        direccion: 'Av. Paseo de la Reforma 123, Col. Juárez, CDMX',
        vehiculos: [
            {
                vehiculo: 'Volvo FH16 - DEF-5678',
                chofer: 'Miguel Rodríguez',
                tecnicos: []
            }
        ],
        estado: 'Programada',
        tiposResiduo: [
            {
                wasteTypeId: 2,
                wasteTypeCode: 'RP-RPBI-002',
                wasteTypeName: 'Residuos no anatómicos',
                wasteType: 'peligroso',
                cantidadEstimada: 2.0,
                unidad: 'kg'
            }
        ],
        observaciones: 'Segunda recolección del contrato',
        activo: true,
        createdAt: now,
        updatedAt: now
    },
    {
        idContrato: 2,
        cliente: 'Plaza Comercial Galerías',
        fecha: nextWeek,
        direccion: 'Blvd. Manuel Ávila Camacho 567, Col. Polanco, CDMX',
        vehiculos: [
            {
                vehiculo: 'Volvo FH16 - DEF-5678',
                chofer: 'Miguel Rodríguez',
                tecnicos: []
            }
        ],
        estado: 'Programada',
        tiposResiduo: [
            {
                wasteTypeId: 5,
                wasteTypeCode: 'RME-CAR-001',
                wasteTypeName: 'Cartón y papel',
                wasteType: 'especial',
                cantidadEstimada: 3.0,
                unidad: 'ton'
            },
            {
                wasteTypeId: 6,
                wasteTypeCode: 'RME-PLA-001',
                wasteTypeName: 'Plástico industrial',
                wasteType: 'especial',
                cantidadEstimada: 2.0,
                unidad: 'ton'
            }
        ],
        observaciones: 'Recolección de cartón, plástico y papel',
        activo: true,
        createdAt: now,
        updatedAt: now
    },
    {
        idContrato: 3,
        cliente: 'Constructora ABC',
        fecha: nextWeek,
        direccion: 'Carretera México-Querétaro Km 23, Cuautitlán Izcalli',
        vehiculos: [
            {
                vehiculo: 'International HV Series - MNO-7890',
                chofer: 'Jorge Martínez',
                tecnicos: ['Luis Pérez', 'Roberto Gómez']
            },
            {
                vehiculo: 'Ford F-550 - JKL-3456',
                chofer: 'Roberto Gómez',
                tecnicos: ['Luis Pérez']
            }
        ],
        estado: 'En ruta',
        tiposResiduo: [
            {
                wasteTypeId: 8,
                wasteTypeCode: 'RME-CON-001',
                wasteTypeName: 'Residuos de construcción',
                wasteType: 'especial',
                cantidadEstimada: 8.5,
                unidad: 'm3'
            }
        ],
        observaciones: 'Escombros y tierra contaminada',
        activo: true,
        createdAt: now,
        updatedAt: now
    },
    {
        idContrato: 2,
        cliente: 'Laboratorios Médicos del Valle',
        fecha: nextWeek,
        direccion: 'Av. Universidad 890, Col. Narvarte, CDMX',
        vehiculos: [
            {
                vehiculo: 'Mercedes-Benz Actros - GHI-9012',
                chofer: 'Ana García',
                tecnicos: ['Carlos Hernández']
            }
        ],
        estado: 'Completada',
        tiposResiduo: [
            {
                wasteTypeId: 4,
                wasteTypeCode: 'RP-SOL-001',
                wasteTypeName: 'Solventes halogenados gastados',
                wasteType: 'peligroso',
                cantidadEstimada: 1.2,
                unidad: 'lt'
            }
        ],
        observaciones: 'Recolección completada exitosamente',
        activo: true,
        createdAt: now,
        updatedAt: now
    }
];

db.recolecciones.insertMany(recolecciones);

print('✅ Base de datos y colecciones creadas exitosamente');
print('📊 Colección recolecciones actualizada con soporte para múltiples residuos');