using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Negocio;

namespace UI
{
    public partial class FormPrincipal : Form
    {
        private DireccionTransito dt;
        private ToolTip tt = new ToolTip();

        public FormPrincipal()
        {
            InitializeComponent();
            dt = new DireccionTransito();
            listBoxInfraccion.DataSource = dt.Infracciones;
            listBoxInfraccion.ClearSelected();
            refreshListBoxIncidentes();
            dataGridViewPagos.DataSource = dt.Pagos.Select(p => new { ID = p.Id, Fecha = p.Fecha.ToShortDateString(), Multa = p.Multa.Infraccion.Descripcion, Monto = "$" + p.Monto }).ToList();
        }

        private void FormPrincipal_Load(object sender, EventArgs e)
        {
            int defaultDescGrave = 20;
            string desc = Microsoft.VisualBasic.Interaction.InputBox("Ingrese un valor para el porcentaje de descuento para las infracciones graves que se paguen 25 dias antes de su vencimiento.", "Infraccion Grave", defaultDescGrave.ToString());
            if (desc.Length != 0)
            {
                defaultDescGrave = int.Parse(desc);
            }
            InfraccionGrave.SetDescuento25Dias(defaultDescGrave);

            int defaultDescLeve = 25;
            desc = Microsoft.VisualBasic.Interaction.InputBox("Ingrese un valor para el porcentaje de descuento para las infracciones leves que se paguen 20 dias antes de su vencimiento", "Infraccion Leve", defaultDescLeve.ToString());
            if (desc.Length != 0)
            {
                defaultDescLeve = int.Parse(desc);
            }
            InfraccionLeve.SetDescuento20Dias(defaultDescLeve);

            int defaultDescLeve2 = 15;
            desc = Microsoft.VisualBasic.Interaction.InputBox("Ingrese un valor para el porcentaje de descuento para las infracciones leves que se paguen 10 dias antes de su vencimiento", "Infraccion Leve", defaultDescLeve2.ToString());
            if (desc.Length != 0)
            {
                defaultDescLeve2 = int.Parse(desc);
            }
            InfraccionLeve.SetDescuento10Dias(defaultDescLeve2);
        }

        // Crear infraccion
        private void buttonCrearInfraccion_Click(object sender, EventArgs e)
        {
            FormInfraccion fi = new FormInfraccion();
            fi.prepararCrear();
            fi.ShowDialog();

            Infraccion inf = fi.Inf;
            if (inf != null)
            {
                dt.agregarInfraccion(inf);
                MessageBox.Show("Infraccion creada con exito.");
                listBoxInfraccion.DataSource = null;
                listBoxInfraccion.DataSource = dt.Infracciones;
                listBoxInfraccion.ClearSelected();
            }
        }

        // Modificar infraccion
        private void buttonModifInfraccion_Click(object sender, EventArgs e)
        {
            Infraccion a = (Infraccion)listBoxInfraccion.SelectedItem;
            if (a == null)
                MessageBox.Show("No hay infraccion seleccionada para modificar.");
            else
            {
                FormInfraccion fi = new FormInfraccion(a);
                fi.prepararModificar();
                fi.ShowDialog();

                listBoxInfraccion.DataSource = null;
                listBoxInfraccion.DataSource = dt.Infracciones;
                listBoxInfraccion.ClearSelected();
            }
        }

        // Eliminar infraccion
        private void buttonElimInfraccion_Click(object sender, EventArgs e)
        {
            Infraccion inf = (Infraccion)listBoxInfraccion.SelectedItem;
            if (inf == null)
                MessageBox.Show("No hay infraccion seleccionada para eliminar.");
            else
            {
                DialogResult dialogResult = MessageBox.Show("Esta seguro que desea eliminar la infraccion seleccionada?", "Eliminar infraccion", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    inf.eliminarInfraccionDb();
                    dt.removerInfraccion(inf);

                    MessageBox.Show("Infraccion eliminada con exito.");
                }

                listBoxInfraccion.DataSource = null;
                listBoxInfraccion.DataSource = dt.Infracciones;
                listBoxInfraccion.ClearSelected();
                refreshListBoxIncidentes();
            }
        }

        // Mostrar infraccion
        private void buttonMostrarInfraccion_Click(object sender, EventArgs e)
        {
            Infraccion a = (Infraccion)listBoxInfraccion.SelectedItem;
            if (a == null)
                MessageBox.Show("No hay infraccion seleccionada para mostrar.");
            else
            {
                FormInfraccion fi = new FormInfraccion(a);
                fi.prepararMostrar();
                fi.ShowDialog();
            }
        }

        // Crear Multa
        private void buttonCrearIncidente_Click(object sender, EventArgs e)
        {
            if (dt.Infracciones.Count == 0)
            {
                MessageBox.Show("No hay infracciones creadas. Primero debe crear por lo menos una infraccion.");
            }
            else
            {
                FormMulta fm = new FormMulta(dt.Infracciones, dt.Vehiculos);
                fm.prepararCrear();
                fm.ShowDialog();

                Multa inc = fm.Inc;
                if (inc != null)
                {
                    dt.agregarIncidente(inc);
                    MessageBox.Show("Incidente creado con exito.");
                    refreshListBoxIncidentes();
                }
            }
        }

