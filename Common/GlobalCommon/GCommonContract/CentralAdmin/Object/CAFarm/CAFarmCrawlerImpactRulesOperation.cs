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





#region using directives
using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
#endregion

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAFarmCrawlerImpactRulesOperation : CAOperation
    {
        [DataMember]
        public List<CrawlerImpactRuleInfo> CrawlerImpactRules { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CrawlerImpactRuleInfo
    {
        [DataMember]
        public string Site { get; set; }
        [DataMember]
        public bool IsRequestLimitDocsChecked { get; set; }
        [DataMember]
        public int NumberLimitText { get; set; }
        [DataMember]
        public string Rules { get; set; }
        //这是取数据时的Order
        [DataMember]
        public int OldOrder { get; set; }
        //这是调整Order之后Set数据的Order
        [DataMember]
        public int NewOrder { get; set; }
        [DataMember]
        public bool IsEdited { get; set; }
    }
}
