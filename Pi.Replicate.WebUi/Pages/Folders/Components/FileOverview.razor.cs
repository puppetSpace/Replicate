using Microsoft.AspNetCore.Components;
using Pi.Replicate.Application.Files.Queries.GetFileOverviewForRecipient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Pages.Folders.Components
{
    public class FileOverviewBase : ComponentBase
    {
		[Parameter]
		public List<FileOverviewModel> Files { get; set; }

		[Parameter]
		public EventCallback OnCloseClicked { get; set; }

	}
}
