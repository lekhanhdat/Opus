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



namespace AvePoint.Media.Storage
{

    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.Media.Storage.Util;
    using System.Collections;
    #endregion

    public interface IXResult
    {
    }

    public class StorageDeleteResult : IXResult
    {
        public bool IsDeleted { get; set; }
        public DeleteStatus DeleteStatus { get; set; }
        public long DeletedFileSize { get; set; }
        public string Message { get; set; }
        public bool IsUnauthorizedAccessException { get; set; }
        public DeleteExceptionType DeleteExceptionType { get; set; }

        //public

        public StorageDeleteResult()
        {
            IsDeleted = false;
            DeletedFileSize = 0;
            DeleteExceptionType = DeleteExceptionType.None;
        }
    }

    public enum DeleteExceptionType
    {
        None = 0,
        IOException = 1,
    }
    public class StorageCopyResult : IXResult
    {
        public bool IsCopyed { get; set; }
        public string Message { get; set; }
        public XURIResult URI { get; set; }

        public StorageCopyResult()
        {
            IsCopyed = true;
        }
    }

    public class StorageRenameResult : IXResult
    {
        public bool IsRenamed { get; set; }
        public string Message { get; set; }
        public XURIResult URI { get; set; }

        public StorageRenameResult()
        {
            IsRenamed = true;
        }
    }

    public class StorageMoveResult : IXResult
    {
        public bool IsMoved { get; set; }
        public string Message { get; set; }
        public XURIResult URI { get; set; }

        public StorageMoveResult()
        {
            IsMoved = true;
        }
    }

    public class StorageListResult : IXResult
    {
        public List<XDirectoryInfo> SubDirs { get; set; }
        public List<XFileInfo> Files { get; set; }
        public string Message { get; set; }

        public StorageListResult()
        {
            SubDirs = new List<XDirectoryInfo>();
            Files = new List<XFileInfo>();
        }
    }

    public class StorageListResultSafety : IXResult
    {
        public ArrayList SubDirs { get; set; }
        public ArrayList Files { get; set; }
        public string Message { get; set; }

        public StorageListResultSafety()
        {
            SubDirs = new ArrayList();
            Files = new ArrayList();
        }
    }

    public class UserPermissions
    {
        public bool Read { get; set; }
        public bool Write { get; set; }
        public bool Delete { get; set; }
    }

    public class StorageOpenValidResult : IXResult
    {
        public bool IsHasPermission { get; set; }
        public bool IsReadAble { get; set; }
        public bool IsWriteAble { get; set; }
        public bool IsDeleteAble { get; set; }
        public bool IsSupportRecursiveDelete { get; set; }
        public ulong TotalSpace { get; set; }
        public ulong TotalUsedSpace { get; set; }
        public ulong TotalFreeSpace { get; set; }
        public double FreeSpacePercent { get { return (TotalFreeSpace * 100.0) / TotalSpace; } }
        public string Message { get; set; }
        public List<StorageOpenValidResult> SubResult { get; set; }
        public XSystemHealth SystemHealth { get; set; }
        /// <summary>
        /// 判断是否所有device可用，只适用于RAID
        /// </summary>
        public XSystemValidateStatus IsAllDeviceAvailable { get; set; }

        public StorageOpenValidResult()
        {
            IsHasPermission = false;
            IsReadAble = false;
            IsWriteAble = false;
            IsDeleteAble = false;
            IsSupportRecursiveDelete = false;
            TotalSpace = 0;
            TotalUsedSpace = 0;
            TotalFreeSpace = 0;
            Message = string.Empty;
            SubResult = new List<StorageOpenValidResult>();
            IsAllDeviceAvailable = XSystemValidateStatus.Available;
        }
    }

    /// <summary>
    /// AvePoint统一资源定位器
    /// </summary>
    public class XURIResult : IXResult
    {
        /// <summary>
        /// System Id = Physical Device Id
        /// </summary>
        private string sysId;

        /// <summary>
        /// System Type = Storage Device Type = AvePoint.GCommon.Contract.Storage.Entity.StorageDeviceType (Enum)
        /// </summary>
        private int sdType;

        /// <summary>
        /// StorageInfo (highName, lowName)
        /// </summary>
        private StorageInfo sInfo;

        /// <summary>
        /// System Id = Physical Device Id
        /// </summary>
        public string SysId { get { return this.sysId; } set { this.sysId = value; } }

        /// <summary>
        /// System Type = Storage Device Type = AvePoint.GCommon.Contract.Storage.Entity.StorageDeviceType (Enum)
        /// </summary>
        public int SdType { get { return this.sdType; } set { this.sdType = value; } }

