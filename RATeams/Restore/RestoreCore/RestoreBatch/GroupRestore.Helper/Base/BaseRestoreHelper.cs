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
    #region

    using System;
    using System.Linq;

    
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.Job.Object;
    using AvePoint.GCommon.Utility;
    using AvePoint.RA.CommonUtil;
    using ExchangeCommonWrapper;

    using Job.ModernManagement.Report;
    using M365GroupTeam;


    #endregion

    public abstract class BaseRestoreHelper : IRestoreHelper
    {
        protected static readonly RALogger logger = RALogger.GetInstance(typeof(BaseRestoreHelper));
        public IReportCenter Report;// = RestoreReport.GetInstance();

        public RestoreConfig Config { get; set; }

        public ExchangeFileHeader fileHeader { get; set; }

        public ReportDto ReportDto { get; set; }

        public virtual void Restore(ExchangeRestoreData data)
        {
            this.InitReport();
            var metadata = data.Metadata;
            logger.Info("Metadata is null?:{0}", metadata == null);
            var entityString = metadata?.GetMetadata<string>();
            logger.Info("Entity String:{0}", entityString);
            var entity = DeserializeToEntityV2(entityString);
            RealRestore(entity);
        }

        public virtual void Init(ExchangeFileHeader fileHeader, RestoreConfig config)
        {
            this.Config = config;
            this.fileHeader = fileHeader;
        }

        protected virtual void InitReport() =>
            ReportDto = new ReportDto
            {
                EntityType = JobReportDetailEntityType.Objects,
                Name = fileHeader.Name,
                Status = ReportStatus.Success,
                Option = RestoreOption.NewCreated.GetEnumDescription(),
                Title = fileHeader.Name,
                SourcePath = fileHeader.Name,
                Path = fileHeader.Name
            };

        protected virtual void RealRestore(Office365GroupEntityV2 entity)
        { }

        protected virtual Boolean NeedNewCreate(ExchangeFileHeader fileHeader, MetadataEntity entity)
        {
            return true;
        }

        public Office365GroupEntityV2 DeserializeToEntityV2(string entityString)
        {
            try
            {
                return SerializerHelper.DeserializeByDataContractSerializer<Office365GroupEntityV2>(entityString);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while deserialize to entity v2,begin to use v1,Exception:{0}", ex.ToString());
                var entityV1 = SerializerHelper.DeserializeByDataContractSerializer<Office365GroupEntity>(entityString);
                return ConvertToV2(entityV1);
            }
        }

        private Office365GroupEntityV2 ConvertToV2(Office365GroupEntity entityV1)
        {
            return new Office365GroupEntityV2()
            {
                OwnerCount = entityV1.OwnerCount,
                Description = entityV1.Description,
                DisplayName = entityV1.DisplayName,
                MailboxGuid = entityV1.MailboxGuid,
                SmtpAddress = entityV1.SmtpAddress,
                SendToMeida = entityV1.SendToMeida,
                IsTeamsGroup = entityV1.IsTeamsGroup,
                Classification = entityV1.Classification,
                AccessType = (GroupAccessTypeV2)entityV1.AccessType,
                ExternalDirectoryObjectId = entityV1.ExternalDirectoryObjectId,
                AdditionalProperties = new GroupAdditionalPropertiesV2()
                {
                    ExternalMemberCount = entityV1.AdditionalProperties.ExternalMemberCount,
                    IsGroupMembershipHidden = entityV1.AdditionalProperties.IsGroupMembershipHidden,
                    //IsMembershipDynamic = entityV1.AdditionalProperties.IsMembershipDynamic,
                    MembershipRule = entityV1.AdditionalProperties.MembershipRule,
                    MembershipRuleProcessingState = entityV1.AdditionalProperties.MembershipRuleProcessingState,
                    SubscriptionEnabled = entityV1.AdditionalProperties.SubscriptionEnabled
                },
                MailboxSettings = new MailboxSettingsV2()
                {
                    AlwaysSubscribeMembersToCalendarEvents = entityV1.MailboxSettings.AlwaysSubscribeMembersToCalendarEvents,
                    AutoSubscribeNewMembers = entityV1.MailboxSettings.AutoSubscribeNewMembers,
                    ExternalSendersEnabled = entityV1.MailboxSettings.ExternalSendersEnabled,
                    MailboxCultureName = entityV1.MailboxSettings.MailboxCultureName
                },
                UserGroupRelationship = new UserGroupRelationshipV2()
                {
                    IsMember = entityV1.UserGroupRelationship.IsMember,
                    IsOwner = entityV1.UserGroupRelationship.IsOwner,
                    IsSubscribed = entityV1.UserGroupRelationship.IsSubscribed
                },
                GroupMemberList = entityV1.GroupMemberList.Select(member => new GroupMemberV2() { IsOwner = member.IsOwner, UserName = member.UserName }).ToList(),
                GroupResources = entityV1.GroupResources.Select(groupResource => new GroupResourceV2() { Type = (GroupResouceTypeV2)groupResource.Type, Url = groupResource.Url }).ToArray(),
                UnifiedGroupSKU = new UnifiedGroupSKUV2() { GroupType = entityV1.UnifiedGroupSKU.ToString(), IsNull = false }
            };
        }
    }
}