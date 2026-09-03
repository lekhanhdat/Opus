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
using System.IO;
using System.Collections;
using System.Data;

namespace AvePoint.Wrapper.Common
{
    public interface IAveBackupRestoreQueryService : IAveQueryService
    {
        /// <summary>
        /// 设置SqlConnection的隔离级别
        /// </summary>
        /// <param name="level"></param>
        void SetIsolationLevel(IsolationLevel level);

        #region Backup
        /// <summary>
        /// 获取Site上所有User的信息
        /// </summary>
        /// <param name="site">所在Site</param>
        /// <param name="option">
        /// AveUserBackupOption.UserQueryOption.AllUsers: 所有用户，包括delete和deactivated的用户。
        /// AveUserBackupOption.UserQueryOption.AllAvailableUsers: 所有可用用户(不包括delete和deactivated用户)，包括没有权限的用户。
        /// AveUserBackupOption.UserQueryOption.OnlyHaveSecurityUsers：所有有权限的用户。
        /// </param>
        /// <returns></returns>
        List<AveUserInfo> GetSiteUsers(Guid siteID, AveUserBackupOption option);
       
        /// <summary>
        /// 获取Web上所有用户基础信息(只包含ID)
        /// </summary>
        /// <param name="web">所在Web，如果Web是继承权限则返回null</param>
        /// <param name="allAvailableUser">
        /// true: 所有用户，即所在Site上的全部用户
        /// false: 对当前Web有权限的用户
        /// </param>
        /// <returns></returns>
        List<AveUserInfo> GetWebUsers(IAveWeb web, bool allAvailableUser);

        /// <summary>
        /// 获取Web上所有组信息
        /// </summary>
        /// <param name="web">所在Web</param>
        /// <param name="allGroups">
        /// true: 所有组，即所在Site上的全部组
        /// false: 对当前Web有权限的组
        /// </param>
        /// <returns></returns>
        List<AveGroupInfo> GetGroups(IAveWeb web, bool allGroups);

        /// <summary>
        /// 获取Site Basic Setting Info
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        AveSiteSettingInfo GetSiteSettingFromSites(IAveSite site);

        /// <summary>
        /// 获取Site DiskUsed信息
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        long GetSiteSizeFromSites(IAveSite site);

        /// <summary>
        /// 获取Site Setting Info，包括：
        /// Site Basic Setting Info
        /// Solution Ids
        /// RootWeb metainfo
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        AveSiteSettingInfo GetSiteSettingAndMetaInfo(IAveSite site);

        string GetPageUrlById(Guid siteId, Guid pageId);

        string GetWebFullUrlById(Guid siteId, Guid webId);

        void GetSubWebsUrl(Guid siteId, Guid parentWebId, Dictionary<string, Dictionary<Guid, string>> infos);

        void GetListPagesUrl(Guid siteId, Guid listId, Dictionary<string, Dictionary<Guid, string>> infos);

        /// <summary>
        /// 获取Web Setting信息
        /// </summary>
        /// <param name="web">所在Web</param>
        /// <returns></returns>
        AveWebSettingInfo GetWebSettingFromWebs(IAveWeb web);

        /// <summary>
        /// 获取List Setting信息
        /// </summary>
        /// <param name="list">所在List</param>
        /// <returns></returns>
        AveListInfo GetListInfo(IAveList list);

        /// <summary>
        /// 获取List的Role Assignment
        /// </summary>
        /// <param name="SiteId"></param>
        /// <param name="ScopeId"></param>
        /// <returns></returns>
        List<AveRoleAssignmentInfo> GetListRoleAssignments(string SiteId, string ScopeId);

        /// <summary>
        /// 获取Item UserData信息。
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetUserData(AveBaseItemInfo info);
        
        /// <summary>
        /// 获取Item UserData信息。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="colNameCollection">
        /// 需要额外获取的column名字。
        /// ,int1,nvarchar1,bit4
        /// </param>
        /// <returns></returns>
        //todo:qlluo: colNameCollection最好和GetDocAndUserInfo,GetVersionAndUserInfo保持一致
        List<Dictionary<string, object>> GetUserData(AveBaseItemInfo info, string colNameCollection);

        /// <summary>
        /// 通过threadIndex获取所在Item的RowId
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="threadIndex"></param>
        /// <returns>item row id</returns>
        int GetParentIdByThreadIndex(Guid siteId, Guid listId, byte[] threadIndex);
        
        /// <summary>
        /// 获取Item ModifiedBy user id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="docId"></param>
        /// <returns></returns>
        int GetFileModifiedByIdByNative(Guid siteId, Guid parentId, Guid docId);
        
        /// <summary>
        /// 获取Item CreatedBy user id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="docId"></param>
        /// <returns></returns>
        int GetFileAuthorIdByNative(Guid siteId, Guid parentId, Guid docId);


        /// <summary>
        /// 获取Item的Role Assignment
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="itemScopeId"></param>
        /// <returns></returns>
        List<AveRoleAssignmentInfo> GetObjectRoleAssignments(Guid siteId, Guid itemScopeId);

        /// <summary>
        /// 获取field的IsRelationship
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="fieldId"></param>
        /// <returns>SPField.IsRelationship</returns>
        bool GetFieldCollectionRelationship(string siteId, string listId, string fieldId);

        /// <summary>
        /// 获取List的所有view schema信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        string GetListViewSchema(Guid siteId, Guid listId);

        /// <summary>
        /// 获取Web上的所有Role信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="FirstUniqueRoleDefinitionWebId"></param>
        /// <returns></returns>
        List<AveRoleInfo> GetWebRoles(Guid siteId, Guid FirstUniqueRoleDefinitionWebId);


        /// <summary>
        /// 获取Content Type的Resource文件
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="folderUrl">content type resource folder url</param>
        /// <returns></returns>
        List<AveContentTypeFileInfo> GetContentTypeCollectionResources(Guid siteId, string folderUrl);

        /// <summary>
        /// 通过Content Type Name获取Id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="contentTypeId"></param>
        /// <returns></returns>
        string GetContentTypeName(Guid siteId, byte[] contentTypeId);

        /// <summary>
        /// 获取Page上所有webpart信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="itemId">page url id</param>
        /// <param name="itemlevel"></param>
        /// <param name="itemIsVersion"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        List<AveWebPartBaseInfo> GetWebParts(Guid siteId, Guid itemId, byte itemlevel, bool itemIsVersion, int version);

        /// <summary>
        /// Obsolete，请使用string GetListTitle(Guid siteId, Guid listId);
        /// </summary>
        /// <param name="listId"></param>
        /// <returns></returns>
        [Obsolete("Please use the new method which includes two parameters.")]
        string GetListTitle(Guid listId);

        /// <summary>
        /// 获取List Title
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        string GetListTitle(Guid siteId, Guid listId);

        /// <summary>
        /// 获取webpart关联的Personalization表信息
        /// </summary>
        /// <param name="webPartInfo"></param>
        /// <param name="siteId"></param>
        /// <param name="itemId"></param>
        void SetWebPartPersonalization(AveWebPartBaseInfo webPartInfo, Guid siteId, Guid itemId);

        /// <summary>
        /// 获取webpart关联的WebPartLists表信息
        /// </summary>
        /// <param name="webPartInfo"></param>
        /// <param name="siteId"></param>
        /// <param name="itemId"></param>
        /// <param name="level"></param>
        void SetWebPartLists(AveWebPartBaseInfo webPartInfo, Guid siteId, Guid itemId, byte level);

        /// <summary>
        /// 获取Version的基本信息(AllDocs)
        /// </summary>
        /// <param name="itemInfo"></param>
        /// <param name="dataCache">返回值</param>
        void GetVersionInfo(AveBaseItemInfo itemInfo, Dictionary<string, object> dataCache);
        
        /// <summary>
        /// 获取Version的column信息(AllUserData)
        /// </summary>
        /// <param name="itemInfo"></param>
        /// <param name="dataCache">返回值</param>
        void GetListItemVersionInfo(AveBaseItemInfo itemInfo, Dictionary<string, object> dataCache);

        /// <summary>
        /// 判断文件时候包含Stream
        /// </summary>
        /// <param name="itemInfo"></param>
        /// <param name="internalVersion"></param>
        /// <returns></returns>
        bool GetDocHasStream(AveBaseItemInfo itemInfo, int internalVersion);

