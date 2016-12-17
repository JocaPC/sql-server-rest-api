using Microsoft.AspNetCore.Http;
using SqlServerRestApi.SQL;
using System;
using System.Collections;

namespace SqlServerRestApi.JQueryDataTable
{
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
            tabSpec.Validate(spec, true);
            return spec;
        }

        private static void ParseSearch(HttpRequest Request, QuerySpec spec)
        {
            spec.keyword = Request.Query[$"[search][value]"].ToString();
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
                                Request.Query[$"order[{i}][column]"][0].ToString())],
                        Request.Query[$"order[{i}][dir]"] == "asc" ? "asc" : "desc"
                    );
                i++;
            }
        }
    }
}