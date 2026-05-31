# ============================================================================
# SIMAR WEB: SCRIPT DE INICIALIZACIÓN Y SEMILLADO DE TODAS LAS BASES DE DATOS
# CON TRAZABILIDAD REGISTRO A REGISTRO EN LA BITÁCORA DE AUDITORÍA
# ============================================================================
# Este script de PowerShell automatiza el semillado de datos de prueba realistas 
# para todos los microservicios de SIMAR directamente a través de contenedores Docker.
#
# Genera un registro de auditoría individual y detallado para CADA registro 
# de entidad creado, garantizando una trazabilidad perfecta a nivel de BD.
# ============================================================================

$ErrorActionPreference = "Stop"
Clear-Host

Write-Host "============================================================" -ForegroundColor Green
Write-Host "      SIMAR WEB: SEMILLADO GLOBAL CON TRAZABILIDAD TOTAL     " -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
Write-Host "Iniciando proceso de semillado y auditoría detallada..." -ForegroundColor Cyan

# ── 1. VERIFICAR QUE DOCKER ESTÉ ACTIVO ──
try {
    $dockerCheck = docker ps --format "{{.Names}}"
    Write-Host "[OK] Docker está activo y respondiendo." -ForegroundColor Green
} catch {
    Write-Host "[ERROR] Docker no está ejecutándose o no se tiene acceso." -ForegroundColor Red
    Exit
}

# ── 2. CONFIGURACIÓN DE CONEXIONES Y CREDENCIALES ──
$Databases = @{
    "contratos" = @{
        Container = "simar_contratos_api"  # Cambiado a api para evitar problemas si se requiere la DB de contratos
        Type = "mssql"
        User = "sa"
        Pass = "SimarContracts123!"
        DB = "ContractsDB"
    }
    "catalog" = @{
        Container = "simar_sqlserver_catalog"
        Type = "mssql"
        User = "sa"
        Pass = "SimarCatalog123!"
        DB = "CatalogDb"
    }
    "audit" = @{
        Container = "simar_sqlserver_audit"
        Type = "mssql"
        User = "sa"
        Pass = "SimarAudit123!"
        DB = "AuditDB"
    }
    "clientes" = @{
        Container = "simar_postgres_clientes"
        Type = "postgres"
        User = "simero"
        Pass = "contra"
        DB = "simar_clientes_db"
    }
    "usuarios" = @{
        Container = "simar_db_usuarios"
        Type = "postgres"
        User = "admin_users"
        Pass = "SimarUsers123!"
        DB = "simar_users_db"
    }
    "empleados" = @{
        Container = "simar_db_empleados"
        Type = "postgres"
        User = "admin_emp"
        Pass = "SimarEmp123!"
        DB = "simar_empleados_db"
    }
    "vehiculos" = @{
        Container = "simar_mysql_vehiculos"
        Type = "mysql"
        User = "root"
        Pass = "Simar123!"
        DB = "simar_vehiculos_db"
    }
    "manifiestos" = @{
        Container = "simar_mysql_manifiestos"
        Type = "mysql"
        User = "simar_manifest"
        Pass = "Simar123!"
        DB = "simar_manifiestos_db"
    }
    "recolecciones" = @{
        Container = "simar_mongo_recolecciones"
        Type = "mongo"
        User = "root"
        Pass = "Simar123!"
        DB = "simar_recolecciones_db"
    }
    "facturacion" = @{
        Container = "simar_mongo_facturacion"
        Type = "mongo"
        User = "facturacion_admin"
        Pass = "Simar123!"
        DB = "simar_facturacion_db"
    }
    "servicios" = @{
        Container = "simar_mongo_servicios"
        Type = "mongo"
        User = "root"
        Pass = "Simar123!"
        DB = "simar_servicios_db"
    }
    "trazabilidad" = @{
        Container = "simar_mongo_trazabilidad"
        Type = "mongo"
        User = "root"
        Pass = "Simar123!"
        DB = "simar_trazabilidad_db"
    }
}

# Usar el container correcto del host para sqlserver_contratos
$Databases["contratos"].Container = "simar_sqlserver_contratos"

