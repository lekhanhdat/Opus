/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */

namespace Microsoft.Exchange.WebServices.Data
{
    using System;
    using System.Diagnostics;
    using System.IO;

    [DebuggerNonUserCode]
    class ExportItemsRequest : MultiResponseServiceRequest<ExportItemsResponse>
    {
        /// <summary>
        /// Items
        /// </summary>
        protected ItemIdWrapperList itemIds = new ItemIdWrapperList();
        protected string exportLocation;
        protected bool cacheFile;

        public ExportItemsRequest(ExchangeService service, ServiceErrorHandling errorHandlingMode) : base(service, errorHandlingMode) { }

        public ExportItemsRequest(ExchangeService service, string exportLocation, ServiceErrorHandling errorHandlingMode) 
            : this(service, errorHandlingMode)
        {
            if (string.IsNullOrEmpty(exportLocation)) throw new ArgumentNullException("exportLocation");
            if (!Directory.Exists(exportLocation)) throw new DirectoryNotFoundException(exportLocation);

            this.exportLocation = exportLocation;
            this.cacheFile = true;
        }
        internal ItemIdWrapperList ItemIds
        {
            get { return this.itemIds; }
        }
        internal override void Validate()
        {
            base.Validate();
            EwsUtilities.ValidateParamCollection(this.itemIds, "itemIds");
        }

        internal override ExportItemsResponse CreateServiceResponse(ExchangeService service, int responseIndex)
        {
            if (cacheFile)
            {
                string filePath = Path.Combine(this.exportLocation, Guid.NewGuid().ToString() + ".fts");
                return new FileExportItemsResponse(filePath);
            }
            return new MemoryExportItemsResponse();
        }

        internal override int GetExpectedResponseMessageCount()
        {
            return this.itemIds.Count;
        }

        internal override ExchangeVersion GetMinimumRequiredServerVersion()
        {
            return ExchangeVersion.Exchange2010_SP1;
        }

        internal override string GetResponseMessageXmlElementName()
        {
            return XmlElementNamesExtension.ExportItemsResponseMessage;
        }

        internal override string GetResponseXmlElementName()
        {
            return XmlElementNamesExtension.ExportItemsResponse;
        }

        internal override string GetXmlElementName()
        {
            return XmlElementNamesExtension.ExportItems;
        }

        internal override void WriteElementsToXml(EwsServiceXmlWriter writer)
        {
            this.ItemIds.WriteToXml(writer, XmlNamespace.Messages, XmlElementNames.ItemIds);
        }
    }
}
