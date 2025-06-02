using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;
using ClosedXML.Excel;
using System.Diagnostics;

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

            PreencherPortasCom();
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

        private void PreencherPortasCom()
        {
            AtualizarPortasCom();
        }

        private void txtCodigoBarras_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                string codigo = txtCodigoBarras.Text.Trim();

                if (string.IsNullOrWhiteSpace(codigo))
                {
                    MessageBox.Show("Por favor, insira um código de barras.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

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

            if (comboBoxPortasCom.SelectedItem == null)
            {
                MessageBox.Show("Por favor, selecione uma porta COM.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string portaComSelecionada = comboBoxPortasCom.SelectedItem.ToString();

            try
            {
                // Aplicar configuração primeiro
                if (File.Exists(dadosSelecionados.CaminhoConfiguracao))
                {
                    stconfigBox.Text = $"Carregando configuração: {Path.GetFileName(dadosSelecionados.CaminhoConfiguracao)}";

                    if (PassarConfiguracaoParaControlador(dadosSelecionados.CaminhoConfiguracao, portaComSelecionada))
                    {
                        stconfigBox.Text = $"Configuração aplicada com sucesso: {Path.GetFileName(dadosSelecionados.CaminhoConfiguracao)}";
                    }
                    else
                    {
                        stconfigBox.Text = "Erro ao aplicar configuração.";
                        MessageBox.Show("Erro ao aplicar configuração. Verifique a conexão.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    stconfigBox.Text = "Arquivo de configuração não encontrado.";
                    MessageBox.Show("Arquivo de configuração não encontrado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Aguardar um pouco antes de aplicar o pack
                System.Threading.Thread.Sleep(2000);

                // Aplicar pack/atualização
                if (File.Exists(dadosSelecionados.CaminhoAtualizacao))
                {
                    pack.Text = $"Carregando atualização: {Path.GetFileName(dadosSelecionados.CaminhoAtualizacao)}";

                    if (PassarPackParaControlador(dadosSelecionados.CaminhoAtualizacao, portaComSelecionada))
                    {
                        pack.Text = $"Atualização aplicada com sucesso: {Path.GetFileName(dadosSelecionados.CaminhoAtualizacao)}";
                        MessageBox.Show("Gravação concluída com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        pack.Text = "Erro ao aplicar atualização.";
                        MessageBox.Show("Erro ao aplicar atualização. Verifique a conexão.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    pack.Text = "Arquivo de atualização não encontrado.";
                    MessageBox.Show("Arquivo de atualização (.pack) não encontrado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro na gravação: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool PassarConfiguracaoParaControlador(string caminhoConfiguracao, string portaCom)
        {
            string sparklyPath = @"C:\Program Files (x86)\CAREL\Sparkly\Sparkly.exe";

            if (!File.Exists(sparklyPath))
            {
                MessageBox.Show("Sparkly não encontrado no caminho: " + sparklyPath, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Primeiro, testa a conexão
            if (!TestarConexaoControlador(portaCom))
            {
                MessageBox.Show($"Não foi possível estabelecer conexão com o controlador na porta {portaCom}.\n\nVerifique:\n- Se o cabo está conectado\n- Se a porta COM está correta\n- Se o controlador está ligado", "Erro de Conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Formatos baseados na documentação CAREL e testes comuns
            string[] formatosConexao = {
                $"Serial:{portaCom}:19200:8:N:2",  // Formato mais comum
                $"Serial:{portaCom}:19200:8:N:1",
                $"Serial,{portaCom},19200,8,N,2", // Formato com vírgula 
                $"Serial,{portaCom},19200,8,N,1",
                $"{portaCom}:19200:8:N:2",        // Formato simples
                $"{portaCom}:19200:8:N:1",
                $"{portaCom},19200,8,N,2",        // Formato simples com vírgula
                $"{portaCom},19200,8,N,1",
                $"Serial;{portaCom};19200;8;N;2", // Formato com ponto e vírgula
                $"Serial;{portaCom};19200;8;N;1",
                $"{portaCom}",                    // Apenas a porta
                $"Serial:{portaCom}",             // Serial + porta
                $"Serial,{portaCom}"              // Serial + porta com vírgula
            };

            string ultimoErro = "";

            foreach (string formatoConexao in formatosConexao)
            {
                try
                {
                    using (var processo = new Process())
                    {
                        processo.StartInfo.FileName = sparklyPath;
                        processo.StartInfo.Arguments = $"configurations apply --src \"{caminhoConfiguracao}\" --connection \"{formatoConexao}\" --verify --verify-delay 20";
                        processo.StartInfo.UseShellExecute = false;
                        processo.StartInfo.CreateNoWindow = true;
                        processo.StartInfo.RedirectStandardOutput = true;
                        processo.StartInfo.RedirectStandardError = true;

                        processo.Start();

                        string output = processo.StandardOutput.ReadToEnd();
                        string error = processo.StandardError.ReadToEnd();

                        processo.WaitForExit(45000);

                        // Salva o último erro para debug
                        ultimoErro = $"Formato: {formatoConexao}\nOutput: {output}\nError: {error}\nExitCode: {processo.ExitCode}";

                        if (processo.ExitCode == 0)
                        {
                            return true;
                        }

                        // Se o erro não for de formato de conexão, mostra detalhes
                        if (!output.Contains("Serial configuration string is invalid") &&
                            !error.Contains("Serial configuration string is invalid"))
                        {
                            // Se não é erro de formato, pode ser outro problema mais sério
                            if (MessageBox.Show($"Erro encontrado (não relacionado ao formato):\n{ultimoErro}\n\nDeseja continuar tentando outros formatos?",
                                              "Debug", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                            {
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ultimoErro = $"Formato: {formatoConexao}\nExceção: {ex.Message}";
                    continue;
                }
            }

            // Mostra detalhes do último erro para debug
            MessageBox.Show($"Não foi possível encontrar um formato de conexão serial válido.\n\nÚltimo erro:\n{ultimoErro}",
                          "Erro de Conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        private bool PassarPackParaControlador(string caminhoPack, string portaCom)
        {
            string sparklyPath = @"C:\Program Files (x86)\CAREL\Sparkly\Sparkly.exe";

            if (!File.Exists(sparklyPath))
            {
                MessageBox.Show("Sparkly não encontrado no caminho: " + sparklyPath, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Formatos baseados na documentação CAREL e testes comuns
            string[] formatosConexao = {
                $"Serial:{portaCom}:19200:8:N:2",  // Formato mais comum
                $"Serial:{portaCom}:19200:8:N:1",
                $"Serial,{portaCom},19200,8,N,2", // Formato com vírgula 
                $"Serial,{portaCom},19200,8,N,1",
                $"{portaCom}:19200:8:N:2",        // Formato simples
                $"{portaCom}:19200:8:N:1",
                $"{portaCom},19200,8,N,2",        // Formato simples com vírgula
                $"{portaCom},19200,8,N,1",
                $"Serial;{portaCom};19200;8;N;2", // Formato com ponto e vírgula
                $"Serial;{portaCom};19200;8;N;1",
                $"{portaCom}",                    // Apenas a porta
                $"Serial:{portaCom}",             // Serial + porta
                $"Serial,{portaCom}"              // Serial + porta com vírgula
            };

            string ultimoErro = "";

            foreach (string formatoConexao in formatosConexao)
            {
                try
                {
                    using (var processo = new Process())
                    {
                        processo.StartInfo.FileName = sparklyPath;
                        processo.StartInfo.Arguments = $"app download --src \"{caminhoPack}\" --connection \"{formatoConexao}\" --working-directory \"C:\\Users\\peterson.junior\\Desktop\\Gelopar\"";
                        processo.StartInfo.UseShellExecute = false;
                        processo.StartInfo.CreateNoWindow = true;
                        processo.StartInfo.RedirectStandardOutput = true;
                        processo.StartInfo.RedirectStandardError = true;

                        processo.Start();

                        string output = processo.StandardOutput.ReadToEnd();
                        string error = processo.StandardError.ReadToEnd();

                        processo.WaitForExit(90000);

                        // Salva o último erro para debug
                        ultimoErro = $"Formato: {formatoConexao}\nOutput: {output}\nError: {error}\nExitCode: {processo.ExitCode}";

                        if (processo.ExitCode == 0)
                        {
                            return true;
                        }

                        // Se o erro nãofor de formato de conexão, mostra detalhes
                        if (!output.Contains("Serial configuration string is invalid") &&
                            !error.Contains("Serial configuration string is invalid"))
                        {
                            // Se não é erro de formato, pode ser outro problema mais sério
                            if (MessageBox.Show($"Erro encontrado (não relacionado ao formato):\n{ultimoErro}\n\nDeseja continuar tentando outros formatos?",
                                              "Debug", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                            {
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ultimoErro = $"Formato: {formatoConexao}\nExceção: {ex.Message}";
                    continue;
                }
            }

            // Mostra detalhes do último erro para debug
            MessageBox.Show($"Não foi possível encontrar um formato de conexão serial válido.\n\nÚltimo erro:\n{ultimoErro}",
                          "Erro de Conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        private bool TestarConexaoControlador(string portaCom)
        {
            try
            {
                using (SerialPort serialPort = new SerialPort())
                {
                    serialPort.PortName = portaCom;
                    serialPort.BaudRate = 19200;
                    serialPort.DataBits = 8;
                    serialPort.Parity = Parity.None;
                    serialPort.StopBits = StopBits.Two;
                    serialPort.Handshake = Handshake.None;
                    serialPort.ReadTimeout = 3000;
                    serialPort.WriteTimeout = 3000;

                    serialPort.Open();

                    if (serialPort.IsOpen)
                    {
                        // Tenta enviar um comando simples para testar
                        serialPort.WriteLine("?");
                        System.Threading.Thread.Sleep(500);

                        serialPort.Close();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log do erro para debug (opcional)
                System.Diagnostics.Debug.WriteLine($"Erro ao testar conexão: {ex.Message}");
                return false;
            }

            return false;
        }

        private void AtualizarPortasCom()
        {
            string portaAtualSelecionada = comboBoxPortasCom.SelectedItem?.ToString();

            string[] portasCom = SerialPort.GetPortNames();
            comboBoxPortasCom.Items.Clear();

            foreach (var porta in portasCom)
                comboBoxPortasCom.Items.Add(porta);

            // Tenta manter a porta anteriormente selecionada
            if (!string.IsNullOrEmpty(portaAtualSelecionada) && comboBoxPortasCom.Items.Contains(portaAtualSelecionada))
            {
                comboBoxPortasCom.SelectedItem = portaAtualSelecionada;
            }
            else if (comboBoxPortasCom.Items.Count > 0)
            {
                comboBoxPortasCom.SelectedIndex = 0;
            }
        }

        // Método para verificar o formato correto da conexão no Sparkly
        private void VerificarFormatoConexaoSparkly()
        {
            string sparklyPath = @"C:\Program Files (x86)\CAREL\Sparkly\Sparkly.exe";

            try
            {
                using (var processo = new Process())
                {
                    processo.StartInfo.FileName = sparklyPath;
                    processo.StartInfo.Arguments = "configurations apply --help";
                    processo.StartInfo.UseShellExecute = false;
                    processo.StartInfo.CreateNoWindow = true;
                    processo.StartInfo.RedirectStandardOutput = true;
                    processo.StartInfo.RedirectStandardError = true;

                    processo.Start();
                    string output = processo.StandardOutput.ReadToEnd();
                    processo.WaitForExit(10000);

                    // Mostra a ajuda em uma caixa de mensagem para debug
                    MessageBox.Show("Ajuda do Sparkly:\n" + output, "Debug - Formato de Conexão");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao obter ajuda do Sparkly: " + ex.Message);
            }
        }

        // Método para testar formatos de conexão sem executar comandos
        private void TestarFormatosConexao(string portaCom)
        {
            string sparklyPath = @"C:\Program Files (x86)\CAREL\Sparkly\Sparkly.exe";

            if (!File.Exists(sparklyPath))
            {
                MessageBox.Show("Sparkly não encontrado no caminho: " + sparklyPath);
                return;
            }

            string[] formatosConexao = {
                $"Serial:{portaCom}:19200:8:N:2",
                $"Serial,{portaCom},19200,8,N,2",
                $"{portaCom}:19200:8:N:2",
                $"{portaCom}",
                $"Serial:{portaCom}",
                $"Serial,{portaCom}"
            };

            string resultados = "Teste de formatos de conexão:\n\n";

            foreach (string formato in formatosConexao)
            {
                try
                {
                    using (var processo = new Process())
                    {
                        processo.StartInfo.FileName = sparklyPath;
                        // Usa um comando simples para testar apenas o formato
                        processo.StartInfo.Arguments = $"--connection \"{formato}\" --help";
                        processo.StartInfo.UseShellExecute = false;
                        processo.StartInfo.CreateNoWindow = true;
                        processo.StartInfo.RedirectStandardOutput = true;
                        processo.StartInfo.RedirectStandardError = true;

                        processo.Start();
                        string output = processo.StandardOutput.ReadToEnd();
                        string error = processo.StandardError.ReadToEnd();
                        processo.WaitForExit(5000);

                        resultados += $"Formato: {formato}\n";
                        resultados += $"ExitCode: {processo.ExitCode}\n";

                        if (output.Contains("Serial configuration string is invalid"))
                        {
                            resultados += "Status: INVÁLIDO\n";
                        }
                        else if (processo.ExitCode == 0)
                        {
                            resultados += "Status: VÁLIDO\n";
                        }
                        else
                        {
                            resultados += "Status: ERRO\n";
                        }

                        resultados += $"Output: {output.Substring(0, Math.Min(100, output.Length))}...\n";
                        resultados += "---\n";
                    }
                }
                catch (Exception ex)
                {
                    resultados += $"Formato: {formato}\nErro: {ex.Message}\n---\n";
                }
            }

            MessageBox.Show(resultados, "Teste de Formatos", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Métodos exigidos pelo Designer
        private void pack_TextChanged(object sender, EventArgs e) { }
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
    }

    public class DadosDoProduto
    {
        public string Modelo { get; set; }
        public string CaminhoConfiguracao { get; set; }
        public string CaminhoAtualizacao { get; set; }
    }
}