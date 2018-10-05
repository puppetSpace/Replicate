using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors.Communication
{
    public interface IUploadLink
    {
        Task<UploadResponse> UploadData(Uri baseAddress,FileChunk input);

        Task<UploadResponse> RequestResendOfFile(Uri baseAddress, Guid fileId);

        Task<UploadResponse> FileReceived(Uri baseAddress, Guid fileId);
    }
}
