using MonarchLearn.Application.DTOs.Courses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Services
{
    public interface ICourseService
    {
        
        Task<List<CourseCardDto>> GetCoursesAsync(CourseFilterDto filter);

        Task<CourseDetailDto> GetCourseDetailAsync(int courseId);

        Task<List<CourseCardDto>> GetInstructorCoursesAsync(int instructorId);
    }
}