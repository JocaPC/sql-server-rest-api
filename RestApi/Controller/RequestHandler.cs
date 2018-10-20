// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Belgrade.SqlClient;
using Common.Logging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
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

        public virtual async Task Process(bool useDefaultContentType = true)
        {
            if(useDefaultContentType) response.ContentType = "application/json";
            await pipe.Sql(cmd)
                .OnError(async e => await ReturnClientError(response, e))
                .Stream(response.Body, IsSingletonResponse?"{}":"[]")
                .ConfigureAwait(false);
        }

        internal static async Task ReturnClientError(HttpResponse response, Exception ex)
        {
            ILog log = StartUp.GetLogger<RequestHandler>();
            if (log != null)
            {
                log.ErrorFormat("Error {error} at {source} thrown while processing request.\n{exception}", ex.Message, ex.Source, ex);
            }
            try
            {
                response.StatusCode = 500;
            }
            catch (Exception ex1){
                if (log != null)
                {
                    log.ErrorFormat("Error {error} at {source} thrown while trying to set status code 500.\n{exception}", ex1.Message, ex1.Source, ex1);
                }
            };
            try
            {
                var error = new JsonErrorPayoad()
                {
                    code = (ex is SqlException) ? (ex as SqlException).Number : -1,
                    message = new JsonErrorPayoad.MessagePayload()
                    {
                        value = ex.Message,
                        innererror = (ex.InnerException != null)? ex.InnerException.Message : null
                    }
                };
                string json = JsonConvert.SerializeObject(error);
                await response.WriteAsync(json).ConfigureAwait(false);
            }
            catch (Exception ex2){
                if (log != null)
                {
                    log.ErrorFormat("Error {error} at {source} thrown while trying to dispose request body.\n{exception}", ex2.Message, ex2.Source, ex2);
                }
            };
        }
    }

    public class JsonErrorPayoad
    {
        public class MessagePayload {
            public string lang = "en";
            public string value = null;
            public string innererror = null;
        };
        public int? code;
        public MessagePayload message;
    } 
}