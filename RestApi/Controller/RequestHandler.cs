// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Belgrade.SqlClient;
using Common.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace MsSql.RestApi
{
    public class RequestHandler
    {
        protected SqlCommand cmd;
        protected HttpResponse response;
        protected bool IsSingletonResponse = false;

        internal RequestHandler(SqlCommand cmd, HttpResponse response, bool isSingleton = false)
        {
            this.cmd = cmd;
            this.response = response;
            this.IsSingletonResponse = isSingleton;
        }

        /// <summary>
        /// Process the current request and returns result using the target database.
        /// </summary>
        /// <param name="target">Connection string to the target database where results will be fetched.</param>
        /// <returns>Async task that will stream results.</returns>
        public virtual async Task Process(string target)
        {
            var pipe = new Belgrade.SqlClient.SqlDb.QueryPipe(target);
            await Process(pipe);
        }

        /// <summary>
        /// Process the current request and returns result using the target database.
        /// </summary>
        /// <param name="pipe">Sql Pipe that will be used to fetch the data.</param>
        /// <returns>Async task that will stream results.</returns>
        public virtual async Task Process(IQueryPipe pipe)
        {
            response.ContentType = "application/json";
            await pipe.Sql(cmd)
                .OnError(async e => await ReturnClientError(response, e))
                .Stream(response.Body, IsSingletonResponse?"{}":"[]")
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Created IActionResult object that contains the processed response result.
        /// </summary>
        /// <param name="target">Connection string to the target database where results will be fetched.</param>
        /// <returns>ContentResult with the data processed by request.</returns>
        public virtual async Task<IActionResult> GetResult(string target)
        {
            var mapper = new Belgrade.SqlClient.SqlDb.QueryMapper(target);
            return await GetResult(mapper);
        }

        /// <summary>
        /// Created IActionResult object that contains the processed response result.
        /// </summary>
        /// <param name="pipe">Sql Pipe that will be used to fetch the data.</param>
        /// <returns>ContentResult with the data processed by request.</returns>
        public virtual async Task<IActionResult> GetResult(IQueryMapper mapper)
        {
            IActionResult result = null;
            try
            {
                var json = await mapper
                    .GetString(cmd)
                    .ConfigureAwait(false);
                result = new ContentResult() {
                    Content = json,
                    StatusCode = StatusCodes.Status200OK,
                    ContentType = "application/json"
                };
            } catch (Exception ex)
            {
                result = new ContentResult()
                {
                    Content = ex.Message,
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

            return result;
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
                if(ex is ArgumentException ||
                    ex is ArgumentNullException ||
                    ex is ArgumentOutOfRangeException)
                    response.StatusCode = 400;
                else
                    response.StatusCode = 500;
            }
            catch (Exception ex1){
                if (log != null)
                {
                    log.ErrorFormat("Error {error} at {source} thrown while trying to set status code.\n{exception}", ex1.Message, ex1.Source, ex1);
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