using ModelLayer.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface ICollaboratorService
    {
        Task AddAsync(int noteId, AddCollaboratorRequestDto request, int ownerUserId);
        Task RemoveAsync(int noteId, int collaboratorUserId, int ownerUserId);
        Task<List<CollaboratorResponseDto>> GetAsync(int noteId, int userId);
        Task<bool> CanEditAsync(int noteId, int userId);
    }

}
