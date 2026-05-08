CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 1. TABLA DE ROLES
CREATE TABLE IF NOT EXISTS roles (
    id_role UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name_role VARCHAR(50) NOT NULL UNIQUE,
    description TEXT,
    -- JSONB para permisos: ["modulo.accion"]
    permissions JSONB NOT NULL DEFAULT '[]'
);

-- 2. TABLA DE EMPLEADOS
CREATE TABLE IF NOT EXISTS employees (
    user_id UUID PRIMARY KEY, -- Proviene de API_Usuarios
    full_name VARCHAR(150) NOT NULL,
    address TEXT,
    birthday DATE,
    curp VARCHAR(18) UNIQUE,
    rfc VARCHAR(13) UNIQUE,
    phone VARCHAR(15),
    genre VARCHAR(20),
    salary DECIMAL(12, 2),
    state INT DEFAULT 1, 
    register_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    id_role UUID REFERENCES roles(id_role),
    manager_id UUID REFERENCES employees(user_id)
);

-- 3. TABLA DE CHOFER (Especialidad)
CREATE TABLE IF NOT EXISTS driver_details (
    employee_id UUID PRIMARY KEY REFERENCES employees(user_id) ON DELETE CASCADE,
    license_number VARCHAR(50) NOT NULL,
    license_type VARCHAR(10) NOT NULL
);

-- 4. TABLA DE STAFF PROFESIONAL (Especialidad)
CREATE TABLE IF NOT EXISTS professional_staff (
    employee_id UUID PRIMARY KEY REFERENCES employees(user_id) ON DELETE CASCADE,
    professional_id VARCHAR(50) NOT NULL 
);

---
--- INSERCIÓN DE ROLES PREDEFINIDOS
---
INSERT INTO roles (name_role, description, permissions) VALUES 
(
    'dueño', 
    'Acceso total al sistema y configuración global', 
    '["all.manage", "reports.view", "system.config", "users.manage"]'
),
(
    'administrador', 
    'Gestión de personal, nóminas y control operativo', 
    '["employee.create", "employee.edit", "employee.view", "reports.view"]'
),
(
    'contador', 
    'Acceso a reportes financieros y gestión de salarios', 
    '["finance.view", "reports.view", "salary.manage"]'
),
(
    'vendedor', 
    'Gestión de clientes y registro de ventas', 
    '["sales.create", "customer.view", "catalog.view"]'
),
(
    'tecnico', 
    'Registro de mantenimientos y estados de equipo', 
    '["maintenance.manage", "inventory.view", "tasks.update"]'
),
(
    'chofer', 
    'Visualización de rutas y estados de entrega', 
    '["routes.view", "delivery.update", "personal.view"]'
)
ON CONFLICT (name_role) DO NOTHING;