using AutoMapper;
using Microsoft.Extensions.Logging;
using MonarchLearn.Application.DTOs.Reviews;
using MonarchLearn.Application.Interfaces.Services;
using MonarchLearn.Application.Interfaces.UnitOfWork;
using MonarchLearn.Domain.Entities.Courses;
using MonarchLearn.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ReviewService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task AddReviewAsync(int userId, CreateReviewDto model)
        {
            _logger.LogInformation(
                "Review request received: User {UserId}, Course {CourseId}, ParentReview {ParentId}",
                userId, model.CourseId, model.ParentReviewId);

            
            var user = await _unitOfWork.AppUsers.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Review creation failed: User {UserId} not found", userId);
                throw new NotFoundException("User", userId);
            }

            
            if (user.IsDeleted)
            {
                _logger.LogWarning("Review creation failed: User {UserId} is deleted", userId);
                throw new ForbiddenException("Your account has been deactivated");
            }

            
            var course = await _unitOfWork.Courses.GetByIdAsync(model.CourseId);
            if (course == null)
            {
                _logger.LogWarning("Review creation failed: Course {CourseId} not found", model.CourseId);
                throw new NotFoundException("Course", model.CourseId);
            }

           
            if (course.IsDeleted)
            {
                _logger.LogWarning("Review creation failed: Course {CourseId} is deleted", model.CourseId);
                throw new BadRequestException("Cannot review a deleted course");
            }

            
            if (model.Rating < 1 || model.Rating > 5)
            {
                _logger.LogWarning("Invalid rating value: {Rating}", model.Rating);
                throw new BadRequestException("Rating must be between 1 and 5");
            }

            
            var enrollments = await _unitOfWork.Enrollments
                .FindAsync(e => e.CourseId == model.CourseId && e.UserId == userId);

            if (!enrollments.Any())
            {
                _logger.LogWarning(
                    "Review denied: User {UserId} is not enrolled in Course {CourseId}",
                    userId, model.CourseId);
                throw new ForbiddenException(
                    "You must be enrolled in this course to leave a review");
            }

            
            var activeEnrollment = enrollments.FirstOrDefault(e => !e.IsDeleted);
            if (activeEnrollment == null)
            {
                _logger.LogWarning(
                    " Review denied: User {UserId} has no active enrollment in Course {CourseId}",
                    userId, model.CourseId);
                throw new ForbiddenException(
                    "Your enrollment has been deactivated. You cannot leave a review");
            }

            
            if (model.ParentReviewId.HasValue)
            {
                var parentReview = await _unitOfWork.Reviews.GetByIdAsync(model.ParentReviewId.Value);
                if (parentReview == null)
                {
                    _logger.LogWarning(
                        "Reply failed: Parent review {ParentReviewId} not found",
                        model.ParentReviewId.Value);
                    throw new NotFoundException("Parent review", model.ParentReviewId.Value);
                }

                
                if (parentReview.IsDeleted)
                {
                    _logger.LogWarning(
                        "Reply failed: Parent review {ParentReviewId} is deleted",
                        model.ParentReviewId.Value);
                    throw new BadRequestException("Cannot reply to a deleted review");
                }

                
                if (parentReview.CourseId != model.CourseId)
                {
                    _logger.LogWarning(
                        "Reply validation failed: Parent review {ParentReviewId} belongs to Course {ParentCourseId}, not {CourseId}",
                        model.ParentReviewId.Value, parentReview.CourseId, model.CourseId);
                    throw new BadRequestException("Parent review does not belong to this course");
                }

                _logger.LogInformation(
                    "Creating reply to review {ParentReviewId} by User {UserId}",
                    model.ParentReviewId.Value, userId);
            }
            else
            {
               
                var existingReviews = await _unitOfWork.Reviews
                    .FindAsync(r => r.CourseId == model.CourseId
                        && r.UserId == userId
                        && r.ParentReviewId == null);

                if (existingReviews.Any())
                {
                    var existingReview = existingReviews.First();
                    _logger.LogWarning(
                        "Duplicate review attempt: User {UserId} already reviewed Course {CourseId} (Review {ReviewId})",
                        userId, model.CourseId, existingReview.Id);
                    throw new ConflictException(
                        "You have already reviewed this course. You can update your existing review instead");
                }

                _logger.LogInformation(
                    "Creating main review for Course {CourseId} by User {UserId}",
                    model.CourseId, userId);
            }

            
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var review = _mapper.Map<CourseReview>(model);
                review.UserId = userId;
                review.CreatedAt = DateTime.UtcNow;
                review.IsDeleted = false;

                await _unitOfWork.Reviews.AddAsync(review);
                await _unitOfWork.SaveChangesAsync();

                
                if (!model.ParentReviewId.HasValue)
                {
                    await UpdateCourseRatingCacheAsync(model.CourseId);
                    _logger.LogInformation(
                        "Course {CourseId} rating cache updated after new review",
                        model.CourseId);
                }

                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Review created successfully: ID {ReviewId}, User {UserId}, Course {CourseId}, Rating {Rating}",
                    review.Id, userId, model.CourseId, model.Rating);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create review for User {UserId}, Course {CourseId}",
                    userId, model.CourseId);
                throw new BadRequestException("Failed to submit review. Please try again.");
            }
        }

        public async Task<(List<ReviewDto> Reviews, int Total)> GetCourseReviewsAsync(
     int courseId,
     int pageNumber = 1,
     int pageSize = 10)
        {
            _logger.LogDebug("Fetching reviews for Course {CourseId}", courseId);

            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null)
            {
                _logger.LogWarning("Review fetch failed: Course {CourseId} not found", courseId);
                throw new NotFoundException("Course", courseId);
            }

            var reviews = await _unitOfWork.Reviews.GetReviewsByCourseIdAsync(courseId);
            var reviewDtos = _mapper.Map<List<ReviewDto>>(reviews);

            var mainReviews = reviewDtos.Where(r => r.ParentReviewId == null).ToList();

            foreach (var mainReview in mainReviews)
            {
                mainReview.Replies = reviewDtos
                    .Where(r => r.ParentReviewId == mainReview.Id)
                    .ToList();
            }

            var total = mainReviews.Count;

            var pagedReviews = mainReviews
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            _logger.LogInformation(
                "Retrieved {Count} main review(s) (page {Page}) for Course {CourseId}",
                pagedReviews.Count, pageNumber, courseId);

            return (pagedReviews, total);
        }


        public async Task UpdateReviewAsync(int userId, int reviewId, UpdateReviewDto model)
        {
            _logger.LogInformation("Review update requested: User {UserId}, Review {ReviewId}",
                userId, reviewId);

            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review == null)
            {
                _logger.LogWarning("Update failed: Review {ReviewId} not found", reviewId);
                throw new NotFoundException("Review", reviewId);
            }

            if (review.UserId != userId)
            {
                _logger.LogWarning("Unauthorized update attempt: User {UserId} tried to edit Review {ReviewId}",
                    userId, reviewId);
                throw new ForbiddenException("You can only edit your own reviews");
            }

            if (review.IsDeleted)
            {
                _logger.LogWarning("Update failed: Review {ReviewId} is deleted", reviewId);
                throw new BadRequestException("Cannot update a deleted review");
            }

            string oldComment = review.Comment;
            int oldRating = review.Rating;

            review.Comment = model.Comment;
            review.Rating = model.Rating;
            review.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Reviews.Update(review);
            await _unitOfWork.SaveChangesAsync();

            if (oldRating != model.Rating && review.ParentReviewId == null)
            {
                await UpdateCourseRatingCacheAsync(review.CourseId);
                _logger.LogInformation("Course rating updated after review edit");
            }

            _logger.LogInformation(
                "Review {ReviewId} updated: Rating {OldRating} -> {NewRating}",
                reviewId, oldRating, model.Rating);
        }

        public async Task DeleteReviewAsync(int userId, int reviewId)
        {
            _logger.LogInformation("Review deletion requested: User {UserId}, Review {ReviewId}",
                userId, reviewId);

            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review == null)
            {
                _logger.LogWarning("Delete failed: Review {ReviewId} not found", reviewId);
                throw new NotFoundException("Review", reviewId);
            }

            if (review.UserId != userId)
            {
                _logger.LogWarning("Unauthorized delete attempt: User {UserId} tried to delete Review {ReviewId}",
                    userId, reviewId);
                throw new ForbiddenException("You can only delete your own reviews");
            }

            if (review.IsDeleted)
            {
                _logger.LogWarning("Review {ReviewId} is already deleted", reviewId);
                throw new BadRequestException("This review has already been deleted");
            }

            review.IsDeleted = true;
            review.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Reviews.Update(review);
            await _unitOfWork.SaveChangesAsync();

            if (review.ParentReviewId == null)
            {
                await UpdateCourseRatingCacheAsync(review.CourseId);
                _logger.LogInformation("Course rating updated after review deletion");
            }

            _logger.LogWarning("Review soft-deleted: ID {ReviewId} by User {UserId}",
                reviewId, userId);
        }

        // ========================================
        // PRIVATE HELPER METHODS
        // ========================================


        private async Task UpdateCourseRatingCacheAsync(int courseId)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null)
                return;

            // Get all main reviews (ParentReviewId = null) for this course
            var mainReviews = await _unitOfWork.Reviews.FindAsync(
                r => r.CourseId == courseId && r.ParentReviewId == null && !r.IsDeleted);

            if (mainReviews.Any())
            {
                course.AverageRating = Math.Round(mainReviews.Average(r => (double)r.Rating), 2);
                course.ReviewCount = mainReviews.Count;
            }
            else
            {
                // No reviews yet
                course.AverageRating = 0;
                course.ReviewCount = 0;
            }

            _unitOfWork.Courses.Update(course);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Course {CourseId} rating cache updated: Avg={AverageRating}, Count={ReviewCount}",
                courseId, course.AverageRating, course.ReviewCount);
        }
    }
}