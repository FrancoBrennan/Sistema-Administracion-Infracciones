﻿using Negocio;
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
                List<Incidente> incidentes = direccionTransito.buscarIncidentesPatente(patente);

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

            direccionTransito.descargarPDF(idIncidente);
        }
    }
}