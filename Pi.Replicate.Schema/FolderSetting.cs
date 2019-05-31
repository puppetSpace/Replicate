using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Schema
{
    public class FolderSetting
    {
		public Guid Id { get; set; }

		public Guid FolderId { get; set; }

		public string Name { get; set; }

		public string Value { get; set; }
	}
}
