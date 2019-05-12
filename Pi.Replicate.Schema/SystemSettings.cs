﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Schema
{
    public class SystemSetting
    {
        public Guid Id { get; set; }

        public string Key { get; set; }

        public object Value { get; set; }
    }
}