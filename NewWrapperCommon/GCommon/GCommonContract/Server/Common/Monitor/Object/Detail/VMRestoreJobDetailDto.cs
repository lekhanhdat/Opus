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
using AvePoint.GCommon.Contract.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Detail
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class VMRestoreJobDetailDto : JobDetailDto
    {
        /// <summary>
        /// vm机器的机器名
        /// </summary>
        [DataMember]
        public string ServerName { get; set; }
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
        public string VMOperatingSystem { get; set; }
        [DataMember]
        public long StartTime { get; set; }
        [DataMember]
        public long FinishTime { get; set; }
        [DataMember]
        public string TotalTime { get; set; }
        [DataMember]
        public string AgentVersion { get; set; }

        /// <summary>
        /// file level restore source file name
        /// </summary>
        [DataMember]
        public string SourceFileName { get; set; }

        #region VM OOP Restore
        [DataMember]
        public string SourceIPOrHostName { get; set; }

        [DataMember]
        public string SourceHostType { get; set; }

        [DataMember]
        public string DestVMName { get; set; }

        //Clone VM Domain Name
        [DataMember]
        public string SourceDomainName { get; set; }
        [DataMember]
        public string DestDomainName { get; set; }
        #endregion VM OOP Restore
        //hyperV cluster group name
        [DataMember]
        public string HyperVGroupName { get; set; }
    }
}
