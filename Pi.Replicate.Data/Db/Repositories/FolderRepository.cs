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
		private readonly string _connectionString;

		public FolderRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		public async Task Create(Folder folder)
		{
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				await sqlConnection.ExecuteAsync(@"INSERT INTO dbo.Folders(Id,Name,FolderOptions_DeleteAfterSent) VALUES(@Id,@Name,@DeleteAfterSent", new { folder.Id, folder.Name, folder.FolderOptions.DeleteAfterSent });
				foreach (var recipient in folder.Recipients)
					await sqlConnection.ExecuteAsync(@"INSERT INTO dbo.FolderRecipient(FolderId,RecipientId) VALUES(@FolderId,RecipientId)", new { FolderId = folder.Id, RecipientId = recipient.Id });
			}
		}

		public async Task<bool> IsUnique(string name)
		{
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				var folderid = await sqlConnection.QueryFirstAsync<Guid>("SELECT Id FROM dbo.Folders WHERE Name = @Name", new { Name = name });
				return folderid != Guid.Empty;
			}
		}

		public async Task<ICollection<Folder>> Get()
		{
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				var folders = await sqlConnection.QueryAsync<Folder, FolderOption, Folder>("SELECT Id, Name, FolderOptions_DeleteAfterSent as DeleteAfterSent FROM dbo.Folders",
				(f, fo) =>
				{
					f.FolderOptions = fo;
					return f;
				}, splitOn: "DeleteAfterSent");
				var folderRecipients = await sqlConnection.QueryAsync<Guid, Recipient, (Guid folderId, Recipient recipient)>(@"
				select fr.FolderId, re.Id,re.Name,re.Address
				from dbo.FolderRecipient fr
				inner join dbo.Recipients re on re.Id = fr.RecipientId", (x, y) => (x, y));

				return folders
					.GroupJoin(folderRecipients, x => x.Id, (x) => x.folderId, (x, y) => { x.Recipients = y.Select(x => x.recipient).ToList(); return x; })
					.ToList();
			}
		}
	}
}
