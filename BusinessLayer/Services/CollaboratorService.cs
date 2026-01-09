using BusinessLayer.Interfaces;
using DataBaseLayer.Entities;
using DataBaseLayer.Repositories.Interfaces;
using ModelLayer.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class CollaboratorService : ICollaboratorService
    {
        private readonly INoteRepository _noteRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICollaboratorRepository _collaboratorRepository;

        public CollaboratorService(
            INoteRepository noteRepository,
            IUserRepository userRepository,
            ICollaboratorRepository collaboratorRepository)
        {
            _noteRepository = noteRepository;
            _userRepository = userRepository;
            _collaboratorRepository = collaboratorRepository;
        } 

        public async Task AddAsync(int noteId, AddCollaboratorRequestDto request, int ownerUserId)
        {
            var note = await _noteRepository.GetByIdAsync(noteId, ownerUserId)
                ?? throw new Exception("Note not found");

            var user = await _userRepository.GetByEmailAsync(request.Email)
                ?? throw new Exception("User not found");

            if (user.UserId == ownerUserId)
                throw new Exception("Owner cannot be collaborator");

            if (await _collaboratorRepository.ExistsAsync(noteId, user.UserId))
                return;

            await _collaboratorRepository.AddAsync(new Collaborator
            {
                NoteId = noteId,
                UserId = user.UserId,
                CanEdit = request.CanEdit
            });
        }

        public async Task RemoveAsync(int noteId, int collaboratorUserId, int ownerUserId)
        {
            var note = await _noteRepository.GetByIdAsync(noteId, ownerUserId)
                ?? throw new Exception("Note not found");

            await _collaboratorRepository.RemoveAsync(noteId, collaboratorUserId);
        }

        public async Task<List<CollaboratorResponseDto>> GetAsync(int noteId, int userId)
        {
            var collaborators = await _collaboratorRepository.GetByNoteIdAsync(noteId);

            return collaborators.Select(c => new CollaboratorResponseDto
            {
                UserId = c.UserId,
                Email = c.User.Email,
                CanEdit = c.CanEdit
            }).ToList();
        }

        // 🔐 Permission check
        public async Task<bool> CanEditAsync(int noteId, int userId)
        {
            var collaborator = await _collaboratorRepository.GetAsync(noteId, userId);
            return collaborator?.CanEdit == true;
        }
    }

}
