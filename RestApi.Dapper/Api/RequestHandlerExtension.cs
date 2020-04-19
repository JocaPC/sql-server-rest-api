using Microsoft.AspNetCore.Mvc;
using MsSql.RestApi;
using System.Data.SqlClient;
using System.Threading.Tasks;
using RestApi.Dapper.Api;

namespace MsSql.RestApi
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
