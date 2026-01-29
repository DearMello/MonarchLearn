using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MonarchLearn.Domain.Entities.Common;
using MonarchLearn.Domain.Entities.Courses;
using MonarchLearn.Domain.Entities.Enrollments;
using MonarchLearn.Domain.Entities.LearningProgress;
using MonarchLearn.Domain.Entities.Quizzes;
using MonarchLearn.Domain.Entities.Statistics;
using MonarchLearn.Domain.Entities.Subscriptions;
using MonarchLearn.Domain.Entities.Users;
using System.Reflection;
using Module = MonarchLearn.Domain.Entities.Courses.Module;

namespace MonarchLearn.Infrastructure.Persistence.Context
{
    public class MonarchLearnDbContext : IdentityDbContext<AppUser, AppRole, int>
    {
        public MonarchLearnDbContext(DbContextOptions<MonarchLearnDbContext> options) : base(options)
        {
        }

        public DbSet<UserEducation> UserEducations { get; set; }
        public DbSet<UserWorkExperience> UserWorkExperiences { get; set; }
        public DbSet<UserStreak> UserStreaks { get; set; }

        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseCategory> CourseCategories { get; set; }
        public DbSet<CourseLevel> CourseLevels { get; set; }
        public DbSet<CourseLanguage> CourseLanguages { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<CourseReview> CourseReviews { get; set; }
        public DbSet<CourseStatistics> CourseStatistics { get; set; }

        public DbSet<Module> Modules { get; set; }
        public DbSet<LessonItem> LessonItems { get; set; }

        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<LessonProgress> LessonProgresses { get; set; }
        public DbSet<Certificate> Certificates { get; set; }

        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<Attempt> Attempts { get; set; }
        public DbSet<Answer> Answers { get; set; }

        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                
                if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(MonarchLearnDbContext)
                        .GetMethod(nameof(ApplySoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)?
                        .MakeGenericMethod(entityType.ClrType);

                    method?.Invoke(null, new object[] { modelBuilder });
                }
            }
        }


        private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder builder)
            where TEntity : class, ISoftDeletable
        {
            builder.Entity<TEntity>().HasQueryFilter(x => !x.IsDeleted);
        }
    }
}