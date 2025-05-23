// Ficheiro: CriarGrupoWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FinalLab.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FinalLab
{
    public partial class CriarGrupoWindow : Window
    {
        public Grupo? GrupoCriadoEditado { get; private set; }
        private List<Aluno> _copiaAlunosAppParaSelecao;
        private Grupo _grupoInternoTrabalho;
        private string _filtroAlunosDisponiveis = string.Empty;

        public CriarGrupoWindow(List<Aluno> todosOsAlunosDaAppCopia)
        {
            InitializeComponent();
            Debug.WriteLine($"[CriarGrupoWindow] Construtor NOVO grupo. {todosOsAlunosDaAppCopia.Count} alunos recebidos.");
            _copiaAlunosAppParaSelecao = todosOsAlunosDaAppCopia;
            _grupoInternoTrabalho = new Grupo("");
            this.Title = "Criar Novo Grupo";
            CarregarListBoxes();
        }

        public CriarGrupoWindow(Grupo grupoParaEditarCopia, List<Aluno> todosOsAlunosDaAppCopia)
        {
            InitializeComponent();
            Debug.WriteLine($"[CriarGrupoWindow] Construtor EDITAR grupo '{grupoParaEditarCopia.Nome}' (ID: {grupoParaEditarCopia.Id}). {todosOsAlunosDaAppCopia.Count} alunos recebidos.");
            _copiaAlunosAppParaSelecao = todosOsAlunosDaAppCopia;
            _grupoInternoTrabalho = new Grupo(grupoParaEditarCopia.Id, grupoParaEditarCopia.Nome); // Usa o construtor de cópia
            // Adiciona os alunos que já estão no grupo a ser editado ao _grupoInternoTrabalho
            foreach (var alunoNoGrupoOriginal in grupoParaEditarCopia.ListaDeAlunosNoGrupo)
            {
                // Encontra o aluno correspondente na lista completa para manter a referência correta
                var alunoRefCompleta = todosOsAlunosDaAppCopia.FirstOrDefault(a => a.NumeroAluno == alunoNoGrupoOriginal.NumeroAluno);
                if (alunoRefCompleta != null)
                {
                    _grupoInternoTrabalho.AdicionarAluno(alunoRefCompleta);
                }
            }

            this.Title = $"Editar Grupo: {_grupoInternoTrabalho.Nome}";
            TextBoxNomeGrupo.Text = _grupoInternoTrabalho.Nome;
            CarregarListBoxes();
        }

        private void CarregarListBoxes()
        {
            Debug.WriteLine($"[CriarGrupoWindow] CarregarListBoxes. Filtro: '{_filtroAlunosDisponiveis}'");
            ListBoxAlunosNoGrupo.Items.Clear();
            ListBoxAlunosDisponiveis.Items.Clear();

            // Como Aluno.NumeroAluno é agora string (não nulo), o HashSet será HashSet<string>
            var numerosAlunosJaNoGrupoInterno = _grupoInternoTrabalho.ListaDeAlunosNoGrupo
                .Select(a => a.NumeroAluno) // Isto agora é IEnumerable<string>
                .ToHashSet(); // Cria HashSet<string>
            Debug.WriteLine($"[CriarGrupoWindow] _grupoInternoTrabalho tem {numerosAlunosJaNoGrupoInterno.Count} alunos.");

            foreach (var alunoOriginalApp in _copiaAlunosAppParaSelecao)
            {
                // A linha 56 (ou perto) é esta. Com Aluno.NumeroAluno como string não nulo, CS8604 não deve ocorrer.
                if (numerosAlunosJaNoGrupoInterno.Contains(alunoOriginalApp.NumeroAluno))
                {
                    ListBoxAlunosNoGrupo.Items.Add(alunoOriginalApp);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(_filtroAlunosDisponiveis) ||
                        (alunoOriginalApp.NomeCompleto.ToLowerInvariant().Contains(_filtroAlunosDisponiveis.ToLowerInvariant())) ||
                        (alunoOriginalApp.NumeroAluno.ToLowerInvariant().Contains(_filtroAlunosDisponiveis.ToLowerInvariant())) ||
                        (alunoOriginalApp.Email.ToLowerInvariant().Contains(_filtroAlunosDisponiveis.ToLowerInvariant()))
                       )
                    {
                        ListBoxAlunosDisponiveis.Items.Add(alunoOriginalApp);
                    }
                }
            }
            Debug.WriteLine($"[CriarGrupoWindow] ListBoxAlunosNoGrupo: {ListBoxAlunosNoGrupo.Items.Count}. ListBoxAlunosDisponiveis: {ListBoxAlunosDisponiveis.Items.Count}.");
        }

        private void TextBoxPesquisaAlunoDisponivel_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox) { _filtroAlunosDisponiveis = textBox.Text; CarregarListBoxes(); }
        }

        private void ButtonAdicionarAlunoAoGrupo_Click(object sender, RoutedEventArgs e)
        {
            var alunosParaAdicionar = ListBoxAlunosDisponiveis.SelectedItems.Cast<Aluno>().ToList();
            foreach (var aluno in alunosParaAdicionar) { _grupoInternoTrabalho.AdicionarAluno(aluno); }
            CarregarListBoxes();
        }

        private void ButtonRemoverAlunoDoGrupo_Click(object sender, RoutedEventArgs e)
        {
            var alunosParaRemover = ListBoxAlunosNoGrupo.SelectedItems.Cast<Aluno>().ToList();
            foreach (var aluno in alunosParaRemover) { _grupoInternoTrabalho.RemoverAluno(aluno); }
            CarregarListBoxes();
        }

        private void ButtonGuardarGrupo_Click(object sender, RoutedEventArgs e)
        {
            string nomeGrupo = TextBoxNomeGrupo.Text.Trim();
            if (string.IsNullOrWhiteSpace(nomeGrupo))
            { MessageBox.Show("Nome do grupo obrigatório.", "Validação"); return; }
            _grupoInternoTrabalho.Nome = nomeGrupo;
            GrupoCriadoEditado = _grupoInternoTrabalho;
            DialogResult = true;
            Close();
        }

        private void ButtonCancelarGrupo_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }
    }
}
