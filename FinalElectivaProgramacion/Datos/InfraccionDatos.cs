using System.Data.OleDb;

namespace Datos
{
    public class InfraccionDatos
    {
        Conexion conexion;

        public InfraccionDatos()
        {
            this.conexion = new Conexion();
        }

        public int agregar(string desc, double importe, string tipo)
        {
            string query = "INSERT INTO Infraccion (Descripcion, Importe, Tipo) VALUES (@Descripcion,@Importe,@Tipo)";

            // Crear y configurar el comando SQL
            using (OleDbCommand cmd = new OleDbCommand(query))
            {
                // Agregar parámetros al comando
                cmd.Parameters.AddWithValue("@Descripcion", desc);
                cmd.Parameters.AddWithValue("@Importe", importe);
                cmd.Parameters.AddWithValue("@Tipo", tipo.ToUpper());

                return this.conexion.ejecutarComando(cmd);
            }
        }

        public int eliminar(int id)
        {
            string query = "DELETE from Infraccion WHERE ID = @Id";

            // Crear y configurar el comando SQL
            using (OleDbCommand cmd = new OleDbCommand(query))
            {
                // Agregar parámetros al comando
                cmd.Parameters.AddWithValue("@Id", id);

                return this.conexion.ejecutarComando(cmd);
            }
        }

        public int modificar(int id, string desc, double importe)
        {
            string query = "UPDATE Infraccion SET Descripcion = @Descripcion, Importe = @Importe WHERE ID = @Id";

            // Crear y configurar el comando SQL
            using (OleDbCommand cmd = new OleDbCommand(query))
            {
                // Agregar parámetros al comando
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Descripcion", desc);
                cmd.Parameters.AddWithValue("@Importe", importe);

                return this.conexion.ejecutarComando(cmd);
            }
        }

        public OleDbDataReader listar()
        {
            return this.conexion.ejecutarSelect(new OleDbCommand("SELECT * FROM Infraccion"));
        }
    }
}
