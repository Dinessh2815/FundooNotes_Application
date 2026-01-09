using ModelLayer.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLayer.Interfaces
{
    public interface INoteService
    {
        Task CreateAsync(CreateNoteRequestDto request, int userId);
        Task<List<NoteResponseDto>> GetAllAsync(int userId);
        Task UpdateAsync(int noteId, UpdateNoteRequestDto request, int userId);
        Task DeleteAsync(int noteId, int userId);
    }

}
