// FinalLab/Models/Perfil.cs
using System.Diagnostics; // Para Debug.WriteLine

namespace FinalLab.Models
{
    public class Perfil
    {
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? CaminhoFoto { get; set; }

        // Construtor vazio é importante para a deserialização JSON
        public Perfil()
        {
            // Debug.WriteLine("Construtor Perfil() chamado.");
        }

        public Perfil(string nome, string email, string caminhoFoto)
        {
            Nome = nome;
            Email = email;
            CaminhoFoto = caminhoFoto;
            // Debug.WriteLine($"Construtor Perfil(nome, email, foto) chamado: {Nome}, {Email}, {CaminhoFoto}");
        }
    }
}
