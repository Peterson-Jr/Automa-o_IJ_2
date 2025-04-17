using System;
using System.Threading;
using System.Windows.Forms;

namespace Projeto_IJ
{
    public partial class Form3 : Form
    {
        private Thread nt;

        public Form3()
        {
            InitializeComponent();
        }

        // Evento do botão "Gravar IJ"
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close(); // Fecha o Form3
            nt = new Thread(novoForm5); // Cria uma nova thread que abrirá o Form5
            nt.SetApartmentState(ApartmentState.STA); // Define o tipo de apartamento
            nt.Start(); // Inicia a thread
        }

        private void novoForm5()
        {
            Application.Run(new Form5()); // Executa o Form5 na nova thread
        }

        // Evento de pressionar Enter no txtCodigoBarras
        private void txtCodigoBarras_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                this.Close();
                nt = new Thread(novoForm5);
                nt.SetApartmentState(ApartmentState.STA);
                nt.Start();
            }
        }

        // Métodos vazios para compatibilidade com o Designer
        private void pictureBox2_Click(object sender, EventArgs e) { }
        private void pictureBox3_Click(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void textBox1_TextChanged_1(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
    }
}
