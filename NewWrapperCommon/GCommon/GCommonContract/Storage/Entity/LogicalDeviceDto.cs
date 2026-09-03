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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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

        /// <summary>
        /// 对应页面 DataType 选项 0, Logical Device , 1 Blob Storage Device ,2 Backup Storage Device 
        /// </summary>
        [DataMember]
        public LogicalDeviceDataType DataType { get; set; }

        /// <summary>
        /// 对应页面Generate folders based on formate Checkbox
        /// </summary>
        [DataMember]
        public bool IsFolderBasedOnFormat { get; set; }

        /// <summary>
        /// 对应页面DataFormate ComboBox中选择项
        /// </summary>
        [DataMember]
        public FolderFormat FolderFormat { get; set; }

        [DataMember]
        public bool IsFilteredDevice { get; set; }

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
        private bool isPdListSorted = false;
        [DataMember]
        public List<PhysicalDeviceDto> PhysicalDrives  //用户选择的Physical Drive
        {
            get
            {
                if (null == this.pdList)
                {
                    this.pdList = new List<PhysicalDeviceDto>();
                }
                if (!isPdListSorted)
                {
                    //给physicalList 按groupNumber,order排序
                    pdList.Sort(delegate (PhysicalDeviceDto pd1, PhysicalDeviceDto pd2)
                    {
                        if (pd1.GroupNum > pd2.GroupNum)
                        {
                            return 1;
                        }
                        else if (pd1.GroupNum == pd2.GroupNum)
                        {
                            return pd1.Order.CompareTo(pd2.Order);
                        }
                        else
                        {
                            return -1;
                        }
                    });
                    isPdListSorted = true;
                }
                return this.pdList;
            }
            set
            {
                this.pdList = value;
                isPdListSorted = false;
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

        [DataMember(EmitDefaultValue = false)]
        public List<ServiceGroupDto> AgentGroups { get; set; }

        public LogicalDeviceDto Clone()
        {
            return new LogicalDeviceDto
            {
                Id = this.Id,
                Name = this.Name,
                Type = this.Type,
                LogicalDeviceType = this.LogicalDeviceType,
                DataType = this.DataType,
                Raid = this.Raid,
                IsDisabledRaid = this.IsDisabledRaid,
                Status = this.Status,
                NetAppONTAPType = this.NetAppONTAPType,
                ModifyTime = this.ModifyTime,
                SyncMode = this.SyncMode,
                BackupLogicalDeviceId = this.BackupLogicalDeviceId,
                TotleSpace = this.TotleSpace,
                TotleUseSpace = this.TotleUseSpace,
                Description = this.Description,
                FolderFormat = this.FolderFormat,
                PhysicalDrives = this.PhysicalDrives,
                PhysicalDeviceOrderDtos = this.PhysicalDeviceOrderDtos,
            };
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
            XRIParameter param = new XRIParameter();
            if (Raid)
            {
                return GetRAIDXRIS(param);
            }
            else
            {
                List<string> xris = new List<string>();
                foreach (PhysicalDeviceDto pd in PhysicalDrives)
                {
                    xris.Add(pd.BuildXRI());
                }
                return xris;
            }
        }

        public List<string> ToRAID1XRIS(string agentGroupid = null)
        {
            XRIParameter param = new XRIParameter();
            if (string.IsNullOrEmpty(agentGroupid))
            {
                return this.GetRAIDXRIS(param);
            }
            else
            {
                return this.GetRAIDXRIS(param, null, agentGroupid);
            }
        }

        /// <summary>
        /// 使用这个方法的地方，请改成使用ToXRIS(XRIParameter param),将param里isValidte参数设置成true
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public List<string> ToValidateXRIS()
        {
            XRIParameter param = new XRIParameter() { IsValidate = true };
            if (Raid)
            {
                return GetRAIDXRIS(param);
            }
            else
            {
                List<string> xris = new List<string>();
                foreach (PhysicalDeviceDto pd in PhysicalDrives)
                {
                    xris.Add(pd.BuildXRI(param));
                }
                return xris;
            }
        }

        public List<string> ToXRIS(XRIParameter param, int groupNumber = 0)
        {
            if (Raid)
            {
                return GetRAIDXRIS(param);
            }
            else
            {
                List<string> xris = new List<string>();
                foreach (PhysicalDeviceDto pd in PhysicalDrives)
                {
                    if (pd.GroupNum == groupNumber)
                    {
                        xris.Add(pd.BuildXRI(param));
                    }
                }
                return xris;
            }
        }

        public List<string> ToXRIS(PhysicalDeviceStatus status, int groupNumber = 0)
        {
            XRIParameter param = new XRIParameter();
            if (Raid)
            {
                return GetRAIDXRIS(param);
            }
            else
            {
                List<string> xris = new List<string>();
                foreach (PhysicalDeviceDto pd in PhysicalDrives)
                {
                    if (pd.Status == (int)status && pd.GroupNum == groupNumber)
                    {
                        xris.Add(pd.BuildXRI());
                    }
                }
                return xris;
            }
        }

        private List<string> GetRAIDXRIS(XRIParameter param, string preferentialPdId = null, string agentGroupId = null)
        {
            List<string> xris = new List<string>();
            StringBuilder xri = new StringBuilder("DOCAVE-XAM://MIRRORFS_VIM?".ToLower(new CultureInfo("en-US")));
            xri.Append("id=").Append(Id).Append("&");
            xri.Append("SyncMode=").Append(SyncMode).Append("&");
            int i = 0;
            if (preferentialPdId != null)
            {
                foreach (PhysicalDeviceDto dto in this.PhysicalDrives)
                {
                    if (dto.Id.Equals(preferentialPdId, StringComparison.OrdinalIgnoreCase))
                    {
                        xri.Append("system_").Append(i++).Append("=").Append(AvePoint.GCommon.Contract.Storage.Entity.PhysicalDeviceDto.XRIUtil.ValueEncode(dto.BuildXRI(param))).Append("&");
                        break;
                    }
                }
                foreach (PhysicalDeviceDto pd in this.PhysicalDrives)
                {
                    if (!pd.Id.Equals(preferentialPdId, StringComparison.OrdinalIgnoreCase))
                    {
                        xri.Append("system_").Append(i++).Append("=").Append(AvePoint.GCommon.Contract.Storage.Entity.PhysicalDeviceDto.XRIUtil.ValueEncode(pd.BuildXRI(param))).Append("&");
                    }
                }
            }
            else if (!string.IsNullOrEmpty(agentGroupId))//与上互斥
            {
                var tmpList = new List<PhysicalDeviceDto>();
                foreach (PhysicalDeviceDto dto in this.PhysicalDrives)
                {
                    if (agentGroupId.Equals(dto.AgentGroupId, StringComparison.OrdinalIgnoreCase))
                    {
                        tmpList.Add(dto);
                    }
                }
                foreach (PhysicalDeviceDto dto in tmpList)
                {
                    xri.Append("system_").Append(i++).Append("=").Append(AvePoint.GCommon.Contract.Storage.Entity.PhysicalDeviceDto.XRIUtil.ValueEncode(dto.BuildXRI(param))).Append("&");
                }
                foreach (PhysicalDeviceDto pd in this.PhysicalDrives)
                {
                    if (!agentGroupId.Equals(pd.AgentGroupId, StringComparison.OrdinalIgnoreCase))
                    {
                        xri.Append("system_").Append(i++).Append("=").Append(AvePoint.GCommon.Contract.Storage.Entity.PhysicalDeviceDto.XRIUtil.ValueEncode(pd.BuildXRI(param))).Append("&");
                    }
                }
            }
            else
            {
                foreach (PhysicalDeviceDto pd in this.PhysicalDrives)
                {
                    xri.Append("system_").Append(i++).Append("=").Append(AvePoint.GCommon.Contract.Storage.Entity.PhysicalDeviceDto.XRIUtil.ValueEncode(pd.BuildXRI(param))).Append("&");
                }
            }
            string xriString = xri.ToString().TrimEnd('&');
            xris.Add(xriString);
            return xris;
        }

        public List<string> GetXRIS(PhysicalDeviceUsage usage, string preferentialPdId = null, int groupNumber = 0)
        {
            List<string> xris = new List<string>();
            XRIParameter param = new XRIParameter();

            if (Raid)
            {
                return GetRAIDXRIS(param, preferentialPdId);
            }
            else
            {
                switch (usage)
                {
                    case PhysicalDeviceUsage.All:
                        foreach (PhysicalDeviceDto pd in PhysicalDrives)
                        {
                            if (pd.DeviceMode == (int)PhysicalDeviceStatus.Online && pd.GroupNum == groupNumber)
                            {
                                xris.Add(pd.BuildXRI());
                            }
                        }
                        break;
                    case PhysicalDeviceUsage.Data:
                        foreach (PhysicalDeviceDto pd in PhysicalDrives)
                        {
                            if ((pd.Usage == PhysicalDeviceUsage.All || pd.Usage == PhysicalDeviceUsage.Data) && pd.GroupNum == groupNumber)
                            {
                                if (pd.DeviceMode == (int)PhysicalDeviceStatus.Online)
                                {
                                    xris.Add(pd.BuildXRI());
                                }
                            }
                        }
                        break;
                    case PhysicalDeviceUsage.Index:
                        foreach (PhysicalDeviceDto pd in PhysicalDrives)
                        {
                            if ((pd.Usage == PhysicalDeviceUsage.All || pd.Usage == PhysicalDeviceUsage.Index) && pd.GroupNum == groupNumber)
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
            foreach (PhysicalDeviceDto pd in PhysicalDrives)
            {
                if (pd.DeviceMode == (int)PhysicalDeviceStatus.Online && pd.IsSnapLock)
                {
                    xris.Add(pd.BuildXRI());
                }
            }
            return xris;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "xris is unmodifiable.")]
        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Logical device information: id:{0},name:{1},device type:{2}, XRIS details:", this.Id, this.Name, this.LogicalDeviceType);
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
        [Obsolete]
        [EnumMember]
        NoDefault = 0,
        [Obsolete]
        [EnumMember]
        Default = 1,
        [EnumMember]
        LogicalDevice = 0,
        [EnumMember]
        BlobStorageDevice = 1,
        [EnumMember]
        BackupStorageDevice = 2,
        [EnumMember]
        ConcurrencyLogicalDevice = 3,
        [EnumMember]
        HighAvailabilityStorageDevice = 4

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum FolderFormat : int
    {
        [EnumMember]
        YYYYMM = 0,
        [EnumMember]
        YYYYMMDD = 1,
        [EnumMember]
        YYYYMMDDHH = 2,
        [EnumMember]
        YYYYMMDDHHMM = 3,
        [EnumMember]
        SharePointStructure = 4
    }

    public enum LogicalDeviceDataType : int
    {
        LogicalDevice = 0,
        BlobStorageDevice = 1,
        BackupStorageDevice = 2,
        ConcurrencyLogicalDevice = 3,
        HighAvailabilityStorageDevice = 4,
    }
}