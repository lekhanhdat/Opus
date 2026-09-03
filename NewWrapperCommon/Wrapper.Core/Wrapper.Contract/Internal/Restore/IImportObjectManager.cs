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
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Core.Common;
using AvePoint.Wrapper.Core.SPRestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Core.Internal.Restore
{
    /// <summary>
    /// 主要是管理restore之后的mapping关系，以及post action
    /// 
    /// 由于这个对象是贯穿整个restore，包括多线程，所以放在这里的数据需要保证唯一性，如果一个数据多变，那就不能放在该接口中，
    /// 比如，Ilanguage对象和web的语言有关系，不能放这里，因为存在web多线程还原。
    /// </summary>
    public interface IImportObjectManager
    {
        /// <summary>
        /// User Mapping
        /// </summary>
        IUserMapping UserMapping { get; set; }

        /// <summary>
        /// Mapping controller
        /// </summary>
        ILanguageMappingController LanguageMappingController { get; set; }

        /// <summary>
        /// Root Web Language mapping
        /// </summary>
        ILanguageMapping RootWebLanguageMapping { get; set; }

        /// <summary>
        /// User Manager
        /// </summary>
        IImportUserManager UserManager { get; }

        /// <summary>
        /// Group Manager
        /// </summary>
        IImportGroupManager GroupManager { get; }

        /// <summary>
        /// Post Action Manager
        /// </summary>
        IImportPostActionManager PostActionManager { get; }

        /// <summary>
        /// User Profile Manager
        /// </summary>
        IImportUserProfileManager UserProfileManager { get; }

        /// <summary>
        /// Metadata Manager
        /// </summary>
        IImportMetadataManager MetadataManager { get; }

        IImportSecurityManager SecurityManager { get; }

        /// <summary>
        /// URL Replace function
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        string ResolveUrl(string url);
    }

    public interface IImportPostActionManager
    {
        /// <summary>
        /// Add post action into collection
        /// </summary>
        /// <param name="postAction"></param>
        void AddPostAction(ISiteImportPostAction postAction);

        /// <summary>
        /// Execute All post actions
        /// </summary>
        void ExecutePostActions(ISiteImport siteImport);

        /// <summary>
        /// Execute all post actions
        /// </summary>
        /// <param name="webImport"></param>
        void ExecutePostActions(IWebImport webImport);

        /// <summary>
        /// Execute Post Actions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="siteImport"></param>
        /// <param name="profiler"></param>
        void ExecutePostActions<T>(ISiteImport siteImport, ISPImportProfiler profiler);
    }

    public interface IImportUserManager
    {
        /// <summary>
        /// Cache UnRestored Users for item level
        /// </summary>
        /// <param name="users"></param>
        void CacheUsers(List<Wrapper.Common.AveUserInfo> users);

        /// <summary>
        /// Remove unrestored user from cache
        /// </summary>
        /// <param name="userId"></param>
        void RemoveUnRestoredUser(int userId);

        /// <summary>
        /// try get restored user id
        /// </summary>
        /// <param name="sourceUserId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool TryGetRestoredUserId(int sourceUserId, out int userId);

        /// <summary>
        /// try get restored user loginName
        /// </summary>
        /// <param name="sourceUserId"></param>
        /// <param name="loginName"></param>
        /// <returns></returns>
        bool TryGetRestoredUserLoginName(int sourceUserId, out string loginName);

        /// <summary>
        /// try get unrestored unser info
        /// </summary>
        /// <param name="sourceUserId"></param>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        bool TryGetUnRestoredUserInfo(int sourceUserId, out AvePoint.Wrapper.Common.AveUserInfo userInfo);

        /// <summary>
        /// 判断是否已经之前还原过，并且出现问题的。
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        bool IsFakeUser(string userLogin);

        /// <summary>
        /// 添加fake user
        /// </summary>
        /// <param name="userLogin"></param>
        void AddFakeUser(string userLogin);

        /// <summary>
        /// Add restored user into cache
        /// </summary>
        /// <param name="sourceUserId"></param>
        /// <param name="destUserId"></param>
        /// <param name="destLoginName"></param>
        void AddRestoredUserInfo(int sourceUserId, int destUserId, string destLoginName);

        /// <summary>
        /// Default User Id
        /// </summary>
        int DefaultUserId { get; set; }
    }

    public interface IImportGroupManager
    {
        /// <summary>
        /// Cache UnRestoredGroups
        /// </summary>
        /// <param name="groups"></param>
        void CacheGroups(List<Wrapper.Common.AveGroupInfo> groups);

        /// <summary>
        /// Remove unrestore group
        /// </summary>
        /// <param name="id"></param>
        void RemoveUnRestoredGroup(int id);

        /// <summary>
        /// Get restored group
        /// </summary>
        /// <param name="sourceGroupId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        bool TryGetRestoredGroup(int sourceGroupId, out int groupId);

        /// <summary>
        /// Add Restored Group Info
        /// </summary>
        /// <param name="sourceGroupId"></param>
        /// <param name="destGroupId"></param>
        /// <param name="destGroupName"></param>
        void AddRestoredGroupInfo(int sourceGroupId, int destGroupId, string destGroupName);
    }

    public interface IImportUserProfileManager
    {

        /// <summary>
        /// Add audience mapping
        /// </summary>
        /// <param name="sourceId"></param>
        /// <param name="currentId"></param>
        /// <param name="name"></param>
        void AddAudienceMapping(string sourceId, string currentId, string name);
    }

    public interface IImportSecurityManager
    {
        /// <summary>
        /// 添加role cache
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="desRoleId"></param>
        void AddRoleInfo(int sourceRoleId, int destRoleId, string roleName);

        /// <summary>
        /// 获取还原后的role 信息
        /// </summary>
        /// <param name="sourceRoleId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        bool TryGetRestoreRole(int sourceRoleId, out int roleId);
 
    }

    internal class TermStoreMappingManager
    {
        private Dictionary<MetadataCacheType, Dictionary<Guid, Guid>> restoredMetaIDMappings = new Dictionary<MetadataCacheType, Dictionary<Guid, Guid>>();
        private Dictionary<Guid, Wrapper.Common.AveMetadataGroupInfo> sourceTermGroupDataCache = new Dictionary<Guid, Wrapper.Common.AveMetadataGroupInfo>();
        private Dictionary<Guid, Wrapper.Common.AveTermSetInfo> sourceTermSetDataCache = new Dictionary<Guid, Wrapper.Common.AveTermSetInfo>();
        private Dictionary<Guid, Wrapper.Common.AveTermInfo> sourceTermDataCache = new Dictionary<Guid, Wrapper.Common.AveTermInfo>();
        private Dictionary<Guid, Guid> pinnedMapping = new Dictionary<Guid, Guid>();

        internal Wrapper.Common.AveTermStoreInfo SourceTermStoreInfo
        {
            get;
            set;
        }

        internal bool TryGetBackupTermGroupData(Guid sourceId, out Wrapper.Common.AveMetadataGroupInfo groupInfo)
        {
            SplitTermStoreInfoInToGroups();
            return sourceTermGroupDataCache.TryGetValue(sourceId, out groupInfo);
        }

        internal bool TryGetBackupTermSetData(Guid sourceId, out Wrapper.Common.AveTermSetInfo termSetInfo)
        {
            SplitTermGroupInfoInToTermSet();
            return sourceTermSetDataCache.TryGetValue(sourceId, out termSetInfo);
        }

        internal void AddRestoredDataMapping(MetadataCacheType type, Guid sourceId, Guid destinationId)
        {
            if (restoredMetaIDMappings.ContainsKey(type))
            {
                restoredMetaIDMappings[type][sourceId] = destinationId;
            }
            else
            {
                restoredMetaIDMappings[type] = new Dictionary<Guid, Guid> { { sourceId, destinationId } };
            }
        }

        internal bool TryGetRestoredMetaDataID(MetadataCacheType type, Guid sourceId, out Guid destinationId)
        {
            destinationId = Guid.Empty;
            if (restoredMetaIDMappings.ContainsKey(type))
            {
                return restoredMetaIDMappings[type].TryGetValue(sourceId, out destinationId);
            }
            return false;
        }

        private void SplitTermStoreInfoInToGroups()
        {
            if (SourceTermStoreInfo != null && SourceTermStoreInfo.Groups != null)
            {
                foreach (var groupInfo in SourceTermStoreInfo.Groups)
                {
                    if (!sourceTermGroupDataCache.ContainsKey(groupInfo.Id))
                    {
                        sourceTermGroupDataCache.Add(groupInfo.Id, groupInfo);
                    }
                }
                SourceTermStoreInfo.Groups = null;
            }
        }

        private void SplitTermGroupInfoInToTermSet()
        {
            SplitTermStoreInfoInToGroups();
            foreach (var groupInfo in sourceTermGroupDataCache.Values)
            {
                if (groupInfo.TermSets != null)
                {
                    foreach (var termSetInfo in groupInfo.TermSets)
                    {
                        if (!sourceTermSetDataCache.ContainsKey(termSetInfo.Id))
                        {
                            sourceTermSetDataCache.Add(termSetInfo.Id, termSetInfo);
                        }
                    }
                    groupInfo.TermSets = null;
                }
            }
        }


        internal void AddPinnedDataMapping(Guid sourceTermId, Guid parentId)
        {
            pinnedMapping[sourceTermId] = parentId;
        }

        internal List<Tuple<Guid, Guid,Guid>> GetPinnedMapping()
        {
            var result = new List<Tuple<Guid, Guid, Guid>>();
            foreach (var kv in pinnedMapping)
            {
                if (restoredMetaIDMappings[MetadataCacheType.TermSet].ContainsKey(kv.Value))
                {
                    result.Add(new Tuple<Guid, Guid, Guid>(restoredMetaIDMappings[MetadataCacheType.TermSet][kv.Key], kv.Value, Guid.Empty));
                }
                else if (restoredMetaIDMappings[MetadataCacheType.Term].ContainsKey(kv.Value))
                {
                    result.Add(new Tuple<Guid, Guid, Guid>(restoredMetaIDMappings[MetadataCacheType.Term][kv.Key], Guid.Empty, kv.Value));
                }
                else
                {
                    //log
                }
            }
            return result;
        }
    }

    public interface IImportMetadataManager
    {
        void AddSourceMetadataInfo(AvePoint.Wrapper.Common.AveTermStoreInfo termStoreInfo);

        void AddRestoredDataMapping(Guid sourceTermStoreId, MetadataCacheType type, Guid sourceId, Guid destinationId);

        bool TryGetRestoredMetaDataID(Guid sourceTermStoreId, MetadataCacheType type, Guid sourceId, out Guid destinationId);

        AvePoint.Wrapper.Common.AveTermStoreInfo TryGetTermStoreInfo(Guid sourceId);

        bool TryGetBackupTermGroupData(Guid sourceTermStoreId, Guid sourceId, out AvePoint.Wrapper.Common.AveMetadataGroupInfo termGroupInfo);

        bool TryGetBackupTermSetData(Guid sourceTermStoreId, Guid sourceId, out AvePoint.Wrapper.Common.AveTermSetInfo termSetInfo);

        void CachePinnedTerm(Guid SourceTermStoreId, Guid guid1, Guid guid2);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SourceTermStoreId"></param>
        /// <returns>pinnedTermid , termset id , parent term id</returns>
        List<Tuple<Guid, Guid,Guid>> GetPinnedTermMapping(Guid SourceTermStoreId);
    }

    class ImportMetadataManager : IImportMetadataManager
    {
        private Dictionary<Guid, TermStoreMappingManager> TermStoreManager = new Dictionary<Guid, TermStoreMappingManager>();

        public void AddRestoredDataMapping(Guid sourceTermStoreId, MetadataCacheType type, Guid sourceId, Guid destinationId)
        {
            lock (TermStoreManager)
            {
                if (!TermStoreManager.ContainsKey(sourceTermStoreId))
                {
                    TermStoreManager[sourceTermStoreId] = new TermStoreMappingManager();
                }
                TermStoreManager[sourceTermStoreId].AddRestoredDataMapping(type, sourceId, destinationId);
            }
        }

        /// <summary>
        /// 取不到时，会返回 Guid.Empty
        /// </summary>
        /// <param name="type"></param>
        /// <param name="destinationId"></param>
        /// <returns></returns>
        public bool TryGetRestoredMetaDataID(Guid sourceTermStoreId, MetadataCacheType type, Guid sourceId, out Guid destinationId)
        {
            destinationId = Guid.Empty;
            lock (TermStoreManager)
            {
                if (TermStoreManager.ContainsKey(sourceTermStoreId))
                {
                    return TermStoreManager[sourceTermStoreId].TryGetRestoredMetaDataID(type, sourceId, out destinationId);
                }
                else
                {
                    return false;
                }
            }
        }

        public void AddSourceMetadataInfo(Wrapper.Common.AveTermStoreInfo termStoreInfo)
        {
            lock (TermStoreManager)
            {
                TermStoreManager[termStoreInfo.Id] = new TermStoreMappingManager() { SourceTermStoreInfo = termStoreInfo };
            }
        }

        public AvePoint.Wrapper.Common.AveTermStoreInfo TryGetTermStoreInfo(Guid sourceTermStoreId)
        {
            lock (TermStoreManager)
            {
                if (TermStoreManager.ContainsKey(sourceTermStoreId))
                {
                    return TermStoreManager[sourceTermStoreId].SourceTermStoreInfo;
                }
            }
            return null;
        }

        public bool TryGetBackupTermGroupData(Guid termStoreId, Guid sourceId, out Wrapper.Common.AveMetadataGroupInfo termGroupInfo)
        {
            termGroupInfo = null;
            lock (TermStoreManager)
            {
                if (TermStoreManager.ContainsKey(termStoreId))
                {
                    return TermStoreManager[termStoreId].TryGetBackupTermGroupData(sourceId, out termGroupInfo);
                }
                else
                {
                    return false;
                }
            }
        }

        public bool TryGetBackupTermSetData(Guid termStoreId, Guid sourceId, out Wrapper.Common.AveTermSetInfo termSetInfo)
        {
            termSetInfo = null;
            lock (TermStoreManager)
            {
                if (TermStoreManager.ContainsKey(termStoreId))
                {
                    return TermStoreManager[termStoreId].TryGetBackupTermSetData(sourceId, out termSetInfo);
                }
                else
                {
                    return false;
                }
            }
        }



        public void CachePinnedTerm(Guid termStoreId, Guid sourceTermId, Guid parentId)
        {
            lock (TermStoreManager)
            {
                if (!TermStoreManager.ContainsKey(termStoreId))
                {
                    TermStoreManager[termStoreId] = new TermStoreMappingManager();
                }
                TermStoreManager[termStoreId].AddPinnedDataMapping(sourceTermId, parentId);
            }
        }
        public List<Tuple<Guid, Guid,Guid>> GetPinnedTermMapping(Guid termStoreId)
        {
            lock (TermStoreManager)
            {
                if (TermStoreManager.ContainsKey(termStoreId))
                {
                    return TermStoreManager[termStoreId].GetPinnedMapping();
                }
                else
                {
                    return null;
                }
            }

        }

    }

    class ImportUserProfileManager : IImportUserProfileManager
    {
        Dictionary<string, Tuple<string, string>> audienceMappings = new Dictionary<string, Tuple<string, string>>(StringComparer.OrdinalIgnoreCase);

        void IImportUserProfileManager.AddAudienceMapping(string sourceId, string currentId, string name)
        {
            lock (audienceMappings)
            {
                audienceMappings[sourceId] = new Tuple<string, string>(currentId, name);
            }
        }
    }

    class ImportPostActionManager : IImportPostActionManager
    {
        private static IAveLogger logger = AveLogger.GetInstance(typeof(ImportPostActionManager));

        private List<ISiteImportPostAction> sitePostActions = new List<ISiteImportPostAction>();
        private List<IWebImportPostAction> webPostActions = new List<IWebImportPostAction>();

        public void AddPostAction(ISiteImportPostAction postAction)
        {
            lock (sitePostActions)
            {
                sitePostActions.Add(postAction);
            }
        }

        public void AddPostAction(IWebImportPostAction postAction)
        {
            lock (webPostActions)
            {
                webPostActions.Add(postAction);
            }
        }

        public void ExecutePostActions(ISiteImport siteImport)
        {
            if (sitePostActions.Count > 0)
            {
                lock (sitePostActions)
                {
                    ExecutePostActions(sitePostActions, siteImport, null);
                    sitePostActions.Clear();
                }
            }
        }

        public void ExecutePostActions<T>(ISiteImport siteImport, ISPImportProfiler profiler)
        {
            if (sitePostActions.Count > 0)
            {
                var postActions = new List<ISiteImportPostAction>();
                lock (sitePostActions)
                {
                    for (int index = sitePostActions.Count - 1; index >= 0; index--)
                    {
                        var item = sitePostActions[index];

                        if (item is T)
                        {
                            postActions.Add(item);
                            sitePostActions.RemoveAt(index);
                        }
                    }
                }

                if (postActions.Count > 0)
                {
                    ExecutePostActions(postActions, siteImport, profiler);
                }
            }
        }

        private void ExecutePostActions(List<ISiteImportPostAction> postActions, ISiteImport siteImport, ISPImportProfiler profile)
        {
            foreach (var item in postActions)
            {
                using (item)
                {
                    try
                    {
                        item.Resolve(siteImport);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(WrapperResource.GetString(WrapperResourceKey.Wrapper_ExecutePostActionFailed, item.GetType().Name, ex));
                    }
                }
            }
        }

        private void ExecutePostActions(List<IWebImportPostAction> postActions, IWebImport webImport, ISPImportProfiler profile)
        {
            foreach (var item in postActions)
            {
                using (item)
                {
                    try
                    {
                        item.Resolve(webImport);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(WrapperResource.GetString(WrapperResourceKey.Wrapper_ExecutePostActionFailed, item.GetType().Name, ex));
                        AddPostAction(item as ISiteImportPostAction);
                    }
                }
            }
        }


        public void ExecutePostActions(IWebImport webImport)
        {
            if (webPostActions.Count > 0)
            {
                lock (webPostActions)
                {
                    ExecutePostActions(webPostActions, webImport, null);
                    webPostActions.Clear();
                }
            }
        }
    }

    /// <summary>
    /// TODO User Manager和Group Manager大致逻辑一致，如果有需求，可以整合
    /// </summary>
    class ImportUserManager : IImportUserManager
    {
        private Dictionary<int, Wrapper.Common.AveUserInfo> unRestoredUserInfo = new Dictionary<int, Wrapper.Common.AveUserInfo>();
        private List<string> fakeUsers = new List<string>();
        private Dictionary<int, Tuple<int, string>> restoredUserInfo = new Dictionary<int, Tuple<int, string>>();
        private int defaultUserId = -1;

        public void CacheUsers(List<Wrapper.Common.AveUserInfo> users)
        {
            lock (unRestoredUserInfo)
            {
                foreach (var item in users)
                {
                    unRestoredUserInfo[item.ID] = item;
                }
            }
        }

        public void RemoveUnRestoredUser(int userId)
        {
            lock (unRestoredUserInfo)
            {
                unRestoredUserInfo.Remove(userId);
            }
        }

        public bool TryGetRestoredUserId(int sourceUserId, out int userId)
        {
            userId = -1;
            lock (restoredUserInfo)
            {
                Tuple<int, string> user = null;
                if (restoredUserInfo.TryGetValue(sourceUserId, out user))
                {
                    userId = user.Item1;
                    return true;
                }
            }

            return false;
        }
        public bool TryGetRestoredUserLoginName(int sourceUserId, out string loginName)
        {
            loginName = string.Empty;
            lock (restoredUserInfo)
            {
                Tuple<int, string> user = null;
                if (restoredUserInfo.TryGetValue(sourceUserId, out user))
                {
                    loginName = user.Item2;
                    return true;
                }
            }
            return false;
        }

        public bool IsFakeUser(string userLogin)
        {
            lock (fakeUsers)
            {
                return fakeUsers.Contains(userLogin);
            }
        }

        public void AddFakeUser(string userLogin)
        {
            lock (fakeUsers)
            {
                if (!fakeUsers.Contains(userLogin))
                {
                    fakeUsers.Add(userLogin);
                }
            }
        }

        public void AddRestoredUserInfo(int sourceUserId, int destUserId, string destLoginName)
        {
            lock (restoredUserInfo)
            {
                restoredUserInfo[sourceUserId] = new Tuple<int, string>(destUserId, destLoginName);
            }
        }

        public bool TryGetUnRestoredUserInfo(int sourceUserId, out Wrapper.Common.AveUserInfo userInfo)
        {
            userInfo = null;

            lock (unRestoredUserInfo)
            {
                if (unRestoredUserInfo.TryGetValue(sourceUserId, out userInfo))
                {
                    return true;
                }
            }

            return false;
        }


        public int DefaultUserId
        {
            get
            {
                return defaultUserId;
            }
            set
            {
                defaultUserId = value;
            }
        }
    }

    class ImportGroupManager : IImportGroupManager
    {
        private Dictionary<int, Wrapper.Common.AveGroupInfo> unRestoredGroupInfo = new Dictionary<int, Wrapper.Common.AveGroupInfo>();
        private Dictionary<int, Tuple<int, string>> restoredGroupInfo = new Dictionary<int, Tuple<int, string>>();

        public void CacheGroups(List<Wrapper.Common.AveGroupInfo> groups)
        {
            lock (unRestoredGroupInfo)
            {
                foreach (var item in groups)
                {
                    unRestoredGroupInfo[item.ID] = item;
                }
            }
        }

        public void RemoveUnRestoredGroup(int id)
        {
            lock (unRestoredGroupInfo)
            {
                unRestoredGroupInfo.Remove(id);
            }
        }


        public bool TryGetRestoredGroup(int sourceGroupId, out int groupId)
        {
            groupId = -1;
            lock (restoredGroupInfo)
            {
                Tuple<int, string> group = null;
                if (restoredGroupInfo.TryGetValue(sourceGroupId, out group))
                {
                    groupId = group.Item1;
                    return true;
                }
            }

            return false;
        }

        public void AddRestoredGroupInfo(int sourceGroupId, int destGroupId, string destGroupName)
        {
            lock (restoredGroupInfo)
            {
                restoredGroupInfo[sourceGroupId] = new Tuple<int, string>(destGroupId, destGroupName);
            }
        }
    }

    class ImportObjectManager : IImportObjectManager
    {
        private readonly IImportGroupManager groupManager = new ImportGroupManager();
        private readonly IImportUserManager userManager = new ImportUserManager();
        private readonly IImportPostActionManager postActionManager = new ImportPostActionManager();
        private readonly IImportUserProfileManager userProfileManager = new ImportUserProfileManager();
        private readonly IImportMetadataManager metadataManager = new ImportMetadataManager();
        private readonly IImportSecurityManager securityManager = new ImportSecurityManager();

        public string ResolveUrl(string url)
        {
            throw new NotImplementedException();
        }

        public IUserMapping UserMapping { get; set; }

        public ILanguageMappingController LanguageMappingController { get; set; }

        public ILanguageMapping RootWebLanguageMapping { get; set; }

        public IImportUserManager UserManager { get { return userManager; } }

        public IImportGroupManager GroupManager { get { return groupManager; } }

        public IImportPostActionManager PostActionManager { get { return postActionManager; } }

        public IImportUserProfileManager UserProfileManager { get { return userProfileManager; } }

        public IImportSecurityManager SecurityManager { get { return securityManager; } }

        public IImportMetadataManager MetadataManager
        {
            get { return metadataManager; }
        }
    }

    public enum MetadataCacheType
    {
        TermStore,
        Group,
        TermSet,
        Term
    }

    internal class ImportSecurityManager : IImportSecurityManager
    {
         private Dictionary<int, Tuple<int, string>> restoredRoleInfo = new Dictionary<int, Tuple<int, string>>();
    
        public void AddRoleInfo(int sourceRoleId, int destRoleId,string roleName)
        {
            lock (restoredRoleInfo)
            {
                restoredRoleInfo[sourceRoleId] = new Tuple<int, string>(destRoleId, roleName);
            }
        }

        public bool TryGetRestoreRole(int sourceRoleId, out int roleId)
        {
            roleId = -1;
            lock (restoredRoleInfo)
            {
                Tuple<int, string> group = null;
                if (restoredRoleInfo.TryGetValue(sourceRoleId, out group))
                {
                    roleId = group.Item1;
                    return true;
                }
            }
            return false;
        }
    }
}
