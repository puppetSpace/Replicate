using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Common
{
    public struct ProcessItem<TE,TMeta>
    {
        public ProcessItem(TE item, TMeta metadata)
        {
            Item = item;
            Metadata = metadata;
        }

        public TE Item { get; }

        public TMeta Metadata { get;  }
    }
}
