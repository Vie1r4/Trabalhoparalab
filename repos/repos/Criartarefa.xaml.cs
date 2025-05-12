using System;
using System.Windows;
using System.Windows.Controls; // Necessário para ComboBoxItem, mas não temos ComboBox aqui. Ainda útil para TextChangedEventArgs, etc.

namespace FinalLab // Certifique-se que é o mesmo namespace do seu projeto
{
    /// <summary>
    /// Interaction logic for CriarTarefaWindow.xaml
    /// </summary>
    public partial class CriarTarefaWindow : Window // Se o seu XAML usa x:Class="FinalLab.Criartarefa", mude aqui para : public partial class Criartarefa : Window
    {
        // Propriedades para aceder aos dados da tarefa após a janela ser fechada
        public Tarefa NovaTarefa { get; private set; }
        public bool TarefaCriadaComSucesso { get; private set; } = false;

        public CriarTarefaWindow() // Construtor
        {
            InitializeComponent(); // Esta linha é ESSENCIAL. Ela cria os controlos definidos no XAML (NomeTarefaTextBox, etc.)

            // Define uma data de prazo padrão (opcional)
            PrazoDatePicker.SelectedDate = DateTime.Today.AddDays(7);
        }

        private void GuardarButton_Click(object sender, RoutedEventArgs e)
        {
            // Limpa mensagens de feedback anteriores
            FeedbackTextBlock.Text = "";

            // Validação do Nome da Tarefa
            if (string.IsNullOrWhiteSpace(NomeTarefaTextBox.Text))
            {
                FeedbackTextBlock.Text = "O nome da tarefa é obrigatório.";
                NomeTarefaTextBox.Focus();
                return; // Interrompe a execução do método se a validação falhar
            }

            // Validação do Prazo
            if (PrazoDatePicker.SelectedDate == null)
            {
                FeedbackTextBlock.Text = "Selecione um prazo.";
                PrazoDatePicker.Focus();
                return;
            }
            if (PrazoDatePicker.SelectedDate.Value < DateTime.Today)
            {
                FeedbackTextBlock.Text = "O prazo não pode ser no passado.";
                PrazoDatePicker.Focus();
                return;
            }

            // Validação do Peso
            int peso;
            if (string.IsNullOrWhiteSpace(PesoTextBox.Text) || !int.TryParse(PesoTextBox.Text, out peso) || peso <= 0 || peso > 100)
            {
                FeedbackTextBlock.Text = "O peso deve ser um número válido entre 1 e 100.";
                PesoTextBox.Focus();
                return;
            }

            // Se todas as validações passarem, cria o objeto Tarefa
            NovaTarefa = new Tarefa
            {
                Nome = NomeTarefaTextBox.Text,
                Prazo = PrazoDatePicker.SelectedDate.Value,
                Peso = peso,
                Descricao = DescricaoTextBox.Text, // DescricaoTextBox pode estar vazio, é opcional
                Notificar = NotificarCheckBox.IsChecked ?? false // Assume false se IsChecked for null
            };

            TarefaCriadaComSucesso = true; // Indica que a tarefa foi criada com sucesso

            // Mostra uma mensagem de sucesso (opcional)
            MessageBox.Show($"Tarefa '{NovaTarefa.Nome}' guardada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

            this.DialogResult = true; // Define o resultado do diálogo como true (útil para a janela que a abriu)
            this.Close(); // Fecha a janela CriarTarefaWindow
        }

        private void CancelarButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Define o resultado do diálogo como false
            this.Close(); // Fecha a janela CriarTarefaWindow
        }
    }

    // Definição da classe Tarefa (certifique-se que está no mesmo namespace ou que há um 'using')
    // Se já tem esta classe noutro ficheiro DENTRO do mesmo namespace 'FinalLab', não precisa de a redefinir aqui.
    // Se estiver num ficheiro separado com um namespace diferente, adicione 'using SeuOutroNamespace;' no topo.
    public class Tarefa
    {
        public string Nome { get; set; }
        public DateTime Prazo { get; set; }
        public int Peso { get; set; }
        public string Descricao { get; set; }
        public bool Notificar { get; set; }
    }
}