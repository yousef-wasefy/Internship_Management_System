using CSharpBasics.Domain;

namespace CSharpBasics.Services;

// An interface defines a contract - what a method is called and what it returns -
// without saying how it works. Code that depends on IApplicationValidator (instead of
// ApplicationValidator directly) doesn't care which implementation it gets. This is what
// lets the real ApplicationService (Phase 9) be tested with a fake validator later.
public interface IApplicationValidator
{
    bool CanApply(Internship internship, DateTime now);
}
