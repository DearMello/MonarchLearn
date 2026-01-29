using MonarchLearn.Application.DTOs.Lessons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Services
{
    public interface ILessonCompletionService
    {
        Task<bool> CanAccessLessonAsync(int userId, int courseId, int lessonItemId);
        Task CompleteLessonAsync(int userId, int courseId, int lessonItemId, int watchedSeconds = 0, bool markAsFinished = false);
        Task<double> GetProgressPercentAsync(int enrollmentId, int? courseId = null);

        // --- BU YENİDİR ---
        Task<LessonProgressDto?> GetLessonProgressAsync(int enrollmentId, int lessonItemId);
        Task<LessonProgressDto> GetLessonProgressForStudentAsync(int userId, int courseId, int lessonItemId);
    }
}
