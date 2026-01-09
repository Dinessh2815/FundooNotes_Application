using DataBaseLayer.DbContexts;
using DataBaseLayer.Entities;
using DataBaseLayer.Repositories.Interfaces;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace DataBaseLayer.Repositories.Implementations
{
    public class NoteRepository : INoteRepository
    {
        private readonly FundooNotesDbContext _context;


        public NoteRepository(FundooNotesDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Note note)
        {
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Note note)
        {
            _context.Notes.Update(note);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Note>> GetAllAsync(int userId)
        {
            return await _context.Notes
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .ToListAsync();
        }

        public async Task<Note?> GetByIdAsync(int noteId, int userId)
        {
            return await _context.Notes
                .FirstOrDefaultAsync(n =>
                    n.NoteId == noteId &&
                    n.UserId == userId &&
                    !n.IsDeleted);
        }

        public async Task<List<Note>> GetAllIncludingCollaborationsAsync(int userId)
        {
            return await _context.Notes
                .Where(n =>
                    !n.IsDeleted &&
                    (
                        n.UserId == userId ||
                        n.Collaborators.Any(c => c.UserId == userId)
                    )
                )
                .Distinct()
                .ToListAsync();
        }


        public async Task<Note?> GetByIdAsync(int noteId)
        {
            return await _context.Notes
                .FirstOrDefaultAsync(n =>
                    n.NoteId == noteId &&
                    !n.IsDeleted);
        }
    }


}
