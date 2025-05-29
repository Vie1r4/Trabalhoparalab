using System;
using System.IO;

public static class AppDataPaths
{
    private static readonly string BaseDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "FinalLab"
    );

    public static string GruposFile => Path.Combine(BaseDir, "grupos.json");
    public static string AlunosFile => Path.Combine(BaseDir, "alunos.json");
    public static string TarefasFile => Path.Combine(BaseDir, "tarefas.json");
    public static string NotasFile => Path.Combine(BaseDir, "notas.json");

    static AppDataPaths()
    {
        // Garante que a pasta existe
        if (!Directory.Exists(BaseDir))
            Directory.CreateDirectory(BaseDir);
    }
}