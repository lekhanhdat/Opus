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
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPSearch
    {
        private AveSPSite mAveParentSite = null;

        public AveSPSite ParentSite
        {
            get { return mAveParentSite; }
        }

        private AveSPWeb mAveWeb = null;
        private SearchLevel mSearchLevel;
        private IAveOSearchServiceApplicationProxy SearchServiceAppProxy = null;
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public AveSPSearch(AveSPSite aveSite)
        {
            mAveParentSite = aveSite;
            mSearchLevel = SearchLevel.site;

            try
            {
                AveServiceContextInfo info = new AveServiceContextInfo
                {
                    Site = mAveParentSite.SPSite,
                    SiteSubscriptionIdentifier = mAveParentSite.ObjectModelFactory.CreateSiteSubscriptionIdentifier().Default,
                    WebApplication = mAveParentSite.SPSite.WebApplication
                };
                IAveServiceContext serviceContext = mAveParentSite.ObjectModelFactory.CreateServerContext(info);
                SearchServiceAppProxy = (IAveOSearchServiceApplicationProxy)serviceContext.GetDefaultProxy(typeof(IAveOSearchServiceApplicationProxy));
                if (SearchServiceAppProxy == null)
                {
                    mLog.Info("Can not get SearchServiceAppProxy, will not backup SearchInfo.");
                }
            }
            catch (Exception e)
            {
                mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while get SearchServiceApplicationProxy. WebUrl:{0}\n error message:{1}", aveSite.SPSite.Url, e));
            }
        }

        public AveSPSearch(AveSPWeb aveWeb)
        {
            mAveWeb = aveWeb;
            mAveParentSite = mAveWeb.ParentSite;
            mSearchLevel = SearchLevel.web;
            try
            {
                AveServiceContextInfo info = new AveServiceContextInfo
                {
                    Site = mAveParentSite.SPSite,
                    SiteSubscriptionIdentifier = mAveParentSite.ObjectModelFactory.CreateSiteSubscriptionIdentifier().Default,
                    WebApplication = mAveParentSite.SPSite.WebApplication
                };
                IAveServiceContext serviceContext = mAveParentSite.ObjectModelFactory.CreateServerContext(info);
                SearchServiceAppProxy = (IAveOSearchServiceApplicationProxy)serviceContext.GetDefaultProxy(typeof(IAveOSearchServiceApplicationProxy));
                if (SearchServiceAppProxy == null)
                {
                    mLog.Info("Can not get SearchServiceAppProxy, will not backup SearchInfo.");
                }
            }
            catch (Exception e)
            {
                mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while get SearchServiceApplicationProxy. WebUrl:{0}\n error message:{1}", mAveWeb.SPWeb.Url, e));
            }
        }

        public AveSearchInfo GetSearchInfo()
        {
            try
            {
                AveSearchInfo aveSearchInfo = new AveSearchInfo();
                if (mSearchLevel == SearchLevel.site)
                {
                    using (AvePerformanceScope pc1 = new AvePerformanceScope("Backup.AveSPSite.SearchInfo"))
                    {
                        AveSPSearchScope searchScope = new AveSPSearchScope(mAveParentSite, SearchServiceAppProxy);
                        aveSearchInfo.AveScopeInfos = searchScope.GetScopeInfo();
                        aveSearchInfo.AveDisplayGroupInfos = searchScope.GetDisplayGroupInfo();

                        AveSPSearchKeywords aveSearchKeywords = new AveSPSearchKeywords(mAveParentSite, SearchServiceAppProxy);
                        aveSearchInfo.AveKeywords = aveSearchKeywords.GetKeyWord();
                    }
                }
                else
                {
                    using (AvePerformanceScope pc1 = new AvePerformanceScope("Backup.AveSPWeb.SearchInfo"))
                    {
                        AveSPSearchScope searchScope = new AveSPSearchScope(mAveWeb, SearchServiceAppProxy);
                        aveSearchInfo.AveScopeInfos = searchScope.GetScopeInfo();
                        aveSearchInfo.AveDisplayGroupInfos = searchScope.GetDisplayGroupInfo();
                    }
                }

                return aveSearchInfo;
            }
            catch (Exception e)
            {
                mLog.Log(AveLogLevel.WARN, "An error occurred while GetSearchInfo. error:{0}", e);
                return null;
            }
        }

        public AveSearchInfo GetSearchInfo13()
        {
             try
            {
                AveSearchInfo aveSearchInfo = new AveSearchInfo();
                if (mSearchLevel == SearchLevel.site)
                {
                    using (AvePerformanceScope pc1 = new AvePerformanceScope("Backup.AveSPSite.SearchInfo"))
                    {
                        ExportSearchScopeInfo(aveSearchInfo);

                        ExportSearchKeywordsInfo(aveSearchInfo);

                        ExportSearchConfigurationInfo(aveSearchInfo);
                    }
                }
                else
                {
                    using (AvePerformanceScope pc1 = new AvePerformanceScope("Backup.AveSPWeb.SearchInfo"))
                    {
                        ExportSearchScopeInfo(aveSearchInfo);

                        ExportSearchConfigurationInfo(aveSearchInfo);
                    }
                }
                return aveSearchInfo;
            }
            catch (Exception e)
            {
                mLog.Log(AveLogLevel.WARN, "An error occurred while GetSearchInfo. error:{0}", e);
                return null;
            }
        }

        private void ExportSearchScopeInfo(AveSearchInfo aveSearchInfo)
        {
            AveSPSearchScope searchScope = (mSearchLevel == SearchLevel.site) ? searchScope = new AveSPSearchScope(mAveParentSite, SearchServiceAppProxy)
                : new AveSPSearchScope(mAveWeb, SearchServiceAppProxy);
            aveSearchInfo.AveScopeInfos = searchScope.GetScopeInfo();
            aveSearchInfo.AveDisplayGroupInfos = searchScope.GetDisplayGroupInfo();
        }

        /// <summary>
        /// 包含SearchQueryConfiguration，SchemaConfiguration 和 BuildInAndSSAQeuryRuleSetting
        /// </summary>
        /// <param name="aveSearchInfo"></param>
        private void ExportSearchConfigurationInfo(AveSearchInfo aveSearchInfo)
        {
            //防止出现空引用         
            if (SearchServiceAppProxy == null)
            {
                return;
            }
            AveObjectModelFactory factory = this.ParentSite.ObjectModelFactory;
            IAveOSearchObjectOwner owner = (mSearchLevel == SearchLevel.site) ? factory.CreateSearchOwner(AveOSearchObjectLevel.SPSite, this.ParentSite.SPSite.RootWeb)
                : factory.CreateSearchOwner(AveOSearchObjectLevel.SPWeb, this.mAveWeb.SPWeb);

            #region QueryConfiguration
            IAveOSearchQueryConfigurationSettings searchQueryConfigurationSettings;
            this.SearchServiceAppProxy.ExportQueryConfiguration(owner, out searchQueryConfigurationSettings);
            aveSearchInfo.SearchQueryConfigurationSettingString = searchQueryConfigurationSettings.SeachConfigurationString;
            #endregion

            #region SchemaConfiguration
            IAveOSearchSchemaConfigurationSettings schemaSetting = this.SearchServiceAppProxy.ExportSchema(owner);
            aveSearchInfo.SchemaConfigurationString = schemaSetting.SearchSchemaSettingString;
            #endregion

            #region Buildin Rule and SSA level Rule COnfiguration
            this.SearchServiceAppProxy.ExportBuildInAndSSAQeuryRuleSetting(owner, aveSearchInfo.BuildInQueryRuleSetting, aveSearchInfo.SSAQueryRuleSetting);
            #endregion
        }

        private void ExportSearchKeywordsInfo(AveSearchInfo aveSearchInfo)
        {
            //only site level have keywords.
            if (mSearchLevel != SearchLevel.site)
            {
                return;
            }
            AveSPSearchKeywords aveSearchKeywords = new AveSPSearchKeywords(mAveParentSite, SearchServiceAppProxy);
            aveSearchInfo.AveKeywords = aveSearchKeywords.GetKeyWord();
        }

        private AveNavigationInfoList ConvertNavNodetoNodeInfo(IAveNavigationNodeCollection SearchNavCollection, AveNavigationScope scope) 
        {
            AveNavigationInfoList NavigationInfoList = new AveNavigationInfoList() 
            {
                SharedTopLink = false,
                ShareQuickLaunch = false,
                BackupFromInheritedWeb = true,//navigation备份时，默认值为true,并且没有修改，这里遵循Navigation
                PublishFeatureAppearance = false //在navigation里已经做过了，这里赋值为false
            };
            foreach(IAveNavigationNode node in SearchNavCollection) 
            {
                AveNavigationInfo navi = new AveNavigationInfo();
                navi.Title = node.Title;
                navi.Scope = scope;
                navi.ParentTitle = node.Parent != null ? node.Parent.Title : "";
                navi.Url = node.Url;
                navi.Eid = node.ID;
                navi.EidParent = node.Parent != null ? node.Parent.ID : 0;
                navi.IsExternal = node.IsExternal;
                NavigationInfoList.NavNodes.Add(navi);
            }
            return NavigationInfoList;
        }

        public void Export(IAveBackupStream output)
        {
            AveSearchInfo searchInfo;
            if (this.mAveParentSite.SPContextKind.IsServerMode13Upper())
            {
                searchInfo = GetSearchInfo13();
            }
            else
            {
                searchInfo = GetSearchInfo();   
            }

            if (searchInfo != null)
            {
                output.WriteMetadata(AveMetadataType.SiteSearchInfo.ToString(), searchInfo);
            }
        }
    }

    public class AveSPSearchScope
    {
        private AveSPSite mAveSite = null;
        private AveSPWeb mAveWeb = null;
        private SearchLevel mSearchLevel;
        private IAveOSearchServiceApplicationProxy SearchServiceAppProxy = null;
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public AveSPSearchScope(AveSPSite aveSite, IAveOSearchServiceApplicationProxy searchServiceAppProxy)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPSearchScope.Constructor"))
            {
                mAveSite = aveSite;
                mSearchLevel = SearchLevel.site;
                if (searchServiceAppProxy != null)
                {
                    SearchServiceAppProxy = searchServiceAppProxy;
                }
                else
                {
                    try
                    {
                        AveServiceContextInfo info = new AveServiceContextInfo
                        {
                            Site = aveSite.SPSite,
                            SiteSubscriptionIdentifier = aveSite.ObjectModelFactory.CreateSiteSubscriptionIdentifier().Default,
                            WebApplication = aveSite.SPSite.WebApplication
                        };
                        IAveServiceContext serviceContext = aveSite.ObjectModelFactory.CreateServerContext(info);

                        SearchServiceAppProxy = (IAveOSearchServiceApplicationProxy)serviceContext.GetDefaultProxy(typeof(IAveOSearchServiceApplicationProxy));
                    }
                    catch (Exception e)
                    {
                        mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while get SearchServiceApplicationProxy. Site Url:{0}\n error message:{1}", mAveSite.SPSite.Url, e));
                    }
                }
            }
        }

        public AveSPSearchScope(AveSPWeb aveWeb, IAveOSearchServiceApplicationProxy searchServiceAppProxy)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPSearchScope.Constructor"))
            {
                mAveWeb = aveWeb;
                mAveSite = mAveWeb.ParentSite;
                mSearchLevel = SearchLevel.web;
                if (searchServiceAppProxy != null)
                {
                    SearchServiceAppProxy = searchServiceAppProxy;
                }
                else
                {
                    try
                    {
                        AveServiceContextInfo info = new AveServiceContextInfo
                        {
                            Site = mAveSite.SPSite,
                            SiteSubscriptionIdentifier = mAveSite.ObjectModelFactory.CreateSiteSubscriptionIdentifier().Default,
                            WebApplication = mAveSite.SPSite.WebApplication
                        };
                        IAveServiceContext serviceContext = mAveSite.ObjectModelFactory.CreateServerContext(info);

                        SearchServiceAppProxy = (IAveOSearchServiceApplicationProxy)serviceContext.GetDefaultProxy(typeof(IAveOSearchServiceApplicationProxy));
                    }
                    catch (Exception e)
                    {
                        mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while get SearchServiceApplicationProxy. WebUrl:{0}\n error message:{1}", mAveWeb.SPWeb.Url, e));
                    }
                }
            }
        }

        public List<AveScopeInfo> GetScopeInfo()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPSearchScope.GetScopeInfo"))
            {
                List<AveScopeInfo> aveScopeInfos = new List<AveScopeInfo>();
                if (SearchServiceAppProxy == null)
                {
                    return null;
                }
                string scopeConsumerName = string.Empty;
                if (mSearchLevel == SearchLevel.site)
                {
                    scopeConsumerName = mAveSite.SPSite.ID.ToString();
                }
                else
                {
                    scopeConsumerName = mAveWeb.SPWeb.ID.ToString();
                }
                List<IAveOScopeInfo> scopeInfos = SearchServiceAppProxy.GetScopesInfo();
                try
                {
                    #region 备份Scopes And Rules信息

                    foreach (IAveOScopeInfo info in scopeInfos)
                    {
                        try
                        {
                            string consumerName = string.Empty;
                            if (mSearchLevel == SearchLevel.site)
                            {
                                consumerName = info.ConsumerName;
                            }
                            else
                            {
                                consumerName = info.Name;
                            }
                            if (consumerName.Equals(scopeConsumerName, StringComparison.OrdinalIgnoreCase))
                            {
                                AveScopeInfo aveScopeInfo = new AveScopeInfo();
                                aveScopeInfo.AlternateResultsPage = info.AlternateResultsPage;
                                aveScopeInfo.CompilationState = info.CompilationState.ToString();
                                aveScopeInfo.CompilationType = info.CompilationType.ToString();
                                aveScopeInfo.ConsumerName = info.ConsumerName;
                                aveScopeInfo.Description = info.Description;
                                aveScopeInfo.DisplayInAdminUI = info.DisplayInAdminUI;
                                aveScopeInfo.Filter = info.Filter;
                                aveScopeInfo.Id = info.ID;
                                aveScopeInfo.IsDeleted = info.IsDeleted;
                                aveScopeInfo.LastCompilationTime = info.LastCompilationTime;
                                aveScopeInfo.LastModifiedBy = info.LastModifiedBy;
                                aveScopeInfo.LastModifiedTime = info.LastModifiedTime;
                                aveScopeInfo.Name = info.Name;
                                aveScopeInfo.SiteUrl = info.SiteUrl;

                                int statusCode = 0;
                                List<IAveORuleInfo> ruleInfos = SearchServiceAppProxy.GetRulesInfo(info.ID, out statusCode);
                                foreach (IAveORuleInfo ruleInfo in ruleInfos)
                                {
                                    try
                                    {
                                        AveRuleInfo aveRuleInfo = new AveRuleInfo();
                                        aveRuleInfo.FilterBehavior = ruleInfo.FilterBehavior.ToString();
                                        aveRuleInfo.Id = ruleInfo.ID;
                                        aveRuleInfo.IsDeleted = ruleInfo.IsDeleted;
                                        if (ruleInfo.ManagedProperty != null)
                                        {
                                            AveManagedPropertyInfo aveManagedPropertyInfo = new AveManagedPropertyInfo();
                                            aveManagedPropertyInfo.EnabledForScoping = ruleInfo.ManagedProperty.EnabledForScoping;
                                            aveManagedPropertyInfo.ManagedType = ruleInfo.ManagedProperty.ManagedType.ToString();
                                            aveManagedPropertyInfo.Name = ruleInfo.ManagedProperty.Name;
                                            aveManagedPropertyInfo.Pid = ruleInfo.ManagedProperty.Pid;
                                            aveRuleInfo.ManagedProperty = aveManagedPropertyInfo;
                                        }
                                        aveRuleInfo.RuleType = ruleInfo.RuleType.ToString();
                                        aveRuleInfo.UrlRuleType = ruleInfo.UrlRuleType.ToString();
                                        aveRuleInfo.UserValue = ruleInfo.UserValue;

                                        aveScopeInfo.AveRuleInfos.Add(aveRuleInfo);
                                    }
                                    catch (Exception e)
                                    {
                                        mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while backup the scope's ruleInfo.scope id:{0}, scope name:{1}, rule id:{2}\n error message:{3}", info.ID, info.Name, ruleInfo.ID, e));
                                    }
                                }
                                aveScopeInfos.Add(aveScopeInfo);
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while backup the ScopeInfo. scope id:{0}, scope name:{1}\n error message:{2}", info.Name, info.ID, e));
                        }
                    }

                    #endregion 备份Scopes And Rules信息
                }
                catch (Exception e)
                {
                    mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while get site searchScope. Scope consumerName:{0}\n error message:{1}", scopeConsumerName, e));
                    return null;
                }
                return aveScopeInfos;
            }
        }

        public List<AveDisplayGroupInfo> GetDisplayGroupInfo()
        {
                List<AveDisplayGroupInfo> aveDisplayGroupInfos = new List<AveDisplayGroupInfo>();
                if (SearchServiceAppProxy == null)
                {
                    return null;
                }
                string scopeConsumerName = string.Empty;
                if (mSearchLevel == SearchLevel.site)
                {
                    scopeConsumerName = mAveSite.SPSite.ID.ToString();
                }
                else
                {
                    scopeConsumerName = mAveWeb.SPWeb.ID.ToString();
                }
                try
                {
                    #region 备份display group信息

                    List<IAveODisplayGroupInfo> groupInfos = SearchServiceAppProxy.GetDisplayGroupsInfo();
                    foreach (IAveODisplayGroupInfo groupInfo in groupInfos)
                    {
                        try
                        {
                            string consumerName = groupInfo.ConsumerName;
                            if (consumerName.Equals(scopeConsumerName, StringComparison.OrdinalIgnoreCase))
                            {
                                AveDisplayGroupInfo aveDisplayGroupInfo = new AveDisplayGroupInfo();
                                aveDisplayGroupInfo.ConsumerName = groupInfo.ConsumerName;
                                aveDisplayGroupInfo.DefaultScopeName = SearchServiceAppProxy.GetScopeInfo(groupInfo.DefaultScopeID).Name;
                                aveDisplayGroupInfo.Description = groupInfo.Description;
                                aveDisplayGroupInfo.DisplayInAdminUI = groupInfo.DisplayInAdminUI;
                                aveDisplayGroupInfo.Id = groupInfo.ID;
                                aveDisplayGroupInfo.IsDeleted = groupInfo.IsDeleted;
                                aveDisplayGroupInfo.IsUndeletable = groupInfo.IsUndeletable;
                                aveDisplayGroupInfo.LastModifiedBy = groupInfo.LastModifiedBy;
                                aveDisplayGroupInfo.LastModifiedTime = groupInfo.LastModifiedTime;
                                aveDisplayGroupInfo.Name = groupInfo.Name;
                                aveDisplayGroupInfo.SiteUrl = groupInfo.SiteUrl;

                                List<int> members = SearchServiceAppProxy.GetDisplayGroupListInfo(groupInfo.ID);
                                for (int i = 0; i < members.Count; i++)
                                {
                                    IAveOScopeInfo tempScopeInfo = SearchServiceAppProxy.GetScopeInfo(members[i]);
                                    AveDisplayGroupMember groupMember = new AveDisplayGroupMember();
                                    groupMember.Name = tempScopeInfo.Name;
                                    groupMember.Description = tempScopeInfo.Description;
                                    aveDisplayGroupInfo.AveDisplayGroupMembers.Add(groupMember);
                                }
                                aveDisplayGroupInfos.Add(aveDisplayGroupInfo);
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while backup the display groupInfo. group name:{0},group url:{1},group id:{2}\n error message:{3}", groupInfo.Name, groupInfo.SiteUrl, groupInfo.ID, e));
                        }
                    }

                    #endregion 备份display group信息
                }
                catch (Exception e)
                {
                    mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while get site Scope DisplayGroups. scope consumerName:{0}\n error message:{1}", scopeConsumerName, e));
                    //mLog.Warn(e, "An error occurred when get site Scope DisplayGroups. scope consumerName:{0}", scopeConsumerName);
                    return null;
                }
                return aveDisplayGroupInfos;
        }

        public AveSearchScopeInfo GetSearchScopeInfo()
        {
            AveSearchScopeInfo aveSearchScopeInfo = new AveSearchScopeInfo();
            aveSearchScopeInfo.AveScopeInfos = GetScopeInfo();
            aveSearchScopeInfo.AveDisplayGroupInfos = GetDisplayGroupInfo();
            return aveSearchScopeInfo;
        }

        public void Export(IAveBackupStream output)
        {
            output.WriteMetadata(AveMetadataType.SearchScope.ToString(), GetSearchScopeInfo());
        }
    }

    public class AveSPSearchKeywords
    {
        private AveSPSite mAveParentSite = null;

        public AveSPSite ParentSite
        {
            get { return mAveParentSite; }
        }

        private IAveOSearchServiceApplicationProxy SearchServiceAppProxy = null;
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public AveSPSearchKeywords(AveSPSite aveSite, IAveOSearchServiceApplicationProxy searchServiceAppProxy)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPSearchKeywords.Constructor"))
            {
                mAveParentSite = aveSite;
                if (searchServiceAppProxy != null)
                {
                    SearchServiceAppProxy = searchServiceAppProxy;
                }
                else
                {
                    try
                    {
                        IAveServiceContext serviceContext = mAveParentSite.ObjectModelFactory.CreateServiceContext().GetContext(mAveParentSite.SPSite.WebApplication.ServiceApplicationProxyGroup, mAveParentSite.ObjectModelFactory.CreateSiteSubscriptionIdentifier().Default);
                        SearchServiceAppProxy = (IAveOSearchServiceApplicationProxy)serviceContext.GetDefaultProxy(typeof(IAveOSearchServiceApplicationProxy));
                    }
                    catch (Exception e)
                    {
                        mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while get SearchServiceApplicationProxy. Site Url:{0}\n error message:{1}", mAveParentSite.SPSite.Url, e));
                        //mLog.Warn(e, "An error ocurred when get SearchServiceApplicationProxy.siteUrl:{0}", mAveSite.SPSite.Url);
                    }
                }
            }
        }

        public List<AveKeyword> GetKeyWord()
        {
            List<AveKeyword> aveKeywords = new List<AveKeyword>();
            IAveSite site = mAveParentSite.SPSite;
            if (SearchServiceAppProxy == null)
            {
                return null;
            }

            #region 备份keywords信息

            try
            {
                Uri siteUrl = new Uri(site.Url);
                IAveOKeywords words = mAveParentSite.ObjectModelFactory.CreateKeywords(SearchServiceAppProxy, siteUrl);
                foreach (IAveOKeyword word in words.AllKeywords)
                {
                    try
                    {
                        AveKeyword aveKeyword = new AveKeyword();

                        foreach (IAveOBestBet bet in word.BestBets)
                        {
                            AveBestBet aveBestBet = new AveBestBet();
                            aveBestBet.Description = bet.Description;
                            aveBestBet.Title = bet.Title;
                            aveBestBet.Url = bet.Url.ToString();
                            aveKeyword.BestBets.Add(aveBestBet);
                        }
                        aveKeyword.Contact = word.Contact;
                        aveKeyword.Definition = word.Definition;
                        aveKeyword.EndDate = word.EndDate;
                        aveKeyword.ReviewDate = word.ReviewDate;
                        aveKeyword.StartDate = word.StartDate;
                        foreach (IAveOSynonym sysn in word.Synonyms)
                        {
                            AveSynonym avesynonym = new AveSynonym();
                            avesynonym.Term = sysn.Term;
                            aveKeyword.Synonyms.Add(avesynonym);
                        }
                        aveKeyword.Term = word.Term;

                        aveKeywords.Add(aveKeyword);
                    }
                    catch (Exception e)
                    {
                        mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while backup the keyword info. word definition:{0}\n error message:{1}", word.Definition, e));
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while get site SearchKeyWords. SiteUrl:{0}\n error message:{1}", site.Url, e));
                //mLog.Warn(e, "An error occurred when get site SearchKeyWords. siteUrl:{0}", site.Url);
                return null;
            }

            #endregion 备份keywords信息

            return aveKeywords;
        }

        public void Export(IAveBackupStream output)
        {
            output.WriteMetadata(AveMetadataType.SearchKeywords.ToString(), GetKeyWord());
        }
    }

    public enum SearchLevel
    {
        site = 1,
        web = 2,
    }
}