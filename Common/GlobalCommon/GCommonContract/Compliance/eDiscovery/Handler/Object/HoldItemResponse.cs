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
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Handler.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class HoldItemResponse : EDiscoveryResponse
    {
        [DataMember]
        public List<FarmDto> Farms { get; set; }
        [DataMember]
        public List<HoldItemDto> HoldItems { get; set; }
        //用于返回创建新的hold item
        [DataMember]
        public string HoldItemId { get; set; }

        [DataMember]
        public AddDocaveHoldItemResult AddResult { get; set; }

        [DataMember]
        //用于返回检测ManagedBy是否合法
        public bool IsAvailableManagedBy { get; set; }
        [DataMember]
        //用于返回Update Hold Ite是否成功
        public bool UpdateSuccessful { get; set; }
        [DataMember]
        //用于返回删除Hold Item，key是hold Item的id，value是表示成功与否
        public Dictionary<string, bool> DeleteResult { get; set; }
        [DataMember]
        public List<HeldFileDto> HeldFiles { get; set; }
        [DataMember]
        public List<string> ManagedByList { get; set; }

        /// <summary>
        /// Check Managed By Name,存在则返回修正的名字,不存在则返回strin.entity
        /// </summary>
        [DataMember]
        public string CheckManagedByName { get; set; }

         [DataMember]
        public SPTreeNodeDto CurrentFarmNode { get; set; }

    }



    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AddDocaveHoldItemResult
    {
        [EnumMember]
        Successful = 0,
        [EnumMember]
        Failed = 1,
        [EnumMember]
        Exist = 2
    }
}
