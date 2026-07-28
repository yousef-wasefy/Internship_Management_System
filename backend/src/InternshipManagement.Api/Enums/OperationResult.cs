namespace InternshipManagement.Api.Enums;

// Shared result for service methods that act on a specific resource by id and need to
// distinguish "doesn't exist" (404) from "exists, but you don't own it" (403) from
// "exists, you own it, but the request violates a business rule" (400) from success.
// Reused by InternshipService here, and expected to be reused for application ownership
// checks in Phase 9/10.
public enum OperationResult
{
    Success,
    NotFound,
    Forbidden,
    ValidationFailed
}
