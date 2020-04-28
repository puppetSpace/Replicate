using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Components
{
    public class DialogBase : ComponentBase
    {
		[Parameter]
		public string Title { get; set; }

		[Parameter]
		public RenderFragment ChildContent { get; set; }

		[Parameter]
		public EventCallback OnClose { get; set; }

		[Parameter]
		public EventCallback OnSubmit { get; set; }

		[Parameter]
		public string TextSubmitButton { get; set; }

		protected bool IsVisible { get; set; }

		public void Show() => IsVisible = true;

		public void Close() => IsVisible = false;
	}
}
