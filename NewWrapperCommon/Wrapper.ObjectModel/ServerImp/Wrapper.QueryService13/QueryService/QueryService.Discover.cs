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
using System.Text;
using AvePoint.Wrapper.Common;
using System.Data.SqlClient;
using AvePoint.GCommon;
using System.Globalization;
using System.Xml;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AvePoint.GCommon.Utility;

namespace AvePoint.Wrapper.QueryService
{
    internal partial class AveQueryService : IAveDiscoverQueryService
    {

        [QueryReview("2012/05/21", "Oliver Luo")]
        private string ReplaceDirNameAndLeafName(string fullUrl, string commandText)
        {
            string dirName;
            string leafName;
            AveUrlUtility.SplitUrl(fullUrl, out dirName, out leafName);
            return commandText.Replace("@DirName", FilterParameterString(dirName)).Replace("@LeafName", FilterParameterString(leafName));
        }

        [QueryReview("2012/05/21", "Oliver Luo")]
        private string FilterParameterString(string str)
        {
            return "N'" + str.Replace("'", "''") + "'";
        }

        [QueryReview("2012/12/10", "Austin Han")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Urls is a variable")]
        private void QueryFolderProperty(Dictionary<string, AveItemObject> noPropertyFolders, AveDiscoverReader discoverReader, AveListObject listObject)
        {
            if (noPropertyFolders.Count <= 0)
            {
                return;
            }
            using (AvePerformanceScope pc = new AvePerformanceScope("AveDiscoverQuery.QueryFolderProperty"))
            {
                string dirName = string.Empty;
                string leafName = string.Empty;
                var needSearchFolders = noPropertyFolders.Values.ToList();

                int index = 0;
                while (index < needSearchFolders.Count)
                {
                    var sb = new StringBuilder();
                    for (var i = 0; index < needSearchFolders.Count && i < 400; i++)
                    {
                        var folder = needSearchFolders[index++];
                        AveUrlUtility.SplitUrl(folder.FullUrl, out dirName, out leafName);
                        sb.Append("OR doc.DirName=N'" + dirName.Replace("'", "''") + "' AND doc.LeafName=N'" + leafName.Replace("'", "''") + "'");
                    }
                    try
                    {
                        AveItemObject folder = null;
                        var needSearchVersions = new Dictionary<Guid, AveItemObject>();
                        string commText = AddAllDocsDirNameAndParentId(discoverReader.GetAllItemsInAllDocQueryString())
                            .Replace("@WHERE", DiscoverConditionString.FolderURLs).Replace("@Urls", sb.ToString().TrimStart('O', 'R'));
                        var idsbuilder = new StringBuilder();
                        using (SqlDataReader sr = mQueryWorker.ExecuteReader(commText))
                        {
                            while (sr.Read())
                            {
                                try
                                {
                                    var id = (Guid)sr["Id"];
                                    if (!needSearchVersions.ContainsKey(id))
                                    {
                                        idsbuilder.AppendFormat("'{0}',", id);
                                        //rootSC,dirName可能为empty.
                                        string fullName = string.Format("{0}/{1}", sr["DirName"], sr["LeafName"]).TrimStart('/');
                                        folder = noPropertyFolders[fullName];
                                        needSearchVersions[id] = folder;
                                        discoverReader.ReadItemContent(folder, sr);
                                        folder.DirName = (string)sr["DirName"];
                                        folder.FullUrl = fullName;
                                        folder.ObjType = ItemType.Folder;
                                        folder.ParentID = (Guid)sr["ParentId"];
                                        noPropertyFolders.Remove(fullName);
                                    }
                                    AveVersionObject version = new AveVersionObject();
                                    discoverReader.ReadVersionContent(version, sr);
                                    AddVersion(version, folder, sr, discoverReader);
                                }
                                catch (Exception e)
                                {
                                    logger.Log(AveLogLevel.WARN, "Error occur while access data from query in QueryFolderProperty. ErrorMessage:{0}.", e);
                                }
                            }
                        }
                        if (idsbuilder.Length > 0)
                        {
                            idsbuilder.Length--;
                            var condition = string.Empty;
                            if (needSearchVersions.Count == 1)
                            {
                                condition = discoverReader.GetItemVersionsWithDocIdCondition();
                                Guid key = needSearchVersions.Keys.First<Guid>();
                                mQueryWorker.AddParameter("@ParentId", needSearchVersions[key].ParentID);
                                mQueryWorker.AddParameter("@Id", key);
                            }
                            else
                            {
                                condition = string.Format(discoverReader.GetItemVersionsWithDocIdsCondition(), idsbuilder.ToString());
                            }
                            QueryItemVersions(needSearchVersions, discoverReader, listObject, condition);
                        }
                    }
                    catch (SqlException queryException)
                    {
                        throw new AveQueryException(queryException);
                    }
                    catch (AveQueryException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        throw new AveQueryException(e.Message, e);
                    }
                }
            }
        }

        [QueryReview("2012/05/21", "Oliver Luo")]
        private void AddVersion(AveVersionObject version, AveItemObject currentItem, SqlDataReader sr, AveDiscoverReader discoverReader)
        {
            //UD 表覆盖alldoc 表中数据。  currentItem.Uiversion  在所有引用地方都已经赋值。
            if (currentItem.Uiversion == version.Uiversion)
            {
                discoverReader.OverriteProperties(sr, currentItem);
            }
            for (var i = 0; i < currentItem.VersionObjs.Count; i++)// VersionObjs 集合为从大到小排序。
            {
                if (currentItem.VersionObjs[i].Uiversion == version.Uiversion)
                {
                    currentItem.VersionObjs[i] = version;// 也是为了current version 上属性覆盖。cuurent version 也会在集合中
                    return;
                }
                else if (version.Uiversion > currentItem.VersionObjs[i].Uiversion)
                {
                    currentItem.VersionObjs.Insert(i, version);
                    return;
                }
            }
            currentItem.VersionObjs.Add(version);
        }

        [QueryReview("2012/05/21", "Oliver Luo")]
        private string AddAllDocsDirName(string commText)
        {
            return commText.Replace("FROM", ",doc.DirName FROM");
        }


        private string AddAllDocsParentId(string commText)
        {
            return commText.Replace("FROM", ",doc.ParentId FROM");
        }

        private string AddAllDocsDirNameAndParentId(string commText)
        {
            return AddAllDocsDirName(AddAllDocsParentId(commText));
        }

        [QueryReview("2012/05/21", "Oliver Luo", false, "在调用方法中Review")]
        private void RemoveNoPropertyFolder(string fullUrl, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            if (noPropertyFolders.ContainsKey(fullUrl))
            {
                noPropertyFolders.Remove(fullUrl);
            }
        }

        [QueryReview("2012/12/10", "Austin Han")]
        private void QueryAttachmentForFB(string commText, Dictionary<int, AveItemObject> attachmentItems, AveDiscoverReader discoverReader)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("QueryService.QueryAttachmentForFB"))
            {
                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(commText))
                    {
                        AveItemObject item;
                        while (sr.Read())
                        {
                            try
                            {
                                Guid docId = (Guid)sr["Id"];
                                string dirName = (string)sr["DirName"];
                                int pos = dirName.LastIndexOf('/');
                                if (pos < 0)
                                {
                                    continue;
                                }
                                int subId;
                                if (int.TryParse(dirName.Substring(pos + 1), out subId)
                                    && attachmentItems.TryGetValue(subId, out item))
                                {
                                    AveItemObject attachment = new AveItemObject();
                                    discoverReader.ReadAttachmentContent(attachment, sr);
                                    item.AttachmentObjs.Add(attachment);
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, "An error occurred while getting data from method QueryAttachmentForFB. ErrorMessage:{0}", e);
                            }
                        }
                    }

                    //deal stub Attachment Extender  
                    if (discoverReader is AveExtenderDiscoverReader)
                    {
                        GetAttanchemntStub(attachmentItems);
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }
        }

