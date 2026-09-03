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


namespace AvePoint.GCommon.Contract.Server.ControlPanel.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Storage.Entity;
    #endregion
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CacheSettingDto
    {
        [DataMember]
        public String Id { get; set; }

        [DataMember]
        public String ServiceName { get; set; }

        [DataMember]
        public Boolean EnableThreshold { get; set; }

        [DataMember]
        public UInt64 LimitFreeSpace { get; set; } // 以byte为单位

        [DataMember]
        public CacheSettingExtension Extension { get; set; }

        public LogicalDeviceDto ConvertToLogicalDeviceDto()
        {
            LogicalDeviceDto ld = new LogicalDeviceDto();
            foreach (PathMap path in Extension.Path)
            {
                ld.PhysicalDrives.Add(PhysicalDeviceDto.GenterateFS(path.DiskInfo.Path, path.DiskInfo.UserName, path.DiskInfo.Password));
            }
            return ld;
        }

        public DiskInfoDto GetDiskInfoDto()
        {
            if (this.Extension != null && this.Extension.Path != null)
            {
                foreach (PathMap path in Extension.Path)
                {
                    return path.DiskInfo;
                }
            }
            return null;
        }
        public void SetDiskInfoDto(DiskInfoDto diskInfo)
        {
            if (this.Extension == null)
            {
                Extension = new CacheSettingExtension() { Path = new List<PathMap>() };
            }
            if (this.Extension.Path == null)
            {
                Extension.Path = new List<PathMap>();
            }
            Extension.Path.Add(new PathMap() { DiskInfo = diskInfo });
        }

        public override String ToString()
        {
            return String.Format("Cache Setting Info: Id: {0}, Service Name: {1}, Extension: {2}",
                this.Id,
                this.ServiceName,
                this.Extension);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CacheSettingExtension
    {
        /// <summary>
        /// Service Type :  0---Control
        /// </summary>
        [DataMember]
        public Int32 ServiceType { get; set; }

        [DataMember]
        public List<PathMap> Path { get; set; }

        /// <summary>
        /// Retention[0]----value,Retention[1]=0----Days
        ///                       Retention[1]=1----Weeks
        ///                       Retention[1]=2----Months
        /// </summary>
        [DataMember]
        public Int32[] Retention { get; set; }

        [DataMember]
        public String Host { get; set; }

        /// <summary>
        /// Threshold[0]=0----MB,Threshold[1]----value
        /// Threshold[0]=1----Percent
        /// </summary>
        [DataMember]
        public int[] Threshold { get; set; }

        [DataMember]
        public List<NotificationMap> Notification { get; set; }

        public override String ToString()
        {
            return String.Format("Service Type: {0}, Host: {1}", this.ServiceType, this.Host);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NotificationMap
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public List<string> UserNames { get; set; }

        public override string ToString()
        {
            if (this.UserNames == null) return "";
            StringBuilder sb = new StringBuilder();
            foreach (var item in UserNames)
            {
                sb.Append(item).Append(";");
            }
            return sb.ToString();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PathMap
    {
        [DataMember]
        public int Index { get; set; }

        [DataMember]
        public DiskInfoDto DiskInfo { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DiskInfoDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Path { get; set; }

        /// <summary>
        /// Usage[0]----free capacity,Usage[1]----total capacity
        /// </summary>
        [DataMember]
        public long[] Usage { get; set; }

        /// <summary>
        /// device type : local path ,unc path
        /// </summary>
        [DataMember]
        public DeviceType Type { get; set; }

        [DataMember]
        public string AccountProfileId { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string Password { get; set; }
    }

    public enum DeviceType : int
    {
        [EnumMember]
        LocalPath = 0,
        [EnumMember]
        UncPath = 1
    }

    public enum ValidatePathResult
    {
        [EnumMember]
        Failed = 0,
        [EnumMember]
        ExistSameDisk = 1,
        [EnumMember]
        Successfull = 2,
        [EnumMember]
        SuccessfullForTest = 3,
        [EnumMember]
        UncPathFailed = 4,
        [EnumMember]
        AuthenticationFailed = 5,

    }
}

