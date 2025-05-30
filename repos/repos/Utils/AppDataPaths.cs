// AppDataPaths.cs (na raiz do projeto FinalLab)
using System;
using System.IO;
using System.Diagnostics; // Para Debug.WriteLine

public static class AppDataPaths
{
    private static readonly string BaseDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "FinalLab" // Nome da tua aplica��o
    );

    // Caminhos para os ficheiros de dados
    public static string GruposFile => Path.Combine(BaseDir, "grupos.json");
    public static string AlunosFile => Path.Combine(BaseDir, "alunos.json");
    public static string TarefasFile => Path.Combine(BaseDir, "tarefas.json");
    public static string NotasFile => Path.Combine(BaseDir, "notas.json");
    public static string PerfilFile => Path.Combine(BaseDir, "perfil.json"); // Adicionado para o perfil

    // Construtor est�tico para garantir que a pasta base existe
    static AppDataPaths()
    {
        try
        {
            if (!Directory.Exists(BaseDir))
            {
                Directory.CreateDirectory(BaseDir);
                Debug.WriteLine($"Pasta de dados criada em: {BaseDir}");
            }
            else
            {
                Debug.WriteLine($"Pasta de dados j� existe em: {BaseDir}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERRO ao criar/verificar diret�rio BaseDir ({BaseDir}): {ex.Message}");
            // Considerar lan�ar uma exce��o ou notificar o utilizador de forma mais robusta
            // dependendo da criticidade de n�o conseguir aceder/criar esta pasta.
        }
    }
}
