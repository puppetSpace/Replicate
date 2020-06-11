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
		private List<FileOverviewModel> _originalFileList;

		[Parameter]
		public List<FileOverviewModel> Files { get; set; }

		[Parameter]
		public EventCallback OnCloseClicked { get; set; }

		protected override void OnParametersSet()
		{
			_originalFileList = Files;
		}


		protected void SearchFile(string searchTerm)
		{
			if (string.IsNullOrWhiteSpace(searchTerm))
				Files = _originalFileList;
			else
				Files = _originalFileList.Where(x => x.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
		}

	}
}
