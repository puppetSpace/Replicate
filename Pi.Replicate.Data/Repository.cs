using Pi.Replicate.Processing.Repositories;
using System;

namespace Pi.Replicate.Data
{
    public class Repository : IRepository
    {
        private readonly ReplicateDbContext _replicateDbContext;

        public Repository(ReplicateDbContext replicateDbContext)
        {
            _replicateDbContext = replicateDbContext;
        }
        
        public IFileChunkRepository FileChunkRepository
        {
            get
            {
                return new FileChunkRepository(_replicateDbContext);
            }
        }

        public IFileRepository FileRepository
        {
            get
            {
                return new FileRepository(_replicateDbContext);
            }
        }

        public IFolderRepository FolderRepository
        {
            get
            {
                return new FolderRepository(_replicateDbContext);
            }
        }

        public IHostRepository HostRepository
        {
            get
            {
                return new HostRepository(_replicateDbContext);
            }
        }

		public ISystemSettingRepository SystemSettingRepository
		{
			get
			{
				return new SystemSettingRepository(_replicateDbContext);
			}
		}
	}
}
