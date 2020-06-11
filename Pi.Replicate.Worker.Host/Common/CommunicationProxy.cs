using Microsoft.AspNetCore.SignalR;
using Pi.Replicate.Shared.Models;
using Pi.Replicate.Worker.Host.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Common
{
    public class CommunicationProxy
    {
		private readonly IHubContext<SystemHub> _context;

		public CommunicationProxy(IHubContext<SystemHub> context)
		{
			_context = context;
		}

		public async Task SendNewFolderAddedNotification(FolderAddedNotification folderAddedNotification)
		{
			await _context.Clients.All.SendAsync("FolderAdded", folderAddedNotification);
		}
    }


	
}
