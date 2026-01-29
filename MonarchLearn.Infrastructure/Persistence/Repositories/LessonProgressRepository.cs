using Microsoft.EntityFrameworkCore;
using MonarchLearn.Application.Interfaces.Repositories;
using MonarchLearn.Domain.Entities.LearningProgress;
using MonarchLearn.Infrastructure.Persistence.Context;

namespace MonarchLearn.Infrastructure.Persistence.Repositories
{
    public class LessonProgressRepository : GenericRepository<LessonProgress>, ILessonProgressRepository
    {
        public LessonProgressRepository(MonarchLearnDbContext context) : base(context) { }

        public async Task<LessonProgress?> GetProgressAsync(int enrollmentId, int lessonItemId)
        {
            return await _context.LessonProgresses
                .FirstOrDefaultAsync(lp =>
                    lp.EnrollmentId == enrollmentId &&
                    lp.LessonItemId == lessonItemId);
        }
    }
}
