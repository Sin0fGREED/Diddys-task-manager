# Diddy's Task Manager - Design Decisions & Architecture Rationale

**Created by:** Sin0f_Greed

---

## Overview

This document explains the architectural choices and design patterns used in the collection system. Each decision was made to balance **flexibility, performance, and educational value**.

---

## Why Multiple Collection Implementations Instead of One?

### Design Choice: Strategy Pattern with 4 Different Collections

**Why this approach?**

- **Single Collection (Bad):** If we just used a simple `List<T>`, we'd lock ourselves into one data structure. Different use cases have different performance profiles.
- **Our Approach (Good):** Implementing `ITaskCollection<T>` interface allows swapping between implementations without changing service code.

**Real-world trade-offs:**
- **Small teams managing few tasks?** → Use `ArrayTaskCollection` (simple, predictable)
- **Frequent task insertions/deletions?** → Use `LinkedListTaskCollection` (O(1) splice if you have reference)
- **Rapid lookups by Task ID?** → Use `HashMapTaskCollection` (O(1) average)
- **Need sorted traversal?** → Use `BinarySearchTreeTaskCollection` (in-order gives you sorted order)

This is **educational AND practical** - you learn the trade-offs of data structures while building enterprise-grade software.

---

## Custom Iterator: Why NOT Use `IEnumerable<T>`?

### Decision: Implement Custom `IMyIterator<T>` Interface

```csharp
// Ours:
public interface IMyIterator<T>
{
    bool HasNext();
    T Next();
    void Reset();
}

// Could have used: IEnumerable<T>
```

**Why we didn't use `IEnumerable<T>`:**

1. **Educational Purpose** - Building custom iterators teaches how iteration fundamentally works
2. **Reset Capability** - Standard `IEnumerable<T>` doesn't have a `Reset()` method. You have to create a new enumerator. Our custom iterator lets you restart from the beginning without allocating new objects
3. **Fine-grained Control** - We control exactly how traversal happens per data structure
4. **Predictability** - Custom interface means predictable behavior across different collection types

**Trade-off:** More code to write, but better learning experience and Reset() functionality

---

## LinkedList Implementation: Why Hand-Rolled Instead of `LinkedList<T>`?

### Decision: Custom Singly-Linked List with `Node` Class

```csharp
private class Node
{
    public T Data;
    public Node? Next;
    public Node(T data) { Data = data; }
}
```

**Why we didn't use `System.Collections.Generic.LinkedList<T>`:**

1. **Understanding the Fundamentals** - Building it yourself teaches what a linked list actually IS
2. **Custom Iteration Behavior** - Our iterator controls exactly how we traverse
3. **Memory Transparency** - You see exactly how many pointers you're using
4. **Simplicity** - .NET's LinkedList has more features we don't need (bidirectional nodes, etc.)

**Trade-off:** Our version is simpler for THIS specific use case, though .NET's would be more optimized for production

---

## FindById() Using Reflection: Why Not Type-Specific?

### Decision: Generic Reflection Over Task-Specific Code

```csharp
// Current approach:
public T FindById(int id)
{
    for (int i = 0; i < items.Length; i++)
    {
        var prop = items[i]?.GetType().GetProperty("Id");
        if (prop != null && (int?)(prop.GetValue(items[i])) == id)
            return items[i];
    }
    return default(T) ?? throw new InvalidOperationException(...);
}

// Alternative (type-specific):
// public TaskItem FindById(int id) { return items.FirstOrDefault(t => t.Id == id); }
```

**Why Reflection?**

1. **Generic Flexibility** - `ITaskCollection<T>` works with ANY type that has an `Id` property, not just `TaskItem`
2. **Education** - Teaches how reflection works and its performance implications
3. **Extensibility** - If you add a new task-like type, it still works

**Downside:** Reflection is slower than direct property access, but for our collection sizes, negligible

---

## HashMap: Why Not Just Use Array or LinkedList?

### Decision: Hash Table for O(1) Average Lookup

**ArrayTaskCollection Problem:**
```csharp
// Searching for task ID 500 in array of 10,000 items:
for (int i = 0; i < items.Length; i++)  // Could be 10,000 iterations!
{
    if (items[i].Id == 500) return items[i];
}
```

**HashMapTaskCollection Solution:**
```csharp
// Searching for task ID 500:
int index = Math.Abs(500.GetHashCode()) % buckets.Length;  // Direct calculation
// Then search only that bucket's chain (usually 1-2 items)
```

**Why HashMap?**

1. **Speed** - O(1) average vs O(n) for array/linkedlist
2. **Real-world Scenario** - Task managers often look up tasks by ID frequently
3. **Trade-off Visibility** - You see how hash collisions are handled (chaining)

