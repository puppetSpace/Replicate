using Microsoft.AspNetCore.Components;
using Pi.Replicate.Application.Files.Queries.GetFileOverviewForRecipient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Pages.Folders.Components
{
    public class FileItemBase : ComponentBase
    {
		[Parameter]
		public FileOverviewModel File { get; set; }
	}
}
