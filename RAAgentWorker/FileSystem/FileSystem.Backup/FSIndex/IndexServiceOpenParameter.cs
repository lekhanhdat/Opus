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




namespace AvePoint.Media.Service.DomainModel
{
    #region using directives
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using Storage;
    using GCommon.Contract.Server.ControlPanel.Cryptography;
    using System;
    using System.Text;

    #endregion

    public class IndexServiceOpenParameter
    {
        public CacheSettingDto CacheSetting { get; set; }
        public IXSystem IndexCacheDeviceSystem { get; set; }
        public IXSystem IndexLogicalDeviceSystem { get; set; }
        public String IndexVolume { get; set; }
        public String IndexDatabaseName { get; set; }
        public String StorageInfo { get; set; }
        public String BackupJobId { get; set; }
        public Boolean IsNeedCreateNewIndex { get; set; }
        public Boolean IsNeedCheckIntegrity { get; set; }
        public Boolean IsNeedBackupIndex { get; set; }
        public Boolean NeedDownLoad { get; set; }
        public String DBPassWord { get; set; }

        public byte DataMode { get; set; }
        public DataEncryptionInfo EncryptionInfo { get; set; }

        public IndexServiceOpenParameter()
        { }

        public IndexServiceOpenParameter(String indexVolume, IXSystem indexLogicalDevice, IXSystem cacheSystem, CacheSettingDto cacheSetting)
        {
            IndexLogicalDeviceSystem = indexLogicalDevice;
            IndexCacheDeviceSystem = cacheSystem;
            IndexVolume = indexVolume;
            CacheSetting = cacheSetting;
        }

        public override String ToString()
        {
            var sb = new StringBuilder();
            sb.Append("IndexServiceOpenParameter: BackupJobId:");
            sb.Append(this.BackupJobId);
            sb.Append(" IndexVolume: ");
            sb.Append(this.IndexVolume);
            sb.Append(" IndexDatabaseName: ");
            sb.Append(this.IndexDatabaseName);
            sb.Append(" IsNeedCreateNewIndex: ");
            sb.Append(this.IsNeedCreateNewIndex);
            sb.Append(" IsNeedCheckIntegrity: ");
            sb.Append(IsNeedCheckIntegrity);
            sb.Append(" StorageInfo: ");
            sb.Append(this.StorageInfo);
            return sb.ToString();
        }
    }
}