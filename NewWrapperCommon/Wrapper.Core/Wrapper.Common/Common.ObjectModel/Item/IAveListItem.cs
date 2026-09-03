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



using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.Wrapper.Common
{
    public interface IAveListItem : IAveSecurableObject
    {
        IAveContentType ContentType { get; }
        IAveContentTypeId ContentTypeId { get; }
        string DisplayName { get; }
        AveBasePermissions EffectiveBasePermissions { get; }
        IAveFieldCollection Fields { get; }
        IAveWorkflowCollection WorkFlows { get; }
        Dictionary<string, object> FieldValues { get; }
        IAveFieldStringValues FieldValuesAsHtml { get; }
        IAveFieldStringValues FieldValuesAsText { get; }
        IAveFieldStringValues FieldValuesForEdit { get; }
        IAveFile File { get; }
        AveFileSystemObjectType FileSystemObjectType { get; }
        IAveAttachmentCollection Attachments { get; }
        int ID { get; }
        string IconOverlay { get; set; }
        AveFileLevel Level { get; }
        IAveList ParentList { get; }
        Hashtable Properties { get; }
        object this[Guid fieldId] { get; set; }
        object this[string fieldName] { get; set; }
        Guid UniqueId { get; }
        string Url { get; }
        IAveFolder Folder { get; }
        string Title { get; }
        IAveWeb Web { get; }
        IAveModerationInformation ModerationInformation { get; }
        bool MissingRequiredFields { get; }
        string Name { get; }
        IAveListItemVersionCollection Versions { get; }
        string Xml { get; }
        IAveAudit Audit { get; }
        AveFileSystemObjectType SortType { get; set; }
        bool HasPublishedVersion { get; }

        Guid Recycle();//
        void SystemUpdate(bool incrementListItemVersion);
        void SystemUpdate();
        void SystemUpdateForRecords();
        void Delete();
        
        void Update();
        void UpdateOverwriteVersion();
        void UpdateInternal(Type[] argsTypes, object[] args);

        void SetValue(Type[] argsTypes, object[] args);
        int GetTpIdByTpGuid(Guid tp_guid, Guid listId);
        Guid GetTPGuid();

        //add for MicroFeed
        void DeletePost(bool needReloadFeedManager = false);
        IAveOSocialFeedManager GetMicroFeedManager();
        Dictionary<int, List<int>> GetMicroFeedReplyID();
        Dictionary<int, List<string>> GetMicroFeedLiker();
        Dictionary<int, string> GetMicroFeedPostID();
        void GetMicroFeedMentionAndTag(ref Dictionary<int, List<string>> microFeedMentionCache, ref Dictionary<int, List<string>> microFeedMentionDisPlayCache, ref Dictionary<int, List<string>> microFeedTagCache);
        #region Add in DA6.8
        IAveLinkCollection BackwardLinks { get; }
        IAveLinkCollection ForwardLinks { get; }
        void ReplaceLink(string oldUrl, string newUrl);
        #endregion
        IAveListItemComplianceInfo ComplianceTagInfo { get; }
        /// <summary>
        /// For backup & restore.
        /// </summary>
        void SetComplianceTag(AveItemComplianceTagInfo info);
        /// <summary>
        /// For apply label directly.
        /// </summary>
        void SetComplianceTag(string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock);
    }
}
