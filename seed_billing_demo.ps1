# ─────────────────────────────────────────────────────────────────────────────
# seed_billing_demo.ps1  —  Datos de demo para probar flujo de Facturación y Contratos
#
# Uso: .\seed_billing_demo.ps1
# ─────────────────────────────────────────────────────────────────────────────

$USERS_CONTAINER      = "simar_db_usuarios"
$USERS_USER           = "admin_users"
$USERS_DB             = "simar_users_db"

$CLIENTS_CONTAINER    = "simar_postgres_clientes"
$CLIENTS_USER         = "simero"
$CLIENTS_DB           = "simar_clientes_db"

$CONTRACTS_CONTAINER  = "simar_sqlserver_contratos"
$SA_PASSWORD          = "SimarContracts123!"

$PASS_HASH = '$2b$12$KlSdkTSRu6zq/9hQdFYyrue1DD7IDuyjJfgjZXNj1kCgz2ysdlbCK' # prueba_password123!

Write-Host ""
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "  SIMAR  -  Seed de Datos para Facturación"      -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan

# ── 1. USUARIOS EN POSTGRESQL ────────────────────────────────────────────────
Write-Host ""
Write-Host "[1/4] Creando usuarios de clientes (PostgreSQL)..." -ForegroundColor Yellow

$usersSql = @"
INSERT INTO users (id_user, username, email, role, password_hash, is_active)
VALUES
  ('b1111111-1111-1111-1111-111111111111', 'cliente_alfa', 'contacto@alfa.com', 'cliente', '$PASS_HASH', TRUE),
  ('b2222222-2222-2222-2222-222222222222', 'cliente_beta', 'info@serviciosbeta.com', 'cliente', '$PASS_HASH', TRUE),
  ('b3333333-3333-3333-3333-333333333333', 'cliente_gamma', 'logistica@gamma.com', 'cliente', '$PASS_HASH', TRUE)
ON CONFLICT (username) DO NOTHING;
"@

$usersResult = $usersSql | docker exec -i $USERS_CONTAINER psql -U $USERS_USER -d $USERS_DB 2>&1
Write-Host "   $usersResult" -ForegroundColor Gray

# ── 2. CLIENTES EN POSTGRESQL ────────────────────────────────────────────────
Write-Host ""
Write-Host "[2/4] Creando clientes (PostgreSQL)..." -ForegroundColor Yellow

$clientsSql = @'
INSERT INTO clientes (name, "businessName", "contactEmail", phone, address, rfc, "semarnatNum", status, "idUser", "registerDate")
SELECT 'Constructora Alfa SA', 'Constructora Alfa de México SA de CV', 'contacto@alfa.com', '5511223344', 'Av. Industrias 450, Monterrey, NL', 'CALF900101XY1', 'MTY-2024-001', 'activo', 'b1111111-1111-1111-1111-111111111111', NOW()
WHERE NOT EXISTS (SELECT 1 FROM clientes WHERE rfc = 'CALF900101XY1');

INSERT INTO clientes (name, "businessName", "contactEmail", phone, address, rfc, "semarnatNum", status, "idUser", "registerDate")
SELECT 'Servicios Beta SC', 'Servicios Integrales Beta SC', 'info@serviciosbeta.com', '5522334455', 'Calle Roble 12, Guadalajara, JAL', 'SBET850505ABC', 'GDL-2024-002', 'activo', 'b2222222-2222-2222-2222-222222222222', NOW()
WHERE NOT EXISTS (SELECT 1 FROM clientes WHERE rfc = 'SBET850505ABC');

INSERT INTO clientes (name, "businessName", "contactEmail", phone, address, rfc, "semarnatNum", status, "idUser", "registerDate")
SELECT 'Logistica Gamma', 'Gamma Soluciones Logísticas SA', 'logistica@gamma.com', '5533445566', 'Puerto de Veracruz, Ver.', 'LGAM701010Z99', 'VER-2024-003', 'activo', 'b3333333-3333-3333-3333-333333333333', NOW()
WHERE NOT EXISTS (SELECT 1 FROM clientes WHERE rfc = 'LGAM701010Z99');
'@

