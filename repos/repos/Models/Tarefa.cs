// FinalLab/Models/Tarefa.cs
using System;
using System.ComponentModel;

namespace FinalLab.Models
{
    public class Tarefa : INotifyPropertyChanged
    {
        private static int _nextId = 1;

        // Propriedade pública para serialização
        public int Id { get; set; }

        private string _titulo = string.Empty;
        public string Titulo
        {
            get => _titulo;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Título não pode ser vazio.", nameof(Titulo));
                if (_titulo != value)
                {
                    _titulo = value;
                    OnPropertyChanged(nameof(Titulo));
                }
            }
        }

        private string _descricao = string.Empty;
        public string Descricao
        {
            get => _descricao;
            set
            {
                if (_descricao != value)
                {
                    _descricao = value ?? string.Empty;
                    OnPropertyChanged(nameof(Descricao));
                }
            }
        }

        private DateTime _dataInicio;
        public DateTime DataInicio
        {
            get => _dataInicio;
            set
            {
                if (_dataInicio != value)
                {
                    _dataInicio = value;
                    OnPropertyChanged(nameof(DataInicio));
                }
            }
        }

        private DateTime _dataTermino;
        public DateTime DataTermino
        {
            get => _dataTermino;
            set
            {
                if (value < DataInicio)
                    throw new ArgumentException("Data de término anterior à de início.", nameof(DataTermino));
                if (_dataTermino != value)
                {
                    _dataTermino = value;
                    OnPropertyChanged(nameof(DataTermino));
                }
            }
        }

        private int _peso;
        public int Peso
        {
            get => _peso;
            set
            {
                if (value < 0 || value > 100)
                    throw new ArgumentOutOfRangeException(nameof(Peso), "Peso entre 0 e 100.");
                if (_peso != value)
                {
                    _peso = value;
                    OnPropertyChanged(nameof(Peso));
                }
            }
        }

        // Construtor sem parâmetros para serialização
        public Tarefa() { }

        // Construtor principal para uso na aplicação
        public Tarefa(string titulo, string? descricao, DateTime dataInicio, DateTime dataTermino, int peso)
        {
            Id = _nextId++;
            Titulo = titulo;
            Descricao = descricao ?? string.Empty;
            DataInicio = dataInicio;
            DataTermino = dataTermino;
            Peso = peso;
        }

        // Construtor para casos onde o ID já é conhecido
        public Tarefa(int id, string titulo, string? descricao, DateTime dataInicio, DateTime dataTermino, int peso)
        {
            Id = id;
            Titulo = titulo;
            Descricao = descricao ?? string.Empty;
            DataInicio = dataInicio;
            DataTermino = dataTermino;
            Peso = peso;
            if (id >= _nextId && id > 0)
            {
                _nextId = id + 1;
            }
            else if (_nextId <= 1 && id > 0)
            {
                _nextId = id + 1;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
