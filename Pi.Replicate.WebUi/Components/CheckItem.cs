﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Components
{
    public class CheckItem<TE>
    {
        public bool IsChecked { get; set; }

        public string DisplayText { get; set; }

        public TE Data { get; set; }
    }
}
