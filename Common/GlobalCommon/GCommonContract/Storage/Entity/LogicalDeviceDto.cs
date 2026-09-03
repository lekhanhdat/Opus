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
using System.Runtime.Serialization;
using System.Text;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Storage.Entity
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LogicalDeviceDto
    {
        private List<PhysicalDeviceDto> pdList = new List<PhysicalDeviceDto>();
        private List<PhysicalDeviceOrderDto> physicalDeviceOrderDtos;
        private long modifyTime;

        [DataMember]
        public String Id { get; set; }

        [DataMember]
        public String Name { get; set; }

        [DataMember]
        public Int32 Type { get; set; }

        /// <summary>
        /// GUI用来判断当前的Logical是否本选中，该属性只在页面中使用
        /// </summary>
        [DataMember]
        public bool IsChecked { get; set; }

        [DataMember]
        public LogicalDeviceType LogicalDeviceType { get; set; }

        [DataMember]
        public bool Raid { get; set; }

        /// <summary>
        /// 用来表示RAID device 的同步方式
        ///  Synchronous = 0
        ///  ASynchronous = 1
        /// </summary>
        [DataMember]
        public int SyncMode { get; set; }

        /// <summary>
        /// 对应页面的Netapp ONTAP
        /// </summary>
        [DataMember]
        public string NetAppONTAPType { get; set; }

        /// <summary>
        /// 对应RAID 1 data storage的可用性
        /// </summary>
        [DataMember]
        public bool IsDisabledRaid { get; set; }

        //[DataMember]
        //public PhysicalDeviceType StorageType { get; set; }  //Storage Type

        [DataMember]
        public long ModifyTime  //Logical Device的修改时间。
        {
            get
            {
                return this.modifyTime;
            }
            set
            {
                this.modifyTime = value;
            }
        }

        [DataMember]
        public int Status { get; set; } ////判断是否是删除的Logical device,以及是否是修改Logical device后新建立的Logical Device

        [DataMember]
        public float FreeSpaceAvailable { get; set; } //对应Logical device detail中的Free Space Available列。

        [DataMember]
        public float TotleSpace { set; get; } //存储磁盘总空间的大小。

        [DataMember]
        public float TotleUseSpace { set; get; } //Logical Device下所有的Physical Device的使用空间大小

        [DataMember]
        public string Description { set; get; }

        [DataMember]
        public string BackupLogicalDeviceId { get; set; }

        [DataMember]
        public ObjectInfoDto ObjectInfo { get; set; }

        //HACK:
        /*
         * I not sure this type of property is how to assign value
         */

        [DataMember]
        public List<PhysicalDeviceDto> PhysicalDrives  //用户选择的Physical Drive
        {
            get
            {
                if (null == this.pdList)
                {
                    this.pdList = new List<PhysicalDeviceDto>();
                }
                return this.pdList;
            }
            set
            {
                this.pdList = value;
            }
        }

        [DataMember]
        public List<ServiceDto> MediaAgentDtos { get; set; }  //选择的Media Service

        [DataMember]
        public List<PhysicalDeviceOrderDto> PhysicalDeviceOrderDtos
        {
            get
            {
                if (null == this.physicalDeviceOrderDtos)
                {
                    this.physicalDeviceOrderDtos = new List<PhysicalDeviceOrderDto>();
                }
                return this.physicalDeviceOrderDtos;
            }
            set
            {
                this.physicalDeviceOrderDtos = value;
            }
        }

        public bool IsNetAppDevice()
        {
            bool result = false;
            if (PhysicalDrives != null && PhysicalDrives.Count > 0)
            {
                foreach (PhysicalDeviceDto ph in PhysicalDrives)
                {
                    if (ph.Type == (int)StorageDeviceType.NetApp || ph.Type == (int)StorageDeviceType.NetApp_LUN || ph.Type == (int)StorageDeviceType.NetApp_CIFS)
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        public bool HasLun()
        {
            bool result = false;
            if (PhysicalDrives != null && PhysicalDrives.Count > 0)
            {
                foreach (PhysicalDeviceDto ph in PhysicalDrives)
                {
                    if (ph.IsLUN)
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        public bool HasSnapLock()
        {
            bool result = false;
            if (PhysicalDrives != null && PhysicalDrives.Count > 0)
            {
                foreach (PhysicalDeviceDto ph in PhysicalDrives)
                {
                    if (ph.IsSnapLock)
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        public List<string> ToXRIS()
        {
            if (Raid)
            {
                return GetRAIDXRIS();
            }
            else
            {
                List<string> xris = new List<string>();
                foreach (PhysicalDeviceDto pd in pdList)
                {
                    xris.Add(pd.BuildXRI());
                }
                return xris;
            }
        }

        public List<string> ToValidateXRIS()
        {
            if (Raid)
            {
                return GetRAIDXRIS();
            }
            else
            {
                List<string> xris = new List<string>();
                foreach (PhysicalDeviceDto pd in pdList)
                {
                    xris.Add(pd.BuildValidateXRI());
                }
                return xris;
            }
        }

        public List<string> ToXRIS(PhysicalDeviceStatus status)
        {
            if (Raid)
            {
                return GetRAIDXRIS();
            }
            else
            {
                List<string> xris = new List<string>();
                foreach (PhysicalDeviceDto pd in pdList)
                {
                    if (pd.Status == (int)status)
                    {
                        xris.Add(pd.BuildXRI());
                    }
                }
                return xris;
            }
        }

        private List<string> GetRAIDXRIS()
        {
            List<string> xris = new List<string>();
            StringBuilder xri = new StringBuilder("DOCAVE-XAM://MIRRORFS_VIM?".ToLower());
            xri.Append("id=").Append(Id).Append("&");
            xri.Append("SyncMode=").Append(SyncMode).Append("&");
            int i = 0;
            foreach (PhysicalDeviceDto pd in pdList)
            {
                //if (pd.DeviceMode == (int)PhysicalDeviceStatus.Online)
                //{
                xri.Append("system_").Append(i++).Append("=").Append(AvePoint.GCommon.Contract.Storage.Entity.PhysicalDeviceDto.XRIUtil.ValueEncode(pd.BuildValidateXRI())).Append("&");
                //}
            }
            string xriString = xri.ToString().TrimEnd('&');
            xris.Add(xriString);
            return xris;
        }

        public List<string> GetXRIS(PhysicalDeviceUsage usage)
        {
            List<string> xris = new List<string>();

            if (Raid)
            {
                return GetRAIDXRIS();
            }
            else
            {
                switch (usage)
                {
                    case PhysicalDeviceUsage.All:
                        foreach (PhysicalDeviceDto pd in pdList)
                        {
                            if (pd.DeviceMode == (int)PhysicalDeviceStatus.Online)
                            {
                                xris.Add(pd.BuildXRI());
                            }
                        }
                        break;
                    case PhysicalDeviceUsage.Data:
                        foreach (PhysicalDeviceDto pd in pdList)
                        {
                            if (pd.Usage == PhysicalDeviceUsage.All || pd.Usage == PhysicalDeviceUsage.Data)
                            {
                                if (pd.DeviceMode == (int)PhysicalDeviceStatus.Online)
                                {
                                    xris.Add(pd.BuildXRI());
                                }
                            }
                        }
                        break;
                    case PhysicalDeviceUsage.Index:
                        foreach (PhysicalDeviceDto pd in pdList)
                        {
                            if (pd.Usage == PhysicalDeviceUsage.All || pd.Usage == PhysicalDeviceUsage.Index)
                            {
                                if (pd.DeviceMode == (int)PhysicalDeviceStatus.Online)
                                {
                                    xris.Add(pd.BuildXRI());
                                }
                            }
                        }
                        break;
                    default:
                        throw new Exception("Unknown Device Usage : " + usage);
                }
            }

            return xris;
        }

        public List<string> GetSnapLockXRIS()
        {
            List<string> xris = new List<string>();
            foreach (PhysicalDeviceDto pd in pdList)
            {
                if (pd.DeviceMode == (int)PhysicalDeviceStatus.Online && pd.IsSnapLock)
                {
                    xris.Add(pd.BuildXRI());
                }
            }
            return xris;
        }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Logical device informaion: id:{0},name:{1},device type:{2}, xris details:", this.Id, this.Name, this.LogicalDeviceType);
            stringBuilder.AppendLine();
            var xris = this.ToXRIS();
            if (xris != null)
            {
                foreach (var xri in xris)
                {
                    stringBuilder.Append(xri);
                    stringBuilder.AppendLine();
                }
            }

            return stringBuilder.ToString();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum LogicalDeviceType : int
    {
        [EnumMember]
        NoDefault = 0,
        [EnumMember]
        Default = 1
    }
}