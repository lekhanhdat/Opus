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



using System.Collections.Generic;
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.Search.Administration;
using AvePoint.Wrapper.Common;
using System;
using Microsoft.Office.Server.Search.Query;
using Microsoft.Office.Server.Search.Portability;
using System.Runtime.Serialization;
using System.Text;
using System.IO;

namespace AvePoint.ObjectModel.ServerSE.Office
{
    class AveOSearchServiceApplicationProxy : AveIisWebServiceApplicationProxy, IAveOSearchServiceApplicationProxy
    {
        private SearchServiceApplicationProxy mSearchServiceApplicationProxy;
        private AveOFASTAdminProxy mFASTAdminProxy;

        public AveOSearchServiceApplicationProxy(SearchServiceApplicationProxy searchServiceApplicationProxy)
            : base(searchServiceApplicationProxy)
        {
            mSearchServiceApplicationProxy = searchServiceApplicationProxy;
        }

        internal SearchServiceApplicationProxy SearchServiceApplicationProxy
        {
            get
            {
                return mSearchServiceApplicationProxy;
            }
        }

        public List<IAveOScopeInfo> GetScopesInfo()
        {
            List<ScopeInfo> scopesInfo = mSearchServiceApplicationProxy.GetScopesInfo();
            List<IAveOScopeInfo> aveScopesInfo = new List<IAveOScopeInfo>();
            foreach (ScopeInfo scopeInfo in scopesInfo)
            {
                aveScopesInfo.Add(new AveOScopeInfo(scopeInfo));
            }
            return aveScopesInfo;
        }

        public List<IAveORuleInfo> GetRulesInfo(int scopeId, out int statusCode)
        {
            int refStatusCode = 0;
            List<RuleInfo> rulesInfo = mSearchServiceApplicationProxy.GetRulesInfo(scopeId, out refStatusCode);
            statusCode = refStatusCode;
            if (rulesInfo != null)
            {
                List<IAveORuleInfo> aveRulesInfo = new List<IAveORuleInfo>();
                foreach (RuleInfo ruleInfo in rulesInfo)
                {
                    if (ruleInfo != null)
                    {
                        aveRulesInfo.Add(new AveORuleInfo(ruleInfo));
                    }
                    else
                    {
                        aveRulesInfo.Add(null);
                    }
                }
                return aveRulesInfo;
            }
            return null;
        }

        public List<IAveODisplayGroupInfo> GetDisplayGroupsInfo()
        {
            List<IAveODisplayGroupInfo> aveDisplayGroupsInfo = new List<IAveODisplayGroupInfo>();
            foreach (DisplayGroupInfo displayGroupInfo in mSearchServiceApplicationProxy.GetDisplayGroupsInfo())
            {
                aveDisplayGroupsInfo.Add(new AveODisplayGroupInfo(displayGroupInfo));
            }
            return aveDisplayGroupsInfo;
        }

        public IAveOScopeInfo GetScopeInfo(int scopeId)
        {
            return new AveOScopeInfo(mSearchServiceApplicationProxy.GetScopeInfo(scopeId));
        }

        public List<int> GetDisplayGroupListInfo(int displayGroupId)
        {
            return mSearchServiceApplicationProxy.GetDisplayGroupListInfo(displayGroupId);
        }

        public int GetScopeIDFromName(string consumerName, string name)
        {
            return mSearchServiceApplicationProxy.GetScopeIDFromName(consumerName, name);
        }

        public void SetScopeInfo(IAveOScopeInfo scopeInfo)
        {
            mSearchServiceApplicationProxy.SetScopeInfo((scopeInfo as AveOScopeInfo).ScopeInfo);
        }

        public int AddScope(IAveOScopeInfo scopeInfo, out int statusCode)
        {
            return mSearchServiceApplicationProxy.AddScope((scopeInfo as AveOScopeInfo).ScopeInfo, out statusCode);
        }

        public int AddConsumer(string consumerName)
        {
            return mSearchServiceApplicationProxy.AddConsumer(consumerName);
        }

        public List<string> GetConsumers()
        {
            return mSearchServiceApplicationProxy.GetConsumers();
        }

        public int AddDisplayGroup(IAveODisplayGroupInfo displayGroupInfo, out int statusCode)
        {
            return mSearchServiceApplicationProxy.AddDisplayGroup((displayGroupInfo as AveODisplayGroupInfo).DisplayGroupInfo, out statusCode);
        }

        public int AddRule(IAveORuleInfo ruleInfo, int scopeId)
        {
            return mSearchServiceApplicationProxy.AddRule((ruleInfo as AveORuleInfo).RuleInfo, scopeId);
        }

