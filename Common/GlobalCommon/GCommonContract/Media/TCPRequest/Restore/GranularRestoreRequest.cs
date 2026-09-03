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




namespace AvePoint.GCommon.Contract.Media.TCPRequest.Restore
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Tree.Object;
    #endregion
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GranularRestoreRequest : MediaTCPRequest
    {
        [DataMember]
        public String FarmName { get; set; }
        [DataMember]
        public String BackupPlanId { get; set; }
        [DataMember]
        public String BackupCycleId { get; set; }
        [DataMember]
        public String BackupJobId { get; set; }
        [DataMember]
        public String StoragePolicyId { get; set; }
        [DataMember]
        public Int64 BackupTime { get; set; }
        [DataMember]
        public Boolean OnlyOneJob { get; set; }

        [DataMember]
        public Double ItemCount { get; set; }

        [DataMember]
        public String ZipFilePassword { get; set; }

        [DataMember]
        public PlatformType PlatformType { get; set; }

        [DataMember]
        public ProductVersion ProductVersion { get; set; }

        [DataMember]
        public List<SiteInfo> SiteInfos { get; set; }

        [DataMember]
        public SPTreeNodeDto TreeRoot { get; set; }

        [DataMember]
        public CacheSettingDto CacheLocation { get; set; }

        [DataMember]
        public LogicalDeviceDto LogicalDevice { get; set; }

        [DataMember]
        public List<RestoreSecurityInfoWrapper> RestoreSecurityInfos { get; set; }

        [DataMember]
        public Boolean IsSearchTree { get; set; }

        [DataMember]
        public bool IsResend { get; set; }

        [DataMember]
        public Boolean IsReadDataViaCache { get; set; }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Granular Restore Request: ");
            stringBuilder.AppendFormat("Farm Name: {0}, ", this.FarmName);
            stringBuilder.AppendFormat("Backup Job Id: {0}, ", this.BackupJobId);
            stringBuilder.AppendFormat("Only One Job: {0}, ", this.OnlyOneJob);
            stringBuilder.AppendFormat("Tree Root: {0}, ", this.TreeRoot);
            stringBuilder.AppendFormat("Cache Location: {0}, ", this.CacheLocation);
            stringBuilder.AppendFormat("Logical Device: {0}", this.LogicalDevice);
            return stringBuilder.ToString();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteInfo
    {
        [DataMember]
        public String SiteUrl { get; set; }

        /// <summary> 存储storage的一些信息，如：EMC存储介质的clip id,Dell存储介质的Object id。</summary>
        [DataMember]
        public String StorageInfo { get; set; }

        public override String ToString()
        {
            return String.Format("Site Url: {0}, Storage Info: {1}",
                this.SiteUrl,
                this.StorageInfo);
        }
    }
}
