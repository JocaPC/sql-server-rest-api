// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Net.Http;
using System.Text;

namespace SqlServerRestApi.OData
{

    public class UriParser
    {
        public static QuerySpec Parse (TableSpec tabSpec, HttpRequest Request)
        {
            var spec = new QuerySpec();
            if (Request.Query["$count"].Count == 1)
                throw new ArgumentException("Parameter $count is not supported.");
            if (Request.Query["$format"].Count == 1 && Request.Query["$format"].ToString().ToLower() != "json")
                throw new ArgumentException("Parameter $format is not supported.");
            if (Request.Query["$expand"].Count == 1)
                throw new ArgumentException("Parameter $expand is not supported.");
            if (Request.Query["$skiptoken"].Count == 1)
                throw new ArgumentException("Parameter $skiptoken is not supported.");

            spec.count = Request.Path.Value.EndsWith("/$count");
            if (Request.Query.ContainsKey("$skip"))
                spec.skip = Convert.ToInt32(Request.Query["$skip"]);
            if(Request.Query.ContainsKey("$top"))
                spec.top = Convert.ToInt32(Request.Query["$top"]);
            spec.select = Request.Query["$select"];
            spec.keyword = Request.Query["$search"];
            ParseSearch(Request.Query["$filter"], spec, tabSpec);
            ParseGroupBy(Request.Query["$apply"], spec, tabSpec);
            ParseOrderBy(tabSpec, Request.Query["$orderby"], spec);
            tabSpec.Validate(spec);
            return spec;
        }

        private static void ParseGroupBy(string apply, QuerySpec spec, TableSpec tabSpec)
        {
            if (string.IsNullOrEmpty(apply))
                return;
            var lexer = new ApplyTranslatorLexer(new AntlrInputStream(apply));
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            // Pass the tokens to the parser
            var parser = new ApplyTranslatorParser(tokens);

            // Run root rule ("apply" in this grammar)
            parser.apply();

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
            ParseOrderBy(tabSpec, parameters["$orderby"], spec);
            tabSpec.Validate(spec);
            return spec;
        }
#endif

        private static void ParseSearch(string filter, QuerySpec spec, TableSpec tabSpec)
        {
            if (!string.IsNullOrWhiteSpace(filter))
            {
                var lexer = new FilterTranslator(new AntlrInputStream(filter), tabSpec, spec);
                var predicate = new StringBuilder();
                while (!lexer.HitEOF)
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
                    var pair = colDir.Split(' ');
                    string column = pair[0].Trim();
                    tabSpec.HasColumn(column);
                    string dir = pair.Length == 1 ? "asc" : (pair[1].Trim() == "asc" ? "asc" : "desc");
                    spec.order.Add(column, dir);
                }
            }
        }
    }
}