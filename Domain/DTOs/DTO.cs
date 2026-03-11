public class AverageGradeDto
{
    public int StudentId { get; set; }
    public decimal AverageGrade { get; set; }
    public string? Message { get; set; }
}

public class TopStudentDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public decimal AverageGrade { get; set; }
    public decimal AttendancePercent { get; set; }
}

public class AttendanceDto
{
    public int StudentId { get; set; }
    public double AttendancePercentage { get; set; }
    public long Attended { get; set; }
    public long Total { get; set; }
    public string? Message { get; set; }
}

public class AttendanceRaw
{
    public long Total { get; set; }
    public long Attended { get; set; }
}
