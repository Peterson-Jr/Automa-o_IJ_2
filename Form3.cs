using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Projeto_IJ
{
    public partial class Form3 : Form
    {
        private Thread nt;

        public Form3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close(); // Fecha o Form3
            nt = new Thread(novoForm5); // Cria uma nova thread para abrir o Form5
            nt.SetApartmentState(ApartmentState.STA); // Define o estado de apartamento
            nt.Start(); // Inicia a thread
        }

        private void novoForm5()
        {
            Application.Run(new Form5()); // Executa o Form5 na nova thread
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }
    }
}
