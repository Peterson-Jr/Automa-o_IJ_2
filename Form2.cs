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
    public partial class Form2 : Form
    {
        private Thread nt;

        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close(); // Fecha o Form2
            nt = new Thread(novoForm4); // Cria uma nova thread que abrirá o Form4
            nt.SetApartmentState(ApartmentState.STA); // Define o tipo de "apartamento" da thread
            nt.Start(); // Inicia a thread
        }

        private void novoForm4()
        {
            Application.Run(new Form4()); // Executa o Form4 na nova thread
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
    }
}