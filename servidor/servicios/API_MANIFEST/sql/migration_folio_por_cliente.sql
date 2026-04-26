USE simar_manifiestos_db;

CREATE TABLE IF NOT EXISTS secuencias_manifiesto (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    id_cliente          INT NOT NULL,
    tipo                ENUM('especial', 'peligroso') NOT NULL,
    anio                YEAR NOT NULL,
    ultimo_consecutivo  INT NOT NULL DEFAULT 0,
    UNIQUE KEY uk_cliente_tipo_anio (id_cliente, tipo, anio)
);

ALTER TABLE manifiestos
    ADD COLUMN IF NOT EXISTS id_cliente INT NOT NULL DEFAULT 0 AFTER id;

ALTER TABLE manifiestos DROP INDEX IF EXISTS numero_manifiesto;
ALTER TABLE manifiestos
    ADD UNIQUE KEY IF NOT EXISTS uk_folio_cliente_tipo (id_cliente, tipo, numero_manifiesto);

CREATE INDEX IF NOT EXISTS idx_cliente ON manifiestos (id_cliente);

