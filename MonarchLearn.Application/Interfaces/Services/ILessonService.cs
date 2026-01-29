using MonarchLearn.Application.DTOs.Lessons;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Services
{
    public interface ILessonService
    {
        Task<CreateLessonResponseDto> CreateLessonAsync(int userId, CreateLessonDto model, bool isAdmin = false);
        Task UpdateLessonAsync(int userId, UpdateLessonDto model, bool isAdmin = false);
        Task<bool> DeleteLessonAsync(int userId, int lessonId, bool isAdmin = false);
        Task<List<LessonDetailDto>> GetLessonsByModuleIdAsync(int moduleId);
    }
}