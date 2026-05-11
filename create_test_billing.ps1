# ─────────────────────────────────────────────────────────────────────────────
# create_test_billing.ps1  —  Crea un cliente, contrato y una prefactura vinculada
#
# Uso: .\create_test_billing.ps1
# ─────────────────────────────────────────────────────────────────────────────

$POSTGRES_CONTAINER   = "simar_postgres_clientes"
$SQLSERVER_CONTAINER  = "simar_sqlserver_contratos"
$MONGO_CONTAINER      = "simar_mongo_facturacion"

$POSTGRES_USER        = "simero"
$POSTGRES_DB          = "simar_clientes_db"
$SA_PASSWORD          = "SimarContracts123!"

$MONGO_USER           = "facturacion_admin"
$MONGO_PASS           = "Simar123!"
$MONGO_DB             = "simar_facturacion_db"

Write-Host ""
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "  SIMAR  -  Generador de Datos de Facturación"     -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan

# ── 1. INSERTAR CLIENTE EN POSTGRESQL ─────────────────────────────────────────
Write-Host ""
Write-Host "[1/3]  Insertando cliente de prueba (PostgreSQL)..." -ForegroundColor Yellow

$RFC = "DQUI900101XYZ"
$CLIENT_NAME = "Distribuidora de Químicos SA"

$insertClientSql = @"
INSERT INTO clientes (name, "businessName", "contactEmail", phone, address, rfc, "semarnatNum", status, "idUser", "registerDate")
SELECT 'Mario Gomez','Distribuidora de Químicos SA','ventas@dquimicos.com','5599887766','Calle Industrial 500, CP 54000, Edo Mex','DQUI900101XYZ','MEX-2024-009988','activo','00000000-0000-0000-0000-000000000088',NOW()
WHERE NOT EXISTS (SELECT 1 FROM clientes WHERE rfc = 'DQUI900101XYZ')
RETURNING id;
"@

$CLIENT_ID = ($insertClientSql | docker exec -i $POSTGRES_CONTAINER psql -U $POSTGRES_USER -d $POSTGRES_DB -t -A 2>&1)
if ($CLIENT_ID -match "ERROR" -or [string]::IsNullOrWhiteSpace("$CLIENT_ID")) {
    $CLIENT_ID = ("SELECT id FROM clientes WHERE rfc = '$RFC' LIMIT 1;" | docker exec -i $POSTGRES_CONTAINER psql -U $POSTGRES_USER -d $POSTGRES_DB -t -A 2>&1)
}
$CLIENT_ID = ("$CLIENT_ID" -replace '\D','').Trim()

Write-Host "   Cliente listo (ID: $CLIENT_ID)" -ForegroundColor Green

# ── 2. INSERTAR CONTRATO EN SQL SERVER ────────────────────────────────────────
Write-Host ""
Write-Host "[2/3]  Insertando contrato y servicios (SQL Server)..." -ForegroundColor Yellow

$FOLIO = "CON-TEST-002"
$WASTE_TYPE = "Recolección de Solventes"

$insertContractSql = @"
IF NOT EXISTS (SELECT 1 FROM Contracts WHERE Folio = '$FOLIO')
BEGIN
    INSERT INTO Contracts (Folio, ClientId, TotalBasePrice, CreatedAt, Status, ClientName, ClientRfc, Representative, ClientAddress, ContractDuration, FirstServiceDate)
    VALUES ('$FOLIO', $CLIENT_ID, 5000.00, GETDATE(), 'activo', '$CLIENT_NAME', '$RFC', 'Mario Gomez', 'Calle Industrial 500, CP 54000, Edo Mex', '12 meses', GETDATE());

    DECLARE @CID INT = SCOPE_IDENTITY();

    INSERT INTO ContractServices (ContractId, WasteType, WasteUnit, Frequency, Vehicles, Technicians, ServiceAddress, WarehouseAddress, Subtotal)
    VALUES (@CID, '$WASTE_TYPE', 'litro', 'Quincenal', 1, 2, 'Planta Tlalnepantla', 'SIMAR CDMX', 5000.00);
END
"@

$contractResult = $insertContractSql | docker exec -i $SQLSERVER_CONTAINER /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -C -d ContractsDB 2>&1
Write-Host "   Contrato $FOLIO creado con servicio: $WASTE_TYPE" -ForegroundColor Green

# ── 3. INSERTAR PREFACTURA EN MONGODB ─────────────────────────────────────────
Write-Host ""
Write-Host "[3/3]  Generando prefactura vinculada (MongoDB)..." -ForegroundColor Yellow

# El service_id debe coincidir con el formato: CONTRACT:{folio}:{waste_type}
$SERVICE_ID = "CONTRACT:${FOLIO}:${WASTE_TYPE}"

$mongoScript = @"
db.facturas.insertOne({
    upload_type: 'DIGITAL',
    record_type: 'Invoice',
    service_type: 'Recolección de Residuos',
    metadata: {
        created_at: new Date(),
        updated_at: new Date(),
        source: 'web_app'
    },
    issuer: {
        tax_id: 'SIMA123456ABC',
        name: 'SIMAR Soluciones Ambientales',
        tax_regime: '601'
    },
    receiver: {
        tax_id: '$RFC',
        name: '$CLIENT_NAME',
        tax_usage: 'G03',
        postal_code: '54000',
        fiscal_regime: '601',
        client_id: '$CLIENT_ID'
    },
    fiscal_data: {
        invoice_folio: 'PRE-TEST-99',
        issue_date: new Date()
    },
    financials: {
        currency: 'MXN',
        exchange_rate: 1.0,
        subtotal: 5000.0,
        discount: 0.0,
        tax_total: 800.0,
        total: 5800.0,
        payment_method: 'PPD',
        payment_form: '99'
    },
    items: [{
        product_code: '80141600',
        description: '$WASTE_TYPE (Servicio de Contrato)',
        quantity: 1.0,
        unit_price: 5000.0,
        amount: 5000.0,
        taxes: [{ type: 'IVA', rate: 0.16, amount: 800.0 }]
    }],
    status: 'Pending',
    service_id: '$SERVICE_ID',
    activo: true
});
"@

$mongoResult = echo $mongoScript | docker exec -i $MONGO_CONTAINER mongosh $MONGO_DB -u $MONGO_USER -p $MONGO_PASS --authenticationDatabase admin --quiet 2>&1
Write-Host "   Prefactura creada vinculada a: $SERVICE_ID" -ForegroundColor Green

Write-Host ""
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "  PROCESO COMPLETADO"                              -ForegroundColor Green
Write-Host "  Ahora puedes ir a 'Servicios Recientes' y verás" -ForegroundColor White
Write-Host "  que el servicio '$WASTE_TYPE' del contrato"     -ForegroundColor White
Write-Host "  '$FOLIO' NO aparece porque ya tiene prefactura." -ForegroundColor White
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""
