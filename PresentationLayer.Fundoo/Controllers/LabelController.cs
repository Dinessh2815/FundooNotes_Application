using BusinessLayer.Interfaces;
using DataBaseLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.DTOs;
using System.Security.Claims;

namespace PresentationLayer.Fundoo
{

    [ApiController]
    [Route("api/labels")]
    [Authorize]
    public class LabelsController : ControllerBase
    {
        private readonly ILabelService _labelService;

        public LabelsController(ILabelService labelService)
        {
            _labelService = labelService;
        }

        private int UserId =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpPost]
        public async Task<IActionResult> Create(CreateLabelRequestDto request)
        {
            await _labelService.CreateAsync(request, UserId);
            return Ok("Label created");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _labelService.GetAllAsync(UserId));
        }

        [HttpPut("{labelId}")]
        public async Task<IActionResult> Update(
            int labelId,
            UpdateLabelRequestDto request)
        {
            await _labelService.UpdateAsync(labelId, request, UserId);
            return Ok("Label updated");
        }

        [HttpDelete("{labelId}")]
        public async Task<IActionResult> Delete(int labelId)
        {
            await _labelService.DeleteAsync(labelId, UserId);
            return Ok("Label deleted");
        }
    }
}
