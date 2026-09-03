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
    class ExportItemsRequestForLargeFile : ExportItemsRequest
    {
        public ExportItemsRequestForLargeFile(ExchangeService service, string exportLocation, ServiceErrorHandling errorHandingMode) :
            base(service, exportLocation, errorHandingMode)
        {

        }

        internal override void Validate()
        {
            base.Validate();
            if (this.ItemIds.Count > 1) throw new ArgumentException("Cannot export more than 1 item if EnableSeekableResponseStreamCache is true.");
        }

        internal override ExportItemsResponse CreateServiceResponse(ExchangeService service, int responseIndex)
        {
            string filePath = Path.Combine(this.exportLocation, Guid.NewGuid().ToString() + ".fts");
            return new ExportItemsResponseForLargeFile(filePath, this.exportLocation);
        }

        //internal override EwsServiceXmlReader CreateEwsServiceXmlReader(Stream responseStream, ExchangeService service)
        //{
        //    if (base.ItemIds.Count > 1) throw new InvalidOperationException("Cannot export more than 1 item.");
        //    return new EwsServiceXmlReaderV2(CreateSeekableStream(responseStream), service);
        //}


        protected override Stream CreateSeekableStream(Stream responseStream)
        {
            string filePath = Path.Combine(this.exportLocation, Guid.NewGuid().ToString() + ".rsp");
            var target = new SelfCleanableFileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
            return target;
        }

        class SelfCleanableFileStream : FileStream
        {
            private string path;

            public SelfCleanableFileStream(string path, FileMode fileMode, FileAccess fileAccess)
                : base(path, fileMode, fileAccess)
            {
                this.path = path;
            }

            public override void Close()
            {
                base.Close();
                SafeDeleteFile();
            }

            private void SafeDeleteFile()
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
                catch { }
            }
        }
    }
}
