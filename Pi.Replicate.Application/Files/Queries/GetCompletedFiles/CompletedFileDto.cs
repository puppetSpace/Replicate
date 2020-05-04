using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Queries.GetCompletedFiles
{
    public class CompletedFileDto
    {
		public File File { get; set; }

		public EofMessage EofMessage { get; set; }
	}
}
