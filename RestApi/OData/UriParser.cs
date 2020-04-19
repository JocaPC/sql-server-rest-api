// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Text;
using MsSql.RestApi;

namespace MsSql.OData
{
    public class UriParser
    {
        public static bool EnableODataExtensions { get; set; } = true;
        internal static Common.Logging.ILog _log { get; set; }
        private static BailErrorStrategy FastFail = new BailErrorStrategy();

        public static QuerySpec Parse (TableSpec tabSpec, HttpRequest Request)
        {
            if (_log == null)
                _log = StartUp.GetLogger<RequestHandler>();

            var spec = new QuerySpec();
            if (Request.Query["$format"].Count == 1 && Request.Query["$format"].ToString().ToLower() != "json")
            {
                if (_log != null) _log.ErrorFormat("Unsupported {parameter} {value} provided to {type} Uri parser", "$format", Request.Query["$format"], "OData");
                throw new ArgumentException("Parameter $format is not supported.");
            }
            foreach (var p in Request.Query.Keys)
            {
                if (p.StartsWith("$")
                    && !(p == "$select" || p == "$orderby" || p == "$format"
                        || p == "$apply" || p == "$systemat"
                        || p == "$top" || p == "$skip"
                        || p == "$filter" || p == "$search"
                        || p == "$expand"))
                {
                    if (_log != null) _log.ErrorFormat("Unsupported {parameter} {value} provided to {type} Uri parser", p, Request.Query[p], "OData");
                    throw new ArgumentException("Parameter " + p + " is not supported.");
                }
            }
            spec.count = Request.Path.Value.EndsWith("/$count");
            if (Request.Query.ContainsKey("$skip"))
                spec.skip = Convert.ToInt32(Request.Query["$skip"]);
            if(Request.Query.ContainsKey("$top"))
                spec.top = Convert.ToInt32(Request.Query["$top"]);
            spec.select = Request.Query["$select"];
            ParseExpand(Request.Query["$expand"], spec, tabSpec);
            spec.keyword = Request.Query["$search"];
            ParseSearch(Request.Query["$filter"], spec, tabSpec);
            ParseGroupBy(Request.Query["$apply"], spec, tabSpec);
            ParseOrderBy(tabSpec, Request.Query["$orderby"], spec);
            if (Request.Query.ContainsKey("$systemat"))
            {
                spec.systemTimeAsOf = Request.Query["$systemat"];
                DateTime asof;
                if (!DateTime.TryParse(spec.systemTimeAsOf, out asof))
                {
                    if (_log != null) _log.ErrorFormat("Invalid date {value} provided as {parameter} in {type} Uri parser", spec.systemTimeAsOf, "$systemat", "OData");
                    throw new ArgumentException(spec.systemTimeAsOf + " is not valid date.");
                }
            }
                
            tabSpec.Validate(spec);
            return spec;
        }

        private static void ParseExpand(StringValues expand, QuerySpec spec, TableSpec tabSpec)
        {
            if (string.IsNullOrEmpty(expand))
                return;
            
            var lexer = new ODataTranslatorLexer(new AntlrInputStream(expand), tabSpec, spec);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            // Pass the tokens to the parser
            var parser = new ODataTranslatorParser(tokens, tabSpec, spec);
            
            spec.expand = new System.Collections.Generic.Dictionary<string, QuerySpec>();
            // Run  rule "expandItems" in this grammar

            parser.expandItems();
            spec.expand = parser.Relations;
           
        }

        private static void ParseGroupBy(string apply, QuerySpec spec, TableSpec tabSpec)
        {
            if (string.IsNullOrEmpty(apply))
                return;
            var lexer = new ODataTranslatorLexer(new AntlrInputStream(apply),tabSpec, spec);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            // Pass the tokens to the parser
            var parser = new ODataTranslatorParser(tokens, tabSpec, spec);

            // Run root rule ("apply" in this grammar)
            parser.apply();
            if (parser.Aggregates.Count == 0)
            {
                _log.ErrorFormat("Cannot extract agregates from $apply= {apply} value. ", apply);
                throw new ArgumentException("Cannot parse $apply operator.", "$apply");
            }

            spec.select = parser.GroupBy;
            foreach (var a in parser.Aggregates)
            {
                spec.select += ((spec.select == null) ?"":",") + a.AggregateColumnAlias + "=" + a.AggregateMethod + "(" + a.AggregateColumn + ")";
            }
            spec.groupBy = parser.GroupBy;

        }
#if net46
        public static QuerySpec Parse(TableSpec tabSpec, HttpRequestMessage Request)
        {
            var spec = new QuerySpec();
            var parameters = Request.RequestUri.ParseQueryString();
            spec.count = (parameters["$count"] != null);
            spec.skip = Convert.ToInt32(parameters["$skip"]);
            spec.top = Convert.ToInt32(parameters["$top"]);
            spec.select = parameters["$select"];
            ParseSearch(parameters["$filter"], spec, tabSpec);
            ParseGroupBy(Request.Query["$apply"], spec, tabSpec);
            ParseOrderBy(tabSpec, parameters["$orderby"], spec);
            if (Request.Query.ContainsKey("$systemat"))
            {
                spec.systemTimeAsOf = Request.Query["$systemat"];
                DateTime asof;
                if (!DateTime.TryParse(spec.systemTimeAsOf, out asof))
                    throw new ArgumentException(spec.systemTimeAsOf + " is not valid date.");
            }
            tabSpec.Validate(spec);
            return spec;
        }
#endif

        private static void ParseSearch(string filter, QuerySpec spec, TableSpec tabSpec)
        {
            if (!string.IsNullOrWhiteSpace(filter))
            {
                var lexer = new ODataTranslatorLexer(new AntlrInputStream(filter), tabSpec, spec);
                var predicate = new StringBuilder();
                while (!lexer._hitEOF)
                {
                    var token = lexer.NextToken();
                    predicate.Append(token.Text);
                }
                spec.predicate = predicate.ToString();
            }
        }

        private static void ParseOrderBy(TableSpec tabSpec, string orderby, QuerySpec spec)
        {
            if (!string.IsNullOrWhiteSpace(orderby))
            {
                spec.order = new Hashtable();
                foreach (var colDir in orderby.Split(','))
                {
                    string dir = "asc", column = colDir;
                    if (colDir.EndsWith(" desc"))
                    {
                        dir = "desc";
                        column = colDir.Substring(0, colDir.Length - 5).Trim();
                    }
                    else if (colDir.EndsWith(" asc"))
                    {
                        column = colDir.Substring(0, colDir.Length - 4).Trim();
                    }

                    if (EnableODataExtensions)
                    {
                        var lexer = new ODataTranslatorLexer(new AntlrInputStream(column), tabSpec, spec);
                        CommonTokenStream tokens = new CommonTokenStream(lexer);
                        // Pass the tokens to the parser
                        var parser = new ODataTranslatorParser(tokens, tabSpec, spec);
                        parser.ErrorHandler = FastFail;
                        var orderBy = parser.orderBy();
                        column = orderBy.Expression + " " + orderBy.Direction;
                        if (string.IsNullOrWhiteSpace(column))
                        {
                            _log.ErrorFormat("Cannot extract order by clause from $orderby= {orderby} value. ", orderby);
                            throw new ArgumentException("Cannot parse $orderby parameter.", "$orderby");
                        }
                    } else
                    {
                        tabSpec.HasColumn(column);
                    }
                    spec.order.Add(column, dir);
                }
                spec.IsOrderClauseValidated = true;
            }
        }
    }
}