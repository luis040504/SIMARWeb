/**
 * Genera el número de manifiesto consecutivo por cliente y tipo de residuo.
 *
 * Reglas de negocio:
 *  - Cada cliente tiene contadores independientes por tipo: 'peligroso' (RP) y 'especial' (RME).
 *  - El contador reinicia cada año natural.
 *  - La operación es atómica (ON DUPLICATE KEY UPDATE) para evitar folios duplicados
 *    bajo concurrencia.
 *
 * Formato resultante: "NNN/AAAA"  (ej. "001/2026", "012/2026", "100/2026")
 *
 * @param {number} clienteId  - ID del cliente registrado
 * @param {'especial'|'peligroso'} tipo - Tipo de residuo
 * @param {object} conn - Conexión MySQL activa (dentro de una transacción)
 * @returns {Promise<string>} Folio generado
 */
async function generarFolio(clienteId, tipo, conn) {
    const anio = new Date().getFullYear();

    // Upsert atómico: crea la fila con consecutivo=1 la primera vez,
    // o incrementa el contador en 1 si ya existe.
    await conn.execute(
        `INSERT INTO secuencias_manifiesto (id_cliente, tipo, anio, ultimo_consecutivo)
         VALUES (?, ?, ?, 1)
         ON DUPLICATE KEY UPDATE ultimo_consecutivo = ultimo_consecutivo + 1`,
        [clienteId, tipo, anio]
    );

    const [[row]] = await conn.execute(
        `SELECT ultimo_consecutivo
         FROM secuencias_manifiesto
         WHERE id_cliente = ? AND tipo = ? AND anio = ?`,
        [clienteId, tipo, anio]
    );

    const consecutivo = row.ultimo_consecutivo;
    return `${String(consecutivo).padStart(3, '0')}/${anio}`;
}

module.exports = { generarFolio };
