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
    public partial class FormMulta : Form
    {
        private Multa mult;
        List<Vehiculo> vehiculos;

        public Multa Inc { get => mult; set => mult = value; }

        public FormMulta(List<Infraccion> infracciones, List<Vehiculo> vehiculos)
        {
            InitializeComponent();
            this.comboBoxInfraccion.DataSource = infracciones;
            this.vehiculos = vehiculos;
            this.comboBoxInfraccion.SelectedIndex = 0;
        }

        public FormMulta(Multa i, List<Infraccion> infracciones)
        {
            InitializeComponent();
            this.mult = i;
            this.comboBoxInfraccion.DataSource = new[] { mult.Infraccion };
            this.textBoxPatente.Text = mult.Vehiculo.Patente;
            this.dateTimePickerMulta.Value = mult.Fecha;
        }

        private void buttonConf_Click(object sender, EventArgs e)
        {
            DateTime fecha = this.dateTimePickerMulta.Value;
            Infraccion inf = (Infraccion)this.comboBoxInfraccion.SelectedItem;
            string patente = textBoxPatente.Text;

            Vehiculo vehi;

            bool found = vehiculos.Any(v => v.Patente == patente);
            if (!found)
            {
                vehi = new Vehiculo(patente);
                vehiculos.Add(vehi);
            } else
            {
                vehi = vehiculos.First(v => v.Patente == patente);
            }

            mult = new Multa(0, fecha, inf, vehi);
            mult.Id = mult.agregarDb(fecha, inf.Id, patente);

            vehi.agregarIncidente(mult);
            inf.agregarIncidente(mult);

            this.Close();
        }


        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void comboBoxInfraccion_Format(object sender, ListControlConvertEventArgs e)
        {
            string desc = ((Infraccion)e.ListItem).Descripcion;

            e.Value = "Infraccion: " + desc;
        }

        public void prepararCrear()
        {
            this.buttonConf.Visible = true;
            this.buttonConf.Enabled = false;
        }

        public void prepararMostrar()
        {
            this.buttonConf.Visible = false;
            this.buttonCancel.Visible = true;
            this.dateTimePickerMulta.Enabled = false;
            this.comboBoxInfraccion.Enabled = false;
            this.textBoxPatente.Enabled = false;
        }

        private void textBoxPatente_Validated(object sender, EventArgs e)
        {
            errorProvider.SetError(textBoxPatente, String.Empty);
        }

        private void textBoxPatente_Validating(object sender, CancelEventArgs e)
        {
            string patente = textBoxPatente.Text;

            string errorMsg;
            if (!validPatente(patente, out errorMsg))
            {
                e.Cancel = true;
                textBoxPatente.Focus();

                this.errorProvider.SetError(textBoxPatente, errorMsg);
            }
        }

        private bool validPatente(string patente, out string errorMessage)
        {
            // Verifica que la petente sea formato ABC123 o AB123CD
            if (System.Text.RegularExpressions.Regex.IsMatch(patente, "(^[A-Z]{3}[0-9]{3}$)|(^[A-Z]{2}[0-9]{3}[A-Z]{2}$)"))
            {
                errorMessage = "";
                return true;
            }

            errorMessage = "Patente erronea. Debe ser formato ABC123 o AB123CD.";
            return false;
        }



        private void checkInputs()
        {
            string error;
            if (validPatente(textBoxPatente.Text, out error))
            {
                buttonConf.Enabled = true;
            }
            else
            {
                buttonConf.Enabled = false;
            }
        }

        private void textBoxPatente_TextChanged(object sender, EventArgs e)
        {
            checkInputs();
        }

        private void FormMulta_Load(object sender, EventArgs e)
        {

        }
    }
}
