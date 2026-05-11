# ─────────────────────────────────────────────────────────────────────────────
# seed_demo.ps1  —  Datos de demo para probar generación de manifiestos
#
# Uso: .\seed_demo.ps1
# Requisito: contenedores corriendo  →  docker compose up -d
# ─────────────────────────────────────────────────────────────────────────────

$POSTGRES_CONTAINER   = "simar_postgres_clientes"
$SQLSERVER_CONTAINER  = "simar_sqlserver_contratos"
$POSTGRES_USER        = "simar_user"
$POSTGRES_DB          = "simar_clientes_db"
$SA_PASSWORD          = "Simar123!"
$MYSQL_CONTAINER      = "simar_mysql_vehiculos"
$MYSQL_ROOT_PASS      = "Simar123!"
$MYSQL_DB             = "simar_vehiculos_db"
$EMPLEADOS_CONTAINER  = "simar_db_empleados"
$EMPLEADOS_USER       = "simar_user"
$EMPLEADOS_DB         = "simar_empleados_db"

Write-Host ""
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "  SIMAR  -  Seed de datos para demo"             -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan

# ── 0. VEHICULOS EN MYSQL ─────────────────────────────────────────────────────
Write-Host ""
Write-Host "[0/4]  Inicializando base de datos de vehiculos (MySQL)..." -ForegroundColor Yellow

$vehiculosInitSql = @'
CREATE TABLE IF NOT EXISTS tipos_desecho (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(100) NOT NULL UNIQUE,
    descripcion TEXT,
    activo BOOLEAN DEFAULT TRUE
);
CREATE TABLE IF NOT EXISTS tipos_gasolina (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(50) NOT NULL UNIQUE,
    descripcion TEXT
);
CREATE TABLE IF NOT EXISTS vehiculos (
    id INT PRIMARY KEY AUTO_INCREMENT,
    numero_economico VARCHAR(20) UNIQUE,
    marca VARCHAR(50) NOT NULL,
    modelo VARCHAR(50) NOT NULL,
    anio INT,
    color VARCHAR(30),
    placas VARCHAR(15) NOT NULL UNIQUE,
    peso_toneladas DECIMAL(8,2) NOT NULL,
    licencia_requerida ENUM('A','B','C','D','E') NOT NULL,
    tipo_gasolina VARCHAR(30) NOT NULL,
    tipo_desecho TEXT NOT NULL,
    descripcion TEXT,
    foto_url VARCHAR(500),
    activo BOOLEAN DEFAULT TRUE,
    fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);
INSERT IGNORE INTO tipos_desecho (nombre, descripcion) VALUES
    ('Residuos Peligrosos','Materiales que representan un riesgo para la salud o el medio ambiente'),
    ('Residuos Biologicos','Desechos provenientes de actividades medicas o de laboratorio'),
    ('Residuos Reciclables','Papel, carton, plastico, vidrio y metales'),
    ('Residuos Organicos','Desechos de origen biologico como alimentos y poda'),
    ('Residuos de Construccion','Escombros, tierra, concreto y materiales de demolicion'),
    ('Residuos Electronicos','Equipos electronicos en desuso'),
    ('Residuos Industriales','Subproductos de procesos industriales'),
    ('Residuos Varios','Otros tipos de residuos no clasificados');
INSERT IGNORE INTO tipos_gasolina (nombre, descripcion) VALUES
    ('Diesel','Combustible para motores diesel'),
    ('Gasolina Magna','Gasolina regular de 87 octanos'),
    ('Gasolina Premium','Gasolina de alto octanaje (91-93 octanos)'),
    ('Gas Natural','Gas natural comprimido para vehiculos'),
    ('Electrico','Vehiculos de bateria electrica'),
    ('Hibrido','Combinacion de gasolina y electrico');
