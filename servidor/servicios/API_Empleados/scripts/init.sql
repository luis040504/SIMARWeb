CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS roles (
    id_role UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name_role VARCHAR(50) NOT NULL UNIQUE,
    description TEXT,
    -- JSONB para guardar una lista de permisos como: ["empleados.leer", "empleados.crear"]
    permissions JSONB NOT NULL DEFAULT '[]'
);

CREATE TABLE IF NOT EXISTS employees (
    user_id UUID PRIMARY KEY, -- ID proveniente del Microservicio de Auth
    full_name VARCHAR(150) NOT NULL,
    address TEXT,
    birthday DATE,
    curp VARCHAR(18) UNIQUE,
    rfc VARCHAR(13) UNIQUE,
    phone VARCHAR(15),
    genre VARCHAR(20),
    salary DECIMAL(12, 2),
    state INT DEFAULT 1, -- 1: Activo, 0: Inactivo
    register_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    id_role UUID REFERENCES roles(id_role),
    
    manager_id UUID REFERENCES employees(user_id)
);

-- tabla de chofer
CREATE TABLE IF NOT EXISTS driver_details (
    employee_id UUID PRIMARY KEY REFERENCES employees(user_id) ON DELETE CASCADE,
    license_number VARCHAR(50) NOT NULL,
    license_type VARCHAR(10) NOT NULL
);

-- Para Vendedores, Administradores, Dueños, etc.
CREATE TABLE IF NOT EXISTS professional_staff (
    employee_id UUID PRIMARY KEY REFERENCES employees(user_id) ON DELETE CASCADE,
    professional_id VARCHAR(50) NOT NULL -- Cédula o ID profesional
);

INSERT INTO roles (name_role, description, permissions) VALUES 
(
    'Owner', 
    'Dueño con acceso total', 
    '["all.manage", "reports.view", "system.config"]'
),
(
    'Admin', 
    'Administrador de SIMAR', 
    '["employee.create", "employee.edit", "employee.view"]'
);