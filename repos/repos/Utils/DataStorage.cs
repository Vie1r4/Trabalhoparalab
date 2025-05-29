using System;
using System.IO;
using System.Text.Json;

public static class DataStorage
{
    public static void SaveToFile<T>(string filePath, T data)
    {
        try
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao salvar arquivo '{filePath}': {ex.Message}");
            // Em produção, use um logger apropriado.
        }
    }

    public static T? LoadFromFile<T>(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) return default;
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao carregar arquivo '{filePath}': {ex.Message}");
            // Em produção, use um logger apropriado.
            return default;
        }
    }
}