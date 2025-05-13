using System;
using System.Collections.Generic; // Para List<T>
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media; // Para Brushes e Color
// using System.IO; // Não parece ser usado neste excerto

namespace FinalLab
{
    public partial class MainWindow : Window
    {
        // Suas propriedades estáticas e lista de tarefas
        public static string NomeUtilizadorLogado { get; set; } = "Utilizador Padrão";
        public static string EmailUtilizadorLogado { get; set; } = "exemplo@email.com";
        public static string? CaminhoFotoUtilizadorLogado { get; set; } // Permite null

        private List<Tarefa> listaDeTarefasPrincipal = new List<Tarefa>();
        private Border? tarefasViewContainer; // Permite null
        private Grid? actualTaskTableGrid;    // Permite null

        public MainWindow()
        {
            InitializeComponent();
            SetupTarefasView();
            if (tarefasViewContainer != null) NavigateToPage(tarefasViewContainer);
            UpdatePlaceholderVisibility();
            UpdateTopBarUserName();
        }

        public void UpdateTopBarUserName()
        {
            // Tenta encontrar o controlo pelo nome. Se não existir, não faz nada.
            if (this.FindName("TopBarUserNameTextBlock") is TextBlock userNameLabel)
            {
                userNameLabel.Text = NomeUtilizadorLogado;
            }
        }

