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





namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    #region
    using System;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAFarmAntivirusSettingsOperation : CAOperation
    {
        // Summary:
        //     Gets or sets a value that indicates whether infected documents can be downloaded.
        //
        // Returns:
        //     true to allow infected documents to be downloaded; otherwise, false. The
        //     default is false.

        [DataMember]
        public Boolean AllowDownload { get; set; }

        [DataMember]
        public Boolean AllowQuarantinedFileDownload { get; set; }
        //
        // Summary:
        //     Gets or sets a value that indicates whether the virus scanner should attempt
        //     to cure infected files.
        //
        // Returns:
        //     If true, the virus scanner attempts to cure infected files; otherwise, false.
        //     The default is false.
        [DataMember]
        public Boolean CleaningEnabled { get; set; }
        //
        // Summary:
        //     Gets or sets a value that indicates whether files are scanned when they are
        //     downloaded.
        //
        // Returns:
        //     If true, files are scanned when they are downloaded; otherwise, false. The
        //     default is false.
        [DataMember]
        public Boolean DownloadScanEnabled { get; set; }
        //
        // Summary:
        //     Gets or sets the number of threads that the virus scanner can use.
        //
        // Returns:
        //     A 32-bit integer that specifies the number of threads. The default is 5.
        Int32 numberOfThreads = 5;
        [DataMember]
        public Int32 NumberOfThreads { get { return numberOfThreads; } set { numberOfThreads = value; } }
        //
        // Summary:
        //     Gets or sets a Boolean value that specifies whether to skip scanning during
        //     a search crawl.
        //
        // Returns:
        //     true to skip skanning during a search crawl; otherwise, false.

        [DataMember]
        public Boolean SkipSearchCrawl { get; set; }
        //
        // Summary:
        //     Gets or sets the amount of time before the virus scanner times out.
        //
        // Returns:
        //     A TimeSpan object. The default is 300 seconds.

        Int32 timeoutSeconds = 300;
        [DataMember]
        public Int32 TimeoutSeconds { get { return timeoutSeconds; } set { timeoutSeconds = value; } }
        //
        // Summary:
        //     Gets or sets a value that indicates whether files are scanned when uploaded.
        //
        // Returns:
        //     If true, files are scanned when they are uploaded; otherwise, false. The
        //     default is false.
        [DataMember]
        public Boolean UploadScanEnabled { get; set; }
        //
        // Summary:
        //     Gets or sets the current increment of the number of times the vendor has
        //     been updated.
        //
        // Returns:
        //     A 32-bit integer that specifies the number of times the vendor has been updated.
        [DataMember]
        public Int32 VendorUpdateCount { get; set; }
    }
}
