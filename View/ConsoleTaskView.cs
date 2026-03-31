public class ConsoleTaskView<T> : ITaskView
{
    private readonly ITaskService<T> _service;
    private readonly IUserContext _userContext;

    public ConsoleTaskView(ITaskService<T> service, IUserContext userContext)
    {
        _service = service;
        _userContext = userContext;
    }

    void DisplayTasks(T[]? tasks)
    {
        if (tasks == null) return;
        Console.Clear();
        Console.WriteLine($"==== ToDo List (Logged in as: {_userContext.CurrentUsername}) ====");
        foreach (var task in tasks)
        {
            if (task != null)
            {
                var taskObj = task as TaskItem;
                Console.WriteLine($"ID: {taskObj?.Id ?? 0}, Description: {taskObj?.Description ?? "N/A"}, Completed: {taskObj?.Completed ?? false}, Priority: {taskObj?.Priority ?? PriorityLevel.Medium}, CreatedBy: {taskObj?.CreatedBy ?? "N/A"}, AssignedTo: {taskObj?.AssignedTo ?? "Unassigned"}");
            }
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
            Console.WriteLine("6. Assign Task");
            Console.WriteLine("7. Unassign Task");
            Console.WriteLine("8. View My Tasks (Assigned to you)");
            Console.WriteLine("9. View My Created Tasks");
            Console.WriteLine("10. Exit");
            string option = Prompt("Select an option: ");
            switch (option)
            {
                case "1":
                    string description = Prompt("Enter task description: ");
                    if (string.IsNullOrWhiteSpace(description))
                    {
                        Console.WriteLine("Error: Task description cannot be empty.");
                        Console.ReadKey();
                        break;
                    }
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
                        default:
                            Console.WriteLine("Invalid input, defaulting to Medium.");
                            break;
                    }
                    _service.AddTask(description, priority);
                    Console.WriteLine("Task added successfully. Press any key to continue...");
                    Console.ReadKey();
                    break;
                case "2":
                    string removeIdStr = Prompt("Enter task id to remove: ");
                    if (string.IsNullOrWhiteSpace(removeIdStr))
                    {
                        Console.WriteLine("Error: Task ID cannot be empty.");
                        Console.ReadKey();
                        break;
                    }
                    if (int.TryParse(removeIdStr, out int removeId))
                    {
                        if (removeId <= 0)
                        {
                            Console.WriteLine("Error: Task ID must be greater than 0.");
                            Console.ReadKey();
                            break;
                        }
                        _service.RemoveTask(removeId);
                        Console.WriteLine("Task removed successfully. Press any key to continue...");
                        Console.ReadKey();
                    }
                    else
                    {
                        Console.WriteLine("Error: Invalid task ID format.");
                        Console.ReadKey();
                    }
                    break;
                case "3":
                    string toggleIdStr = Prompt("Enter task id to toggle: ");
                    if (string.IsNullOrWhiteSpace(toggleIdStr))
                    {
                        Console.WriteLine("Error: Task ID cannot be empty.");
                        Console.ReadKey();
                        break;
                    }
                    if (int.TryParse(toggleIdStr, out int toggleId))
                    {
                        if (toggleId <= 0)
                        {
                            Console.WriteLine("Error: Task ID must be greater than 0.");
                            Console.ReadKey();
                            break;
                        }
                        _service.ToggleTaskCompletion(toggleId);
                        Console.WriteLine("Task status toggled. Press any key to continue...");
                        Console.ReadKey();
                    }
                    else
                    {
                        Console.WriteLine("Error: Invalid task ID format.");
                        Console.ReadKey();
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
                    if (string.IsNullOrWhiteSpace(updateIdStr))
                    {
                        Console.WriteLine("Error: Task ID cannot be empty.");
                        Console.ReadKey();
                        break;
                    }
                    if (int.TryParse(updateIdStr, out int updateId))
                    {
                        if (updateId <= 0)
                        {
                            Console.WriteLine("Error: Task ID must be greater than 0.");
                            Console.ReadKey();
                            break;
                        }
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
                            if (string.IsNullOrWhiteSpace(newDescription))
                            {
                                Console.WriteLine("Error: Description cannot be empty.");
                                Console.ReadKey();
                                break;
                            }
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
                                default:
                                    Console.WriteLine("Invalid input, keeping current priority.");
                                    break;
                            }
                        }
                        _service.UpdateTask(updateId, newDescription, newPriority);
                        Console.WriteLine("Task updated. Press any key to continue...");
                        Console.ReadKey();
                    }
                    else
                    {
                        Console.WriteLine("Error: Invalid task ID format.");
                        Console.ReadKey();
                    }
                    break;
                case "6":
                    string assignIdStr = Prompt("Enter task id to assign: ");
                    if (string.IsNullOrWhiteSpace(assignIdStr))
                    {
                        Console.WriteLine("Error: Task ID cannot be empty.");
                        Console.ReadKey();
                        break;
                    }
                    if (int.TryParse(assignIdStr, out int assignId))
                    {
                        if (assignId <= 0)
                        {
                            Console.WriteLine("Error: Task ID must be greater than 0.");
                            Console.ReadKey();
                            break;
                        }
                        string assigneeName = Prompt("Enter assignee username: ");
                        if (string.IsNullOrWhiteSpace(assigneeName))
                        {
                            Console.WriteLine("Error: Username cannot be empty.");
                            Console.ReadKey();
                            break;
                        }
                        _service.AssignTask(assignId, assigneeName);
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                    }
                    else
                    {
                        Console.WriteLine("Error: Invalid task ID format.");
                        Console.ReadKey();
                    }
                    break;
                case "7":
                    string unassignIdStr = Prompt("Enter task id to unassign: ");
                    if (string.IsNullOrWhiteSpace(unassignIdStr))
                    {
                        Console.WriteLine("Error: Task ID cannot be empty.");
                        Console.ReadKey();
                        break;
                    }
                    if (int.TryParse(unassignIdStr, out int unassignId))
                    {
                        if (unassignId <= 0)
                        {
                            Console.WriteLine("Error: Task ID must be greater than 0.");
                            Console.ReadKey();
                            break;
                        }
                        _service.UnassignTask(unassignId);
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                    }
                    else
                    {
                        Console.WriteLine("Error: Invalid task ID format.");
                        Console.ReadKey();
                    }
                    break;
                case "8":
                    Console.Clear();
                    Console.WriteLine($"==== Tasks Assigned to {_userContext.CurrentUsername} ====");
                    var assignedTasks = _service.GetTasksAssignedToUser(_userContext.CurrentUsername ?? "Unknown");
                    if (assignedTasks.Length == 0)
                    {
                        Console.WriteLine("No tasks assigned to you.");
                    }
                    else
                    {
                        foreach (var task in assignedTasks)
                        {
                            var taskObj = task as TaskItem;
                            Console.WriteLine($"ID: {taskObj?.Id}, Description: {taskObj?.Description}, Completed: {taskObj?.Completed}, Priority: {taskObj?.Priority}");
                        }
                    }
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    break;
                case "9":
                    Console.Clear();
                    Console.WriteLine($"==== Tasks Created by {_userContext.CurrentUsername} ====");
                    var createdTasks = _service.GetTasksCreatedByUser(_userContext.CurrentUsername ?? "Unknown");
                    if (createdTasks.Length == 0)
                    {
                        Console.WriteLine("No tasks created by you.");
                    }
                    else
                    {
                        foreach (var task in createdTasks)
                        {
                            var taskObj = task as TaskItem;
                            Console.WriteLine($"ID: {taskObj?.Id}, Description: {taskObj?.Description}, Completed: {taskObj?.Completed}, AssignedTo: {taskObj?.AssignedTo ?? "Unassigned"}");
                        }
                    }
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    break;
                case "10":
                    Console.WriteLine("Goodbye!");
                    return;
                default:
                    Console.WriteLine("Invalid option. Press any key to continue...");
                    Console.ReadKey();
                    break;
            }
        }
    }
}