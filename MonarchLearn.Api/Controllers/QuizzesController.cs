using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonarchLearn.Application.DTOs.Quizzes;
using MonarchLearn.Application.Interfaces.Services;

namespace MonarchLearn.Api.Controllers
{
    [Route("api/v1/quizzes")]
    [Authorize(Roles = "Student,Admin,Instructor")]
    public class QuizzesController : BaseController
    {
        private readonly IQuizService _quizService;
        public QuizzesController(IQuizService quizService) => _quizService = quizService;

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var quiz = await _quizService.GetQuizForStudentAsync(CurrentUserId, id);
            return Ok(quiz);
        }

        [HttpPost("{id:int}/submissions")]
        public async Task<IActionResult> CreateSubmission(int id, [FromBody] QuizSubmissionDto model)
        {
            if (id != model.QuizId) return BadRequest(new { message = "QuizId mismatch" });

            var result = await _quizService.SubmitQuizAsync(CurrentUserId, model);
            return Ok(result);
        }

        [HttpGet("{id:int}/attempts")]
        public async Task<IActionResult> GetAttempts(int id, [FromQuery] int enrollmentId)
        {
            var attempts = await _quizService.GetQuizAttemptsAsync(CurrentUserId, id, enrollmentId);
            return Ok(attempts);
        }
    }
}