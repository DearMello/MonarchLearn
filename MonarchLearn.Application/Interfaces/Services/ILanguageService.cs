using MonarchLearn.Application.DTOs.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Services
{
    public interface ILanguageService
    {
        // CRUD: READ
        Task<List<LookupDto>> GetAllLanguagesAsync();
        Task<LookupDto> GetLanguageByIdAsync(int languageId);

        // CRUD: CREATE
        Task<LookupDto> CreateLanguageAsync(CreateLookupDto dto);

        // CRUD: UPDATE
        Task UpdateLanguageAsync(int languageId, CreateLookupDto dto);

        // CRUD: DELETE
        Task DeleteLanguageAsync(int languageId);
    }
}