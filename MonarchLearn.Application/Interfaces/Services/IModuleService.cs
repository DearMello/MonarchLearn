using MonarchLearn.Application.DTOs.Modules;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Services
{
    public interface IModuleService
    {
        Task<int> CreateModuleAsync(int userId, CreateModuleDto model, bool isAdmin = false);
        Task UpdateModuleAsync(int userId, UpdateModuleDto model, bool isAdmin = false);
        Task DeleteModuleAsync(int userId, int moduleId, bool isAdmin = false);
        Task<List<ModuleWithLessonsDto>> GetModulesByCourseIdAsync(int courseId);
    }
}