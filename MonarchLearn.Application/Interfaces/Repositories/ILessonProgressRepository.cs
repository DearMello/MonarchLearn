using MonarchLearn.Domain.Entities.LearningProgress;

namespace MonarchLearn.Application.Interfaces.Repositories
{
    public interface ILessonProgressRepository
        : IGenericRepository<LessonProgress>
    {
        Task<LessonProgress?> GetProgressAsync(int enrollmentId, int lessonItemId);
    }
}
