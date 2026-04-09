const { promisePool: db } = require('../config/database');

class Manifest {

    // ─── CONSULTA ────────────────────────────────────────────────────────────

    static async findAll(filters = {}) {
        let query = `
            SELECT
                m.id,
                m.numero_manifiesto,
                m.tipo,
                m.estado,
                m.razon_social,
                m.municipio,
                m.fecha_manifiesto,
                m.razon_social_transportista,
                m.fecha_creacion
            FROM manifiestos m
            WHERE m.activo = TRUE
        `;
        const params = [];

        if (filters.numero) {
            query += ' AND m.numero_manifiesto LIKE ?';
            params.push(`%${filters.numero}%`);
        }
        if (filters.razon_social) {
            query += ' AND m.razon_social LIKE ?';
            params.push(`%${filters.razon_social}%`);
        }
        if (filters.tipo) {
            query += ' AND m.tipo = ?';
            params.push(filters.tipo);
        }
        if (filters.estado) {
            query += ' AND m.estado = ?';
            params.push(filters.estado);
        }
        if (filters.fecha_desde) {
            query += ' AND m.fecha_manifiesto >= ?';
            params.push(filters.fecha_desde);
        }
        if (filters.fecha_hasta) {
            query += ' AND m.fecha_manifiesto <= ?';
            params.push(filters.fecha_hasta);
        }

        query += ' ORDER BY m.fecha_creacion DESC';

        const [rows] = await db.query(query, params);

        // Adjuntar resumen de residuos
        for (const m of rows) {
            m.resumen_residuos = await this._getResidueSummary(m.id, m.tipo);
        }

        return rows;
    }

    static async findById(id) {
        const [rows] = await db.query(
            'SELECT * FROM manifiestos WHERE id = ? AND activo = TRUE',
            [id]
        );
        if (!rows[0]) return null;

        const manifest = rows[0];
        manifest.residuos = await this._getResidues(id, manifest.tipo);
        return manifest;
    }

    // ─── CREAR ────────────────────────────────────────────────────────────────

    static async create(data) {
        const conn = await db.getConnection();
        try {
            await conn.beginTransaction();

            const fields = this._buildFields(data);
            const [result] = await conn.query(
                `INSERT INTO manifiestos (${fields.columns}) VALUES (${fields.placeholders})`,
                fields.values
            );
            const manifestId = result.insertId;

            await this._insertResidues(conn, manifestId, data);

            await conn.commit();
            return this.findById(manifestId);
        } catch (err) {
            await conn.rollback();
            throw err;
        } finally {
            conn.release();
        }
    }

    // ─── ACTUALIZAR ───────────────────────────────────────────────────────────

    static async update(id, data) {
        const conn = await db.getConnection();
        try {
            await conn.beginTransaction();

            const fields = this._buildFields(data);
            await conn.query(
                `UPDATE manifiestos SET ${fields.updates}, fecha_actualizacion = NOW() WHERE id = ?`,
                [...fields.values, id]
            );

            // Reemplazar residuos
            await conn.query('DELETE FROM residuos_especiales WHERE manifiesto_id = ?', [id]);
            await conn.query('DELETE FROM residuos_peligrosos WHERE manifiesto_id = ?', [id]);
            await this._insertResidues(conn, id, data);

            await conn.commit();
            return this.findById(id);
        } catch (err) {
            await conn.rollback();
            throw err;
        } finally {
            conn.release();
        }
    }

    // ─── ACTUALIZAR ESTADO ────────────────────────────────────────────────────

    static async updateStatus(id, estado, firmaData = {}) {
        const sets = ['estado = ?', 'fecha_actualizacion = NOW()'];
        const values = [estado];

        if (firmaData.fecha_firma) {
            sets.push('fecha_firma = ?');
            values.push(firmaData.fecha_firma);
        }
        if (firmaData.nombre_archivo_firmado) {
            sets.push('nombre_archivo_firmado = ?');
            values.push(firmaData.nombre_archivo_firmado);
        }

        values.push(id);
        await db.query(`UPDATE manifiestos SET ${sets.join(', ')} WHERE id = ?`, values);
        return this.findById(id);
    }

    // ─── ELIMINAR (soft) ──────────────────────────────────────────────────────

    static async delete(id) {
        const [result] = await db.query(
            'UPDATE manifiestos SET activo = FALSE, fecha_actualizacion = NOW() WHERE id = ?',
            [id]
        );
        return result.affectedRows > 0;
    }

    // ─── PDF FIRMA ────────────────────────────────────────────────────────────

    static async setFirmaFile(id, filename) {
        await db.query(
            `UPDATE manifiestos
             SET nombre_archivo_firmado = ?, estado = 'completado', fecha_actualizacion = NOW()
             WHERE id = ?`,
            [filename, id]
        );
        return this.findById(id);
    }

    static async getFirmaFilename(id) {
        const [rows] = await db.query(
            'SELECT nombre_archivo_firmado FROM manifiestos WHERE id = ? AND activo = TRUE',
            [id]
        );
        return rows[0]?.nombre_archivo_firmado || null;
    }

    // ─── HELPERS PRIVADOS ─────────────────────────────────────────────────────

