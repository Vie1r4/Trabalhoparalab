// DataStorage.cs (na raiz do projeto FinalLab)
using System;
using System.IO;
using System.Text.Json; // Usar System.Text.Json
using System.Text.Encodings.Web; // Para JavaScriptEncoder
using System.Diagnostics; // Para Debug.WriteLine
using System.Text; // Para Encoding

public static class DataStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Permite caracteres como '�' sem escapar
    };

    public static void SaveToFile<T>(string filePath, T data)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.WriteLine("ERRO: Caminho do ficheiro para salvar � nulo ou vazio.");
            return;
        }
        if (data == null)
        {
            Debug.WriteLine($"AVISO: Dados para salvar em '{filePath}' s�o nulos. O ficheiro n�o ser� modificado ou ser� limpo se j� existir.");
            // Opcional: decidir se quer limpar o ficheiro ou n�o fazer nada
            // File.WriteAllText(filePath, "null", Encoding.UTF8); // Exemplo de limpar com "null"
            return;
        }

        try
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                Debug.WriteLine($"Diret�rio criado: {dir}");
            }

            var json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(filePath, json, Encoding.UTF8); // Especificar UTF-8
            Debug.WriteLine($"Dados salvos com sucesso em: {filePath}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERRO GRAVE ao salvar dados em '{filePath}': {ex.Message}");
            Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            // Em uma aplica��o real, poderia lan�ar uma exce��o customizada ou logar para um sistema de logging
        }
    }

    public static T? LoadFromFile<T>(string filePath) where T : class // Adicionar restri��o de classe para default ser null
    {
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.WriteLine("ERRO: Caminho do ficheiro para carregar � nulo ou vazio.");
            return default; // ou null
        }

        try
        {
            if (!File.Exists(filePath))
            {
                Debug.WriteLine($"AVISO: Ficheiro n�o encontrado para carregar: {filePath}. Retornando default.");
                return default; // ou null
            }

            var json = File.ReadAllText(filePath, Encoding.UTF8); // Especificar UTF-8
            if (string.IsNullOrWhiteSpace(json) || json.Trim().Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine($"AVISO: Ficheiro '{filePath}' est� vazio ou cont�m 'null'. Retornando default.");
                return default; // ou null
            }

            T? loadedData = JsonSerializer.Deserialize<T>(json, JsonOptions);
            Debug.WriteLine($"Dados carregados com sucesso de: {filePath}");
            return loadedData;
        }
        catch (JsonException jsonEx)
        {
            Debug.WriteLine($"ERRO DE JSON ao carregar dados de '{filePath}': {jsonEx.Message}. O ficheiro pode estar corrompido.");
            Debug.WriteLine($"Linha: {jsonEx.LineNumber}, Posi��o: {jsonEx.BytePositionInLine}, Caminho: {jsonEx.Path}");
            // Opcional: tentar criar um backup do ficheiro corrompido
            // File.Move(filePath, filePath + ".corrupted." + DateTime.Now.ToString("yyyyMMddHHmmss"));
            return default; // ou null
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERRO GRAVE ao carregar dados de '{filePath}': {ex.Message}");
            Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            return default; // ou null
        }
    }
}
