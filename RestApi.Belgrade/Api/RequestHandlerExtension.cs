using Belgrade.SqlClient;
using Belgrade.SqlClient.SqlDb;
using Microsoft.AspNetCore.Mvc;
using RestApi.Belgrade.Api;
using System.Threading.Tasks;

namespace TSql.RestApi
{
    public static class BgRequestHandlerExtension
    {
        public static Task Process(this RequestHandler rh, ICommand cmd)
        {
            var pipe = new TSqlCommandAdapter(cmd);
            return rh.Process(pipe);
        }

        /// <summary>
        /// Process the current request and returns result using the target database.
        /// </summary>
        /// <param name="connection">Connection string to the target database where results will be fetched.</param>
        /// <returns>Async task that will stream results.</returns>
        public static Task Process(this RequestHandler rh, string connection)
        {
            var pipe = new QueryPipe(connection);
            var pipeAdapter = new TSqlCommandAdapter(pipe);
            return rh.Process(pipeAdapter);
        }

        /// <summary>
        /// Returns results from <code>RequestHandler</code> as single string.
        /// </summary>
        /// <param name="connection">Connection string to the target database where results will be fetched.</param>
        /// <returns>Async tatsk with ActionResult contianing the results.</returns>
        public static Task<IActionResult> GetResultString(this RequestHandler rh, string connection)
        {
            var pipe = new QueryPipe(connection);
            var pipeAdapter = new TSqlCommandAdapter(pipe);
            return rh.GetResult(pipeAdapter);
        }

        /// <summary>
        /// Returns text from <code>RequestHandler</code> as single string.
        /// </summary>
        /// <param name="connection">Connection string to the target database where results will be fetched.</param>
        /// <returns>Async tatsk with ActionResult contianing the results.</returns>
        public static Task<string> GetString(this RequestHandler rh, string connection)
        {
            var pipe = new QueryPipe(connection);
            var pipeAdapter = new TSqlCommandAdapter(pipe);
            return rh.GetString(pipeAdapter);
        }

    }
}
