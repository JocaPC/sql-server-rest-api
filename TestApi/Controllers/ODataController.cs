using Microsoft.AspNetCore.Mvc;
using TSql.OData;
using TSql.RestApi;

namespace TestApi.Controllers
{
    public class ODataController : Controller
    {
        private readonly TSqlCommand DbCommand;

        public ODataController(TSqlCommand command)
        {
            this.DbCommand = command;
        }

        [HttpGet("odata")]
        public async Task OData()
        {
            var tableSpec = new TableSpec("dbo", "Users").AddColumn("Id", "int", isKeyColumn: true).AddColumn("DisplayName", "nvarchar", isKeyColumn: false).AddColumn("EmailAddress", "nvarchar", isKeyColumn: false).AddColumn("OrgDefinedId", "nvarchar", isKeyColumn: false).AddColumn("ProfileBadgeUrl", "nvarchar", isKeyColumn: false).AddColumn("ProfileIdentifier", "nvarchar", isKeyColumn: false);
            await this
                    .OData(tableSpec)
                    .Process(DbCommand);
        }
    }
}
