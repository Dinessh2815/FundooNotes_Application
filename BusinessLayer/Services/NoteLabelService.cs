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
    public class NoteLabelService : INoteLabelService
    {
        private readonly INoteRepository _noteRepository;
        private readonly ILabelRepository _labelRepository;
        private readonly INoteLabelRepository _noteLabelRepository;

        public NoteLabelService(
            INoteRepository noteRepository,
            ILabelRepository labelRepository,
            INoteLabelRepository noteLabelRepository)
        {
            _noteRepository = noteRepository;
            _labelRepository = labelRepository;
            _noteLabelRepository = noteLabelRepository;
        }

        public async Task AddLabelAsync(int noteId, int labelId, int userId)
        {
            // Ownership validation
            var note = await _noteRepository.GetByIdAsync(noteId, userId)
                ?? throw new Exception("Note not found");

            var label = await _labelRepository.GetByIdAsync(labelId, userId)
                ?? throw new Exception("Label not found");

            if (!await _noteLabelRepository.ExistsAsync(noteId, labelId))
            {
                await _noteLabelRepository.AddAsync(new NoteLabel
                {
                    NoteId = noteId,
                    LabelId = labelId
                });
            }
        }

        public async Task RemoveLabelAsync(int noteId, int labelId, int userId)
        {
            var note = await _noteRepository.GetByIdAsync(noteId, userId)
                ?? throw new Exception("Note not found");

            await _noteLabelRepository.RemoveAsync(noteId, labelId);
        }

        public async Task<List<LabelResponseDto>> GetLabelsAsync(int noteId, int userId)
        {
            var labels = await _noteLabelRepository
                .GetLabelsByNoteIdAsync(noteId, userId);

            return labels.Select(l => new LabelResponseDto
            {
                LabelId = l.LabelId,
                Name = l.Name
            }).ToList();
        }
    }

}
