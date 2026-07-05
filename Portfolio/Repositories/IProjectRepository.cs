using System.Threading.Tasks;
using Portfolio.Models;

namespace Portfolio.Repositories
{
    public interface IProjectRepository
    {
        Task<Project> InsertProject(Project project);

        Task<Project> UpdateProject(Project project);

        Task<IEnumerable<Project>> GetAllProjects();

        Task<Project> GetProjectById(string id);

        Task<IEnumerable<Project>> GetProjectsByYear(int year);

        Task<Project> DeleteProject(string id);

    }
}
