using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.SystemSettings.Queries.GetSystemSettingsForConfiguration
{
	public class GetSystemSettingsForConfigurationQuery : IRequest<Result<ICollection<SystemSettingDto>>>
	{

	}

	public class GetSystemSettingsForConfigurationQueryHandler : IRequestHandler<GetSystemSettingsForConfigurationQuery, Result<ICollection<SystemSettingDto>>>
	{
		private readonly IDatabase _database;
		private const string _selectStatement = "SELECT [Key],[Value] FROM dbo.SystemSetting";

		public GetSystemSettingsForConfigurationQueryHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Result<ICollection<SystemSettingDto>>> Handle(GetSystemSettingsForConfigurationQuery request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				var queryResult = await _database.Query<SystemSettingDto>(_selectStatement, null);
				return Result<ICollection<SystemSettingDto>>.Success(queryResult);
			}
		}
	}
}
