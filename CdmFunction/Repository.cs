using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSql.RestApi;

namespace CdmFunction
{
    internal static class Repository
    {
        internal static string GetConnectionString()
        {
            return "Server=.;Database=WideWorldImporters;Integrated Security=True";
        }

        internal static TableSpec GetTable(string name)
        {
            return tables[name];
        }

        internal static TableSpec[] GetTables()
        {
            return tables.Values.ToArray();
        }

        static readonly IDictionary<string, TableSpec> tables = new Dictionary<string, TableSpec>()
        {
            {   "objects",
                new TableSpec("sys", "objects")
                .AddColumn("object_id", "int", isKeyColumn: true)
                .AddColumn("name", "nvarchar", 128)
                .AddColumn("type", "nvarchar", 20)
                .AddColumn("schema_id", "int")
            },
            {   "columns",
                new TableSpec("sys", "columns")
                    .AddColumn("object_id", "int", isKeyColumn: true)
                    .AddColumn("column_id", "int", isKeyColumn: true)
                    .AddColumn("name", "nvarchar", 128)
            },
            {   "parameters",
                new TableSpec("sys", "parameters")
                    .AddColumn("object_id", "int", isKeyColumn: true)
                    .AddColumn("name", "nvarchar", 128)
            }
        };
    }
}
