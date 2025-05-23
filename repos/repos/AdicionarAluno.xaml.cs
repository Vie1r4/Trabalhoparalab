// Ficheiro: AdicionarAluno.xaml.cs
using System;
using System.Text.RegularExpressions;
using System.Windows;
using FinalLab.Models;

namespace FinalLab
{
    public partial class AdicionarAluno : Window
    {
        public Aluno? NovoAluno { get; private set; }
        public bool AlunoAdicionadoComSucesso { get; private set; } = false;

        public AdicionarAluno()
        {
            InitializeComponent();
            // Tradução do título da janela
            this.Title = "Adicionar Novo Aluno";
        }

        private void GuardarButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string nomeCompleto = NomeCompletoTextBox.Text.Trim();
                string numeroAluno = NumeroAlunoTextBox.Text.Trim();
                string email = EmailTextBox.Text.Trim();
                string? grupo = GrupoTextBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(nomeCompleto))
                {
                    MessageBox.Show("O campo 'Nome Completo' é de preenchimento obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                    NomeCompletoTextBox.Focus(); return;
                }
                if (string.IsNullOrWhiteSpace(numeroAluno))
                {
                    MessageBox.Show("O campo 'N.º Aluno' é de preenchimento obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                    NumeroAlunoTextBox.Focus(); return;
                }
                if (string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show("O campo 'Email' é de preenchimento obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                    EmailTextBox.Focus(); return;
                }

                // Validação de Email UTAD - assumindo 5 dígitos para 'alxxxxx'
                if (!Regex.IsMatch(email, @"^al\d{5}@alunos\.utad\.pt$", RegexOptions.IgnoreCase))
                {
                    MessageBox.Show("O formato do email é inválido. Deve ser 'alxxxxx@alunos.utad.pt', onde x é um dígito.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                    EmailTextBox.Focus(); return;
                }

                NovoAluno = new Aluno(nomeCompleto, numeroAluno, email, string.IsNullOrWhiteSpace(grupo) ? null : grupo);
                AlunoAdicionadoComSucesso = true;
                MessageBox.Show("Aluno adicionado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true; this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro ao adicionar o aluno: {ex.Message}", "Erro Inesperado", MessageBoxButton.OK, MessageBoxImage.Error);
                AlunoAdicionadoComSucesso = false;
            }
        }

        private void CancelarButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; this.Close();
        }
    }
}
