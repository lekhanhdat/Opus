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



namespace AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Detail
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PlatformBackupJobDetailDto : JobDetailDto
    {
        [DataMember]
        public string IndexStatus { get; set; }

        [DataMember]
        public string VerifyStatus { get; set; }

        [DataMember]
        public string ServerName { get; set; }
        [DataMember]
        public string ServerType { get; set; }
        [DataMember]
        public string RuleName { get; set; }
        [DataMember]
        public string Checktarget { get; set; }
        [DataMember]
        public string Expected { get; set; }

        [DataMember]
        public string ParentBlobName { get; set; }
        [DataMember]
        public string FileName { get; set; }
        [DataMember]
        public string SourcePhysicalDevice { get; set; }

        /// <summary>
        /// time
        /// </summary>
        [DataMember]
        public long StartTime { get; set; }
        [DataMember]
        public long FinishTime { get; set; }
        [DataMember]
        public string TotalTime { get; set; }

        /// <summary>
        /// smsp maintenance details
        /// </summary>
        [DataMember]
        public string MaintenanceJobId { get; set; }
        [DataMember]
        public string MaintenanceContent { get; set; }
        [DataMember]
        public string MaintenanceActions { get; set; }
        [DataMember]
        public string GranularLevel { get; set; }

        //PRVM
        /// <summary>
        /// vm在vm tree上显示的名字
        /// </summary>
        [DataMember]
        public string VMName { get; set; }
        /// <summary>
        /// vm的ip或者hostname
        /// </summary>
        [DataMember]
        public string IPOrHostName { get; set; }
        /// <summary>
        /// vm机器的类型 见VMType类
        /// </summary>
        [DataMember]
        public string HostType { get; set; }
        /// <summary>
        /// vm 机器的操作系统
        /// </summary>
        [DataMember]
        public string VMOperatingSystem {get;set;}

        /// <summary>
        /// Customized VM标记
        /// </summary>
        [DataMember]
        public string CustomizedVM { get; set; }
        //hyperV cluster group name
        [DataMember]
        public string HyperVGroupName { get; set; }
    }
}
