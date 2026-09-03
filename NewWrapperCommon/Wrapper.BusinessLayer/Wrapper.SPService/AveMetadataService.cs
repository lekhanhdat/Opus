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
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Resource.SPService;

namespace AvePoint.Wrapper.SPService
{
    public class AveMetadataService
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveServiceContext mServiceContext;
        private int DefaultLCID = 1033;

        public Dictionary<string, string> TermStoreMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<Guid, Guid> TermStoreIdMapping = new Dictionary<Guid, Guid>();
        public Dictionary<Guid, Guid> TermGroupIdMapping = new Dictionary<Guid, Guid>();
        public Dictionary<Guid, Guid> TermSetIdMapping = new Dictionary<Guid, Guid>();
        public Dictionary<Guid, Guid> TermIdMapping = new Dictionary<Guid, Guid>();

        public AveMetadataService(AveServiceContext serviceContext)
        {
            mServiceContext = serviceContext;
        }

        public void Restore(List<AveTermStoreInfo> termStoreInfos)
        {
            try
            {
                log.Debug(string.Format("Before Restore MetadataService Time:{0}", DateTime.Now.ToString()));
                IAveTaxonomySession session = mServiceContext.TaxonomySession; ;
                if (session.TermStores.Count <= 0)
                {
                    log.Warn("The Destination did not relative to metadata service.");
                    return;
                }
                OutputDestServiceInfo(session);
                foreach (AveTermStoreInfo termStoreInfo in termStoreInfos)
                {
                    IAveTermStore termStore = RestoreTermStore(session, termStoreInfo);
                    if (!TermStoreIdMapping.ContainsKey(termStoreInfo.Id))
                    {
                        TermStoreIdMapping.Add(termStoreInfo.Id, termStore.ID);
                    }
                }
                log.Debug(string.Format("After Restore MetadataService Time:{0}", DateTime.Now.ToString()));
                OutputDebugServiceInfo();
            }
            catch (Exception e)
            {
                log.Warn(string.Format("An error occurred while Restore AveMetadataService. error:{0}", e.ToString()));
            }
        }

        public void OutputDestServiceInfo(IAveTaxonomySession session)
        {
            StringBuilder info = new StringBuilder();
            info.Append("Destination TermStores:");
            foreach (IAveTermStore store in session.TermStores)
            {
                info.AppendLine(store.Name);
            }
            log.Info(info.ToString());
        }

        public void OutputDebugServiceInfo()
        {
            StringBuilder info = new StringBuilder();
            info.AppendLine("TermStoreMapping:");
            foreach (KeyValuePair<string, string> pair in TermStoreMapping)
            {
                info.AppendLine(pair.Key + " -> " + pair.Value);
            }
            info.AppendLine("TermStoreIdMapping:");
            foreach (KeyValuePair<Guid, Guid> pair in TermStoreIdMapping)
            {
                info.AppendLine(pair.Key.ToString() + " -> " + pair.Value.ToString());
            }
            info.AppendLine("TermGroupIdMapping:");
            foreach (KeyValuePair<Guid, Guid> pair in TermGroupIdMapping)
            {
                info.AppendLine(pair.Key.ToString() + " -> " + pair.Value.ToString());
            }
            info.AppendLine("TermSetIdMapping:");
            foreach (KeyValuePair<Guid, Guid> pair in TermSetIdMapping)
            {
                info.AppendLine(pair.Key.ToString() + " -> " + pair.Value.ToString());
            }
            info.AppendLine("TermIdMapping:");
            foreach (KeyValuePair<Guid, Guid> pair in TermIdMapping)
            {
                info.AppendLine(pair.Key.ToString() + " -> " + pair.Value.ToString());
            }
            log.Debug(info.ToString());
        }

