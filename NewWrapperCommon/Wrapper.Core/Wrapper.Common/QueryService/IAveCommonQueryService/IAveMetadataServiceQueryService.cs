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
namespace AvePoint.Wrapper.Common
{
    using System;
    using System.Collections.Generic;
    public interface IAveMetadataServiceQueryService : IAveQueryService
    {
        #region MetadataServiceApplication

        /// <summary>
        /// 获取MMS的default Language,同时返回AveTermStoreInfo信息(只有TermStoreAdministrators信息)
        /// 出错return 0,正常return一个languageId
        /// </summary>
        /// <param name="termStoreInfo"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        int GetLanguage(ref AveTermStoreInfo termStoreInfo, Guid defaultPartitionId);

        /// <summary>
        /// 获取MMS的default Language
        /// 出错return 0,正常return一个languageId
        /// </summary>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        int GetLanguage(Guid defaultPartitionId);

        /// <summary>
        /// 返回AveTermStoreInfo信息(只有TermStoreAdministrators信息)
        /// </summary>
        /// <param name="defaultPartitionId">Term store PartitionId</param>
        /// <returns>返回AveTermStoreInfo信息</returns>
        AveTermStoreInfo GetTermStoreInfo(Guid defaultPartitionId);

        /// <summary>
        /// 获取Global Groups,效率考虑，有API实现
        /// </summary>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        List<AveMetadataGroupInfo> GetGlobalGroups(Guid defaultPartitionId);

        /// <summary>
        /// 按照GroupId（Guid）获取特定Group，效率考虑，有API实现
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        AveMetadataGroupInfo GetGroup(Guid groupId, Guid defaultPartitionId);

        /// <summary>
        /// 按照GroupName获取特定Group，效率考虑，有API实现
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        AveMetadataGroupInfo GetGroup(string groupName, Guid defaultPartitionId);

        /// <summary>
        /// 按照GroupId（Int）获取特定Group，效率考虑，有API实现
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        AveMetadataGroupInfo GetGroup(int groupId, Guid defaultPartitionId);

        /// <summary>
        /// 获取Local Groups，效率考虑，有API实现
        /// </summary>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        List<AveMetadataGroupInfo> GetLocalGroups(Guid defaultPartitionId);

        /// <summary>
        /// 按照Guid获取指定TermSet下指定Term信息（唯一），效率考虑，有API实现.
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="termId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        AveTermInfo GetTerm(Guid termSetId, int termId, Guid defaultPartitionId, int defaultLanguage);

        /// <summary>
        /// 按照Guid获取指定TermSet中,指定Term下的Terms信息（多值），效率考虑，有API实现.
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="termId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns>返回集(可能为空)，不会返回null</returns>
        List<AveTermInfo> GetTermsInTerm(Guid termSetId, Guid termId, Guid defaultPartitionId, int defaultLanguage);

        /// <summary>
        /// 按照Guid获取termSet下的全部Terms信息，效率考虑，有API实现.
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns>返回集(可能为空)，不会返回null</returns>
        List<AveTermInfo> GetTermsInTermSet(Guid termSetId, Guid defaultPartitionId, int defaultLanguage);

        /// <summary>
        /// 按照TermSet Guid获取TermSet下的Term的UniqueId集合，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns>返回集(可能为空)，不会返回null</returns>
        List<Guid> GetTermIds(Guid termSetId, Guid defaultPartitionId);

        /// <summary>
        /// 按照Guid获取特定TermSet中指定Term下的Term UniqueId 集合，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="termId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns>返回集(可能为空)，不会返回null</returns>
        List<Guid> GetTermIds(Guid termSetId, Guid termId, Guid defaultPartitionId);

        /// <summary>
        /// 判断是否是SiteCollection下的Group，效率考虑，有API实现
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        bool IsSiteCollectionGroup(Guid groupId, Guid defaultPartitionId);

        /// <summary>
        /// 通过TermGroupId获取SiteCollectionId，效率考虑，有API实现
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        List<Guid> GetSiteCollectionIdList(Guid groupId, Guid defaultPartitionId);
        /// <summary>
        /// 从EMCChangeLog中获取特定Group+TermSet下的Changes为Incremental处理，效率考虑，有API实现.
        /// </summary>
        /// <param name="groupId">Allow null</param>
        /// <param name="termSetId">Allow null</param>
        /// <param name="sinceTime">Not null</param>
        /// <param name="changedItemType">Allow null</param>
        /// <param name="defaultPartitionId">Not null</param>
        /// <returns></returns>
        List<AveTermChangeItem> GetChanges(int? groupId, int? termSetId, DateTime sinceTime, int? changedItemType, Guid defaultPartitionId);
        /// <summary>
        /// 获取term下change的terms,为Incremental处理，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="termId"></param>
        /// <param name="sinceTime">如果为null，则查询term下所有term，不为null，按change log查询</param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        List<AveTermChangeItem> GetChangesInTerm(Guid termSetId, Guid termId, DateTime? sinceTime, Guid defaultPartitionId, int defaultLanguage);
        /// <summary>
        /// 获取termset下所有的source terms,为反插discover还原pin source term做处理，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        List<AveTermChangeItem> GetTermSetChildren(Guid termSetId, Guid defaultPartitionId, int defaultLanguage);
        /// <summary>
        /// 获取term的parent term(可能为termset),为反插discover还原pin source term做处理，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="termId"></param>
        /// <param name="parentTermId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="isRoot"></param>
        /// <param name="isSourceTerm"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        AveTermChangeItem GetTermParent(Guid termSetId, Guid termId, Guid parentTermId, Guid partitionId, bool isRoot, bool isSourceTerm, int defaultLanguage);
        /// <summary>
        /// 获取termset的parent(group),为反插discover还原pin source term做处理，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        AveTermChangeItem GetTermSetParent(Guid termSetId, Guid partitionId, int defaultLanguage);
        /// <summary>
        /// 根据term的UniqueId获取int的TermId，效率考虑，有API实现.
        /// </summary>
        /// <param name="termId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        int GetTermId(Guid termId, Guid defaultPartitionId);