    static _buildFields(data) {
        const map = {
            numero_manifiesto: data.numero_manifiesto,
            tipo: data.tipo,
            estado: data.estado,
            // Generador
            numero_registro_ambiental: data.numero_registro_ambiental,
            razon_social: data.razon_social,
            domicilio: data.domicilio,
            calle: data.calle,
            numero_exterior: data.numero_exterior,
            numero_interior: data.numero_interior,
            colonia: data.colonia,
            estado_generador: data.estado_generador,
            codigo_postal: data.codigo_postal,
            municipio: data.municipio,
            telefono: data.telefono,
            correo: data.correo,
            fecha_manifiesto: data.fecha_manifiesto,
            hora_manifiesto: data.hora_manifiesto,
            observaciones_generador: data.observaciones_generador,
            instrucciones_manejo_seguro: data.instrucciones_manejo_seguro,
            nombre_responsable_generador: data.nombre_responsable_generador,
            fecha_firma_generador: data.fecha_firma_generador,
            // Transportista
            numero_autorizacion_transportista: data.numero_autorizacion_transportista,
            numero_permiso_sct: data.numero_permiso_sct,
            razon_social_transportista: data.razon_social_transportista,
            domicilio_transportista: data.domicilio_transportista,
            calle_transportista: data.calle_transportista,
            numero_exterior_transportista: data.numero_exterior_transportista,
            numero_interior_transportista: data.numero_interior_transportista,
            colonia_transportista: data.colonia_transportista,
            estado_transportista: data.estado_transportista,
            correo_transportista: data.correo_transportista,
            codigo_postal_transportista: data.codigo_postal_transportista,
            municipio_transportista: data.municipio_transportista,
            telefono_transportista: data.telefono_transportista,
            tipo_vehiculo: data.tipo_vehiculo,
            placa: data.placa,
            licencia_conductor: data.licencia_conductor,
            ruta_transporte: data.ruta_transporte,
            observaciones_transportista: data.observaciones_transportista,
            nombre_responsable_transportista: data.nombre_responsable_transportista,
            fecha_recepcion_transportista: data.fecha_recepcion_transportista,
            hora_recepcion_transportista: data.hora_recepcion_transportista,
            fecha_firma_transportista: data.fecha_firma_transportista,
            // Destinatario
            numero_autorizacion_destinatario: data.numero_autorizacion_destinatario,
            razon_social_destinatario: data.razon_social_destinatario,
            domicilio_destinatario: data.domicilio_destinatario,
            calle_destinatario: data.calle_destinatario,
            numero_exterior_destinatario: data.numero_exterior_destinatario,
            numero_interior_destinatario: data.numero_interior_destinatario,
            colonia_destinatario: data.colonia_destinatario,
            estado_destinatario: data.estado_destinatario,
            correo_destinatario: data.correo_destinatario,
            codigo_postal_destinatario: data.codigo_postal_destinatario,
            municipio_destinatario: data.municipio_destinatario,
            telefono_destinatario: data.telefono_destinatario,
            tipo_disposicion: data.tipo_disposicion,
            fecha_destinatario: data.fecha_destinatario,
            hora_destinatario: data.hora_destinatario,
            persona_recibe: data.persona_recibe,
            fecha_firma_destinatario: data.fecha_firma_destinatario,
            nombre_responsable_destinatario: data.nombre_responsable_destinatario,
            observaciones_destinatario: data.observaciones_destinatario
        };

        const defined = Object.entries(map).filter(([, v]) => v !== undefined && v !== null);
        return {
            columns: defined.map(([k]) => k).join(', '),
            placeholders: defined.map(() => '?').join(', '),
            updates: defined.map(([k]) => `${k} = ?`).join(', '),
            values: defined.map(([, v]) => v)
        };
    }

    static async _insertResidues(conn, manifestId, data) {
        if (data.residuos_especiales?.length) {
            for (const r of data.residuos_especiales) {
                await conn.query(
                    `INSERT INTO residuos_especiales
                     (manifiesto_id, clave_residuo, nombre_residuo, tipo_envase, capacidad, peso, unidad)
                     VALUES (?, ?, ?, ?, ?, ?, ?)`,
                    [manifestId, r.clave_residuo, r.nombre_residuo, r.tipo_envase,
                     r.capacidad, r.peso, r.unidad || 'kg']
                );
            }
        }
        if (data.residuos_peligrosos?.length) {
            for (const r of data.residuos_peligrosos) {
                await conn.query(
                    `INSERT INTO residuos_peligrosos
                     (manifiesto_id, nombre_residuo, es_corrosivo, es_reactivo, es_explosivo,
                      es_toxico, es_inflamable, es_biologico, es_mutagenico,
                      tipo_envase, capacidad_envase, cantidad_kg, tiene_etiqueta)
                     VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)`,
                    [manifestId, r.nombre_residuo,
                     r.es_corrosivo || false, r.es_reactivo || false, r.es_explosivo || false,
                     r.es_toxico || false, r.es_inflamable || false, r.es_biologico || false,
                     r.es_mutagenico || false,
                     r.tipo_envase, r.capacidad_envase, r.cantidad_kg, r.tiene_etiqueta ?? null]
                );
            }
        }
    }

    static async _getResidues(id, tipo) {
        if (tipo === 'especial') {
            const [rows] = await db.query(
                'SELECT * FROM residuos_especiales WHERE manifiesto_id = ?', [id]
            );
            return rows;
        } else {
            const [rows] = await db.query(
                'SELECT * FROM residuos_peligrosos WHERE manifiesto_id = ?', [id]
            );
            return rows;
        }
    }

    static async _getResidueSummary(id, tipo) {
        if (tipo === 'especial') {
            const [rows] = await db.query(
                'SELECT nombre_residuo, peso, unidad FROM residuos_especiales WHERE manifiesto_id = ?', [id]
            );
            return rows.map(r => `${r.nombre_residuo} (${r.peso} ${r.unidad})`).join(', ');
        } else {
            const [rows] = await db.query(
                'SELECT nombre_residuo, cantidad_kg FROM residuos_peligrosos WHERE manifiesto_id = ?', [id]
            );
            return rows.map(r => `${r.nombre_residuo} (${r.cantidad_kg} kg)`).join(', ');
        }
    }
}

module.exports = Manifest;
