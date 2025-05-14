using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32; // Para OpenFileDialog
using System.IO; // Para System.IO.File e Path
// using System.Text.RegularExpressions; // Não é estritamente necessário aqui, mas pode ter estado em uso

namespace FinalLab
{
    // Definição da classe Aluno (como antes)
    public class Aluno
    {
        public string NomeCompleto { get; }
        public string NumeroAluno { get; }
        public string Email { get; }
        public string? Grupo { get; set; }

        public Aluno(string nomeCompleto, string numeroAluno, string email, string? grupo = null)
        {
            NomeCompleto = nomeCompleto;
            NumeroAluno = numeroAluno;
            Email = email;
            Grupo = grupo ?? "Sem Grupo";
        }
    }

    public partial class MainWindow : Window
    {
        // ... (propriedades estáticas e listas como antes) ...
        public static string NomeUtilizadorLogado { get; set; } = "Utilizador Padrão";
        public static string EmailUtilizadorLogado { get; set; } = "exemplo@email.com";
        public static string? CaminhoFotoUtilizadorLogado { get; set; }

        private List<Tarefa> listaDeTarefasPrincipal = new List<Tarefa>();
        private Border? tarefasViewContainer;
        private Grid? actualTaskTableGrid;

        private List<Aluno> listaDeAlunosPrincipal = new List<Aluno>();
        private Border? alunosViewContainer;
        private Grid? actualAlunosTableGrid;

        public MainWindow()
        {
            InitializeComponent();
            SetupTarefasView();
            SetupAlunosView();

            if (tarefasViewContainer != null) NavigateToPage(tarefasViewContainer);

            UpdatePlaceholderVisibility();
            UpdateTopBarUserName();
        }

        // ... (UpdateTopBarUserName, SetupTarefasView, SetupAlunosView como antes) ...
        public void UpdateTopBarUserName()
        {
            if (this.FindName("TopBarUserNameTextBlock") is TextBlock userNameLabel)
            {
                userNameLabel.Text = NomeUtilizadorLogado;
            }
        }

