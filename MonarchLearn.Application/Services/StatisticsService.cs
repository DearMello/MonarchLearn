using AutoMapper;
using Microsoft.Extensions.Logging;
using MonarchLearn.Application.DTOs.Courses;
using MonarchLearn.Application.Interfaces.Services;
using MonarchLearn.Application.Interfaces.UnitOfWork;
using MonarchLearn.Domain.Entities.Statistics;
using MonarchLearn.Domain.Exceptions;

namespace MonarchLearn.Application.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<StatisticsService> _logger;

        public StatisticsService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<StatisticsService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

     
        public async Task<List<CourseCardDto>> GetMostPopularCoursesAsync(int count)
        {
            _logger.LogInformation("Fetching top {Count} most popular courses", count);

            
            if (count <= 0)
            {
                _logger.LogWarning("Invalid count value: {Count}", count);
                throw new BadRequestException("Count must be greater than 0");
            }

            
            if (count > 100)
            {
                _logger.LogWarning("Count {Count} exceeds maximum allowed (100)", count);
                throw new BadRequestException("Count cannot exceed 100");
            }

            try
            {
                
                var courses = await _unitOfWork.Courses.GetTopPopularCoursesAsync(count);

                _logger.LogInformation("Retrieved {Count} popular course(s)", courses.Count);

                return _mapper.Map<List<CourseCardDto>>(courses);
            }
            catch (Exception ex) when (ex is not DomainException)
            {
                _logger.LogError(ex, "Failed to fetch popular courses");
                throw new BadRequestException("Failed to load popular courses. Please try again later.");
            }
        }

      
        public async Task<List<CourseCardDto>> GetTrendingCoursesAsync(int count)
        {
            _logger.LogInformation("Fetching top {Count} trending courses (last 30 days)", count);

            //  Validation - count must be positive
            if (count <= 0)
            {
                _logger.LogWarning("Invalid count value: {Count}", count);
                throw new BadRequestException("Count must be greater than 0");
            }

            //  Validation - Reasonable limit
            if (count > 100)
            {
                _logger.LogWarning("Count {Count} exceeds maximum allowed (100)", count);
                throw new BadRequestException("Count cannot exceed 100");
            }

            try
            {
                // Calculate cutoff date (30 days ago)
                var cutoffDate = DateTime.UtcNow.AddDays(-30);

                _logger.LogDebug("Trending courses cutoff date: {CutoffDate}", cutoffDate);

                // Repository method returns recently popular courses
                var courses = await _unitOfWork.Courses.GetTrendingCoursesAsync(count, cutoffDate);

                _logger.LogInformation(
                    "Retrieved {Count} trending course(s) created after {CutoffDate}",
                    courses.Count, cutoffDate.ToShortDateString());

                return _mapper.Map<List<CourseCardDto>>(courses);
            }
            catch (Exception ex) when (ex is not DomainException)
            {
                _logger.LogError(ex, "Failed to fetch trending courses");
                throw new BadRequestException("Failed to load trending courses. Please try again later.");
            }
        }

     
        // GET TOTAL VIEWS ACROSS ALL COURSES
       
        public async Task<int> GetTotalViewsAsync()
        {
            _logger.LogDebug("Calculating total views across all courses");

            try
            {
                // Get all course statistics
                var allStats = await _unitOfWork.CourseStatistics.GetAllAsync();

                if (!allStats.Any())
                {
                    _logger.LogInformation("No course statistics found, returning 0 total views");
                    return 0;
                }

                // Sum all view counts
                int totalViews = allStats.Sum(s => s.ViewCount);

                _logger.LogInformation(
                    "Total views calculated: {TotalViews} across {CourseCount} course(s)",
                    totalViews, allStats.Count);

                return totalViews;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate total views");
                throw new BadRequestException("Failed to load statistics. Please try again later.");
            }
        }


        public async Task IncrementViewCountAsync(int courseId)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null) return;

            // Statistikaları yükləyirik (və ya yaradırıq)
            var stats = await _unitOfWork.CourseStatistics.FindAsync(s => s.CourseId == courseId);
            var courseStats = stats.FirstOrDefault();

            if (courseStats == null)
            {
                courseStats = new CourseStatistics
                {
                    CourseId = courseId,
                    ViewCount = 1,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.CourseStatistics.AddAsync(courseStats);
            }
            else
            {
                courseStats.ViewCount++;
                courseStats.UpdatedAt = DateTime.UtcNow;
            }

            // Popularity Score hesablanması (Məntiq bura köçdü)
            double viewScore = Math.Log10(courseStats.ViewCount + 1) * 5;
            var enrollments = await _unitOfWork.Enrollments.FindAsync(e => e.CourseId == courseId && !e.IsDeleted);
            double enrollScore = enrollments.Count * 10;

            // Rəylərin ortalaması
            var reviews = await _unitOfWork.Reviews.FindAsync(r => r.CourseId == courseId && !r.IsDeleted);
            double ratingScore = reviews.Any() ? reviews.Average(r => r.Rating) * 20 : 0;

            courseStats.PopularityScore = Math.Round(viewScore + enrollScore + ratingScore, 2);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}