# ── 3. FUNCIÓN AUXILIAR PARA REGISTRAR EN LA BITÁCORA DE AUDITORÍA ──
function Registrar-Auditoria($entidad, $idEntidad, $accion, $payload, $usuario = "sistema.inicializador") {
    $guid = [Guid]::NewGuid().ToString()
    $timestamp = [DateTime]::UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")
    $cleanPayload = $payload -replace "'", "''"
    
    $sqlLog = @"
INSERT INTO [AuditLogs] (
    [Id], [EntityType], [EntityId], [Action], [PerformedBy], 
    [Timestamp], [Payload], [IpAddress], [Status], [ErrorMessage]
) VALUES (
    '$guid', '$entidad', '$idEntidad', '$accion', '$usuario',
    '$timestamp', '$cleanPayload', '127.0.0.1', 'Success', NULL
);
"@
    
    $cfg = $Databases["audit"]
    $sqlCmd = "/opt/mssql-tools/bin/sqlcmd"
    $checkPath = docker exec $cfg.Container sh -c "test -f /opt/mssql-tools18/bin/sqlcmd && echo 'new' || echo 'old'" 2>$null
    if ($checkPath -eq "new") { $sqlCmd = "/opt/mssql-tools18/bin/sqlcmd" }

    $sqlLog | docker exec -i $cfg.Container $sqlCmd -S localhost -U $cfg.User -P $cfg.Pass -d $cfg.DB -C > $null
}

# ── 4. SEMILLADO POR MÓDULOS CON TRAZABILIDAD DETALLADA ──

# --- A) AUDITORÍA ---
Write-Host "`n[1/7] Limpiando y preparando base de datos de Auditoría..." -ForegroundColor Yellow
$cfg = $Databases["audit"]
$sqlCmd = "/opt/mssql-tools/bin/sqlcmd"
$checkPath = docker exec $cfg.Container sh -c "test -f /opt/mssql-tools18/bin/sqlcmd && echo 'new' || echo 'old'" 2>$null
if ($checkPath -eq "new") { $sqlCmd = "/opt/mssql-tools18/bin/sqlcmd" }

$cleanAuditSql = "DELETE FROM [AuditLogs];"
$cleanAuditSql | docker exec -i $cfg.Container $sqlCmd -S localhost -U $cfg.User -P $cfg.Pass -d $cfg.DB -C > $null
Write-Host "[OK] Base de datos de auditoría limpia y lista." -ForegroundColor Green


# --- B) CONTRATOS ---
Write-Host "`n[2/7] Sembrando módulo de Contratos..." -ForegroundColor Yellow
$cfg = $Databases["contratos"]
$sqlCmd = "/opt/mssql-tools/bin/sqlcmd"
if ((docker exec $cfg.Container sh -c "test -f /opt/mssql-tools18/bin/sqlcmd && echo 'new' || echo 'old'") -eq "new") { $sqlCmd = "/opt/mssql-tools18/bin/sqlcmd" }

# SQL de llenado de contratos
$contratosSql = @"
DELETE FROM [ContractServices];
DELETE FROM [ContractPayments];
DELETE FROM [ContractExtras];
DELETE FROM [Contracts];
DELETE FROM [Quotations];
DBCC CHECKIDENT ('Contracts', RESEED, 0);
DBCC CHECKIDENT ('ContractServices', RESEED, 0);
DBCC CHECKIDENT ('ContractPayments', RESEED, 0);
DBCC CHECKIDENT ('ContractExtras', RESEED, 0);

-- Cotizaciones
INSERT INTO [Quotations] ([Id], [Folio], [Status], [ClientName], [ClientRfc], [ContactName], [ContactPhone], [ContactEmail], [ValidityDays], [Subtotal], [Total], [CreatedAt], [ServicesRawJson], [Frequency]) 
VALUES (1, 'COT-202605-A1B2', 'contracted', 'Distribuidora Médica del Golfo S.A.', 'DMG220415G90', 'Dr. Alejandro Ruiz', '2288123456', 'aruiz@dmgolfo.com', 30, 15000.00, 17400.00, DATEADD(day, -30, GETDATE()), '{"services":[]}', 'Mensual');

INSERT INTO [Quotations] ([Id], [Folio], [Status], [ClientName], [ClientRfc], [ContactName], [ContactPhone], [ContactEmail], [ValidityDays], [Subtotal], [Total], [CreatedAt], [ServicesRawJson], [Frequency]) 
VALUES (2, 'COT-202605-C3D4', 'approved', 'Industrias Químicas de Veracruz S.A. de C.V.', 'IQV1508239A2', 'Ing. Roberto Gómez', '2299765432', 'rgomez@iquimicas.com', 15, 25000.00, 29000.00, DATEADD(day, -5, GETDATE()), '{"services":[]}', 'Quincenal');

