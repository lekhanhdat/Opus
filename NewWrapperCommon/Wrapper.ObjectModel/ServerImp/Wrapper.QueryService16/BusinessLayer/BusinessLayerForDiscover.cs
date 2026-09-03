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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.QueryService
{
    internal class BusinessLayerForDiscover
    {
        /// <summary>
        /// 返回结果给Discover Query使用
        /// </summary>
        public class AveItemChangedResultCollection
        {
            public Dictionary<int, AveItemObject> Items = new Dictionary<int, AveItemObject>();
            public Dictionary<Guid, AveItemObject> SystemItems = new Dictionary<Guid, AveItemObject>();
            public Dictionary<int, List<AveItemObject>> Attachments = new Dictionary<int, List<AveItemObject>>();
            public Dictionary<Guid, AveAlertObject> FolderAlerts = new Dictionary<Guid, AveAlertObject>();
            public Dictionary<int, AveItemObject> ItemAlerts = new Dictionary<int, AveItemObject>();
            public Dictionary<Guid, EventObject> SystemItemViews = new Dictionary<Guid, EventObject>();
        }

        class AveItemObjectBasicInfo
        {
            public string DirName { get; set; }
            public string LeafName { get; set; }
            public string FullUrl { get; set; }
            public Guid DocId { get; set; }
        }

        protected static AveLogger logger = AveLogger.GetInstance(typeof (BusinessLayerForDiscover));
        private const int DocList = 1;

        /// <summary>
        /// 将version加到集合中，集合中version为从大到小排序
        /// </summary>
        /// <param name="version"></param>
        /// <param name="currentItem"></param>
        public void AddVersionToOrderedItemVersions(AveVersionObject version, AveItemObject currentItem)
        {
            for (var i = 0; i < currentItem.VersionObjs.Count; i++) // VersionObjs 集合为从大到小排序。
            {
                if (currentItem.VersionObjs[i].Uiversion == version.Uiversion)
                {
                    currentItem.VersionObjs[i] = version; // 也是为了current version 上属性覆盖。cuurent version 也会在集合中
                    return;
                }
                if (version.Uiversion > currentItem.VersionObjs[i].Uiversion)
                {
                    currentItem.VersionObjs.Insert(i, version);
                    return;
                }
            }
            currentItem.VersionObjs.Add(version);
        }

        private AveSecurityObject GetOrAddScopeSecurity(ICollection<AveSecurityObject> securitys, Guid scopeId)
        {
            foreach (var asc in securitys.Where(asc => asc.ScopeId == scopeId))
            {
                return asc;
            }
            var security = new AveSecurityObject
            {
                ScopeId = scopeId,
                ObjectType = SecurityType.Scope
            };
            securitys.Add(security);
            return security;
        }

        [AveQueryServiceBase.QueryReview("2012/05/22", "Oliver Luo")]
        private void DeleteAllRelatedRoleByRoleId(Dictionary<int, List<AveSecurityObject>> securityChanges, int roleId)
        {
            var needDeleteObjects = new List<AveSecurityObject>();
            foreach (var kvp in securityChanges)
            {
                needDeleteObjects.Clear();
                if (kvp.Key != AveSecurityObject.RoleChangeId && kvp.Key != AveSecurityObject.ScopeChangeId)
                {
                    // we shoud delete scope and principle relate current role
                    needDeleteObjects.AddRange(kvp.Value.Where(asc => asc.RoleId == roleId));
                    needDeleteObjects.ForEach(obj => kvp.Value.Remove(obj));
                }
            }
        }

        [AveQueryServiceBase.QueryReview("2012/05/22", "Oliver Luo")]
        private void ScopeSecurityChange(AveSecurityObject securityObj, IDictionary<int, List<AveSecurityObject>> mSecurityChanges)
        {
            if (securityObj.ScopeId == Guid.Empty)
            {
                return;
            }
            List<AveSecurityObject> scopeSecuritys = null;
            mSecurityChanges.TryGetValue(AveSecurityObject.ScopeChangeId, out scopeSecuritys);
            if (scopeSecuritys == null)
            {
                scopeSecuritys = new List<AveSecurityObject>();
                mSecurityChanges.Add(AveSecurityObject.ScopeChangeId, scopeSecuritys);
            }

            var scopeSecurity = GetOrAddScopeSecurity(scopeSecuritys, securityObj.ScopeId);

            if (scopeSecurity.ChangeType == ChangeType.Add)
            {
                if (securityObj.ChangeType == ChangeType.Delete)
                {
                    scopeSecuritys.Remove(scopeSecurity);
                    return;
                }
            }
            else
            {
                scopeSecurity.ChangeType = securityObj.ChangeType;
            }
            if (scopeSecurity.ChangeType == ChangeType.Delete)
            {
                return;
            }
            scopeSecurity.RoleId = securityObj.RoleId;
        }

        private void AssignmentSecurityChange(AveSecurityObject securityObj, Dictionary<int, List<AveSecurityObject>> securityChanges)
        {
            if (securityObj.PrincipleId < 0 || securityObj.RoleId < 0)
            {
                return;
            }
            List<AveSecurityObject> securitys;
            securityChanges.TryGetValue(securityObj.PrincipleId, out securitys);
            if (securitys == null)
            {
                securitys = new List<AveSecurityObject>();
                securityChanges.Add(securityObj.PrincipleId, securitys);
            }
            var security = TryGetAssignmentSecurity(securitys, securityObj.RoleId);
            if (security.ChangeType == ChangeType.Add)
            {
                //todo:wbhu,逻辑做了修正，原来的不太对
                if (securityObj.ChangeType == ChangeType.Delete)
                    //if (security.ChangeType == ChangeType.Delete)
                {
                    securitys.Remove(security);
                    DeleteAllRelatedRoleByRoleId(securityChanges, securityObj.RoleId);
                    return;
                }
            }
            else
            {
                security.ChangeType = securityObj.ChangeType;
            }
            if (security.ChangeType == ChangeType.Delete)
            {
                DeleteAllRelatedRoleByRoleId(securityChanges, securityObj.RoleId);
            }
            else
            {
                security.ScopeId = securityObj.ScopeId;
            }
        }

        [AveQueryServiceBase.QueryReview("2012/05/22", "Oliver Luo")]
        private AveSecurityObject TryGetAssignmentSecurity(ICollection<AveSecurityObject> Securitys, int roleId)
        {
            var security = new AveSecurityObject();
            foreach (var asc in Securitys.Where(asc => asc.RoleId == roleId))
            {
                return asc;
            }
            security.RoleId = roleId;
            security.ObjectType = SecurityType.Assignment;
            Securitys.Add(security);
            return security;
        }


        private void RoleSecurityChange(AveSecurityObject securityObject, Dictionary<int, List<AveSecurityObject>> securityChanges)
        {
            List<AveSecurityObject> roleSecuritys;
            securityChanges.TryGetValue(AveSecurityObject.RoleChangeId, out roleSecuritys);
            if (roleSecuritys == null)
            {
                roleSecuritys = new List<AveSecurityObject>();
                securityChanges.Add(AveSecurityObject.RoleChangeId, roleSecuritys);
            }

            AveSecurityObject security = TryGetRoleSecurity(roleSecuritys, securityObject.RoleId);

            if (security.ChangeType == ChangeType.Add)
            {
                if (securityObject.ChangeType == ChangeType.Delete)
                {
                    roleSecuritys.Remove(security);
                    DeleteAllRelatedRoleByRoleId(securityChanges, securityObject.RoleId);
                    return;
                }
            }
            else
            {
                security.ChangeType = securityObject.ChangeType;
            }
            if (security.ChangeType == ChangeType.Delete)
            {
                DeleteAllRelatedRoleByRoleId(securityChanges, securityObject.RoleId);
                return;
            }
            security.ScopeId = securityObject.ScopeId;
        }

        [AveQueryServiceBase.QueryReview("2012/05/22", "Oliver Luo")]
        private AveSecurityObject TryGetRoleSecurity(List<AveSecurityObject> securitys, int roleId)
        {
            foreach (var asc in securitys.Where(asc => asc.RoleId == roleId))
            {
                return asc;
            }
            var security = new AveSecurityObject
            {
                RoleId = roleId,
                ObjectType = SecurityType.Role
            };
            securitys.Add(security);
            return security;
        }

        public void AddSecurityChangeObjectToCollection(AveSecurityObject securityObject, Dictionary<int, List<AveSecurityObject>> securityChanges)
        {
            switch (securityObject.ObjectType)
            {
                case SecurityType.Assignment:
                    AssignmentSecurityChange(securityObject, securityChanges);
                    break;
                case SecurityType.Scope: //break inherate
                    ScopeSecurityChange(securityObject, securityChanges);
                    break;
                case SecurityType.Role:
                    RoleSecurityChange(securityObject, securityChanges);
                    break;
            }
        }

        public void AddAlertChangeInfoToCollection(IDictionary<Guid, AveAlertObject> changeAlerts, Guid alertId, ChangeType changeType, bool isAlertDeleted)
        {
            if (changeAlerts.ContainsKey(alertId))
            {
                var cachedAlert = changeAlerts[alertId];
                if (cachedAlert.ChangeType == ChangeType.Add)
                {
                    if (changeType == ChangeType.Delete)
                    {
                        changeAlerts.Remove(alertId);
                    }
                }
            }
            else
            {
                if (isAlertDeleted)
                {
                    //this alert is delete we can't know the alert belong to this list or folder
                    //or it is folder alert
                    return;
                }
                var alert = new AveAlertObject
                {
                    Id = alertId,
                    ChangeType = changeType
                };
                changeAlerts.Add(alertId, alert);
            }
        }

        public void AddChangedViewToCollection(IDictionary<Guid, AveViewObject> changeViews, Guid viewId, AveViewObject viewChange, ChangeType changeType)
        {
            if (!changeViews.ContainsKey(viewId))
            {
                changeViews.Add(viewId, viewChange);
            }
            viewChange = changeViews[viewId];
            if (viewChange.ChangeType == ChangeType.Add)
            {
                if (changeType == ChangeType.Delete)
                {
                    changeViews.Remove(viewId);
                }
            }
            else
            {
                viewChange.ChangeType = changeType;
            }
        }

        public void AddChangedContentTypeToCollection(Dictionary<byte[], AveContentTypeObject> contentTypeChanges, byte[] contentTypeId, ChangeType changeType, ChangeObjectType objType)
        {
            AveContentTypeObject contentTypeChange;
            if (contentTypeChanges.TryGetValueByByteArray(contentTypeId, out contentTypeChange))
            {
                contentTypeChange = new AveContentTypeObject
                {
                    ContentTypeId = contentTypeId
                };
                if (objType == ChangeObjectType.Field)
                {
                    contentTypeChange.IsColumn = true;
                }
                contentTypeChange.SchemaXml = string.Empty;
                contentTypeChange.Name = string.Empty;
                contentTypeChanges.Add(contentTypeId, contentTypeChange);
            }
            if (contentTypeChange.ChangeType == ChangeType.Add)
            {
                if (changeType == ChangeType.Delete)
                {
                    contentTypeChanges.RemoveByByteArray(contentTypeId);
                }
            }
            else
            {
                contentTypeChange.ChangeType = changeType;
            }
        }

        #region Item Changed private methods

        [AveQueryServiceBase.QueryReview("2012/05/22", "Oliver Luo")]
        private void HandleItemAlert(EventObject ev, ChangeType changeType, IDictionary<int, AveItemObject> items, IDictionary<int, AveItemObject> itemAlerts)
        {
            var alertId = ev.Guid0;
            var itemId = ev.ItemId;

            AveItemObject item;
            if (!itemAlerts.ContainsKey(itemId))
            {
                if (items.ContainsKey(itemId))
                {
                    item = items[itemId];
                }
                else
                {
                    item = new AveItemObject
                    {
                        ID = itemId
                    };
                    //AlertObjs will never be null
                    itemAlerts.Add(itemId, item);
                }
            }
            else
            {
                item = itemAlerts[itemId];
            }
            if (!item.AlertObjs.ContainsKey(alertId))
            {
                item.AlertObjs.Add(alertId, new AveAlertObject
                {
                    Id = alertId
                });
            }
            var alert = item.AlertObjs[alertId];
            if (alert.ChangeType == ChangeType.Add)
            {
                if (changeType == ChangeType.Delete)
                {
                    item.AlertObjs.Remove(alertId);
                }
            }
            else
            {
                alert.ChangeType = changeType;
            }
        }

        [AveQueryServiceBase.QueryReview("2012/05/22", "Oliver Luo")]
        private void HandleFolderAlert(EventObject ev, ChangeType changeType, Dictionary<Guid, AveAlertObject> folderAlerts)
        {
            var alertId = ev.Guid0;
            AveAlertObject alert;
            if (!folderAlerts.TryGetValue(alertId, out alert))
            {
                if (changeType != ChangeType.Delete) //we do not know the deleted alert belong to list or a folder
                {
                    alert = new AveAlertObject
                    {
                        Id = alertId
                    };
                    folderAlerts.Add(alertId, alert);
                }
            }
            if (alert == null)
            {
                return;
            }
            if (alert.ChangeType == ChangeType.Add)
            {
                if (changeType == ChangeType.Delete)
                {
                    folderAlerts.Remove(alertId);
                }
            }
            else
            {
                alert.ChangeType = changeType;
            }
        }

        [AveQueryServiceBase.QueryReview("2012/05/22", "Oliver Luo")]
        private string GetFullName(EventObject ev, DocObject doc, ChangeObjectType objectType)
        {
            if (string.IsNullOrEmpty(ev.ItemFullUrl))
            {
                return doc.Id == Guid.Empty ? string.Empty : (doc.DirName + "/" + doc.LeafName).Trim('/');
            }
            if (objectType == ChangeObjectType.Item && !string.IsNullOrEmpty(doc.DirName) && !string.IsNullOrEmpty(doc.LeafName))
            {
                return (doc.DirName + "/" + doc.LeafName).Trim('/');
            }
            return ev.ItemFullUrl;
        }

        [AveQueryServiceBase.QueryReview("2012/05/22", "Oliver Luo")]
        private bool InvalidDirName(string dirName, DocObject doc)
        {
            if (doc.Id == Guid.Empty)
            {
                return false;
            }
            return !dirName.Equals(doc.DirName.Trim('/'), StringComparison.OrdinalIgnoreCase);
        }

        [AveQueryServiceBase.QueryReview("2012/05/22", "Oliver Luo")]
        private Guid GetDocId(EventObject ev, DocObject tempDoc)
        {
            return ev.DocId == Guid.Empty ? tempDoc.Id : ev.DocId;
        }

        [AveQueryServiceBase.QueryReview("2012/05/22", "Oliver Luo")]
        private string GetItemName(EventObject ev, DocObject tempDoc)
        {
            if (string.IsNullOrEmpty(ev.ItemName))
            {
                return string.IsNullOrEmpty(tempDoc.LeafName) ? string.Empty : tempDoc.LeafName;
            }
            return ev.ItemName;
        }

        private string GetFullUrl(string dirName, string leafName)
        {
            return (dirName + "/" + leafName).Trim('/');
        }

        [AveQueryServiceBase.QueryReview("2012/05/22", "Oliver Luo")]
        private int GetItemId(string itemFullUrl, out string leafName)
        {
            var strs = itemFullUrl.Split('/');
            leafName = strs[strs.Length - 1];
            return Convert.ToInt32(strs[strs.Length - 2], CultureInfo.InvariantCulture);
        }

        [AveQueryServiceBase.QueryReview("2012/05/22", "Oliver Luo")]
        private AveItemObject GetAttachment(ICollection<AveItemObject> attachments, Guid docId)
        {
            foreach (var attach in attachments.Where(attach => attach.DocID == docId))
            {
                return attach;
            }
            var attachment = new AveItemObject
            {
                DocID = docId
            };
            attachments.Add(attachment);
            return attachment;
        }

        [AveQueryServiceBase.QueryReview("2012/05/22", "Oliver Luo")]
        private void AddAttachmentForIB(Dictionary<int, List<AveItemObject>> attachments, EventObject ev, DocObject tempDoc, string itemFullUrl, ChangeType changeType)
        {
            var docId = GetDocId(ev, tempDoc);
            var timeLastModified = ev.TimeLastModified;

            string leafName;
            var itemId = GetItemId(itemFullUrl, out leafName);

            if (!attachments.ContainsKey(itemId))
            {
                attachments.Add(itemId, new List<AveItemObject>());
            }

            var attachment = GetAttachment(attachments[itemId], docId);

            if (tempDoc.Id == Guid.Empty) //the attachment is deleted from recycle
            {
                attachment.ChangeType = ChangeType.Delete;
                attachment.LeafName = leafName;
                attachment.DirName = itemFullUrl.Replace(leafName, "").TrimEnd('/');
                attachment.TimeLastModified = timeLastModified;
                return;
            }

            if (string.IsNullOrEmpty(attachment.LeafName))
            {
                AveDiscoverReader.GetInstance().ReadAttachmentContent(attachment, tempDoc);
            }

            #region Analyse Event

            if (attachment.ChangeType == ChangeType.Add || attachment.ChangeType == ChangeType.Restore)
            {
                if (changeType == ChangeType.Delete)
                {
                    attachments[itemId].Remove(attachment);
                }
            }
            else
            {
                if (attachment.ChangeType == ChangeType.Delete && changeType == ChangeType.Restore)
                {
                    attachments[itemId].Remove(attachment);
                }
                else
                {
                    attachment.ChangeType = changeType;
                }
            }

            #endregion
        }

        [AveQueryServiceBase.QueryReview("2012/05/22", "Oliver Luo")]
        public AveItemObject GetParentFolder(string dirName, AveItemObject rootFolder, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            var listRootFolderUrl = rootFolder.FullUrl;

            if (dirName.Equals(listRootFolderUrl))
            {
                return rootFolder;
            }
            if (!dirName.Contains(listRootFolderUrl))
            {
                return null;
            }
            var foldersDirName = dirName.Substring(listRootFolderUrl.Length).Trim('/');

            var tempParentFolder = rootFolder;
            foreach (var str in foldersDirName.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!FolderExist(ref tempParentFolder, str))
                {
                    var tempFolder = new AveItemObject
                    {
                        LeafName = str,
                        DirName = tempParentFolder.FullUrl.Trim('/'),
                        ObjType = ItemType.Folder
                    };
                    tempFolder.FullUrl = (tempFolder.DirName + "/" + tempFolder.LeafName).Trim('/');
                    if (tempParentFolder.SubFolderObjs == null)
                    {
                        tempParentFolder.SubFolderObjs = new List<AveItemObject>();
                    }
                    tempParentFolder.SubFolderObjs.Add(tempFolder);
                    noPropertyFolders.Add(tempFolder.FullUrl, tempFolder);
                    tempParentFolder = tempFolder;
                }
            }
            return tempParentFolder;
        }

        //
        [AveQueryServiceBase.QueryReview("2012/05/22", "Oliver Luo")]
        private void DoRecycleBin(AveItemObject parentFolder, string fullName, string itemName, Guid Id, int itemId, Dictionary<int, AveItemObject> items, DateTime eventTime, Dictionary<string, AveItemObject> noPropertyFolders, string dirName, string modifyBy)
        {
            AveItemObject folder = null;
            foreach (var afc in parentFolder.SubFolderObjs.Where(afc => afc.FullUrl.Equals(fullName, StringComparison.OrdinalIgnoreCase)))
            {
                folder = afc;
                if (noPropertyFolders.ContainsKey(fullName))
                {
                    noPropertyFolders.Remove(fullName);
                }
                break;
            }
            if (folder == null)
            {
                AveItemObject item = null;

                #region Find Item

                if (itemId == 0)
                {
                    foreach (var aic in parentFolder.SubItemObjs.Where(aic => aic.DocID == Id))
                    {
                        item = aic;
                        break;
                    }
                }
                else
                {
                    if (items != null && items.ContainsKey(itemId))
                    {
                        item = items[itemId];
                    }
                }

                #endregion

                if (item == null) //no current item
                {
                    item = new AveItemObject
                    {
                        DocID = Id,
                        FullUrl = fullName,
                        LeafName = fullName.Substring(fullName.LastIndexOf('/') + 1),
                        ItemName = itemName,
                        ChangeType = ChangeType.Delete,
                        EventTime = eventTime,
                        ID = itemId,
                        DirName = dirName,
                        ModifyBy = modifyBy,
                    };

                    if (!parentFolder.NoTypeDeleteItems.ContainsKey(fullName))
                    {
                        parentFolder.NoTypeDeleteItems.Add(fullName, item);
                    }
                }
                else //item finded
                {
                    item.ChangeType = ChangeType.Delete;
                    item.EventTime = eventTime;
                }
            }
            else //folder finded 
            {
                folder.ChangeType = ChangeType.Delete;
                folder.EventTime = eventTime;
            }
        }

        [AveQueryServiceBase.QueryReview("2012/05/22", "Oliver Luo")]
        public AveItemObject GetCurrentFolder(AveItemObject parent, string fullUrl, bool deleteNoPropertyFolders, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            AveItemObject folder = null;

            foreach (var afc in parent.SubFolderObjs.Where(afc => afc.FullUrl.Equals(fullUrl, StringComparison.OrdinalIgnoreCase)))
            {
                folder = afc;
                if (deleteNoPropertyFolders && noPropertyFolders.ContainsKey(fullUrl))
                {
                    RemoveNoPropertyFolder(fullUrl, noPropertyFolders);
                }
                break;
            }

            if (folder == null)
            {
                folder = new AveItemObject();
                parent.SubFolderObjs.Add(folder);
            }
            return folder;
        }

        [AveQueryServiceBase.QueryReview("2012/05/22", "Oliver Luo")]
        private bool FolderExist(ref AveItemObject tempParentFolder, string str)
        {
            foreach (var folder in tempParentFolder.SubFolderObjs.Where(folder => folder.LeafName.Equals(str, StringComparison.OrdinalIgnoreCase)))
            {
                tempParentFolder = folder;
                return true;
            }
            return false;
        }

        private void RemoveNoPropertyFolder(string fullUrl, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            if (noPropertyFolders.ContainsKey(fullUrl))
            {
                noPropertyFolders.Remove(fullUrl);
            }
        }

        [AveQueryServiceBase.QueryReview("2012/05/22", "Oliver Luo")]
        private void AnalyseFolderEvent(AveItemObject parentFolder, AveItemObject folder, NativeChangeType nativeChageType, ChangeType changeType, string sourceFullUrl, Dictionary<int, AveItemObject> items, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            //和(nativeChageType & NativeChangeType.Rename) != 0相比, Enum.HasFlag()在执行速度上慢两个数量级左右(10^2)[800-900ms/10M vs 7-8ms/10M], 如果此处成为效率瓶颈, 请直接使用位运算代替。
            //if ((nativeChageType & NativeChangeType.Rename) != 0)//可读性差，速度快
            if (nativeChageType.HasFlag(NativeChangeType.Rename))//可读性好并且使用于所有枚举类型
            {
                //当用Sharepoint Designer在同一个list下去move一个folder的时候，触发的事件即为rename，在这里标记为true。
                folder.isRename = true; //For replicator
                folder.ChangeType = ChangeType.Edit;
                return;
            }
            //当用Sharepoint designer去跨list move一个folder的时候，触发的事件为move into,也让其走rename逻辑。
            if (nativeChageType == NativeChangeType.MoveInto)
            {
                folder.isRename = true;
            }
            if (nativeChageType == NativeChangeType.AssignmentAdd || nativeChageType == NativeChangeType.AssignmentDelete || nativeChageType == (NativeChangeType.RoleAdd | NativeChangeType.AssignmentAdd))
            {
                folder.ItemPermissionChanged = true;
                folder.RoleAssignmentsChangeType = ChangeType.Edit;
                return;
            }
            //folder.ItemPermissionChanged = false;   16 has one more event   ChangeSystemModify.
            if (folder.ChangeType == ChangeType.Add || folder.ChangeType == ChangeType.Restore)
            {
                if (changeType == ChangeType.Delete)
                {
                    folder.ChangeTypeBeforeDelete = folder.ChangeType;
                    folder.ChangeType = ChangeType.Delete;
                }
            }
            else
            {
                if (folder.ChangeType == ChangeType.Delete && changeType == ChangeType.Restore)
                {
                    folder.ChangeType = folder.ChangeTypeBeforeDelete;
                    if (folder.ChangeType == ChangeType.None)
                    {
                        parentFolder.SubFolderObjs.Remove(folder);
                        if (items != null && folder.ID.HasValue)
                        {
                            items.Remove(folder.ID.Value);
                        }
                    }
                }
                else
                {
                    if (changeType == ChangeType.Delete)
                    {
                        folder.ChangeTypeBeforeDelete = folder.ChangeType;
                    }
                    folder.ChangeType = changeType;
                }
            }
        }

        [AveQueryServiceBase.QueryReview("2012/05/22", "Oliver Luo")]
        private void AnalyseItemEvent(AveItemObject parentFolder, AveItemObject item, NativeChangeType nativeChageType, ChangeType changeType, string fullName, Dictionary<int, AveItemObject> items)
        {
            //和(nativeChageType & NativeChangeType.Rename) != 0相比, Enum.HasFlag()在执行速度上慢两个数量级左右(10^2)[800-900ms/10M vs 7-8ms/10M], 如果此处成为效率瓶颈, 请直接使用位运算代替。
            //if ((nativeChageType & NativeChangeType.Rename) != 0)//可读性差，速度快
            if (nativeChageType.HasFlag(NativeChangeType.Rename)|| nativeChageType == NativeChangeType.MoveInto)
            {
                //当用Sharepoint designer在同一个list下move一个document的时候，触发的事件为rename。
                //当用Sharepoint designer去跨list move一个document的时候，触发的事件为move into,也让其走rename逻辑。
                item.isRename = true;
                item.ChangeType = ChangeType.Edit; //we regard rename as edit
                item.ItemName = fullName.Substring(fullName.LastIndexOf('/') + 1);
                item.FullUrl = fullName;
                return;
            }
            if (nativeChageType == NativeChangeType.AssignmentAdd ||
                nativeChageType == NativeChangeType.AssignmentDelete ||
                nativeChageType == (NativeChangeType.RoleAdd | NativeChangeType.AssignmentAdd))
            {
                item.ItemPermissionChanged = true;
                item.RoleAssignmentsChangeType = ChangeType.Edit;
                return;
            }
            //当先checkout,之后change permission,然后discard checkout时，下面的else代码将itempermissionchanged属性给覆盖了，所以将其注释
            //else
            //{
            //    item.ItemPermissionChanged = false;
            //}
            if (item.ChangeType == ChangeType.Add || item.ChangeType == ChangeType.Restore)
            {
                if (changeType == ChangeType.Delete)
                {
                    item.ChangeTypeBeforeDelete = item.ChangeType;
                    item.ChangeType = ChangeType.Delete;
                }
            }
            else
            {
                if (item.ChangeType == ChangeType.Delete && changeType == ChangeType.Restore)
                {
                    item.ChangeType = item.ChangeTypeBeforeDelete;
                    if (item.ChangeType == ChangeType.None)
                    {
                        parentFolder.SubItemObjs.Remove(item);
                        if (items != null && item.ID.HasValue)
                        {
                            items.Remove(item.ID.Value);
                        }
                    }
                }
                else
                {
                    if (changeType == ChangeType.Delete)
                    {
                        item.ChangeTypeBeforeDelete = item.ChangeType;
                    }
                    item.ChangeType = changeType;
                }
            }
        }

        private bool HandleDeletedItemOrFolder(Dictionary<string, AveItemObject> noPropertyFolders, DocObject tempDoc, EventObject ev, AveItemObject parentFolder, AveItemObjectBasicInfo itemBasicInfo, DateTime eventTime, Dictionary<int, AveItemObject> itemAlerts, Dictionary<int, AveItemObject> items)
        {
            #region deleted item or folder

            if (tempDoc.Id == Guid.Empty) //id is null ,delete from recyclebin
            {
                var modifyBy = string.Empty;
                if (!string.IsNullOrEmpty(ev.ModifiedBy))
                {
                    modifyBy = ev.ModifiedBy;
                }
                if (ev.ItemId == 0) //System file or folder delete  
                {
                    DoRecycleBin(parentFolder, itemBasicInfo.FullUrl, string.Empty, itemBasicInfo.DocId, 0, null, eventTime, noPropertyFolders, itemBasicInfo.DirName, modifyBy);
                }
                else
                {
                    var itemId = ev.ItemId;
                    if (itemAlerts.ContainsKey(itemId))
                    {
                        itemAlerts.Remove(itemId);
                    }
                    DoRecycleBin(parentFolder, itemBasicInfo.FullUrl, itemBasicInfo.LeafName, itemBasicInfo.DocId, itemId, items, eventTime, noPropertyFolders, itemBasicInfo.DirName, modifyBy);
                }
                return true;
            }

            #endregion

            return false;
        }

        private AveItemObjectBasicInfo GetItemObjectBasicInfo(EventObject ev, DocObject tempDoc, ChangeObjectType objectType)
        {
            var itemBasicInfo = new AveItemObjectBasicInfo();
            itemBasicInfo.FullUrl = GetFullName(ev, tempDoc, objectType);
            itemBasicInfo.DirName = itemBasicInfo.FullUrl.LastIndexOf('/') > 0 ? itemBasicInfo.FullUrl.Substring(0, itemBasicInfo.FullUrl.LastIndexOf('/')) : itemBasicInfo.FullUrl;
            itemBasicInfo.LeafName = GetItemName(ev, tempDoc);
            itemBasicInfo.DocId = GetDocId(ev, tempDoc);
            return itemBasicInfo;
        }

        private static void HandleChangedView(EventObject ev, Dictionary<Guid, EventObject> systemItemViews, ChangeType changeType)
        {
            var viewId = ev.Guid0;
            if (systemItemViews.ContainsKey(viewId) && changeType == ChangeType.Delete)
            {
                systemItemViews.Remove(viewId);
            }
            else
            {
                systemItemViews[viewId] = ev;
            }
        }

        private static void HandleItemRoleAssignment(NativeChangeType nativeChangeType, EventObject ev, DateTime eventTime, AveItemObject item)
        {
            if (nativeChangeType == NativeChangeType.AssignmentDelete)
            {
                if (ev.Int0 != 0)
                {
                    var deleteRoleAssignment = new AveSecurityObject
                    {
                        ObjectType = SecurityType.Assignment,
                        PrincipleId = ev.Int0
                    };
                    // 删除RoleAssignmet时，
                    // int0存放principalID,int1存放RoleID
                    if (ev.Int1 != 0)
                    {
                        deleteRoleAssignment.RoleId = ev.Int1;
                    }
                    //如果int1为Null，说明把该user/group的权限全部移除了
                    else
                    {
                        deleteRoleAssignment.RoleId = -1;
                    }
                    deleteRoleAssignment.EventTime = eventTime;
                    item.DeleteRoleAssignments.Add(deleteRoleAssignment);
                }
            }
        }

        private void HandleChangedItemOrDocument(AveListObject listObject, IAveDiscoverReader discoverReader, int docLibRowId, Dictionary<Guid, AveItemObject> systemItems, AveItemObjectBasicInfo itemBasicInfo, Dictionary<int, AveItemObject> items, Dictionary<int, AveItemObject> itemAlerts, DocObject tempDoc, AveItemObject parentFolder, DateTime eventTime, EventObject ev, NativeChangeType nativeChangeType, ChangeType changeType)
        {
            AveItemObject item;
            var hasAddProperty = false;

            if (docLibRowId == 0) //System item
            {
                if (!systemItems.ContainsKey(itemBasicInfo.DocId))
                {
                    item = new AveItemObject();
                    hasAddProperty = true;
                }
                else
                {
                    item = systemItems[itemBasicInfo.DocId];
                }
            }
            else if (!items.ContainsKey(docLibRowId))
            {
                hasAddProperty = true;
                if (itemAlerts.ContainsKey(docLibRowId)) //this item may related an alert,when we do alert change,we cached it
                {
                    item = itemAlerts[docLibRowId];
                    itemAlerts.Remove(docLibRowId);
                }
                else
                {
                    item = new AveItemObject();
                }
            }
            else
            {
                item = items[docLibRowId];
            }
            if (hasAddProperty) //Item is null created
            {
                discoverReader.ReadItemContentForIB(item, tempDoc);
                if (listObject != null && listObject.Type == DocList)
                {
                    item.ObjType = ItemType.Document;
                    item.SourceName = itemBasicInfo.LeafName;
                }
                else
                {
                    item.ObjType = ItemType.Item;
                }
                item.FullUrl = itemBasicInfo.FullUrl;
                item.IsCurrentVersion = true;
                item.ItemName = itemBasicInfo.LeafName;
                parentFolder.SubItemObjs.Add(item);
                if (docLibRowId != 0)
                {
                    items.Add(docLibRowId, item);
                }
                else
                {
                    systemItems.Add(itemBasicInfo.DocId, item);
                }
            }
            item.EventTime = eventTime;
            if (!string.IsNullOrEmpty(ev.ModifiedBy))
            {
                item.ModifyBy = ev.ModifiedBy;
            }
            //把document与listItem的RoleAssignment删除记录load出来
            HandleItemRoleAssignment(nativeChangeType, ev, eventTime, item);
            AnalyseItemEvent(parentFolder, item, nativeChangeType, changeType, itemBasicInfo.FullUrl, items);
        }

        private void HandleChangedFolder(IAveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders, AveItemObject parentFolder, AveItemObjectBasicInfo itemBasicInfo, DocObject tempDoc, int docLibRowId, Dictionary<int, AveItemObject> items, DateTime eventTime, EventObject ev, NativeChangeType nativeChangeType, ChangeType changeType)
        {
            var folder = GetCurrentFolder(parentFolder, itemBasicInfo.FullUrl, true, noPropertyFolders);
            discoverReader.ReadItemContentForIB(folder, tempDoc);
            folder.IsCurrentVersion = true;
            folder.FullUrl = itemBasicInfo.FullUrl;
            folder.ItemName = itemBasicInfo.LeafName;
            folder.SourceName = itemBasicInfo.LeafName;
            folder.LeafName = itemBasicInfo.FullUrl.Substring(itemBasicInfo.DirName.Length + 1);
            folder.DirName = itemBasicInfo.DirName;
            folder.ObjType = ItemType.Folder;
            if (docLibRowId > 0 && !items.ContainsKey(docLibRowId))
            {
                items.Add(docLibRowId, folder);
            }
            folder.EventTime = eventTime;
            if (!string.IsNullOrEmpty(ev.ModifiedBy))
            {
                folder.ModifyBy = ev.ModifiedBy;
            }
            //把folder的RoleAssignment删除记录load出来
            HandleItemRoleAssignment(nativeChangeType, ev, eventTime, folder);
            AnalyseFolderEvent(parentFolder, folder, nativeChangeType, changeType, GetFullUrl(itemBasicInfo.DirName, itemBasicInfo.LeafName), items, noPropertyFolders);
        }

        #region custom discover items

        private void AddExtraAttachmentForIB(IDictionary<int, List<AveItemObject>> attachments, string itemFullUrl, ChangeType changeType, IDictionary<string, object> itemDoc)
        {
            var docId = (Guid) itemDoc["Id"];
            string leafName;
            var itemId = GetItemId(itemFullUrl, out leafName);

            if (!attachments.ContainsKey(itemId))
            {
                attachments.Add(itemId, new List<AveItemObject>());
            }
            if (attachments[itemId].Any(attach => attach.DocID == docId))
            {
                return;
            }
            var attachment = new AveItemObject
            {
                DocID = docId
            };
            attachments[itemId].Add(attachment);
            InitItemObject(attachment, itemDoc);
            attachment.ChangeType = changeType;
        }

        private void InitItemObject(AveItemObject obj, IDictionary<string, object> itemDocInfo)
        {
            obj.DocID = (Guid) itemDocInfo["Id"];
            obj.DirName = (string) itemDocInfo["DirName"];
            obj.SourceName = obj.LeafName = obj.ItemName = (string) itemDocInfo["LeafName"];
            obj.FullUrl = (obj.DirName + "/" + obj.LeafName).Trim('/');
            obj.TimeLastModified = (DateTime) itemDocInfo["TimeLastModified"];
            obj.Uiversion = (int) itemDocInfo["UIVersion"];
            if (!(itemDocInfo["DoclibRowId"] is DBNull))
            {
                obj.ID = (int?) itemDocInfo["DoclibRowId"];
            }
            obj.Type = (byte) itemDocInfo["Type"];
            obj.Level = (byte) itemDocInfo["Level"];
        }

        public void HandleExtraItems(AveItemObject rootFolder, AveListObject listObject, Dictionary<string, AveItemObject> noPropertyFolders, List<Dictionary<string, object>> extraItemInfos, Dictionary<int, AveItemObject> items, Dictionary<Guid, AveItemObject> systemItems, string attachmentUrl, Dictionary<int, List<AveItemObject>> attachments)
        {
            //vault模块因item export failed需要IB重新备份。
            if (extraItemInfos != null && extraItemInfos.Count > 0)
            {
                foreach (var itemInfo in extraItemInfos)
                {
                    var failDocLibRowId = itemInfo["DoclibRowId"] is DBNull ? -1 : (int) itemInfo["DoclibRowId"];
                    var failDocId = itemInfo["Id"] is DBNull ? Guid.Empty : (Guid) itemInfo["Id"];
                    if (itemInfo["Id"] is DBNull || itemInfo["LeafName"] is DBNull || items.ContainsKey(failDocLibRowId) || systemItems.ContainsKey(failDocId))
                    {
                        continue;
                    }
                    var itemName = (string) itemInfo["LeafName"];
                    var dirName = ((string) itemInfo["DirName"]).Trim('/');
                    var fullName = (dirName + "/" + itemName).Trim('/');
                    var hasStream = (bool) ((int) itemInfo["HasStream"] == 1 ? true : false);
                    var sizeObj = itemInfo["Size"];
                    var size = sizeObj != null && sizeObj != DBNull.Value ? (int) sizeObj : 0;
                    var docFlags = (int?) itemInfo["DocFlags"];
                    var deleteTransactionId = (byte[]) itemInfo["DeleteTransactionId"];
                    //attachment:
                    if (!string.IsNullOrEmpty(attachmentUrl) && fullName.StartsWith(attachmentUrl, StringComparison.OrdinalIgnoreCase) && listObject.Type != DocList) //Attachment,Library中可以创建出名为“Attachments”的folder
                    {
                        AddExtraAttachmentForIB(attachments, fullName, ChangeType.Edit, itemInfo);
                        continue;
                    }

                    AveItemObject parentFolder = null;
                    if ((parentFolder = GetParentFolder(dirName, rootFolder, noPropertyFolders)) == null)
                    {
                        continue;
                    }
                    if ((byte) itemInfo["Type"] == 1) //Folder
                    {
                        var folder = GetCurrentFolder(parentFolder, fullName, true, noPropertyFolders);
                        InitItemObject(folder, itemInfo);
                        folder.IsCurrentVersion = true;
                        folder.ObjType = ItemType.Folder;
                        folder.ItemPermissionChanged = false;
                        folder.ChangeType = ChangeType.Edit;
                        if (failDocLibRowId > 0 && !items.ContainsKey(failDocLibRowId))
                        {
                            items.Add(failDocLibRowId, folder);
                        }
                    }
                    else //ListItem or Document
                    {
                        AveItemObject item = new AveItemObject();
                        InitItemObject(item, itemInfo);
                        if (listObject != null && listObject.Type == DocList)
                        {
                            item.ObjType = ItemType.Document;
                        }
                        else
                        {
                            item.ObjType = ItemType.Item;
                        }
                        item.IsCurrentVersion = true;
                        item.ItemPermissionChanged = false;
                        item.ChangeType = ChangeType.Edit;
                        item.HasStream = hasStream;
                        item.Size = size;
                        item.DocFlags = docFlags;
                        item.DeleteTransactionId = deleteTransactionId;
                        parentFolder.SubItemObjs.Add(item);
                        if (failDocLibRowId != 0)
                        {
                            items.Add(failDocLibRowId, item);
                        }
                        else
                        {
                            systemItems.Add(failDocId, item);
                        }
                    }
                }
            }
        }

        #endregion

        #endregion

        [AveQueryServiceBase.QueryReview("2012/12/11", "Austin Han", true, "Add folderCache in the parameter list to improve performance.")]
        public AveItemChangedResultCollection HandleItemChanged(IEnumerable<EventObject> allEvents, Dictionary<Guid, DocObject> allDocs, AveItemObject rootFolder, AveFolderCache folderCache, AveListObject listObject, IAveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders, string attachmentUrl, List<Dictionary<string, object>> extraItemInfos)
        {
           
            var items = new Dictionary<int, AveItemObject>();
            var systemItems = new Dictionary<Guid, AveItemObject>();
            var attachments = new Dictionary<int, List<AveItemObject>>();
            var folderAlerts = new Dictionary<Guid, AveAlertObject>();
            var itemAlerts = new Dictionary<int, AveItemObject>();
            var systemItemViews = new Dictionary<Guid, EventObject>(); //
            var result = new AveItemChangedResultCollection
            {
                Items = items,
                SystemItems = systemItems,
                Attachments = attachments,
                FolderAlerts = folderAlerts,
                ItemAlerts = itemAlerts,
                SystemItemViews = systemItemViews
            };

            foreach (var ev in allEvents)
            {
                try
                {
                    DocObject tempDoc = null;
                    if (allDocs.ContainsKey(ev.DocId))
                    {
                        tempDoc = allDocs[ev.DocId];
                    }
                    else if (listObject != null)
                    {
                        tempDoc = new DocObject();
                    }
                    else
                    {
                        continue;
                    }
                    var eventTime = ev.EventTime;
                    var nativeChangeType = (NativeChangeType) ev.EventType;
                    var changeType = DiscoverUtility.GetChangeType(nativeChangeType);
                    var objectType = (ChangeObjectType) ev.ObjectType;
                    switch (objectType)
                    {
                        case ChangeObjectType.View:
                            HandleChangedView(ev, systemItemViews, changeType);
                            break;
                        case ChangeObjectType.Alert:
                            if (ev.ItemId != 0)
                            {
                                HandleItemAlert(ev, changeType, items, itemAlerts);
                            }
                            else
                            {
                                HandleFolderAlert(ev, changeType, folderAlerts);
                            }
                            break;

                            #region File Item Folder

                        case ChangeObjectType.File:
                        case ChangeObjectType.Item:
                        case ChangeObjectType.Folder:
                            var itemBasicInfo = GetItemObjectBasicInfo(ev, tempDoc, objectType);
                            if (string.IsNullOrEmpty(itemBasicInfo.FullUrl))
                            {
                                break;
                            }
                            if (InvalidDirName(itemBasicInfo.DirName, tempDoc))
                            {
                                break;
                            }

                            if (tempDoc.Type == 1 && listObject == null && discoverReader.IsUnusedFolder(itemBasicInfo.LeafName, true))
                            {
                                break;
                            }

                            #region attachment

                            if (objectType == ChangeObjectType.File) //If system we should identify whether it is attachemnt
                            {
                                if (!string.IsNullOrEmpty(attachmentUrl) && itemBasicInfo.FullUrl.StartsWith(attachmentUrl, StringComparison.OrdinalIgnoreCase) && listObject.Type != DocList) //Attachment,Library中可以创建出名为“Attachments”的folder
                                {
                                    if (!(discoverReader is AveReplicatorDiscoverReader)) //attachment变化，item也会变化，replicator需要获取全部的attachment信息，否则会出现丢失的问题
                                    {
                                        AddAttachmentForIB(attachments, ev, tempDoc, itemBasicInfo.FullUrl, changeType);
                                        break;
                                    }
                                }
                            }

                            #endregion attachment

                            //Attachments及其子folder不需要备份，FB的时候也过滤。
                            if (objectType == ChangeObjectType.Folder && !string.IsNullOrEmpty(attachmentUrl) && (itemBasicInfo.FullUrl + '/').StartsWith(attachmentUrl, StringComparison.OrdinalIgnoreCase) && listObject.Type != DocList)
                            {
                                continue;
                            }

                            AveItemObject parentFolder;
                            if ((parentFolder = GetParentFolder(itemBasicInfo.DirName, rootFolder, noPropertyFolders)) == null)
                            {
                                if (objectType == ChangeObjectType.Folder && changeType != ChangeType.None && itemBasicInfo.DocId.Equals(rootFolder.DocID))
                                {
                                    rootFolder.ChangeType = ChangeType.Edit;
                                }
                                break;
                            }
                            if (HandleDeletedItemOrFolder(noPropertyFolders, tempDoc, ev, parentFolder, itemBasicInfo, eventTime, itemAlerts, items))
                            {
                                break;
                            }
                            var docLibRowId = 0;
                            if (tempDoc.DoclibRowId != 0)
                            {
                                docLibRowId = tempDoc.DoclibRowId; //must not be System item, System should use docId to cache it
                            }

                            #region folder

                            if (tempDoc.Type == 1) //Folder
                            {
                                HandleChangedFolder(discoverReader, noPropertyFolders, parentFolder, itemBasicInfo, tempDoc, docLibRowId, items, eventTime, ev, nativeChangeType, changeType);
                            }
                                #endregion
                                #region Item or File

                            else //ListItem or Document
                            {
                                HandleChangedItemOrDocument(listObject, discoverReader, docLibRowId, systemItems, itemBasicInfo, items, itemAlerts, tempDoc, parentFolder, eventTime, ev, nativeChangeType, changeType);
                            }

                            #endregion Item or File

                            break;

                            #endregion File Item Folder
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, "Error occur while access data from method QueryListItemForIB. ErrorMessage:{0}", e);
                }
            }
            //HandleExtraItems(rootFolder, listObject, noPropertyFolders, extraItemInfos, items, systemItems, attachmentUrl, attachments);
            return result;
        }

        [AveQueryServiceBase.QueryReview("2012/05/09", "Fengfu Zhang")]
        public string GetFileFilter(string filter)
        {
            if (!string.IsNullOrEmpty(filter))
            {
                var doc = new XmlDocument();
                doc.LoadXml(filter);
                foreach (XmlNode node in doc.GetElementsByTagName("property"))
                {
                    if (node.Attributes != null)
                    {
                        var value = node.Attributes["value"].Value;
                        var name = node.Attributes["name"].Value;
                        if (name.Equals("filterPath", StringComparison.OrdinalIgnoreCase))
                        {
                            return value.Trim('/');
                        }
                    }
                }
            }
            return "";
        }

        [AveQueryServiceBase.QueryReview("2012/05/22", "Oliver Luo")]
        public void AddAttachmentToItem(Dictionary<int, List<AveItemObject>> attachments, Dictionary<int, AveItemObject> items)
        {
            if (attachments.Count <= 0 || items.Count <= 0)
            {
                return;
            }
            foreach (var kvp in attachments.Where(kvp => items.ContainsKey(kvp.Key)))
            {
                items[kvp.Key].AttachmentObjs = kvp.Value;
            }
        }

        [AveQueryServiceBase.QueryReview("2012/05/22", "Oliver Luo")]
        public void SetDeleteFolders(AveItemObject rootFolder, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            try
            {
                foreach (var v in noPropertyFolders)
                {
                    var fullUrl = v.Key;
                    var dirName = fullUrl.Substring(0, fullUrl.LastIndexOf('/'));
                    var parentFolder = GetParentFolder(dirName, rootFolder, noPropertyFolders);
                    if (parentFolder == null)
                    {
                        continue;
                    }
                    var folder = GetCurrentFolder(parentFolder, fullUrl, false, noPropertyFolders);
                    folder.ChangeType = ChangeType.Delete;

                    if (parentFolder.NoTypeDeleteItems.ContainsKey(fullUrl))
                    {
                        var temp = parentFolder.NoTypeDeleteItems[fullUrl];
                        folder.DocID = temp.DocID;
                        folder.EventTime = temp.EventTime;
                        folder.LeafName = temp.LeafName;
                        folder.ItemName = temp.ItemName;
                        parentFolder.NoTypeDeleteItems.Remove(fullUrl);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.ERROR, "Error occur while doing SetDeleteFolders. ErrorMessage:{0}", e);
            }
        }

        public void HandleSingleChangedListForIB(IDictionary<Guid, AveListObject> listObjs, IDictionary<Guid, AveListObject> deleteListObjs, Guid listId, ChangeObjectType ObjType, string itemUrl, DateTime modifiedTime, ChangeType changeType, string modifiedBy, NativeChangeType nativeChangeType, int? int0, int? int1, int? itemId, AveQueryService aveQueryService)
        {
            var listObj = GetOrCreateListObj(listObjs, listId, ObjType, itemUrl);
            if (ObjType == ChangeObjectType.List)
            {
                listObj.ModifiedTime = modifiedTime;
                var currentType = listObj.ChangeType;
                //ChangeType changeType = DiscoverUtility.GetChangeType(nativeChangeType);
                if (currentType == ChangeType.Add ||
                    currentType == ChangeType.Restore)
                {
                    if (changeType == ChangeType.Delete)
                    {
                        if (modifiedBy != null)
                        {
                            listObj.ModifiedBy = modifiedBy;
                        }
                        listObj.ChangeTypeBeforeDelete = listObj.ChangeType;
                        listObj.ChangeType = ChangeType.Delete;
                    }
                    //otherwise not change.
                }
                else //"None or Edit", change to "Edit or Delete".
                {
                    if (currentType == ChangeType.Delete &&
                        changeType == ChangeType.Restore)
                    {
                        //currentList.ListCache.ChangeType = currentList.ListCache.ChangeTypeBeforeDelete;
                        listObj.ChangeType = listObj.ChangeTypeBeforeDelete;
                        if (listObj.ChangeType == ChangeType.None)
                        {
                            listObjs.Remove(listId);
                            deleteListObjs.Remove(listId);
                        }
                    }
                    else
                    {
                        if (changeType == ChangeType.Delete)
                        {
                            listObj.ChangeTypeBeforeDelete = listObj.ChangeType;
                            listObj.ChangeType = changeType;
                            deleteListObjs.Add(listId, listObj);
                        }
                        else if (changeType != ChangeType.None)
                        {
                            listObj.ChangeType = changeType;
                        }
                    }
                }
                //提取list上删除RoleAssignment事件的信息
                switch (nativeChangeType)
                {
                    case NativeChangeType.AssignmentDelete:
                    case NativeChangeType.AssignmentAdd:
                    case NativeChangeType.ScopeDelete:
                    case NativeChangeType.ScopeAdd:
                        listObj.RoleAssignmentsChangeType = ChangeType.Edit;
                        break;
                }

                if (nativeChangeType == NativeChangeType.AssignmentDelete)
                {
                    if (int0.HasValue)
                    {
                        var deleteRoleAssignment = new AveSecurityObject
                        {
                            ObjectType = SecurityType.Assignment,
                            PrincipleId = int0.Value,
                            // 删除RoleAssignmet时，第13个字段为int0,第14个字段为int1
                            // int0存放principalID,int1存放RoleID
                            //如果int1为Null，说明把该user/group的权限全部移除了
                            RoleId = int1 ?? -1,
                            EventTime = modifiedTime
                        };
                        listObj.DeleteRoleAssignments.Add(deleteRoleAssignment);
                    }
                }
            }
            else if (ObjType == ChangeObjectType.Alert && listId != Guid.Empty && !itemId.HasValue)
            {
                listObj.AlertChangeType = ChangeType.Edit;
            }
        }

        private AveListObject GetOrCreateListObj(IDictionary<Guid, AveListObject> listObjs, Guid listId, ChangeObjectType ObjType, string itemUrl)
        {
            AveListObject listObj;
            if (!listObjs.ContainsKey(listId))
            {
                listObj = new AveListObject
                {
                    ListId = listId
                };
                if (ObjType == ChangeObjectType.List && itemUrl != null) //当list被彻底删除时需要eventcache表中itemurl来初始化属性；
                {
                    var rootFolderUrl = itemUrl;
                    listObj.RootFolderUrl = rootFolderUrl;
                    listObj.Name = rootFolderUrl.Contains("/") ? rootFolderUrl.Substring(rootFolderUrl.LastIndexOf('/') + 1) : rootFolderUrl;
                    listObj.Title = listObj.Name;
                }
                listObjs.Add(listId, listObj);
            }
            else
            {
                listObj = listObjs[listId];
            }
            return listObj;
        }

        public void HandleChangeEventInWebForIB(ChangeObjectType objType, AveWebObject webObj, Dictionary<Guid, AveWebObject> changeWebObjs,
            Guid webId, NativeChangeType nativeChangeType, DateTime eventTime, string itemFullUrl, int? principleId, int? roleId,
            string roleName, bool isWebRootFolder)
        {
            switch (objType)
            {
                case ChangeObjectType.Web:
                    HandleWebChangeForIB(webObj, changeWebObjs, webId, nativeChangeType, eventTime, itemFullUrl, principleId, roleId, roleName);
                    break;
                case ChangeObjectType.Folder:
                    if (isWebRootFolder && webObj.ChangeType == ChangeType.None)
                    {
                        webObj.ChangeType = ChangeType.Edit;
                    }
                    break;
                case ChangeObjectType.Field:
                    webObj.ColumnChangeType |= DiscoverUtility.GetChangeType(nativeChangeType);
                    break;
                case ChangeObjectType.ContentType:
                    webObj.ContentTypeChangeType |= DiscoverUtility.GetChangeType(nativeChangeType);
                    break;
            }
        }

        private void HandleWebChangeForIB(AveWebObject webObj, Dictionary<Guid, AveWebObject> changeWebObjs, Guid webId, NativeChangeType nativeChangeType, DateTime eventTime,
            string itemFullUrl, int? principleId, int? roleId, string roleName)
        {
            var preChange = webObj.ChangeType;
            var changeType = DiscoverUtility.GetChangeType(nativeChangeType);
            webObj.EventTime = eventTime;

            if (changeType == ChangeType.Delete && itemFullUrl != null)
            {
                webObj.FullUrl = itemFullUrl;
            }
            if (preChange == ChangeType.Add || preChange == ChangeType.Restore)
            {
                if (changeType == ChangeType.Delete)
                {
                    webObj.ChangeTypeBeforeDelete = webObj.ChangeType;
                    webObj.ChangeType = ChangeType.Delete;
                }
            }
            else
            {
                if (preChange == ChangeType.Delete && changeType == ChangeType.Restore)
                {
                    webObj.ChangeType = webObj.ChangeTypeBeforeDelete;
                    if (webObj.ChangeType == ChangeType.None)
                    {
                        changeWebObjs.Remove(webId);
                    }
                }
                else
                {
                    if (changeType == ChangeType.Delete)
                    {
                        webObj.ChangeTypeBeforeDelete = webObj.ChangeType;
                    }
                    webObj.ChangeType = changeType;
                }
            }
            HandleWebSecurityChange(nativeChangeType, webObj, principleId, roleId, roleName, eventTime);
        }

        private void HandleWebSecurityChange(NativeChangeType nativeChangeType, AveWebObject webObj, int? principleId,
            int? roleId, string roleName, DateTime eventTime)
        {
            //提取web上删除Role与RoleAssignment事件的信息
            switch (nativeChangeType)
            {
                case NativeChangeType.AssignmentAdd:
                case NativeChangeType.ScopeAdd:
                    webObj.RoleAssignmentsChangeType |= ChangeType.Add;
                    break;
                case NativeChangeType.AssignmentDelete:
                case NativeChangeType.ScopeDelete:
                    webObj.RoleAssignmentsChangeType |= ChangeType.Delete;
                    break;
                case NativeChangeType.RoleAdd:
                    webObj.PermissionLevelChangeType |= ChangeType.Add;
                    break;
                case NativeChangeType.RoleUpdate:
                    webObj.PermissionLevelChangeType |= ChangeType.Edit;
                    break;
                case NativeChangeType.RoleDelete:
                    webObj.PermissionLevelChangeType |= ChangeType.Delete;
                    break;
                case NativeChangeType.Navigation:
                    webObj.NavigationChanged = true;
                    webObj.NavigationChangeType = ChangeType.Edit;
                    break;
            }

            if (nativeChangeType == NativeChangeType.RoleDelete || nativeChangeType == NativeChangeType.AssignmentDelete)
            {
                if (!principleId.HasValue && roleId.HasValue && roleName != null)
                {
                    var deleteSecurity = new AveSecurityObject
                    {
                        PrincipleId = -1,
                        RoleId = roleId.Value,
                        RoleName = roleName,
                        ObjectType = SecurityType.Role,
                        EventTime = eventTime
                    };
                    webObj.DeleteSecurities.Add(deleteSecurity);
                }
                if (principleId.HasValue && roleName == null)
                {
                    var deleteSecurity = new AveSecurityObject
                    {
                        PrincipleId = principleId.Value,
                        RoleId = roleId ?? -1,
                        ObjectType = SecurityType.Assignment,
                        EventTime = eventTime
                    };
                    webObj.DeleteSecurities.Add(deleteSecurity);
                }
            }
        }

        [AveQueryServiceBase.QueryReviewAttribute("2012/05/22", "Oliver Luo")]
        private AveSiteMemberObject GetOrAddUser(Dictionary<int, AveSiteMemberObject> users, int userId, DateTime eventTime)
        {
            AveSiteMemberObject user = null;
            if (users.ContainsKey(userId))
            {
                user = users[userId];
                user.EventTime = eventTime;
            }
            else
            {
                user = new AveSiteMemberObject
                {
                    PrincipleId = userId,
                    IsUser = true,
                    EventTime = eventTime,
                };
                users.Add(userId, user);
            }
            return user;
        }

        public void HandleSecurityChangedForSite(IDictionary<int, AveSiteMemberObject> memberChanges, int principalId, ChangeObjectType changeObjectType, Dictionary<int, AveSiteMemberObject> groups, Dictionary<int, AveSiteMemberObject> users, DateTime eventTime, string title, NativeChangeType eventType, int? userId)
        {
            AveSiteMemberObject memberChange;
            if (!memberChanges.TryGetValue(principalId, out memberChange))
            {
                if (changeObjectType == ChangeObjectType.Group)
                {
                    memberChange = new AveSiteMemberObject()
                    {
                        PrincipleId = principalId,
                        IsGroup = true,
                    };
                    groups.Add(principalId, memberChange);
                }
                else
                {
                    memberChange = GetOrAddUser(users, principalId, eventTime);
                }
                memberChanges.Add(principalId, memberChange);
            }
            memberChange.EventTime = eventTime;

            if (string.IsNullOrEmpty(memberChange.Title) || !memberChange.Title.Equals(title))
            {
                memberChange.Title = title;
            }

            var changeType = DiscoverUtility.GetChangeType(eventType);
            if (memberChange.ChangeType == ChangeType.Add)
            {
                if (changeType == ChangeType.Delete)
                {
                    memberChange.ChangeType = ChangeType.Delete;
                    return;
                }
            }
            else
            {
                memberChange.ChangeType = changeType;
            }

            #region Get group members

            if (changeObjectType == ChangeObjectType.Group && userId.HasValue)
            {
                AveSiteMemberObject user;
                switch (eventType)
                {
                    case NativeChangeType.MemberAdd:
                        user = GetOrAddUser(users, userId.Value, eventTime);
                        if (memberChange.AddedMemberIds == null)
                        {
                            memberChange.AddedMemberIds = new Dictionary<int, AveSiteMemberObject>();
                        }
                        memberChange.AddedMemberIds.Add(userId.Value, user);
                        break;
                    case NativeChangeType.MemberDelete:
                        user = GetOrAddUser(users, userId.Value, eventTime);
                        if (memberChange.DeletedMemberIds == null)
                        {
                            memberChange.DeletedMemberIds = new Dictionary<int, AveSiteMemberObject>();
                        }
                        memberChange.DeletedMemberIds.Add(userId.Value, user);
                        if (memberChange.AddedMemberIds != null && memberChange.AddedMemberIds.ContainsKey(userId.Value))
                        {
                            memberChange.AddedMemberIds.Remove(userId.Value);
                        }
                        break;
                }
            }

            #endregion

        }
    }
}