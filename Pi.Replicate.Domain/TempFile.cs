﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Domain
{
    public class TempFile
    {
		public Guid Id { get; set; }

		public string Path { get; set; }

        public string Hash { get; set; }

        public Guid FileId { get; set; }
    }
}