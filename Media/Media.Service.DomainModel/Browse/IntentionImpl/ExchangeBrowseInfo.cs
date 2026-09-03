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

    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;


    #endregion using directives

    public class ExchangeBrowseInfo
        : BrowseInfoBase
        , IBrowseInfo
    {
        public string Path { get; set; }

        public TreeNodeLevel Level { get; set; }

        public long EndTime { get; set; }

        public int OffSet { get; set; }

        public int Length { get; set; }
        public string TenantGroupId { get; set; }

        public string BackupJobId { get; set; }

        public string BackupPlanId { get; set; }

        public string BackupCycleId { get; set; }

        public string StorageInfo { get; set; }

        public string UserAddress { get; set; }

        public string ObjectId { get; set; }

        public bool OnlyOneJob { get; set; }

        public IEnumerable<string> FilterOutJobIds { get; set; }
        public MailboxType MailboxType { get; set; }

        public bool SupportObjectId { get; set; }

        public bool UseObjectIdPath { get; set; }

        public long RetentionEarliestTime { get; set; }
        /// <summary>
        /// distinguish if comes from new UI search
        /// </summary>
        public bool BackendPagination { get; set; }

        public string Keyword { get; set; }

        public string DisplayName { get; set; }

        //public BrowseShowDataType ShowDataType { get; set; }

        public string OrderRule { get; set; }
        public string OrderField { get; set; }

        public ExchangeBrowseInfo()
        { }

        public ExchangeBrowseInfo(ExchangeRestoreBrowseParam param)
        {
            Path = param.Path;

            Level = System.SafeConvertExtensions.ToEnum<TreeNodeLevel>(param.Level.ToString());
            EndTime = param.EndTime;
            OffSet = param.OffSet;
            UserAddress = param.UserAddress;
            ObjectId = param.ObjectId;
            SupportObjectId = param.SupportObjectId;
            RetentionEarliestTime = param.RetentionEarliestTime;
            BackendPagination = param.BackendPagination;
            Keyword = param.Keyword;
            TenantGroupId = param.TenantGroupId;
            BackupJobId = param.BackupJobId.Trim();
            BackupPlanId = param.BackupPlanId.Trim();
            BackupCycleId = param.BackupCycleId.Trim();
            StorageInfo = param.StorageInfo;
            LogicalDevice = param.LogicalDevice;
            CacheSetting = param.CacheLocation;
            //ShowDataType = param.ShowDataType;
            //OversizeIndexDBCacheInfo = param.OversizeIndexDBCacheInfo;
            //HostIndexDBCacheInfo = param.HostIndexDBCacheInfo;
            OnlyOneJob = param.OnlyOneJob;
            //studo
            //var volumeGenerator = VolumeGeneratorFactory.GetVolumeGenerator(VolumeType.ExchangeBackup);
            //if (param.Level == GCommon.Contract.Tree.Object.NodeLevel.PowerBIWorkspace || param.Level == GCommon.Contract.Tree.Object.NodeLevel.PowerBIReport)
            //{
            //    volumeGenerator = VolumeGeneratorFactory.GetVolumeGenerator(VolumeType.PowerPlatformsBackup);
            //}
            //IndexVolume = volumeGenerator.GenerateIndexVolume(new VolumeParameter());
            MailboxType = param.MailboxType;
            IndexEncryptionInfoWrapper = param.IndexEncryptionInfoWrapper;
            DisplayName = param.DisplayName;
            OrderRule = param.OrderRule;
            OrderField = param.OrderField;
        }

        public ExchangeBrowseInfo(EORestoreParamDto param, List<string> ExcludeJobIds = null)
        {
            Path = param.Path;
            Level = System.SafeConvertExtensions.ToEnum<TreeNodeLevel>(param.Level.ToString());
            EndTime = param.EndTime;
            OffSet = param.OffSet;
            Length = param.Length;
            UserAddress = param.Address;
            TenantGroupId = param.TenantGroupId;
            BackupJobId = param.BackupJobId.Trim();
            BackupPlanId = param.BackupPlanId.Trim();
            BackupCycleId = param.BackupCycleID.Trim();
            StorageInfo = param.StorageInfo;
            LogicalDevice = param.LogicalDevice;
            CacheSetting = param.CacheLocation;
            //OversizeIndexDBCacheInfo = param.OversizeIndexDBCacheInfo;
            //HostIndexDBCacheInfo = param.HostIndexDBCacheInfo;
            OnlyOneJob = param.OnlyOneJob;
            //studo
            //var volumeGenerator = VolumeGeneratorFactory.GetVolumeGenerator(VolumeType.ExchangeBackup);
            //if (param.Level == GCommon.Contract.Tree.Object.NodeLevel.PowerBIWorkspace || param.Level == GCommon.Contract.Tree.Object.NodeLevel.PowerBIReport || param.Level == GCommon.Contract.Tree.Object.NodeLevel.PowerAutomate || param.Level == GCommon.Contract.Tree.Object.NodeLevel.PowerApps)
            //{
            //    volumeGenerator = VolumeGeneratorFactory.GetVolumeGenerator(VolumeType.PowerPlatformsBackup);
            //}
            //IndexVolume = volumeGenerator.GenerateIndexVolume(new VolumeParameter(this));
            FilterOutJobIds = ExcludeJobIds ?? new List<string>();
            MailboxType = param.MailboxType;
            IndexEncryptionInfoWrapper = param.IndexEncryptionInfoWrapper;
            ObjectId = param.ObjectId;
            SupportObjectId = param.SupportObjectId;
            RetentionEarliestTime = param.RetentionEarliestTime;
            BackendPagination = param.BackendPagination;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ExchangeBrowseInfo: ");
            sb.Append(BackupPlanId);
            sb.Append(" ");
            sb.Append(BackupCycleId);
            sb.Append(" ");
            sb.Append(BackupJobId);
            sb.Append(" ");
            sb.Append(Path);
            return sb.ToString();
        }
    }
}