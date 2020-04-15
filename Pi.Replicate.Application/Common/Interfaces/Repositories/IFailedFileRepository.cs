using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Common.Interfaces.Repositories
{
    public interface IFailedFileRepository
    {
        Task<ICollection<FailedFile>> Get();
        Task Create(FailedFile failedFile);
        Task DeleteAll();
    }
}
