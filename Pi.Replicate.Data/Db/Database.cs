using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Pi.Replicate.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Data.Db
{
    public class Database : IDatabase
    {

        public Database(IConfiguration configuration)
        {
            Connection = new SqlConnection(configuration.GetConnectionString("ReplicateDatabase"));
        }

        public IDbConnection Connection { get; }

        public Task Execute(string query, object parameters)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<TE>> Query<TE>(string query, object parameters)
        {
            throw new NotImplementedException();
        }

        public Task<TE> QuerySingle<TE>(string query, object parameters)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Connection?.Close();
        }

        public Task<ICollection<TResult>> Query<TFirst, TSecond, TResult>(string query, object parameters, Func<TFirst, TSecond, TResult> map, string splitOn = "Id")
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<TResult>> Query<TFirst, TSecond, TThird, TResult>(string query, object parameters, Func<TFirst, TSecond, TThird, TResult> map, string splitOn = "Id")
        {
            throw new NotImplementedException();
        }
    }

    public class DatabaseFactory : IDatabaseFactory
    {
        private readonly IConfiguration _configuration;

        public DatabaseFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDatabase Get()
        {
            return new Database(_configuration);
        }
    }
}
