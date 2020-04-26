using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;

namespace Pi.Replicate.Application.Files.Queries.GetFailedTransmissions
{
    public class GetFailedFileTransmissionsCommand : IRequest<ICollection<FailedFile>>
    {

    }

    public class GetFailedFileTransmissionsCommandHandler : IRequestHandler<GetFailedFileTransmissionsCommand, ICollection<FailedFile>>
    {
        private readonly IDatabase _database;
		//todo query
		private const string _selectStatement = @"SELECT fi.Id, fi.FolderId, fi.Version, fi.LastModifiedDate, fi.Name, fi.Path, fi.Signature, fi.Size, fi.Status, fi.Source 
					FROM dbo.[File] fi
					INNER JOIN dbo.FailedTransmission ftn on ftn.FileId = fi.id
					WHERE fi.Source = 0";

        public GetFailedFileTransmissionsCommandHandler(IDatabase database)
		{
            _database = database;
        }
        public Task<ICollection<FailedFile>> Handle(GetFailedFileTransmissionsCommand request, CancellationToken cancellationToken)
        {
            using(_database)
				return _database.Query<Folder,File,Recipient,FailedFile>(_selectStatement,null
				,(fo,fi,re) => new FailedFile{ File = fi, Folder = fo, Recipient = re }
				);
        }
    }
}