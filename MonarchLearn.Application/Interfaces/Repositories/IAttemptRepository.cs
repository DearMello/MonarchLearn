using MonarchLearn.Domain.Entities.LearningProgress;

namespace MonarchLearn.Application.Interfaces.Repositories
{
    public interface IAttemptRepository : IGenericRepository<Attempt>
    {
        Task<List<Attempt>> GetAttemptsByEnrollmentAsync(int enrollmentId);
    }
}
