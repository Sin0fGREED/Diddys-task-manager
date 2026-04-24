using Xunit;

namespace Diddys_task_manager.Tests;

internal class InMemoryRepository : ITaskRepository<TaskItem>
{
    public ITaskCollection<TaskItem> LoadTasks() => new ArrayTaskCollection<TaskItem>();
    public void SaveTasks(ITaskCollection<TaskItem> tasks) { }
}

internal class FixedUserContext : IUserContext
{
    public string? CurrentUsername { get; private set; }
    public FixedUserContext(string username) => CurrentUsername = username;
    public void SetCurrentUser(string username) => CurrentUsername = username;
    public void Logout() => CurrentUsername = null;
}


internal static class DependencyTestHelpers
{
    public static TaskItem CreateTask(int id, StatusLevel status = StatusLevel.ToDo)
        => new TaskItem { Id = id, Description = $"Task {id}", CreatedBy = "tester", Status = status };

    public static TaskService<TaskItem> BuildService(params TaskItem[] tasks)
    {
        var repo = new InMemoryRepository();
        var ctx = new FixedUserContext("tester");
        var col = new ArrayTaskCollection<TaskItem>();
        foreach (var t in tasks) col.Add(t);
        return new TaskService<TaskItem>(repo, ctx, col);
    }
}


public class TaskServiceDependencyTests
{
    [Fact]
    public void AddDependency_ValidIds_StoresDependency()
    {
        var t1 = DependencyTestHelpers.CreateTask(1);
        var t2 = DependencyTestHelpers.CreateTask(2);
        var svc = DependencyTestHelpers.BuildService(t1, t2);

        svc.AddDependency(2, 1);

        Assert.Contains(1, t2.DependsOnTaskIds);
    }

    [Fact]
    public void AddDependency_SelfDependency_Rejected()
    {
        var t1 = DependencyTestHelpers.CreateTask(1);
        var svc = DependencyTestHelpers.BuildService(t1);

        svc.AddDependency(1, 1);

        Assert.Empty(t1.DependsOnTaskIds);
    }

    [Fact]
    public void AddDependency_Duplicate_NotAddedTwice()
    {
        var t1 = DependencyTestHelpers.CreateTask(1);
        var t2 = DependencyTestHelpers.CreateTask(2);
        var svc = DependencyTestHelpers.BuildService(t1, t2);

        svc.AddDependency(2, 1);
        svc.AddDependency(2, 1);

        Assert.Equal(1, t2.DependsOnTaskIds.Length);
    }

    [Fact]
    public void AddDependency_DirectCycle_Rejected()
    {
        var t1 = DependencyTestHelpers.CreateTask(1);
        var t2 = DependencyTestHelpers.CreateTask(2);
        var svc = DependencyTestHelpers.BuildService(t1, t2);

        svc.AddDependency(2, 1);
        svc.AddDependency(1, 2);

        Assert.Empty(t1.DependsOnTaskIds);
    }

    [Fact]
    public void AddDependency_TransitiveCycle_Rejected()
    {
        var t1 = DependencyTestHelpers.CreateTask(1);
        var t2 = DependencyTestHelpers.CreateTask(2);
        var t3 = DependencyTestHelpers.CreateTask(3);
        var svc = DependencyTestHelpers.BuildService(t1, t2, t3);

        svc.AddDependency(2, 1);
        svc.AddDependency(3, 2);
        svc.AddDependency(1, 3);

        Assert.Empty(t1.DependsOnTaskIds);
    }

    [Fact]
    public void SetTaskStatus_ToDone_BlockedByUnmetDependency()
    {
        var t1 = DependencyTestHelpers.CreateTask(1, StatusLevel.ToDo);
        var t2 = DependencyTestHelpers.CreateTask(2, StatusLevel.ToDo);
        var svc = DependencyTestHelpers.BuildService(t1, t2);
        svc.AddDependency(2, 1);

        svc.SetTaskStatus(2, StatusLevel.Done);

        Assert.NotEqual(StatusLevel.Done, t2.Status);
    }

    [Fact]
    public void SetTaskStatus_ToInProgress_AllowedEvenWithDependency()
    {
        var t1 = DependencyTestHelpers.CreateTask(1, StatusLevel.ToDo);
        var t2 = DependencyTestHelpers.CreateTask(2, StatusLevel.ToDo);
        var svc = DependencyTestHelpers.BuildService(t1, t2);
        svc.AddDependency(2, 1);

        svc.SetTaskStatus(2, StatusLevel.InProgress);

        Assert.Equal(StatusLevel.InProgress, t2.Status);
    }

