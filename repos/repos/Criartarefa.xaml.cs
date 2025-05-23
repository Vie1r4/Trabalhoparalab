using System;
using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;
using FinalLab.Models;

namespace FinalLab
{
    public partial class Criartarefa : Window
    {
        // REMOVIDAS PROPRIEDADES DUPLICADAS - Mantida apenas uma versão
        public Tarefa? NovaTarefa { get; private set; }
        public bool TarefaCriadaComSucesso { get; private set; } = false;

        public Criartarefa()
        {
            InitializeComponent();
            DataInicioPicker.SelectedDate = DateTime.Today;
            DataTerminoPicker.SelectedDate = DateTime.Today.AddDays(7);
        }

        // REMOVIDOS MÉTODOS DUPLICADOS - Mantida apenas uma versão
        private void GuardarButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TituloTextBox.Text))
                {
                    MessageBox.Show("O título da tarefa é obrigatório.", "Erro de Validação", MessageBoxButton.OK, MessageBoxImage.Error);
                    TituloTextBox.Focus();
                    return;
                }
                if (DataInicioPicker.SelectedDate == null)
                {
                    MessageBox.Show("A data de início é obrigatória.", "Erro de Validação", MessageBoxButton.OK, MessageBoxImage.Error);
                    DataInicioPicker.Focus();
                    return;
                }
                if (DataTerminoPicker.SelectedDate == null)
                {
                    MessageBox.Show("A data de término é obrigatória.", "Erro de Validação", MessageBoxButton.OK, MessageBoxImage.Error);
                    DataTerminoPicker.Focus();
                    return;
                }
                DateTime dataInicio = DataInicioPicker.SelectedDate.Value;
                DateTime dataTermino = DataTerminoPicker.SelectedDate.Value;

                if (!int.TryParse(PesoTextBox.Text, out int peso) || peso < 0 || peso > 100)
                {
                    MessageBox.Show("O peso deve ser um número entre 0 e 100.", "Erro de Validação", MessageBoxButton.OK, MessageBoxImage.Error);
                    PesoTextBox.Focus();
                    return;
                }

                NovaTarefa = new Tarefa(
                    TituloTextBox.Text,
                    DescricaoTextBox.Text,
                    dataInicio,
                    dataTermino,
                    peso
                );

                TarefaCriadaComSucesso = true;
                MessageBox.Show("Tarefa criada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                this.Close();
            }
            catch (ArgumentException argEx)
            {
                MessageBox.Show(argEx.Message, "Erro ao Criar Tarefa", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro inesperado: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelarButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void PesoTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
