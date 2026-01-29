using MonarchLearn.Application.DTOs.CourseAdmin;
using MonarchLearn.Application.DTOs.Courses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Services
{
    public interface ICourseManagementService
    {
        Task<int> CreateCourseAsync(int userId, CreateCourseDto model);
        Task UpdateCourseAsync(int userId, UpdateCourseDto model, bool isAdmin = false);
        Task DeleteCourseAsync(int userId, int courseId, bool isAdmin = false);
        Task<List<CourseCardDto>> GetInstructorCoursesAsync(int userId);
    }
}