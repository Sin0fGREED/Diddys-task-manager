using System.Text.Json;

public class JsonTaskRepository<T> : ITaskRepository<T>
{
    private readonly string _filePath;
    public JsonTaskRepository(string filePath) => _filePath = filePath;

    public T[] LoadTasks()
    {
        if (!File.Exists(_filePath))
        {
            return new T[0];
        }
        string json = File.ReadAllText(_filePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new T[0];
        }
        var tasks = JsonSerializer.Deserialize<T[]>(json);
        return tasks ?? new T[0];
    }

    public void SaveTasks(T[] tasks)
    {
        string json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}