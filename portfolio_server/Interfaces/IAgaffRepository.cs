using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using portfolio_server.Models;

namespace portfolio_server.Interfaces
{
    public interface IAgaffRepository
    {
        Task<Agaff> InsertAgaff(Agaff agaff);
        Task<IEnumerable<Agaff>> GetAllAgaff();
        Task<Agaff?> GetAgaffById(Guid id);
        Task<Agaff?> UpdateAgaff(Guid id, Agaff agaff);
        Task<Agaff?> ToggleAgaffActive(Guid id);
    }
}