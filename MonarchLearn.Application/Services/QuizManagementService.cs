using AutoMapper;
using Microsoft.Extensions.Logging;
using MonarchLearn.Application.DTOs.Quizzes;
using MonarchLearn.Application.Interfaces.Services;
using MonarchLearn.Application.Interfaces.UnitOfWork;
using MonarchLearn.Domain.Entities.Quizzes;
using MonarchLearn.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MonarchLearn.Application.Services
{
    public class QuizManagementService : IQuizManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<QuizManagementService> _logger;

        public QuizManagementService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<QuizManagementService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        private async Task ValidateQuizOwnershipAsync(int userId, int quizId, bool isAdmin)
        {
            var quiz = await _unitOfWork.Quizzes.GetByIdAsync(quizId);
            if (quiz == null || quiz.IsDeleted) throw new NotFoundException("Quiz", quizId);

            var lesson = await _unitOfWork.LessonItems.GetByIdAsync(quiz.LessonItemId);
            var module = await _unitOfWork.Modules.GetByIdAsync(lesson.ModuleId);
            var course = await _unitOfWork.Courses.GetByIdAsync(module.CourseId);

            if (!isAdmin && (course == null || course.InstructorId != userId))
            {
                _logger.LogWarning("Unauthorized access attempt: User {UserId} tried to access Quiz {QuizId}", userId, quizId);
                throw new ForbiddenException("You can only manage quizzes belonging to your own courses.");
            }
        }

        public async Task UpdateQuizSettingsAsync(int userId, int quizId, UpdateQuizSettingsDto model, bool isAdmin = false)
        {
            _logger.LogInformation("User {UserId} updating settings for Quiz {QuizId}", userId, quizId);

            await ValidateQuizOwnershipAsync(userId, quizId, isAdmin);

            var quiz = await _unitOfWork.Quizzes.GetByIdAsync(quizId);
            var lesson = await _unitOfWork.LessonItems.GetByIdAsync(quiz.LessonItemId);
            var module = await _unitOfWork.Modules.GetByIdAsync(lesson.ModuleId);
            var course = await _unitOfWork.Courses.GetByIdAsync(module.CourseId);

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                quiz.PassingScorePercent = model.PassingScorePercent;
                quiz.TimeLimitSeconds = model.TimeLimitSeconds;
                quiz.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Quizzes.Update(quiz);

                if (model.TimeLimitSeconds > 0)
                {
                    lesson.EstimatedMinutes = (int)Math.Ceiling(model.TimeLimitSeconds / 60.0);
                }
                _unitOfWork.LessonItems.Update(lesson);

                await _unitOfWork.SaveChangesAsync();
                await RecalculateCourseDurationAsync(course.Id);

                await transaction.CommitAsync();
                _logger.LogInformation("Quiz {QuizId} settings and Lesson {LessonId} duration successfully updated", quizId, lesson.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to update quiz settings for Quiz {QuizId}", quizId);
                throw new BadRequestException("Failed to update quiz settings: " + ex.Message);
            }
        }

        public async Task<int> CreateQuestionAsync(int userId, int quizId, CreateQuestionDto model, bool isAdmin = false)
        {
            await ValidateQuizOwnershipAsync(userId, quizId, isAdmin);

            int nextOrder = await _unitOfWork.Questions
                .GetQueryable()
                .Where(q => q.QuizId == quizId && !q.IsDeleted)
                .Select(q => (int?)q.Order)
                .MaxAsync() ?? 0;

            nextOrder++;

            var question = _mapper.Map<Question>(model);
            question.QuizId = quizId;
            question.Order = model.Order > 0 ? model.Order : nextOrder;
            question.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Questions.AddAsync(question);
            await _unitOfWork.SaveChangesAsync();

            return question.Id;
        }

        public async Task UpdateQuestionAsync(int userId, int questionId, UpdateQuestionDto model, bool isAdmin = false)
        {
            var question = await _unitOfWork.Questions.GetByIdAsync(questionId);
            if (question == null || question.IsDeleted) throw new NotFoundException("Question", questionId);

            await ValidateQuizOwnershipAsync(userId, question.QuizId, isAdmin);

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                
                _mapper.Map(model, question);
                question.UpdatedAt = DateTime.UtcNow;

                
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new BadRequestException("Update failed: " + ex.Message);
            }
        }

        public async Task DeleteQuestionAsync(int userId, int questionId, bool isAdmin = false)
        {
            var question = await _unitOfWork.Questions.GetByIdAsync(questionId);
            if (question == null || question.IsDeleted) throw new NotFoundException("Question", questionId);

            await ValidateQuizOwnershipAsync(userId, question.QuizId, isAdmin);

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                question.IsDeleted = true;
                question.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Questions.Update(question);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> CreateOptionAsync(int userId, int questionId, CreateOptionDto model, bool isAdmin = false)
        {
            var question = await _unitOfWork.Questions.GetByIdAsync(questionId);
            if (question == null || question.IsDeleted) throw new NotFoundException("Question", questionId);

            await ValidateQuizOwnershipAsync(userId, question.QuizId, isAdmin);

            var existingOptions = await _unitOfWork.Options.FindAsync(o => o.QuestionId == questionId && !o.IsDeleted);
            int newOrder = existingOptions.Any() ? existingOptions.Max(o => o.Order) + 1 : 1;

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var option = _mapper.Map<Option>(model);
                option.QuestionId = questionId;
                option.Order = newOrder;
                option.CreatedAt = DateTime.UtcNow;
                option.IsDeleted = false;

                await _unitOfWork.Options.AddAsync(option);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
                return option.Id;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new BadRequestException("Option creation failed: " + ex.Message);
            }
        }

        public async Task UpdateOptionAsync(int userId, int optionId, UpdateOptionDto model, bool isAdmin = false)
        {
            var option = await _unitOfWork.Options.GetByIdAsync(optionId);
            if (option == null || option.IsDeleted) throw new NotFoundException("Option", optionId);

            var question = await _unitOfWork.Questions.GetByIdAsync(option.QuestionId);

            await ValidateQuizOwnershipAsync(userId, question.QuizId, isAdmin);

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                _mapper.Map(model, option);
                option.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<QuestionDto>> GetQuestionsByQuizIdAsync(int userId, int quizId, bool isAdmin = false)
        {
            await ValidateQuizOwnershipAsync(userId, quizId, isAdmin);

            var questions = await _unitOfWork.Questions
                .GetQueryable()
                .Where(q => q.QuizId == quizId && !q.IsDeleted)
                .Include(q => q.Options.Where(o => !o.IsDeleted))
                .OrderBy(q => q.Order)
                .ToListAsync();

            return _mapper.Map<List<QuestionDto>>(questions);
        }

        public async Task DeleteOptionAsync(int userId, int optionId, bool isAdmin = false)
        {
            var option = await _unitOfWork.Options.GetByIdAsync(optionId);
            if (option == null || option.IsDeleted) throw new NotFoundException("Option", optionId);

            var question = await _unitOfWork.Questions.GetByIdAsync(option.QuestionId);

            await ValidateQuizOwnershipAsync(userId, question.QuizId, isAdmin);

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                option.IsDeleted = true;
                option.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Options.Update(option);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task RecalculateCourseDurationAsync(int courseId)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null) return;

            var modules = await _unitOfWork.Modules.FindAsync(m => m.CourseId == courseId);
            var moduleIds = modules.Select(m => m.Id).ToList();
            var allLessons = await _unitOfWork.LessonItems.FindAsync(l => moduleIds.Contains(l.ModuleId) && !l.IsDeleted);

            int totalSeconds = 0;
            foreach (var lesson in allLessons)
            {
                if (lesson.VideoDurationSeconds.HasValue && lesson.VideoDurationSeconds.Value > 0)
                    totalSeconds += lesson.VideoDurationSeconds.Value;
                else if (lesson.EstimatedMinutes.HasValue)
                    totalSeconds += lesson.EstimatedMinutes.Value * 60;
            }
            course.TotalDurationSeconds = totalSeconds;
            _unitOfWork.Courses.Update(course);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Course {CourseId} duration updated to {Seconds}s after quiz settings change", courseId, totalSeconds);
        }
    }
}