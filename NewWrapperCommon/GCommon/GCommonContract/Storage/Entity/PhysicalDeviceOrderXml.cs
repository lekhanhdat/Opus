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
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using System.Xml.Serialization;

namespace AvePoint.GCommon.Contract.Storage.Entity
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PhysicalDeviceOrderXml
    {
        private string physicalDeviceId;
        private int order;
        private string name;
        private string asnodename;
        private string nodeName;
        private long freeSpace;
        private bool isStorageData;
        private bool isStorageIndex;
        private int groupNum;
        private string agentGroupId;
        private string agentGroupName;
        [DataMember]
        [XmlAttribute("agentGroupId")]
        public string AgentGroupId
        {
            get
            {
                return this.agentGroupId;
            }
            set
            {
                this.agentGroupId = value;
            }
        }
        [DataMember]
        [XmlAttribute("agentGroupName")]
        public string AgentGroupName
        {
            get
            {
                return this.agentGroupName;
            }
            set
            {
                this.agentGroupName = value;
            }
        }
        [DataMember]
        [XmlAttribute("physicalDeviceId")]
        public string PhysicalDeviceId
        {
            get
            {
                return this.physicalDeviceId;
            }
            set
            {
                this.physicalDeviceId = value;
            }
        }
        [DataMember]
        [XmlAttribute("order")]
        public int Order
        {
            get
            {
                return this.order;
            }
            set
            {
                this.order = value;
            }
        }
        [DataMember]
        [XmlAttribute("groupNum")]
        public int GroupNum
        {
            get
            {
                return this.groupNum;
            }
            set
            {
                this.groupNum = value;
            }
        }
        [DataMember]
        [XmlAttribute("name")]
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }
        [DataMember]
        [XmlAttribute("asnodename")]
        public string Asnodename
        {
            get
            {
                return this.asnodename;
            }
            set
            {
                this.asnodename = value;
            }
        }
        [DataMember]
        [XmlAttribute("nodeName")]
        public string NodeName
        {
            get
            {
                return this.nodeName;
            }
            set
            {
                this.nodeName = value;
            }
        }
        [DataMember]
        [XmlAttribute("freeSpace")]
        public long FreeSpace
        {
            get
            {
                return this.freeSpace;
            }
            set
            {
                this.freeSpace = value;
            }
        }
        [DataMember]
        [XmlAttribute("isStorageData")]
        public bool IsStorageData
        {
            get
            {
                return this.isStorageData;
            }
            set
            {
                this.isStorageData = value;
            }
        }
        [DataMember]
        [XmlAttribute("isStorageIndex")]
        public bool IsStorageIndex
        {
            get
            {
                return this.isStorageIndex;
            }
            set
            {
                this.isStorageIndex = value;
            }
        }
        [DataMember]
        [XmlAttribute("isSnaplock")]
        public bool IsSnaplock { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LogicalDeviceXml
    {
        [DataMember]
        [XmlAttribute("Raid")]
        public bool Raid { get; set; }

        [DataMember]
        [XmlAttribute("SyncMode")]
        public int SyncMode { get; set; }

        [DataMember]
        [XmlAttribute("DataType")]
        public int DataType { get; set; }

        [DataMember]
        [XmlAttribute("IsFolderBasedOnFormat")]
        public bool IsFolderBasedOnFormat { get; set; }

        [DataMember]
        [XmlAttribute("FolderFormat")]
        public int FolderFormat { get; set; }

        [DataMember]
        [XmlAttribute("IsFilteredDevice")]
        public bool IsFilteredDevice { get; set; }

        /// <summary>
        /// 对应页面的Netapp ONTAP
        /// </summary>
        [DataMember]
        public string NetAppONTAPType { get; set; }

        [DataMember]
        public string BackupLogicalDeviceId { get; set; }

        [DataMember]
        public List<PhysicalDeviceOrderXml> PhysicalDeviceOrders { get; set; }
    }
}