        public IAveODisplayGroupInfo GetDisplayGroupInfo(int displayGroupId)
        {
            DisplayGroupInfo displayGroupInfo = mSearchServiceApplicationProxy.GetDisplayGroupInfo(displayGroupId);
            if (displayGroupInfo == null)
            {
                return null;
            }
            return new AveODisplayGroupInfo(displayGroupInfo);
        }

        public int GetDisplayGroupIDFromName(string consumerName, string name)
        {
            return mSearchServiceApplicationProxy.GetDisplayGroupIDFromName(consumerName, name);
        }

        public void SetDisplayGroupInfo(IAveODisplayGroupInfo displayGroupInfo)
        {
            mSearchServiceApplicationProxy.SetDisplayGroupInfo((displayGroupInfo as AveODisplayGroupInfo).DisplayGroupInfo);
        }

        public void SetDisplayGroupListInfo(int displayGroupId, List<int> scopeIds)
        {
            mSearchServiceApplicationProxy.SetDisplayGroupListInfo(displayGroupId, scopeIds);
        }

        public IAveOSearchServiceApplication GetProxy(IAveServiceContext ServiceContext)
        {
            if (ServiceContext == null)
            {
                throw new ArgumentNullException("IAveServiceContext");
            }
            AveOSearchServiceApplication defaultProxy = (AveOSearchServiceApplication)ServiceContext.GetDefaultProxy(typeof(IAveOSearchServiceApplicationProxy));
            if (null == defaultProxy)
            {
                throw new AveSearchServiceNotFoundException();
            }
            return defaultProxy;
        }

        public IAveOFASTAdminProxy FASTAdminProxy
        {
            get
            {
                if (mFASTAdminProxy == null)
                {
                    FASTAdminProxy fASTAdminProxy = mSearchServiceApplicationProxy.FASTAdminProxy;
                    if (fASTAdminProxy != null)
                    {
                        mFASTAdminProxy = new AveOFASTAdminProxy(fASTAdminProxy);
                    }
                }
                return mFASTAdminProxy;
            }
        }

        public IAveOLocationConfiguration[] GetLocationConfigurations(out long lastUpdate, out bool useCrawlProxy)
        {
            LocationConfiguration[] spLocationConfigurations = mSearchServiceApplicationProxy.GetLocationConfigurations(out lastUpdate, out useCrawlProxy);
            if (spLocationConfigurations != null)
            {
                IAveOLocationConfiguration[] locationConfigurations = new IAveOLocationConfiguration[spLocationConfigurations.Length];
                for (int i = 0; i < spLocationConfigurations.Length; i++)
                {
                    if (spLocationConfigurations[i] != null)
                    {
                        locationConfigurations[i] = new AveOLocationConfiguration(spLocationConfigurations[i]);
                    }
                    else
                    {
                        locationConfigurations[i] = null;
                    }
                }
                return locationConfigurations;
            }
            return null;
        }

        public IAveOSearchServiceApplicationInfo GetSearchServiceApplicationInfo()
        {
            SearchServiceApplicationInfo info = mSearchServiceApplicationProxy.GetSearchServiceApplicationInfo();
            if (info != null)
            {
                return new AveOSearchServiceApplicationInfo(info);
            }
            return null;
        }

        public void ExportQueryConfiguration(IAveOSearchObjectOwner owningScope, out IAveOSearchQueryConfigurationSettings outPackage)
        {
            SearchQueryConfigurationSettings SPOutPackage;
            this.mSearchServiceApplicationProxy.ExportQueryConfiguration(((AveOSearchObjectOwner)owningScope).Owner, out SPOutPackage);
            var sources = (List<Microsoft.Office.Server.Search.Administration.Query.SourceRecord>)AveAssemblyUtility.GetPropertyValue(SPOutPackage, "Sources");
            foreach(Microsoft.Office.Server.Search.Administration.Query.SourceRecord s in sources)
            {
                if (s.AuthInfo == null)
                {
                    var sources2 = new Microsoft.Office.Server.Search.Administration.Query.FederationManager(this.mSearchServiceApplicationProxy).GetSourceByName(s.Name, ((AveOSearchObjectOwner)owningScope).Owner);
                    s.AuthInfo = sources2.AuthInfo;
                }
            }
            outPackage = new AveSearchQueryConfigurationSettings();
     
            MemoryStream ms=new MemoryStream();
            NetDataContractSerializer NDCS = new NetDataContractSerializer();
            NDCS.WriteObject(ms, SPOutPackage);

            outPackage.SeachConfigurationString= Encoding.UTF8.GetString(ms.ToArray());
        }

