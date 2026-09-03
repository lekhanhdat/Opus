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



namespace AvePoint.GCommon.Contract.Media.TCPRequest.Restore
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    #endregion
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SolutionRestoreRequest : MediaTCPRequest
    {
        [DataMember]
        public CacheSettingDto CacheLocation { get; set; }
        [DataMember]
        public LogicalDeviceDto LogicalDevice { get; set; }
        /// <summary>
        /// key 为solution文件名，value为create time，如果用户没有指定create time，给默认值0，默认还原最新的数据
        /// </summary>
        [DataMember]
        public Dictionary<String, Int64> SolutionFiles { get; set; }

        public override String ToString()
        {
            return String.Format("Cache Location: {0}, Logical Device: {1}",
                this.CacheLocation,
                this.LogicalDevice);
        }
    }
}
