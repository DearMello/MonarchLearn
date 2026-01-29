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
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;
        private readonly IDistributedCache _cache;
        private const string CACHE_KEY = "all_categories_lookup";

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CategoryService> logger, IDistributedCache cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<List<LookupDto>> GetAllCategoriesAsync()
        {
            _logger.LogDebug("Fetching all categories from cache or database");

            var cachedCategories = await _cache.GetRecordAsync<List<LookupDto>>(CACHE_KEY);
            if (cachedCategories != null)
            {
                _logger.LogInformation("Returning categories from Cache.");
                return cachedCategories;
            }

            var categories = await _unitOfWork.CourseCategories.GetAllAsync();
            var dtos = _mapper.Map<List<LookupDto>>(categories);

            await _cache.SetRecordAsync(CACHE_KEY, dtos, TimeSpan.FromHours(1));
            _logger.LogInformation("Retrieved {Count} categories from DATABASE and updated cache", categories.Count);

            return dtos;
        }

        public async Task<LookupDto> GetCategoryByIdAsync(int categoryId)
        {
            _logger.LogDebug("Fetching category with ID: {CategoryId}", categoryId);
            var category = await _unitOfWork.CourseCategories.GetByIdAsync(categoryId);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found", categoryId);
                throw new NotFoundException("Category", categoryId);
            }
            return _mapper.Map<LookupDto>(category);
        }

        public async Task<LookupDto> CreateCategoryAsync(CreateLookupDto dto)
        {
            _logger.LogInformation("Creating new category: {Name}", dto.Name);
            var existingCategories = await _unitOfWork.CourseCategories.FindAsync(c => c.Name == dto.Name);
            if (existingCategories.Any())
            {
                _logger.LogWarning("Category creation failed: Name '{Name}' already exists", dto.Name);
                throw new ConflictException($"Category with name '{dto.Name}' already exists");
            }

            var category = _mapper.Map<CourseCategory>(dto);
            category.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.CourseCategories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            await _cache.RemoveAsync(CACHE_KEY);
            _logger.LogInformation("Category created successfully: ID {Id}, Name '{Name}'. Cache invalidated.", category.Id, category.Name);

            return _mapper.Map<LookupDto>(category);
        }

        public async Task UpdateCategoryAsync(int categoryId, CreateLookupDto dto)
        {
            _logger.LogInformation("Updating category ID {CategoryId}", categoryId);
            var category = await _unitOfWork.CourseCategories.GetByIdAsync(categoryId);
            if (category == null)
            {
                _logger.LogWarning("Update failed: Category with ID {CategoryId} not found", categoryId);
                throw new NotFoundException("Category", categoryId);
            }

            var duplicateCategories = await _unitOfWork.CourseCategories.FindAsync(c => c.Name == dto.Name && c.Id != categoryId);
            if (duplicateCategories.Any())
            {
                _logger.LogWarning("Update failed: Category name '{Name}' already exists", dto.Name);
                throw new ConflictException($"Category with name '{dto.Name}' already exists");
            }

            string oldName = category.Name;
            _mapper.Map(dto, category);
            category.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.CourseCategories.Update(category);
            await _unitOfWork.SaveChangesAsync();

            await _cache.RemoveAsync(CACHE_KEY);
            _logger.LogInformation("Category ID {Id} updated: '{OldName}' -> '{NewName}'. Cache invalidated.", categoryId, oldName, category.Name);
        }

        public async Task DeleteCategoryAsync(int categoryId)
        {
            _logger.LogInformation("Deleting category ID {CategoryId}", categoryId);
            var category = await _unitOfWork.CourseCategories.GetByIdAsync(categoryId);
            if (category == null)
            {
                _logger.LogWarning("Delete failed: Category with ID {CategoryId} not found", categoryId);
                throw new NotFoundException("Category", categoryId);
            }

            var coursesUsingCategory = await _unitOfWork.Courses.FindAsync(c => c.CategoryId == categoryId);
            if (coursesUsingCategory.Any())
            {
                _logger.LogWarning("Delete failed: Category ID {CategoryId} is used by {Count} courses", categoryId, coursesUsingCategory.Count);
                throw new ConflictException($"Cannot delete category because it is used by {coursesUsingCategory.Count} courses.");
            }

            _unitOfWork.CourseCategories.Delete(category);
            await _unitOfWork.SaveChangesAsync();

            await _cache.RemoveAsync(CACHE_KEY);
            _logger.LogWarning("Category ID {Id} ('{Name}') deleted successfully. Cache invalidated.", categoryId, category.Name);
        }
    }
}