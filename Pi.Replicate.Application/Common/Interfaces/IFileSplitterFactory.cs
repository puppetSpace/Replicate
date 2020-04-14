using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Common.Interfaces
{
    public interface IFileSplitterFactory
    {
        IFileSplitter Get();
    }

    public interface IFileSplitter
    {
        Task<byte[]> ProcessFile(File file, Action<byte[]> chunkCreatedDelegate);
    }
}