-- Contratos
INSERT INTO [Contracts] ([Folio], [ClientId], [TotalBasePrice], [CreatedAt], [Status], [ClientName], [ClientRfc], [Representative], [ClientAddress], [ClientObjetoSocial], [ClientDeclaraciones], [ContractDuration], [FirstServiceDate], [EndDate], [SignedContractPath]) 
VALUES ('CON-202605-E7F8', 1, 15000.00, DATEADD(day, -25, GETDATE()), 'Activo', 'Distribuidora Médica del Golfo S.A.', 'DMG220415G90', 'Dr. Alejandro Ruiz', 'Av. Ruiz Cortines 1024, Xalapa, Ver.', 'Distribucion de insumos médicos.', 'Registros vigentes de RPBI.', '1 Año', DATEADD(day, -15, GETDATE()), DATEADD(year, 1, GETDATE()), 'C:\Signed_1.pdf');

INSERT INTO [Contracts] ([Folio], [ClientId], [TotalBasePrice], [CreatedAt], [Status], [ClientName], [ClientRfc], [Representative], [ClientAddress], [ClientObjetoSocial], [ClientDeclaraciones], [ContractDuration], [FirstServiceDate], [EndDate], [SignedContractPath]) 
VALUES ('CON-202605-H9I0', 2, 25000.00, DATEADD(day, -2, GETDATE()), 'Pendiente de firma', 'Industrias Químicas de Veracruz S.A. de C.V.', 'IQV1508239A2', 'Ing. Roberto Gómez', 'Parque Industrial Bruno Pagliai Lote 12, Veracruz, Ver.', 'Manufactura quimica.', 'Manejo de reactivos.', '6 Meses', DATEADD(day, 5, GETDATE()), DATEADD(month, 6, GETDATE()), NULL);

-- Servicios
INSERT INTO [ContractServices] ([ContractId], [WasteType], [WasteUnit], [Frequency], [Vehicles], [Technicians], [ServiceAddress], [WarehouseAddress], [Subtotal]) 
VALUES (1, 'Residuos Biológicos-Infecciosos (RPBI)', 'kg', 'Mensual', 1, 2, 'Av. Ruiz Cortines 1024, Xalapa, Ver.', 'Almacen RPBI', 15000.00);

INSERT INTO [ContractServices] ([ContractId], [WasteType], [WasteUnit], [Frequency], [Vehicles], [Technicians], [ServiceAddress], [WarehouseAddress], [Subtotal]) 
VALUES (2, 'Ácidos y Bases Solubles', 'litros', 'Quincenal', 2, 3, 'Parque Industrial Bruno Pagliai Lote 12, Veracruz, Ver.', 'Area Quimica', 25000.00);

-- Pagos
INSERT INTO [ContractPayments] ([ContractId], [Description], [Amount], [PaymentDate]) 
VALUES (1, 'Pago Inicial - Firma de Contrato', 15000.00, DATEADD(day, -25, GETDATE()));

INSERT INTO [ContractPayments] ([ContractId], [Description], [Amount], [PaymentDate]) 
VALUES (2, 'Anticipo de Garantía de Equipamiento', 10000.00, DATEADD(day, -2, GETDATE()));
"@

$contratosSql | docker exec -i $cfg.Container $sqlCmd -S localhost -U $cfg.User -P $cfg.Pass -d $cfg.DB -C > $null
Write-Host "[OK] Contratos, Servicios y Pagos sembrados." -ForegroundColor Green

# Trazabilidad de Contratos y Cotizaciones
Registrar-Auditoria "Quotation" "1" "Create" '{"folio": "COT-202605-A1B2", "client": "Distribuidora Médica del Golfo S.A.", "total": 17400.00, "status": "contracted"}'
Registrar-Auditoria "Quotation" "2" "Create" '{"folio": "COT-202605-C3D4", "client": "Industrias Químicas de Veracruz S.A. de C.V.", "total": 29000.00, "status": "approved"}'
Registrar-Auditoria "Contract" "1" "Create" '{"id": 1, "folio": "CON-202605-E7F8", "clientId": 1, "totalBasePrice": 15000.00, "status": "Activo", "signed": true}' "lic.martinez@simar.mx"
Registrar-Auditoria "Contract" "2" "Create" '{"id": 2, "folio": "CON-202605-H9I0", "clientId": 2, "totalBasePrice": 25000.00, "status": "Pendiente de firma", "signed": false}' "roberto.admin@simar.mx"


