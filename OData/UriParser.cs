// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime;
using Microsoft.AspNetCore.Http;
using SqlServerRestApi.SQL;
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
            spec.count = (Request.Query["$count"].Count == 1);
            spec.skip = Convert.ToInt32(Request.Query["$skip"]);
            spec.top = Convert.ToInt32(Request.Query["$top"]);
            spec.select = Request.Query["$select"];
            spec.keyword = Request.Query["$search"];
            ParseSearch(Request.Query["$filter"], spec, tabSpec);
            ParseOrderBy(tabSpec, Request.Query["$orderby"], spec);
            tabSpec.Validate(spec);
            return spec;
        }

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

        private static void ParseSearch(string filter, QuerySpec spec, TableSpec tabSpec)
        {
            if (!string.IsNullOrWhiteSpace(filter))
            {
                var lexer = new FilterTranslator(new AntlrInputStream(filter), tabSpec, spec);
                var predicate = new StringBuilder();
                while (!lexer._hitEOF)
                {
                    predicate.Append(lexer.NextToken().Text);
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