        /// <summary>
        /// group的Group Managers，Contributors和termset的Owner，Stakeholders，可能带有类似i:0#.w|的头，
        /// 当是AD group的时候，用API取出来的account是Sid格式的，但是添加的时候需要用Account格式。
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static string EnsureAccountName(string account, AveObjectModelFactory modelFactory)
        {
            if (account.IndexOf('|') > 0)
            {
                account = account.Substring(account.IndexOf('|') + 1);
            }
            if (AveDirectoryServiceUtility.IsStringSid(account))
            {
                account = AveDirectoryServiceUtility.GetAccountFromSid(account, modelFactory);
            }
            return account;
        }

        public IAveTermStore RestoreTermStore(IAveTaxonomySession session, AveTermStoreInfo termStoreInfo)
        {
            string termStoreName = termStoreInfo.Name;
            if (TermStoreMapping != null && TermStoreMapping.ContainsKey(termStoreName))
            {
                termStoreName = TermStoreMapping[termStoreName];
            }
            IAveTermStore termStore = null;
            try
            {
                termStore = session.TermStores[termStoreName];
            }
            catch(Exception e)
            {
                log.Log(AveLogLevel.INFO,WrapperSPServiceResource.CannotGetTermStore, termStoreName,e);
            }
            //TODO: check if the metadata service enable local term
            //获取不到，使用DefaultSiteCollectionTermStore
            if (termStore == null)
            {
                termStore = session.DefaultKeywordsTermStore;
            }
            if (termStore == null)
            {
                termStore = session.DefaultSiteCollectionTermStore;
            }
            if (termStore == null)
            {
                termStore = session.TermStores[0];
            }
            DefaultLCID = termStore.DefaultLanguage;

            foreach (AveMetadataGroupInfo groupInfo in termStoreInfo.Groups)
            {
                IAveTaxonomyGroup group = RestoreMetadataGroup(termStore, groupInfo);
                if (group != null && !TermGroupIdMapping.ContainsKey(groupInfo.Id))
                {
                    TermGroupIdMapping.Add(groupInfo.Id, group.ID);
                }
            }
            return termStore;
        }

