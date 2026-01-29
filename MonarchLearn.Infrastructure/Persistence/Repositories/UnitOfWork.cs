using Microsoft.EntityFrameworkCore.Storage;
using MonarchLearn.Application.Interfaces.Repositories;
using MonarchLearn.Application.Interfaces.UnitOfWork;
using MonarchLearn.Domain.Entities.Courses;
using MonarchLearn.Domain.Entities.LearningProgress;
using MonarchLearn.Domain.Entities.Quizzes;
using MonarchLearn.Domain.Entities.Statistics;
using MonarchLearn.Domain.Entities.Subscriptions;
using MonarchLearn.Domain.Entities.Users;
using MonarchLearn.Infrastructure.Persistence.Context;

namespace MonarchLearn.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MonarchLearnDbContext _context;

       

        // Xüsusi (Custom) Repository-lər
        public ICourseRepository Courses { get; private set; }
        public IEnrollmentRepository Enrollments { get; private set; }
        public ILessonProgressRepository LessonProgresses { get; private set; }
        public IAttemptRepository Attempts { get; private set; }
        public IQuizRepository Quizzes { get; private set; }
        public IUserSubscriptionRepository UserSubscriptions { get; private set; }
        public IReviewRepository Reviews { get; private set; }
        public IUserStreakRepository UserStreaks { get; private set; }

        // Generic Repository-lər
        public IGenericRepository<LessonItem> LessonItems { get; private set; }
        public IGenericRepository<Module> Modules { get; private set; }
        public IGenericRepository<AppUser> AppUsers { get; private set; }
        public IGenericRepository<Certificate> Certificates { get; private set; }
        public IGenericRepository<UserEducation> UserEducations { get; private set; }
        public IGenericRepository<UserWorkExperience> UserWorkExperiences { get; private set; }
        public IGenericRepository<SubscriptionPlan> SubscriptionPlans { get; private set; }

        public IGenericRepository<CourseStatistics> CourseStatistics { get; private set; }
        public IGenericRepository<CourseLanguage> CourseLanguages { get; private set; }
        public IGenericRepository<CourseCategory> CourseCategories { get; private set; }

        public IGenericRepository<CourseLevel> CourseLevels { get; private set; }

        public IGenericRepository<Skill> Skills { get; private set; }
        public IGenericRepository<Question> Questions { get; private set; }
        public IGenericRepository<Option> Options { get; private set; }

        public UnitOfWork(MonarchLearnDbContext context)
        {
            _context = context;

            // Xüsusi Repolar
            Courses = new CourseRepository(_context);
            Enrollments = new EnrollmentRepository(_context);
            LessonProgresses = new LessonProgressRepository(_context);
            Attempts = new AttemptRepository(_context);
            Quizzes = new QuizRepository(_context);
            UserSubscriptions = new UserSubscriptionRepository(_context);
            Reviews = new ReviewRepository(_context);
            UserStreaks = new UserStreakRepository(_context);
            
            

            // Generic Repolar
            LessonItems = new GenericRepository<LessonItem>(_context);
            Modules = new GenericRepository<Module>(_context);
            AppUsers = new GenericRepository<AppUser>(_context);
            Certificates = new GenericRepository<Certificate>(_context);
            UserEducations = new GenericRepository<UserEducation>(_context);
            UserWorkExperiences = new GenericRepository<UserWorkExperience>(_context);
            SubscriptionPlans = new GenericRepository<SubscriptionPlan>(_context);
            CourseStatistics = new GenericRepository<CourseStatistics>(_context);
            CourseLanguages = new GenericRepository<CourseLanguage>(_context);
            CourseCategories = new GenericRepository<CourseCategory>(_context);
            CourseLevels = new GenericRepository<CourseLevel>(_context);
            Skills = new GenericRepository<Skill>(_context);
            Questions = new GenericRepository<Question>(_context);
            Options = new GenericRepository<Option>(_context);

        }


        public async Task<IDbContextTransaction?> BeginTransactionAsync()
        {
            if (_context.Database.CurrentTransaction != null)
            {
                return null; 
            }
            return await _context.Database.BeginTransactionAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}