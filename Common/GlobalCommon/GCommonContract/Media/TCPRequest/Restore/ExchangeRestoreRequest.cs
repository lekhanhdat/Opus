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

    #endregion using directives

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExchangeRestoreRequest : MediaTCPRequest
    {
        [DataMember]
        public string BackupPlanId { get; set; }

        [DataMember]
        public string BackupCycleId { get; set; }

        [DataMember]
        public string BackupJobId { get; set; }

        [DataMember]
        public long BackupTime { get; set; }

        [DataMember]
        public bool OnlyOneJob { get; set; }

        [DataMember]
        public bool IsSoftDeleted { get; set; }

        [DataMember]
        public bool IsIncludeDeletedContents { get; set; }

        [DataMember]
        public PlatformType PlatformType { get; set; }

        [DataMember]
        public ExchangeOnlineTreeNodeDto TreeRoot { get; set; }

        [DataMember]
        public CacheSettingDto CacheLocation { get; set; }

        /// <summary>
        /// key is user id, value is the storage info of that index file
        /// </summary>
        [DataMember]
        public Dictionary<string, string> IndexStorageInfoMap { get; set; }

        [DataMember]
        public List<RestoreSecurityInfoWrapper> RestoreSecurityInfos { get; set; }

        [Obsolete("don't support resend any more.")]
        [DataMember]
        public bool IsResend { get; set; }

        //public List<PowerPlatformRestoreDto> Items { get; set; }

        [DataMember]
        public long? FromSentDate { get; set; }

        [DataMember]
        public long? ToSentDate { get; set; }

        [DataMember]
        public bool IsRestoreFromArchiveTier { get; set; }

        [DataMember]
        public virtual LogicalDeviceDto LogicalDevice { get; set; }

        [DataMember]
        public virtual LogicalDeviceDto IndexDBLogicalDevice { get; set; }


        public string GetModulePath()
        {
            if (IsRestoreFromArchiveTier)
            {
                return $"Rehydrate##{JobId}";
            }
            return "data_exchange";
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Exchange Restore Request: ");
            stringBuilder.AppendFormat("Backup Job Id: {0}, ", BackupJobId);
            stringBuilder.AppendFormat("Only One Job: {0}, ", OnlyOneJob);
            stringBuilder.AppendFormat("Tree Root: {0}, ", TreeRoot);
            stringBuilder.AppendFormat("Cache Location: {0}, ", CacheLocation);
            stringBuilder.AppendFormat("Logical Device: {0}", LogicalDevice);
            return stringBuilder.ToString();
        }
    }
}