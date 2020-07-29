using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TSql.RestApi;

namespace SynapseCDM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CdmController : Controller
    {
        private readonly ILogger<CdmController> logger;
        private readonly TSqlCommand command;
        public CdmController(TSqlCommand command, ILogger<CdmController> logger)
        {
            this.logger = logger;
            this.command = command;
        }

        static readonly IDictionary<string, TableSpec> tables = new Dictionary<string, TableSpec>()
        {
            /*
            {   "external_data_sources",
                new TableSpec("sys", "external_data_sources")
                .AddColumn("name", "nvarchar", 128, true)
                .AddColumn("location", "nvarchar", 512)
                .AddColumn("credential_id", "int")
            },
            {   "views",
                new TableSpec("sys", "views")
                    .AddColumn("schema_id", "int")
                    .AddColumn("name", "nvarchar", 128, true)
            },
            {   "external_tables",
                new TableSpec("sys", "external_tables")
                    .AddColumn("schema_id", "int")
                    .AddColumn("name", "nvarchar", 128, true)
            }
            */
        };

        public ActionResult Index()
        {
            return OData();
        }

        [HttpGet("OData")]
        public ActionResult OData()
        {
            return this.GetODataServiceDocumentJsonV4(tables.Values.ToArray(), MetadataPath: "api/cdm/odata/$metadata");
        }


        [HttpGet("odata/{entity}")]
        public async Task Entity(string entity)
        {
            if (string.IsNullOrEmpty(entity))
                throw new System.Exception("Entity name is required");

            if (entity == "$metadata")
            {
                var ModelNamespace = "CDM.Models";
                var metadata = ODataHandler.GetMetadataXmlV4(tables.Values.ToArray(), ModelNamespace);
                this.Response.Headers.Add("Content-Type", "application/xml");
                await this.Response.WriteAsync(metadata);
                return;
            }

            TableSpec table = null;

            if (!tables.ContainsKey(entity))
                throw new System.Exception($"Cannot find entity {entity}. You need to load it from database using api/cdm/add/{{schema}}/{entity} or register it api/cdm/registerModel?entity={entity}&modelPath=<URI of model JSON>");
            else
                table = tables[entity];

            await this.OData(table, metadata: ODataHandler.Metadata.MINIMAL).Process(this.command);
        }

        [HttpGet("add/{schema}/{entity}")]
        public async Task<ActionResult> AddEntity(string entity, string schema = "dbo")
        {
            if (string.IsNullOrEmpty(entity))
                throw new System.Exception("Entity name is required");
            if (string.IsNullOrEmpty(schema))
                throw new System.Exception("Schema is required");

            await this.LoadTable(entity, schema);
            return OData();
        }


        [HttpGet("RegisterModel")]
        public async Task<ActionResult> RegisterModel(string entity, string modelPath, string schema = "dbo" )
        {
            if (string.IsNullOrEmpty(entity))
                throw new System.Exception("Entity name is required");
            if (string.IsNullOrEmpty(modelPath))
                throw new System.Exception("Model.json URI must be specified in ?modelPath= parameter.");

            string query =
@"declare @view nvarchar(100) = @schema+'.'+@entity;

SET QUOTED_IDENTIFIER OFF;

declare @json nvarchar(max);
declare @sqlGetModelJson nvarchar(max) = ""
select @model_json = c.value
from openrowset(bulk '""+@root+""',
FORMAT = 'CSV',
        FIELDTERMINATOR = '0x0b', 
        FIELDQUOTE = '0x0b', 
        ROWTERMINATOR = '0x0b'
) WITH(value varchar(max)) c;
"";
EXECUTE sp_executesql
    @sqlGetModelJson
    ,N'@model_json nvarchar(max) OUTPUT'
    ,@model_json = @json OUTPUT;

if(@json is null or LEN(@json) < 1)
    THROW 51000, 'Model.json cannot be loaded.', 1;  

declare @columns nvarchar(max) = '',
        @openrowset nvarchar(max) = '',
        @sql nvarchar(max) = '',
        @mapping nvarchar(max) = '';

            --TODO: Replace with STRING_AGG
select
        @columns += IIF(name IS NULL, '', (quotename(name) + ',')), 
        @mapping += IIF(name IS NULL, '', (quotename(name) + ' ' +
         CASE dataType
            WHEN 'int64' THEN 'bigint'
            WHEN 'int32' THEN 'int'
            WHEN 'dateTime' THEN 'datetime2'
            WHEN 'datetimeoffset' THEN 'datetimeoffset'
            WHEN 'decimal' THEN 'decimal'
            WHEN 'double' THEN 'float'
            WHEN 'boolean' THEN 'varchar(5)'-- > True or False
            WHEN 'string' THEN 'nvarchar(max)'
            WHEN 'guid' THEN 'uniqueidentifier'
            WHEN 'json' THEN 'nvarchar(max)'
            ELSE dataType
         END + ','))
from openjson(@json, '$.entities') e
   cross apply openjson(e.value, '$.attributes')
with(name sysname, dataType sysname)
where json_value(e.value, '$.name') = @entity;

if(@columns is null or LEN(@columns) < 1)
    THROW 51000, 'Entity cannot be loaded from Model.json.', 1;  


SET @columns = TRIM(',' FROM @columns);
SET @mapping = TRIM(',' FROM @mapping);

with locations as (
            select location, [format], hasColumnHeader, delimiter
            from openjson(@json, '$.entities') e
               cross apply openjson(e.value, '$.partitions')
with(location nvarchar(4000),
        [format] nvarchar(50) '$.fileFormatSettings.""$type""',
        [hasColumnHeader] bit '$.fileFormatSettings.columnHeaders',
        delimiter nvarchar(1) '$.fileFormatSettings.delimiter')
where json_value(e.value, '$.name') = @entity
)
select *
into #files
from locations;

declare @domain varchar(max);
set @domain = (
            select top 1 SUBSTRING(f.location, 0, charindex(t.value, f.location)) + t.value
from #files f
    cross apply string_split(f.location, '/') as t
group by charindex(t.value, f.location), t.value, SUBSTRING(f.location, 0, charindex(t.value, f.location))
having count(*) = (select count(*) from #files)
order by charindex(t.value, f.location) desc
);

declare @filelist varchar(max) = ''
select @filelist += '''' + REPLACE(location, '.dfs.core.windows.net', '.blob.core.windows.net') + ''','
from #files;

set @filelist = TRIM(' ,' FROM @filelist);

--Grouping by wildcards
with t1 as (
    select
            path = location,
            file_name = SUBSTRING(location, LEN(@domain) + 1, 8000),
            [format], hasColumnHeader, delimiter
    from #files
),
t2 as (
select[format], hasColumnHeader, delimiter,
         pattern = @domain
          +REPLACE(REPLACE(REPLACE(TRANSLATE(file_name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-', '#####################################'), '/#', '/*'), '#', ''), '_', '') + '*'
from t1)
select distinct pattern, [format], hasColumnHeader, delimiter
into #groups
from t2;
with rowsets as (
            select rs = CONCAT(' OPENROWSET( BULK ''', pattern, ''',
                                       FORMAT = ''', CASE [format] WHEN 'CsvFormatSettings' THEN 'CSV' ELSE 'PARQUET' END ,''',
                                       FIRSTROW = ',IIF(hasColumnHeader=1, '2', '1'),',
                                       FIELDTERMINATOR = ''',delimiter,''', ROWS_PER_BATCH = 10000)')
from #groups
)
--TODO: Replace with STRING_AGG
SELECT  @sql += ' UNION ALL 
SELECT ' + @columns + '
        FROM ' + rowsets.rs + '
        WITH(' +
        @mapping + ') as cdm
WHERE cdm.filepath() IN(' + @filelist +')'
FROM rowsets;

            if (SUBSTRING(@sql, 1, 10) = ' UNION ALL')
                SET @sql = SUBSTRING(@sql, 12, 4000000);

            set @sql = 'CREATE OR ALTER VIEW ' + @view + ' AS ' + @sql;
            EXEC(@sql)
            --SELECT[XML_F52E2B61 - 18A1 - 11d1 - B105 - 00805F49916B] = @sql
            --print @sql
";
            try
            {
                //query = "create or alter view a as select name from sys.objects";
                await this.command
                    .Sql(query)
                    .Param("entity", entity)
                    .Param("schema", schema)
                    .Param("root", modelPath)
                    .OnError(ex =>
                    {
                        throw ex;
                    }
                    )
                    .Execute(null);
            } catch (Exception ex)
            {
                throw ex;
            }

            await LoadTable(entity, schema);

            return Redirect("/entities.html");
        }



        [HttpGet("GetModel")]
        public async Task GetModel(string entity, string modelPath)
        {
            if (string.IsNullOrEmpty(modelPath))
                throw new System.Exception("Model.json URI must be specified in ?modelPath= parameter.");

            string query =
@"SET QUOTED_IDENTIFIER OFF;

declare @json nvarchar(max);
declare @sqlGetModelJson nvarchar(max) = ""
select @model_json = c.value
from openrowset(bulk '""+@root+""',
FORMAT = 'CSV',
        FIELDTERMINATOR = '0x0b', 
        FIELDQUOTE = '0x0b', 
        ROWTERMINATOR = '0x0b'
) WITH(value varchar(max)) c;
"";
EXECUTE sp_executesql
    @sqlGetModelJson
    ,N'@model_json nvarchar(max) OUTPUT'
    ,@model_json = @json OUTPUT;

if(@json is null or LEN(@json) < 1)
    THROW 51000, 'Model.json cannot be loaded.', 1;  

SELECT @json;

";
            try
            {
                //query = "create or alter view a as select name from sys.objects";
                await this.command
                    .Sql(query)
                    .Param("root", modelPath)
                    .Stream(this.Response.Body);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private async Task LoadTable(string entity, string schema)
        {
            var generator = new TableSpecGenerator(this.command, schema, entity);
            if (tables.ContainsKey(entity))
                tables.Remove(entity);
            var table = await generator.GetTable();
            tables.Add(entity, table);
        }
    }
}
