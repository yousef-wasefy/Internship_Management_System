using CSharpBasics.Domain;

namespace CSharpBasics.Services;

// The concrete implementation of IApplicationValidator.
// Mirrors the real business rule from REQUIREMENTS.md §4.1: a student can only apply to
// an Open internship, and only before its deadline.
public class ApplicationValidator : IApplicationValidator
{
    public bool CanApply(Internship internship, DateTime now)
    {
        return internship.Status == InternshipStatus.Open && now <= internship.ApplicationDeadline;
    }
}
