db = db.getSiblingDB('simar_recolecciones_db');
db.createCollection('recolecciones', {
    validator: {
        $jsonSchema: {
            bsonType: 'object',
            required: ['cliente', 'fecha', 'direccion', 'vehiculo', 'tecnico', 'estado'],
            properties: {
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
                vehiculo: {
                    bsonType: 'string',
                    description: 'Vehículo asignado - requerido'
                },
                tecnico: {
                    bsonType: 'string',
                    description: 'Técnico asignado - requerido'
                },
                estado: {
                    bsonType: 'string',
                    enum: ['Programada', 'En ruta', 'Completada', 'Cancelada'],
                    description: 'Estado de la recolección - requerido'
                },
                tipoResiduo: {
                    bsonType: 'string',
                    description: 'Tipo de residuo a recolectar'
                },
                cantidadEstimada: {
                    bsonType: 'double',
                    description: 'Cantidad estimada en toneladas'
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


db.recolecciones.createIndex({ cliente: 1 });
db.recolecciones.createIndex({ fecha: -1 });
db.recolecciones.createIndex({ vehiculo: 1 });
db.recolecciones.createIndex({ tecnico: 1 });
db.recolecciones.createIndex({ estado: 1 });
db.recolecciones.createIndex({ cliente: 'text', direccion: 'text' });


const now = new Date();
const tomorrow = new Date(now);
tomorrow.setDate(now.getDate() + 1);
const nextWeek = new Date(now);
nextWeek.setDate(now.getDate() + 7);

const recolecciones = [
    {
        cliente: 'Hospital Ángeles',
        fecha: tomorrow,
        direccion: 'Av. Paseo de la Reforma 123, Col. Juárez, CDMX',
        vehiculo: 'Kenworth T680 - ABC-1234',
        tecnico: 'Carlos Hernández',
        estado: 'Programada',
        tipoResiduo: 'Residuos Biológicos',
        cantidadEstimada: 2.5,
        observaciones: 'Recolección de residuos peligrosos biológico-infecciosos',
        activo: true,
        createdAt: now,
        updatedAt: now
    },
    {
        cliente: 'Plaza Comercial Galerías',
        fecha: nextWeek,
        direccion: 'Blvd. Manuel Ávila Camacho 567, Col. Polanco, CDMX',
        vehiculo: 'Volvo FH16 - DEF-5678',
        tecnico: 'Miguel Rodríguez',
        estado: 'Programada',
        tipoResiduo: 'Residuos Reciclables',
        cantidadEstimada: 5.0,
        observaciones: 'Recolección de cartón, plástico y papel',
        activo: true,
        createdAt: now,
        updatedAt: now
    },
    {
        cliente: 'Constructora ABC',
        direccion: 'Carretera México-Querétaro Km 23, Cuautitlán Izcalli',
        vehiculo: 'International HV Series - MNO-7890',
        tecnico: 'Jorge Martínez',
        estado: 'En ruta',
        tipoResiduo: 'Residuos de Construcción',
        cantidadEstimada: 8.5,
        observaciones: 'Escombros y tierra contaminada',
        activo: true,
        createdAt: now,
        updatedAt: now
    },
    {
        cliente: 'Laboratorios Médicos del Valle',
        direccion: 'Av. Universidad 890, Col. Narvarte, CDMX',
        vehiculo: 'Mercedes-Benz Actros - GHI-9012',
        tecnico: 'Ana García',
        estado: 'Completada',
        tipoResiduo: 'Residuos Peligrosos',
        cantidadEstimada: 1.2,
        observaciones: 'Recolección completada exitosamente',
        activo: true,
        createdAt: now,
        updatedAt: now
    },
    {
        cliente: 'Gasolinera PEMEX',
        direccion: 'Eje Central 456, Col. Doctores, CDMX',
        vehiculo: 'Ford F-550 - JKL-3456',
        tecnico: 'Luis Pérez',
        estado: 'Cancelada',
        tipoResiduo: 'Residuos Industriales',
        cantidadEstimada: 3.0,
        observaciones: 'Cancelada por solicitud del cliente',
        activo: true,
        createdAt: now,
        updatedAt: now
    }
];


db.recolecciones.insertMany(recolecciones);

print(' Base de datos y colecciones creadas exitosamente');
print(' Colección recolecciones inicializada con datos de prueba');