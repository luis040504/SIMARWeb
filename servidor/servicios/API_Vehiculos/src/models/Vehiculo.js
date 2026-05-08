const { promisePool: db } = require('../config/database');

class Vehiculo {
    
    static async findAll() {
        const [rows] = await db.query('SELECT id, numero_economico, marca, modelo, anio, color, placas, peso_toneladas, licencia_requerida, tipo_gasolina, tipo_desecho, descripcion, foto_url FROM vehiculos WHERE activo = TRUE');
        return rows;
    }

    static async search(filtro) {
        const [rows] = await db.query('CALL sp_buscar_vehiculos(?)', [filtro]);
        return rows[0];
    }

    static async findById(id) {
        const [rows] = await db.query('SELECT id, numero_economico, marca, modelo, anio, color, placas, peso_toneladas, licencia_requerida, tipo_gasolina, tipo_desecho, descripcion, foto_url FROM vehiculos WHERE id = ? AND activo = TRUE', [id]);
        return rows[0];
    }

    static async getTiposDesecho() {
        const [rows] = await db.query('SELECT nombre FROM tipos_desecho WHERE activo = TRUE');
        return rows.map(row => row.nombre);
    }

    static async getTiposGasolina() {
        const [rows] = await db.query('SELECT nombre FROM tipos_gasolina');
        return rows.map(row => row.nombre);
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
            tipo_desecho, descripcion, foto_url 
        } = data;
        
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
        
        const [result] = await db.query(
            `INSERT INTO vehiculos (numero_economico, marca, modelo, anio, color, placas, peso_toneladas, licencia_requerida, tipo_gasolina, tipo_desecho, descripcion, foto_url)
             VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)`,
            [numero_economico, marca, modelo, anio, color, placas, peso_toneladas, licencia_requerida, tipo_gasolina, tipo_desecho, descripcion, foto_url]
        );
        
        return this.findById(result.insertId);
    }

    static async update(id, data) {
        const { 
            numero_economico, marca, modelo, anio, color, placas, 
            peso_toneladas, licencia_requerida, tipo_gasolina, 
            tipo_desecho, descripcion, foto_url 
        } = data;
        
        const existing = await this.findById(id);
        if (!existing) return null;
        
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
        
        await db.query(
            `UPDATE vehiculos 
             SET numero_economico = ?, marca = ?, modelo = ?, anio = ?, color = ?, 
                 placas = ?, peso_toneladas = ?, licencia_requerida = ?, 
                 tipo_gasolina = ?, tipo_desecho = ?, descripcion = ?, foto_url = ?
             WHERE id = ?`,
            [numero_economico, marca, modelo, anio, color, placas, peso_toneladas, 
             licencia_requerida, tipo_gasolina, tipo_desecho, descripcion, foto_url, id]
        );
        
        return this.findById(id);
    }

    static async delete(id) {
        const [result] = await db.query('UPDATE vehiculos SET activo = FALSE WHERE id = ?', [id]);
        return result.affectedRows > 0;
    }
}

module.exports = Vehiculo;