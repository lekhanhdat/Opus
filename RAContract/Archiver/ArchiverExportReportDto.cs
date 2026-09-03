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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Archiver
{
    [DataContract]
    public class ArchiverExportReportDto
    {
        [DataMember]
        [JsonProperty]
        public ReportType ReportType { get; set; }
        [DataMember]
        [JsonProperty]
        public int? ProfileId { get; set; }
        [DataMember]
        [JsonProperty]
        public List<ArchiverSiteSizeInfo> SiteInfos { get; set; }
        [DataMember]
        [JsonProperty]
        public TimeRange TimeRange { get; set; }
        [DataMember]
        [JsonProperty]
        public DateTime StartTime { get; set; }
        [DataMember]
        [JsonProperty]
        public DateTime EndTime { get; set; }
    }
    [DataContract]
    public enum ReportType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        SiteCollection = 1,
        [EnumMember]
        AllItem = 2,
        [EnumMember]
        AllSubSite = 4,
        [EnumMember]
        AllTeamsGroup = 5,
        [EnumMember]
        AllRetentionSimulate = 6,
        [EnumMember]
        AllGoogleDrive = 7,
        [EnumMember]
        AllGoogleItem = 8,
    }

    [DataContract]
    public enum TimeRange
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        All = 1,
        [EnumMember]
        Custom = 2
    }
}