    [Fact]
    public void SetTaskStatus_ToDone_AllowedWhenPrereqDone()
    {
        var t1 = DependencyTestHelpers.CreateTask(1, StatusLevel.Done);
        var t2 = DependencyTestHelpers.CreateTask(2, StatusLevel.ToDo);
        var svc = DependencyTestHelpers.BuildService(t1, t2);
        svc.AddDependency(2, 1);

        svc.SetTaskStatus(2, StatusLevel.Done);

        Assert.Equal(StatusLevel.Done, t2.Status);
    }

    [Fact]
    public void GetBlockingTasks_ReturnsOnlyIncompletePrereqs()
    {
        var t1 = DependencyTestHelpers.CreateTask(1, StatusLevel.Done);
        var t2 = DependencyTestHelpers.CreateTask(2, StatusLevel.ToDo);
        var t3 = DependencyTestHelpers.CreateTask(3, StatusLevel.ToDo);
        var svc = DependencyTestHelpers.BuildService(t1, t2, t3);
        svc.AddDependency(3, 1);
        svc.AddDependency(3, 2);

        var blocking = svc.GetBlockingTasks(3);

        Assert.Equal(1, blocking.Length);
        Assert.Equal(2, blocking[0].Id);
    }

    [Fact]
    public void RemoveDependency_ValidIds_RemovesLink()
    {
        var t1 = DependencyTestHelpers.CreateTask(1);
        var t2 = DependencyTestHelpers.CreateTask(2);
        var svc = DependencyTestHelpers.BuildService(t1, t2);
        svc.AddDependency(2, 1);

        svc.RemoveDependency(2, 1);

        Assert.Empty(t2.DependsOnTaskIds);
    }


    [Fact]
    public void RemoveDependency_NonExistentLink_NoSideEffect()
    {
        var t1 = DependencyTestHelpers.CreateTask(1);
        var t2 = DependencyTestHelpers.CreateTask(2);
        var svc = DependencyTestHelpers.BuildService(t1, t2);


        var ex = Record.Exception(() => svc.RemoveDependency(2, 1));
        Assert.Null(ex);
    }


    [Fact]
    public void RemoveTask_WithDependents_IsBlocked()
    {
        var t1 = DependencyTestHelpers.CreateTask(1);
        var t2 = DependencyTestHelpers.CreateTask(2);
        var svc = DependencyTestHelpers.BuildService(t1, t2);
        svc.AddDependency(2, 1);

        svc.RemoveTask(1);


        var all = svc.GetAllTasks();
        Assert.Contains(all, t => t.Id == 1);
    }


    [Fact]
    public void RemoveTask_NoDependents_Succeeds()
    {
        var t1 = DependencyTestHelpers.CreateTask(1);
        var t2 = DependencyTestHelpers.CreateTask(2);
        var svc = DependencyTestHelpers.BuildService(t1, t2);

        svc.RemoveTask(1);

        var all = svc.GetAllTasks();
        Assert.DoesNotContain(all, t => t.Id == 1);
    }


    [Fact]
    public void RemoveTask_NoDependents_RemainingIdsStable()
    {
        var t1 = DependencyTestHelpers.CreateTask(1);
        var t2 = DependencyTestHelpers.CreateTask(2);
        var t3 = DependencyTestHelpers.CreateTask(3);
        var svc = DependencyTestHelpers.BuildService(t1, t2, t3);

        svc.RemoveTask(2);

        var all = svc.GetAllTasks();

        Assert.Contains(all, t => t.Id == 1);
        Assert.Contains(all, t => t.Id == 3);
        Assert.DoesNotContain(all, t => t.Id == 2);
    }


    [Fact]
    public void AddTask_AfterDelete_GetsHigherUniqueId()
    {
        var t1 = DependencyTestHelpers.CreateTask(1);
        var t2 = DependencyTestHelpers.CreateTask(2);
        var svc = DependencyTestHelpers.BuildService(t1, t2);
        svc.RemoveTask(2);

        svc.AddTask("New task");

        var all = svc.GetAllTasks();

        Assert.Contains(all, t => t.Id == 1);
        Assert.Contains(all, t => t.Description == "New task");
    }
}
