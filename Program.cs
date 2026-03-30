class Program
{
    static void Main(string[] args)
    {
        string filePath = "tasks.json";
        ITaskRepository<TaskItem> repository = new JsonTaskRepository<TaskItem>(filePath);

        Console.WriteLine("Choose data structure for tasks:");
        Console.WriteLine("1. Array");
        Console.WriteLine("2. Linked List");
        Console.Write("Enter choice (1 or 2): ");
        string choice = Console.ReadLine() ?? "";

        ITaskCollection<TaskItem> collection;
        if (choice == "2")
            collection = new LinkedListTaskCollection<TaskItem>();
        else
            collection = new ArrayTaskCollection<TaskItem>();

        ITaskCollection<TaskItem> loaded = repository.LoadTasks();
        TaskItem[] arr = loaded.ToArray();
        for (int i = 0; i < arr.Length; i++)
            collection.Add(arr[i]);

        ITaskService<TaskItem> service = new TaskService<TaskItem>(repository, collection);
        ConsoleTaskView<TaskItem> view = new ConsoleTaskView<TaskItem>(service);
        view.Run();
    }
}