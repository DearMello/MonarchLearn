using Microsoft.EntityFrameworkCore; // <--- BAX BU SƏTİR ÇATIŞMIRDI
using MonarchLearn.Application.Interfaces.Repositories;
using MonarchLearn.Domain.Entities.Courses;
using MonarchLearn.Infrastructure.Persistence.Context;

namespace MonarchLearn.Infrastructure.Persistence.Repositories
{
    public class ReviewRepository : GenericRepository<CourseReview>, IReviewRepository
    {
        public ReviewRepository(MonarchLearnDbContext context) : base(context) { }

        public async Task<List<CourseReview>> GetReviewsByCourseIdAsync(int courseId)
        {
            
            return await _context.Set<CourseReview>()
                .Where(r => r.CourseId == courseId)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}