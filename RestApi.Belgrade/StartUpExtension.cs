// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Belgrade.SqlClient;
using Belgrade.SqlClient.SqlDb;
using Belgrade.SqlClient.SqlDb.Rls;
using Common.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TSql.RestApi.Util;
using RestApi.Belgrade.Api;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace TSql.RestApi
{
    public static class StartUpExtension
    {
        public static IServiceCollection AddBelgradeSqlClient(this IServiceCollection services, string ConnString, Action<Option> init = null)
        {
            return AddBelgradeSqlClient(services, options =>
                                            {
                                                options.UseSqlServer(ConnString);
                                                init?.Invoke(options);
                                            });
        }
        
        public static IServiceCollection AddBelgradeSqlClient(this IServiceCollection services, Action<Option> init)
        {
            Option options = new Option();
            init(options);

            switch (options.ServiceScope) {
                case Option.ServiceScopeEnum.SCOPED:
                    services.AddScoped<TSqlCommand>(sp => new TSqlCommandAdapter(CreateCommand(options, sp)));
                    services.AddScoped<IQueryPipe>(sp => CreateQueryPipe(options, sp));
                    services.AddScoped<IQueryMapper>(sp => CreateQueryMapper(options, sp));
                    services.AddScoped<ICommand>(sp => CreateCommand(options, sp));
                    break;
                case Option.ServiceScopeEnum.SINGLETON:
                    services.AddSingleton<TSqlCommand>(sp => new TSqlCommandAdapter(CreateCommand(options, sp))); 
                    services.AddSingleton<IQueryPipe>(sp => CreateQueryPipe(options, sp));
                    services.AddSingleton<IQueryMapper>(sp => CreateQueryMapper(options, sp));
                    services.AddSingleton<ICommand>(sp => CreateCommand(options, sp));
                    break;
                case Option.ServiceScopeEnum.TRANSIENT:
                default:
                    services.AddTransient<TSqlCommand>(sp => new TSqlCommandAdapter(CreateCommand(options, sp)));
                    services.AddTransient<IQueryPipe>(sp => CreateQueryPipe(options, sp));
                    services.AddTransient<IQueryMapper>(sp => CreateQueryMapper(options, sp));
                    services.AddTransient<ICommand>(sp => CreateCommand(options, sp));
                    break;
            }

            Belgrade.SqlClient.SqlDb.RetryErrorHandler.Enable(options.EnableRetryLogic);
            Belgrade.SqlClient.SqlDb.RetryErrorHandler.EnableDelayedRetries(options.EnableDelayedRetryLogic);
            

            return services;
        }

        private static IQueryMapper CreateQueryMapper(Option options, IServiceProvider sp)
        {
            var m = new QueryMapper(new SqlConnection(options.ReadScaleOut? (options.ReadOnlyConnString?? (options.ConnString + "ApplicationIntent=ReadOnly;")): options.ConnString));
            var logger = TryGetLogger<QueryPipe>(sp);
            if (logger != null)
                m.AddLogger(logger);
            if (options.SessionContext != null && options.SessionContext.Count >= 1)
            {
                foreach (var rls in options.SessionContext)
                {
                    m.AddContextVariable(rls.Key, () => rls.Value(sp));
                }
            }
            return m;
        }

        private static IQueryPipe CreateQueryPipe(Option options, IServiceProvider sp)
        {
            var pipe = new QueryPipe(new SqlConnection(options.ReadScaleOut ? (options.ReadOnlyConnString ?? (options.ConnString + "ApplicationIntent=ReadOnly;")) : options.ConnString));
            var logger = TryGetLogger<QueryPipe>(sp);
            if (logger != null)
                pipe.AddLogger(logger);
            if (options.SessionContext != null && options.SessionContext.Count >= 1)
            {
                foreach (var rls in options.SessionContext)
                {
                    pipe.AddContextVariable(rls.Key, () => rls.Value(sp));
                }
            }
            return pipe;
        }

        private static ICommand CreateCommand(Option options, IServiceProvider sp)
        {
            var cmd = new Command(new SqlConnection(options.ConnString));
            var logger = TryGetLogger<QueryPipe>(sp);
            if (logger != null)
                cmd.AddLogger(logger);
            if (options.SessionContext != null && options.SessionContext.Count >= 1)
            {
                foreach (var rls in options.SessionContext)
                {
                    cmd.AddContextVariable(rls.Key, () => rls.Value(sp));
                }
            }
            return cmd;
        }

        static ILogManager _logManager = null;
        static ILoggerFactory _loggerFactory = null;
        internal static ILog GetLogger<T>() {
            if (_logManager != null)
                return _logManager.GetLogger<T>();
            else 
                if (_loggerFactory != null)
                    return new CommonILogAdapter4ExtensionILogger(_loggerFactory.CreateLogger<T>());
            return LogManager.GetLogger<T>();
        }
        
        private static Common.Logging.ILog TryGetLogger<T>(IServiceProvider sp)
        {   
            if (_logManager == null)
            {
                _logManager = sp.GetServices<ILogManager>().FirstOrDefault();
            } 
                if (_logManager == null && _loggerFactory == null)
                {
                    _loggerFactory = sp.GetServices<ILoggerFactory>().FirstOrDefault();
                }
            

            return GetLogger<T>();
        }
    }

    public class Option
    {

        public string ConnString;
        public bool ReadScaleOut = false;
        public string ReadOnlyConnString;
        public void UseSqlServer(string ConnString) => this.ConnString = ConnString;
        public bool EnableRetryLogic = false;
        public bool EnableDelayedRetryLogic = false;
        public bool EnableODataExtensions = true;

        public enum ServiceScopeEnum { SINGLETON, SCOPED, TRANSIENT };

        public ServiceScopeEnum ServiceScope = ServiceScopeEnum.TRANSIENT;

        public Dictionary<string, Func<IServiceProvider, string>> SessionContext = new Dictionary<string, Func<IServiceProvider, string>>();
    }
}
