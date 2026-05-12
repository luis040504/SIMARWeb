# ─────────────────────────────────────────────────────────────────────────────
# seed_portuaria_contract.ps1  —  Registro de Operadora Portuaria y Logística
# ─────────────────────────────────────────────────────────────────────────────

$USERS_CONTAINER      = "simar_db_usuarios"
$CLIENTS_CONTAINER    = "simar_postgres_clientes"
$CONTRACTS_CONTAINER  = "simar_sqlserver_contratos"
$SA_PASSWORD          = "SimarContracts123!"

$PASS_HASH = '$2b$12$KlSdkTSRu6zq/9hQdFYyrue1DD7IDuyjJfgjZXNj1kCgz2ysdlbCK' # prueba_password123!
$USER_ID   = 'c108c108-c108-c108-c108-c108c108c108'

Write-Host "`n=== REGISTRANDO CLIENTE: OPERADORA PORTUARIA ===" -ForegroundColor Cyan

# 1. Crear Usuario
$usersSql = "INSERT INTO users (id_user, username, email, role, password_hash, is_active) VALUES ('$USER_ID', 'opl_golfo', 'logistica@oplgolfo.com.mx', 'cliente', '$PASS_HASH', TRUE) ON CONFLICT DO NOTHING;"
$usersSql | docker exec -i $USERS_CONTAINER psql -U admin_users -d simar_users_db

# 2. Crear Cliente
$clientsSql = @"
INSERT INTO clientes (name, "businessName", "contactEmail", phone, address, rfc, "semarnatNum", status, "idUser", "registerDate")
SELECT 'Operadora Portuaria y Logística del Golfo', 'Operadora Portuaria y Logística del Golfo SA de CV', 'logistica@oplgolfo.com.mx', '2291234567', 'Blvd. Fidel Velázquez S/N, Muelle 4, Veracruz', 'OPL150620XYZ', 'VER-OPL-2026', 'activo', '$USER_ID', NOW()
WHERE NOT EXISTS (SELECT 1 FROM clientes WHERE rfc = 'OPL150620XYZ');
"@
$clientsSql | docker exec -i $CLIENTS_CONTAINER psql -U simero -d simar_clientes_db

# 3. Crear Presupuesto/Contrato con JSON Complejo
$COMPLEX_JSON = @'
[{"id":"srv-veracruz-01","activity":"final_disposal","location":{"cp":"91700","street":"Blvd. Fidel Velázquez S/N, Muelle 4","municipality":"Veracruz"},"wastes":[{"name":"Lodos de cárcamos","clave":"RLTAR-1","quantity":800,"unit":"Litro","pricePerUnit":8.50}],"vehicles":[{"name":"Camioneta 3.5 Ton","price":1000}]}]
'@

$quotationsSql = @"
IF NOT EXISTS (SELECT 1 FROM Quotations WHERE Folio = 'SIMAR-2026-108')
BEGIN
    INSERT INTO Quotations (Id, Folio, Status, ClientName, ClientRfc, ContactName, ContactPhone, ContactEmail, ValidityDays, Subtotal, Total, CreatedAt, ServicesRawJson, Frequency)
    VALUES (108, 'SIMAR-2026-108', 'draft', 'Operadora Portuaria y Logística del Golfo SA de CV', 'OPL150620XYZ', 'Ing. Valeria Mendoza', '2291234567', 'logistica@oplgolfo.com.mx', 30, 21830.00, 25322.80, GETDATE(), '$COMPLEX_JSON', 'Mensual (4 servicios)');
END
GO
"@
$quotationsSql | docker exec -i $CONTRACTS_CONTAINER /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -C -d ContractsDB

Write-Host "`n[OK] Cliente y Contrato #108 registrados correctamente." -ForegroundColor Green
