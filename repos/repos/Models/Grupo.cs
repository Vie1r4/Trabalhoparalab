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

        // Propriedades públicas para serialização
        public string Id { get; set; } = string.Empty;

        private string _nome = string.Empty;
        public string Nome
        {
            get => _nome;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Nome do grupo não pode ser vazio.", nameof(Nome));
                if (_nome != value)
                {
                    _nome = value;
                    OnPropertyChanged(nameof(Nome));
                }
            }
        }

        // Usar List<Aluno> para facilitar a serialização
        public List<Aluno> AlunosDoGrupo { get; set; } = new();

        // Construtor sem parâmetros para serialização
        public Grupo() { }

        // Construtor principal para uso na aplicação
        public Grupo(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome do grupo não pode ser vazio.", nameof(nome));

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
        public Grupo(string id, string nome, List<Aluno>? alunos = null)
        {
            Id = id;
            Nome = nome;
            AlunosDoGrupo = alunos ?? new List<Aluno>();
            // Não incrementa _nextIdCounter se o ID for de um placeholder conhecido
            // ou se estivermos a recriar um grupo com ID já existente.
            if (int.TryParse(id, out int numericId) && numericId >= _nextIdCounter)
            {
                _nextIdCounter = numericId + 1;
            }
        }

        public void AdicionarAluno(Aluno aluno)
        {
            ArgumentNullException.ThrowIfNull(aluno);
            if (!AlunosDoGrupo.Exists(a => a.NumeroAluno == aluno.NumeroAluno))
            {
                AlunosDoGrupo.Add(aluno);
                OnPropertyChanged(nameof(AlunosDoGrupo));
            }
        }

        public void RemoverAluno(Aluno aluno)
        {
            ArgumentNullException.ThrowIfNull(aluno);
            var al = AlunosDoGrupo.Find(a => a.NumeroAluno == aluno.NumeroAluno);
            if (al != null)
            {
                AlunosDoGrupo.Remove(al);
                OnPropertyChanged(nameof(AlunosDoGrupo));
            }
        }

        public void LimparAlunos()
        {
            if (AlunosDoGrupo.Count > 0)
            {
                AlunosDoGrupo.Clear();
                OnPropertyChanged(nameof(AlunosDoGrupo));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
