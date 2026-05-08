db = db.getSiblingDB('simar_servicios_db');

// Colección: servicios
db.createCollection('servicios', {
    validator: {
        $jsonSchema: {
            bsonType: 'object',
            required: ['cliente', 'direccion', 'fechaServicio', 'estado', 'tipoResiduo'],
            properties: {
                cliente: { 
                    bsonType: 'string',
                    description: 'Nombre del cliente - requerido'
                },
                direccion: { 
                    bsonType: 'string',
                    description: 'Dirección del servicio - requerido'
                },
                fechaServicio: { 
                    bsonType: 'date',
                    description: 'Fecha programada del servicio - requerido'
                },
                estado: { 
                    bsonType: 'string',
                    enum: ['Asignado', 'Recolectado', 'En curso', 'Concluido'],
                    description: 'Estado del servicio - requerido'
                },
                tipoResiduo: { 
                    bsonType: 'string',
                    description: 'Tipo de residuo - requerido'
                },
                contrato: { 
                    bsonType: 'string',
                    description: 'Número de contrato'
                },
                conductor: { 
                    bsonType: 'string',
                    description: 'Conductor asignado'
                },
                vehiculo: { 
                    bsonType: 'string',
                    description: 'Vehículo asignado'
                },
                placa: { 
                    bsonType: 'string',
                    description: 'Placa del vehículo'
                },
                tipoVehiculo: { 
                    bsonType: 'string',
                    description: 'Tipo de vehículo'
                },
                tecnico: { 
                    bsonType: 'string',
                    description: 'Técnico asignado'
                },
                operadorAsignado: { 
                    bsonType: 'string',
                    description: 'Operador asignado'
                },
                cantidadEstimada: { 
                    bsonType: 'double',
                    description: 'Cantidad estimada en kg'
                },
                observaciones: { 
                    bsonType: 'string',
                    description: 'Observaciones adicionales'
                },
                manifiesto: { 
                    bsonType: 'string',
                    description: 'Número de manifiesto'
                },
                tipoResiduoTransporte: { 
                    bsonType: 'string',
                    description: 'Tipo de residuo para transporte'
                },
                fechaRecoleccion: {
                    bsonType: 'date',
                    description: 'Fecha en que se realizó la recolección'
                },
                fechaTransporte: {
                    bsonType: 'date',
                    description: 'Fecha en que inició el transporte'
                },
                fechaConclusion: {
                    bsonType: 'date',
                    description: 'Fecha en que concluyó el servicio'
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

// Índices
db.servicios.createIndex({ cliente: 1 });
db.servicios.createIndex({ fechaServicio: -1 });
db.servicios.createIndex({ estado: 1 });
db.servicios.createIndex({ contrato: 1 });
db.servicios.createIndex({ manifiesto: 1 });
db.servicios.createIndex({ cliente: 'text', direccion: 'text', contrato: 'text' });

const now = new Date();
const yesterday = new Date(now);
yesterday.setDate(now.getDate() - 1);
const lastWeek = new Date(now);
lastWeek.setDate(now.getDate() - 7);
const lastMonth = new Date(now);
lastMonth.setDate(now.getDate() - 14);
const tomorrow = new Date(now);
tomorrow.setDate(now.getDate() + 1);

const servicios = [
    {
        cliente: 'Industrias ABC',
        direccion: 'Av. Industrial 123, Parque Industrial',
        contrato: 'CON-2024-001',
        conductor: 'Juan Pérez',
        vehiculo: 'Kenworth T680',
        placa: 'ABC-123',
        tipoVehiculo: 'Tractocamión',
        tipoResiduoTransporte: 'Plásticos industriales',
        tecnico: 'Carlos López',
        operadorAsignado: 'Pedro Ramírez',
        fechaServicio: now,
        estado: 'Asignado',
        observaciones: 'Residuos industriales no peligrosos - PENDIENTE',
        tipoResiduo: 'Plásticos',
        cantidadEstimada: 500.5,
        manifiesto: 'MAN-2024-001',
        activo: true,
        createdAt: now,
        updatedAt: now
    },
    {
        cliente: 'Comercial XYZ',
        direccion: 'Calle Comercio 456, Centro',
        contrato: 'CON-2024-045',
        conductor: 'María García',
        vehiculo: 'International 4300',
        placa: 'DEF-456',
        tipoVehiculo: 'Camión mediano',
        tipoResiduoTransporte: 'Orgánicos',
        tecnico: 'Roberto Sánchez',
        operadorAsignado: 'Ana Torres',
        fechaServicio: now,
        estado: 'Asignado',
        observaciones: 'Recolección semanal de residuos orgánicos',
        tipoResiduo: 'Orgánicos',
        cantidadEstimada: 300.0,
        manifiesto: 'MAN-2024-045',
        activo: true,
        createdAt: now,
        updatedAt: now
    },
    {
        cliente: 'Hospital del Sur',
        direccion: 'Av. Salud 789, Colonia Médica',
        contrato: 'CON-2024-089',
        conductor: 'Ana Martínez',
        vehiculo: 'Mercedes-Benz Actros',
        placa: 'GHI-789',
        tipoVehiculo: 'Camión pesado',
        tipoResiduoTransporte: 'Biológicos',
        tecnico: 'José Ramírez',
        operadorAsignado: 'Laura Méndez',
        fechaServicio: tomorrow,
        estado: 'Asignado',
        observaciones: 'Residuos biológicos - Manejo especial',
        tipoResiduo: 'Biológicos',
        cantidadEstimada: 150.75,
        manifiesto: 'MAN-2024-089',
        activo: true,
        createdAt: now,
        updatedAt: now
    },
    {
        cliente: 'Restaurante El Sabor',
        direccion: 'Calle Principal 321, Zona Centro',
        contrato: 'CON-2024-112',
        conductor: 'Pedro González',
        vehiculo: 'Ford F-550',
        placa: 'JKL-012',
        tipoVehiculo: 'Camión ligero',
        tipoResiduoTransporte: 'Aceites',
        tecnico: 'Miguel Ángel',
        operadorAsignado: 'Carlos Ruiz',
        fechaServicio: now,
        estado: 'Recolectado',
        fechaRecoleccion: now,
        observaciones: 'Aceites y grasas',
        tipoResiduo: 'Aceites',
        cantidadEstimada: 50.0,
        manifiesto: 'MAN-2024-112',
        activo: true,
        createdAt: now,
        updatedAt: now
    },
    {
        cliente: 'Constructora Moderna',
        direccion: 'Blvd. Construcción 567, Zona Industrial',
        contrato: 'CON-2024-078',
        conductor: 'Luis Hernández',
        vehiculo: 'Volvo FH16',
        placa: 'MNO-345',
        tipoVehiculo: 'Tractocamión',
        tipoResiduoTransporte: 'Escombros',
        tecnico: 'Fernando Díaz',
        operadorAsignado: 'Roberto Méndez',
        fechaServicio: yesterday,
        estado: 'En curso',
        fechaRecoleccion: yesterday,
        fechaTransporte: now,
        observaciones: 'Escombros y materiales de construcción',
        tipoResiduo: 'Escombros',
        cantidadEstimada: 1000.0,
        manifiesto: 'MAN-2024-078',
        activo: true,
        createdAt: now,
        updatedAt: now
    },
    {
        cliente: 'Industrias ABC',
        direccion: 'Av. Industrial 123, Parque Industrial',
        contrato: 'CON-2024-001',
        conductor: 'Juan Pérez',
        vehiculo: 'Kenworth T680',
        placa: 'ABC-123',
        tipoVehiculo: 'Tractocamión',
        tipoResiduoTransporte: 'Plásticos industriales',
        tecnico: 'Carlos López',
        operadorAsignado: 'Pedro Ramírez',
        fechaServicio: lastWeek,
        estado: 'Concluido',
        fechaRecoleccion: lastWeek,
        fechaTransporte: lastWeek,
        fechaConclusion: lastWeek,
        observaciones: 'Recolección de residuos plásticos - Lote 2',
        tipoResiduo: 'Plásticos',
        cantidadEstimada: 450.0,
        manifiesto: 'MAN-2024-006',
        activo: true,
        createdAt: lastWeek,
        updatedAt: lastWeek
    }
];

db.servicios.insertMany(servicios);

print('Base de datos simar_servicios_db creada exitosamente');
print('Colección servicios inicializada con ' + servicios.length + ' registros');