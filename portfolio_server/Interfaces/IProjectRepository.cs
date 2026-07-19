using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using portfolio_server.Models;

namespace portfolio_server.Interfaces
{
    public interface IProjectRepository
    {
        Task<Project> InsertProject(Project project);

        Task<Project?> UpdateProject(Project project);

        Task<Project?> GetProjectById(Guid id);

        Task<IEnumerable<Project>> GetProjectsByYear(int year);

        Task<IEnumerable<Project>> GetAllProjects();
        
        Task<Project?> DeleteProject(Guid id);

        Task<Project?> ToggleProjectActive(Guid id);

        Task<IEnumerable<Project>> CopyProjectsFromPreviousYear(int year);

    }
}