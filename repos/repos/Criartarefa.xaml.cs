using System;
using System.Windows;
using System.Windows.Controls; // Adicionado para DatePicker, TextBox, CheckBox

namespace FinalLab
{
    public class Tarefa
    {
        public string Nome { get; }
        public string Descricao { get; }
        public DateTime Prazo { get; }
        public int Peso { get; }
        public bool Notificar { get; }

        public Tarefa(string nome, string descricao, DateTime prazo, int peso, bool notificar)
        {
            Nome = nome;
            Descricao = descricao;
            Prazo = prazo;
            Peso = peso;
            Notificar = notificar;
        }
    }

    public partial class Criartarefa : Window
    {
        public Tarefa? NovaTarefa { get; private set; } // Tornada anulável
        public bool TarefaCriadaComSucesso { get; private set; } = false;

        public Criartarefa()
        {
            InitializeComponent(); // Deve funcionar se Criartarefa.xaml estiver correto

            if (this.FindName("PrazoDatePicker") is DatePicker prazoPicker) // Usar FindName para segurança
            {
                prazoPicker.DisplayDateStart = DateTime.Today;
            }
        }

        private void GuardarButton_Click(object sender, RoutedEventArgs e)
        {
            // Aceder aos controlos usando x:Name definido no XAML
            string nome = NomeTarefaTextBox.Text.Trim();
            string descricao = DescricaoTarefaTextBox.Text.Trim();
            DateTime? prazo = PrazoDatePicker.SelectedDate;
            string pesoStr = PesoTextBox.Text.Trim();
            bool notificar = NotificarCheckBox.IsChecked ?? false;

            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(descricao) || !prazo.HasValue || string.IsNullOrWhiteSpace(pesoStr))
            {
                MessageBox.Show("Todos os campos da tarefa são obrigatórios.", "Erro de Validação", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (prazo.Value.Date < DateTime.Today)
            {
                MessageBox.Show("A data de prazo não pode ser no passado.", "Erro de Validação", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!int.TryParse(pesoStr, out int peso) || peso < 0 || peso > 100)
            {
                MessageBox.Show("O peso deve ser um número entre 0 e 100.", "Erro de Validação", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                NovaTarefa = new Tarefa(nome, descricao, prazo.Value, peso, notificar);
                TarefaCriadaComSucesso = true;
                MessageBox.Show("Tarefa criada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro ao criar a tarefa: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                TarefaCriadaComSucesso = false;
            }
        }

        private void CancelarButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
