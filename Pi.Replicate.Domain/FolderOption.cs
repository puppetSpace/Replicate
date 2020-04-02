using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Domain
{
    //todo check out how to create this in the database without the use of an ID
    //https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/implement-value-objects
    public class FolderOption
    {
        public Guid Id { get; set; }

        public bool DeleteAfterSent { get; set; }

        public List<Recipient> Recipient { get; set; }
    }
}
