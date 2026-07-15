using CSharpBasics.Data;
using CSharpBasics.Domain;
using CSharpBasics.Services;

Console.WriteLine("=== C# Basics Practice (Phase 2) ===\n");

// 1) Classes, constructors, object initializers, properties
var company = new Company(1, "Acme Corp") { IsApproved = true };
var internship = new Internship(1, "Backend Intern", DateTime.Now.AddDays(10))
{
    Status = InternshipStatus.Open
};
Console.WriteLine($"Company: {company.CompanyName} (approved: {company.IsApproved})");
Console.WriteLine($"Internship: {internship.Title} - status: {internship.Status}");

// 2) List<T>
var student = new Student(1, "Omar Khaled", "Ain Shams University");
student.Skills.Add("C#");
student.Skills.Add("SQL");
Console.WriteLine($"Student: {student.FullName}, skills: {string.Join(", ", student.Skills)}");

// 3) Interface + implementation - we program against IApplicationValidator, not
//    ApplicationValidator, so the check below doesn't care which implementation runs.
IApplicationValidator validator = new ApplicationValidator();
bool canApply = validator.CanApply(internship, DateTime.Now);
Console.WriteLine($"Can {student.FullName} apply? {canApply}");

if (canApply)
{
    var application = new InternshipApplication(1, student.Id, internship.Id);
    Console.WriteLine($"Application created with status: {application.Status}");
}

// Prove the validator actually rejects invalid states too, not just accepts everything.
var closedInternship = new Internship(2, "Closed Internship", DateTime.Now.AddDays(10))
{
    Status = InternshipStatus.Closed
};
Console.WriteLine($"Can apply to a Closed internship? {validator.CanApply(closedInternship, DateTime.Now)}");

var expiredInternship = new Internship(3, "Expired Internship", DateTime.Now.AddDays(-1))
{
    Status = InternshipStatus.Open
};
Console.WriteLine($"Can apply after the deadline? {validator.CanApply(expiredInternship, DateTime.Now)}");

// 4) async/await - fetching "from the database" without blocking the thread
var repository = new FakeStudentRepository();
Console.WriteLine("\nFetching a student from a simulated database...");
Student fetchedStudent = await repository.GetByIdAsync(42);
Console.WriteLine($"Fetched: {fetchedStudent.FullName} from {fetchedStudent.University}");

Console.WriteLine("\nDone.");
