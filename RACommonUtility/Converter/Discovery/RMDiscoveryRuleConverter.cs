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
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.FileSystem;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Office365;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.RACommonUtility.Converter.Discovery
{
    public class RMDiscoveryRuleConverter
    {
        public static List<RMDiscoveryRuleDefinition> Convert(IEnumerable<RMDiscoveryOffice365RuleInfo> rules)
        {
            return rules?.Select(Convert)?.ToList();
        }

        public static List<RMDiscoveryRuleDefinition> Convert(IEnumerable<RMDiscoveryAOSPRuleInfo> rules)
        {
            return rules?.Select(Convert)?.ToList();
        }

        public static RMDiscoveryRuleDefinition Convert(RMDiscoveryOffice365RuleInfo ruleInfo)
        {
            return new RMDiscoveryRuleDefinition
            {
                Id = ruleInfo.Id,
                UniqueId = ruleInfo.UniqueId,
                Name = ruleInfo.Name,
                Description = ruleInfo.Description,
                Order = ruleInfo.Order,
                IsEnable = ruleInfo.IsEnable,
                Kind = ruleInfo.DefinitionKind,
                AnalyseMethod = ruleInfo.AnalyseMethod,
                CriteriaInfoes = JsonConvert.DeserializeObject<List<RMDiscoveryRuleCriteriaInfo>>(ruleInfo.CriteriaInfoesJson)
            };
        }

        public static RMDiscoveryOffice365RuleInfo ConvertToOffice365RuleInfo(RMDiscoveryRuleDefinition ruleDefinition, RMDiscoveryRuleDefinitionKind kind, RMDiscoveryRuleCategory category)
        {
            return new RMDiscoveryOffice365RuleInfo
            {
                Id = ruleDefinition.Id,
                UniqueId = ruleDefinition.UniqueId,
                Name = ruleDefinition.Name,
                Description = ruleDefinition.Description,
                Order = ruleDefinition.Order,
                IsEnable = ruleDefinition.IsEnable,
                Category = category,
                DefinitionKind = kind,
                AnalyseMethod = ruleDefinition.AnalyseMethod,
                CriteriaInfoesJson = JsonConvert.SerializeObject(ruleDefinition.CriteriaInfoes)
            };
        }

        public static RMDiscoveryAOSPRuleInfo ConvertToAOSPRuleInfo(RMDiscoveryRuleDefinition ruleDefinition, RMDiscoveryRuleDefinitionKind kind, RMDiscoveryRuleCategory category)
        {
            return new RMDiscoveryAOSPRuleInfo
            {
                Id = ruleDefinition.Id,
                UniqueId = ruleDefinition.UniqueId,
                Name = ruleDefinition.Name,
                Description = ruleDefinition.Description,
                Order = ruleDefinition.Order,
                IsEnable = ruleDefinition.IsEnable,
                Category = category,
                DefinitionKind = kind,
                AnalyseMethod = ruleDefinition.AnalyseMethod,
                CriteriaInfoesJson = JsonConvert.SerializeObject(ruleDefinition.CriteriaInfoes)
            };
        }

        public static RMDiscoveryGoogleRuleInfo ConvertToGoogleRuleInfo(RMDiscoveryRuleDefinition ruleDefinition, RMDiscoveryRuleDefinitionKind kind, RMDiscoveryRuleCategory category)
        {
            return new RMDiscoveryGoogleRuleInfo
            {
                Id = ruleDefinition.Id,
                UniqueId = ruleDefinition.UniqueId,
                Name = ruleDefinition.Name,
                Description = ruleDefinition.Description,
                Order = ruleDefinition.Order,
                IsEnable = ruleDefinition.IsEnable,
                Category = category,
                DefinitionKind = kind,
                AnalyseMethod = ruleDefinition.AnalyseMethod,
                CriteriaInfoesJson = JsonConvert.SerializeObject(ruleDefinition.CriteriaInfoes)
            };
        }

        public static RMDiscoveryFSRuleInfo ConvertToFileSystemRuleInfo(RMDiscoveryRuleDefinition ruleDefinition, RMDiscoveryRuleDefinitionKind kind, RMDiscoveryRuleCategory category)
        {
            return new RMDiscoveryFSRuleInfo
            {
                Id = ruleDefinition.Id,
                UniqueId = ruleDefinition.UniqueId,
                Name = ruleDefinition.Name,
                Description = ruleDefinition.Description,
                Order = ruleDefinition.Order,
                IsEnable = ruleDefinition.IsEnable,
                Category = category,
                DefinitionKind = kind,
                AnalyseMethod = ruleDefinition.AnalyseMethod,
                CriteriaInfoesJson = JsonConvert.SerializeObject(ruleDefinition.CriteriaInfoes)
            };
        }

        public static RMDiscoveryRuleDefinition Convert(RMDiscoveryGoogleRuleInfo ruleInfo)
        {
            return new RMDiscoveryRuleDefinition
            {
                Id = ruleInfo.Id,
                UniqueId = ruleInfo.UniqueId,
                Name = ruleInfo.Name,
                Description = ruleInfo.Description,
                Order = ruleInfo.Order,
                IsEnable = ruleInfo.IsEnable,
                Kind = ruleInfo.DefinitionKind,
                AnalyseMethod = ruleInfo.AnalyseMethod,
                CriteriaInfoes = JsonConvert.DeserializeObject<List<RMDiscoveryRuleCriteriaInfo>>(ruleInfo.CriteriaInfoesJson)
            };
        }

        public static RMDiscoveryRuleDefinition Convert(RMDiscoveryAOSPRuleInfo ruleInfo)
        {
            return new RMDiscoveryRuleDefinition
            {
                Id = ruleInfo.Id,
                UniqueId = ruleInfo.UniqueId,
                Name = ruleInfo.Name,
                Description = ruleInfo.Description,
                Order = ruleInfo.Order,
                IsEnable = ruleInfo.IsEnable,
                Kind = ruleInfo.DefinitionKind,
                AnalyseMethod = ruleInfo.AnalyseMethod,
                CriteriaInfoes = JsonConvert.DeserializeObject<List<RMDiscoveryRuleCriteriaInfo>>(ruleInfo.CriteriaInfoesJson)
            };
        }

        public static RMDiscoveryRuleDefinition Convert(RMDiscoveryFSRuleInfo ruleInfo)
        {
            return new RMDiscoveryRuleDefinition
            {
                Id = ruleInfo.Id,
                UniqueId = ruleInfo.UniqueId,
                Name = ruleInfo.Name,
                Description = ruleInfo.Description,
                Order = ruleInfo.Order,
                IsEnable = ruleInfo.IsEnable,
                Kind = ruleInfo.DefinitionKind,
                AnalyseMethod = ruleInfo.AnalyseMethod,
                CriteriaInfoes = JsonConvert.DeserializeObject<List<RMDiscoveryRuleCriteriaInfo>>(ruleInfo.CriteriaInfoesJson)
            };
        }
    }
}
