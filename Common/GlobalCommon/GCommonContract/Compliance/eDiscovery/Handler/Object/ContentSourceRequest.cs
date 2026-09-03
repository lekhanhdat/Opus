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



using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Handler.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ContentSourceRequest : EDiscoveryRequest
    {
        [DataMember]
        public string SSAId { get; set; }
        [DataMember]
        public string SSAName { get; set; }
        [DataMember]
        public SSAState SSAState { get; set; }
        [DataMember]
        public bool IsAvailable { get; set; }
        [DataMember]
        public bool AgentIsAvailable { get; set; }
        [DataMember]
        public string FarmId { get; set; }


        [DataMember]
        public ContentSourceDto ContentSource { get; set; }
        [DataMember]
        public CrawlType CrawlType { get; set; }
        [DataMember]
        public List<ContentSourceDto> ContentSourceList { get; set; }
        [DataMember]
        public ContentSourceAction ContentSourceAction { get; set; }


    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ContentSourceAction : uint
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        LoadContentSourceList = 1,
        [EnumMember]
        DeleteContentSource = 2,
        [EnumMember]
        SaveContentSourcePlan = 3,
        [EnumMember]
        SaveContentSourcePlanAndRunNow = 4,
        [EnumMember]
        SaveSchedule = 5,
        [EnumMember]
        SaveScheduleAndRunNow = 6,
        [EnumMember]
        StartCrawl = 9,
        [EnumMember]
        LoadContentSourceById = 11
    }
}
