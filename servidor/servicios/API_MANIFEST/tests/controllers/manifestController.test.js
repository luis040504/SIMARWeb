jest.mock('../../src/models/Manifest');

const Manifest    = require('../../src/models/Manifest');
const controller  = require('../../src/controllers/manifestController');

// ─── Helpers ──────────────────────────────────────────────────────────────────

function mockRes() {
    const res = {};
    res.status = jest.fn().mockReturnValue(res);
    res.json   = jest.fn().mockReturnValue(res);
    return res;
}

function mockReq(overrides = {}) {
    return {
        body: {},
        params: {},
        query: {},
        file: null,
        ...overrides
    };
}

function sampleManifest(overrides = {}) {
    return {
        id: 1,
        id_cliente: 1,
        numero_manifiesto: '001/2026',
        tipo: 'peligroso',
        estado: 'borrador',
        razon_social: 'Cliente Test',
        ...overrides
    };
}

beforeEach(() => jest.clearAllMocks());

// ─── POST /api/manifiestos ────────────────────────────────────────────────────

describe('create()', () => {
    test('retorna 400 si id_cliente no está en el body', async () => {
        const req = mockReq({ body: { tipo: 'peligroso' } });
        const res = mockRes();

        await controller.create(req, res);

        expect(res.status).toHaveBeenCalledWith(400);
        expect(res.json).toHaveBeenCalledWith(expect.objectContaining({ success: false }));
    });

    test('retorna 400 si tipo no está en el body', async () => {
        const req = mockReq({ body: { id_cliente: 1 } });
        const res = mockRes();

        await controller.create(req, res);

        expect(res.status).toHaveBeenCalledWith(400);
        expect(res.json).toHaveBeenCalledWith(expect.objectContaining({ success: false }));
    });

    test('retorna 400 si tipo tiene un valor inválido', async () => {
        const req = mockReq({ body: { id_cliente: 1, tipo: 'invalido' } });
        const res = mockRes();

        await controller.create(req, res);

        expect(res.status).toHaveBeenCalledWith(400);
    });

    test('retorna 400 con ambos campos faltantes', async () => {
        const req = mockReq({ body: {} });
        const res = mockRes();

        await controller.create(req, res);

        expect(res.status).toHaveBeenCalledWith(400);
    });

    test('retorna 201 con el manifiesto creado (tipo especial / RME)', async () => {
        const manifest = sampleManifest({ tipo: 'especial', numero_manifiesto: '001/2026' });
        Manifest.create.mockResolvedValue(manifest);

        const req = mockReq({ body: { id_cliente: 1, tipo: 'especial', razon_social: 'X' } });
        const res = mockRes();

        await controller.create(req, res);

        expect(res.status).toHaveBeenCalledWith(201);
        expect(res.json).toHaveBeenCalledWith(
            expect.objectContaining({ success: true, data: manifest })
        );
    });

    test('retorna 201 con el manifiesto creado (tipo peligroso / RP)', async () => {
        const manifest = sampleManifest({ tipo: 'peligroso', numero_manifiesto: '005/2026' });
        Manifest.create.mockResolvedValue(manifest);

        const req = mockReq({ body: { id_cliente: 2, tipo: 'peligroso' } });
        const res = mockRes();

        await controller.create(req, res);

        expect(res.status).toHaveBeenCalledWith(201);
        expect(res.json).toHaveBeenCalledWith(
            expect.objectContaining({ success: true, data: manifest })
        );
    });

    test('retorna 500 si Manifest.create() lanza una excepción', async () => {
        Manifest.create.mockRejectedValue(new Error('DB error'));

        const req = mockReq({ body: { id_cliente: 1, tipo: 'peligroso' } });
        const res = mockRes();

        await controller.create(req, res);

        expect(res.status).toHaveBeenCalledWith(500);
        expect(res.json).toHaveBeenCalledWith(expect.objectContaining({ success: false }));
    });

    test('el folio generado viene en la respuesta (no lo provee el cliente)', async () => {
        const manifest = sampleManifest({ numero_manifiesto: '003/2026' });
        Manifest.create.mockResolvedValue(manifest);

        const req = mockReq({ body: { id_cliente: 1, tipo: 'peligroso' } });
        const res = mockRes();

        await controller.create(req, res);

        const responseBody = res.json.mock.calls[0][0];
        expect(responseBody.data.numero_manifiesto).toBe('003/2026');
        // El body del request NO debe haber tenido numero_manifiesto
        expect(req.body.numero_manifiesto).toBeUndefined();
    });
});