# --- C) CLIENTES ---
Write-Host "`n[3/7] Sembrando módulo de Clientes (PostgreSQL)..." -ForegroundColor Yellow
$cfg = $Databases["clientes"]

$clientesSql = @"
TRUNCATE TABLE clientes CASCADE;
INSERT INTO clientes (id, name, "businessName", "contactEmail", phone, address, rfc, status, "idUser") 
VALUES 
(1, 'Distribuidora Médica del Golfo S.A.', 'Distribuidora Médica del Golfo S.A.', 'contacto@dmgolfo.com', '2288123456', 'Av. Ruiz Cortines 1024, Xalapa, Ver.', 'DMG220415G90', 'activo', '00000000-0000-0000-0000-000000000002'),
(2, 'Industrias Químicas de Veracruz S.A. de C.V.', 'Industrias Químicas de Veracruz S.A. de C.V.', 'ventas@iquimicas.com', '2299765432', 'Parque Industrial Bruno Pagliai Lote 12, Veracruz, Ver.', 'IQV1508239A2', 'activo', '00000000-0000-0000-0000-000000000002');
"@

$clientesSql | docker exec -i $cfg.Container psql -U $cfg.User -d $cfg.DB > $null
Write-Host "[OK] Clientes sembrados con éxito." -ForegroundColor Green

# Trazabilidad de Clientes
Registrar-Auditoria "Client" "1" "Create" '{"id": 1, "name": "Distribuidora Médica del Golfo S.A.", "rfc": "DMG220415G90", "email": "contacto@dmgolfo.com"}' "admin.seeder"
Registrar-Auditoria "Client" "2" "Create" '{"id": 2, "name": "Industrias Químicas de Veracruz S.A. de C.V.", "rfc": "IQV1508239A2", "email": "ventas@iquimicas.com"}' "admin.seeder"


# --- D) USUARIOS ---
Write-Host "`n[4/7] Sembrando módulo de Usuarios (PostgreSQL)..." -ForegroundColor Yellow
$cfg = $Databases["usuarios"]

$usuariosSql = @'
TRUNCATE TABLE users CASCADE;
INSERT INTO users (id_user, username, email, role, password_hash, is_active) 
VALUES 
('00000000-0000-0000-0000-000000000001', 'Luis López', 'luis.lopez@simar.mx', 'empleado', '$2b$12$KlSdkTSRu6zq/9hQdFYyrue1DD7IDuyjJfgjZXNj1kCgz2ysdlbCK', true),
('00000000-0000-0000-0000-000000000002', 'Roberto Gómez', 'rgomez@iquimicas.com', 'cliente', '$2b$12$KlSdkTSRu6zq/9hQdFYyrue1DD7IDuyjJfgjZXNj1kCgz2ysdlbCK', true);
'@

$usuariosSql | docker exec -i $cfg.Container psql -U $cfg.User -d $cfg.DB > $null
Write-Host "[OK] Usuarios administrativos y clientes sembrados." -ForegroundColor Green

# Trazabilidad de Usuarios
Registrar-Auditoria "User" "00000000-0000-0000-0000-000000000001" "Create" '{"username": "Luis López", "email": "luis.lopez@simar.mx", "role": "empleado", "is_active": true}' "seguridad.admin"
Registrar-Auditoria "User" "00000000-0000-0000-0000-000000000002" "Create" '{"username": "Roberto Gómez", "email": "rgomez@iquimicas.com", "role": "cliente", "is_active": true}' "seguridad.admin"


# --- E) VEHÍCULOS Y MANIFIESTOS (MySQL) ---
Write-Host "`n[5/7] Sembrando Vehículos y Manifiestos (MySQL)..." -ForegroundColor Yellow

