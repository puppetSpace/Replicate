using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processing.Repositories
{
    public interface IHostRepository
    {
        Task<IEnumerable<Host>> GetDestinationHosts(Guid folderId);

        Task Save(Host host);

        Task Delete(Guid hostId);
    }
}
