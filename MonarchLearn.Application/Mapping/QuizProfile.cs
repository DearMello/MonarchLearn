using AutoMapper;
using MonarchLearn.Application.DTOs.Quizzes;
using MonarchLearn.Domain.Entities.LearningProgress;
using MonarchLearn.Domain.Entities.Quizzes;
using System;
using System.Linq;

namespace MonarchLearn.Application.Mapping
{
    public class QuizProfile : Profile
    {
        public QuizProfile()
        {
            CreateMap<CreateQuizDto, Quiz>();
            CreateMap<QuizSubmissionDto, Attempt>();

            CreateMap<Quiz, QuizDto>()
                .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src =>
                    (int)Math.Ceiling(src.TimeLimitSeconds / 60.0)))
                .ForMember(dest => dest.TotalQuestions, opt => opt.MapFrom(src =>
                    src.Questions.Count(q => !q.IsDeleted)));

            CreateMap<Question, QuestionDto>()
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src =>
                    src.Options.OrderBy(o => o.Order)));

            CreateMap<Option, OptionDto>();
            CreateMap<Quiz, QuizDetailDto>();

            CreateMap<Question, QuestionDetailDto>()
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src =>
                    src.Options.OrderBy(o => o.Order)));

            CreateMap<Option, OptionDetailDto>();
            CreateMap<CreateQuestionDto, Question>();
            CreateMap<CreateOptionDto, Option>();

            
            CreateMap<UpdateQuestionDto, Question>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.QuizId, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UpdateOptionDto, Option>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.QuestionId, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}