using System;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions; // <<<<<<<<<<<<<<<<<<<< ADICIONE ESTE USING

namespace FinalLab
{
    // Definição da classe Tarefa (como na resposta anterior)
    public class Tarefa
    {
        public Guid Id { get; }
        public string Titulo { get; }
        public string? Descricao { get; }
        public DateTime DataHoraInicio { get; }
        public DateTime DataHoraTermino { get; }
        public int Peso { get; }

        public Tarefa(string titulo, string? descricao, DateTime dataHoraInicio, DateTime dataHoraTermino, int peso)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("O título da tarefa não pode ser vazio.", nameof(titulo));
            if (dataHoraTermino < dataHoraInicio)
                throw new ArgumentException("A data de término não pode ser anterior à data de início.", nameof(dataHoraTermino));
            if (peso < 0 || peso > 100)
                throw new ArgumentOutOfRangeException(nameof(peso), "O peso deve estar entre 0 e 100.");

            Id = Guid.NewGuid();
            Titulo = titulo;
            Descricao = descricao;
            DataHoraInicio = dataHoraInicio;
            DataHoraTermino = dataHoraTermino;
            Peso = peso;
        }
    }

    public partial class Criartarefa : Window
    {
        public Tarefa? NovaTarefa { get; private set; }
        public bool TarefaCriadaComSucesso { get; private set; } = false;

        public Criartarefa()
        {
            InitializeComponent();
            // Tenta encontrar os controlos e definir valores padrão.
            // Certifique-se que os nomes no FindName correspondem aos x:Name no seu XAML.
            if (this.FindName("DataTerminoPicker") is DatePicker dpTermino)
            {
                dpTermino.SelectedDate = DateTime.Today.AddDays(7);
            }
            if (this.FindName("DataInicioPicker") is DatePicker dpInicio)
            {
                dpInicio.SelectedDate = DateTime.Today;
            }
        }

        private void GuardarButton_Click(object sender, RoutedEventArgs e)
        {
            // Aceder aos controlos XAML usando this.FindName
            var tituloTextBox = this.FindName("TituloTextBox") as TextBox;
            var descricaoTextBox = this.FindName("DescricaoTextBox") as TextBox;
            var dataInicioPicker = this.FindName("DataInicioPicker") as DatePicker;
            var dataTerminoPicker = this.FindName("DataTerminoPicker") as DatePicker;
            var pesoTextBox = this.FindName("PesoTextBox") as TextBox;
            // var notificarCheckBox = this.FindName("NotificarCheckBox") as CheckBox; // Se o tiver

            // Verificar se os controlos foram encontrados
            if (tituloTextBox == null || dataInicioPicker == null || dataTerminoPicker == null || pesoTextBox == null)
            {
                MessageBox.Show("Erro interno: Um ou mais controlos não foram encontrados na janela CriarTarefa. Verifique os nomes no XAML.", "Erro de UI", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string titulo = tituloTextBox.Text.Trim();
            string? descricao = descricaoTextBox?.Text.Trim();

            if (!dataInicioPicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Por favor, selecione uma data de início.", "Erro de Validação", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DateTime dataHoraInicio = dataInicioPicker.SelectedDate.Value;

            if (!dataTerminoPicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Por favor, selecione uma data de término.", "Erro de Validação", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DateTime dataHoraTermino = dataTerminoPicker.SelectedDate.Value;

            string pesoStr = pesoTextBox.Text.Trim();
            // bool notificar = notificarCheckBox?.IsChecked ?? false; // Se tiver

            if (string.IsNullOrWhiteSpace(titulo))
            {
                MessageBox.Show("O título da tarefa é obrigatório.", "Erro de Validação", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!int.TryParse(pesoStr, out int peso) || peso < 0 || peso > 100)
            {
                MessageBox.Show("O peso da tarefa deve ser um número entre 0 e 100.", "Erro de Validação", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (dataHoraTermino < dataHoraInicio)
            {
                MessageBox.Show("A data de término não pode ser anterior à data de início.", "Erro de Validação", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                NovaTarefa = new Tarefa(titulo, string.IsNullOrEmpty(descricao) ? null : descricao, dataHoraInicio, dataHoraTermino, peso);
                TarefaCriadaComSucesso = true;
                MessageBox.Show("Tarefa criada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Erro de Validação na Tarefa", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro inesperado: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelarButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // MÉTODO QUE ESTAVA EM FALTA:
        private void PesoTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Permite apenas a entrada de dígitos numéricos
            Regex regex = new Regex("[^0-9]+"); // Expressão regular para encontrar qualquer coisa que NÃO seja um dígito
            e.Handled = regex.IsMatch(e.Text);  // Se o texto introduzido não for um dígito, marca o evento como tratado (ignora a entrada)
        }
    }
}
