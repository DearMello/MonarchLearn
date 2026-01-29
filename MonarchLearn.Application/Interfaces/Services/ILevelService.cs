using MonarchLearn.Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Services
{
    public interface ILevelService
    {
        Task<List<LookupDto>> GetAllLevelsAsync();
        Task<LookupDto> GetLevelByIdAsync(int levelId);
        Task<LookupDto> CreateLevelAsync(CreateLookupDto dto);
        Task UpdateLevelAsync(int levelId, CreateLookupDto dto);
        Task DeleteLevelAsync(int levelId);
    }
}
