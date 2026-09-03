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

        AveTermStoreInfo GetTermStore();

        List<AveMetadataGroupInfo> GetGlobalGroups();
        List<AveMetadataGroupInfo> GetLocalGroups();
        AveMetadataGroupInfo GetGroup(Guid groupId);
        AveMetadataGroupInfo GetGroup(int groupId);
        AveMetadataGroupInfo GetGroup(string groupName);

        List<AveTermSetInfo> GetTermSets(Guid groupId);
        AveTermSetInfo GetTermSet(Guid setId);
        AveTermSetInfo GetTermSet(int setId);
        //AveTermSetInfo GetTermSet(Guid groupId, string setName);

        List<AveTermInfo> GetTerms(Guid termSetId);
        List<AveTermInfo> GetTermsInTerm(Guid termSetId, Guid termId);
        List<AveTermInfo> GetTermsInTermSet(Guid termSetId);
        AveTermInfo GetTerm(Guid termSetId, int termId);
        AveTermInfo GetTerm(Guid termSetId, Guid termId);
        //AveTermInfo GetTerm(Guid termSetId, string termName);
        string GetTermDefaultLabel(int termId);

        List<AveTermChangeItem> GetAllChanges(Nullable<DateTime> sinceTime);
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
        bool IsSiteCollectionGroup(Guid groupId);
        List<Guid> GetSiteCollectionId(Guid groupId);

        bool IsPublished(string contentTypeId);
        bool IsPublished(string contentTypeId, Guid defaultPartitionId);
        bool IsUnPublished(string contentTypeId);
        bool IsUnPublished(string contentTypeId, Guid defaultPartitionId);

        void GetLanguage(Guid defaultPartitionId);
    }
}
