using ModelLayer.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface ILabelService
    {
        Task CreateAsync(CreateLabelRequestDto request, int userId);
        Task<List<LabelResponseDto>> GetAllAsync(int userId);
        Task UpdateAsync(int labelId, UpdateLabelRequestDto request, int userId);
        Task DeleteAsync(int labelId, int userId);
    }

}
