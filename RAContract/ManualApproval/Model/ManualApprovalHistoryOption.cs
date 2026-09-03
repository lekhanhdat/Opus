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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.ManualApproval.Model
{
    [DataContract]
    public class ManualApprovalHistoryOption
    {
        [DataMember]
        public int LatestExportType { get;set; }
        [DataMember]
        public ManualHistoryCustomDataTime CustomDate { get;set; }
        [DataMember]
        public string ServiceUrl { get;set; }
        [DataMember]
        public string LogonUserId { get; set; }

        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string FullPath { get; set; }

        [DataMember]
        public string PartitionKeyId { get; set; }

        [DataMember]
        public List<ManualApprovalFilterDefinition> Filters { get; set; } = new List<ManualApprovalFilterDefinition>();

        #region Myhub JPMC
        [DataMember]
        public string DisplayName { get; set; }
        #endregion

    }
    [DataContract]
    public class ManualHistoryCustomDataTime
    {
        [DataMember]
        public DateTime StartDateTime { get; set; }
        [DataMember]
        public DateTime EndDateTime { get; set; }
        [DataMember]
        public long StartDateTimeTicks { get; set; }
        [DataMember]
        public long EndDateTimeTicks { get; set; }
        [DataMember]
        public string TimeZoneId { get; set; }

        [DataMember]
        public bool IsDaylight { get; set; }
    }
}
