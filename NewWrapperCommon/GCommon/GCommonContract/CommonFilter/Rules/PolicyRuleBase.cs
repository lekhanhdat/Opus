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
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    [KnownType(typeof(AnonymousAccessRule))]
    [KnownType(typeof(AccessTimeRule))]
    [KnownType(typeof(AttributeRule))]
    [KnownType(typeof(AuditingRule))]
    [KnownType(typeof(ColumnBooleanRule))]
    [KnownType(typeof(ColumnDateTimeRule))]
    [KnownType(typeof(ColumnNumberRule))]
    [KnownType(typeof(ColumnTextRule))]
    [KnownType(typeof(ParentSiteCustomPropertyColumnTextRule))]
    [KnownType(typeof(ParentSiteCollectionCustomPropertyColumnTextRule))]
    [KnownType(typeof(CreatedByRule))]
    [KnownType(typeof(CreatedRule))]
    [KnownType(typeof(KeepHistoryVersionRule))]
    [KnownType(typeof(LikedByRule))]
    [KnownType(typeof(ListTypeRule))]
    [KnownType(typeof(MentionRule))]
    [KnownType(typeof(ModifiedByRule))]
    [KnownType(typeof(ModifiedRule))]
    [KnownType(typeof(NameAndExtentionRule))]
    [KnownType(typeof(NameRule))]
    [KnownType(typeof(OwnerRule))]
    [KnownType(typeof(ParticipationRule))]
    [KnownType(typeof(PostedByRule))]
    [KnownType(typeof(PostContentRule))]
    [KnownType(typeof(SizeRule))]
    [KnownType(typeof(TemplateRule))]
    [KnownType(typeof(TitleRule))]
    [KnownType(typeof(UrlRule))]
    [KnownType(typeof(VersionsRule))]
    [KnownType(typeof(VersioningRule))]
    [KnownType(typeof(UserAndGroupRule))]
    [KnownType(typeof(InheritanceRule))]
    [KnownType(typeof(RepliedByRule))]
    [KnownType(typeof(StubCreationTimeRule))]
    [KnownType(typeof(StubLastAccessTimeRule))]
    [KnownType(typeof(TagRule))]
    [KnownType(typeof(CustomPropertyBooleanRule))]
    [KnownType(typeof(CustomPropertyDateTimeRule))]
    [KnownType(typeof(CustomPropertyNumberRule))]
    [KnownType(typeof(CustomPropertyTextRule))]
    [KnownType(typeof(WorkflowRule))]
    [KnownType(typeof(TemplateIdRule))]
    [KnownType(typeof(LockStatusRule))]
    [KnownType(typeof(ContentUnderViewRule))]
    [KnownType(typeof(ColumnsRule))]
    [KnownType(typeof(LikedByRule))]
    [KnownType(typeof(MentionRule))]
    [KnownType(typeof(ParticipationRule))]
    [KnownType(typeof(PostContentRule))]
    [KnownType(typeof(RepliedByRule))]
    [KnownType(typeof(TagRule))]
    [KnownType(typeof(PostedByRule))]
    [KnownType(typeof(ContentTypeRule))]
    [KnownType(typeof(ContentTypeCollectionRule))]
    [KnownType(typeof(ContentTypeCollectionNameRule))]
    [KnownType(typeof(ContentTypeCollectionIdRule))]
    [KnownType(typeof(ContentTypeNameRule))]
    [KnownType(typeof(ContentTypeIdRule))]
    [KnownType(typeof(FileExtensionsRule))]
    [KnownType(typeof(ChoiceRule))]
    [KnownType(typeof(DocumentTypeRule))]
    [KnownType(typeof(ItemCountRule))]
    [KnownType(typeof(TermRule))]
    [KnownType(typeof(FSTermRule))]
    [KnownType(typeof(FilePathRule))]
    [KnownType(typeof(ParentFolderNameRule))]
    public class PolicyRuleBase
    {
        [DataMember]
        public string Value1 { get; set; }
        /// <summary>
        /// 返回rule的名称。
        /// </summary>
        /// <returns></returns>
        public string ToStringPro()
        {
            return this.GetType().Name;
        }
    }
}
