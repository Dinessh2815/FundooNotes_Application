using DataBaseLayer.DbContexts;
using DataBaseLayer.Repositories.Interfaces;
using DataBaseLayer.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseLayer.Repositories.Implementations
{
    public class LabelRepository : ILabelRepository
    {
        private readonly FundooNotesDbContext _context;

        public LabelRepository(FundooNotesDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Label label)
        {
            _context.Labels.Add(label);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Label>> GetAllAsync(int userId)
        {
            return await _context.Labels
                .Where(l => l.UserId == userId)
                .ToListAsync();
        }

        public async Task<Label?> GetByIdAsync(int labelId, int userId)
        {
            return await _context.Labels
                .FirstOrDefaultAsync(l => l.LabelId == labelId && l.UserId == userId);
        }

        public async Task UpdateAsync(Label label)
        {
            _context.Labels.Update(label);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Label label)
        {
            _context.Labels.Remove(label);
            await _context.SaveChangesAsync();
        }
    }

}
