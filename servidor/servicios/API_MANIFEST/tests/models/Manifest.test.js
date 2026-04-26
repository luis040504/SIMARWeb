// Mocks deben declararse antes del require del módulo bajo prueba
const mockConn = {
    beginTransaction: jest.fn(),
    commit: jest.fn(),
    rollback: jest.fn(),
    release: jest.fn(),
    query: jest.fn()
};

jest.mock('../../src/config/database', () => ({
    promisePool: {
        getConnection: jest.fn(),
        query: jest.fn()
    }
}));

jest.mock('../../src/services/folioService', () => ({
    generarFolio: jest.fn()
}));

const { promisePool: db } = require('../../src/config/database');
const { generarFolio }    = require('../../src/services/folioService');
const Manifest            = require('../../src/models/Manifest');

// ─── Helpers ──────────────────────────────────────────────────────────────────

function buildData(overrides = {}) {
    return {
        id_cliente: 1,
        tipo: 'peligroso',
        estado: 'borrador',
        razon_social: 'Cliente Test S.A.',
        fecha_manifiesto: '2026-04-26',
        ...overrides
    };
}

function buildManifestRow(overrides = {}) {
    return {
        id: 1,
        id_cliente: 1,
        numero_manifiesto: '001/2026',
        tipo: 'peligroso',
        estado: 'borrador',
        razon_social: 'Cliente Test S.A.',
        activo: true,
        residuos: [],
        ...overrides
    };
}

// ─── create() ─────────────────────────────────────────────────────────────────

describe('Manifest.create()', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        db.getConnection.mockResolvedValue(mockConn);
        generarFolio.mockResolvedValue('001/2026');
        mockConn.query
            .mockResolvedValueOnce([{ insertId: 1 }])  // INSERT manifiestos
            .mockResolvedValueOnce([[buildManifestRow()]]); // SELECT en findById
        db.query.mockResolvedValue([[buildManifestRow()]]);
    });

    test('llama a generarFolio con clienteId, tipo y la conexión activa', async () => {
        const data = buildData();
        await Manifest.create(data);

        expect(generarFolio).toHaveBeenCalledWith(1, 'peligroso', mockConn);
    });

    test('el folio generado se inserta en la base de datos', async () => {
        generarFolio.mockResolvedValue('003/2026');
        const data = buildData();
        await Manifest.create(data);

        const insertSql = mockConn.query.mock.calls[0][0];
        const insertValues = mockConn.query.mock.calls[0][1];
        expect(insertSql).toMatch(/INSERT INTO manifiestos/);
        expect(insertValues).toContain('003/2026');
    });

    test('el id_cliente se incluye en el INSERT', async () => {
        await Manifest.create(buildData({ id_cliente: 5 }));

        const insertValues = mockConn.query.mock.calls[0][1];
        expect(insertValues).toContain(5);
    });

    test('confirma la transacción (commit) en caso de éxito', async () => {
        await Manifest.create(buildData());
        expect(mockConn.commit).toHaveBeenCalledTimes(1);
    });

    test('hace rollback si generarFolio lanza un error', async () => {
        generarFolio.mockRejectedValue(new Error('Deadlock'));
        await expect(Manifest.create(buildData())).rejects.toThrow('Deadlock');
        expect(mockConn.rollback).toHaveBeenCalledTimes(1);
    });

    test('hace rollback si el INSERT falla', async () => {
        // mockReset limpia también la cola de implementaciones pendientes del beforeEach
        mockConn.query.mockReset();
        db.getConnection.mockResolvedValue(mockConn);
        generarFolio.mockResolvedValue('001/2026');
        mockConn.query.mockRejectedValueOnce(new Error('Duplicate entry'));

        await expect(Manifest.create(buildData())).rejects.toThrow('Duplicate entry');
        expect(mockConn.rollback).toHaveBeenCalledTimes(1);
    });

    test('libera la conexión siempre (finally), incluso con error', async () => {
        generarFolio.mockRejectedValue(new Error('Error'));
        await expect(Manifest.create(buildData())).rejects.toThrow();
        expect(mockConn.release).toHaveBeenCalledTimes(1);
    });

    test('lanza error si id_cliente no está presente', async () => {
        const data = buildData({ id_cliente: undefined });
        await expect(Manifest.create(data)).rejects.toThrow('id_cliente es requerido');
    });

    test('lanza error si tipo no está presente', async () => {
        const data = buildData({ tipo: undefined });
        await expect(Manifest.create(data)).rejects.toThrow('tipo es requerido');
    });
});

