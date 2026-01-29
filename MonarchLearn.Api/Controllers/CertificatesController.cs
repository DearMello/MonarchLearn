using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonarchLearn.Application.DTOs.Certificates;
using MonarchLearn.Application.Interfaces.Services;

namespace MonarchLearn.Api.Controllers
{
    [Route("api/v1/certificates")]
    [Authorize(Roles = "Student")]
    public class CertificatesController : BaseController
    {
        private readonly ICertificateService _certificateService;

        public CertificatesController(ICertificateService certificateService)
        {
            _certificateService = certificateService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GenerateCertificateDto model)
        {
            var certificate = await _certificateService.GenerateCertificateAsync(CurrentUserId, model.EnrollmentId);
            return Created(string.Empty, certificate);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var certificate = await _certificateService.GetCertificateByIdAsync(CurrentUserId, id);
            return Ok(certificate);
        }

        [HttpGet("{id:int}/pdf")]
        public async Task<IActionResult> Download(int id)
        {
            var fileData = await _certificateService.GetCertificatePdfAsync(CurrentUserId, id);
            return File(fileData.FileBytes, "application/pdf", fileData.FileName);
        }
    }
}