using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processing
{
    public interface IWorkItemQueue<TE>
    {
        Task Enqueue(TE item);

        Task<TE> Dequeue();

        bool HasItems();
    }
}
