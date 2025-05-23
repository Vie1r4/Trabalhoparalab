// Ficheiro: Models/Aluno.cs
using System; // Para ArgumentException
// using System.ComponentModel; // Descomente se e quando usar INotifyPropertyChanged
// using System.Runtime.CompilerServices; // Descomente se e quando usar INotifyPropertyChanged

namespace FinalLab.Models
{
    public class Aluno
    {
        private string _nomeCompleto;
        public string NomeCompleto
        {
            get => _nomeCompleto;
            set { _nomeCompleto = value; /* OnPropertyChanged(); */ }
        }

        private string _numeroAluno; // Campo de apoio para NumeroAluno
        public string NumeroAluno
        {
            get => _numeroAluno;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("O número do aluno não pode ser vazio.", nameof(NumeroAluno));
                _numeroAluno = value;
                /* OnPropertyChanged(); */
            }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; /* OnPropertyChanged(); */ }
        }

        private string _grupo; // Campo de apoio para Grupo
        public string Grupo
        {
            get => _grupo;
            set { _grupo = value ?? "Sem Grupo Atribuído"; /* OnPropertyChanged(); */ }
        }

        public string NomeCompletoNumero => $"{NomeCompleto} ({NumeroAluno})";

        // Construtor principal
        public Aluno(string nomeCompleto, string numeroAluno, string email, string? grupoParam = null)
        {
            // Validações de entrada
            if (string.IsNullOrWhiteSpace(nomeCompleto))
                throw new ArgumentException("Nome completo é obrigatório.", nameof(nomeCompleto));
            // A validação de numeroAluno será feita pelo setter da propriedade NumeroAluno
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email é obrigatório.", nameof(email));

            // Inicializa os campos de apoio diretamente ou através de propriedades (que inicializam os campos)
            _nomeCompleto = nomeCompleto;
            NumeroAluno = numeroAluno; // Usa o setter para que a validação e inicialização de _numeroAluno ocorra
            _email = email;
            _grupo = grupoParam ?? "Sem Grupo Atribuído"; // Garante que _grupo nunca é null
        }

        // Se INotifyPropertyChanged for necessário no futuro:
        /* 
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        */
    }
}
