public class ConsoleTaskView<T>
{
    private readonly ITaskService<T> _service;
    public ConsoleTaskView(ITaskService<T> service)
    {
        _service = service;
    }
    void DisplayTasks(T[] tasks)
    {
        Console.Clear();
        Console.WriteLine("==== ToDo List ====");
        foreach (var task in tasks)
        Console.WriteLine($"{task}");
    }
    string Prompt(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? string.Empty;
    }
    public void Run()
    {
        while (true)
        {
            DisplayTasks(_service.GetAllTasks());
            Console.WriteLine("\nOptions:");
            Console.WriteLine("1. Add Task");
            Console.WriteLine("2. Remove Task");
            Console.WriteLine("3. Toggle Task State");
            Console.WriteLine("4. List Tasks");
            Console.WriteLine("5. Exit");
            string option=Prompt("Select an option: ");
            switch (option)
            {
                case "1":
                    string description = Prompt("Enter task description: ");
                    Console.WriteLine("Select priority:");
                    Console.WriteLine("1. Low");
                    Console.WriteLine("2. Medium");
                    Console.WriteLine("3. High");
                    string priorityInput = Prompt("Choose priority (1-3): ");
                    PriorityLevel priority = PriorityLevel.Medium;
                    switch (priorityInput)
                    {
                        case "1": priority = PriorityLevel.Low; break;
                        case "2": priority = PriorityLevel.Medium; break;
                        case "3": priority = PriorityLevel.High; break;
                        default: Console.WriteLine("Invalid input, defaulting to Medium."); break;
                    }
                    _service.AddTask(description, priority);
                    break;
                case "2":
                    string removeIdStr = Prompt("Enter task id to remove: ");
                    if (int.TryParse(removeIdStr, out int removeId))
                    {
                        _service.RemoveTask(removeId);
                    }
                    break;
                case "3":
                    string toggleIdStr = Prompt("Enter task id to toggle: ");
                    if (int.TryParse(toggleIdStr, out int toggleId))
                    {
                        _service.ToggleTaskCompletion(toggleId);
                    }
                    break;
                case "4":
                    Console.WriteLine("How would you like to filter tasks?");
                    Console.WriteLine("1. By Priority (High → Low)");
                    Console.WriteLine("2. By Status (Completed → Pending)");
                    Console.WriteLine("3. By Creation Date");
                    Console.WriteLine("4. No Filter (show all)");
                    string filterOption = Prompt("Choose filter option (1-4): ");
                    string? filterBy = null;
                    string? filterValue = null;
                    string? sortBy = null;
                    switch (filterOption)
                    {
                        case "1":
                            sortBy = "prioritydesc";
                            break;
                        case "2":
                            sortBy = "statusdesc";
                            break;
                        case "3":
                            sortBy = "creationdate";
                            break;
                        case "4":
                            break;
                        default:
                            Console.WriteLine("Invalid input, showing all tasks.");
                            break;
                    }
                    Console.WriteLine("Tasks: ");
                    _service.ListTasks(filterBy, filterValue, sortBy);
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    break;
                case "5":
                    return;
                default:
                    Console.WriteLine("Invalid option. Press any key to continue...");
                    Console.ReadKey();
                    break;
            }
        }
    }
}