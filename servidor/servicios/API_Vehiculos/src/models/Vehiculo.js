const { promisePool: db } = require('../config/database');

class Vehiculo {
    
    static async findAll() {
        const [rows] = await db.query('SELECT * FROM v_vehiculos_completo');
        return rows;
    }

    static async search(filtro) {
        const [rows] = await db.query('CALL sp_buscar_vehiculos(?)', [filtro]);
        return rows[0];
    }

    static async findById(id) {
        const [rows] = await db.query('SELECT * FROM v_vehiculos_completo WHERE id = ?', [id]);
        return rows[0];
    }

    static async getTiposGasolina() {
        const [rows] = await db.query('SELECT nombre FROM tipos_gasolina');
        return rows.map(row => row.nombre);
    }

    // Métodos para sincronizar con el catálogo de residuos
    static async sincronizarCatalogoResiduos(wasteTypesFromCatalog) {
        // Limpiar tabla temporalmente o actualizar selectivamente
        for (const waste of wasteTypesFromCatalog) {
            await db.query(
                `INSERT INTO tipos_residuo_catalogo (codigo_catalogo, nombre, tipo_residuo, descripcion, activo)
                 VALUES (?, ?, ?, ?, TRUE)
                 ON DUPLICATE KEY UPDATE
                 nombre = VALUES(nombre),
                 tipo_residuo = VALUES(tipo_residuo),
                 descripcion = VALUES(descripcion),
                 activo = TRUE`,
                [waste.code, waste.name, waste.type, waste.description || null]
            );
        }
        
        // Marcar como inactivos los que ya no existen en el catálogo
        const activeCodes = wasteTypesFromCatalog.map(w => w.code);
        if (activeCodes.length > 0) {
            const placeholders = activeCodes.map(() => '?').join(',');
            await db.query(
                `UPDATE tipos_residuo_catalogo 
                 SET activo = FALSE 
                 WHERE codigo_catalogo NOT IN (${placeholders})`,
                activeCodes
            );
        }
    }

    static async getTiposResiduoDisponibles() {
        const [rows] = await db.query(
            'SELECT id, codigo_catalogo, nombre, tipo_residuo, descripcion FROM tipos_residuo_catalogo WHERE activo = TRUE ORDER BY codigo_catalogo'
        );
        return rows;
    }

    static async isPlacasUnique(placas, excludeId = null) {
        let query = 'SELECT id FROM vehiculos WHERE placas = ?';
        let params = [placas];
        
        if (excludeId) {
            query += ' AND id != ?';
            params.push(excludeId);
        }
        
        const [rows] = await db.query(query, params);
        return rows.length === 0;
    }

    static async isNumeroEconomicoUnique(numeroEconomico, excludeId = null) {
        if (!numeroEconomico) return true;
        
        let query = 'SELECT id FROM vehiculos WHERE numero_economico = ?';
        let params = [numeroEconomico];
        
        if (excludeId) {
            query += ' AND id != ?';
            params.push(excludeId);
        }
        
        const [rows] = await db.query(query, params);
        return rows.length === 0;
    }

    static async create(data) {
        const { 
            numero_economico, marca, modelo, anio, color, placas, 
            peso_toneladas, licencia_requerida, tipo_gasolina, 
            tipos_residuo_ids,  // Array de IDs de tipos de residuo
            descripcion, foto    // foto es buffer binario
        } = data;
        
        // Validaciones de unicidad
        const isPlacasUnique = await this.isPlacasUnique(placas);
        if (!isPlacasUnique) {
            throw new Error('PLACAS_DUPLICADAS');
        }
        
        if (numero_economico) {
            const isNumeroEconomicoUnique = await this.isNumeroEconomicoUnique(numero_economico);
            if (!isNumeroEconomicoUnique) {
                throw new Error('NUMERO_ECONOMICO_DUPLICADO');
            }
        }

        // Iniciar transacción
        const connection = await db.getConnection();
        await connection.beginTransaction();

        try {
            // Insertar vehículo
            const [result] = await connection.query(
                `INSERT INTO vehiculos (numero_economico, marca, modelo, anio, color, placas, 
                 peso_toneladas, licencia_requerida, tipo_gasolina, descripcion, foto)
                 VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)`,
                [numero_economico, marca, modelo, anio, color, placas, peso_toneladas, 
                 licencia_requerida, tipo_gasolina, descripcion, foto]
            );

            const vehiculoId = result.insertId;

            // Insertar relaciones con tipos de residuo
            if (tipos_residuo_ids && tipos_residuo_ids.length > 0) {
                for (const tipoResiduoId of tipos_residuo_ids) {
                    await connection.query(
                        `INSERT INTO vehiculo_tipo_residuo (vehiculo_id, tipo_residuo_id)
                         VALUES (?, ?)`,
                        [vehiculoId, tipoResiduoId]
                    );
                }
            }

            await connection.commit();
            return this.findById(vehiculoId);
        } catch (error) {
            await connection.rollback();
            throw error;
        } finally {
            connection.release();
        }
    }

    static async update(id, data) {
        const { 
            numero_economico, marca, modelo, anio, color, placas, 
            peso_toneladas, licencia_requerida, tipo_gasolina, 
            tipos_residuo_ids,  // Array de IDs de tipos de residuo
            descripcion, foto
        } = data;
        
        const existing = await this.findById(id);
        if (!existing) return null;
        
        // Validaciones de unicidad
        if (placas && placas !== existing.placas) {
            const isPlacasUnique = await this.isPlacasUnique(placas, id);
            if (!isPlacasUnique) {
                throw new Error('PLACAS_DUPLICADAS');
            }
        }
        
        if (numero_economico && numero_economico !== existing.numero_economico) {
            const isNumeroEconomicoUnique = await this.isNumeroEconomicoUnique(numero_economico, id);
            if (!isNumeroEconomicoUnique) {
                throw new Error('NUMERO_ECONOMICO_DUPLICADO');
            }
        }

        const connection = await db.getConnection();
        await connection.beginTransaction();

        try {
            // Actualizar vehículo
            await connection.query(
                `UPDATE vehiculos 
                 SET numero_economico = ?, marca = ?, modelo = ?, anio = ?, color = ?, 
                     placas = ?, peso_toneladas = ?, licencia_requerida = ?, 
                     tipo_gasolina = ?, descripcion = ?, foto = ?
                 WHERE id = ?`,
                [numero_economico, marca, modelo, anio, color, placas, peso_toneladas, 
                 licencia_requerida, tipo_gasolina, descripcion, foto, id]
            );

            // Actualizar relaciones (borrar existentes y crear nuevas)
            await connection.query('DELETE FROM vehiculo_tipo_residuo WHERE vehiculo_id = ?', [id]);
            
            if (tipos_residuo_ids && tipos_residuo_ids.length > 0) {
                for (const tipoResiduoId of tipos_residuo_ids) {
                    await connection.query(
                        `INSERT INTO vehiculo_tipo_residuo (vehiculo_id, tipo_residuo_id)
                         VALUES (?, ?)`,
                        [id, tipoResiduoId]
                    );
                }
            }

            await connection.commit();
            return this.findById(id);
        } catch (error) {
            await connection.rollback();
            throw error;
        } finally {
            connection.release();
        }
    }

    static async delete(id) {
        const [result] = await db.query('UPDATE vehiculos SET activo = FALSE WHERE id = ?', [id]);
        return result.affectedRows > 0;
    }
}

module.exports = Vehiculo;