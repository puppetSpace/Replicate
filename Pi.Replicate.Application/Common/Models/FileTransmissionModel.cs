using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Common.Models
{
    public class FileTransmissionModel
    {
		public Guid Id { get; set; }

		public string FolderName { get; set; }

        public string Name { get; set; }

        public long Size { get; set; }

        public int Version { get; set; }

        public byte[] Signature { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public string Path { get; set; }
    }
}
