// FinalLab/Models/Grupo.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace FinalLab.Models
{
    public class Grupo : INotifyPropertyChanged
    {
        private static int _nextIdCounter = 1;
        public string Id { get; private set; } // String ID

        private string _nome = default!;
        public string Nome
        {
            get => _nome;
            set
            { /* ... (setter com validação e OnPropertyChanged) ... */
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Nome do grupo não pode ser vazio.", nameof(Nome));
                if (_nome != value) { _nome = value; OnPropertyChanged(nameof(Nome)); }
            }
        }

        private ObservableCollection<Aluno> _alunosDoGrupo = new ObservableCollection<Aluno>();
        public ObservableCollection<Aluno> AlunosDoGrupo
        {
            get => _alunosDoGrupo;
            set { if (_alunosDoGrupo != value) { _alunosDoGrupo = value; OnPropertyChanged(nameof(AlunosDoGrupo)); } }
        }

        // Construtor principal para novos grupos (gera ID numérico como string)
        public Grupo(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("Nome do grupo não pode ser vazio.", nameof(nome));

            // Se o nome for "Todos os Grupos", usa o ID especial definido em Pauta.
            // Caso contrário, gera um novo ID.
            if (nome.Equals(Pauta.TODOS_GRUPOS_NOME_DISPLAY, StringComparison.OrdinalIgnoreCase))
            {
                Id = Pauta.TODOS_GRUPOS_ID;
            }
            else
            {
                Id = (_nextIdCounter++).ToString();
            }
            Nome = nome;
        }

        // Construtor para casos onde o ID já é conhecido (ex: placeholder "Todos os Grupos" com ID fixo)
        // Ou para recriar objetos se necessário (embora sem persistência, menos comum)
        internal Grupo(string id, string nome, ObservableCollection<Aluno>? alunos = null)
        {
            Id = id;
            Nome = nome;
            AlunosDoGrupo = alunos ?? new ObservableCollection<Aluno>();
            // Não incrementa _nextIdCounter se o ID for de um placeholder conhecido
            // ou se estivermos a recriar um grupo com ID já existente.
            if (int.TryParse(id, out int numericId) && numericId >= _nextIdCounter)
            {
                _nextIdCounter = numericId + 1;
            }
        }

        public void AdicionarAluno(Aluno aluno) { /* ... (código como antes) ... */ ArgumentNullException.ThrowIfNull(aluno); if (!_alunosDoGrupo.Any(a => a.NumeroAluno == aluno.NumeroAluno)) { _alunosDoGrupo.Add(aluno); OnPropertyChanged(nameof(AlunosDoGrupo)); } }
        public void RemoverAluno(Aluno aluno) { /* ... (código como antes) ... */ ArgumentNullException.ThrowIfNull(aluno); var al = _alunosDoGrupo.FirstOrDefault(a => a.NumeroAluno == aluno.NumeroAluno); if (al != null) { _alunosDoGrupo.Remove(al); OnPropertyChanged(nameof(AlunosDoGrupo)); } }
        public void LimparAlunos() { if (_alunosDoGrupo.Any()) { _alunosDoGrupo.Clear(); OnPropertyChanged(nameof(AlunosDoGrupo)); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }
}
