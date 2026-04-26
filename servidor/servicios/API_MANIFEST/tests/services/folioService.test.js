const { generarFolio } = require('../../src/services/folioService');

describe('generarFolio', () => {
    let mockConn;
    const ANIO = new Date().getFullYear();

    beforeEach(() => {
        mockConn = { execute: jest.fn() };
    });

    // ─── Formato del folio ─────────────────────────────────────────────────────

    test('primer folio de un cliente nuevo es 001/ANIO', async () => {
        mockConn.execute
            .mockResolvedValueOnce([{}])                           // upsert
            .mockResolvedValueOnce([[{ ultimo_consecutivo: 1 }]]); // select

        const folio = await generarFolio(1, 'peligroso', mockConn);

        expect(folio).toBe(`001/${ANIO}`);
    });

    test('segundo folio del mismo cliente es 002/ANIO', async () => {
        mockConn.execute
            .mockResolvedValueOnce([{}])
            .mockResolvedValueOnce([[{ ultimo_consecutivo: 2 }]]);

        const folio = await generarFolio(1, 'peligroso', mockConn);

        expect(folio).toBe(`002/${ANIO}`);
    });

    test('consecutivo 10 se formatea como 010/ANIO (padding de 3 dígitos)', async () => {
        mockConn.execute
            .mockResolvedValueOnce([{}])
            .mockResolvedValueOnce([[{ ultimo_consecutivo: 10 }]]);

        const folio = await generarFolio(1, 'especial', mockConn);

        expect(folio).toBe(`010/${ANIO}`);
    });

    test('consecutivo 100 se formatea sin padding como 100/ANIO', async () => {
        mockConn.execute
            .mockResolvedValueOnce([{}])
            .mockResolvedValueOnce([[{ ultimo_consecutivo: 100 }]]);

        const folio = await generarFolio(1, 'peligroso', mockConn);

        expect(folio).toBe(`100/${ANIO}`);
    });

    test('el folio contiene el año actual', async () => {
        mockConn.execute
            .mockResolvedValueOnce([{}])
            .mockResolvedValueOnce([[{ ultimo_consecutivo: 5 }]]);

        const folio = await generarFolio(1, 'especial', mockConn);

        expect(folio).toMatch(new RegExp(`/${ANIO}$`));
    });

    // ─── Secuencias independientes por tipo ───────────────────────────────────

    test('RP y RME tienen secuencias independientes para el mismo cliente', async () => {
        // Primer folio RP
        mockConn.execute
            .mockResolvedValueOnce([{}])
            .mockResolvedValueOnce([[{ ultimo_consecutivo: 1 }]]);
        const folioRP = await generarFolio(1, 'peligroso', mockConn);

        // Primer folio RME — secuencia propia: también comienza en 001
        mockConn.execute
            .mockResolvedValueOnce([{}])
            .mockResolvedValueOnce([[{ ultimo_consecutivo: 1 }]]);
        const folioRME = await generarFolio(1, 'especial', mockConn);

        expect(folioRP).toBe(`001/${ANIO}`);
        expect(folioRME).toBe(`001/${ANIO}`);
    });

    test('RP avanza independiente de RME: tercer RP no interfiere con RME', async () => {
        mockConn.execute
            .mockResolvedValueOnce([{}])
            .mockResolvedValueOnce([[{ ultimo_consecutivo: 3 }]]);
        const folioRP3 = await generarFolio(1, 'peligroso', mockConn);

        mockConn.execute
            .mockResolvedValueOnce([{}])
            .mockResolvedValueOnce([[{ ultimo_consecutivo: 1 }]]);
        const folioRME1 = await generarFolio(1, 'especial', mockConn);

        expect(folioRP3).toBe(`003/${ANIO}`);
        expect(folioRME1).toBe(`001/${ANIO}`);
    });

    // ─── Secuencias independientes por cliente ────────────────────────────────

    test('clientes distintos tienen secuencias independientes', async () => {
        mockConn.execute
            .mockResolvedValueOnce([{}])
            .mockResolvedValueOnce([[{ ultimo_consecutivo: 1 }]]);
        const folioCliente1 = await generarFolio(1, 'peligroso', mockConn);

        mockConn.execute
            .mockResolvedValueOnce([{}])
            .mockResolvedValueOnce([[{ ultimo_consecutivo: 1 }]]);
        const folioCliente2 = await generarFolio(2, 'peligroso', mockConn);

        expect(folioCliente1).toBe(`001/${ANIO}`);
        expect(folioCliente2).toBe(`001/${ANIO}`);
    });

    // ─── Operación atómica ────────────────────────────────────────────────────

    test('el upsert usa ON DUPLICATE KEY UPDATE (operación atómica)', async () => {
        mockConn.execute
            .mockResolvedValueOnce([{}])
            .mockResolvedValueOnce([[{ ultimo_consecutivo: 1 }]]);

        await generarFolio(1, 'peligroso', mockConn);

        const sqlUpsert = mockConn.execute.mock.calls[0][0];
        expect(sqlUpsert).toMatch(/ON DUPLICATE KEY UPDATE/i);
    });

    test('el upsert recibe clienteId, tipo y año como parámetros', async () => {
        mockConn.execute
            .mockResolvedValueOnce([{}])
            .mockResolvedValueOnce([[{ ultimo_consecutivo: 1 }]]);

        await generarFolio(7, 'especial', mockConn);

        const paramsUpsert = mockConn.execute.mock.calls[0][1];
        expect(paramsUpsert[0]).toBe(7);
        expect(paramsUpsert[1]).toBe('especial');
        expect(paramsUpsert[2]).toBe(ANIO);
    });

    test('el select de consecutivo recibe los mismos parámetros que el upsert', async () => {
        mockConn.execute
            .mockResolvedValueOnce([{}])
            .mockResolvedValueOnce([[{ ultimo_consecutivo: 1 }]]);

        await generarFolio(3, 'peligroso', mockConn);

        const paramsSelect = mockConn.execute.mock.calls[1][1];
        expect(paramsSelect).toEqual([3, 'peligroso', ANIO]);
    });

    // ─── Manejo de errores ────────────────────────────────────────────────────

    test('propaga el error si el upsert falla (permite rollback de la transacción)', async () => {
        mockConn.execute.mockRejectedValueOnce(new Error('Deadlock found'));

        await expect(generarFolio(1, 'peligroso', mockConn))
            .rejects.toThrow('Deadlock found');
    });

    test('propaga el error si el select falla', async () => {
        mockConn.execute
            .mockResolvedValueOnce([{}])
            .mockRejectedValueOnce(new Error('Connection lost'));

        await expect(generarFolio(1, 'especial', mockConn))
            .rejects.toThrow('Connection lost');
    });
});
