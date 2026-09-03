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

using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using System;
using System.Collections.Generic;
using System.IO;
using AvePoint.RA.Contract.DocAve.SOArchiver;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.RA.Contract.Explorer;
using System.Threading.Tasks;
using AvePoint.RA.Contract.RMWeb.CP;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IRMReportService
    {
        string StartReportJob(JobType jobType, int profileId, bool IsOrphanedTermReport = false, bool isRetiredTermReport = false);

        string StartArchivedSiteReportJob(int profileId);

        string GetMetaDataColumnName(Guid webAppId);

        Task<List<TermTreeNode>> GetRATermTreeNodesAsync();

        /// <summary>
        /// 从TermTree上获取被选中的Term的信息集合
        /// </summary>
        Task<Dictionary<Guid, RMTermIdentity>> GetTermIDsFromBCSTermTreeAsync(string ext1);

        Task<string> GetJobMessageForFSAsync(string jobId);

        DateTime GetUtcTimePoint(string ext1);

        RMSPTreeNode GetFarmSPTreeNode(string ext2);

        // [Obsolete("方法已经停用")]
        //Dictionary<int, Rule> GetRules(Guid webApplicationId, DateTime timePoint);

        /// <summary>
        /// 获取term相关的Rule信息
        /// </summary>
        Task<Dictionary<Guid, RMRuleItemCollection>> GetTermAndRuleMappingsAsync(DateTime timePoint);

        Dictionary<Guid, RMRuleItemCollection> GetTermAndRuleMappingsNew(DateTime timePoint, SourceFlag flag);

        Task<RAReturnMessage> BuildProfileAsync(RMProfileDto profile);

        Task<RAReturnMessage> BuildJobNotificationProfileAsync(JobNotificationDto jobNotificationInfo);

        Task<RAReturnMessage> EditJobNotificationProfileAsync(JobNotificationDto jobNotificationInfo);

        /// <summary>
        /// 不包含被remove的profile
        /// </summary>
        /// <param name="pageInfo"></param>
        /// <returns></returns>
        Task<ShowProfilesReportPageInfo> GetProfilesAsync(ShowProfilesReportPageInfo pageInfo);

        /// <summary>
        /// 包含被remove的profile
        /// </summary>
        /// <param name="pageInfo"></param>
        /// <returns></returns>
        Task<ShowProfilesReportPageInfo> GetAllProfilesAsync(ShowProfilesReportPageInfo pageInfo);
        RMProfileDto GetProfileByIdForReportJob(string Id);
        Task<RMProfileDto> GetProfileByIdAsync(string Id);
        Task<bool> GenarateReportSchedule(string scheduleId);
        Task<bool> UpdateProfileScheduleIdAsync(int profileId, string scheduleId);
        Task <List<RMProfileDto>> GetJobNotificationProfiles();
        Task<RAReturnMessage> EidtProfileAsync(RMProfileDto profile);
        Task<(bool, List<string> runningJobProfileNames)> DeleteProfilesAsync(DelProfileInfo dpi);
        void SyncReportJobDatas(IEnumerable<BaseReport> jobDetails, BaseJobDto jobInfo);
        void UpdateReportJobDatas(IEnumerable<BaseReport> jobDetails, BaseJobDto jobInfo);
        void FinalUpdateAndWaitCompleted();
        Task<string> GetCommonReportJobDatasAsync(ShowReportQuery query);

        Task<(IEnumerable<BaseReport>,int)> GetReportJobDatasAsync(int PageSize, int StartPage, string conditionFilter, 
            BaseJobDto jobInfo, string sortKey = null, bool isAscending = true);
        ReportFilter GetReportJobFilterData(ShowReportQuery query);
        Task<bool> GenerateReportAsync(BaseJobDto jobInfo, bool IsOrphanedTermReport = false, bool isRetiredTermReport = false);

        Task<List<ProfileSimpleInfo>> GetProfilesByTypesAsync(List<JobType> jobTypes, List<SourceFlag> sources);

        List<KeyValuePair<string, string>> GetProfilesByIds(List<int> ids);

        SOArchiverSettings GetSOArchiverSettings();
        

        int GetPageIndexByProfileId(int profileId);

        Task<Dictionary<Guid, RMTermIdentity>> GetOrphanedTermsOfRMAsync();

        Task<Dictionary<Guid, RMTermIdentity>> GetRetiredTermsOfRMAsync();

        Task<List<TermTreeNode>> GetRATermTreeNodeOfOrphanedTermAsync();

        Task<ShowProfilesReportPageInfo> GetTermUsageAndOrphanedTermProfilesAsync(ShowProfilesReportPageInfo pageInfo);

        Task<ShowProfilesReportPageInfo> GetAvailableSpaceReportProfilesAsync(ShowProfilesReportPageInfo pageInfo);

        Task<int> GetLocationTermIdFromProfileIdAsync(string profileId);

        string GetRemoteFarmId();

        List<PolicyLevel> GetRuleLevels(Dictionary<Guid, RMRuleItemCollection> mTermAndRulesMapping);

        bool CheckHasLowLevelRule(List<PolicyLevel> levels, PolicyLevel curLevel);
        Task<string> RealRunReportJobAsync(JobType jobType, string jobRunByUser, int profileId, bool IsOrphanedTermReport, bool isRetiredTermReport = false);

        Dictionary<Guid, RMRuleItemCollection> GetTermAndRuleMappingsForEXO(DateTime timePoint);
        bool CheckFSRootNode(string treeNodesJsonStr);
        bool CheckBoxRootNode(string treeNodesJsonStr);
        List<RMTermDto> GetTermTree(string termJsonStr);

        Task<string[][]> GenerateReportForJobAsync(int jobType, string[][] datas, int newJobType,IEnumerable<BaseReport> reports, bool isCreateHeader);

        RAReturnMessage RunExportReportJob(string reportParameters);

        Task<string> RealRunExportReportJobAsync(string reportParameters);
        Task<List<RMSPTreeNode>> AssembleSitesAsync(RMProfileDto dto, RMBrowseTreeNodeSourceType type, bool needValidSiteExist = true);
        void SetRestoreReportDisplayMod(bool setDynamicSizeDisplay);

        void DeleteJobNotificationProfile(List<int> profileIds);
    }

}
