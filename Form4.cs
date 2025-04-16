using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Projeto_IJ
{
    public partial class Form4 : Form
    {
        private Dictionary<string, DadosDoProduto> dadosPorCodigo;
        private DadosDoProduto dadosSelecionados = null;

        public Form4()
        {
            InitializeComponent();

            txtCodigoBarras.KeyDown += new KeyEventHandler(txtCodigoBarras_KeyDown);
            txtData.ReadOnly = true;
            partnumberBox.ReadOnly = true;

            CarregarDadosManual();
        }

        private void CarregarDadosManual()
        {
            // Preencha os dados manualmente por enquanto
            dadosPorCodigo = new Dictionary<string, DadosDoProduto>
            {
                {
                    "ABC123", new DadosDoProduto
                    {
                        Modelo = "Linha A",
                        CaminhoConfiguracao = @"C:\configs\a.stconfig",
                        CaminhoAtualizacao = @"C:\firmware\fwA.pack"
                    }
                },
                {
                    "XYZ789", new DadosDoProduto
                    {
                        Modelo = "Linha B",
                        CaminhoConfiguracao = @"C:\configs\b.stconfig",
                        CaminhoAtualizacao = @"C:\firmware\fwB.pack"
                    }
                }
            };
        }

        private void txtCodigoBarras_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                string codigo = txtCodigoBarras.Text.Trim();

                if (dadosPorCodigo.TryGetValue(codigo, out var dados))
                {
                    partnumberBox.Text = dados.Modelo;
                    txtData.Text = DateTime.Now.ToString("dd/MM/yyyy");
                    dadosSelecionados = dados;
                }
                else
                {
                    MessageBox.Show("Código de barras não encontrado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    partnumberBox.Text = "";
                    txtData.Text = "";
                    dadosSelecionados = null;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dadosSelecionados == null)
            {
                MessageBox.Show("Nenhum código de barras válido foi lido.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                if (File.Exists(dadosSelecionados.CaminhoConfiguracao))
                    MessageBox.Show("Rodando configuração: " + Path.GetFileName(dadosSelecionados.CaminhoConfiguracao));

                if (File.Exists(dadosSelecionados.CaminhoAtualizacao))
                    MessageBox.Show("Rodando atualização: " + Path.GetFileName(dadosSelecionados.CaminhoAtualizacao));

                MessageBox.Show("Gravação concluída!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro na gravação: " + ex.Message);
            }
        }

        // Resto dos métodos intactos
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void pictureBox3_Click(object sender, EventArgs e) { }
        private void pictureBox2_Click(object sender, EventArgs e) { }
        private void textBox1_TextChanged_1(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void pictureBox4_Click(object sender, EventArgs e) { }
        private void button2_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void pictureBox2_Click_1(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }
    }

    public class DadosDoProduto
    {
        public string Modelo { get; set; }
        public string CaminhoConfiguracao { get; set; }
        public string CaminhoAtualizacao { get; set; }
    }
}
