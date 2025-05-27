// FinalLab/Models/NotaAlunoTarefa.cs
using System;
using System.ComponentModel;

namespace FinalLab.Models
{
    public class NotaAlunoTarefa : INotifyPropertyChanged
    {
        // Chaves não devem mudar após criação, então podem ser readonly properties
        public string NumeroAluno { get; }
        public int IdTarefa { get; }

        private double? _valor;
        public double? Valor
        {
            get => _valor;
            set
            {
                if (value.HasValue && (value < 0 || value > 20))
                {
                    throw new ArgumentOutOfRangeException(nameof(Valor), "A nota deve estar entre 0 e 20.");
                }
                if (!Nullable.Equals(_valor, value))
                {
                    _valor = value;
                    OnPropertyChanged(nameof(Valor));
                }
            }
        }

        private DateTime _dataAtribuicao;
        public DateTime DataAtribuicao
        {
            get => _dataAtribuicao;
            set
            {
                if (_dataAtribuicao != value)
                {
                    _dataAtribuicao = value;
                    OnPropertyChanged(nameof(DataAtribuicao));
                }
            }
        }

        private bool _atribuidaViaGrupo;
        public bool AtribuidaViaGrupo
        {
            get => _atribuidaViaGrupo;
            set
            {
                if (_atribuidaViaGrupo != value)
                {
                    _atribuidaViaGrupo = value;
                    OnPropertyChanged(nameof(AtribuidaViaGrupo));
                }
            }
        }

        private string? _observacoes; // Observações podem ser nulas
        public string? Observacoes
        {
            get => _observacoes;
            set
            {
                if (_observacoes != value)
                {
                    _observacoes = value;
                    OnPropertyChanged(nameof(Observacoes));
                }
            }
        }

        public NotaAlunoTarefa(string numeroAluno, int idTarefa)
        {
            if (string.IsNullOrWhiteSpace(numeroAluno))
                throw new ArgumentNullException(nameof(numeroAluno));

            NumeroAluno = numeroAluno;
            IdTarefa = idTarefa;
            _dataAtribuicao = DateTime.Now; // Definir valor inicial
            _atribuidaViaGrupo = false;    // Definir valor inicial
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
