using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MonarchLearn.Application.DTOs.Quizzes;
using MonarchLearn.Application.Interfaces.Services;
using MonarchLearn.Application.Interfaces.UnitOfWork;
using MonarchLearn.Domain.Entities.Courses;
using MonarchLearn.Domain.Entities.LearningProgress;
using MonarchLearn.Domain.Entities.Users;
using MonarchLearn.Domain.Entities.Quizzes;
using MonarchLearn.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Services
{
    public class QuizService : IQuizService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<QuizService> _logger;
        private readonly ILessonCompletionService _lessonCompletionService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public QuizService(
            IUnitOfWork unitOfWork,
            ILogger<QuizService> logger,
            ILessonCompletionService lessonCompletionService,
            IConfiguration configuration,
            IMapper mapper,
            UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _lessonCompletionService = lessonCompletionService;
            _configuration = configuration;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<QuizDto> GetQuizForStudentAsync(int userId, int quizId)
        {
            var user = await _unitOfWork.AppUsers.GetByIdAsync(userId);
            if (user == null) throw new NotFoundException("User", userId);

            var quiz = await _unitOfWork.Quizzes.GetQuizWithQuestionsAsync(quizId);
            if (quiz == null || quiz.IsDeleted)
                throw new NotFoundException("Quiz", quizId);

            var userRoles = await _userManager.GetRolesAsync(user);
            bool isPrivileged = userRoles.Contains("Admin") || userRoles.Contains("Instructor");

            if (!isPrivileged)
            {
                if (quiz.LessonItem == null)
                    quiz.LessonItem = await _unitOfWork.LessonItems.GetByIdAsync(quiz.LessonItemId);

                var module = await _unitOfWork.Modules.GetByIdAsync(quiz.LessonItem.ModuleId);
                if (module == null)
                    throw new NotFoundException("Module", quiz.LessonItem.ModuleId);

                var enrollment = await _unitOfWork.Enrollments.GetUserEnrollmentAsync(userId, module.CourseId);
                if (enrollment == null)
                    throw new ForbiddenException("You must enroll in this course first");
            }

            return _mapper.Map<QuizDto>(quiz);
        }

        public async Task<bool> CanStartQuizAsync(int userId, int quizId, int enrollmentId)
        {
            _logger.LogInformation("Checking quiz access for User {UserId}, Quiz {QuizId}, Enrollment {EnrollmentId}", userId, quizId, enrollmentId);

            var user = await _unitOfWork.AppUsers.GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("User", userId);

            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains("Admin") || userRoles.Contains("Instructor"))
                return true;

            var quiz = await _unitOfWork.Quizzes.GetByIdAsync(quizId);
            if (quiz == null || quiz.IsDeleted)
                throw new NotFoundException("Quiz", quizId);

            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(enrollmentId);
            if (enrollment == null || enrollment.IsDeleted || enrollment.UserId != userId)
                throw new ForbiddenException("Invalid enrollment");

            var attempts = await _unitOfWork.Attempts.FindAsync(a => a.EnrollmentId == enrollmentId && a.QuizId == quizId);

            var lastFailedAttempt = attempts
                .Where(a => !a.IsPassed)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefault();

            if (lastFailedAttempt != null)
            {
                int cooldownHours = _configuration.GetValue<int>("QuizSettings:CooldownHours", 4);
                var banUntil = lastFailedAttempt.CreatedAt.AddHours(cooldownHours);

                if (DateTime.UtcNow < banUntil)
                {
                    _logger.LogWarning("Quiz access denied for User {UserId}: Active cooldown until {BanUntil}", userId, banUntil);
                    return false;
                }
            }

            _logger.LogInformation("Quiz access granted for User {UserId}", userId);
            return true;
        }

        public async Task<QuizResultDto> SubmitQuizAsync(int userId, QuizSubmissionDto submission)
        {
            _logger.LogInformation("Processing quiz submission: User {UserId}, Quiz {QuizId}", userId, submission.QuizId);

            var user = await _unitOfWork.AppUsers.GetByIdAsync(userId);
            if (user == null) throw new NotFoundException("User", userId);

            var userRoles = await _userManager.GetRolesAsync(user);
            bool isPrivileged = userRoles.Contains("Admin") || userRoles.Contains("Instructor");

            if (!isPrivileged && !await CanStartQuizAsync(userId, submission.QuizId, submission.EnrollmentId))
                throw new BadRequestException("Cooldown period not met.");

            var quiz = await _unitOfWork.Quizzes.GetQuizWithQuestionsAsync(submission.QuizId);
            if (quiz == null || quiz.IsDeleted)
                throw new NotFoundException("Quiz", submission.QuizId);

            if (quiz.TimeLimitSeconds > 0)
            {
                
                if (submission.TimeSpentSeconds > quiz.TimeLimitSeconds + 15)
                {
                    _logger.LogWarning("User {UserId} exceeded time limit for Quiz {QuizId}. Spent: {Spent}s, Limit: {Limit}s",
                        userId, quiz.Id, submission.TimeSpentSeconds, quiz.TimeLimitSeconds);

                    throw new BadRequestException("Unfortunately, you have exceeded the time limit allocated for this quiz. Your answers were not accepted.");
                }
            }

            if (quiz.LessonItem == null)
            {
                quiz.LessonItem = await _unitOfWork.LessonItems.GetByIdAsync(quiz.LessonItemId);
                if (quiz.LessonItem == null)
                    throw new NotFoundException("LessonItem", quiz.LessonItemId);
            }

            var activeQuestions = quiz.Questions.Where(q => !q.IsDeleted).ToList();
            var allActiveQuestionIds = activeQuestions.Select(q => q.Id).ToHashSet();
            var answeredQuestionIds = submission.Answers.Select(a => a.QuestionId).ToHashSet();

            if (answeredQuestionIds.Count != allActiveQuestionIds.Count)
            {
                _logger.LogWarning("Quiz submission rejected: User {UserId} answered {Answered}/{Total} active questions", userId, answeredQuestionIds.Count, allActiveQuestionIds.Count);
                throw new BadRequestException($"All {allActiveQuestionIds.Count} questions must be answered. You answered {answeredQuestionIds.Count}.");
            }

            var invalidQuestionIds = answeredQuestionIds.Except(allActiveQuestionIds).ToList();
            if (invalidQuestionIds.Any())
            {
                _logger.LogWarning("Quiz submission rejected: Invalid question IDs: {InvalidIds}", string.Join(", ", invalidQuestionIds));
                throw new BadRequestException("Submission contains invalid question IDs");
            }

            var duplicates = submission.Answers
                .GroupBy(a => a.QuestionId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Any())
            {
                throw new BadRequestException("Cannot submit multiple answers for the same question");
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                int correctCount = 0;
                var attemptAnswers = new List<Answer>();

                foreach (var userAnswer in submission.Answers)
                {
                    var question = activeQuestions.FirstOrDefault(q => q.Id == userAnswer.QuestionId);
                    if (question != null)
                    {
                        var isCorrect = question.Options.Any(o =>
                            o.Id == userAnswer.SelectedOptionId &&
                            o.IsCorrect &&
                            !o.IsDeleted);

                        if (isCorrect) correctCount++;
                    }

                    attemptAnswers.Add(new Answer
                    {
                        QuestionId = userAnswer.QuestionId,
                        SelectedOptionId = userAnswer.SelectedOptionId
                    });
                }

                double percentage = allActiveQuestionIds.Count > 0
                    ? Math.Round(((double)correctCount / allActiveQuestionIds.Count) * 100, 2)
                    : 0;

                int passingScore = quiz.PassingScorePercent > 0 ? quiz.PassingScorePercent : 50;

                bool isPassed = percentage >= passingScore;

                TimeSpan duration = TimeSpan.FromSeconds(submission.TimeSpentSeconds > 0 ? submission.TimeSpentSeconds : 0);

                var attempt = new Attempt
                {
                    EnrollmentId = submission.EnrollmentId,
                    QuizId = submission.QuizId,
                    Score = correctCount,
                    Percentage = percentage,
                    IsPassed = isPassed,
                    TimeSpent = duration,
                    CreatedAt = DateTime.UtcNow,
                    Answers = attemptAnswers
                };

                await _unitOfWork.Attempts.AddAsync(attempt);
                await _unitOfWork.SaveChangesAsync();

                DateTime? nextAttemptAt = null;

                if (isPassed)
                {
                    _logger.LogInformation("User {UserId} passed Quiz {QuizId} with {Percentage}%", userId, submission.QuizId, percentage);

                    if (!isPrivileged)
                    {
                        var module = await _unitOfWork.Modules.GetByIdAsync(quiz.LessonItem.ModuleId);
                        if (module != null)
                        {
                            await _lessonCompletionService.CompleteLessonAsync(userId, module.CourseId, quiz.LessonItemId);
                            _logger.LogInformation("Lesson {LessonId} auto-completed after quiz pass", quiz.LessonItemId);
                        }
                    }
                }
                else
                {
                    int cooldownHours = _configuration.GetValue<int>("QuizSettings:CooldownHours", 2);
                    nextAttemptAt = DateTime.UtcNow.AddHours(cooldownHours);
                    _logger.LogWarning("User {UserId} failed Quiz {QuizId} with {Percentage}%. Next attempt at {NextAttempt}", userId, submission.QuizId, percentage, nextAttemptAt);
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Quiz submission processed successfully: Score {Percentage}%", percentage);

                return new QuizResultDto
                {
                    AttemptId = attempt.Id,
                    Score = percentage,
                    IsPassed = isPassed,
                    Message = isPassed ? "Congratulations! You passed the quiz." : $"You scored {percentage}%. Keep learning and try again.",
                    NextAttemptAt = nextAttemptAt
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Critical failure during quiz submission for User {UserId}", userId);
                throw;
            }
        }

        public async Task<List<QuizAttemptDto>> GetQuizAttemptsAsync(int userId, int quizId, int enrollmentId)
        {
            _logger.LogInformation("Fetching quiz attempts: User {UserId}, Quiz {QuizId}, Enrollment {EnrollmentId}", userId, quizId, enrollmentId);

            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(enrollmentId);
            if (enrollment == null || enrollment.UserId != userId)
                throw new ForbiddenException("Invalid enrollment");

            var attempts = await _unitOfWork.Attempts.FindAsync(a => a.QuizId == quizId && a.EnrollmentId == enrollmentId);

            var result = attempts
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new QuizAttemptDto
                {
                    Id = a.Id,
                    Score = a.Score,
                    Percentage = a.Percentage,
                    IsPassed = a.IsPassed,
                    TimeSpent = a.TimeSpent,
                    CreatedAt = a.CreatedAt
                })
                .ToList();

            _logger.LogInformation("Retrieved {Count} attempt(s) for Quiz {QuizId}", result.Count, quizId);

            return result;
        }
    }
}