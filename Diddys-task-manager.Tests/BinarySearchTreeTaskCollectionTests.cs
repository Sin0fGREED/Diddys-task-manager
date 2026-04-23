using System;
using System.Linq;
using Model;

namespace Diddys_task_manager.Tests;

public class BinarySearchTreeTaskCollectionTests
{
    [Fact]
    public void ToArray_ReturnsItemsInSortedIdOrder()
    {
        var collection = new BinarySearchTreeTaskCollection();
        collection.Add(CreateTask(5));
        collection.Add(CreateTask(1));
        collection.Add(CreateTask(3));

        var ids = collection.ToArray().Select(t => t.Id).ToArray();

        Assert.Equal(new[] { 1, 3, 5 }, ids);
    }

    [Fact]
    public void FindById_ReturnsInsertedItem()
    {
        var collection = new BinarySearchTreeTaskCollection();
        var task = CreateTask(10);
        collection.Add(task);

        var found = collection.FindById(10);

        Assert.Same(task, found);
    }

    [Fact]
    public void Remove_NodeWithTwoChildren_KeepsTreeValid()
    {
        var collection = new BinarySearchTreeTaskCollection();
        collection.Add(CreateTask(4));
        collection.Add(CreateTask(2));
        collection.Add(CreateTask(6));
        collection.Add(CreateTask(5));
        collection.Add(CreateTask(7));

        var removed = collection.Remove(CreateTask(6));

        Assert.True(removed);
        var ids = collection.ToArray().Select(t => t.Id).ToArray();
        Assert.Equal(new[] { 2, 4, 5, 7 }, ids);
    }

    [Fact]
    public void Sort_ThrowsNotSupportedException()
    {
        var collection = new BinarySearchTreeTaskCollection();

        Assert.Throws<NotSupportedException>(() =>
            collection.Sort((a, b) => a.Id.CompareTo(b.Id)));
    }

    [Fact]
    public void Iterator_CanTraverseAndReset()
    {
        var collection = new BinarySearchTreeTaskCollection();
        collection.Add(CreateTask(2));
        collection.Add(CreateTask(1));

        var iterator = collection.GetIterator();
        var first = iterator.Next().Id;
        var second = iterator.Next().Id;
        Assert.False(iterator.HasNext());

        iterator.Reset();
        var afterReset = iterator.Next().Id;

        Assert.Equal(1, first);
        Assert.Equal(2, second);
        Assert.Equal(1, afterReset);
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
