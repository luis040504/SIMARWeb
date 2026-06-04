import pytest
from httpx import AsyncClient
from bson import ObjectId

pytestmark = pytest.mark.asyncio

valid_payload = {
    "upload_type": "DIGITAL",
    "metadata": {
        "created_at": "2023-10-01T12:00:00Z",
        "updated_at": "2023-10-01T12:00:00Z",
        "source": "web_app"
    },
    "issuer": {
        "tax_id": "ABC123456T1A",
        "name": "Empresa Emisora SA de CV",
        "tax_regime": "601"
    },
    "receiver": {
        "tax_id": "XYZ987654R2B",
        "name": "Cliente Receptor SA de CV",
        "tax_usage": "G03",
        "postal_code": "12345"
    },
    "fiscal_data": {
        "issue_date": "2023-10-01T12:00:00Z"
    },
    "financials": {
        "currency": "MXN",
        "exchange_rate": 1.0,
        "subtotal": 1000.0,
        "discount": 0.0,
        "tax_total": 160.0,
        "total": 1160.0,
        "payment_method": "PUE",
        "payment_form": "01"
    },
    "items": [
        {
            "product_code": "12345678",
            "description": "Servicio de consultoría",
            "quantity": 1,
            "unit_price": 1000.0,
            "amount": 1000.0
        }
    ],
    "attachments": {},
    "status": "VALID",
    "activo": True
}

async def test_create_billing_success(async_client: AsyncClient, mock_collection):
    mock_id = ObjectId()
    mock_insert_result = type('InsertOneResult', (), {'inserted_id': mock_id})
    mock_collection.insert_one.return_value = mock_insert_result
    
    mock_doc = valid_payload.copy()
    mock_doc["_id"] = str(mock_id)
    mock_collection.find_one.return_value = mock_doc
    
    response = await async_client.post("/billing/", json=valid_payload)
    
    assert response.status_code == 201
    data = response.json()
    assert data["_id"] == str(mock_id)
    assert data["financials"]["total"] == 1160.0

async def test_create_billing_negative_amount_fails(async_client: AsyncClient, mock_collection):
    payload = valid_payload.copy()
    payload["financials"] = payload["financials"].copy()
    payload["financials"]["subtotal"] = -100.0
    
    response = await async_client.post("/billing/", json=payload)
    
    assert response.status_code == 422
    data = response.json()
    assert data["code"] == "VALIDATION_ERROR"

async def test_create_billing_invalid_math_fails(async_client: AsyncClient, mock_collection):
    payload = valid_payload.copy()
    payload["financials"] = payload["financials"].copy()
    payload["financials"]["total"] = 9999.0 
    
    response = await async_client.post("/billing/", json=payload)
    
    assert response.status_code == 400
    data = response.json()
    assert data["code"] == "INVALID_TOTAL"
    assert "no cuadra" in data["message"]

async def test_get_all_billing_success(async_client: AsyncClient, mock_collection):
    mock_doc = valid_payload.copy()
    mock_doc["_id"] = str(ObjectId())
    
    mock_collection.find.return_value.sort.return_value.to_list.return_value = [mock_doc]
    
    response = await async_client.get("/billing/")
    
    assert response.status_code == 200
    data = response.json()
    assert isinstance(data, list)
    assert len(data) == 1
    assert data[0]["_id"] == mock_doc["_id"]

async def test_update_billing_success(async_client: AsyncClient, mock_collection):
    mock_id = str(ObjectId())
    
    mock_existing = valid_payload.copy()
    mock_existing["_id"] = mock_id
    mock_existing["status"] = "PENDING_APPROVAL" 
    
    mock_collection.find_one.side_effect = [mock_existing, mock_existing]
    
    update_payload = {"status": "VALID"}
    
    response = await async_client.put(f"/billing/{mock_id}", json=update_payload)
    
    assert response.status_code == 200
    assert mock_collection.update_one.called

async def test_update_billing_not_found(async_client: AsyncClient, mock_collection):
    mock_id = str(ObjectId())
    mock_collection.find_one.return_value = None
    
    response = await async_client.put(f"/billing/{mock_id}", json={"status": "VALID"})
    assert response.status_code == 404

