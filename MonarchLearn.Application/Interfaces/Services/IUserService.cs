using MonarchLearn.Application.DTOs.Users;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Services
{
    public interface IUserService
    {

      
        Task<UserProfileDto> GetUserProfileAsync(int userId);
        Task UpdateProfileAsync(int userId, UpdateProfileDto model);
        Task<int> AddEducationAsync(int userId, UserEducationDto model);
        Task UpdateEducationAsync(int userId, int educationId, UserEducationDto model);
        Task DeleteEducationAsync(int userId, int educationId);   
        Task<int> AddWorkExperienceAsync(int userId, UserWorkExperienceDto model);
        Task UpdateWorkExperienceAsync(int userId, int workId, UserWorkExperienceDto model);
        Task DeleteWorkExperienceAsync(int userId, int workId);
    }
}