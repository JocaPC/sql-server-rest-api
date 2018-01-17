// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Belgrade.SqlClient;
using Belgrade.SqlClient.SqlDb;
using Belgrade.SqlClient.SqlDb.Rls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

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

                services.AddScoped<IQueryMapper>(
                    sp => new QueryMapper(new SqlConnection(ConnString)));

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

            switch (options.ServiceScope) {
                case Option.ServiceScopeEnum.SCOPED:
                    services.AddScoped<IQueryPipe>(sp => CreateQueryPipe(options, sp));
                    services.AddScoped<IQueryMapper>(sp => CreateQueryMapper(options, sp));
                    services.AddScoped<ICommand>(sp => CreateCommand(options, sp));
                    break;
                case Option.ServiceScopeEnum.TRANSIENT:
                    services.AddTransient<IQueryPipe>(sp => CreateQueryPipe(options, sp));
                    services.AddTransient<IQueryMapper>(sp => CreateQueryMapper(options, sp));
                    services.AddTransient<ICommand>(sp => CreateCommand(options, sp));
                    break;
                case Option.ServiceScopeEnum.SINGLETON:
                    services.AddSingleton<IQueryPipe>(sp => CreateQueryPipe(options, sp));
                    services.AddSingleton<IQueryMapper>(sp => CreateQueryMapper(options, sp));
                    services.AddSingleton<ICommand>(sp => CreateCommand(options, sp));
                    break;
            }
            return services;
        }

        private static IQueryMapper CreateQueryMapper(Option options, IServiceProvider sp)
        {
            var m = new QueryMapper(new SqlConnection(options.ReadScaleOut? (options.ReadOnlyConnString?? (options.ConnString + "ApplicationIntent=ReadOnly;")): options.ConnString));
            if (options.SessionContext.Count >= 1)
            {
                var rls = options.SessionContext.First();
                m.AddRls(rls.Key, () => rls.Value(sp));
                if (options.SessionContext.Count > 1)
                    throw new Exception("Multiple session context variables are still not supported!");
            }
            return m;
        }

        private static IQueryPipe CreateQueryPipe(Option options, IServiceProvider sp)
        {
            var m = new QueryPipe(new SqlConnection(options.ReadScaleOut ? (options.ReadOnlyConnString ?? (options.ConnString + "ApplicationIntent=ReadOnly;")) : options.ConnString));
            if (options.SessionContext.Count >= 1)
            {
                var rls = options.SessionContext.First();
                m.AddRls(rls.Key, () => rls.Value(sp));
                if (options.SessionContext.Count > 1)
                    throw new Exception("Multiple session context variables are still not supported!");
            }
            return m;
        }

        private static ICommand CreateCommand(Option options, IServiceProvider sp)
        {
            var m = new Command(new SqlConnection(options.ConnString));
            if (options.SessionContext.Count >= 1)
            {
                var rls = options.SessionContext.First();
                m.AddRls(rls.Key, () => rls.Value(sp));
                if (options.SessionContext.Count > 1)
                    throw new Exception("Multiple session context variables are still not supported!");
            }
            return m;
        }


    }

    public class Option {

        public string ConnString;
        public bool ReadScaleOut = false;
        public string ReadOnlyConnString;

        public enum ServiceScopeEnum { SINGLETON, SCOPED, TRANSIENT };

        public ServiceScopeEnum ServiceScope = ServiceScopeEnum.SCOPED;

        public Dictionary<string, Func<IServiceProvider, string>> SessionContext = new Dictionary<string, Func<IServiceProvider, string>>(); 
    }
}
