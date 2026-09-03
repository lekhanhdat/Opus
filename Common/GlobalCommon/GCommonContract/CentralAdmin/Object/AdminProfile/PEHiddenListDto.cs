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


using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PEHiddenListDto
    {
        [DataMember]
        public AdminRuleBasicInfo RuleInfo { get; set; }

        [DataMember]
        public string ProfileName { get; set; }

        /// <summary>
        /// 违规节点的URL
        /// </summary>
        [DataMember]
        public string URL { get; set; }

        [DataMember]
        public long HiddenDate { get; set; }

        [DataMember]
        public long ExpiredDate { get; set; }

        [DataMember]
        public List<string> Details { get; set; }

        //以下属性用于更新DB
        [DataMember]
        public string RuleId { get; set; }
        [DataMember]
        public string ViolationNodeId { get; set; }
        [DataMember]
        public Guid FarmId { get; set; }
        /// <summary>
        /// Auditor类型的Detail包含多条，需要更新每个Detail对应的数据
        /// </summary>
        [DataMember]
        public List<long> OccurredTimeTicks { get; set; }

        [DataMember]
        public AdminEventType TriggeredEventType { get; set; }
        [DataMember]
        public string ResultId { get; set; }
    }
}
