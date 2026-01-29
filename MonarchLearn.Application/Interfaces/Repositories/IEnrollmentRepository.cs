using MonarchLearn.Domain.Entities.Enrollments;

namespace MonarchLearn.Application.Interfaces.Repositories
{
    public interface IEnrollmentRepository : IGenericRepository<Enrollment>
    {
        Task<Enrollment?> GetUserEnrollmentAsync(int userId, int courseId);
        Task<List<Enrollment>> GetUserEnrollmentsAsync(int userId);
    }
}
