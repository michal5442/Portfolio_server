using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using portfolio_server.Models;

namespace portfolio_server.Interfaces
{
    public interface ITsevetMevatseaRepository
    {
        Task<TsevetMevatsea> InsertTsevetMevatsea(TsevetMevatsea team);
        Task<IEnumerable<TsevetMevatsea>> GetAllTsevetMevatsea();
        Task<TsevetMevatsea?> GetTsevetMevatseaById(Guid id);
        Task<TsevetMevatsea?> UpdateTsevetMevatsea(Guid id, TsevetMevatsea team);
        Task<TsevetMevatsea?> ToggleTsevetMevatseaActive(Guid id);
    }
}