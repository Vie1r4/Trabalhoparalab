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
        public List<Aluno> listaDeAlunosPrincipal { get; private set; }
        public List<Grupo> listaDeGruposPrincipal { get; private set; }
        public List<Tarefa> listaDeTarefasPrincipal { get; private set; }
        public List<NotaAlunoTarefa> listaDeNotasPrincipal { get; private set; }

        private Perfil? _perfilPage;
        private EditarPerfil? _editarPerfilPage;
        private Pauta? _pautaPage;
        private HistogramWindow? _histogramWindow; // Adicionado para a janela do histograma

        private Grid _alunosGrid = default!;
        private TextBox _filtroAlunosTextBox = default!;
        private Grid _tarefasGrid = default!;
        private Grid _gruposGrid = default!;

        private static string _nomeUtilizadorSistema = Environment.UserName ?? "utilizador.desconhecido";
        private static string _defaultEmail = $"{_nomeUtilizadorSistema.ToLowerInvariant().Replace(" ", "")}@alunos.utad.pt";
        private static string _defaultCaminhoFoto = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "default_profile.png");

        private static string _emailUtilizadorLogadoBackingField = _defaultEmail;
        public static string EmailUtilizadorLogado { get => _emailUtilizadorLogadoBackingField; set { string v = value ?? _defaultEmail; if (_emailUtilizadorLogadoBackingField != v) { _emailUtilizadorLogadoBackingField = v; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(EmailUtilizadorLogado))); } } }

        private static string _caminhoFotoUtilizadorLogadoOriginal = _defaultCaminhoFoto; // Mantém o original para reset
        private static string _caminhoFotoUtilizadorLogadoBackingField = _defaultCaminhoFoto;
        public static string CaminhoFotoUtilizadorLogado { get => _caminhoFotoUtilizadorLogadoBackingField; set { string v = value ?? _caminhoFotoUtilizadorLogadoOriginal; if (_caminhoFotoUtilizadorLogadoBackingField != v) { _caminhoFotoUtilizadorLogadoBackingField = v; _caminhoFotoUtilizadorLogadoOriginal = v; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(CaminhoFotoUtilizadorLogado))); } } }

        private static string _nomePerfilEditavelBackingField = _nomeUtilizadorSistema;
        public static string NomePerfilEditavel { get => _nomePerfilEditavelBackingField; set { string v = value ?? _nomeUtilizadorSistema; if (_nomePerfilEditavelBackingField != v) { _nomePerfilEditavelBackingField = v; StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(NomePerfilEditavel))); } } }

        public static string NomeUtilizadorSistema => _nomeUtilizadorSistema;
        public static event PropertyChangedEventHandler? StaticPropertyChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            listaDeAlunosPrincipal = App.Alunos;
            listaDeGruposPrincipal = App.Grupos;
            listaDeTarefasPrincipal = App.Tarefas; // Inicialização corrigida
            listaDeNotasPrincipal = App.Notas;     // Inicialização corrigida
            DataContext = this;

            _nomePerfilEditavelBackingField = _nomeUtilizadorSistema; // Re-inicializa
            _emailUtilizadorLogadoBackingField = $"{_nomeUtilizadorSistema.ToLowerInvariant().Replace(" ", "")}@alunos.utad.pt"; // Re-inicializa

            try
            {
                SetupAlunosView();
                SetupTarefasView();
                SetupGruposView();

                StaticPropertyChanged += OnStaticPropertyChanged;
                UpdateDisplayedUserNameAndProfilePage();
                UpdateUserProfilePictureEverywhere();
                NavigateToPage("Tarefas");
                AtualizarContadoresSumario();
                Debug.WriteLine("MainWindow inicializada com sucesso.");
            }
            catch (Exception ex) { HandleFatalError(ex, "inicialização da MainWindow"); }
        }

        private void OnStaticPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NomePerfilEditavel)) UpdateDisplayedUserNameAndProfilePage();
            else if (e.PropertyName == nameof(CaminhoFotoUtilizadorLogado)) UpdateUserProfilePictureEverywhere();
            else if (e.PropertyName == nameof(EmailUtilizadorLogado)) _perfilPage?.LoadUserProfileData();
        }

        public void UpdateDisplayedUserNameAndProfilePage() { if (TopBarUserName != null) TopBarUserName.Text = NomePerfilEditavel; _perfilPage?.UpdateProfileNameOnPage(NomePerfilEditavel); }
        public void UpdateUserProfilePictureEverywhere() { try { BitmapImage bmp = new BitmapImage(); string path = CaminhoFotoUtilizadorLogado; if (string.IsNullOrEmpty(path) || !File.Exists(path)) path = _defaultCaminhoFoto; if (!File.Exists(path)) { if (TopBarUserImageBrush != null) TopBarUserImageBrush.ImageSource = null; _perfilPage?.UpdateProfileImageOnPage(null); return; } bmp.BeginInit(); bmp.UriSource = new Uri(path, UriKind.Absolute); bmp.CacheOption = BitmapCacheOption.OnLoad; bmp.EndInit(); if (TopBarUserImageBrush != null) TopBarUserImageBrush.ImageSource = bmp; _perfilPage?.UpdateProfileImageOnPage(bmp); } catch (Exception ex) { Debug.WriteLine($"Erro img perfil: {ex}"); if (TopBarUserImageBrush != null) TopBarUserImageBrush.ImageSource = null; _perfilPage?.UpdateProfileImageOnPage(null); } }

        public void NavigateToPage(string pageName, UserControl? pageInstance = null)
        {
            try
            {
                UserControl? contentToShow = null; HideAllActionButtons();
                switch (pageName.ToLower())
                {
                    case "alunos": PageTitleTextBlock.Text = "Gestão de Alunos"; SetupAlunosView(); MainContentArea.Content = _alunosGrid; AdicionarAlunoButton.Visibility = Visibility.Visible; break;
                    case "tarefas": PageTitleTextBlock.Text = "Gestão de Tarefas"; SetupTarefasView(); MainContentArea.Content = _tarefasGrid; CriarTarefaButton.Visibility = Visibility.Visible; break;
                    case "grupos": PageTitleTextBlock.Text = "Gestão de Grupos"; SetupGruposView(); MainContentArea.Content = _gruposGrid; CriarGrupoButton.Visibility = Visibility.Visible; break;
                    case "perfil do utilizador": _perfilPage = pageInstance as Perfil ?? new Perfil(this); contentToShow = _perfilPage; PageTitleTextBlock.Text = "O Meu Perfil"; break;
                    case "editar perfil": _editarPerfilPage = pageInstance as EditarPerfil ?? new EditarPerfil(this); contentToShow = _editarPerfilPage; PageTitleTextBlock.Text = "Editar Perfil"; break;
                    case "pauta": _pautaPage = pageInstance as Pauta ?? new Pauta(this); contentToShow = _pautaPage; PageTitleTextBlock.Text = "Pauta de Avaliação"; break;
                    default: PageTitleTextBlock.Text = "Página Desconhecida"; MainContentArea.Content = new TextBlock { Text = "Conteúdo não encontrado.", Margin = new Thickness(20) }; return;
                }
                // Corrigido: Não sobrescrever o content de Alunos, Tarefas, Grupos que já foi definido.
                if (contentToShow != null) MainContentArea.Content = contentToShow;
                Debug.WriteLine($"Navegou para: {pageName}");
            }
            catch (Exception ex) { HandleClickError(ex, $"NavigateToPage ({pageName})"); }
        }

        private void HideAllActionButtons() { AdicionarAlunoButton.Visibility = Visibility.Collapsed; CriarTarefaButton.Visibility = Visibility.Collapsed; CriarGrupoButton.Visibility = Visibility.Collapsed; }
        private void MainMenuPerfilButton_Click(object s, RoutedEventArgs e) => NavigateToPage("Perfil do Utilizador");
        private void AlunosButton_Click(object s, RoutedEventArgs e) => NavigateToPage("Alunos");
        private void TarefasButton_Click(object s, RoutedEventArgs e) => NavigateToPage("Tarefas");
        private void GruposButton_Click(object s, RoutedEventArgs e) => NavigateToPage("Grupos");
        private void PautaButton_Click(object s, RoutedEventArgs e) { try { _pautaPage = new Pauta(this); NavigateToPage("Pauta", _pautaPage); } catch (Exception ex) { HandleClickError(ex, "PautaButton_Click"); } }

        // Adicionar o handler para o botão do Histograma que pode estar no menu principal
        public void HistogramaMenuButton_Click(object sender, RoutedEventArgs e) // Tornar público se chamado de Pauta
        {
            try { if (_histogramWindow == null || !_histogramWindow.IsLoaded) { _histogramWindow = new HistogramWindow(this) { Owner = this }; _histogramWindow.Show(); } else _histogramWindow.Activate(); }
            catch (Exception ex) { HandleClickError(ex, "Abrir Histograma"); }
        }

        private void SetupAlunosView() { if (_alunosGrid == null) { _alunosGrid = new Grid(); _alunosGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); _alunosGrid.RowDefinitions.Add(new RowDefinition()); var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) }; sp.Children.Add(new TextBlock { Text = "Filtrar:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) }); _filtroAlunosTextBox = new TextBox { Width = 200 }; _filtroAlunosTextBox.TextChanged += (s, e) => AtualizarTabelaDeAlunosUI(); sp.Children.Add(_filtroAlunosTextBox); var btnCsv = new Button { Content = "Importar CSV", Margin = new Thickness(10, 0, 0, 0), Style = (Style)FindResource("ActionButtonStyle") }; btnCsv.Click += InserirFicheiroAlunosButton_Click; sp.Children.Add(btnCsv); Grid.SetRow(sp, 0); _alunosGrid.Children.Add(sp); var sv = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto }; Grid.SetRow(sv, 1); _alunosGrid.Children.Add(sv); } AtualizarTabelaDeAlunosUI(); }
        private void AtualizarTabelaDeAlunosUI() { if (_alunosGrid == null || !(_alunosGrid.Children.OfType<ScrollViewer>().FirstOrDefault() is ScrollViewer sv)) return; var tab = new Grid { Margin = new Thickness(0, 5, 0, 0) }; sv.Content = tab; string[] headers = { "Nome", "Nº Aluno", "Email", "Grupo", "Ações" }; double[] widths = { 2, 1, 2, 1.5, 0 }; bool[] isStar = { true, true, true, true, false }; for (int i = 0; i < headers.Length; i++) { tab.ColumnDefinitions.Add(new ColumnDefinition { Width = isStar[i] ? new GridLength(widths[i], GridUnitType.Star) : GridLength.Auto }); tab.Children.Add(CreateTableCellUI(headers[i], true, 0, i)); } tab.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); string filtro = _filtroAlunosTextBox?.Text.ToLowerInvariant() ?? ""; var filtrados = listaDeAlunosPrincipal.Where(a => string.IsNullOrEmpty(filtro) || (a.NomeCompleto?.ToLowerInvariant().Contains(filtro) == true) || (a.NumeroAluno?.ToLowerInvariant().Contains(filtro) == true)).OrderBy(a => a.NomeCompleto).ToList(); int rIdx = 1; foreach (var al in filtrados) { tab.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); tab.Children.Add(CreateTableCellUI(al.NomeCompleto ?? "", false, rIdx, 0)); tab.Children.Add(CreateTableCellUI(al.NumeroAluno ?? "", false, rIdx, 1)); tab.Children.Add(CreateTableCellUI(al.Email ?? "", false, rIdx, 2)); tab.Children.Add(CreateTableCellUI(al.Grupo ?? "Sem Grupo", false, rIdx, 3)); var btnDel = new Button { Content = "Apagar", Tag = al, Margin = new Thickness(2), Style = (Style)FindResource("ActionButtonStyle"), Background = Brushes.IndianRed }; btnDel.Click += ApagarAlunoButton_Click; var spAcoes = new StackPanel { Orientation = Orientation.Horizontal }; spAcoes.Children.Add(btnDel); tab.Children.Add(CreateTableCellUI(spAcoes, false, rIdx, 4)); rIdx++; } AtualizarContadoresSumario(); }
        private void AdicionarAlunoButton_Click(object s, RoutedEventArgs e) { try { var win = new AdicionarAluno { Owner = this }; if (win.ShowDialog() == true && win.NovoAluno != null) { if (listaDeAlunosPrincipal.Any(a => a.NumeroAluno.Equals(win.NovoAluno.NumeroAluno, StringComparison.OrdinalIgnoreCase))) { MessageBox.Show("Aluno já existe.", "Duplicado", MessageBoxButton.OK, MessageBoxImage.Warning); return; } listaDeAlunosPrincipal.Add(win.NovoAluno); if (win.NovoAluno.Grupo != "Sem Grupo Atribuído") AssociarAlunoAGrupo(win.NovoAluno, win.NovoAluno.Grupo); AtualizarTabelaDeAlunosUI(); } } catch (Exception ex) { HandleClickError(ex, "AdicionarAluno"); } }
        private void ApagarAlunoButton_Click(object s, RoutedEventArgs e) { try { if (s is Button { Tag: Aluno alDel } && MessageBox.Show($"Apagar '{alDel.NomeCompleto}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) { if (!string.IsNullOrWhiteSpace(alDel.Grupo) && alDel.Grupo != "Sem Grupo Atribuído") listaDeGruposPrincipal.FirstOrDefault(g => g.Nome == alDel.Grupo)?.RemoverAluno(alDel); listaDeAlunosPrincipal.Remove(alDel); listaDeNotasPrincipal.RemoveAll(n => n.NumeroAluno == alDel.NumeroAluno); AtualizarTabelaDeAlunosUI(); AtualizarTabelaDeGruposUI(); } } catch (Exception ex) { HandleClickError(ex, "ApagarAluno"); } }
        private void InserirFicheiroAlunosButton_Click(object s, RoutedEventArgs e) { OpenFileDialog ofd = new OpenFileDialog { Filter = "CSV(*.csv)|*.csv|TXT(*.txt)|*.txt" }; if (ofd.ShowDialog() == true) try { ProcessarCSV(ofd.FileName); } catch (Exception ex) { Debug.WriteLine($"ERRO CSV:{ex}"); MessageBox.Show($"Erro CSV:{ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error); } }

        // CONTINUAÇÃO do MainWindow.xaml.cs (a partir do ProcessarCSV)
        private void ProcessarCSV(string filePath)
        {
            Debug.WriteLine($"Iniciando ProcessarCSV para o ficheiro: {filePath}");
            Encoding[] encodingsToTry = { Encoding.UTF8, Encoding.GetEncoding("iso-8859-1"), Encoding.Default };
            string[]? lines = null; bool fileReadSuccessfully = false;
            foreach (var encoding in encodingsToTry)
            {
                try { lines = File.ReadAllLines(filePath, encoding); if (lines.Length > 0 && lines.Any(line => line.Contains((char)65533))) { lines = null; continue; } fileReadSuccessfully = true; break; }
                catch (Exception ex) { Debug.WriteLine($"Exceção ao ler com {encoding.EncodingName}: {ex.Message}"); }
            }
            if (!fileReadSuccessfully || lines == null || lines.Length == 0) { MessageBox.Show("Não foi possível ler o ficheiro CSV.", "Erro de Leitura", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            int alunosAdicionados = 0; int linhasIgnoradas = 0; List<string> errosDetalhados = new List<string>(); List<string> avisosDeGrupo = new List<string>();
            for (int i = 0; i < lines.Length; i++)
            { // Assumindo que não há cabeçalho ou que ele é tratado/ignorado
                var line = lines[i]; if (string.IsNullOrWhiteSpace(line)) { linhasIgnoradas++; errosDetalhados.Add($"Linha {i + 1}: Vazia."); continue; }
                try
                {
                    var fields = line.Split(','); if (fields.Length < 3) { errosDetalhados.Add($"Linha {i + 1}: Formato inválido."); linhasIgnoradas++; continue; }
                    string nomeCompleto = fields[0].Trim(); string numeroAluno = fields[1].Trim(); string email = fields[2].Trim();
                    string? grupoNomeInput = fields.Length > 3 ? fields[3].Trim() : null; if (string.IsNullOrWhiteSpace(grupoNomeInput)) grupoNomeInput = null;
                    if (string.IsNullOrWhiteSpace(nomeCompleto) || string.IsNullOrWhiteSpace(numeroAluno) || string.IsNullOrWhiteSpace(email)) { errosDetalhados.Add($"Linha {i + 1}: Campo obrigatório em branco."); linhasIgnoradas++; continue; }
                    if (!Regex.IsMatch(email, @"^al\d{5}@alunos\.utad\.pt$", RegexOptions.IgnoreCase)) { errosDetalhados.Add($"Linha {i + 1}: Email '{email}' inválido."); linhasIgnoradas++; continue; }
                    if (listaDeAlunosPrincipal.Any(a => a.NumeroAluno.Equals(numeroAluno, StringComparison.OrdinalIgnoreCase))) { errosDetalhados.Add($"Linha {i + 1}: Nº aluno '{numeroAluno}' duplicado."); linhasIgnoradas++; continue; }
                    string? grupoFinalParaAluno = null;
                    if (!string.IsNullOrWhiteSpace(grupoNomeInput))
                    {
                        if (listaDeGruposPrincipal.Any(g => g.Nome.Equals(grupoNomeInput, StringComparison.OrdinalIgnoreCase))) grupoFinalParaAluno = grupoNomeInput;
                        else avisosDeGrupo.Add($"Linha {i + 1}: Grupo '{grupoNomeInput}' não encontrado. Aluno '{numeroAluno}' adicionado sem grupo.");
                    }
                    Aluno novoAluno = new Aluno(nomeCompleto, numeroAluno, email, grupoFinalParaAluno);
                    listaDeAlunosPrincipal.Add(novoAluno);
                    if (grupoFinalParaAluno != null) AssociarAlunoAGrupo(novoAluno, grupoFinalParaAluno);
                    alunosAdicionados++;
                }
                catch (Exception ex) { errosDetalhados.Add($"Linha {i + 1}: Erro - {ex.Message}."); linhasIgnoradas++; }
            }
            if (alunosAdicionados > 0) { AtualizarTabelaDeAlunosUI(); AtualizarTabelaDeGruposUI(); }
            StringBuilder summary = new StringBuilder(); summary.AppendLine($"Processamento CSV concluído. Adicionados: {alunosAdicionados}, Ignorados: {linhasIgnoradas}.");
            if (errosDetalhados.Any()) { summary.AppendLine("\nErros:"); errosDetalhados.Take(10).ToList().ForEach(e => summary.AppendLine(e)); if (errosDetalhados.Count > 10) summary.AppendLine("..."); }
            if (avisosDeGrupo.Any()) { summary.AppendLine("\nAvisos de Grupos:"); avisosDeGrupo.Take(5).ToList().ForEach(a => summary.AppendLine(a)); if (avisosDeGrupo.Count > 5) summary.AppendLine("..."); }
            MessageBox.Show(summary.ToString(), "Resultado da Importação CSV", MessageBoxButton.OK, MessageBoxImage.Information);
        } // Fim de ProcessarCSV

        public void AssociarAlunoAGrupo(Aluno aluno, string? nomeGrupoAlvo) { ArgumentNullException.ThrowIfNull(aluno); string grupoAntigo = aluno.Grupo; if (string.IsNullOrWhiteSpace(nomeGrupoAlvo) || nomeGrupoAlvo.Equals("Sem Grupo Atribuído", StringComparison.OrdinalIgnoreCase)) { if (grupoAntigo != "Sem Grupo Atribuído" && !string.IsNullOrWhiteSpace(grupoAntigo)) listaDeGruposPrincipal.FirstOrDefault(g => g.Nome.Equals(grupoAntigo, StringComparison.OrdinalIgnoreCase))?.RemoverAluno(aluno); aluno.Grupo = "Sem Grupo Atribuído"; return; } var grupoObj = listaDeGruposPrincipal.FirstOrDefault(g => g.Nome.Equals(nomeGrupoAlvo, StringComparison.OrdinalIgnoreCase)); if (grupoObj == null) { if (grupoAntigo != "Sem Grupo Atribuído" && !string.IsNullOrWhiteSpace(grupoAntigo)) listaDeGruposPrincipal.FirstOrDefault(g => g.Nome.Equals(grupoAntigo, StringComparison.OrdinalIgnoreCase))?.RemoverAluno(aluno); aluno.Grupo = "Sem Grupo Atribuído"; return; } if (grupoAntigo.Equals(grupoObj.Nome, StringComparison.OrdinalIgnoreCase)) { if (!grupoObj.AlunosDoGrupo.Any(a => a.NumeroAluno == aluno.NumeroAluno)) grupoObj.AdicionarAluno(aluno); aluno.Grupo = grupoObj.Nome; return; } if (grupoAntigo != "Sem Grupo Atribuído" && !string.IsNullOrWhiteSpace(grupoAntigo)) listaDeGruposPrincipal.FirstOrDefault(g => g.Nome.Equals(grupoAntigo, StringComparison.OrdinalIgnoreCase))?.RemoverAluno(aluno); grupoObj.AdicionarAluno(aluno); aluno.Grupo = grupoObj.Nome; }

        // Implementações de SetupTarefasView, AtualizarTabelaDeTarefasUI, etc.
        private void SetupTarefasView() { if (_tarefasGrid == null) { _tarefasGrid = new Grid(); var sv = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto }; _tarefasGrid.Children.Add(sv); } AtualizarTabelaDeTarefasUI(); }
        private void AtualizarTabelaDeTarefasUI() { if (_tarefasGrid == null || !(_tarefasGrid.Children.OfType<ScrollViewer>().FirstOrDefault() is ScrollViewer sv)) return; var tab = new Grid { Margin = new Thickness(0, 5, 0, 0) }; sv.Content = tab; string[] headers = { "Título", "Descrição", "Início", "Fim", "Peso(%)", "Ações" }; double[] widths = { 2, 3, 1, 1, 0.5, 0 }; bool[] isStar = { true, true, true, true, true, false }; for (int i = 0; i < headers.Length; i++) { tab.ColumnDefinitions.Add(new ColumnDefinition { Width = isStar[i] ? new GridLength(widths[i], GridUnitType.Star) : GridLength.Auto }); tab.Children.Add(CreateTableCellUI(headers[i], true, 0, i)); } tab.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); int rIdx = 1; foreach (var tar in listaDeTarefasPrincipal.OrderBy(t => t.DataInicio)) { tab.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); tab.Children.Add(CreateTableCellUI(tar.Titulo ?? "", false, rIdx, 0)); tab.Children.Add(CreateTableCellUI(tar.Descricao ?? "", false, rIdx, 1)); tab.Children.Add(CreateTableCellUI(tar.DataInicio.ToString("dd/MM/yyyy"), false, rIdx, 2)); tab.Children.Add(CreateTableCellUI(tar.DataTermino.ToString("dd/MM/yyyy"), false, rIdx, 3)); tab.Children.Add(CreateTableCellUI(tar.Peso.ToString(), false, rIdx, 4)); var btnDel = new Button { Content = "Apagar", Tag = tar, Margin = new Thickness(2), Style = (Style)FindResource("ActionButtonStyle"), Background = Brushes.IndianRed }; btnDel.Click += ApagarTarefaButton_Click; var spAcoes = new StackPanel { Orientation = Orientation.Horizontal }; spAcoes.Children.Add(btnDel); tab.Children.Add(CreateTableCellUI(spAcoes, false, rIdx, 5)); rIdx++; } AtualizarContadoresSumario(); }
        private void CriarTarefaButton_Click(object s, RoutedEventArgs e) { try { var win = new Criartarefa { Owner = this }; if (win.ShowDialog() == true && win.NovaTarefa != null) { listaDeTarefasPrincipal.Add(win.NovaTarefa); AtualizarTabelaDeTarefasUI(); } } catch (Exception ex) { HandleClickError(ex, "CriarTarefa"); } }
        private void ApagarTarefaButton_Click(object s, RoutedEventArgs e) { try { if (s is Button { Tag: Tarefa tarDel } && MessageBox.Show($"Apagar '{tarDel.Titulo}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) { listaDeTarefasPrincipal.Remove(tarDel); listaDeNotasPrincipal.RemoveAll(n => n.IdTarefa == tarDel.Id); AtualizarTabelaDeTarefasUI(); } } catch (Exception ex) { HandleClickError(ex, "ApagarTarefa"); } }

        private void SetupGruposView() { if (_gruposGrid == null) { _gruposGrid = new Grid(); var sv = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto }; _gruposGrid.Children.Add(sv); } AtualizarTabelaDeGruposUI(); }
        private void AtualizarTabelaDeGruposUI() { if (_gruposGrid == null || !(_gruposGrid.Children.OfType<ScrollViewer>().FirstOrDefault() is ScrollViewer sv)) return; var tab = new Grid { Margin = new Thickness(0, 5, 0, 0) }; sv.Content = tab; string[] headers = { "Nome Grupo", "Alunos", "Ações" }; double[] widths = { 1.5, 3, 0 }; bool[] isStar = { true, true, false }; for (int i = 0; i < headers.Length; i++) { tab.ColumnDefinitions.Add(new ColumnDefinition { Width = isStar[i] ? new GridLength(widths[i], GridUnitType.Star) : GridLength.Auto }); tab.Children.Add(CreateTableCellUI(headers[i], true, 0, i)); } tab.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); int rIdx = 1; foreach (var grp in listaDeGruposPrincipal.OrderBy(g => g.Nome)) { tab.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); tab.Children.Add(CreateTableCellUI(grp.Nome ?? "", false, rIdx, 0)); string alunos = string.Join(Environment.NewLine, grp.AlunosDoGrupo.Select(a => a.NomeCompleto ?? "N/A").OrderBy(n => n)); if (string.IsNullOrEmpty(alunos)) alunos = "(Vazio)"; var tbAlunos = new TextBlock { Text = alunos, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Top, Padding = new Thickness(5) }; tab.Children.Add(CreateTableCellUI(tbAlunos, false, rIdx, 1)); var btnEdit = new Button { Content = "Editar", Tag = grp, Margin = new Thickness(2), Style = (Style)FindResource("ActionButtonStyle") }; btnEdit.Click += EditarGrupoButton_Click; var btnDel = new Button { Content = "Apagar", Tag = grp, Margin = new Thickness(2), Style = (Style)FindResource("ActionButtonStyle"), Background = Brushes.IndianRed }; btnDel.Click += ApagarGrupoButton_Click; var spAcoes = new StackPanel { Orientation = Orientation.Horizontal }; spAcoes.Children.Add(btnEdit); spAcoes.Children.Add(btnDel); tab.Children.Add(CreateTableCellUI(spAcoes, false, rIdx, 2)); rIdx++; } AtualizarContadoresSumario(); }
        private void CriarGrupoButton_Global_Click(object s, RoutedEventArgs e) { try { var win = new CriarGrupoWindow(new List<Aluno>(listaDeAlunosPrincipal)) { Owner = this }; if (win.ShowDialog() == true && win.GrupoCriadoEditado != null) { Grupo novoGrp = win.GrupoCriadoEditado; if (listaDeGruposPrincipal.Any(g => g.Nome.Equals(novoGrp.Nome, StringComparison.OrdinalIgnoreCase))) { MessageBox.Show("Grupo já existe.", "Duplicado", MessageBoxButton.OK, MessageBoxImage.Warning); return; } listaDeGruposPrincipal.Add(novoGrp); foreach (var al in novoGrp.AlunosDoGrupo) { Aluno? alPrinc = listaDeAlunosPrincipal.FirstOrDefault(a => a.NumeroAluno == al.NumeroAluno); if (alPrinc != null) AssociarAlunoAGrupo(alPrinc, novoGrp.Nome); } AtualizarTabelaDeGruposUI(); AtualizarTabelaDeAlunosUI(); } } catch (Exception ex) { HandleClickError(ex, "CriarGrupo"); } }
        private void EditarGrupoButton_Click(object s, RoutedEventArgs e)
        {
            try
            {
                if (s is Button { Tag: Grupo grpOrig })
                {
                    var win = new CriarGrupoWindow(
                        new Grupo(grpOrig.Id, grpOrig.Nome, new ObservableCollection<Aluno>(grpOrig.AlunosDoGrupo)),
                        new List<Aluno>(listaDeAlunosPrincipal)
                    )
                    { Owner = this, Title = "Editar Grupo" };

                    if (win.ShowDialog() == true && win.GrupoCriadoEditado != null)
                    {
                        Grupo grpEdit = win.GrupoCriadoEditado;
                        if (!grpOrig.Nome.Equals(grpEdit.Nome, StringComparison.OrdinalIgnoreCase) &&
                            listaDeGruposPrincipal.Any(g => g.Id != grpOrig.Id && g.Nome.Equals(grpEdit.Nome, StringComparison.OrdinalIgnoreCase)))
                        {
                            MessageBox.Show("Nome de grupo já existe.", "Duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        grpOrig.Nome = grpEdit.Nome;
                        var alsSairam = grpOrig.AlunosDoGrupo.Where(aO => !grpEdit.AlunosDoGrupo.Any(aE => aE.NumeroAluno == aO.NumeroAluno)).ToList();
                        foreach (var alS in alsSairam) AssociarAlunoAGrupo(alS, null);
                        foreach (var alE in grpEdit.AlunosDoGrupo)
                        {
                            Aluno? alP = listaDeAlunosPrincipal.FirstOrDefault(a => a.NumeroAluno == alE.NumeroAluno);
                            if (alP != null) AssociarAlunoAGrupo(alP, grpOrig.Nome);
                        }
                        AtualizarTabelaDeGruposUI();
                        AtualizarTabelaDeAlunosUI();
                    }
                }
            }
            catch (Exception ex) { HandleClickError(ex, "EditarGrupo"); }
        }
        private void ApagarGrupoButton_Click(object s, RoutedEventArgs e) { try { if (s is Button { Tag: Grupo grpDel } && MessageBox.Show($"Apagar '{grpDel.Nome}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) { foreach (var al in grpDel.AlunosDoGrupo.ToList()) AssociarAlunoAGrupo(al, null); listaDeGruposPrincipal.Remove(grpDel); AtualizarTabelaDeGruposUI(); AtualizarTabelaDeAlunosUI(); } } catch (Exception ex) { HandleClickError(ex, "ApagarGrupo"); } }

        private FrameworkElement CreateTableCellUI(object? content, bool isHeader = false, int r = 0, int c = 0) { FrameworkElement el; if (content is string txt) el = new TextBlock { Text = txt, Padding = new Thickness(5), VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap }; else if (content is FrameworkElement fe) { el = fe; if (el is TextBlock txb && txb.Text.Contains(Environment.NewLine)) txb.VerticalAlignment = VerticalAlignment.Top; else if (el is TextBlock tb) tb.VerticalAlignment = VerticalAlignment.Center; } else el = new TextBlock { Text = content?.ToString() ?? "", Padding = new Thickness(5), VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap }; var brd = new Border { BorderBrush = Brushes.LightGray, Child = el, Background = isHeader ? Brushes.LightSteelBlue : Brushes.Transparent }; string? hTxt = (el as TextBlock)?.Text; if (isHeader && hTxt == "Ações" && (c == 4 || c == 5 || c == 2)) brd.BorderThickness = new Thickness(0, 0, 0, 1); else brd.BorderThickness = new Thickness(0, 0, 1, 1); if (isHeader && el is TextBlock htb) { htb.FontWeight = FontWeights.Bold; htb.HorizontalAlignment = HorizontalAlignment.Center; } Grid.SetRow(brd, r); Grid.SetColumn(brd, c); return brd; }
        private void AtualizarContadoresSumario() { if (TotalAlunosTextBlock != null) TotalAlunosTextBlock.Text = listaDeAlunosPrincipal.Count.ToString(); if (TotalTarefasTextBlock != null) TotalTarefasTextBlock.Text = listaDeTarefasPrincipal.Count.ToString(); if (TotalGruposTextBlock != null) TotalGruposTextBlock.Text = listaDeGruposPrincipal.Count.ToString(); }
        private void HandleClickError(Exception ex, string method) { Debug.WriteLine($"ERRO {method}: {ex}"); MessageBox.Show($"Erro {method}: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error); }
        private void HandleFatalError(Exception ex, string ctx) { Debug.WriteLine($"ERRO FATAL {ctx}: {ex}"); MessageBox.Show($"Erro crítico {ctx}: {ex.Message}\nA aplicação encerra.", "Erro Crítico", MessageBoxButton.OK, MessageBoxImage.Error); if (Application.Current != null) Application.Current.Shutdown(); }
        protected virtual void OnPropertyChanged(string prop) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop)); }

    } // Fim da classe MainWindow
      // Add the following properties to the App class to define Alunos, Grupos, Tarefas, and Notas.

    public partial class App : Application
    {
        public static List<Aluno> Alunos { get; private set; } = new List<Aluno>();
        public static List<Grupo> Grupos { get; private set; } = new List<Grupo>();
        public static List<Tarefa> Tarefas { get; private set; } = new List<Tarefa>();
        public static List<NotaAlunoTarefa> Notas { get; private set; } = new List<NotaAlunoTarefa>();

        // You can initialize these lists with data if needed in the Application_Startup method.
    }
} // Fim do namespace FinalLab

