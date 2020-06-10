using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Models
{
    public class FileConflictDeleteArgument
    {
		public Guid FileConflictId { get; set; }

		public Guid FileId { get; set; }
	}
}
