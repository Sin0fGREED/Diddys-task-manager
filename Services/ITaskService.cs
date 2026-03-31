public interface ITaskService<T>
{
    T[] GetAllTasks();
    void AddTask(string description, PriorityLevel priority = PriorityLevel.Medium);
    void RemoveTask(int id);
    void ToggleTaskCompletion(int id);

    void ListTasks(string? filterBy = null, string? filterValue = null, string? sortBy = null);

    void UpdateTask(int id, string? newDescription = null, PriorityLevel? newPriority = null);

    // New methods for task assignment
    void AssignTask(int id, string assigneeName);
    void UnassignTask(int id);
    T[] GetTasksAssignedToUser(string username);
    T[] GetTasksCreatedByUser(string username);
    bool CanModifyTask(int taskId, string username);
}