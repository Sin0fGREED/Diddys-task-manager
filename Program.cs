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

        IUserContext userContext = new UserContext();

        Console.Clear();
        Console.WriteLine("==== TASK MANAGER LOGIN ====");
        bool loggedIn = false;
        while (!loggedIn)
        {
            Console.Write("Enter username: ");
            string username = Console.ReadLine() ?? "";
            
            if (!string.IsNullOrEmpty(username))
            {
                userContext.SetCurrentUser(username);
                loggedIn = true;
                Console.WriteLine($"Welcome, {username}!");
                System.Threading.Thread.Sleep(1000);
            }
            else
            {
                Console.WriteLine("Username cannot be empty. Please try again.");
            }
        }

        ITaskService<TaskItem> service = new TaskService<TaskItem>(repository, userContext, collection);
        ConsoleTaskView<TaskItem> view = new ConsoleTaskView<TaskItem>(service, userContext);
        view.Run();
    }
}