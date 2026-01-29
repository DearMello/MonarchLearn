using MonarchLearn.Application.DTOs.Common; // Artıq Category DTO yox, Common Lookup DTO
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        // CategoryDto əvəzinə LookupDto istifadə olunur
        Task<List<LookupDto>> GetAllCategoriesAsync();
        Task<LookupDto> GetCategoryByIdAsync(int categoryId);

        // CreateCategoryDto əvəzinə CreateLookupDto istifadə olunur
        Task<LookupDto> CreateCategoryAsync(CreateLookupDto dto);
        Task UpdateCategoryAsync(int categoryId, CreateLookupDto dto);

        Task DeleteCategoryAsync(int categoryId);
    }
}