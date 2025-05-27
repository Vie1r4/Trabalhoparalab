// FinalLab/CriarGrupoWindow.xaml.cs
using FinalLab.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FinalLab
{
    public partial class CriarGrupoWindow : Window
    {
        public Grupo? GrupoCriadoEditado { get; private set; } // Propriedade anulável
        private List<Aluno> _todosOsAlunosApp;
        private ObservableCollection<Aluno> _alunosDisponiveisView;
        private ObservableCollection<Aluno> _alunosNoGrupoView;
        private bool _isEditingMode = false;
        // _idGrupoOriginalEmEdicao não parece ser usado no código original, removido para simplificar.
        // Se for necessário para lógica de ID, pode ser reintroduzido como string?

        // Construtor para CRIAR novo grupo
        public CriarGrupoWindow(List<Aluno> todosAlunosApp)
        {
            InitializeComponent();
            _todosOsAlunosApp = new List<Aluno>(todosAlunosApp);
            _alunosDisponiveisView = new ObservableCollection<Aluno>();
            _alunosNoGrupoView = new ObservableCollection<Aluno>();

            ListBoxAlunosDisponiveis.ItemsSource = _alunosDisponiveisView;
            ListBoxAlunosNoGrupo.ItemsSource = _alunosNoGrupoView;

            Title = "Criar Novo Grupo";
            CarregarListBoxes();
            Debug.WriteLine($"CriarGrupoWindow: Modo Criação. Total Alunos App: {_todosOsAlunosApp.Count}");
        }

        // Construtor para EDITAR grupo existente
        public CriarGrupoWindow(Grupo grupoParaEditar, List<Aluno> todosAlunosApp)
        {
            InitializeComponent();
            _isEditingMode = true;
            _todosOsAlunosApp = new List<Aluno>(todosAlunosApp);



            // With this corrected line:
            GrupoCriadoEditado = new Grupo(grupoParaEditar.Nome);
            foreach (var aluno in grupoParaEditar.AlunosDoGrupo)
            {
                GrupoCriadoEditado.AdicionarAluno(aluno);
            }
            // Se o ID precisar ser preservado na edição:
            // GrupoCriadoEditado.Id = grupoParaEditar.Id; // Assumindo que Id tem um setter acessível ou é passado ao construtor

            _alunosDisponiveisView = new ObservableCollection<Aluno>();
            _alunosNoGrupoView = new ObservableCollection<Aluno>(GrupoCriadoEditado.AlunosDoGrupo.OrderBy(a => a.NomeCompleto));

            ListBoxAlunosDisponiveis.ItemsSource = _alunosDisponiveisView;
            ListBoxAlunosNoGrupo.ItemsSource = _alunosNoGrupoView;

            TextBoxNomeGrupo.Text = GrupoCriadoEditado.Nome;
            Title = $"Editar Grupo: {GrupoCriadoEditado.Nome}";
            CarregarListBoxes();
            Debug.WriteLine($"CriarGrupoWindow: Modo Edição para '{GrupoCriadoEditado.Nome}'. Alunos no grupo: {_alunosNoGrupoView.Count}, Total Alunos App: {_todosOsAlunosApp.Count}");
        }

        private void CarregarListBoxes()
        {
            _alunosDisponiveisView.Clear();
            string filtro = TextBoxPesquisaAlunoDisponivel.Text.ToLowerInvariant();

            foreach (var alunoApp in _todosOsAlunosApp.OrderBy(a => a.NomeCompleto))
            {
                bool noGrupoAtual = _alunosNoGrupoView.Any(aNoGrupo => aNoGrupo.NumeroAluno == alunoApp.NumeroAluno);
                bool correspondeFiltro = string.IsNullOrWhiteSpace(filtro) ||
                                         (alunoApp.NomeCompleto?.ToLowerInvariant().Contains(filtro) == true) ||
                                         (alunoApp.NumeroAluno?.ToLowerInvariant().Contains(filtro) == true);

                if (!noGrupoAtual && correspondeFiltro)
                {
                    _alunosDisponiveisView.Add(alunoApp);
                }
            }
            Debug.WriteLine($"CarregarListBoxes: Disponíveis: {_alunosDisponiveisView.Count}, No Grupo: {_alunosNoGrupoView.Count}, Filtro: '{filtro}'");
        }

        private void TextBoxPesquisaAlunoDisponivel_TextChanged(object sender, TextChangedEventArgs e)
        {
            CarregarListBoxes();
        }

        private void ButtonAdicionarAlunoAoGrupo_Click(object sender, RoutedEventArgs e)
        {
            var selecionados = ListBoxAlunosDisponiveis.SelectedItems.Cast<Aluno>().ToList();
            if (!selecionados.Any()) return;

            foreach (var aluno in selecionados)
            {
                _alunosDisponiveisView.Remove(aluno);
                _alunosNoGrupoView.Add(aluno);
            }
            // Reordenar a lista de alunos no grupo
            var tempSorted = _alunosNoGrupoView.OrderBy(a => a.NomeCompleto).ToList();
            _alunosNoGrupoView.Clear();
            foreach (var a in tempSorted) _alunosNoGrupoView.Add(a);

            Debug.WriteLine($"Adicionados ao grupo: {string.Join(", ", selecionados.Select(a => a.NomeCompleto))}");
        }

        private void ButtonRemoverAlunoDoGrupo_Click(object sender, RoutedEventArgs e)
        {
            var selecionados = ListBoxAlunosNoGrupo.SelectedItems.Cast<Aluno>().ToList();
            if (!selecionados.Any()) return;

            foreach (var aluno in selecionados)
            {
                _alunosNoGrupoView.Remove(aluno);
                _alunosDisponiveisView.Add(aluno); // Adicionar de volta aos disponíveis
            }
            // Reordenar a lista de alunos disponíveis após adicionar
            var tempSortedDisponiveis = _alunosDisponiveisView.OrderBy(a => a.NomeCompleto).ToList();
            _alunosDisponiveisView.Clear();
            foreach (var a in tempSortedDisponiveis) _alunosDisponiveisView.Add(a);

            Debug.WriteLine($"Removidos do grupo: {string.Join(", ", selecionados.Select(a => a.NomeCompleto))}");
        }

        private void ButtonGuardarGrupo_Click(object sender, RoutedEventArgs e)
        {
            string nomeGrupo = TextBoxNomeGrupo.Text.Trim();
            if (string.IsNullOrWhiteSpace(nomeGrupo))
            {
                MessageBox.Show("O nome do grupo não pode ser vazio.", "Nome Inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_isEditingMode)
            {
                if (GrupoCriadoEditado != null) // GrupoCriadoEditado foi inicializado no construtor de edição
                {
                    GrupoCriadoEditado.Nome = nomeGrupo;
                    GrupoCriadoEditado.LimparAlunos();
                    foreach (var aluno in _alunosNoGrupoView)
                    {
                        GrupoCriadoEditado.AdicionarAluno(aluno);
                    }
                }
                else // Segurança, não deve acontecer se _isEditingMode = true e o construtor está correto
                {
                    GrupoCriadoEditado = new Grupo(nomeGrupo);
                    foreach (var aluno in _alunosNoGrupoView) GrupoCriadoEditado.AdicionarAluno(aluno);
                }
            }
            else
            {
                GrupoCriadoEditado = new Grupo(nomeGrupo);
                foreach (var aluno in _alunosNoGrupoView)
                {
                    GrupoCriadoEditado.AdicionarAluno(aluno);
                }
            }

            DialogResult = true;
            Close();
        }

        private void ButtonCancelarGrupo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
