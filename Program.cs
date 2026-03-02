class Program
{
    static void Main(string[] args)
    {
        string filePath = "tasks.json";
        ITaskRepository<TaskItem> repository = new JsonTaskRepository<TaskItem>(filePath);
        ITaskService<TaskItem> service = new TaskService<TaskItem>(repository);
        ConsoleTaskView<TaskItem> view = new ConsoleTaskView<TaskItem>(service);
        view.Run();
    }
}