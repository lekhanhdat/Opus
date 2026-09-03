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
using System.Runtime.Serialization;

namespace DocAveOnline.WebApi.Contracts
{
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
    [KnownType(typeof(SubjectRule))]
    [KnownType(typeof(ParentListNameRule))]
    [KnownType(typeof(FSTermRule))]
    [KnownType(typeof(FilePathRule))]
    [KnownType(typeof(FileExtensionsRule))]
    [KnownType(typeof(MetadataTextColumnRule))]
    [KnownType(typeof(MetadataNumberColumnRule))]
    [KnownType(typeof(ParentFolderNameHeirarchicallyRule))]
    [KnownType(typeof(RetentionLabelRule))]
    [KnownType(typeof(StubLastActiveTimeRule))]
    [KnownType(typeof(DocumentName))]
    [KnownType(typeof(RequireCheckoutRule))]
    public class PolicyRuleBase
    {
        [DataMember]
        public string Value1 { get; set; }
        [DataMember]
        public string Type { get; set; }

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
    public class AnonymousAccessRule : PolicyRuleBase { }
    public class AttributeRule : PolicyRuleBase { }
    public class AttachmentRule : PolicyRuleBase { }
    public class AuditingRule : PolicyRuleBase { }
    public class CategoryRule : PolicyRuleBase { }
    public class ColumnBooleanRule : PolicyRuleBase { }
    public class ColumnDateTimeRule : PolicyRuleBase { }
    public class ColumnNumberRule : PolicyRuleBase { }
    public class ColumnTextRule : PolicyRuleBase { }
    public class ContentTypeRule : PolicyRuleBase { }
    public class CreatedByRule : PolicyRuleBase { }
    public class CreatedRule : PolicyRuleBase { }
    public class KeepHistoryVersionRule : PolicyRuleBase { }
    public class ListTypeRule : PolicyRuleBase { }
    public class ModifiedByRule : PolicyRuleBase { }
    public class ModifiedRule : PolicyRuleBase { }
    public class NameAndExtentionRule : PolicyRuleBase { }
    public class NameRule : PolicyRuleBase { }
    public class OwnerRule : PolicyRuleBase { }
    public class SendDateRule : PolicyRuleBase { }
    public class SizeRule : PolicyRuleBase { }
    public class TemplateRule : PolicyRuleBase { }
    public class TitleRule : PolicyRuleBase { }
    public class UrlRule : PolicyRuleBase { }
    public class VersionsRule : PolicyRuleBase { }
    public class VersioningRule : PolicyRuleBase { }
    public class UserAndGroupRule : PolicyRuleBase { }
    public class InheritanceRule : PolicyRuleBase { }
    public class StubCreationTimeRule : PolicyRuleBase { }
    public class StubLastAccessTimeRule : PolicyRuleBase { }
    public class CustomPropertyBooleanRule : PolicyRuleBase { }
    public class CustomPropertyDateTimeRule : PolicyRuleBase { }
    public class CustomPropertyNumberRule : PolicyRuleBase { }
    public class CustomPropertyTextRule : PolicyRuleBase { }
    public class WorkflowRule : PolicyRuleBase { }
    public class TemplateIdRule : PolicyRuleBase { }
    public class LockStatusRule : PolicyRuleBase { }
    public class ParentFolderNameRule : PolicyRuleBase { }
    public class SendFromRule : PolicyRuleBase { }
    public class SendToRule : PolicyRuleBase { }
    public class TermRule : PolicyRuleBase { }
    public class DisplayPathRule : PolicyRuleBase { }
    public class MailboxAddressRule : PolicyRuleBase { }
    public class SubFolderCountRule : PolicyRuleBase { }
    public class ItemCountRule : PolicyRuleBase { }
    public class FolderTypeRule : PolicyRuleBase { }
    public class ItemTypeRule : PolicyRuleBase { }
    public class SendDateUTCRule : PolicyRuleBase { }
    public class SubjectRule : PolicyRuleBase { }
    public class ParentListNameRule : PolicyRuleBase { }
    public class FilePathRule : PolicyRuleBase { }
    public class FSTermRule : PolicyRuleBase { }
    public class FileExtensionsRule : PolicyRuleBase { }
    public class MetadataTextColumnRule : PolicyRuleBase { }
    public class MetadataNumberColumnRule : PolicyRuleBase { }
    public class ParentFolderNameHeirarchicallyRule : PolicyRuleBase { }
    public class RetentionLabelRule : PolicyRuleBase { }
    public class StubLastActiveTimeRule : PolicyRuleBase { }
    public class DocumentName : PolicyRuleBase { }
    public class RequireCheckoutRule : PolicyRuleBase { }
    public enum PolicyRuleBaseType
    {
        AnonymousAccessRule,
        AttributeRule,
        AttachmentRule,
        AuditingRule,
        CategoryRule,
        ColumnBooleanRule,
        ColumnDateTimeRule,
        ColumnNumberRule,
        ColumnTextRule,
        ContentTypeRule,
        CreatedByRule,
        CreatedRule,
        KeepHistoryVersionRule,
        ListTypeRule,
        ModifiedByRule,
        ModifiedRule,
        NameAndExtentionRule,
        NameRule,
        OwnerRule,
        SendDateRule,
        SizeRule,
        TemplateRule,
        TitleRule,
        UrlRule,
        VersionsRule,
        VersioningRule,
        UserAndGroupRule,
        InheritanceRule,
        StubCreationTimeRule,
        StubLastAccessTimeRule,
        CustomPropertyBooleanRule,
        CustomPropertyDateTimeRule,
        CustomPropertyNumberRule,
        CustomPropertyTextRule,
        WorkflowRule,
        TemplateIdRule,
        LockStatusRule,
        ParentFolderNameRule,
        SendFromRule,
        SendToRule,
        TermRule,
        DisplayPathRule,
        MailboxAddressRule,
        SubFolderCountRule,
        ItemCountRule,
        FolderTypeRule,
        ItemTypeRule,
        SendDateUTCRule,
        SubjectRule,
        ParentListNameRule,
        TypeRule,
        FSTermRule,
        FilePathRule,
        FileExtensionsRule,
        MetadataTextColumnRule,
        MetadataNumberColumnRule,
        ParentFolderNameHeirarchicallyRule,
        RetentionLabelRule,
        StubLastActiveTimeRule,
        DocumentName,
        RequireCheckoutRule
    }
}

