using Microsoft.AspNetCore.Components;
using Pi.Replicate.Application.Folders.Queries.GetFolderOverview;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Pages.Folders.Components
{
    public class RecipientTileBase : ComponentBase
    {
		[Parameter]
		public RecipientOverviewModel Recipient { get; set; }

		[Parameter]
		public EventCallback<Guid> Clicked { get; set; }

	}
}
