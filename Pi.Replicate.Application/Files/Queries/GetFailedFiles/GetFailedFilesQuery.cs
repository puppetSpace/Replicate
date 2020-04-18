using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Queries.GetFailedFiles
{
    public class GetFailedFilesQuery : IRequest<ICollection<FailedFile>>
    {
        
    }

    public class GetFailedFilesQueryHandler : IRequestHandler<GetFailedFilesQuery, ICollection<FailedFile>>
    {
        private readonly IDatabase _database;
        private const string _deleteStatement = "DELETE FROM dbo.FailedFile";
        private const string _selectStatement = @"
				select fi.Id, fi.FolderId,fi.AmountOfChunks, fi.Hash, fi.LastModifiedDate, fi.Name,fi.Path, fi.Signature,fi.Size, fi.Status
				, re.Id, re.Name, re.Address
				from dbo.FailedFile ff
				inner join dbo.File fi on fi.Id = ff.FileId
				inner join dbo.Recipient re on re.Id = ff.RecipientId";

        public GetFailedFilesQueryHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<ICollection<FailedFile>> Handle(GetFailedFilesQuery request, CancellationToken cancellationToken)
        {
            using (_database)
            {
                var failedFiles = await _database.Query<FailedFile, File, Recipient, FailedFile>(_selectStatement,null,
                    (ff, fi, re) =>
                    {
                        ff.File = fi;
                        ff.Recipient = re;
                        return ff;
                    });

                await _database.Execute(_deleteStatement, null);

                return failedFiles;
            }
        }
    }
}
