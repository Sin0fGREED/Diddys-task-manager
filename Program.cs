class Program
{
    static void Main(string[] args)
    {
        string filePath = "tasks.json";
        ITaskRepository repository = new JsonTaskRepository(filePath);
        ITaskService service = new TaskService(repository);
        ITaskView view = new ConsoleTaskView(service);
        view.Run();
    }
}