async def test_update_billing_immutable_state(async_client: AsyncClient, mock_collection):
    mock_id = str(ObjectId())
    
    mock_existing = valid_payload.copy()
    mock_existing["_id"] = mock_id
    mock_existing["status"] = "Accepted" 
    
    mock_collection.find_one.return_value = mock_existing
    
    response = await async_client.put(f"/billing/{mock_id}", json={"status": "VALID"})
    
    assert response.status_code == 400
    data = response.json()
    assert data["code"] == "IMMUTABLE_STATE"

async def test_get_billing_by_id_success(async_client: AsyncClient, mock_collection):
    mock_id = str(ObjectId())
    mock_doc = valid_payload.copy()
    mock_doc["_id"] = mock_id
    mock_collection.find_one.return_value = mock_doc
    
    response = await async_client.get(f"/billing/{mock_id}")
    assert response.status_code == 200
    assert response.json()["_id"] == mock_id

async def test_get_billing_by_id_not_found(async_client: AsyncClient, mock_collection):
    mock_id = str(ObjectId())
    mock_collection.find_one.return_value = None
    response = await async_client.get(f"/billing/{mock_id}")
    assert response.status_code == 404

async def test_get_billing_invalid_id(async_client: AsyncClient):
    response = await async_client.get("/billing/invalid_id")
    assert response.status_code == 400

async def test_get_billing_by_client_id(async_client: AsyncClient, mock_collection):
    mock_doc = valid_payload.copy()
    mock_doc["_id"] = str(ObjectId())
    mock_collection.find.return_value.sort.return_value.to_list.return_value = [mock_doc]
    
    response = await async_client.get("/billing/client/client123")
    assert response.status_code == 200
    assert len(response.json()) == 1

async def test_delete_billing_success(async_client: AsyncClient, mock_collection):
    mock_id = str(ObjectId())
    mock_update_result = type('UpdateResult', (), {'matched_count': 1})
    mock_collection.update_one.return_value = mock_update_result
    
    response = await async_client.delete(f"/billing/{mock_id}")
    assert response.status_code == 200
    assert mock_collection.update_one.called
    
async def test_change_status_success(async_client: AsyncClient, mock_collection):
    mock_id = str(ObjectId())
    mock_update_result = type('UpdateResult', (), {'matched_count': 1})
    mock_collection.update_one.return_value = mock_update_result
    
    mock_doc = valid_payload.copy()
    mock_doc["_id"] = mock_id
    mock_doc["status"] = "Accepted"
    mock_doc["reason"] = "Todo en orden"
    mock_collection.find_one.return_value = mock_doc
    
    response = await async_client.patch(f"/billing/{mock_id}/status?new_status=Accepted&reason=Todo%20en%20orden")
    assert response.status_code == 200
    assert response.json()["status"] == "Accepted"
    assert mock_collection.update_one.called

async def test_upload_physical_invoice_invalid_extension(async_client: AsyncClient):
    mock_id = str(ObjectId())
    files = {"file": ("factura.png", b"dummy content", "image/png")}
    response = await async_client.post(f"/billing/{mock_id}/upload", files=files)
    assert response.status_code == 400
    assert "PDF" in response.json()["message"]

async def test_upload_physical_invoice_success(async_client: AsyncClient, mock_collection):
    mock_id = str(ObjectId())
    mock_update_result = type('UpdateResult', (), {'matched_count': 1})
    mock_collection.update_one.return_value = mock_update_result
    
    mock_doc = valid_payload.copy()
    mock_doc["_id"] = mock_id
    mock_collection.find_one.return_value = mock_doc
    
    files = {"file": ("factura.pdf", b"dummy content", "application/pdf")}
    response = await async_client.post(f"/billing/{mock_id}/upload", files=files)
    assert response.status_code == 200
    assert mock_collection.update_one.called

async def test_get_pac_settings(async_client: AsyncClient, mock_collection):
    mock_collection.pac_settings.find_one.return_value = {
        "_id": "current_settings",
        "pac_mode": "PAID",
        "timbres_used": 2,
        "timbres_limit": 10
    }
    
    response = await async_client.get("/billing/pac/settings")
    assert response.status_code == 200
    data = response.json()
    assert data["pac_mode"] == "PAID"
    assert data["timbres_used"] == 2
    assert data["timbres_limit"] == 10

