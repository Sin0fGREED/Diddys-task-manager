class TaskService<T> : ITaskService<T> where T : TaskItem
{
    private readonly ITaskRepository<T> _repository;
    private ITaskCollection<T> _tasks;

    public TaskService(ITaskRepository<T> repository, ITaskCollection<T>? collection = null)
    {
        _repository = repository;
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
        var newTask = new TaskItem { Id = newId, Description = description, Completed = false, Priority = priority, CreationDate = DateTime.Now };
        _tasks.Add((T)(object)newTask);
        _repository.SaveTasks(_tasks);
    }

    public void RemoveTask(int id)
    {
        T found = _tasks.FindById(id);
        if (found != null)
        {
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
            found.Completed = !found.Completed;
            _repository.SaveTasks(_tasks);
        }
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

        for (int i = 0; i < filteredCount; i++)
        {
            var task = arr[indices[i]];
            Console.WriteLine($"{task.Description} - {(task.Completed ? "Completed" : "Pending")} | Priority: {task.Priority} | Created: {task.CreationDate}");
        }
    }
    public void UpdateTask(int id, string? newDescription = null, PriorityLevel? newPriority = null)
    {
        T found = _tasks.FindById(id);
        if (found != null)
        {
            if (!string.IsNullOrEmpty(newDescription))
                found.Description = newDescription;
            if (newPriority.HasValue)
                found.Priority = newPriority.Value;
            _repository.SaveTasks(_tasks);
        }
    }
}