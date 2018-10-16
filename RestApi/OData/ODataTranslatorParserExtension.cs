using System;
using System.Collections.Generic;
using static SqlServerRestApi.OData.ODataTranslatorParser;

namespace SqlServerRestApi.OData
{
    public static class ODataTranslatorParserExtension
    {
        internal static void AddExpandRelation(this ODataTranslatorParser parser, string name, Dictionary<string, string> props)
        {
            if (parser.tableSpec.Relations == null)
                throw new System.Exception("Relations are not specified in the table.");
            if (!parser.tableSpec.Relations.ContainsKey(name))
                throw new System.Exception("Relation " + name + " is not found in the table.");
            TableSpec relation = parser.tableSpec.Relations[name];
            QuerySpec s = null;

            if (props != null || props.Count > 0)
            {
                var top = -1;
                var skip = -1;
                if ((props.ContainsKey("top") && props["top"] != null)
                    ||
                    (props.ContainsKey("skip") && props["skip"] != null)) // recover actual value that lexer sanitized and replaced with parameter.
                {
                    foreach(var p in parser.querySpec.parameters)
                    {
                        if (props.ContainsKey("top") && props["top"] != null && p.ParameterName == props["top"])
                        {
                            top = System.Convert.ToInt16(p.Value);
                        }
                        if (props.ContainsKey("skip") && props["skip"] != null && p.ParameterName == props["skip"])
                        {
                            skip = System.Convert.ToInt16(p.Value);
                        }

                        // if top is not provided or already set and if $skip is not provided or already set, skip the further search.
                        if ((!props.ContainsKey("top") || props["top"] == null || top != -1) 
                            &&
                            (!props.ContainsKey("skip") || props["skip"] == null || skip != -1))
                            break;
                    }
                }
                s = new QuerySpec()
                {
                    select = props.ContainsKey("select") ? props["select"] : relation.columnList,
                    top = top,
                    skip = skip,
                    predicate = props.ContainsKey("filter") ? props["filter"] : null
                };

                if (props.ContainsKey("orderBy"))
                {
                    s.order = new System.Collections.Hashtable();
                    s.order.Add(props["orderBy"], "");
                }
            }
            else
                s = new QuerySpec()
                {
                    select = relation.columnList
                };
            parser.Relations.Add(name, s);
        }

        internal static void AddAggregateExpression(this ODataTranslatorParser parser, string Method, string Column, string Alias)
        {
            var agg = new RestApi.OData.Aggregate() { AggregateMethod = Method, AggregateColumn = Column, AggregateColumnAlias = Alias };
            parser.Aggregates.AddLast(agg);
        }

        internal static void SetValidationScope(this ODataTranslatorParser parser, string tableName)
        {
            if (!parser.tableSpec.Relations.ContainsKey(tableName))
                throw new InvalidOperationException("Cannot validate entity " + tableName + " because it is not registered as a relation.");
            parser.currentTableScopeSpec = parser.tableSpec.Relations[tableName];
        }

        internal static void ResetValidationScope(this ODataTranslatorParser parser)
        {
            parser.currentTableScopeSpec = parser.tableSpec;
        }

        internal static void ValidateColumn(this ODataTranslatorParser parser, string name)
        {
            parser.currentTableScopeSpec.HasColumn(name);
        }
    }
}
