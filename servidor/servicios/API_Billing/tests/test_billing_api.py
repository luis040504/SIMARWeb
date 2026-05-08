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
