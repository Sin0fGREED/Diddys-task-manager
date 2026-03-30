using System.Text.Json;

public class JsonTaskRepository<T> : ITaskRepository<T>
{
    private readonly string _filePath;
    public JsonTaskRepository(string filePath) => _filePath = filePath;

    public ITaskCollection<T> LoadTasks()
    {
        var collection = new ArrayTaskCollection<T>();
        if (!File.Exists(_filePath))
        {
            return collection;
        }
        string json = File.ReadAllText(_filePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return collection;
        }
        var tasks = JsonSerializer.Deserialize<T[]>(json);
        if (tasks != null)
        {
            for (int i = 0; i < tasks.Length; i++)
                collection.Add(tasks[i]);
        }
        return collection;
    }

    public void SaveTasks(ITaskCollection<T> tasks)
    {
        string json = JsonSerializer.Serialize(tasks.ToArray(), new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}