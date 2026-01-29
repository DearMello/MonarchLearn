using MonarchLearn.Application.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Services
{
    public interface IAdminService
    {
        Task<AdminDashboardDto> GetDashboardStatsAsync();
    }
}
