using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.FileChanges.Models
{
    public class FileChangeTransmissionModel
    {

        public string FilePath { get; set; }

        public byte[] FileSignature { get; set; }

        public long FileSize { get; set; }

        public int VersionNo { get; set; }

        public int AmountOfChunks { get; set; }

        public byte[] Signature { get; set; }

        public DateTime LastModifiedDate { get; set; }
    }
}
