using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors.Upload
{
    public class UploadResult<TE>
    {
        public TE Data { get; set; }

        //todo add properties for error
    }
}
