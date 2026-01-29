using AutoMapper;
using Microsoft.Extensions.Logging;
using MonarchLearn.Application.DTOs.Gamification;
using MonarchLearn.Application.Interfaces.Services;
using MonarchLearn.Application.Interfaces.UnitOfWork;
using MonarchLearn.Domain.Entities.Users;
using MonarchLearn.Domain.Exceptions;

namespace MonarchLearn.Application.Services
{
    public class StreakService : IStreakService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<StreakService> _logger;

        public StreakService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<StreakService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

     
        public async Task<UserStreakDto> GetUserStreakAsync(int userId)
        {
            _logger.LogDebug("Fetching streak data for User {UserId}", userId);

            
            var user = await _unitOfWork.AppUsers.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Streak fetch failed: User {UserId} not found", userId);
                throw new NotFoundException("User", userId);
            }

            
            if (user.IsDeleted)
            {
                _logger.LogWarning("Streak fetch failed: User {UserId} is deleted", userId);
                throw new ForbiddenException("Your account has been deactivated");
            }

            
            var streaks = await _unitOfWork.UserStreaks.FindAsync(s => s.UserId == userId);
            var userStreak = streaks.FirstOrDefault();

            
            if (userStreak == null)
            {
                _logger.LogInformation("No streak record found for User {UserId}, returning initial state", userId);
                return new UserStreakDto
                {
                    UserId = userId,
                    CurrentStreakDays = 0,
                    LastActiveDate = null,
                    Message = "Start learning today to begin your streak! "
                };
            }

            var dto = _mapper.Map<UserStreakDto>(userStreak);

            
            dto.Message = dto.CurrentStreakDays switch
            {
                >= 30 => "Unstoppable! You're a legend! Keep going! ",
                >= 14 => " Two weeks strong! You're building an amazing habit! ",
                >= 7 => " One week streak! Excellent consistency! ",
                >= 3 => " Great start! Keep up the momentum! ",
                > 0 => " Nice beginning! Stay consistent! ",
                _ => " Complete a lesson today to start your streak!"
            };

            _logger.LogInformation(
                "Streak data retrieved for User {UserId}: {Days} day(s)",
                userId, dto.CurrentStreakDays);

            return dto;
        }

       
        
      
        public async Task UpdateStreakAsync(int userId)
        {
            _logger.LogInformation("Checking and updating streak for User {UserId}", userId);

            
            var user = await _unitOfWork.AppUsers.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Streak update failed: User {UserId} not found", userId);
                throw new NotFoundException("User", userId);
            }

            
            if (user.IsDeleted)
            {
                _logger.LogWarning("Streak update failed: User {UserId} is deleted", userId);
                throw new ForbiddenException("Your account has been deactivated");
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var streaks = await _unitOfWork.UserStreaks.FindAsync(s => s.UserId == userId);
                var userStreak = streaks.FirstOrDefault();
                var today = DateTime.UtcNow.Date;

                //   First time activity - Create new streak
                if (userStreak == null)
                {
                    userStreak = new UserStreak
                    {
                        UserId = userId,
                        CurrentStreakDays = 1,
                        LastActiveDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.UserStreaks.AddAsync(userStreak);

                    _logger.LogInformation(
                        " New streak started for User {UserId}: 1 day",
                        userId);
                }
                else
                {
                    var lastActiveDate = userStreak.LastActiveDate.Date;

                    //  Already active today - No change needed
                    if (lastActiveDate == today)
                    {
                        _logger.LogDebug(
                            "User {UserId} already active today. Streak unchanged: {Days} day(s)",
                            userId, userStreak.CurrentStreakDays);
                        return; 
                    }

                    //   Active yesterday - Increment streak
                    else if (lastActiveDate == today.AddDays(-1))
                    {
                        userStreak.CurrentStreakDays++;

                        _logger.LogInformation(
                            " Streak incremented for User {UserId}: {Days} day(s)",
                            userId, userStreak.CurrentStreakDays);
                    }

                    //   Gap > 1 day - Reset streak
                    else
                    {
                        int daysMissed = (today - lastActiveDate).Days;
                        int previousStreak = userStreak.CurrentStreakDays;

                        userStreak.CurrentStreakDays = 1;

                        _logger.LogWarning(
                            " Streak reset for User {UserId}: Missed {MissedDays} day(s). Previous streak: {PreviousStreak} days, New streak: 1 day",
                            userId, daysMissed, previousStreak);
                    }

                    
                    userStreak.LastActiveDate = DateTime.UtcNow;
                    userStreak.UpdatedAt = DateTime.UtcNow;

                    _unitOfWork.UserStreaks.Update(userStreak);
                }

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Streak update completed successfully for User {UserId}",
                    userId);
            }
            catch (Exception ex) when (ex is not DomainException)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to update streak for User {UserId}", userId);
                throw new BadRequestException("Failed to update streak. Please try again.");
            }
        }
    }
}