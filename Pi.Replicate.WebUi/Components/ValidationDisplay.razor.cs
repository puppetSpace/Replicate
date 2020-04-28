using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Components
{
    public class ValidationDisplayBase : ComponentBase
    {
		[Parameter]
		public List<string> ValidationMessages { get; set; } = new List<string>();
	}
}
