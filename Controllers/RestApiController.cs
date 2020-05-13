using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TSql.RestApi;

namespace MyApp.Controllers
{
    public class RestApiController : Controller
    {
        private readonly TSqlCommand DbCommand;
        private readonly ILogger _logger;

        public RestApiController(TSqlCommand command, ILogger<RestApiController> logger)
        {
            this.DbCommand = command;
            this._logger = logger;
        }
        
        public async Task Objects()
        {
            await this.DbCommand
                        .Sql("select top 1 * from sys.objects for json path, without_array_wrapper")
                        .Stream(this.Response.Body);
        }

        public async Task Columns()
        {
            await this.DbCommand
                        .Sql("select top 10 * from sys.columns for json path")
                        .Stream(this.Response.Body);
        }

        public async Task Parameters()
        {
            await this.DbCommand
                        .Sql("select system_type_id, cnt = count(*) from sys.parameters group by system_type_id for json path")
                        .Stream(this.Response.Body);
        }
    }
}