// FinalLab/MainWindow.xaml.cs
using FinalLab.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FinalLab
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // ========================================================================
        // --- CAMPOS E PROPRIEDADES PRINCIPAIS
        // ========================================================================

        // Listas de dados principais (carregadas de App.xaml.cs no construtor)
        public List<Aluno> listaDeAlunosPrincipal { get; private set; }
        public List<Grupo> listaDeGruposPrincipal { get; private set; }
        public List<Tarefa> listaDeTarefasPrincipal { get; private set; }
        public List<NotaAlunoTarefa> listaDeNotasPrincipal { get; private set; }

        // Referências para páginas/janelas auxiliares
        private Perfil? _perfilPage;
        private EditarPerfil? _editarPerfilPage;
        private Pauta? _pautaPage;
        private HistogramWindow? _histogramWindow;

        // Referências para elementos de UI dinâmicos (Grids e Filtros)
        private Grid _alunosGrid = default!;
        private TextBox _filtroAlunosTextBox = default!;
        private Grid _tarefasGrid = default!;
        private Grid _gruposGrid = default!;

        // --- Propriedades Estáticas de Utilizador e Perfil ---
        private static string _nomeUtilizadorSistema = Environment.UserName ?? "utilizador.desconhecido";
        private static string _defaultEmail = $"{_nomeUtilizadorSistema.ToLowerInvariant().Replace(" ", "")}@alunos.utad.pt";
        private static string _defaultCaminhoFoto = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "default_profile.png");

        // Backing fields para as propriedades estáticas do perfil.
        private static string _emailUtilizadorLogadoBackingField = _defaultEmail;
        private static string _caminhoFotoUtilizadorLogadoOriginal = _defaultCaminhoFoto;
        private static string _caminhoFotoUtilizadorLogadoBackingField = _defaultCaminhoFoto;
        private static string _nomePerfilEditavelBackingField = _nomeUtilizadorSistema;

        public static string EmailUtilizadorLogado { get => _emailUtilizadorLogadoBackingField; set { string v = value ?? _defaultEmail; if (_emailUtilizadorLogadoBackingField != v) { _emailUtilizadorLogadoBackingField = v; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(EmailUtilizadorLogado))); } } }
        public static string CaminhoFotoUtilizadorLogado { get => _caminhoFotoUtilizadorLogadoBackingField; set { string v = value ?? _caminhoFotoUtilizadorLogadoOriginal; if (_caminhoFotoUtilizadorLogadoBackingField != v) { _caminhoFotoUtilizadorLogadoBackingField = v; _caminhoFotoUtilizadorLogadoOriginal = v; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(CaminhoFotoUtilizadorLogado))); } } }
        public static string NomePerfilEditavel { get => _nomePerfilEditavelBackingField; set { string v = value ?? _nomeUtilizadorSistema; if (_nomePerfilEditavelBackingField != v) { _nomePerfilEditavelBackingField = v; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(NomePerfilEditavel))); } } }
        public static string NomeUtilizadorSistema => _nomeUtilizadorSistema;

        // Eventos para notificação de alteração de propriedades.
        public static event PropertyChangedEventHandler? StaticPropertyChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        // ========================================================================
        // --- CONSTRUTOR E INICIALIZAÇÃO DA JANELA
        // ========================================================================

        public MainWindow()
        {
            InitializeComponent();

            // Carrega as listas de dados globais.
            listaDeAlunosPrincipal = App.Alunos;
            listaDeGruposPrincipal = App.Grupos;
            listaDeTarefasPrincipal = App.Tarefas;
            listaDeNotasPrincipal = App.Notas;

            DataContext = this;

            // Define valores iniciais para os backing fields do perfil.
            _nomePerfilEditavelBackingField = _nomeUtilizadorSistema;
            _emailUtilizadorLogadoBackingField = $"{_nomeUtilizadorSistema.ToLowerInvariant().Replace(" ", "")}@alunos.utad.pt";

            LoadUserProfile(); // Carrega o perfil do utilizador.

            try
            {
                // Configura as vistas (grids) principais.
                SetupAlunosView();
                SetupTarefasView();
                SetupGruposView();

                // Subscreve eventos e atualiza a UI.
                StaticPropertyChanged += OnStaticPropertyChanged;
                UpdateDisplayedUserNameAndProfilePage();
                UpdateUserProfilePictureEverywhere();
                NavigateToPage("Tarefas"); // Página inicial.
                AtualizarContadoresSumario();

                Debug.WriteLine("MainWindow inicializada com sucesso.");
            }
            catch (Exception ex)
            {
                HandleFatalError(ex, "inicialização da MainWindow");
            }
        }

        // ========================================================================
        // --- GESTÃO DE PERFIL DO UTILIZADOR (CARREGAR/SALVAR)
        // ========================================================================

        private void LoadUserProfile()
        {
            Debug.WriteLine("A iniciar LoadUserProfile...");
            Models.Perfil? perfilSalvo = DataStorage.LoadFromFile<Models.Perfil>(AppDataPaths.PerfilFile);

            if (perfilSalvo != null)
            {
                Debug.WriteLine($"Perfil carregado: Nome='{perfilSalvo.Nome}', Email='{perfilSalvo.Email}', Foto='{perfilSalvo.CaminhoFoto}'");
                NomePerfilEditavel = perfilSalvo.Nome ?? _nomeUtilizadorSistema;
                EmailUtilizadorLogado = perfilSalvo.Email ?? _defaultEmail;
                CaminhoFotoUtilizadorLogado = perfilSalvo.CaminhoFoto ?? _defaultCaminhoFoto;
            }
            else
            {
                Debug.WriteLine("Nenhum perfil salvo encontrado ou erro ao carregar. A usar/criar defaults.");
                NomePerfilEditavel = _nomeUtilizadorSistema;
                EmailUtilizadorLogado = _defaultEmail;
                CaminhoFotoUtilizadorLogado = _defaultCaminhoFoto;

                var perfilPadrao = new Models.Perfil(
                    NomePerfilEditavel,
                    EmailUtilizadorLogado,
                    CaminhoFotoUtilizadorLogado
                );
                DataStorage.SaveToFile(AppDataPaths.PerfilFile, perfilPadrao);
                Debug.WriteLine("Perfil padrão (não encontrado anteriormente) criado e salvo.");
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Salva o perfil do utilizador ao fechar a janela.
            Debug.WriteLine("MainWindow_OnClosing: A salvar perfil...");
            var perfilParaSalvar = new Models.Perfil(
                NomePerfilEditavel,
                EmailUtilizadorLogado,
                CaminhoFotoUtilizadorLogado
            );
            DataStorage.SaveToFile(AppDataPaths.PerfilFile, perfilParaSalvar);
            Debug.WriteLine("Perfil salvo ao fechar.");
            base.OnClosing(e);
        }

        // ========================================================================
        // --- ATUALIZAÇÕES DE UI (PERFIL, NAVEGAÇÃO, GERAL)
        // ========================================================================

        private void OnStaticPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Atualiza a UI quando as propriedades estáticas do perfil mudam.
            if (e.PropertyName == nameof(NomePerfilEditavel)) UpdateDisplayedUserNameAndProfilePage();
            else if (e.PropertyName == nameof(CaminhoFotoUtilizadorLogado)) UpdateUserProfilePictureEverywhere();
            else if (e.PropertyName == nameof(EmailUtilizadorLogado)) _perfilPage?.LoadUserProfileData();
        }

        public void UpdateDisplayedUserNameAndProfilePage()
        {
            if (TopBarUserName != null) TopBarUserName.Text = NomePerfilEditavel;
            _perfilPage?.UpdateProfileNameOnPage(NomePerfilEditavel);
        }

        public void UpdateUserProfilePictureEverywhere()
        {
            try
            {
                BitmapImage bmp = new BitmapImage();
                string path = CaminhoFotoUtilizadorLogado;
                if (string.IsNullOrEmpty(path) || !File.Exists(path)) path = _defaultCaminhoFoto;

                if (!File.Exists(path))
                {
                    if (TopBarUserImageBrush != null) TopBarUserImageBrush.ImageSource = null;
                    _perfilPage?.UpdateProfileImageOnPage(null);
                    return;
                }

                bmp.BeginInit();
                bmp.UriSource = new Uri(path, UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();

                if (TopBarUserImageBrush != null) TopBarUserImageBrush.ImageSource = bmp;
                _perfilPage?.UpdateProfileImageOnPage(bmp);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro img perfil: {ex}");
                if (TopBarUserImageBrush != null) TopBarUserImageBrush.ImageSource = null;
                _perfilPage?.UpdateProfileImageOnPage(null);
            }
        }

        public void NavigateToPage(string pageName, UserControl? pageInstance = null)
        {
            try
            {
                UserControl? contentToShow = null;
                HideAllActionButtons();

                switch (pageName.ToLower())
                {
                    case "alunos":
                        PageTitleTextBlock.Text = "Gestão de Alunos";
                        SetupAlunosView();
                        MainContentArea.Content = _alunosGrid;
                        AdicionarAlunoButton.Visibility = Visibility.Visible;
                        break;
                    case "tarefas":
                        PageTitleTextBlock.Text = "Gestão de Tarefas";
                        SetupTarefasView();
                        MainContentArea.Content = _tarefasGrid;
                        CriarTarefaButton.Visibility = Visibility.Visible;
                        break;
                    case "grupos":
                        PageTitleTextBlock.Text = "Gestão de Grupos";
                        SetupGruposView();
                        MainContentArea.Content = _gruposGrid;
                        CriarGrupoButton.Visibility = Visibility.Visible;
                        break;
                    case "perfil do utilizador":
                        _perfilPage = pageInstance as Perfil ?? new Perfil(this);
                        contentToShow = _perfilPage;
                        PageTitleTextBlock.Text = "O Meu Perfil";
                        break;
                    case "editar perfil":
                        _editarPerfilPage = pageInstance as EditarPerfil ?? new EditarPerfil(this);
                        contentToShow = _editarPerfilPage;
                        PageTitleTextBlock.Text = "Editar Perfil";
                        break;
                    case "pauta":
                        _pautaPage = pageInstance as Pauta ?? new Pauta(this);
                        contentToShow = _pautaPage;
                        PageTitleTextBlock.Text = "Pauta de Avaliação";
                        break;
                    default:
                        PageTitleTextBlock.Text = "Página Desconhecida";
                        MainContentArea.Content = new TextBlock { Text = "Conteúdo não encontrado.", Margin = new Thickness(20) };
                        return;
                }

                if (contentToShow != null) MainContentArea.Content = contentToShow;
                Debug.WriteLine($"Navegou para: {pageName}");
            }
            catch (Exception ex)
            {
                HandleClickError(ex, $"NavigateToPage ({pageName})");
            }
        }

        private void HideAllActionButtons()
        {
            AdicionarAlunoButton.Visibility = Visibility.Collapsed;
            CriarTarefaButton.Visibility = Visibility.Collapsed;
            CriarGrupoButton.Visibility = Visibility.Collapsed;
        }

        private void AtualizarContadoresSumario()
        {
            if (TotalAlunosTextBlock != null) TotalAlunosTextBlock.Text = listaDeAlunosPrincipal.Count.ToString();
            if (TotalTarefasTextBlock != null) TotalTarefasTextBlock.Text = listaDeTarefasPrincipal.Count.ToString();
            if (TotalGruposTextBlock != null) TotalGruposTextBlock.Text = listaDeGruposPrincipal.Count.ToString();
        }

        // --- Handlers de Clique para Navegação do Menu Principal ---
        private void MainMenuPerfilButton_Click(object s, RoutedEventArgs e) => NavigateToPage("Perfil do Utilizador");
        private void AlunosButton_Click(object s, RoutedEventArgs e) => NavigateToPage("Alunos");
        private void TarefasButton_Click(object s, RoutedEventArgs e) => NavigateToPage("Tarefas");
        private void GruposButton_Click(object s, RoutedEventArgs e) => NavigateToPage("Grupos");
        private void PautaButton_Click(object s, RoutedEventArgs e)
        {
            try
            {
                _pautaPage = new Pauta(this);
                NavigateToPage("Pauta", _pautaPage);
            }
            catch (Exception ex) { HandleClickError(ex, "PautaButton_Click"); }
        }
        public void HistogramaMenuButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_histogramWindow == null || !_histogramWindow.IsLoaded)
                {
                    _histogramWindow = new HistogramWindow(this) { Owner = this };
                    _histogramWindow.Show();
                }
                else
                {
                    _histogramWindow.Activate();
                }
            }
            catch (Exception ex) { HandleClickError(ex, "Abrir Histograma"); }
        }

        // ========================================================================
        // --- GESTÃO DE ALUNOS (SETUP DA VIEW, CRUD, CSV)
        // ========================================================================

        private void SetupAlunosView()
        {
            if (_alunosGrid == null)
            {
                _alunosGrid = new Grid();
                _alunosGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                _alunosGrid.RowDefinitions.Add(new RowDefinition());

                var spFiltroBotoes = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
                spFiltroBotoes.Children.Add(new TextBlock { Text = "Filtrar:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) });
                _filtroAlunosTextBox = new TextBox { Width = 200 };
                _filtroAlunosTextBox.TextChanged += (s, e) => AtualizarTabelaDeAlunosUI();
                spFiltroBotoes.Children.Add(_filtroAlunosTextBox);

                var btnImportarCsv = new Button { Content = "Importar CSV", Margin = new Thickness(10, 0, 0, 0), Style = (Style)FindResource("ActionButtonStyle") };
                btnImportarCsv.Click += InserirFicheiroAlunosButton_Click;
                spFiltroBotoes.Children.Add(btnImportarCsv);

                Grid.SetRow(spFiltroBotoes, 0);
                _alunosGrid.Children.Add(spFiltroBotoes);

                var scrollViewerAlunos = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
                Grid.SetRow(scrollViewerAlunos, 1);
                _alunosGrid.Children.Add(scrollViewerAlunos);
            }
            AtualizarTabelaDeAlunosUI();
        }

        private void AtualizarTabelaDeAlunosUI()
        {
            if (_alunosGrid == null || !(_alunosGrid.Children.OfType<ScrollViewer>().FirstOrDefault() is ScrollViewer sv)) return;

            var tabelaAlunos = new Grid { Margin = new Thickness(0, 5, 0, 0) };
            sv.Content = tabelaAlunos;

            string[] headers = { "Nome", "Nº Aluno", "Email", "Grupo", "Ações" };
            double[] widths = { 2, 1, 2, 1.5, 0 };
            bool[] isStar = { true, true, true, true, false };

            for (int i = 0; i < headers.Length; i++)
            {
                tabelaAlunos.ColumnDefinitions.Add(new ColumnDefinition { Width = isStar[i] ? new GridLength(widths[i], GridUnitType.Star) : GridLength.Auto });
                tabelaAlunos.Children.Add(CreateTableCellUI(headers[i], true, 0, i));
            }
            tabelaAlunos.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            string filtro = _filtroAlunosTextBox?.Text.ToLowerInvariant() ?? "";
            var alunosFiltrados = listaDeAlunosPrincipal
                .Where(a => string.IsNullOrEmpty(filtro) ||
                            (a.NomeCompleto?.ToLowerInvariant().Contains(filtro) == true) ||
                            (a.NumeroAluno?.ToLowerInvariant().Contains(filtro) == true))
                .OrderBy(a => a.NomeCompleto)
                .ToList();

            int rIdx = 1;
            foreach (var aluno in alunosFiltrados)
            {
                tabelaAlunos.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                tabelaAlunos.Children.Add(CreateTableCellUI(aluno.NomeCompleto ?? "", false, rIdx, 0));
                tabelaAlunos.Children.Add(CreateTableCellUI(aluno.NumeroAluno ?? "", false, rIdx, 1));
                tabelaAlunos.Children.Add(CreateTableCellUI(aluno.Email ?? "", false, rIdx, 2));
                tabelaAlunos.Children.Add(CreateTableCellUI(aluno.Grupo ?? "Sem Grupo", false, rIdx, 3));

                var btnApagar = new Button { Content = "Apagar", Tag = aluno, Margin = new Thickness(2), Style = (Style)FindResource("ActionButtonStyle"), Background = Brushes.IndianRed };
                btnApagar.Click += ApagarAlunoButton_Click;
                var spAcoes = new StackPanel { Orientation = Orientation.Horizontal };
                spAcoes.Children.Add(btnApagar);
                tabelaAlunos.Children.Add(CreateTableCellUI(spAcoes, false, rIdx, 4));
                rIdx++;
            }
            AtualizarContadoresSumario();
        }

        private void AdicionarAlunoButton_Click(object s, RoutedEventArgs e)
        {
            try
            {
                var winAdicionarAluno = new AdicionarAluno { Owner = this };
                if (winAdicionarAluno.ShowDialog() == true && winAdicionarAluno.NovoAluno != null)
                {
                    if (listaDeAlunosPrincipal.Any(a => a.NumeroAluno.Equals(winAdicionarAluno.NovoAluno.NumeroAluno, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show("Aluno já existe.", "Duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    listaDeAlunosPrincipal.Add(winAdicionarAluno.NovoAluno);
                    if (winAdicionarAluno.NovoAluno.Grupo != "Sem Grupo Atribuído")
                    {
                        AssociarAlunoAGrupo(winAdicionarAluno.NovoAluno, winAdicionarAluno.NovoAluno.Grupo);
                    }
                    AtualizarTabelaDeAlunosUI();
                }
            }
            catch (Exception ex) { HandleClickError(ex, "AdicionarAluno"); }
        }

        private void ApagarAlunoButton_Click(object s, RoutedEventArgs e)
        {
            try
            {
                if (s is Button { Tag: Aluno alDel } &&
                    MessageBox.Show($"Apagar '{alDel.NomeCompleto}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    if (!string.IsNullOrWhiteSpace(alDel.Grupo) && alDel.Grupo != "Sem Grupo Atribuído")
                    {
                        listaDeGruposPrincipal.FirstOrDefault(g => g.Nome == alDel.Grupo)?.RemoverAluno(alDel);
                    }
                    listaDeAlunosPrincipal.Remove(alDel);
                    listaDeNotasPrincipal.RemoveAll(n => n.NumeroAluno == alDel.NumeroAluno);
                    AtualizarTabelaDeAlunosUI();
                    AtualizarTabelaDeGruposUI();
                }
            }
            catch (Exception ex) { HandleClickError(ex, "ApagarAluno"); }
        }

        private void InserirFicheiroAlunosButton_Click(object s, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "CSV(*.csv)|*.csv|TXT(*.txt)|*.txt" };
            if (ofd.ShowDialog() == true)
            {
                try { ProcessarCSV(ofd.FileName); }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ERRO CSV:{ex}");
                    MessageBox.Show($"Erro CSV:{ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ProcessarCSV(string filePath)
        {
            Debug.WriteLine($"Iniciando ProcessarCSV para o ficheiro: {filePath}");
            Encoding[] encodingsToTry = { Encoding.UTF8, Encoding.GetEncoding("iso-8859-1"), Encoding.Default };
            string[]? lines = null;
            bool fileReadSuccessfully = false;

            foreach (var encoding in encodingsToTry)
            {
                try
                {
                    lines = File.ReadAllLines(filePath, encoding);
                    if (lines.Length > 0 && lines.Any(line => line.Contains((char)65533)))
                    {
                        lines = null;
                        continue;
                    }
                    fileReadSuccessfully = true;
                    break;
                }
                catch (Exception ex) { Debug.WriteLine($"Exceção ao ler com {encoding.EncodingName}: {ex.Message}"); }
            }

            if (!fileReadSuccessfully || lines == null || lines.Length == 0)
            {
                MessageBox.Show("Não foi possível ler o ficheiro CSV.", "Erro de Leitura", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int alunosAdicionados = 0;
            int linhasIgnoradas = 0;
            List<string> errosDetalhados = new List<string>();
            List<string> avisosDeGrupo = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                {
                    linhasIgnoradas++;
                    errosDetalhados.Add($"Linha {i + 1}: Vazia.");
                    continue;
                }

                try
                {
                    var fields = line.Split(',');
                    if (fields.Length < 3)
                    {
                        errosDetalhados.Add($"Linha {i + 1}: Formato inválido.");
                        linhasIgnoradas++;
                        continue;
                    }
                    string nomeCompleto = fields[0].Trim();
                    string numeroAluno = fields[1].Trim();
                    string email = fields[2].Trim();
                    string? grupoNomeInput = fields.Length > 3 ? fields[3].Trim() : null;
                    if (string.IsNullOrWhiteSpace(grupoNomeInput)) grupoNomeInput = null;

                    if (string.IsNullOrWhiteSpace(nomeCompleto) || string.IsNullOrWhiteSpace(numeroAluno) || string.IsNullOrWhiteSpace(email))
                    {
                        errosDetalhados.Add($"Linha {i + 1}: Campo obrigatório em branco.");
                        linhasIgnoradas++;
                        continue;
                    }
                    if (!Regex.IsMatch(email, @"^al\d{5}@alunos\.utad\.pt$", RegexOptions.IgnoreCase))
                    {
                        errosDetalhados.Add($"Linha {i + 1}: Email '{email}' inválido.");
                        linhasIgnoradas++;
                        continue;
                    }
                    if (listaDeAlunosPrincipal.Any(a => a.NumeroAluno.Equals(numeroAluno, StringComparison.OrdinalIgnoreCase)))
                    {
                        errosDetalhados.Add($"Linha {i + 1}: Nº aluno '{numeroAluno}' duplicado.");
                        linhasIgnoradas++;
                        continue;
                    }

                    string? grupoFinalParaAluno = null;
                    if (!string.IsNullOrWhiteSpace(grupoNomeInput))
                    {
                        if (listaDeGruposPrincipal.Any(g => g.Nome.Equals(grupoNomeInput, StringComparison.OrdinalIgnoreCase)))
                            grupoFinalParaAluno = grupoNomeInput;
                        else
                            avisosDeGrupo.Add($"Linha {i + 1}: Grupo '{grupoNomeInput}' não encontrado. Aluno '{numeroAluno}' adicionado sem grupo.");
                    }

                    Aluno novoAluno = new Aluno(nomeCompleto, numeroAluno, email, grupoFinalParaAluno);
                    listaDeAlunosPrincipal.Add(novoAluno);
                    if (grupoFinalParaAluno != null)
                    {
                        AssociarAlunoAGrupo(novoAluno, grupoFinalParaAluno);
                    }
                    alunosAdicionados++;
                }
                catch (Exception ex)
                {
                    errosDetalhados.Add($"Linha {i + 1}: Erro - {ex.Message}.");
                    linhasIgnoradas++;
                }
            }

            if (alunosAdicionados > 0)
            {
                AtualizarTabelaDeAlunosUI();
                AtualizarTabelaDeGruposUI();
            }

            StringBuilder summary = new StringBuilder();
            summary.AppendLine($"Processamento CSV concluído. Adicionados: {alunosAdicionados}, Ignorados: {linhasIgnoradas}.");
            if (errosDetalhados.Any())
            {
                summary.AppendLine("\nErros:");
                errosDetalhados.Take(10).ToList().ForEach(e => summary.AppendLine(e));
                if (errosDetalhados.Count > 10) summary.AppendLine("...");
            }
            if (avisosDeGrupo.Any())
            {
                summary.AppendLine("\nAvisos de Grupos:");
                avisosDeGrupo.Take(5).ToList().ForEach(a => summary.AppendLine(a));
                if (avisosDeGrupo.Count > 5) summary.AppendLine("...");
            }
            MessageBox.Show(summary.ToString(), "Resultado da Importação CSV", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void AssociarAlunoAGrupo(Aluno aluno, string? nomeGrupoAlvo)
        {
            ArgumentNullException.ThrowIfNull(aluno);
            string grupoAntigo = aluno.Grupo;

            if (string.IsNullOrWhiteSpace(nomeGrupoAlvo) || nomeGrupoAlvo.Equals("Sem Grupo Atribuído", StringComparison.OrdinalIgnoreCase))
            {
                if (grupoAntigo != "Sem Grupo Atribuído" && !string.IsNullOrWhiteSpace(grupoAntigo))
                {
                    listaDeGruposPrincipal.FirstOrDefault(g => g.Nome.Equals(grupoAntigo, StringComparison.OrdinalIgnoreCase))?.RemoverAluno(aluno);
                }
                aluno.Grupo = "Sem Grupo Atribuído";
                return;
            }

            var grupoObj = listaDeGruposPrincipal.FirstOrDefault(g => g.Nome.Equals(nomeGrupoAlvo, StringComparison.OrdinalIgnoreCase));
            if (grupoObj == null)
            {
                if (grupoAntigo != "Sem Grupo Atribuído" && !string.IsNullOrWhiteSpace(grupoAntigo))
                {
                    listaDeGruposPrincipal.FirstOrDefault(g => g.Nome.Equals(grupoAntigo, StringComparison.OrdinalIgnoreCase))?.RemoverAluno(aluno);
                }
                aluno.Grupo = "Sem Grupo Atribuído";
                return;
            }

            if (grupoAntigo.Equals(grupoObj.Nome, StringComparison.OrdinalIgnoreCase))
            {
                if (!grupoObj.AlunosDoGrupo.Any(a => a.NumeroAluno == aluno.NumeroAluno))
                {
                    grupoObj.AdicionarAluno(aluno);
                }
                aluno.Grupo = grupoObj.Nome;
                return;
            }

            if (grupoAntigo != "Sem Grupo Atribuído" && !string.IsNullOrWhiteSpace(grupoAntigo))
            {
                listaDeGruposPrincipal.FirstOrDefault(g => g.Nome.Equals(grupoAntigo, StringComparison.OrdinalIgnoreCase))?.RemoverAluno(aluno);
            }
            grupoObj.AdicionarAluno(aluno);
            aluno.Grupo = grupoObj.Nome;
        }

        // ========================================================================
        // --- GESTÃO DE TAREFAS (SETUP DA VIEW, CRUD)
        // ========================================================================

        private void SetupTarefasView()
        {
            if (_tarefasGrid == null)
            {
                _tarefasGrid = new Grid();
                var sv = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
                _tarefasGrid.Children.Add(sv);
            }
            AtualizarTabelaDeTarefasUI();
        }

        private void AtualizarTabelaDeTarefasUI()
        {
            if (_tarefasGrid == null || !(_tarefasGrid.Children.OfType<ScrollViewer>().FirstOrDefault() is ScrollViewer sv)) return;

            var tabelaTarefas = new Grid { Margin = new Thickness(0, 5, 0, 0) };
            sv.Content = tabelaTarefas;

            string[] headers = { "Título", "Descrição", "Início", "Fim", "Peso(%)", "Ações" };
            double[] widths = { 2, 3, 1, 1, 0.5, 0 };
            bool[] isStar = { true, true, true, true, true, false };

            for (int i = 0; i < headers.Length; i++)
            {
                tabelaTarefas.ColumnDefinitions.Add(new ColumnDefinition { Width = isStar[i] ? new GridLength(widths[i], GridUnitType.Star) : GridLength.Auto });
                tabelaTarefas.Children.Add(CreateTableCellUI(headers[i], true, 0, i));
            }
            tabelaTarefas.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            int rIdx = 1;
            foreach (var tarefa in listaDeTarefasPrincipal.OrderBy(t => t.DataInicio))
            {
                tabelaTarefas.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                tabelaTarefas.Children.Add(CreateTableCellUI(tarefa.Titulo ?? "", false, rIdx, 0));
                tabelaTarefas.Children.Add(CreateTableCellUI(tarefa.Descricao ?? "", false, rIdx, 1));
                tabelaTarefas.Children.Add(CreateTableCellUI(tarefa.DataInicio.ToString("dd/MM/yyyy"), false, rIdx, 2));
                tabelaTarefas.Children.Add(CreateTableCellUI(tarefa.DataTermino.ToString("dd/MM/yyyy"), false, rIdx, 3));
                tabelaTarefas.Children.Add(CreateTableCellUI(tarefa.Peso.ToString(), false, rIdx, 4));

                var btnApagar = new Button { Content = "Apagar", Tag = tarefa, Margin = new Thickness(2), Style = (Style)FindResource("ActionButtonStyle"), Background = Brushes.IndianRed };
                btnApagar.Click += ApagarTarefaButton_Click;
                var spAcoes = new StackPanel { Orientation = Orientation.Horizontal };
                spAcoes.Children.Add(btnApagar);
                tabelaTarefas.Children.Add(CreateTableCellUI(spAcoes, false, rIdx, 5));
                rIdx++;
            }
            AtualizarContadoresSumario();
        }

        private void CriarTarefaButton_Click(object s, RoutedEventArgs e)
        {
            try
            {
                var winCriarTarefa = new Criartarefa { Owner = this };
                if (winCriarTarefa.ShowDialog() == true && winCriarTarefa.NovaTarefa != null)
                {
                    listaDeTarefasPrincipal.Add(winCriarTarefa.NovaTarefa);
                    AtualizarTabelaDeTarefasUI();
                }
            }
            catch (Exception ex) { HandleClickError(ex, "CriarTarefa"); }
        }

        private void ApagarTarefaButton_Click(object s, RoutedEventArgs e)
        {
            try
            {
                if (s is Button { Tag: Tarefa tarDel } &&
                    MessageBox.Show($"Apagar '{tarDel.Titulo}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    listaDeTarefasPrincipal.Remove(tarDel);
                    listaDeNotasPrincipal.RemoveAll(n => n.IdTarefa == tarDel.Id);
                    AtualizarTabelaDeTarefasUI();
                }
            }
            catch (Exception ex) { HandleClickError(ex, "ApagarTarefa"); }
        }

        // ========================================================================
        // --- GESTÃO DE GRUPOS (SETUP DA VIEW, CRUD)
        // ========================================================================

        private void SetupGruposView()
        {
            if (_gruposGrid == null)
            {
                _gruposGrid = new Grid();
                var sv = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
                _gruposGrid.Children.Add(sv);
            }
            AtualizarTabelaDeGruposUI();
        }

        private void AtualizarTabelaDeGruposUI()
        {
            if (_gruposGrid == null || !(_gruposGrid.Children.OfType<ScrollViewer>().FirstOrDefault() is ScrollViewer sv)) return;

            var tabelaGrupos = new Grid { Margin = new Thickness(0, 5, 0, 0) };
            sv.Content = tabelaGrupos;

            string[] headers = { "Nome Grupo", "Alunos", "Ações" };
            double[] widths = { 1.5, 3, 0 };
            bool[] isStar = { true, true, false };

            for (int i = 0; i < headers.Length; i++)
            {
                tabelaGrupos.ColumnDefinitions.Add(new ColumnDefinition { Width = isStar[i] ? new GridLength(widths[i], GridUnitType.Star) : GridLength.Auto });
                tabelaGrupos.Children.Add(CreateTableCellUI(headers[i], true, 0, i));
            }
            tabelaGrupos.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            int rIdx = 1;
            foreach (var grupo in listaDeGruposPrincipal.OrderBy(g => g.Nome))
            {
                tabelaGrupos.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                tabelaGrupos.Children.Add(CreateTableCellUI(grupo.Nome ?? "", false, rIdx, 0));

                string alunosDoGrupoStr = string.Join(Environment.NewLine, grupo.AlunosDoGrupo.Select(a => a.NomeCompleto ?? "N/A").OrderBy(n => n));
                if (string.IsNullOrEmpty(alunosDoGrupoStr)) alunosDoGrupoStr = "(Vazio)";
                var tbAlunos = new TextBlock { Text = alunosDoGrupoStr, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Top, Padding = new Thickness(5) };
                tabelaGrupos.Children.Add(CreateTableCellUI(tbAlunos, false, rIdx, 1));

                var btnEditar = new Button { Content = "Editar", Tag = grupo, Margin = new Thickness(2), Style = (Style)FindResource("ActionButtonStyle") };
                btnEditar.Click += EditarGrupoButton_Click;
                var btnApagar = new Button { Content = "Apagar", Tag = grupo, Margin = new Thickness(2), Style = (Style)FindResource("ActionButtonStyle"), Background = Brushes.IndianRed };
                btnApagar.Click += ApagarGrupoButton_Click;

                var spAcoes = new StackPanel { Orientation = Orientation.Horizontal };
                spAcoes.Children.Add(btnEditar);
                spAcoes.Children.Add(btnApagar);
                tabelaGrupos.Children.Add(CreateTableCellUI(spAcoes, false, rIdx, 2));
                rIdx++;
            }
            AtualizarContadoresSumario();
        }

        private void CriarGrupoButton_Global_Click(object s, RoutedEventArgs e)
        {
            try
            {
                var winCriarGrupo = new CriarGrupoWindow(new List<Aluno>(listaDeAlunosPrincipal)) { Owner = this };
                if (winCriarGrupo.ShowDialog() == true && winCriarGrupo.GrupoCriadoEditado != null)
                {
                    Grupo novoGrupo = winCriarGrupo.GrupoCriadoEditado;
                    if (listaDeGruposPrincipal.Any(g => g.Nome.Equals(novoGrupo.Nome, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show("Grupo já existe.", "Duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    listaDeGruposPrincipal.Add(novoGrupo);
                    foreach (var aluno in novoGrupo.AlunosDoGrupo)
                    {
                        Aluno? alunoPrincipal = listaDeAlunosPrincipal.FirstOrDefault(a => a.NumeroAluno == aluno.NumeroAluno);
                        if (alunoPrincipal != null)
                        {
                            AssociarAlunoAGrupo(alunoPrincipal, novoGrupo.Nome);
                        }
                    }
                    AtualizarTabelaDeGruposUI();
                    AtualizarTabelaDeAlunosUI();
                }
            }
            catch (Exception ex) { HandleClickError(ex, "CriarGrupo"); }
        }

        private void EditarGrupoButton_Click(object s, RoutedEventArgs e)
        {
            try
            {
                if (s is Button { Tag: Grupo grpOrig })
                {
                    var winEditarGrupo = new CriarGrupoWindow(
                        new Grupo(grpOrig.Id, grpOrig.Nome, grpOrig.AlunosDoGrupo.ToList()), // Passa cópia para edição
                        new List<Aluno>(listaDeAlunosPrincipal)
                    )
                    { Owner = this, Title = "Editar Grupo" };

                    if (winEditarGrupo.ShowDialog() == true && winEditarGrupo.GrupoCriadoEditado != null)
                    {
                        Grupo grpEdit = winEditarGrupo.GrupoCriadoEditado;
                        if (!grpOrig.Nome.Equals(grpEdit.Nome, StringComparison.OrdinalIgnoreCase) &&
                            listaDeGruposPrincipal.Any(g => g.Id != grpOrig.Id && g.Nome.Equals(grpEdit.Nome, StringComparison.OrdinalIgnoreCase)))
                        {
                            MessageBox.Show("Nome de grupo já existe.", "Duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        grpOrig.Nome = grpEdit.Nome; // Atualiza nome

                        // Alunos que saíram
                        var alsSairam = grpOrig.AlunosDoGrupo.Where(aO => !grpEdit.AlunosDoGrupo.Any(aE => aE.NumeroAluno == aO.NumeroAluno)).ToList();
                        foreach (var alS in alsSairam)
                        {
                            AssociarAlunoAGrupo(alS, null);
                        }

                        // Alunos que entraram/permaneceram
                        foreach (var alE in grpEdit.AlunosDoGrupo)
                        {
                            Aluno? alP = listaDeAlunosPrincipal.FirstOrDefault(a => a.NumeroAluno == alE.NumeroAluno);
                            if (alP != null)
                            {
                                AssociarAlunoAGrupo(alP, grpOrig.Nome);
                            }
                        }
                        AtualizarTabelaDeGruposUI();
                        AtualizarTabelaDeAlunosUI();
                    }
                }
            }
            catch (Exception ex) { HandleClickError(ex, "EditarGrupo"); }
        }

        private void ApagarGrupoButton_Click(object s, RoutedEventArgs e)
        {
            try
            {
                if (s is Button { Tag: Grupo grpDel } &&
                    MessageBox.Show($"Apagar '{grpDel.Nome}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    foreach (var aluno in grpDel.AlunosDoGrupo.ToList()) // ToList para evitar modificação durante iteração
                    {
                        AssociarAlunoAGrupo(aluno, null);
                    }
                    listaDeGruposPrincipal.Remove(grpDel);
                    AtualizarTabelaDeGruposUI();
                    AtualizarTabelaDeAlunosUI();
                }
            }
            catch (Exception ex) { HandleClickError(ex, "ApagarGrupo"); }
        }

        // ========================================================================
        // --- UTILITÁRIOS DE UI, TRATAMENTO DE ERROS E INotifyPropertyChanged
        // ========================================================================

        private FrameworkElement CreateTableCellUI(object? content, bool isHeader = false, int r = 0, int c = 0)
        {
            FrameworkElement elementToDisplay;
            if (content is string textContent)
            {
                elementToDisplay = new TextBlock { Text = textContent, Padding = new Thickness(5), VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
            }
            else if (content is FrameworkElement frameworkElementContent)
            {
                elementToDisplay = frameworkElementContent;
                if (elementToDisplay is TextBlock tbMulti && tbMulti.Text.Contains(Environment.NewLine))
                {
                    tbMulti.VerticalAlignment = VerticalAlignment.Top;
                }
                else if (elementToDisplay is TextBlock tbSingle)
                {
                    tbSingle.VerticalAlignment = VerticalAlignment.Center;
                }
            }
            else
            {
                elementToDisplay = new TextBlock { Text = content?.ToString() ?? "", Padding = new Thickness(5), VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
            }

            var border = new Border
            {
                BorderBrush = Brushes.LightGray,
                Child = elementToDisplay,
                Background = isHeader ? Brushes.LightSteelBlue : Brushes.Transparent
            };

            string? headerText = (elementToDisplay as TextBlock)?.Text;
            if (isHeader && headerText == "Ações" && (c == 4 || c == 5 || c == 2))
            {
                border.BorderThickness = new Thickness(0, 0, 0, 1);
            }
            else
            {
                border.BorderThickness = new Thickness(0, 0, 1, 1);
            }

            if (isHeader && elementToDisplay is TextBlock headerTextBlock)
            {
                headerTextBlock.FontWeight = FontWeights.Bold;
                headerTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            }

            Grid.SetRow(border, r);
            Grid.SetColumn(border, c);
            return border;
        }

        private void HandleClickError(Exception ex, string methodName)
        {
            Debug.WriteLine($"ERRO em {methodName}: {ex.Message}\nStackTrace: {ex.StackTrace}");
            MessageBox.Show($"Ocorreu um erro em {methodName}: {ex.Message}", "Erro na Operação", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void HandleFatalError(Exception ex, string context)
        {
            Debug.WriteLine($"ERRO FATAL em {context}: {ex.Message}\nStackTrace: {ex.StackTrace}");
            MessageBox.Show($"Ocorreu um erro crítico ({context}): {ex.Message}\nA aplicação poderá ter de ser encerrada.", "Erro Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
            if (Application.Current != null) Application.Current.Shutdown();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    } // Fim da classe MainWindow
} // Fim do namespace FinalLab
