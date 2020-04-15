using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Recipients.Queries.GetRecipientList
{
    public class RecipientListVm
    {
        public ICollection<Recipient> Recipients { get; set; }
    }
}
