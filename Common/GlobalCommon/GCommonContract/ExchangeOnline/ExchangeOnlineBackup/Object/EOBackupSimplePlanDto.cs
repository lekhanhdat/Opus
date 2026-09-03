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


namespace AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup.Object
{
    #region == using directives ==
    using System;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Tree.Object;
    #endregion ==

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EOBackupSimplePlanDto
    {
        /// <summary> 选择的tree结构,从Farm开始，不包括Root </summary>
        [DataMember]
        public SPTreeNodeDto Tree { set; get; }

        /// <summary>
        /// 用来表示是哪些模块调用了backup,没有的话，需要自己添加
        /// </summary>
        [DataMember]
        public EOBackupPlanType Type { get; set; }

        [DataMember]
        public FarmDto Farm { get; set; }
        [DataMember]
        public ServiceGroupDto AgentGroup { get; set; }
        [DataMember]
        public StoragePolicyDto StoragePolicy { get; set; }

        /// <summary>
        /// 调用了backup功能模块的job的id，此id保存到backup job中
        /// </summary>
        [DataMember]
        public String RelatedJobId { get; set; }

        /// <summary>
        /// 标识是否使用了office365功能,item还未实现，先传入false
        /// </summary>
        [DataMember]
        public bool IsBPOS { get; set; }

        /// <summary>
        /// 标识那个功能调用的backup
        /// </summary>
        [DataMember]
        public PlanCategory PlanCategory { get; set; }

        [DataMember]
        public RunJobMode JobMode { get; set; }

        [DataMember]
        public string RunJobUser { get; set; }

        /// <summary>
        /// 用来处理同一个外部job多次调Backup的情况，外部job初次调Backup时该属性为空，创建backup plan
        /// 以后改外部job再调backup时，该属性是第一次创建的backup plan的ID，更新该ID标识的plan
        /// </summary>
        [DataMember]
        public string BackupPlanId { get; set; }

        [DataMember]
        public bool IncludeVersions { get; set; }
    }
}
