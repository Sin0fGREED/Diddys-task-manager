class TaskService<T> : ITaskService<T> where T : TaskItem
{
    private readonly ITaskRepository<T> _repository;
    private T[] _tasks;

    public TaskService(ITaskRepository<T> repository)
    {
        _repository = repository;
        _tasks = _repository.LoadTasks();
    }

    public T[] GetAllTasks() => _tasks;

    public void AddTask(string description)
    {
        AddTask(description, PriorityLevel.Medium);
    }

    public void AddTask(string description, PriorityLevel priority)
    {
        int newId = _tasks.Length > 0 ? _tasks[_tasks.Length - 1].Id + 1 : 1;
        var newTask = new TaskItem { Id = newId, Description = description, Completed = false, Priority = priority, CreationDate = DateTime.Now };

        T[] newTasks = new T[_tasks.Length + 1];
        for (int i = 0; i < _tasks.Length; i++)
            newTasks[i] = _tasks[i];
        newTasks[_tasks.Length] = (T)(object)newTask;

        _tasks = newTasks;
        _repository.SaveTasks(_tasks);
    }

    public void RemoveTask(int id)
    {
        int index = -1;
        for (int i = 0; i < _tasks.Length; i++)
        {
            if (_tasks[i].Id == id)
            {
                index = i;
                break;
            }
        }

        if (index == -1) return;

        T[] newTasks = new T[_tasks.Length - 1];
        for (int i = 0, j = 0; i < _tasks.Length; i++)
        {
            if (i != index)
                newTasks[j++] = _tasks[i];
        }

        _tasks = newTasks;
        _repository.SaveTasks(_tasks);
    }

    public void ToggleTaskCompletion(int id)
    {
        for (int i = 0; i < _tasks.Length; i++)
        {
            if (_tasks[i].Id == id)
            {
                _tasks[i].Completed = !_tasks[i].Completed;
                _repository.SaveTasks(_tasks);
                return;
            }
        }
    }

    public void ListTasks(string? filterBy = null, string? filterValue = null, string? sortBy = null)
    {
        int[] indices = new int[_tasks.Length];
        int filteredCount = 0;
        for (int i = 0; i < _tasks.Length; i++)
        {
            bool keep = true;
            if (!string.IsNullOrEmpty(filterBy) && !string.IsNullOrEmpty(filterValue))
            {
                switch (filterBy.ToLower())
                {
                    case "priority":
                        PriorityLevel pval;
                        keep = false;
                        if (Enum.TryParse(filterValue, true, out pval) && _tasks[i].Priority == pval)
                            keep = true;
                        break;
                    case "status":
                        if (filterValue.Equals("completed", StringComparison.OrdinalIgnoreCase))
                            keep = _tasks[i].Completed;
                        else if (filterValue.Equals("pending", StringComparison.OrdinalIgnoreCase))
                            keep = !_tasks[i].Completed;
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
                            if (_tasks[idxA].Priority < _tasks[idxB].Priority) swap = true;
                            break;
                        case "statusdesc":
                            if (!_tasks[idxA].Completed && _tasks[idxB].Completed) swap = true;
                            break;
                        case "priority":
                            if (_tasks[idxA].Priority > _tasks[idxB].Priority) swap = true;
                            break;
                        case "creationdate":
                            if (_tasks[idxA].CreationDate > _tasks[idxB].CreationDate) swap = true;
                            break;
                        case "description":
                            if (string.Compare(_tasks[idxA].Description, _tasks[idxB].Description, StringComparison.Ordinal) > 0) swap = true;
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
                    if (string.Compare(_tasks[idxA].Description, _tasks[idxB].Description, StringComparison.Ordinal) > 0)
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
            var task = _tasks[indices[i]];
            Console.WriteLine($"{task.Description} - {(task.Completed ? "Completed" : "Pending")} | Priority: {task.Priority} | Created: {task.CreationDate}");
        }
    }
}