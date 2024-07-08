using System;
using System.Data.OleDb;

namespace Datos
{
    public class PagoDatos
    {
        Conexion conexion;

        public PagoDatos()
        {
            this.conexion = new Conexion();
        }

        public int agregar(DateTime fecha, int idIncidente, double monto)
        {
            string query = "INSERT INTO Pago (Fecha, IdIncidente, Monto) VALUES (@Fecha,@idIncidente,@Monto)";

            // Crear y configurar el comando SQL
            using (OleDbCommand cmd = new OleDbCommand(query))
            {
                // Agregar parámetros al comando
                cmd.Parameters.AddWithValue("@Fecha", fecha);
                cmd.Parameters.AddWithValue("@idIncidente", idIncidente);
                cmd.Parameters.AddWithValue("@Monto", monto);

                return this.conexion.ejecutarComando(cmd);
            }
        }

        public OleDbDataReader listar()
        {
            return this.conexion.ejecutarSelect(new OleDbCommand("SELECT * FROM Pago"));
        }
    }
}
