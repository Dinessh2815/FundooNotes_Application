using BusinessLayer.Interfaces;
using DataBaseLayer.Repositories.Interfaces;
using ModelLayer.DTOs;
using DataBaseLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class LabelService : ILabelService
    {
        private readonly ILabelRepository _labelRepository;

        public LabelService(ILabelRepository labelRepository)
        {
            _labelRepository = labelRepository;
        }

        public async Task CreateAsync(CreateLabelRequestDto request, int userId)
        {
            var label = new Label
            {
                Name = request.Name,
                UserId = userId
            };

            await _labelRepository.AddAsync(label);
        }

        public async Task<List<LabelResponseDto>> GetAllAsync(int userId)
        {
            var labels = await _labelRepository.GetAllAsync(userId);

            return labels.Select(l => new LabelResponseDto
            {
                LabelId = l.LabelId,
                Name = l.Name
            }).ToList();
        }

        public async Task UpdateAsync(int labelId, UpdateLabelRequestDto request, int userId)
        {
            var label = await _labelRepository.GetByIdAsync(labelId, userId)
                ?? throw new Exception("Label not found");

            label.Name = request.Name;
            await _labelRepository.UpdateAsync(label);
        }

        public async Task DeleteAsync(int labelId, int userId)
        {
            var label = await _labelRepository.GetByIdAsync(labelId, userId)
                ?? throw new Exception("Label not found");

            await _labelRepository.DeleteAsync(label);
        }
    }

}
