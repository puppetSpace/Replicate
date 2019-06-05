using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Schema
{
    public class FolderOption
    {
		public Guid Id { get; set; }

		public Guid FolderId { get; set; }

		public string Key { get; set; }

		public string Value { get; set; }
	}
}
