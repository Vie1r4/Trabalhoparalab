// Ficheiro: Models/Aluno.cs
using System;

namespace FinalLab.Models // Certifica-te que o namespace está correto
{
    public class Aluno
    {
        private string _nomeCompleto;
        public string NomeCompleto
        {
            get => _nomeCompleto;
            // O setter pode ser simplificado se não houver lógica adicional como INotifyPropertyChanged
            set { _nomeCompleto = value; }
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
                // Se INotifyPropertyChanged for adicionado, chamar OnPropertyChanged() aqui.
            }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; }
        }

        private string _grupo;
        public string Grupo
        {
            get => _grupo;
            set { _grupo = value ?? "Sem Grupo Atribuído"; }
        }

        public string NomeCompletoNumero => $"{NomeCompleto} ({NumeroAluno})";

        // Construtor principal
        public Aluno(string nomeCompleto, string numeroAlunoParam, string email, string? grupoParam = null)
        {
            // Validações de entrada para os parâmetros
            if (string.IsNullOrWhiteSpace(nomeCompleto))
                throw new ArgumentException("Nome completo é obrigatório.", nameof(nomeCompleto));

            // Validar o parâmetro numeroAlunoParam ANTES de tentar atribuí-lo ao campo.
            // Isto garante que se numeroAlunoParam for inválido, a exceção é lançada antes da atribuição,
            // e se for válido, _numeroAluno será definitivamente inicializado.
            if (string.IsNullOrWhiteSpace(numeroAlunoParam))
                throw new ArgumentException("O número do aluno não pode ser vazio.", nameof(numeroAlunoParam));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email é obrigatório.", nameof(email));

            // Inicializa os campos de apoio diretamente.
            _nomeCompleto = nomeCompleto;
            _numeroAluno = numeroAlunoParam; // Atribuição direta do parâmetro validado ao campo.
            _email = email;
            _grupo = grupoParam ?? "Sem Grupo Atribuído";
        }


    }
}
