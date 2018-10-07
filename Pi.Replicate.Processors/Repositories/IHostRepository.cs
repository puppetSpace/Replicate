using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors.Repositories
{
    public interface IHostRepository
    {
        Task<IEnumerable<Host>> GetDestinationHosts(Guid folderId);
    }
}
