CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TYPE role_enum AS ENUM ('empleado', 'cliente');

CREATE TABLE IF NOT EXISTS users (
    id_user UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    username VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(100) NOT NULL UNIQUE,
    role role_enum NOT NULL,
    password_hash TEXT NOT NULL, -- para las contraseñas hasheadas
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Inserción en la base de datos de Usuarios
INSERT INTO users (
    id_user, 
    username, 
    email, 
    role, 
    password_hash, 
    is_active
) 
VALUES (
    '00000000-0000-0000-0000-000000000001', 
    'admin_root', 
    'admin@simar.com', 
    'empleado',
    '$2b$12$KlSdkTSRu6zq/9hQdFYyrue1DD7IDuyjJfgjZXNj1kCgz2ysdlbCK', -- prueba_password123!
    TRUE
) ON CONFLICT (username) DO NOTHING;