**Why NOT Sort in HashMap?**
- Hash tables have NO inherent order (that's the point!)
- Forcing order defeats the purpose of hashing
- If you need sorting, convert to `BinarySearchTreeTaskCollection`

---

## Binary Search Tree: Why NOT Just Use HashMap?

### Decision: BST for Ordered Access + Fast Lookup

**HashMap Problem:**
```csharp
// Get all tasks in ID order:
var tasks = collection.ToArray();  // Requires full traversal
Array.Sort(tasks);  // Extra O(n log n) operation!
```

**BST Solution:**
```csharp
// Get all tasks in ID order:
public TaskItem[] ToArray()
{
    TaskItem[] arr = new TaskItem[count];
    int idx = 0;
    InOrder(root, arr, ref idx);  // In-order traversal = sorted!
    return arr;
}
```

**Why BST?**

1. **Maintains Order** - Tasks naturally come back sorted by ID
2. **Balanced Lookup** - O(log n) is fast enough for most cases
3. **Educational** - Most complex data structure; teaches recursion, tree balancing
4. **Practical** - Some use cases need both speed AND order

**Why Don't Both HashMap and BST Support Sort()?**
- **HashMap:** No natural order. Sorting would require O(n log n) work defeating the hash advantage
- **BST:** Already implicitly sorted by traversal order. Explicit sort redundant

---

## Dirty Flag Pattern: Why Track Modifications?

```csharp
public bool Dirty { get; set; } = false;

public void Add(T item)
{
    // ... add logic ...
    Dirty = true;  // Mark as modified
}
```

**Why this pattern?**

1. **Persistence Optimization** - Only save to disk if `Dirty == true`
2. **Change Tracking** - Know if data has changed since last save
3. **UI Optimization** - Mark views as needing refresh only when data changes
4. **Performance** - Avoid unnecessary I/O operations

**Real-world example:**
```csharp
if (taskCollection.Dirty)
{
    await jsonRepository.SaveAsync(taskCollection.ToArray());  // Expensive I/O
    taskCollection.Dirty = false;
}
```

---

## Functional Operations: Filter, Reduce, Predicates

### Decision: Higher-Order Functions Over Imperative Loops

```csharp
// Our way (Functional):
public ITaskCollection<T> Filter(Func<T, bool> predicate)
{
    var filtered = new ArrayTaskCollection<T>();
    foreach (var item in items)
    {
        if (predicate(item))
            filtered.Add(item);
    }
    return filtered;
}

// Alternative (Procedural):
// var filtered = new ArrayTaskCollection<T>();
// for (int i = 0; i < items.Length; i++) {
//     if (items[i].Priority == PriorityLevel.High) filtered.Add(items[i]);
// }
```

**Why Functional Approach?**

1. **Reusability** - Pass ANY predicate: `Filter(t => t.Priority == PriorityLevel.High)`
2. **Composability** - Chain operations: `Filter(...).Filter(...).ToArray()`
3. **Separation of Concerns** - Collection logic separate from business logic
4. **Modern C#** - Delegates and Func<T> are core C# features

**Reduce Example:**
```csharp
// Count high-priority tasks:
int highPriorityCount = collection.Reduce(0, (acc, task) => 
    task.Priority == PriorityLevel.High ? acc + 1 : acc);

// Sum of task IDs:
int idSum = collection.Reduce(0, (acc, task) => acc + task.Id);
```

**Why not LINQ?**
- Educational - you see how functional programming works under the hood
- Custom implementations - you control exact behavior
- Some systems have minimal .NET dependencies

---

## Task Dependencies: Array Instead of HashSet

```csharp
public int[] DependsOnTaskIds { get; set; } = new int[0];
```

**Why array instead of `HashSet<int>` or `LinkedList<int>`?**

1. **Simplicity** - Most tasks have 0-3 dependencies (small arrays)
2. **JSON Serialization** - Arrays serialize cleanly to JSON
3. **Memory Small** - For small collections, overhead of HashSet not worth it
4. **Traversal Order** - Dependencies might be ordered by importance

**Trade-off:** For tasks with 100+ dependencies, HashSet would be better, but that's rare

---

## Why Generic `ITaskCollection<T>` Over `ITaskCollection`?

### Decision: Generic Interface Over Non-Generic

```csharp
// Ours (Generic):
public interface ITaskCollection<T>
{
    void Add(T item);
    T FindById(int id);
    // ...
}

// Could have been:
// public interface ITaskCollection {
//     void Add(object item);
//     object FindById(int id);
// }
```

**Why Generics?**

1. **Type Safety** - Compiler catches type mismatches at compile-time, not runtime
2. **Performance** - No boxing/unboxing of value types
3. **Intellisense** - IDE knows what type you're working with
4. **Modern C#** - Generics are the standard for collection interfaces

---

## Summary: Design Philosophy

**Sin0f_Greed's Approach:**

✅ **Educational** - Each implementation teaches a data structure principle  
✅ **Practical** - Real-world performance considerations  
✅ **Flexible** - Strategy pattern allows swapping implementations  
✅ **Clean** - Consistent interface across all types  
✅ **Extensible** - Easy to add new collection types  
✅ **Transparent** - You see the trade-offs, not hidden behind abstractions  

This codebase demonstrates that **good architecture is about making intentional trade-offs**, not following dogma. Each choice balances learning, performance, and maintainability.

---

## Performance Profile Reference

| Scenario | Best Choice | Why |
|----------|------------|-----|
| Small team, few tasks | Array | Simple, predictable |
| Heavy task insertion/deletion | LinkedList | O(1) splice ops |
| Frequent "get task by ID" queries | HashMap | O(1) average lookup |
| Need sorted tasks by ID | BST | Maintains order, O(log n) lookup |
| Mixed operations | BST | Best all-rounder |

