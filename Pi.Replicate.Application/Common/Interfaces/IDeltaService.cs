using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Common.Interfaces
{
    public interface IDeltaService
    {
		ReadOnlyMemory<byte> CreateSignature(Stream input);
		ReadOnlyMemory<byte> CreateSignature(string path);
		ReadOnlyMemory<byte> CreateDelta(Stream input, ReadOnlyMemory<byte> signature);
		ReadOnlyMemory<byte> CreateDelta(string path, ReadOnlyMemory<byte> signature);
		void ApplyDelta(Stream input, ReadOnlyMemory<byte> delta, Stream output);
		void ApplyDelta(string path, ReadOnlyMemory<byte> delta);
	}
}
