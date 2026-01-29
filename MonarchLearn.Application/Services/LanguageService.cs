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
    public class LanguageService : ILanguageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<LanguageService> _logger;
        private readonly IDistributedCache _cache;
        private const string CACHE_KEY = "all_languages_lookup";

        public LanguageService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<LanguageService> logger, IDistributedCache cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<List<LookupDto>> GetAllLanguagesAsync()
        {
            _logger.LogDebug("Fetching all languages from cache or database");
            var cached = await _cache.GetRecordAsync<List<LookupDto>>(CACHE_KEY);
            if (cached != null)
            {
                _logger.LogInformation("Returning languages from Cache.");
                return cached;
            }

            var languages = await _unitOfWork.CourseLanguages.GetAllAsync();
            var dtos = _mapper.Map<List<LookupDto>>(languages);

            await _cache.SetRecordAsync(CACHE_KEY, dtos, TimeSpan.FromHours(24));
            _logger.LogInformation("Retrieved {Count} language(s) from DATABASE and updated cache", languages.Count);
            return dtos;
        }

        public async Task<LookupDto> GetLanguageByIdAsync(int languageId)
        {
            _logger.LogDebug("Fetching language ID {LanguageId}", languageId);
            var language = await _unitOfWork.CourseLanguages.GetByIdAsync(languageId);
            if (language == null)
            {
                _logger.LogWarning("Language ID {LanguageId} not found", languageId);
                throw new NotFoundException("Language", languageId);
            }
            return _mapper.Map<LookupDto>(language);
        }

        public async Task<LookupDto> CreateLanguageAsync(CreateLookupDto dto)
        {
            _logger.LogInformation("Creating language: {Name}", dto.Name);
            var existingLanguages = await _unitOfWork.CourseLanguages.FindAsync(l => l.Name == dto.Name);
            if (existingLanguages.Any())
            {
                _logger.LogWarning("Language creation failed: Name '{Name}' already exists", dto.Name);
                throw new ConflictException($"Language with name '{dto.Name}' already exists");
            }

            var language = _mapper.Map<CourseLanguage>(dto);
            language.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.CourseLanguages.AddAsync(language);
            await _unitOfWork.SaveChangesAsync();

            await _cache.RemoveAsync(CACHE_KEY);
            _logger.LogInformation("Language created successfully: ID {Id}, Name '{Name}'. Cache invalidated.", language.Id, language.Name);
            return _mapper.Map<LookupDto>(language);
        }

        public async Task UpdateLanguageAsync(int languageId, CreateLookupDto dto)
        {
            _logger.LogInformation("Updating language ID {LanguageId}", languageId);
            var language = await _unitOfWork.CourseLanguages.GetByIdAsync(languageId);
            if (language == null)
            {
                _logger.LogWarning("Update failed: Language ID {LanguageId} not found", languageId);
                throw new NotFoundException("Language", languageId);
            }

            var duplicateLanguages = await _unitOfWork.CourseLanguages.FindAsync(l => l.Name == dto.Name && l.Id != languageId);
            if (duplicateLanguages.Any())
            {
                _logger.LogWarning("Update failed: Language name '{Name}' already exists", dto.Name);
                throw new ConflictException($"Language with name '{dto.Name}' already exists");
            }

            string oldName = language.Name;
            language.Name = dto.Name;
            language.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.CourseLanguages.Update(language);
            await _unitOfWork.SaveChangesAsync();

            await _cache.RemoveAsync(CACHE_KEY);
            _logger.LogInformation("Language ID {Id} updated: '{OldName}' -> '{NewName}'. Cache invalidated.", languageId, oldName, language.Name);
        }

        public async Task DeleteLanguageAsync(int languageId)
        {
            _logger.LogInformation("Deleting language ID {LanguageId}", languageId);
            var language = await _unitOfWork.CourseLanguages.GetByIdAsync(languageId);
            if (language == null)
            {
                _logger.LogWarning("Delete failed: Language ID {LanguageId} not found", languageId);
                throw new NotFoundException("Language", languageId);
            }

            var coursesUsingLanguage = await _unitOfWork.Courses.FindAsync(c => c.LanguageId == languageId);
            if (coursesUsingLanguage.Any())
            {
                _logger.LogWarning("Delete failed: Language ID {LanguageId} is used by {Count} course(s)", languageId, coursesUsingLanguage.Count);
                throw new ConflictException($"Cannot delete language '{language.Name}' because it is used by courses");
            }

            _unitOfWork.CourseLanguages.Delete(language);
            await _unitOfWork.SaveChangesAsync();

            await _cache.RemoveAsync(CACHE_KEY);
            _logger.LogWarning("Language ID {Id} ('{Name}') deleted successfully. Cache invalidated.", languageId, language.Name);
        }
    }
}