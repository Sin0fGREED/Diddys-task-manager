using System;
using System.Linq;
using Model;

namespace Diddys_task_manager.Tests;

public class HashMapTaskCollectionTests
{
    [Fact]
    public void Add_DuplicateId_UpdatesValueWithoutChangingCount()
    {
        var collection = new HashMapTaskCollection();
        var original = CreateTask(1, "Original");
        var updated = CreateTask(1, "Updated");

        collection.Add(original);
        collection.Add(updated);

        Assert.Equal(1, collection.Count);
        Assert.Equal("Updated", collection.FindById(1).Description);
    }

    [Fact]
    public void Remove_ExistingItem_ReturnsTrue_AndItemCannotBeFound()
    {
        var collection = new HashMapTaskCollection();
        var task = CreateTask(5);
        collection.Add(task);

        var removed = collection.Remove(task);

        Assert.True(removed);
        Assert.Equal(0, collection.Count);
        Assert.Throws<InvalidOperationException>(() => collection.FindById(5));
    }

    [Fact]
    public void ToArray_ContainsAllItems_EvenWhenKeysCollide()
    {
        var collection = new HashMapTaskCollection();
        collection.Add(CreateTask(1));
        collection.Add(CreateTask(17));

        var ids = collection.ToArray().Select(t => t.Id).OrderBy(id => id).ToArray();

        Assert.Equal(new[] { 1, 17 }, ids);
    }

    [Fact]
    public void Sort_ThrowsNotSupportedException()
    {
        var collection = new HashMapTaskCollection();

        Assert.Throws<NotSupportedException>(() =>
            collection.Sort((a, b) => a.Id.CompareTo(b.Id)));
    }

    [Fact]
    public void Reduce_SumsIds()
    {
        var collection = new HashMapTaskCollection();
        collection.Add(CreateTask(2));
        collection.Add(CreateTask(3));

        var total = collection.Reduce(0, (sum, task) => sum + task.Id);

        Assert.Equal(5, total);
    }

    private static TaskItem CreateTask(int id, string? description = null)
    {
        return new TaskItem
        {
            Id = id,
            Description = description ?? $"Task {id}",
            CreatedBy = "tester"
        };
    }
}