        /// <summary>
        /// 获取Attachment信息
        /// </summary>
        /// <param name="baseItemInfo"></param>
        /// <returns></returns>
        Dictionary<string, object> GetAttachmentInfo(AveBaseItemInfo baseItemInfo);

        /// <summary>
        /// 获取Item基础信息(AllDocs)
        /// </summary>
        /// <param name="itemInfo"></param>
        /// <param name="dataCache"></param>
        void GetDocInfo(AveBaseItemInfo itemInfo, Dictionary<string, object> dataCache);

        /// <summary>
        /// Query specific count row of DocData(AllDocs) And UserData(AllUserData)
        /// </summary>
        /// <param name="siteId">Query index, for performance</param>
        /// <param name="parentId">Parent folder id</param>
        /// <param name="currentDocLibRowId">Query entities which DocLibRowId is larger or equal than currentDocLibRowId</param>
        /// <param name="count">count of row to query</param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetDocAndUserInfo(Guid siteId, Guid parentId, int currentDocLibRowId, int count);
        /// <summary>
        /// Query specific count row of DocData(AllDocs) And UserData(AllUserData), SharePoint 2013及之后版本使用
        /// </summary>
        /// <param name="siteId">Query index, for performance</param>
        /// <param name="parentId">Parent folder id</param>
        /// <param name="currentDocLibRowId">Query entities which DocLibRowId is larger or equal than currentDocLibRowId</param>
        /// <param name="count">count of row to query</param>
        /// <param name="colNameCollection">
        /// 需要额外获取的column名字。
        /// int1,nvarchar1,bit4,
        /// </param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetDocAndUserInfo(Guid siteId, Guid parentId, int currentDocLibRowId, int count, string colNameCollection);
        /// <summary>
        /// Query specific count row of DocData(AllDocVersions) And UserData(AllUserData)
        /// </summary>
        /// <param name="siteId">Query index, for performance</param>
        /// <param name="parentId">Parent folder id</param>
        /// <param name="currentDocLibRowId">Query entities which DocLibRowId is larger or equal than currentDocLibRowId</param>
        /// <param name="count">count of row to query</param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetVersionAndUserInfo(Guid siteId, Guid parentId, int currentDocLibRowId, int count);
        /// <summary>
        /// Query specific count row of DocData(AllDocVersions) And UserData(AllUserData), SharePoint 2013及之后版本使用
        /// </summary>
        /// <param name="siteId">Query index, for performance</param>
        /// <param name="parentId">Parent folder id</param>
        /// <param name="currentDocLibRowId">Query entities which DocLibRowId is larger or equal than currentDocLibRowId</param>
        /// <param name="count">count of row to query</param>
        /// <param name="colNameCollection">
        /// 需要额外获取的column名字。
        /// int1,nvarchar1,bit4,
        /// </param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetVersionAndUserInfo(Guid siteId, Guid parentId, int currentDocLibRowId, int count, string colNameCollection);
        /// <summary>
        /// 获取Item的InternalVersion
        /// </summary>
        /// <param name="itemInfo"></param>
        /// <returns></returns>
        int GetInternalVersion(AveBaseItemInfo itemInfo);
        /// <summary>
        /// 获取Item的DocFlag
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        int GetDocFlag(AveBaseItemInfo info);

        /// <summary>
        /// 获取Stub文件的rbsId
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        byte[] GetRbsIdByNative(AveBaseItemInfo info);

        /// <summary>
        /// 获取Stub文件的rbsId集合，SharePoint 2013及以后版本使用
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        List<AveRBSStubInfo13> GetRbsIdListByNative(AveBaseItemInfo info);

        /// <summary>
        /// 获取Stub文件的StubInfo，存在Content表中的内容。
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="id"></param>
        /// <param name="internalVersion"></param>
        /// <returns></returns>
        string GetStubInfoByNative(Guid siteId, Guid id, int internalVersion);

        /// <summary>
        /// 已经过期，请使用 string GetFields(Guid siteId, Guid webId, Guid listId);
        /// </summary>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        [Obsolete("We should use another method which includes the site id in the parameters.")]
        string GetFields(Guid webId, Guid listId);

        /// <summary>
        /// 获取List Fields信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        string GetFields(Guid siteId, Guid webId, Guid listId);

        /// <summary>
        /// 获取List Default View Fields信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        string GetViewFields(Guid siteId, Guid listId);

        /// <summary>
        /// 已经过期，请使用 public int GetParentIdByThreadIndex(Guid siteId, Guid listId, byte[] threadIndex)
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="threadIndex"></param>
        /// <returns></returns>
        [Obsolete("请使用 public int GetParentIdByThreadIndex(Guid siteId, Guid listId, byte[] threadIndex)")]
        int GetThreadIndexParentId(Guid listId, byte[] threadIndex);

        /// <summary>
        /// 获取多值Lookup column的user data junction信息
        /// </summary>
        /// <param name="infoItem"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetUserDataJunction(AveBaseItemInfo infoItem);

        /// <summary>
        /// 批处理操作，获取当前folder下所有Item的user data junction信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId">parent folder id</param>
        /// <param name="maxRow">最大返回user data junction记录条数</param>
        /// <returns>[item.UniqueId,UIVersion,List[UserDataJunctionInfo]]</returns>
        //todo:qlluo: 三层Dictionary
        Dictionary<Guid, Dictionary<int, List<Dictionary<string, object>>>> GetFolderItemsUserDataJunctions(Guid siteId, Guid parentId, int maxRow);
        /// <summary>
        /// 批处理操作，获取当前folder下所有Item的user data junction信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId">parent folder id</param>
        /// <returns>[item.UniqueId,UIVersion,List[UserDataJunctionInfo]]</returns>
        Dictionary<Guid, Dictionary<int, List<Dictionary<string, object>>>> GetFolderItemsUserDataJunctions(Guid siteId, Guid parentId);

        /// <summary>
        /// 获取RBSStubInfo
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="blob_num"></param>
        /// <param name="blobStoreId"></param>
        /// <returns></returns>
        AveRBSStubInfo AveRBSBackup_BackupRBSStub(int collectionId, long blob_num, short blobStoreId);

        /// <summary>
        /// 获取RBS Blob Number
        /// </summary>
        /// <param name="rbs_id"></param>
        /// <returns></returns>
        long AveRBSBackup_GenerateBlobNumber(byte[] rbs_id);

        /// <summary>
        /// 插入一条Blob记录
        /// </summary>
        /// <param name="stubinfo"></param>
        /// <param name="collectionId"></param>
        /// <param name="blobStoreId"></param>
        /// <returns>blob number</returns>
        long AveRBSBackup_WriteBlobInformationToDB(AveRBSStubInfo stubinfo, int collectionId, short blobStoreId);

        /// <summary>
        /// 添加一个RBS Pool
        /// </summary>
        /// <param name="poolId"></param>
        /// <param name="canStoreNewBlobs"></param>
        /// <param name="collectionId"></param>
        /// <param name="blobStoreId"></param>
        void AveRBSExtenderRestore_CreatePool(byte[] poolId, bool canStoreNewBlobs, int collectionId, short blobStoreId);

        /// <summary>
        /// 获取CollectionId和ProviderId，对于DocAve ProviderName和CollectionName是固定的，因此没有传递参数
        /// </summary>
        /// <returns>
        /// int[0]=CollectionId
        /// int[1]=ProviderId</returns>
        int[] AveRBSCommon_GetCollectionIdAndProviderId();

        /// <summary>
        /// 获取所有Pool的Id集合
        /// </summary>
        /// <returns></returns>
        List<Guid> AveRBSCommon_GetPoolsOfDB();

        /// <summary>
        /// 插入一条Blob记录
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="blobStoreId"></param>
        /// <param name="storePoolId"></param>
        /// <param name="storeBlobId"></param>
        /// <param name="createTime"></param>
        /// <param name="blobSize"></param>
        /// <returns>blob number</returns>
        long AveRBSConnectorRestore_RegisterBlob(int collectionId, int blobStoreId, byte[] storePoolId, byte[] storeBlobId, DateTime createTime, long blobSize);


        /// <summary>
        /// 判断Blob是否存在
        /// </summary>
        /// <param name="storePoolId"></param>
        /// <param name="storeBlobId"></param>
        /// <param name="blobStoreId"></param>
        /// <param name="collectionId"></param>
        /// <param name="blobNumber"></param>
        /// <returns></returns>
        bool AveRBSConnectorRestore_CheckBlobExist(byte[] storePoolId, byte[] storeBlobId, int blobStoreId, int collectionId, ref long blobNumber);