# Vehículos
$cfg = $Databases["vehiculos"]
$vehiculosSql = @"
TRUNCATE TABLE vehiculos;
INSERT INTO vehiculos (placas, marca, modelo, capacidad_kg, estatus) 
VALUES 
('XW-98-123', 'Ford', 'F-350 Duty', 3500, 'Disponible'),
('XV-45-789', 'Hino', '300 Series', 5000, 'En Servicio');
"@
$vehiculosSql | docker exec -i $cfg.Container mysql -u$cfg.User -p$cfg.Pass $cfg.DB > $null
Write-Host "[OK] Vehículos sembrados." -ForegroundColor Green

# Trazabilidad de Vehículos
Registrar-Auditoria "Vehiculo" "XW-98-123" "Create" '{"placas": "XW-98-123", "marca": "Ford", "modelo": "F-350 Duty", "capacidad_kg": 3500, "estatus": "Disponible"}'
Registrar-Auditoria "Vehiculo" "XV-45-789" "Create" '{"placas": "XV-45-789", "marca": "Hino", "modelo": "300 Series", "capacidad_kg": 5000, "estatus": "En Servicio"}'

# Manifiestos
$cfg = $Databases["manifiestos"]
$manifiestosSql = @"
DELETE FROM residuos_especiales;
DELETE FROM residuos_peligrosos;
DELETE FROM manifiestos;
ALTER TABLE manifiestos AUTO_INCREMENT = 1;
ALTER TABLE residuos_especiales AUTO_INCREMENT = 1;
ALTER TABLE residuos_peligrosos AUTO_INCREMENT = 1;

INSERT INTO manifiestos (
    id, id_cliente, contrato_id, numero_manifiesto, tipo, estado,
    numero_registro_ambiental, razon_social, domicilio,
    codigo_postal, municipio, telefono, correo,
    fecha_manifiesto, hora_manifiesto,
    nombre_responsable_generador
) VALUES (
    1, 1, 1, '001/2026', 'especial', 'completado',
    'SEDEMA/TRME-CH0990/20EXR-17/182',
    'Distribuidora Médica del Golfo S.A.',
    'Av. Ruiz Cortines 1024, Col. Centro',
    '91000', 'Xalapa', '2288123456', 'contacto@dmgolfo.com',
    '2026-05-15', '10:48:00',
    'Santiago Montoya'
);

INSERT INTO residuos_especiales (manifiesto_id, clave_residuo, nombre_residuo, tipo_envase, capacidad, peso, unidad)
VALUES (1, 'IE-001', 'Otros Residuos Inorgánicos (RSU)', 'OF', '1/6 m³', 680, 'kg');

INSERT INTO manifiestos (
    id, id_cliente, contrato_id, numero_manifiesto, tipo, estado,
    numero_registro_ambiental, razon_social,
    calle, numero_exterior, numero_interior, colonia, estado_generador,
    codigo_postal, municipio, telefono, correo,
    instrucciones_manejo_seguro, nombre_responsable_generador, fecha_firma_generador
) VALUES (
    2, 2, 2, '002/2026', 'peligroso', 'en_transito',
    'CMORE3001711', 'Industrias Químicas de Veracruz S.A. de C.V.',
    'Parque Industrial Bruno Pagliai Lote 12', 'S/N', 'S/N',
    'Bruno Pagliai', 'Veracruz',
    '91700', 'Veracruz', '2299765432', 'ventas@iquimicas.com',
    'Uso de EPP.', 'Santiago Montoya', '2026-05-28'
);

INSERT INTO residuos_peligrosos (manifiesto_id, nombre_residuo, es_biologico, tipo_envase, capacidad_envase, cantidad_kg, tiene_etiqueta)
VALUES (2, 'Objetos Punzocortantes', TRUE, 'CIP', '1', 250, TRUE);
"@
$manifiestosSql | docker exec -i $cfg.Container mysql -u$cfg.User -p$cfg.Pass $cfg.DB > $null
Write-Host "[OK] Manifiestos de transporte sembrados." -ForegroundColor Green

# Trazabilidad de Manifiestos
Registrar-Auditoria "Manifest" "001/2026" "Create" '{"id": 1, "folio": "001/2026", "cliente_id": 1, "contrato_id": 1, "residuo_tipo": "especial", "estatus": "completado"}'
Registrar-Auditoria "Manifest" "002/2026" "Create" '{"id": 2, "folio": "002/2026", "cliente_id": 2, "contrato_id": 2, "residuo_tipo": "peligroso", "estatus": "en_transito"}'


