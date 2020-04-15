using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pi.Replicate.Domain;

namespace Pi.Replicate.Application.Common.Interfaces.Repositories
{
    public interface IRecipientRepository
    {
        Task<ICollection<Recipient>> GetRecipients();
        Task<ICollection<Recipient>> GetRecipientsForFolder(Guid folderId);
        Task Create(Recipient recipient);

        Task<bool> IsNameUnique(string name);
        Task<bool> IsAddressUnique(string address);
    }
}
