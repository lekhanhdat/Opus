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
namespace AvePoint.ObjectModel.ClientOM
{
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint.Client;
    using Microsoft.SharePoint.Client.Taxonomy;
    using System;
    using System.Collections.Generic;

    public partial class AveClientOM2013Request
    {
        public Dictionary<string, object> GetTaxonomySession()
        {
            using (var context = CreateRetryContext())
            {
                TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                context.Load(session);
                TermStore keyTermStore = session.GetDefaultKeywordsTermStore();
                context.Load(keyTermStore);
                context.Load(keyTermStore, t => t.ContentTypePublishingHub);
                TermStore sitecollectionTermStore = session.GetDefaultSiteCollectionTermStore();
                context.Load(sitecollectionTermStore);
                context.Load(sitecollectionTermStore, t => t.ContentTypePublishingHub);
                context.ExecuteQuery();
                Dictionary<string, object> sessionProp = new Dictionary<string, object>();
                CopyProperty(sessionProp, session);
                Dictionary<string, object> keyTermStoreProp = new Dictionary<string, object>();
                CopyProperty(keyTermStoreProp, keyTermStore);
                Dictionary<string, object> sitecollectionTermStoreProp = new Dictionary<string, object>();
                CopyProperty(sitecollectionTermStoreProp, sitecollectionTermStore);
                sessionProp["DefaultKeywordsTermStore" + AveObjectModelConstant.ObjectPropertySuffix] = keyTermStoreProp;
                sessionProp["DefaultSiteCollectionTermStore" + AveObjectModelConstant.ObjectPropertySuffix] = sitecollectionTermStoreProp;
                return sessionProp;
            }
        }
        public Dictionary<string, object> GetTermStores()
        {
            Dictionary<string, object> termStoresProp = new Dictionary<string, object>();
            var termStoresList = new List<IDictionary<string, object>>();
            using (var context = CreateRetryContext())
            {
                TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                context.Load(session.TermStores, ts => ts.IncludeWithDefaultProperties(t => t.ContentTypePublishingHub));
                context.ExecuteQuery();
                foreach (TermStore store in session.TermStores)
                {
                    List<int> languages = new List<int>();
                    Dictionary<string, object> storeProp = new Dictionary<string, object>();
                    CopyProperty(storeProp, store);
                    foreach (int language in store.Languages)
                    {
                        languages.Add(language);
                    }
                    storeProp["Languages" + AveObjectModelConstant.ObjectPropertySuffix] = languages;
                    termStoresList.Add(storeProp);
                }
            }
            termStoresProp.AddChildren(termStoresList);
            return termStoresProp;
        }
        public Dictionary<string, object> GetTaxonomyGroups(Guid guid)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> groupsProp = new Dictionary<string, object>();
                var groupsList = new List<IDictionary<string, object>>();
                try
                {
                    TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                    TermStore store = session.TermStores.GetById(guid);
                    context.Load(store.Groups);
                    context.ExecuteQuery();
                    foreach (TermGroup group in store.Groups)
                    {
                        Dictionary<string, object> groupProp = new Dictionary<string, object>();
                        CopyProperty(groupProp, group);
                        groupsList.Add(groupProp);
                    }
                }
                catch (Exception e)
                {
                    mLogger.Error("Get TermGroups Failed, error message:{0}", e.ToString());
                }
                groupsProp.AddChildren(groupsList);
                return groupsProp;
            }
        }
        public Dictionary<string, object> GetTermSets(Guid termStoreId, string groupName)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> termSetsProp = new Dictionary<string, object>();
                var termSetsList = new List<IDictionary<string, object>>();
                try
                {
                    TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                    TermStore store = session.TermStores.GetById(termStoreId);
                    TermGroup group = store.Groups.GetByName(groupName);
                    AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(3, AveSPErrorCode.ERROR_OUT_RANGE_INDEX);
                    retryHelper.ExecuteWithRetryMechanism(() =>
                    {
                        context.Load(group.TermSets);
                        context.ExecuteQuery();
                    });
                    foreach (TermSet set in group.TermSets)
                    {
                        Dictionary<string, object> setProp = new Dictionary<string, object>();
                        CopyProperty(setProp, set);
                        termSetsList.Add(setProp);
                    }
                }
                catch (Exception e)
                {
                    mLogger.Error("Get TermSets Failed, groupName: {0},error message:{1}", groupName, e.ToString());
                }
                termSetsProp.AddChildren(termSetsList);
                return termSetsProp;
            }
        }

        public Dictionary<string, object> GetTermSetsInTermStores(string termSetName, int LCID)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> termSetCollectionProperties = new Dictionary<string, object>();
                Dictionary<string, Dictionary<string, object>> termStoresProperties = new Dictionary<string, Dictionary<string, object>>();
                try
                {
                    TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                    TermSetCollection termSetCollection = session.GetTermSetsByName(termSetName, LCID);
                    context.Load(termSetCollection, tempSetCollection => tempSetCollection.IncludeWithDefaultProperties(temp => temp.Group, temp => temp.TermStore));
                    context.ExecuteQuery();
                    foreach (TermSet termSet in termSetCollection)
                    {
                        Dictionary<string, object> termStoreProperties = null;
                        Dictionary<string, object> groupProperties = new Dictionary<string, object>();
                        Dictionary<string, object> termSetProperties = new Dictionary<string, object>();

                        string storeId = termSet.TermStore.Id.ToString();
                        string groupId = termSet.Group.Id.ToString();
                        CopyProperty(termSetProperties, termSet);
                        CopyProperty(groupProperties, termSet.Group);
                        groupProperties["TermSet"] = termSetProperties;

                        if (termStoresProperties.ContainsKey(storeId))
                        {
                            termStoreProperties = termStoresProperties[storeId];
                        }
                        else
                        {
                            termStoreProperties = new Dictionary<string, object>();
                            CopyProperty(termStoreProperties, termSet.TermStore);
                            termStoresProperties[storeId] = termStoreProperties;
                        }
                        if (!termStoreProperties.ContainsKey("Groups"))
                        {
                            termStoreProperties["Groups"] = new Dictionary<string, Dictionary<string, object>>();
                            Dictionary<string, Dictionary<string, object>> dic = termStoreProperties["Groups"] as Dictionary<string, Dictionary<string, object>>;
                            dic[groupId] = groupProperties;
                        }
                        else
                        {
                            Dictionary<string, Dictionary<string, object>> dic = termStoreProperties["Groups"] as Dictionary<string, Dictionary<string, object>>;
                            dic[groupId] = groupProperties;
                        }
                    }
                    termSetCollectionProperties.Add("TermStores", termStoresProperties);
                }
                catch (Exception e)
                {
                    mLogger.Error("Get TermSets in TermStore Failed, error message:{0}", e.ToString());
                }
                return termSetCollectionProperties;
            }
        }
        public Dictionary<string, object> GetTerms(Guid termStoreId, string groupName, string termSetName, Guid parentTermId)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> termsProp = new Dictionary<string, object>();
                var termsList = new List<IDictionary<string, object>>();
                try
                {
                    TermCollection terms = null;
                    AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(3, AveSPErrorCode.ERROR_OUT_RANGE_INDEX);
                    retryHelper.ExecuteWithRetryMechanism(() =>
                    {
                        TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                        TermStore store = session.TermStores.GetById(termStoreId);
                        TermGroup group = store.Groups.GetByName(groupName);
                        TermSet set = group.TermSets.GetByName(termSetName);
                        if (parentTermId.Equals(Guid.Empty))
                        {
                            terms = set.Terms;
                        }
                        else
                        {
                            terms = set.GetTerm(parentTermId).Terms;
                        }
                        context.Load(terms, termCollection => termCollection.IncludeWithDefaultProperties(t => t.Parent.Id, t => t.PinSourceTermSet, t => t.Labels.IncludeWithDefaultProperties()));
                        context.ExecuteQuery();
                    });
                    foreach (Term term in terms)
                    {
                        Dictionary<string, object> termProp = new Dictionary<string, object>();
                        CopyProperty(termProp, term);
                        //AveObjectCopy.GetObjectBasicProperties(termProp, term);
                        termProp["ParentTermId"] = term.Parent.IsPropertyAvailable("Id") ? term.Parent.Id : Guid.Empty;
                        if (!term.PinSourceTermSet.ServerObjectIsNull.Value)
                        {
                            termProp["PinSourceTermSetId"] = term.PinSourceTermSet.Id;
                        }

                        LabelCollection labels = term.Labels;
                        List<Dictionary<string, object>> lableList = new List<Dictionary<string, object>>();
                        foreach (Label label in term.Labels)
                        {
                            Dictionary<string, object> labelProperties = new Dictionary<string, object>();
                            CopyProperty(labelProperties, label);
                            lableList.Add(labelProperties);
                        }

                        Dictionary<string, object> labelsProp = new Dictionary<string, object>();
                        labelsProp.Add("Labels" + AveObjectModelConstant.ObjectPropertySuffix, lableList);
                        termProp.Add("Labels" + AveObjectModelConstant.ObjectPropertySuffix, labelsProp);

                        termsList.Add(termProp);
                    }
                }
                catch (Exception e)
                {
                    mLogger.Error("Get Terms Failed, groupName: {0},termSetName: {1},error message:{2}", groupName, termSetName, e.ToString());
                }
                termsProp.AddChildren(termsList);
                return termsProp;
            }
        }

        public Dictionary<string, object> GetLables(Guid termStoreId, Guid termSetId, Guid parentTermId)
        {
            try
            {
                using (var context = CreateRetryContext())
                {
                    TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                    TermStore store = session.TermStores.GetById(termStoreId);
                    TermSet set = store.GetTermSet(termSetId);
                    Term term = set.GetTerm(parentTermId);
                    context.Load(term.Labels);
                    context.ExecuteQuery();
                    LabelCollection labels = term.Labels;
                    List<Dictionary<string, object>> lableList = new List<Dictionary<string, object>>();
                    foreach (Label label in term.Labels)
                    {
                        Dictionary<string, object> labelProperties = new Dictionary<string, object>();
                        CopyProperty(labelProperties, label);
                        lableList.Add(labelProperties);
                    }
                    Dictionary<string, object> labelsProp = new Dictionary<string, object>();
                    labelsProp.Add("Labels" + AveObjectModelConstant.ObjectPropertySuffix, lableList);
                    return labelsProp;
                }
            }
            /*review-qlluo*/
            catch (Exception ex)
            {
                throw new Exception(string.Format("get lables with term store id:{0}, term set id:{1}, term id:{2} failed:{3}", termStoreId, termSetId, parentTermId, ex));
            }
        }
        public Dictionary<string, object> GetSiteCollectionGroup(Guid termStoreId, string siteUrl)
        {
            using (var context = CreateRetryContext())
            {
                TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                TermStore store = session.TermStores.GetById(termStoreId);
                TermGroup group = store.GetSiteCollectionGroup(context.Site, true);
                context.Load(group);
                context.ExecuteQuery();
                Dictionary<string, object> groupProp = new Dictionary<string, object>();
                CopyProperty(groupProp, group);
                return groupProp;
            }
        }
        public Dictionary<string, object> GetTermSet(Guid termStoreId, Guid termSetId)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> termSetProperties = new Dictionary<string, object>();
                try
                {
                    TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                    TermStore store = session.TermStores.GetById(termStoreId);
                    TermSet termSet = store.GetTermSet(termSetId);
                    context.Load(termSet);
                    TermGroup group = termSet.Group;
                    context.Load(group);
                    context.ExecuteQuery();
                    Dictionary<string, object> groupProperties = new Dictionary<string, object>();
                    CopyProperty(groupProperties, group);
                    Dictionary<string, object> setProperties = new Dictionary<string, object>();
                    CopyProperty(setProperties, termSet);
                    groupProperties.Add("TermSet", setProperties);
                    termSetProperties.Add("Group", groupProperties);
                }
                catch (Exception e)
                {
                    mLogger.Error("Get TermSet Failed, error message:{0}", e.ToString());
                }
                return termSetProperties;
            }
        }
        public Dictionary<string, object> GetTerm(Guid termStoreId, Guid termId)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> termProperties = new Dictionary<string, object>();
                try
                {
                    if (termId == Guid.Empty)
                    {
                        return termProperties;
                    }
                    TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                    Term term = null;
                    if (termStoreId != Guid.Empty)
                    {
                        TermStore store = session.TermStores.GetById(termStoreId);
                        term = store.GetTerm(termId);
                    }
                    else
                    {
                        term = session.GetTerm(termId);
                    }
                    context.Load(term);
                    context.Load(term, t => t.Parent.Id);
                    TermSet termSet = term.TermSet;
                    context.Load(termSet);
                    TermGroup group = termSet.Group;
                    context.Load(termSet.Group);
                    context.ExecuteQuery();
                    Dictionary<string, object> groupProperties = new Dictionary<string, object>();
                    AveObjectCopy.GetObjectBasicProperties(groupProperties, group);
                    Dictionary<string, object> setProperties = new Dictionary<string, object>();
                    CopyProperty(setProperties, termSet);
                    groupProperties.Add("TermSet", setProperties);
                    Dictionary<string, object> findedTermProperties = new Dictionary<string, object>();
                    AssembleTermProperties(term, findedTermProperties);
                    setProperties.Add("Term", findedTermProperties);
                    termProperties.Add("Group", groupProperties);
                }
                catch (Exception e)
                {
                    mLogger.Warn("Can not get the term. termId:{0}, error message:{1}", termId, e.ToString());
                }
                return termProperties;
            }
        }

        public Dictionary<string, object> GetTerm(Guid termStoreId, Guid termSetId, Guid termId)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> termProperties = new Dictionary<string, object>();
                try
                {
                    if (termId == Guid.Empty)
                    {
                        return termProperties;
                    }
                    TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                    Term term = null;
                    TermStore store = session.TermStores.GetById(termStoreId);
                    term = store.GetTermInTermSet(termSetId, termId);

                    context.Load(term);
                    context.Load(term, t => t.Parent.Id);
                    TermSet termSet = term.TermSet;
                    context.Load(termSet);
                    TermGroup group = termSet.Group;
                    context.Load(termSet.Group);
                    context.ExecuteQuery();
                    Dictionary<string, object> groupProperties = new Dictionary<string, object>();
                    AveObjectCopy.GetObjectBasicProperties(groupProperties, group);
                    Dictionary<string, object> setProperties = new Dictionary<string, object>();
                    CopyProperty(setProperties, termSet);
                    groupProperties.Add("TermSet", setProperties);
                    Dictionary<string, object> findedTermProperties = new Dictionary<string, object>();
                    AssembleTermProperties(term, findedTermProperties);
                    setProperties.Add("Term", findedTermProperties);
                    termProperties.Add("Group", groupProperties);
                }
                catch (Exception e)
                {
                    mLogger.Warn("Can not get the term. termId:{0}, error message:{1}", termId, e.ToString());
                }
                return termProperties;
            }
        }

        public Dictionary<string, object> GetTerms(Guid termStoreId, Guid termSetId, string termLabel, bool trimUnavailable)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> termsProp = new Dictionary<string, object>();
                var termsList = new List<IDictionary<string, object>>();
                try
                {
                    TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                    TermCollection terms = null;
                    LabelMatchInformation info = new LabelMatchInformation(context);
                    info.TermLabel = termLabel;
                    info.TrimUnavailable = trimUnavailable;
                    if (termStoreId != Guid.Empty)
                    {
                        TermStore store = session.TermStores.GetById(termStoreId);
                        TermSet set = store.GetTermSet(termSetId);
                        terms = set.GetTerms(info);
                    }
                    else
                    {
                        terms = session.GetTerms(info);
                    }
                    context.Load(terms, termCollection => termCollection.IncludeWithDefaultProperties(t => t.Parent.Id, t => t.PinSourceTermSet));
                    context.ExecuteQuery();
                    foreach (Term term in terms)
                    {
                        Dictionary<string, object> termProp = new Dictionary<string, object>();
                        //AveObjectCopy.GetObjectBasicProperties(termProp, term);
                        CopyProperty(termProp, term);
                        termProp["ParentTermId"] = term.Parent.IsPropertyAvailable("Id") ? term.Parent.Id : Guid.Empty;
                        if (!term.PinSourceTermSet.ServerObjectIsNull.Value)
                        {
                            termProp["PinSourceTermSetId"] = term.PinSourceTermSet.Id;
                        }
                        termsList.Add(termProp);
                    }
                }
                catch (Exception e)
                {
                    mLogger.Warn("Failed to get terms properties, error message : {0}", e.ToString());
                }
                termsProp.AddChildren(termsList);
                return termsProp;
            }
        }

        public Dictionary<string, object> GetAllTerms(Guid termStoreId, Guid termSetId)
        {
            using (var context = CreateRetryContext())
            {
                Dictionary<string, object> termsProp = new Dictionary<string, object>();
                var termsList = new List<IDictionary<string, object>>();
                try
                {
                    TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                    TermStore store = session.TermStores.GetById(termStoreId);
                    TermSet set = store.GetTermSet(termSetId);
                    TermCollection terms = set.GetAllTerms();
                    context.Load(terms);
                    context.ExecuteQuery();
                    foreach (Term term in terms)
                    {
                        Dictionary<string, object> termProp = new Dictionary<string, object>();
                        CopyProperty(termProp, term);
                        termsList.Add(termProp);
                    }
                }
                catch (Exception e)
                {
                    mLogger.Warn("Get terms failed. TermSetId : {0}, Error message: {1}", termSetId, e.ToString());
                }
                termsProp.AddChildren(termsList);
                return termsProp;
            }
        }
        public string GetDefaultLabel(Guid termStoreId, Guid termId, int defaultID)
        {
            using (var context = CreateRetryContext())
            {
                TaxonomySession session = TaxonomySession.GetTaxonomySession(context);
                TermStore store = session.TermStores.GetById(termStoreId);
                Term term = store.GetTerm(termId);
                ClientResult<string> defaultLabel = term.GetDefaultLabel(defaultID);
                context.ExecuteQuery();
                return defaultLabel.Value;
            }
        }
    }
}
