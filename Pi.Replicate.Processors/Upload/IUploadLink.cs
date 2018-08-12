﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors.Upload
{
    public interface IUploadLink<Tin,Tout>
    {
        Task<UploadResult<Tout>> UploadData(Tin input);
    }
}
