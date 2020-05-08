using Pi.Replicate.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Recipients.Queries.GetRecipientsForSettings
{
    public class RecipientViewModel : ViewModelBase
    {
		private string _name;
		private string _address;
		private bool _verified;


		public string Name
		{
			get => _name;
			set => Set(ref _name, value);
		}

		public string Address
		{
			get => _address;
			set => Set(ref _address, value);
		}

		public bool Verified
		{
			get => _verified;
			set => Set(ref _verified, value);
		}

		public bool IsVerifying { get; set; }

		public string VerifyResult { get; set; }
	}
}
