using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;

namespace FinalLab
{
    public partial class MainWindow : Window
    {
        public static string NomeUtilizadorLogado { get; set; } = "Utilizador Padrão";
        public static string EmailUtilizadorLogado { get; set; } = "exemplo@email.com";
        public static string? CaminhoFotoUtilizadorLogado { get; set; }

        private List<Tarefa> listaDeTarefasPrincipal = new List<Tarefa>();
        private Border? tarefasViewContainer;
        private Grid? actualTaskTableGrid;

        public MainWindow()
        {
            InitializeComponent();
            SetupTarefasView();
            if (tarefasViewContainer != null) NavigateToPage(tarefasViewContainer);
            UpdatePlaceholderVisibility();
            UpdateTopBarUserName(); // Carrega o nome do utilizador ao iniciar
        }

        // Método para atualizar o nome do utilizador na barra superior
        public void UpdateTopBarUserName()
        {
            if (TopBarUserNameTextBlock != null)
            {
                TopBarUserNameTextBlock.Text = NomeUtilizadorLogado;
            }
        }

        private void SetupTarefasView()
        {
            tarefasViewContainer = new Border { BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(1.0) };
            actualTaskTableGrid = new Grid();
            actualTaskTableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            actualTaskTableGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            string[] headers = { "Nome da Tarefa", "Prazo", "Peso", "Ações" };
            for (int i = 0; i < headers.Length; i++)
            {
                Border headerBorder = new Border { Background = new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0)), Padding = new Thickness(10.0) };
                headerBorder.Child = new TextBlock { Text = headers[i], FontWeight = FontWeights.Bold };
                Grid.SetRow(headerBorder, 0); Grid.SetColumn(headerBorder, i);
                actualTaskTableGrid?.Children.Add(headerBorder);
            }
            ScrollViewer taskScrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = actualTaskTableGrid };
            if (tarefasViewContainer != null)
            {
                tarefasViewContainer.Child = taskScrollViewer;
            }
        }

        public void NavigateToPage(object? pageContent)
        {
            if (MainContentArea != null)
            {
                MainContentArea.Content = pageContent;

                if (pageContent == tarefasViewContainer && tarefasViewContainer != null)
                {
                    ShowContextSpecificControls(true);
                    AtualizarTabelaDeTarefasUI();
                }
                else if (pageContent is Perfil || pageContent is EditarPerfil)
                {
                    ShowContextSpecificControls(false);
                }
                else
                {
                    ShowContextSpecificControls(false);
                }
            }
        }

        private void ShowContextSpecificControls(bool show)
        {
            Visibility visibility = show ? Visibility.Visible : Visibility.Collapsed;

            TopBarGrid?.SetCurrentValue(Grid.VisibilityProperty, visibility);
            SummaryBoxesStackPanel?.SetCurrentValue(StackPanel.VisibilityProperty, visibility);
            CriarTarefaButton?.SetCurrentValue(Button.VisibilityProperty, visibility);

            if (show)
            {
                UpdatePlaceholderVisibility();
            }
            else
            {
                PlaceholderTextBlock?.SetCurrentValue(TextBlock.VisibilityProperty, Visibility.Collapsed);
            }
        }

        private void UpdatePlaceholderVisibility()
        {
            if (SearchTextBox != null && PlaceholderTextBlock != null)
            {
                bool isTopBarVisible = (TopBarGrid != null && TopBarGrid.Visibility == Visibility.Visible);
                PlaceholderTextBlock.Visibility = (isTopBarVisible && string.IsNullOrEmpty(SearchTextBox.Text))
                                                  ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void SearchTextBox_TextChanged_UpdatePlaceholder(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholderVisibility();
            if (MainContentArea.Content == tarefasViewContainer)
            {
                AtualizarTabelaDeTarefasUI();
            }
        }

        private void MainMenuPerfilButton_Click(object sender, RoutedEventArgs e) { NavigateToPage(new Perfil()); }
        private void DashboardButton_Click(object sender, RoutedEventArgs e) { NavigateToPage(tarefasViewContainer); }
        private void AlunosButton_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Alunos Clicado!"); NavigateToPage(null); }
        private void GruposButton_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Grupos Clicado!"); NavigateToPage(null); }
        private void TarefasButton_Click(object sender, RoutedEventArgs e) { NavigateToPage(tarefasViewContainer); }
        private void PautaButton_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Pauta Clicado!"); NavigateToPage(null); }
        private void ConfiguracoesButton_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Configurações Clicado!"); NavigateToPage(null); }

        private void CriarTarefaButton_Click(object sender, RoutedEventArgs e)
        {
            CriarTarefaWindow criarTarefaWin = new CriarTarefaWindow { Owner = this };
            if (criarTarefaWin.ShowDialog() == true && criarTarefaWin.TarefaCriadaComSucesso)
            {
                listaDeTarefasPrincipal.Add(criarTarefaWin.NovaTarefa);
                if (MainContentArea.Content == tarefasViewContainer) { AtualizarTabelaDeTarefasUI(); }
            }
        }

        private void ApagarTarefaButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button apagarButton && apagarButton.Tag is Tarefa tarefaParaApagar)
            {
                if (MessageBox.Show($"Tem a certeza que deseja apagar a tarefa '{tarefaParaApagar.Nome}'?", "Confirmar Apagar Tarefa", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    listaDeTarefasPrincipal.Remove(tarefaParaApagar);
                    if (MainContentArea.Content == tarefasViewContainer) { AtualizarTabelaDeTarefasUI(); }
                    MessageBox.Show($"Tarefa '{tarefaParaApagar.Nome}' apagada.", "Tarefa Apagada", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void AtualizarTabelaDeTarefasUI()
        {
            if (actualTaskTableGrid == null) return;

            for (int i = actualTaskTableGrid.Children.Count - 1; i >= 0; i--)
            {
                if (Grid.GetRow(actualTaskTableGrid.Children[i]) > 0)
                {
                    actualTaskTableGrid.Children.RemoveAt(i);
                }
            }
            while (actualTaskTableGrid.RowDefinitions.Count > 2)
            {
                actualTaskTableGrid.RowDefinitions.RemoveAt(1);
            }

            int rowIndex = 1;
            foreach (var tarefa in listaDeTarefasPrincipal)
            {
                if (actualTaskTableGrid.RowDefinitions.Count < 2 && rowIndex == 1)
                {
                    actualTaskTableGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                }
                actualTaskTableGrid.RowDefinitions.Insert(actualTaskTableGrid.RowDefinitions.Count - 1, new RowDefinition { Height = GridLength.Auto });

                actualTaskTableGrid.Children.Add(CreateTableCellUI(tarefa.Nome, rowIndex, 0));
                actualTaskTableGrid.Children.Add(CreateTableCellUI(tarefa.Prazo.ToShortDateString(), rowIndex, 1));
                actualTaskTableGrid.Children.Add(CreateTableCellUI($"{tarefa.Peso}%", rowIndex, 2));
                Button apagarButton = new Button { Content = "Apagar", Tag = tarefa, Margin = new Thickness(5.0, 2.0, 5.0, 2.0), Padding = new Thickness(5.0, 2.0, 5.0, 2.0), Foreground = Brushes.Red };
                apagarButton.Click += ApagarTarefaButton_Click;
                Grid.SetRow(apagarButton, rowIndex); Grid.SetColumn(apagarButton, 3);
                actualTaskTableGrid.Children.Add(apagarButton);
                rowIndex++;
            }
        }

        private Border CreateTableCellUI(string text, int row, int column)
        {
            Border cellBorder = new Border
            {
                BorderBrush = Brushes.Gainsboro,
                Padding = new Thickness(10.0),
                BorderThickness = new Thickness(0.0, 0.0, (column < 2) ? 1.0 : 0.0, 1.0)
            };
            TextBlock content = new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
            cellBorder.Child = content;
            Grid.SetRow(cellBorder, row); Grid.SetColumn(cellBorder, column);
            return cellBorder;
        }
    }
}
