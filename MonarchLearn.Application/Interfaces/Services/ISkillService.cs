using MonarchLearn.Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Services
{
    public interface ISkillService
    {
        Task<List<LookupDto>> GetAllSkillsAsync();
        Task<LookupDto> GetSkillByIdAsync(int skillId);
        Task<LookupDto> CreateSkillAsync(CreateLookupDto dto);
        Task UpdateSkillAsync(int skillId, CreateLookupDto dto);
        Task DeleteSkillAsync(int skillId);
    }
}
