class TaskService<T> : ITaskService<T> where T : TaskItem
{
    private readonly ITaskRepository<T> _repository;
    private readonly IUserContext _userContext;
    private ITaskCollection<T> _tasks;

    public TaskService(ITaskRepository<T> repository, IUserContext userContext, ITaskCollection<T>? collection = null)
    {
        _repository = repository;
        _userContext = userContext;
        _tasks = collection ?? _repository.LoadTasks();
    }

    public T[] GetAllTasks() => _tasks.ToArray();

    public void AddTask(string description)
    {
        AddTask(description, PriorityLevel.Medium);
    }

    public void AddTask(string description, PriorityLevel priority)
    {
        int newId = 1;
        T[] arr = _tasks.ToArray();
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i].Id >= newId) newId = arr[i].Id + 1;
        }
        var newTask = new TaskItem { 
            Id = newId, 
            Description = description, 
            Completed = false, 
            Priority = priority, 
            CreationDate = DateTime.Now,
            CreatedBy = _userContext.CurrentUsername ?? "Unown"
        };
        _tasks.Add((T)(object)newTask);
        _repository.SaveTasks(_tasks);
    }

    public void RemoveTask(int id)
    {
        T found = _tasks.FindById(id);
        if (found != null)
        {
            if (!CanModifyTask(id, _userContext.CurrentUsername ?? "Unknown"))
            {
                Console.WriteLine("Error: You do not have permission to delete this task.");
                return;
            }
            _tasks.Remove(found);
            T[] arr = _tasks.ToArray();
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i].Id = i + 1;
            }
            _tasks.Clear();
            for (int i = 0; i < arr.Length; i++)
            {
                _tasks.Add(arr[i]);
            }
            _repository.SaveTasks(_tasks);
        }
    }

    public void ToggleTaskCompletion(int id)
    {
        T found = _tasks.FindById(id);
        if (found != null)
        {
            if (!CanModifyTask(id, _userContext.CurrentUsername ?? "Unknown"))
            {
                Console.WriteLine("Error: You do not have permission to modify this task.");
                return;
            }
            found.Completed = !found.Completed;
            _repository.SaveTasks(_tasks);
        }
    }

    private void PrintTaskTable(T[] tasks)
    {
        if (tasks.Length == 0)
        {
            Console.WriteLine("No tasks found.");
            return;
        }

        // Column widths
        int idWidth = 4;
        int descWidth = 25;
        int statusWidth = 10;
        int priorityWidth = 8;
        int createdByWidth = 12;
        int assignedToWidth = 12;

        // Header
        string idHeader = "ID".PadRight(idWidth);
        string descHeader = "Description".PadRight(descWidth);
        string statusHeader = "Status".PadRight(statusWidth);
        string priorityHeader = "Priority".PadRight(priorityWidth);
        string createdByHeader = "Created By".PadRight(createdByWidth);
        string assignedToHeader = "Assigned To".PadRight(assignedToWidth);
        
        string header = idHeader + " | " + descHeader + " | " + statusHeader + " | " + priorityHeader + " | " + createdByHeader + " | " + assignedToHeader;
        string separator = new string('─', header.Length);
        
        Console.WriteLine("┌" + separator + "┐");
        Console.WriteLine("│ " + header + " │");
        Console.WriteLine("├" + separator + "┤");

        // Rows
        foreach (var task in tasks)
        {
            string id = task.Id.ToString().PadRight(idWidth);
            string desc = task.Description.Length > descWidth ? task.Description.Substring(0, descWidth - 3) + "..." : task.Description;
            desc = desc.PadRight(descWidth);
            string status = (task.Completed ? "✓ Done" : "⧖ Pending").PadRight(statusWidth);
            string priority = task.Priority.ToString().PadRight(priorityWidth);
            string createdBy = (task.CreatedBy ?? "Unknown").PadRight(createdByWidth);
            string assignedTo = (task.AssignedTo ?? "Unassigned").PadRight(assignedToWidth);

            string row = id + " | " + desc + " | " + status + " | " + priority + " | " + createdBy + " | " + assignedTo;
            Console.WriteLine("│ " + row + " │");
        }

        // Footer
        Console.WriteLine("└" + separator + "┘");
    }

    public void ListTasks(string? filterBy = null, string? filterValue = null, string? sortBy = null)
    {
        T[] arr = _tasks.ToArray();
        int[] indices = new int[arr.Length];
        int filteredCount = 0;
        for (int i = 0; i < arr.Length; i++)
        {
            bool keep = true;
            if (!string.IsNullOrEmpty(filterBy) && !string.IsNullOrEmpty(filterValue))
            {
                switch (filterBy.ToLower())
                {
                    case "priority":
                        PriorityLevel pval;
                        keep = false;
                        if (Enum.TryParse(filterValue, true, out pval) && arr[i].Priority == pval)
                            keep = true;
                        break;
                    case "status":
                        if (filterValue.Equals("completed", System.StringComparison.OrdinalIgnoreCase))
                            keep = arr[i].Completed;
                        else if (filterValue.Equals("pending", System.StringComparison.OrdinalIgnoreCase))
                            keep = !arr[i].Completed;
                        break;
                }
            }
            if (keep)
            {
                indices[filteredCount++] = i;
            }
        }

        if (!string.IsNullOrEmpty(sortBy))
        {
            for (int i = 0; i < filteredCount - 1; i++)
            {
                for (int j = 0; j < filteredCount - i - 1; j++)
                {
                    bool swap = false;
                    int idxA = indices[j];
                    int idxB = indices[j + 1];
                    switch (sortBy.ToLower())
                    {
                        case "prioritydesc":
                            if (arr[idxA].Priority < arr[idxB].Priority) swap = true;
                            break;
                        case "statusdesc":
                            if (!arr[idxA].Completed && arr[idxB].Completed) swap = true;
                            break;
                        case "priority":
                            if (arr[idxA].Priority > arr[idxB].Priority) swap = true;
                            break;
                        case "creationdate":
                            if (arr[idxA].CreationDate > arr[idxB].CreationDate) swap = true;
                            break;
                        case "description":
                            if (string.Compare(arr[idxA].Description, arr[idxB].Description, System.StringComparison.Ordinal) > 0) swap = true;
                            break;
                    }
                    if (swap)
                    {
                        int temp = indices[j];
                        indices[j] = indices[j + 1];
                        indices[j + 1] = temp;
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < filteredCount - 1; i++)
            {
                for (int j = 0; j < filteredCount - i - 1; j++)
                {
                    int idxA = indices[j];
                    int idxB = indices[j + 1];
                    if (string.Compare(arr[idxA].Description, arr[idxB].Description, System.StringComparison.Ordinal) > 0)
                    {
                        int temp = indices[j];
                        indices[j] = indices[j + 1];
                        indices[j + 1] = temp;
                    }
                }
            }
        }

        T[] tasksToDisplay = new T[filteredCount];
        for (int i = 0; i < filteredCount; i++)
        {
            var task = arr[indices[i]];
            tasksToDisplay[i] = task;
        }

        PrintTaskTable(tasksToDisplay);
    }
    
    public void UpdateTask(int id, string? newDescription = null, PriorityLevel? newPriority = null)
    {
        T found = _tasks.FindById(id);
        if (found != null)
        {
            if (!CanModifyTask(id, _userContext.CurrentUsername ?? "Unknown"))
            {
                Console.WriteLine("Error: You do not have permission to modify this task.");
                return;
            }
            if (!string.IsNullOrEmpty(newDescription))
                found.Description = newDescription;
            if (newPriority.HasValue)
                found.Priority = newPriority.Value;
            _repository.SaveTasks(_tasks);
        }
    }

    // New methods for task assignment
    public void AssignTask(int id, string assigneeName)
    {
        T found = _tasks.FindById(id);
        if (found != null)
        {
            if (found.CreatedBy != _userContext.CurrentUsername && _userContext.CurrentUsername != "admin")
            {
                Console.WriteLine("Error: Only the task creator or admin can assign this task.");
                return;
            }
            found.AssignedTo = assigneeName;
            _repository.SaveTasks(_tasks);
            Console.WriteLine($"Task {id} assigned to {assigneeName}.");
        }
    }

    public void UnassignTask(int id)
    {
        T found = _tasks.FindById(id);
        if (found != null)
        {
            if (found.CreatedBy != _userContext.CurrentUsername && _userContext.CurrentUsername != "admin")
            {
                Console.WriteLine("Error: Only the task creator or admin can unassign this task.");
                return;
            }
            found.AssignedTo = null;
            _repository.SaveTasks(_tasks);
            Console.WriteLine($"Task {id} unassigned.");
        }
    }

    public T[] GetTasksAssignedToUser(string username)
    {
        T[] arr = _tasks.ToArray();
        var assigned = new System.Collections.Generic.List<T>();
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i].AssignedTo == username)
            {
                assigned.Add(arr[i]);
            }
        }
        return assigned.ToArray();
    }

    public T[] GetTasksCreatedByUser(string username)
    {
        T[] arr = _tasks.ToArray();
        var created = new System.Collections.Generic.List<T>();
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i].CreatedBy == username)
            {
                created.Add(arr[i]);
            }
        }
        return created.ToArray();
    }

    public bool CanModifyTask(int taskId, string username)
    {
        T found = _tasks.FindById(taskId);
        if (found == null) return false;

        // Creator can always modify their own tasks
        if (found.CreatedBy == username) return true;

        // Assigned user can modify task status
        if (found.AssignedTo == username) return true;

        // Admin can modify any task
        if (username == "admin") return true;

        return false;
    }
}