using MonarchLearn.Domain.Entities.Quizzes;

namespace MonarchLearn.Application.Interfaces.Repositories
{
    public interface IQuizRepository : IGenericRepository<Quiz>
    {
        Task<Quiz?> GetQuizWithQuestionsAsync(int quizId);
    }
}
