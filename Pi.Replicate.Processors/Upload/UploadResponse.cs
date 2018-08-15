using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors.Upload
{
    public class UploadResponse<TE>
    {
        public TE Data { get; set; }

        public bool IsSuccessful { get; set; }

        public string ErrorMessage { get; set; }


    }
}
