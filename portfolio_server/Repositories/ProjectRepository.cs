using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using portfolio_server.Models;
using portfolio_server.Interfaces;
using portfolio_server.Repositories;

namespace portfolio_server.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly IMongoCollection<Project> _collection;
        private readonly IAgaffRepository _agaffRepository;
        private readonly ITsevetMevatseaRepository _tsevetRepository;

        public ProjectRepository(IMongoDatabase database,
            IAgaffRepository agaffRepository,
            ITsevetMevatseaRepository tsevetRepository)
        {
            _collection = database.GetCollection<Project>("Projects");
            _agaffRepository = agaffRepository;
            _tsevetRepository = tsevetRepository;

        }

        public async Task<Project> InsertProject(Project project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            await ValidateReferencesAndSyncNames(project);

            project.Id = Guid.NewGuid();
            project.Active = true;
            project.CreatedAt = DateTime.UtcNow;
            project.UpdatedAt = DateTime.UtcNow;

            await _collection.InsertOneAsync(project);

            return project;
        }


        public async Task<Project?> UpdateProject(Project project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            await ValidateReferencesAndSyncNames(project);

            project.UpdatedAt = DateTime.UtcNow;

            var result = await _collection.ReplaceOneAsync(p => p.Id == project.Id, project);

            return result.MatchedCount == 0 ? null : project;
        }

        public async Task<Project?> GetProjectById(Guid id)
        {
            var project = await _collection.Find(p => p.Id == id && p.Active).FirstOrDefaultAsync();
            return project;
        }

        public async Task<IEnumerable<Project>> GetProjectsByYear(int year)
        {
            var projects = await _collection.Find(p => p.Year == year && p.Active).ToListAsync();
            return projects;
        }

        public async Task<IEnumerable<Project>> GetAllProjects()
        {
            var projects = await _collection.Find(_ => true).ToListAsync();
            return projects;
        }

        public async Task<IEnumerable<Project>> CopyProjectsFromPreviousYear(int year)
        {
            var previousYear = year - 1;

            var sourceProjects = await _collection
                .Find(p => p.Year == previousYear && p.Active)
                .ToListAsync();

            var copiedProjects = new List<Project>(sourceProjects.Count);
            foreach (var source in sourceProjects)
            {
                copiedProjects.Add(await CloneForYear(source, year));
            }

            if (copiedProjects.Count == 0)
            {
                return copiedProjects;
            }

            await _collection.InsertManyAsync(copiedProjects);

            return copiedProjects;
        }

        public async Task<Project?> DeleteProject(Guid id)
        {
            var project = await _collection.Find(p => p.Id == id && p.Active).FirstOrDefaultAsync();
            if (project == null)
            {
                return null;
            }

            return await SaveActiveChange(project, active: false);
        }

        public async Task<Project?> ToggleProjectActive(Guid id)
        {
            var project = await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (project == null)
            {
                return null;
            }

            return await SaveActiveChange(project, active: !project.Active);
        }

        private async Task<Project?> SaveActiveChange(Project project, bool active)
        {
            project.Active = active;
            project.UpdatedAt = DateTime.UtcNow;

            var result = await _collection.ReplaceOneAsync(p => p.Id == project.Id, project);
            return result.MatchedCount == 0 ? null : project;
        }

        private async Task ValidateReferencesAndSyncNames(Project project)
        {
            var agaff = await _agaffRepository.GetAgaffById(project.IdntAgaff);
            if (agaff is null || !agaff.Active)
            {
                throw new InvalidOperationException($"Agaff with id {project.IdntAgaff} was not found or is inactive.");
            }

            var tsevet = await _tsevetRepository.GetTsevetMevatseaById(project.IdntTsevetMevatsea);
            if (tsevet is null || !tsevet.Active)
            {
                throw new InvalidOperationException($"TsevetMevatsea with id {project.IdntTsevetMevatsea} was not found or is inactive.");
            }

            project.AgaffName = agaff.AgaffName;
            project.TsevetMevatseaName = tsevet.TsevetMevatseaName;
        }

        private async Task<Project> CloneForYear(Project source, int year)
        {
            var clone = new Project
            {
                Id = Guid.NewGuid(),
                IdntAgaff = source.IdntAgaff,
                AgaffName = source.AgaffName,
                IdntTsevetMevatsea = source.IdntTsevetMevatsea,
                TsevetMevatseaName = source.TsevetMevatseaName,
                ProjectName = source.ProjectName,
                Teur = source.Teur,
                Maslol = source.Maslol,
                IdntMaslol = source.IdntMaslol,
                LogHemsheci = source.LogHemsheci,
                TotalTakzivCoachAdam = source.TotalTakzivCoachAdam,
                TotalTakzivRechesh = source.TotalTakzivRechesh,
                CoachAdam = source.CoachAdam,
                Hearot = source.Hearot,
                Year = year,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await ValidateReferencesAndSyncNames(clone);

            return clone;
        }
    }
}