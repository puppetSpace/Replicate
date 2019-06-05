using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processing.Repositories
{
    public interface ISystemSettingRepository
    {
		Task<SystemSetting> Get(string key);

		Task<IEnumerable<SystemSetting>> Get();

		Task Add(string key, string value);
    }
}
