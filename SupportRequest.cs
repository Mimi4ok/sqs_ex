namespace sqs_ex.Models;

public enum Priority { Low = 1, Medium = 2, High = 3 }

public class SupportRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserName { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Priority Priority { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}