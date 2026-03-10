public enum PriorityLevel
{
	Low,
	Medium,
	High
}

public class TaskItem
{
	public int Id { get; set; }
	public required string Description { get; set; }
	public bool Completed { get; set; }
	public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
	public DateTime CreationDate { get; set; } = DateTime.Now;
}