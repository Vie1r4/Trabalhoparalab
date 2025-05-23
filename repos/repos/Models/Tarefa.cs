// Ficheiro: Models/Tarefa.cs
using System; // Necessário para Guid, DateTime, ArgumentException, ArgumentOutOfRangeException

namespace FinalLab.Models // Substitua FinalLab pelo namespace real do seu projeto, se diferente
{
    public class Tarefa
    {
        public Guid Id { get; }
        public string Titulo { get; }
        public string? Descricao { get; } // Anulável se a descrição pode não existir
        public DateTime DataHoraInicio { get; }
        public DateTime DataHoraTermino { get; }
        public int Peso { get; }

        public Tarefa(string titulo, string? descricao, DateTime dataHoraInicio, DateTime dataHoraTermino, int peso)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("O título da tarefa não pode ser vazio.", nameof(titulo));

            if (dataHoraTermino < dataHoraInicio)
                throw new ArgumentException("A data de término não pode ser anterior à data de início.", nameof(dataHoraTermino));

            if (peso < 0 || peso > 100)
                throw new ArgumentOutOfRangeException(nameof(peso), "O peso deve estar entre 0 e 100.");

            Id = Guid.NewGuid();
            Titulo = titulo;
            Descricao = descricao;
            DataHoraInicio = dataHoraInicio;
            DataHoraTermino = dataHoraTermino;
            Peso = peso;
        }
    }
}
