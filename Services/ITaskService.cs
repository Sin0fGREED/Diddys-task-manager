public interface ITaskService<T>
{
    T[] GetAllTasks();
    void AddTask(string description);
    void RemoveTask(int id);
    void ToggleTaskCompletion(int id);

    void ListTasks();
}