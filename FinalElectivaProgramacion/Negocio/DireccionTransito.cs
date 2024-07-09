using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using Datos;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace Negocio
{
    public class DireccionTransito
    {
        private List<Infraccion> infracciones;
        private List<Incidente> incidentes;
        private List<Pago> pagos;
        private List<Vehiculo> vehiculos;

        InfraccionDatos infDb;
        IncidenteDatos incDb;
        PagoDatos pagoDB;

        public List<Infraccion> Infracciones { get => infracciones; set => infracciones = value; }
        public List<Incidente> Incidentes { get => incidentes; set => incidentes = value; }
        public List<Pago> Pagos { get => pagos; set => pagos = value; }
        public List<Vehiculo> Vehiculos { get => vehiculos; set => vehiculos = value; }

        public DireccionTransito()
        {
            infDb = new InfraccionDatos();
            incDb = new IncidenteDatos();
            pagoDB = new PagoDatos();

            inicializarListas();
            cargarDatos();
            vincular();
        }

        private void cargarDatos()
        {
            cargarInfracciones();
            cargarIncidentes();
            cargarPagos();
        }
        private void inicializarListas()
        {
            vehiculos = new List<Vehiculo>();
            incidentes = new List<Incidente>();
            infracciones = new List<Infraccion>();
            pagos = new List<Pago>();
        }

        private void cargarInfracciones()
        {
            OleDbDataReader readers = infDb.listar();

            while (readers.Read())
            {
                Infraccion infraccion;
                string tipo = (string)readers["Tipo"];

                if (tipo == "LEVE")
                {
                    infraccion = new InfraccionLeve((int)readers["Id"], (string)readers["Descripcion"], (double)((long)readers["Importe"]), tipo);
                }
                else
                {
                    infraccion = new InfraccionGrave((int)readers["Id"], (string)readers["Descripcion"], (double)((long)readers["Importe"]), tipo);
                }

                infracciones.Add(infraccion);
            }

            readers.Close();
        }
        
        private void cargarIncidentes()
        {
            OleDbDataReader readers = incDb.listar();

            while (readers.Read())
            {
                string patente = (string)readers["Patente"];
                Vehiculo vehiculo = vehiculos.Find(v => v.Patente == patente);

                // Agregar vehiculo a la lista si no está
                if (vehiculo == null)
                {
                    vehiculo = new Vehiculo(patente);
                    vehiculos.Add(vehiculo);
                }

                // Busca la infraccion en la lista
                Infraccion infraccion = infracciones.First(i => i.Id == (int)readers["IdInfraccion"]);

                // Agrega el incidente
                Incidente inc = new Incidente((int)readers["Id"], DateTime.Parse((string)readers["Fecha"]), infraccion, vehiculo);

                incidentes.Add(inc);
            }

            readers.Close();
        }
        
        private void cargarPagos()
        {
            OleDbDataReader readers = pagoDB.listar();

            while (readers.Read())
            {
                // Buscar el incidente que se pagó
                Incidente inc = incidentes.Find(i => i.Id == ((int)readers["IdIncidente"]));

                Pago pago = new Pago((int)readers["Id"], DateTime.Parse((string)readers["Fecha"]), inc, (double)((long)readers["Monto"]));

                pagos.Add(pago);
            }

            readers.Close();
        }


        private void vincular()
        {
            // Vinculamos los incidentes a sus infracciones y vehiculos.
            foreach (var inc in incidentes)
            {
                inc.Infraccion.agregarIncidente(inc);
                inc.Vehiculo.agregarIncidente(inc);
            }

            // Vinculamos los pagos a sus vehiculos.
            foreach (var pago in pagos)
            {
                pago.Incidente.Vehiculo.agregarPago(pago);
            }
        }

        public void agregarInfraccion(Infraccion inf)
        {
            this.infracciones.Add(inf);
        }

        public void removerInfraccion(Infraccion inf)
        {
            this.infracciones.Remove(inf);

            // Removemos los incidentes vinculados a esa infraccion.
            foreach (var a in incidentes.Where(i => i.Infraccion == inf).ToArray())
            {
                incidentes.Remove(a);
                a.eliminar();
            }
        }

        public void agregarIncidente(Incidente inc)
        {
            this.Incidentes.Add(inc);
        }

        public void removerIncidente(Incidente inc)
        {
            this.Incidentes.Remove(inc);
        }

        public void agregarVehiculo(Vehiculo v)
        {
            this.vehiculos.Add(v);
        }

        public void agregarPago(Incidente inc, double monto)
        {
            Pago pago = new Pago(0, DateTime.Now, inc, monto);
            pago.Id = pago.agregarDb(pago.Fecha, pago.Incidente.Id, pago.Monto);
            pago.Incidente.Vehiculo.agregarPago(pago);
            pagos.Add(pago);
        }

        public bool tienePagoVinculado(Infraccion inf)
        {
            var incidentes = inf.Incidentes.Select(i => i.Id);
            return this.pagos.Any(p => incidentes.Contains(p.Incidente.Id));
        }

        public bool tienePagoVinculado(Incidente inc)
        {
            return this.pagos.Any(p => p.Incidente.Id == inc.Id);
        }

        public List<Incidente> buscarIncidentesPatente(string patente)
        {
            return Incidentes.FindAll(inc => inc.Vehiculo.Patente.ToLower() == patente.ToLower());
        }

        public void descargarPDF(int idIncidente)
        {
            Incidente incidente = incidentes.Find(i => i.Id == idIncidente);
            double monto = incidente.Infraccion.calcularImporte(DateTime.Now);

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            PdfDocument doc = new PdfDocument();
            PdfPage page = doc.AddPage();

            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Arial", 20);

            gfx.DrawString("ORDEN DE PAGO INFRACCIÓN DE TRÁNSITO", font, XBrushes.Black, new XRect(0, 0, page.Width, page.Height), XStringFormats.Center);
            gfx.DrawString("Orden de pago: " + idIncidente, font, XBrushes.Black, new XRect(0, 0, page.Width, page.Height), XStringFormats.Center);
            gfx.DrawString("Monto a pagar: " + monto, font, XBrushes.Black, new XRect(0, 0, page.Width, page.Height), XStringFormats.Center);

            doc.Save("OrdenPago-" + idIncidente);
        }
    }
}
