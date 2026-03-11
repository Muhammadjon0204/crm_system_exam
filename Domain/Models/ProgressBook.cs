namespace Domain.Models;

public class ProgressBook
{
    public int Id { get; set; }
    public int Grade { get; set; }
    public int StudentId { get; set; }
    public bool IsAttended { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public int GroupId { get; set; }
    public string Notes { get; set; } = "";
    public int LateMinutes { get; set; } = 0;
    public string UpdateByUserId { get; set; } = "";
}