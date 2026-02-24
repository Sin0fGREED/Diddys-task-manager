class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private TaskItem[] _tasks;

    public TaskService(ITaskRepository repository)
    {
        _repository = repository;
        _tasks = _repository.LoadTasks();
    }

    public TaskItem[] GetAllTasks() => _tasks;

    public void AddTask(string description)
    {
        int newId = _tasks.Length > 0 ? _tasks[_tasks.Length - 1].Id + 1 : 1;
        var newTask = new TaskItem { Id = newId, Description = description, Completed = false };

        TaskItem[] newTasks = new TaskItem[_tasks.Length + 1];
        for (int i = 0; i < _tasks.Length; i++)
            newTasks[i] = _tasks[i];
        newTasks[_tasks.Length] = newTask;

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

        TaskItem[] newTasks = new TaskItem[_tasks.Length - 1];
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
}