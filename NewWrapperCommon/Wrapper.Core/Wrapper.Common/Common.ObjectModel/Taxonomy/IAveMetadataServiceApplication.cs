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

namespace AvePoint.Wrapper.Common
{
    public interface IAveMetadataServiceApplication: IAveIisWebServiceApplication, IDisposable
    {
        IAveDatabase Database { get; }
        int DefaultLanguage { get; set; }
        /// <summary>
        /// 对于Multi-Tenant类型的MMS，PartitionId并不准确，仅仅是从数据库查询数据的第一条数据的PartitionId值
        /// 主要是为取MMS的Name
        /// 对于普通的MMS，只有一个PartitionId值，他是准确的
        /// </summary>
        Guid PartitionId { get; }

        AveTermStoreInfo GetTermStore();
        AveTermStoreInfo GetTermStore(Guid defaultPartitionId);

        List<AveMetadataGroupInfo> GetGlobalGroups();
        List<AveMetadataGroupInfo> GetGlobalGroups(Guid defaultPartitionId);

        List<AveMetadataGroupInfo> GetLocalGroups();
        List<AveMetadataGroupInfo> GetLocalGroups(Guid defaultPartitionId);

        AveMetadataGroupInfo GetGroup(Guid groupId);
        AveMetadataGroupInfo GetGroup(Guid groupId, Guid defaultPartitionId);

        AveMetadataGroupInfo GetGroup(int groupId);
        AveMetadataGroupInfo GetGroup(int groupId, Guid defaultPartitionId);

        AveMetadataGroupInfo GetGroup(string groupName);
        AveMetadataGroupInfo GetGroup(string groupName, Guid defaultPartitionId);

        List<AveTermSetInfo> GetTermSets(Guid groupId);
        List<AveTermSetInfo> GetTermSets(Guid groupId, Guid defaultPartitionId);

        AveTermSetInfo GetTermSet(Guid setId);
        AveTermSetInfo GetTermSet(Guid setId, Guid defaultPartitionId);

        AveTermSetInfo GetTermSet(int setId);
        AveTermSetInfo GetTermSet(int setId, Guid defaultPartitionId);
        //AveTermSetInfo GetTermSet(Guid groupId, string setName);

        List<AveTermInfo> GetTermsInTerm(Guid termSetId, Guid termId);
        List<AveTermInfo> GetTermsInTerm(Guid termSetId, Guid termId, Guid defaultPartitionId);

        List<AveTermInfo> GetTermsInTermSet(Guid termSetId);
        List<AveTermInfo> GetTermsInTermSet(Guid termSetId, Guid defaultPartitionId);

        AveTermInfo GetTerm(Guid termSetId, int termId);
        AveTermInfo GetTerm(Guid termSetId, int termId, Guid defaultPartitionId);

        AveTermInfo GetTerm(Guid termSetId, Guid termId);
        AveTermInfo GetTerm(Guid termSetId, Guid termId, Guid defaultPartitionId);
        AveTermChangeItem GetTermParent(Guid termSetId, Guid termId, Guid parentTermId, Guid partitionId, bool isRoot, bool isSourceTerm);
        List<AveTermChangeItem> GetTermSetChildren(Guid termSetId, Guid partitionId);
        AveTermChangeItem GetTermSetParent(Guid termSetId, Guid partitionId);
        //AveTermInfo GetTerm(Guid termSetId, string termName);

        string GetTermDefaultLabel(int termId);
        string GetTermDefaultLabel(int termId, Guid defaultPartitionId);

        List<AveTermChangeItem> GetAllChanges(Nullable<DateTime> sinceTime);
        List<AveTermChangeItem> GetAllChanges(Nullable<DateTime> sinceTime, Guid defaultPartitionId);

        List<AveTermChangeItem> GetChangesInTerm(Guid termSetId, Guid termId, Nullable<DateTime> sinceTime);
        
        List<AveTermChangeItem> GetChangesInTerm(Guid termSetId, Guid termId, Nullable<DateTime> sinceTime, Guid defaultPartitionId);

        List<AveTermChangeItem> GetChangesInGroup(Guid groupId, Nullable<DateTime> sinceTime);
        List<AveTermChangeItem> GetChangesInGroup(Guid groupId, Nullable<DateTime> sinceTime, Guid defaultPartitionId);

        List<AveTermChangeItem> GetChangesInTermSet(Guid termSetId, Nullable<DateTime> sinceTime);
        List<AveTermChangeItem> GetChangesInTermSet(Guid termSetId, Nullable<DateTime> sinceTime, Guid defaultPartitionId);

        List<AveTermChangeItem> GetChangesInStore(Nullable<DateTime> sinceTime, bool isGlobal);
        List<AveTermChangeItem> GetChangesInStore(Nullable<DateTime> sinceTime, bool isGlobal, Guid defaultPartitionId);

        List<AveTermChangeItem> GetChangesInGroup(Guid groupId, Nullable<DateTime> sinceTime, Nullable<DateTime> toTime);
        List<AveTermChangeItem> GetChangesInGroup(Guid groupId, Nullable<DateTime> sinceTime, Nullable<DateTime> toTime, Guid defaultPartitionId);

        List<AveTermChangeItem> GetChangesInTermSet(Guid termSetId, Nullable<DateTime> sinceTime, Nullable<DateTime> toTime);
        List<AveTermChangeItem> GetChangesInTermSet(Guid termSetId, Nullable<DateTime> sinceTime, Nullable<DateTime> toTime, Guid defaultPartitionId);

        Uri GetContentTypeSyndicationHubLocal();
        Uri GetContentTypeSyndicationHubLocal(Guid defaultPartitionId);

        bool IsSiteCollectionGroup(Guid groupId);
        bool IsSiteCollectionGroup(Guid groupId, Guid defaultPartitionId);

        List<Guid> GetSiteCollectionId(Guid groupId);
        List<Guid> GetSiteCollectionId(Guid groupId, Guid defaultPartitionId);

        bool IsPublished(string contentTypeId);
        bool IsPublished(string contentTypeId, Guid defaultPartitionId);

        bool IsUnPublished(string contentTypeId);
        bool IsUnPublished(string contentTypeId, Guid defaultPartitionId);

        List<ServiceSetting> GetPartitionServiceSettings();
        List<AveSiteMapVisible> GetTenancyAdminSiteId(Guid defaultPartitionId);
        bool IsMetadataPartition(Guid ApplicationId);

        void GetLanguage(Guid defaultPartitionId);
        void GetLanguage();
    }
}
