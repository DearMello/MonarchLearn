using Microsoft.EntityFrameworkCore.Storage;
using MonarchLearn.Application.Interfaces.Repositories;
using MonarchLearn.Domain.Entities.Courses;
using MonarchLearn.Domain.Entities.LearningProgress;
using MonarchLearn.Domain.Entities.Quizzes;
using MonarchLearn.Domain.Entities.Statistics;
using MonarchLearn.Domain.Entities.Subscriptions;
using MonarchLearn.Domain.Entities.Users;

namespace MonarchLearn.Application.Interfaces.UnitOfWork
{
    public interface IUnitOfWork
    {
        ICourseRepository Courses { get; }
        IEnrollmentRepository Enrollments { get; }
        ILessonProgressRepository LessonProgresses { get; }
        IAttemptRepository Attempts { get; }
        IQuizRepository Quizzes { get; }
        IUserSubscriptionRepository UserSubscriptions { get; }
        IUserStreakRepository UserStreaks { get; }
        IGenericRepository<LessonItem> LessonItems { get; }
        IGenericRepository<Module> Modules { get; }
        IGenericRepository<AppUser> AppUsers { get; }
        IGenericRepository<Certificate> Certificates { get; }
        IGenericRepository<UserEducation> UserEducations { get; }
        IGenericRepository<UserWorkExperience> UserWorkExperiences { get; }
        IGenericRepository<SubscriptionPlan> SubscriptionPlans { get; }

        IGenericRepository<CourseStatistics> CourseStatistics { get; }
        IGenericRepository<CourseLanguage> CourseLanguages { get; }
        IGenericRepository<CourseCategory> CourseCategories { get; }
        IGenericRepository<CourseLevel> CourseLevels { get; }
        IGenericRepository<Skill> Skills { get; }
        IGenericRepository<Question> Questions { get; }
        IGenericRepository<Option> Options { get; }

        IReviewRepository Reviews { get; }
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> SaveChangesAsync();
    }
}
