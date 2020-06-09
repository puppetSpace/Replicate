using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Models
{
    public class FileConflict
    {
		public Guid FileId { get; private set; }

		public string Message { get; private set; }

		public static FileConflict Create(Guid fileId, string message)
		{
			return new FileConflict
			{
				FileId = fileId,
				Message = message
			};
		}
	}
}
