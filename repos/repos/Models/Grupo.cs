// Ficheiro: Models/Grupo.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace FinalLab.Models
{
    public class Grupo
    {
        public Guid Id { get; private set; }
        public string Nome { get; set; }
        public List<Aluno> ListaDeAlunosNoGrupo { get; private set; }

        // Construtor principal para novos grupos ou quando o ID não é predefinido
        public Grupo(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome) && nome != "") // Permite nome vazio inicialmente para edição no TextBox
            {
                // Não lança exceção aqui se o nome for "" para permitir que o TextBox seja preenchido.
                // A validação do nome não vazio deve ser feita antes de guardar.
                // No entanto, um nome verdadeiramente nulo ou apenas com espaços ainda é problemático.
                // Para ser mais robusto, o nome só deve ser validado quando se tenta *usar* o grupo.
            }
            Id = Guid.NewGuid();
            Nome = nome;
            ListaDeAlunosNoGrupo = new List<Aluno>();
            Debug.WriteLine($"[Grupo.cs] Novo Grupo criado (construtor 1): Nome='{Nome}', ID={Id}");
        }

        // Construtor para criar um Grupo com um ID específico (usado na lógica de edição para manter o ID)
        public Grupo(Guid id, string nome) : this(nome) // Chama o construtor principal para definir Nome e Lista
        {
            this.Id = id; // Sobrescreve o ID gerado com o ID fornecido
            Debug.WriteLine($"[Grupo.cs] Grupo criado/copiado (construtor 2): Nome='{Nome}', ID={Id}");
        }

        public void AdicionarAluno(Aluno aluno)
        {
            if (aluno != null && !ListaDeAlunosNoGrupo.Any(a => a.NumeroAluno == aluno.NumeroAluno))
            { ListaDeAlunosNoGrupo.Add(aluno); Debug.WriteLine($"[Grupo.cs] Aluno '{aluno.NomeCompleto}' adicionado ao grupo '{this.Nome}'."); }
        }

        public void RemoverAluno(Aluno aluno)
        {
            if (aluno != null)
            {
                var alunoParaRemover = ListaDeAlunosNoGrupo.FirstOrDefault(a => a.NumeroAluno == aluno.NumeroAluno);
                if (alunoParaRemover != null) { ListaDeAlunosNoGrupo.Remove(alunoParaRemover); Debug.WriteLine($"[Grupo.cs] Aluno '{aluno.NomeCompleto}' removido do grupo '{this.Nome}'."); }
            }
        }
        public void LimparAlunos() { ListaDeAlunosNoGrupo.Clear(); Debug.WriteLine($"[Grupo.cs] Lista de alunos do grupo '{this.Nome}' limpa."); }
        public override string ToString() { return Nome; }
    }
}
