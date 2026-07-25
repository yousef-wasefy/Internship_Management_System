using InternshipManagement.Api.DTOs.Students;

namespace InternshipManagement.Api.Services.Interfaces;

public interface IStudentService
{
    // Looked up by the logged-in user's id (from the JWT), not the profile's own id -
    // there's no route where a student's profile id is public/relevant.
    Task<StudentProfileDto?> GetMyProfileAsync(int userId);
    Task<bool> UpdateMyProfileAsync(int userId, UpdateStudentProfileDto dto);
}
