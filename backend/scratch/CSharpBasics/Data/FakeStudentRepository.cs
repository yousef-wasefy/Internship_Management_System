using CSharpBasics.Domain;

namespace CSharpBasics.Data;

// Simulates fetching a student from a database.
// Real database calls are I/O-bound (the CPU sits idle while waiting on the network/disk),
// so they use async/await instead of blocking the calling thread. Every EF Core query from
// Phase 4 onward will look exactly like this: an `async Task<T>` method the caller `await`s.
public class FakeStudentRepository
{
    public async Task<Student> GetByIdAsync(int id)
    {
        await Task.Delay(200); // pretend this is a 200ms database round-trip
        return new Student(id, "Sara Ahmed", "Cairo University");
    }
}
