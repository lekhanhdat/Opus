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
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Tree.Object;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExchangeOnlineUpgradeDto
    {
        [DataMember]
        public EITreeNodeDto Tree { get; set; }

        [DataMember]
        public PlatformType PlatformType { get; set; }

        [DataMember]
        public ProductVersion ProductVersion { get; set; }

        [DataMember]
        public string StoragePolicyId { get; set; }

        /// <summary>
        /// Job Detail显示信息,Control端赋值.
        /// </summary>
        [DataMember]
        public string StoragePolicyName { get; set; }

        [DataMember]
        public string NotificationProfileID { get; set; }

        /// <summary>Logical Device数据control后台赋值. </summary>
        [DataMember]
        public List<LogicalDeviceDto> LogicalDevices { get; set; }

        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public int JobType { get; set; }

        [DataMember]
        public string RunJobUserId { get; set; }

        [DataMember]
        public CacheSettingDto CacheLocation { get; set; }

        [DataMember]
        public long JobStartTime { get; set; }

        public ServiceDto MediaService { get; set; }
    }

}
