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
    professional_id VARCHAR(10) NOT NULL UNIQUE,
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

-- 3. TABLA DE CHOFER
CREATE TABLE IF NOT EXISTS driver_details (
    employee_id UUID PRIMARY KEY REFERENCES employees(user_id) ON DELETE CASCADE,
    license_number VARCHAR(50) NOT NULL,
    license_type VARCHAR(10) NOT NULL
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


DO $$ 
DECLARE 
    role_id_admin UUID;
BEGIN
    SELECT id_role INTO role_id_admin FROM roles WHERE name_role = 'administrador';

    INSERT INTO employees (
        user_id, 
        professional_id, 
        full_name, 
        address, 
        birthday, 
        curp, 
        rfc, 
        phone, 
        genre, 
        salary, 
        state, 
        id_role
    )
    VALUES (
        '00000000-0000-0000-0000-000000000001', 
        'EMP-001',                            
        'Administrador Root SIMAR',           
        'Av. Principal No. 123, Col. Centro',  
        '1990-01-01',                         
        'ROOT000000HDFRRR01',                 
        'ROOT000000AA1',                      
        '2281000000',                         
        'Masculino',                          
        75000.00,                             
        1,                                    
        role_id_admin                         
    ) ON CONFLICT (user_id) DO NOTHING;

    -- Nota: Al ser administrador, no necesita registro en driver_details 
    -- a menos que también fuera a manejar vehículos.

END $$;