        public IAveOSearchSchemaConfigurationSettings ExportSchema(IAveOSearchObjectOwner owner)
        {
            SearchSchemaConfigurationSettings schemaSetting;
            schemaSetting =this.mSearchServiceApplicationProxy.ExportSchema(((AveOSearchObjectOwner)owner).Owner);

            MemoryStream ms = new MemoryStream();
            NetDataContractSerializer NDCS = new NetDataContractSerializer();
            NDCS.WriteObject(ms, schemaSetting);

            IAveOSearchSchemaConfigurationSettings aveSearchSchemaSettings = new AveSearchQueryConfigurationSettings();
            aveSearchSchemaSettings.SearchSchemaSettingString = Encoding.UTF8.GetString(ms.ToArray());

            return aveSearchSchemaSettings;
        }

        public void ImportSchema(IAveOSearchObjectOwner owningScope,AveSearchInfo searchInfo)
        {
            if (!string.IsNullOrEmpty(searchInfo.SchemaConfigurationString))
            {
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(searchInfo.SchemaConfigurationString));

                NetDataContractSerializer NDCS = new NetDataContractSerializer();               
                SearchSchemaConfigurationSettings schemaSetting =(SearchSchemaConfigurationSettings)NDCS.ReadObject(ms);

                this.mSearchServiceApplicationProxy.ImportSchema(((AveOSearchObjectOwner)owningScope).Owner, schemaSetting);
            }
        }

        public void ImportQueryConfiguration(IAveOSearchObjectOwner owningScope, AveSearchInfo searchInfo, Dictionary<string, string> queryTemplateParameters)
        {
            if (!string.IsNullOrEmpty(searchInfo.SearchQueryConfigurationSettingString))
            {
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(searchInfo.SearchQueryConfigurationSettingString));

                NetDataContractSerializer NDCS = new NetDataContractSerializer();
                SearchQueryConfigurationSettings SPOutPackage = (SearchQueryConfigurationSettings)NDCS.ReadObject(ms);

                this.mSearchServiceApplicationProxy.ImportQueryConfiguration(((AveOSearchObjectOwner)owningScope).Owner, SPOutPackage, queryTemplateParameters);
            }
        }

        public void ExportBuildInAndSSAQeuryRuleSetting(IAveOSearchObjectOwner owningScope, Dictionary<Guid, bool> buildInRuleSetting, Dictionary<string, bool> ssaRuleSetting)
        {
            var rules = this.mSearchServiceApplicationProxy.GetQueryRules(new SearchObjectFilter(((AveOSearchObjectOwner)owningScope).Owner));
            if (rules != null)
            {
                foreach (var rule in rules)
                {
                    if (rule.IsSystem)//build in rule.
                    {
                        buildInRuleSetting[rule.Id] = rule.IsActive;
                    }
                    else if (rule.Owner.SPFarmId == Guid.Empty)//SSA Level rule.
                    {
                        ssaRuleSetting[rule.DisplayName] = rule.IsActive;
                    }
                }
            }
        }

        public void ImportBuildInAndSSAQeuryRuleSetting(IAveOSearchObjectOwner owningScope, Dictionary<Guid, bool> buildInRuleSetting, Dictionary<string, bool> ssaRuleSetting)
        {
            if (buildInRuleSetting.Count == 0 && ssaRuleSetting.Count == 0)
            {
                return;
            }

            var rules = this.mSearchServiceApplicationProxy.GetQueryRules(new SearchObjectFilter(((AveOSearchObjectOwner)owningScope).Owner));
            for (int index = 0; index < rules.Count; index++)
            {
                var rule = rules[index];
                if (rule.IsDeactivatedAtHigherLevel)//在parent节点把此rule inactive掉了。不能更新此rule的状态。
                {
                    continue;
                }

                bool isActive = true;
                bool needUpdate = false;
                if (rule.IsSystem)//Build in rule.
                {
                    if (buildInRuleSetting.TryGetValue(rule.Id, out isActive) && isActive != rule.IsActive)
                    {
                        needUpdate = true;
                    }
                }
                else if (rule.Owner.SPFarmId == Guid.Empty)//SSA level rule.
                {
                    if (ssaRuleSetting.TryGetValue(rule.DisplayName, out isActive) && isActive != rule.IsActive)
                    {
                        needUpdate = true;
                    }
                }

                if (needUpdate)
                {
                    rule.IsActive = isActive;
                    rule.UpdateActiveStatus();
                }
            }
        }

        public void DropScope(int scopeId)
        {
            mSearchServiceApplicationProxy.DropScope(scopeId);
        }

        public void DropRule(int ruleId)
        {
            mSearchServiceApplicationProxy.DropRule(ruleId);
        }

        public void DropDisplayGroup(int displayGroupId)
        {
            mSearchServiceApplicationProxy.DropDisplayGroup(displayGroupId);
        }
    }
}
