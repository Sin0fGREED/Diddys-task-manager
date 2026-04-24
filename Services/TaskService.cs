public class TaskService<T> : ITaskService<T> where T : TaskItem
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
        var newTask = new TaskItem
        {
            Id = newId,
            Description = description,
            Status = StatusLevel.ToDo,
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
            // Check if any task depends on this one before deleting
            T[] all = _tasks.ToArray();
            var blockers = new int[all.Length];
            int blockerCount = 0;
            for (int i = 0; i < all.Length; i++)
            {
                int[] deps = all[i].DependsOnTaskIds;
                for (int j = 0; j < deps.Length; j++)
                {
                    if (deps[j] == id)
                    {
                        blockers[blockerCount++] = all[i].Id;
                        break;
                    }
                }
            }
            if (blockerCount > 0)
            {
                Console.Write("Error: Cannot delete task because the following tasks depend on it: ");
                for (int i = 0; i < blockerCount; i++)
                    Console.Write(i == 0 ? $"{blockers[i]}" : $", {blockers[i]}");
                Console.WriteLine(". Remove those dependencies first.");
                return;
            }

            _tasks.Remove(found);
            // IDs are intentionally NOT renumbered so dependency references remain valid.
            _repository.SaveTasks(_tasks);
        }
    }

    public void CycleTaskStatus(int id)
    {
        T found = _tasks.FindById(id);
        if (found != null)
        {
            if (!CanModifyTask(id, _userContext.CurrentUsername ?? "Unknown"))
            {
                Console.WriteLine("Error: You do not have permission to modify this task.");
                return;
            }
            StatusLevel next = found.Status switch
            {
                StatusLevel.ToDo => StatusLevel.InProgress,
                StatusLevel.InProgress => StatusLevel.Done,
                StatusLevel.Done => StatusLevel.ToDo,
                _ => StatusLevel.ToDo
            };
            if (next == StatusLevel.Done)
            {
                T[] blocking = GetBlockingTasks(id);
                if (blocking.Length > 0)
                {
                    Console.WriteLine("Error: Cannot mark task as Done. The following prerequisite tasks are not yet Done:");
                    for (int i = 0; i < blocking.Length; i++)
                        Console.WriteLine($"  Task {blocking[i].Id}: {blocking[i].Description} [{blocking[i].Status}]");
                    return;
                }
            }
            found.Status = next;
            _repository.SaveTasks(_tasks);
        }
    }

    public void SetTaskStatus(int id, StatusLevel status)
    {
        T found = _tasks.FindById(id);
        if (found != null)
        {
            if (!CanModifyTask(id, _userContext.CurrentUsername ?? "Unknown"))
            {
                Console.WriteLine("Error: You do not have permission to modify this task.");
                return;
            }
            // Enforce dependency rule: cannot move to Done while prerequisites are incomplete
            if (status == StatusLevel.Done)
            {
                T[] blocking = GetBlockingTasks(id);
                if (blocking.Length > 0)
                {
                    Console.WriteLine("Error: Cannot mark task as Done. The following prerequisite tasks are not yet Done:");
                    for (int i = 0; i < blocking.Length; i++)
                        Console.WriteLine($"  Task {blocking[i].Id}: {blocking[i].Description} [{blocking[i].Status}]");
                    return;
                }
            }
            found.Status = status;
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
            string status = (task.Status switch { StatusLevel.ToDo => "⧖ To Do", StatusLevel.InProgress => "▶ In Progress", StatusLevel.Done => "✓ Done", _ => "⧖ To Do" }).PadRight(statusWidth);
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
                        if (Enum.TryParse(filterValue, true, out StatusLevel sval))
                            keep = arr[i].Status == sval;
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
                            if (arr[idxA].Status < arr[idxB].Status) swap = true;
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

    // ── Dependency methods ─────────────────────────────────────────────────────

    public void AddDependency(int taskId, int dependsOnTaskId)
    {
        if (taskId == dependsOnTaskId)
        {
            Console.WriteLine("Error: A task cannot depend on itself.");
            return;
        }

        T task = _tasks.FindById(taskId);
        if (task == null) { Console.WriteLine($"Error: Task {taskId} not found."); return; }

        // Verify the prerequisite task exists
        T prereq = _tasks.FindById(dependsOnTaskId);
        if (prereq == null) { Console.WriteLine($"Error: Task {dependsOnTaskId} not found."); return; }

        // Check for duplicate
        int[] existing = task.DependsOnTaskIds;
        for (int i = 0; i < existing.Length; i++)
        {
            if (existing[i] == dependsOnTaskId)
            {
                Console.WriteLine($"Error: Task {taskId} already depends on task {dependsOnTaskId}.");
                return;
            }
        }

        // Cycle check: adding taskId → dependsOnTaskId would create a cycle if
        // dependsOnTaskId can already reach taskId through its own dependencies.
        if (WouldCreateCycle(taskId, dependsOnTaskId))
        {
            Console.WriteLine($"Error: Adding this dependency would create a circular dependency chain.");
            return;
        }

        // Append to the array (no List<T> allowed)
        int[] newDeps = new int[existing.Length + 1];
        for (int i = 0; i < existing.Length; i++)
            newDeps[i] = existing[i];
        newDeps[existing.Length] = dependsOnTaskId;
        task.DependsOnTaskIds = newDeps;

        _repository.SaveTasks(_tasks);
        Console.WriteLine($"Task {taskId} now depends on task {dependsOnTaskId} ({prereq.Description}).");
    }

    public void RemoveDependency(int taskId, int dependsOnTaskId)
    {
        T task = _tasks.FindById(taskId);
        if (task == null) { Console.WriteLine($"Error: Task {taskId} not found."); return; }

        int[] existing = task.DependsOnTaskIds;
        int index = -1;
        for (int i = 0; i < existing.Length; i++)
        {
            if (existing[i] == dependsOnTaskId) { index = i; break; }
        }

        if (index == -1)
        {
            Console.WriteLine($"Error: Task {taskId} does not depend on task {dependsOnTaskId}.");
            return;
        }

        // Remove from array
        int[] newDeps = new int[existing.Length - 1];
        for (int i = 0, j = 0; i < existing.Length; i++)
        {
            if (i != index) newDeps[j++] = existing[i];
        }
        task.DependsOnTaskIds = newDeps;
        _repository.SaveTasks(_tasks);
        Console.WriteLine($"Dependency removed: task {taskId} no longer depends on task {dependsOnTaskId}.");
    }

    public T[] GetBlockingTasks(int taskId)
    {
        T task = _tasks.FindById(taskId);
        if (task == null) return new T[0];

        int[] deps = task.DependsOnTaskIds;
        // Count how many are not Done
        int count = 0;
        for (int i = 0; i < deps.Length; i++)
        {
            try
            {
                T prereq = _tasks.FindById(deps[i]);
                if (prereq.Status != StatusLevel.Done) count++;
            }
            catch { /* prerequisite task was deleted — treat as non-blocking */ }
        }

        T[] result = new T[count];
        int idx = 0;
        for (int i = 0; i < deps.Length; i++)
        {
            try
            {
                T prereq = _tasks.FindById(deps[i]);
                if (prereq.Status != StatusLevel.Done) result[idx++] = prereq;
            }
            catch { }
        }
        return result;
    }

    // DFS cycle detection — uses a hand-rolled int stack (no List<T>).
    // Returns true if adding edge (newTaskId → dependsOnId) would create a cycle.
    // Strategy: starting from dependsOnId, follow all DependsOnTaskIds recursively.
    // If we ever reach newTaskId, a cycle would exist.
    private bool WouldCreateCycle(int newTaskId, int dependsOnId)
    {
        // Manual stack using an int array
        int maxNodes = _tasks.Count + 1;
        int[] stack = new int[maxNodes];
        int[] visited = new int[maxNodes];
        int stackTop = 0;
        int visitedCount = 0;

        stack[stackTop++] = dependsOnId;

        while (stackTop > 0)
        {
            int current = stack[--stackTop];

            // Check already visited (avoid infinite loops in existing broken data)
            bool alreadyVisited = false;
            for (int i = 0; i < visitedCount; i++)
            {
                if (visited[i] == current) { alreadyVisited = true; break; }
            }
            if (alreadyVisited) continue;
            visited[visitedCount++] = current;

            if (current == newTaskId) return true; // cycle found

            try
            {
                T node = _tasks.FindById(current);
                int[] deps = node.DependsOnTaskIds;
                for (int i = 0; i < deps.Length; i++)
                {
                    if (stackTop < stack.Length - 1)
                        stack[stackTop++] = deps[i];
                }
            }
            catch { /* task not found, skip */ }
        }
        return false;
    }
}