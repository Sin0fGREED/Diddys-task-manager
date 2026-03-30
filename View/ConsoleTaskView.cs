public class ConsoleTaskView<T>
{
    private readonly ITaskService<T> _service;
    public ConsoleTaskView(ITaskService<T> service)
    {
        _service = service;
    }
    void DisplayTasks(T[]? tasks)
    {
        if (tasks == null) return;
        Console.Clear();
        Console.WriteLine("==== ToDo List ====");
        foreach (var task in tasks)
        {
            if (task != null)
                Console.WriteLine($"ID: {task.GetType().GetProperty("Id")?.GetValue(task) ?? "N/A"}, Description: {task.GetType().GetProperty("Description")?.GetValue(task) ?? "N/A"}, Completed: {task.GetType().GetProperty("Completed")?.GetValue(task) ?? "N/A"}, Priority: {task.GetType().GetProperty("Priority")?.GetValue(task) ?? "N/A"}, Created: {task.GetType().GetProperty("CreationDate")?.GetValue(task) ?? "N/A"}");
        }
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
            Console.WriteLine("5. Update Task");
            Console.WriteLine("6. Exit");
            string option = Prompt("Select an option: ");
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
                    string updateIdStr = Prompt("Enter task id to update: ");
                    if (int.TryParse(updateIdStr, out int updateId))
                    {
                        Console.WriteLine("What do you want to update?");
                        Console.WriteLine("1. Task Name");
                        Console.WriteLine("2. Priority");
                        Console.WriteLine("3. Both");
                        string updateChoice = Prompt("Choose option (1-3): ");
                        string? newDescription = null;
                        PriorityLevel? newPriority = null;
                        if (updateChoice == "1" || updateChoice == "3")
                        {
                            newDescription = Prompt("Enter new task name/description: ");
                        }
                        if (updateChoice == "2" || updateChoice == "3")
                        {
                            Console.WriteLine("Select new priority:");
                            Console.WriteLine("1. Low");
                            Console.WriteLine("2. Medium");
                            Console.WriteLine("3. High");
                            string newPriorityInput = Prompt("Choose priority (1-3): ");
                            switch (newPriorityInput)
                            {
                                case "1": newPriority = PriorityLevel.Low; break;
                                case "2": newPriority = PriorityLevel.Medium; break;
                                case "3": newPriority = PriorityLevel.High; break;
                                default: Console.WriteLine("Invalid input, keeping current priority."); break;
                            }
                        }
                        _service.UpdateTask(updateId, newDescription, newPriority);
                        Console.WriteLine("Task updated. Press any key to continue...");
                        Console.ReadKey();
                    }
                    else
                    {
                        Console.WriteLine("Invalid task id. Press any key to continue...");
                        Console.ReadKey();
                    }
                    break;
                case "6":
                    return;
                default:
                    Console.WriteLine("Invalid option. Press any key to continue...");
                    Console.ReadKey();
                    break;
            }
        }
    }
}