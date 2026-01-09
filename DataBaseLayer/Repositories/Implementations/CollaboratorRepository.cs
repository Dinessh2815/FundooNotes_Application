using DataBaseLayer.DbContexts;
using DataBaseLayer.Entities;
using DataBaseLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseLayer.Repositories.Implementations
{
    public class CollaboratorRepository : ICollaboratorRepository
    {
        private readonly FundooNotesDbContext _context;

        public CollaboratorRepository(FundooNotesDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(int noteId, int userId)
        {
            return await _context.Collaborators
                .AnyAsync(c => c.NoteId == noteId && c.UserId == userId);
        }

        public async Task AddAsync(Collaborator collaborator)
        {
            _context.Collaborators.Add(collaborator);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(int noteId, int userId)
        {
            var collab = await GetAsync(noteId, userId);
            if (collab != null)
            {
                _context.Collaborators.Remove(collab);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Collaborator>> GetByNoteIdAsync(int noteId)
        {
            return await _context.Collaborators
                .Include(c => c.User)
                .Where(c => c.NoteId == noteId)
                .ToListAsync();
        }

        public async Task<Collaborator?> GetAsync(int noteId, int userId)
        {
            return await _context.Collaborators
                .FirstOrDefaultAsync(c => c.NoteId == noteId && c.UserId == userId);
        }
    }

}
