using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Workers
{
    internal abstract class FilePreExportQueueItem
    {

    }

    internal class FilePreExportQueueItem<TE> : FilePreExportQueueItem
    {
        public FilePreExportQueueItem(TE item)
        {
            Item = item;
        }

        public TE Item { get; }
    }
}
