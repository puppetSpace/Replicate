using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Pi.Replicate.Worker.Host.Processing.Transmission
{
    public class TransmissionActionFactory
    {
		private readonly IServiceProvider _serviceProvider;

		public TransmissionActionFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public FileReceivedAction GetForFileReceived()
		{
			return _serviceProvider.GetService<FileReceivedAction>();
		}

		public EofMessageReceivedAction GetForEofMessageReceived()
		{
			return _serviceProvider.GetService<EofMessageReceivedAction>();
		}

		public FileChunkReceivedAction GetForFileChunkReceived()
		{
			return _serviceProvider.GetService<FileChunkReceivedAction>();
		}
    }
}
