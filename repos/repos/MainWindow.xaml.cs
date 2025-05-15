// MainWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FinalLab
{
    // Definição da classe Aluno (como no seu paste.txt)
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

    // A CLASSE TAREFA está definida em Tarefa.cs ou Criartarefa.xaml.cs
    // (Assumindo que esta definição é a correta e está acessível)
    // public class Tarefa { /* ... com Id, Titulo, Descricao, DataHoraInicio, DataHoraTermino, Peso ... */ }


    public partial class MainWindow : Window
    {
        public static string NomeUtilizadorLogado { get; set; } = Environment.UserName;
        public static string EmailUtilizadorLogado { get; set; } = $"{Environment.UserName.ToLower().Replace(" ", ".")}@exemplo.com";
        public static string? CaminhoFotoUtilizadorLogado { get; set; }

        private List<Tarefa> listaDeTarefasPrincipal = new List<Tarefa>();
        private Border? tarefasViewContainer;
        private Grid? actualTaskTableGrid;

        private List<Aluno> listaDeAlunosPrincipal = new List<Aluno>();
        private Border? alunosViewContainer;
        private Grid? actualAlunosTableGrid;
        private TextBox? pesquisaAlunosLocalTextBox;
        private TextBlock? placeholderPesquisaAlunosLocal;

        public MainWindow()
        {
            InitializeComponent();

            SetupTarefasView();
            SetupAlunosView();
            UpdateTopBarUserName();
            if (tarefasViewContainer != null) NavigateToPage(tarefasViewContainer);

            UpdatePlaceholderVisibility();
            AtualizarContadoresSumario();
        }

        private void AtualizarContadoresSumario()
        {
            if (this.FindName("TotalAlunosTextBlock") is TextBlock totalAlunosLabel)
            {
                totalAlunosLabel.Text = $"Total de Alunos: {listaDeAlunosPrincipal.Count}";
            }
        }

        public void UpdateTopBarUserName()
        {
            if (this.FindName("TopBarUserNameTextBlock") is TextBlock userNameLabel)
            {
                userNameLabel.Text = NomeUtilizadorLogado;
            }
        }

        private void SetupTarefasView() // MODIFICADO AQUI
        {
            tarefasViewContainer = new Border { BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(1.0) };
            actualTaskTableGrid = new Grid();
            actualTaskTableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            actualTaskTableGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            actualTaskTableGrid.ColumnDefinitions.Clear();
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) }); // Id
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) }); // Título
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2.0, GridUnitType.Star) }); // Descrição
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) }); // Data Início
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) }); // Data Término
            // AUMENTAR A LARGURA PROPORCIONAL DA COLUNA PESO
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.9, GridUnitType.Star) }); // Peso (%) - Aumentado de 0.6 para 0.9
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });                         // Ações

            string[] headers = { "ID", "Título", "Descrição", "Início", "Término", "Peso (%)", "Ações" };
            for (int i = 0; i < headers.Length; i++)
            {
                Border headerBorder = new Border { Background = new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0)), Padding = new Thickness(10, 5, 10, 5) }; // Ajustado padding
                TextBlock headerText = new TextBlock
                {
                    Text = headers[i],
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center // Para melhor alinhamento vertical
                };

                // Permitir quebra de linha para cabeçalhos que podem ser mais compridos ou para a coluna do Peso
                if (headers[i] == "Descrição" || headers[i] == "Peso (%)")
                {
                    headerText.TextWrapping = TextWrapping.Wrap;
                }

                headerBorder.Child = headerText;
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

        private void SetupAlunosView() // (Como no seu paste.txt)
        {
            alunosViewContainer = new Border { BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(1.0) };
            Grid layoutInternoAlunos = new Grid();
            layoutInternoAlunos.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layoutInternoAlunos.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            Grid searchBarGrid = new Grid { Margin = new Thickness(0, 0, 0, 10), Height = 32 };
            Border searchBarBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0)),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(15)
            };
            searchBarGrid.Children.Add(searchBarBorder);
            StackPanel searchBarContentPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5, 0, 0, 0) };
            TextBlock lupaIcon = new TextBlock
            {
                Text = "🔍",
                FontFamily = new FontFamily("Segoe UI Emoji"),
                FontSize = 14,
                Foreground = Brushes.Gray,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 5, 0)
            };
            searchBarContentPanel.Children.Add(lupaIcon);
            Grid textBoxPlaceholderGrid = new Grid();
            pesquisaAlunosLocalTextBox = new TextBox
            {
                Padding = new Thickness(5, 2, 5, 2),
                VerticalContentAlignment = VerticalAlignment.Center,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                MinWidth = 200
            };
            pesquisaAlunosLocalTextBox.TextChanged += PesquisaAlunosLocalTextBox_TextChanged;
            textBoxPlaceholderGrid.Children.Add(pesquisaAlunosLocalTextBox);
            placeholderPesquisaAlunosLocal = new TextBlock
            {
                Text = "Pesquisar alunos...",
                Foreground = Brushes.Gray,
                IsHitTestVisible = false,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(7, 0, 0, 0),
                Visibility = Visibility.Visible
            };
            textBoxPlaceholderGrid.Children.Add(placeholderPesquisaAlunosLocal);
            searchBarContentPanel.Children.Add(textBoxPlaceholderGrid);
            searchBarGrid.Children.Add(searchBarContentPanel);
            Grid.SetRow(searchBarGrid, 0);
            layoutInternoAlunos.Children.Add(searchBarGrid);
            actualAlunosTableGrid = new Grid();
            actualAlunosTableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
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
                Grid.SetRow(headerBorder, 0); Grid.SetColumn(headerBorder, i);
                actualAlunosTableGrid?.Children.Add(headerBorder);
            }
            ScrollViewer alunosScrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = actualAlunosTableGrid };
            Grid.SetRow(alunosScrollViewer, 1);
            layoutInternoAlunos.Children.Add(alunosScrollViewer);
            if (alunosViewContainer != null) alunosViewContainer.Child = layoutInternoAlunos;
        }
        private void PesquisaAlunosLocalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (placeholderPesquisaAlunosLocal != null && pesquisaAlunosLocalTextBox != null)
            {
                placeholderPesquisaAlunosLocal.Visibility = string.IsNullOrEmpty(pesquisaAlunosLocalTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            }
            AtualizarTabelaDeAlunosUI();
        }

        public void NavigateToPage(object? pageContent)
        {
            if (this.FindName("MainContentArea") is ContentControl mainContentArea)
            {
                mainContentArea.Content = pageContent;
                bool isTarefasView = pageContent == tarefasViewContainer;
                bool isAlunosView = pageContent == alunosViewContainer;
                bool isPerfilView = pageContent is Perfil; // Certifique-se que a classe Perfil existe

                ShowContextSpecificControls(isTarefasView, isAlunosView, isPerfilView);

                if (isTarefasView) AtualizarTabelaDeTarefasUI();
                else if (isAlunosView) AtualizarTabelaDeAlunosUI();
            }
        }

        private void ShowContextSpecificControls(bool showTarefasControls, bool showAlunosControls, bool showPerfilControls)
        {
            bool showCommonTopElements = showTarefasControls || showAlunosControls;
            Visibility commonVisibility = showCommonTopElements ? Visibility.Visible : Visibility.Collapsed;

            if (this.FindName("TopBarGrid") is Grid topBar) topBar.Visibility = commonVisibility;
            if (this.FindName("SummaryBoxesStackPanel") is StackPanel summaryBoxes) summaryBoxes.Visibility = commonVisibility;
            if (this.FindName("CriarTarefaButton") is Button criarTarefaBtn) criarTarefaBtn.Visibility = showTarefasControls ? Visibility.Visible : Visibility.Collapsed;
            if (this.FindName("AdicionarAlunoButton") is Button adicionarAlunoBtn) adicionarAlunoBtn.Visibility = showAlunosControls ? Visibility.Visible : Visibility.Collapsed;
            if (this.FindName("InserirFicheiroAlunosButton") is Button inserirFicheiroBtn) inserirFicheiroBtn.Visibility = showAlunosControls ? Visibility.Visible : Visibility.Collapsed;

            if (showCommonTopElements) UpdatePlaceholderVisibility();
            else if (this.FindName("PlaceholderTextBlock") is TextBlock placeholderGlobal) placeholderGlobal.Visibility = Visibility.Collapsed;
        }

        private void UpdatePlaceholderVisibility()
        {
            if (this.FindName("SearchTextBox") is TextBox searchTextBoxGlobal &&
                this.FindName("PlaceholderTextBlock") is TextBlock placeholderGlobal &&
                this.FindName("TopBarGrid") is Grid topBarGrid)
            {
                placeholderGlobal.Visibility = (topBarGrid.Visibility == Visibility.Visible && string.IsNullOrEmpty(searchTextBoxGlobal.Text))
                                                  ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void SearchTextBox_TextChanged_UpdatePlaceholder(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholderVisibility();
            if (this.FindName("MainContentArea") is ContentControl mainContentArea)
            {
                if (mainContentArea.Content == tarefasViewContainer) AtualizarTabelaDeTarefasUI();
                else if (mainContentArea.Content == alunosViewContainer) AtualizarTabelaDeAlunosUI();
            }
        }
        private void MainMenuPerfilButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigateToPage(new Perfil()); // Certifique-se que a classe Perfil existe
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao tentar abrir Perfil: {ex.Message}\nVerifique se Perfil.xaml e Perfil.xaml.cs existem e estão corretos.", "Erro");
            }
        }
        private void DashboardButton_Click(object sender, RoutedEventArgs e) { NavigateToPage(tarefasViewContainer); }
        private void AlunosButton_Click(object sender, RoutedEventArgs e) { NavigateToPage(alunosViewContainer); }
        private void GruposButton_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Grupos Clicado!"); NavigateToPage(null); }
        private void TarefasButton_Click(object sender, RoutedEventArgs e) { NavigateToPage(tarefasViewContainer); }
        private void PautaButton_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Pauta Clicado!"); NavigateToPage(null); }

        private void CriarTarefaButton_Click(object sender, RoutedEventArgs e) // MODIFICADO
        {
            try
            {
                // Assumindo que Criartarefa.xaml e Criartarefa.xaml.cs foram atualizados
                // para lidar com a nova estrutura da classe Tarefa.
                Criartarefa criarTarefaWin = new Criartarefa { Owner = this };
                if (criarTarefaWin.ShowDialog() == true && criarTarefaWin.TarefaCriadaComSucesso)
                {
                    if (criarTarefaWin.NovaTarefa != null) // NovaTarefa será do tipo Tarefa (a nova definição)
                    {
                        listaDeTarefasPrincipal.Add(criarTarefaWin.NovaTarefa);
                    }
                    if (this.FindName("MainContentArea") is ContentControl mcc && mcc.Content == tarefasViewContainer)
                    {
                        AtualizarTabelaDeTarefasUI();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao tentar abrir Criar Tarefa: {ex.Message}\nVerifique se Criartarefa.xaml/cs e a classe Tarefa estão corretos.", "Erro");
            }
        }

        private void ApagarTarefaButton_Click(object sender, RoutedEventArgs e) // MODIFICADO
        {
            if (sender is Button btn && btn.Tag is Tarefa tarefa) // Usa a nova classe Tarefa
            {
                if (MessageBox.Show($"Tem a certeza que deseja apagar a tarefa '{tarefa.Titulo}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    listaDeTarefasPrincipal.Remove(tarefa);
                    if (this.FindName("MainContentArea") is ContentControl mcc && mcc.Content == tarefasViewContainer)
                    {
                        AtualizarTabelaDeTarefasUI();
                    }
                    MessageBox.Show($"Tarefa '{tarefa.Titulo}' apagada.", "Apagada", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void AtualizarTabelaDeTarefasUI() // MODIFICADO
        {
            if (actualTaskTableGrid == null) return;

            string termoPesquisaGlobal = "";
            if (this.FindName("SearchTextBox") is TextBox searchBoxGlobal) termoPesquisaGlobal = searchBoxGlobal.Text.Trim().ToLower();

            IEnumerable<Tarefa> tarefasParaExibir = listaDeTarefasPrincipal;
            if (!string.IsNullOrEmpty(termoPesquisaGlobal) && (this.FindName("MainContentArea") as ContentControl)?.Content == tarefasViewContainer)
            {
                tarefasParaExibir = listaDeTarefasPrincipal.Where(t =>
                    (t.Titulo?.ToLower().Contains(termoPesquisaGlobal) ?? false) || // Pesquisar por Título
                    (t.Descricao?.ToLower().Contains(termoPesquisaGlobal) ?? false)
                );
            }

            // Limpar tabela (exceto cabeçalhos)
            for (int i = actualTaskTableGrid.Children.Count - 1; i >= 0; i--)
                if (Grid.GetRow(actualTaskTableGrid.Children[i]) > 0) actualTaskTableGrid.Children.RemoveAt(i);
            // Limpar RowDefinitions de dados
            while (actualTaskTableGrid.RowDefinitions.Count > 1) actualTaskTableGrid.RowDefinitions.RemoveAt(1);

            int rowIndex = 1;
            if (!tarefasParaExibir.Any())
            {
                if (actualTaskTableGrid.RowDefinitions.Count <= 1) actualTaskTableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                TextBlock msgVazio = new TextBlock { Text = "Nenhuma tarefa encontrada.", Margin = new Thickness(10), FontStyle = FontStyles.Italic, HorizontalAlignment = HorizontalAlignment.Center };
                Grid.SetRow(msgVazio, 1); Grid.SetColumnSpan(msgVazio, actualTaskTableGrid.ColumnDefinitions.Count);
                actualTaskTableGrid.Children.Add(msgVazio);
            }
            else
            {
                foreach (var tarefa in tarefasParaExibir)
                {
                    if (actualTaskTableGrid.RowDefinitions.Count <= rowIndex) // Garante que a RowDefinition existe
                        actualTaskTableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    // Preencher células com os novos campos da Tarefa
                    actualTaskTableGrid.Children.Add(CreateTableCellUI(tarefa.Id.ToString().Substring(0, Math.Min(8, tarefa.Id.ToString().Length)), rowIndex, 0, actualTaskTableGrid));
                    actualTaskTableGrid.Children.Add(CreateTableCellUI(tarefa.Titulo, rowIndex, 1, actualTaskTableGrid));
                    actualTaskTableGrid.Children.Add(CreateTableCellUI(tarefa.Descricao ?? "", rowIndex, 2, actualTaskTableGrid));
                    actualTaskTableGrid.Children.Add(CreateTableCellUI(tarefa.DataHoraInicio.ToString("g"), rowIndex, 3, actualTaskTableGrid));
                    actualTaskTableGrid.Children.Add(CreateTableCellUI(tarefa.DataHoraTermino.ToString("g"), rowIndex, 4, actualTaskTableGrid));
                    actualTaskTableGrid.Children.Add(CreateTableCellUI($"{tarefa.Peso}", rowIndex, 5, actualTaskTableGrid)); // Removido '%' para mais espaço

                    Button btnApagar = new Button { Content = "Apagar", Tag = tarefa, Margin = new Thickness(5, 2, 5, 2), Padding = new Thickness(5, 2, 5, 2), Foreground = Brushes.Red };
                    btnApagar.Click += ApagarTarefaButton_Click;
                    Grid.SetRow(btnApagar, rowIndex); Grid.SetColumn(btnApagar, 6);
                    actualTaskTableGrid.Children.Add(btnApagar);
                    rowIndex++;
                }
            }
        }

        private void AdicionarAlunoButton_Click(object sender, RoutedEventArgs e) // (Como no seu paste.txt)
        {
            try
            {
                AdicionarAluno adicionarAlunoWin = new AdicionarAluno { Owner = this };
                if (adicionarAlunoWin.ShowDialog() == true && adicionarAlunoWin.AlunoAdicionadoComSucesso)
                {
                    if (adicionarAlunoWin.NovoAluno != null) listaDeAlunosPrincipal.Add(adicionarAlunoWin.NovoAluno);
                    AtualizarTabelaDeAlunosUI();
                    AtualizarContadoresSumario();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao tentar abrir Adicionar Aluno: {ex.Message}\nVerifique se AdicionarAluno.xaml e AdicionarAluno.xaml.cs existem e estão corretos.", "Erro");
            }
        }

        private void InserirFicheiroAlunosButton_Click(object sender, RoutedEventArgs e) // (Como no seu paste.txt)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Selecionar Ficheiro de Alunos (CSV)",
                Filter = "Ficheiros CSV (*.csv)|*.csv|Todos os Ficheiros (*.*)|*.*",
                FilterIndex = 1,
                DefaultExt = ".csv",
                CheckFileExists = true,
                CheckPathExists = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                try
                {
                    int numAlunosAntes = listaDeAlunosPrincipal.Count; ProcessarCSV(filePath);
                    int numAlunosDepois = listaDeAlunosPrincipal.Count;
                    if (numAlunosDepois > numAlunosAntes) { AtualizarTabelaDeAlunosUI(); AtualizarContadoresSumario(); }
                    MessageBox.Show($"Importação de alunos concluída. {numAlunosDepois - numAlunosAntes} alunos adicionados. Verifique a tabela e a janela 'Output' para detalhes.", "Importação", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex) { MessageBox.Show($"Ocorreu um erro ao processar o ficheiro: {ex.Message}", "Erro de Importação", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }
        private void ProcessarCSV(string filePath) // (Como no seu paste.txt)
        {
            var linhas = File.ReadAllLines(filePath, System.Text.Encoding.UTF8); bool cabecalhoIgnorado = false;
            int alunosAdicionadosComSucesso = 0; int linhaNumeroAtual = 0;
            Console.WriteLine($"--- Iniciando processamento do CSV: {filePath} ---");
            foreach (var linhaOriginal in linhas)
            {
                linhaNumeroAtual++; string linhaLimpa = linhaOriginal.Trim();
                if (string.IsNullOrWhiteSpace(linhaLimpa)) { Console.WriteLine($"Linha {linhaNumeroAtual} ignorada (vazia)."); continue; }
                if (!cabecalhoIgnorado) { cabecalhoIgnorado = true; Console.WriteLine($"Linha {linhaNumeroAtual} (Cabeçalho) ignorada: \"{linhaLimpa}\""); continue; }
                var colunas = linhaLimpa.Split(','); // ASSUMINDO VÍRGULA
                Console.WriteLine($"Linha {linhaNumeroAtual} processando: \"{linhaLimpa}\". Colunas encontradas: {colunas.Length}");
                if (colunas.Length >= 3)
                {
                    try
                    {
                        string nome = colunas[0].Trim(); string numero = colunas[1].Trim(); string email = colunas[2].Trim();
                        string? grupo = (colunas.Length > 3 && !string.IsNullOrWhiteSpace(colunas[3])) ? colunas[3].Trim() : null;
                        Console.WriteLine($"  Dados extraídos: Nome='{nome}', Numero='{numero}', Email='{email}', Grupo='{grupo ?? "N/A"}'");
                        if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(numero) || string.IsNullOrWhiteSpace(email))
                        { Console.WriteLine($"  -> Linha {linhaNumeroAtual} IGNORADA (dados essenciais em falta)."); continue; }
                        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        { Console.WriteLine($"  -> Linha {linhaNumeroAtual} IGNORADA (formato de email inválido: '{email}')."); continue; }
                        listaDeAlunosPrincipal.Add(new Aluno(nome, numero, email, grupo)); alunosAdicionadosComSucesso++;
                        Console.WriteLine($"  -> Aluno ADICIONADO: {nome}");
                    }
                    catch (IndexOutOfRangeException) { Console.WriteLine($"  ERRO na Linha {linhaNumeroAtual}: Formato de colunas inesperado. Linha: \"{linhaOriginal}\""); }
                    catch (Exception ex) { Console.WriteLine($"  ERRO na Linha {linhaNumeroAtual}: \"{linhaOriginal}\". Detalhes: {ex.Message}"); }
                }
                else { Console.WriteLine($"Linha {linhaNumeroAtual} IGNORADA (formato incorreto - esperadas >=3 colunas, encontradas: {colunas.Length}): \"{linhaOriginal}\""); }
            }
            Console.WriteLine($"--- Processamento CSV concluído. {alunosAdicionadosComSucesso} alunos adicionados. ---");
        }

        private void ApagarAlunoButton_Click(object sender, RoutedEventArgs e) // (Como no seu paste.txt)
        {
            if (sender is Button btn && btn.Tag is Aluno aluno)
            {
                if (MessageBox.Show($"Tem a certeza que deseja apagar o aluno '{aluno.NomeCompleto}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    listaDeAlunosPrincipal.Remove(aluno); AtualizarTabelaDeAlunosUI(); AtualizarContadoresSumario();
                    MessageBox.Show($"Aluno '{aluno.NomeCompleto}' apagado.", "Apagado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void AtualizarTabelaDeAlunosUI() // (Como no seu paste.txt)
        {
            if (actualAlunosTableGrid == null) return;
            string termoPesquisaLocal = pesquisaAlunosLocalTextBox?.Text.Trim().ToLower() ?? "";
            string termoPesquisaGlobal = "";
            if (this.FindName("SearchTextBox") is TextBox searchBoxGlobal) termoPesquisaGlobal = searchBoxGlobal.Text.Trim().ToLower();
            IEnumerable<Aluno> alunosParaExibir = listaDeAlunosPrincipal;
            if (!string.IsNullOrEmpty(termoPesquisaLocal))
            {
                alunosParaExibir = listaDeAlunosPrincipal.Where(a =>
                    (a.NomeCompleto?.ToLower().Contains(termoPesquisaLocal) ?? false) || (a.NumeroAluno?.ToLower().Contains(termoPesquisaLocal) ?? false) ||
                    (a.Email?.ToLower().Contains(termoPesquisaLocal) ?? false) || (a.Grupo?.ToLower().Contains(termoPesquisaLocal) ?? false));
            }
            else if (!string.IsNullOrEmpty(termoPesquisaGlobal) && (this.FindName("MainContentArea") as ContentControl)?.Content == alunosViewContainer)
            {
                alunosParaExibir = listaDeAlunosPrincipal.Where(a =>
                    (a.NomeCompleto?.ToLower().Contains(termoPesquisaGlobal) ?? false) || (a.NumeroAluno?.ToLower().Contains(termoPesquisaGlobal) ?? false) ||
                    (a.Email?.ToLower().Contains(termoPesquisaGlobal) ?? false) || (a.Grupo?.ToLower().Contains(termoPesquisaGlobal) ?? false));
            }
            for (int i = actualAlunosTableGrid.Children.Count - 1; i >= 0; i--)
                if (Grid.GetRow(actualAlunosTableGrid.Children[i]) > 0) actualAlunosTableGrid.Children.RemoveAt(i);
            while (actualAlunosTableGrid.RowDefinitions.Count > 1) actualAlunosTableGrid.RowDefinitions.RemoveAt(1);
            int rowIndex = 1;
            if (!alunosParaExibir.Any())
            {
                if (actualAlunosTableGrid.RowDefinitions.Count <= 1) actualAlunosTableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                TextBlock msgVazio = new TextBlock { Text = "Nenhum aluno para exibir.", Margin = new Thickness(10), FontStyle = FontStyles.Italic, HorizontalAlignment = HorizontalAlignment.Center };
                Grid.SetRow(msgVazio, 1); Grid.SetColumnSpan(msgVazio, actualAlunosTableGrid.ColumnDefinitions.Count);
                actualAlunosTableGrid.Children.Add(msgVazio);
            }
            else
            {
                foreach (var aluno in alunosParaExibir)
                {
                    if (actualAlunosTableGrid.RowDefinitions.Count <= rowIndex) actualAlunosTableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    actualAlunosTableGrid.Children.Add(CreateTableCellUI(aluno.NomeCompleto, rowIndex, 0, actualAlunosTableGrid));
                    actualAlunosTableGrid.Children.Add(CreateTableCellUI(aluno.NumeroAluno, rowIndex, 1, actualAlunosTableGrid));
                    actualAlunosTableGrid.Children.Add(CreateTableCellUI(aluno.Email, rowIndex, 2, actualAlunosTableGrid));
                    actualAlunosTableGrid.Children.Add(CreateTableCellUI(aluno.Grupo ?? "N/A", rowIndex, 3, actualAlunosTableGrid));
                    Button btnApagar = new Button { Content = "Apagar", Tag = aluno, Margin = new Thickness(5, 2, 5, 2), Padding = new Thickness(5, 2, 5, 2), Foreground = Brushes.Red };
                    btnApagar.Click += ApagarAlunoButton_Click;
                    Grid.SetRow(btnApagar, rowIndex); Grid.SetColumn(btnApagar, 4);
                    actualAlunosTableGrid.Children.Add(btnApagar);
                    rowIndex++;
                }
            }
        }
        private Border CreateTableCellUI(string text, int row, int column, Grid targetGrid) // (Como no seu paste.txt)
        {
            Border cellBorder = new Border { BorderBrush = Brushes.Gainsboro, Padding = new Thickness(10.0) };
            int totalColumns = targetGrid?.ColumnDefinitions.Count ?? 1;
            cellBorder.BorderThickness = new Thickness(0.0, 0.0, (column < totalColumns - 1) ? 1.0 : 0.0, 1.0);
            TextBlock content = new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis, TextWrapping = TextWrapping.Wrap };
            cellBorder.Child = content; Grid.SetRow(cellBorder, row); Grid.SetColumn(cellBorder, column);
            return cellBorder;
        }
    }
}
