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
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRHisSubSiteMasterIndexDto
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string JobId { get; set; }
        [DataMember]
        public string PlanId { get; set; }
        [DataMember]
        public string PlanName { get; set; }
        [DataMember]
        public long BackupTime { get; set; }
        [DataMember]
        public string AgentHost { get; set; }
        [DataMember]
        public string LogicalDriveId { get; set; }
        [DataMember]
        public string PhysicalDriveId { get; set; }
        [DataMember]
        public string Location { get; set; }
        [DataMember]
        public int JobStatus { get; set; }
        [DataMember]
        public int IndexLevel { get; set; }
        [DataMember]
        public int IndexStatus { get; set; }
        [DataMember]
        public int PruneStatus { get; set; }
        [DataMember]
        public int SPVersion { get; set; }
        [DataMember]
        public string SecurityKey { get; set; }
        [DataMember]
        public string ContentDbGuid { get; set; }
        [DataMember]
        public string SiteUrlPathMd5 { get; set; }
    }
}
