using Pi.Replicate.Shared.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Data
{
	public interface IDatabase : IDisposable
	{
		IDbConnection Connection { get; }
		Task<Result<ICollection<TE>>> Query<TE>(string query, object parameters);
		Task<Result<ICollection<TResult>>> Query<TFirst, TSecond, TResult>(string query, object parameters, Func<TFirst, TSecond, TResult> map, string splitOn = "Id");
		Task<Result<ICollection<TResult>>> Query<TFirst, TSecond, TThird, TResult>(string query, object parameters, Func<TFirst, TSecond, TThird, TResult> map, string splitOn = "Id");
		Task<Result<TE>> QuerySingle<TE>(string query, object parameters);
		Task<Result> Execute(string query, object parameters);
		Task<Result<TE>> Execute<TE>(string query, object parameters);

	}

	public interface IDatabaseFactory
	{
		IDatabase Get();
	}
}
