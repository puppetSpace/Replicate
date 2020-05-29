using AutoMapper;
using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.SystemSettings.Queries.GetSystemSettingOverview
{
	public class GetSystemSettingOverviewQuery : IRequest<Result<ICollection<SystemSettingViewModel>>>
	{

	}

	public class GetSystemSettingOverviewQueryHandler : IRequestHandler<GetSystemSettingOverviewQuery, Result<ICollection<SystemSettingViewModel>>>
	{
		private readonly IDatabase _database;
		private readonly IMapper _mapper;
		private const string _selectStatement = "SELECT Id,[Key],[Value],DataType,Description from dbo.SystemSetting";

		public GetSystemSettingOverviewQueryHandler(IDatabase database, IMapper mapper)
		{
			_database = database;
			_mapper = mapper;
		}

		public async Task<Result<ICollection<SystemSettingViewModel>>> Handle(GetSystemSettingOverviewQuery request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				var queryResult = await _database.Query<SystemSetting>(_selectStatement, null);
				return Result<ICollection<SystemSettingViewModel>>.Success(_mapper.Map<ICollection<SystemSettingViewModel>>(queryResult));
			}
		}
	}
}
