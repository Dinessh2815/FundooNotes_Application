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
    public class NoteLabelRepository : INoteLabelRepository
    {
        private readonly FundooNotesDbContext _context;

        public NoteLabelRepository(FundooNotesDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(int noteId, int labelId)
        {
            return await _context.NoteLabels
                .AnyAsync(nl => nl.NoteId == noteId && nl.LabelId == labelId);
        }

        public async Task AddAsync(NoteLabel noteLabel)
        {
            _context.NoteLabels.Add(noteLabel);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(int noteId, int labelId)
        {
            var mapping = await _context.NoteLabels
                .FirstOrDefaultAsync(nl => nl.NoteId == noteId && nl.LabelId == labelId);

            if (mapping != null)
            {
                _context.NoteLabels.Remove(mapping);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Label>> GetLabelsByNoteIdAsync(int noteId, int userId)
        {
            return await _context.NoteLabels
                .Where(nl => nl.NoteId == noteId && nl.Note.UserId == userId)
                .Select(nl => nl.Label)
                .ToListAsync();
        }
    }

}
