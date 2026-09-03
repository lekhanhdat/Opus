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
using AvePoint.GCommon.Contract.CloudAppAdmin.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.Rule;
using AvePoint.RA.DB.Model;
using DocAveOnline.WebApi.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMRuleDao: IBaseDao<RMRule>
    {
        void AddOrUpdateRMRule(RMRule rule, Guid? containerId = null);
        RMRule GetRuleById(Guid ruleId);
        void DeleteRule(List<Guid> ruleId);
        /// <summary>
        /// 只有在 Cloud Archiver Migration Job里可以调用此方法来清理历史数据
        /// </summary>
        Task<int> DeleteMigratedRulesAsync();
        /// <summary>
        /// 只有在 Cloud Archiver Migration Job里可以调用此方法来清理历史数据
        /// </summary>
        Task<int> DeleteMigratedRuleContainerMembershipsAsync();
        List<RMRule> GetAllRules();
        Task<List<RMRule>> GetRulesWithoutRemovedAsync();
        Task<List<RMRule>> GetGoogleRulesWithoutRemovedAsync();
        List<RMRule> GetRulesByIds(List<Guid> ids);
        Task<List<RMRule>> GetRuleByLevelAsync(PolicyLevel level);
        List<RMRule> GetSearchRules(List<RuleModel> ruleModels, string SearchValue, Guid? containerId = null);

        List<RMRule> GetAvailableRules(List<Guid> containerIds = null);
        Task<List<RMRule>> GetAvailableRulesBySearch(RulePageRequestModel pageRequest);
        List<RMRule> GetAvailableFSRules(List<Guid> containerIds = null);
        List<RMRule> GetRecordsAvailableRules(List<Guid> containerIds = null);
        List<RMRule> GetArchiverAvailableRules(List<Guid> containerIds = null);
        Dictionary<Guid, string> GetRuleContainerNameMemberships(List<Guid> ruleIds);
        //List<RMRule> GetAvailableFSRules();
        bool IsExistRule(string name, Guid id);

        public List<Guid> GetTeamsArchiverRuleIdsByLevels(List<GCommon.Contract.CommonFilter.PolicyLevel> levels);

        RMRuleContainer UpsertRuleContainer(RMRuleContainer ruleContainer);
        List<RMRuleContainer> GetRuleContainersByPager(RuleContainerQuery query, List<Guid> ruleContainers);
        Dictionary<Guid, int> GetRuleContainersMapping(List<Guid> ruleContainerIds);
        RMRuleContainer GetRuleContainersById(Guid guid);
        int GetRuleContainersCount(string searchKey, List<Guid> ruleContainers);
        bool CheckRuleContainerNameExist(string name);
        bool DeleteRuleContainer(Guid containerId);
        List<RMRuleContainer> GetAllRuleContainers(List<Guid> ruleContainers = null);
        RMRuleContainer GetRuleContainersByRuleId(Guid ruleId);
        Dictionary<Guid, Guid> GetAllRulesContainerIDs();
        List<RMRuleContainer> GetRuleContainersByRuleIds(IEnumerable<Guid> ruleIds);
        RAReturnMessage CheckContainerCrossSecurityGroup(Guid oldContainerId, Guid newContainerId, string ruleId);
        List<RMRule> GetAvailableRules(List<RuleModel> ruleModels, List<Guid> containerIds = null);
        List<int> GetRuleIntIdsByRuleGuIds(List<Guid> ids);
        Task<IEnumerable<RMRuleContainer>> LoadRuleContainerByPager(int pageIndex, int pageSize);
        Task<IEnumerable<RMRule>> LoadRulesByPager(int pageIndex, int pageSize);
        Task<IEnumerable<RMRuleContainerMembership>> LoadRuleContainerMembershipByPager(int pageIndex, int pageSize);
        Task<long> MultiGeoInsertRuleContainerTableAsync(IEnumerable<RMRuleContainer> ruleContainers);
        Task<long> MultiGeoDeleteAllRuleContainerAsync();
        Task<long> MultiGeoInsertRuleTableAsync(IEnumerable<RMRule> rules);
        Task<long> MultiGeoDeleteAllRuleAsync();
        Task<long> MultiGeoInsertRuleContainerMembershipTableAsync(IEnumerable<RMRuleContainerMembership> ruleContainerMemberships);
        Task<long> MultiGeoDeleteAllRuleContainerMembershipAsync();
    }
}
