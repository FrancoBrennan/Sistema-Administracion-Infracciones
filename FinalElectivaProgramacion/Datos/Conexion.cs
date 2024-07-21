using System;
using System.Data.OleDb;

namespace Datos
{
    public class Conexion
    {
        private OleDbConnection conexion;

        public OleDbConnection establecerConexion()
        {
            // https://download.cnet.com/microsoft-access-database-engine-2010-redistributable-64-bit/3001-10254_4-75452796.html?dt=internalDownload
            // string connectionString = "Data Source=C:\\Proyecto Club Deportivo\\TPClubDeportivo\\CapaDeDatos\\BD.db";
            // Cadena de conexión a la base de datos Access
            string connectionString = "Provider=Microsoft.ACE.OLEDB.16.0;Data Source=Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\Proyecto Infracciones\\FinalElectivaProgram\\FinalElectivaProgramacion\\Datos\\DB.accdb";

            this.conexion = new OleDbConnection(connectionString);
            return conexion;
        }

        public int ejecutarComando(OleDbCommand comando) // Insert - Delete - Update
        {
            try
            {
                comando.Connection = establecerConexion();

                this.conexion.Open();

                comando.ExecuteNonQuery();

                string identityQuery = "SELECT @@IDENTITY";

                int idElemento;

                // Obtener el ID del elemento recién creado
                using (OleDbCommand identityCommand = new OleDbCommand(identityQuery, this.conexion))
                {
                    idElemento = (int)identityCommand.ExecuteScalar();
                }

                this.conexion.Close();

                return idElemento;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        public OleDbDataReader ejecutarSelect(OleDbCommand comando)
        {
            try
            {
                comando.Connection = establecerConexion();

                this.conexion.Open();

                return comando.ExecuteReader();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
