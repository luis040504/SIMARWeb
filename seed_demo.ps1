# ─────────────────────────────────────────────────────────────────────────────
# seed_demo.ps1  —  Datos de demo para probar generación de manifiestos
#
# Uso: .\seed_demo.ps1
# Requisito: contenedores corriendo  →  docker compose up -d
# ─────────────────────────────────────────────────────────────────────────────

$POSTGRES_CONTAINER  = "simar_postgres_clientes"
$SQLSERVER_CONTAINER = "simar_sqlserver_contratos"
$POSTGRES_USER       = "simar_user"
$POSTGRES_DB         = "simar_clientes_db"
$SA_PASSWORD         = "Simar123!"

Write-Host ""
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "  SIMAR  -  Seed de datos para demo"             -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan

# ── 1. CLIENTE EN POSTGRESQL ──────────────────────────────────────────────────
Write-Host ""
Write-Host "[1/2]  Insertando cliente en PostgreSQL..." -ForegroundColor Yellow

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
Write-Host "[2/2]  Verificando SQL Server..." -ForegroundColor Yellow

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
Write-Host "   Insertando contrato activo..." -ForegroundColor Yellow

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
Write-Host "  Flujo de prueba:"                              -ForegroundColor Yellow
Write-Host "  1. Login en el sistema"                        -ForegroundColor White
Write-Host "  2. Manifiestos -> Generar manifiesto"          -ForegroundColor White
Write-Host "  3. Selecciona el contrato DEMO-2024-001"       -ForegroundColor White
Write-Host "  4. Los residuos se pre-llenaran solos"         -ForegroundColor White
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""
