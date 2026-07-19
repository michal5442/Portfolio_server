using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using portfolio_server.Models;
using portfolio_server.Interfaces;

namespace portfolio_server.Repositories
{
    public class TsevetMevatseaRepository : ITsevetMevatseaRepository
    {
        private readonly IMongoCollection<TsevetMevatsea> _collection;
        private readonly IMongoCollection<Project> _projectCollection;

        public TsevetMevatseaRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<TsevetMevatsea>("TsevetMevatsea");
            _projectCollection = database.GetCollection<Project>("Projects");
        }

        public async Task<TsevetMevatsea> InsertTsevetMevatsea(TsevetMevatsea team)
        {
            team.IdntTsevetMevatsea = Guid.NewGuid();
            team.Active = true;

            await _collection.InsertOneAsync(team);
            return team;
        }

        public async Task<IEnumerable<TsevetMevatsea>> GetAllTsevetMevatsea()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<TsevetMevatsea?> GetTsevetMevatseaById(Guid id)
        {
            return await _collection.Find(t => t.IdntTsevetMevatsea == id).FirstOrDefaultAsync();
        }

        public async Task<TsevetMevatsea?> UpdateTsevetMevatsea(Guid id, TsevetMevatsea team)
        {
            // No Builders: fetch the existing document, mutate the field, then replace the whole document.
            var existing = await _collection.Find(t => t.IdntTsevetMevatsea == id).FirstOrDefaultAsync();
            if (existing is null)
            {
                return null;
            }

            existing.TsevetMevatseaName = team.TsevetMevatseaName;

            var result = await _collection.ReplaceOneAsync(t => t.IdntTsevetMevatsea == id, existing);
            if (result.MatchedCount == 0)
            {
                return null;
            }

            await SyncProjectsTsevetMevatseaName(id, existing.TsevetMevatseaName);

            return existing;
        }

        /// <summary>
        /// Keeps the denormalized TsevetMevatseaName on existing Projects in sync after a rename,
        /// so callers reading a Project never see a stale team name.
        /// Uses Builders&lt;Project&gt;.Filter.Eq(p =&gt; p.Id, ...) to build each filter: this is
        /// the type-safe option — the compiler checks that "Id" is a real property on
        /// Project, and a rename via Visual Studio's refactor tools updates it automatically.
        /// </summary>
        private async Task SyncProjectsTsevetMevatseaName(Guid tsevetId, string? tsevetMevatseaName)
        {
            var projects = await _projectCollection.Find(p => p.IdntTsevetMevatsea == tsevetId).ToListAsync();
            if (projects.Count == 0)
            {
                return;
            }

            var models = new List<WriteModel<Project>>(projects.Count);
            foreach (var project in projects)
            {
                project.TsevetMevatseaName = tsevetMevatseaName;

                var filter = Builders<Project>.Filter.Eq(p => p.Id, project.Id);
                models.Add(new ReplaceOneModel<Project>(filter, project));
            }

            await _projectCollection.BulkWriteAsync(models);
        }

        public async Task<TsevetMevatsea?> ToggleTsevetMevatseaActive(Guid id)
        {
            var team = await _collection.Find(t => t.IdntTsevetMevatsea == id).FirstOrDefaultAsync();
            if (team == null)
            {
                return null;
            }

            team.Active = !team.Active;

            var result = await _collection.ReplaceOneAsync(t => t.IdntTsevetMevatsea == id, team);
            return result.MatchedCount == 0 ? null : team;
        }
    }
}