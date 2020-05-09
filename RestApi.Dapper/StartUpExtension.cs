// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using MsSql.RestApi.DAO;
using RestApi.Dapper.Api;
using System;
using System.Data;
using System.Data.SqlClient;

namespace MsSql.RestApi
{
    public static class StartUpExtension
    {
        public static IServiceCollection AddDapperSqlConnection(this IServiceCollection services, 
                                                            string ConnString, Action<Option> init = null)
        {
            return AddSqlConnection(services, options =>
                                            {
                                                options.UseSqlServer(ConnString);
                                                if(init!=null) init(options);
                                            });
        }
        
        public static IServiceCollection AddSqlConnection(this IServiceCollection services, Action<Option> init)
        {
            Option options = new Option();
            init(options);

            switch (options.ServiceScope)
            {
                case Option.ServiceScopeEnum.SCOPED:
                    services.AddScoped<TSqlCommand>(sp => new TSqlCommandAdapter(options.ConnString));
                    services.AddScoped<IDbConnection>(sp => new SqlConnection(options.ConnString));
                    break;
                case Option.ServiceScopeEnum.SINGLETON:
                    services.AddSingleton<TSqlCommand>(sp => new TSqlCommandAdapter(options.ConnString));
                    services.AddSingleton<IDbConnection>(sp => new SqlConnection(options.ConnString)); 
                    break;
                case Option.ServiceScopeEnum.TRANSIENT:
                default:
                    services.AddTransient<TSqlCommand>(sp => new TSqlCommandAdapter(options.ConnString));
                    services.AddTransient<IDbConnection>(sp => new SqlConnection(options.ConnString));
                    break;
            }


            return services;
        }
    }

    public class Option
    {

        public string ConnString;
        public bool ReadScaleOut = false;
        public string ReadOnlyConnString;
        public void UseSqlServer(string ConnString) => this.ConnString = ConnString;
        public bool EnableRetryLogic = true;
        public bool EnableDelayedRetryLogic = false;
        public bool EnableODataExtensions = true;

        public enum ServiceScopeEnum { SINGLETON, SCOPED, TRANSIENT };

        public ServiceScopeEnum ServiceScope = ServiceScopeEnum.SCOPED;

    }
}
