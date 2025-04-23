using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace Projeto_IJ
{
    public partial class Form5 : Form
    {
        private Dictionary<string, DadosDoProduto> dadosPorCodigo;
        private DadosDoProduto dadosSelecionados = null;

        public Form5()
        {
            InitializeComponent();
            this.Load += new System.EventHandler(this.Form5_Load); // Correção: associa corretamente o evento de Load

            txtCodigoBarras.KeyDown += new KeyEventHandler(txtCodigoBarras_KeyDown);
            txtData.ReadOnly = true;
            partnumberBox.ReadOnly = true;
        }

        // Método chamado ao carregar o formulário
        private void Form5_Load(object sender, EventArgs e)
        {
            PreencherPortasCom();
            CarregarDadosDoExcel();
        }

        private void CarregarDadosDoExcel()
        {
            string caminhoExcel = @"C:\Users\peter\OneDrive\Área de Trabalho\layout_dados.xlsx";

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

        private void PreencherPortasCom()
        {
            string[] portasCom = SerialPort.GetPortNames();
            comboBoxPortasCom.Items.Clear();

            foreach (var porta in portasCom)
            {
                comboBoxPortasCom.Items.Add(porta);
            }

            if (comboBoxPortasCom.Items.Count > 0)
            {
                comboBoxPortasCom.SelectedIndex = 0;
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
                if (comboBoxPortasCom.SelectedItem == null)
                {
                    MessageBox.Show("Por favor, selecione uma porta COM.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string portaComSelecionada = comboBoxPortasCom.SelectedItem.ToString();

                if (File.Exists(dadosSelecionados.CaminhoConfiguracao))
                {
                    stconfigBox.Text = $"Arquivo de configuração carregado com sucesso: {Path.GetFileName(dadosSelecionados.CaminhoConfiguracao)}";
                    PassarConfiguracaoParaControlador(dadosSelecionados.CaminhoConfiguracao, portaComSelecionada);
                }
                else
                {
                    stconfigBox.Text = "Arquivo de configuração não encontrado.";
                }

                if (File.Exists(dadosSelecionados.CaminhoAtualizacao))
                {
                    pack.Text = $"Arquivo de atualização carregado com sucesso: {Path.GetFileName(dadosSelecionados.CaminhoAtualizacao)}";
                    PassarPackParaControlador(dadosSelecionados.CaminhoAtualizacao, portaComSelecionada);
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

        private void PassarConfiguracaoParaControlador(string caminhoConfiguracao, string portaCom)
        {
            string comando = $"configurations apply --src \"{caminhoConfiguracao}\" --connection \"Serial,{portaCom},1152008N2,248\" --verify --verify-delay 20";
            ExecutarComandoNoControlador(comando);
        }

        private void PassarPackParaControlador(string caminhoPack, string portaCom)
        {
            string comando = $"app download --src \"{caminhoPack}\" --connection Serial,{portaCom},192008N2,1";
            ExecutarComandoNoControlador(comando);
        }

        private void ExecutarComandoNoControlador(string comando)
        {
            try
            {
                System.Diagnostics.Process.Start("cmd.exe", "/C " + comando);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao executar comando: " + ex.Message);
            }
        }

        // Métodos vazios do Designer
        private void pictureBox2_Click(object sender, EventArgs e) { }
        private void pictureBox3_Click(object sender, EventArgs e) { }
        private void pictureBox4_Click(object sender, EventArgs e) { }
        private void button2_Click(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }
        private void txtCodigoBarras_TextChanged(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void textBox1_TextChanged_1(object sender, EventArgs e) { }
        private void pack_TextChanged(object sender, EventArgs e) { }
    }
}