        public IAveTaxonomyGroup RestoreMetadataGroup(IAveTermStore termStore, AveMetadataGroupInfo groupInfo)
        {
            string groupName = groupInfo.Name;
            IAveTaxonomyGroup group = null;
            bool isNewCreated = false;
            try
            {

                if (groupInfo.IsSiteCollectionGroup)
                {
                    bool newCreatedGroup = false;
                    try
                    {
                        //处理删除sitecollection做inplace还原的时候，由于删除的sitecollection的local group仍然存在，导致无法新建的问题
                        string siteCollectionGroupName = termStore.GetSiteCollectionGroupName(mServiceContext.Site);//(string)Invoker.CallMethod(termStore, "GetSiteCollectionGroupName", new Type[] { mSite.GetType() }, new object[] { mSite });
                        group = termStore.Groups[siteCollectionGroupName];
                        if (group.IsSiteCollectionGroup && !group.SiteCollectionAccessIds.Contains(mServiceContext.Site.ID))
                        {
                            for (int i = group.TermSets.Count - 1; i >= 0; i--)
                            {
                                group.TermSets[i].Delete();
                            }
                            group.Delete();
                            termStore.CommitAll();
                            newCreatedGroup = true;
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, WrapperSPServiceResource.CannotDeleteMetadataGroup, e);
                        newCreatedGroup = true;
                    }
                    group = termStore.GetSiteCollectionGroup(mServiceContext.Site);
                    //(IAveTaxonomyGroup)Invoker.CallMethod(termStore, "GetSiteCollectionGroup", new Type[] { mSite.GetType() }, new object[] { mSite });
                    if (newCreatedGroup)
                    {
                        UpdateMetadataGroup(group, groupInfo);
                    }
                    //TODO...need to implement in server mode.
                    //group = (IAveTaxonomyGroup)Invoker.CallMethod(termStore, "GetSiteCollectionGroup", new Type[] { mSite.GetType() }, new object[] { mSite });
                }
                else
                {
                    group = termStore.Groups[groupInfo.Name];
                }
            }
            catch (AveException e)
            {
                log.Warn("An error occurred while create Site Group" + e.InnerException.ToString());
            }
            catch(Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperSPServiceResource.RestoreMetadataGroupError, e.ToString());
                //IsSystemGroup和IsSiteCollectionGroup属性都是只读属性。此处处理为，如果源端IsSystemGroup或IsSiteCollectionGroup等于true，先找对应的Group。
                if (groupInfo.IsSystemGroup && termStore.SystemGroup != null)
                {
                    group = termStore.SystemGroup;
                }
                if (group == null)
                {
                    group = CreateMetadataGroup(termStore, groupInfo);
                    if (group == null)
                    {
                        return null;
                    }
                    isNewCreated = true;
                }
            }
            foreach (AveTermSetInfo termSetInfo in groupInfo.TermSets)
            {
                IAveTermSet termSet = RestoreTermSet(group, termSetInfo, isNewCreated);
                if (termSet != null && !TermSetIdMapping.ContainsKey(termSetInfo.Id))
                {
                    TermSetIdMapping.Add(termSetInfo.Id, termSet.ID);
                }
            }
            return group;
        }

        private void UpdateMetadataGroup(IAveTaxonomyGroup group, AveMetadataGroupInfo groupInfo)
        {
            try
            {
                group.Description = groupInfo.Description;
                foreach (AveAceInfo groupManager in groupInfo.GroupManagers)
                {
                    string principalName = groupManager.PrincipalName;
                    if (principalName.Contains('|'))
                    {
                        principalName = principalName.Substring(principalName.IndexOf('|') + 1);
                    }
                    group.AddGroupManager(principalName);
                }
                foreach (AveAceInfo contributor in groupInfo.Contributors)
                {
                    string principalName = contributor.PrincipalName;
                    if (principalName.Contains('|'))
                    {
                        principalName = principalName.Substring(principalName.IndexOf('|') + 1);
                    }
                    group.AddContributor(principalName);
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while update group property. groupName:{0}, error:{1}", group.Name, e.ToString());
            }
            group.TermStore.CommitAll();
        }

        public IAveTaxonomyGroup CreateMetadataGroup(IAveTermStore termStore, AveMetadataGroupInfo groupInfo)
        {
            try
            {
                IAveTaxonomyGroup group = termStore.CreateGroup(groupInfo.Name);
                try
                {
                    group.Description = groupInfo.Description;
                    foreach (AveAceInfo groupManager in groupInfo.GroupManagers)
                    {
                        string principalName = groupManager.PrincipalName;
                        group.AddGroupManager(principalName);
                        try
                        {
                            principalName = EnsureAccountName(principalName, mServiceContext.OMFactory);
                            group.AddGroupManager(principalName);
                        }
                        catch (Exception e)
                        {
                            log.Warn("An error occurred while add group manager. principalName:{0}, error:{1}", principalName, e.ToString());
                        }
                    }
                    foreach (AveAceInfo contributor in groupInfo.Contributors)
                    {
                        string principalName = contributor.PrincipalName;
                        try
                        {
                            principalName = EnsureAccountName(principalName, mServiceContext.OMFactory);
                            group.AddContributor(principalName);
                        }
                        catch (Exception e)
                        {
                            log.Warn("An error occurred while add group contributor. principalName:{0}, error:{1}", principalName, e.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Warn(string.Format("An error occurred while set new create group property. groupName:{0}, error:{1}", groupInfo.Name, e.ToString()));
                }
                group.TermStore.CommitAll();
                return group;
            }
            catch (Exception e)
            {
                log.Warn(string.Format("An error occurred while create term Group. group Name:{0}, error:{1}", groupInfo.Name, e.ToString()));
                return null;
            }
        }

        public IAveTermSet RestoreTermSet(IAveTaxonomyGroup group, AveTermSetInfo termSetInfo, bool isNewCreatedGroup)
        {
            string termSetName = termSetInfo.Name;
            IAveTermSet termSet = null;
            bool isNewCreated = false;
            if (!isNewCreatedGroup)
            {
                try
                {
                    termSet = group.TermSets[termSetName];
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperSPServiceResource.GetTermByNameError, e.ToString());
                }
            }
            if (termSet == null)
            {
                termSet = CreateTermSet(group, termSetInfo);
                if (termSet == null)
                {
                    return null;
                }
                isNewCreated = true;
            }
            foreach (AveTermInfo termInfo in termSetInfo.Terms)
            {
                IAveTerm term = RestoreTerm(termSet, termInfo, isNewCreated);
                if (term != null && !TermIdMapping.ContainsKey(termInfo.Id))
                {
                    TermIdMapping.Add(termInfo.Id, term.ID);
                }
            }
            return termSet;
        }

        public IAveTermSet CreateTermSet(IAveTaxonomyGroup group, AveTermSetInfo termSetInfo)
        {
            try
            {
                IAveTermSet termSet = group.CreateTermSet(termSetInfo.Name, termSetInfo.Id);
                try
                {
                    termSet.Description = termSetInfo.Description;
                    //termSet.Owner = termSetInfo.Owner;
                    string owner = termSetInfo.Owner;
                    try
                    {
                        owner = EnsureAccountName(owner, mServiceContext.OMFactory);
                        termSet.Owner = owner;
                    }
                    catch (Exception e)
                    {
                        log.Warn("An error occurred while set term set owner. term set:{0}, owner:{1}.error:{2}", termSetInfo.Name, owner, e.ToString());
                    }

                    termSet.Contact = termSetInfo.Contact;
                    termSet.IsOpenForTermCreation = termSetInfo.IsOpenForTermCreation;
                    termSet.IsAvailableForTagging = termSetInfo.IsAvailableForTagging;
                    foreach (string stakeHolder in termSetInfo.Stakeholders)
                    {
                        string tStakeHolder = stakeHolder;
                        try
                        {
                            tStakeHolder = EnsureAccountName(stakeHolder, mServiceContext.OMFactory);
                            termSet.AddStakeholder(tStakeHolder);
                        }
                        catch (Exception e)
                        {
                            log.Warn("An error occurred while add term set stakeholder. term set:{0}, stakeholder:{1}. error:{2}.", termSetInfo.Name, stakeHolder, e.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Warn(string.Format("An error occurred while set term set property. term set name:{0}, error:{1}", termSetInfo.Name, e.ToString()));
                }
                termSet.TermStore.CommitAll();
                return termSet;
            }
            catch (Exception e)
            {
                log.Warn(string.Format("An error occurred while create term set. term set Name:{0}, error:{1}", termSetInfo.Name, e.ToString()));
                return null;
            }
        }

        public IAveTerm RestoreTerm(IAveTermSet termSet, AveTermInfo termInfo, bool isNewCreatedTermSet)
        {
            string termName = termInfo.Name;
            IAveTerm term = null;
            bool isNewCreated = false;
            if (!isNewCreatedTermSet)
            {
                try
                {
                    term = termSet.Terms[termName];
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperSPServiceResource.GetTermByNameError, e.ToString());
                }
            }
            if (term == null)
            {
                term = CreateTerm(termSet, termInfo);
                if (term == null)
                {
                    return null;
                }
                isNewCreated = true;
            }
            foreach (AveTermInfo subTerm in termInfo.Terms)
            {
                IAveTerm sTerm = RestoreSubTerm(term, subTerm, isNewCreated);
                if (sTerm != null && !TermIdMapping.ContainsKey(subTerm.Id))
                {
                    TermIdMapping.Add(subTerm.Id, sTerm.ID);
                }
            }
            return term;
        }

        public IAveTerm CreateTerm(IAveTermSet termSet, AveTermInfo termInfo)
        {
            try
            {

                IAveTerm term = termSet.CreateTerm(termInfo.TermName, DefaultLCID);
                try
                {
                    SetTermDescription(term, termInfo.Description);
                    string owner = termInfo.Owner;
                    try
                    {
                        owner = EnsureAccountName(owner, mServiceContext.OMFactory);
                        term.Owner = owner;
                    }
                    catch (Exception e)
                    {
                        log.Warn("An error occurred while set term owner. term:{0}, owner:{1}.error:{2}", termInfo.Name, owner, e.ToString());
                    }
                    term.IsAvailableForTagging = termInfo.IsAvailableForTagging;
                    foreach (AveLableInfo labelInfo in termInfo.Labels)
                    {
                        if (labelInfo.Value.Equals(term.Name))
                        {
                            continue;
                        }
                        term.CreateLabel(labelInfo.Value, labelInfo.Language, labelInfo.IsDefaultForLanguage);
                    }
                }
                catch (Exception e)
                {
                    log.Warn(string.Format("An error occurred while set term property. termName:{0}, error:{1}", termInfo.Name, e.ToString()));
                }
                term.TermStore.CommitAll();
                return term;
            }
            catch (Exception e)
            {
                log.Warn(string.Format("An error occurred while Create term. term Name:{0}, error:{1}", termInfo.Name, e.ToString()));
                return null;
            }
        }

        public IAveTerm RestoreSubTerm(IAveTerm term, AveTermInfo termInfo, bool isNewCreatedTerm)
        {
            string termName = termInfo.Name;
            IAveTerm sTerm = null;
            bool isNewCreated = false;
            if (!isNewCreatedTerm)
            {
                try
                {
                    sTerm = term.Terms[termName];
                }
                catch(Exception e) 
                {
                    log.Log(AveLogLevel.DEBUG, WrapperSPServiceResource.GetTermByNameError, e.ToString());
                }
            }
            if (sTerm == null)
            {
                sTerm = CreateSubTerm(term, termInfo);
                if (sTerm == null)
                {
                    return null;
                }
                isNewCreated = true;
            }
            foreach (AveTermInfo subTerm in termInfo.Terms)
            {
                IAveTerm ssTerm = RestoreSubTerm(sTerm, subTerm, isNewCreated);
                if (ssTerm != null && !TermIdMapping.ContainsKey(subTerm.Id))
                {
                    TermIdMapping.Add(subTerm.Id, ssTerm.ID);
                }
            }
            return sTerm;
        }

        public IAveTerm CreateSubTerm(IAveTerm term, AveTermInfo termInfo)
        {
            try
            {
                IAveTerm sTerm = term.CreateTerm(termInfo.Name, DefaultLCID);
                try
                {
                    SetTermDescription(term, termInfo.Description);
                    //sTerm.Owner = termInfo.Owner;
                    string owner = termInfo.Owner;
                    try
                    {
                        owner = EnsureAccountName(owner,  mServiceContext.OMFactory);
                        sTerm.Owner = owner;
                    }
                    catch (Exception e)
                    {
                        log.Warn("An error occurred while set sub term owner. term:{0}, owner:{1}.error:{2}", termInfo.Name, owner, e.ToString());
                    }
                    sTerm.IsAvailableForTagging = termInfo.IsAvailableForTagging;
                    foreach (AveLableInfo labelInfo in termInfo.Labels)
                    {
                        if (labelInfo.Value.Equals(sTerm.Name))
                        {
                            continue;
                        }
                        sTerm.CreateLabel(labelInfo.Value, labelInfo.Language, labelInfo.IsDefaultForLanguage);
                    }
                }
                catch (Exception e)
                {
                    log.Warn(string.Format("An error occurred while set subTerm property. subTermName:{0}, error:{1}", termInfo.Name, e.ToString()));
                }
                sTerm.TermStore.CommitAll();
                return sTerm;
            }
            catch (Exception e)
            {
                log.Warn(string.Format("An error occurred while create subTerm. term Name:{0}, error:{1}", termInfo.Name, e.ToString()));
                return null;
            }
        }

        private void SetTermDescription(IAveTerm term, Dictionary<int, string> descriptionDic)
        {
            if (descriptionDic != null && descriptionDic.Count > 0)
            {
                string description;
                foreach (int lcid in term.TermStore.Languages)
                {
                    if (descriptionDic.TryGetValue(lcid, out description) && !String.IsNullOrEmpty(description))
                    {
                        term.SetDescription(description, lcid);
                    }
                }
            }
        }
    }
}
