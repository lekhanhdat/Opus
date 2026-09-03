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
    public class EDExecutePlanRequest : EDiscoveryRequest
    {

        #region 必要的属性

        /// <summary>
        /// job 的id
        /// </summary>
        [DataMember]
        public string JobId { get; set; }


        [DataMember]
        public ActionEnum Action { get; set; }


        /// <summary>
        /// 如果本次是保存最后一批Search Result数据到磁盘
        /// 那么NextDo附上非None的值，表示接下来要做什么操作
        /// 默认是None，表示什么都不做
        /// </summary>
        [DataMember]
        public NextDoEnum NextDo { get; set; }

        #endregion



        #region 保存Search Result到磁盘用到的属性

        /// <summary>
        /// 要保存到磁盘上的search result
        /// </summary>
        [DataMember]
        public List<SearchResult> SearchResults { get; set; }

        #endregion



        #region 查询磁盘Search Result用到的属性


        /// <summary>
        /// 查询起始位置
        /// </summary>
        [DataMember]
        public long StartIndex { get; set; }


        /// <summary>
        /// 想要查询的条数
        /// </summary>
        [DataMember]
        public long Count { get; set; }


        #endregion



        [DataContract(Namespace = ContractConstants.Namespace)]
        public enum ActionEnum
        {
            [EnumMember]
            SaveResults = 0,
            [EnumMember]
            LoadResults = 1
        }


        [DataContract(Namespace = ContractConstants.Namespace)]
        public enum NextDoEnum
        {
            [EnumMember]
            None = 0,
            [EnumMember]
            Hold = 1,
            [EnumMember]
            Export = 2,
            [EnumMember]
            both = 3
        }


    }
}
