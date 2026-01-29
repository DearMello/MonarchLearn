using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonarchLearn.Application.DTOs.Quizzes;
using MonarchLearn.Application.Interfaces.Services;

namespace MonarchLearn.Api.Controllers
{
    [Route("api/v1/management/quizzes")]
    [Authorize(Roles = "Instructor,Admin")]
    public class QuizManagementController : BaseController
    {
        private readonly IQuizManagementService _quizManagementService;

        public QuizManagementController(IQuizManagementService quizManagementService)
        {
            _quizManagementService = quizManagementService;
        }

        [HttpPut("{quizId:int}/settings")]
        public async Task<IActionResult> UpdateSettings(int quizId, [FromBody] UpdateQuizSettingsDto model)
        {
            await _quizManagementService.UpdateQuizSettingsAsync(CurrentUserId, quizId, model, IsAdmin);
            return Ok(new { message = "Quiz settings updated successfully." });
        }

        [HttpGet("{quizId:int}/questions")]
        public async Task<IActionResult> GetQuestions(int quizId)
        {
            var questions = await _quizManagementService.GetQuestionsByQuizIdAsync(CurrentUserId, quizId, IsAdmin);
            return Ok(questions);
        }

        [HttpPost("{quizId:int}/questions")]
        public async Task<IActionResult> CreateQuestion(int quizId, [FromBody] CreateQuestionDto model)
        {
            var questionId = await _quizManagementService.CreateQuestionAsync(CurrentUserId, quizId, model, IsAdmin);
            return Created(string.Empty, new { id = questionId });
        }

        [HttpPut("questions/{id:int}")]
        public async Task<IActionResult> UpdateQuestion(int id, [FromBody] UpdateQuestionDto model)
        {
            if (id != model.Id) return BadRequest(new { message = "ID mismatch." });

            await _quizManagementService.UpdateQuestionAsync(CurrentUserId, id, model, IsAdmin);
            return Ok(new { message = "Question updated successfully." });
        }

        [HttpDelete("questions/{id:int}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            await _quizManagementService.DeleteQuestionAsync(CurrentUserId, id, IsAdmin);
            return NoContent();
        }

        [HttpPost("questions/{questionId:int}/options")]
        public async Task<IActionResult> CreateOption(int questionId, [FromBody] CreateOptionDto model)
        {
            var optionId = await _quizManagementService.CreateOptionAsync(CurrentUserId, questionId, model, IsAdmin);
            return Created(string.Empty, new { id = optionId });
        }

        [HttpPut("options/{id:int}")]
        public async Task<IActionResult> UpdateOption(int id, [FromBody] UpdateOptionDto model)
        {
            if (id != model.Id) return BadRequest(new { message = "ID mismatch." });

            await _quizManagementService.UpdateOptionAsync(CurrentUserId, id, model, IsAdmin);
            return Ok(new { message = "Option updated successfully." });
        }

        [HttpDelete("options/{id:int}")]
        public async Task<IActionResult> DeleteOption(int id)
        {
            await _quizManagementService.DeleteOptionAsync(CurrentUserId, id, IsAdmin);
            return NoContent();
        }
    }
}