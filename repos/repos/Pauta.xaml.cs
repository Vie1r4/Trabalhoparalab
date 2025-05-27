// FinalLab/Pauta.xaml.cs
using FinalLab.Models; // Certifica-te que este namespace está correto
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace FinalLab
{
    public partial class Pauta : UserControl, INotifyPropertyChanged
    {
        public const string TODOS_GRUPOS_ID = "placeholder_todos_grupos_id";
        public const string TODOS_GRUPOS_NOME_DISPLAY = "Todos os Grupos";

        private MainWindow _mainWindow;
        public ObservableCollection<Grupo> GruposDisponiveisParaFiltro { get; set; }
        public ObservableCollection<Tarefa> TarefasDisponiveisParaFiltro { get; set; }

        private Grupo? _selectedGrupo;
        public Grupo? SelectedGrupo
        {
            get => _selectedGrupo;
            set { if (_selectedGrupo != value) { _selectedGrupo = value; OnPropertyChanged(nameof(SelectedGrupo)); } }
        }

        private Tarefa? _selectedTarefa;
        public Tarefa? SelectedTarefa
        {
            get => _selectedTarefa;
            set { if (_selectedTarefa != value) { _selectedTarefa = value; OnPropertyChanged(nameof(SelectedTarefa)); } }
        }

        private string _textoPesquisaAluno = string.Empty;
        public string TextoPesquisaAluno
        {
            get => _textoPesquisaAluno;
            set { if (_textoPesquisaAluno != value) { _textoPesquisaAluno = value; OnPropertyChanged(nameof(TextoPesquisaAluno)); AtualizarTabelaPauta(); } }
        }

        private ObservableCollection<PautaItemViewModel> _pautaItems = new ObservableCollection<PautaItemViewModel>();
        public ObservableCollection<PautaItemViewModel> PautaItems
        {
            get => _pautaItems;
            set { _pautaItems = value; OnPropertyChanged(nameof(PautaItems)); }
        }

        public Pauta(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
            DataContext = this;
            GruposDisponiveisParaFiltro = new ObservableCollection<Grupo>();
            TarefasDisponiveisParaFiltro = new ObservableCollection<Tarefa>();
        }

        private void Pauta_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Pauta_Loaded acionado.");
            CarregarFiltros();
            ReconfigurarColunasDataGrid();
            AtualizarTabelaPauta();
        }

        private void CarregarFiltros()
        {
            GruposDisponiveisParaFiltro.Clear();
            var todosOsGruposPlaceholder = new Grupo(TODOS_GRUPOS_ID, TODOS_GRUPOS_NOME_DISPLAY);
            GruposDisponiveisParaFiltro.Add(todosOsGruposPlaceholder);
            foreach (var grupo in _mainWindow.listaDeGruposPrincipal.OrderBy(g => g.Nome)) GruposDisponiveisParaFiltro.Add(grupo);
            if (SelectedGrupo == null && GruposDisponiveisParaFiltro.Any()) SelectedGrupo = GruposDisponiveisParaFiltro.First();

            TarefasDisponiveisParaFiltro.Clear();
            foreach (var tarefa in _mainWindow.listaDeTarefasPrincipal.OrderBy(t => t.Titulo)) TarefasDisponiveisParaFiltro.Add(tarefa);
            if (SelectedTarefa == null && TarefaVisualizarComboBox != null) TarefaVisualizarComboBox.SelectedIndex = -1;
        }

        private void ReconfigurarColunasDataGrid()
        {
            if (DataGridPauta == null || _mainWindow == null) return;
            DataGridPauta.Columns.Clear();

            DataGridPauta.Columns.Add(new DataGridTextColumn
            {
                Header = "Nº Aluno",
                Binding = new Binding("NumeroAluno"),
                IsReadOnly = true,
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                MinWidth = 70,
                CellStyle = (Style)FindResource("CentralizedDataGridCellStyle")
            });
            DataGridPauta.Columns.Add(new DataGridTextColumn
            {
                Header = "Nome Aluno",
                Binding = new Binding("NomeAluno"),
                IsReadOnly = true,
                Width = new DataGridLength(2.5, DataGridLengthUnitType.Star),
                MinWidth = 180,
                CellStyle = (Style)FindResource("CentralizedDataGridCellStyle")
            });
            DataGridPauta.Columns.Add(new DataGridTextColumn
            {
                Header = "Grupo",
                Binding = new Binding("GrupoAluno"),
                IsReadOnly = true,
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                MinWidth = 90,
                CellStyle = (Style)FindResource("CentralizedDataGridCellStyle")
            });

            foreach (var tarefa in _mainWindow.listaDeTarefasPrincipal.OrderBy(t => t.Id))
            {
                DataGridPauta.Columns.Add(new DataGridTextColumn
                {
                    Header = $"{tarefa.Titulo}\n(Peso: {tarefa.Peso}%)",
                    Binding = new Binding($"NotasPorTarefa[{tarefa.Id}]") { StringFormat = "N1", UpdateSourceTrigger = UpdateSourceTrigger.LostFocus, Mode = BindingMode.TwoWay, FallbackValue = string.Empty },
                    IsReadOnly = false,
                    Width = new DataGridLength(0.8, DataGridLengthUnitType.Star),
                    MinWidth = 75,
                    CellStyle = (Style)FindResource("CentralizedDataGridCellStyle")
                });
            }

            DataGridPauta.Columns.Add(new DataGridTextColumn
            {
                Header = "Nota Final",
                Binding = new Binding("NotaFinal") { StringFormat = "N2", FallbackValue = "-" },
                IsReadOnly = true,
                FontWeight = FontWeights.Bold,
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                MinWidth = 80,
                CellStyle = (Style)FindResource("CentralizedDataGridCellStyle")
            });
        }

        public void AtualizarTabelaPauta()
        {
            PautaItems.Clear();
            if (_mainWindow == null) return;
            IEnumerable<Aluno> alunosParaExibir = _mainWindow.listaDeAlunosPrincipal;

            if (SelectedGrupo != null && SelectedGrupo.Id != TODOS_GRUPOS_ID)
                alunosParaExibir = alunosParaExibir.Where(a => a.Grupo == SelectedGrupo.Nome);

            if (!string.IsNullOrWhiteSpace(TextoPesquisaAluno))
            {
                string filtroLower = TextoPesquisaAluno.ToLowerInvariant();
                alunosParaExibir = alunosParaExibir.Where(a => (a.NomeCompleto?.ToLowerInvariant().Contains(filtroLower) == true) || (a.NumeroAluno?.ToLowerInvariant().Contains(filtroLower) == true));
            }

            foreach (var aluno in alunosParaExibir.OrderBy(a => a.NomeCompleto))
            {
                var pautaItem = new PautaItemViewModel { NumeroAluno = aluno.NumeroAluno, NomeAluno = aluno.NomeCompleto, GrupoAluno = aluno.Grupo };
                foreach (var tarefa in _mainWindow.listaDeTarefasPrincipal.OrderBy(t => t.Id))
                {
                    var nota = _mainWindow.listaDeNotasPrincipal.FirstOrDefault(n => n.NumeroAluno == aluno.NumeroAluno && n.IdTarefa == tarefa.Id);
                    pautaItem.NotasPorTarefa[tarefa.Id] = nota?.Valor;
                }
                RecalcularNotaFinalViewModel(pautaItem);
                PautaItems.Add(pautaItem);
            }
            if (DataGridPauta != null) DataGridPauta.Items.Refresh();
        }

        private void RecalcularNotaFinalViewModel(PautaItemViewModel viewModel)
        {
            if (viewModel == null || _mainWindow == null) return;
            double notaFinalCalculada = 0;
            foreach (var tarefa in _mainWindow.listaDeTarefasPrincipal)
            {
                if (viewModel.NotasPorTarefa.TryGetValue(tarefa.Id, out double? notaValor) && notaValor.HasValue)
                {
                    notaFinalCalculada += notaValor.Value * (tarefa.Peso / 100.0);
                }
            }
            viewModel.NotaFinal = (_mainWindow.listaDeTarefasPrincipal.Any(t => viewModel.NotasPorTarefa.ContainsKey(t.Id) && viewModel.NotasPorTarefa[t.Id].HasValue)) ? notaFinalCalculada : (double?)null;
        }

        private void GrupoFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AtualizarTabelaPauta();
        }
        private void TarefaVisualizarComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Esta seleção já não muda as colunas, mas o botão "Atribuir Nota" pode depender dela.
            // Se o botão "Atribuir Nota" usar SelectedTarefa, esta lógica é importante.
            // Se não, este evento pode não precisar de fazer nada.
        }
        private void AlunoSearchTextBox_TextChanged(object sender, TextChangedEventArgs e) { /* O setter já chama AtualizarTabelaPauta */ }

        private void NotaTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = (TextBox)sender; string currentText = tb.Text; int caretIndex = tb.CaretIndex; string proposedText = currentText.Insert(caretIndex, e.Text);
            if (Regex.IsMatch(e.Text, @"[0-9]")) { if (Regex.IsMatch(proposedText, @"^(20([,.][0]*)?|[0-1]?[0-9]([,.][0-9]*)?)$")) { if (currentText == "0" && caretIndex == 1 && e.Text != "," && e.Text != "." && proposedText.Length <= 2 && !proposedText.Contains(',')) { tb.Text = e.Text; tb.CaretIndex = 1; e.Handled = true; return; } e.Handled = false; return; } }
            else if ((e.Text == "," || e.Text == ".") && !currentText.Contains(",") && !currentText.Contains(".") && caretIndex > 0) { if (currentText.Length > 0 && !currentText.Contains(e.Text)) { e.Handled = false; return; } }
            e.Handled = true;
        }

        private void AtribuirNotasButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridPauta == null || DataGridPauta.SelectedItems.Count == 0 || string.IsNullOrWhiteSpace(NotaParaAtribuirTextBox.Text)) { MessageBox.Show("Selecione alunos e insira nota.", "Ação Requerida", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (!double.TryParse(NotaParaAtribuirTextBox.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double nota) || nota < 0 || nota > 20) { MessageBox.Show("Nota inválida (0-20).", "Nota Inválida", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            Tarefa? tarefaAlvo = SelectedTarefa;
            if (tarefaAlvo == null)
            {
                MessageBox.Show("Por favor, selecione uma tarefa no ComboBox 'Visualizar tarefa:' para a qual deseja atribuir esta nota em massa.", "Tarefa não Especificada", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            foreach (PautaItemViewModel itemVm in DataGridPauta.SelectedItems)
            {
                var nExistente = _mainWindow.listaDeNotasPrincipal.FirstOrDefault(n => n.NumeroAluno == itemVm.NumeroAluno && n.IdTarefa == tarefaAlvo.Id);
                if (nExistente != null) { nExistente.Valor = nota; nExistente.DataAtribuicao = DateTime.Now; }
                else _mainWindow.listaDeNotasPrincipal.Add(new NotaAlunoTarefa(itemVm.NumeroAluno, tarefaAlvo.Id) { Valor = nota, DataAtribuicao = DateTime.Now });
                itemVm.NotasPorTarefa[tarefaAlvo.Id] = nota; RecalcularNotaFinalViewModel(itemVm);
            }
            NotaParaAtribuirTextBox.Clear(); MessageBox.Show($"Nota {nota.ToString(CultureInfo.CurrentCulture)} atribuída à tarefa '{tarefaAlvo.Titulo}'.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PautaDataGrid_BeginningEdit(object? sender, DataGridBeginningEditEventArgs e) { }

        private void PautaDataGrid_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var vm = e.Row.Item as PautaItemViewModel; TextBox? tb = e.EditingElement as TextBox; Binding? bPath = (e.Column as DataGridBoundColumn)?.Binding as Binding;
                if (vm == null || tb == null || bPath == null || !bPath.Path.Path.StartsWith("NotasPorTarefa[")) return;
                int idTarefa; try { idTarefa = int.Parse(bPath.Path.Path.Substring(bPath.Path.Path.IndexOf('[') + 1, bPath.Path.Path.IndexOf(']') - bPath.Path.Path.IndexOf('[') - 1)); } catch { return; }
                double? val; if (string.IsNullOrWhiteSpace(tb.Text)) val = null; else if (double.TryParse(tb.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double pNota)) val = pNota; else { MessageBox.Show("Nota inválida.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning); e.Cancel = true; tb.Text = vm.NotasPorTarefa.TryGetValue(idTarefa, out double? v) && v.HasValue ? v.Value.ToString("N1") : ""; return; }
                if (val.HasValue && (val < 0 || val > 20)) { MessageBox.Show("Nota 0-20.", "Inválida", MessageBoxButton.OK, MessageBoxImage.Warning); e.Cancel = true; tb.Text = vm.NotasPorTarefa.TryGetValue(idTarefa, out double? v) && v.HasValue ? v.Value.ToString("N1") : ""; return; }
                var nExist = _mainWindow.listaDeNotasPrincipal.FirstOrDefault(n => n.NumeroAluno == vm.NumeroAluno && n.IdTarefa == idTarefa);
                if (nExist != null) { nExist.Valor = val; if (val.HasValue) nExist.DataAtribuicao = DateTime.Now; else _mainWindow.listaDeNotasPrincipal.Remove(nExist); }
                else if (val.HasValue) _mainWindow.listaDeNotasPrincipal.Add(new NotaAlunoTarefa(vm.NumeroAluno, idTarefa) { Valor = val, DataAtribuicao = DateTime.Now });
                vm.NotasPorTarefa[idTarefa] = val; RecalcularNotaFinalViewModel(vm);
            }
        }

        private void SalvarAlteracoesButton_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Alterações em memória. A persistência de dados (se ativa) ocorre ao fechar a aplicação principal.", "Informação", MessageBoxButton.OK, MessageBoxImage.Information); }
        private void HistogramaButton_Click(object sender, RoutedEventArgs e) { if (_mainWindow != null) _mainWindow.HistogramaMenuButton_Click(sender, e); else MessageBox.Show("Erro MainWindow.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error); }
        private void CalcularNotasFinaisButton_Click(object sender, RoutedEventArgs e) { foreach (var item in PautaItems) RecalcularNotaFinalViewModel(item); MessageBox.Show("Notas finais recalculadas.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string prop) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop)); }
    }

    public class PautaItemViewModel : INotifyPropertyChanged
    {
        public string NumeroAluno { get; set; } = "";
        public string NomeAluno { get; set; } = "";
        public string GrupoAluno { get; set; } = "";
        public Dictionary<int, double?> NotasPorTarefa { get; set; } = new Dictionary<int, double?>();
        private double? _notaFinal;
        public double? NotaFinal { get => _notaFinal; set { if ((!_notaFinal.HasValue && value.HasValue) || (_notaFinal.HasValue && !value.HasValue) || (_notaFinal.HasValue && value.HasValue && Math.Abs(_notaFinal.Value - value.Value) > 0.001)) { _notaFinal = value; OnPropertyChanged(nameof(NotaFinal)); } } }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string prop) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop)); }
    }
}
