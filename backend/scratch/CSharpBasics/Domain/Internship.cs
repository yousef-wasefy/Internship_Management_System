namespace CSharpBasics.Domain;

public class Internship
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public InternshipStatus Status { get; set; }
    public DateTime ApplicationDeadline { get; set; }

    public Internship(int id, string title, DateTime applicationDeadline)
    {
        Id = id;
        Title = title;
        ApplicationDeadline = applicationDeadline;
        Status = InternshipStatus.Draft; // every internship starts as a draft (Phase 8)
    }
}
