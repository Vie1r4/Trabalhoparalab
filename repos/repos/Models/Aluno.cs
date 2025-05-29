// Ficheiro: Models/Aluno.cs
using System;

namespace FinalLab.Models // Certifica-te que o namespace está correto
{
    public class Aluno
    {
        // Propriedades públicas simples para serialização
        public string NomeCompleto { get; set; } = string.Empty;
        public string NumeroAluno { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Grupo { get; set; } = "Sem Grupo Atribuído";

        // Propriedade auxiliar (não precisa de setter)
        public string NomeCompletoNumero => $"{NomeCompleto} ({NumeroAluno})";

        // Construtor sem parâmetros necessário para serialização
        public Aluno() { }

        // Construtor principal para uso na aplicação
        public Aluno(string nomeCompleto, string numeroAluno, string email, string? grupo = null)
        {
            NomeCompleto = nomeCompleto ?? throw new ArgumentException("Nome completo é obrigatório.", nameof(nomeCompleto));
            NumeroAluno = string.IsNullOrWhiteSpace(numeroAluno) ? throw new ArgumentException("O número do aluno não pode ser vazio.", nameof(numeroAluno)) : numeroAluno;
            Email = email ?? throw new ArgumentException("Email é obrigatório.", nameof(email));
            Grupo = grupo ?? "Sem Grupo Atribuído";
        }
    }
}
