using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Services
{
    public interface IScheduledTaskService
    {
        // Gecə 00:00-da işləyəcək əsas metod (Streak sıfırlama)
        Task ResetInactiveStreaksAsync();

        // Gələcəkdə abunəlik bildirişləri üçün
        Task SendSubscriptionExpiryNotificationsAsync();
    }
}
