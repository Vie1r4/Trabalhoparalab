using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging; // NECESSÁRIO PARA BitmapImage
using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Shapes;



namespace FinalLab
{
    // Definição da classe Aluno
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
        public static string NomeUtilizadorLogado { get; set; } = Environment.UserName;
        public static string EmailUtilizadorLogado { get; set; } = $"{Environment.UserName.ToLower().Replace(" ", ".")}@exemplo.com";

        // Defina o caminho para a imagem de perfil.
        // Este valor deve ser atualizado pela página de Perfil.
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
            UpdateUserProfilePicture();

            if (tarefasViewContainer != null) NavigateToPage(tarefasViewContainer, "Gestão de Tarefas");

            AtualizarContadoresSumario();
        }

        // Método público para que outras partes da aplicação (ex: Perfil.xaml.cs) possam forçar a atualização
        public void UpdateUserProfilePicture()
        {
            if (this.FindName("UserProfileImageBrush") is ImageBrush imageBrush)
            {
                imageBrush.ImageSource = null; // Limpa a imagem anterior para forçar recarregamento

                if (!string.IsNullOrEmpty(CaminhoFotoUtilizadorLogado) && File.Exists(CaminhoFotoUtilizadorLogado))
                {
                    try
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(CaminhoFotoUtilizadorLogado, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad; // Carrega a imagem e liberta o ficheiro
                        bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache; // Tenta ignorar o cache do disco
                        bitmap.EndInit();

                        imageBrush.ImageSource = bitmap;

                        // Restaurar o Fill da Ellipse se estava com cor de fallback
                        if (this.FindName("UserProfileImageEllipse") is Ellipse ellipse)
                        {
                            ellipse.Fill = imageBrush; // Reatribuir o ImageBrush
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao carregar imagem de perfil: {ex.Message}");
                        imageBrush.ImageSource = null;
                        if (this.FindName("UserProfileImageEllipse") is Ellipse ellipse)
                        {
                            ellipse.Fill = new SolidColorBrush(Colors.LightGray);
                        }
                    }
                }
                else
                {
                    imageBrush.ImageSource = null;
                    if (this.FindName("UserProfileImageEllipse") is Ellipse ellipse)
                    {
                        ellipse.Fill = new SolidColorBrush(Colors.Gainsboro);
                    }
                    // Removido Console.WriteLine para não poluir se for chamado frequentemente
                }
            }
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
        private void SetupTarefasView()
        {
            tarefasViewContainer = new Border { BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(1.0) };
            actualTaskTableGrid = new Grid();
            actualTaskTableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            actualTaskTableGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            actualTaskTableGrid.ColumnDefinitions.Clear();
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70, GridUnitType.Pixel) });
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2.5, GridUnitType.Star) });
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            actualTaskTableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            string[] headers = { "ID", "Título", "Descrição", "Início", "Término", "Peso (%)", "Ações" };
            for (int i = 0; i < headers.Length; i++)
            {
                Border headerBorder = new Border { Background = new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0)), Padding = new Thickness(10, 5, 10, 5) };
                TextBlock headerText = new TextBlock
                {
                    Text = headers[i],
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                if (headers[i] == "Peso (%)" || headers[i] == "Descrição")
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

        private void SetupAlunosView()
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
                Grid.SetRow(headerBorder, 0);
                Grid.SetColumn(headerBorder, i);
                actualAlunosTableGrid?.Children.Add(headerBorder);
            }

            ScrollViewer alunosScrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = actualAlunosTableGrid };
            Grid.SetRow(alunosScrollViewer, 1);
            layoutInternoAlunos.Children.Add(alunosScrollViewer);

            if (alunosViewContainer != null)
            {
                alunosViewContainer.Child = layoutInternoAlunos;
            }
        }
        private void PesquisaAlunosLocalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (placeholderPesquisaAlunosLocal != null && pesquisaAlunosLocalTextBox != null)
            {
                placeholderPesquisaAlunosLocal.Visibility = string.IsNullOrEmpty(pesquisaAlunosLocalTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            }
            AtualizarTabelaDeAlunosUI();
        }

        public void NavigateToPage(object? pageContent, string pageTitle = "Bem-vindo")
        {
            if (this.FindName("MainContentArea") is ContentControl mainContentArea &&
                this.FindName("PageTitleTextBlock") is TextBlock titleTextBlock)
            {
                mainContentArea.Content = pageContent;
                titleTextBlock.Text = pageTitle;

                UpdateUserProfilePicture(); // Tenta atualizar a foto em cada navegação

                bool isTarefasView = pageContent == tarefasViewContainer;
                bool isAlunosView = pageContent == alunosViewContainer;
                bool isPerfilView = pageContent is Perfil;

                ShowContextSpecificControls(isTarefasView, isAlunosView, isPerfilView);

                if (isTarefasView) AtualizarTabelaDeTarefasUI();
                else if (isAlunosView) AtualizarTabelaDeAlunosUI();
            }
        }

        private void ShowContextSpecificControls(bool showTarefasControls, bool showAlunosControls, bool showPerfilControls)
        {
            bool showCommonTopElements = showTarefasControls || showAlunosControls || showPerfilControls;
            Visibility commonVisibility = showCommonTopElements ? Visibility.Visible : Visibility.Collapsed;

            if (this.FindName("TopBarGrid") is Grid topBar) topBar.Visibility = commonVisibility;

            Visibility summaryVisibility = (showTarefasControls || showAlunosControls) ? Visibility.Visible : Visibility.Collapsed;
            if (this.FindName("SummaryBoxesStackPanel") is StackPanel summaryBoxes) summaryBoxes.Visibility = summaryVisibility;

            if (this.FindName("CriarTarefaButton") is Button criarTarefaBtn) criarTarefaBtn.Visibility = showTarefasControls ? Visibility.Visible : Visibility.Collapsed;
            if (this.FindName("AdicionarAlunoButton") is Button adicionarAlunoBtn) adicionarAlunoBtn.Visibility = showAlunosControls ? Visibility.Visible : Visibility.Collapsed;
            if (this.FindName("InserirFicheiroAlunosButton") is Button inserirFicheiroBtn) inserirFicheiroBtn.Visibility = showAlunosControls ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MainMenuPerfilButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Se a sua página Perfil precisa de uma referência à MainWindow para chamar UpdateUserProfilePicture:
                Perfil perfilPage = new Perfil(this); // Crie um construtor em Perfil que aceite MainWindow
                NavigateToPage(perfilPage, "Perfil do Utilizador");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao tentar abrir Perfil: {ex.Message}\nVerifique se Perfil.xaml e Perfil.xaml.cs existem e estão corretos.", "Erro");
            }
        }

        private void AlunosButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(alunosViewContainer, "Gestão de Alunos");
        }
        private void GruposButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Grupos Clicado!");
            NavigateToPage(null, "Grupos");
        }
        private void TarefasButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(tarefasViewContainer, "Gestão de Tarefas");
        }
        private void PautaButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Pauta Clicado!");
            NavigateToPage(null, "Pauta de Avaliação");
        }

        private void CriarTarefaButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Criartarefa criarTarefaWin = new Criartarefa { Owner = this };
                if (criarTarefaWin.ShowDialog() == true && criarTarefaWin.TarefaCriadaComSucesso)
                {
                    if (criarTarefaWin.NovaTarefa != null) listaDeTarefasPrincipal.Add(criarTarefaWin.NovaTarefa);
                    if (this.FindName("MainContentArea") is ContentControl mcc && mcc.Content == tarefasViewContainer) AtualizarTabelaDeTarefasUI();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao tentar abrir Criar Tarefa: {ex.Message}\nVerifique se Criartarefa.xaml/cs e a classe Tarefa estão corretos.", "Erro");
            }
        }

        private void ApagarTarefaButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Tarefa tarefa)
            {
                if (MessageBox.Show($"Tem a certeza que deseja apagar a tarefa '{tarefa.Titulo}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    listaDeTarefasPrincipal.Remove(tarefa);
                    if (this.FindName("MainContentArea") is ContentControl mcc && mcc.Content == tarefasViewContainer) AtualizarTabelaDeTarefasUI();
                    MessageBox.Show($"Tarefa '{tarefa.Titulo}' apagada.", "Apagada", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        private void AtualizarTabelaDeTarefasUI()
        {
            if (actualTaskTableGrid == null) return;

            IEnumerable<Tarefa> tarefasParaExibir = listaDeTarefasPrincipal;

            for (int i = actualTaskTableGrid.Children.Count - 1; i >= 0; i--)
                if (Grid.GetRow(actualTaskTableGrid.Children[i]) > 0) actualTaskTableGrid.Children.RemoveAt(i);
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
                    if (actualTaskTableGrid.RowDefinitions.Count <= rowIndex)
                        actualTaskTableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    actualTaskTableGrid.Children.Add(CreateTableCellUI(tarefa.Id.ToString().Substring(0, Math.Min(8, tarefa.Id.ToString().Length)), rowIndex, 0, actualTaskTableGrid));
                    actualTaskTableGrid.Children.Add(CreateTableCellUI(tarefa.Titulo, rowIndex, 1, actualTaskTableGrid));
                    actualTaskTableGrid.Children.Add(CreateTableCellUI(tarefa.Descricao ?? "-", rowIndex, 2, actualTaskTableGrid));
                    actualTaskTableGrid.Children.Add(CreateTableCellUI(tarefa.DataHoraInicio.ToString("g"), rowIndex, 3, actualTaskTableGrid));
                    actualTaskTableGrid.Children.Add(CreateTableCellUI(tarefa.DataHoraTermino.ToString("g"), rowIndex, 4, actualTaskTableGrid));
                    actualTaskTableGrid.Children.Add(CreateTableCellUI($"{tarefa.Peso}", rowIndex, 5, actualTaskTableGrid));

                    Button btnApagar = new Button { Content = "Apagar", Tag = tarefa, Margin = new Thickness(5, 2, 5, 2), Padding = new Thickness(5, 2, 5, 2), Foreground = Brushes.Red };
                    btnApagar.Click += ApagarTarefaButton_Click;
                    Grid.SetRow(btnApagar, rowIndex); Grid.SetColumn(btnApagar, 6);
                    actualTaskTableGrid.Children.Add(btnApagar);
                    rowIndex++;
                }
            }
        }

        private void AdicionarAlunoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AdicionarAluno adicionarAlunoWin = new AdicionarAluno { Owner = this };
                if (adicionarAlunoWin.ShowDialog() == true && adicionarAlunoWin.AlunoAdicionadoComSucesso)
                {
                    if (adicionarAlunoWin.NovoAluno != null) listaDeAlunosPrincipal.Add(adicionarAlunoWin.NovoAluno);
                    AtualizarTabelaDeAlunosUI(); // Deveria ser AtualizarTabelaDeALUNOSUI aqui
                    AtualizarContadoresSumario();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao tentar abrir Adicionar Aluno: {ex.Message}\nVerifique se AdicionarAluno.xaml e AdicionarAluno.xaml.cs existem e estão corretos.", "Erro");
            }
        }

        private void InserirFicheiroAlunosButton_Click(object sender, RoutedEventArgs e)
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
                    if (numAlunosDepois > numAlunosAntes) { AtualizarTabelaDeAlunosUI(); AtualizarContadoresSumario(); } // Corrigido
                    MessageBox.Show($"Importação de alunos concluída. {numAlunosDepois - numAlunosAntes} alunos adicionados. Verifique a tabela e a janela 'Output' para detalhes.", "Importação", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex) { MessageBox.Show($"Ocorreu um erro ao processar o ficheiro: {ex.Message}", "Erro de Importação", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }
        private void ProcessarCSV(string filePath)
        {
            var linhas = File.ReadAllLines(filePath, System.Text.Encoding.UTF8); bool cabecalhoIgnorado = false;
            int alunosAdicionadosComSucesso = 0; int linhaNumeroAtual = 0;
            Console.WriteLine($"--- Iniciando processamento do CSV: {filePath} ---");
            foreach (var linhaOriginal in linhas)
            {
                linhaNumeroAtual++; string linhaLimpa = linhaOriginal.Trim();
                if (string.IsNullOrWhiteSpace(linhaLimpa)) { Console.WriteLine($"Linha {linhaNumeroAtual} ignorada (vazia)."); continue; }
                if (!cabecalhoIgnorado) { cabecalhoIgnorado = true; Console.WriteLine($"Linha {linhaNumeroAtual} (Cabeçalho) ignorada: \"{linhaLimpa}\""); continue; }
                var colunas = linhaLimpa.Split(',');
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

        private void ApagarAlunoButton_Click(object sender, RoutedEventArgs e)
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

        private void AtualizarTabelaDeAlunosUI()
        {
            if (actualAlunosTableGrid == null) return;
            string termoPesquisaLocal = pesquisaAlunosLocalTextBox?.Text.Trim().ToLower() ?? "";
            IEnumerable<Aluno> alunosParaExibir = listaDeAlunosPrincipal;
            if (!string.IsNullOrEmpty(termoPesquisaLocal))
            {
                alunosParaExibir = listaDeAlunosPrincipal.Where(a =>
                    (a.NomeCompleto?.ToLower().Contains(termoPesquisaLocal) ?? false) || (a.NumeroAluno?.ToLower().Contains(termoPesquisaLocal) ?? false) ||
                    (a.Email?.ToLower().Contains(termoPesquisaLocal) ?? false) || (a.Grupo?.ToLower().Contains(termoPesquisaLocal) ?? false));
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
        private Border CreateTableCellUI(string text, int row, int column, Grid targetGrid)
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
