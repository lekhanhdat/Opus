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
using System.Threading.Tasks;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.Object;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.RA.Contract.RMWeb.Rule;
using AvePoint.RA.Contract.JobMonitor;

namespace AvePoint.RA.Contract.DocAve
{
    public interface IRuleManagerService
    {
        /// <summary>
        /// Currently only support fs ,later support all data source.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        List<Rule> GetRulesFromRecords();
        List<Rule> GetFSRulesFromRecords();

        Rule GetSpecifyTeamsArchiverBackupRule();
        List<Rule> GetRulesByIds(List<Guid> ids);

        /// <summary>
        /// Use for get RMRuleInfos
        /// </summary>
        /// <returns></returns>
        Task<List<RMRuleInfos>> GetRuleInfosFromDAAsync();

        Task<List<RMRuleInfos>> GetRuleInfosFromRecordsAsync();

        List<RuleDto> GetBaseRulesFromDB();

        Task<List<RMRuleInfos>> GetSearchRuleFromDBAsync(RuleParameter SearchValue);

        Task<List<RMRuleInfos>> GetSearchRuleAsync(RulePageRequestModel requestModels);

        Task<List<RMRuleInfos>> GetSimpleRulesFromDBAsync(List<Guid> containerIds = null);

        Task<List<RMRuleInfos>> GetSimpleRecordsRulesFromDBAsync(List<Guid> containerIds = null);

        Task<List<RMRuleInfos>> GetSimpleArchiverRulesFromDBAsync(List<Guid> containerIds = null);

        Task<List<RMRuleInfos>> GetCanCopyRulesByTermIdAsync(int termId, int moduleType);
        Task<List<RMRuleInfos>> GetCanCopyRulesForDisableClassificationAsync(int moduleType);

        DestinationLocationInfo ConverDADestinationInfo(MoveDestinationInfo info);

        //RAReturnMessage ValidateMoveUrl(DestinationLocationInfo destinationInfo);

        /// <summary>
        /// Rule中的IncludeNew置成非null, 例如"1", 以标记是RA创建的Rule
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        System.Threading.Tasks.Task BuildManualAprovalJobScheduleForCreateRule(RMRuleInfos ruleInfo);
        System.Threading.Tasks.Task BuildManualAprovalJobScheduleForEditRule(RMRuleInfos ruleInfo);

        Task<RAReturnMessage> CreateRuleInDAAsync(RMRuleInfos rule);
        Task<RAReturnMessage> CreateImportRuleInDAAsync(RMRuleInfos rule);
        Task<RAReturnMessage> ModifyRuleInDAAsync(RMRuleInfos rule);

        Task<Rule> BuildRMRuleAsync(RMRuleInfos info);

        Task<RAReturnMessage> DeleteRulesAsync(List<string> ids);

        Task<RMRuleInfos> LoadRuleAsync(string ruleId, bool isControlPlus = false);

        /// <summary>
        /// 获取所有的Archiver Regular Rule, 目前还没有区分RA Create的
        /// </summary>
        /// <returns></returns>
        Task<List<Rule>> GetRulesFromDAAsync();
        void ResetSOFilter(Rule rule);
        RMRuleTermsDto GetRuleTermInfos(List<RMRuleTermInfos> infos);

        System.Threading.Tasks.Task GenerateReportForRuleReportAsync(string folderPath, string fileName, string sheetName, string ruleId);

        System.Threading.Tasks.Task SyncDARuleToRecordsAsync();
        Rule ConvertToEXORule(Rule rule);
        Rule ConvertToOneDriveRule(Rule rule);
        Rule ConvertToTeamsRule(Rule rule);
        Rule ConvertToPhysicalRule(Rule rule);
        Task<List<string[]>> ConvertRuleInfosToListAsync();
        Task<List<string>> GetBaseRulesNameFromDBAsync(List<string> ids);

        string ConvertPolicyLevelToI18NStr(PolicyLevel level);
        Task<RAReturnMessage> SyncADUsersAsync(RMRuleInfos ruleInfo);
        List<AOSUserDto> Convert2AOSUserDtos(List<UserInfo> users);
        List<UserInfo> Convert2RecordOwnerInfos(List<AOSUserDto> users);
        Task<List<RMRuleInfos>> GetExchangeRulesAsync(List<Guid> containerIds = null);
        Task<List<RMRuleInfos>> GetGoogleRulesAsync(List<Guid> containerIds = null);
        Task<(List<RMRuleInfos>, List<Guid>)> GetGoogleRulesAndMixedRuleIdsAsync();
        Task<List<RMRuleInfos>> GetRulesByDataSourceAsync(int dataSource,List<Guid> containerIds = null);
        Task<List<RMRuleInfos>> GetArchiverRulesByDataSourceAsync(int dataSource, List<Guid> containerIds = null);

        Task<RMRuleInfos> ConvertToRuleInfoAsync(Rule rule, bool isControlPlus = true);
        void EnableInsightsDataCollection(List<RuleFilter> filters);
        string GetArchiverRuleActionStringForDiscoveryOptimization(Rule rule, bool isSimulation = false);
        string GetArchiverRuleActionString(Rule rule, JobType jobType);
    }
}
