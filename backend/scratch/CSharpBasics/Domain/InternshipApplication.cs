namespace CSharpBasics.Domain;

// Named "InternshipApplication" (not just "Application") to avoid confusion with the
// .NET application itself, and to match the entity name planned in DATABASE_DESIGN.md.
public class InternshipApplication
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int InternshipId { get; set; }
    public ApplicationStatus Status { get; set; }

    public InternshipApplication(int id, int studentId, int internshipId)
    {
        Id = id;
        StudentId = studentId;
        InternshipId = internshipId;
        Status = ApplicationStatus.Pending; // every application starts Pending (Phase 9)
    }
}