INSERT IGNORE INTO vehiculos (numero_economico, marca, modelo, anio, color, placas, peso_toneladas, licencia_requerida, tipo_gasolina, tipo_desecho, descripcion) VALUES
    ('VH-001','Kenworth','T680',2022,'Blanco','ABC-1234',15.5,'E','Diesel','Residuos Peligrosos,Residuos Industriales','Tractocamion para residuos peligrosos'),
    ('VH-002','Volvo','FH16',2023,'Rojo','DEF-5678',18.0,'E','Diesel','Residuos Industriales,Residuos de Construccion','Camion de carga pesada'),
    ('VH-003','Mercedes-Benz','Actros',2021,'Gris','GHI-9012',14.0,'E','Diesel','Residuos Peligrosos','Transporte de materiales peligrosos'),
    ('VH-004','Ford','F-550',2023,'Blanco','JKL-3456',4.5,'C','Diesel','Residuos Biologicos,Residuos Reciclables','Camion para recoleccion urbana'),
    ('VH-005','International','HV Series',2022,'Azul','MNO-7890',12.0,'E','Diesel','Residuos Industriales','Camion para industria pesada');
'@

$vehiculosResult = $vehiculosInitSql | docker exec -i $MYSQL_CONTAINER mysql -uroot -p"$MYSQL_ROOT_PASS" $MYSQL_DB 2>&1
if ($vehiculosResult -match "ERROR") {
    Write-Host "   ADVERTENCIA: $vehiculosResult" -ForegroundColor Red
} else {
    Write-Host "   Vehiculos listos (5 unidades)." -ForegroundColor Green
}

# ── 0b. CHOFERES EN POSTGRESQL (empleados) ────────────────────────────────────
Write-Host ""
Write-Host "[0b/4] Insertando choferes en empleados..." -ForegroundColor Yellow

$choferRoleSql = "SELECT id_role FROM roles WHERE name_role = 'chofer' LIMIT 1;"
$CHOFER_ROLE_ID = $choferRoleSql | docker exec -i $EMPLEADOS_CONTAINER psql -U $EMPLEADOS_USER -d $EMPLEADOS_DB -t -A 2>&1
$CHOFER_ROLE_ID = $CHOFER_ROLE_ID.Trim()

if ([string]::IsNullOrWhiteSpace($CHOFER_ROLE_ID)) {
    Write-Host "   ERROR: No se encontro el rol 'chofer' en la BD de empleados." -ForegroundColor Red
} else {
    $insertChoferesSql = @"
INSERT INTO employees (user_id, professional_id, full_name, address, birthday, curp, rfc, phone, genre, salary, state, id_role)
VALUES
  ('aaaaaaaa-0000-0000-0000-000000000001','CHO-001','Juan Carlos Mendez Lopez','Av. Insurgentes 45, Col. Centro','1988-03-15','MELJ880315HDFNPN01','MELJ880315AB1','2281112233','Masculino',18000.00,1,'$CHOFER_ROLE_ID'),
  ('aaaaaaaa-0000-0000-0000-000000000002','CHO-002','Maria Elena Torres Ruiz','Calle Reforma 78, Col. Moderna','1992-07-22','TORM920722MDFRRR01','TORM920722CD2','2282223344','Femenino',18000.00,1,'$CHOFER_ROLE_ID')
ON CONFLICT (user_id) DO NOTHING;

INSERT INTO driver_details (employee_id, license_number, license_type)
VALUES
  ('aaaaaaaa-0000-0000-0000-000000000001','LIC-E-001234','E'),
  ('aaaaaaaa-0000-0000-0000-000000000002','LIC-C-005678','C')
ON CONFLICT (employee_id) DO NOTHING;
"@
    $insertChoferesResult = $insertChoferesSql | docker exec -i $EMPLEADOS_CONTAINER psql -U $EMPLEADOS_USER -d $EMPLEADOS_DB 2>&1
    Write-Host "   Choferes listos (Juan Carlos Mendez y Maria Elena Torres)." -ForegroundColor Green
}

