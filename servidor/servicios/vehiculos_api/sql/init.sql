-- ============================================
-- BASE DE DATOS: VEHICULOS - SIMAR
-- ============================================

CREATE DATABASE IF NOT EXISTS simar_vehiculos_db;
USE simar_vehiculos_db;

-- ============================================
-- TABLA: VEHICULOS
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
    tipo_desecho TEXT NOT NULL,
    descripcion TEXT,
    foto_url VARCHAR(500),
    activo BOOLEAN DEFAULT TRUE,
    fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_placa (placas),
    INDEX idx_numero_economico (numero_economico),
    INDEX idx_marca_modelo (marca, modelo)
);

-- ============================================
-- TABLA: TIPOS DE DESECHO (Catalogo)
-- ============================================
CREATE TABLE IF NOT EXISTS tipos_desecho (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(100) NOT NULL UNIQUE,
    descripcion TEXT,
    activo BOOLEAN DEFAULT TRUE
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
-- DATOS INICIALES (Catalogos)
-- ============================================

INSERT INTO tipos_desecho (nombre, descripcion) VALUES
('Residuos Peligrosos', 'Materiales que representan un riesgo para la salud o el medio ambiente'),
('Residuos Biologicos', 'Desechos provenientes de actividades medicas o de laboratorio'),
('Residuos Reciclables', 'Papel, carton, plastico, vidrio y metales'),
('Residuos Organicos', 'Desechos de origen biologico como alimentos y poda'),
('Residuos de Construccion', 'Escombros, tierra, concreto y materiales de demolicion'),
('Residuos Electronicos', 'Equipos electronicos en desuso'),
('Residuos Industriales', 'Subproductos de procesos industriales'),
('Residuos Varios', 'Otros tipos de residuos no clasificados');

INSERT INTO tipos_gasolina (nombre, descripcion) VALUES
('Diesel', 'Combustible para motores diesel'),
('Gasolina Magna', 'Gasolina regular de 87 octanos'),
('Gasolina Premium', 'Gasolina de alto octanaje (91-93 octanos)'),
('Gas Natural', 'Gas natural comprimido para vehiculos'),
('Electrico', 'Vehiculos de bateria electrica'),
('Hibrido', 'Combinacion de gasolina y electrico');

-- ============================================
-- EJEMPLOS DE VEHICULOS (Datos de prueba)
-- ============================================
INSERT INTO vehiculos (numero_economico, marca, modelo, anio, color, placas, peso_toneladas, licencia_requerida, tipo_gasolina, tipo_desecho, descripcion, foto_url) VALUES
('VH-001', 'Kenworth', 'T680', 2022, 'Blanco', 'ABC-1234', 15.5, 'E', 'Diesel', 'Residuos Peligrosos,Residuos Industriales', 'Tractocamion para residuos peligrosos', '/images/vehiculos/kenworth-t680.jpg'),
('VH-002', 'Volvo', 'FH16', 2023, 'Rojo', 'DEF-5678', 18.0, 'E', 'Diesel', 'Residuos Industriales,Residuos de Construccion', 'Camion de carga pesada', '/images/vehiculos/volvo-fh16.jpg'),
('VH-003', 'Mercedes-Benz', 'Actros', 2021, 'Gris', 'GHI-9012', 14.0, 'E', 'Diesel', 'Residuos Peligrosos', 'Transporte de materiales peligrosos', '/images/vehiculos/mercedes-actros.jpg'),
('VH-004', 'Ford', 'F-550', 2023, 'Blanco', 'JKL-3456', 4.5, 'C', 'Diesel', 'Residuos Biologicos,Residuos Reciclables', 'Camion para recoleccion urbana', '/images/vehiculos/ford-f550.jpg'),
('VH-005', 'International', 'HV Series', 2022, 'Azul', 'MNO-7890', 12.0, 'E', 'Diesel', 'Residuos Industriales', 'Camion para industria pesada', '/images/vehiculos/international-hv.jpg');

-- ============================================
-- VISTAS UTILES
-- ============================================
CREATE VIEW v_vehiculos_activos AS
SELECT 
    id,
    numero_economico,
    marca,
    modelo,
    anio,
    color,
    placas,
    peso_toneladas,
    licencia_requerida,
    tipo_gasolina,
    tipo_desecho,
    descripcion,
    foto_url
FROM vehiculos
WHERE activo = TRUE;

-- ============================================
-- PROCEDIMIENTOS ALMACENADOS
-- ============================================

DELIMITER //

CREATE PROCEDURE sp_buscar_vehiculos(
    IN p_search VARCHAR(100)
)
BEGIN
    SELECT * FROM vehiculos 
    WHERE activo = TRUE 
    AND (
        marca LIKE CONCAT('%', p_search, '%')
        OR modelo LIKE CONCAT('%', p_search, '%')
        OR placas LIKE CONCAT('%', p_search, '%')
        OR numero_economico LIKE CONCAT('%', p_search, '%')
        OR tipo_desecho LIKE CONCAT('%', p_search, '%')
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