        /// <summary>
        /// 获取Site下所有Web的模板信息
        /// </summary>
        /// <param name="site"></param>
        /// <param name="lcid"></param>
        /// <returns></returns>
        Dictionary<Guid, string> GetALLWebTemplates(IAveSite site, uint lcid);

        /// <summary>
        /// 获取checked out文件的checkout user id
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        int GetCheckOutUserId(AveBaseItemInfo info);

        /// <summary>
        /// 获取Item的所有Version列表
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        List<int> GetDocVersions(AveBaseItemInfo info);

        /// <summary>
        /// 获取Content Type的祖先链信息
        /// </summary>
        /// <param name="contentTypeInfo">当前Content type</param>
        /// <param name="siteId"></param>
        /// <param name="parentIdList">parent content type id集合，从父亲开始有序的集合</param>
        void GetParentContentTypeInfoTree(AveContentTypeInfo contentTypeInfo, Guid siteId, List<byte[]> parentIdList);

        /// <summary>
        /// 已经过期，请使用 string GetContentTypeContent(Guid siteId, Guid listId, Guid webId);
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="webId"></param>
        /// <returns></returns>
        [Obsolete("Please use the other method which includes siteId in the parameters.")]
        string GetContentTypeContent(Guid listId, Guid webId);

        /// <summary>
        /// 获取List所有Content Type Schema集合
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="webId"></param>
        /// <returns></returns>
        //todo:qlluo: 修改接口名
        string GetContentTypeSchema(Guid siteId, Guid listId, Guid webId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        AveContentTypeCollectionInfo GetContentTypeInfos(Guid siteId, string scope);

        /// <summary>
        /// 已经过期，请使用 void GetViews(Dictionary<Guid, List<AveViewInfo>> viewCache, Guid siteId, Guid listId, Guid defaultViewId);
        /// </summary>
        /// <param name="viewCache"></param>
        /// <param name="listId"></param>
        /// <param name="defaultViewId"></param>
        [Obsolete("Please use the other method which includes siteId in the parameters.")]
        void GetViews(Dictionary<Guid, List<AveViewInfo>> viewCache, Guid listId, Guid defaultViewId);

        /// <summary>
        /// 获取List View的集合
        /// </summary>
        /// <param name="viewCache">返回值</param>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="defaultViewId"></param>
        //todo:qlluo: 修改接口将Dictionary作为返回值
        void GetViews(Dictionary<Guid, List<AveViewInfo>> viewCache, Guid siteId, Guid listId, Guid defaultViewId);

        /// <summary>
        /// 获取Web fields schema集合，builtin并且没有修改过查不出来
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="scope">定义Web的Server Related Url</param>
        /// <returns></returns>
        List<string> GetFields(Guid siteId, string scope);

        /// <summary>
        /// 判断Content type是否存在
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="ctId"></param>
        /// <returns></returns>
        bool CheckContentTypeExist(Guid siteId, byte[] ctId);

        /// <summary>
        /// 判断Content type是否存在
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="scope">定义Web的Server Related Url</param>
        /// <param name="ctId"></param>
        /// <returns>
        /// true: 在scope或者scope的sub site下存在
        /// </returns>
        bool CheckIfContentTypeExistInChildren(Guid siteId, string scope, byte[] ctId);

        /// <summary>
        /// 删除webpart
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="docId"></param>
        /// <param name="webPartId"></param>
        void DeleteWebPartByNative(Guid siteId, Guid docId, Guid webPartId);

        /// <summary>
        /// 删除当前page上所有非personal view webpart的personal webpart，只删除当前page version上的(tp_PageVersion=0 and tp_level=@level)
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="docId"></param>
        /// <param name="level"></param>
        void DeleteAllPersonalWebParts(Guid siteId, Guid docId, int level, List<Guid> viewIds);

        /// <summary>
        /// 获取Web Content type
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="contentTypeId"></param>
        /// <returns></returns>
        string GetWebCTNameById(Guid siteId, string contentTypeId);

        /// <summary>
        /// 获取List Settings
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="listSettingInfo"></param>
        void GetListSettingInfoByNative(Guid siteId, Guid webId, Guid listId, AveListSettingInfo listSettingInfo);

        /// <summary>
        /// 获取立即执行的Alert
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="itemRowId">对于List级别的Alert传Guid.Empty</param>
        /// <param name="hostType"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetImmedSubscriptions(Guid siteId, Guid webId, Guid listId, int itemRowId, AveSPAlertHostType hostType);

        /// <summary>
        /// 获取周期执行的Alert
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="itemRowId">对于List级别的Alert传Guid.Empty</param>
        /// <param name="hostType"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetSchedSubscriptions(Guid siteId, Guid webId, Guid listId, int itemRowId, AveSPAlertHostType hostType);

        /// <summary>
        /// 获取附件的大小
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        int GetAttachmentSize(AveBaseItemInfo info);

        /// <summary>
        /// 获取GetFirstUniqueRoleDefinitionWeb的Id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="scopeId"></param>
        /// <returns></returns>
        Guid GetFirstUniqueRoleDefinitionWebGuid(Guid siteId, Guid scopeId);

        /// <summary>
        /// 获取特定User的RoleAssignment数量
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="scopeId"></param>
        /// <param name="roleId"></param>
        /// <param name="principalId"></param>
        /// <returns></returns>
        int GetRoleAssignmentCount(Guid siteId, Guid scopeId, int roleId, int principalId);

        /// <summary>
        /// 更新用户信息。包括：tp_SystemId,tp_Login,tp_Title,tp_Email
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="userId"></param>
        /// <param name="old"></param>
        /// <param name="displayField"></param>
        /// <param name="nameField"></param>
        /// <param name="eMailField"></param>
        void UpdateUserInfo(Guid siteId, Guid listId, int userId, AveUserInfo old, string displayField, string nameField, string eMailField);

        /// <summary>
        /// 获取特定Group的信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="principalId"></param>
        /// <returns></returns>
        AveGroupInfo GetGroupInfo(Guid siteId, int principalId);
        /// <summary>
        /// 获取特定User的信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="principalId"></param>
        /// <returns></returns>
        AveUserInfo GetUserInfo(Guid siteId, int principalId);
        /// <summary>
        /// 获取特定User的信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="principalId"></param>
        /// <param name="checkDeleted"></param>
        /// <returns></returns>
        AveUserInfo GetUserInfo(Guid siteId, int principalId, bool checkDeleted);

        /// <summary>
        /// 判断用户是否可用，可用表示Actived, Not Deleted, 有权限
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool CheckUserIfAvailable(Guid siteId, int userId);
        
        /// <summary>
        /// 已经过期，请使用 Guid GetListId(Guid siteId, Guid webId, string listTitle);
        /// </summary>
        /// <param name="webId"></param>
        /// <param name="listTitle"></param>
        /// <returns></returns>
        [Obsolete("Please use the other method which includes siteId in the parameters.")]
        Guid GetListId(Guid webId, string listTitle);

        /// <summary>
        /// 获取List Id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listTitle"></param>
        /// <returns></returns>
        Guid GetListId(Guid siteId, Guid webId, string listTitle);

        /// <summary>
        /// 更新Item的Author, Editor, Created, Modified, AppAuthor, AppEditor
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="author"></param>
        /// <param name="modified"></param>
        /// <param name="created"></param>
        /// <param name="info"></param>
        void UpdateSpecialPropertyByNative(string editor, string author, DateTime modified, DateTime created, AveBaseItemInfo info);

        /// <summary>
        /// 获取folder Unique Id
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        Guid GetFolderIdByName(AveBaseItemInfo info);

        /// <summary>
        /// 获取隐藏文件信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="folderId"></param>
        /// <returns></returns>
        List<AveHiddenFileInfo> GetHiddenFiles(Guid siteId, Guid webId, Guid listId, Guid folderId);

        /// <summary>
        /// 已经过期，请使用Guid GetListItemGuid(Guid siteId, Guid listId, int rowId);
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="rowId"></param>
        /// <returns></returns>
        [Obsolete("Please use another method which includes site id in the parameters.")]
        Guid GetListItemGuid(Guid listId, int rowId);

        /// <summary>
        /// 获取Item的tp_guid
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="rowId"></param>
        /// <returns>tp_guid</returns>
        Guid GetListItemGuid(Guid siteId, Guid listId, int rowId);
        
        /// <summary>
        /// 判断Attachment是否存在
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="leafName"></param>
        /// <returns></returns>
        bool IsAttachmentExist(Guid siteId, Guid parentId, string leafName);


        /// <summary>
        /// 获取当前Version的DocInfo信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="itemId"></param>
        Dictionary<string, object> GetCurrentVersionDocInfo(Guid siteId, Guid parentId, Guid itemId);

        /// <summary>
        /// 获取Attachment所在folder的Id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="attachmentRootFolderId"></param>
        /// <returns>
        /// Dictionary.Key,Attachment所在Item的RowId
        /// Dictionary.Value, Attachment所在Folder的UniqueId
        /// </returns>
        Dictionary<int, Guid> GetListAttachmentFolderIds(Guid siteId, Guid attachmentRootFolderId);

        /// <summary>
        /// 获取某Item下所有Attachment的名字
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="attachmentFolderId"></param>
        /// <returns></returns>
        List<string> GetAttachments(Guid siteId, Guid attachmentFolderId);

        /// <summary>
        /// 已经过期，请使用 Guid GetLookupGUIDById(Guid siteId, Guid lookupListId, int rowId);
        /// </summary>
        /// <param name="lookupListId"></param>
        /// <param name="rowId"></param>
        /// <returns></returns>
        [Obsolete("Please use another method which includes siteId in the parameters.")]
        Guid GetLookupGUIDById(Guid lookupListId, int rowId);

        /// <summary>
        /// 获取Item的tp_guid
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="lookupListId"></param>
        /// <param name="rowId"></param>
        /// <returns></returns>
        Guid GetLookupGUIDById(Guid siteId, Guid lookupListId, int rowId);

        /// <summary>
        /// 获取文件的Content
        /// </summary>
        /// <param name="info"></param>
        /// <param name="internalVersion"></param>
        /// <returns></returns>
        IAveQueryDataReader ExportContentByNative(AveBaseItemInfo info, int internalVersion);

        /// <summary>
        /// 获取文件的Shred信息，SharePoint 2013及以上版本可用
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        List<AveShredInfo> GetShredInfo(AveBaseItemInfo info);

        /// <summary>
        /// 获取一条Shred的RBS Id以及Content信息
        /// </summary>
        /// <param name="info"></param>
        /// <param name="shredInfo"></param>
        /// <returns></returns>
        IAveQueryDataReader GetRBSIdOrContentOfOneShred(AveBaseItemInfo info, AveShredInfo shredInfo);

        /// <summary>
        /// 判断文件是否是DocAve的EBS stub
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="Id"></param>
        /// <param name="internalVersion"></param>
        /// <returns></returns>
        bool CheckContentIfAveStub(Guid siteId, Guid Id, int internalVersion);

        /// <summary>
        /// 获取子Web的数量，
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="serverRelativeUrl"></param>
        /// <returns>所有后代的数量</returns>
        int GetSubWebCounts(Guid siteId, string serverRelativeUrl);
        
        /// <summary>
        /// 获取Scope Url
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="scopeId"></param>
        /// <returns></returns>
        string GetScopeUrl(Guid siteId, Guid scopeId);

        /// <summary>
        /// 获取Web的Last Accessed Day
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <returns></returns>
        DateTime GetLastAccessedDayOfWeb(Guid siteId, Guid webId);

        /// <summary>
        /// 获取Item的Content Type Id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="docId"></param>
        /// <param name="itemVersion"></param>
        /// <returns></returns>
        string GetItemContentTypeId(Guid siteId, Guid parentId, Guid docId, int itemVersion);

        /// <summary>
        /// 获取Navigation的Metainfo
        /// </summary>
        /// <param name="web"></param>
        /// <param name="Eid"></param>
        /// <returns></returns>
        string GetNavigationNodeMetainfo(IAveWeb web, int Eid);

        /// <summary>
        /// 获取Web的Feature集合
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        AveFeatureInfoBox GetFeatures(Guid siteId, Guid webId, AveFeatureScope scope);

        /// <summary>
        /// 获取AppAuthor或AppEditor的Id，取决于appPrincipalId的值
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="appPrincipalId"></param>
        /// <returns></returns>
        int GetAppAuthorAndAppEditor(Guid siteId, string appPrincipalId);

        #region RBSUtility

        /// <summary>
        /// 获取RBS Stub信息
        /// </summary>
        /// <param name="rbs_id"></param>
        /// <param name="blobStoreId"></param>
        /// <param name="collectionId"></param>
        /// <returns></returns>
        AveRBSStubInfo BackupRBSStub(byte[] rbs_id, short blobStoreId, int collectionId);

        #endregion

        #region Workflow
        //todo:qlluo: 返回List集合
        /// <summary>
        /// 获取Item上所有Workflow Id集合
        /// </summary>
        /// <param name="tempIds"></param>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="itemId"></param>
        /// <param name="listId"></param>
        void GetWorkflowId(List<Guid> tempIds, Guid siteId, Guid webId, int itemId, Guid listId);
        //todo:qlluo: 返回List集合
        /// <summary>
        /// 获取List上所有Workflow Association BaseId集合
        /// </summary>
        /// <param name="tempIds"></param>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        void GetWorkflowAssociationId(List<Guid> tempIds, Guid siteId, Guid webId, Guid listId);

        //todo:qlluo: 去掉使用guid的重载
        /// <summary>
        /// 获取Workflow status
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        AveWorkflowStatus GetWorkflowStatus(string workflowId);
        /// <summary>
        /// 获取Workflow status
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        AveWorkflowStatus GetWorkflowStatus(Guid workflowId);
        
        /// <summary>
        /// 获取List fields 的Schema Xml
        /// </summary>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        [Obsolete("We should use another method string GetFieldsSchemaXML(Guid webId, Guid listId)")]
        string GetFieldsSchemaXML(Guid webId, Guid listId);

        /// <summary>
        /// 获取Workflow instance的信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IAveQueryDataReader BackupInstance(Guid id);

        void BackupInstanceSelf(Guid siteId, Guid webId, Guid id, Hashtable properties, string customFieldProfix);

        /// <summary>
        /// 获取Scheduled work item信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        IAveQueryDataReader BackupScheduledWorkItem(Guid siteId, Guid instanceId);
        /// <summary>
        /// 获取workflow instance关联的Task Item信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="workflowInstanceId"></param>
        /// <returns></returns>
        IAveQueryDataReader BackupTasks(Guid siteId, Guid webId, Guid listId, Guid workflowInstanceId);

        /// <summary>
        /// 不知道是干毛用的
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="hostId"></param>
        /// <param name="contextCollectionId"></param>
        /// <returns></returns>
        IAveQueryDataReader BackupTaskItemEvents(Guid siteId, Guid webId, Guid hostId, byte[] contextCollectionId);

        /// <summary>
        /// 不知道是干毛用的
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="hostId"></param>
        /// <param name="contextCollectionId"></param>
        /// <returns></returns>
        IAveQueryDataReader BackupInstanceParentItemEvents(Guid siteId, Guid webId, Guid hostId, byte[] contextCollectionId);

        /// <summary>
        /// 不知道是干毛用的
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="contextCollectionId"></param>
        /// <returns></returns>
        IAveQueryDataReader BackupInstanceEvents(Guid siteId, Guid webId, byte[] contextCollectionId);

        /// <summary>
        /// 获取workflow instance关联的History Item信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="historyListId"></param>
        /// <param name="workflowInstanceId"></param>
        /// <param name="instanceIdColName"></param>
        /// <returns></returns>
        IAveQueryDataReader BackupHistory(Guid siteId, Guid webId, Guid historyListId, Guid workflowInstanceId, string instanceIdColName);

        #endregion

        #endregion

        #region Restore
        /// <summary>
        /// 给Item涨Version
        /// </summary>
        /// <param name="originalVersion"></param>
        /// <param name="siteId"></param>
        /// <param name="uniqueId"></param>
        /// <param name="version"></param>
        /// <param name="rowId"></param>
        /// <param name="parentFolderId"></param>
        void IncreaseVersionByNative(int originalVersion, Guid siteId, Guid uniqueId, int version, int rowId, Guid parentFolderId);
        
        /// <summary>
        /// 已经过期，请使用int GetTpIdByTpGuid(Guid siteId, Guid tp_guid, Guid listId);
        /// </summary>
        /// <param name="tp_guid"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        [Obsolete("Please use the other method which includes siteId in parameters.")]
        int GetTpIdByTpGuid(Guid tp_guid, Guid listId);

        /// <summary>
        /// 获取Item RowId
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="tp_guid"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        int GetTpIdByTpGuid(Guid siteId, Guid tp_guid, Guid listId);

        /// <summary>
        /// 插入一条记录到AllUserDataJunction中
        /// </summary>
        /// <param name="item"></param>
        /// <param name="fieldId"></param>
        /// <param name="sourceListId"></param>
        /// <param name="id"></param>
        /// <param name="ordinal"></param>
        /// <param name="version"></param>
        void InsertIntoAllUserDataJunction(IAveListItem item, Guid fieldId, Guid sourceListId, int id, int ordinal, int version);

        /// <summary>
        /// 已经过期，请使用  void UpdateWebsAuthorByNative(int userId, Guid siteId, Guid webId);
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="webId"></param>
        [Obsolete("Please use another method which contains siteId in the parameters.")]        
        void UpdateWebsAuthorByNative(int userId, Guid webId);

        /// <summary>
        /// 更新Web的Author
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        void UpdateWebsAuthorByNative(int userId, Guid siteId, Guid webId);

        /// <summary>
        /// 更新column value值
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="item"></param>
        /// <param name="version"></param>
        /// <param name="rowOrdinal"></param>
        /// <param name="colName"></param>
        /// <param name="colValue"></param>
        void UpdateColumnByNative(Guid siteId, IAveListItem item, int version, int rowOrdinal, string colName, object colValue);

        /// <summary>
        /// 从回收站中删除Item
        /// remove item by tp_guid, only for a ListItem
        /// </summary>
        /// <param name="mQueryWorker"></param>
        /// <param name="spSite"></param>
        /// <param name="parentId"></param>
        /// <param name="tp_Guid"></param>
        void RemoveListItemInRecycleBin(IAveSite site, Guid parentId, Guid tp_Guid);

        /// <summary>
        /// 从回收站里面删除Item
        /// </summary>
        /// <param name="site"></param>
        /// <param name="parentId"></param>
        /// <param name="name"></param>
        void RemoveItemInRecycleBin(IAveSite site, Guid parentId, string name);

        /// <summary>
        /// 修改Alldocs中对应的TimeCreated和TimeLastModified字段。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="timeCreated"></param>
        /// <param name="timeLastModified"></param>
        /// <param name="version"></param>
        void UpdateAllDocsPropertyByNative(AveBaseItemInfo info, DateTime timeCreated, DateTime timeLastModified, int version);

        /// <summary>
        /// 创建一个Item的Version
        /// 用于ListItem插version
        /// </summary>
        /// <param name="info"></param>
        /// <param name="version"></param>
        /// <param name="restoringDto"></param>
        /// <returns></returns>
        bool CreateVersionByNative(AveBaseItemInfo info, int version, RestoringDto restoringDto);

        /// <summary>
        /// 已经过期，请使用 void ChangeNextItemId(int toId, Guid siteId, Guid listId)
        /// </summary>
        /// <param name="toId"></param>
        /// <param name="listId"></param>
        [Obsolete("Please use another method which contains siteId in the parameters.")]
        void ChangeNextItemId(int toId, Guid listId);

        /// <summary>
        /// 获取List中下一个可用Row Id
        /// </summary>
        /// <param name="toId"></param>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        void ChangeNextItemId(int toId, Guid siteId, Guid listId);
        /// <summary>
        /// 修改Item的Row Id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="id"></param>
        /// <param name="rootFolderId"></param>
        /// <param name="itemType">
        /// itemType=1, list item
        /// itemType=2, document
        /// itemType=3, folder
        /// </param>
        /// <param name="fromId"></param>
        /// <param name="toId"></param>
        /// <param name="mQueryWorker"></param>
        /// <returns></returns>
        int ChangeItemId(
           Guid siteId,
           Guid id,
           Guid rootFolderId,
           int itemType,
           int fromId,
           int toId);

        /// <summary>
        /// 修改Item的Row Id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="parentId"></param>
        /// <param name="id"></param>
        /// <param name="fromId"></param>
        /// <param name="toId"></param>
        /// <returns></returns>
        int ChangeItemId(Guid siteId, Guid listId, Guid parentId, Guid id, int fromId, int toId);
        /// <summary>
        /// 判断list下itemId是否被占用,没被占用返回true。
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        bool CheckItemIdAvailable(Guid siteId, Guid listId, int itemId);

        /// <summary>
        /// 已经过期，请使用 DateTime CheckItemIdAvailableAndGetModifiedTimeForAppend(Guid siteId, Guid listId, int itemId);
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        [Obsolete("Please use another method which contains site id in the parameters.")]
        DateTime CheckItemIdAvailableAndGetModifiedTimeForAppend(Guid listId, int itemId);
        
        /// <summary>
        /// 获取Item最大version的Modified Time, 不存在返回DateTime.MinValue
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        DateTime CheckItemIdAvailableAndGetModifiedTimeForAppend(Guid siteId, Guid listId, int itemId);

        /// <summary>
        /// 更新Version信息
        /// </summary>
        /// <param name="info"></param>
        /// <param name="restoringDto"></param>
        /// <param name="allDocData"></param>
        /// <param name="allUserData"></param>
        /// <param name="version"></param>
        void UpdateVersionByNative(AveBaseItemInfo info, RestoringDto restoringDto, Dictionary<string, object> allDocData, Dictionary<string, object> allUserData, int version);

        /// <summary>
        /// 修改Item的Level
        /// </summary>
        /// <param name="info"></param>
        /// <param name="item"></param>
        /// <param name="version"></param>
        /// <param name="originaleLevel"></param>
        /// <param name="draftOwnerId"></param>
        void ChangeLevelByNative(AveBaseItemInfo info, IAveListItem item, int version, int originaleLevel, int draftOwnerId);
        
        //TODO:Combine it with the function that with the same name but belong to aveDoc
        /// <summary>
        /// 修改Checkout文件的Checkout user
        /// </summary>
        /// <param name="info"></param>
        /// <param name="uniqueID"></param>
        /// <param name="newUserID"></param>
        void ChangeCheckoutUserID(AveBaseItemInfo info, Guid uniqueID, int newUserID);

        /// <summary>
        /// 修改Checkout文件的Checkout user.  Use ChangeCheckoutUserID(Guid siteId, Guid uniqueID, int newUserID) method if can get parentId.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="uniqueID"></param>
        /// <param name="newUserID"></param>
        void ChangeCheckoutUserID(Guid siteId, Guid uniqueID, int newUserID);

        /// <summary>
        /// 修改Checkout文件的Checkout user
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="uniqueID"></param>
        /// <param name="parentId"></param>
        /// <param name="newUserID"></param>
        void ChangeCheckoutUserID(Guid siteId, Guid uniqueID,Guid parentId, int newUserID);

        // TODO:Add User Mapping
        // TODO:make this an the same function in AveSPItem one function
        void ChangeCheckoutUserIDForAllVersion(Guid siteId, Guid parentId, Guid fileId, int newUserID);

        /// <summary>
        /// 用来统一使用Native方法更改AllDocs
        /// </summary>
        /// <param name="info">SiteId,ParentId,Level需要初始化</param>
        /// <param name="uniqueId"></param>
        /// <param name="docdataObjects"></param>
        void ChangeUserdataByNative(AveBaseItemInfo info, Guid uniqueId, Dictionary<string, object> userdata);

        /// <summary>
        /// 已经过期，请使用int GetCurrentUIVersion(Guid siteId, Guid parentId, Guid id);
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [Obsolete("Please use GetCurrentUIVersion(Guid siteId, Guid parentId, Guid id) for proformance")]
        int GetCurrentUIVersion(Guid siteId, Guid id);

        /// <summary>
        /// 获取Item的Current UIVersion
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        int GetCurrentUIVersion(Guid siteId, Guid parentId, Guid id);
        
        /// <summary>
        /// 获取Item的Last Modified Time
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="docLibRowId"></param>
        /// <returns></returns>
        DateTime GetLastModifiedByNative(Guid siteId, Guid listId, int docLibRowId, bool onlyPublishVersion);
        /// <summary>
        /// 把文件移动到Folder中
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="parentFolderServerRelativeUrl"></param>
        /// <param name="listItemName"></param>
        /// <param name="docLibRowId"></param>
        /// <param name="listItemUniqueId"></param>
        /// <param name="conflictFolderUniqueId"></param>
        /// <param name="conflictFolderName"></param>
        /// <param name="lastModified"></param>
        /// <param name="isSourceWin"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        bool MoveDocToConflictFolderByNative(Guid listId, string parentFolderServerRelativeUrl, string listItemName, int docLibRowId, Guid listItemUniqueId, Guid conflictFolderUniqueId, string conflictFolderName, DateTime lastModified, bool isSourceWin, Guid siteId);
        /// <summary>
        /// 把List Item移动到folder中
        /// </summary>
        /// <param name="titleColName"></param>
        /// <param name="parentFolderServerRelativeUrl"></param>
        /// <param name="conflictFolderName"></param>
        /// <param name="conflictFolderlistId"></param>
        /// <param name="conflictFolderUniqueId"></param>
        /// <param name="docLibRowId"></param>
        /// <param name="listItemUniqueId"></param>
        /// <param name="lastModified"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        bool MoveListItemToConflictFolderByNative(string titleColName, string parentFolderServerRelativeUrl, string conflictFolderName, Guid conflictFolderlistId, Guid conflictFolderUniqueId, int docLibRowId, Guid listItemUniqueId, DateTime lastModified, Guid siteId);

        #region modify for pic slide image thumbnail
        /// <summary>
        /// 获取Item的tp_Author, tp_Editor, tp_Created, tp_Modified, TimeCreated, TimeLastModified
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="itemId"></param>
        /// <param name="uiversion"></param>
        /// <param name="level"></param>
        /// <param name="author"></param>
        /// <param name="editor"></param>
        /// <param name="tp_create"></param>
        /// <param name="tp_modify"></param>
        /// <param name="create"></param>
        /// <param name="modify"></param>
        /// <returns></returns>
        //todo:qlluo:接口: 方法名不合理，可以查询所有Item不只是Thumbnail
        
        bool QueryBasicInfoForThumbNail(Guid siteId, Guid parentId, Guid itemId, int uiversion, int level, AveBasicItemInfo basicItemInfo);
        /// <summary>
        /// 更新Item的tp_Author, tp_Editor, tp_Created, tp_Modified, TimeCreated, TimeLastModified
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="itemId"></param>
        /// <param name="uiversion"></param>
        /// <param name="level"></param>
        /// <param name="author"></param>
        /// <param name="editor"></param>
        /// <param name="tp_create"></param>
        /// <param name="tp_modify"></param>
        /// <param name="create"></param>
        /// <param name="modify"></param>
        //todo:qlluo:接口: 方法名不合理，可以更新所有Item不只是Thumbnail
        
        void UpdateBasicInfoForThumbNail(Guid siteId, Guid parentId, Guid itemId, int uiversion, int level, AveBasicItemInfo basicItemInfo);
        #endregion

        /// <summary>
        /// 获取Item的Internal Version, SharePoint 2016不支持
        /// </summary>
        /// <param name="info"></param>
        /// <param name="isVersion"></param>
        /// <param name="id"></param>
        /// <param name="UIVersion"></param>
        /// <returns></returns>
        int? GetInternalVersion(AveBaseItemInfo info, bool isVersion, Guid id, int UIVersion);

        /// <summary>
        /// 判断文件是否被checkout
        /// </summary>
        /// <param name="info"></param>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        bool IsCheckOutFile(AveBaseItemInfo info, Guid siteId, Guid parentId, string name);
        /// <summary>
        /// 判断文件是否被checkout
        /// </summary>
        /// <param name="info"></param>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        bool IsCheckOutFile(Guid siteId, Guid fileId, ref int checkId);
        /// <summary>
        /// 判断文件是为checkout version
        /// </summary>
        /// <param name="info"></param>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        bool IsCheckOutVersion(Guid siteId, Guid fileId, int uiVersion, ref int checkId);
        /// <summary>
        /// 判断文件是否被checkout
        /// </summary>
        /// <param name="info"></param>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        bool IsCheckOutFile(Guid siteId, string url, ref int checkId);
        /// <summary>
        /// 判断文件是否被checkout
        /// </summary>
        /// <param name="info"></param>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        bool IsCheckOutFile(Guid siteId, Guid listId, int fileId, out int checkId, out Guid id);

        //for form library item, to change content.
        [Obsolete("We don't support to update content by native method.")]
        void ChangeContentByNative(AveBaseItemInfo info, byte[] content);

        /// <summary>
        /// 获取文件的Level信息
        /// </summary>
        /// <param name="info"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        byte GetLevel(AveBaseItemInfo info, int version);

        /// <summary>
        /// 更新Internal version
        /// </summary>
        /// <param name="info"></param>
        /// <param name="restoringDto"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        int SetInternalVersion(AveBaseItemInfo info, RestoringDto restoringDto, int version);

        /// <summary>
        /// 更新checkin comment
        /// checkin comment可以通过API改的，但是这个主要是为了改Approve Comment，因为Approve comment和Checkin Comment是同一个。
        /// </summary>
        /// <param name="checkinComment"></param>
        void UpdateCheckinCommentByNative(AveBaseItemInfo info, Guid fileGuid, string checkinComment);

        /// <summary>
        /// 更新View的last modified time
        /// </summary>
        /// <param name="info"></param>
        /// <param name="spFile"></param>
        /// <param name="timeLastModified"></param>
        void UpdateViewLastModifiedTimeByNative(AveBaseItemInfo info, IAveFile spFile, DateTime timeLastModified);

        void ResetContentByNative(AveSPItemNativeInfo info);

        /// <summary>
        /// 获取List下Item的最大Row Id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        int GetMaxListItemRowId(Guid siteId, Guid listId);

        /// <summary>
        /// 获取List下Item的最大LeafName
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        string GetMaxSubLeafName(Guid siteId, Guid parentId);
        
        /// <summary>
        /// List Item冲突处理, row id作为条件
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="restoringDto"></param>
        void CheckConflictInfoForListItem(Guid siteId, Guid listId, RestoringDto restoringDto);

        /// <summary>
        /// 文件的冲突处理，LeafName作条件
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="restoringDto"></param>
        //todo:qlluo:接口: ForDocument
        void CheckConflictInfo(Guid siteId, Guid parentId, RestoringDto restoringDto);

        [Obsolete("Please use another method which contains listid in the parameters.")]
        void CheckConflictInfo(Guid siteId, Guid parentId, Guid tp_Guid, RestoringDto restoringDto);

        /// <summary>
        /// List Item冲突处理, tp_guid作为条件
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="parentId"></param>
        /// <param name="tp_Guid"></param>
        /// <param name="restoringDto"></param>
        void CheckConflictInfo(Guid siteId, Guid listId, Guid parentId, Guid tp_Guid, RestoringDto restoringDto);

        void CheckConflictInfoBySpecialColumn(Guid siteId, Guid parentId, object columnValue, string fieldColumn, RestoringDto restoringDto);

        /// <summary>
        /// 更新文件内容
        /// </summary>
        /// <param name="info"></param>
        /// <param name="fs"></param>
        void UpdateFileContentByNative(AveSPItemNativeInfo info, Stream fs);

        /// <summary>
        /// <summary>        /// 获取Item的Editor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        int GetItemEditorByNative(AveBaseItemInfo info, IAveListItem item);

        /// <summary>
        /// 修改Item的Editor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="item"></param>
        /// <param name="modified"></param>
        void SetItemEditorByNative(AveBaseItemInfo info, IAveListItem item, int modified);

        /// <summary>
        /// 给附件重命名
        /// </summary>
        /// <param name="info"></param>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        void RenameAttachment(AveBaseItemInfo info, string oldName, string newName);

        /// <summary>
        /// 获取附件的Unique Id
        /// </summary>
        /// <param name="info"></param>
        /// <param name="realName"></param>
        /// <returns></returns>
        Guid GetAttachmentUniqueId(AveBaseItemInfo info, string realName);

        /// <summary>
        /// 获取附件的UIVersion
        /// </summary>
        /// <param name="info"></param>
        /// <param name="realName"></param>
        /// <returns></returns>
        int GetAttachmentVersion(AveBaseItemInfo info, string realName);


       /// <summary>
       /// 更新webpart Id
       /// </summary>
       /// <param name="oldId"></param>
       /// <param name="siteId"></param>
       /// <param name="fileId"></param>
       /// <param name="newId"></param>
        void UpdateWebPartInfo(Guid oldId, Guid siteId, Guid fileId, Guid newId);

        byte[] GetIListWebPartView(Guid siteId, Guid fileId, Guid webPartId);

        void SetIListWebPartView(Guid siteId, Guid fileId, Guid webPartId, byte[] view);

        /// <summary>
        /// 更新webpart的Properties
        /// </summary>
        /// <param name="webPartId"></param>
        /// <param name="siteId"></param>
        /// <param name="fileId"></param>
        /// <param name="allUsersProperties"></param>
        /// <param name="perUserProperties"></param>
        void UpdateWebpartPropertiesByNative(Guid webPartId, Guid siteId, Guid fileId, byte[] allUsersProperties, byte[] perUserProperties);

        /// <summary>
        /// 更新WebPart的tp_Level,tp_PageVersion等信息
        /// </summary>
        /// <param name="webPartId"></param>
        /// <param name="siteId"></param>
        /// <param name="fileId"></param>
        /// <param name="pageVersion"></param>
        /// <param name="oldLevel"></param>
        /// <param name="newLevel"></param>
        /// <param name="isCurrentVersion"></param>
        /// <param name="uIVersion"></param>
        void UpdateWebPartInfo(Guid webPartId, Guid siteId, Guid fileId, int pageVersion, byte oldLevel, byte newLevel, bool isCurrentVersion, int uIVersion);

        /// <summary>
        /// 更新webpart的view信息
        /// </summary>
        /// <param name="webPartId"></param>
        /// <param name="siteId"></param>
        /// <param name="fileId"></param>
        /// <param name="baseViewId"></param>
        /// <param name="view"></param>
        /// <param name="contentTypeId"></param>
        /// <param name="displayName"></param>
        void UpdateView(Guid webPartId, Guid siteId, Guid fileId, int baseViewId, byte[] view, byte[] contentTypeId, string displayName);

        /// <summary>
        /// 更新webpart的user id
        /// </summary>
        /// <param name="webPartId"></param>
        /// <param name="siteId"></param>
        /// <param name="fileId"></param>
        /// <param name="currentUserId"></param>
        /// <param name="userId"></param>
        /// <param name="isPersonal"></param>
        void UpdateWebPartUserID(Guid webPartId, Guid siteId, Guid fileId, int currentUserId, int userId, bool isPersonal);

        /// <summary>
        /// 更新Item的tp_guid
        /// </summary>
        /// <param name="tpGuid"></param>
        /// <param name="itemUniqueId"></param>
        /// <param name="parentUniqueId"></param>
        /// <param name="siteId"></param>
        /// <param name="isCurrentVersion"></param>
        /// <param name="level"></param>
        /// <param name="calculatedVersion"></param>
        void UpdateItemGuid(Guid tpGuid, Guid itemUniqueId, Guid parentUniqueId, Guid siteId, bool isCurrentVersion, byte level, int calculatedVersion);

        /// <summary>
        /// 更新personal webpart的属性值
        /// </summary>
        /// <param name="webPartId"></param>
        /// <param name="siteId"></param>
        /// <param name="currentUserId"></param>
        /// <param name="perUserBytes"></param>
        void UpdatePersonalPropertiesByNative(Guid webPartId, Guid siteId, int currentUserId, byte[] perUserBytes);

        [Obsolete("Please use another method GetTpIdByTpGuid instead.")]
        int GetLookupIdByGUID(Guid lookupListId, Guid GUID);

        /// <summary>
        /// 通过tp_guid查找row id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="lookupListId"></param>
        /// <param name="GUID"></param>
        /// <returns></returns>
        int GetLookupIdByGUID(Guid siteId, Guid lookupListId, Guid GUID);

        /// <summary>
        /// 获取所有web的guid
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        List<Guid> GetAllWebsGuidByNative(Guid siteId);

        /// <summary>
        /// 获取所有web的size集合
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        Dictionary<Guid, long> GetAllWebSize(IAveSite site);

        /// <summary>
        /// 删除一条AllUserDataJuncations表记录
        /// </summary>
        /// <param name="item"></param>
        /// <param name="fieldId"></param>
        /// <param name="sourceListId"></param>
        /// <param name="version"></param>
        void RemoveDataJunctionByNative(IAveListItem item, Guid fieldId, Guid sourceListId, int version);

        /// <summary>
        /// 获取Folder的UniqueId
        /// </summary>
        /// <param name="name"></param>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        Guid GetFolderIdByName(string name, Guid siteId, Guid parentId);

        /// <summary>
        /// 根据DirName, leafName 查询 Item UniqueId
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="leafName"></param>
        /// <param name="dirName"></param>
        /// <returns></returns>
        Guid GetItemIdByName(Guid siteId, Guid webId, string leafName, string dirName);

        /// <summary>
        /// 生成Web Id Mapping
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webSettingInfo"></param>
        /// <param name="siteManagedMappings"></param>
        /// <param name="sourceSiteInfo"></param>
        /// <param name="destSiteUrl"></param>
        /// <param name="webIdMapping"></param>
        /// <returns></returns>
        Dictionary<Guid, Guid> ReloadHiddenWebProperty(Guid siteId, AveWebSettingInfo webSettingInfo, List<Dictionary<string, string>> siteManagedMappings, AveSiteInfo sourceSiteInfo, string destSiteUrl, Dictionary<Guid, Guid> webIdMapping);

        [Obsolete("Please use another method which contains siteid in the parameters.")]
        bool IsConflictWithRecycle(string name, Guid webId);

        /// <summary>
        /// Web在回收站中是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <returns></returns>
        bool IsConflictWithRecycle(string name, Guid siteId, Guid webId);

        /// <summary>
        /// Web在回收站中是否存在
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webUrl"></param>
        /// <returns></returns>
        bool IsConflictWithRecycle(Guid siteId, string webUrl);

        /// <summary>
        /// 更新Item的TimeCreated和TimeLastModified
        /// </summary>
        /// <param name="timeCreated"></param>
        /// <param name="timeLastModified"></param>
        /// <param name="parentId"></param>
        /// <param name="siteId"></param>
        /// <param name="leafName"></param>
        void UpdateAllDocsPropertyByNative(DateTime timeCreated, DateTime timeLastModified, Guid parentId, Guid siteId, string leafName);

        [Obsolete("Please use another method which contains site id in the parameters.")]
        int GetNextAvailableId(Guid listId);

        /// <summary>
        /// 获取List的下一个可用Row Id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        int GetNextAvailableId(Guid siteId, Guid listId);

        /// <summary>
        /// 获取Web Id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        Guid GetWebId(Guid siteId, string url);
        /// <summary>
        /// 获取ContentType的User Resource
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        Dictionary<string, Dictionary<string, Dictionary<int, string>>> GetContentTypeResource(Guid siteId, Guid webId, Guid listId);

        /// <summary>
        /// 获取Web的Alert
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <returns></returns>
        IAveQueryDataReader GetWebAlerts(Guid siteId, Guid webId);

        /// <summary>
        /// 判断List是否存在Alert
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        bool ListHasLerts(Guid siteId, Guid listId);

        /// <summary>
        /// 获取Version的Modified，AllUserData
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="rowId"></param>
        /// <param name="uiVersion"></param>
        /// <returns></returns>
        DateTime GetVersionModified(Guid siteId, Guid parentId, int rowId, int uiVersion);

        /// <summary>
        /// 判断Field在Web上是否存在
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="siteId"></param>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        bool GetFieldInSiteChildren(string scope, Guid siteId, Guid fieldId);

        /// <summary>
        /// Item是否被删除
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="id"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        bool IsItemExist(Guid listId, int id, Guid siteId);

        #region RBS Utility

        /// <summary>
        /// 获取RBS CollectionId和Provider Id
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        int[] GetCollectionIdAndProviderId(Guid siteId);

        /// <summary>
        /// 写入一条stub记录
        /// </summary>
        /// <param name="stubinfo"></param>
        /// <param name="poolsOfDB"></param>
        /// <param name="blobStoreId"></param>
        /// <param name="collectionId"></param>
        /// <returns></returns>
        byte[] RestoreRBSStub(AveRBSStubInfo stubinfo, List<Guid> poolsOfDB, short blobStoreId, int collectionId);

        /// <summary>
        /// 删除一条DocsToStreams和DocStreams记录, SharePoint 2013及以后版本使用
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="DocId"></param>
        /// <param name="HistVersion"></param>
        /// <param name="level"></param>
        /// <param name="clearDocStreams">是否清除DocStreams表</param>
        void ClearDocsToStreamsAndDocStreams(Guid siteId, Guid DocId, int HistVersion, byte level, bool clearDocStreams);


        #endregion

        #region Workflow

        /// <summary>
        /// 删除特定event receiver
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="hostId"></param>
        /// <param name="contextCollectionId"></param>
        /// <param name="sequenceNumber"></param>
        void DeleteSpecificEventFromEventReceiver(Guid siteId, Guid webId, Guid hostId, byte[] contextCollectionId, object sequenceNumber);

        /// <summary>
        /// 插入一条记录
        /// </summary>
        /// <param name="data"></param>
        /// <param name="tableName"></param>
        void InsertTableRow(Hashtable data, string tableName);

        /// <summary>
        /// 更新workflow status的值
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="tpGuid"></param>
        /// <param name="tpId"></param>
        /// <param name="StatusFieldValue"></param>
        /// <param name="rowOrdinal"></param>
        /// <param name="statusField"></param>
        void UpdateWorkflowStatusFieldValue(Guid siteId, Guid listId, Guid tpGuid, int tpId, byte[] StatusFieldValue, short rowOrdinal, string statusField);

        /// <summary>
        /// 更新一条数据库记录
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="excludeField"></param>
        /// <param name="conditionParam"></param>
        /// <param name="tableName"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        int UpdateTableRow(Hashtable metadata, List<string> excludeField, Hashtable conditionParam, string tableName, string condition);

        /// <summary>
        /// 更新NameValuePair中workflow instance相关的信息
        /// </summary>
        /// <param name="fieldId"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        int UpdateTableNameValuePairForWFInstance(Guid siteId, Guid listId, int itemId, Guid workflowInstanceId, Guid fieldId, int level);

        /// <summary>
        /// 更新workflow status column的Name
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="workflowAssociationId"></param>
        /// <param name="internalNameStatusField"></param>
        void UpdateWorkflowStatusFieldName(Guid siteId, Guid workflowAssociationId, string internalNameStatusField);

        /// <summary>
        /// 更新workflow configuration
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="workflowAssociationId"></param>
        /// <param name="configuration"></param>
        void UpdateWorkflowConfiguration(Guid siteId, Guid workflowAssociationId, int configuration);
        /// <summary>
        /// 更新workflow association name
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="idworkflowAssociationId"></param>
        /// <param name="name"></param>
        void UpdateAssociationName(Guid siteId, Guid idworkflowAssociationId, string name);
        /// <summary>
        /// 更新workflow association 创建时间
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="workflowAssociationId"></param>
        /// <param name="created"></param>
        void UpdateWorkflowAssociationCreatedTime(Guid siteId, Guid workflowAssociationId, DateTime created);
        
        /// <summary>
        /// 更新workflow association Author
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="workflowAssociationId"></param>
        /// <param name="userId"></param>
        void UpdateWorkflowAssociationAuthor(Guid siteId, Guid workflowAssociationId, int userId);

        /// <summary>
        /// 更新workflow association modified time
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="workflowAssociationId"></param>
        /// <param name="modified"></param>
        void UpdateWorkflowAssociationModifiedTime(Guid siteId, Guid workflowAssociationId, DateTime modified);

        /// <summary>
        /// 更新List modified time
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="lastModified"></param>
        void UpdateListModifiedTime(Guid siteId, Guid listId, DateTime lastModified);

        /// <summary>
        /// 计算并更新workflow running instance count
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="workflowAssociationId"></param>
        void RecalculateRunningInstanceCount(Guid siteId, Guid webId, Guid listId, Guid workflowAssociationId);

        /// <summary>
        /// 获取workflow instance信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="itemId"></param>
        /// <param name="workflowAssociationId"></param>
        /// <param name="hasRunningInstance"></param>
        /// <returns></returns>
        List<Dictionary<String, object>> TryGetWorkflowInfo(Guid siteId, Guid webId, Guid listId, int itemId, Guid workflowAssociationId, out bool hasRunningInstance);

        /// <summary>
        /// 更新文件DocFlags信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="uniqueId"></param>
        /// <param name="level"></param>
        void UpdateWorkflowTemplateFileDocFlags(Guid siteId, Guid parentId, Guid uniqueId, byte level);
        #endregion
        /// <summary>
        /// 通过column值，查询list中Item集合
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="colName"></param>
        /// <param name="colValue"></param>
        /// <returns></returns>
        List<int> GetItemsByColumnValue(Guid siteId, Guid listId, string colName, string colValue);
        /// <summary>
        /// 通过column值，查询folder中的Item集合
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="parentId">folder id</param>
        /// <param name="colName"></param>
        /// <param name="colValue"></param>
        /// <returns></returns>
        List<int> GetItemsByColumnValue(Guid siteId, Guid listId, Guid parentId, string colName, string colValue);
        /// <summary>
        /// 更新List的column信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="listColumns"></param>
        void UpdateListInfoByNative(Guid siteId, Guid webId, Guid listId, Dictionary<string, object> listColumns);

        /// <summary>
        /// 移动webpart，主要更新两个Properties
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="fileId"></param>
        /// <param name="fromWebPartId"></param>
        /// <param name="toWebPartId"></param>
        void MoveWebPartProperty(Guid siteId, Guid fileId, Guid fromWebPartId, Guid toWebPartId);

        #region add for apps
        /// <summary>
        /// 获取app instance status
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="sourceInfoId"></param>
        /// <returns></returns>
        AveAppInstanceStatus CheckAppInstallationStatus(Guid siteId, Guid webId, Guid sourceInfoId);

        /// <summary>
        /// 获取app manifest信息
        /// </summary>
        /// <param name="appFingerprint"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        string GetAppManifest(byte[] appFingerprint, Guid siteId);
        #endregion

        #endregion

        /// <summary>
        /// 通过system id获取login name
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="systemId"></param>
        /// <returns></returns>
        string GetUserLoginBySystemId(Guid siteId, byte[] systemId);

        /// <summary>
        /// 设置user的active为true
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="systemId"></param>
        void ActiveDeletedUserBySystemId(Guid siteId, byte[] systemId);

        /// <summary>
        /// Item上是否有alert
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        bool ItemHasAlerts(Guid siteId, Guid listId, int itemId);

        /// <summary>
        /// 是否存在alert
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="itemId"></param>
        /// <param name="eventType"></param>
        /// <param name="userId"></param>
        /// <param name="frequency"></param>
        /// <returns></returns>
        bool HasAlertsOfSpecificConditions(Guid siteId, Guid listId, int? itemId, AveEventType eventType, int userId, AveAlertFrequency frequency);

        /// <summary>
        /// 插入一条记录到DocsToStreams
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="docId"></param>
        /// <param name="aveRBSStubInfo13"></param>
        /// <param name="isCheckOut"></param>
        /// <param name="level"></param>
        void InsertDocsToStreams(Guid siteId, Guid docId, AveRBSStubInfo13 aveRBSStubInfo13, bool isCheckOut, byte level);

        /// <summary>
        /// 插入一条记录到DocStreams
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="docId"></param>
        /// <param name="aveRBSStubInfo13"></param>
        void InsertDocStreams(Guid siteId, Guid docId, AveRBSStubInfo13 aveRBSStubInfo13);

        /// <summary>
        /// 获取文件的Size
        /// </summary>
        /// <param name="mBaseItemInfo"></param>
        /// <returns></returns>
        long GetNativeContentSize(AveBaseItemInfo mBaseItemInfo);

        /// <summary>
        /// 获取Item rowId和column value的mapping
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns>[rowid,colValue]</returns>
        Dictionary<string, string> GetLookupItemIdAndDisplayValue(AveLookupFieldInfo fieldInfo);

        string GetDocIdUrl(string docDirName, string docLeafName, Guid siteId);

        /// <summary>
        /// 更新AllDocs表相关信息
        /// </summary>
        /// <param name="info">SiteId,ParentId,Level,UnVersionedMetaInfo,Name需要初始化</param>
        /// <param name="guid"></param>
        /// <param name="docdataObjects"></param>
        void ChangeDocdataByNative(AveBaseItemInfo info, Guid guid, Dictionary<string, object> docdataObjects);
        
        /// <summary>
        /// check user has enough permission to db
        /// </summary>
        /// <returns></returns>
        bool DoesUserHasEnoughPermission();

        /// <summary>
        /// for workflow customaction. update customaction id to source wf id. 
        /// case: after job, publish wf again, customaction will be double.
        /// </summary>
        /// <param name="newId"></param>
        void ReplaceCustomActionId(Guid siteId, Guid webId, string scopeId, Guid oldId, Guid newId);
    }
}
