using MonarchLearn.Application.DTOs.Quizzes;

namespace MonarchLearn.Application.Interfaces.Services
{
    public interface IQuizService
    {
        // Quizə başlamaq istəyəndə (Ban yoxlanışı burada olur)
        Task<bool> CanStartQuizAsync(int userId, int quizId, int enrollmentId);

        // Quizi bitirib göndərəndə (Hesablama burada olur)
        Task<QuizResultDto> SubmitQuizAsync(int userId, QuizSubmissionDto submission);
        Task<List<QuizAttemptDto>> GetQuizAttemptsAsync(int userId, int quizId, int enrollmentId);
        Task<QuizDto> GetQuizForStudentAsync(int userId, int quizId);


    }
}