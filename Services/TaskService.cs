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
        int newId = _tasks.Length > 0 ? _tasks[_tasks.Length - 1].Id + 1 : 1;
        var newTask = new TaskItem { Id = newId, Description = description, Completed = false };

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

    public void ListTasks()
    {
        foreach (var task in _tasks)
        {
            Console.WriteLine($"{task.Id}: {task.Description} - {(task.Completed ? "Completed" : "Pending")}");
        }
    }
}