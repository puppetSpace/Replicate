using Pi.Replicate.Processing.Repositories;
using System;

namespace Pi.Replicate.Data
{
    public class Repository : IRepository
    {
        public IFileChunkRepository FileChunkRepository
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IFileRepository FileRepository
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IFolderRepository FolderRepository
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IHostRepository HostRepository
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
