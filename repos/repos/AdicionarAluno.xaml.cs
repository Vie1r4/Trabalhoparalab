using System;
using System.Windows;
using System.Text.RegularExpressions; // Necessário para Regex

namespace FinalLab
{
    public partial class AdicionarAluno : Window
    {
        public Aluno? NovoAluno { get; private set; }
        public bool AlunoAdicionadoComSucesso { get; private set; } = false;

        public AdicionarAluno()
        {
            InitializeComponent();
        }

        private void GuardarButton_Click(object sender, RoutedEventArgs e)
        {
            string nomeCompleto = NomeCompletoTextBox.Text.Trim();
            string numeroAluno = NumeroAlunoTextBox.Text.Trim();
            // string turma = TurmaTextBox.Text.Trim(); // Removido conforme a sua última instrução
            string email = EmailTextBox.Text.Trim(); // Obtém o email e remove espaços no início/fim

            // Validações de campos obrigatórios
            if (string.IsNullOrWhiteSpace(nomeCompleto) ||
                string.IsNullOrWhiteSpace(numeroAluno) ||
                string.IsNullOrWhiteSpace(email)) // Turma removida da validação de obrigatório
            {
                MessageBox.Show("Os campos Nome, Número e Email são obrigatórios.", "Erro de Validação", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            // SECÇÃO DE VALIDAÇÃO DO EMAIL
            //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            // A expressão regular para validar formatos de email comuns.
            // Aceita: local-part@domain.top-level-domain
            // Rejeita: espaços, múltiplos @, falta de partes essenciais.
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            if (!Regex.IsMatch(email, emailPattern))
            {
                MessageBox.Show("Formato de email inválido. Por favor, insira um email como 'nome@dominio.com'.", "Erro de Validação", MessageBoxButton.OK, MessageBoxImage.Error);
                return; // Interrompe a execução se o email for inválido
            }
            //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            // FIM DA SECÇÃO DE VALIDAÇÃO DO EMAIL
            //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            try
            {
                // O grupo é passado como null, o construtor de Aluno define "Sem Grupo" por defeito
                NovoAluno = new Aluno(nomeCompleto, numeroAluno, email, null);
                AlunoAdicionadoComSucesso = true;
                MessageBox.Show("Aluno adicionado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro ao adicionar o aluno: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                AlunoAdicionadoComSucesso = false;
            }
        }

        private void CancelarButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
