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
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.JobMonitor;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.RMWeb.ReportCenter
{
    [RACodeReview("Allen Yin")]
    [DataContract]
    public class ShowProfilesReportPageInfo
    {
        [DataMember]
        public int PageIndex { get; set; }
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public int TotalCount { get; set; }
        [DataMember]
        public JobType Type { get; set; }
        [DataMember]
        public bool IsDesc { get; set; }
        [DataMember]
        public List<RMProfileDto> Profiles { get; set; }
        [DataMember]
        public string SearchValue { get; set; }
        
    }

    [RACodeReview("Allen Yin")]
    [DataContract]
    public class DelProfileInfo
    {
        [DataMember]
        public Dictionary<int, string> ProfileNames { get; set; }
        [DataMember]
        public List<int> Ids { get; set; }
        [DataMember]
        public List<string> Names { get; set; }
        [DataMember]
        public JobType Type { get; set; }
        [DataMember]
        public bool DeleteJobs { get; set; }
        
    }

    //public class ArchiverDBAndAgentInfo
    //{
    //    public StubDatabaseInfo dbInfo { get; set; }
    //    public string connconnectionStr { get; set; }
    //    public List<ServiceDto> agents { get; set; }
    //}
}
