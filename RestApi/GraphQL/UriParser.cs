// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using System;
using TSql.RestApi;

namespace TSql.GraphQL
{

#if NETSTANDARD2_0 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP3_0 || NETCOREAPP3_1

    using GraphQLParser;
    using GraphQLParser.AST;

    public class UriParser
    {
        public static QuerySpec Parse (TableSpec tabSpec, HttpRequest Request)
        {
            var spec = new QuerySpec();

            var parser = new Parser(new Lexer());
            GraphQLDocument document = parser.Parse(new Source(Request.Body.ToString()));
            
            /*
            GraphQLFieldSelection selection = GetSelection(document);
            BuildSelectExpandClause(spec, selection.SelectionSet);
            BuildFilterClause(spec, selection);
            LiftRequiredSingleNavigationPropertyFilter(spec);
            */





            spec.skip = Convert.ToInt32(Request.Query["start"]);
            spec.top = Convert.ToInt32(Request.Query["length"]);
            spec.select = null;//JQuery datatables don't provide column list.
            if (Request.Query.ContainsKey("$systemat"))
            {
                spec.systemTimeAsOf = Request.Query["$systemat"];
                DateTime asof;
                if (!DateTime.TryParse(spec.systemTimeAsOf, out asof))
                    throw new ArgumentException(spec.systemTimeAsOf + " is not valid date.");
            }
            tabSpec.Validate(spec, true);
            return spec;
        }

    }
#endif
}