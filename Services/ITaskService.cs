public interface ITaskService 
{
    TaskItem[] GetAllTasks();
    void AddTask(string description);
    void RemoveTask(int id);
    void ToggleTaskCompletion(int id);
}