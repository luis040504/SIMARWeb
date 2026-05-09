-- Migración: agrega contrato_id a manifiestos y ajusta id_cliente para clientes externos
USE simar_manifiestos_db;

ALTER TABLE manifiestos
    MODIFY COLUMN id_cliente INT NOT NULL DEFAULT 0,
    ADD COLUMN contrato_id INT NULL AFTER id_cliente,
    ADD INDEX idx_contrato (contrato_id);
