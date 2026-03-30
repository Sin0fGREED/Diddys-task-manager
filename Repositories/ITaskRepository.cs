public interface ITaskRepository<T>
{
    ITaskCollection<T> LoadTasks();
    void SaveTasks(ITaskCollection<T> tasks);
}