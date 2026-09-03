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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.HostManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot(ElementName = "VMClusterInfo")]
    public class VMClusterInfo
    {
        /// <summary>
        /// Cluster name
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// All of cluster ip
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public List<string> IPs { get; set; }

        /// <summary>
        /// Identification of cluster
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public string InstanceId { get; set; }

        /// <summary>
        /// Operation system
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public string OS { get; set; }

        /// <summary>
        /// Operation system version
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public string OSVersion { get; set; }

        /// <summary>
        /// Volumes root
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public string SharedVolumesRootBase { get; set; }
    }
}