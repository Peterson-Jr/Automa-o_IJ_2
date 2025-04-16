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
    public partial class Form1 : Form
    {
        private Thread nt; // nt significa new thread

        public Form1()
        {
            InitializeComponent();

            // Associa o evento KeyDown para fazer funcionar a tecla enter nos campos de login e senha
            box_login.KeyDown += new KeyEventHandler(box_login_KeyDown);
            box_senha.KeyDown += new KeyEventHandler(box_senha_KeyDown);
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
        }

        private void button3_Click(object sender, EventArgs e)
        {
            RealizarLogin();
        }

        // Aqui eu criei um atalho que, para que quando eu apertar enter (no teclado) no campo de login vai ir para a senha
        private void box_login_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                box_senha.Focus();
            }
        }

        // Aqui eu criei um atalho que, para que quando eu apertar enter (no teclado) no campo de senha vai realizar o login
        private void box_senha_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                RealizarLogin();
            }
        }

        private void RealizarLogin()
        {
            string usuario = box_login.Text;
            string senha = box_senha.Text;

            if (usuario == "admin" && senha == "Carel.123")
            {
                this.Close();
                nt = new Thread(novoFormAdm);
            }
            else if (usuario == "user" && senha == "12345678")
            {
                this.Close();
                nt = new Thread(novoFormUser);
            }
            else
            {
                MessageBox.Show("Usuário ou senha incorretos!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            nt.SetApartmentState(ApartmentState.STA);
            nt.Start();
        }

        // Método para abrir o Form2 (Acesso ADM)
        private void novoFormAdm()
        {
            Application.Run(new Form2());
        }

        // Método para abrir o Form3 (Acesso User)
        private void novoFormUser()
        {
            Application.Run(new Form3());
        }
    }
}