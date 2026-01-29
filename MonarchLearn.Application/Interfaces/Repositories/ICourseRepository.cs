using MonarchLearn.Application.DTOs.Courses;
using MonarchLearn.Domain.Entities.Courses;

namespace MonarchLearn.Application.Interfaces.Repositories
{
    public interface ICourseRepository : IGenericRepository<Course>
    {
        Task<Course?> GetCourseWithFullContentAsync(int courseId);
        Task<List<Course>> GetInstructorCoursesAsync(int instructorId);
        Task<List<Course>> GetCoursesByFilterAsync(CourseFilterDto filter);

        Task<List<Course>> GetTopPopularCoursesAsync(int count);
        Task<List<Course>> GetTrendingCoursesAsync(int count, DateTime afterDate);
    }
}
