// FinalLab/Criartarefa.xaml.cs
using FinalLab.Models;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace FinalLab
{
    public partial class Criartarefa : Window
    {
        public Tarefa? NovaTarefa { get; private set; } // Propriedade anulável
        public bool TarefaCriadaComSucesso { get; private set; } = false;

        public Criartarefa()
        {
            InitializeComponent();
            DataInicioPicker.SelectedDate = DateTime.Today;
            DataTerminoPicker.SelectedDate = DateTime.Today.AddDays(7);
        }

        private void GuardarButton_Click(object sender, RoutedEventArgs e)
        {
            string titulo = TituloTextBox.Text.Trim();
            string? descricao = DescricaoTextBox.Text.Trim(); // Descrição é opcional

            if (string.IsNullOrWhiteSpace(titulo))
            {
                MessageBox.Show("O título da tarefa é obrigatório.", "Campo Obrigatório", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DataInicioPicker.SelectedDate.HasValue || !DataTerminoPicker.SelectedDate.HasValue)
            {
                MessageBox.Show("As datas de início e término são obrigatórias.", "Campos Obrigatórios", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime dataInicio = DataInicioPicker.SelectedDate.Value;
            DateTime dataTermino = DataTerminoPicker.SelectedDate.Value;

            if (dataTermino < dataInicio)
            {
                MessageBox.Show("A data de término não pode ser anterior à data de início.", "Datas Inválidas", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(PesoTextBox.Text) || !int.TryParse(PesoTextBox.Text, out int peso) || peso < 0 || peso > 100)
            {
                MessageBox.Show("O peso deve ser um número inteiro entre 0 e 100.", "Peso Inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                NovaTarefa = new Tarefa(titulo, string.IsNullOrWhiteSpace(descricao) ? null : descricao, dataInicio, dataTermino, peso);
                TarefaCriadaComSucesso = true;
                MessageBox.Show("Tarefa criada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Erro ao criar tarefa: {ex.Message}", "Erro de Validação", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"ArgumentException em CriarTarefa: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro inesperado: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"Exception em CriarTarefa: {ex.ToString()}");
            }
        }

        private void CancelarButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void PesoTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
