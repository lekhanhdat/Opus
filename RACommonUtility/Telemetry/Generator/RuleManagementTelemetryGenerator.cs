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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Dao;
using Cloud.Sdk.Telemetry.Data.CloudRecords;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Telemetry.Generator
{
    public class RuleManagementTelemetryGenerator : TelemetryGenerator
    {

        private readonly IRMRuleDao RMRuleDao = PlatformWindsorManager.GetService<IRMRuleDao>();

        public override TelemetryModule Module => TelemetryModule.RuleManagement;

        protected override Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>> EventTypeGenerateMapping =>
            new Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>>
            {
                //{ TelemetryEventType.RuleAdded, AnalysisRuleInfos },
                //{ TelemetryEventType.RuleModified, AnalysisRuleInfos },
                //{ TelemetryEventType.RuleDeleted, AnalysisRuleInfos },
            };

        public CloudRecordsCommonRecord AnalysisRuleInfos(IList<object> args)
        {
            var record = new CloudRecordsRuleManagementRecord();

            var rules = RMRuleDao.GetAvailableRules();

            var sourceMappingCriterias = new Dictionary<SourceFlag, HashSet<string>>
            {
                { SourceFlag.SharePoint, new HashSet<string>() },
                { SourceFlag.SharePointOnPrem, new HashSet<string>() },
                { SourceFlag.FileSystem, new HashSet<string>() },
                { SourceFlag.Exchange, new HashSet<string>() },
                { SourceFlag.OneDrive, new HashSet<string>() },
            };

            var levels = new HashSet<string>();

            Parallel.ForEach(rules, item =>
            {
                var policyLevel = (PolicyLevel)item.RuleLevel;
                levels.Add(policyLevel.ToString());
                var sourceFlag = GetRule(item.Extension, out var rule);
                rule.SOFilters.ForEach(filter =>
                {
                    var criteriaType = filter.Rule.ToString();
                    criteriaType = criteriaType.Substring(0, criteriaType.IndexOf('('));
                    criteriaType = criteriaType.Substring(0, criteriaType.Length - 4);
                    sourceMappingCriterias[sourceFlag].Add(criteriaType);
                });
            });

            record.RuleCount = rules.Count;
            record.RuleLevels = string.Join(",", levels);

            var ruleCriteriaTypesKeyValues = new List<KeyValuePair<string, string>>();
            foreach(var source in sourceMappingCriterias)
            {
                var ruleCriteraTypes = "{" + string.Join(",", source.Value) + "}";
                ruleCriteriaTypesKeyValues.Add(new KeyValuePair<string, string>(source.Key.ToString(), ruleCriteraTypes));
            }

            record.RuleCriteriaTypes = string.Join(",", ruleCriteriaTypesKeyValues);

            return record;
        }

        private SourceFlag GetRule(string ruleExtension, out Rule rule)
        {
            rule = SerializerHelper.DeserializeByDataContractJsonSerializer<Rule>(ruleExtension);
            if (rule.SPLocalRule != null)
            {
                rule = rule.SPLocalRule;
                return SourceFlag.SharePointOnPrem;
            }
            else if (rule.EXORule != null)
            {
                rule = rule.EXORule;
                return SourceFlag.Exchange;
            }
            else if (rule.OneDriveRule != null)
            {
                rule = rule.OneDriveRule;
                return SourceFlag.OneDrive;
            }
            else if (rule.FSRule != null)
            {
                rule = rule.FSRule;
                return SourceFlag.FileSystem;
            }
            else if (rule.AzureFileRule != null)
            {
                rule = rule.AzureFileRule;
                return SourceFlag.AzureFileShare;
            }
            else if (rule.ConnectorRule != null)
            {
                rule = rule.ConnectorRule;
                return SourceFlag.Connector;
            }
            return SourceFlag.SharePoint;
        }
    }
}
