import pytest
from httpx import AsyncClient, ASGITransport
from main import app
from unittest.mock import AsyncMock, patch, MagicMock

@pytest.fixture
def mock_collection():
    with patch('src.controller.billing_controller.facturas_collection') as mock_coll:
        mock_coll.find_one = AsyncMock()
        mock_coll.insert_one = AsyncMock()
        mock_coll.update_one = AsyncMock()
        
        # Setup mock for find().to_list()
        mock_cursor = MagicMock()
        mock_cursor.to_list = AsyncMock()
        mock_cursor.sort = MagicMock(return_value=mock_cursor)
        mock_coll.find = MagicMock(return_value=mock_cursor)
        
        yield mock_coll

import pytest_asyncio

@pytest_asyncio.fixture
async def async_client():
    async with AsyncClient(transport=ASGITransport(app=app), base_url="http://test") as client:
        yield client