$clientsResult = $clientsSql | docker exec -i $CLIENTS_CONTAINER psql -U $CLIENTS_USER -d $CLIENTS_DB 2>&1
Write-Host "   $clientsResult" -ForegroundColor Gray

# Obtener IDs de los clientes para las cotizaciones
$CLIENT_A_ID = ("SELECT id FROM clientes WHERE rfc = 'CALF900101XY1' LIMIT 1;" | docker exec -i $CLIENTS_CONTAINER psql -U $CLIENTS_USER -d $CLIENTS_DB -t -A)
$CLIENT_B_ID = ("SELECT id FROM clientes WHERE rfc = 'SBET850505ABC' LIMIT 1;" | docker exec -i $CLIENTS_CONTAINER psql -U $CLIENTS_USER -d $CLIENTS_DB -t -A)
$CLIENT_C_ID = ("SELECT id FROM clientes WHERE rfc = 'LGAM701010Z99' LIMIT 1;" | docker exec -i $CLIENTS_CONTAINER psql -U $CLIENTS_USER -d $CLIENTS_DB -t -A)

# Limpiar IDs por si acaso traen basura
$CLIENT_A_ID = ("$CLIENT_A_ID" -replace '\D','').Trim()
$CLIENT_B_ID = ("$CLIENT_B_ID" -replace '\D','').Trim()
$CLIENT_C_ID = ("$CLIENT_C_ID" -replace '\D','').Trim()

# ── 3. COTIZACIONES EN SQL SERVER ────────────────────────────────────────────
Write-Host ""
Write-Host "[3/4] Creando cotizaciones en ContractsDB (SQL Server)..." -ForegroundColor Yellow

# JSON Complejo requerido por el Generador de Contratos
$COMPLEX_JSON_ALFA = '[{"location":{"street":"Av. Industrias 450","neighborhood":"Industrial","municipality":"Monterrey","cp":"64000"},"wastes":[{"name":"Residuos Peligrosos","unit":"kg","quantity":1000,"pricePerUnit":25}],"vehicles":[{"name":"Camion 3.5","price":2000}],"crew":[{"name":"Operador","cost":500},{"name":"Auxiliar","cost":300}],"logistics":{"primaryDestination":"Planta SIMAR Norte","fuelLiters":50,"fuelPricePerLiter":22,"totalTollCost":350,"viaticos":200},"extraCosts":[],"supplies":[]}]'
$COMPLEX_JSON_BETA = '[{"location":{"street":"Calle Roble 12","neighborhood":"Fresno","municipality":"Guadalajara","cp":"44900"},"wastes":[{"name":"Lodos de pintura","unit":"kg","quantity":2000,"pricePerUnit":22.5}],"vehicles":[{"name":"Torton","price":3500}],"crew":[{"name":"Operador","cost":600}],"logistics":{"primaryDestination":"Planta SIMAR Poniente","fuelLiters":80,"fuelPricePerLiter":22,"totalTollCost":500,"viaticos":300},"extraCosts":[],"supplies":[]}]'
$COMPLEX_JSON_GAMMA = '[{"location":{"street":"Puerto de Veracruz","neighborhood":"Centro","municipality":"Veracruz","cp":"91700"},"wastes":[{"name":"Aceites usados","unit":"litro","quantity":1500,"pricePerUnit":10}],"vehicles":[{"name":"Pipa","price":4000}],"crew":[{"name":"Operador","cost":600}],"logistics":{"primaryDestination":"Refineria","fuelLiters":100,"fuelPricePerLiter":22,"totalTollCost":200,"viaticos":400},"extraCosts":[],"supplies":[]}]'