async def test_update_pac_settings(async_client: AsyncClient, mock_collection):
    mock_collection.pac_settings.find_one.return_value = {
        "_id": "current_settings",
        "pac_mode": "PAID",
        "timbres_used": 0,
        "timbres_limit": 15
    }
    
    payload = {
        "pac_mode": "PAID",
        "timbres_limit": 15,
        "timbres_used": 0
    }
    
    response = await async_client.post("/billing/pac/settings", json=payload)
    assert response.status_code == 200
    data = response.json()
    assert data["pac_mode"] == "PAID"
    assert data["timbres_limit"] == 15
    assert data["timbres_used"] == 0
    assert mock_collection.pac_settings.update_one.called

async def test_reset_pac_timbres(async_client: AsyncClient, mock_collection):
    mock_collection.pac_settings.find_one.return_value = {
        "_id": "current_settings",
        "pac_mode": "PAID",
        "timbres_used": 0,
        "timbres_limit": 5
    }
    
    response = await async_client.post("/billing/pac/reset")
    assert response.status_code == 200
    data = response.json()
    assert data["timbres_used"] == 0
    assert mock_collection.pac_settings.update_one.called

async def test_change_status_accepted_paid_mode_under_limit(async_client: AsyncClient, mock_collection):
    # PAC settings in PAID mode and under limit (1/5)
    mock_collection.pac_settings.find_one.return_value = {
        "_id": "current_settings",
        "pac_mode": "PAID",
        "timbres_used": 1,
        "timbres_limit": 5
    }
    
    mock_id = str(ObjectId())
    mock_update_result = type('UpdateResult', (), {'matched_count': 1})
    mock_collection.update_one.return_value = mock_update_result
    
    # Document returned after update
    mock_doc = valid_payload.copy()
    mock_doc["_id"] = mock_id
    mock_doc["status"] = "Accepted"
    mock_doc["fiscal_data"] = {
        "invoice_folio": "PAC-1234",
        "pac_rfc": "FIN1203015JA",
        "sat_certificate_number": "00001000000508881234",
        "uuid": "SOME-UUID-STRING-1234",
        "issue_date": "2026-05-21T00:00:00"
    }
    mock_doc["pac_type"] = "PAID"
    mock_collection.find_one.return_value = mock_doc
    
    response = await async_client.patch(f"/billing/{mock_id}/status?new_status=Accepted")
    assert response.status_code == 200
    data = response.json()
    assert data["status"] == "Accepted"
    assert "PAC-" in data["fiscal_data"]["invoice_folio"]
    assert data["fiscal_data"]["pac_rfc"] == "FIN1203015JA"
    assert data["fiscal_data"]["sat_certificate_number"] == "00001000000508881234"
    assert data["pac_type"] == "PAID"
    
    # Verify counter increment was saved to database
    assert mock_collection.pac_settings.update_one.called
    update_call_args = mock_collection.pac_settings.update_one.call_args
    assert update_call_args[0][0] == {"_id": "current_settings"}
    assert update_call_args[0][1]["$set"]["timbres_used"] == 2

async def test_change_status_accepted_paid_mode_exceeds_limit_fallback(async_client: AsyncClient, mock_collection):
    # PAC settings in PAID mode but at/exceeding limit (5/5)
    mock_collection.pac_settings.find_one.return_value = {
        "_id": "current_settings",
        "pac_mode": "PAID",
        "timbres_used": 5,
        "timbres_limit": 5
    }
    
    mock_id = str(ObjectId())
    mock_update_result = type('UpdateResult', (), {'matched_count': 1})
    mock_collection.update_one.return_value = mock_update_result
    
    # Document returned after update should have fallback values (A-XXXX, SAT970701NN3)
    mock_doc = valid_payload.copy()
    mock_doc["_id"] = mock_id
    mock_doc["status"] = "Accepted"
    mock_doc["fiscal_data"] = {
        "invoice_folio": "A-9876",
        "pac_rfc": "SAT970701NN3",
        "sat_certificate_number": "00001000000504465028",
        "uuid": "SOME-UUID-STRING-9876",
        "issue_date": "2026-05-21T00:00:00"
    }
    mock_doc["pac_type"] = "SIMULATED"
    mock_collection.find_one.return_value = mock_doc
    
    response = await async_client.patch(f"/billing/{mock_id}/status?new_status=Accepted")
    assert response.status_code == 200
    data = response.json()
    assert data["status"] == "Accepted"
    assert "A-" in data["fiscal_data"]["invoice_folio"]
    assert data["fiscal_data"]["pac_rfc"] == "SAT970701NN3"
    assert data["fiscal_data"]["sat_certificate_number"] == "00001000000504465028"
    assert data["pac_type"] == "SIMULATED"
    
    # Counter should NOT be incremented because we fell back to SIMULATED
    assert not mock_collection.pac_settings.update_one.called

