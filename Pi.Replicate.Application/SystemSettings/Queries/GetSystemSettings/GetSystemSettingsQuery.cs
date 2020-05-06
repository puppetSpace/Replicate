using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.SystemSettings.Queries.GetSystemSettings
{
    public class GetSystemSettingsQuery : IRequest<ICollection<SystemSetting>>
    {
        
    }

	public class GetSystemSettingsQueryHandler : IRequestHandler<GetSystemSettingsQuery, ICollection<SystemSetting>>
	{
		private readonly IDatabase _database;
		private const string _selectStatement = "SELECT Id,[Key],[Value],DataType,Info from dbo.SystemSetting";

		public GetSystemSettingsQueryHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<ICollection<SystemSetting>> Handle(GetSystemSettingsQuery request, CancellationToken cancellationToken)
		{
			using (_database)
				return await _database.Query<SystemSetting>(_selectStatement, null);
		}
	}
}