$quotationsSql = @"
IF NOT EXISTS (SELECT 1 FROM Quotations WHERE Folio = 'COT-ALFA-001')
BEGIN
    INSERT INTO Quotations (Id, Folio, Status, ClientName, ClientRfc, ContactName, ContactPhone, ContactEmail, ValidityDays, Subtotal, Total, CreatedAt, ServicesRawJson, Frequency)
    VALUES (101, 'COT-ALFA-001', 'contracted', 'Constructora Alfa SA', 'CALF900101XY1', 'Ing. Roberto', '5511223344', 'contacto@alfa.com', 30, 25000.00, 29000.00, GETDATE(), '$COMPLEX_JSON_ALFA', 'Mensual');
END
IF NOT EXISTS (SELECT 1 FROM Quotations WHERE Folio = 'COT-ALFA-002')
BEGIN
    INSERT INTO Quotations (Id, Folio, Status, ClientName, ClientRfc, ContactName, ContactPhone, ContactEmail, ValidityDays, Subtotal, Total, CreatedAt, ServicesRawJson, Frequency)
    VALUES (102, 'COT-ALFA-002', 'approved', 'Constructora Alfa SA', 'CALF900101XY1', 'Ing. Roberto', '5511223344', 'contacto@alfa.com', 30, 12000.00, 13920.00, GETDATE(), '$COMPLEX_JSON_ALFA', 'Quincenal');
END
IF NOT EXISTS (SELECT 1 FROM Quotations WHERE Folio = 'COT-ALFA-003')
BEGIN
    INSERT INTO Quotations (Id, Folio, Status, ClientName, ClientRfc, ContactName, ContactPhone, ContactEmail, ValidityDays, Subtotal, Total, CreatedAt, ServicesRawJson, Frequency)
    VALUES (103, 'COT-ALFA-003', 'pending', 'Constructora Alfa SA', 'CALF900101XY1', 'Ing. Roberto', '5511223344', 'contacto@alfa.com', 15, 8000.00, 9280.00, GETDATE(), '$COMPLEX_JSON_ALFA', 'Eventual');
END
IF NOT EXISTS (SELECT 1 FROM Quotations WHERE Folio = 'COT-BETA-001')
BEGIN
    INSERT INTO Quotations (Id, Folio, Status, ClientName, ClientRfc, ContactName, ContactPhone, ContactEmail, ValidityDays, Subtotal, Total, CreatedAt, ServicesRawJson, Frequency)
    VALUES (104, 'COT-BETA-001', 'contracted', 'Servicios Beta SC', 'SBET850505ABC', 'Lic. Martha', '5522334455', 'info@serviciosbeta.com', 30, 45000.00, 52200.00, GETDATE(), '$COMPLEX_JSON_BETA', 'Mensual');
END
IF NOT EXISTS (SELECT 1 FROM Quotations WHERE Folio = 'COT-GAMMA-001')
BEGIN
    INSERT INTO Quotations (Id, Folio, Status, ClientName, ClientRfc, ContactName, ContactPhone, ContactEmail, ValidityDays, Subtotal, Total, CreatedAt, ServicesRawJson, Frequency)
    VALUES (105, 'COT-GAMMA-001', 'approved', 'Logistica Gamma', 'LGAM701010Z99', 'Sr. Arturo', '5533445566', 'logistica@gamma.com', 30, 15000.00, 17400.00, GETDATE(), '$COMPLEX_JSON_GAMMA', 'Bimestral');
END
PRINT 'OK: Cotizaciones verificadas.';
GO
"@

$quotationsResult = $quotationsSql | docker exec -i $CONTRACTS_CONTAINER /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -C -d ContractsDB 2>&1
Write-Host "   $quotationsResult" -ForegroundColor Gray

# ── 4. CONTRATOS EN SQL SERVER ───────────────────────────────────────────────
Write-Host ""
Write-Host "[4/4] Creando contratos activos (SQL Server)..." -ForegroundColor Yellow

