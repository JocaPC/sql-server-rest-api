using Belgrade.SqlClient;
using Belgrade.SqlClient.SqlDb;
using Microsoft.Extensions.DependencyInjection;
using System.Data.SqlClient;

namespace SqlServerRestApi.SQL
{
    public static class StartUp
    {
        public static IServiceCollection AddSqlClient(this IServiceCollection services, string ConnString)
        {
            // Adding data access services/components.
            services.AddScoped<IQueryPipe>(
                sp => new QueryPipe(new SqlConnection(ConnString)));

            services.AddScoped<ICommand>(
                sp => new Command(new SqlConnection(ConnString)));

            return services;
        }
    }
}
