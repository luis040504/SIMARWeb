CREATE DATABASE IF NOT EXISTS simar_manifiestos_db;
USE simar_manifiestos_db;

CREATE TABLE IF NOT EXISTS secuencias_manifiesto (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    id_cliente          INT NOT NULL,
    tipo                ENUM('especial', 'peligroso') NOT NULL,
    anio                YEAR NOT NULL,
    ultimo_consecutivo  INT NOT NULL DEFAULT 0,
    UNIQUE KEY uk_cliente_tipo_anio (id_cliente, tipo, anio)
);

CREATE TABLE IF NOT EXISTS manifiestos (
    id INT PRIMARY KEY AUTO_INCREMENT,
    id_cliente          INT NOT NULL,
    numero_manifiesto   VARCHAR(50) NOT NULL,
    tipo                ENUM('especial', 'peligroso') NOT NULL,
    estado              ENUM('borrador', 'en_transito', 'completado') DEFAULT 'borrador',

    numero_registro_ambiental   VARCHAR(100),
    razon_social                VARCHAR(200) NOT NULL,
    domicilio                   VARCHAR(300),
    calle                       VARCHAR(200),
    numero_exterior             VARCHAR(20),
    numero_interior             VARCHAR(20),
    colonia                     VARCHAR(100),
    estado_generador            VARCHAR(50),
    -- Comunes
    codigo_postal               VARCHAR(10),
    municipio                   VARCHAR(100),
    telefono                    VARCHAR(20),
    correo                      VARCHAR(100),
    -- Especial
    fecha_manifiesto            DATE,
    hora_manifiesto             TIME,
    observaciones_generador     TEXT,
    -- Peligroso
    instrucciones_manejo_seguro TEXT,
    -- Firma generador
    nombre_responsable_generador VARCHAR(100),
    fecha_firma_generador        DATE,

    -- === TRANSPORTISTA ===
    numero_autorizacion_transportista VARCHAR(100),
    numero_permiso_sct                VARCHAR(100),
    razon_social_transportista        VARCHAR(200),
    -- Especial
    domicilio_transportista           VARCHAR(300),
    -- Peligroso
    calle_transportista               VARCHAR(200),
    numero_exterior_transportista     VARCHAR(20),
    numero_interior_transportista     VARCHAR(20),
    colonia_transportista             VARCHAR(100),
    estado_transportista              VARCHAR(50),
    correo_transportista              VARCHAR(100),
    -- Comunes
    codigo_postal_transportista       VARCHAR(10),
    municipio_transportista           VARCHAR(100),
    telefono_transportista            VARCHAR(20),
    tipo_vehiculo                     VARCHAR(100),
    placa                             VARCHAR(20),
    licencia_conductor                VARCHAR(50),
    ruta_transporte                   TEXT,
    observaciones_transportista       TEXT,
    nombre_responsable_transportista  VARCHAR(100),
    -- Especial
    fecha_recepcion_transportista     DATE,
    hora_recepcion_transportista      TIME,
    -- Peligroso
    fecha_firma_transportista         DATE,

    -- === DESTINATARIO ===
    numero_autorizacion_destinatario  VARCHAR(100),
    razon_social_destinatario         VARCHAR(200),
    -- Especial
    domicilio_destinatario            VARCHAR(300),
    tipo_disposicion                  VARCHAR(100),
    fecha_destinatario                DATE,
    hora_destinatario                 TIME,
    -- Peligroso
    calle_destinatario                VARCHAR(200),
    numero_exterior_destinatario      VARCHAR(20),
    numero_interior_destinatario      VARCHAR(20),
    colonia_destinatario              VARCHAR(100),
    estado_destinatario               VARCHAR(50),
    correo_destinatario               VARCHAR(100),
    persona_recibe                    VARCHAR(100),
    fecha_firma_destinatario          DATE,
    -- Comunes
    codigo_postal_destinatario        VARCHAR(10),
    municipio_destinatario            VARCHAR(100),
    telefono_destinatario             VARCHAR(20),
    nombre_responsable_destinatario   VARCHAR(100),
    observaciones_destinatario        TEXT,

    nombre_archivo_firmado  VARCHAR(500),
    fecha_firma             DATE,

    activo              BOOLEAN DEFAULT TRUE,
    fecha_creacion      TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,


    UNIQUE KEY uk_folio_cliente_tipo (id_cliente, tipo, numero_manifiesto),
    INDEX idx_cliente (id_cliente),
    INDEX idx_numero (numero_manifiesto),
    INDEX idx_tipo_estado (tipo, estado),
    INDEX idx_razon_social (razon_social),
    INDEX idx_fecha (fecha_manifiesto)
);


