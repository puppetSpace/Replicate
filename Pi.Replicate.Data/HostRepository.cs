using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Processing.Repositories;
using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Data
{
    internal sealed class HostRepository : IHostRepository
    {
        private ReplicateDbContext _replicateDbContext;

        public HostRepository(ReplicateDbContext replicateDbContext)
        {
            _replicateDbContext = replicateDbContext;
        }

        public async Task Delete(Guid hostId)
        {
            var foundHost = await _replicateDbContext.Hosts.FirstOrDefaultAsync(x => x.Id == hostId);
            if (foundHost != null)
            {
                _replicateDbContext.Hosts.Remove(foundHost);
                await _replicateDbContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Host>> GetDestinationHosts(Guid folderId)
        {
            return await _replicateDbContext.Hosts.ToListAsync();
        }

        public async Task Save(Host host)
        {
            _replicateDbContext.Hosts.Add(host);
            await _replicateDbContext.SaveChangesAsync();
        }
    }
}
