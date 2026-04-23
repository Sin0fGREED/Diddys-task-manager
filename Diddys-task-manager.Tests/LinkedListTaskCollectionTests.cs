using System;
using System.Linq;

namespace Diddys_task_manager.Tests;

public class LinkedListTaskCollectionTests
{
    [Fact]
    public void Add_AndFindById_WorkCorrectly()
    {
        var collection = new LinkedListTaskCollection<TaskItem>();
        var task = CreateTask(7);

        collection.Add(task);

        Assert.Equal(1, collection.Count);
        Assert.Same(task, collection.FindById(7));
    }

    [Fact]
    public void Remove_NonExistingItem_ReturnsFalse()
    {
        var collection = new LinkedListTaskCollection<TaskItem>();
        collection.Add(CreateTask(1));

        var removed = collection.Remove(CreateTask(999));

        Assert.False(removed);
        Assert.Equal(1, collection.Count);
    }

    [Fact]
    public void Filter_ReturnsOnlyMatchingItems()
    {
        var collection = new LinkedListTaskCollection<TaskItem>();
        collection.Add(CreateTask(1));
        collection.Add(CreateTask(2));
        collection.Add(CreateTask(3));

        var filtered = collection.Filter(t => t.Id % 2 == 1);
        var ids = filtered.ToArray().Select(t => t.Id).ToArray();

        Assert.Equal(2, filtered.Count);
        Assert.Equal(new[] { 1, 3 }, ids);
    }

    [Fact]
    public void Iterator_TraversesAllItems_AndCanReset()
    {
        var collection = new LinkedListTaskCollection<TaskItem>();
        collection.Add(CreateTask(1));
        collection.Add(CreateTask(2));

        var iterator = collection.GetIterator();

        var firstPass = new[] { iterator.Next().Id, iterator.Next().Id };
        Assert.False(iterator.HasNext());

        iterator.Reset();
        var secondPass = new[] { iterator.Next().Id, iterator.Next().Id };

        Assert.Equal(new[] { 1, 2 }, firstPass);
        Assert.Equal(new[] { 1, 2 }, secondPass);
    }

    [Fact]
    public void Clear_ResetsCollection()
    {
        var collection = new LinkedListTaskCollection<TaskItem>();
        collection.Add(CreateTask(1));
        collection.Dirty = false;

        collection.Clear();

        Assert.Equal(0, collection.Count);
        Assert.Empty(collection.ToArray());
        Assert.True(collection.Dirty);
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
