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
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    public class AveSPSearch : AvePoint.Wrapper.Restore.IAveSPSearch, IDisposable
    {
        private AveSPSite mAveSite = null;
        private AveSPWeb mAveWeb = null;
        private SearchLevel mSearchLevel;
        private IAveOSearchServiceApplicationProxy SearchServiceAppProxy = null;
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private bool IsOverWrite = true;
        private IReport report = new AveWrapperReport();

        public AveSPSearch(AveSPSite aveSite)
        {
            mAveSite = aveSite;
            mSearchLevel = SearchLevel.site;

            try
            {
                AveServiceContextInfo info = new AveServiceContextInfo
                {
                    Site = mAveSite.SPSite,
                    WebApplication = mAveSite.SPSite.WebApplication,
                    SiteSubscriptionIdentifier = mAveSite.ObjectModelFactory.CreateSiteSubscriptionIdentifier().Default
                };

                IAveServiceContext serviceContext = mAveSite.ObjectModelFactory.CreateServerContext(info);
                SearchServiceAppProxy = (IAveOSearchServiceApplicationProxy)serviceContext.GetDefaultProxy(typeof(IAveOSearchServiceApplicationProxy));
                if (SearchServiceAppProxy == null)
                {
                    mLog.Info("Can not get SearchServiceAppProxy, will not restore SearchInfo.");
                }
            }
            catch (Exception e)
            {

                mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while get SearchServiceApplicationProxy. SiteUrl:{0}\n error message:{1}", mAveSite.SPSite.Url, e));
            }

        }

        public AveSPSearch(AveSPWeb aveWeb)
        {
            mAveWeb = aveWeb;
            mAveSite = mAveWeb.ParentSite;
            mSearchLevel = SearchLevel.web;


            try
            {
                AveServiceContextInfo info = new AveServiceContextInfo
                {
                    Site = mAveSite.SPSite,
                    WebApplication = mAveSite.SPSite.WebApplication,
                    SiteSubscriptionIdentifier = mAveSite.ObjectModelFactory.CreateSiteSubscriptionIdentifier().Default
                };

                IAveServiceContext serviceContext = mAveSite.ObjectModelFactory.CreateServerContext(info);

                SearchServiceAppProxy = (IAveOSearchServiceApplicationProxy)serviceContext.GetDefaultProxy(typeof(IAveOSearchServiceApplicationProxy));
                if (SearchServiceAppProxy == null)
                {
                    mLog.Info("Can not get SearchServiceAppProxy, will not restore SearchInfo.");
                }
            }
            catch (Exception e)
            {
                mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while get SearchServiceApplicationProxy. WebUrl:{0}\n error message:{1}", mAveWeb.SPWeb.Url, e));
                //mLog.Warn(e, "An error ocurred when get SearchServiceApplicationProxy.webUrl:{0}", mAveSite.SPSite.Url);
            }

        }

        public IReport GetReport()
        {
            return report;
        }

        public void Restore(AveSearchInfo aveSearchInfo)
        {
            try
            {
                if (this.mAveSite.SPContextKind != AveContextKind.ClientObjectModel)
                {
                    if (mSearchLevel == SearchLevel.site)
                    {

                        using (AvePerformanceScope pc = new AvePerformanceScope("Restore.Site.Search"))
                        {
                            RestoreSearchScope(aveSearchInfo);

                            RestoreSearchKeywords(aveSearchInfo);

                            RestoreSearchConfiguration(aveSearchInfo);

                        }

                    }
                    else
                    {

                        using (AvePerformanceScope pc = new AvePerformanceScope("Restore.Web.Search"))
                        {
                            RestoreSearchScope(aveSearchInfo);

                            RestoreSearchConfiguration(aveSearchInfo);

                        }

                    }
                }
            }
            catch (AveSecurityTrimingException ex)
            {
                if (mSearchLevel == SearchLevel.web)
                {
                    mLog.Warn("An error occurred while restore Web AveSPSearch. error:{0}", ex.ToString());
                    report.AddDetail(new AveWrapperReportDto("WebSearch", "WebSearch", AveReportObjectType.WebSearch, AveStatus.Skipped, AveReportResource.Wrapper_Report_NoPermissionToRestoreWebSearch , ex.Message));
                }
                if (mSearchLevel == SearchLevel.site)
                {
                    mLog.Warn("An error occurred while restore Site AveSPSearch. error:{0}", ex.ToString());
                    report.AddDetail(new AveWrapperReportDto("SiteSearch", "SiteSearch", AveReportObjectType.SiteSearch, AveStatus.Skipped, AveReportResource.Wrapper_Report_NoPermissionToRestoreSiteSearch , ex.Message));
                }
            }
            catch (Exception e)
            {
                mLog.Warn("An error occurred while restore AveSPSearch. error:{0}", e.ToString());
            }
        }

        private void RestoreSearchScope(AveSearchInfo aveSearchInfo)
        {
            AveSPSearchScope searchScope = (mSearchLevel == SearchLevel.site) ? new AveSPSearchScope(mAveSite, SearchServiceAppProxy)
                : new AveSPSearchScope(mAveWeb, SearchServiceAppProxy);
            searchScope.SetOverWriteOption(IsOverWrite);
            searchScope.SetReport(report);
            searchScope.Restore(aveSearchInfo.AveScopeInfos, aveSearchInfo.AveDisplayGroupInfos);
        }

        private void RestoreSearchKeywords(AveSearchInfo aveSearchInfo)
        {
            if (mSearchLevel != SearchLevel.site)
            {
                return;
            }
            AveSPSearchKeywords searchKeywords = new AveSPSearchKeywords(mAveSite, SearchServiceAppProxy);
            searchKeywords.SetOverWriteOption(IsOverWrite);
            searchKeywords.SetReport(report);
            searchKeywords.Retore(aveSearchInfo.AveKeywords);
        }

        private void RestoreSearchConfiguration(AveSearchInfo aveSearchInfo)
        {
            if (SearchServiceAppProxy == null)
            {
                return;
            }
            var factory = this.mAveSite.ObjectModelFactory;
            var owner = (mSearchLevel == SearchLevel.site) ? factory.CreateSearchOwner(AveOSearchObjectLevel.SPSite, this.mAveSite.SPSite.RootWeb) :
                factory.CreateSearchOwner(AveOSearchObjectLevel.SPWeb, this.mAveWeb.SPWeb);
            if (!string.IsNullOrEmpty(aveSearchInfo.SearchQueryConfigurationSettingString))
            {
                SearchServiceAppProxy.ImportQueryConfiguration(owner, aveSearchInfo, null);
            }

            if (!string.IsNullOrEmpty(aveSearchInfo.SchemaConfigurationString))
            {
                SearchServiceAppProxy.ImportSchema(owner, aveSearchInfo);
            }

            SearchServiceAppProxy.ImportBuildInAndSSAQeuryRuleSetting(owner, aveSearchInfo.BuildInQueryRuleSetting, aveSearchInfo.SSAQueryRuleSetting);
        }

        public void SetOverWriteOption(bool overWrite)
        {
            IsOverWrite = overWrite;
        }

        public void AddToSearchNavNodesCache(AveNavigationInfoList navigationInfoList)
        {

            using(AvePerformanceScope pc = new AvePerformanceScope("Restore.WebSearchNavigation"))
            {

                try
                {
                    MapNavTitle(navigationInfoList.NavNodes);
                    //WrapperRuntime.WrapperCache.NavigationCache.FindAndAddValue(mCurrentWebId, navigationInfoList);
                    if (!mAveSite.MappingManager.SiteMappingManager.SearchNavigationCache.ContainsKey(mAveWeb.SPWeb.ID))
                    {
                        mAveSite.MappingManager.SiteMappingManager.SearchNavigationCache.Add(mAveWeb.SPWeb.ID, navigationInfoList);
                    }
                }
                catch(Exception e)
                {
                    mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while add navigation node.\n error message:{0}", e));
                    //mLog.Warn(e, "An error occurred while adding navigation node.");
                }

            }

        }
        private void MapNavTitle(List<AveNavigationInfo> navigationInfoList)
        {
            foreach(AveNavigationInfo navNodeInfo in navigationInfoList)
            {
                if(!string.IsNullOrEmpty(navNodeInfo.Title))
                {
                    navNodeInfo.Title = mAveSite.GetNameByLanguageMapping(navNodeInfo.Title, AveLanguageMappingType.NavigationMapping);
                }
                //MapNavTitle(navNodeInfo.Children);
            }
        }

        public void Dispose()
        {
            report.Dispose();
        }
    }

    public class AveSPSearchScope : AvePoint.Wrapper.Restore.IAveSPSearchScope, IDisposable
    {
        private AveSPSite mAveSite = null;
        private AveSPWeb mAveWeb = null;
        private SearchLevel mSearchLevel;
        private IAveOSearchServiceApplicationProxy SearchServiceAppProxy = null;
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private bool IsOverWrite = true;
        private IReport mReport = new AveWrapperReport();
        public AveSPSearchScope(AveSPSite aveSite, IAveOSearchServiceApplicationProxy searchServiceAppProxy)
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
                        Site = mAveSite.SPSite,
                        WebApplication = mAveSite.SPSite.WebApplication,
                        SiteSubscriptionIdentifier = mAveSite.ObjectModelFactory.CreateSiteSubscriptionIdentifier().Default
                    };

                    IAveServiceContext serviceContext = mAveSite.ObjectModelFactory.CreateServerContext(info);
                    SearchServiceAppProxy = (IAveOSearchServiceApplicationProxy)serviceContext.GetDefaultProxy(typeof(IAveOSearchServiceApplicationProxy));
                }
                catch (Exception e)
                {
                    mReport.AddDetail(new AveWrapperReportDto(mAveSite.SPSite.Url, mAveSite.SPSite.Url, AveReportObjectType.SiteSearchScope, AveStatus.Failed, AveReportResource.Wrapper_Report_GetSearchServiceApplicationProxyError, mAveSite.SPSite.Url, e.Message));
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while get SearchServiceApplicationProxy. siteUrl:{0}\n error message:{1}", mAveSite.SPSite.Url, e));
                    //mLog.Warn(e, "An error ocurred when get SearchServiceApplicationProxy.siteUrl:{0}", mAveSite.SPSite.Url);
                }
            }

        }

        public AveSPSearchScope(AveSPWeb aveWeb, IAveOSearchServiceApplicationProxy searchServiceAppProxy)
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
                //add by adrian for 07 item restore

                try
                {
                    AveServiceContextInfo info = new AveServiceContextInfo
                    {
                        Site = mAveSite.SPSite,
                        WebApplication = mAveSite.SPSite.WebApplication,
                        SiteSubscriptionIdentifier = mAveSite.ObjectModelFactory.CreateSiteSubscriptionIdentifier().Default
                    };

                    IAveServiceContext serviceContext = mAveSite.ObjectModelFactory.CreateServerContext(info);

                    SearchServiceAppProxy = (IAveOSearchServiceApplicationProxy)serviceContext.GetDefaultProxy(typeof(IAveOSearchServiceApplicationProxy));
                }
                catch (Exception e)
                {
                    mReport.AddDetail(new AveWrapperReportDto(mAveSite.SPSite.Url, mAveSite.SPSite.Url, AveReportObjectType.WebSearchScope, AveStatus.Failed, AveReportResource.Wrapper_Report_GetSearchServiceApplicationProxyError, mAveSite.SPSite.Url, e.Message));
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while get SearchServiceApplicationProxy. siteUrl:{0}\n error message:{1}", mAveSite.SPSite.Url, e));
                    //mLog.Warn(e, "An error ocurred when get SearchServiceApplicationProxy.siteUrl:{0}", mAveSite.SPSite.Url);
                }

            }
        }

        private void RestoreScope(List<AveScopeInfo> aveScopeInfos)
        {
            string consumerName = string.Empty;
            if (mSearchLevel == SearchLevel.site)
            {
                consumerName = mAveSite.SPSite.ID.ToString();
            }
            else
            {
                consumerName = mAveWeb.SPWeb.ID.ToString();
            }
            if (SearchServiceAppProxy == null || aveScopeInfos == null)
            {
                return;
            }
            foreach (AveScopeInfo scopeInfo in aveScopeInfos)
            {
                int scopeId = 0;
                try
                {
                    if (mSearchLevel == SearchLevel.site)
                    {
                        scopeId = SearchServiceAppProxy.GetScopeIDFromName(consumerName, scopeInfo.Name);
                    }
                    else
                    {
                        scopeId = SearchServiceAppProxy.GetScopeIDFromName(scopeInfo.ConsumerName, mAveWeb.SPWeb.ID.ToString());
                    }
                    IAveOScopeInfo tempScope = SearchServiceAppProxy.GetScopeInfo(scopeId);
                    tempScope.AlternateResultsPage = scopeInfo.AlternateResultsPage;
                    //tempScope.CompilationState = (ScopeCompilationState)Enum.Parse(typeof(ScopeCompilationState), scopeInfo.CompilationState);
                    tempScope.CompilationType = (AveScopeCompilationType)Enum.Parse(typeof(AveScopeCompilationType), scopeInfo.CompilationType);
                    if (mSearchLevel == SearchLevel.site)
                    {
                        tempScope.ConsumerName = mAveSite.SPSite.ID.ToString();
                    }
                    else
                    {
                        tempScope.ConsumerName = scopeInfo.ConsumerName;
                    }
                    tempScope.Description = scopeInfo.Description;
                    tempScope.DisplayInAdminUI = scopeInfo.DisplayInAdminUI;
                    tempScope.Filter = scopeInfo.Filter;
                    //tempScope.IsDeleted = scopeInfo.IsDeleted;
                    tempScope.LastCompilationTime = scopeInfo.LastCompilationTime;
                    //tempScope.LastModifiedBy = scopeInfo.LastModifiedBy;
                    //tempScope.SiteUrl = scopeInfo.SiteUrl;
                    SearchServiceAppProxy.SetScopeInfo(tempScope);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.RestoreSearchScopeError, ex.ToString());
                    try
                    {
                        IAveOScopeInfo tempScope = mAveSite.ObjectModelFactory.CreateScopeInfo();
                        tempScope.AlternateResultsPage = scopeInfo.AlternateResultsPage;
                        //tempScope.CompilationState = (ScopeCompilationState)Enum.Parse(typeof(ScopeCompilationState), scopeInfo.CompilationState);
                        tempScope.CompilationType = (AveScopeCompilationType)Enum.Parse(typeof(AveScopeCompilationType), scopeInfo.CompilationType);
                        if (mSearchLevel == SearchLevel.site)
                        {
                            tempScope.Name = scopeInfo.Name;
                            tempScope.ConsumerName = mAveSite.SPSite.ID.ToString();
                            tempScope.Description = scopeInfo.Description;
                        }
                        else
                        {
                            tempScope.Name = mAveWeb.SPWeb.ID.ToString();
                            tempScope.ConsumerName = scopeInfo.ConsumerName;
                            tempScope.Description = scopeInfo.Description;
                        }

                        tempScope.DisplayInAdminUI = scopeInfo.DisplayInAdminUI;
                        tempScope.Filter = scopeInfo.Filter;
                        //tempScope.IsDeleted = scopeInfo.IsDeleted;
                        tempScope.LastCompilationTime = scopeInfo.LastCompilationTime;
                        //tempScope.LastModifiedBy = scopeInfo.LastModifiedBy;
                        //tempScope.SiteUrl = scopeInfo.SiteUrl;
                        int statusCode = 0;
                        scopeId = SearchServiceAppProxy.AddScope(tempScope, out statusCode);
                        if (scopeId < 0)
                        {
                            List<string> consumers = SearchServiceAppProxy.GetConsumers();
                            if (!consumers.Contains(consumerName))
                            {
                                SearchServiceAppProxy.AddConsumer(consumerName);
                            }
                            scopeId = SearchServiceAppProxy.AddScope(tempScope, out statusCode);
                        }
                    }
                    catch (Exception e)
                    {
                        mReport.AddDetail(new AveWrapperReportDto(mAveSite.SPSite.Url, mAveSite.SPSite.Url, AveReportObjectType.SiteSearchScope, AveStatus.Failed, AveReportResource.Wrapper_Report_CreateNewScopeError, consumerName, e.Message));
                        log.Log(AveLogLevel.WARN, string.Format("An error occurred while create a new scope. consumerName:{0}\n error message:{1}", consumerName, e));
                        //mLog.Warn(e, "An error ocurred when create a new scope.consumerName:{0}", consumerName);
                        continue;
                    }
                }
                #region restore scope rules
                if (scopeId > 0)
                {
                    List<IAveORuleInfo> rules = null;
                    foreach (AveRuleInfo ruleInfo in scopeInfo.AveRuleInfos)
                    {
                        int ruleId = 0;
                        bool isExist = false;
                        int statusCode = 0;

                        if (rules == null)
                        {
                            rules = SearchServiceAppProxy.GetRulesInfo(scopeId, out statusCode);
                        }
                        foreach (IAveORuleInfo rule in rules)
                        {
                            if (ruleInfo.FilterBehavior.Equals(rule.FilterBehavior.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                if (ruleInfo.RuleType.Equals(rule.RuleType.ToString(), StringComparison.OrdinalIgnoreCase))
                                {
                                    if (ruleInfo.UrlRuleType.Equals(rule.UrlRuleType.ToString(), StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (ruleInfo.UserValue == rule.UserValue)
                                        {
                                            isExist = true;
                                            ruleId = rule.ID;
                                            break;
                                        }
                                        else if ((AveScopeRuleType)Enum.Parse(typeof(AveScopeRuleType), ruleInfo.RuleType) == AveScopeRuleType.Url
                                            && (AveUrlScopeRuleType)Enum.Parse(typeof(AveUrlScopeRuleType), ruleInfo.UrlRuleType) == AveUrlScopeRuleType.Folder
                                            && !string.IsNullOrEmpty(ruleInfo.UserValue))
                                        {
                                            string oldUrl = ruleInfo.UserValue;
                                            AveSPSite tmpSite = null;
                                            if (mSearchLevel == SearchLevel.site)
                                            {
                                                tmpSite = mAveSite;
                                            }
                                            else
                                            {
                                                tmpSite = mAveWeb.ParentSite;
                                            }
                                            string newUrl = AveReplaceProcessor.UrlReplace(oldUrl, tmpSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), tmpSite.SourceSiteInfo, tmpSite.ServerRelativeUrl);
                                            if (newUrl == rule.UserValue)
                                            {
                                                isExist = true;
                                                ruleId = rule.ID;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (!isExist)
                        {
                            try
                            {
                                IAveORuleInfo tempRule = mAveSite.ObjectModelFactory.CreateRuleInfo();
                                tempRule.FilterBehavior = (AveScopeRuleFilterBehavior)Enum.Parse(typeof(AveScopeRuleFilterBehavior), ruleInfo.FilterBehavior);
                                //tempRule.IsDeleted = ruleInfo.IsDeleted;
                                tempRule.RuleType = (AveScopeRuleType)Enum.Parse(typeof(AveScopeRuleType), ruleInfo.RuleType);
                                tempRule.UrlRuleType = (AveUrlScopeRuleType)Enum.Parse(typeof(AveUrlScopeRuleType), ruleInfo.UrlRuleType);
                                tempRule.UserValue = ruleInfo.UserValue;
                                //如果tempRule.UserValue=null，AddRule的时候会抛异常
                                if (tempRule.UserValue == null)
                                {
                                    tempRule.UserValue = "";
                                }
                                if (tempRule.RuleType == AveScopeRuleType.Url && tempRule.UrlRuleType == AveUrlScopeRuleType.Folder && !string.IsNullOrEmpty(tempRule.UserValue))
                                {
                                    string oldUrl = tempRule.UserValue;
                                    AveSPSite tmpSite = null;
                                    if (mSearchLevel == SearchLevel.site)
                                    {
                                        tmpSite = mAveSite;
                                    }
                                    else
                                    {
                                        tmpSite = mAveWeb.ParentSite;
                                    }
                                    tempRule.UserValue = AveReplaceProcessor.UrlReplace(oldUrl, tmpSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), tmpSite.SourceSiteInfo, tmpSite.ServerRelativeUrl);
                                }
                                if (ruleInfo.ManagedProperty != null)
                                {
                                    //?是否需要还原ManagedProperty
                                    IAveOManagedPropertyInfo managedPropertyInfo = mAveSite.ObjectModelFactory.CreateManagedPropertyInfo();
                                    managedPropertyInfo.EnabledForScoping = ruleInfo.ManagedProperty.EnabledForScoping;
                                    managedPropertyInfo.ManagedType = (AveManagedDataType)Enum.Parse(typeof(AveManagedDataType), ruleInfo.ManagedProperty.ManagedType);
                                    managedPropertyInfo.Name = ruleInfo.ManagedProperty.Name;
                                    managedPropertyInfo.Pid = ruleInfo.ManagedProperty.Pid;
                                    tempRule.ManagedProperty = managedPropertyInfo;
                                }
                                ruleId = SearchServiceAppProxy.AddRule(tempRule, scopeId);
                            }
                            catch (Exception e)
                            {
                                mReport.AddDetail(new AveWrapperReportDto(mAveSite.SPSite.Url, mAveSite.SPSite.Url, AveReportObjectType.SiteSearchScope, AveStatus.Failed, AveReportResource.Wrapper_Report_CreateNewScopeRuleError, consumerName, e.Message));
                                log.Log(AveLogLevel.WARN, string.Format("An error occurred while create a new scope rule. consumerName:{0}\n error message:{1}", consumerName, e));
                                //mLog.Warn(e, "An error ocurred when create a new scope rule.consumerName:{0}", consumerName);
                            }
                        }
                    }
                }
                #endregion
            }
        }

        private void RestoreDisplayGroup(List<AveDisplayGroupInfo> aveDisplayGroupInfo)
        {
            string consumerName = string.Empty;
            if (mSearchLevel == SearchLevel.site)
            {
                consumerName = mAveSite.SPSite.ID.ToString();
            }
            else
            {
                consumerName = mAveWeb.SPWeb.ID.ToString();
            }
            if (SearchServiceAppProxy == null || aveDisplayGroupInfo == null)
            {
                return;
            }
            foreach (AveDisplayGroupInfo groupInfo in aveDisplayGroupInfo)
            {
                int displayGroupId = 0;
                try
                {
                    displayGroupId = SearchServiceAppProxy.GetDisplayGroupIDFromName(mAveSite.SPSite.ID.ToString(), groupInfo.Name);
                    IAveODisplayGroupInfo tempGroup = SearchServiceAppProxy.GetDisplayGroupInfo(displayGroupId);
                    if (groupInfo.DefaultScopeName != null)
                    {
                        try
                        {
                            tempGroup.DefaultScopeID = SearchServiceAppProxy.GetScopeIDFromName(mAveSite.SPSite.ID.ToString(), groupInfo.DefaultScopeName);
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.SetScopeIdFailed, e);
                            //共享的scope，需要这么取
                            tempGroup.DefaultScopeID = SearchServiceAppProxy.GetScopeIDFromName("shared", groupInfo.DefaultScopeName);
                        }
                    }
                    tempGroup.Description = groupInfo.Description;
                    tempGroup.DisplayInAdminUI = groupInfo.DisplayInAdminUI;
                    //tempGroup.IsDeleted = groupInfo.IsDeleted;
                    tempGroup.IsUndeletable = groupInfo.IsUndeletable;
                    //tempGroup.LastModifiedBy = groupInfo.LastModifiedBy;
                    tempGroup.LastModifiedTime = groupInfo.LastModifiedTime;
                    //tempGroup.Name = groupInfo.Name;
                    //tempGroup.SiteUrl = groupInfo.SiteUrl;
                    SearchServiceAppProxy.SetDisplayGroupInfo(tempGroup);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.RestoreDisplayGroupError, ex.ToString());
                    try
                    {
                        IAveODisplayGroupInfo tempGroup = mAveSite.ObjectModelFactory.CreateDisplayGroupInfo();
                        tempGroup.Name = groupInfo.Name;
                        tempGroup.ConsumerName = mAveSite.SPSite.ID.ToString();
                        if (!string.IsNullOrEmpty(groupInfo.DefaultScopeName))
                        {
                            try
                            {
                                tempGroup.DefaultScopeID = SearchServiceAppProxy.GetScopeIDFromName(mAveSite.SPSite.ID.ToString(), groupInfo.DefaultScopeName);
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.SetScopeIdFailed, e);
                                //共享的scope，需要这么取
                                tempGroup.DefaultScopeID = SearchServiceAppProxy.GetScopeIDFromName("shared", groupInfo.DefaultScopeName);
                            }
                        }
                        tempGroup.Description = groupInfo.Description;
                        tempGroup.DisplayInAdminUI = groupInfo.DisplayInAdminUI;
                        //tempGroup.IsDeleted = groupInfo.IsDeleted;
                        tempGroup.IsUndeletable = groupInfo.IsUndeletable;
                        //tempGroup.LastModifiedBy = groupInfo.LastModifiedBy;
                        tempGroup.LastModifiedTime = groupInfo.LastModifiedTime;
                        //tempGroup.Name = groupInfo.Name;
                        //tempGroup.SiteUrl = groupInfo.SiteUrl;
                        int statusCode = 0;
                        displayGroupId = SearchServiceAppProxy.AddDisplayGroup(tempGroup, out statusCode);
                    }
                    catch (Exception e)
                    {
                        mReport.AddDetail(new AveWrapperReportDto(mAveSite.SPSite.Url, mAveSite.SPSite.Url, AveReportObjectType.SiteSearchScope, AveStatus.Failed, AveReportResource.Wrapper_Report_CreateNewDisplayGroupError, consumerName, e.Message));
                        log.Log(AveLogLevel.WARN, string.Format("An error occurred while create a new DisplayGroup. consumerName:{0}\n error message:{1}", consumerName, e));
                        //mLog.Warn(e, "An error ocurred when create a new DisplayGroup.consumerName:{0}", consumerName);
                        continue;
                    }
                }
                #region restore display group members
                if (displayGroupId > 0)
                {
                    List<int> groupList = SearchServiceAppProxy.GetDisplayGroupListInfo(displayGroupId);
                    if (IsOverWrite)
                    {
                        groupList.Clear();
                    }

                    foreach (AveDisplayGroupMember member in groupInfo.AveDisplayGroupMembers)
                    {
                        try
                        {
                            int id = -1;
                            try
                            {
                                id = SearchServiceAppProxy.GetScopeIDFromName(mAveSite.SPSite.ID.ToString(), member.Name);
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetGroupIdFromMemberNameFaild, e.ToString());
                                id = SearchServiceAppProxy.GetScopeIDFromName("shared", member.Name);
                            }
                            if (id >= 0 && !groupList.Contains(id))
                            {
                                groupList.Add(id);
                            }
                        }
                        catch (Exception e)
                        {
                            mReport.AddDetail(new AveWrapperReportDto(mAveSite.SPSite.Url, mAveSite.SPSite.Url, AveReportObjectType.SiteSearchScope, AveStatus.Skipped, AveReportResource.Wrapper_Report_CannotGetScope, member.Name, e.Message));
                            log.Log(AveLogLevel.WARN, string.Format("Cannot get scope. scope name:{0}\n error message:{1}", member.Name, e));
                            //mLog.Warn(e, "Can not get scope. scope Name:", member.Name);
                            continue;
                        }
                    }
                    try
                    {
                        SearchServiceAppProxy.SetDisplayGroupListInfo(displayGroupId, groupList);
                    }
                    catch (Exception e)
                    {
                        mReport.AddDetail(new AveWrapperReportDto(mAveSite.SPSite.Url, mAveSite.SPSite.Url, AveReportObjectType.SiteSearchScope, AveStatus.Failed, AveReportResource.Wrapper_Report_SetDisplayGroupListInfoError, consumerName, displayGroupId, e.Message));
                        log.Log(AveLogLevel.WARN, string.Format("An error occurred while set displayGroupListInfo. consumerName:{0}, displayGroup:{1}\n error message:{2}", consumerName, displayGroupId, e));
                        //mLog.Warn(e, "An error ocurred when set displayGroupListInfo.consumerName:{0}, displayGroup Name:{1}", consumerName, displayGroupId);
                    }
                }
                #endregion
            }
        }

        public void Restore(List<AveScopeInfo> aveScopeInfos, List<AveDisplayGroupInfo> aveDisplayGroupInfo)
        {
            RestoreScope(aveScopeInfos);
            RestoreDisplayGroup(aveDisplayGroupInfo);
        }

        public void SetOverWriteOption(bool overWrite)
        {
            IsOverWrite = overWrite;
        }

        public void SetReport(IReport report)
        {
            mReport = report;
        }
        public void Dispose()
        {
            mReport.Dispose();
        }
    }

    public class AveSPSearchKeywords : AvePoint.Wrapper.Restore.IAveSPSearchKeywords, IDisposable
    {
        private AveSPSite mAveSite = null;
        private IAveOSearchServiceApplicationProxy SearchServiceAppProxy = null;
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private bool IsOverWrite = true;
        private IReport mReport = new AveWrapperReport();
        public AveSPSearchKeywords(AveSPSite aveSite, IAveOSearchServiceApplicationProxy searchServiceAppProxy)
        {
            mAveSite = aveSite;
            if (searchServiceAppProxy != null)
            {
                SearchServiceAppProxy = searchServiceAppProxy;
            }
            //add by adian

            try
            {
                AveServiceContextInfo info = new AveServiceContextInfo
                {
                    Site = mAveSite.SPSite,
                    WebApplication = mAveSite.SPSite.WebApplication,
                    SiteSubscriptionIdentifier = mAveSite.ObjectModelFactory.CreateSiteSubscriptionIdentifier().Default
                };

                IAveServiceContext serviceContext = mAveSite.ObjectModelFactory.CreateServerContext(info);

                SearchServiceAppProxy = (IAveOSearchServiceApplicationProxy)serviceContext.GetDefaultProxy(typeof(IAveOSearchServiceApplicationProxy));
            }
            catch (Exception e)
            {
                mReport.AddDetail(new AveWrapperReportDto(mAveSite.SPSite.Url, mAveSite.SPSite.Url, AveReportObjectType.SiteSearchKeyWords, AveStatus.Failed, AveReportResource.Wrapper_Report_GetSearchServiceApplicationProxyError, mAveSite.SPSite.Url, e.Message));
                mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while get SearchServiceApplicationProxy. site url:{0}\n error message:{1}", mAveSite.SPSite.Url, e));
                //mLog.Warn(e, "An error ocurred when get SearchServiceApplicationProxy.siteUrl:{0}", mAveSite.SPSite.Url);
            }

        }
        public void Retore(List<AveKeyword> aveKeywords)
        {
            IAveSite site = mAveSite.SPSite;
            if (SearchServiceAppProxy == null || aveKeywords == null)
            {
                return;
            }

            IAveOKeywords words = mAveSite.ObjectModelFactory.CreateKeywords(SearchServiceAppProxy, new Uri(site.Url));
            if (IsOverWrite)
            {
                List<string> keyWordName = new List<string>();
                foreach (IAveOKeyword tempWordName in words.AllKeywords)
                {
                    keyWordName.Add(tempWordName.Term);
                }
                if (keyWordName.Count > 0)
                {
                    for (int i = 0; i < keyWordName.Count; i++)
                    {
                        IAveOKeyword wordName = words.AllKeywords[keyWordName[i]];
                        wordName.Delete();
                    }
                }
                mLog.Log(AveLogLevel.DEBUG, string.Format("Deleted the Destination Keywords."));
                //mLog.Debug("Deleted the Destination Keywords");
            }
            IAveOKeywordCollection siteWords = words.AllKeywords;//取得站点内的的key集合
            List<string> wordsList = new List<string>();
            foreach (IAveOKeyword WordName in siteWords)
            {
                wordsList.Add(WordName.Term);
            }
            foreach (AveKeyword keyword in aveKeywords)
            {
                try
                {
                    IAveOKeyword word = null;
                    try
                    {
                        if (wordsList.Contains(keyword.Term)) //存在的keyword
                        {
                            word = siteWords[keyword.Term];
                        }
                        else
                        {
                            word = words.AllKeywords.Create(keyword.Term, keyword.StartDate);
                        }
                    }
                    catch (Exception e)
                    {
                        mReport.AddDetail(new AveWrapperReportDto(mAveSite.SPSite.Url, mAveSite.SPSite.Url, AveReportObjectType.SiteSearchKeyWords, AveStatus.Failed, AveReportResource.Wrapper_Report_GetOrCreateKeywordError, keyword.Term, e.Message));
                        mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while get or create keyword. keyword team:{0}\n error message:{1}", keyword.Term, e));
                        //mLog.Warn(e, "An error ocurred when Get or Create keyword. keyword term:{0}", keyword.Term);
                        continue;
                    }
                    word.Contact = keyword.Contact;
                    word.Definition = keyword.Definition;
                    word.EndDate = keyword.EndDate;
                    word.ReviewDate = keyword.ReviewDate;
                    word.StartDate = keyword.StartDate;
                    if (word.BestBets != null)
                    {
                        foreach (AveBestBet aveBet in keyword.BestBets)
                        {
                            try
                            {
                                string Url = aveBet.Url;
                                Url = AveReplaceProcessor.UrlReplace(Url, mAveSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveSite.SourceSiteInfo, mAveSite.ServerRelativeUrl);
                                aveBet.Url = Url;
                                Uri uri = new Uri(Url);
                                word.BestBets.Create(aveBet.Title, aveBet.Description, uri);
                            }
                            catch (ArgumentException)
                            {
                                mLog.Info("BestBet:{0} is exists.", aveBet.Url);
                            }
                            catch (Exception e)
                            {
                                mReport.AddDetail(new AveWrapperReportDto(mAveSite.SPSite.Url, mAveSite.SPSite.Url, AveReportObjectType.SiteSearchKeyWords, AveStatus.Failed, AveReportResource.Wrapper_Report_RestoreBestBetsError, aveBet.Title, e.Message));
                                mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while restore a bestBets. bestBets title:{0}\n error message:{1}", aveBet.Title, e));
                                //mLog.Warn(e, "An error ocurred when restore a BestBets. BestBets title:{0}", aveBet.Title);
                                continue;
                            }
                        }
                    }
                    if (word.Synonyms != null)
                    {
                        foreach (AveSynonym aveSyn in keyword.Synonyms)
                        {
                            try
                            {
                                word.Synonyms.Create(aveSyn.Term);
                            }
                            catch (ArgumentException)
                            {
                                mLog.Info("Synonym:{0} is exists.", aveSyn.Term);
                            }
                            catch (Exception e)
                            {
                                mReport.AddDetail(new AveWrapperReportDto(mAveSite.SPSite.Url, mAveSite.SPSite.Url, AveReportObjectType.SiteSearchKeyWords, AveStatus.Failed, AveReportResource.Wrapper_Report_RestoreSynonymError, aveSyn.Term, e.Message));
                                mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while restore a synonym. Synonym term:{0}\n error message:{1}", aveSyn.Term, e));
                                //mLog.Warn(e, "An error ocurred when restore a Synonym. Synonym term:{0}", aveSyn.Term);
                                continue;
                            }
                        }
                    }
                    word.Update();
                }
                catch (Exception e)
                {
                    mReport.AddDetail(new AveWrapperReportDto(mAveSite.SPSite.Url, mAveSite.SPSite.Url, AveReportObjectType.SiteSearchKeyWords, AveStatus.Failed, AveReportResource.Wrapper_Report_RestoreKeywordError, keyword.Definition, keyword.Contact, e.Message));
                    mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while restore keyword. keyword definition:{0}, keyword contact:{1}\n error message:{2}", keyword.Definition, keyword.Contact, e));
                    //mLog.Log(AveLogSeverity.Warn, "AveSPSearch00450",keyword.Definition, keyword.Contact, e);
                }
            }
        }

        public void SetOverWriteOption(bool overWrite)
        {
            IsOverWrite = overWrite;
        }

        public void SetReport(IReport report)
        {
            mReport = report;
        }
        public void Dispose()
        {
            mReport.Dispose();
        }
    }

    #region moved to wrapper contract
    //public enum SearchLevel
    //{
    //    site = 1,
    //    web = 2,
    //}
    #endregion
}
