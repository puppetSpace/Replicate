using Dapper;
using Microsoft.Data.SqlClient;
using Pi.Replicate.Application.Common.Interfaces.Repositories;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Data.Db.Repositories
{
	public class FolderRepository : IFolderRepository
	{
		private SqlConnection _sqlConnection;

		public FolderRepository(SqlConnection sqlConnection)
		{
			_sqlConnection = sqlConnection;
		}

		public async Task Create(Folder folder)
		{
			await _sqlConnection.ExecuteAsync(@"INSERT INTO dbo.Folders(Id,Name,FolderOptions_DeleteAfterSent) VALUES(@Id,@Name,@DeleteAfterSent", new { folder.Id, folder.Name, folder.FolderOptions.DeleteAfterSent });
			foreach(var recipient in folder.Recipients)
				await _sqlConnection.ExecuteAsync(@"INSERT INTO dbo.FolderRecipient(FolderId,RecipientId) VALUES(@FolderId,RecipientId)", new { FolderId=folder.Id, RecipientId = recipient.Id });
		}

		public async Task<bool> IsUnique(string name)
		{
			var folderid = await _sqlConnection.QueryFirstAsync<Guid>("SELECT Id FROM dbo.Folders WHERE Name = @Name",new {Name = name });
			return folderid != Guid.Empty;
		}

		public async Task<ICollection<Folder>> Get()
		{
			var folders = await _sqlConnection.QueryAsync<Folder>("SELECT Id, Name, FolderOptions_DeleteAfterSent FROM dbo.Folders");
			var folderRecipients = await _sqlConnection.QueryAsync<Guid,Recipient,(Guid folderId,Recipient recipient)>(@"
				select fr.FolderId, re.Id,re.Name,re.Address
				from dbo.FolderRecipient fr
				inner join dbo.Recipients re on re.Id = fr.RecipientId", (x, y) =>(x,y));

			return folders
				.GroupJoin(folderRecipients, x => x.Id, (x) => x.folderId, (x, y) => { x.Recipients = y.Select(x => x.recipient).ToList(); return x; })
				.ToList();
		}
	}
}
