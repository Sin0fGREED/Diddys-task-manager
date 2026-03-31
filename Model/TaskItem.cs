public enum PriorityLevel
{
	Low,
	Medium,
	High
}

public enum StatusLevel
{
	ToDo,
	InProgress,
	Done
}

public class TaskItem
{
	public int Id { get; set; }
	public required string Description { get; set; }
	public StatusLevel Status { get; set; } = StatusLevel.ToDo;
	public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
	public DateTime CreationDate { get; set; } = DateTime.Now;
	public string CreatedBy { get; set; } = string.Empty; // Username of task creator
	public string? AssignedTo { get; set; } = null; // Username of assigned team member
}