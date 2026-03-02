public interface ITaskRepository<T>
{
    T[] LoadTasks();
    void SaveTasks(T[] tasks);
}