// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Belgrade.SqlClient;
using Microsoft.AspNetCore.Http;
using System;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerRestApi
{
    public class RequestHandler
    {
        protected SqlCommand cmd;
        protected IQueryPipe pipe;
        protected HttpResponse response;
        protected bool IsSingletonResponse = false;

        internal RequestHandler(SqlCommand cmd, IQueryPipe pipe, HttpResponse response, bool isSingleton = false)
        {
            this.cmd = cmd;
            this.pipe = pipe;
            this.response = response;
            this.IsSingletonResponse = isSingleton;
        }

        public virtual RequestHandler OnError(Action<Exception> onErrorHandler)
        {
            pipe.OnError(onErrorHandler);
            return this;
        }

        public virtual async Task Process()
        {
            await pipe
                .OnError(e => ReturnClientError(response))
                .Stream(cmd, response.Body, IsSingletonResponse?"{}":"[]");
        }

        public virtual async Task Get()
        {
            response.ContentType = "application/json";
            await pipe
                .OnError(e => ReturnClientError(response))
                .Stream(cmd, response.Body, IsSingletonResponse ? "{}" : "[]");
        }

        protected void ReturnClientError(HttpResponse response)
        {
            try
            {
                response.StatusCode = 500;
            }
            catch { };
            try
            {
#if net46 || netcoreapp2_0
                response.Body.Close();
#else
                response.Body.Dispose();
#endif
            }
            catch { };
        }
    }
}