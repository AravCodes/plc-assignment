using System.Collections.Concurrent;
using TaskManagerAPI.Models;

namespace TaskManagerAPI.Services;

public class TaskRepository
{
    // projectId -> (taskId -> TaskItem)
    private readonly ConcurrentDictionary<int, ConcurrentDictionary<int, TaskItem>> _store = new();
    private int _nextId = 0;

    public IEnumerable<TaskItem> GetAll(int projectId)
    {
        if (_store.TryGetValue(projectId, out var tasks))
        {
            return tasks.Values.OrderBy(t => t.Id);
        }
        return Enumerable.Empty<TaskItem>();
    }

    public TaskItem Add(int projectId, string description)
    {
        var id = Interlocked.Increment(ref _nextId);
        var item = new TaskItem
        {
            Id = id,
            ProjectId = projectId,
            Description = description,
            IsCompleted = false
        };
        var bucket = _store.GetOrAdd(projectId, _ => new ConcurrentDictionary<int, TaskItem>());
        bucket[id] = item;
        return item;
    }

    public TaskItem? Update(int projectId, int id, string? description, bool? isCompleted)
    {
        if (!_store.TryGetValue(projectId, out var bucket))
            return null;
        if (!bucket.TryGetValue(id, out var existing))
            return null;

        var updated = new TaskItem
        {
            Id = existing.Id,
            ProjectId = projectId,
            Description = string.IsNullOrWhiteSpace(description) ? existing.Description : description!.Trim(),
            IsCompleted = isCompleted ?? existing.IsCompleted
        };
        bucket[id] = updated;
        return updated;
    }

    public bool Delete(int projectId, int id)
    {
        if (!_store.TryGetValue(projectId, out var bucket))
            return false;
        return bucket.TryRemove(id, out _);
    }
}


