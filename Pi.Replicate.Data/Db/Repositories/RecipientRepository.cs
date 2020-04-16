using Microsoft.Data.SqlClient;
using Pi.Replicate.Application.Common.Interfaces.Repositories;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Pi.Replicate.Data.Db.Repositories
{
    public class RecipientRepository : IRecipientRepository
    {
		private readonly string _connectionString;

		public RecipientRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		public async Task Create(Recipient recipient)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                await sqlConnection.ExecuteAsync("INSERT INTO dbo.Recipient(Id,Name,Address) VALUES(@Id,@Name,@Address", new { recipient.Id, recipient.Name, recipient.Address });
            }
        }

        public async Task<ICollection<Recipient>> GetRecipients()
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                var result = await sqlConnection.QueryAsync<Recipient>("SELECT Id,Name,Address FROM dbo.Recipients");
                return result.ToList();
            }
        }

        public async Task<ICollection<Recipient>> GetRecipientsForFolder(Guid folderId)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                var result = await sqlConnection.QueryAsync<Recipient>(@"
			SELECT re.Id,re.Name,re.Address 
			FROM dbo.Recipients re
			INNER JOIN dbo.FolderRecipients fr on fr.RecipientsId = re.Id and fr.FolderId = @FolderId",
            new { FolderId = folderId });
                return result.ToList();
            }
        }

        public async Task<bool> IsAddressUnique(string address)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                var result = await sqlConnection.QueryFirstAsync<Guid>("SELECT Id FROM dbo.Recipients where address = @Address", new { Address = address });
                return result != Guid.Empty;
            }
		}

        public async Task<bool> IsNameUnique(string name)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                var result = await sqlConnection.QueryFirstAsync<Guid>("SELECT Id FROM dbo.Recipients where name = @Name", new { Name = name });
                return result != Guid.Empty;
            }
        }
    }
}