// ─── GET /api/manifiestos ─────────────────────────────────────────────────────

describe('getAll()', () => {
    test('retorna lista de manifiestos', async () => {
        const manifests = [sampleManifest(), sampleManifest({ id: 2 })];
        Manifest.findAll.mockResolvedValue(manifests);

        const req = mockReq({ query: {} });
        const res = mockRes();

        await controller.getAll(req, res);

        expect(res.json).toHaveBeenCalledWith(
            expect.objectContaining({ success: true, count: 2, data: manifests })
        );
    });

    test('pasa los filtros del query al modelo', async () => {
        Manifest.findAll.mockResolvedValue([]);

        const req = mockReq({ query: { tipo: 'peligroso', estado: 'borrador' } });
        const res = mockRes();

        await controller.getAll(req, res);

        expect(Manifest.findAll).toHaveBeenCalledWith(
            expect.objectContaining({ tipo: 'peligroso', estado: 'borrador' })
        );
    });

    test('retorna 500 si el modelo lanza error', async () => {
        Manifest.findAll.mockRejectedValue(new Error('DB error'));

        const req = mockReq({ query: {} });
        const res = mockRes();

        await controller.getAll(req, res);

        expect(res.status).toHaveBeenCalledWith(500);
    });
});

// ─── GET /api/manifiestos/:id ─────────────────────────────────────────────────

describe('getById()', () => {
    test('retorna 200 con el manifiesto cuando existe', async () => {
        const manifest = sampleManifest();
        Manifest.findById.mockResolvedValue(manifest);

        const req = mockReq({ params: { id: '1' } });
        const res = mockRes();

        await controller.getById(req, res);

        expect(res.json).toHaveBeenCalledWith(
            expect.objectContaining({ success: true, data: manifest })
        );
    });

    test('retorna 404 cuando el manifiesto no existe', async () => {
        Manifest.findById.mockResolvedValue(null);

        const req = mockReq({ params: { id: '999' } });
        const res = mockRes();

        await controller.getById(req, res);

        expect(res.status).toHaveBeenCalledWith(404);
        expect(res.json).toHaveBeenCalledWith(expect.objectContaining({ success: false }));
    });

    test('retorna 500 si el modelo lanza error', async () => {
        Manifest.findById.mockRejectedValue(new Error('DB error'));

        const req = mockReq({ params: { id: '1' } });
        const res = mockRes();

        await controller.getById(req, res);

        expect(res.status).toHaveBeenCalledWith(500);
    });
});

// ─── PATCH /api/manifiestos/:id/estado ───────────────────────────────────────

describe('updateStatus()', () => {
    test('retorna 400 para estado inválido', async () => {
        const req = mockReq({ params: { id: '1' }, body: { estado: 'fantasma' } });
        const res = mockRes();

        await controller.updateStatus(req, res);

        expect(res.status).toHaveBeenCalledWith(400);
        expect(res.json).toHaveBeenCalledWith(expect.objectContaining({ success: false }));
    });

    test.each(['borrador', 'en_transito', 'completado'])(
        'acepta el estado válido "%s"', async (estado) => {
            const manifest = sampleManifest({ estado });
            Manifest.updateStatus.mockResolvedValue(manifest);

            const req = mockReq({ params: { id: '1' }, body: { estado } });
            const res = mockRes();

            await controller.updateStatus(req, res);

            expect(res.json).toHaveBeenCalledWith(expect.objectContaining({ success: true }));
        }
    );

    test('retorna 404 cuando el manifiesto no existe', async () => {
        Manifest.updateStatus.mockResolvedValue(null);

        const req = mockReq({ params: { id: '999' }, body: { estado: 'completado' } });
        const res = mockRes();

        await controller.updateStatus(req, res);

        expect(res.status).toHaveBeenCalledWith(404);
    });
});

// ─── DELETE /api/manifiestos/:id ──────────────────────────────────────────────

describe('delete()', () => {
    test('retorna 200 cuando el manifiesto es eliminado (soft-delete)', async () => {
        Manifest.delete.mockResolvedValue(true);

        const req = mockReq({ params: { id: '1' } });
        const res = mockRes();

        await controller.delete(req, res);

        expect(res.json).toHaveBeenCalledWith(expect.objectContaining({ success: true }));
    });

    test('retorna 404 cuando el manifiesto no existe', async () => {
        Manifest.delete.mockResolvedValue(false);

        const req = mockReq({ params: { id: '999' } });
        const res = mockRes();

        await controller.delete(req, res);

        expect(res.status).toHaveBeenCalledWith(404);
    });
});