# ── 1. CLIENTE EN POSTGRESQL ──────────────────────────────────────────────────
Write-Host ""
Write-Host "[1/4]  Insertando cliente en PostgreSQL..." -ForegroundColor Yellow

# Pasamos el SQL por stdin (-i) para preservar las comillas dobles de los identificadores
$insertClientSql = @'
INSERT INTO clientes (name, "businessName", "contactEmail", phone, address, rfc, "semarnatNum", status, "idUser", "registerDate")
SELECT 'Juan Perez Lopez','Industrias Demo SA de CV','demo@industriasdemo.com','5512345678','Av. Reforma 123, Col. Centro, CDMX, CP 06600','IDEMO123456XYZ','CDMX-2024-001234','activo','00000000-0000-0000-0000-000000000099',NOW()
WHERE NOT EXISTS (SELECT 1 FROM clientes WHERE rfc = 'IDEMO123456XYZ')
RETURNING id;
'@

$CLIENT_ID = $insertClientSql | docker exec -i $POSTGRES_CONTAINER psql -U $POSTGRES_USER -d $POSTGRES_DB -t -A 2>&1

if ($CLIENT_ID -match "ERROR" -or [string]::IsNullOrWhiteSpace($CLIENT_ID)) {
    # Puede que ya exista, obtener el id
    $CLIENT_ID = "SELECT id FROM clientes WHERE rfc = 'IDEMO123456XYZ' LIMIT 1;" | `
        docker exec -i $POSTGRES_CONTAINER psql -U $POSTGRES_USER -d $POSTGRES_DB -t -A 2>&1
}

$CLIENT_ID = ($CLIENT_ID -replace '\D','').Trim()

if ([string]::IsNullOrWhiteSpace($CLIENT_ID)) {
    Write-Host "   ERROR: No se pudo obtener el ID del cliente." -ForegroundColor Red
    Write-Host "   Verifica que el contenedor '$POSTGRES_CONTAINER' este corriendo." -ForegroundColor Red
    exit 1
}

Write-Host "   Cliente listo - ID: $CLIENT_ID" -ForegroundColor Green

# ── 2. ESPERAR QUE ContractsDB EXISTA ────────────────────────────────────────
Write-Host ""
Write-Host "[2/4]  Verificando SQL Server..." -ForegroundColor Yellow

$maxRetries = 15
$attempt = 0
$dbReady = $false

while ($attempt -lt $maxRetries -and -not $dbReady) {
    $attempt++
    $checkDb = "SELECT name FROM sys.databases WHERE name = 'ContractsDB';" | `
        docker exec -i $SQLSERVER_CONTAINER /opt/mssql-tools18/bin/sqlcmd `
            -S localhost -U sa -P $SA_PASSWORD -C -d master -t -A -h -1 2>&1

    if ($checkDb -match "ContractsDB") {
        $dbReady = $true
        Write-Host "   ContractsDB encontrada." -ForegroundColor Green
    } else {
        Write-Host "   Intento $attempt/$maxRetries - ContractsDB no lista aun, esperando 5s..." -ForegroundColor Gray
        Start-Sleep -Seconds 5
    }
}

if (-not $dbReady) {
    Write-Host ""
    Write-Host "   ERROR: ContractsDB no existe despues de $maxRetries intentos." -ForegroundColor Red
    Write-Host "   El API de contratos (simar_contratos_api) la crea al iniciar." -ForegroundColor Red
    Write-Host "   Asegurate de que 'docker compose up -d' este completo y espera ~30 seg." -ForegroundColor Red
    exit 1
}

# ── 3. INSERTAR CONTRATO ──────────────────────────────────────────────────────
Write-Host "[3/4]  Insertando contrato activo..." -ForegroundColor Yellow

$insertContractSql = @"
IF NOT EXISTS (SELECT 1 FROM Contracts WHERE Folio = 'DEMO-2024-001')
BEGIN
    INSERT INTO Contracts (Folio, ClientId, TotalBasePrice, CreatedAt, Status, ClientName, ClientRfc, Representative, ClientAddress, ClientObjetoSocial, ClientDeclaraciones, ContractDuration, FirstServiceDate)
    VALUES ('DEMO-2024-001', $CLIENT_ID, 15000.00, GETDATE(), 'activo', 'Industrias Demo SA de CV', 'IDEMO123456XYZ', 'Juan Perez Lopez', 'Av. Reforma 123, Col. Centro, CDMX, CP 06600', 'Gestion y disposicion final de residuos industriales.', 'La empresa declara cumplir con la normativa ambiental vigente (NOM-052-SEMARNAT).', '12 meses', DATEADD(day, 7, GETDATE()));

    DECLARE @CID INT = SCOPE_IDENTITY();

    INSERT INTO ContractServices (ContractId, WasteType, WasteUnit, Frequency, Vehicles, Technicians, ServiceAddress, WarehouseAddress, Subtotal)
    VALUES
        (@CID, 'Solventes usados',              'kg',    'Mensual',   1, 2, 'Av. Reforma 123, CDMX', 'Planta SIMAR Norte', 3500.00),
        (@CID, 'Aceites lubricantes residuales', 'litro', 'Mensual',   1, 2, 'Av. Reforma 123, CDMX', 'Planta SIMAR Norte', 4500.00),
        (@CID, 'Residuos de pintura (lodos)',    'kg',    'Bimestral', 1, 2, 'Av. Reforma 123, CDMX', 'Planta SIMAR Norte', 7000.00);

    INSERT INTO ContractPayments (ContractId, Description, Amount, PaymentDate)
    VALUES (@CID, 'Pago inicial - Tratamiento y disposicion final', 15000.00, DATEADD(month, 1, GETDATE()));

    PRINT 'OK: Contrato DEMO-2024-001 creado.';
END
ELSE
    PRINT 'INFO: El contrato DEMO-2024-001 ya existia.';
"@

$result = $insertContractSql | docker exec -i $SQLSERVER_CONTAINER /opt/mssql-tools18/bin/sqlcmd `
    -S localhost -U sa -P $SA_PASSWORD -C -d ContractsDB 2>&1

Write-Host "   $result" -ForegroundColor Gray

# ── RESUMEN ───────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "  OK  Seed completado"                            -ForegroundColor Green
Write-Host ""
Write-Host "  Cliente  :  Industrias Demo SA de CV"          -ForegroundColor White
Write-Host "  RFC      :  IDEMO123456XYZ"                    -ForegroundColor White
Write-Host "  SEMARNAT :  CDMX-2024-001234"                  -ForegroundColor White
Write-Host "  Contrato :  DEMO-2024-001  (status: activo)"   -ForegroundColor White
Write-Host "  Residuos :  Solventes / Aceites / Pintura"     -ForegroundColor White
Write-Host ""
Write-Host "  Vehiculos: 5 (Kenworth T680, Volvo FH16, Mercedes Actros, Ford F-550, International HV)" -ForegroundColor White
Write-Host "  Choferes : Juan Carlos Mendez (Lic E) / Maria Elena Torres (Lic C)" -ForegroundColor White
Write-Host ""
Write-Host "  Flujo de prueba:"                              -ForegroundColor Yellow
Write-Host "  1. Login en el sistema"                        -ForegroundColor White
Write-Host "  2. Manifiestos -> Generar manifiesto"          -ForegroundColor White
Write-Host "  3. Selecciona el contrato DEMO-2024-001"       -ForegroundColor White
Write-Host "  4. Los residuos se pre-llenaran solos"         -ForegroundColor White
Write-Host "  5. Selecciona vehiculo y chofer del catalogo"  -ForegroundColor White
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""