        /// <summary>
        /// StorageInfo (highName, lowName)
        /// </summary>
        public StorageInfo SInfo { get { return sInfo; } set { this.sInfo = value; } }
    }

    /// <summary>
    /// 计算IOPS及吞吐量返回值
    /// </summary>
    public class XPerformanceResult : IXResult
    {
        public long Throughput { get; set; }
        public long ReadBytes { get; set; }
        public long WriteBytes { get; set; }
        public long IOCount { get; set; }
        public long ReadIOCount { get; set; }
        public long WriteIOCount { get; set; }
        public double ReadIopsStdDev { get; set; }
        public double WriteIopsStdDev { get; set; }
        public double IopsStdDev { get; set; }
        public double Iops { get; set; }
        public double ReadIops { get; set; }
        public double WriteIops { get; set; }
    }

    public class StorageResult : IXResult
    {
        private bool deleted;
        private bool existsed;
        private XFileInfo fileInfo;
        private XDirectoryInfo directoryInfo;
        private List<XFileInfo> fileList;
        private List<XDirectoryInfo> directoryList;
        private bool copyed;
        private bool moved;

        private bool readAble;
        private bool writeAble;
        private bool deleteAble;

        private string ldId;
        private string pdId;
        private string highName;
        private string lowName;
        private string uriId;
        private string message;

        private ulong totalSpace;
        private ulong totalUsedSpace;
        private ulong totalFreeSpace;
        private double freeSpacePercent;

        private string storageInfo;
        private int storageInfoId;
        private bool needCommit;
        private long deletedSize;
        //默认为true, 不可随意更改
        private bool needNotifyCommitNextTime;
        private List<StorageResult> mSubResult = new List<StorageResult>();

        public string StorageInfo { get { return storageInfo; } set { this.storageInfo = value; } }
        public List<StorageResult> SubResult { get { return mSubResult; } }
        public int StorageInfoId { get { return storageInfoId; } set { this.storageInfoId = value; } }
        public bool Deleted { get { return deleted; } set { this.deleted = value; } }
        public bool Existsed { get { return existsed; } set { this.existsed = value; } }
        public XFileInfo FileInfo { get { return fileInfo; } set { this.fileInfo = value; } }
        public XDirectoryInfo DirectoryInfo { get { return directoryInfo; } set { this.directoryInfo = value; } }
        public List<XFileInfo> FileList { get { return fileList; } set { this.fileList = value; } }
        public List<XDirectoryInfo> DirectoryList { get { return directoryList; } set { this.directoryList = value; } }
        public bool ReadAble { get { return readAble; } set { this.readAble = value; } }
        public bool WriteAble { get { return writeAble; } set { this.writeAble = value; } }
        public bool DeleteAble { get { return deleteAble; } set { this.deleteAble = value; } }
        public string LdId { get { return ldId; } set { this.ldId = value; } }
        public string PdId { get { return pdId; } set { this.pdId = value; } }
        public string HighName { get { return highName; } set { this.highName = value; } }
        public string LowName { get { return lowName; } set { this.lowName = value; } }
        public string UriId { get { return uriId; } set { this.uriId = value; } }
        public string Message { get { return message; } set { this.message = value; } }
        public ulong TotalSpace { get { return totalSpace; } set { this.totalSpace = value; } }
        public ulong TotalUsedSpace { get { return totalUsedSpace; } set { this.totalUsedSpace = value; } }
        public ulong TotalFreeSpace { get { return totalFreeSpace; } set { this.totalFreeSpace = value; } }
        public double FreeSpacePercent { get { return freeSpacePercent; } set { this.freeSpacePercent = value; } }
        public long DeletedSize { get { return this.deletedSize; } set { this.deletedSize = value; } }
        public bool Copyed { get { return copyed; } set { this.copyed = value; } }
        public bool Moved { get { return this.moved; } set { this.moved = value; } }
        public bool NeedNotifyCommitNextTime { get { return this.needNotifyCommitNextTime; } set { this.needNotifyCommitNextTime = value; } }
        public bool NeedCommit { get { return needCommit; } set { this.needCommit = value; } }

        public bool IsCommited { get; set; }
        public XURIResult URI { get; set; }
    }

    public class SpaceInfo
    {
        public ulong TotalSpace { get; set; }
        public ulong TotalUsedSpace { get; set; }
        public ulong TotalFreeSpace { get; set; }
        /// <summary>
        /// 取得SpaceInfo时的当前时间
        /// </summary>
        public long DataObtainTime { get; set; }
    }
}
