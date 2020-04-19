// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Common.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MsSql.RestApi.Util;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace MsSql.RestApi
{
    public class StartUp
    {    
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

}
