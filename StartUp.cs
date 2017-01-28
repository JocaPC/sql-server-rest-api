using Belgrade.SqlClient;
using Belgrade.SqlClient.SqlDb;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data.SqlClient;

namespace SqlServerRestApi
{
    public static class StartUp
    {
        public static IServiceCollection AddSqlClient(this IServiceCollection services, string ConnString, Action<Option> init = null)
        {
            if (init == null)
            {
                // Adding data access services/components.
                services.AddScoped<IQueryPipe>(
                    sp => new QueryPipe(new SqlConnection(ConnString)));

                services.AddScoped<ICommand>(
                    sp => new Command(new SqlConnection(ConnString)));
                return services;
            } else
            {
                return AddSqlClient(services, option => { init(option); option.ConnString = ConnString; });
            }
        }

        public static IServiceCollection AddSqlClient(this IServiceCollection services, Action<Option> init)
        {
            Option options = new Option();
            init(options);

            // Adding data access services/components.
            string QueryConnString = options.ConnString;
            if(options.ReadScaleOut){
                if(options.ReadOnlyConnString == null){
                    QueryConnString = options.ConnString + "ApplicationIntent=ReadOnly;";
                } else {
                    QueryConnString = options.ReadOnlyConnString;
                }
            }
            string ConnString = options.ConnString;

            switch (options.ServiceScope) {
                case Option.ServiceScopeEnum.SCOPED:
                    services.AddScoped<IQueryPipe>(
                       sp => new QueryPipe(new SqlConnection(QueryConnString)));
                    services.AddScoped<IQueryMapper>(
                        sp => new QueryMapper(new SqlConnection(QueryConnString)));
                    services.AddScoped<ICommand>(
                        sp => new Command(new SqlConnection(ConnString)));
                    break;
                case Option.ServiceScopeEnum.TRANSIENT:
                    services.AddTransient<IQueryPipe>(
                       sp => new QueryPipe(new SqlConnection(QueryConnString)));
                    services.AddTransient<IQueryMapper>(
                        sp => new QueryMapper(new SqlConnection(QueryConnString)));
                    services.AddTransient<ICommand>(
                        sp => new Command(new SqlConnection(ConnString)));
                    break;
                case Option.ServiceScopeEnum.SINGLETON:
                    services.AddSingleton<IQueryPipe>(
                       sp => new QueryPipe(new SqlConnection(QueryConnString)));
                    services.AddSingleton<IQueryMapper>(
                        sp => new QueryMapper(new SqlConnection(QueryConnString)));
                    services.AddSingleton<ICommand>(
                        sp => new Command(new SqlConnection(ConnString)));
                    break;
            }
            return services;
        }
    }

    public class Option {

        public string ConnString;
        public bool ReadScaleOut = false;
        public string ReadOnlyConnString;

        public enum ServiceScopeEnum { SINGLETON, SCOPED, TRANSIENT };

        public ServiceScopeEnum ServiceScope = ServiceScopeEnum.SCOPED;
    }
}
