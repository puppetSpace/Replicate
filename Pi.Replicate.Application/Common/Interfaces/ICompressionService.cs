using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Common.Interfaces
{
    public interface ICompressionService
    {
		Task Compress(string source,string destination);
		Task Decompress(string compressedFile, string destination);
	}
}
