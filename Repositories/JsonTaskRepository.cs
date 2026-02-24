using System.Text.Json;

public class JsonTaskRepository : ITaskRepository
{
    private readonly string _filePath;
    public JsonTaskRepository(string filePath) => _filePath = filePath;

    public TaskItem[] LoadTasks()
    {
        if (!File.Exists(_filePath))
        {
            return new TaskItem[0];
        }
        string json = File.ReadAllText(_filePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new TaskItem[0];
        }
        var tasks = JsonSerializer.Deserialize<TaskItem[]>(json);
        return tasks ?? new TaskItem[0];
    }

    public void SaveTasks(TaskItem[] tasks)
    {
        string json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}