        // Eliminar Multa
        private void buttonElimIncidente_Click(object sender, EventArgs e)
        {
            Multa inc = (Multa)listBoxMulta.SelectedItem;
            if (inc == null)
                MessageBox.Show("No hay incidente seleccionado para eliminar.");
            else
            {
                DialogResult dialogResult = MessageBox.Show("Esta seguro que desea eliminar el incidente seleccionado?", "Eliminar incidente", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    inc.eliminarDb();
                    inc.eliminar();
                    dt.removerIncidente(inc);

                    MessageBox.Show("Incidente eliminado con exito.");
                }

                refreshListBoxIncidentes();
            }
        }

        // Mostrar Multa
        private void buttonMostrarIncidente_Click(object sender, EventArgs e)
        {
            Multa i = (Multa)listBoxMulta.SelectedItem;
            if (i == null)
                MessageBox.Show("No hay incidente seleccionado para mostrar.");
            else
            {
                FormMulta fi = new FormMulta(i, dt.Infracciones);
                fi.prepararMostrar();
                fi.ShowDialog();
            }
        }

        // Pagar Multa
        private void buttonPagoIncidente_Click(object sender, EventArgs e)
        {
            Multa inc = (Multa)listBoxMulta.SelectedItem;
            
            if (inc == null)
                MessageBox.Show("No hay incidente seleccionado para pagar.");
            else
            {
                double monto = inc.Infraccion.calcularImporte(inc.Fecha);
                DialogResult dialogResult = MessageBox.Show("Esta seguro que desea realizar el pago correspondiente a $" + monto + " para el incidente seleccionado ?", "Realizar Pago", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    dt.agregarPago(inc, monto);
                    MessageBox.Show("Pago realizado con exito.");
                }

                refreshListBoxIncidentes();
                dataGridViewPagos.DataSource = null;
                dataGridViewPagos.DataSource = dt.Pagos.Select(p => new { ID = p.Id, Fecha = p.Fecha.ToShortDateString(), Multa = p.Multa.Infraccion.Descripcion, Monto = "$" + p.Monto }).ToList();
            }
        }

        // Buscar Multa por patente
        private void textBoxBuscarInc_TextChanged(object sender, EventArgs e)
        {
            listBoxMulta.DataSource = null;
            if (string.IsNullOrEmpty(textBoxBuscarInc.Text) == false)
            {
                listBoxMulta.Items.Clear();
                List<Multa> incedentesNoPagos = dt.Incidentes.Where(i => !dt.tienePagoVinculado(i)).ToList();
                foreach (Multa a in incedentesNoPagos)
                {
                    if (a.Vehiculo.Patente.ToLower().StartsWith(textBoxBuscarInc.Text.ToLower()))
                    {
                        listBoxMulta.Items.Add(a);
                    }
                }
            }
            else
            {
                refreshListBoxIncidentes();
            }
        }

        // Habilitar boton de eliminar infraccion
        private void listBoxInfraccion_SelectedIndexChanged(object sender, EventArgs e)
        {
            Infraccion inf = (Infraccion)listBoxInfraccion.SelectedItem;
            if (inf != null)
            {
                if (dt.tienePagoVinculado(inf))
                {
                    buttonElimInfraccion.Enabled = false;
                }
                else
                {
                    buttonElimInfraccion.Enabled = true;
                }
            }
        }

        // Deshabilitar boton de eliminar infraccion
        private void panel1_MouseHover(object sender, EventArgs e)
        {
            tt.SetToolTip(this.panel1, "La infraccion tiene pagos asociados, no se puede eliminar.");
            if (buttonElimInfraccion.Enabled == false && listBoxInfraccion.SelectedItem != null)
            {
                tt.Active = true;
            }
            else
            {
                tt.Active = false;
            }
        }

        // Habilitar boton de pago de infraccion si no esta vencido
        private void listBoxIncidente_SelectedIndexChanged(object sender, EventArgs e)
        {
            Multa inc = (Multa)listBoxMulta.SelectedItem;
            if (inc != null)
            {
                if (!inc.verificarVencimiento())
                {
                    buttonPagoIncidente.Enabled = false;
                }
                else
                {
                    buttonPagoIncidente.Enabled = true;
                }
            }
        }

        // Si el pago esta vencido mostrar error
        private void panel2_MouseHover(object sender, EventArgs e)
        {
            tt.SetToolTip(this.panel2, "Ya pasaron mas de 30 dias, el pago esta vencido.");
            if (buttonPagoIncidente.Enabled == false && listBoxMulta.SelectedItem != null)
            {
                tt.Active = true;
            }
            else
            {
                tt.Active = false;
            }
        }

        // Mostrar Multas sin pagos vinculados
        private void refreshListBoxIncidentes()
        {
            listBoxMulta.DataSource = null;
            listBoxMulta.DataSource = dt.Incidentes.Where(i => !dt.tienePagoVinculado(i)).ToList();
            listBoxMulta.ClearSelected();
        }

        // Metodos para dar formato a los listbox

        private void listBoxInfraccion_Format(object sender, ListControlConvertEventArgs e)
        {
            string desc = ((Infraccion)e.ListItem).Descripcion;
            string monto = ((Infraccion)e.ListItem).Importe.ToString();

            e.Value = "Infraccion: " + desc + " | Monto: $" + monto;
        }

        private void listBoxIncidente_Format(object sender, ListControlConvertEventArgs e)
        {
            string desc = ((Multa)e.ListItem).Infraccion.Descripcion;
            string fecha = ((Multa)e.ListItem).Fecha.ToString("dd/MM/yy");
            string patente = ((Multa)e.ListItem).Vehiculo.Patente;

            e.Value = fecha + " | Patente: " + patente + " | " + desc;
        }
    }
}
