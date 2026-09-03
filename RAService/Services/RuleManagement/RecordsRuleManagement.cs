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
using AvePoint.CloudInsights.SDK.Model;
using AvePoint.Cryptography;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RuleManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using AvePoint.Wrapper.Restore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RuleManagement
{
    public class RecordsRuleManagement : IRecordsRuleManagement
    {
        private IRMMiscProfileDao MiscProfileDao => PlatformWindsorManager.GetService<IRMMiscProfileDao>();
        private IRMRunningJobRuleMappingDao RMRunningJobRuleMappingDao => PlatformWindsorManager.GetService<IRMRunningJobRuleMappingDao>();
        private ITermRuleAssociationDao TermRuleAssociationDao => PlatformWindsorManager.GetService<ITermRuleAssociationDao>();
        private IJobMonitorDao JobMonitorDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
        public Task<int> BatchDeleteRulesAsync(List<string> rulesIds)
        {
            var rules = GetAllByIdsAsync(rulesIds).GetAwaiter().GetResult();
            foreach (var tempRule in rules)
            {
                if (CheckRuleIsUsedByRunningJobAsync(tempRule).GetAwaiter().GetResult())
                {
                    throw new Exception(I18NEntity.GetString("RM_JS_RDM_EditRule_UsedByJob"));
                }
            }
            return MiscProfileDao.BatchDeleteAsync(rulesIds);
        }

        public bool CreateRecordsRule(Rule ruleInfo, bool validateRuleInfo = true, bool daoMigrated = false)
        {
            if (validateRuleInfo && MiscProfileDao.IsNameExist((int)ruleInfo.ProfileType, ruleInfo.Name))
            {
                throw new Exception(SOConstants.TheRuleNameAlreadyExists);
            }
            ruleInfo.Id = ruleInfo.Id != null ? ruleInfo.Id : new Guid().ToString();
            //validate
            string errorMessage = string.Empty;
            if (validateRuleInfo && !ValidateRuleInfo(ruleInfo, ref errorMessage))
            {
                throw new Exception(errorMessage);
            }
            return MiscProfileDao.Create(new DB.Model.RMMiscProfile()
            {
                Id = ruleInfo.Id,
                Type = (int)ruleInfo.ProfileType,
                Name = ruleInfo.Name,
                Extension = SerializerHelper.SerializeByDataContractSerializer(ruleInfo),
                DAOMigrated = daoMigrated
            }) == 0;
        }

        public async Task<bool> EditRecordsRuleAsync(Rule ruleInfo)
        {
            //validate + check if job running
            if (await CheckRuleIsUsedByRunningJobAsync(ruleInfo))
            {
                throw new Exception(I18NEntity.GetString("RM_JS_RDM_EditRule_UsedByJob"));
            }

            string errorMessage = string.Empty;
            if (!ValidateRuleInfo(ruleInfo, ref errorMessage))
            {
                throw new Exception(errorMessage);
            }

            var dbRule = MiscProfileDao.Load(ruleInfo.Id);
            if ((int)ruleInfo.ProfileType != dbRule.Type)
            {
                throw new Exception("Rule type changed.");
            }

            return await MiscProfileDao.UpdateAsync(new DB.Model.RMMiscProfile()
            {
                Id = ruleInfo.Id,
                Type = (int)ruleInfo.ProfileType,
                Name = ruleInfo.Name,
                Extension = SerializerHelper.SerializeByDataContractSerializer(ruleInfo),
                DAOMigrated = dbRule.DAOMigrated
            }) == 0;
        }

        private async Task<bool> CheckRuleIsUsedByRunningJobAsync(Rule rule)
        {
            if ((rule.SOFilters != null && rule.SOFilters.Count > 0)
                || (rule.OneDriveRule != null && rule.OneDriveRule.SOFilters != null && rule.OneDriveRule.SOFilters.Count > 0)
                || (rule.EXORule != null && rule.EXORule.SOFilters != null && rule.EXORule.SOFilters.Count > 0)
                || (rule.GoogleDriveRule != null && rule.GoogleDriveRule.SOFilters != null && rule.GoogleDriveRule.SOFilters.Count > 0)
                || (rule.TeamsRule != null && rule.TeamsRule.SOFilters != null && rule.TeamsRule.SOFilters.Count > 0))
            {
                if (await RMRunningJobRuleMappingDao.IsRuleUsedByJobAsync(TenantLocalValue.LogonGroupId, new Guid(rule.Id)))
                {
                    return true;
                }
            }

            //if rule is related with term
            var termIds = TermRuleAssociationDao.GetTermIdsByRuleId(rule.Id);
            if (termIds != null && termIds.Count > 0)
            {
                List<int> jobTypes = new List<int>();
                if ((rule.FSRule != null && rule.FSRule.SOFilters != null && rule.FSRule.SOFilters.Count > 0))
                {
                    jobTypes.Add((int)JobType.FSDisposal);
                    jobTypes.Add((int)JobType.FSDisposalSchedule);
                    jobTypes.Add((int)JobType.FSDisposalByClassCode);
                }

                if ((rule.PhysicalRule != null && rule.PhysicalRule.SOFilters != null && rule.PhysicalRule.SOFilters.Count > 0))
                {
                    jobTypes.Add((int)JobType.PhysicalDisposal);
                    jobTypes.Add((int)JobType.PhysicalRecordsDisposal);
                }

                if ((rule.SPLocalRule != null && rule.SPLocalRule.SOFilters != null && rule.SPLocalRule.SOFilters.Count > 0))
                {
                    jobTypes.Add((int)JobType.SPOnPremEnforceRuleAction);
                    jobTypes.Add((int)JobType.SPOnPremEnforceRuleActionSchedule);
                }

                if ((rule.BoxRule != null && rule.BoxRule.SOFilters != null && rule.BoxRule.SOFilters.Count > 0))
                {
                    jobTypes.Add((int)JobType.BoxRecordsDisposal);
                }

                if (jobTypes != null && jobTypes.Count > 0)
                {
                    var jobs = JobMonitorDao.Count(j => jobTypes.Contains(j.JobType) && (j.Status == (int)AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Wait || j.Status == (int)AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.InProgress));
                    if (jobs > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public Rule LoadRule(string ruleId)
        {
            var profile = MiscProfileDao.Load(ruleId);
            return SerializerHelper.DeserializeByDataContractSerializer<Rule>(profile.Extension);
        }

        public async Task<List<Rule>> GetAllAsync()
        {
            List<Rule> rules = new List<Rule>();
            var profiles = (await MiscProfileDao.FindListAsync(p => p.Type == (int)ProfileType.ArchiverRuleForRevIM)).ToList();
            foreach (var p in profiles)
            {
                rules.Add(SerializerHelper.DeserializeByDataContractSerializer<Rule>(p.Extension));
            }
            return rules;
        }
        private async Task<List<Rule>> GetAllByIdsAsync(List<string> rulesIds)
        {
            List<Rule> rules = new List<Rule>();
            var profiles = (await MiscProfileDao.FindListAsync(p => rulesIds.Contains(p.Id))).ToList();
            foreach (var p in profiles)
            {
                rules.Add(SerializerHelper.DeserializeByDataContractSerializer<Rule>(p.Extension));
            }
            return rules;
        }
        private bool ValidateRuleInfo(Rule rule, ref string errorMessage)
        {
            if (string.IsNullOrEmpty(rule.Name))
            {
                errorMessage = "Rule name can not be null or empty";
                return false;
            }
            if ((rule.SOFilters == null || rule.SOFilters.Count == 0)
                && (rule.EXORule != null && (rule.EXORule.SOFilters == null || rule.EXORule.SOFilters.Count == 0))
                && (rule.PhysicalRule != null && (rule.PhysicalRule.SOFilters == null || rule.PhysicalRule.SOFilters.Count == 0))
                && (rule.OneDriveRule != null && (rule.OneDriveRule.SOFilters == null || rule.OneDriveRule.SOFilters.Count == 0))
                && (rule.AzureFileRule != null && (rule.AzureFileRule.SOFilters == null || rule.AzureFileRule.SOFilters.Count == 0))
                && (rule.BoxRule != null && (rule.BoxRule.SOFilters == null || rule.BoxRule.SOFilters.Count == 0))
                && (rule.ConnectorRule != null && (rule.ConnectorRule.SOFilters == null || rule.ConnectorRule.SOFilters.Count == 0)))
            {
                errorMessage = "The criteria cannot be empty";
                return false;
            }
            if (rule.MoveToRecordCenterAndDelareSetting != null && rule.MoveToRecordCenterAndDelareSetting.OperateDataMode == OperatingSharePointDataMode.MoveToRecordCenterAndDelare)
            {
                if (rule.MoveToRecordCenterAndDelareSetting.DestinationLocation == null || rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url == null || rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.UserName == null || rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Password == null)
                {
                    errorMessage = "Destination location information cannot be empty while the rule is move to rule";
                    return false;
                }
                //if (!ValidateMoveToRecordInfo(rule.MoveToRecordCenterAndDelareSetting.DestinationLocation, ref message))
                //{
                //    return false;
                //}
                rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.UserName = Encoding.UTF8.GetString(CspCommunicationWrapper.UnWrapKey(rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.UserName));
                rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url = Encoding.UTF8.GetString(CspCommunicationWrapper.UnWrapKey(rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url));
            }
            //else if ((rule.EXORule == null || rule.EXORule.SOFilters == null) && (rule.PhysicalRule == null || rule.PhysicalRule.SOFilters == null) && (rule.FSRule == null || rule.FSRule.SOFilters == null) && (rule.SPLocalRule == null || rule.SPLocalRule.SOFilters == null) && (rule.AzureFileRule == null || rule.AzureFileRule.SOFilters == null) && (rule.ConnectorRule == null || rule.ConnectorRule.SOFilters == null))  //EXO and physical Rule Delete data Only,  do not check storage policy 
            //{
            //    if (string.IsNullOrEmpty(rule.StoragePolicyId) || string.IsNullOrEmpty(rule.StoragePolicyName) /*|| rule.StoragePolicyDto == null*/)
            //    {
            //        errorMessage = "Storage policy cannot be empty";
            //        return false;
            //    }
            //}
            //if ((string.IsNullOrEmpty(rule.DataEncryptionProfileId) && !string.IsNullOrEmpty(rule.DataEncryptionProfileName)) || (!string.IsNullOrEmpty(rule.DataEncryptionProfileId) && string.IsNullOrEmpty(rule.DataEncryptionProfileName)))
            //{
            //    errorMessage = "Data encryption profile info is imperfect";
            //    return false;
            //}
            if (rule.KeepDataOption == ((int)KeepDataOption.Delete | (int)KeepDataOption.Keep | (int)KeepDataOption.TagContent))
            {
                if (rule.TagContentInfo.Count <= 0)
                {
                    errorMessage = "Tag content info is imperfect";
                    return false;
                }
                else
                {
                    foreach (var item in rule.TagContentInfo)
                    {
                        if (item.Type == TagContentInfoType.Text || item.Type == TagContentInfoType.Number || item.Type == TagContentInfoType.Boolean || item.Type == TagContentInfoType.DateTime)
                        {
                            if (string.IsNullOrEmpty(item.ColumnName) || string.IsNullOrEmpty(item.Value))
                            {
                                errorMessage = "Column name or column value cannot be empty";
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}
