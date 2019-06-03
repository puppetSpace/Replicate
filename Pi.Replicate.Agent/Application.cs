using Pi.Replicate.Processing.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Agent.Send
{
    public class Application
    {
		private readonly IRepository _repository;

		public Application(IRepository repository)
		{
			_repository = repository;
		}

		public async Task Run()
		{
			var folders = await _repository.FolderRepository.Get();
		}
    }
}
