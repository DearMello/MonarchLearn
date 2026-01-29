using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using MonarchLearn.Application.DTOs.Courses;
using MonarchLearn.Application.Interfaces.Services;
using MonarchLearn.Application.Interfaces.UnitOfWork;
using MonarchLearn.Domain.Exceptions;
using MonarchLearn.Application.Cache;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Services
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CourseService> _logger;
        private readonly IDistributedCache _cache;
        private readonly IFileService _fileService; 

        public CourseService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CourseService> logger,
            IDistributedCache cache,
            IFileService fileService) 
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _fileService = fileService;
        }

        public async Task<List<CourseCardDto>> GetInstructorCoursesAsync(int instructorId)
        {
            _logger.LogInformation("Fetching authored courses for Instructor {InstructorId}", instructorId);

            try
            {
                var courses = await _unitOfWork.Courses.GetInstructorCoursesAsync(instructorId);

                if (courses == null || !courses.Any())
                {
                    _logger.LogInformation("No authored courses found for Instructor {InstructorId}", instructorId);
                    return new List<CourseCardDto>();
                }

                var dtos = _mapper.Map<List<CourseCardDto>>(courses);

                
                foreach (var dto in dtos)
                {
                    dto.ImageUrl = _fileService.GetFullUrl(dto.ImageUrl);
                }

                _logger.LogInformation("Successfully mapped {Count} courses for Instructor {InstructorId}", dtos.Count, instructorId);

                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching instructor courses for ID {InstructorId}", instructorId);
                throw new BadRequestException("Failed to retrieve your courses.");
            }
        }

        public async Task<List<CourseCardDto>> GetCoursesAsync(CourseFilterDto filter)
        {
            string cacheKey = $"courses_p{filter.PageNumber}_s{filter.PageSize}" +
                  $"_cat{filter.CategoryId ?? 0}" +
                  $"_lvl{filter.LevelId ?? 0}" +
                  $"_lng{filter.LanguageId ?? 0}" +
                  $"_rt{filter.MinRating ?? 0}" +
                  $"_sk{string.Join("-", filter.SkillIds ?? new List<int>())}" +
                  $"_q{filter.SearchTerm?.GetHashCode() ?? 0}";

            _logger.LogInformation("Fetching courses. CacheKey: {CacheKey}", cacheKey);

            try
            {
                var cachedCourses = await _cache.GetRecordAsync<List<CourseCardDto>>(cacheKey);
                if (cachedCourses != null)
                {
                    _logger.LogInformation("Returning courses from Cache.");
                    return cachedCourses;
                }

                _logger.LogInformation("Cache miss. Fetching from Database.");
                var courses = await _unitOfWork.Courses.GetCoursesByFilterAsync(filter);
                var dtos = _mapper.Map<List<CourseCardDto>>(courses);

               
                foreach (var dto in dtos)
                {
                    dto.ImageUrl = _fileService.GetFullUrl(dto.ImageUrl);
                }

                await _cache.SetRecordAsync(cacheKey, dtos, TimeSpan.FromMinutes(10));
                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching courses with key {CacheKey}", cacheKey);
                throw new BadRequestException("Failed to retrieve courses.");
            }
        }

        public async Task<CourseDetailDto> GetCourseDetailAsync(int courseId)
        {
            _logger.LogInformation("Fetching detailed course data for ID {CourseId}", courseId);

            var course = await _unitOfWork.Courses.GetCourseWithFullContentAsync(courseId);
            if (course == null)
            {
                _logger.LogWarning("Course not found: ID {CourseId}", courseId);
                throw new NotFoundException("Course", courseId);
            }

            var dto = _mapper.Map<CourseDetailDto>(course);

           
            dto.ImageUrl = _fileService.GetFullUrl(dto.ImageUrl);

            dto.Rating = course.AverageRating;

            BackgroundJob.Enqueue<IStatisticsService>(s => s.IncrementViewCountAsync(courseId));

            return dto;
        }
    }
}