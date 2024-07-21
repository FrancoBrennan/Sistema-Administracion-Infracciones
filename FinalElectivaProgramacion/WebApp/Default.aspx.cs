using Negocio;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            {
                if (!IsPostBack)
                {
                    DireccionTransito direccionTransito = new DireccionTransito();
                    Session["DT"] = direccionTransito;
                }
            }
        }

        // Boton de buscar
        protected void Button_onClick(object sender, EventArgs e)
        {
            DireccionTransito direccionTransito = (DireccionTransito)Session["DT"];
            string patente = TextBox.Text;

            if (string.IsNullOrEmpty(patente) == false)
            {
                List<Multa> incidentes = direccionTransito.buscarIncidentesPatente(patente);

                incidentes.ForEach(inc => inc.Pagada = direccionTransito.Pagos.Exists(p => p.Incidente.Id == inc.Id));

                GridView.DataSource = incidentes;
                GridView.DataBind();
            } else
            {
                GridView.DataSource = null;
                GridView.DataBind();
            }
        }

        // Descargar PDF
        protected void DownloadButton_Click(object sender, EventArgs e)
        {
            DireccionTransito direccionTransito = (DireccionTransito)Session["DT"];
            Button btn = (Button)sender;
            int idIncidente = int.Parse(btn.CommandArgument);

            PdfDocument doc = direccionTransito.descargarPDF(idIncidente);

            // Guardar el documento en un MemoryStream
            using (var stream = new System.IO.MemoryStream())
            {
                doc.Save(stream, false);

                // Enviar el PDF al navegador para su descarga
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment; filename=OrdenDe_Pago_Multa" + idIncidente + ".pdf");
                Response.OutputStream.Write(stream.ToArray(), 0, stream.ToArray().Length);
                Response.Flush();
                Response.End();
            }
        }
    }
}