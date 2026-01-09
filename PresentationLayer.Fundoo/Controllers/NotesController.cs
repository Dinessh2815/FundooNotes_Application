using BusinessLayer.Interfaces;
using DataBaseLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.DTOs;
using System.Security.Claims;

namespace PresentationLayer.Fundoo.Controllers
{
    [ApiController]
    [Route("api/notes")]
    [Authorize]
    public class NotesController : ControllerBase
    {
        private readonly INoteService _noteService;
        private readonly INoteLabelService _noteLabelService;
        private readonly ICollaboratorService _collaboratorService;

        public NotesController(
            INoteService noteService,
            INoteLabelService noteLabelService,
            ICollaboratorService collaboratorService)
        {
            _noteService = noteService;
            _noteLabelService = noteLabelService;
            _collaboratorService = collaboratorService;
        }


        private int UserId =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpPost]
        public async Task<IActionResult> Create(CreateNoteRequestDto request)
        {
            await _noteService.CreateAsync(request, UserId);
            return Ok("Note created");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _noteService.GetAllAsync(UserId));
        }

        [HttpPut("{noteId}")]
        public async Task<IActionResult> Update(
            int noteId,
            UpdateNoteRequestDto request)
        {
            await _noteService.UpdateAsync(noteId, request, UserId);
            return Ok("Note updated");
        }

        [HttpDelete("{noteId}")]
        public async Task<IActionResult> Delete(int noteId)
        {
            await _noteService.DeleteAsync(noteId, UserId);
            return Ok("Note moved to trash");
        }

        [HttpPost("{noteId}/labels/{labelId}")]
        public async Task<IActionResult> AddLabel(int noteId, int labelId)
        {
            await _noteLabelService.AddLabelAsync(noteId, labelId, UserId);
            return Ok("Label added to note");
        }

        [HttpDelete("{noteId}/labels/{labelId}")]
        public async Task<IActionResult> RemoveLabel(int noteId, int labelId)
        {
            await _noteLabelService.RemoveLabelAsync(noteId, labelId, UserId);
            return Ok("Label removed from note");
        }

        [HttpGet("{noteId}/labels")]
        public async Task<IActionResult> GetLabels(int noteId)
        {
            return Ok(await _noteLabelService.GetLabelsAsync(noteId, UserId));
        }

        [HttpPost("{noteId}/collaborators")]
        public async Task<IActionResult> AddCollaborator(
    int noteId,
    AddCollaboratorRequestDto request)
        {
            await _collaboratorService.AddAsync(noteId, request, UserId);
            return Ok("Collaborator added");
        }

        [HttpDelete("{noteId}/collaborators/{collaboratorUserId}")]
        public async Task<IActionResult> RemoveCollaborator(
            int noteId,
            int collaboratorUserId)
        {
            await _collaboratorService.RemoveAsync(noteId, collaboratorUserId, UserId);
            return Ok("Collaborator removed");
        }

        [HttpGet("{noteId}/collaborators")]
        public async Task<IActionResult> GetCollaborators(int noteId)
        {
            return Ok(await _collaboratorService.GetAsync(noteId, UserId));
        }
    }

}
