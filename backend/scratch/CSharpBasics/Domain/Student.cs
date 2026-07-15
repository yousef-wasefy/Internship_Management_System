namespace CSharpBasics.Domain;

// A class bundles related data (properties) and behavior (methods) into one type.
// Properties are C#'s syntax for controlled field access (get/set) - similar to writing
// getStudent()/setStudent() in Java, but built into the language instead of hand-written.
public class Student
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string University { get; set; } = string.Empty;

    // List<T> is C#'s generic resizable array - the equivalent of Java's List<T> /
    // C++'s std::vector<T>. Initialized here so callers never get a null list.
    public List<string> Skills { get; set; } = new();

    // A constructor runs once, when `new Student(...)` is called, to guarantee the
    // required fields are set before anyone can use the object.
    public Student(int id, string fullName, string university)
    {
        Id = id;
        FullName = fullName;
        University = university;
    }
}
