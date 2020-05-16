// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using System;
using TSql.OData;
using TSql.TableApi;

namespace TSql.RestApi
{
    public static class RestApiControllerExtensions 
    {
        //private static ILog _log = null;
        public static RequestHandler OData(
            this Microsoft.AspNetCore.Mvc.Controller ctrl,
            TableSpec tableSpec,
            ODataHandler.Metadata metadata = ODataHandler.Metadata.NONE,
            string metadataUrl = null,
            object id = null)
        {
            return ctrl.Request.OData(tableSpec, ctrl.Response, metadata,
                metadataUrl: metadataUrl ??
                    ((ctrl is ODataController) ? (ctrl as ODataController).MetadataUrl : null) ??
                    ((ctrl.Request.Scheme + "://" + ctrl.Request.Host + ctrl.Request.Path.Value.Replace("/" + tableSpec.Name, ""))),
                id: id);
            #region Extracted & refactored
            /*
            if (_log == null)
                _log = StartUp.GetLogger<RequestHandler>();
            try
            {
                var querySpec = SqlServerRestApi.OData.UriParser.Parse(tableSpec, ctrl.Request);
                if (id != null)
                {
                    querySpec.predicate = tableSpec.primaryKey + " = @Id";
                    var p = new SqlParameter("Id", id);
                    if (querySpec.parameters == null)
                    {
                        querySpec.parameters = new System.Collections.Generic.LinkedList<SqlParameter>();
                    }
                    querySpec.parameters.AddFirst(p);
                }
                var sql = QueryBuilder.Build(querySpec, tableSpec);

                if (id != null)
                {
                    sql = sql.AsSingleJson();
                }
                else if (!querySpec.count)
                {
                    if (metadata == ODataHandler.Metadata.NONE)
                        sql = sql.AsJson("value");
                    else
                        sql = sql.AsJson();
                }

                return new ODataHandler(sql, sqlQuery, ctrl.Response, tableSpec,
                    metadataUrl ??
                    ((ctrl is ODataController) ? (ctrl as ODataController).MetadataUrl : null) ??
                    ((ctrl.Request.Scheme + "://" + ctrl.Request.Host + ctrl.Request.Path.Value.Replace("/" + tableSpec.Name, ""))), metadata,
                    countOnly: querySpec.count,
                    returnSingleResult: (id != null));
            } catch (Exception ex)
            {
                return new ErrorResponseHandler(ctrl.Response, ex);
            }
            */
            #endregion
        }

        public static ActionResult GetODataServiceDocumentJsonV4(
          this Microsoft.AspNetCore.Mvc.Controller ctrl,
          TableSpec[] tables,
          string MetadataPath)
        {
            ctrl.Response.Headers.Add("OData-Version", "4.0");
            return ctrl.Content(ODataHandler.GetRootMetadataJsonV4(
                                    ctrl.Request.Scheme + "://" + ctrl.Request.Host + "/" + MetadataPath,
                                    tables),
                                "application/json; odata.metadata=minimal");
        }

        public static ActionResult GetODataMetadataXmlV4(
            this Microsoft.AspNetCore.Mvc.Controller ctrl,
            TableSpec[] tables,
            string ModelNamespace = null)
        {
            if (string.IsNullOrWhiteSpace(ModelNamespace))
                ModelNamespace = ctrl.ControllerContext.ActionDescriptor.ControllerName + ".Models";
            ctrl.Response.Headers.Add("OData-Version", "4.0"); // Probably not nessecary but someone might need it as root.
            return ctrl.Content(ODataHandler.GetMetadataXmlV4(tables, ModelNamespace), "application/xml");
        }

        public static RequestHandler Table(
            this Microsoft.AspNetCore.Mvc.Controller ctrl,
            TableSpec tableSpec)
        {
            var querySpec = TSql.TableApi.UriParser.Parse(tableSpec, ctrl.Request);
            var sql = QueryBuilder.Build(querySpec, tableSpec);
            if (!querySpec.count)
                sql = sql.AsJson();
            return new JQueryDataTablesHandler(sql, ctrl.Request.Query["draw"].ToString(), Convert.ToInt32(ctrl.Request.Query["start"]), Convert.ToInt32(ctrl.Request.Query["length"]), ctrl.Response);
        }
    }
}