if ([string]::IsNullOrWhiteSpace($CLIENT_A_ID) -or [string]::IsNullOrWhiteSpace($CLIENT_B_ID)) {
    Write-Host "   ERROR: No se pudieron obtener los IDs de los clientes. Verifica el paso [2/4]." -ForegroundColor Red
} else {
    $contractsSql = @"
    IF NOT EXISTS (SELECT 1 FROM Contracts WHERE Folio = 'CON-ALFA-2024')
    BEGIN
        INSERT INTO Contracts (Folio, ClientId, TotalBasePrice, CreatedAt, Status, ClientName, ClientRfc, Representative, ClientAddress, ClientObjetoSocial, ClientDeclaraciones, ContractDuration, FirstServiceDate)
        VALUES ('CON-ALFA-2024', $CLIENT_A_ID, 25000.00, GETDATE(), 'activo', 'Constructora Alfa SA', 'CALF900101XY1', 'Ing. Roberto', 'Av. Industrias 450, Monterrey, NL', 'Construcción y obra civil.', 'Declara cumplir con normas ambientales.', '12 meses', DATEADD(day, 15, GETDATE()));
        
        DECLARE @CON_A INT = SCOPE_IDENTITY();
        INSERT INTO ContractServices (ContractId, WasteType, WasteUnit, Frequency, Vehicles, Technicians, ServiceAddress, WarehouseAddress, Subtotal)
        VALUES (@CON_A, 'Residuos Peligrosos', 'kg', 'Mensual', 1, 2, 'Av. Industrias 450, Monterrey', 'Planta SIMAR Norte', 25000.00);
        
        INSERT INTO ContractPayments (ContractId, Description, Amount, PaymentDate)
        VALUES (@CON_A, 'Mensualidad Enero', 29000.00, DATEADD(month, 1, GETDATE()));
    END

    IF NOT EXISTS (SELECT 1 FROM Contracts WHERE Folio = 'CON-BETA-2024')
    BEGIN
        -- Contrato para Beta
        INSERT INTO Contracts (Folio, ClientId, TotalBasePrice, CreatedAt, Status, ClientName, ClientRfc, Representative, ClientAddress, ClientObjetoSocial, ClientDeclaraciones, ContractDuration, FirstServiceDate)
        VALUES ('CON-BETA-2024', $CLIENT_B_ID, 45000.00, GETDATE(), 'activo', 'Servicios Beta SC', 'SBET850505ABC', 'Lic. Martha', 'Calle Roble 12, Guadalajara, JAL', 'Servicios industriales integrales.', 'Declara capacidad técnica.', '12 meses', DATEADD(day, 10, GETDATE()));
        
        DECLARE @CON_B INT = SCOPE_IDENTITY();
        INSERT INTO ContractServices (ContractId, WasteType, WasteUnit, Frequency, Vehicles, Technicians, ServiceAddress, WarehouseAddress, Subtotal)
        VALUES (@CON_B, 'Lodos de pintura', 'kg', 'Mensual', 2, 3, 'Calle Roble 12, Guadalajara', 'Planta SIMAR Poniente', 45000.00);
        
        INSERT INTO ContractPayments (ContractId, Description, Amount, PaymentDate)
        VALUES (@CON_B, 'Mensualidad Enero', 52200.00, DATEADD(month, 1, GETDATE()));
    END
    PRINT 'OK: Contratos verificados.';
    GO
"@

    $contractsResult = $contractsSql | docker exec -i $CONTRACTS_CONTAINER /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -C -d ContractsDB 2>&1
    Write-Host "   $contractsResult" -ForegroundColor Gray
}

Write-Host ""
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "  OK  Seed de Facturación completado"            -ForegroundColor Green
Write-Host ""
Write-Host "  Usuarios creados (Pass: prueba_password123!):" -ForegroundColor White
Write-Host "  - cliente_alfa"                                -ForegroundColor White
Write-Host "  - cliente_beta"                                -ForegroundColor White
Write-Host "  - cliente_gamma"                               -ForegroundColor White
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""
