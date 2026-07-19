using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using portfolio_server.Models;
using portfolio_server.Interfaces;

namespace portfolio_server.Repositories
{
    public class AgaffRepository : IAgaffRepository
    {
        private readonly IMongoCollection<Agaff> _collection;
        private readonly IMongoCollection<Project> _projectCollection;

        public AgaffRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<Agaff>("Agaff");
            _projectCollection = database.GetCollection<Project>("Projects");
        }

        public async Task<Agaff> InsertAgaff(Agaff agaff)
        {
            agaff.IdntAgaff = Guid.NewGuid();
            agaff.Active = true;

            await _collection.InsertOneAsync(agaff);
            return agaff;
        }

        public async Task<IEnumerable<Agaff>> GetAllAgaff()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<Agaff?> GetAgaffById(Guid id)
        {
            return await _collection.Find(a => a.IdntAgaff == id).FirstOrDefaultAsync();
        }

        public async Task<Agaff?> UpdateAgaff(Guid id, Agaff agaff)
        {
            // No Builders: fetch the existing document, mutate the field, then replace the whole document.
            var existing = await _collection.Find(a => a.IdntAgaff == id).FirstOrDefaultAsync();
            if (existing is null)
            {
                return null;
            }

            existing.AgaffName = agaff.AgaffName;

            var result = await _collection.ReplaceOneAsync(a => a.IdntAgaff == id, existing);
            if (result.MatchedCount == 0)
            {
                return null;
            }

            await SyncProjectsAgaffName(id, existing.AgaffName);

            return existing;
        }

        /// <summary>
        /// Keeps the denormalized AgaffName on existing Projects in sync after a rename,
        /// so callers reading a Project never see a stale Agaff name.
        /// Uses Builders&lt;Project&gt;.Filter.Eq(p =&gt; p.Id, ...) to build each filter: this is
        /// the type-safe option — the compiler checks that "Id" is a real property on
        /// Project, and a rename via Visual Studio's refactor tools updates it automatically.
        /// A plain BsonDocument("_id", ...) filter or a bare lambda would both work too,
        /// but neither is checked by the compiler the way this is.
        /// </summary>
        private async Task SyncProjectsAgaffName(Guid agaffId, string? agaffName)
        {
            var projects = await _projectCollection.Find(p => p.IdntAgaff == agaffId).ToListAsync();
            if (projects.Count == 0)
            {
                return;
            }

            var models = new List<WriteModel<Project>>(projects.Count);
            foreach (var project in projects)
            {
                project.AgaffName = agaffName;

                var filter = Builders<Project>.Filter.Eq(p => p.Id, project.Id);
                models.Add(new ReplaceOneModel<Project>(filter, project));
            }

            await _projectCollection.BulkWriteAsync(models);
        }

        public async Task<Agaff?> ToggleAgaffActive(Guid id)
        {
            var agaff = await _collection.Find(a => a.IdntAgaff == id).FirstOrDefaultAsync();
            if (agaff == null)
            {
                return null;
            }

            agaff.Active = !agaff.Active;

            var result = await _collection.ReplaceOneAsync(a => a.IdntAgaff == id, agaff);
            return result.MatchedCount == 0 ? null : agaff;
        }
    }
}