using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Threading.Tasks;
using RestApi.Dapper.Api;
using System;

namespace TSql.RestApi
{
    public static class DapperRequestHandlerExtension
    {
        public static Task Process(this RequestHandler rh, SqlConnection conn)
        {
            var pipe = new TSqlCommandAdapter(conn);
            return rh.Process(pipe);
        }

        /// <summary>
        /// Process the current request and returns result using the target database.
        /// </summary>
        /// <param name="connection">Connection string to the target database where results will be fetched.</param>
        /// <returns>Async task that will stream results.</returns>
        public static Task Process(this RequestHandler rh, string connection)
        {
            var pipeAdapter = new TSqlCommandAdapter(new SqlConnection(connection));
            return rh.Process(pipeAdapter);
        }

        /// <summary>
        /// Returns results from <code>RequestHandler</code> as single string.
        /// </summary>
        /// <param name="connection">Connection string to the target database where results will be fetched.</param>
        /// <returns>Async tatsk with ActionResult contianing the results.</returns>
        public static Task<IActionResult> GetResultString(this RequestHandler rh, string connection)
        {
            var pipeAdapter = new TSqlCommandAdapter(new SqlConnection(connection));
            return rh.GetResult(pipeAdapter);
        }

    }
}


namespace MsSql.RestApi
{
    using TSql.RestApi;

    [Obsolete("Use TSql.RestApi namespace")]
    public static class DapperRequestHandlerExtension
    {
        [Obsolete("Use TSql.RestApi namespace")]
        public static Task Process(this RequestHandler rh, SqlConnection conn)
        {
            return TSql.RestApi.DapperRequestHandlerExtension.Process(rh, conn);
        }

        /// <summary>
        /// Process the current request and returns result using the target database.
        /// </summary>
        /// <param name="connection">Connection string to the target database where results will be fetched.</param>
        /// <returns>Async task that will stream results.</returns>
        [Obsolete("Use TSql.RestApi namespace")]
        public static Task Process(this RequestHandler rh, string connection)
        {
            return TSql.RestApi.DapperRequestHandlerExtension.Process(rh, connection);
        }

        /// <summary>
        /// Returns results from <code>RequestHandler</code> as single string.
        /// </summary>
        /// <param name="connection">Connection string to the target database where results will be fetched.</param>
        /// <returns>Async tatsk with ActionResult contianing the results.</returns>
        [Obsolete("Use TSql.RestApi namespace")]
        public static Task<IActionResult> GetResultString(this RequestHandler rh, string connection)
        {
            return TSql.RestApi.DapperRequestHandlerExtension.GetResultString(rh, connection);
        }
    }
}