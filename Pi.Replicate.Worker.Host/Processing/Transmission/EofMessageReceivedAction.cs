using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Processing.Transmission
{
    public class EofMessageReceivedAction
    {
		private readonly IEofMessageRepository _eofMessageRepository;

		public EofMessageReceivedAction(IEofMessageRepository eofMessageRepository)
		{
			_eofMessageRepository = eofMessageRepository;
		}

		public async Task<bool> Execute(Guid fileId, int amountOfChunks)
		{
			var eofMessage = EofMessage.Build(fileId,amountOfChunks);
			var result = await _eofMessageRepository.AddReceivedEofMessage(eofMessage);
			return result.WasSuccessful;
		}
    }
}
