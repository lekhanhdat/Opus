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
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class JobParallelSettingContent : ISystemSettingContent
    {
        /// <summary>
        /// tenant group的权值
        /// </summary>
        [XmlAttribute]
        [DataMember]
        public int TenantWeight { get; set; }

        /// <summary>
        /// job等待时间的权值
        /// </summary>
        [XmlAttribute]
        [DataMember]
        public int JobWeight { get; set; }

        /// <summary>
        /// 不同level的tenant group，允许运行job的最大个数
        /// </summary>
        [XmlElement]
        [DataMember]
        public List<TenantJobCount> MaxJobParallelTenant { get; set; }

        /// <summary>
        /// 每对agent/media可以运行的最大job数量
        /// </summary>
        [XmlAttribute]
        [DataMember]
        public int MaxParallel { get; set; }

        /// <summary>
        /// 系统Job队列里,最大job记录个数
        /// </summary>
        [XmlAttribute]
        [DataMember]
        public int MaxJobInQueue { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class TenantJobCount
    {
        [XmlAttribute]
        [DataMember]
        public int TenantLevel { get; set; }

        [XmlAttribute]
        [DataMember]
        public int JobCount { get; set; }

        [XmlAttribute]
        [DataMember]
        public int SubJobCount { get; set; }
    }
}
