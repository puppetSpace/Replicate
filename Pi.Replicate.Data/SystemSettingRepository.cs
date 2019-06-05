using Pi.Replicate.Processing.Repositories;
using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Data
{
	public class SystemSettingRepository : ISystemSettingRepository
	{
		private readonly ReplicateDbContext _replicateDbContext;

		public SystemSettingRepository(ReplicateDbContext replicateDbContext)
		{
			_replicateDbContext = replicateDbContext;
		}

		public Task Add(string key, string value)
		{
			throw new NotImplementedException();
		}

		public Task<SystemSetting> Get(string key)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<SystemSetting>> Get()
		{
			throw new NotImplementedException();
		}
	}
}
