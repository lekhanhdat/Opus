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

namespace AvePoint.Wrapper.Common.Office
{
    public interface IAveOSearchServiceApplicationProxy : IAveIisWebServiceApplicationProxy
    {
        int AddScope(IAveOScopeInfo scopeInfo, out int statusCode);
        int AddConsumer(string consumerName);
        int AddDisplayGroup(IAveODisplayGroupInfo displayGroupInfo, out int statusCode);
        int AddRule(IAveORuleInfo ruleInfo, int scopeId);
        List<string> GetConsumers();
        List<int> GetDisplayGroupListInfo(int displayGroupId);
        IAveODisplayGroupInfo GetDisplayGroupInfo(int displayGroupId);
        int GetDisplayGroupIDFromName(string consumerName, string name);
        List<IAveODisplayGroupInfo> GetDisplayGroupsInfo();
        List<IAveORuleInfo> GetRulesInfo(int scopeId, out int statusCode);
        IAveOSearchServiceApplication GetProxy(IAveServiceContext ServiceContext);
        IAveOScopeInfo GetScopeInfo(int scopeId);
        int GetScopeIDFromName(string consumerName, string name);
        List<IAveOScopeInfo> GetScopesInfo();
        void SetScopeInfo(IAveOScopeInfo scopeInfo);
        void SetDisplayGroupInfo(IAveODisplayGroupInfo displayGroupInfo);
        void SetDisplayGroupListInfo(int displayGroupId, List<int> scopeIds);
        IAveOLocationConfiguration[] GetLocationConfigurations(out long lastUpdate, out bool useCrawlProxy);
        IAveOSearchServiceApplicationInfo GetSearchServiceApplicationInfo();

        /// <summary>
        /// 删除Scope。
        /// </summary>
        /// <param name="scopeId"></param>
        void DropScope(int scopeId);

        /// <summary>
        /// 删除Rule。
        /// </summary>
        /// <param name="ruleId"></param>
        void DropRule(int ruleId);

        /// <summary>
        /// 删除display Group。
        /// </summary>
        /// <param name="displayGroupId"></param>
        void DropDisplayGroup(int displayGroupId);
        IAveOFASTAdminProxy FASTAdminProxy { get; }

        #region for13

        void ExportQueryConfiguration(IAveOSearchObjectOwner owningScope, out IAveOSearchQueryConfigurationSettings outPackage);
        void ImportQueryConfiguration(IAveOSearchObjectOwner owningScope, AveSearchInfo searchInfo13, Dictionary<string, string> queryTemplateParameters);
        IAveOSearchSchemaConfigurationSettings ExportSchema(IAveOSearchObjectOwner owner);
        void ImportSchema(IAveOSearchObjectOwner owningScope,AveSearchInfo searchInfo13);
        void ExportBuildInAndSSAQeuryRuleSetting(IAveOSearchObjectOwner owningScope, Dictionary<Guid,bool> buildInRuleSetting, Dictionary<string,bool> ssaRuleSetting);
        void ImportBuildInAndSSAQeuryRuleSetting(IAveOSearchObjectOwner owningScope, Dictionary<Guid, bool> buildInRuleSetting, Dictionary<string, bool> ssaRuleSetting);

        #endregion
    }
}
