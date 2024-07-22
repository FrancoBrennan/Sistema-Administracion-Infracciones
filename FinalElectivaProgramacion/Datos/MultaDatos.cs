using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;

namespace Datos
{
    public class MultaDatos
    {
        Conexion conexion;

        public MultaDatos()
        {
            this.conexion = new Conexion();
        }

        public int agregar(DateTime fecha, int idInfraccion, string patente)
        {
            string query = "INSERT INTO Multa (Fecha, idInfraccion, Patente) VALUES (@Fecha,@idInfraccion,@Patente)";

            // Crear y configurar el comando SQL
            using (OleDbCommand cmd = new OleDbCommand(query))
            {
                // Agregar parámetros al comando
                cmd.Parameters.AddWithValue("@Fecha", fecha);
                cmd.Parameters.AddWithValue("@idInfraccion", idInfraccion);
                cmd.Parameters.AddWithValue("@Patente", patente);

                return conexion.ejecutarComando(cmd);
            }
        }

        public int eliminar(int id)
        {
            string query = "DELETE from Multa WHERE ID = @Id";

            // Crear y configurar el comando SQL
            using (OleDbCommand cmd = new OleDbCommand(query))
            {
                // Agregar parámetros al comando
                cmd.Parameters.AddWithValue("@Id", id);

                return this.conexion.ejecutarComando(cmd);
            }
        }

        public OleDbDataReader listar()
        {
            return this.conexion.ejecutarSelect(new OleDbCommand("SELECT * FROM Multa"));
        }
    }
}
