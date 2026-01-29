using MonarchLearn.Application.DTOs.Quizzes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Services
{
    public interface IQuizManagementService
    {
        Task UpdateQuizSettingsAsync(int userId, int quizId, UpdateQuizSettingsDto model, bool isAdmin = false);
        Task<int> CreateQuestionAsync(int userId, int quizId, CreateQuestionDto model, bool isAdmin = false);
        Task UpdateQuestionAsync(int userId, int questionId, UpdateQuestionDto model, bool isAdmin = false);
        Task DeleteQuestionAsync(int userId, int questionId, bool isAdmin = false);
        Task<int> CreateOptionAsync(int userId, int questionId, CreateOptionDto model, bool isAdmin = false);
        Task UpdateOptionAsync(int userId, int optionId, UpdateOptionDto model, bool isAdmin = false);
        Task DeleteOptionAsync(int userId, int optionId, bool isAdmin = false);
        Task<List<QuestionDto>> GetQuestionsByQuizIdAsync(int userId, int quizId, bool isAdmin = false);
    }
}