CREATE TABLE IF NOT EXISTS residuos_especiales (
    id              INT PRIMARY KEY AUTO_INCREMENT,
    manifiesto_id   INT NOT NULL,
    clave_residuo   VARCHAR(50),
    nombre_residuo  VARCHAR(200) NOT NULL,
    tipo_envase     VARCHAR(50),
    capacidad       VARCHAR(50),
    peso            DECIMAL(10, 2),
    unidad          ENUM('kg', 'ton', 'lt', 'm3', 'pza') DEFAULT 'kg',
    FOREIGN KEY (manifiesto_id) REFERENCES manifiestos(id) ON DELETE CASCADE
);


CREATE TABLE IF NOT EXISTS residuos_peligrosos (
    id              INT PRIMARY KEY AUTO_INCREMENT,
    manifiesto_id   INT NOT NULL,
    nombre_residuo  VARCHAR(200) NOT NULL,
    es_corrosivo    BOOLEAN DEFAULT FALSE,
    es_reactivo     BOOLEAN DEFAULT FALSE,
    es_explosivo    BOOLEAN DEFAULT FALSE,
    es_toxico       BOOLEAN DEFAULT FALSE,
    es_inflamable   BOOLEAN DEFAULT FALSE,
    es_biologico    BOOLEAN DEFAULT FALSE,
    es_mutagenico   BOOLEAN DEFAULT FALSE,
    tipo_envase     VARCHAR(50),
    capacidad_envase VARCHAR(50),
    cantidad_kg     DECIMAL(10, 2),
    tiene_etiqueta  BOOLEAN,
    FOREIGN KEY (manifiesto_id) REFERENCES manifiestos(id) ON DELETE CASCADE
);


CREATE OR REPLACE VIEW v_manifiestos_resumen AS
SELECT
    m.id,
    m.numero_manifiesto,
    m.tipo,
    m.estado,
    m.razon_social,
    m.municipio,
    m.fecha_manifiesto,
    m.razon_social_transportista,
    m.nombre_archivo_firmado,
    m.fecha_firma,
    m.fecha_creacion
FROM manifiestos m
WHERE m.activo = TRUE;


DELIMITER //

CREATE PROCEDURE IF NOT EXISTS sp_buscar_manifiestos(
    IN p_numero      VARCHAR(50),
    IN p_razon       VARCHAR(200),
    IN p_tipo        VARCHAR(20),
    IN p_estado      VARCHAR(20),
    IN p_fecha_desde DATE,
    IN p_fecha_hasta DATE
)
BEGIN
    SELECT
        m.id,
        m.numero_manifiesto,
        m.tipo,
        m.estado,
        m.razon_social,
        m.municipio,
        m.fecha_manifiesto,
        m.razon_social_transportista
    FROM manifiestos m
    WHERE m.activo = TRUE
      AND (p_numero IS NULL      OR m.numero_manifiesto LIKE CONCAT('%', p_numero, '%'))
      AND (p_razon  IS NULL      OR m.razon_social LIKE CONCAT('%', p_razon, '%'))
      AND (p_tipo   IS NULL      OR m.tipo = p_tipo)
      AND (p_estado IS NULL      OR m.estado = p_estado)
      AND (p_fecha_desde IS NULL OR m.fecha_manifiesto >= p_fecha_desde)
      AND (p_fecha_hasta IS NULL OR m.fecha_manifiesto <= p_fecha_hasta)
    ORDER BY m.fecha_creacion DESC;
END//

DELIMITER ;

INSERT INTO secuencias_manifiesto (id_cliente, tipo, anio, ultimo_consecutivo) VALUES
    (1, 'especial',  2026, 9),
    (1, 'peligroso', 2026, 2)
ON DUPLICATE KEY UPDATE ultimo_consecutivo = VALUES(ultimo_consecutivo);