        private void SetupTarefasView()
        {
            tarefasViewContainer = new Border { BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(1.0) };
            actualTaskTableGrid = new Grid();
            actualTaskTableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Linha para cabeçalhos
            actualTaskTableGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Linha para o conteúdo scrollable

            // Definir colunas para a tabela
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) }); // Col 0: Nome da Tarefa
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });   // Col 1: Descrição
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });                         // Col 2: Prazo
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });                         // Col 3: Peso
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });                         // Col 4: Ações

            string[] headers = { "Nome da Tarefa", "Descrição", "Prazo", "Peso", "Ações" }; // Cabeçalhos atualizados
            for (int i = 0; i < headers.Length; i++)
            {
                Border headerBorder = new Border { Background = new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0)), Padding = new Thickness(10.0) };
                headerBorder.Child = new TextBlock { Text = headers[i], FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center };
                Grid.SetRow(headerBorder, 0);
                Grid.SetColumn(headerBorder, i);
                actualTaskTableGrid?.Children.Add(headerBorder); // Usa o operador ?. para segurança
            }
            ScrollViewer taskScrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = actualTaskTableGrid };
            if (tarefasViewContainer != null)
            {
                tarefasViewContainer.Child = taskScrollViewer;
            }
        }

        public void NavigateToPage(object? pageContent) // Permite pageContent null
        {
            if (this.FindName("MainContentArea") is ContentControl mainContentArea) // Tenta encontrar MainContentArea
            {
                mainContentArea.Content = pageContent;

                if (pageContent == tarefasViewContainer && tarefasViewContainer != null)
                {
                    ShowContextSpecificControls(true);
                    AtualizarTabelaDeTarefasUI();
                }
                else if (pageContent is Perfil || pageContent is EditarPerfil) // Usa os nomes de classe corretos
                {
                    ShowContextSpecificControls(false);
                }
                else
                {
                    ShowContextSpecificControls(false); // Default para outras páginas ou null
                }
            }
        }

        private void ShowContextSpecificControls(bool show)
        {
            Visibility visibility = show ? Visibility.Visible : Visibility.Collapsed;
            // Tenta encontrar os controlos antes de lhes aceder
            if (this.FindName("TopBarGrid") is Grid topBar) topBar.Visibility = visibility;
            if (this.FindName("SummaryBoxesStackPanel") is StackPanel summaryBoxes) summaryBoxes.Visibility = visibility;
            if (this.FindName("CriarTarefaButton") is Button criarBtn) criarBtn.Visibility = visibility; // Nome do botão Criar Tarefa

            if (show)
            {
                UpdatePlaceholderVisibility();
            }
            else
            {
                if (this.FindName("PlaceholderTextBlock") is TextBlock placeholderTextBlock)
                {
                    placeholderTextBlock.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void UpdatePlaceholderVisibility()
        {
            if (this.FindName("SearchTextBox") is TextBox searchTextBox &&
                this.FindName("PlaceholderTextBlock") is TextBlock placeholderTextBlock &&
                this.FindName("TopBarGrid") is Grid topBarGrid)
            {
                bool isTopBarVisible = (topBarGrid.Visibility == Visibility.Visible);
                placeholderTextBlock.Visibility = (isTopBarVisible && string.IsNullOrEmpty(searchTextBox.Text))
                                                  ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        // Event Handlers para os botões de menu
        private void SearchTextBox_TextChanged_UpdatePlaceholder(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholderVisibility();
            if (this.FindName("MainContentArea") is ContentControl mainContentArea && mainContentArea.Content == tarefasViewContainer)
            {
                AtualizarTabelaDeTarefasUI(); // Poderia adicionar lógica de filtro aqui
            }
        }

        private void MainMenuPerfilButton_Click(object sender, RoutedEventArgs e) { NavigateToPage(new Perfil()); } // Usa o nome da classe Perfil
        private void DashboardButton_Click(object sender, RoutedEventArgs e) { NavigateToPage(tarefasViewContainer); }
        private void AlunosButton_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Alunos Clicado!"); NavigateToPage(null); }
        private void GruposButton_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Grupos Clicado!"); NavigateToPage(null); }
        private void TarefasButton_Click(object sender, RoutedEventArgs e) { NavigateToPage(tarefasViewContainer); }
        private void PautaButton_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Pauta Clicado!"); NavigateToPage(null); }

        // private void ConfiguracoesButton_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Configurações Clicado!"); NavigateToPage(null); } // MÉTODO COMENTADO

        private void CriarTarefaButton_Click(object sender, RoutedEventArgs e)
        {
            Criartarefa criarTarefaWin = new Criartarefa { Owner = this }; // Usa o nome da classe "Criartarefa"
            if (criarTarefaWin.ShowDialog() == true && criarTarefaWin.TarefaCriadaComSucesso)
            {
                if (criarTarefaWin.NovaTarefa != null) // Verifica se NovaTarefa não é null
                {
                    listaDeTarefasPrincipal.Add(criarTarefaWin.NovaTarefa);
                }
                if (this.FindName("MainContentArea") is ContentControl mainContentArea && mainContentArea.Content == tarefasViewContainer)
                {
                    AtualizarTabelaDeTarefasUI();
                }
            }
        }

        private void ApagarTarefaButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button apagarButton && apagarButton.Tag is Tarefa tarefaParaApagar)
            {
                if (MessageBox.Show($"Tem a certeza que deseja apagar a tarefa '{tarefaParaApagar.Nome}'?", "Confirmar Apagar Tarefa", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    listaDeTarefasPrincipal.Remove(tarefaParaApagar);
                    if (this.FindName("MainContentArea") is ContentControl mainContentArea && mainContentArea.Content == tarefasViewContainer)
                    {
                        AtualizarTabelaDeTarefasUI();
                    }
                    MessageBox.Show($"Tarefa '{tarefaParaApagar.Nome}' apagada.", "Tarefa Apagada", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void AtualizarTabelaDeTarefasUI()
        {
            if (actualTaskTableGrid == null) return;

            // Limpar children da grid que são linhas de dados (Grid.GetRow > 0)
            for (int i = actualTaskTableGrid.Children.Count - 1; i >= 0; i--)
            {
                if (Grid.GetRow(actualTaskTableGrid.Children[i]) > 0)
                {
                    actualTaskTableGrid.Children.RemoveAt(i);
                }
            }

            // Limpar RowDefinitions de dados, mantendo a do cabeçalho (idx 0) e a do conteúdo (idx 1 com Height=*)
            while (actualTaskTableGrid.RowDefinitions.Count > 2)
            {
                actualTaskTableGrid.RowDefinitions.RemoveAt(1); // Remove a primeira linha de dados, empurrando as outras para cima
            }

            int rowIndex = 1; // Começa a adicionar dados na linha 1 (depois do cabeçalho)
            foreach (var tarefa in listaDeTarefasPrincipal)
            {
                // Insere uma nova RowDefinition para a tarefa atual ANTES da última RowDefinition (que é a de Height=*)
                actualTaskTableGrid.RowDefinitions.Insert(actualTaskTableGrid.RowDefinitions.Count - 1, new RowDefinition { Height = GridLength.Auto });

                // Adicionar células para a tarefa
                // Coluna 0: Nome
                actualTaskTableGrid.Children.Add(CreateTableCellUI(tarefa.Nome, rowIndex, 0));
                // Coluna 1: Descrição
                actualTaskTableGrid.Children.Add(CreateTableCellUI(tarefa.Descricao, rowIndex, 1));
                // Coluna 2: Prazo
                actualTaskTableGrid.Children.Add(CreateTableCellUI(tarefa.Prazo.ToShortDateString(), rowIndex, 2));
                // Coluna 3: Peso
                actualTaskTableGrid.Children.Add(CreateTableCellUI($"{tarefa.Peso}%", rowIndex, 3));

                // Coluna 4: Ações (Botão Apagar)
                Button apagarButton = new Button { Content = "Apagar", Tag = tarefa, Margin = new Thickness(5.0, 2.0, 5.0, 2.0), Padding = new Thickness(5.0, 2.0, 5.0, 2.0), Foreground = Brushes.Red };
                apagarButton.Click += ApagarTarefaButton_Click;
                Grid.SetRow(apagarButton, rowIndex);
                Grid.SetColumn(apagarButton, 4);
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
                BorderThickness = new Thickness(0.0, 0.0, (column < 3) ? 1.0 : 0.0, 1.0) // Borda direita até à coluna ANTES das ações
            };
            TextBlock content = new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis, TextWrapping = TextWrapping.Wrap };
            cellBorder.Child = content;
            Grid.SetRow(cellBorder, row); Grid.SetColumn(cellBorder, column);
            return cellBorder;
        }
    }
}
