// Copyright (c) Jovan Popovic. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using TSql.RestApi;

namespace TSql.OData
{
    /// <summary>
    /// Controller class that should be used to expose OData REST API with minimal metadata.
    /// </summary>
    public abstract class ODataController : Controller
    {
        /// <summary>
        /// Url that will be placed in XML metadata.
        /// </summary>
        public virtual string MetadataUrl
        {
            get {
                return this.Request.Scheme + "://" + this.Request.Host + "/" + this.MetadataPath;
            }
        }

        public abstract string MetadataPath { get; }

        public virtual string ModelNamespace
        {
            get
            {
                return this.ControllerContext.ActionDescriptor.ControllerName + ".Models";
            }
        }

        public abstract TableSpec[] GetTableSpec { get; }

        // see https://services.odata.org/TripPinRESTierService/(S(0rl14bktppv5tp5hy3obiftc))/
        //[Produces("application/json; odata.metadata=minimal")]
        [HttpGet]
        public ActionResult Root()
        {
            return this.GetODataServiceDocumentJsonV4(this.GetTableSpec, "$metadata");
            //this.Response.Headers.Add("OData-Version", "4.0");
            //return this.Content(ODataHandler.GetRootMetadataJsonV4(this.MetadataUrl, this.GetTableSpec), 
            //    "application/json; odata.metadata=minimal");
        }

        [HttpGet("[controller]/$metadata")]
        public ActionResult Metadata()
        {
            return this.GetODataMetadataXmlV4(this.GetTableSpec);
            //return base.Content(ODataHandler.GetMetadataXmlV4(this.GetTableSpec, this.ModelNamespace), "application/xml");
        }
    }
}
