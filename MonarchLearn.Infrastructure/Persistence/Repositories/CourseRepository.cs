using Microsoft.EntityFrameworkCore;
using MonarchLearn.Application.DTOs.Courses;
using MonarchLearn.Application.Interfaces.Repositories;
using MonarchLearn.Domain.Entities.Courses;
using MonarchLearn.Infrastructure.Persistence.Context;

namespace MonarchLearn.Infrastructure.Persistence.Repositories
{
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        public CourseRepository(MonarchLearnDbContext context) : base(context) { }

        public async Task<List<Course>> GetTopPopularCoursesAsync(int count)
        {
            return await _context.Courses
                .AsNoTracking()
                .Include(c => c.Instructor)
                .Include(c => c.Category)
                .Include(c => c.Level)
                .Include(c => c.Statistics)
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.Statistics != null ? c.Statistics.PopularityScore : 0)
                .ThenByDescending(c => c.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Course>> GetTrendingCoursesAsync(int count, DateTime afterDate)
        {
            return await _context.Courses
                .AsNoTracking()
                .Include(c => c.Instructor)
                .Include(c => c.Category)
                .Include(c => c.Level)
                .Include(c => c.Statistics)
                .Where(c => !c.IsDeleted && c.CreatedAt >= afterDate)
                .OrderByDescending(c => c.Statistics != null ? c.Statistics.PopularityScore : 0)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Course?> GetCourseWithFullContentAsync(int courseId)
        {
            return await _context.Courses
                .Include(c => c.Modules)
                    .ThenInclude(m => m.LessonItems)
                        .ThenInclude(li => li.Quiz)
                            .ThenInclude(q => q.Questions)
                                .ThenInclude(q => q.Options)
                .Include(c => c.Category)
                .Include(c => c.Level)
                .Include(c => c.Language)
                .Include(c => c.Skills)  
                .Include(c => c.Statistics)
                .Include(c => c.Instructor)  
                .FirstOrDefaultAsync(c => c.Id == courseId);
        }

        public async Task<List<Course>> GetInstructorCoursesAsync(int instructorId)
        {
            return await _context.Courses
                .Include(c => c.Category)  
                .Include(c => c.Level)      
                .Include(c => c.Instructor) 
                .Include(c => c.Statistics) 
                .Where(c => c.InstructorId == instructorId && !c.IsDeleted)
                .ToListAsync();
        }

        public async Task<List<Course>> GetCoursesByFilterAsync(CourseFilterDto filter)
        {
            var query = _context.Courses
                .AsNoTracking()
                .Include(c => c.Instructor)
                .Include(c => c.Level)
                .Include(c => c.Category)
                .Include(c => c.Language)   
                .Include(c => c.Skills)     
                .Include(c => c.Statistics)
                .Where(c => !c.IsDeleted)
                .AsQueryable();

            
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(c => c.Title.Contains(filter.SearchTerm) ||
                                         c.ShortDescription.Contains(filter.SearchTerm));
            }

            if (filter.CategoryId.HasValue)
                query = query.Where(c => c.CategoryId == filter.CategoryId.Value);

            if (filter.LevelId.HasValue)
                query = query.Where(c => c.LevelId == filter.LevelId.Value);

            if (filter.LanguageId.HasValue)
                query = query.Where(c => c.LanguageId == filter.LanguageId.Value);

            //  Use cached AverageRating instead of calculating from Reviews
            if (filter.MinRating.HasValue)
            {
                query = query.Where(c => c.AverageRating >= filter.MinRating.Value);
            }

            
            if (filter.SkillIds != null && filter.SkillIds.Any())
            {
                // Course must have at least ONE of the selected skills
                query = query.Where(c => c.Skills.Any(s => filter.SkillIds.Contains(s.Id)));
            }

           
            query = query.OrderByDescending(c => c.Statistics != null ? c.Statistics.PopularityScore : 0)
                         .ThenByDescending(c => c.CreatedAt);

            
            return await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();
        }
    }
}