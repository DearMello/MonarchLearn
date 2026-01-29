using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MonarchLearn.Application.DTOs.Auth;
using MonarchLearn.Application.DTOs.Common;
using MonarchLearn.Application.DTOs.CourseAdmin;
using MonarchLearn.Application.DTOs.Courses;
using MonarchLearn.Application.DTOs.Enrollment;
using MonarchLearn.Application.DTOs.Lessons;
using MonarchLearn.Application.DTOs.Modules;
using MonarchLearn.Application.DTOs.Quizzes;
using MonarchLearn.Application.DTOs.Reviews;
using MonarchLearn.Application.DTOs.Subscriptions;
using MonarchLearn.Application.DTOs.Users;
using MonarchLearn.Application.Interfaces.Services;
using MonarchLearn.Application.Services;
using MonarchLearn.Application.Validators.Auth;
using MonarchLearn.Application.Validators.AuthVal;
using MonarchLearn.Application.Validators.CourseAdminVal;
using MonarchLearn.Application.Validators.CourseVal;
using MonarchLearn.Application.Validators.EnrollmentVal;
using MonarchLearn.Application.Validators.LessonsVal;
using MonarchLearn.Application.Validators.LookUpVal;
using MonarchLearn.Application.Validators.ModulesVal;
using MonarchLearn.Application.Validators.QuizzesVal;
using MonarchLearn.Application.Validators.ReviewsVal;
using MonarchLearn.Application.Validators.Subscriptions;
using MonarchLearn.Application.Validators.SubscriptionsVal;
using MonarchLearn.Application.Validators.UsersVal;


namespace MonarchLearn.Application.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(
                typeof(MonarchLearn.Application.Mapping.EnrollmentProfile).Assembly);

            services.AddScoped<IEnrollmentService, EnrollmentService>();
            services.AddScoped<ILessonCompletionService, LessonCompletionService>();
            services.AddScoped<IQuizService, QuizService>();
            services.AddScoped<ICertificateService, CertificateService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IReviewService,ReviewService>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<ILessonService, LessonService>();
            services.AddScoped<IStatisticsService, StatisticsService>();
            services.AddScoped<IStreakService, StreakService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ILanguageService, LanguageService>();
            services.AddScoped<ILevelService, LevelService>();
            services.AddScoped<ISkillService, SkillService>();
            services.AddScoped<ILeaderboardService, LeaderboardService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<ICourseManagementService, CourseManagementService>();
            services.AddScoped<IQuizManagementService, QuizManagementService>();



            services.AddScoped<IValidator<RegisterDto>, RegisterDtoValidator>();
            services.AddScoped<IValidator<LoginDto>, LoginDtoValidator>();
            services.AddScoped<IValidator<RefreshTokenDto>, RefreshTokenDtoValidator>();

           
            services.AddScoped<IValidator<CreateCourseDto>, CreateCourseDtoValidator>();
            services.AddScoped<IValidator<UpdateCourseDto>, UpdateCourseDtoValidator>();

           
            services.AddScoped<IValidator<CreateLessonDto>, CreateLessonDtoValidator>();
            services.AddScoped<IValidator<UpdateLessonDto>, UpdateLessonDtoValidator>();

            services.AddScoped<IValidator<CreateModuleDto>, CreateModuleDtoValidator>();
            services.AddScoped<IValidator<UpdateModuleDto>, UpdateModuleDtoValidator>();

          
            services.AddScoped<IValidator<QuizSubmissionDto>, QuizSubmissionDtoValidator>();

          
            services.AddScoped<IValidator<CreateReviewDto>, CreateReviewDtoValidator>();

        
            services.AddScoped<IValidator<EnrollRequestDto>, EnrollRequestDtoValidator>();

          
            services.AddScoped<IValidator<UpdateProfileDto>, UpdateProfileDtoValidator>();
            services.AddScoped<IValidator<UserEducationDto>, UserEducationDtoValidator>();
            services.AddScoped<IValidator<UserWorkExperienceDto>, UserWorkExperienceDtoValidator>();

          
            services.AddScoped<IValidator<CreateLookupDto>, CreateLookupDtoValidator>();

            services.AddScoped<IValidator<VerifyEmailDto>, VerifyEmailDtoValidator>();
            services.AddScoped<IValidator<ForgotPasswordDto>, ForgotPasswordDtoValidator>();
            services.AddScoped<IValidator<ResetPasswordDto>, ResetPasswordDtoValidator>();
            services.AddScoped<IValidator<CreatePlanDto>, CreatePlanDtoValidator>();
            services.AddScoped<IValidator<QuizSubmissionDto>, QuizSubmissionDtoValidator>();
            services.AddScoped<IValidator<CreateQuestionDto>, CreateQuestionDtoValidator>();
            services.AddScoped<IValidator<UpdateQuestionDto>, UpdateQuestionDtoValidator>();
            services.AddScoped<IValidator<CreateOptionDto>, CreateOptionDtoValidator>();
            services.AddScoped<IValidator<UpdateOptionDto>, UpdateOptionDtoValidator>();

            services.AddScoped<IValidator<CreateLessonDto>, CreateLessonDtoValidator>();
            services.AddScoped<IValidator<UpdateLessonDto>, UpdateLessonDtoValidator>();
            services.AddScoped<IValidator<CompleteLessonDto>, CompleteLessonDtoValidator>();
            services.AddScoped<IValidator<CourseFilterDto>, CourseFilterDtoValidator>();
            services.AddScoped<IValidator<UpdatePlanDto>, UpdatePlanDtoValidator>();







            return services;
        }
    }
}
