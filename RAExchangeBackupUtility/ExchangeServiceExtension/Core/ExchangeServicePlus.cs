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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    [DebuggerNonUserCode]
    public class ExchangeServicePlus : ExchangeService
    {
        public ExchangeServicePlus() { }
        public ExchangeServicePlus(ExchangeVersion requestedServerVersion) : base(requestedServerVersion) { }
        public ExchangeServicePlus(TimeZoneInfo timeZone) : base(timeZone) { }
        public ExchangeServicePlus(ExchangeVersion requestedServerVersion, TimeZoneInfo timeZone) : base(requestedServerVersion, timeZone) { }

        /// <summary>
        /// Set HttpWebRequest.AllowWriteStreamBuffering = false, HttpWebRequest.SendChunked = true to avoid large memory usage while sending the request.
        /// Some operation does not support chunked encoding, be careful while using this option.
        /// </summary>
        internal bool DisableRequestBuffer
        {
            get
            {
                return this.HttpWebRequestFactory is BufferFreeEwsHttpWebRequestFactory;
            }
            set
            {
                if (value)
                {
                    this.HttpWebRequestFactory = new BufferFreeEwsHttpWebRequestFactory();
                }
                else
                {
                    //set backup to default factory
                    this.HttpWebRequestFactory = null;
                }
            }
        }


        #region Export Items

        /// <summary>
        /// Export Items, save export data in memory
        /// </summary>
        /// <param name="itemIds">export item id list</param>
        /// <returns></returns>
        public ServiceResponseCollection<ExportItemsResponse> ExportItems(IEnumerable<ItemId> itemIds)
        {
            var request = new ExportItemsRequest(this, ServiceErrorHandling.ReturnErrors);
            request.ItemIds.AddRange(itemIds);
            return request.Execute();
        }

        /// <summary>
        /// Export Items, save export data in file
        /// </summary>
        /// <param name="itemIds">export item id list</param>
        /// <param name="exportLocation">Directory path where the export data is saved.</param>
        /// <returns></returns>
        public ServiceResponseCollection<ExportItemsResponse> ExportItems(IEnumerable<ItemId> itemIds, string exportLocation)
        {
            var request = GetExportItemsRequestRequest(exportLocation);
            request.ItemIds.AddRange(itemIds);
            return request.Execute();
        }

        private ExportItemsRequest GetExportItemsRequestRequest(string exportLocation)
        {
            if (this.EnableSeekableResponseStreamCache)
            {
                return new ExportItemsRequestForLargeFile(this, exportLocation, ServiceErrorHandling.ReturnErrors);
            }
            else
            {
                return new ExportItemsRequest(this, exportLocation, ServiceErrorHandling.ReturnErrors);
            }
        }
        #endregion

        #region Import Items
        public ServiceResponseCollection<UploadItemsResponse> UploadItems(IEnumerable<UploadItemParameter> parameters)
        {
            try
            {
                DisableRequestBufferForLargeItem(parameters.Sum(p => p.DataSize));
                var request = new UploadItemsRequest(this, ServiceErrorHandling.ReturnErrors);
                request.UploadItemParameters.AddRange(parameters);
                return request.Execute();
            }
            finally
            {
                EnableRequestBuffer();
            }
        }

        private void DisableRequestBufferForLargeItem(long? totalSize)
        {
            const int LagerFileLimit = 50 * 1024 * 1024;
            if (totalSize.HasValue && totalSize.Value > LagerFileLimit)
            {
                this.DisableRequestBuffer = true;
            }
        }

        private void EnableRequestBuffer()
        {
            if (this.DisableRequestBuffer)
            {
                this.DisableRequestBuffer = false;
            }
        }
        #endregion


    }
}