        private void SetupTarefasView()
        {
            tarefasViewContainer = new Border { BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(1.0) };
            actualTaskTableGrid = new Grid();
            actualTaskTableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            actualTaskTableGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            string[] headers = { "Nome da Tarefa", "Descrição", "Prazo", "Peso", "Ações" };
            for (int i = 0; i < headers.Length; i++)
            {
                Border headerBorder = new Border { Background = new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0)), Padding = new Thickness(10.0) };
                headerBorder.Child = new TextBlock { Text = headers[i], FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center };
                Grid.SetRow(headerBorder, 0);
                Grid.SetColumn(headerBorder, i);
                actualTaskTableGrid?.Children.Add(headerBorder);
            }
            ScrollViewer taskScrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = actualTaskTableGrid };
            if (tarefasViewContainer != null)
            {
                tarefasViewContainer.Child = taskScrollViewer;
            }
        }

        private void SetupAlunosView()
        {
            alunosViewContainer = new Border { BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(1.0) };
            actualAlunosTableGrid = new Grid();
            actualAlunosTableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            actualAlunosTableGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            actualAlunosTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            actualAlunosTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            actualAlunosTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            actualAlunosTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
            actualAlunosTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            string[] headers = { "Nome Completo", "Nº Aluno", "Email", "Grupo", "Ações" };
            for (int i = 0; i < headers.Length; i++)
            {
                Border headerBorder = new Border { Background = new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0)), Padding = new Thickness(10.0) };
                headerBorder.Child = new TextBlock { Text = headers[i], FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center };
                Grid.SetRow(headerBorder, 0);
                Grid.SetColumn(headerBorder, i);
                actualAlunosTableGrid?.Children.Add(headerBorder);
            }
            ScrollViewer alunosScrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = actualAlunosTableGrid };
            if (alunosViewContainer != null)
            {
                alunosViewContainer.Child = alunosScrollViewer;
            }
        }

        public void NavigateToPage(object? pageContent)
        {
            if (this.FindName("MainContentArea") is ContentControl mainContentArea)
            {
                mainContentArea.Content = pageContent;

                bool isTarefasView = pageContent == tarefasViewContainer;
                bool isAlunosView = pageContent == alunosViewContainer;
                bool isPerfilView = pageContent is Perfil;

                ShowContextSpecificControls(isTarefasView, isAlunosView, isPerfilView);

                if (isTarefasView)
                {
                    AtualizarTabelaDeTarefasUI();
                }
                else if (isAlunosView)
                {
                    AtualizarTabelaDeAlunosUI();
                }
            }
        }

        private void ShowContextSpecificControls(bool showTarefasControls, bool showAlunosControls, bool showPerfilControls)
        {
            bool showCommonTopElements = showTarefasControls || showAlunosControls;
            Visibility commonVisibility = showCommonTopElements ? Visibility.Visible : Visibility.Collapsed;

            if (this.FindName("TopBarGrid") is Grid topBar) topBar.Visibility = commonVisibility;
            if (this.FindName("SummaryBoxesStackPanel") is StackPanel summaryBoxes) summaryBoxes.Visibility = commonVisibility;

            if (this.FindName("CriarTarefaButton") is Button criarTarefaBtn)
                criarTarefaBtn.Visibility = showTarefasControls ? Visibility.Visible : Visibility.Collapsed;

            if (this.FindName("AdicionarAlunoButton") is Button adicionarAlunoBtn)
                adicionarAlunoBtn.Visibility = showAlunosControls ? Visibility.Visible : Visibility.Collapsed;
            if (this.FindName("InserirFicheiroAlunosButton") is Button inserirFicheiroBtn)
                inserirFicheiroBtn.Visibility = showAlunosControls ? Visibility.Visible : Visibility.Collapsed;

            if (showCommonTopElements) UpdatePlaceholderVisibility();
            else if (this.FindName("PlaceholderTextBlock") is TextBlock placeholderTextBlock) placeholderTextBlock.Visibility = Visibility.Collapsed;
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

        private void SearchTextBox_TextChanged_UpdatePlaceholder(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholderVisibility();
            if (this.FindName("MainContentArea") is ContentControl mainContentArea)
            {
                if (mainContentArea.Content == tarefasViewContainer)
                {
                    AtualizarTabelaDeTarefasUI();
                }
                else if (mainContentArea.Content == alunosViewContainer)
                {
                    AtualizarTabelaDeAlunosUI();
                }
            }
        }

        private void MainMenuPerfilButton_Click(object sender, RoutedEventArgs e) { NavigateToPage(new Perfil()); }
        private void DashboardButton_Click(object sender, RoutedEventArgs e) { NavigateToPage(tarefasViewContainer); }
        private void AlunosButton_Click(object sender, RoutedEventArgs e) { NavigateToPage(alunosViewContainer); }
        private void GruposButton_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Grupos Clicado!"); NavigateToPage(null); }
        private void TarefasButton_Click(object sender, RoutedEventArgs e) { NavigateToPage(tarefasViewContainer); }
        private void PautaButton_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Pauta Clicado!"); NavigateToPage(null); }

        private void CriarTarefaButton_Click(object sender, RoutedEventArgs e)
        {
            Criartarefa criarTarefaWin = new Criartarefa { Owner = this };
            if (criarTarefaWin.ShowDialog() == true && criarTarefaWin.TarefaCriadaComSucesso)
            {
                if (criarTarefaWin.NovaTarefa != null)
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
                actualTaskTableGrid.RowDefinitions.Insert(actualTaskTableGrid.RowDefinitions.Count - 1, new RowDefinition { Height = GridLength.Auto });

                actualTaskTableGrid.Children.Add(CreateTableCellUI(tarefa.Nome, rowIndex, 0, actualTaskTableGrid));
                actualTaskTableGrid.Children.Add(CreateTableCellUI(tarefa.Descricao, rowIndex, 1, actualTaskTableGrid));
                actualTaskTableGrid.Children.Add(CreateTableCellUI(tarefa.Prazo.ToShortDateString(), rowIndex, 2, actualTaskTableGrid));
                actualTaskTableGrid.Children.Add(CreateTableCellUI($"{tarefa.Peso}%", rowIndex, 3, actualTaskTableGrid));

                Button apagarButton = new Button { Content = "Apagar", Tag = tarefa, Margin = new Thickness(5.0, 2.0, 5.0, 2.0), Padding = new Thickness(5.0, 2.0, 5.0, 2.0), Foreground = Brushes.Red };
                apagarButton.Click += ApagarTarefaButton_Click;
                Grid.SetRow(apagarButton, rowIndex);
                Grid.SetColumn(apagarButton, 4);
                actualTaskTableGrid.Children.Add(apagarButton);

                rowIndex++;
            }
        }

        private void AdicionarAlunoButton_Click(object sender, RoutedEventArgs e)
        {
            AdicionarAluno adicionarAlunoWin = new AdicionarAluno { Owner = this };
            if (adicionarAlunoWin.ShowDialog() == true && adicionarAlunoWin.AlunoAdicionadoComSucesso)
            {
                if (adicionarAlunoWin.NovoAluno != null)
                {
                    listaDeAlunosPrincipal.Add(adicionarAlunoWin.NovoAluno);
                }
                if (this.FindName("MainContentArea") is ContentControl mainContentArea && mainContentArea.Content == alunosViewContainer)
                {
                    AtualizarTabelaDeAlunosUI();
                }
            }
        }

        private void InserirFicheiroAlunosButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Selecionar Ficheiro de Alunos",
                Filter = "Ficheiros CSV (*.csv)|*.csv|Ficheiros Excel (*.xlsx)|*.xlsx|Todos os Ficheiros (*.*)|*.*",
                FilterIndex = 1, // CSV selecionado por defeito (se for o segundo item, caso contrário ajuste)
                                 // Se CSV é o primeiro, FilterIndex = 1. Se Excel é o primeiro e CSV o segundo, FilterIndex = 2.
                                 // No nosso filtro, CSV é o primeiro, então FilterIndex = 1 (ou FilterIndex = 2 se quiser XLSX como default).
                                 // Vamos assumir que quer CSV como primeiro e default:
                                 // Filter = "Ficheiros CSV (*.csv)|*.csv|Ficheiros Excel (*.xlsx)|*.xlsx|Todos os Ficheiros (*.*)|*.*",
                                 // FilterIndex = 1,
                DefaultExt = ".csv",
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string fileExtension = Path.GetExtension(filePath).ToLower();

                try
                {
                    if (fileExtension == ".csv")
                    {
                        ProcessarCSV(filePath);
                    }
                    // else if (fileExtension == ".xlsx")
                    // {
                    //     ProcessarXLSX(filePath); // Implementar depois
                    // }
                    else
                    {
                        MessageBox.Show("Formato de ficheiro não suportado. Por favor, selecione um ficheiro .csv ou .xlsx.", "Erro de Formato", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Atualizar a UI após o processamento
                    if (this.FindName("MainContentArea") is ContentControl mainContentArea && mainContentArea.Content == alunosViewContainer)
                    {
                        AtualizarTabelaDeAlunosUI();
                    }
                    MessageBox.Show("Alunos importados com sucesso!", "Importação Concluída", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocorreu um erro ao processar o ficheiro: {ex.Message}", "Erro de Importação", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ProcessarCSV(string filePath)
        {
            var linhas = File.ReadAllLines(filePath);
            bool cabecalhoIgnorado = false;
            int alunosAdicionados = 0;

            foreach (var linha in linhas)
            {
                if (!cabecalhoIgnorado)
                {
                    cabecalhoIgnorado = true;
                    // Opcional: Validar cabeçalho aqui se necessário
                    // Ex: if (!linha.ToLower().Contains("nomecompleto") || ...) throw new Exception("Cabeçalho CSV inválido.");
                    continue; // Pula a linha de cabeçalho
                }

                var colunas = linha.Split(','); // Assume separador por vírgula
                if (colunas.Length >= 3) // Precisa de pelo menos Nome, Número, Email. Grupo é opcional.
                {
                    try
                    {
                        string nome = colunas[0].Trim();
                        string numero = colunas[1].Trim();
                        string email = colunas[2].Trim();
                        // Grupo pode não existir ou estar vazio
                        string? grupo = (colunas.Length > 3 && !string.IsNullOrWhiteSpace(colunas[3])) ? colunas[3].Trim() : null;

                        // Validações básicas (poderia adicionar mais, como formato de email)
                        if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(numero) || string.IsNullOrWhiteSpace(email))
                        {
                            // Logar ou ignorar linha inválida
                            Console.WriteLine($"Linha CSV ignorada (dados em falta): {linha}");
                            continue;
                        }

                        // Opcional: Verificar se aluno já existe pelo número ou email para evitar duplicados
                        // if (listaDeAlunosPrincipal.Any(a => a.NumeroAluno == numero || a.Email == email))
                        // {
                        //    Console.WriteLine($"Aluno duplicado ignorado: {numero} - {email}");
                        //    continue;
                        // }

                        Aluno novoAluno = new Aluno(nome, numero, email, grupo);
                        listaDeAlunosPrincipal.Add(novoAluno);
                        alunosAdicionados++;
                    }
                    catch (Exception ex)
                    {
                        // Logar erro para esta linha específica e continuar se possível
                        Console.WriteLine($"Erro ao processar linha CSV '{linha}': {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"Linha CSV ignorada (formato incorreto): {linha}");
                }
            }
            Console.WriteLine($"{alunosAdicionados} alunos adicionados do CSV.");
        }

        // private void ProcessarXLSX(string filePath)
        // {
        //     // Esta implementação requer uma biblioteca como ExcelDataReader
        //     // Exemplo (precisa instalar ExcelDataReader e ExcelDataReader.DataSet via NuGet):
        //     // System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance); // Necessário para .NET Core/.NET 5+
        //     // using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
        //     // {
        //     //     using (var reader = ExcelReaderFactory.CreateReader(stream))
        //     //     {
        //     //         var result = reader.AsDataSet(new ExcelDataSetConfiguration() {
        //     //             ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
        //     //         });
        //     //         DataTable dataTable = result.Tables[0];
        //     //         foreach (DataRow row in dataTable.Rows)
        //     //         {
        //     //             try
        //     //             {
        //     //                 string nome = row["NomeCompleto"]?.ToString()?.Trim() ?? "";
        //     //                 string numero = row["NumeroAluno"]?.ToString()?.Trim() ?? "";
        //     //                 string email = row["Email"]?.ToString()?.Trim() ?? "";
        //     //                 string? grupo = row["Grupo"]?.ToString()?.Trim();
        //     //                 if (string.IsNullOrWhiteSpace(grupo)) grupo = null;

        //     //                 if (!string.IsNullOrWhiteSpace(nome) && !string.IsNullOrWhiteSpace(numero) && !string.IsNullOrWhiteSpace(email))
        //     //                 {
        //     //                     listaDeAlunosPrincipal.Add(new Aluno(nome, numero, email, grupo));
        //     //                 }
        //     //             }
        //     //             catch (Exception ex) { Console.WriteLine($"Erro ao ler linha XLSX: {ex.Message}"); }
        //     //         }
        //     //     }
        //     // }
        //     MessageBox.Show("Processamento de XLSX ainda não implementado.", "Info");
        // }


        private void ApagarAlunoButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button apagarButton && apagarButton.Tag is Aluno alunoParaApagar)
            {
                if (MessageBox.Show($"Tem a certeza que deseja apagar o aluno '{alunoParaApagar.NomeCompleto}'?", "Confirmar Apagar Aluno", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    listaDeAlunosPrincipal.Remove(alunoParaApagar);
                    if (this.FindName("MainContentArea") is ContentControl mainContentArea && mainContentArea.Content == alunosViewContainer)
                    {
                        AtualizarTabelaDeAlunosUI();
                    }
                    MessageBox.Show($"Aluno '{alunoParaApagar.NomeCompleto}' apagado.", "Aluno Apagado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void AtualizarTabelaDeAlunosUI()
        {
            if (actualAlunosTableGrid == null) return;

            for (int i = actualAlunosTableGrid.Children.Count - 1; i >= 0; i--)
            {
                if (Grid.GetRow(actualAlunosTableGrid.Children[i]) > 0)
                {
                    actualAlunosTableGrid.Children.RemoveAt(i);
                }
            }

            while (actualAlunosTableGrid.RowDefinitions.Count > 2)
            {
                actualAlunosTableGrid.RowDefinitions.RemoveAt(1);
            }

            int rowIndex = 1;
            foreach (var aluno in listaDeAlunosPrincipal)
            {
                actualAlunosTableGrid.RowDefinitions.Insert(actualAlunosTableGrid.RowDefinitions.Count - 1, new RowDefinition { Height = GridLength.Auto });

                actualAlunosTableGrid.Children.Add(CreateTableCellUI(aluno.NomeCompleto, rowIndex, 0, actualAlunosTableGrid));
                actualAlunosTableGrid.Children.Add(CreateTableCellUI(aluno.NumeroAluno, rowIndex, 1, actualAlunosTableGrid));
                actualAlunosTableGrid.Children.Add(CreateTableCellUI(aluno.Email, rowIndex, 2, actualAlunosTableGrid));
                actualAlunosTableGrid.Children.Add(CreateTableCellUI(aluno.Grupo ?? "N/A", rowIndex, 3, actualAlunosTableGrid));

                Button apagarAlunoButton = new Button { Content = "Apagar", Tag = aluno, Margin = new Thickness(5.0, 2.0, 5.0, 2.0), Padding = new Thickness(5.0, 2.0, 5.0, 2.0), Foreground = Brushes.Red };
                apagarAlunoButton.Click += ApagarAlunoButton_Click;
                Grid.SetRow(apagarAlunoButton, rowIndex);
                Grid.SetColumn(apagarAlunoButton, 4);
                actualAlunosTableGrid.Children.Add(apagarAlunoButton);

                rowIndex++;
            }
        }

        private Border CreateTableCellUI(string text, int row, int column, Grid targetGrid)
        {
            Border cellBorder = new Border
            {
                BorderBrush = Brushes.Gainsboro,
                Padding = new Thickness(10.0)
            };

            int totalColumns = targetGrid?.ColumnDefinitions.Count ?? 5;
            cellBorder.BorderThickness = new Thickness(0.0, 0.0, (column < totalColumns - 1) ? 1.0 : 0.0, 1.0);

            TextBlock content = new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis, TextWrapping = TextWrapping.Wrap };
            cellBorder.Child = content;
            Grid.SetRow(cellBorder, row); Grid.SetColumn(cellBorder, column);
            return cellBorder;
        }
    }
}
