using ModelLayer.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface INoteLabelService
    {
        Task AddLabelAsync(int noteId, int labelId, int userId);
        Task RemoveLabelAsync(int noteId, int labelId, int userId);
        Task<List<LabelResponseDto>> GetLabelsAsync(int noteId, int userId);
    }

}
