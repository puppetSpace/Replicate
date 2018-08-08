using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors
{
    internal class FileObservableBuilder
    {
        private readonly IRepository _repository;

        public FileObservableBuilder(IRepository repository)
        {
            _repository = repository;
        }

        public FileCollector Build(Folder folder)
        {
            var observable = new FileCollector(folder, _repository);
            observable.Subscribe(async file=> 
            {
                var fileSplitter = new FileSplitter(file);
                fileSplitter.Subscribe(new FileChunkObserver(_repository));

                await fileSplitter.Split();
            });
            return observable;
        }
    }
}
