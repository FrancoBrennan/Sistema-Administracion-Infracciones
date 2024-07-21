using System;
using Datos;

namespace Negocio
{
    public class Multa
    {
        private int id;
        private DateTime fecha;
        private Infraccion infraccion;
        private Vehiculo vehiculo;
        private MultaDatos multDb;
        private bool pagada; // solo usada en la ui

        public int Id { get => id; set => id = value; }
        public DateTime Fecha { get => fecha; set => fecha = value; }
        public Infraccion Infraccion { get => infraccion; set => infraccion = value; }
        public Vehiculo Vehiculo { get => vehiculo; set => vehiculo = value; }
        public bool Pagada { get => pagada; set => pagada = value; }

        public Multa(int id, DateTime fecha, Infraccion infraccion, Vehiculo vehiculo)
        {
            this.multDb = new MultaDatos();

            this.id = id;
            this.fecha = fecha;
            this.infraccion = infraccion;
            this.vehiculo = vehiculo;
        }


        public void eliminar()
        {
            vehiculo.removerIncidente(this);
            infraccion.removerIncidente(this);
        }

        public void eliminarDb()
        {
            multDb.eliminar(this.id);
        }

        public int agregarDb(DateTime fecha, int idInfraccion, string patente)
        {
            return multDb.agregar(fecha, idInfraccion, patente);
        }

        // verificarVencimiento retorna false si ya esta vencido y true si aún no.
        public bool verificarVencimiento()
        {
            DateTime vencimiento = this.fecha.AddDays(30);
            TimeSpan ts = vencimiento.Subtract(DateTime.Now);
            int dias = Convert.ToInt32(ts.TotalDays);

            return dias >= 0 && dias <= 30; // Vence a los 30 dias el pago
        }

    }
}