async def test_get_ready_to_bill_external_api_failure(async_client: AsyncClient, mock_collection):
    from unittest.mock import patch, AsyncMock
    
    # Mocking httpx.AsyncClient specifically inside the controller to raise an exception or fail
    with patch("src.controller.billing_controller.httpx.AsyncClient") as mock_client_class:
        mock_instance = mock_client_class.return_value
        mock_instance.__aenter__.return_value = mock_instance
        mock_instance.get = AsyncMock(side_effect=Exception("Connection error"))
        
        response = await async_client.get("/billing/ready-to-bill")
        assert response.status_code == 200
        assert response.json() == []

async def test_get_ready_to_bill_success(async_client: AsyncClient, mock_collection):
    from unittest.mock import patch, MagicMock, AsyncMock
    
    # We need to mock several httpx responses
    # 1. /api/manifiestos?estado=completado
    # 2. /client/name/Cliente%20Test
    # 3. /api/manifiestos/1
    # 4. /api/contracts/10/detail
    # 5. /api/contracts?status=Activo & status=Aceptado
    
    mock_manifests = {
        "data": [
            {
                "id": 1,
                "numero_manifiesto": "MAN-001",
                "razon_social": "Cliente Test",
                "tipo": "especial",
                "fecha_manifiesto": "2026-05-31T00:00:00Z"
            }
        ]
    }
    
    mock_client = {
        "id": 123,
        "businessName": "Cliente Test",
        "rfc": "TES123456AAA",
        "address": "Calle Falsa 123"
    }
    
    mock_manifest_detail = {
        "data": {
            "id": 1,
            "contrato_id": 10,
            "residuos_especiales": [
                {
                    "nombre_residuo": "RPBI-01",
                    "peso": 100.0,
                    "unidad": "kg"
                }
            ]
        }
    }
    
    mock_contract_detail = {
        "id": 10,
        "folio": "CON-2026-001",
        "totalBasePrice": 5000.0,
        "contractDuration": "1 Año",
        "clientId": 123,
        "clientName": "Cliente Test",
        "clientRfc": "TES123456AAA",
        "clientAddress": "Calle Falsa 123",
        "services": [
            {
                "wasteType": "RPBI-01",
                "subtotal": 50.0,
                "wasteUnit": "kg"
            }
        ]
    }
    
    # Mocking httpx.AsyncClient inside the controller
    with patch("src.controller.billing_controller.httpx.AsyncClient") as mock_client_class:
        mock_instance = mock_client_class.return_value
        mock_instance.__aenter__.return_value = mock_instance
        
        # Create mock responses
        def side_effect(url, *args, **kwargs):
            mock_resp = MagicMock()
            mock_resp.status_code = 200
            
            url_str = str(url)
            if "manifiestos?estado=completado" in url_str:
                mock_resp.json.return_value = mock_manifests
            elif "client/name" in url_str:
                mock_resp.json.return_value = mock_client
            elif "manifiestos/1" in url_str:
                mock_resp.json.return_value = mock_manifest_detail
            elif "contracts/10/detail" in url_str:
                mock_resp.json.return_value = mock_contract_detail
            elif "contracts?status=" in url_str:
                mock_resp.json.return_value = [] # No active contracts to avoid infinite loop
            else:
                mock_resp.status_code = 404
                mock_resp.json.return_value = {}
                
            return mock_resp
            
        mock_instance.get = AsyncMock(side_effect=side_effect)
        
        response = await async_client.get("/billing/ready-to-bill")
        assert response.status_code == 200
        data = response.json()
        assert len(data) == 1
        assert data[0]["numero_manifiesto"] == "MAN-001"
        assert data[0]["cliente"]["rfc"] == "TES123456AAA"
        assert data[0]["contrato"]["folio"] == "CON-2026-001"
        assert len(data[0]["detalles_servicio"]) == 1
        assert data[0]["detalles_servicio"][0]["subtotal"] == 5000.0 # 100 kg * 50 subtotal
        assert data[0]["total_estimado"] == 5000.0