INSERT INTO manifiestos (
    id_cliente, numero_manifiesto, tipo, estado,
    numero_registro_ambiental, razon_social, domicilio,
    codigo_postal, municipio, telefono, correo,
    fecha_manifiesto, hora_manifiesto,
    nombre_responsable_generador,
    numero_autorizacion_transportista, razon_social_transportista,
    domicilio_transportista, codigo_postal_transportista, municipio_transportista,
    telefono_transportista, tipo_vehiculo, placa, licencia_conductor,
    ruta_transporte, nombre_responsable_transportista,
    fecha_recepcion_transportista, hora_recepcion_transportista,
    numero_autorizacion_destinatario, razon_social_destinatario,
    domicilio_destinatario, codigo_postal_destinatario, municipio_destinatario,
    tipo_disposicion, nombre_responsable_destinatario,
    nombre_archivo_firmado, fecha_firma
) VALUES (
    1, '009/2026', 'especial', 'completado',
    'SEDEMA/TRME-CH0990/20EXR-17/182',
    'Cementos Moctezuma S.A. de C.V.',
    'Dom. conocido Predio de Los Gallineros, Camino Vecinal Cerro Colorado',
    '91645', 'Apazapan', '2288326510', 'operaciones@moctezuma.com.mx',
    '2026-02-26', '10:48:00',
    'Santiago Montoya',
    'SEDEMA/TRME-SMA010239-2025/073',
    'Sistemas en Manejo y Administración de Residuos S.A. de C.V.',
    'Av. Pípila No. 126, Col. José Cardel', '91030', 'Xalapa',
    '2288343149', 'Camión Caja Seca 3.5 Ton.', 'YJ-9638-A', 'UB0030UNC',
    'De Apazapan, Ver. a Xalapa, Ver.', 'Fernando López Sánchez',
    '2026-02-26', '12:15:00',
    'SEDEMA/AATRME-SMA0810239LG-2024/16',
    'Sistemas en Manejo y Administración de Residuos S.A. de C.V.',
    'Félix Licona #209 Col. Rafael Lucio', '91110', 'Xalapa',
    'Almacén Temporal', 'Gustavo Cruz Torres',
    NULL, NULL
);

INSERT INTO residuos_especiales (manifiesto_id, clave_residuo, nombre_residuo, tipo_envase, capacidad, peso, unidad)
VALUES (1, 'IE-001', 'Otros Residuos Inorgánicos (RSU)', 'OF', '1/6 m³', 680, 'kg');

INSERT INTO manifiestos (
    id_cliente, numero_manifiesto, tipo, estado,
    numero_registro_ambiental, razon_social,
    calle, numero_exterior, numero_interior, colonia, estado_generador,
    codigo_postal, municipio, telefono, correo,
    instrucciones_manejo_seguro, nombre_responsable_generador, fecha_firma_generador,
    numero_autorizacion_transportista, numero_permiso_sct, razon_social_transportista,
    calle_transportista, numero_exterior_transportista, colonia_transportista,
    codigo_postal_transportista, municipio_transportista, estado_transportista,
    telefono_transportista, tipo_vehiculo, placa, ruta_transporte,
    nombre_responsable_transportista, fecha_firma_transportista,
    numero_autorizacion_destinatario, razon_social_destinatario,
    calle_destinatario, numero_exterior_destinatario, colonia_destinatario,
    codigo_postal_destinatario, municipio_destinatario, estado_destinatario,
    nombre_responsable_destinatario, fecha_firma_destinatario
) VALUES (
    1, '002/2026', 'peligroso', 'en_transito',
    'CMORE3001711', 'Cementos Moctezuma S.A. de C.V.',
    'Dom. conocido Predio de Los Gallineros', 'S/N', 'S/N',
    'Camino Vecinal Cerro Colorado', 'Veracruz',
    '91645', 'Apazapan', '2288326510', 'operaciones@moctezuma.com.mx',
    'Uso de EPP: guantes, ropa de algodón, calzado de seguridad.',
    'Santiago Montoya', '2026-02-26',
    '30-087-PS-I-07D-17', '3063SMA1005201723031000',
    'Sistemas en Manejo y Administración de Residuos S.A. de C.V.',
    'Avenida Pípila', '126', 'José Cardel',
    '91030', 'Xalapa', 'Veracruz',
    '2288343149', 'Camión Caja Seca 4.5 Ton.', '39AE4N',
    'Apazapan, Ver. a Xalapa, Ver.',
    'Fernando López Sánchez', '2026-02-26',
    '21-V-23-20', 'Exitum Tratamientos Ecológicos S.A. de C.V.',
    'Carr. Gpe. Victoria Valsequillo KM15', '147',
    'Localidad San Baltasar Torija',
    '75235', 'Cuautinchan', 'Puebla',
    'Responsable Exitum', '2026-02-27'
);

INSERT INTO residuos_peligrosos
    (manifiesto_id, nombre_residuo, es_biologico, tipo_envase, capacidad_envase, cantidad_kg, tiene_etiqueta)
VALUES
    (2, 'Objetos Punzocortantes', TRUE, 'CIP', '1', 250, TRUE),
    (2, 'Residuos No Anatómicos',  TRUE, 'RIP', 'N/A', 640, TRUE);
