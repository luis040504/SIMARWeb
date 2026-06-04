-- ============================================
-- BASE DE DATOS: VEHICULOS - SIMAR (ACTUALIZADO)
-- ============================================

CREATE DATABASE IF NOT EXISTS simar_vehiculos_db;
USE simar_vehiculos_db;

-- ============================================
-- TABLA: VEHICULOS (ACTUALIZADA)
-- ============================================
CREATE TABLE IF NOT EXISTS vehiculos (
    id INT PRIMARY KEY AUTO_INCREMENT,
    numero_economico VARCHAR(20) UNIQUE,
    marca VARCHAR(50) NOT NULL,
    modelo VARCHAR(50) NOT NULL,
    anio INT,
    color VARCHAR(30),
    placas VARCHAR(15) NOT NULL UNIQUE,
    peso_toneladas DECIMAL(8,2) NOT NULL,
    licencia_requerida ENUM('A', 'B', 'C', 'D', 'E') NOT NULL,
    tipo_gasolina VARCHAR(30) NOT NULL,
    descripcion TEXT,
    foto MEDIUMBLOB,  -- Cambiado de foto_url a foto (binario)
    activo BOOLEAN DEFAULT TRUE,
    fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_placa (placas),
    INDEX idx_numero_economico (numero_economico),
    INDEX idx_marca_modelo (marca, modelo)
);

-- ============================================
-- TABLA: TIPOS DE RESIDUO (CATÁLOGO EXTERNO)
-- Esta tabla solo almacena IDs/referencias al catálogo
-- ============================================
CREATE TABLE IF NOT EXISTS tipos_residuo_catalogo (
    id INT PRIMARY KEY AUTO_INCREMENT,
    codigo_catalogo VARCHAR(50) NOT NULL UNIQUE,  -- Código del catálogo (ej: "RP-RPBI-001")
    nombre VARCHAR(200) NOT NULL,
    tipo_residuo VARCHAR(20) NOT NULL,  -- "peligroso" o "especial"
    descripcion TEXT,
    activo BOOLEAN DEFAULT TRUE
);

-- ============================================
-- TABLA RELACIONAL: VEHICULO_TIPO_RESIDUO
-- ============================================
CREATE TABLE IF NOT EXISTS vehiculo_tipo_residuo (
    vehiculo_id INT NOT NULL,
    tipo_residuo_id INT NOT NULL,
    fecha_asignacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    PRIMARY KEY (vehiculo_id, tipo_residuo_id),
    FOREIGN KEY (vehiculo_id) REFERENCES vehiculos(id) ON DELETE CASCADE,
    FOREIGN KEY (tipo_residuo_id) REFERENCES tipos_residuo_catalogo(id),
    
    INDEX idx_vehiculo (vehiculo_id),
    INDEX idx_tipo_residuo (tipo_residuo_id)
);

-- ============================================
-- TABLA: TIPOS DE GASOLINA (Catalogo)
-- ============================================
CREATE TABLE IF NOT EXISTS tipos_gasolina (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(50) NOT NULL UNIQUE,
    descripcion TEXT
);

-- ============================================
-- DATOS INICIALES (Catalogos locales)
-- ============================================

INSERT INTO tipos_gasolina (nombre, descripcion) VALUES
('Diesel', 'Combustible para motores diesel'),
('Gasolina Magna', 'Gasolina regular de 87 octanos'),
('Gasolina Premium', 'Gasolina de alto octanaje (91-93 octanos)'),
('Gas Natural', 'Gas natural comprimido para vehiculos'),
('Electrico', 'Vehiculos de bateria electrica'),
('Hibrido', 'Combinacion de gasolina y electrico');

-- NOTA: Los tipos de residuo se sincronizarán desde el microservicio de catálogo
-- mediante un job o API call periódica, o se insertarán manualmente desde el seed
-- inicial basado en el catálogo proporcionado.

-- ============================================
-- VISTAS UTILES
-- ============================================

-- Vista que incluye los tipos de residuo como JSON
CREATE VIEW v_vehiculos_completo AS
SELECT 
    v.id,
    v.numero_economico,
    v.marca,
    v.modelo,
    v.anio,
    v.color,
    v.placas,
    v.peso_toneladas,
    v.licencia_requerida,
    v.tipo_gasolina,
    v.descripcion,
    v.foto,
    v.activo,
    v.fecha_creacion,
    v.fecha_actualizacion,
    (
        SELECT JSON_ARRAYAGG(
            JSON_OBJECT(
                'id', tr.id,
                'codigo', tr.codigo_catalogo,
                'nombre', tr.nombre,
                'tipo', tr.tipo_residuo
            )
        )
        FROM vehiculo_tipo_residuo vtr
        JOIN tipos_residuo_catalogo tr ON vtr.tipo_residuo_id = tr.id
        WHERE vtr.vehiculo_id = v.id AND tr.activo = TRUE
    ) AS tipos_residuo
FROM vehiculos v
WHERE v.activo = TRUE;

-- ============================================
-- PROCEDIMIENTOS ALMACENADOS
-- ============================================

DELIMITER //

CREATE PROCEDURE sp_buscar_vehiculos(
    IN p_search VARCHAR(100)
)
BEGIN
    SELECT * FROM v_vehiculos_completo 
    WHERE activo = TRUE 
    AND (
        marca LIKE CONCAT('%', p_search, '%')
        OR modelo LIKE CONCAT('%', p_search, '%')
        OR placas LIKE CONCAT('%', p_search, '%')
        OR numero_economico LIKE CONCAT('%', p_search, '%')
    );
END//

DELIMITER ;

-- ============================================
-- TRIGGER PARA VALIDAR ANIO
-- ============================================

DELIMITER //

CREATE TRIGGER validar_anio_vehiculo
BEFORE INSERT ON vehiculos
FOR EACH ROW
BEGIN
    IF NEW.anio IS NOT NULL AND NEW.anio > YEAR(CURDATE()) THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'El anio del vehiculo no puede ser futuro';
    END IF;
END//

CREATE TRIGGER validar_anio_vehiculo_update
BEFORE UPDATE ON vehiculos
FOR EACH ROW
BEGIN
    IF NEW.anio IS NOT NULL AND NEW.anio > YEAR(CURDATE()) THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'El anio del vehiculo no puede ser futuro';
    END IF;
END//

DELIMITER ;