namespace CSharpBasics.Domain;

// An enum restricts a value to one of a fixed set of named options, instead of a raw
// string or int that could hold anything. Mirrors the workflow in docs/REQUIREMENTS.md §5.
public enum InternshipStatus
{
    Draft,
    Open,
    Closed,
    Cancelled
}

public enum ApplicationStatus
{
    Pending,
    Shortlisted,
    Accepted,
    Rejected,
    Withdrawn
}
