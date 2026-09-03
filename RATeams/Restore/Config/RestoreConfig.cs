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

namespace Office365GroupRestore
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Text;

    using AvePoint.Common;
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object;
    using AvePoint.Media.Service;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.RA.CommonUtil;
    using ExchangeUtility.Graph;
    using Storage;



    #endregion

    public class RestoreConfig : EORestoreConfig
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(RestoreConfig));
        public ExchangeRestoreJob exchangeRestoreJob;

        #region --static--
        public static MailboxType CurrentMailboxType;
        
        public static string CurrentMailboxIndexCode { get; set; }
        public static string TenantGroupId;
        public static string CurrentMailbox;
        public static string CurrentMailboxAddress;
        public static string CurrentFileExtension;
        public static string CurrentRestoreMailbox;
        public static Boolean EntirePlannerPlan;
        public static Boolean NeedRecordTaskAttachmentsLink { get; set; }

        public BposInfo BposInfo { get; set; }

        public static Dictionary<string, BposInfo> EmailBposInfoMap { get; set; }
        public static Dictionary<string, BposInfo> OutPlaceEmailBposInfoMap { get; set; }

        public static Dictionary<string, string> EmailAddressMap = new Dictionary<string, string>();
        public static Dictionary<string, string> TenantIdMap = new Dictionary<string, string>();
        public static Dictionary<string, long> ItemCreateTimeInfo = new Dictionary<string, long>();

        public static HashSet<string> TopicItemIds = new HashSet<string>();
        public static HashSet<string> FileNames = new HashSet<string>();
        public IXSystem DestinationPhysicalDevice;

        #endregion

        #region ---Job Info---
        //public ExchangeMailboxType MailboxType { get; private set; }//mailbox type for destination, should be added in dest tree node, but control did not send dest tree node to agent. Add a option here, assume there is only on mailbox in dest.

        public string PlanId { get; set; }

        public string JobId { get; set; }
        public string RestoreJobId { get; set; }
        public string JobDir { get; set; }

        //public int JobCategory { get; set; }
        #endregion

        #region ---Options
        public bool BulkRestoreItems { get; set; }

        public int MaxBulkItemsCount { get; set; }

        public int MaxBulkItemSize { get; set; }

        public int MaxTotalSizeOnDownload { get; private set; }

        public int EWSMonitorMode { get; set; }

        public int EWSMonitorInterval { get; set; }

        public bool IsSupportLockedSite { get; set; } = false;
        #endregion

        #region ---UserMapping---
        public Dictionary<string, string> UserMapping { get; set; }

        public Dictionary<string, string> DomainMapping { get; set; }

        public string DefaultUser4Mapping { get; private set; }
        #endregion

        #region ---No use---
        //public ICacheService CacheManager { get; set; }

        //public string TenantGroupOwner { get; private set; }

        //public DestStorageInfo DestStorageInfo { get; set; }

        //public int MaxFolderCount = 0;

        #endregion


        public RestoreConfig(ERMessage message)
        {
            exchangeRestoreJob = new ExchangeRestoreJob(message.ConfigForMedia);

            #region ---Static---
            TenantGroupId = message.TenantGroupId;
            ItemCreateTimeInfo = new Dictionary<string, long>();
            TopicItemIds = new HashSet<string>();
            GlobalExchangeSetting.IsO365GroupMailBox = message.Config.IsO365Group;
            logger.Info("Is O365 Group Mailbox: {0}.", message.Config.IsO365Group);
            #endregion

            #region ---Options---
            MaxTotalSizeOnDownload = 20;
            BulkRestoreItems = true;
            MaxBulkItemsCount = 10000;
            MaxBulkItemSize = 50;
            EWSMonitorMode = 3;
            EWSMonitorInterval = 300;
            #endregion

            #region ---BPOSInfo---
            InitBposInfoMap(message);
            #endregion

            #region ---JobInfo---
            PlanId = message.PlanId;
            JobId = message.ConfigForMedia.JobId;
            IsSoftDeleted = message.ConfigForMedia.IsSoftDeleted;
            RestoreJobId = message.JobId;
            JobDir = Path.Combine(AveEnv.AgentJobFolder, JobId);
            if (!Directory.Exists(JobDir))
            {
                Directory.CreateDirectory(JobDir);
            }

            BposInfo = message.BposInfo;
            JobType = message.Config.JobType;
            JobCategory = message.Config.JobCategory;
            RestoreType = message.Config.RestoreType;
            ContainerConflictResolution = message.Config.ContainerConflictResolution;
            ContentConflictResolution = message.Config.ContentConflictResolution;
            IsO365Group = message.Config.IsO365Group;
            IsMicrosoftTeams = message.Config.IsMicrosoftTeams;
            IsYammerGroup = message.Config.IsYammerGroup;
            //MailboxType = (ExchangeMailboxType)message.Config.MailboxType;
            IsSkipRestoreConversation = message.Config.RestoreConversationType == RestoreConversationType.Skip || message.Config.IsSkipRestoreConversation;
            RestoreConversationType = message.Config.RestoreConversationType;
            UseImportApi = message.Config.UseImportApi;
            ReportOnlyHighLevel = message.Config.ReportOnlyHighLevel;
            NeedMergeConversation = message.Config.NeedMergeConversation;
            SkippedErrorCodeList = message.Config.SkippedErrorCodeList;
            ZipFilePassword = message.Config.ZipFilePassword;
            DestinationFSDevice = message.Config.DestinationFSDevice;
            NotificationUsers = message.Config.NotificationUsers;
            #endregion

            #region ---UserMapping---
            UserMapping = message.Config.UserMapping == null ? new Dictionary<string, string>() : message.Config.UserMapping.UserMappings.UserMapping.ToDictionary(user => user.sourceUser, user => user.destinationUser, StringComparer.OrdinalIgnoreCase);
            DefaultUser4Mapping = message.Config.UserMapping?.destDefaultUser;
            DomainMapping = message.Config.DomainMapping == null ? new Dictionary<string, string>() : message.Config.DomainMapping.DomainMappings.DomainMapping.ToDictionary(user => user.sourceDomain, user => user.destinationDomain, StringComparer.OrdinalIgnoreCase);
            logger.Info("User Mapping:");
            UserMapping.ForEach(um => logger.Info("{0}--{1}", um.Key, um.Value));
            logger.Info("Domain Mapping:");
            DomainMapping.ForEach(dm => logger.Info("{0}--{1}", dm.Key, dm.Value));
            logger.Info("Default User: {0}", DefaultUser4Mapping);
            #endregion

            #region--For Restore Teams with original owner is deleted---

            IsSpecifyUser = message.Config.IsSpecifyUser;
            SpecifyUserList = IsSpecifyUser ? message.Config.SpecifyUserList : [];

            #endregion
            #region--For Restore Teams with specify restore version number
            RestoreVersionOption = message.Config.RestoreVersionOption;
            KeepVersionsNumber = message.Config.KeepVersionsNumber;
            #endregion

            logger.Info(ToString());
        }

        private static void InitBposInfoMap(ERMessage message)
        {
            EmailBposInfoMap = message.EmailBposInfoMap;
            OutPlaceEmailBposInfoMap = message.OutPlaceEmailBposInfoMap;
            var outPlaceEmailAddress = string.Empty;
            var destTenantId = string.Empty;
            if (OutPlaceEmailBposInfoMap != null && OutPlaceEmailBposInfoMap.Count > 0)
            {
                var outMap = OutPlaceEmailBposInfoMap.First();
                //logger.Info("Key: {0}. ConnectionType:[{1}]. Username:[{2}]. TenantId:[{3}]. PasswordExist:[{4}]. AppUserName: {5} ", outMap.Key, outMap.Value.ConnectionType, outMap.Value.UserAccountInfo.ServiceAccountUsername,
                //        outMap.Value.UserAccountInfo.TenantId, false, outMap.Value.UserAccountInfo.AppProfileUsername);studo
                //OutPlaceEmailBposInfoMap.ForEach(oM => logger.Info("Key: {0}. ConnectionType:[{1}]. Username:[{2}]. TenantId:[{3}]. PasswordExist:[{4}]. ", oM.Key, oM.Value.ConnectionType, oM.Value.UserAccountInfo.Username,
                //        oM.Value.UserAccountInfo.TenantId, !string.IsNullOrEmpty(oM.Value.UserAccountInfo.Password)));
                outPlaceEmailAddress = outMap.Key;
                destTenantId = outMap.Value.UserAccountInfo.TenantId;
            }
            else
            {
                logger.Info("OutPlaceEmailBposInfoMap is null. ");
            }
            if (!string.IsNullOrEmpty(outPlaceEmailAddress) && EmailBposInfoMap != null)
            {
                EmailBposInfoMap.ForEach(eBIM =>
                {
                    if (EmailAddressMap != null && !EmailAddressMap.ContainsKey(eBIM.Key)) EmailAddressMap.Add(eBIM.Key, outPlaceEmailAddress);
                    var sourceTenantId = eBIM.Value != null && eBIM.Value.UserAccountInfo != null ? eBIM.Value.UserAccountInfo.TenantId : string.Empty;
                    if (!string.IsNullOrEmpty(sourceTenantId) && TenantIdMap != null && !TenantIdMap.ContainsKey(sourceTenantId)) TenantIdMap.Add(sourceTenantId, destTenantId);
                });
            }
        }


        public override string ToString()
        {
            var config = new StringBuilder();
            config.AppendLine($"PlanId:                  {PlanId}");
            config.AppendLine($"JobId:                   {JobId}");
            config.AppendLine($"ContainerRestoreMode:    {ContainerConflictResolution}");
            config.AppendLine($"ContentRestoreMode:      {ContentConflictResolution}");
            config.AppendLine($"RestoreType:             {RestoreType}");
            config.AppendLine($"JobType:                 {JobType}");
            config.AppendLine($"RestoreConversationType: {RestoreConversationType}");
            config.AppendLine($"NeedMergeConversation: {NeedMergeConversation}");
            return config.ToString();
        }
    }
}