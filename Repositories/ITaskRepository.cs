public interface ITaskRepository
{
    TaskItem[] LoadTasks();
    void SaveTasks(TaskItem[] tasks);
}