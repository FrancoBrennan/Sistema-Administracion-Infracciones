using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datos;

namespace Negocio
{
    public class Vehiculo
    {
        private string patente;
        List<Multa> incidentes;
        List<Pago> pagos;

        public Vehiculo(string patente)
        {
            this.patente = patente;
            this.incidentes = new List<Multa>();
            this.pagos = new List<Pago>();
        }

        public string Patente { get => patente; set => patente = value; }
        public List<Multa> Incidentes { get => incidentes; set => incidentes = value; }
        public List<Pago> Pagos { get => pagos; }

        public void agregarIncidente(Multa inc)
        {
            this.incidentes.Add(inc);
        }

        public void agregarPago(Pago pago)
        {
            this.pagos.Add(pago);
        }

        public void removerIncidente(Multa inc)
        {
            this.incidentes.Remove(inc);
        }
    }
}
