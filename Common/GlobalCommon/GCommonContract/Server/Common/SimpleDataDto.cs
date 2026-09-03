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




namespace AvePoint.GCommon.Contract.Server.Common
{
    #region == using directives ==
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.ContentManager.Object;
    #endregion ==

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SimpleDataDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// 该属性在Backup PlanManager页面Plan显示时,赋的值是PlanType. 
        /// 该属性在CM PlanManager页面Plan显示时,赋的值是Operation. 
        /// 注释：如果其它模块使用情况不一样，请添加相应使用注释说明
        /// </summary>
        [DataMember]
        public int Type { get; set; }

        [DataMember]
        public int PlanType { get; set; }
        /// <summary>
        /// 对应CM PlanManager中的Level属性.
        /// 只有CM模块有
        /// </summary>
        [DataMember]
        public GradeReuslt GradeReuslt { get; set; }

        [DataMember]
        public string FarmName { get; set; }

        /// <summary> Front desk show users farm name. </summary>
        [DataMember]
        public string FarmDisplayName { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int Category { get; set; }

        [DataMember]
        public long LastModifiedTime { get; set; }

        [DataMember]
        public bool IsBposPlan { get; set; }

        [DataMember]
        public List<NameAndIdDto> PlanGroups { get; set; }
    }
}
