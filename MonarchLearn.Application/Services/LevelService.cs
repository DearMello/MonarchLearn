using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using MonarchLearn.Application.DTOs.Common;
using MonarchLearn.Application.Interfaces.Services;
using MonarchLearn.Application.Interfaces.UnitOfWork;
using MonarchLearn.Domain.Entities.Courses;
using MonarchLearn.Domain.Exceptions;
using MonarchLearn.Application.Cache;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace MonarchLearn.Application.Services
{
    public class LevelService : ILevelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<LevelService> _logger;
        private readonly IDistributedCache _cache;
        private const string CACHE_KEY = "all_levels_lookup";

        public LevelService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<LevelService> logger, IDistributedCache cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<List<LookupDto>> GetAllLevelsAsync()
        {
            _logger.LogDebug("Fetching all levels from cache or database");
            var cached = await _cache.GetRecordAsync<List<LookupDto>>(CACHE_KEY);
            if (cached != null)
            {
                _logger.LogInformation("Returning levels from Cache.");
                return cached;
            }

            var levels = await _unitOfWork.CourseLevels.GetAllAsync();
            var dtos = _mapper.Map<List<LookupDto>>(levels);

            await _cache.SetRecordAsync(CACHE_KEY, dtos, TimeSpan.FromHours(24));
            _logger.LogInformation("Retrieved {Count} level(s) from DATABASE and updated cache", levels.Count);
            return dtos;
        }

        public async Task<LookupDto> GetLevelByIdAsync(int levelId)
        {
            _logger.LogDebug("Fetching level ID {LevelId}", levelId);
            var level = await _unitOfWork.CourseLevels.GetByIdAsync(levelId);
            if (level == null)
            {
                _logger.LogWarning("Level ID {LevelId} not found", levelId);
                throw new NotFoundException("Level", levelId);
            }
            return _mapper.Map<LookupDto>(level);
        }

        public async Task<LookupDto> CreateLevelAsync(CreateLookupDto dto)
        {
            _logger.LogInformation("Creating level: {Name}", dto.Name);
            var existingLevels = await _unitOfWork.CourseLevels.FindAsync(l => l.Name == dto.Name);
            if (existingLevels.Any())
            {
                _logger.LogWarning("Level creation failed: Name '{Name}' already exists", dto.Name);
                throw new ConflictException($"Level '{dto.Name}' already exists");
            }

            var level = _mapper.Map<CourseLevel>(dto);
            level.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.CourseLevels.AddAsync(level);
            await _unitOfWork.SaveChangesAsync();

            await _cache.RemoveAsync(CACHE_KEY);
            _logger.LogInformation("Level created: {Name}. Cache invalidated.", level.Name);
            return _mapper.Map<LookupDto>(level);
        }

        public async Task UpdateLevelAsync(int levelId, CreateLookupDto dto)
        {
            _logger.LogInformation("Updating level ID {LevelId}", levelId);
            var level = await _unitOfWork.CourseLevels.GetByIdAsync(levelId);
            if (level == null)
            {
                _logger.LogWarning("Update failed: Level ID {LevelId} not found", levelId);
                throw new NotFoundException("Level", levelId);
            }

            var duplicates = await _unitOfWork.CourseLevels.FindAsync(l => l.Name == dto.Name && l.Id != levelId);
            if (duplicates.Any())
            {
                _logger.LogWarning("Update failed: Level name '{Name}' already exists", dto.Name);
                throw new ConflictException($"Level '{dto.Name}' already exists");
            }

            string oldName = level.Name;
            level.Name = dto.Name;
            level.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.CourseLevels.Update(level);
            await _unitOfWork.SaveChangesAsync();

            await _cache.RemoveAsync(CACHE_KEY);
            _logger.LogInformation("Level ID {Id} updated: '{OldName}' -> '{NewName}'. Cache invalidated.", levelId, oldName, level.Name);
        }

        public async Task DeleteLevelAsync(int levelId)
        {
            _logger.LogInformation("Deleting level ID {LevelId}", levelId);
            var level = await _unitOfWork.CourseLevels.GetByIdAsync(levelId);
            if (level == null)
            {
                _logger.LogWarning("Delete failed: Level ID {LevelId} not found", levelId);
                throw new NotFoundException("Level", levelId);
            }

            var coursesUsingLevel = await _unitOfWork.Courses.FindAsync(c => c.LevelId == levelId);
            if (coursesUsingLevel.Any())
            {
                _logger.LogWarning("Delete failed: Level ID {LevelId} is used by courses", levelId);
                throw new ConflictException($"Cannot delete level '{level.Name}' - used by {coursesUsingLevel.Count} course(s)");
            }

            _unitOfWork.CourseLevels.Delete(level);
            await _unitOfWork.SaveChangesAsync();

            await _cache.RemoveAsync(CACHE_KEY);
            _logger.LogWarning("Level ID {Id} ('{Name}') deleted successfully. Cache invalidated.", levelId, level.Name);
        }
    }
}