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

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PhysicalDeviceOrderDto
    {
        private string physicalDeviceId;
        private int order;
        private string name;
        private string nodeName;
        private string asnodeName;
        private long freeSpace;
        private bool isStorageData;
        private bool isStorageIndex;
        private string agentGroupId;
        private string agentGroupName;
        [DataMember]
        public string AgentGroupId
        {
            get
            {
                return agentGroupId;
            }
            set
            {
                agentGroupId = value;
            }
        }
        [DataMember]
        public string AgentGroupName
        {
            get
            {
                return agentGroupName;
            }
            set
            {
                agentGroupName = value;
            }
        }
        [DataMember]
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
        public string Asnodename
        {
            get
            {
                return this.asnodeName;
            }
            set
            {
                this.asnodeName = value;
            }
        }

        [DataMember]
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
        /// <summary>
        /// 每个实例对应的位置列表
        /// </summary>
        [DataMember]
        public List<int> OrderList { get; set; }
        /// <summary>
        /// 记录StorageIndex和StorageData的选择情况:
        /// 1:StorageData选中;2:StorageIndex选中;3:都选中
        /// </summary>
        [DataMember]
        public int StorageMode { get; set; }

        /// <summary>
        /// 获取或设置一个值,该值表示当前数据是否进行Raid
        /// </summary>
        [DataMember]
        public bool Raid { get; set; }

        [DataMember]
        public bool IsSnaplock { get; set; }

        /// <summary>
        /// 当前PhysicalDevice所在组序号
        /// </summary>
        [DataMember]
        public int GroupNum { get; set; }
    }
}
