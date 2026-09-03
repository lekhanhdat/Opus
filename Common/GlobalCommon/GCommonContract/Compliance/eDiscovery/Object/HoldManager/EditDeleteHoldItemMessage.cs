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

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Object.HoldManager
{
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
     [DataContract(Namespace = ContractConstants.Namespace)]
    public class EditDeleteHoldItemMessage : HoldBaseMessage
    {
        //        public HoldItemInfo OldHoldItemInfo { get; set; }
        //        public HoldItemInfo NewHoldItemInfo { get; set; }
        //        public ReturnResult Result { get; set; }
        //


        //要Edit的hold item 对象.
        [DataMember]
        public HoldItemDto HoldItem { get; set; }
        //要Delete 的hold item对象集合
        [DataMember]
        public List<HoldItemDto> HoldItems { get; set; }


        //Edit的返回结果
        [DataMember]
        public bool EditResult { get; set; }
        //Delete的返回结果集，key是hold item的id（不是sharepoint中的），value是成功与否
        [DataMember]
        public Dictionary<string,bool> DeleteResults  { get; set; }


    }

    
}
