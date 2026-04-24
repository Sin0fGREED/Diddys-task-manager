public class ConsoleTaskView<T> : ITaskView
{
    private readonly ITaskService<T> _service;
    private readonly IUserContext _userContext;

    public ConsoleTaskView(ITaskService<T> service, IUserContext userContext)
    {
        _service = service;
        _userContext = userContext;
    }

    void PrintTitle()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════╗
║                                                           ║
║         ██████╗ ██╗██████╗ ██████╗ ██╗   ██╗███████╗    ║
║         ██╔══██╗██║██╔══██╗██╔══██╗╚██╗ ██╔╝██╔════╝    ║
║         ██║  ██║██║██║  ██║██║  ██║ ╚████╔╝ ███████╗    ║
║         ██║  ██║██║██║  ██║██║  ██║  ╚██╔╝  ╚════██║    ║
║         ██████╔╝██║██████╔╝██████╔╝   ██║   ███████║    ║
║         ╚═════╝ ╚═╝╚═════╝ ╚═════╝    ╚═╝   ╚══════╝    ║
║                                                           ║
║              TASK MANAGER 📋 for DIDDY                   ║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝
");
        Console.ResetColor();
    }

    void PrintPageHeader(string pageTitle)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"┌──────────────────── PAGE: {pageTitle,-35} ──────┐");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"│ User: {_userContext.CurrentUsername,-65} │");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("└────────────────────────────────────────────────────────────────────┘\n");
        Console.ResetColor();
    }

    void PrintColoredText(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ {message}");
        Console.ResetColor();
    }

    void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"✗ {message}");
        Console.ResetColor();
    }

    void PrintInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"ℹ {message}");
        Console.ResetColor();
    }

    void DisplayTasks(T[]? tasks)
    {
        if (tasks == null || tasks.Length == 0) return;

        Console.WriteLine($"\n╔════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine($"║  ToDo List (Logged in as: {_userContext.CurrentUsername,-50} ║");
        Console.WriteLine($"╚════════════════════════════════════════════════════════════════════╝\n");

        // Column widths
        int idWidth = 4;
        int descWidth = 25;
        int statusWidth = 10;
        int priorityWidth = 8;
        int createdByWidth = 12;
        int assignedToWidth = 12;
        int depsWidth = 14;

        // Header
        string idHeader = "ID".PadRight(idWidth);
        string descHeader = "Description".PadRight(descWidth);
        string statusHeader = "Status".PadRight(statusWidth);
        string priorityHeader = "Priority".PadRight(priorityWidth);
        string createdByHeader = "Created By".PadRight(createdByWidth);
        string assignedToHeader = "Assigned To".PadRight(assignedToWidth);
        string depsHeader = "Depends On".PadRight(depsWidth);

        string header = idHeader + " | " + descHeader + " | " + statusHeader + " | " + priorityHeader + " | " + createdByHeader + " | " + assignedToHeader + " | " + depsHeader;
        string separator = new string('─', header.Length);
        Console.WriteLine("┌" + separator + "┐");
        Console.WriteLine("│ " + header + " │");
        Console.WriteLine("├" + separator + "┤");

        // Rows
        foreach (var task in tasks)
        {
            if (task != null)
            {
                var taskObj = task as TaskItem;
                string id = (taskObj?.Id.ToString() ?? "?").PadRight(idWidth);

                string descStr = taskObj?.Description ?? "N/A";
                if (descStr.Length > descWidth)
                    descStr = descStr.Substring(0, descWidth - 3) + "...";
                string desc = descStr.PadRight(descWidth);

                string status = ((taskObj?.Status ?? StatusLevel.ToDo) switch { StatusLevel.ToDo => "⧖ To Do", StatusLevel.InProgress => "▶ In Progress", StatusLevel.Done => "✓ Done", _ => "⧖ To Do" }).PadRight(statusWidth);
                string priority = (taskObj?.Priority ?? PriorityLevel.Medium).ToString().PadRight(priorityWidth);
                string createdBy = (taskObj?.CreatedBy ?? "Unknown").PadRight(createdByWidth);
                string assignedTo = (taskObj?.AssignedTo ?? "Unassigned").PadRight(assignedToWidth);

                // Build dependency string (e.g. "→ 1,3" or "none")
                string depsStr = "none";
                if (taskObj?.DependsOnTaskIds != null && taskObj.DependsOnTaskIds.Length > 0)
                {
                    depsStr = "→ ";
                    for (int i = 0; i < taskObj.DependsOnTaskIds.Length; i++)
                        depsStr += (i == 0 ? "" : ",") + taskObj.DependsOnTaskIds[i];
                }
                if (depsStr.Length > depsWidth) depsStr = depsStr.Substring(0, depsWidth - 3) + "...";
                string deps = depsStr.PadRight(depsWidth);

                string row = id + " | " + desc + " | " + status + " | " + priority + " | " + createdBy + " | " + assignedTo + " | " + deps;
                Console.WriteLine("│ " + row + " │");
            }
        }

        // Footer
        Console.WriteLine("└" + separator + "┘\n");
    }

    string Prompt(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? string.Empty;
    }

    public void Run()
    {
        Console.Clear();
        PrintTitle();
        PrintInfo("Welcome to Task Manager! Starting up...");
        System.Threading.Thread.Sleep(1000);

        while (true)
        {
            Console.Clear();
            PrintTitle();
            PrintPageHeader("Main Menu");
            DisplayTasks(_service.GetAllTasks());

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║          MAIN MENU OPTIONS             ║");
            Console.WriteLine("╠════════════════════════════════════════╣");
            Console.WriteLine("║ 1. Add Task                            ║");
            Console.WriteLine("║ 2. Remove Task                         ║");
            Console.WriteLine("║ 3. Change Task Status                  ║");
            Console.WriteLine("║ 4. List Tasks (with filters)           ║");
            Console.WriteLine("║ 5. Update Task                         ║");
            Console.WriteLine("║ 6. Assign Task                         ║");
            Console.WriteLine("║ 7. Unassign Task                       ║");
            Console.WriteLine("║ 8. View My Tasks (Assigned to you)     ║");
            Console.WriteLine("║ 9. View My Created Tasks               ║");
            Console.WriteLine("║ 10. Exit                               ║");
            Console.WriteLine("╠════════════════════════════════════════╣");
            Console.WriteLine("║ 11. Add Task Dependency                ║");
            Console.WriteLine("║ 12. Remove Task Dependency             ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            Console.ResetColor();
            string option = Prompt("\nSelect an option (1-12): ");
            switch (option)
            {
                case "1":
                    Console.Clear();
                    PrintTitle();
                    PrintPageHeader("Add Task");
                    string description = Prompt("Enter task description: ");
                    if (string.IsNullOrWhiteSpace(description))
                    {
                        PrintError("Task description cannot be empty.");
                        Console.ReadKey();
                        break;
                    }
                    PrintInfo("Select priority:");
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
                            PrintError("Invalid input, defaulting to Medium.");
                            break;
                    }
                    _service.AddTask(description, priority);
                    PrintSuccess("Task added successfully!");
                    Console.ReadKey();
                    break;
                case "2":
                    Console.Clear();
                    PrintTitle();
                    PrintPageHeader("Remove Task");
                    string removeIdStr = Prompt("Enter task id to remove: ");
                    if (string.IsNullOrWhiteSpace(removeIdStr))
                    {
                        PrintError("Task ID cannot be empty.");
                        Console.ReadKey();
                        break;
                    }
                    if (int.TryParse(removeIdStr, out int removeId))
                    {
                        if (removeId <= 0)
                        {
                            PrintError("Task ID must be greater than 0.");
                            Console.ReadKey();
                            break;
                        }
                        _service.RemoveTask(removeId);
                        PrintSuccess("Task removed successfully!");
                        Console.ReadKey();
                    }
                    else
                    {
                        PrintError("Invalid task ID format.");
                        Console.ReadKey();
                    }
                    break;
                case "3":
                    Console.Clear();
                    PrintTitle();
                    PrintPageHeader("Change Task Status");
                    string toggleIdStr = Prompt("Enter task id to change status: ");
                    if (string.IsNullOrWhiteSpace(toggleIdStr))
                    {
                        PrintError("Task ID cannot be empty.");
                        Console.ReadKey();
                        break;
                    }
                    if (int.TryParse(toggleIdStr, out int toggleId))
                    {
                        if (toggleId <= 0)
                        {
                            PrintError("Task ID must be greater than 0.");
                            Console.ReadKey();
                            break;
                        }
                        PrintInfo("Select new status:");
                        Console.WriteLine("1. To Do");
                        Console.WriteLine("2. In Progress");
                        Console.WriteLine("3. Done");
                        string statusInput = Prompt("Choose status (1-3): ");
                        StatusLevel newStatus = StatusLevel.ToDo;
                        switch (statusInput)
                        {
                            case "1": newStatus = StatusLevel.ToDo; break;
                            case "2": newStatus = StatusLevel.InProgress; break;
                            case "3": newStatus = StatusLevel.Done; break;
                            default:
                                PrintError("Invalid input, defaulting to To Do.");
                                break;
                        }
                        _service.SetTaskStatus(toggleId, newStatus);
                        PrintSuccess("Task status updated!");
                        Console.ReadKey();
                    }
                    else
                    {
                        PrintError("Invalid task ID format.");
                        Console.ReadKey();
                    }
                    break;
                case "4":
                    Console.Clear();
                    PrintTitle();
                    PrintPageHeader("Filtered Tasks");
                    PrintInfo("How would you like to filter tasks?");
                    Console.WriteLine("1. By Priority (High → Low)");
                    Console.WriteLine("2. By Status (Done → To Do)");
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
                            PrintError("Invalid input, showing all tasks.");
                            break;
                    }
                    Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════════╗");
                    Console.WriteLine("║                        FILTERED TASK LIST                          ║");
                    Console.WriteLine("╚═══════════════════════════════════════════════════════════════════╝\n");
                    _service.ListTasks(filterBy, filterValue, sortBy);
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    break;
                case "5":
                    Console.Clear();
                    PrintTitle();
                    PrintPageHeader("Update Task");
                    string updateIdStr = Prompt("Enter task id to update: ");
                    if (string.IsNullOrWhiteSpace(updateIdStr))
                    {
                        PrintError("Task ID cannot be empty.");
                        Console.ReadKey();
                        break;
                    }
                    if (int.TryParse(updateIdStr, out int updateId))
                    {
                        if (updateId <= 0)
                        {
                            PrintError("Task ID must be greater than 0.");
                            Console.ReadKey();
                            break;
                        }
                        PrintInfo("What do you want to update?");
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
                                PrintError("Description cannot be empty.");
                                Console.ReadKey();
                                break;
                            }
                        }
                        if (updateChoice == "2" || updateChoice == "3")
                        {
                            PrintInfo("Select new priority:");
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
                                    PrintError("Invalid input, keeping current priority.");
                                    break;
                            }
                        }
                        _service.UpdateTask(updateId, newDescription, newPriority);
                        PrintSuccess("Task updated!");
                        Console.ReadKey();
                    }
                    else
                    {
                        PrintError("Invalid task ID format.");
                        Console.ReadKey();
                    }
                    break;
                case "6":
                    Console.Clear();
                    PrintTitle();
                    PrintPageHeader("Assign Task");
                    string assignIdStr = Prompt("Enter task id to assign: ");
                    if (string.IsNullOrWhiteSpace(assignIdStr))
                    {
                        PrintError("Task ID cannot be empty.");
                        Console.ReadKey();
                        break;
                    }
                    if (int.TryParse(assignIdStr, out int assignId))
                    {
                        if (assignId <= 0)
                        {
                            PrintError("Task ID must be greater than 0.");
                            Console.ReadKey();
                            break;
                        }
                        string assigneeName = Prompt("Enter assignee username: ");
                        if (string.IsNullOrWhiteSpace(assigneeName))
                        {
                            PrintError("Username cannot be empty.");
                            Console.ReadKey();
                            break;
                        }
                        _service.AssignTask(assignId, assigneeName);
                        PrintSuccess("Task assigned successfully!");
                        Console.ReadKey();
                    }
                    else
                    {
                        PrintError("Invalid task ID format.");
                        Console.ReadKey();
                    }
                    break;
                case "7":
                    Console.Clear();
                    PrintTitle();
                    PrintPageHeader("Unassign Task");
                    string unassignIdStr = Prompt("Enter task id to unassign: ");
                    if (string.IsNullOrWhiteSpace(unassignIdStr))
                    {
                        PrintError("Task ID cannot be empty.");
                        Console.ReadKey();
                        break;
                    }
                    if (int.TryParse(unassignIdStr, out int unassignId))
                    {
                        if (unassignId <= 0)
                        {
                            PrintError("Task ID must be greater than 0.");
                            Console.ReadKey();
                            break;
                        }
                        _service.UnassignTask(unassignId);
                        PrintSuccess("Task unassigned successfully!");
                        Console.ReadKey();
                    }
                    else
                    {
                        PrintError("Invalid task ID format.");
                        Console.ReadKey();
                    }
                    break;
                case "8":
                    Console.Clear();
                    Console.WriteLine($"\n╔════════════════════════════════════════════════════════════════════╗");
                    Console.WriteLine($"║  Tasks Assigned to: {_userContext.CurrentUsername,-50} ║");
                    Console.WriteLine($"╚════════════════════════════════════════════════════════════════════╝\n");
                    var assignedTasks = _service.GetTasksAssignedToUser(_userContext.CurrentUsername ?? "Unknown");
                    if (assignedTasks.Length == 0)
                    {
                        Console.WriteLine("  ✗  No tasks assigned to you.\n");
                    }
                    else
                    {
                        int idWidth = 4;
                        int descWidth = 25;
                        int statusWidth = 10;
                        int priorityWidth = 8;

                        string idHeader = "ID".PadRight(idWidth);
                        string descHeader = "Description".PadRight(descWidth);
                        string statusHeader = "Status".PadRight(statusWidth);
                        string priorityHeader = "Priority".PadRight(priorityWidth);

                        string header = idHeader + " | " + descHeader + " | " + statusHeader + " | " + priorityHeader;
                        string separator = new string('─', header.Length);

                        Console.WriteLine("┌" + separator + "┐");
                        Console.WriteLine("│ " + header + " │");
                        Console.WriteLine("├" + separator + "┤");

                        foreach (var task in assignedTasks)
                        {
                            var taskObj = task as TaskItem;
                            string id = (taskObj?.Id.ToString() ?? "?").PadRight(idWidth);

                            string descStr = taskObj?.Description ?? "N/A";
                            if (descStr.Length > descWidth)
                                descStr = descStr.Substring(0, descWidth - 3) + "...";
                            string desc = descStr.PadRight(descWidth);

                            string status = ((taskObj?.Status ?? StatusLevel.ToDo) switch { StatusLevel.ToDo => "⧖ To Do", StatusLevel.InProgress => "▶ In Progress", StatusLevel.Done => "✓ Done", _ => "⧖ To Do" }).PadRight(statusWidth);
                            string priorityStr = (taskObj?.Priority ?? PriorityLevel.Medium).ToString().PadRight(priorityWidth);

                            string row = id + " | " + desc + " | " + status + " | " + priorityStr;
                            Console.WriteLine("│ " + row + " │");
                        }
                        Console.WriteLine("└" + separator + "┘\n");
                    }
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    break;
                case "9":
                    Console.Clear();
                    Console.WriteLine($"\n╔════════════════════════════════════════════════════════════════════╗");
                    Console.WriteLine($"║  Tasks Created by: {_userContext.CurrentUsername,-50} ║");
                    Console.WriteLine($"╚════════════════════════════════════════════════════════════════════╝\n");
                    var createdTasks = _service.GetTasksCreatedByUser(_userContext.CurrentUsername ?? "Unknown");
                    if (createdTasks.Length == 0)
                    {
                        Console.WriteLine("  ✗  No tasks created by you.\n");
                    }
                    else
                    {
                        int idWidth = 4;
                        int descWidth = 25;
                        int statusWidth = 10;
                        int assignedToWidth = 12;

                        string idHeader = "ID".PadRight(idWidth);
                        string descHeader = "Description".PadRight(descWidth);
                        string statusHeader = "Status".PadRight(statusWidth);
                        string assignedToHeader = "Assigned To".PadRight(assignedToWidth);

                        string header = idHeader + " | " + descHeader + " | " + statusHeader + " | " + assignedToHeader;
                        string separator = new string('─', header.Length);

                        Console.WriteLine("┌" + separator + "┐");
                        Console.WriteLine("│ " + header + " │");
                        Console.WriteLine("├" + separator + "┤");

                        foreach (var task in createdTasks)
                        {
                            var taskObj = task as TaskItem;
                            string id = (taskObj?.Id.ToString() ?? "?").PadRight(idWidth);

                            string descStr = taskObj?.Description ?? "N/A";
                            if (descStr.Length > descWidth)
                                descStr = descStr.Substring(0, descWidth - 3) + "...";
                            string desc = descStr.PadRight(descWidth);

                            string status = ((taskObj?.Status ?? StatusLevel.ToDo) switch { StatusLevel.ToDo => "⧖ To Do", StatusLevel.InProgress => "▶ In Progress", StatusLevel.Done => "✓ Done", _ => "⧖ To Do" }).PadRight(statusWidth);
                            string assignedTo = (taskObj?.AssignedTo ?? "Unassigned").PadRight(assignedToWidth);

                            string row = id + " | " + desc + " | " + status + " | " + assignedTo;
                            Console.WriteLine("│ " + row + " │");
                        }
                        Console.WriteLine("└" + separator + "┘\n");
                    }
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    break;
                case "11":
                    Console.Clear();
                    PrintTitle();
                    PrintPageHeader("Add Task Dependency");
                    PrintInfo("Specify which task must be completed BEFORE another task can be marked Done.");
                    string addDepTaskIdStr = Prompt("Enter task ID (the task that will have a prerequisite): ");
                    if (!int.TryParse(addDepTaskIdStr, out int addDepTaskId) || addDepTaskId <= 0)
                    {
                        PrintError("Invalid task ID.");
                        Console.ReadKey();
                        break;
                    }
                    string addDepPrereqIdStr = Prompt("Enter prerequisite task ID (must be Done first): ");
                    if (!int.TryParse(addDepPrereqIdStr, out int addDepPrereqId) || addDepPrereqId <= 0)
                    {
                        PrintError("Invalid prerequisite task ID.");
                        Console.ReadKey();
                        break;
                    }
                    _service.AddDependency(addDepTaskId, addDepPrereqId);
                    Console.ReadKey();
                    break;
                case "12":
                    Console.Clear();
                    PrintTitle();
                    PrintPageHeader("Remove Task Dependency");
                    string remDepTaskIdStr = Prompt("Enter task ID: ");
                    if (!int.TryParse(remDepTaskIdStr, out int remDepTaskId) || remDepTaskId <= 0)
                    {
                        PrintError("Invalid task ID.");
                        Console.ReadKey();
                        break;
                    }
                    string remDepPrereqIdStr = Prompt("Enter prerequisite task ID to remove: ");
                    if (!int.TryParse(remDepPrereqIdStr, out int remDepPrereqId) || remDepPrereqId <= 0)
                    {
                        PrintError("Invalid prerequisite task ID.");
                        Console.ReadKey();
                        break;
                    }
                    _service.RemoveDependency(remDepTaskId, remDepPrereqId);
                    Console.ReadKey();
                    break;
                case "10":
                    Console.Clear();
                    PrintTitle();
                    PrintSuccess("\n╔════════════════════════════════════════════════════════════════╗");
                    PrintSuccess("║          Thank you for using Task Manager! Goodbye!           ║");
                    PrintSuccess("╚════════════════════════════════════════════════════════════════╝\n");
                    System.Threading.Thread.Sleep(1500);
                    return;
                default:
                    Console.WriteLine("Invalid option. Press any key to continue...");
                    Console.ReadKey();
                    break;
            }
        }
    }
}