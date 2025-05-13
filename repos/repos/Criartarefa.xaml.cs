using System;
using System.Windows;

namespace FinalLab
{
    // Esta classe Tarefa pode estar aqui ou num ficheiro Tarefa.cs separado.
    // Se estiver separada, certifique-se que o namespace é o mesmo.
    public class Tarefa
    {
        public string Nome { get; }
        public string Descricao { get; } // Propriedade para a Descrição
        public DateTime Prazo { get; }
        public int Peso { get; }
        public bool Notificar { get; }

        // Construtor atualizado
        public Tarefa(string nome, string descricao, DateTime prazo, int peso, bool notificar)
        {
            Nome = nome;
            Descricao = descricao; // Atribuir a descrição
            Prazo = prazo;
            Peso = peso;
            Notificar = notificar;
        }
    }

    public partial class Criartarefa : Window // O nome da classe deve ser "Criartarefa"
    {
        public Tarefa NovaTarefa { get; private set; }
        public bool TarefaCriadaComSucesso { get; private set; } = false;

        public Criartarefa() // O construtor deve chamar-se "Criartarefa"
        {
            InitializeComponent(); // Este método é gerado pelo compilador a partir do XAML
            PrazoDatePicker.DisplayDateStart = DateTime.Today;
        }

        private void GuardarButton_Click(object sender, RoutedEventArgs e)
        {
            string nome = NomeTarefaTextBox.Text.Trim();
            string descricao = DescricaoTarefaTextBox.Text.Trim(); // Obter o valor da descrição
            DateTime? prazo = PrazoDatePicker.SelectedDate;
            string pesoStr = PesoTextBox.Text.Trim();
            bool notificar = NotificarCheckBox.IsChecked ?? false;

            if (string.IsNullOrWhiteSpace(nome))
            {
                MessageBox.Show("O nome da tarefa é obrigatório.", "Erro de Validação", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validação opcional para descrição (se quiser que seja obrigatória)
            if (string.IsNullOrWhiteSpace(descricao))
            {
                MessageBox.Show("A descrição da tarefa é obrigatória.", "Erro de Validação", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!prazo.HasValue)
            {
                MessageBox.Show("Selecione uma data de prazo.", "Erro de Validação", MessageBoxButton.OK, MessageBoxImage.Error);
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
                // Criar a NovaTarefa usando o construtor que aceita a descrição
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
