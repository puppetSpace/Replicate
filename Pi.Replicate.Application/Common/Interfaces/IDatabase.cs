using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Common.Interfaces
{
    public interface IDatabase :IDisposable
    {
        IDbConnection Connection { get; }
        Task<ICollection<TE>> Query<TE>(string query, object parameters);
        Task<ICollection<TResult>> Query<TFirst,TSecond,TResult>(string query, object parameters, Func<TFirst, TSecond, TResult> map,string splitOn = "Id");
        Task<ICollection<TResult>> Query<TFirst,TSecond,TThird,TResult>(string query, object parameters, Func<TFirst, TSecond, TThird, TResult> map, string splitOn = "Id");
        Task<TE> QuerySingle<TE>(string query, object parameters);
        Task Execute(string query, object parameters);
        Task<TE> Execute<TE>(string query, object parameters);

	}

    public interface IDatabaseFactory
    {
        IDatabase Get();
    }
}