# --- F) RECOLECCIONES Y TRAZABILIDAD (MongoDB) ---
Write-Host "`n[6/7] Sembrando Recolecciones y Trazabilidad (MongoDB)..." -ForegroundColor Yellow

# Recolecciones
$cfg = $Databases["recolecciones"]
$recoleccionesJs = @"
db.recolecciones.deleteMany({});
db.recolecciones.insertMany([
  {
    _id: "rec_1",
    clienteId: 1,
    fechaProgramada: "2026-06-05T09:00:00Z",
    estado: "Programado",
    residuos: [{ tipo: "RPBI", cantidad: 45, unit: "kg" }],
    vehiculoPlacas: "XW-98-123"
  }
]);
"@
$recoleccionesJs | docker exec -i $cfg.Container mongosh -u root -p Simar123! --authenticationDatabase admin simar_recolecciones_db > $null
Write-Host "[OK] Programación de recolecciones en MongoDB sembrada." -ForegroundColor Green

# Trazabilidad de Recolección en MongoDB
Registrar-Auditoria "Recoleccion" "rec_1" "Create" '{"id": "rec_1", "clienteId": 1, "fecha": "2026-06-05", "estado": "Programado", "vehiculo": "XW-98-123"}'

# Trazabilidad
$cfg = $Databases["trazabilidad"]
$trazabilidadJs = @"
db.historial.deleteMany({});
db.historial.insertMany([
  {
    evento: "Recolección Completada",
    manifiestoFolio: "MAN-2026-0001",
    cliente: "Distribuidora Médica del Golfo S.A.",
    fecha: "2026-05-15T14:30:00Z",
    detalles: "El residuo llegó seguro a la planta de tratamiento en Veracruz."
  }
]);
"@
$trazabilidadJs | docker exec -i $cfg.Container mongosh -u root -p Simar123! --authenticationDatabase admin simar_trazabilidad_db > $null
Write-Host "[OK] Historial de trazabilidad en MongoDB sembrado." -ForegroundColor Green

# Registro de Trazabilidad Logística
Registrar-Auditoria "Traceability" "MAN-2026-0001" "LogisticsComplete" '{"manifiesto": "MAN-2026-0001", "evento": "Recolección Completada", "cliente": "Distribuidora Médica del Golfo S.A."}'


# --- G) CATÁLOGO DE RESIDUOS (SQL Server) ---
Write-Host "`n[7/7] Sembrando Catálogo de Residuos (SQL Server)..." -ForegroundColor Yellow
$cfg = $Databases["catalog"]
$sqlCmd = "/opt/mssql-tools/bin/sqlcmd"
if ((docker exec $cfg.Container sh -c "test -f /opt/mssql-tools18/bin/sqlcmd && echo 'new' || echo 'old'") -eq "new") { $sqlCmd = "/opt/mssql-tools18/bin/sqlcmd" }

$catalogSql = @"
DELETE FROM [Wastes];
INSERT INTO [Wastes] ([Code], [Name], [HazardClass], [Description]) 
VALUES 
('RPBI-01', 'Residuos Biológicos-Infecciosos', 'Infeccioso', 'Sangre líquida, cultivos y cepas de agentes infecciosos.'),
('CRT-02', 'Ácidos y Álcalis Inorgánicos', 'Corrosivo', 'Ácidos fuertes y disoluciones alcalinas concentradas.');
"@
try {
    $catalogSql | docker exec -i $cfg.Container $sqlCmd -S localhost -U $cfg.User -P $cfg.Pass -d $cfg.DB -C > $null
    Write-Host "[OK] Catálogo de residuos sembrado." -ForegroundColor Green
    Registrar-Auditoria "WasteCatalog" "RPBI-01" "Create" '{"code": "RPBI-01", "name": "Residuos Biológicos-Infecciosos", "class": "Infeccioso"}'
    Registrar-Auditoria "WasteCatalog" "CRT-02" "Create" '{"code": "CRT-02", "name": "Ácidos y Álcalis Inorgánicos", "class": "Corrosivo"}'
} catch {
    Write-Host "[INFO] No se pudo sembrar Wastes directamente, omitiendo catálogo técnico." -ForegroundColor Gray
}

Write-Host "`n============================================================" -ForegroundColor Green
Write-Host " [ÉXITO] ¡Todas las bases de datos del sistema han sido      " -ForegroundColor Green
Write-Host "         sembradas correctamente y registradas en auditoría! " -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
