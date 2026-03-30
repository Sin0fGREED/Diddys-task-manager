public interface ITaskService<T>
{
    T[] GetAllTasks();
    void AddTask(string description, PriorityLevel priority = PriorityLevel.Medium);
    void RemoveTask(int id);
    void ToggleTaskCompletion(int id);

    void ListTasks(string? filterBy = null, string? filterValue = null, string? sortBy = null);

    void UpdateTask(int id, string? newDescription = null, PriorityLevel? newPriority = null);
}