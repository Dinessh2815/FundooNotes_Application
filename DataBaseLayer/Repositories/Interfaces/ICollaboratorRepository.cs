using DataBaseLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseLayer.Repositories.Interfaces
{
    public interface ICollaboratorRepository
    {
        Task<bool> ExistsAsync(int noteId, int userId);
        Task AddAsync(Collaborator collaborator);
        Task RemoveAsync(int noteId, int userId);
        Task<List<Collaborator>> GetByNoteIdAsync(int noteId);
        Task<Collaborator?> GetAsync(int noteId, int userId);
    }

}
