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

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Object.HoldManager
{
    using System.Runtime.Serialization;
    /// <summary>
    /// 用来同步被Hold的item/Files
    /// </summary>
    [DataContract]
    [Obsolete("不再使用，使用SyncHoldMessage，重构时修改SyncHoldNameMessage")]
    public class SyncHoldFilesMessage : HoldBaseMessage
    {
        [DataMember]
        public string FarmId { get; set; }
        //schedule同步时选的web app的名字
        [DataMember]
        public string WebApplicationId { get; set; }
        //server hold表中存储的关于所选web app包含的site collection集合

        [DataMember]
        public string WebApplicationName { get; set; }

        [DataMember]
        public string PlanId { get; set; }

        [DataMember]
        public List<Guid> SiteIds { get; set; }
        [DataMember]
        public string JobId { get; set; }
        //用来发送进度的
        [DataMember]
        public string SubJobId { get; set; }
    }

    //[DataContract]
    //public class HoldItemDto
    //{
    //    [DataMember]
    //    public Guid HoldItemId { get; set; }
    //    [DataMember]
    //    public List<Guid> HoldFileIdList { get; set; }
    //}
}
