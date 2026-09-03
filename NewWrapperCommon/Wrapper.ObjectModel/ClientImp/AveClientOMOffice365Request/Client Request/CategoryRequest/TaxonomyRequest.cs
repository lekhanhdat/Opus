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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.ClientOM
{
    public partial class AveClientOMOffice365Request
    {
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetTaxonomySession()
        {
            return base.GetTaxonomySession();
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetTermStores()
        {
            return base.GetTermStores();
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetChanges(Guid termStoreId, DateTime startTime)
        {
            return base.GetChanges(termStoreId, startTime);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetChanges(Guid termStoreId, TimeSpan sinceTimeAgo)
        {
            return base.GetChanges(termStoreId, sinceTimeAgo);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetChanges(Guid termStoreId, DateTime startTime, AveChangedItemType itemType)
        {
            return base.GetChanges(termStoreId, startTime, itemType);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetChanges(Guid termStoreId, DateTime startTime, AveChangedItemType itemType, AveChangedOperationType operationType)
        {
            return base.GetChanges(termStoreId, startTime, itemType, operationType);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetTaxonomyGroups(Guid guid)
        {
            using (ClientContext context = CreateContext())
            {
                Dictionary<string, object> groupsProp = new Dictionary<string, object>();
                List<Dictionary<string, object>> groupsList = new List<Dictionary<string, object>>();
                try
                {
                    TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                    TermStore store = session.TermStores.GetById(guid);
                    ExceptionHandlingScope principalCondition = new ExceptionHandlingScope(context);
                    using (principalCondition.StartScope())
                    {
                        using (principalCondition.StartTry())
                        {
                            context.Load(store.Groups);
                            context.Load(store.Groups, groupCollection => groupCollection.IncludeWithDefaultProperties(t => t.GroupManagerPrincipalNames, t => t.ContributorPrincipalNames));
                        }
                        using (principalCondition.StartCatch())
                        {
                            context.Load(store.Groups);
                        }
                    }
                    context.ExecuteQuery();
                    foreach (TermGroup group in store.Groups)
                    {
                        Dictionary<string, object> groupProp = new Dictionary<string, object>();
                        AveObjectCopy.GetObjectBasicProperties(groupProp, group);
                        List<Dictionary<string, object>> groupManagers = new List<Dictionary<string, object>>();
                        List<Dictionary<string, object>> groupContributors = new List<Dictionary<string, object>>();
                        if (group.IsPropertyAvailable("GroupManagerPrincipalNames"))
                        {
                            foreach (string principalName in group.GroupManagerPrincipalNames)
                            {
                                Dictionary<string, object> manager = new Dictionary<string, object>();
                                manager["PrincipalName"] = principalName;
                                manager["DisplayName"] = string.Empty;
                                manager["GrantRightsMask"] = (ulong)(AveTaxonomyRights.GroupManager | AveTaxonomyRights.EditTerm | AveTaxonomyRights.AddTermSetEditPermissions | AveTaxonomyRights.EditGroup | AveTaxonomyRights.EditTermSet);
                                manager["DenyRightsMask"] = (ulong)AveTaxonomyRights.None;
                                groupManagers.Add(manager);
                            }
                        }
                        if (group.IsPropertyAvailable("ContributorPrincipalNames"))
                        {
                            foreach (string principalName in group.ContributorPrincipalNames)
                            {
                                Dictionary<string, object> contributor = new Dictionary<string, object>();
                                contributor["PrincipalName"] = principalName;
                                contributor["DisplayName"] = string.Empty;
                                contributor["GrantRightsMask"] = (ulong)(AveTaxonomyRights.Contributor | AveTaxonomyRights.EditTerm | AveTaxonomyRights.EditTermSet);
                                contributor["DenyRightsMask"] = (ulong)AveTaxonomyRights.None;
                                groupContributors.Add(contributor);
                            }
                        }
                        groupProp["GroupManagers"] = groupManagers;
                        groupProp["Contributors"] = groupContributors;
                        groupsList.Add(groupProp);
                    }
                }
                catch (Exception e)
                {
                    mLogger.Error("Get TermGroups Failed, error message:{0}", e.ToString());
                }
                groupsProp[AveObjectModelConstant.ChildrenProperties] = groupsList;
                return groupsProp;
            }
        }
        //[KeepOriginalWithAPI]
        //public override Dictionary<string, object> GetTermGroup(Guid termStoreId, Guid groupId)
        //{
        //    return base.GetTermGroup(termStoreId, groupId);
        //}
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetTermSet(Guid termStoreId, Guid termSetId)
        {
            return base.GetTermSet(termStoreId, termSetId);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetTermSets(Guid termStoreId, Guid groupId)
        {
            return base.GetTermSets(termStoreId, groupId);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetTermSetsInTermStores(string termSetName, int LCID)
        {
            return base.GetTermSetsInTermStores(termSetName, LCID);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetTerms(Guid termStoreId, Guid groupId, Guid termSetId, Guid parentTermId)
        {
            return base.GetTerms(termStoreId, groupId, termSetId, parentTermId);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetTerms(Guid termStoreId, Guid termSetId, string termLabel, bool trimUnavailable)
        {
            return base.GetTerms(termStoreId, termSetId, termLabel, trimUnavailable);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetLables(Guid termStoreId, Guid termSetId, Guid parentTermId)
        {
            return base.GetLables(termStoreId, termSetId, parentTermId);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetTermGroup(Guid termStoreId, Guid groupId)
        {
            return base.GetTermGroup(termStoreId, groupId);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetTerm(Guid termStoreId, Guid termId)
        {
            return base.GetTerm(termStoreId, termId);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetTerm(Guid termStoreId, Guid termSetId, Guid termId)
        {
            return base.GetTerm(termStoreId, termSetId, termId);
        }
        [KeepOriginalWithAPI]
        public override bool IsTermExist(Guid termStoreId, Guid termId)
        {
            return base.IsTermExist(termStoreId, termId);
        }
        [KeepOriginalWithAPI]
        public override bool IsTermSetExist(Guid termStoreId, Guid termSetId)
        {
            return base.IsTermSetExist(termStoreId, termSetId);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetSiteCollectionGroup(Guid termStoreId, string siteUrl, bool createIfMissing)
        {
            return base.GetSiteCollectionGroup(termStoreId, siteUrl, createIfMissing);
        }
        [KeepOriginalWithAPI]
        public override string GetDefaultLabel(Guid termStoreId, Guid termId, int defaultID)
        {
            return base.GetDefaultLabel(termStoreId, termId, defaultID);
        }
        [KeepOriginalWithAPI]
        public override string GetDescription(Guid termStoreId, Guid termSetId, Guid parentTermId, int lcid)
        {
            return base.GetDescription(termStoreId, termSetId, parentTermId, lcid);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<int, string> GetAllDescriptions(Guid termStoreId, Guid termSetId, Guid parentTermId, Collection<int> lcids)
        {
            return base.GetAllDescriptions(termStoreId, termSetId, parentTermId, lcids);
        }

        [NoAPI]
        public override List<Dictionary<string, object>> GetKeyWords()
        {
            return base.GetKeyWords();
        }

        [NoAPI]
        public override void UpdateMetadataListFieldSettings(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProperties)
        {
            base.UpdateMetadataListFieldSettings(webServerRelativeUrl, listId, updateProperties);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, string> SetCustomProperty(Guid termStoreId, Guid termSetId, Guid termId, string name, string value, AveTermSetItemType type)
        {
            return base.SetCustomProperty(termStoreId, termSetId, termId, name, value, type);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, string> SetLocalCustomProperty(Guid termStoreId, Guid termSetId, Guid termId, string name, string value)
        {
            return base.SetLocalCustomProperty(termStoreId, termSetId, termId, name, value);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> UpdateTermStore(Guid guid, int termStoreDefaultLanguage, Dictionary<string, object> needUpdateProperties)
        {
            return base.UpdateTermStore(guid, termStoreDefaultLanguage, needUpdateProperties);
        }

    }
}
