const db = require('../config/database');

class Vehiculo {
    
    static async findAll() {
        const [rows] = await db.query('SELECT * FROM vehiculos WHERE activo = TRUE');
        return rows;
    }

    
    static async search(filtro) {
        const [rows] = await db.query('CALL sp_buscar_vehiculos(?)', [filtro]);
        return rows[0];
    }

    
    static async findById(id) {
        const [rows] = await db.query('SELECT * FROM vehiculos WHERE id = ? AND activo = TRUE', [id]);
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

    
    static async create(data) {
        const { numero_economico, marca, modelo, año, color, placas, peso_toneladas, licencia_requerida, tipo_gasolina, tipo_desecho, descripcion, foto_url } = data;
        
        const [result] = await db.query(
            `INSERT INTO vehiculos (numero_economico, marca, modelo, año, color, placas, peso_toneladas, licencia_requerida, tipo_gasolina, tipo_desecho, descripcion, foto_url)
             VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)`,
            [numero_economico, marca, modelo, año, color, placas, peso_toneladas, licencia_requerida, tipo_gasolina, tipo_desecho, descripcion, foto_url]
        );
        
        return this.findById(result.insertId);
    }

    
    static async update(id, data) {
        const { numero_economico, marca, modelo, año, color, placas, peso_toneladas, licencia_requerida, tipo_gasolina, tipo_desecho, descripcion, foto_url } = data;
        
        await db.query(
            `UPDATE vehiculos 
             SET numero_economico = ?, marca = ?, modelo = ?, año = ?, color = ?, placas = ?, peso_toneladas = ?, licencia_requerida = ?, tipo_gasolina = ?, tipo_desecho = ?, descripcion = ?, foto_url = ?
             WHERE id = ?`,
            [numero_economico, marca, modelo, año, color, placas, peso_toneladas, licencia_requerida, tipo_gasolina, tipo_desecho, descripcion, foto_url, id]
        );
        
        return this.findById(id);
    }

    
    static async delete(id) {
        const [result] = await db.query('UPDATE vehiculos SET activo = FALSE WHERE id = ?', [id]);
        return result.affectedRows > 0;
    }
}

module.exports = Vehiculo;