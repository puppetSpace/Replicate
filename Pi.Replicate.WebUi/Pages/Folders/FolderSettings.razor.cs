using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Pages.Folders
{
    public class FolderSettingsBase : ComponentBase
    {
		[Parameter]
		public string FolderId { get; set; }

		protected override Task OnInitializedAsync()
		{
			return base.OnInitializedAsync();
		}
	}
}
