using MonarchLearn.Application.DTOs.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Services
{
    public interface IStatisticsService
    {
        // Ən məşhur kurslar (Popularity Score-a görə)
        Task<List<CourseCardDto>> GetMostPopularCoursesAsync(int count);

        // Trenddə olanlar (Son 30 gündə yaranan və populyar olanlar)
        Task<List<CourseCardDto>> GetTrendingCoursesAsync(int count);

        // Ümumi baxış sayı (Admin Dashboard üçün)
        Task<int> GetTotalViewsAsync();

        // YENİ: Arxa planda işləyəcək metod
        Task IncrementViewCountAsync(int courseId);
    }
}