        /// <summary>
        /// 按照Global或者Local获取GroupIds信息，效率考虑，有API实现. TODO需要添加partitionId
        /// todo:wbhu,获取所有TermGroup还是指定TermStore中的？需要结合调用逻辑确定下怎样更合理
        /// </summary>
        /// <param name="isGlobal"></param>
        /// <returns></returns>
        Dictionary<Guid, string> GetGroupIds(bool isGlobal);

        /// <summary>
        /// 获取指定Guid的GroupId，效率考虑，有API实现.
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        int? GetGroupId(Guid groupId);

        /// <summary>
        /// Get the group id by unique id in specific term store
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        int? GetGroupId(Guid groupId, Guid defaultPartitionId);

        /// <summary>
        /// 判断contentType是否published，效率考虑，有API实现
        /// </summary>
        /// <param name="contentTypeId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        bool IsPublished(string contentTypeId, Guid defaultPartitionId);

        /// <summary>
        /// 判断contentType是否Unpublished，效率考虑，有API实现
        /// </summary>
        /// <param name="contentTypeId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        bool IsUnPublished(string contentTypeId, Guid defaultPartitionId);

        /// <summary>
        /// 获取TermStore setting xml，效率考虑，有API实现.
        /// </summary>
        /// <param name="defaultpartitionId"></param>
        /// <returns>如果TermStore不存在，返回String.Empty</returns>
        string GetTermStore(Guid defaultpartitionId);

        /// <summary>
        ///获取MMS下store中的Group改变，为Incremental处理，效率考虑，有API实现
        ///GetGroupIds时不会用PartitionId做条件
        /// </summary>
        /// <param name="sinceTime"></param>
        /// <param name="isGlobal"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        List<AveTermChangeItem> GetChangesInStore(DateTime? sinceTime, bool isGlobal, Guid defaultPartitionId);

        /// <summary>
        ///获取MMS下store中的Group改变，为Incremental处理，效率考虑，有API实现
        /// GetGroupIds时会用PartitionId做条件
        /// </summary>
        /// <param name="sinceTime"></param>
        /// <param name="isGlobal"></param>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        List<AveTermChangeItem> GetChangesInStoreForTenant(DateTime? sinceTime, bool isGlobal, Guid defaultPartitionId);

        /// <summary>
        /// 获取MMS某个Group下的TermSet改变为Incremental处理,效率考虑，有API实现
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="sinceTime"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        List<AveTermChangeItem> GetChangesInGroup(Guid groupId, DateTime? sinceTime, Guid defaultPartitionId, int defaultLanguage);

        /// <summary>
        /// 获取TermSet中的Term改变为Incremental处理，效率考虑，有API实现
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="sinceTime"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        List<AveTermChangeItem> GetChangesInTermSet(Guid termSetId, DateTime? sinceTime, Guid defaultPartitionId, int defaultLanguage);

        /// <summary>
        /// 查询特定时间范围内，特定group上的Metadata change item.
        /// </summary>
        /// <param name="defaultPartitionId"></param>
        /// <param name="groupId"></param>
        /// <param name="sinceTime"></param>
        /// <param name="toTime"></param>
        /// <returns></returns>
        List<AveTermChangeItem> GetChangesInGroup(Guid groupId, DateTime? sinceTime, DateTime? toTime, Guid defaultPartitionId);

        /// <summary>
        /// 查询特定时间范围内，特定TermSet上的Metadata change item.
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="sinceTime"></param>
        /// <param name="toTime"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        List<AveTermChangeItem> GetChangesInTermSet(Guid termSetId, DateTime? sinceTime, DateTime? toTime, Guid defaultPartitionId);
        /// <summary>
        /// 按照Id获取Group下的TermSet信息，效率考虑，有API实现.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="partitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        List<AveTermSetInfo> GetTermSetIds(Guid groupId,Guid partitionId, int defaultLanguage);

        /// <summary>
        /// 通过Id获取Group下的TermSets，效率考虑，有API实现.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        List<AveTermSetInfo> GetTermSets(Guid groupId, Guid defaultPartitionId, int defaultLanguage);

        /// <summary>
        /// 通过TermSet Id 获取 term set的信息
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        AveTermSetInfo GetTermSet(int setId, Guid defaultPartitionId, int defaultLanguage);

        /// <summary>
        /// 通过TermSet UniqueId 获取 term set的信息
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        AveTermSetInfo GetTermSet(Guid setId, Guid defaultPartitionId, int defaultLanguage);

        /// <summary>
        /// 通过MetadataServiceApplication获取不到Term中的GetDefaultLabel方法,只能通过SQL实现
        /// </summary>
        /// <param name="termId"></param>
        /// <param name="defaultPartitionId"></param>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        string GetTermDefaultLabel(int termId, Guid defaultPartitionId, int defaultLanguage);

        /// <summary>
        /// 获取metadata service中所有partition 的setting信息
        /// </summary>
        /// <returns></returns>
        List<ServiceSetting> GetPartitionServiceSettings();
        /// <summary>
        /// 通过PartitionId从SharePoint_Config DB的SiteMappingVisible表查询
        /// </summary>
        /// <param name="defaultPartitionId"></param>
        /// <returns></returns>
        List<AveSiteMapVisible> GetTenancyAdminSiteId(Guid defaultPartitionId);
        #endregion

    }
}
