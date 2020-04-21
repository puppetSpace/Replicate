using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;

namespace Pi.Replicate.Application.Files.Commands.UpdateChangedFiles
{
    public class UpdateChangedFilesCommand : IRequest<ICollection<File>>
    {
        public ICollection<System.IO.FileInfo> Files { get; set; }
    }

    public class UpdateChangedFilesCommandHandler : IRequestHandler<UpdateChangedFilesCommand, ICollection<File>>
    {
        private readonly IDatabase _database;
        private readonly PathBuilder _pathBuilder;

        private const string _selectStatement = @"select Id, FolderId, AmountOfChunks, LastModifiedDate, Name,Path, Signature,Size,Status,Source from dbo.[File] where Path = @Path";
        private const string _updateStatement = @"Update dbo.[File] set Size = @Size, LastModifiedDate = @LastModifiedDate, Status = @Status where Id = @Id";

        public UpdateChangedFilesCommandHandler(IDatabase database, PathBuilder pathBuilder)
        {
            _database = database;
            _pathBuilder = pathBuilder;
        }

        public async Task<ICollection<File>> Handle(UpdateChangedFilesCommand request, CancellationToken cancellationToken)
        {
            var updatedFiles = new List<File>();
            foreach (var changedFile in request.Files)
            {
                var path = changedFile.FullName.Replace(_pathBuilder.BasePath + "\\", "");
                var foundFile = await _database.QuerySingle<File>(_selectStatement, new { Path = path });
                if (foundFile is null)
                {
                    Log.Information($"Unable to perform update action. No file found with path '{path}'");
                    continue;
                }

                foundFile.UpdateForChange(changedFile);
                await _database.Execute(_updateStatement, new {foundFile.Id, foundFile.Size ,foundFile.LastModifiedDate, foundFile.Status});
                updatedFiles.Add(foundFile);
            }

            return updatedFiles;
        }
    }
}