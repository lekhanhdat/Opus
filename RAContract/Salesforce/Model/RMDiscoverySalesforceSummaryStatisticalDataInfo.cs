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
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.Salesforce.Model
{
    [DataContract]
    public class RMDiscoverySalesforceSummaryStatisticalDataInfo
    {
        [DataMember]
        public int ObjectTotalCount { get; set; }
        [DataMember]
        public long RecordsTotalCount { get; set; }
        [DataMember]
        public string BiggestObjectByDataSize { get; set; }
        [DataMember]
        public int OldestRecords { get; set; }
        [DataMember]
        public long DataTotalSize { get; set; }
        [DataMember]
        public long DataStorageUsage { get; set; }
        [DataMember]
        public string BiggestObjectByRecordCount { get; set; }
        [DataMember]
        public long FileTotalSize { get; set; }
        [DataMember]
        public long FileStorageUsage { get; set; }
        [DataMember]
        public string BiggestObjectByFileSize { get; set; }
    }
}
