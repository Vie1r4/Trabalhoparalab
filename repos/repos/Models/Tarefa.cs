// FinalLab/Models/Tarefa.cs
using System;
using System.ComponentModel;

namespace FinalLab.Models
{
    public class Tarefa : INotifyPropertyChanged
    {
        private static int _nextId = 1;
        public int Id { get; private set; }

        private string _titulo = default!;
        public string Titulo
        {
            get => _titulo;
            set
            { /* ... (setter com validação e OnPropertyChanged) ... */
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Título não pode ser vazio.", nameof(Titulo));
                if (_titulo != value) { _titulo = value; OnPropertyChanged(nameof(Titulo)); }
            }
        }

        private string _descricao = string.Empty;
        public string Descricao
        {
            get => _descricao;
            set { if (_descricao != value) { _descricao = value ?? string.Empty; OnPropertyChanged(nameof(Descricao)); } }
        }

        private DateTime _dataInicio;
        public DateTime DataInicio
        {
            get => _dataInicio;
            set { if (_dataInicio != value) { _dataInicio = value; OnPropertyChanged(nameof(DataInicio)); } }
        }

        private DateTime _dataTermino;
        public DateTime DataTermino
        {
            get => _dataTermino;
            set
            { /* ... (setter com validação e OnPropertyChanged) ... */
                if (value < DataInicio) throw new ArgumentException("Data de término anterior à de início.", nameof(DataTermino));
                if (_dataTermino != value) { _dataTermino = value; OnPropertyChanged(nameof(DataTermino)); }
            }
        }

        private int _peso;
        public int Peso
        {
            get => _peso;
            set
            { /* ... (setter com validação e OnPropertyChanged) ... */
                if (value < 0 || value > 100) throw new ArgumentOutOfRangeException(nameof(Peso), "Peso entre 0 e 100.");
                if (_peso != value) { _peso = value; OnPropertyChanged(nameof(Peso)); }
            }
        }

        // Construtor principal para novas tarefas
        public Tarefa(string titulo, string? descricao, DateTime dataInicio, DateTime dataTermino, int peso)
        {
            Id = _nextId++;
            Titulo = titulo; // Usa o setter para validação
            Descricao = descricao ?? string.Empty;
            DataInicio = dataInicio;
            DataTermino = dataTermino; // Usa o setter para validação
            Peso = peso; // Usa o setter para validação
        }

        // Construtor para placeholders ou casos onde o ID é conhecido (como no Histograma)
        // Este construtor tem 6 argumentos, o que parece ser o que o HistogramaWindow está a chamar.
        internal Tarefa(int id, string titulo, string? descricao, DateTime dataInicio, DateTime dataTermino, int peso)
        {
            Id = id; // Usa o ID fornecido
            Titulo = titulo;
            Descricao = descricao ?? string.Empty;
            DataInicio = dataInicio;
            DataTermino = dataTermino;
            Peso = peso;
            // Não incrementa _nextId se o ID for negativo (placeholders)
            if (id >= _nextId && id > 0) { _nextId = id + 1; }
            else if (_nextId <= 1 && id > 0) { _nextId = id + 1; } // Caso _nextId ainda não tenha sido usado
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
