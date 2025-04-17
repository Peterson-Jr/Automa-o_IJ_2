using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ClosedXML.Excel;

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

            CarregarDadosDoExcel();
        }

        private void CarregarDadosDoExcel()
        {
            string caminhoExcel = @"C:\Users\peterson.junior\Desktop\layout_dados.xlsx";

            dadosPorCodigo = new Dictionary<string, DadosDoProduto>();

            if (!File.Exists(caminhoExcel))
            {
                MessageBox.Show("Arquivo de dados não encontrado: " + caminhoExcel);
                return;
            }

            using (var workbook = new XLWorkbook(caminhoExcel))
            {
                var planilha = workbook.Worksheet(1);
                var tabela = planilha.RangeUsed();

                foreach (var linha in tabela.RowsUsed().Skip(1))
                {
                    string codigo = linha.Cell(1).GetValue<string>().Trim();

                    var dados = new DadosDoProduto
                    {
                        Modelo = linha.Cell(3).GetValue<string>(),
                        CaminhoConfiguracao = linha.Cell(4).GetValue<string>(),
                        CaminhoAtualizacao = linha.Cell(5).GetValue<string>()
                    };

                    if (!dadosPorCodigo.ContainsKey(codigo))
                        dadosPorCodigo.Add(codigo, dados);
                }
            }
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
                // Atualiza stconfigBox
                if (File.Exists(dadosSelecionados.CaminhoConfiguracao))
                {
                    stconfigBox.Text = $"Arquivo de configuração carregado com sucesso: {Path.GetFileName(dadosSelecionados.CaminhoConfiguracao)}";
                }
                else
                {
                    stconfigBox.Text = "Arquivo de configuração não encontrado.";
                }

                // Atualiza pack
                if (File.Exists(dadosSelecionados.CaminhoAtualizacao))
                {
                    pack.Text = $"Arquivo de atualização carregado com sucesso: {Path.GetFileName(dadosSelecionados.CaminhoAtualizacao)}";
                }
                else
                {
                    pack.Text = "Arquivo de atualização não encontrado.";
                }

                MessageBox.Show("Gravação concluída!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro na gravação: " + ex.Message);
            }
        }

        // Métodos vazios para evitar erro no Designer
        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }
        private void pictureBox2_Click(object sender, EventArgs e) { }
        private void pictureBox2_Click_1(object sender, EventArgs e) { }
        private void pictureBox3_Click(object sender, EventArgs e) { }
        private void pictureBox4_Click(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void textBox1_TextChanged_1(object sender, EventArgs e) { }
        private void button2_Click(object sender, EventArgs e) { }

        private void pack_TextChanged(object sender, EventArgs e)
        {

        }
    }

    public class DadosDoProduto
    {
        public string Modelo { get; set; }
        public string CaminhoConfiguracao { get; set; }
        public string CaminhoAtualizacao { get; set; }
    }
}