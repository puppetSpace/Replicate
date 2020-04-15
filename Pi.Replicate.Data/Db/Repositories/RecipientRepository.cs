using Microsoft.Data.SqlClient;
using Pi.Replicate.Application.Common.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Data.Db.Repositories
{
	public class RecipientRepository : IRecipientRepository
	{
		private SqlConnection _sqlConnection;

		public RecipientRepository(SqlConnection sqlConnection)
		{
			_sqlConnection = sqlConnection;
		}
	}
}
