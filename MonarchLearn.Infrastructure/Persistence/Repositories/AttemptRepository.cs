using Microsoft.EntityFrameworkCore;
using MonarchLearn.Application.Interfaces.Repositories;
using MonarchLearn.Domain.Entities.LearningProgress;
using MonarchLearn.Infrastructure.Persistence.Context;

namespace MonarchLearn.Infrastructure.Persistence.Repositories
{
    public class AttemptRepository : GenericRepository<Attempt>, IAttemptRepository
    {
        public AttemptRepository(MonarchLearnDbContext context) : base(context) { }

        public async Task<List<Attempt>> GetAttemptsByEnrollmentAsync(int enrollmentId)
        {
            return await _context.Attempts
                .Include(a => a.Answers)
                    .ThenInclude(ans => ans.SelectedOption)
                .Where(a => a.EnrollmentId == enrollmentId)
                .ToListAsync();
        }
    }
}
