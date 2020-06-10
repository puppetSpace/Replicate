using Microsoft.AspNetCore.Components;
using Pi.Replicate.Application.FileConflicts.Queries.GetFileConflicts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pi.Replicate.Domain;
using Pi.Replicate.WebUi.Models;

namespace Pi.Replicate.WebUi.Pages.Folders.Components
{
    public class FileConflictDetailBase : ComponentBase
    {
		[Parameter]
		public FileModel File { get; set; } = new FileModel();

		[Parameter]
		public Guid FileConflictId { get; set; }

		[Parameter]
		public EventCallback<FileConflictDeleteArgument> OnDelete { get; set; }
	}
}
