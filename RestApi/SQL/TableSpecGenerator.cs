using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSql.RestApi
{
    public class TableSpecGenerator
    {
        public TableSpecGenerator(TSqlCommand cmd, string schema, string table)
        {
            this.Command = cmd;
            this.SchemaName = schema;
            this.TableName = table;
        }

        public TSqlCommand Command { get; }
        public string SchemaName { get; }
        public string TableName { get; }

        public async Task<TableSpec> GetTable()
        {
            TableSpec result = new TableSpec(this.SchemaName, this.TableName);

            string sql =
@"
select c.name, type = t.name, c.max_length
from sys.columns c
join sys.types t on c.user_type_id = t.user_type_id " +
$"where object_id = OBJECT_ID('{this.SchemaName}.{this.TableName}')";

            await this.Command.Sql(sql).Execute(row=> {
                result.AddColumn(row["name"].ToString(),
                    row["type"].ToString(),
                    Convert.ToInt32(row["max_length"]));
            });

            result.InferPrimaryKey();

            return result;
        }
    }
}
