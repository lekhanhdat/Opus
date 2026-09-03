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
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Handler.Object;
using AvePoint.GCommon.Contract.Server.Common;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EDPlanDto : PlanDto
    {
        /// <summary>
        /// sharepoint search 用的条件
        /// </summary>
        [DataMember]
        public QueryMessage SPSearchCondition { get; set; }

        /// <summary>
        /// Full Text Index 用的条件
        /// </summary>
        [DataMember]
        public EDFullTextIndexSearchRequest ArchiveSearchCondition { get; set; }

        /// <summary>
        /// 是否包含hold操作
        /// </summary>
        [DataMember]
        public bool NeedHold { get; set; }

        /// <summary>
        /// 是否包含export操作
        /// </summary>
        [DataMember]
        public bool NeedExport { get; set; }


        [DataMember]
        public List<HoldItemDto> HoldItems { get; set; }

        [DataMember]
        public EDExportLocationDto ExportLocation { get; set; }


    }
}
