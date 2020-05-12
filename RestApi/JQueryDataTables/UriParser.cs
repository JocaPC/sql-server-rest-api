// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using System;
using System.Collections;

namespace MsSql.TableApi
{
    [Obsolete("use TSql.RestApi namespace")]
    public class UriParser : TSql.TableApi.UriParser { }
}

namespace TSql.TableApi
{
    using TSql.RestApi;
    public class UriParser
    {
        public static QuerySpec Parse (TableSpec tabSpec, HttpRequest Request)
        {
            var spec = new QuerySpec();
            spec.skip = Convert.ToInt32(Request.Query["start"]);
            spec.top = Convert.ToInt32(Request.Query["length"]);
            spec.select = null;//JQuery datatables don't provide column list.
            ParseSearch(Request, spec);
            ParseOrderBy(tabSpec, Request, spec);
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

        private static void ParseSearch(HttpRequest Request, QuerySpec spec)
        {
            spec.keyword = Request.Query["search[value]"].ToString();
            spec.columnFilter = new Hashtable();
            int i = 0;
            
            while (Request.Query[$"columns[{i}][searchable]"].Count != 0)
            {
                if (Request.Query[$"columns[{i}][searchable]"][0] == "true")
                {
                    spec.columnFilter.Add(
                        Request.Query[$"columns[{i}][data]"][0].ToString(),
                        Request.Query[$"columns[{i}][search][value]"][0].ToString());
                }
                i++;
            }
        }

        private static void ParseOrderBy(TableSpec tabSpec, HttpRequest Request, QuerySpec spec)
        {
            spec.order = new Hashtable();
            int i = 0;
            while (Request.Query[$"order[{i}][column]"].Count != 0)
            {
                spec.order.Add(
                        tabSpec.columns[
                            Convert.ToInt16(
                                Request.Query[$"order[{i}][column]"][0].ToString())].Name,
                        Request.Query[$"order[{i}][dir]"] == "asc" ? "asc" : "desc"
                    );
                i++;
            }
        }
    }
}