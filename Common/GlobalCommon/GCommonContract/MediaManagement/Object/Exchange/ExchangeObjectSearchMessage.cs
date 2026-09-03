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

namespace AvePoint.GCommon.Contract.MediaManagement.Object
{

    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExchangeObjectSearchMessage : AveMessage
    {
        [DataMember]
        public string JobId { get; set; }
        [DataMember]
        public string SubJobId { get; set; }
        
        
        /// <summary>
        /// Tree Node和下载Index db需要的信息的集合
        /// 一个ExchangeRestoreSearchContractDto代表一个mailbox的一个cycle。
        /// mailbox1:
        /// plan1(cycle1, cycle2)
        /// plan2(cycle3)
        /// mailbox2:
        /// plan1(cycle4, cycle5)
        /// </summary>
        [DataMember]
        public List<ExchangeRestoreSearchContractDto> SearchContracts { get; set; }

        /// <summary>
        /// Filter Policy
        /// </summary>
        [DataMember]
        public RestoreSearchFilterPolicy FilterPolicy { get; set; }

        [DataMember]
        public List<string> ExcludeJobIds { get; set; }
    }
}