        private void GetAttanchemntStub(Dictionary<int, AveItemObject> attachmentItems)
        {
            var result = from pair in attachmentItems orderby pair.Key select pair;
            foreach (KeyValuePair<int, AveItemObject> pair in result)
            {
                foreach (var att in pair.Value.AttachmentObjs)
                {
                    mQueryWorker.AddParameter("@DocId", att.DocID);
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveQueryString13.Sp13ContentOrStub))
                    {
                        if (sr.HasRows)
                        {
                            pair.Value.StubAttachmentObjs.Add(att);
                        }
                    }
                }

            }
        }

        [QueryReview("2012/05/21", "Oliver Luo")]
        private DateTime GetAndCheckItemId(SqlDataReader sr, ref Guid docId, ref int docRowId)
        {
            DateTime result = DateTime.MinValue;
            if (sr.Read())
            {
                docId = sr.GetGuid(0);
                result = sr.GetDateTime(1);
                docRowId = sr.IsDBNull(2) ? 0 : sr.GetInt32(2);
                return result;
            }
            return DateTime.MinValue;
        }

        [QueryReview("2012/05/21", "Oliver Luo")]
        private DateTime GetTimeAndDocId(SqlDataReader sr, ref Guid docId)
        {
            DateTime result = DateTime.MinValue;
            if (sr.Read())
            {
                docId = sr.GetGuid(0);
                result = sr.GetDateTime(1);
                return result;
            }
            return DateTime.MinValue;
        }

        [QueryReview("2012/05/21", "Oliver Luo")]
        private void GetListItem(AveItemObject parentFolderObject, string attachmentUrl, AveListObject listObject, AveDiscoverReader discoverReader, bool includeRecycleBin,bool includeVersion)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("QueryService.Discover.GetListItem"))
            {
                if (parentFolderObject.DocID == Guid.Empty)
                {
                    logger.Log(AveLogLevel.WARN, "parentId should not be null.ParentFolder Url:{0}", parentFolderObject.FullUrl);
                    return;
                }

                mQueryWorker.AddParameter("@ParentId", parentFolderObject.DocID);

                Dictionary<int, AveItemObject> attachments = null;
                if (listObject == null)//System Folder
                {
                    using (AvePerformanceScope scope = new AvePerformanceScope("AveDiscoverQuery.GetListItem.QueryDocsForWeb"))
                    {
                        QueryDocsForFB(discoverReader.GetAllItemsInAllDocQueryString().Replace("@WHERE", includeRecycleBin ? DiscoverConditionString.WebItemsWithRecycleBin : DiscoverConditionString.WebItems), parentFolderObject, attachments, listObject, discoverReader);
                        if (includeVersion)
                        {
                            var condition = discoverReader.GetWebItemVersionCondition(includeRecycleBin);
                            QueryItemVersions(parentFolderObject.SubItemObjs.ToDictionary(key => key.DocID, value => value), discoverReader, listObject, condition, includeRecycleBin);
                            QueryItemVersions(parentFolderObject.SubFolderObjs.ToDictionary(key => key.DocID, value => value), discoverReader, listObject, condition, includeRecycleBin);
                        }
                    }
                }
                else
                {
                    bool enableAttachment = listObject.Flag != null && DiscoverUtility.IsEnableAttachment((long)listObject.Flag);
                    if (enableAttachment)
                    {
                        attachments = new Dictionary<int, AveItemObject>();
                    }
                    using (AvePerformanceScope scope = new AvePerformanceScope("AveDiscoverQuery.GetListItem.QueryDocsForList"))
                    {
                        //查alldoc 表中记录，当前version，checkout，publish 
                        QueryDocsForFB(discoverReader.GetAllItemsInAllDocQueryString().Replace("@WHERE", includeRecycleBin ? DiscoverConditionString.ListItemsWithRecycleBin : DiscoverConditionString.ListItems), parentFolderObject, attachments, listObject, discoverReader);
                        if (includeVersion)
                        {
                            var condition = discoverReader.GetListItemVersionCondition(includeRecycleBin);
                            QueryItemVersions(parentFolderObject.SubItemObjs.ToDictionary(key => key.DocID, value => value), discoverReader, listObject, condition, includeRecycleBin);
                            QueryItemVersions(parentFolderObject.SubFolderObjs.ToDictionary(key => key.DocID, value => value), discoverReader, listObject, condition, includeRecycleBin);
                        }
                    }
                    if (!string.IsNullOrEmpty(attachmentUrl) && enableAttachment && attachments.Count > 0)
                    {
                        mQueryWorker.AddParameter("@AttachmentUrl", attachmentUrl);
                        QueryAttachmentForFB(includeRecycleBin ? discoverReader.GetAttachmentsWithRecycleBinQueryString() : discoverReader.GetAttachmentsQueryString(), attachments, discoverReader);
                    }
                }
            }
        }
        /// <summary>
        /// Add the interface for discover API. if there is any changes, it doesn't effect the native  method.
        /// </summary>
        /// <param name="itemCollection"></param>
        /// <param name="listObject"></param>
        /// <param name="discoverReader"></param>
        public void QueryItemVersionsForAPI(Dictionary<int, AveItemObject> itemCollection, AveListObject listObject, AveDiscoverReader discoverReader)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("QueryService.Discover.QueryItemVersionsForAPI"))
            {
                AddVersionToItems(itemCollection, listObject, discoverReader);
            }
        }

        public void QueryItemVersionsForAPIFB(Guid siteId, Guid parentId, List<AveItemObject> itemObjs, AveListObject listObject, AveDiscoverReader discoverReader)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("QueryService.Discover.QueryItemVersionsForAPIFB"))
            {
                bool includeRecycleBin = false;
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ParentId", parentId);
                var condition = string.Empty;
                if (listObject == null)//System Folder
                {
                    condition = discoverReader.GetWebItemVersionCondition(includeRecycleBin);
                    mQueryWorker.AddParameter("@ListId", Guid.Empty);
                }
                else
                {
                    condition = discoverReader.GetListItemVersionCondition(includeRecycleBin);
                    mQueryWorker.AddParameter("@ListId", listObject.ListId);
                }
                QueryItemVersions(itemObjs.ToDictionary(key => key.DocID, value => value), discoverReader, listObject, condition, includeRecycleBin);
            }

        }


        /// <summary>
        /// 查询parent 下所有version 的记录，包括在alldoc表中查到的记录，为了补全属性 Lastmodifytime size 等
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parentFolderObject"></param>
        /// <param name="discoverReader"></param>
        private void QueryItemVersions(Dictionary<Guid, AveItemObject> collections, AveDiscoverReader discoverReader, AveListObject listObject, string condition, bool includeRecycleBin = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("QueryService.Discover.QueryItemVersionsInUD"))
            {
                try
                {
                    if (collections.Count > 0)//查到Item 情况才需要查version
                    {
                        var commandText = discoverReader.GetAllVersionsQueryString(includeRecycleBin);
                        var isSpecialLibrary = listObject != null && listObject.Type == 1 && listObject.MaxMajorwithMinorVersionCount.HasValue;
                        if (discoverReader is AveExtenderDiscoverReader)
                        {
                            commandText = commandText.Replace("@WHERE", condition);
                        }
                        else
                        {
                            commandText = commandText.Replace("@WHERE", condition + " AND data.tp_RowOrdinal = 0 ");
                        }
                        if (isSpecialLibrary && !(discoverReader is AveExtenderDiscoverReader))
                        {
                            QueryVersionsForSpecialLibrarySetting(collections, discoverReader, commandText);
                        }
                        else
                        {
                            QueryItemVersionsInUDBasic(collections, discoverReader, commandText);
                        }

                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }
        }

        private void QueryItemVersionsInUDBasic(Dictionary<Guid, AveItemObject> collections, AveDiscoverReader discoverReader, string commandText)
        {
            AveItemObject previousItem = null;
            using (SqlDataReader sr = mQueryWorker.ExecuteReader(commandText))
            {
                while (sr.Read())
                {
                    Guid docId = (Guid)sr["tp_DocId"];
                    if (previousItem == null || previousItem.DocID != docId)
                    {
                        if (!collections.TryGetValue(docId, out previousItem))
                        {
                            continue;
                        }
                    }
                    AveVersionObject version = new AveVersionObject();
                    discoverReader.ReadVersionContentWithDeleteState(version, sr);
                    AddVersion(version, previousItem, sr, discoverReader);
                }
            }
        }

        private void QueryVersionsForSpecialLibrarySetting(Dictionary<Guid, AveItemObject> collections, AveDiscoverReader discoverReader, string commandText)
        {
            var allItems = collections.Select(item => item.Value.DocID).ToArray();
            int index = 0;
            var allDocVersionsCache = new Dictionary<Guid, List<int>>();
            while (index < allItems.Length)
            {
                List<Guid> queryItemDocIds = new List<Guid>();
                //SQL command text limited 64k
                for (var idCount = 0; idCount < 800; ++idCount)
                {
                    queryItemDocIds.Add(allItems[index++]);
                    if (index >= allItems.Length)
                    {
                        break;
                    }
                }
                var tempAllDocCommand = AveQueryUtility.GetAllDocVersionsForSpecialLibrary_Select_AllDocVersions(queryItemDocIds);
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(tempAllDocCommand))
                {
                    while (sr.Read())
                    {
                        Guid allDocVersionId = (Guid)sr["Id"];
                        int uiVersion = (int)sr["UIVersion"];
                        if (allDocVersionsCache.ContainsKey(allDocVersionId))
                        {
                            allDocVersionsCache[allDocVersionId].Add(uiVersion);
                        }
                        else
                        {
                            var uiList = new List<int>();
                            uiList.Add(uiVersion);
                            allDocVersionsCache.Add(allDocVersionId, uiList);
                        }
                    }
                }
            }
            using (SqlDataReader sr = mQueryWorker.ExecuteReader(commandText))
            {
                AveItemObject previousItem = null;
                while (sr.Read())
                {
                    Guid audDocId = (Guid)sr["tp_DocId"];
                    Guid audSiteId = (Guid)sr["tp_SiteId"];
                    int audCalculatedVersion = Convert.ToInt32(sr["tp_CalculatedVersion"]);
                    int audTPCurrentVersion = Convert.ToInt32(sr["tp_IsCurrentVersion"]);
                    if (audTPCurrentVersion == 1 || (allDocVersionsCache.ContainsKey(audDocId) && allDocVersionsCache[audDocId].Exists(uiversion => uiversion == audCalculatedVersion)))
                    {

                        try
                        {
                            Guid docId = (Guid)sr["tp_DocId"];
                            if (previousItem == null || previousItem.DocID != docId)
                            {
                                if (!collections.TryGetValue(docId, out previousItem))
                                {
                                    continue;
                                }
                            }
                            AveVersionObject version = new AveVersionObject();
                            discoverReader.ReadVersionContentWithDeleteState(version, sr);
                            AddVersion(version, previousItem, sr, discoverReader);
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Error occur while access data from AddVersionToItems. ErrorMessage:{0}", e);
                        }

                    }
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The Wrong words are the part of sql statement. ")]
        [QueryReview("2012/05/22", "Oliver Luo")]
        private void GetStubItem(Guid parentId, AveItemObject parentFolder, string attachmentUrl, AveListObject listObject, AveDiscoverReader discoverReader, bool includeRecycleBin)
        {
            if (parentId == Guid.Empty)
            {
                logger.Log(AveLogLevel.WARN, "GetStubItem parentId should not be null.ParentFolder Url:{0}", parentFolder.FullUrl);
                return;
            }
            mQueryWorker.AddParameter("@ParentId", parentId);

            bool enableAttachment = false;
            Dictionary<int, AveItemObject> itemEntities = null;
            if (listObject == null)//System Folder
            {
                using (AvePerformanceScope scope = new AvePerformanceScope("AveDiscoverQuery.GetStubItem.QueryDocsForWeb"))
                {
                    if (includeRecycleBin)
                    {
                        QueryDocsForFB(AveQueryString13.Sp13StubAllItemAndVersionsWithRecycleBin.Replace("@WHEREAllDocs", DiscoverConditionString.WebStubItemsWithRecycleBin).Replace("@WHEREAllDocVersions", DiscoverConditionString.WebStubItemsForAllDocVersionsWithRecycleBin), parentFolder, itemEntities, listObject, discoverReader);
                    }
                    else
                    {
                        QueryDocsForFB(AveQueryString13.Sp13StubAllItemAndVersions.Replace("@WHEREAllDocs", DiscoverConditionString.WebStubItems).Replace("@WHEREAllDocVersions", DiscoverConditionString.WebStubItemsForAllDocVersions), parentFolder, itemEntities, listObject, discoverReader);
                    }
                }
            }
            else
            {
                enableAttachment = listObject != null && DiscoverUtility.IsEnableAttachment((long)listObject.Flag);
                if (enableAttachment)
                {
                    itemEntities = new Dictionary<int, AveItemObject>();
                }
                using (AvePerformanceScope scope = new AvePerformanceScope("AveDiscoverQuery.GetStubItem.QueryDocsForList"))
                {
                    if (includeRecycleBin)
                    {
                        QueryDocsForFB(AveQueryString13.Sp13StubAllItemAndVersionsWithRecycleBin.Replace("@WHEREAllDocs", DiscoverConditionString.ListStubItemsWithRecycleBin).Replace("@WHEREAllDocVersions", DiscoverConditionString.ListStubItemsForAllDocVersionsWithRecycleBin), parentFolder, itemEntities, listObject, discoverReader);
                    }
                    else
                    {
                        QueryDocsForFB(AveQueryString13.Sp13StubAllItemAndVersions.Replace("@WHEREAllDocs", DiscoverConditionString.ListStubItems).Replace("@WHEREAllDocVersions", DiscoverConditionString.ListStubItemsForAllDocVersions), parentFolder, itemEntities, listObject, discoverReader);
                    }
                }
            }
        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private void AssignmentSecurityChange(ChangeType changeType, SqlDataReader sr, Dictionary<int, List<AveSecurityObject>> securityChanges)
        {
            if (sr.IsDBNull(2) || sr.IsDBNull(3)) //assign role to principle 
            {
                //有assignment事件的时候，可定时关联RoleId和PrincipleId
                return;
            }
            int principleId = 0;
            int roleId = sr.GetInt32(3);
            List<AveSecurityObject> securitys = null;
            AveSecurityObject security = new AveSecurityObject();
            principleId = sr.GetInt32(2);
            securityChanges.TryGetValue(principleId, out securitys);
            if (securitys == null)
            {
                securitys = new List<AveSecurityObject>();
                securityChanges.Add(principleId, securitys);
            }
            security = TryGetAssignmentSecurity(securitys, roleId);
            if (security.ChangeType == ChangeType.Add)
            {
                if (security.ChangeType == ChangeType.Delete)
                {
                    securitys.Remove(security);
                    DeleteAllRelatedRole(securityChanges, roleId);
                    return;
                }
            }
            else
            {
                security.ChangeType = changeType;
            }
            if (security.ChangeType == ChangeType.Delete)
            {
                DeleteAllRelatedRole(securityChanges, roleId);
                return;
            }
            security.ScopeId = sr.IsDBNull(4) ? Guid.Empty : sr.GetGuid(4);
        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private void DeleteAllRelatedRole(Dictionary<int, List<AveSecurityObject>> securityChanges, int roleId)
        {
            foreach (var kvp in securityChanges)
            {
                if (kvp.Key != AveSecurityObject.RoleChangeId && kvp.Key != AveSecurityObject.ScopeChangeId)
                { // we shoud delete scope and principle relate current role
                    foreach (AveSecurityObject asc in kvp.Value)
                    {
                        if (asc.RoleId == roleId)
                        {
                            kvp.Value.Remove(asc);
                        }
                    }
                }
            }
        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private AveSecurityObject TryGetAssignmentSecurity(List<AveSecurityObject> Securitys, int roleId)
        {
            AveSecurityObject security = new AveSecurityObject();
            foreach (AveSecurityObject asc in Securitys)
            {
                if (asc.RoleId == roleId)
                {
                    return asc;
                }
            }
            security.RoleId = roleId;
            security.ObjectType = SecurityType.Assignment;
            Securitys.Add(security);
            return security;
        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private void ScopeSecurityChange(ChangeType changeType, SqlDataReader sr, Dictionary<int, List<AveSecurityObject>> mSecurityChanges)
        {
            Guid scopeId = sr.GetGuid(4);
            int scopeRoleId = sr.GetInt32(3);
            List<AveSecurityObject> scopeSecuritys = null;
            mSecurityChanges.TryGetValue(AveSecurityObject.ScopeChangeId, out scopeSecuritys);
            if (scopeSecuritys == null)
            {
                scopeSecuritys = new List<AveSecurityObject>();
                mSecurityChanges.Add(AveSecurityObject.ScopeChangeId, scopeSecuritys);
            }

            AveSecurityObject scopeSecurity = TryGetScopeSecurity(scopeSecuritys, scopeId);

            if (scopeSecurity.ChangeType == ChangeType.Add)
            {
                if (changeType == ChangeType.Delete)
                {
                    scopeSecuritys.Remove(scopeSecurity);
                    return;
                }
            }
            else
            {
                scopeSecurity.ChangeType = changeType;
            }
            if (scopeSecurity.ChangeType == ChangeType.Delete)
            {
                return;
            }
            scopeSecurity.RoleId = scopeRoleId;
        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private AveSecurityObject TryGetScopeSecurity(List<AveSecurityObject> securitys, Guid scopeId)
        {
            foreach (var asc in securitys)
            {
                if (asc.ScopeId == scopeId)
                {
                    return asc;
                }
            }
            AveSecurityObject security = new AveSecurityObject
            {
                ScopeId = scopeId,
                ObjectType = SecurityType.Scope
            };
            securitys.Add(security);
            return security;
        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private void HandleItemAlert(EventObject ev, ChangeType changeType, Dictionary<int, AveItemObject> items, Dictionary<int, AveItemObject> itemAlerts)
        {
            Guid alertId = ev.Guid0;
            int itemId = ev.ItemId;

            AveItemObject item = null;
            if (!itemAlerts.ContainsKey(itemId))
            {
                if (items.ContainsKey(itemId))
                {
                    item = items[itemId];
                }
                else
                {
                    item = new AveItemObject();
                    item.ID = itemId;
                    if (item.AlertObjs == null)
                    {
                        item.AlertObjs = new Dictionary<Guid, AveAlertObject>();
                    }
                    itemAlerts.Add(itemId, item);
                }
            }
            else
            {
                item = itemAlerts[itemId];
            }
            AveAlertObject alert = null;
            if (item.AlertObjs == null || !item.AlertObjs.ContainsKey(alertId))
            {
                item.AlertObjs.Add(alertId, new AveAlertObject
                {
                    Id = alertId
                });
            }
            alert = item.AlertObjs[alertId];
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

        [QueryReview("2012/05/22", "Oliver Luo")]
        private void HandleFolderAlert(EventObject ev, ChangeType changeType, Dictionary<Guid, AveAlertObject> folderAlerts)
        {
            Guid alertId = ev.Guid0;
            AveAlertObject alert = null;
            if (!folderAlerts.TryGetValue(alertId, out alert))
            {
                if (changeType != ChangeType.Delete)//we do not know the deleted alert belong to list or a folder
                {
                    alert = new AveAlertObject
                    {
                        Id = alertId
                    };
                    folderAlerts.Add(alertId, alert);
                }
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

        [QueryReview("2012/05/22", "Oliver Luo")]
        private string GetFullName(EventObject ev, DocObject doc, ChangeObjectType objectType)
        {
            if (string.IsNullOrEmpty(ev.ItemFullUrl))
            {
                if (doc.Id == Guid.Empty)
                {
                    return string.Empty;
                }
                else
                {
                    return (doc.DirName + "/" + doc.LeafName).Trim('/');
                }
            }
            else
            {
                if (objectType == ChangeObjectType.Item && !string.IsNullOrEmpty(doc.DirName) && !string.IsNullOrEmpty(doc.LeafName))
                {
                    return (doc.DirName + "/" + doc.LeafName).Trim('/');
                }
                else
                {
                    return ev.ItemFullUrl;
                }
            }
        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private Guid GetDocId(EventObject ev, DocObject tempDoc)
        {
            if (ev.DocId == Guid.Empty)
            {
                if (tempDoc.Id == Guid.Empty)
                {
                    return Guid.Empty;
                }
                else
                {
                    return tempDoc.Id;
                }
            }
            else
            {
                return ev.DocId;
            }
        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private string GetItemName(EventObject ev, DocObject tempDoc)
        {
            if (string.IsNullOrEmpty(ev.ItemName))
            {
                if (string.IsNullOrEmpty(tempDoc.LeafName))
                {
                    return string.Empty;
                }
                else
                {
                    return tempDoc.LeafName;
                }
            }
            else
            {
                return ev.ItemName;
            }
        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private int GetItemId(string itemFullUrl, out string leafName)
        {
            string[] strs = itemFullUrl.Split('/');
            leafName = strs[strs.Length - 1];
            return Convert.ToInt32(strs[strs.Length - 2], CultureInfo.InvariantCulture);
        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private AveItemObject GetAttachment(List<AveItemObject> attachments, Guid docId)
        {
            foreach (AveItemObject attach in attachments)
            {
                if (attach.DocID == docId)
                {
                    return attach;
                }
            }
            AveItemObject attachment = new AveItemObject
            {
                DocID = docId
            };
            attachments.Add(attachment);
            return attachment;
        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private void AddAttachmentForIB(Dictionary<int, List<AveItemObject>> attachments, EventObject ev, DocObject tempDoc, string itemFullUrl, ChangeType changeType)
        {
            Guid docId = ev.DocId == Guid.Empty ? tempDoc.Id : ev.DocId;
            DateTime timeLastModified = ev.TimeLastModified;

            string leafName = string.Empty;
            int itemId = GetItemId(itemFullUrl, out leafName);

            if (!attachments.ContainsKey(itemId))
            {
                attachments.Add(itemId, new List<AveItemObject>());
            }

            AveItemObject attachment = GetAttachment(attachments[itemId], docId);

            if (tempDoc.Id == Guid.Empty) //the attachment is deleted from recycle
            {
                attachment.ChangeType = ChangeType.Delete;
                attachment.LeafName = leafName;
                attachment.DirName = itemFullUrl.Replace(leafName, "").TrimEnd('/');
                attachment.TimeLastModified = timeLastModified;
                return;
            }

            if (String.IsNullOrEmpty((string)attachment.LeafName))
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

        [QueryReview("2012/05/22", "Oliver Luo")]
        private bool FolderExist(ref AveItemObject tempParentFolder, string str)
        {
            foreach (AveItemObject folder in tempParentFolder.SubFolderObjs)
            {
                if (folder.LeafName.Equals(str, StringComparison.OrdinalIgnoreCase))
                {
                    tempParentFolder = folder;
                    return true;
                }
            }
            return false;
        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private AveItemObject GetParentFolder(string dirName, AveItemObject rootFolder, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            string listRootFolderUrl = rootFolder.FullUrl;

            if (dirName.Equals(listRootFolderUrl))
            {
                return rootFolder;
            }
            if (!dirName.Contains(listRootFolderUrl))
            {
                return null;
            }
            string foldersDirName = dirName.Substring(listRootFolderUrl.Length).Trim('/');

            AveItemObject tempFolder = rootFolder;
            AveItemObject tempParentFolder = rootFolder;
            foreach (string str in foldersDirName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (FolderExist(ref tempParentFolder, str))
                {
                    continue;
                }
                else
                {
                    tempFolder = new AveItemObject
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

        [QueryReview("2012/05/22", "Oliver Luo")]
        private void DoRecycleBin(AveItemObject parentFolder, string fullName, string itemName, Guid Id, int itemId, Dictionary<int, AveItemObject> items, DateTime eventTime, Dictionary<string, AveItemObject> noPropertyFolders, string dirName, string modifyBy)
        {
            AveItemObject folder = null;
            foreach (AveItemObject afc in parentFolder.SubFolderObjs)
            {
                if (afc.FullUrl.Equals(fullName, StringComparison.OrdinalIgnoreCase))
                {
                    folder = afc;
                    if (noPropertyFolders.ContainsKey(fullName))
                    {
                        noPropertyFolders.Remove(fullName);
                    }
                    break;
                }
            }
            if (folder == null)
            {
                AveItemObject item = null;

                #region Find Item
                if (itemId == 0)
                {
                    foreach (AveItemObject aic in parentFolder.SubItemObjs)
                    {
                        if (aic.DocID == Id)
                        {
                            item = aic;
                            break;
                        }
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
                else//item finded
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

        [QueryReview("2012/05/22", "Oliver Luo")]
        private AveItemObject GetCurrentFolder(AveItemObject parent, string fullUrl, bool deleteNoPropertyFolders, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            AveItemObject folder = null;

            foreach (AveItemObject afc in parent.SubFolderObjs)
            {
                if (afc.FullUrl.Equals(fullUrl, StringComparison.OrdinalIgnoreCase))
                {
                    folder = afc;
                    if (deleteNoPropertyFolders && noPropertyFolders.ContainsKey(fullUrl))
                    {
                        RemoveNoPropertyFolder(fullUrl, noPropertyFolders);
                    }
                    break;
                }
            }

            if (folder == null)
            {
                folder = new AveItemObject();
                parent.SubFolderObjs.Add(folder);
            }
            return folder;
        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private void SetNoPropertyFolderUrl(string sourceFullUrl, string newUrl, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            try
            {
                Dictionary<string, string> mapping = new Dictionary<string, string>();
                foreach (var folder in noPropertyFolders)
                {
                    string url = folder.Key;
                    if (url.StartsWith(sourceFullUrl + "/", StringComparison.OrdinalIgnoreCase))
                    {
                        url = newUrl + url.Substring(sourceFullUrl.Length);
                        mapping.Add(folder.Key, url);
                        folder.Value.DirName = url.Substring(0, url.LastIndexOf('/') + 1);
                        folder.Value.FullUrl = url;
                    }
                }
                foreach (var map in mapping)
                {
                    AveItemObject folder = noPropertyFolders[map.Key];
                    noPropertyFolders.Remove(map.Key);
                    noPropertyFolders.Add(map.Value, folder);
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while doing method SetNoPropertyFolderUrl. Error Message:{0}", e);
            }
        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private void AnalyseItemEvent(AveItemObject parentFolder, AveItemObject item, NativeChangeType nativeChageType, ChangeType changeType, string fullName, Dictionary<int, AveItemObject> items)
        {
            //当用Sharepoint designer在同一个list下move一个document的时候，触发的事件为rename。
            //当用Sharepoint designer去跨list move一个document的时候，触发的事件为move into,也让其走rename逻辑。
            if (nativeChageType == NativeChangeType.Rename || nativeChageType == NativeChangeType.MoveInto)
            {
                item.isRename = true;
                item.ChangeType = ChangeType.Edit; //we regard rename as edit
                item.ItemName = fullName.Substring(fullName.LastIndexOf('/') + 1);
                item.FullUrl = fullName;
                return;
            }
            if (nativeChageType == NativeChangeType.AssignmentAdd || nativeChageType == NativeChangeType.AssignmentDelete || nativeChageType == (NativeChangeType.RoleAdd | NativeChangeType.AssignmentAdd))
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

        [QueryReview("2012/05/22", "Oliver Luo")]
        private void AnalyseFolderEvent(AveItemObject parentFolder, AveItemObject folder, NativeChangeType nativeChageType, ChangeType changeType, string sourceFullUrl, Dictionary<int, AveItemObject> items, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            if (nativeChageType == NativeChangeType.Rename)
            {
                //当用Sharepoint Designer在同一个list下去move一个folder的时候，触发的事件即为rename，在这里标记为true。
                folder.isRename = true;//For replicator
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
            else
            {
                folder.ItemPermissionChanged = false;
            }
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


        private string GetAttachmentFolderUrl(Guid itemDocId, Guid listId, Guid siteId)
        {
            var dirNameAndRowId = GetItemDirNameAndIdByDocId(itemDocId, siteId);
            string listRootFolderUrl = GetListRootFolderUrl(listId, siteId);
            string itemDirName = string.IsNullOrEmpty(listRootFolderUrl) ? dirNameAndRowId.Item1 : listRootFolderUrl;
            return string.Format("{0}/Attachments/{1}", itemDirName, dirNameAndRowId.Item2);
        }

        private string GetListRootFolderUrl(Guid listId, Guid siteId)
        {
            string listRootFolderUrl = "";
            string query = "Select top 1 DirName,LeafName from AllDocs where ListId=@ListId and SiteId=@SiteId order by Dirname,LeafName";
            mQueryWorker.ResetCommand(System.Data.CommandType.Text);
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ListId", listId);
            using (SqlDataReader sr = mQueryWorker.ExecuteReader(query))
            {
                while (sr.Read())
                {
                    listRootFolderUrl = sr.GetString(0) + "/" + sr.GetString(1);
                }
            }
            return listRootFolderUrl;
        }

        private Tuple<string, string> GetItemDirNameAndIdByDocId(Guid itemDocId, Guid siteId)
        {
            string itemDirName = "";
            string itemDoclibRowId = "";
            mQueryWorker.ResetCommand(System.Data.CommandType.Text);
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ItemId", itemDocId);
            using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.ItemDirNameAndLibRowId13))
            {
                while (sr.Read())
                {
                    itemDirName = sr[0].ToString();
                    itemDoclibRowId = sr[1].ToString();
                }
            }
            return new Tuple<string, string>(itemDirName, itemDoclibRowId);
        }

        private List<Guid> GetItemAttachments(Guid siteId, Guid listId, Guid itemDocId)
        {
            List<Guid> attachments = new List<Guid>();
            try
            {
                string attachmentFolderUrl = GetAttachmentFolderUrl(itemDocId, listId, siteId);
                mQueryWorker.ResetCommand(System.Data.CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@AttachmentDirName", attachmentFolderUrl);
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.AttachmentsByCustomItem13))
                {
                    while (sr.Read())
                    {
                        attachments.Add(new Guid(sr[0].ToString()));
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while get attachments for extra items.SiteId:{0},ListId:{1},ItemDocId:{2},Error:{3}", siteId, listId, itemDocId, e);
            }
            return attachments;
        }

        private void AddAttachmentsGuidToExtraItems(AveFolderCache folderCache, List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            List<Guid> attachmentCollection = new List<Guid>();
            for (int i = 0; i < extraItems.Count; i++)
            {
                var attachments = GetItemAttachments(folderCache.SiteId, folderCache.ListId, extraItems[i].Id);
                attachmentCollection.AddRange(attachments);
            }
            attachmentCollection.ForEach(delegate (Guid attachmentGuid)
            {
                if (!extraItems.Exists(itemBaseInfo => itemBaseInfo.Id == attachmentGuid))
                {
                    extraItems.Add(new AveDiscoverExtraItemBaseInfo() { Id = attachmentGuid, ObjectType = ChangeObjectType.File });
                }
            });
        }
        
        private void ItemChanged(List<EventObject> allEvents, Dictionary<Guid, DocObject> allDocs, AveItemObject rootFolder,
            AveFolderCache folderCache, AveListObject listObject, AveDiscoverReader discoverReader,
            Dictionary<string, AveItemObject> noPropertyFolders, List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            try
            {
                string attachmentUrl = null;
                if (listObject != null)
                {
                    attachmentUrl = listObject.RootFolderUrl + "/Attachments/";
                }
                var extraItemInfos = GetItemsDocsInfo(folderCache.SiteId, extraItems);
                var result = HandleItemChanged(allEvents, allDocs, rootFolder, folderCache, listObject, discoverReader, noPropertyFolders,attachmentUrl, extraItemInfos);
                HandleExtraItems(rootFolder, listObject, noPropertyFolders, extraItemInfos, result.Items, result.SystemItems, attachmentUrl, result.Attachments);
                DoLastCache(result.SystemItems, result.SystemItemViews, result.ItemAlerts, result.FolderAlerts, result.Items, result.Attachments, rootFolder, listObject, discoverReader, noPropertyFolders);
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }
        public AveItemChangedResultCollection HandleItemChanged(IEnumerable<EventObject> allEvents, Dictionary<Guid, DocObject> allDocs, AveItemObject rootFolder, AveFolderCache folderCache, AveListObject listObject, IAveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders,string attachmentUrl, List<Dictionary<string, object>> extraItemInfos)
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
                    var nativeChangeType = (NativeChangeType)ev.EventType;
                    var changeType = DiscoverUtility.GetChangeType(nativeChangeType);
                    var objectType = (ChangeObjectType)ev.ObjectType;
                    switch (objectType)
                    {
                        case ChangeObjectType.View:
                            Guid viewId = ev.Guid0;
                            if (systemItemViews.ContainsKey(viewId) && changeType == ChangeType.Delete)
                            {
                                systemItemViews.Remove(viewId);
                            }
                            else
                            {
                                systemItemViews[viewId] = ev;
                            }
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
                            string fullName = GetFullName(ev, tempDoc, objectType);
                            if (string.IsNullOrEmpty(fullName))
                            {
                                break;
                            }
                            string dirName = fullName.LastIndexOf('/') > 0 ? fullName.Substring(0, fullName.LastIndexOf('/')) : fullName;
                            if (InvalidDirName(dirName, tempDoc))
                            {
                                break;
                            }
                            string itemName = GetItemName(ev, tempDoc);

                            if (tempDoc.Type == 1 && listObject == null && discoverReader.IsUnusedFolder(itemName, true))
                            {
                                break;
                            }
                            Guid docId = GetDocId(ev, tempDoc);

                            if (objectType == ChangeObjectType.File)//If system we should identify whether it is attachemnt
                            {
                                if (!string.IsNullOrEmpty(attachmentUrl) && fullName.StartsWith(attachmentUrl, StringComparison.OrdinalIgnoreCase) && listObject.Type != DocList) //Attachment,Library中可以创建出名为“Attachments”的folder
                                {
                                    if (!(discoverReader is AveReplicatorDiscoverReader))//attachment变化，item也会变化，replicator需要获取全部的attachment信息，否则会出现丢失的问题
                                    {
                                        AddAttachmentForIB(attachments, ev, tempDoc, fullName, changeType);
                                        break;
                                    }
                                }
                            }

                            if (objectType == ChangeObjectType.Folder && !string.IsNullOrEmpty(attachmentUrl) && (fullName + '/').StartsWith(attachmentUrl, StringComparison.OrdinalIgnoreCase) && listObject.Type != DocList) //Attachments及其子folder不需要备份，FB的时候也过滤。
                            {
                                continue;
                            }

                            AveItemObject parentFolder = null;
                            if ((parentFolder = GetParentFolder(dirName, rootFolder, noPropertyFolders)) == null)
                            {
                                if (objectType == ChangeObjectType.Folder && changeType != ChangeType.None && docId.Equals(rootFolder.DocID))
                                {
                                    rootFolder.ChangeType = ChangeType.Edit;
                                }
                                break;
                            }

                            if (tempDoc.Id == Guid.Empty) //id is null ,delete from recyclebin
                            {
                                string modifyBy = string.Empty;
                                if (!string.IsNullOrEmpty(ev.ModifiedBy))
                                {
                                    modifyBy = ev.ModifiedBy;
                                }
                                if (ev.ItemId == 0)//System file or folder delete  
                                {
                                    DoRecycleBin(parentFolder, fullName, string.Empty, docId, 0, null, eventTime, noPropertyFolders, dirName, modifyBy);
                                }
                                else
                                {
                                    int itemId = ev.ItemId;
                                    if (itemAlerts.ContainsKey(itemId))
                                    {
                                        itemAlerts.Remove(itemId);
                                    }
                                    DoRecycleBin(parentFolder, fullName, itemName, docId, itemId, items, eventTime, noPropertyFolders, dirName, modifyBy);
                                }
                                break;
                            }

                            int docLibRowId = 0;
                            if (tempDoc.DoclibRowId != 0)
                            {
                                docLibRowId = tempDoc.DoclibRowId;//must not be System item, System should use docId to cache it
                            }

                            if (tempDoc.Type == 1)//Folder
                            {
                                AveItemObject folder = GetCurrentFolder(parentFolder, fullName, true, noPropertyFolders);
                                discoverReader.ReadItemContentForIB(folder, tempDoc);
                                folder.IsCurrentVersion = true;
                                folder.FullUrl = fullName;
                                folder.ItemName = itemName;
                                folder.SourceName = itemName;
                                folder.LeafName = fullName.Substring(dirName.Length + 1);
                                folder.DirName = dirName;
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
                                if (nativeChangeType == NativeChangeType.AssignmentDelete)
                                {
                                    if (ev.Int0 != 0)
                                    {
                                        AveSecurityObject deleteRoleAssignment = new AveSecurityObject();
                                        // 删除RoleAssignmet时，
                                        // int0存放principalID,int1存放RoleID
                                        deleteRoleAssignment.ObjectType = SecurityType.Assignment;
                                        deleteRoleAssignment.PrincipleId = ev.Int0;
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
                                        folder.DeleteRoleAssignments.Add(deleteRoleAssignment);
                                    }
                                }
                                AnalyseFolderEvent(parentFolder, folder, nativeChangeType, changeType, (dirName + "/" + itemName).Trim('/'), items, noPropertyFolders);
                            }

                            else //ListItem or Document
                            {
                                AveItemObject item = null;
                                bool hasAddProperty = false;

                                if (docLibRowId == 0)//System item
                                {
                                    if (!systemItems.ContainsKey(docId))
                                    {
                                        item = new AveItemObject();
                                        hasAddProperty = true;
                                    }
                                    else
                                    {
                                        item = systemItems[docId];
                                    }
                                }
                                else if (!items.ContainsKey(docLibRowId))
                                {
                                    hasAddProperty = true;
                                    if (itemAlerts.ContainsKey(docLibRowId))//this item may related an alert,when we do alert change,we cached it
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
                                if (hasAddProperty == true)//Item is null created
                                {
                                    discoverReader.ReadItemContentForIB(item, tempDoc);
                                    if (listObject != null && listObject.Type == DocList)
                                    {
                                        item.ObjType = ItemType.Document;
                                        item.SourceName = itemName;
                                    }
                                    else
                                    {
                                        item.ObjType = ItemType.Item;
                                    }
                                    item.FullUrl = fullName;
                                    item.IsCurrentVersion = true;
                                    item.ItemName = itemName;
                                    parentFolder.SubItemObjs.Add(item);
                                    if (docLibRowId != 0)
                                    {
                                        items.Add(docLibRowId, item);
                                    }
                                    else
                                    {
                                        systemItems.Add(docId, item);
                                    }
                                }
                                item.EventTime = eventTime;
                                if (!string.IsNullOrEmpty(ev.ModifiedBy))
                                {
                                    item.ModifyBy = ev.ModifiedBy;
                                }
                                //把document与listItem的RoleAssignment删除记录load出来
                                if (nativeChangeType == NativeChangeType.AssignmentDelete)
                                {
                                    if (ev.Int0 != 0)
                                    {
                                        AveSecurityObject deleteRoleAssignment = new AveSecurityObject();
                                        // 删除RoleAssignmet时，
                                        // int0存放principalID,int1存放RoleID
                                        deleteRoleAssignment.ObjectType = SecurityType.Assignment;
                                        deleteRoleAssignment.PrincipleId = ev.Int0;
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
                                AnalyseItemEvent(parentFolder, item, nativeChangeType, changeType, fullName, items);
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, "Error occur while access data from method QueryListItemForIB. ErrorMessage:{0}", e);
                }
            }

           // HandleExtraItems(rootFolder, listObject, noPropertyFolders, extraItemInfos, items, systemItems, attachmentUrl, attachments);
            return result;
        }
        private void HandleExtraItems(AveItemObject rootFolder, AveListObject listObject, Dictionary<string, AveItemObject> noPropertyFolders, List<Dictionary<string, object>> extraItemInfos, Dictionary<int, AveItemObject> items, Dictionary<Guid, AveItemObject> systemItems, string attachmentUrl, Dictionary<int, List<AveItemObject>> attachments)
        {
            //vault模块因item export failed需要IB重新备份。
            if (extraItemInfos != null && extraItemInfos.Count > 0)
            {
                foreach (var itemInfo in extraItemInfos)
                {
                    var failDocLibRowId = itemInfo["DoclibRowId"] is DBNull ? -1 : (int)itemInfo["DoclibRowId"];
                    var failDocId = itemInfo["Id"] is DBNull ? Guid.Empty : (Guid)itemInfo["Id"];
                    if (itemInfo["Id"] is DBNull || itemInfo["LeafName"] is DBNull || items.ContainsKey(failDocLibRowId) || systemItems.ContainsKey(failDocId))
                    {
                        continue;
                    }
                    var itemName = (string)itemInfo["LeafName"];
                    var dirName = ((string)itemInfo["DirName"]).Trim('/');
                    var fullName = (dirName + "/" + itemName).Trim('/');
                    var hasStream = (bool)((int)itemInfo["HasStream"] == 1 ? true : false);
                    var sizeObj = itemInfo["Size"];
                    var size = sizeObj != null && sizeObj != DBNull.Value ? (int)sizeObj : 0;
                    var docFlags = (int?)itemInfo["DocFlags"];
                    var deleteTransactionId = (byte[])itemInfo["DeleteTransactionId"];
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
                    if ((byte)itemInfo["Type"] == 1) //Folder
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

        public void QueryItemFromExtraItemList(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, AveDiscoverReader discoverReader,
            Dictionary<string, AveItemObject> noPropertyFolders, List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            try
            {
                string attachmentUrl = null;
                if (listObject != null)
                {
                    attachmentUrl = listObject.RootFolderUrl + "/Attachments/";
                }
                var extraItemInfos = GetItemsDocsInfo(folderCache.SiteId, extraItems);
                var result = new AveItemChangedResultCollection();
                HandleExtraItems(folderObject, listObject, noPropertyFolders, extraItemInfos, result.Items, result.SystemItems, attachmentUrl, result.Attachments);
                DoLastCache(result.SystemItems, result.SystemItemViews, result.ItemAlerts, result.FolderAlerts, result.Items, result.Attachments, folderObject, listObject, discoverReader, noPropertyFolders);
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }
        [QueryReview("2012/05/22", "Oliver Luo")]
        private bool IsContainContentTypeId(Dictionary<byte[], AveContentTypeObject> contentTypeChanges, byte[] contentTypeId, out AveContentTypeObject contentTypeChange)
        {
            foreach (var kvp in contentTypeChanges)
            {
                byte[] bs = kvp.Key;
                if (bs.Length != contentTypeId.Length)
                {
                    continue;
                }
                else
                {
                    int i = 0;
                    for (; i < bs.Length; i++)
                    {
                        if (bs[i] != contentTypeId[i])
                        {
                            break;
                        }
                    }
                    if (i == bs.Length)
                    {
                        contentTypeChange = kvp.Value;
                        return true;
                    }
                }
            }
            contentTypeChange = null;
            return false;
        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private void RemoveContentType(Dictionary<byte[], AveContentTypeObject> ContentTypeChanges, byte[] contentTypeId)
        {
            foreach (var kvp in ContentTypeChanges)
            {
                byte[] bs = kvp.Key;
                if (bs.Length != contentTypeId.Length)
                {
                    continue;
                }
                else
                {
                    int i = 0;
                    for (; i < bs.Length; i++)
                    {
                        if (bs[i] != contentTypeId[i])
                        {
                            break;
                        }
                    }
                    if (i == bs.Length)
                    {
                        ContentTypeChanges.Remove(kvp.Key);
                        return;
                    }
                }
            }
        }


        [QueryReview("2012/12/11", "Austin Han", true, "Add SiteId in the where condition to improve the performance")]
        private void QueryViewItemsForIB(Dictionary<Guid, AveItemObject> systemItems, Dictionary<Guid, EventObject> views, AveItemObject rootFolder, AveListObject listObject, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("QueryService.Discover.QueryViewItemsForIB"))
            {
                var docIds = new Dictionary<Guid, EventObject>();
                try
                {
                    var index = 0;
                    var keys = new List<Guid>(views.Keys);
                    while (index < keys.Count)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int idCount = 0; index < keys.Count && idCount < 800; ++idCount)
                        {
                            var key = keys[index++];
                            var changeType = DiscoverUtility.GetChangeType((NativeChangeType)views[key].EventType);
                            if (changeType == ChangeType.Edit)
                            {
                                sb.AppendFormat("'{0}',", key);
                            }
                        }
                        if (sb.Length > 0)
                        {
                            sb.Length--;
                            var ids = sb.ToString();
                            var command = string.Format(AveDiscoverQueryString.SpecifyViewDocIds, ids);

                            using (SqlDataReader sr = mQueryWorker.ExecuteReader(command))
                            {
                                while (sr.Read())
                                {
                                    var id = (Guid)sr["tp_PageUrlID"];
                                    var viewId = (Guid)sr["tp_ID"];
                                    if (!systemItems.ContainsKey(id))
                                    {
                                        docIds[id] = views[viewId];
                                    }
                                    else
                                    {
                                        if (systemItems[id].EventTime < views[viewId].EventTime)
                                        {
                                            systemItems[id].EventTime = views[viewId].EventTime;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                QueryViewInDocsWithDocId(docIds, rootFolder, discoverReader, noPropertyFolders);
            }
        }

        private void QueryViewInDocsWithDocId(Dictionary<Guid, EventObject> docIds, AveItemObject rootFolder, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("QueryService.Discover.QueryViewInDocsWithDocId"))
            {
                var index = 0;
                var ids = docIds.Keys.ToList();
                while (index < ids.Count)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int idCount = 0; index < ids.Count && idCount < 800; ++idCount)
                    {
                        sb.AppendFormat("'{0}',", ids[index++]);
                    }
                    if (sb.Length > 0)
                    {
                        sb.Length--;
                        var condition = string.Format(DiscoverConditionString.DocIdsFor13, sb.ToString());
                        try
                        {
                            //由于IB的时候，需要查询比FB多查询个列  DirName, 所以在这里把DirName加入查询列里
                            string commText = AddAllDocsDirName(discoverReader.GetAllItemsInAllDocQueryString().Replace("@WHERE", condition));
                            using (SqlDataReader sr = mQueryWorker.ExecuteReader(commText))
                            {
                                while (sr.Read())
                                {
                                    string dirName = (string)sr["DirName"];
                                    string leafName = (string)sr["LeafName"];
                                    Guid docId = (Guid)sr["Id"];
                                    AveItemObject parentFolder = null;
                                    if ((parentFolder = GetParentFolder(dirName, rootFolder, noPropertyFolders)) == null)
                                    {
                                        continue;
                                    }
                                    AveItemObject item = new AveItemObject();
                                    discoverReader.ReadItemContent(item, sr);
                                    item.DirName = dirName;
                                    item.FullUrl = (dirName + '/' + leafName).Trim('/');
                                    item.EventTime = docIds[docId].EventTime;
                                    item.ChangeType = ChangeType.Edit;
                                    item.ObjType = ItemType.Document;
                                    parentFolder.SubItemObjs.Add(item);
                                }
                            }
                        }
                        catch (SqlException queryException)
                        {
                            throw new AveQueryException(queryException);
                        }
                        catch (Exception e)
                        {
                            throw new AveQueryException(e.Message, e);
                        }
                    }
                }
            }
        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private void ExecuteListItemFB(string commText, AveItemObject rootFolder, Dictionary<int, AveItemObject> itemAlerts, Dictionary<Guid, AveItemObject> systemItems, AveListObject listObject, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.QueryListItemsForFB"))
            {
                AveItemObject previousItem = null;
                Guid lastItemId = Guid.Empty;
                Dictionary<int, AveItemObject> attachmentItems = new Dictionary<int, AveItemObject>();
                Dictionary<int, List<AveItemObject>> attachments = new Dictionary<int, List<AveItemObject>>();

                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(commText))
                    {
                        while (sr.Read())
                        {
                            string dirName = (string)sr["DirName"];
                            string leafName = (string)sr["LeafName"];
                            string fullName = (dirName + '/' + leafName).Trim('/');
                            int? docLibRowId = sr["DoclibRowId"] is DBNull ? (int?)null : (int?)sr["DoclibRowId"];
                            Guid docId = (Guid)sr["Id"];
                            if (systemItems != null && systemItems.ContainsKey(docId))
                            {
                                //this item should have queryed in IB
                                continue;
                            }

                            try
                            {

                                //Version
                                if (docId.Equals(lastItemId) && previousItem != null)
                                {
                                    AveVersionObject version = new AveVersionObject();
                                    discoverReader.ReadVersionContent(version, sr);
                                    AddVersion(version, previousItem, sr, discoverReader);
                                    continue;
                                }

                                AveItemObject parentFolder = null;
                                if ((parentFolder = GetParentFolder(dirName, rootFolder, noPropertyFolders)) == null)
                                {
                                    continue;
                                }

                                //Folder
                                if ((byte)sr["Type"] == 1)
                                {
                                    AveItemObject folder = GetCurrentFolder(parentFolder, fullName, true, noPropertyFolders);
                                    //if (!folder.PropertyAdded)
                                    //{
                                    discoverReader.ReadItemContent(folder, sr);
                                    folder.DirName = dirName;
                                    folder.FullUrl = (folder.DirName + "/" + folder.LeafName).Trim('/');
                                    folder.ObjType = ItemType.Folder;
                                    //folder.PropertyAdded = true;

                                    AveVersionObject version = new AveVersionObject();
                                    discoverReader.ReadVersionContent(version, sr);
                                    AddVersion(version, folder, sr, discoverReader);

                                    if (docLibRowId.HasValue)
                                    {
                                        attachmentItems.Add(docLibRowId.Value, folder);
                                    }
                                    //}
                                    previousItem = folder;
                                }
                                //ListItem or Document
                                else
                                {
                                    AveItemObject item = new AveItemObject();
                                    discoverReader.ReadItemContent(item, sr);
                                    item.DirName = dirName;
                                    item.FullUrl = (item.DirName + "/" + item.LeafName).Trim('/');
                                    if (listObject.Type != DocList && docLibRowId.HasValue)
                                    {
                                        item.ObjType = ItemType.Item;
                                    }
                                    else
                                    {
                                        item.ObjType = ItemType.Document;
                                    }
                                    item.ChangeType = ChangeType.Edit;//将item的changetype先赋值（只有是edit才会走这的逻辑），这样找到的view对象的changetype不为none，alert在后面有自己的处理。

                                    parentFolder.SubItemObjs.Add(item);
                                    if (itemAlerts != null)
                                    {
                                        AveItemObject alertItem;
                                        if (itemAlerts.TryGetValue(docLibRowId.Value, out alertItem))
                                        {
                                            int itemId = alertItem.ID.Value;
                                            item.AlertObjs = alertItem.AlertObjs;
                                            //item.ChangeType = alertItem.ChangeType;
                                        }
                                    }

                                    AveVersionObject version = new AveVersionObject();
                                    discoverReader.ReadVersionContent(version, sr);
                                    AddVersion(version, item, sr, discoverReader);

                                    if (docLibRowId.HasValue)
                                    {
                                        if (item.ObjType == ItemType.Item)
                                        {
                                            attachmentItems.Add(docLibRowId.Value, item);
                                        }
                                    }
                                    previousItem = item;
                                }
                                lastItemId = docId;
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, "Error occur while access data from ExecuteListItemFB. DirName:{0}. LeafName:{1}. Id:{2}. ErrorMessage:{3}",
                                    dirName, leafName, docId, e);
                            }
                        }
                    }
                    if (attachments.Count > 0)
                    {
                        AddAttachmentToItem(attachments, attachmentItems);
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }
        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private void AddAttachmentToItem(Dictionary<int, List<AveItemObject>> attachments, Dictionary<int, AveItemObject> items)
        {
            if (attachments.Count <= 0 || items.Count <= 0)
            {
                return;
            }
            foreach (var kvp in attachments)
            {
                //已经被删除的ListItem的附件是不需要Add进来的
                if (items.ContainsKey(kvp.Key))
                {
                    items[kvp.Key].AttachmentObjs = kvp.Value;
                }
            }
        }

        private List<AveItemObject> QueryItemsAlertsInDocs(Dictionary<int, AveItemObject> itemAlerts, AveItemObject rootFolder, AveListObject listObject, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("QueryService.Discover.QueryItemsAlertsInDocs"))
            {
                //记录查出来的Item，用来查找对应的Version和UserData信息。
                List<AveItemObject> items = new List<AveItemObject>();
                AveItemObject previousItem = null;
                Guid lastItemId = Guid.Empty;
                AveItemObject parentFolder = null;

                System.Text.StringBuilder ids = new System.Text.StringBuilder();
                foreach (var item in itemAlerts)
                {
                    ids.Append(item.Key + ",");
                }
                ids.Length -= 1;

                string dirNameCondition = string.IsNullOrEmpty(rootFolder.FullUrl) ? "%" : rootFolder.FullUrl + "%";
                string itemAlertCondition = string.Format(DiscoverConditionString.ItemDocLibRowIds, dirNameCondition, ids);

                //IB需要查询DirName。ParentId用来查询AUD表时，补全索引。
                string queryString = AddAllDocsDirNameAndParentId(discoverReader.GetAllItemsInAllDocQueryString()).Replace("@WHERE", itemAlertCondition);
                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(queryString))
                    {
                        while (sr.Read())
                        {
                            string dirName = (string)sr["DirName"];
                            string leafName = (string)sr["LeafName"];
                            string fullName = (dirName + '/' + leafName).Trim('/');
                            int? docLibRowId = sr["DoclibRowId"] is DBNull ? (int?)null : (int?)sr["DoclibRowId"];
                            Guid docId = (Guid)sr["Id"];
                            try
                            {
                                if (!docId.Equals(lastItemId) || previousItem == null)
                                {
                                    if ((parentFolder = GetParentFolder(dirName, rootFolder, noPropertyFolders)) == null)
                                    {
                                        continue;
                                    }

                                    if ((byte)sr["Type"] == 1)
                                    {
                                        logger.Debug("It is not a item alert. Url: {0}", fullName);
                                        continue;
                                    }

                                    AveItemObject item = new AveItemObject();
                                    item.ParentID = (Guid)sr["ParentId"];
                                    discoverReader.ReadItemContent(item, sr);
                                    item.DirName = dirName;
                                    item.FullUrl = (item.DirName + "/" + item.LeafName).Trim('/');
                                    if (listObject.Type != DocList && docLibRowId.HasValue)
                                    {
                                        item.ObjType = ItemType.Item;
                                    }
                                    else
                                    {
                                        item.ObjType = ItemType.Document;
                                    }
                                    AveItemObject alertItem;
                                    if (itemAlerts.TryGetValue(docLibRowId.Value, out alertItem))
                                    {
                                        int itemId = alertItem.ID.Value;
                                        item.AlertObjs = alertItem.AlertObjs;
                                    }
                                    item.ChangeType = ChangeType.Edit;
                                    parentFolder.SubItemObjs.Add(item);
                                    items.Add(item);

                                    previousItem = item;
                                    lastItemId = item.DocID;
                                }
                                AveVersionObject version = new AveVersionObject();
                                discoverReader.ReadVersionContent(version, sr);
                                AddVersion(version, previousItem, sr, discoverReader);
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, "Error occur while access data from QueryItemsAlertsInDocs. DirName:{0}. LeafName:{1}. Id:{2}. ErrorMessage:{3}",
                                    dirName, leafName, docId, e);
                            }
                        }

                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                return items;
            }
        }

        [QueryReview("2012/12/10", "Austin Han")]
        private void QueryItemsAlerts(Dictionary<int, AveItemObject> itemAlerts, AveItemObject rootFolder, AveListObject listObject, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            if (itemAlerts.Count <= 0)
            {
                return;
            }
            using (new AvePerformanceScope("AveDiscoverQuery.QueryItemsAlerts"))
            {
                var items = QueryItemsAlertsInDocs(itemAlerts, rootFolder, listObject, discoverReader, noPropertyFolders);
                if (items != null && items.Count > 0)
                {
                    StringBuilder ids = new StringBuilder();
                    foreach (var item in items)
                    {
                        ids.Append("'" + item.DocID + "',");
                    }
                    ids.Length -= 1;
                    var condition = discoverReader.GetItemVersionsWithDocIdsCondition();
                    QueryItemVersions(items.ToDictionary(key => key.DocID, value => value), discoverReader, listObject, string.Format(condition, ids));
                }
            }
        }

        [QueryReview("2012/12/10", "Austin Han")]
        private void QueryFoldersAlerts(Dictionary<Guid, AveAlertObject> folderAlerts, AveItemObject rootFolder, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AveDiscoverQuery.QueryFoldersAlerts"))
            {
                if (folderAlerts.Count <= 0)
                {
                    return;
                }

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (Guid guid in folderAlerts.Keys)
                {
                    sb.Append("'" + guid.ToString() + "',");
                }
                string guids = sb.ToString().TrimEnd(',');

                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.FolderAlerts.Replace("@WHERE", guids)))
                    {
                        while (sr.Read())
                        {
                            try
                            {
                                Guid alertId = sr.GetGuid(0);
                                string properties = sr.IsDBNull(1) ? string.Empty : sr.GetString(1);
                                string folderFullUrl = GetFileFilter(properties);
                                if (string.IsNullOrEmpty(folderFullUrl))
                                {
                                    continue;
                                }
                                string dir = folderFullUrl.Substring(0, folderFullUrl.LastIndexOf('/'));

                                AveItemObject parent = null;
                                if ((parent = GetParentFolder(dir, rootFolder, noPropertyFolders)) == null)
                                {
                                    return;
                                }

                                AveItemObject folder = GetCurrentFolder(parent, folderFullUrl, true, noPropertyFolders);

                                //if (!folder.PropertyAdded)
                                //{
                                folder.AlertObjs = new Dictionary<Guid, AveAlertObject>();
                                folder.FullUrl = folderFullUrl;
                                folder.LeafName = folderFullUrl.Substring(folderFullUrl.LastIndexOf('/') + 1);
                                if (!noPropertyFolders.ContainsKey(folderFullUrl))
                                {
                                    noPropertyFolders.Add(folderFullUrl, folder);
                                }
                                //}

                                if (!folder.AlertObjs.ContainsKey(alertId))
                                {
                                    ChangeType changeType = folderAlerts[alertId].ChangeType;
                                    AveAlertObject alertObject = new AveAlertObject
                                    {
                                        Id = alertId,
                                        ChangeType = changeType
                                    };
                                    folder.AlertObjs.Add(alertId, alertObject);
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, "Error occur while access data from QueryFoldersAlerts. ErrorMessage:{0}", e);
                            }
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }
        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private void SetDeleteFolders(AveItemObject rootFolder, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            try
            {
                foreach (var v in noPropertyFolders)
                {
                    string fullUrl = v.Key;
                    string dirName = fullUrl.Substring(0, fullUrl.LastIndexOf('/'));
                    AveItemObject parentFolder = null;
                    if ((parentFolder = GetParentFolder(dirName, rootFolder, noPropertyFolders)) == null)
                    {
                        continue;
                    }
                    AveItemObject folder = GetCurrentFolder(parentFolder, fullUrl, false, noPropertyFolders);
                    folder.ChangeType = ChangeType.Delete;

                    if (parentFolder.NoTypeDeleteItems.ContainsKey(fullUrl))
                    {
                        AveItemObject temp = parentFolder.NoTypeDeleteItems[fullUrl];
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

        [QueryReview("2012/05/22", "Oliver Luo")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "DocId is a part of Keys")]
        private void AddVersionToItems(Dictionary<int, AveItemObject> items, AveListObject listObject, AveDiscoverReader discoverReader)
        {
            if (items.Count <= 0)
            {
                return;
            }

            bool isSpecialLibrary = listObject != null && listObject.MaxMajorwithMinorVersionCount.HasValue && listObject.Type == 1;

            var allItems = items.Select(item => item.Value.DocID).ToArray();
            int index = 0;
            while (index < allItems.Length)
            {
                List<Guid> queryItemDocIds = new List<Guid>();
                //SQL command text limited 64k
                for (var idCount = 0; idCount < 800; ++idCount)
                {
                    queryItemDocIds.Add(allItems[index++]);
                    if (index >= allItems.Length)
                    {
                        break;
                    }
                }
                if (isSpecialLibrary && !(discoverReader is AveExtenderDiscoverReader))
                {
                    AddVersionToItemsForSpecialLibrary(items, discoverReader, queryItemDocIds);
                }
                else
                {
                    AddVersionToItemsForNormal(items, discoverReader, queryItemDocIds);
                }
            }
        }

        private void AddVersionToItemsForNormal(Dictionary<int, AveItemObject> items, AveDiscoverReader discoverReader, List<Guid> queryItemDocIds)
        {
            AveItemObject item = null;
            var tempCommand = AveQueryUtility.GetAllDocVersionsUserData_Select_AllUserData_AllDocs_AllDocVersions(queryItemDocIds, discoverReader);
            using (var sr = mQueryWorker.ExecuteReader(tempCommand))
            {
                while (sr.Read())
                {
                    try
                    {
                        var docId = (Guid)sr["tp_DocId"];
                        var id = (int)sr["DoclibRowId"];
                        if (item == null || item.DocID != docId)
                        {
                            item = items[id];
                        }
                        var version = new AveVersionObject();
                        discoverReader.ReadVersionContentWithDeleteState(version, sr);
                        AddVersion(version, item, sr, discoverReader);
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.WARN, "Error occur while access data from AddVersionToItems. ErrorMessage:{0}", e);
                    }
                }
            }
        }

        private void AddVersionToItemsForSpecialLibrary(Dictionary<int, AveItemObject> items, AveDiscoverReader discoverReader, List<Guid> queryItemDocIds)
        {
            AveItemObject item = null;
            var allDocVersionsCache = new Dictionary<Guid, List<int>>();
            var tempAllDocCommand = AveQueryUtility.GetAllDocVersionsForSpecialLibrary_Select_AllDocVersions(queryItemDocIds);
            using (var sr = mQueryWorker.ExecuteReader(tempAllDocCommand))
            {
                while (sr.Read())
                {
                    var allDocVersionId = (Guid)sr["Id"];
                    var uiVersion = (int)sr["UIVersion"];
                    if (allDocVersionsCache.ContainsKey(allDocVersionId))
                    {
                        allDocVersionsCache[allDocVersionId].Add(uiVersion);
                    }
                    else
                    {
                        var uiList = new List<int> { uiVersion };
                        allDocVersionsCache.Add(allDocVersionId, uiList);
                    }
                }
            }
            var tempAllUserDataCommand = AveQueryUtility.GetAllDocVersionsUserData_Select_AllUserData_AllDocs_AllDocVersions(queryItemDocIds, discoverReader);
            using (var sr = mQueryWorker.ExecuteReader(tempAllUserDataCommand))
            {
                while (sr.Read())
                {
                    var audDocId = (Guid)sr["tp_DocId"];
                    var audCalculatedVersion = Convert.ToInt32(sr["tp_CalculatedVersion"]);
                    var audTPCurrentVersion = Convert.ToInt32(sr["tp_IsCurrentVersion"]);
                    if (audTPCurrentVersion == 1 || (allDocVersionsCache.ContainsKey(audDocId) && allDocVersionsCache[audDocId].Exists(uiversion => uiversion == audCalculatedVersion)))
                    {
                        try
                        {
                            var docId = (Guid)sr["tp_DocId"];
                            var id = (int)sr["DoclibRowId"];
                            if (item == null || item.DocID != docId)
                            {
                                item = items[id];
                            }
                            var version = new AveVersionObject();
                            discoverReader.ReadVersionContentWithDeleteState(version, sr);
                            AddVersion(version, item, sr, discoverReader);
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "Error occur while access data from AddVersionToItems. ErrorMessage:{0}", e);
                        }
                    }
                }
            }
        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private void DoLastCache(Dictionary<Guid, AveItemObject> systemItems, Dictionary<Guid, EventObject> views, Dictionary<int, AveItemObject> itemAlerts, Dictionary<Guid, AveAlertObject> folderAlerts, Dictionary<int, AveItemObject> items, Dictionary<int, List<AveItemObject>> attachments, AveItemObject rootFolder, AveListObject listObject, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            try
            {
                QueryViewItemsForIB(systemItems, views, rootFolder, listObject, discoverReader, noPropertyFolders);
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.ERROR, "Error occur while doing  QueryViewItemsForIB. ErrorMessage:{0}", e);
            }

            try
            {
                QueryItemsAlerts(itemAlerts, rootFolder, listObject, discoverReader, noPropertyFolders);
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.ERROR, "Error occur while doing QueryItemsAlerts. ErrorMessage:{0}", e);
            }

            try
            {
                QueryFoldersAlerts(folderAlerts, rootFolder, noPropertyFolders);
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.ERROR, "Error occur while doing QueryFoldersAlerts. ErrorMessage:{0}", e);
            }

            try
            {
                QueryFolderProperty(noPropertyFolders, discoverReader, listObject);
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.ERROR, "Error occur while doing QueryFolderProperty. ErrorMessage:{0}", e);
            }

            SetDeleteFolders(rootFolder, noPropertyFolders);

            try
            {
                AddVersionToItems(items, listObject, discoverReader);
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.ERROR, "Error occur while doing AddVersionToItems. ErrorMessage:{0}", e);
            }

            AddAttachmentToItem(attachments, items);

        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private AveSecurityObject TryGetRoleSecurity(List<AveSecurityObject> securitys, int roleId)
        {
            foreach (AveSecurityObject asc in securitys)
            {
                if (asc.RoleId == roleId)
                {
                    return asc;
                }
            }
            AveSecurityObject security = new AveSecurityObject
            {
                RoleId = roleId,
                ObjectType = SecurityType.Role
            };
            securitys.Add(security);
            return security;
        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private void RoleSecurityChange(ChangeType changeType, SqlDataReader sr, Dictionary<int, List<AveSecurityObject>> securityChanges)
        {
            int roleId = sr.GetInt32(3);
            List<AveSecurityObject> roleSecuritys = null;
            securityChanges.TryGetValue(AveSecurityObject.RoleChangeId, out roleSecuritys);
            if (roleSecuritys == null)
            {
                roleSecuritys = new List<AveSecurityObject>();
                securityChanges.Add(AveSecurityObject.RoleChangeId, roleSecuritys);
            }

            AveSecurityObject security = TryGetRoleSecurity(roleSecuritys, roleId);

            if (security.ChangeType == ChangeType.Add)
            {
                if (changeType == ChangeType.Delete)
                {
                    roleSecuritys.Remove(security);
                    DeleteAllRelatedRole(securityChanges, roleId);
                    return;
                }
            }
            else
            {
                security.ChangeType = changeType;
            }
            if (security.ChangeType == ChangeType.Delete)
            {
                DeleteAllRelatedRole(securityChanges, roleId);
                return;
            }
            security.ScopeId = sr.IsDBNull(4) ? Guid.Empty : sr.GetGuid(4);
        }


        [QueryReview("2012/12/10", "Austin Han")]
        private void QueryUserProperty(Dictionary<int, AveSiteMemberObject> users, Guid siteId)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.QueryUserProperty"))
            {
                if (users == null || users.Count <= 0)
                {
                    return;
                }
                if (siteId != Guid.Empty)
                {
                    mQueryWorker.AddParameter("@siteId", siteId);
                }
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (var item in users)
                {
                    sb.Append("tp_id='" + item.Key + "' or ");
                }
                string ids = sb.ToString().Remove(sb.Length - 4, 4);

                string commText = @"select tp_id,tp_DomainGroup,tp_title,tp_login from UserInfo WITH(NOLOCK) where tp_SiteID=@siteId and (" + ids + ")";

                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(commText))
                    {
                        while (sr.Read())
                        {
                            int userId = sr.GetInt32(0);
                            users[userId].IsDomainGroup = sr.GetBoolean(1);
                            users[userId].Title = sr.GetString(2);
                            users[userId].Login = sr.GetString(3);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.ERROR, "Error occur while doing QueryUserProperty. ErrorMessage:{0}", e);
                }
            }
        }

        [QueryReview("2012/12/10", "Austin Han")]
        private void QueryGroupProperty(Dictionary<int, AveSiteMemberObject> groups, Guid siteId)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.QueryGroupProperty"))
            {
                if (groups == null || groups.Count <= 0)
                {
                    return;
                }
                if (siteId != Guid.Empty)
                {
                    mQueryWorker.AddParameter("@siteId", siteId);
                }
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (var item in groups)
                {
                    QueryUserProperty(item.Value.AddedMemberIds, siteId);
                    QueryUserProperty(item.Value.DeletedMemberIds, siteId);
                    sb.Append("ID='" + item.Key + "' or ");
                }
                string ids = sb.ToString().Remove(sb.Length - 4, 4);

                string commText = @"select ID,Title from Groups WITH(NOLOCK) where SiteId=@siteId and (" + ids + ")";
                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(commText))
                    {
                        while (sr.Read())
                        {
                            int groupId = sr.GetInt32(0);
                            groups[groupId].Title = sr.GetString(1);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.ERROR, "Error occur while doing QueryGroupProperty. ErrorMessage:{0}", e);
                }
            }
        }

        [QueryReview("2012/05/22", "Oliver Luo")]
        private AveSiteMemberObject GetUser(Dictionary<int, AveSiteMemberObject> users, int userId, DateTime eventTime)
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

        [QueryReview("2012/05/22", "Oliver Luo")]
        private bool InvalidDirName(string dirName, DocObject doc)
        {
            if (doc.Id == Guid.Empty)
            {
                return false;
            }
            else
            {
                return !dirName.Equals(doc.DirName.Trim('/'), StringComparison.OrdinalIgnoreCase);
            }
        }

        #endregion

        #region Discover

        /// <summary>
        /// 初始化DiscoverFolder
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="folderObj"></param>
        /// <param name="noPropertyFolders"></param>
        /// <param name="listObject"></param>
        /// <param name="discoverReader"></param>
        [QueryReview("2012/05/21", "Oliver Luo", true, "AllDocs增加Level索引")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "tp_MaxMajorwithMinorVersionCount is a part of Keys")]
        public void InitDiscoverFolder(AveFolderCache folderCache, AveItemObject folderObj, Dictionary<string, AveItemObject> noPropertyFolders, ref AveListObject listObject, AveDiscoverReader discoverReader)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AveDiscoverQuery.InitDiscoverFolder"))
            {
                mQueryWorker.AddParameter("@SiteId", folderCache.SiteId);
                mQueryWorker.AddParameter("@WebId", folderCache.WebId);
                noPropertyFolders.Add(folderObj.FullUrl, folderObj);
                if (folderCache.ListId == Guid.Empty)
                {
                    listObject = null;
                    QueryFolderProperty(noPropertyFolders, discoverReader, listObject);
                }
                else
                {
                    #region InitParentList
                    mQueryWorker.AddParameter("@ListId", folderCache.ListId);
                    listObject = QuerySingleListProperty(folderCache);
                    if (listObject != null)
                    {
                        folderCache.ListUrl = listObject.RootFolderUrl;// 只能在这设置，cache 层取不到 listObject 对象
                    }
                    QueryFolderProperty(noPropertyFolders, discoverReader, listObject);

                    #endregion

                    if (folderObj.ID.HasValue)//Query Attachemnts
                    {
                        Dictionary<int, AveItemObject> itemEntities = new Dictionary<int, AveItemObject>();
                        itemEntities.Add(folderObj.ID.Value, folderObj);

                        mQueryWorker.AddParameter("@ItemId", folderObj.ID.Value);
                        mQueryWorker.AddParameter("@AttachmentUrl", listObject.RootFolderUrl + "/" + "Attachments");
                        QueryAttachmentForFB(discoverReader.GetSingleItemAttachmentsQueryString(), itemEntities, discoverReader);
                    }
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "tp_MaxMajorwithMinorVersionCount")]
        private AveListObject QuerySingleListProperty(AveFolderCache folderCache)
        {
            using (AvePerformanceScope pc3 = new AvePerformanceScope("AveDiscoverQuery.QuerySingleListProperty"))
            {
                AveListObject listObject = null;
                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveQueryString13.Sp13ListWithRootFolderById))
                    {
                        if (sr.Read())
                        {
                            try
                            {
                                listObject = new AveListObject
                                {
                                    ListId = folderCache.ListId,
                                    Name = (string)sr["tp_Title"],
                                    Title = (string)sr["tp_Title"],
                                    RootFolderId = (Guid)sr["tp_RootFolder"],
                                    Type = (int)sr["tp_BaseType"],
                                    Flag = (long)sr["tp_Flags"],
                                    ServerTemplate = (int?)sr["tp_ServerTemplate"],
                                    Hidden = ((long)sr["tp_Flags"] & ((long)0x100L)) != 0L,
                                    RootFolderUrl = (string)sr["DirName"] + "/" + (string)sr["LeafName"]
                                };
                                if (!Convert.IsDBNull(sr["tp_MaxMajorwithMinorVersionCount"]))
                                {
                                    listObject.MaxMajorwithMinorVersionCount = (int)sr["tp_MaxMajorwithMinorVersionCount"];
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, "Error occur while access data from InitDiscoverFolder.GetListInfo. ErrorMessage:{0}", e);
                            }
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                return listObject;
            }
        }

        /// <summary>
        /// 初始化DiscoverList
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="listCache"></param>
        /// <param name="listObj"></param>
        [QueryReview("2012/05/21", "Oliver Luo")]
        public void InitDiscoverList(AveListCache listCache, AveListObject listObj)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.InitDiscoverList"))
            {
                mQueryWorker.AddParameter("@SiteId", listCache.SiteId);
                try
                {
                    Guid listId = (Guid)mQueryWorker.ExecuteScalar(ReplaceDirNameAndLeafName(listObj.RootFolderUrl, AveDiscoverQueryString.ListIdByItem));
                    mQueryWorker.AddParameter("@ListId", listId);
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.ListByIdSP2013))
                    {
                        if (sr.Read())
                        {
                            try
                            {
                                //listCache.ParentWeb.WebID = (Guid)sr["tp_WebId"];
                                //listCache.ListID = listObj.ListId = listId;                            
                                listObj.ListId = listId;
                                listObj.Title = listObj.Name = (string)sr["tp_Title"];
                                listObj.RootFolderId = (Guid)sr["tp_RootFolder"];
                                listObj.Type = (int)sr["tp_BaseType"];
                                listObj.Flag = (long)sr["tp_Flags"];
                                listObj.RootFolderUrl = listObj.RootFolderUrl.Trim('/');
                                listObj.ServerTemplate = (int?)sr["tp_ServerTemplate"];
                                listObj.Hidden = ((long)sr["tp_Flags"] & ((long)0x100L)) != 0L;
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, "Error occur while access data from InitDiscoverList.SiteId:{0}. RootFolderUrl:{1}.  ErrorMessage:{2}", listCache.SiteId, listObj.RootFolderUrl, e);
                            }
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }
        }

        /// <summary>
        /// 初始化DiscoverWeb
        ///效率考虑，有API实现
        /// </summary>
        /// <param name="webCache"></param>
        /// <param name="webObj"></param>
        [QueryReview("2012/05/21", "Oliver Luo")]
        public void InitDiscoverWeb(AveWebCache webCache, AveWebObject webObj)
        {
            mQuerySessionSchema.InitDiscoverWeb(webCache, webObj);
        }

        /// <summary>
        /// 查询特定parentId下的所有Items and Versions（包括Attachments)
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="commText"></param>
        /// <param name="parentFolderObject"></param>
        /// <param name="attachmentItems"></param>
        /// <param name="listObject"></param>
        /// <param name="discoverReader"></param>
        /// <param name="getStubItem"></param>
        [QueryReview("2012/12/11", "Austin Han", false, "在调用方法中Review")]
        public void QueryDocsForFB(string commText, AveItemObject parentFolderObject, Dictionary<int, AveItemObject> attachmentItems, AveListObject listObject, AveDiscoverReader discoverReader)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("QueryService.Discover.QueryDocsForFB"))
            {
                AveItemObject previousItem = null;
                Guid lastItemId = Guid.Empty;
                int subId = 0;
                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(commText))
                    {
                        while (sr.Read())
                        {
                            try
                            {
                                string leafName = (string)sr["LeafName"];
                                int? docLibRowId = sr["DoclibRowId"] is DBNull ? null : (int?)sr["DoclibRowId"];
                                Guid docId = (Guid)sr["Id"];
                                byte type = (byte)sr["Type"];
                                var deleteTransactionId = sr.GetVaule<byte[]>("DeleteTransactionId");
                                if (!docLibRowId.HasValue && string.Compare(leafName, "Attachments", StringComparison.OrdinalIgnoreCase) == 0
                                    || listObject == null && discoverReader.IsUnusedFolder(leafName, true))
                                {
                                    continue;
                                }
                                if (!docId.Equals(lastItemId) || previousItem == null)
                                {
                                    AveItemObject queryData = new AveItemObject();
                                    queryData.DirName = parentFolderObject.FullUrl;

                                    //Folder
                                    if (type == 1)
                                    {
                                        parentFolderObject.SubFolderObjs.Add(queryData);
                                        discoverReader.ReadItemContent(queryData, sr);
                                        queryData.ObjType = ItemType.Folder;
                                    }
                                    else
                                    {
                                        discoverReader.ReadItemContent(queryData, sr);
                                        if (listObject != null && listObject.Type != DocList && docLibRowId.HasValue)
                                        {
                                            queryData.ObjType = ItemType.Item;
                                        }
                                        else
                                        {
                                            queryData.ObjType = ItemType.Document;
                                        }
                                        parentFolderObject.SubItemObjs.Add(queryData);
                                    }
                                    //需要在之前reader 中赋值属性后再进行处理

                                    //对于root site collection,一些parentFolder的FullUrl为empty. DirName为empty,调用CombineUrl方法会抛异常
                                    queryData.FullUrl = queryData.DirName.Length > 0 ? AveUrlUtility.CombineUrl(queryData.DirName, queryData.LeafName) : queryData.LeafName;
                                    //此处先将Item 的对象加入集合，之后会将attachment 放入item 对象的属性上
                                    if (docLibRowId.HasValue)
                                    {
                                        subId = docLibRowId.Value;
                                        if (attachmentItems != null)
                                        {
                                            attachmentItems.Add(subId, queryData);
                                        }
                                    }
                                    queryData.DeleteTransactionId = deleteTransactionId;
                                    previousItem = queryData;
                                    lastItemId = docId;
                                }
                                //Item 本身也要在version 集合中存在,之前外围需要。此处可以商议是否去掉，暂时不影响效率，不做修改。
                                AveVersionObject version = new AveVersionObject();
                                discoverReader.ReadVersionContent(version, sr);
                                AddVersion(version, previousItem, sr, discoverReader);
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, "Error occurred while getting data from QueryDocsForFB. ErrorMessage:{0}", e);
                            }
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "docver")]
        public void SetVersionsStubInfo(List<AveVersionObject> versions, Guid siteId, Guid itemId, AveDiscoverReader discoverReader)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("QueryService.Discover.SetVersionsStubInfo"))
            {
                try
                {
                    string commText = discoverReader.GetAllItemAndVersionsStubInfoQueryString();
                    if (String.IsNullOrEmpty(commText))
                    {
                        return;
                    }
                    Dictionary<int, AveVersionObject> versionsKeyValues = versions.ToDictionary(key => key.Uiversion, value => value);
                    commText = commText.Replace("@WHEREAllDocs", DiscoverConditionString.StubInfoByIdForAllDocs).Replace("@WHEREAllDocVersions", DiscoverConditionString.StubInfoByIdForAllDocVersions);
                    mQueryWorker.AddParameter("@Id", itemId);
                    mQueryWorker.AddParameter("@SiteId", siteId);

                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(commText))
                    {
                        while (sr.Read())
                        {
                            try
                            {
                                AveVersionObject currentVersionObj;
                                if (versionsKeyValues.TryGetValue((int)sr["UIVersion"], out currentVersionObj))
                                {
                                    discoverReader.ReadVersionStubInfo(sr, currentVersionObj);
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, "Error occurred while getting data from SetVersionsStubInfo. ErrorMessage:{0}", e);
                            }
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }
        }

        /// <summary>
        /// For Extender,set attachments stubInfo
        /// </summary>
        /// <param name="attachments">同一个Item或folder上的attachment集合</param>
        /// <param name="siteId"></param>
        /// <param name="discoverReader"></param>
        public void SetAttachmentsStubInfo(List<AveItemObject> attachments, Guid siteId, AveDiscoverReader discoverReader)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("QueryService.Discover.SetAttachmentsStubInfo"))
            {
                try
                {
                    string commText = discoverReader.GetAllAttachmentsStubInfoQueryString();
                    //只有Extender模块commText不为空
                    if (String.IsNullOrEmpty(commText) || attachments.Count == 0)
                    {
                        return;
                    }
                    Dictionary<Guid, AveItemObject> attachmentsKeyValues = attachments.ToDictionary(key => key.DocID, value => value);
                    mQueryWorker.AddParameter("@SiteId", siteId);
                    //一个Item上所有attachment的DirName都相同，取第一个元素的DirName做为parameter
                    mQueryWorker.AddParameter("@DirName", attachments[0].DirName);
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(commText))
                    {
                        while (sr.Read())
                        {
                            try
                            {
                                AveItemObject currentAttachmentObj;
                                if (attachmentsKeyValues.TryGetValue((Guid)sr["Id"], out currentAttachmentObj))
                                {
                                    discoverReader.ReadAttachmentStubInfo(sr, currentAttachmentObj);
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, "Error occurred while getting data from SetAttachmentsStubInfo. ErrorMessage:{0}", e);
                            }
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }
        }

        #region For Replicator

        /// <summary>
        /// 获取Attachments
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listRootUrl"></param>
        /// <param name="itemObj"></param>
        /// <param name="discoverReader"></param>
        [QueryReview("2012/12/10", "Austin Han")]
        public void QueryItemAttachment(Guid siteId, string listRootUrl, AveItemObject itemObj, AveDiscoverReader discoverReader)
        {
            Dictionary<int, AveItemObject> attachItemObj = new Dictionary<int, AveItemObject>();
            if (itemObj.ID.HasValue)
            {
                int itemObjId = (int)itemObj.ID;
                attachItemObj.Add(itemObjId, itemObj);
                itemObj.AttachmentObjs.Clear();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ItemId", itemObj.ID.Value.ToString());
                mQueryWorker.AddParameter("@AttachmentUrl", listRootUrl + "/" + "Attachments");
                QueryAttachmentForFB(discoverReader.GetSingleItemAttachmentsQueryString(), attachItemObj, discoverReader);
            }
        }

        private AveItemObject GetItemInfoFromDocs(Guid SiteId, Guid parentId, Guid id, string dirName, string leafName, bool isListItem, AveDiscoverReader discoverReader)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("QueryService.Discover.GetItemInfoFromDocs"))
            {
                AveItemObject item = null;
                string commText = string.Empty;
                if (isListItem)
                {
                    commText = AddAllDocsDirName(discoverReader.GetAllItemsInAllDocQueryString()).Replace("@WHERE", DiscoverConditionString.ListItemExits);

                    mQueryWorker.AddParameter("@SiteId", SiteId);
                    mQueryWorker.AddParameter("@ParentId", parentId);
                    mQueryWorker.AddParameter("@Id", id);
                }
                else
                {
                    commText = AddAllDocsParentId(discoverReader.GetAllItemsInAllDocQueryString()).Replace("@WHERE", DiscoverConditionString.DocumentExits);

                    mQueryWorker.AddParameter("@SiteId", SiteId);
                    mQueryWorker.AddParameter("@LeafName", leafName);
                    mQueryWorker.AddParameter("@DirName", dirName);
                }
                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(commText))
                    {
                        while (sr.Read())
                        {
                            try
                            {
                                if (item == null)
                                {
                                    item = new AveItemObject();
                                    discoverReader.ReadItemContent(item, sr);
                                    if (isListItem)
                                    {
                                        item.DirName = (string)sr["DirName"];
                                        item.ParentID = parentId;
                                    }
                                    else
                                    {
                                        item.DirName = dirName;
                                        item.ParentID = (Guid)sr["ParentId"];
                                    }
                                    if ((byte)sr["Type"] == 1)
                                    {
                                        item.ObjType = ItemType.Folder;
                                    }
                                    else
                                    {
                                        if (isListItem)
                                        {
                                            item.ObjType = ItemType.Item;
                                        }
                                        else
                                        {
                                            item.ObjType = ItemType.Document;
                                        }
                                    }
                                    item.FullUrl = AveUrlUtility.CombineUrl(item.DirName, item.LeafName);
                                }
                                AveVersionObject version = new AveVersionObject();
                                discoverReader.ReadVersionContent(version, sr);
                                AddVersion(version, item, sr, discoverReader);
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, "Error occur while access data from GetItemExist.GetItemInfoFromDocs. ErrorMessage:{0}", e);
                            }
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                return item;
            }
        }

        /// <summary>
        /// 获取特定Item/Document下的所有version信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="SiteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="id"></param>
        /// <param name="dirName"></param>
        /// <param name="leafName"></param>
        /// <param name="isListItem"></param>
        /// <param name="discoverReader"></param>
        /// <param name="maxMajorwithMinorVersionCount"></param>
        /// <returns></returns>
        public AveItemObject GetItemExist(Guid SiteId, Guid webId, Guid listId, Guid parentId, Guid id, string listRootFolder, string dirName, string leafName, bool isListItem, AveDiscoverReader discoverReader, int? maxMajorwithMinorVersionCount)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("QueryService.Discover.GetItemExist"))
            {
                AveItemObject item = null;
                item = GetItemInfoFromDocs(SiteId, parentId, id, dirName, leafName, isListItem, discoverReader);
                if (item != null)
                {
                    AveListObject listObject = null;
                    //Special Library.
                    if (!isListItem && item.ID.HasValue && item.ID.Value > 0 && maxMajorwithMinorVersionCount.HasValue)
                    {
                        listObject = new AveListObject();
                        listObject.Type = 1;
                        listObject.MaxMajorwithMinorVersionCount = maxMajorwithMinorVersionCount;
                    }
                    mQueryWorker.AddParameter("@ParentId", item.ParentID);
                    mQueryWorker.AddParameter("@Id", item.DocID);
                    var condition = discoverReader.GetItemVersionsWithDocIdCondition();
                    QueryItemVersions(new Dictionary<Guid, AveItemObject> { { item.DocID, item } }, discoverReader, listObject, condition);

                    #region Query Attachments.
                    if (isListItem && !string.IsNullOrEmpty(listRootFolder))
                    {
                        mQueryWorker.AddParameter("@WebId", webId);
                        mQueryWorker.AddParameter("@ListId", listId);

                        try
                        {
                            Dictionary<int, AveItemObject> itemEntities = new Dictionary<int, AveItemObject>();
                            itemEntities.Add((int)item.ID, item);

                            mQueryWorker.AddParameter("@ItemId", item.ID);
                            mQueryWorker.AddParameter("@AttachmentUrl", (listRootFolder).Trim('/') + "/" + "Attachments");
                            QueryAttachmentForFB(discoverReader.GetSingleItemAttachmentsQueryString(), itemEntities, discoverReader);
                        }
                        catch (SqlException queryException)
                        {
                            throw new AveQueryException(queryException);
                        }
                        catch (AveQueryException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            throw new AveQueryException(e.Message, e);
                        }
                    }
                    #endregion
                }
                return item;
            }
        }

        /// <summary>
        /// 获取Item/Document的LastModifiedTime
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="dirName"></param>
        /// <param name="leafName"></param>
        /// <param name="docId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/09", "Oliver Luo")]
        public DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, string dirName, string leafName, ref Guid docId)
        {
            int docRowId = 0;
            DateTime lastModify = DateTime.MinValue;
            lastModify = GetItemLastModifiedTime(siteId, webId, listId, dirName, leafName, ref docId, ref docRowId);
            if (docRowId <= 0)
            {
                return lastModify;
            }
            else
            {
                return GetItemLastModifiedTime(siteId, listId, docRowId);
                //if (docId.Equals(Guid.Empty))
                //{
                //    return DateTime.MinValue;
                //}
                //else
                //{
                //    return GetItemLastModifiedTime(siteId, webId, listId, docId, true);
                //}
            }
        }

        /// <summary>
        /// 获取Item/Document的LastModifiedTime
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="dirName"></param>
        /// <param name="leafName"></param>
        /// <param name="docId"></param>
        /// <param name="docRowId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/09", "Oliver Luo")]
        public DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, string dirName, string leafName, ref Guid docId, ref int docRowId)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.GetItemLastModifiedTime"))
            {
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@DirName", dirName);
                mQueryWorker.AddParameter("@LeafName", leafName);

                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.ItemLastModifiedTimeWithDirName))
                    {
                        return GetAndCheckItemId(sr, ref docId, ref docRowId);
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }
        }

        /// <summary>
        /// 获取Item/Document的LastModifiedTime
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="rowId"></param>
        /// <returns></returns>
        public DateTime GetItemLastModifiedTime(Guid siteId, Guid listId, int rowId)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.GetItemLastModifiedTimeForListItem"))
            {
                DateTime result = DateTime.MinValue;
                try
                {
                    mQueryWorker.AddParameter("@SiteId", siteId);
                    mQueryWorker.AddParameter("@ListId", listId);
                    mQueryWorker.AddParameter("@Id", rowId);
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.ItemLastModifiedTimeByListIdAndDoclibRowId13))
                    {
                        if (sr.Read())
                        {
                            result = sr.GetDateTime(0);
                            return result;
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                return result;
            }
        }

        /// <summary>
        /// 获取Item/Document的LastModifiedTime
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/09", "Oliver Luo", false, "AveDiscoverQueryString.ItemLastModifiedTimeWithDoclibRowId中AllUserData表索引使用不全。")]
        public DateTime GetItemLastModifiedTime(Guid siteId, Guid itemId)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.GetItemLastModifiedTimeForListItemForDoc"))
            {
                DateTime result = DateTime.MinValue;
                try
                {
                    mQueryWorker.AddParameter("@SiteId", siteId);
                    mQueryWorker.AddParameter("@Id", itemId);
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.ItemLastModifiedTimeWithoutDoclibRowId13))
                    {
                        if (sr.Read())
                        {
                            result = sr.GetDateTime(0);
                            return result;
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                return result;
            }
        }

        /// <summary>
        /// 根据DoclibRowId查找该Item下的所有Versions
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="docLibRowId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/09", "Oliver Luo", true, "AveDiscoverQueryString.ItemVersions中AllUserData表索引使用不全，增加tp_IsCurrentVersion。")]
        public AveItemObject GetItemVersions(Guid siteId, Guid listId, int docLibRowId)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.GetItemVersions"))
            {
                AveItemObject item = new AveItemObject
                {
                    ID = docLibRowId
                };

                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ListId", listId);
                mQueryWorker.AddParameter("@docLibId", docLibRowId);

                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString13.ItemVersions))
                    {
                        while (sr.Read())
                        {
                            try
                            {
                                AveVersionObject version = new AveVersionObject
                                {
                                    Uiversion = sr.GetInt32(0),
                                    TimeLastModified = sr.GetDateTime(1),
                                    IsCurrentVersion = sr.GetBoolean(2),
                                    UserDataGuid = sr.GetGuid(3),
                                    ID = sr.GetInt32(4),
                                    UiVersionString = sr.GetString(5),
                                    Level = sr.GetByte(6),
                                    Size = sr.GetInt32(7),
                                    Tp_IsCurrentVersion = sr.GetBoolean(8),
                                };
                                item.VersionObjs.Add(version);
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, "Error occur while access data from GetItemVersions. ErrorMessage:{0}", e);
                            }
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                return item;
            }
        }

        /// <summary>

        /// 根据tp_Guid去查询Item的DocId
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="tp_Guid"></param>
        /// <returns></returns>
        [QueryReview("2012/05/09", "Oliver Luo")]
        public Guid GetDocIdByTp_Guid(Guid siteId, Guid parentId, Guid tp_Guid)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.GetDocIdByTp_Guid"))
            {
                mQueryWorker.AddParameter("@siteId", siteId);
                mQueryWorker.AddParameter("@parentId", parentId);
                mQueryWorker.AddParameter("@tp_Guid", tp_Guid);

                try
                {
                    object result;
                    string cmdtext = "SELECT tp_DocId from AllUserData WITH(NOLOCK) where tp_siteid=@siteId and tp_DeleteTransactionId=0x and (tp_IsCurrentVersion=0 OR tp_IsCurrentVersion=1) and tp_parentid=@parentId and tp_guid=@tp_Guid";

                    result = mQueryWorker.ExecuteScalar(cmdtext);
                    if (result != null)
                    {
                        return (Guid)result;
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                return Guid.Empty;
            }
        }

        /// <summary>
        /// 根据Leafname去数据库中查询是否有相同记录
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="dirName"></param>
        /// <param name="leafName"></param>
        /// <returns></returns>
        [QueryReview("2012/05/09", "Oliver Luo")]
        public bool IsHaveSameName(Guid siteId, Guid webId, Guid listId, string dirName, string leafName)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.QueryRootWeb"))
            {
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@dirName", dirName);
                mQueryWorker.AddParameter("@LeafName", leafName);

                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.IsHaveSameNameByLeafName))
                    {
                        if (sr.Read() && sr.GetInt32(0) > 0)
                        {
                            return true;
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                return false;
            }
        }

        /// <summary>
        /// 根据tp_Guid去查询数据库中是否有相同记录
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="tpGuid"></param>
        /// <param name="listId"></param>
        /// <param name="rowId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/09", "Oliver Luo")]
        public bool IsListItemHaveSameName(Guid siteId, Guid webId, Guid tpGuid, Guid listId, int rowId)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.IsListItemHaveSameName"))
            {
                mQueryWorker.AddParameter("@ListId", listId);
                mQueryWorker.AddParameter("@tp_Guid", tpGuid);
                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.IsHaveSameNameByTpGuid))
                    {
                        if (sr.Read() && sr.GetInt32(0) > 0)
                        {
                            return true;
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                return false;
            }
        }

        /// <summary>
        /// 查询Item上的WebParts
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="itemDocId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/09", "Oliver Luo", true, "AveDiscoverQueryString.ItemWebParts，AllWebParts没有使用索引，增加tp_IsCurrentVersion")]
        public List<AveWebPartObject> GetItemWebParts(Guid siteId, Guid webId, Guid listId, Guid itemDocId)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.GetItemWebParts"))
            {
                List<AveWebPartObject> webParts = new List<AveWebPartObject>();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@DocId", itemDocId);

                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.ItemWebParts))
                    {
                        while (sr.Read())
                        {
                            try
                            {
                                AveWebPartObject webPart = new AveWebPartObject();
                                Guid id = sr.GetGuid(0);
                                webPart.Id = id;
                                if (!sr.IsDBNull(1))
                                {
                                    webPart.Flags = (int)sr.GetValue(1);
                                }
                                if (!sr.IsDBNull(2))
                                {
                                    webPart.DisplayName = (string)sr.GetValue(2);
                                }
                                if (!sr.IsDBNull(3))
                                {
                                    webPart.PartOrder = (int)sr.GetValue(3);
                                }
                                if (!sr.IsDBNull(4))
                                {
                                    webPart.ZoneId = (string)sr.GetValue(4);
                                }
                                webPart.IsIncluded = (bool)sr.GetValue(5);
                                if (!sr.IsDBNull(6))
                                {
                                    webPart.View = (byte[])sr.GetValue(6);
                                }
                                if (!sr.IsDBNull(7))
                                {
                                    webPart.AllUsersProperties = (byte[])sr.GetValue(7);
                                }
                                if (!sr.IsDBNull(8))
                                {
                                    webPart.PerUserProperties = (byte[])sr.GetValue(8);
                                }
                                webParts.Add(webPart);
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, "Error occur while access data from GetItemWebParts. ErrorMessage:{0}", e);
                            }
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                return webParts;
            }
        }

        /// <summary>
        /// 获取Item的size
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="docId"></param>
        /// <param name="createdBy"></param>
        /// <param name="modifiedBy"></param>
        /// <returns></returns>
        [QueryReview("2012/05/09", "Oliver Luo")]
        public long GetItemSizeAndUserInfo(Guid siteId, Guid webId, Guid listId, Guid docId, int level, ref string createdBy, ref string modifiedBy)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.QueryRootWeb"))
            {
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@DocId", docId);
                mQueryWorker.AddParameter(@"Level", level);
                long size = 0;
                int createdUserId = 0;
                int modifiedUserId = 0;
                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.ItemSizeAndParnetId))
                    {
                        if (sr.Read())
                        {
                            object sizeObj = sr["Size"];//size会出现空的情况，需要判断
                            if (sizeObj != null && sizeObj != DBNull.Value)
                            {
                                size = long.Parse(sizeObj.ToString());
                            }
                            mQueryWorker.AddParameter("@ParentId", (Guid)sr["ParentId"]);
                        }
                    }
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.AuthorAndEditor))
                    {
                        if (sr.Read())
                        {
                            createdUserId = sr["tp_Author"] is DBNull ? 0 : (int)sr["tp_Author"];
                            modifiedUserId = sr["tp_Editor"] is DBNull ? 0 : (int)sr["tp_Editor"];
                        }
                    }
                    if (createdUserId != 0)
                    {
                        mQueryWorker.AddParameter("@UserId", createdUserId);
                        createdBy = (string)mQueryWorker.ExecuteScalar(AveDiscoverQueryString.UserTitle);
                    }
                    else
                    {
                        createdBy = string.Empty;
                    }
                    if (modifiedUserId != 0)
                    {
                        mQueryWorker.AddParameter("@UserId", modifiedUserId);
                        modifiedBy = (string)mQueryWorker.ExecuteScalar(AveDiscoverQueryString.UserTitle);
                    }
                    else
                    {
                        modifiedBy = string.Empty;
                    }
                    return size;
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }
        }

        /// <summary>
        /// 根据parentId获取Document的tp_Guid-tp_DocId,DocId-type的Mapping
        /// 效率考虑，有API实现
        /// </summary> 
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="itemsMapping"></param>
        /// <param name="foldersMapping"></param>
        /// <returns></returns>
        [QueryReview("2012/05/09", "Oliver Luo")]
        public Dictionary<Guid, Guid> GetTPGUIDAndDocIdMapping(Guid siteId, Guid parentId)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.GetTPGUIDAndDocIdMapping"))
            {
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ParentId", parentId);
                try
                {
                    Dictionary<Guid, Guid> idAndGUIDMappings = new Dictionary<Guid, Guid>();
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.ItemIdAndTPGUID))
                    {
                        while (sr.Read())
                        {
                            idAndGUIDMappings[sr.GetGuid(1)] = sr.GetGuid(0);
                        }
                    }
                    //Dictionary<Guid, byte> idAndTypeMappings = new Dictionary<Guid, byte>();
                    //using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.ItemIdAndType))
                    //{
                    //    while (sr.Read())
                    //    {
                    //        idAndTypeMappings[sr.GetGuid(0)] = sr.GetByte(1);
                    //    }
                    //}

                    return idAndGUIDMappings;
                    //foldersMapping = new Dictionary<Guid, Guid>();

                    //StringBuilder builder = new StringBuilder();

                    //foreach (KeyValuePair<Guid, Guid> keyValue in idAndGUIDMappings)
                    //{
                    //    bool isFolder = false;
                    //    if (idAndTypeMappings.ContainsKey(keyValue.Key))
                    //    {
                    //        if (idAndTypeMappings[keyValue.Key] == 1)
                    //        {
                    //            isFolder = true;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        builder.AppendFormat("Cannot verify the item type of id:{0} tp_GUID:{1}\r\n", keyValue.Key, keyValue.Value);
                    //    }

                    //    if (isFolder)
                    //    {
                    //        if (foldersMapping.ContainsKey(keyValue.Value))
                    //        {
                    //            builder.AppendFormat("Same TP_GUID for folder:{0} Id1:{1}, Id2:{2}\r\n", keyValue.Value, foldersMapping[keyValue.Value], keyValue.Key);
                    //        }
                    //        else
                    //        {
                    //            foldersMapping[keyValue.Value] = keyValue.Key;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        if (itemsMapping.ContainsKey(keyValue.Value))
                    //        {
                    //            builder.AppendFormat("Same TP_GUID for Item:{0} Id1:{1}, Id2:{2}\r\n", keyValue.Value, itemsMapping[keyValue.Value], keyValue.Key);
                    //        }
                    //        else
                    //        {
                    //            itemsMapping[keyValue.Value] = keyValue.Key;
                    //        }
                    //    }
                    //}

                    //if (builder.Length > 0)
                    //{
                    //    logger.Warn("Query Item Id And TPGUID Details:{0}", builder.ToString());
                    //}
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }
        }

        #endregion

        #region FB

        /// <summary>
        /// 获取Site下的所有web信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/10", "Austin Han")]
        public Dictionary<Guid, AveWebObject> QuerySiteWebForFB(Guid siteId)
        {
            return mQuerySessionSchema.QuerySiteWebForFB(siteId);
        }

        /// <summary>
        /// 获取Site的RootWeb信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/09", "Oliver Luo")]
        public AveWebObject QueryRootWeb(Guid siteId)
        {
            return mQuerySessionSchema.QueryRootWeb(siteId);
        }

        /// <summary>
        /// 获取sub webs
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentWebId"></param>
        /// <param name="includeRecycleBin">FOR SO</param>
        /// <returns></returns>
        [QueryReview("2012/05/09", "Oliver Luo")]
        public Dictionary<Guid, AveWebObject> GetSubWebs(Guid siteId, Guid parentWebId, bool includeRecycleBin)
        {
            return mQuerySessionSchema.GetSubWebs(siteId, parentWebId, includeRecycleBin);
        }

        /// <summary>
        /// 获取Web下的所有Lists信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/11", "Austin Han", true, "Add SiteId to improve performance")]
        public Dictionary<Guid, AveListObject> QueryWebListForFB(Guid siteId, Guid webId, bool includeRecycleBin)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AveDiscoverQuery.QueryWebListForFB"))
            {
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@WebId", webId);
                Dictionary<Guid, AveListObject> listObjs = new Dictionary<Guid, AveListObject>();
                try
                {
                    var command = includeRecycleBin ? AveDiscoverQueryString13.ListsWithRecycleBin : AveDiscoverQueryString13.Lists;
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(command))
                    {
                        while (sr.Read())
                        {
                            Guid listId = sr.GetGuid(0);
                            try
                            {
                                if (!listObjs.ContainsKey(listId))
                                {
                                    string name = sr.GetString(1);
                                    Guid rootFolderId = sr.GetGuid(2);
                                    int nodeType = sr.GetInt32(3);
                                    long flag = sr.GetInt64(4);
                                    string rootFolderUrl = sr.GetString(5).Trim('/');
                                    int serverTemplate = sr.GetInt32(6);
                                    var deleteTransactionId = (byte[])sr["tp_DeleteTransactionId"];
                                    var bytes = (byte[])sr["tp_Fields"];

                                    AveListObject listObj = new AveListObject
                                    {
                                        ListId = listId,
                                        RootFolderId = rootFolderId,
                                        Name = name,
                                        Title = name,
                                        Type = nodeType,
                                        RootFolderUrl = rootFolderUrl,
                                        Flag = flag,
                                        ServerTemplate = serverTemplate,
                                        Hidden = (flag & ((long)0x100L)) != 0L,
                                        DeleteTransactionId = deleteTransactionId
                                    };
                                    var fieldsSchema = string.Empty;
                                    if (bytes != null && bytes.Length > 0)
                                    {
                                        fieldsSchema = AveCompressedUtility.GetTCompressedString(bytes);
                                    }
                                    if (fieldsSchema != null && fieldsSchema.Contains("<"))
                                    {
                                        fieldsSchema = fieldsSchema.Substring(fieldsSchema.IndexOf("<", StringComparison.OrdinalIgnoreCase));
                                    }
                                    listObj.Fields = "<Fields>" + fieldsSchema + "</Fields>";
                                    listObjs.Add(listId, listObj);
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, "Error occur while access data from query QueryWebListForFB.SiteId:{0}. WebId:{1}. ListId:{2} ErrorMessage:{3}", siteId, webId, listId, e);
                            }
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                return listObjs;
            }
        }

        /// <summary>
        /// 获取List下的所有Views信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/11", "Austin Han")]
        public Dictionary<Guid, AveViewObject> QueryListViewForFB(Guid siteId, Guid webId, Guid listId)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.QueryListViewForFB"))
            {
                Dictionary<Guid, AveViewObject> views = new Dictionary<Guid, AveViewObject>();

                mQueryWorker.AddParameter("@siteId", siteId);
                mQueryWorker.AddParameter("@webId", webId);
                mQueryWorker.AddParameter("@listId", listId);

                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.ListViews))
                    {
                        while (sr.Read())
                        {
                            try
                            {
                                AveViewObject view = new AveViewObject();
                                DiscoverUtility.FillWebPartDicFromAllWebParts(view, sr);
                                views.Add(sr.GetGuid(ViewColumn.Id), view);
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, "Error occur while access data from method QueryListViewForFB.SiteId:{0}. WebId:{1}. ListId:{2} ErrorMessage:{3}", siteId, webId, listId, e);
                            }
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                return views;
            }
        }

        /// <summary>
        /// Wrapper内部没有调用，外围模块也没有调用。
        /// 获取某Folder下的子Folders信息(包括folder下的Item和Version信息)
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="folderObject"></param>
        /// <param name="listObject"></param>
        /// <param name="discoverReader"></param>
        [QueryReview("2012/12/10", "Austin Han")]
        public void QuerySubFoldersForFB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, AveDiscoverReader discoverReader)
        {
            if (folderObject.DocID == Guid.Empty)
            {
                logger.Log(AveLogLevel.WARN, "parentId should not be null.ParentFolder Url:{0}", folderObject.FullUrl);
                return;
            }
            QueryListItemForFB(folderCache, folderObject, listObject, discoverReader, false, true);

        }

        /// <summary>
        /// 获取一个Item下的所有Attachements
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="item"></param>
        /// <param name="listObject"></param>
        /// <param name="discoverReader"></param>
        [QueryReview("2012/12/10", "Austin Han")]
        public void QueryAttachment(AveFolderCache folderCache, AveItemObject item, AveListObject listObject, AveDiscoverReader discoverReader)
        {
            if (listObject != null && item.ID.HasValue)
            {
                bool enableAttachment = listObject.Flag != null && DiscoverUtility.IsEnableAttachment((long)listObject.Flag);
                if (enableAttachment)
                {
                    string attachmentUrl = string.Empty;
                    if (!string.IsNullOrEmpty(listObject.RootFolderUrl))
                    {
                        attachmentUrl = listObject.RootFolderUrl + '/' + "Attachments";
                    }
                    if (!string.IsNullOrEmpty(attachmentUrl))
                    {
                        mQueryWorker.AddParameter("@SiteId", folderCache.SiteId);
                        mQueryWorker.AddParameter("@ItemId", item.ID.Value.ToString());
                        mQueryWorker.AddParameter("@AttachmentUrl", attachmentUrl);
                        try
                        {
                            using (var sr = mQueryWorker.ExecuteReader(discoverReader.GetSingleItemAttachmentsQueryString()))
                            {
                                while (sr.Read())
                                {
                                    AveItemObject attachment = new AveItemObject();
                                    discoverReader.ReadAttachmentContent(attachment, sr);
                                    item.AttachmentObjs.Add(attachment);
                                }
                            }
                        }
                        catch (SqlException queryException)
                        {
                            throw new AveQueryException(queryException);
                        }
                        catch (AveQueryException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            throw new AveQueryException(e.Message, e);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取某Folder下的Items和Versions信息，包括Attachement.
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="folderObject"></param>
        /// <param name="listObject"></param>
        /// <param name="discoverReader"></param>
        /// <param name="includeRecycleBin"></param>
        [QueryReview("2012/12/10", "Austin Han", false, "在GetListItem中Review")]
        public void QueryListItemForFB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, AveDiscoverReader discoverReader, bool includeRecycleBin, bool includeVersion)
        {
            string attachmentUrl = string.Empty;
            if (listObject != null)
            {
                if (!string.IsNullOrEmpty(listObject.RootFolderUrl))
                {
                    attachmentUrl = listObject.RootFolderUrl + '/' + "Attachments";
                }
                else
                {
                    logger.Log(AveLogLevel.WARN, "Current List should have RootFolderUrl. ListId:{0}", folderCache.ListId);
                }
            }

            mQueryWorker.AddParameter("@SiteId", folderCache.SiteId);
            mQueryWorker.AddParameter("@ListId", folderCache.ListId);

            GetListItem(folderObject, attachmentUrl, listObject, discoverReader, includeRecycleBin,includeVersion);
        }

        /// <summary>
        /// 查询某folder下的stub Item信息
        /// 无API实现
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="folderObject"></param>
        /// <param name="listObject"></param>
        /// <param name="discoverReader"></param>
        /// <param name="includeRecycleBin"></param>
        [QueryReview("2012/05/10", "Oliver Luo", false, "在GetStubItem中Review")]
        public void QueryStubItemForFB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, AveDiscoverReader discoverReader, bool includeRecycleBin)
        {
            string attachmentUrl = string.Empty;

            if (listObject != null)
            {
                if (!string.IsNullOrEmpty(listObject.RootFolderUrl))
                {
                    attachmentUrl = listObject.RootFolderUrl + '/' + "Attachments";
                }
                else
                {
                    logger.Log(AveLogLevel.WARN, "Current List should have RootFolderUrl.Current folder DocId:{0}. Url:{1}", folderObject.DocID, folderObject.FullUrl);
                }
            }
            mQueryWorker.AddParameter("@SiteId", folderCache.SiteId);
            mQueryWorker.AddParameter("@ListId", folderCache.ListId);

            GetStubItem(folderObject.DocID, folderObject, attachmentUrl, listObject, discoverReader, includeRecycleBin);
        }

        /// <summary>
        /// 获取ParentId下所有Stub Item数量(包括AllDocs表中和AllDocVersions表中)
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="folderObject"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "HistVersion is the parameter of the sql statement. ")]
        [QueryReview("2012/05/10", "Oliver Luo")]
        public int GetAllStubCount(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, bool includeRecycleBin = false)
        {
            mQueryWorker.AddParameter("@SiteId", folderCache.SiteId);
            mQueryWorker.AddParameter("@WebId", folderCache.WebId);
            mQueryWorker.AddParameter("@ListId", folderCache.ListId);
            mQueryWorker.AddParameter("@ParentId", folderObject.DocID);

            int stubFileNum = 0;
            int stubAttachmentNum = 0;
            try
            {
                var command = includeRecycleBin ? AveQueryString13.Sp13StubFilesInFolderCountWithRecycle
                    : AveQueryString13.Sp13StubFilesInFolderCount;
                stubFileNum = (int)mQueryWorker.ExecuteScalar(command);
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }

            if (listObject != null)
            {
                using (AvePerformanceScope pc2 = new AvePerformanceScope("QueryService.Discover.GetAllStubCount.stubAttachmentNum"))
                {
                    bool enableAttachment = listObject.Flag != null && DiscoverUtility.IsEnableAttachment((long)listObject.Flag);
                    if (enableAttachment)
                    {
                        var attachmentDir = listObject.RootFolderUrl + '/' + "Attachments/";
                        mQueryWorker.AddParameter("@AttachmentDir", attachmentDir);
                        stubAttachmentNum = (int)mQueryWorker.ExecuteScalar(AveQueryString13.Sp13ItemStubAttachmentsInFolder);
                    }
                }
            }
            return stubFileNum + stubAttachmentNum;
        }

        /// <summary>
        /// 查询Web下的所有ContentTypes信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/11", "Austin Han", false, "Add an overload method to improve the performance.")]
        public Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForFB(Guid siteId, Guid webId)
        {
            return mQuerySessionSchema.QueryWebContentTypeForFB(siteId, webId);
        }

        /// <summary>
        /// 查询Web下的所有ContentTypes信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/11", "Austin Han", true, "Use serverrelativeurl directly to do the query.")]
        public Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForFB(Guid siteId, string serverRelativeUrl)
        {
            return mQuerySessionSchema.QueryWebContentTypeForFB(siteId, serverRelativeUrl);
        }

        #endregion

        #region Item Level

        /// <summary>
        /// 从EventCache表中查询Item的Security改变
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="itemId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [QueryReview("2012/12/10", "Austin Han")]
        public Dictionary<int, List<AveSecurityObject>> QueryItemSecurityForIB(Guid siteId, Guid webId, Guid listId, int itemId, DateTime startTime, DateTime endTime)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.QueryItemSecurityForIB"))
            {
                Dictionary<int, List<AveSecurityObject>> securityChanges = new Dictionary<int, List<AveSecurityObject>>();
                mQueryWorker.AddParameter("@endTime", endTime);
                mQueryWorker.AddParameter("@startTime", startTime);
                mQueryWorker.AddParameter("@webId", webId);
                mQueryWorker.AddParameter("@listId", listId);
                mQueryWorker.AddParameter("@itemId", itemId);
                mQueryWorker.AddParameter("@SiteId", siteId);
                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString13.ItemSecurityChanged))
                    {
                        while (sr.Read())
                        {
                            var nativeChangeType = (NativeChangeType)sr.GetValue(0);
                            SecurityType securityType = DiscoverUtility.GetSecurityObjectType(nativeChangeType);
                            ChangeType changeType = DiscoverUtility.GetSecurityChangeType(nativeChangeType);
                            switch (securityType)
                            {
                                case SecurityType.Assignment:
                                    try
                                    {
                                        AssignmentSecurityChange(changeType, sr, securityChanges);
                                    }
                                    catch (SqlException ex)
                                    {
                                        logger.Log(AveLogLevel.WARN, "Error occur while access data from QueryItemSecurityForIB.SecurityType.Assignment.EventTime:{0}.  ErrorMessage:{1}", sr.GetDateTime(5), new AveQueryException(string.Format("Exception Error Code----{0}", ex.Number), ex));
                                    }
                                    catch (AveQueryException queryException)
                                    {
                                        logger.Log(AveLogLevel.WARN, "Error occur while access data from QueryItemSecurityForIB.SecurityType.Assignment.EventTime:{0}.  ErrorMessage:{1}", sr.GetDateTime(5), queryException);
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Log(AveLogLevel.WARN, "Error occur while access data from QueryItemSecurityForIB.SecurityType.Assignment.EventTime:{0}.  ErrorMessage:{1}", sr.GetDateTime(5), new AveQueryException("", e));
                                    }
                                    break;
                                case SecurityType.Scope: //break inherate
                                    try
                                    {
                                        ScopeSecurityChange(changeType, sr, securityChanges);
                                    }
                                    catch (SqlException ex)
                                    {
                                        logger.Log(AveLogLevel.WARN, "Error occur while access data from QueryItemSecurityForIB.SecurityType.Scope.EventTime:{0}.  ErrorMessage:{1}", sr.GetDateTime(5), new AveQueryException(string.Format("Exception Error Code----{0}", ex.Number), ex));
                                    }
                                    catch (AveQueryException queryException)
                                    {
                                        logger.Log(AveLogLevel.WARN, "Error occur while access data from QueryItemSecurityForIB.SecurityType.Scope.EventTime:{0}.  ErrorMessage:{1}", sr.GetDateTime(5), queryException);
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Log(AveLogLevel.WARN, "Error occur while access data from QueryItemSecurityForIB.SecurityType.Scope.EventTime:{0}.  ErrorMessage:{1}", sr.GetDateTime(5), new AveQueryException("", e));
                                    }
                                    break;
                                case SecurityType.None:
                                default:
                                    break;
                            }
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                return securityChanges;
            }
        }

        #endregion

        #region List Level

        /// <summary>
        /// 获取List下的RootFolder信息
        /// 效率考虑，有API实现 
        /// </summary>
        /// <param name="listCache"></param>
        /// <param name="itemColumns"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "list or rootFolder properties.")]
        [QueryReview("2012/12/11", "Austin Han", true, "Add SiteId to improve the performance.")]
        public void QueryListRootFolder(AveListCache listCache, AveDiscoverReader discoverReader, AveListObject listObject, AveItemObject rootFolderObject)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.QueryRootWeb"))
            {
                mQueryWorker.AddParameter("@SiteId", listCache.SiteId);
                mQueryWorker.AddParameter("@WebId", listCache.WebId);
                mQueryWorker.AddParameter("@ListId", listCache.ListId);
                try
                {
                    using (var sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString13.ListRootFolder.Replace("@Column", discoverReader.GetItemColumns())))
                    {
                        if (sr.Read())
                        {
                            try
                            {
                                discoverReader.ReadItemContent(rootFolderObject, sr);
                                rootFolderObject.ObjType = ItemType.Folder;
                                rootFolderObject.DirName = (string)sr["DirName"];
                                if (!Convert.IsDBNull(sr["tp_MaxMajorwithMinorVersionCount"]))
                                {
                                    listObject.MaxMajorwithMinorVersionCount = (int)sr["tp_MaxMajorwithMinorVersionCount"];
                                }
                                rootFolderObject.FullUrl = string.Format("{0}/{1}", rootFolderObject.DirName, rootFolderObject.LeafName).Trim('/');
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, "An exception occurred while access data from QueryListRootFolder. Error Message: {0}", e);
                            }
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }
        }

        /// <summary>
        /// 从EventCache表中获取List下Alert的改变
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [QueryReview("2012/12/10", "Austin Han")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public Dictionary<Guid, AveAlertObject> QueryListAlertForIB(Guid siteId, Guid webId, Guid listId, DateTime startTime, DateTime endTime)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.QueryRootWeb"))
            {
                Dictionary<Guid, AveAlertObject> changeAlerts = new Dictionary<Guid, AveAlertObject>();

                mQueryWorker.AddParameter("@endTime", endTime);
                mQueryWorker.AddParameter("@startTime", startTime);
                mQueryWorker.AddParameter("@siteId", siteId);
                mQueryWorker.AddParameter("@webId", webId);
                mQueryWorker.AddParameter("@listId", listId);
                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.ListAlertChanged))
                    {
                        while (sr.Read())
                        {
                            try
                            {
                                var nativeChangeType = (NativeChangeType)sr.GetValue(0);
                                ChangeType changeType = DiscoverUtility.GetChangeType(nativeChangeType);
                                Guid alertId = sr.GetGuid(2);

                                //AveAlertObject alert = null;
                                AveAlertObject alert = null;
                                if (changeAlerts.ContainsKey(alertId))
                                {
                                    alert = changeAlerts[alertId];
                                    if (alert.ChangeType == ChangeType.Add)
                                    {
                                        if (changeType == ChangeType.Delete)
                                        {
                                            changeAlerts.Remove(alertId);
                                        }
                                    }
                                }
                                else
                                {
                                    if (sr.IsDBNull(6) && sr.IsDBNull(7) ||
                                        sr.GetString(4).ToLower(CultureInfo.InvariantCulture).Contains("filterpath") ||
                                        sr.GetString(5).ToLower(CultureInfo.InvariantCulture).Contains("filterpath"))
                                    {
                                        //this alert is delete we can't know the alert belong to this list or folder
                                        //or it is folder alert
                                        continue;
                                    }
                                    alert = new AveAlertObject
                                    {
                                        Id = alertId,
                                        ChangeType = changeType
                                    };
                                    changeAlerts.Add(alertId, alert);
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, "Exception occur while access data from method QueryListAlertForIB.EventTime:{0}.  ErrorMessage:{1}", sr.GetDateTime(3), e);
                            }
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                return changeAlerts;
            }
        }

        /// <summary>
        /// 获取List下的View信息的改变
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [QueryReview("2012/12/11", "Austin Han")]
        public Dictionary<Guid, AveViewObject> QueryListViewForIB(Guid siteId, Guid webId, Guid listId, DateTime startTime, DateTime endTime)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.QueryListViewForIB"))
            {
                Dictionary<Guid, AveViewObject> changeViews = new Dictionary<Guid, AveViewObject>();

                mQueryWorker.AddParameter("@endTime", endTime);
                mQueryWorker.AddParameter("@startTime", startTime);
                mQueryWorker.AddParameter("@siteId", siteId);
                mQueryWorker.AddParameter("@webId", webId);
                mQueryWorker.AddParameter("@ListId", listId);

                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.ListViewChanged))
                    {
                        while (sr.Read())
                        {
                            try
                            {
                                ChangeType changeType = DiscoverUtility.GetChangeType((NativeChangeType)sr.GetValue(6));
                                Guid viewId = (Guid)sr.GetValue(7);
                                AveViewObject viewChange = null;
                                if (!changeViews.ContainsKey(viewId))
                                {
                                    viewChange = new AveViewObject();
                                    if (!sr.IsDBNull(ViewColumn.Id)) //tp_ID is not null
                                    {
                                        DiscoverUtility.FillWebPartDicFromAllWebParts(viewChange, sr);
                                    }
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
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, "Error occur while access data from method QueryListViewForIB. EventTime:{0}.  ErrorMessage:{1}", sr.GetDateTime(29), e);
                            }
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                return changeViews;
            }
        }

        /// <summary>
        /// 获取List下的Security信息的改变
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [QueryReview("2012/12/11", "Austin Han")]
        public Dictionary<int, List<AveSecurityObject>> QueryListSecurityForIB(Guid siteId, Guid webId, Guid listId, DateTime startTime, DateTime endTime)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.QueryListSecurityForIB"))
            {
                //add siteId and scopeId
                Dictionary<int, List<AveSecurityObject>> securityChanges = new Dictionary<int, List<AveSecurityObject>>();

                mQueryWorker.AddParameter("@endTime", endTime);
                mQueryWorker.AddParameter("@startTime", startTime);
                mQueryWorker.AddParameter("@siteId", siteId);
                mQueryWorker.AddParameter("@webId", webId);
                mQueryWorker.AddParameter("@ListId", listId);
                //add parameter
                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.ListSecurityChanged))
                    {
                        while (sr.Read())
                        {
                            var nativeChangeType = (NativeChangeType)sr.GetValue(0);
                            SecurityType securityType = DiscoverUtility.GetSecurityObjectType(nativeChangeType);
                            ChangeType changeType = DiscoverUtility.GetSecurityChangeType(nativeChangeType);

                            switch (securityType)
                            {
                                case SecurityType.Assignment:
                                    try
                                    {
                                        AssignmentSecurityChange(changeType, sr, securityChanges);
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Log(AveLogLevel.WARN, "Error occur while access data from QueryListSecurityForIB.SecurityType.Assignment.EventTime:{0}.  ErrorMessage:{1}", sr.GetDateTime(5), e);
                                    }
                                    break;
                                case SecurityType.Scope: //break inherate
                                    try
                                    {
                                        ScopeSecurityChange(changeType, sr, securityChanges);
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Log(AveLogLevel.WARN, "Error occur while access data from QueryListSecurityForIB.SecurityType.Scope. EventTime:{0}.  ErrorMessage:{1}", sr.GetDateTime(5), e);
                                    }
                                    break;
                                case SecurityType.None:
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                return securityChanges;
            }
        }

        /// <summary>
        /// 获取List下ContentType信息的改变
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [QueryReview("2012/12/10", "Austin Han")]
        public Dictionary<byte[], AveContentTypeObject> QueryListContentTypeForIB(Guid siteId, Guid webId, Guid listId, DateTime startTime, DateTime endTime)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.QueryListContentTypeForIB"))
            {
                Dictionary<byte[], AveContentTypeObject> contentTypeChanges = new Dictionary<byte[], AveContentTypeObject>();
                //can't get a content modify from list  just can get add and delete
                //we create a culumn to a list it belongs to modify view,list 级别没有column的概念
                mQueryWorker.AddParameter("@endTime", endTime);
                mQueryWorker.AddParameter("@startTime", startTime);
                mQueryWorker.AddParameter("@siteId", siteId);
                mQueryWorker.AddParameter("@webId", webId);
                mQueryWorker.AddParameter("@ListId", listId);
                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.ListContentTypeChanged))
                    {
                        while (sr.Read())
                        {
                            try
                            {
                                var nativeChangeType = (NativeChangeType)sr.GetValue(0);
                                ChangeType changeType = DiscoverUtility.GetChangeType(nativeChangeType);
                                if (nativeChangeType == NativeChangeType.ListContenTypeAdd)
                                {
                                    changeType = ChangeType.Add;
                                }
                                else if (nativeChangeType == NativeChangeType.ListContenTypeDelete)
                                {
                                    changeType = ChangeType.Delete;
                                }

                                var objType = (ChangeObjectType)sr.GetValue(1);
                                var contentTypeId = (byte[])sr.GetValue(3);
                                AveContentTypeObject contentTypeChange = null;

                                if (!IsContainContentTypeId(contentTypeChanges, contentTypeId, out contentTypeChange))
                                {
                                    //contentTypeChange = new AveContentTypeObject { ContentTypeId = contentTypeId };
                                    contentTypeChange = new AveContentTypeObject
                                    {
                                        ContentTypeId = contentTypeId
                                    };
                                    contentTypeChanges.Add(contentTypeId, contentTypeChange);
                                }
                                if (contentTypeChange.ChangeType == ChangeType.Add)
                                {
                                    if (changeType == ChangeType.Delete)
                                    {
                                        RemoveContentType(contentTypeChanges, contentTypeId);
                                    }
                                }

                                else
                                {
                                    contentTypeChange.ChangeType = changeType;
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, "Error occur while access data from method QueryListContentTypeForIB.EventTime:{0}. ErrorMessage:{1}", sr.GetDateTime(4), e);
                            }
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                return contentTypeChanges;
            }
        }

        /// <summary>
        /// 获取Web下系统文件的改变
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="folderObject"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="listObject"></param>
        /// <param name="discoverReader"></param>
        /// <param name="noPropertyFolders"></param>
        [QueryReview("2012/12/11", "Austin Han")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are the part of sql statement. ")]
        public void QuerySystemListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, DateTime startTime, DateTime endTime, AveListObject listObject, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders, List<AveDiscoverExtraItemBaseInfo> extraItems = null)
        {
            var command = string.Format(AveDiscoverQueryString.ItemChangedInCache.Replace("@WHERE", DiscoverConditionString.WebItemChanged), AveWrapperConstants.MaxRows.ToString());
            var allEvents = GetAllEventsObject(command, startTime, endTime);
            var allDocs = GetAllDocsInfos(allEvents, discoverReader,true);
            ItemChanged(allEvents, allDocs, folderObject, folderCache, listObject, discoverReader, noPropertyFolders, extraItems);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ec:EventCache As ec.")]
        /// <summary>
        /// 获取List下Item信息的改变
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="folderObject"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="listObject"></param>
        /// <param name="discoverReader"></param>
        /// <param name="noPropertyFolders"></param>
        public void QueryListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, DateTime startTime, DateTime endTime, AveListObject listObject, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders, List<AveDiscoverExtraItemBaseInfo> extraItems = null)
        {
            string whereString = string.Empty;
            if (WrapperConfiguration.IgnoreDiscoverModifiedBySystem)
            {
                whereString = DiscoverConditionString.ListItemChangedIgnoreModifiedBySystem;
            }
            else
            {
                whereString = DiscoverConditionString.ListItemChanged;
            }
            //ItemChanged(AveDiscoverQueryString.ItemChanged.Replace("@WHERE", whereString), folderObject, startTime, endTime, listObject, discoverReader, noPropertyFolders, extraItems);
            var command = string.Format(AveDiscoverQueryString.ItemChangedInCache.Replace("@WHERE", whereString), AveWrapperConstants.MaxRows.ToString());
            var allEvents = GetAllEventsObject(command, startTime, endTime);
            var allDocs = GetAllDocsInfos(allEvents, discoverReader,false);
            bool enableAttachment = listObject != null && listObject.Flag != null && DiscoverUtility.IsEnableAttachment((long)listObject.Flag);
            //此处逻辑为：当extraItems的Count大于0，同时enableAttachment的时候进行添加attachment操作。但是在list flag ==null的情况下不考虑enableAttachment，都进行添加操作以防丢失数据。
            if (extraItems != null && extraItems.Count > 0 && enableAttachment)
            {
                this.AddAttachmentsGuidToExtraItems(folderCache, extraItems);
            }
            ItemChanged(allEvents, allDocs, folderObject, folderCache, listObject, discoverReader, noPropertyFolders, extraItems);
        }

        private Dictionary<Guid, DocObject> GetAllDocsInfos(List<EventObject> allEvents, AveDiscoverReader discoverReader,bool isSystemList)
        {
            var index = 0;
            var tempDocs = new List<DocObject>();
            while (index < allEvents.Count)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                var tempids = new List<Guid>();
                do
                {
                    var eventObj = allEvents[index++];
                    if (eventObj.DocId == Guid.Empty)
                    {
                        continue;
                    }
                    if (!tempids.Contains(eventObj.DocId))
                    {
                        tempids.Add(eventObj.DocId);
                        sb.AppendFormat("'{0}',", eventObj.DocId);
                    }
                } while (index < allEvents.Count && tempids.Count < 800);
                if (sb.Length > 0) //有需要在Alldoc 表中查询的数据, view,webpart 等不需要再alldoc 中查询
                {
                    sb.Length--;
                    var condition = string.Format(isSystemList ? DiscoverConditionString.SystemDocIdsFor13 : DiscoverConditionString.DocIdsFor13, sb.ToString());
                    var command = discoverReader.GetDocInfoForIBQueryString().Replace("@WHERE", condition);
                    var queryResults = AveQueryUtility.GetDBRows<DocObject>(mQueryWorker, command, string.Empty);
                    if (queryResults != null)
                    {
                        tempDocs.AddRange(queryResults);
                    }
                }
            }
            return tempDocs.Distinct().ToDictionary(k => k.Id, v => v);
        }

        private List<EventObject> GetAllEventsObject(string command, DateTime startTime, DateTime endTime)
        {
            var allEvents = new List<EventObject>();
            var tempEvents = new List<EventObject>();
            mQueryWorker.AddParameter("@endTime", endTime);
            mQueryWorker.AddParameter("@startTime", startTime);
            do
            {
                tempEvents = AveQueryUtility.GetDBRows<EventObject>(mQueryWorker, command, string.Empty);
                if (tempEvents == null)
                {
                    break;
                }
                else if (tempEvents.Count > 0)
                {
                    mQueryWorker.AddParameter("@startTime", tempEvents[tempEvents.Count - 1].EventTime);
                    allEvents.AddRange(tempEvents);
                }
            } while (tempEvents.Count == AveWrapperConstants.MaxRows);
            return allEvents;
        }

        #endregion

        #region Web Level

        /// <summary>
        /// 获取Web的RootFolder信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="listCache"></param>
        /// <param name="rootFolderObject"></param>
        /// <param name="listObject"></param>
        /// <param name="discoverReader"></param>
        /// <param name="noPropertyFolders"></param>
        [QueryReview("2012/12/11", "Austin Han")]
        public void QueryWebRootFolder(AveListCache listCache, AveItemObject rootFolderObject, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            mQuerySessionSchema.QueryWebRootFolder(listCache, rootFolderObject, discoverReader, noPropertyFolders);
        }

        /// <summary>
        /// 获取Web下Securitty信息的改变
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [QueryReview("2012/12/11", "Austin Han")]
        public Dictionary<int, List<AveSecurityObject>> QueryWebSecurityForIB(Guid siteId, Guid webId, DateTime startTime, DateTime endTime)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.QueryWebSecurityForIB"))
            {
                Dictionary<int, List<AveSecurityObject>> webSecurityChanges = new Dictionary<int, List<AveSecurityObject>>();
                mQueryWorker.AddParameter("@endTime", endTime);
                mQueryWorker.AddParameter("@startTime", startTime);
                mQueryWorker.AddParameter("@siteId", siteId);
                mQueryWorker.AddParameter("@webId", webId);
                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.WebSecurityChanged))
                    {
                        while (sr.Read())
                        {
                            var nativeChangeType = (NativeChangeType)sr.GetValue(0);
                            SecurityType securityType = DiscoverUtility.GetSecurityObjectType(nativeChangeType);
                            ChangeType changeType = DiscoverUtility.GetSecurityChangeType(nativeChangeType);

                            switch (securityType)
                            {
                                case SecurityType.Role:
                                    try
                                    {
                                        RoleSecurityChange(changeType, sr, webSecurityChanges);
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Log(AveLogLevel.WARN, "Error occur while access data from method QueryWebSecurityForIB.SecurityType.Role. EventTime:{0}.  ErrorMessage:{1}.", sr.GetDateTime(5), e);
                                    }
                                    break;
                                case SecurityType.Assignment:
                                    try
                                    {
                                        AssignmentSecurityChange(changeType, sr, webSecurityChanges);
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Log(AveLogLevel.WARN, "Error occur while access data from method QueryWebSecurityForIB.SecurityType.Assignment.EventTime:{0}.  ErrorMessage:{1}", sr.GetDateTime(5), e);
                                    }
                                    break;
                                case SecurityType.Scope: //break inherate
                                    try
                                    {
                                        ScopeSecurityChange(changeType, sr, webSecurityChanges);
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Log(AveLogLevel.WARN, "Error occur while access data from method QueryWebSecurityForIB.SecurityType.Scope.EventTime:{0}.  ErrorMessage:{1}", sr.GetDateTime(5), e);
                                    }
                                    break;
                                case SecurityType.None:
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                return webSecurityChanges;
            }
        }

        /// <summary>
        /// 获取Web下所有List的改变
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [QueryReview("2012/12/10", "Austin Han")]
        public Dictionary<Guid, AveListObject> QueryListForIB(Guid siteId, Guid webId, DateTime startTime, DateTime endTime)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.QueryListForIB"))
            {
                Dictionary<Guid, AveListObject> listObjs = new Dictionary<Guid, AveListObject>();
                Dictionary<Guid, AveListObject> deleteListObjs = new Dictionary<Guid, AveListObject>();

                mQueryWorker.AddParameter("@endTime", endTime);
                mQueryWorker.AddParameter("@startTime", startTime);
                mQueryWorker.AddParameter("@siteId", siteId);
                mQueryWorker.AddParameter("@webId", webId);
                try
                {
                    CacheChangeListObject(listObjs, deleteListObjs);
                    InitChangeListObeject(listObjs, deleteListObjs);

                    if (deleteListObjs.Keys.Count > 0)
                    {
                        string delListCmd = @"select u.tp_Title as username from recyclebin r with(nolock)
                                                left join userinfo u with(nolock) on  u.tp_siteid=r.siteid and u.tp_id=r.deleteUserID
                                                inner join alllists a with(nolock) on r.siteid=a.tp_SiteId and r.listid=a.tp_id and r.webid=a.tp_WebId
                                                where r.itemtype=4 and r.siteid=@siteId and a.tp_WebId=@webId and a.tp_id=@ListId;";
                        foreach (Guid listId in deleteListObjs.Keys)
                        {
                            AveListObject deleteList = listObjs[listId];
                            try
                            {
                                mQueryWorker.AddParameter("@ListId", listId);
                                using (SqlDataReader reader = mQueryWorker.ExecuteReader(delListCmd))
                                {
                                    while (reader.Read())
                                    {
                                        deleteList.ModifiedBy = reader.GetString(0);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Log(AveLogLevel.WARN, "Error occur while get the modifiedBy user of delete list:{0} Error Message{1}", deleteList.Title, ex);
                            }
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                return listObjs;
            }
        }

        /// <summary>
        /// 从AllLists和AllDocs表获取属性初始化AveListObject对象；
        /// </summary>
        /// <param name="listObjs"></param>
        [QueryReview("2012/05/21", "Oliver Luo")]
        private void InitChangeListObeject(Dictionary<Guid, AveListObject> listObjs, Dictionary<Guid, AveListObject> deleteListObjs)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                string commandTex = @"
SELECT al.tp_ID,al.tp_Title,al.tp_RootFolder,al.tp_BaseType,al.tp_Flags,ad.DirName+'/'+ad.LeafName as RootFolderUrl,al.tp_ServerTemplate 
FROM AllLists al WITH(NOLOCK) INNER JOIN AllDocs AS ad WITH (NOLOCK, INDEX=Docs_IdLevelUnique) ON ad.SiteId=al.tp_SiteId AND ad.Id=al.tp_RootFolder AND Level=1
WHERE al.tp_SiteId=@siteId AND al.tp_WebId=@WebId AND (";
                foreach (Guid id in listObjs.Keys)
                {
                    sb.Append("tp_ID='" + id + "' or ");
                }
                if (string.IsNullOrEmpty(sb.ToString()))
                {
                    return;
                }
                commandTex += sb.ToString().Remove(sb.Length - 4, 4) + ")";
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(commandTex))
                {
                    while (sr.Read())
                    {
                        Guid listId = sr.GetGuid(0);
                        if (!listObjs.ContainsKey(listId))
                        {
                            continue;
                        }
                        AveListObject listObj = listObjs[listId];
                        long flag = sr.GetInt64(4);
                        listObj.RootFolderUrl = sr.GetString(5).Trim('/');
                        listObj.Name = sr.GetString(1);
                        listObj.Title = sr.GetString(1);
                        listObj.Type = sr.GetInt32(3);
                        listObj.Flag = flag;
                        listObj.RootFolderId = sr.GetGuid(2);
                        listObj.ServerTemplate = sr.GetInt32(6);
                        listObj.Hidden = (flag & ((long)0x100L)) != 0L;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "Error occurred while Init Change List Object. Error Message {0}", ex);
            }
        }

        /// <summary>
        /// 获取changed list object； 
        /// </summary>
        /// <param name="listObjs"></param>
        /// <param name="deleteListObjs"></param>
        [QueryReview("2012/05/21", "Oliver Luo")]
        private void CacheChangeListObject(Dictionary<Guid, AveListObject> listObjs, Dictionary<Guid, AveListObject> deleteListObjs)
        {
            bool hasSystemList = false;
            //List<Guid> docObjs = new List<Guid>();
            //Guid docid;
            try
            {
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.ListChangedEvent))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            if (sr.IsDBNull(2)) //ListId is dbnull {System List}
                            {
                                //if (!sr.IsDBNull(8))
                                //{
                                //    docid = sr.GetGuid(8);
                                //    if (!docObjs.Contains(docid))
                                //    {
                                //        docObjs.Add(docid);
                                //    }
                                //}
                                if (hasSystemList)
                                {
                                    continue;
                                }
                                AveListObject systemList = new AveListObject
                                {
                                    ListId = Guid.Empty,
                                    Name = "{System Folder}",
                                    Title = "{System Folder}"
                                };
                                listObjs.Add(Guid.Empty, systemList);
                                hasSystemList = true;
                                continue;
                            }
                            var ObjType = (ChangeObjectType)sr.GetValue(1);
                            Guid listId = sr.GetGuid(2);
                            AveListObject listObj = null;
                            if (!listObjs.ContainsKey(listId))
                            {
                                listObj = new AveListObject
                                {
                                    ListId = listId
                                };
                                if (ObjType == ChangeObjectType.List && !sr.IsDBNull(5))//当list被彻底删除时需要eventcache表中itemurl来初始化属性；
                                {
                                    string rootFolderUrl = sr.GetString(5);
                                    listObj.RootFolderUrl = rootFolderUrl;
                                    listObj.Name = rootFolderUrl.Contains("/") ? rootFolderUrl.Substring(rootFolderUrl.LastIndexOf('/') + 1) : rootFolderUrl;
                                    listObj.Title = listObj.Name;
                                }
                                //listObj.ModifiedTime = sr.GetDateTime(4);
                                listObjs.Add(listId, listObj);
                            }
                            else
                            {
                                listObj = listObjs[listId];
                            }
                            if (ObjType == ChangeObjectType.List)
                            {
                                listObj.ModifiedTime = sr.GetDateTime(4);
                                ChangeType currentType = listObj.ChangeType;
                                NativeChangeType nativeChangeType = (NativeChangeType)sr[0];
                                ChangeType changeType = DiscoverUtility.GetChangeType(nativeChangeType);
                                if (currentType == ChangeType.Add ||
                                    currentType == ChangeType.Restore)
                                {
                                    if (changeType == ChangeType.Delete)
                                    {
                                        if (!sr.IsDBNull(3))
                                        {
                                            listObj.ModifiedBy = sr.GetString(3);
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
                                    default:
                                        break;
                                }

                                if (nativeChangeType == NativeChangeType.AssignmentDelete)
                                {
                                    if (!sr.IsDBNull(6))
                                    {
                                        AveSecurityObject deleteRoleAssignment = new AveSecurityObject();
                                        // 删除RoleAssignmet时，第13个字段为int0,第14个字段为int1
                                        // int0存放principalID,int1存放RoleID
                                        deleteRoleAssignment.ObjectType = SecurityType.Assignment;
                                        deleteRoleAssignment.PrincipleId = sr.GetInt32(6);
                                        if (!sr.IsDBNull(7))
                                        {
                                            deleteRoleAssignment.RoleId = sr.GetInt32(7);
                                        }
                                        //如果int1为Null，说明把该user/group的权限全部移除了
                                        else
                                        {
                                            deleteRoleAssignment.RoleId = -1;
                                        }
                                        deleteRoleAssignment.EventTime = sr.GetDateTime(4);
                                        listObj.DeleteRoleAssignments.Add(deleteRoleAssignment);
                                    }
                                }
                            }
                            else if (ObjType == ChangeObjectType.Alert && listId != Guid.Empty && sr.IsDBNull(9))
                            {
                                listObj.AlertChangeType = ChangeType.Edit;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Log(AveLogLevel.WARN, "Error occur while Get Change List From EventCache Table. ErrorMessage:{0}", ex);
                        }
                    }
                }
                //以下添加通过DocId回查对应listId的方法，解决ADO-17242，或者page页上删除webpart时eventlog只有docId没有listId的情况；
                try
                {
                    Guid listid;
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString13.ListViewWebPartChangedEvent))
                    {
                        while (sr.Read())
                        {
                            if (!sr.IsDBNull(0))
                            {
                                listid = sr.GetGuid(0);
                                if (!listObjs.ContainsKey(listid))
                                {
                                    AveListObject docListObj = new AveListObject
                                    {
                                        ListId = listid
                                    };
                                    listObjs.Add(listid, docListObj);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.WARN, "Error occur while Get ListId by DocId. ErrorMessage:{0}", ex);
                }
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, "Error occur while access data from method CacheChangeListObject. ErrorMessage:{0}", e);
            }
        }
        #endregion

        #region Site Level

        /// <summary>
        /// 获取Site下的改变信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [QueryReview("2012/05/21", "Oliver Luo")]
        public int GetSiteChangedForIB(Guid siteId, DateTime startTime, DateTime endTime)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.GetSiteChangedForIB (3)"))
            {
                mQueryWorker.AddParameter("@endTime", endTime);
                mQueryWorker.AddParameter("@startTime", startTime);
                mQueryWorker.AddParameter("@siteId", siteId);
                ChangeType type = ChangeType.None;
                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.SiteChanged))
                    {
                        while (sr.Read())
                        {
                            ChangeType changeType = DiscoverUtility.GetChangeType((NativeChangeType)sr[1]);
                            if (changeType == ChangeType.Delete)
                            {
                                type = changeType;
                                break;
                            }
                            else
                            {
                                if (type != ChangeType.Add)
                                {
                                    type = changeType;
                                }
                            }
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                return (int)type;
            }
        }

        /// <summary>
        /// 获取Site本身的改变信息，还有User 以及Group的改变
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="siteCollectionChangeType"></param>
        /// <param name="userChangeType"></param>
        /// <param name="groupChangeType"></param>
        /// <returns></returns>
        [QueryReview("2013/01/29", "Long Liang")]
        public bool GetSiteChangedForIB(Guid siteId, DateTime startTime, DateTime endTime, ref ChangeType siteCollectionChangeType, ref ChangeType userChangeType, ref ChangeType groupChangeType)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.GetSiteChangedForIB (6)"))
            {
                mQueryWorker.AddParameter("@endTime", endTime);
                mQueryWorker.AddParameter("@startTime", startTime);
                mQueryWorker.AddParameter("@siteId", siteId);
                bool changed = false;
                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.SiteCollectionChangedWithUserAndGroup))
                    {
                        while (sr.Read())
                        {
                            changed = true;

                            ChangeType changeType = DiscoverUtility.GetChangeType((NativeChangeType)sr[0]);
                            ChangeObjectType objectType = (ChangeObjectType)sr[1];

                            switch (objectType)
                            {
                                case ChangeObjectType.Site:
                                    if (siteCollectionChangeType == ChangeType.Add)
                                    {
                                        if (changeType == ChangeType.Delete)
                                        {
                                            siteCollectionChangeType = changeType;
                                        }
                                    }
                                    else
                                    {
                                        siteCollectionChangeType = changeType;
                                    }
                                    break;
                                case ChangeObjectType.Group:
                                    groupChangeType |= changeType;
                                    break;
                                case ChangeObjectType.User:
                                    userChangeType |= changeType;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                return changed;
            }
        }


        /// <summary>
        /// 获取Site下Web的改变信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [QueryReview("2012/12/11", "Austin Han")]
        public Dictionary<Guid, AveWebObject> QueryWebForIB(Guid siteId, DateTime startTime, DateTime endTime)
        {
            return mQuerySessionSchema.QueryWebForIB(siteId, startTime, endTime);
        }

        /// <summary>
        /// 获取Site下Security信息的改变(User/Group）
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [QueryReview("2012/12/10", "Austin Han")]
        public Dictionary<int, AveSiteMemberObject> QuerySiteSecurityForIB(Guid siteId, DateTime startTime, DateTime endTime)
        {
            using (new AvePerformanceScope("AveDiscoverQuery.QuerySiteSecurityForIB"))
            {
                mQueryWorker.AddParameter("@endTime", endTime);
                mQueryWorker.AddParameter("@startTime", startTime);
                mQueryWorker.AddParameter("@siteId", siteId);

                Dictionary<int, AveSiteMemberObject> groups = new Dictionary<int, AveSiteMemberObject>();
                Dictionary<int, AveSiteMemberObject> users = new Dictionary<int, AveSiteMemberObject>();

                Dictionary<int, AveSiteMemberObject> memberChanges = new Dictionary<int, AveSiteMemberObject>();
                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.SiteSecurityChanged))
                    {
                        while (sr.Read())
                        {
                            DateTime eventTime = sr.GetDateTime(0);
                            try
                            {
                                int principalId = sr.GetInt32(2);
                                var eventType = (NativeChangeType)sr.GetValue(4);
                                var changeObjectType = (ChangeObjectType)sr.GetValue(5);

                                AveSiteMemberObject memberChange = null;
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
                                        memberChange = GetUser(users, principalId, eventTime);
                                    }
                                    memberChanges.Add(principalId, memberChange);
                                }
                                memberChange.EventTime = eventTime;

                                string title = sr.IsDBNull(3) ? string.Empty : sr.GetString(3);

                                if (string.IsNullOrEmpty(memberChange.Title) || !memberChange.Title.Equals(title))
                                {
                                    memberChange.Title = title;
                                }

                                ChangeType changeType = DiscoverUtility.GetChangeType(eventType);
                                if (memberChange.ChangeType == ChangeType.Add)
                                {
                                    if (changeType == ChangeType.Delete)
                                    {
                                        memberChange.ChangeType = ChangeType.Delete;
                                        continue;
                                    }
                                }
                                else
                                {
                                    memberChange.ChangeType = changeType;
                                }
                                #region Get group members
                                if (changeObjectType == ChangeObjectType.Group && !sr.IsDBNull(1))
                                {
                                    int userId = sr.GetInt32(1);
                                    AveSiteMemberObject user = null;
                                    if (eventType == NativeChangeType.MemberAdd)
                                    {
                                        user = GetUser(users, userId, eventTime);
                                        if (memberChange.AddedMemberIds == null)
                                        {
                                            memberChange.AddedMemberIds = new Dictionary<int, AveSiteMemberObject>();
                                        }
                                        memberChange.AddedMemberIds.Add(userId, user);
                                    }
                                    else if (eventType == NativeChangeType.MemberDelete)
                                    {
                                        user = GetUser(users, userId, eventTime);
                                        if (memberChange.DeletedMemberIds == null)
                                        {
                                            memberChange.DeletedMemberIds = new Dictionary<int, AveSiteMemberObject>();
                                        }
                                        memberChange.DeletedMemberIds.Add(userId, user);
                                        if (memberChange.AddedMemberIds != null && memberChange.AddedMemberIds.ContainsKey(userId))
                                        {
                                            memberChange.AddedMemberIds.Remove(userId);
                                        }
                                    }
                                }
                                #endregion
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, "Exception occur while access data from method QuerySiteSecurityForIB. EventTime:{0}.  Exception:{1}.  SiteId:{2}", eventTime, e, siteId);
                            }
                        }
                    }
                    QueryUserProperty(users, Guid.Empty);
                    QueryGroupProperty(groups, Guid.Empty);
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                return memberChanges;
            }
        }

        #endregion

        #endregion

        #region IAveDiscoverQueryService Members
        public long GetObjectChangedSize(Guid siteId, Guid webId, Guid listId, string folderPath, DateTime beginTime)
        {
            using (new AvePerformanceScope("AveCLReader.GetObjectChangedSize"))
            {
                List<string> parentIdList;
                long changeSizeInAllDocs = 0;
                long changeSizeInAllDocVersions = 0;
                long changeSizeInAllUserData = 0;
                try
                {
                    //不确定要获取Size的对象类型，所以根据参数是否为空来确定查询条件。
                    changeSizeInAllDocs = GetObjectChangeSizeInAllDocs(siteId, webId, listId, folderPath, beginTime, out parentIdList);
                    changeSizeInAllDocVersions = GetObjectChangeSizeInAllDocVersions(siteId, webId, listId, folderPath, beginTime);
                    changeSizeInAllUserData = GetObjectChangeSizeInAllUserData(siteId, webId, listId, folderPath, beginTime, parentIdList);
                }
                catch (Exception ex)
                {
                    logger.Warn(@"An error occurred while getting object changed size, 
                              site id: {0}, web id: {1}, list id: {2}, folder path: {3}, start time: {4}, error: {5}"
                                , siteId, webId, listId, folderPath, beginTime, ex);
                }
                return changeSizeInAllDocs + changeSizeInAllDocVersions + changeSizeInAllUserData;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "sql table abbreviation. ")]
        private long GetObjectChangeSizeInAllDocs(Guid siteId, Guid webId, Guid listId, string folderPath, DateTime beginTime, out List<string> parentIdList)
        {
            long allDocsChangeSize = 0;
            mQueryWorker.ClearParameters();
            StringBuilder cmdBuilder = new StringBuilder();
            cmdBuilder.AppendLine("SELECT SUM(cast(Size as bigint)) ,ParentId FROM AllDocs with(nolock) WHERE Id IN (");
            cmdBuilder.AppendLine("SELECT DISTINCT dc.Id FROM AllDocs dc with(nolock) INNER JOIN EventCache et with(nolock) ON et.SiteId=dc.SiteId AND et.WebId=dc.WebId AND et.DocId=dc.Id ");
            cmdBuilder.AppendLine("WHERE et.EventTime>@StartTime AND dc.DeleteTransactionId=0x ");
            if (siteId != Guid.Empty)
            {
                mQueryWorker.AddParameter("@SiteId", siteId);
                cmdBuilder.AppendLine("AND dc.SiteId=@SiteId");
            }
            if (webId != Guid.Empty)
            {
                mQueryWorker.AddParameter("@WebId", webId);
                cmdBuilder.AppendLine("AND dc.WebId=@WebId");
            }
            if (listId != Guid.Empty)
            {
                mQueryWorker.AddParameter("@ListId", listId);
                cmdBuilder.AppendLine("AND dc.ListId=@ListId");
            }
            if (!string.IsNullOrEmpty(folderPath))
            {
                mQueryWorker.AddParameter("@DirName", folderPath + "%");
                cmdBuilder.AppendLine("AND dc.DirName like @DirName ");
            }

            mQueryWorker.AddParameter("@StartTime", beginTime);
            cmdBuilder.Append(" AND dc.Size is not null)");
            cmdBuilder.Append(" Group By ParentId");
            object obj;
            parentIdList = new List<string>();
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdBuilder.ToString()))
            {
                while (reader.Read())
                {
                    obj = reader.GetValue(0);
                    if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                    {
                        allDocsChangeSize += long.Parse(obj.ToString());
                    }
                    parentIdList.Add(reader.GetValue(1).ToString());
                }
            }
            return allDocsChangeSize;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "sql table abbreviation. ")]
        private long GetObjectChangeSizeInAllDocVersions(Guid siteId, Guid webId, Guid listId, string folderPath, DateTime beginTime)
        {
            long changeSizeInAllDocVersions = 0;
            mQueryWorker.ClearParameters();
            StringBuilder cmdBuilder = new StringBuilder();
            cmdBuilder.AppendLine("SELECT  SUM(cast(Size as bigint)) FROM AllDocVersions with(nolock) WHERE Id IN (");
            cmdBuilder.AppendLine("SELECT DISTINCT dc.Id FROM AllDocs dc with(nolock) INNER JOIN EventCache et with(nolock) ON et.SiteId=dc.SiteId AND et.WebId=dc.WebId AND et.DocId=dc.Id ");
            cmdBuilder.AppendLine("WHERE et.EventTime>@StartTime AND dc.DeleteTransactionId=0x ");

            if (siteId != Guid.Empty)
            {
                mQueryWorker.AddParameter("@SiteId", siteId);
                cmdBuilder.AppendLine("AND dc.SiteId=@SiteId");
            }
            if (webId != Guid.Empty)
            {
                mQueryWorker.AddParameter("@WebId", webId);
                cmdBuilder.AppendLine("AND dc.WebId=@WebId");
            }
            if (listId != Guid.Empty)
            {
                mQueryWorker.AddParameter("@ListId", listId);
                cmdBuilder.AppendLine("AND dc.ListId=@ListId");
            }
            if (!string.IsNullOrEmpty(folderPath))
            {
                mQueryWorker.AddParameter("@DirName", folderPath + "%");
                cmdBuilder.AppendLine("AND dc.DirName like @DirName");
            }
            mQueryWorker.AddParameter("@StartTime", beginTime);
            cmdBuilder.Append(") AND DeleteTransactionId=0x");
            object obj = mQueryWorker.ExecuteScalar(cmdBuilder.ToString());
            if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
            {
                changeSizeInAllDocVersions = long.Parse(obj.ToString());
            }
            return changeSizeInAllDocVersions;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "sql table abbreviation. ")]
        private long GetObjectChangeSizeInAllUserData(Guid siteId, Guid webId, Guid listId, string folderPath, DateTime beginTime, List<string> parentIdList)
        {
            long changeSizeInAllUserData = 0;
            mQueryWorker.ClearParameters();
            StringBuilder cmdBuilder = new StringBuilder();
            string parentIdStr = string.Empty;
            List<string> parentIdUnit = new List<string>();
            for (int i = 0; i < parentIdList.Count; i++)
            {
                mQueryWorker.AddParameter("@ParentId" + i, parentIdList[i]);
                parentIdStr += ("@ParentId" + i + ",");
                if ((i % 5000) == 0 && (i != 0))
                {
                    parentIdUnit.Add(parentIdStr);
                    parentIdStr = string.Empty;
                }
            }
            parentIdUnit.Add(parentIdStr);

            foreach (string pIdUnit in parentIdUnit)
            {
                cmdBuilder.AppendLine("SELECT SUM(cast(tp_Size as bigint)) FROM AllUserData with(nolock) WHERE tp_GUID IN (");
                cmdBuilder.AppendLine("SELECT DISTINCT ud.tp_GUID FROM AllUserData ud with(nolock) INNER JOIN EventCache et with(nolock) ON et.SiteId=ud.tp_SiteId AND et.ListId=ud.tp_ListId AND et.ItemId=ud.tp_ID ");
                cmdBuilder.AppendLine("WHERE et.EventTime>@StartTime AND ud.tp_DeleteTransactionId=0x ");
                if (siteId != Guid.Empty)
                {
                    mQueryWorker.AddParameter("@SiteId", siteId);
                    cmdBuilder.AppendLine("AND ud.tp_SiteId=@SiteId");
                }
                if (webId != Guid.Empty)
                {
                    mQueryWorker.AddParameter("@WebId", webId);
                    cmdBuilder.AppendLine("AND et.WebId=@WebId");
                }
                if (listId != Guid.Empty)
                {
                    mQueryWorker.AddParameter("@ListId", listId);
                    cmdBuilder.AppendLine("AND ud.tp_ListId=@ListId");
                }
                mQueryWorker.AddParameter("@StartTime", beginTime);
                if (string.IsNullOrEmpty(folderPath))
                {
                    cmdBuilder.AppendLine("AND ud.tp_ParentId in (");
                    string parentIDString = pIdUnit.TrimEnd(',') + ")";
                    cmdBuilder.Append(parentIDString);
                }

                cmdBuilder.Append(")");
                Object obj = mQueryWorker.ExecuteScalar(cmdBuilder.ToString());
                if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                {
                    changeSizeInAllUserData = long.Parse(obj.ToString());
                }
            }
            return changeSizeInAllUserData;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ud is the abbreviation fo alluserdata. ")]
        public long GetListSize(Guid siteId, Guid webId, Guid listId)
        {
            using (new AvePerformanceScope("AveCLReader.GetListSize"))
            {
                long result = 0;
                try
                {
                    #region Calculate size in AllDocs table
                    string sCmdTxt = @"SELECT SUM(cast(Size as bigint)) FROM AllDocs with(nolock) 
                                   WHERE SiteId=@SiteId AND WebId=@WebId AND ListId=@ListId AND DeleteTransactionId=0x";
                    mQueryWorker.AddParameter("@SiteId", siteId);
                    mQueryWorker.AddParameter("@WebId", webId);
                    mQueryWorker.AddParameter("@ListId", listId);

                    object obj = mQueryWorker.ExecuteScalar(sCmdTxt);
                    if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                        result += long.Parse(obj.ToString());
                    #endregion

                    #region Calculate size in AllDocVersions table
                    sCmdTxt = @"SELECT SUM(cast(Size as bigint)) FROM AllDocVersions with(nolock) 
                            WHERE Id IN (SELECT Id FROM AllDocs with(nolock) 
                            WHERE SiteId=@SiteId AND WebId=@WebId AND ListId=@ListId AND DeleteTransactionId=0x) 
                            AND DeleteTransactionId=0x";

                    obj = mQueryWorker.ExecuteScalar(sCmdTxt);
                    if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                        result += long.Parse(obj.ToString());
                    #endregion

                    #region Calculate size in AllUserData table //jisuan chongfu duohang
                    sCmdTxt = @"SELECT SUM(cast(tp_Size as bigint)) FROM AllUserData with(nolock) 
                            WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND tp_DeleteTransactionId=0x";

                    obj = mQueryWorker.ExecuteScalar(sCmdTxt);
                    if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                        result += long.Parse(obj.ToString());
                    #endregion
                }
                catch (Exception ex)
                {
                    logger.Warn("An error occurred while getting list size, site id: {0}, web id: {1}, list id: {2}, error: {3}", siteId, webId, listId, ex);
                }
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentIdCollection"></param>
        /// <param name="startIndex"> min 0  , max parentIdCollection</param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        private string GetQueryUserDataSizeCommmand(List<string> parentIdCollection, int startIndex, int batchSize)
        {
            if (startIndex >= parentIdCollection.Count)
            {
                return string.Empty;
            }
            const string commandBaseStr = "SELECT SUM(cast(tp_Size as bigint)) FROM AllUserData with(nolock) WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND tp_ParentId in ({0}) AND tp_DeleteTransactionId=0x";
            StringBuilder commandArgsBuilder = new StringBuilder();
            int endIndex = parentIdCollection.Count > startIndex + batchSize ? startIndex + batchSize : parentIdCollection.Count;
            for (var k = startIndex; k < endIndex; k++)
            {
                commandArgsBuilder.AppendFormat("'{0}',", parentIdCollection[k]);
            }
            return string.Format(commandBaseStr, commandArgsBuilder.ToString().TrimEnd(','));
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ud is the abbreviation fo alluserdata. ")]
        public long GetFolderSize(Guid siteId, Guid webId, Guid listId, string folderUrl)
        {
            using (new AvePerformanceScope("AveCLReader.GetFolderSize"))
            {
                long result = 0;
                try
                {
                    #region Calculate size in AllDocs table
                    string sCmdTxt = @"SELECT SUM(cast(Size as bigint)),ParentId FROM AllDocs with(nolock) 
                                   WHERE SiteId=@SiteId AND WebId=@WebId AND ListId=@ListId AND DirName like @DirName AND DeleteTransactionId=0x Group By ParentId ";
                    mQueryWorker.AddParameter("@SiteId", siteId);
                    mQueryWorker.AddParameter("@WebId", webId);
                    mQueryWorker.AddParameter("@ListId", listId);
                    mQueryWorker.AddParameter("@DirName", folderUrl + "%");

                    object obj;
                    List<string> parentIdCollection = new List<string>();
                    using (SqlDataReader reader = mQueryWorker.ExecuteReader(sCmdTxt.ToString()))
                    {
                        while (reader.Read())
                        {
                            obj = reader.GetValue(0);
                            if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                            {
                                result += long.Parse(obj.ToString());
                            }
                            //record parentid, use it query AllUserData later
                            parentIdCollection.Add(reader.GetValue(1).ToString());
                        }
                    }
                    #endregion

                    #region Calculate size in AllDocVersions table
                    sCmdTxt = @"SELECT SUM(cast(Size as bigint)) FROM AllDocVersions with(nolock) 
                            WHERE SiteId=@SiteId AND Id IN (SELECT Id FROM AllDocs with(nolock) 
                            WHERE SiteId=@SiteId AND WebId=@WebId AND ListId=@ListId AND DirName like @DirName AND DeleteTransactionId=0x) 
                            AND DeleteTransactionId=0x";

                    obj = mQueryWorker.ExecuteScalar(sCmdTxt);
                    if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                        result += long.Parse(obj.ToString());
                    #endregion

                    #region Calculate size in AllUserData table
                    int queryBatchSize = 1000;
                    int startIndex = 0;
                    int parentIdCount = parentIdCollection.Count;
                    while (startIndex < parentIdCount)
                    {
                        var command = string.Empty;
                        try
                        {
                            command = GetQueryUserDataSizeCommmand(parentIdCollection, startIndex, queryBatchSize);
                            obj = mQueryWorker.ExecuteScalar(command);
                            if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                                result += long.Parse(obj.ToString());
                        }
                        catch (Exception e)
                        {
                            logger.Warn("An error occurred while getting folder size in calculation, folder url: {0}, error:{1} ", folderUrl, e);
                        }
                        startIndex += queryBatchSize;
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    logger.Warn("An error occurred while getting folder size, site id: {0}, web id: {1}, list id: {2}, folder url:{3}, error: {4}", siteId, webId, listId, folderUrl, ex);
                }
                return result;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ud is the abbreviation fo alluserdata. ")]
        public long GetWebSize(Guid siteId, Guid webId)
        {
            using (new AvePerformanceScope("AveCLReader.GetWebSize"))
            {
                long result = 0;
                try
                {
                    #region Calculate size in AllDocs table
                    string sCmdTxt = @"SELECT SUM(cast(Size as bigint)) FROM AllDocs with(nolock) 
                                   WHERE SiteId=@SiteId AND WebId=@WebId AND DeleteTransactionId=0x";

                    mQueryWorker.AddParameter("@SiteId", siteId);
                    mQueryWorker.AddParameter("@WebId", webId);


                    Object obj = mQueryWorker.ExecuteScalar(sCmdTxt);
                    if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                        result += long.Parse(obj.ToString());
                    #endregion

                    #region Calculate size in AllDocVersion table
                    sCmdTxt = @"SELECT SUM(cast(Size as bigint)) FROM AllDocVersions with(nolock) 
                            WHERE SiteId=@SiteId AND Id IN (SELECT Id FROM AllDocs with(nolock) 
                            WHERE SiteId=@SiteId AND WebId=@WebId AND DeleteTransactionId=0x) 
                            AND DeleteTransactionId=0x";

                    obj = mQueryWorker.ExecuteScalar(sCmdTxt);
                    if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                        result += long.Parse(obj.ToString());
                    #endregion

                    #region Calculate size in AllUserData table
                    sCmdTxt = @"SELECT SUM (cast(tp_Size as bigint)) FROM AllUserData U with(nolock) INNER JOIN AllLists L with(nolock) ON U.tp_ListId = L.tp_ID 
                            INNER JOIN Webs W with(nolock) ON W.SiteId = U.tp_SiteId AND W.ID = L.tp_WebId 
                            WHERE U.tp_SiteId = @SiteId AND W.ID = @WebId AND U.tp_DeleteTransactionId = 0x";

                    obj = mQueryWorker.ExecuteScalar(sCmdTxt);
                    if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                        result += long.Parse(obj.ToString());
                    #endregion
                }
                catch (Exception ex)
                {
                    logger.Warn("An error occurred while getting web size, site id: {0}, web id: {1}, error: {2}", siteId, webId, ex);
                }
                return result;
            }
        }
        #endregion


        [QueryReview("2012/12/11", "Austin Han")]
        private List<Dictionary<string, object>> GetItemsDocsInfo(Guid siteId, List<AveDiscoverExtraItemBaseInfo> items)
        {
            var results = new List<Dictionary<string, object>>();
            if (items != null && items.Count > 0)
            {
                StringBuilder cmdText = new StringBuilder(AveDiscoverQueryString.ItemChangedByCustomItems13);
                for (int i = 0; i < items.Count; i++)
                {
                    cmdText.Append("'");
                    cmdText.Append(items[i].Id.ToString());
                    cmdText.Append("' ,");
                    if (cmdText.Length > 40960 || i == items.Count - 1)
                    {
                        --cmdText.Length;
                        cmdText.Append(')'); //去除最后的逗号
                        mQueryWorker.AddParameter("@SiteId", siteId);
                        using (SqlDataReader sr = mQueryWorker.ExecuteReader(cmdText.ToString()))
                        {
                            var result = AveSqlUtility.GetDBRows(sr, true);
                            if (result != null)
                            {
                                results.AddRange(result);
                            }
                        }
                        cmdText.Length = AveDiscoverQueryString.ItemChangedByCustomItems13.Length;
                    }
                }
            }
            return results;
        }

        private void AddExtraAttachmentForIB(Dictionary<int, List<AveItemObject>> attachments, string itemFullUrl, ChangeType changeType, Dictionary<string, object> itemDoc)
        {
            var docId = (Guid)itemDoc["Id"];
            string leafName = string.Empty;
            int itemId = GetItemId(itemFullUrl, out leafName);

            if (!attachments.ContainsKey(itemId))
            {
                attachments.Add(itemId, new List<AveItemObject>());
            }
            foreach (AveItemObject attach in attachments[itemId])
            {
                if (attach.DocID == docId)
                {
                    return;
                }
            }
            AveItemObject attachment = new AveItemObject
            {
                DocID = docId
            };
            attachments[itemId].Add(attachment);
            InitItemObject(attachment, itemDoc);
            attachment.ChangeType = changeType;
        }

        private void InitItemObject(AveItemObject obj, Dictionary<string, object> itemDocInfo)
        {
            obj.DocID = (Guid)itemDocInfo["Id"];
            obj.DirName = (string)itemDocInfo["DirName"];
            obj.SourceName = obj.LeafName = obj.ItemName = (string)itemDocInfo["LeafName"];
            obj.FullUrl = (obj.DirName + "/" + obj.LeafName).Trim('/');
            obj.TimeLastModified = (DateTime)itemDocInfo["TimeLastModified"];
            obj.Uiversion = (int)itemDocInfo["UIVersion"];
            if (!(itemDocInfo["DoclibRowId"] is DBNull))
            {
                obj.ID = (int?)itemDocInfo["DoclibRowId"];
            }
            obj.Type = (byte)itemDocInfo["Type"];
            obj.Level = (byte)itemDocInfo["Level"];
        }

        public void GetDeleteSites(Dictionary<Guid, AveSiteObject> deletedSites, DateTime startTime, DateTime endTime)
        {
            if (deletedSites == null)
            {
                return;
            }
            using (new AvePerformanceScope("AveDiscoverQuery.GetDeleteSites"))
            {
                mQueryWorker.AddParameter("@startTime", startTime);
                mQueryWorker.AddParameter("@endTime", endTime);
                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.SiteDeleted))
                    {
                        while (sr.Read())
                        {
                            AveSiteObject site = new AveSiteObject();
                            site.ChangeType = ChangeType.Delete;
                            site.Id = (Guid)sr["SiteId"];
                            site.EventTime = (DateTime)sr["EventTime"];
                            site.Url = string.Empty;
                            deletedSites[site.Id] = site;
                        }
                    }
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }
        }

        public void SetItemStubInfo(List<AveItemObject> allitems, Guid siteId)
        {
            SetItemStubInfo(allitems, siteId, false);
        }

        public void SetItemStubInfo(List<AveItemObject> allitems, Guid siteId, bool includeRecycleBin)
        {
            using (var scope = new AvePerformanceScope("AveDiscoverQuery.SetItemStubInfo"))
            {
                mQueryWorker.AddParameter("@SiteId", siteId);
                var index = 0;
                Dictionary<Guid, AveItemObject> allItemsKeyValues = allitems.Distinct(new ItemObjectDistinc()).ToDictionary(key => key.DocID, value => value);
                while (index < allitems.Count)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    var tempids = new List<Guid>();
                    do
                    {
                        var item = allitems[index++];
                        if (item.DocID == Guid.Empty)
                        {
                            continue;
                        }
                        if (!tempids.Contains(item.DocID))
                        {
                            tempids.Add(item.DocID);
                            sb.AppendFormat("'{0}',", item.DocID);
                        }
                    } while (index < allitems.Count && tempids.Count < 800);
                    if (sb.Length > 0) //有需要在Alldoc 表中查询的数据, view,webpart 等不需要再alldoc 中查询
                    {
                        sb.Length--;
                        string condition = string.Format(includeRecycleBin ? AveQueryString13.Sp13ItemStubsByIdsWithRecycleBin : AveQueryString13.Sp13ItemStubsByIds, sb.ToString());
                        var command = AveQueryString13.Sp13ItemStubsByIdsCammandLine.Replace("@WHEREAllDocs", condition);
                        using (var reader = mQueryWorker.ExecuteReader(command))
                        {
                            while (reader.Read())
                            {
                                try
                                {
                                    var id = (Guid)reader["Id"];
                                    var currentItem = allItemsKeyValues[id];
                                    if (!(reader["DocFlags"] is DBNull))
                                    {
                                        currentItem.DocFlags = (int)reader["DocFlags"];
                                    }
                                    if (!(reader["RbsId"] is DBNull))
                                    {
                                        currentItem.RbsId = (byte[])reader["RbsId"];
                                    }
                                    if (!(reader["Content"] is DBNull))
                                    {
                                        currentItem.Content = (byte[])reader["Content"];
                                    }
                                }
                                catch (Exception e)
                                {
                                    logger.Warn("An error occurred while getting stub infos.Error:{0}", e);
                                }
                            }
                        }
                    }
                }
            }
        }


        #region IAveDiscoverQueryService Members


        public List<Dictionary<String, Object>> GetCheckoutListItems(AveFolderCache folderCache, AveListObject listObj)
        {
            var checkoutItemInfoList = new List<Dictionary<String, Object>>();

            const String commandSQLString = @"SELECT doc.CheckoutUserId, doc.DoclibRowId, Id FROM AllDocs as doc WITH(NOLOCK)
                                              WHERE doc.level = 255 AND doc.SiteId = @SiteId AND doc.WebId = @WebId AND doc.ListId = @ListId  
                                              AND doc.CheckoutUserId IS NOT NULL AND doc.DoclibRowId IS NOT NULL AND DeleteTransactionId=0x";
            if (folderCache == null || folderCache.SiteId == null || folderCache.WebId == null ||
                folderCache.ListId == null || folderCache.AveSite == null || folderCache.AveWeb == null)
            {
                return checkoutItemInfoList;
            }
            mQueryWorker.AddParameter("@SiteId", folderCache.SiteId);
            mQueryWorker.AddParameter("@WebId", folderCache.WebId);
            mQueryWorker.AddParameter("@ListId", folderCache.ListId);
            using (SqlDataReader sr = mQueryWorker.ExecuteReader(commandSQLString))
            {
                while (sr.Read())
                {
                    try
                    {
                        var checkoutItemInfo = new Dictionary<String, Object>();
                        var userId = Convert.ToInt32(sr["CheckoutUserId"]);
                        var rowId = Convert.ToInt32(sr["DoclibRowId"]);
                        var itemId = (Guid)sr["Id"];
                        checkoutItemInfo.Add("UserId", userId);
                        checkoutItemInfo.Add("RowId", rowId);
                        checkoutItemInfo.Add("ItemId", itemId);
                        checkoutItemInfoList.Add(checkoutItemInfo);
                    }
                    catch (Exception e)
                    {
                        logger.Error("An error occurred while getting checkout ListItem infos.Error:{0}", e);
                    }
                }
            }
            return checkoutItemInfoList;
        }

        public void QueryUserOrGroupProperty(Dictionary<int, AveSiteMemberObject> siteMember, Guid siteId, ChangeObjectType changeObjType)
        {
            if (changeObjType == ChangeObjectType.User)
            {
                this.QueryUserProperty(siteMember, siteId);
            }
            else if (changeObjType == ChangeObjectType.Group)
            {
                this.QueryGroupProperty(siteMember, siteId);
            }
            else
            {
                return;
            }
        }
        #endregion

    }
    public class AveItemChangedResultCollection
    {
        public Dictionary<int, AveItemObject> Items = new Dictionary<int, AveItemObject>();
        public Dictionary<Guid, AveItemObject> SystemItems = new Dictionary<Guid, AveItemObject>();
        public Dictionary<int, List<AveItemObject>> Attachments = new Dictionary<int, List<AveItemObject>>();
        public Dictionary<Guid, AveAlertObject> FolderAlerts = new Dictionary<Guid, AveAlertObject>();
        public Dictionary<int, AveItemObject> ItemAlerts = new Dictionary<int, AveItemObject>();
        public Dictionary<Guid, EventObject> SystemItemViews = new Dictionary<Guid, EventObject>();
    }

}
