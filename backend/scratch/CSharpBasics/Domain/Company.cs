namespace CSharpBasics.Domain;

public class Company
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;

    // Companies start unapproved and must be approved by an admin (REQUIREMENTS.md CO-3).
    public bool IsApproved { get; set; }

    public Company(int id, string companyName)
    {
        Id = id;
        CompanyName = companyName;
        IsApproved = false;
    }
}
