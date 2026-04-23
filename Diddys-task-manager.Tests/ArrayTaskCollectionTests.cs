using System;
using System.Linq;

namespace Diddys_task_manager.Tests;

public class ArrayTaskCollectionTests
{
    [Fact]
    public void Add_IncreasesCount_AndSetsDirty()
    {
        var collection = new ArrayTaskCollection<TaskItem>();

        collection.Add(CreateTask(1));

        Assert.Equal(1, collection.Count);
        Assert.True(collection.Dirty);
    }

    [Fact]
    public void Remove_ExistingItem_ReturnsTrue_AndDecreasesCount()
    {
        var collection = new ArrayTaskCollection<TaskItem>();
        var task = CreateTask(1);
        collection.Add(task);
        collection.Dirty = false;

        var removed = collection.Remove(task);

        Assert.True(removed);
        Assert.Equal(0, collection.Count);
        Assert.True(collection.Dirty);
    }

    [Fact]
    public void FindById_ReturnsMatchingTask()
    {
        var collection = new ArrayTaskCollection<TaskItem>();
        var expected = CreateTask(42);
        collection.Add(expected);

        var found = collection.FindById(42);

        Assert.Same(expected, found);
    }

    [Fact]
    public void Sort_OrdersByIdAscending()
    {
        var collection = new ArrayTaskCollection<TaskItem>();
        collection.Add(CreateTask(3));
        collection.Add(CreateTask(1));
        collection.Add(CreateTask(2));
        collection.Dirty = false;

        collection.Sort((a, b) => a.Id.CompareTo(b.Id));

        var ids = collection.ToArray().Select(t => t.Id).ToArray();
        Assert.Equal(new[] { 1, 2, 3 }, ids);
        Assert.True(collection.Dirty);
    }

    [Fact]
    public void Reduce_SumsIds()
    {
        var collection = new ArrayTaskCollection<TaskItem>();
        collection.Add(CreateTask(1));
        collection.Add(CreateTask(2));
        collection.Add(CreateTask(3));

        var total = collection.Reduce(0, (sum, task) => sum + task.Id);

        Assert.Equal(6, total);
    }

    private static TaskItem CreateTask(int id)
    {
        return new TaskItem
        {
            Id = id,
            Description = $"Task {id}",
            CreatedBy = "tester"
        };
    }
}
