using Pi.Replicate.Application.Common;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.SystemSettings.Queries.GetSystemSettingOverview
{
    public class SystemSettingViewModel : ViewModelBase
    {
		private string _key;
		private string _value;

		public string Key { get => _key; set => Set(ref _key, value); }

		public string Value { get => _value; set => Set(ref _value, value); }

		public string DataType { get; set; }

		public string Info { get; set; }
	}
}
