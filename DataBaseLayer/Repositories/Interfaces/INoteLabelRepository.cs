using DataBaseLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseLayer.Repositories.Interfaces
{
    public interface INoteLabelRepository
    {
        Task<bool> ExistsAsync(int noteId, int labelId);
        Task AddAsync(NoteLabel noteLabel);
        Task RemoveAsync(int noteId, int labelId);
        Task<List<Label>> GetLabelsByNoteIdAsync(int noteId, int userId);
    }

}
