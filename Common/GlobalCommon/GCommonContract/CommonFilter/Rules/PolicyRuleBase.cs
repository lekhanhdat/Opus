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




namespace AvePoint.GCommon.Contract.CommonFilter
{
    #region using directives
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.CommonFilter.Rules;
    using System.Runtime.Serialization;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    [KnownType(typeof(AnonymousAccessRule))]
    [KnownType(typeof(AttributeRule))]
    [KnownType(typeof(AttachmentRule))]
    [KnownType(typeof(AuditingRule))]
    [KnownType(typeof(CategoryRule))]
    [KnownType(typeof(ColumnBooleanRule))]
    [KnownType(typeof(ColumnDateTimeRule))]
    [KnownType(typeof(ColumnNumberRule))]
    [KnownType(typeof(ColumnTextRule))]
    [KnownType(typeof(ContentTypeRule))]
    [KnownType(typeof(CreatedByRule))]
    [KnownType(typeof(CreatedRule))]
    [KnownType(typeof(KeepHistoryVersionRule))]
    [KnownType(typeof(ListTypeRule))]
    [KnownType(typeof(ModifiedByRule))]
    [KnownType(typeof(ModifiedRule))]
    [KnownType(typeof(NameAndExtentionRule))]
    [KnownType(typeof(NameRule))]
    [KnownType(typeof(OwnerRule))]
    [KnownType(typeof(SendDateRule))]
    [KnownType(typeof(SizeRule))]
    [KnownType(typeof(TemplateRule))]
    [KnownType(typeof(TitleRule))]
    [KnownType(typeof(UrlRule))]
    [KnownType(typeof(VersionsRule))]
    [KnownType(typeof(VersioningRule))]
    [KnownType(typeof(UserAndGroupRule))]
    [KnownType(typeof(InheritanceRule))]
    [KnownType(typeof(StubCreationTimeRule))]
    [KnownType(typeof(StubLastAccessTimeRule))]
    [KnownType(typeof(CustomPropertyBooleanRule))]
    [KnownType(typeof(CustomPropertyDateTimeRule))]
    [KnownType(typeof(CustomPropertyNumberRule))]
    [KnownType(typeof(CustomPropertyTextRule))]
    [KnownType(typeof(WorkflowRule))]
    [KnownType(typeof(TemplateIdRule))]
    [KnownType(typeof(LockStatusRule))]
    [KnownType(typeof(ParentFolderNameRule))]
    [KnownType(typeof(ParentFolderNameHeirarchicallyRule))]
    [KnownType(typeof(ParentListNameRule))]
    [KnownType(typeof(SendFromRule))]
    [KnownType(typeof(SendToRule))]
    [KnownType(typeof(TermRule))]
    [KnownType(typeof(DisplayPathRule))]
    [KnownType(typeof(MailboxAddressRule))]
    [KnownType(typeof(SubFolderCountRule))]
    [KnownType(typeof(ItemCountRule))]
    [KnownType(typeof(FolderTypeRule))]
    [KnownType(typeof(ItemTypeRule))]
    [KnownType(typeof(SendDateUTCRule))]
    [KnownType(typeof(RequireCheckoutRule))]
    [KnownType(typeof(SubjectRule))]
    [KnownType(typeof(FilePathRule))]
    [KnownType(typeof(FSTermRule))]
    [KnownType(typeof(FileExtensionsRule))]
    [KnownType(typeof(MetadataTextColumnRule))]
    [KnownType(typeof(MetadataNumberColumnRule))]
    [KnownType(typeof(RetentionLabelRule))]
    [KnownType(typeof(SensitivityLabelRule))]
    [KnownType(typeof(StubLastActiveTimeRule))]
    [KnownType(typeof(DocumentName))]
    [KnownType(typeof(RequireCheckoutRule))]
    [KnownType(typeof(LabelPropertyTextRule))]
    [KnownType(typeof(LabelPropertyNumberRule))]
    [KnownType(typeof(LabelPropertyDateTimeRule))]
    [KnownType(typeof(LabelNameRule))]
    [KnownType(typeof(TeamsClassificationRule))]
    [KnownType(typeof(DisplayNameRule))]
    [KnownType(typeof(MemberRule))]
    [KnownType(typeof(PrivacyRule))]
    [KnownType(typeof(TeamStatusRule))]
    [KnownType(typeof(TeamsTypeRule))]
    [KnownType(typeof(SensitivityLabelFullNameRule))]
    [KnownType(typeof(DocumentModifiedRule))]
    [KnownType(typeof(ParentLibraryTextRule))]
    [KnownType(typeof(ParentLibraryBooleanRule))]
    [KnownType(typeof(ParentLibraryNumberRule))]
    [KnownType(typeof(ParentLibraryDateTimeRule))]
    [KnownType(typeof(ParentSiteCollectionTextRule))]
    [KnownType(typeof(ParentSiteCollectionBooleanRule))]
    [KnownType(typeof(ParentSiteCollectionNumberRule))]
    [KnownType(typeof(ParentSiteCollectionDateTimeRule))]
    [KnownType(typeof(PropertyBagTextRule))]
    [KnownType(typeof(PropertyBagBooleanRule))]
    [KnownType(typeof(PropertyBagNumberRule))]
    [KnownType(typeof(PropertyBagDateTimeRule))]
    [KnownType(typeof(LastestFolderDisposalDueDateRule))]
    [KnownType(typeof(OrphanedFolderRule))]
    public class PolicyRuleBase
    {
        [DataMember]
        public string Value1 { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.Value1))
            {
                return this.GetType().Name;
            }
            else
            {
                return string.Format("{0}({1})", this.GetType().Name, this.Value1);
            }
        }
    }
}
