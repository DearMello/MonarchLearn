using MonarchLearn.Application.DTOs.Enrollment;
using MonarchLearn.Domain.Entities.Enrollments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Services
{
    public interface IEnrollmentService
    {
        // Tələbəni kursa yazır
        Task<EnrollmentDto> EnrollStudentAsync(int userId, int courseId);

        // Tələbənin "My Learning" səhifəsindəki kursları gətirir
        Task<List<EnrollmentDto>> GetStudentEnrollmentsAsync(int userId);

        // Resume düyməsi basılanda son qaldığı dərsin ID-sini verir
        Task<int?> GetLastLessonIdAsync(int userId, int courseId);
        // Application/Interfaces/Services/IEnrollmentService.cs
        Task<Enrollment?> GetUserEnrollmentAsync(int userId, int courseId);
        Task<object> GetCourseProgressAsync(int userId, int courseId);
    }
}