// ─── findById() ───────────────────────────────────────────────────────────────

describe('Manifest.findById()', () => {
    beforeEach(() => jest.clearAllMocks());

    test('retorna el manifiesto cuando existe', async () => {
        const row = buildManifestRow({ tipo: 'especial' });
        db.query
            .mockResolvedValueOnce([[row]])  // manifiestos
            .mockResolvedValueOnce([[]]);    // residuos

        const result = await Manifest.findById(1);

        expect(result).not.toBeNull();
        expect(result.id).toBe(1);
        expect(result.numero_manifiesto).toBe('001/2026');
    });

    test('retorna null cuando el manifiesto no existe', async () => {
        db.query.mockResolvedValueOnce([[]]); // sin resultados

        const result = await Manifest.findById(999);

        expect(result).toBeNull();
    });
});

// ─── findAll() ────────────────────────────────────────────────────────────────

describe('Manifest.findAll()', () => {
    beforeEach(() => jest.clearAllMocks());

    test('retorna lista de manifiestos', async () => {
        const rows = [buildManifestRow({ id: 1 }), buildManifestRow({ id: 2 })];
        db.query
            .mockResolvedValueOnce([rows])  // query principal
            .mockResolvedValue([[]])        // _getResidueSummary por cada uno

        const result = await Manifest.findAll();

        expect(result).toHaveLength(2);
    });

    test('aplica filtro de tipo', async () => {
        db.query
            .mockResolvedValueOnce([[]])
            .mockResolvedValue([[]]);

        await Manifest.findAll({ tipo: 'peligroso' });

        const sql = db.query.mock.calls[0][0];
        expect(sql).toContain('m.tipo = ?');
    });

    test('aplica filtro de estado', async () => {
        db.query
            .mockResolvedValueOnce([[]])
            .mockResolvedValue([[]]);

        await Manifest.findAll({ estado: 'completado' });

        const sql = db.query.mock.calls[0][0];
        expect(sql).toContain('m.estado = ?');
    });

    test('aplica filtro de rango de fechas', async () => {
        db.query
            .mockResolvedValueOnce([[]])
            .mockResolvedValue([[]]);

        await Manifest.findAll({ fecha_desde: '2026-01-01', fecha_hasta: '2026-12-31' });

        const sql = db.query.mock.calls[0][0];
        expect(sql).toContain('fecha_manifiesto >=');
        expect(sql).toContain('fecha_manifiesto <=');
    });

    test('retorna lista vacía si no hay manifiestos', async () => {
        db.query.mockResolvedValueOnce([[]]);

        const result = await Manifest.findAll();

        expect(result).toEqual([]);
    });
});

// ─── delete() ─────────────────────────────────────────────────────────────────

describe('Manifest.delete()', () => {
    beforeEach(() => jest.clearAllMocks());

    test('retorna true cuando el soft-delete afecta una fila', async () => {
        db.query.mockResolvedValueOnce([{ affectedRows: 1 }]);

        const result = await Manifest.delete(1);

        expect(result).toBe(true);
    });

    test('retorna false cuando el manifiesto no existe', async () => {
        db.query.mockResolvedValueOnce([{ affectedRows: 0 }]);

        const result = await Manifest.delete(999);

        expect(result).toBe(false);
    });

    test('ejecuta soft-delete (activo = FALSE) no DELETE físico', async () => {
        db.query.mockResolvedValueOnce([{ affectedRows: 1 }]);

        await Manifest.delete(1);

        const sql = db.query.mock.calls[0][0];
        expect(sql).toMatch(/activo\s*=\s*FALSE/i);
        expect(sql).not.toMatch(/DELETE FROM/i);
    });
});

// ─── updateStatus() ───────────────────────────────────────────────────────────

describe('Manifest.updateStatus()', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        db.query
            .mockResolvedValueOnce([{ affectedRows: 1 }])     // UPDATE estado
            .mockResolvedValueOnce([[buildManifestRow()]])     // findById manifiestos
            .mockResolvedValueOnce([[]]);                      // findById residuos
    });

    test('actualiza el estado del manifiesto', async () => {
        await Manifest.updateStatus(1, 'en_transito');

        const sql = db.query.mock.calls[0][0];
        expect(sql).toContain('estado = ?');
    });

    test('retorna el manifiesto actualizado', async () => {
        const result = await Manifest.updateStatus(1, 'completado');
        expect(result).not.toBeNull();
    });
});
