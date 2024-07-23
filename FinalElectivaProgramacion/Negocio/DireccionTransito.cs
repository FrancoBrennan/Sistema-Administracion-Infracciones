using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Datos;
using PdfSharp.Drawing;
using PdfSharp.Drawing.BarCodes;
using PdfSharp.Pdf;
using BarcodeStandard;
using System.Drawing.Imaging;
using System.Net.NetworkInformation;
using ZXing;

namespace Negocio
{
    public class DireccionTransito
    {
        private List<Infraccion> infracciones;
        private List<Multa> incidentes;
        private List<Pago> pagos;
        private List<Vehiculo> vehiculos;

        InfraccionDatos infDb;
        MultaDatos multDb;
        PagoDatos pagoDB;

        public List<Infraccion> Infracciones { get => infracciones; set => infracciones = value; }
        public List<Multa> Incidentes { get => incidentes; set => incidentes = value; }
        public List<Pago> Pagos { get => pagos; set => pagos = value; }
        public List<Vehiculo> Vehiculos { get => vehiculos; set => vehiculos = value; }

        public DireccionTransito()
        {
            infDb = new InfraccionDatos();
            multDb = new MultaDatos();
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
            incidentes = new List<Multa>();
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
            OleDbDataReader readers = multDb.listar();

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
                Multa inc = new Multa((int)readers["Id"], DateTime.Parse((string)readers["Fecha"]), infraccion, vehiculo);

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
                Multa inc = incidentes.Find(i => i.Id == ((int)readers["IdIncidente"]));

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
                pago.Multa.Vehiculo.agregarPago(pago);
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

        public void agregarIncidente(Multa inc)
        {
            this.Incidentes.Add(inc);
        }

        public void removerIncidente(Multa inc)
        {
            this.Incidentes.Remove(inc);
        }

        public void agregarVehiculo(Vehiculo v)
        {
            this.vehiculos.Add(v);
        }

        public void agregarPago(Multa inc, double monto)
        {
            Pago pago = new Pago(0, DateTime.Now, inc, monto);
            pago.Id = pago.agregarDb(pago.Fecha, pago.Multa.Id, pago.Monto);
            pago.Multa.Vehiculo.agregarPago(pago);
            pagos.Add(pago);
        }

        public bool tienePagoVinculado(Infraccion inf)
        {
            var incidentes = inf.Multas.Select(i => i.Id);
            return this.pagos.Any(p => incidentes.Contains(p.Multa.Id));
        }

        public bool tienePagoVinculado(Multa inc)
        {
            return this.pagos.Any(p => p.Multa.Id == inc.Id);
        }

        public List<Multa> buscarIncidentesPatente(string patente)
        {
            return Incidentes.FindAll(inc => inc.Vehiculo.Patente.ToLower() == patente.ToLower());
        }

        public PdfDocument descargarPDF(int idIncidente)
        {
            Multa incidente = incidentes.Find(i => i.Id == idIncidente);
            double monto = incidente.Infraccion.calcularImporte(DateTime.Now);
            DateTime vencimiento = incidente.Fecha.AddDays(30);

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            PdfDocument doc = new PdfDocument();
            PdfPage page = doc.AddPage();

            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Fuentes y colores
            XFont titleFont = new XFont("Arial", 24, XFontStyle.Bold);
            XFont textFont = new XFont("Arial", 16);
            XBrush titleBrush = XBrushes.Black;
            XBrush textBrush = XBrushes.DarkBlue;
            XPen linePen = new XPen(XColors.Black, 1);

            double yPoint = 60; // Punto Y inicial

            // Dibujar título
            gfx.DrawString("ORDEN DE PAGO INFRACCIÓN DE TRÁNSITO", titleFont, titleBrush,
                           new XRect(0, yPoint, page.Width, page.Height), XStringFormats.TopCenter);

            yPoint += 60; // Incremento de Y para la línea separadora

            // Dibujar separador
            gfx.DrawLine(linePen, 40, yPoint, page.Width - 40, yPoint);

            yPoint += 200; // Incremento de Y

            // Dibujar orden de pago
            gfx.DrawString("Orden de pago: #" + idIncidente, textFont, textBrush,
                           new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft);

            yPoint += 40; // Incremento de Y

            // Dibujar monto a pagar
            gfx.DrawString("Monto a pagar: U$D " + monto, textFont, textBrush,
                           new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft);

            yPoint += 40; // Incremento de Y

            // Dibujar monto a pagar
            gfx.DrawString("Vencimiento: " + vencimiento.ToString(), textFont, textBrush,
                           new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft);

            yPoint += 200; // Incremento de Y para el código de barras

            // Dibujar separador
            gfx.DrawLine(linePen, 40, yPoint, page.Width - 40, yPoint);

            yPoint += 60; // Incremento de Y para la línea separadora

            // Generar el código de barras
            var barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = 300,
                    Height = 100
                }
            };
            var barcodeBitmap = barcodeWriter.Write(idIncidente.ToString());

            // Convertir el Bitmap a un MemoryStream
            using (var barcodeStream = new MemoryStream())
            {
                barcodeBitmap.Save(barcodeStream, ImageFormat.Png);
                barcodeStream.Seek(0, SeekOrigin.Begin);

                // Crear una imagen XImage desde el MemoryStream
                XImage barcodeImage = XImage.FromStream(barcodeStream);

                // Dibujar el código de barras en el PDF
                gfx.DrawImage(barcodeImage, (page.Width - 300) / 2, yPoint, 300, 100);
            }

            return doc;
        }
    }

}

