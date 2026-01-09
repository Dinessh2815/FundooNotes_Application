using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseLayer.Entities;

namespace DataBaseLayer.Repositories.Interfaces
{
    public interface ILabelRepository
    {
        Task AddAsync(Label label);
        Task<List<Label>> GetAllAsync(int userId);
        Task<Label?> GetByIdAsync(int labelId, int userId);
        Task UpdateAsync(Label label);
        Task DeleteAsync(Label label);
    }

}
