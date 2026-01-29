using MonarchLearn.Domain.Entities.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Repositories
{
    public interface IReviewRepository : IGenericRepository<CourseReview>
    {
        // Kursa aid bütün rəyləri (User məlumatları ilə birlikdə) gətirir
        Task<List<CourseReview>> GetReviewsByCourseIdAsync(int courseId);
    }
}
