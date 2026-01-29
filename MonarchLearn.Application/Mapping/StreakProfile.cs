using AutoMapper;
using MonarchLearn.Application.DTOs.Gamification;
using MonarchLearn.Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Mapping
{
    public class StreakProfile : Profile
    {
        public StreakProfile()
        {
            CreateMap<UserStreak, UserStreakDto>();
        }
    }
}
