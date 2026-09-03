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
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMWorkflowDefinitionDao : IBaseDao<RMWorkflowDefinition> {

        Task<RMWorkflowDefinition> LoadAsync(Guid id);

        Task SaveWorkflowAsync(WorkflowDefinitionDto dto);
        Task UpsertReplicaWorkflowAsync(WorkflowDefinitionDto dto);
        RMWorkflowDefinition LoadWorkflow(Guid id);
        RMWorkflowDefinition GetWorkflowByReferenceId(Guid id);
        RMWorkflowDefinition LoadWorkflow(string name);
        void DeleteWorkflow(Guid id);
        List<RMWorkflowDefinition> QueryWorkflows(ProcessQueryDto queryDto, out int totalCount);
        List<string> GetReviewerIds(Guid workflowId);
        bool IsRunningWorkflow(Guid id);
        void CheckSameWorkflow(WorkflowDefinitionDto dto);
        List<RMWorkflowDefinition> GetAllWorkflows();
        List<RMWorkflowInstance> GetInstances(List<string> userIds);
        List<RMWorkflowInstance> GetInstancesByHasSiteOwnersReviewerTypeDefinition(List<string> definitionReferenceIds, List<string> userAndGroupIds);
        List<RMWorkflowInstance> GetInstances(List<Guid> instanceIds);
        List<string> GetReviewerIdsByStepId(Guid id);
        List<string> GetReviewersByStepIdAndSiteId(Guid workflowInstanceId, Guid stepId, Guid siteId);
        Task<RMWorkflowInstance> GetWorkflowInstanceAsync(Guid id);
        List<Guid> GetCompleteInstanceIds();
        bool NeedUpgradeVersion(WorkflowDefinitionDto dto);
        List<RMWorkflowInstance> GetAllInstances();
        void AddExcludedOwnerForInstance(Guid instanceId, string ownerId, string stepId);

        bool ValidateHasCompleteWorkflows(List<Guid> instanceIds);

        RMWorkflowDefinition GetWorkflowByName(string name);

        Task<List<RMWorkflowDefinition>> GetCustomNotificationWorkflowAsync();
        Task<IEnumerable<RMWorkflowDefinition>> LoadWorkflowDefinitionsByPager(int pageIndex, int pageSize);
        Task<IEnumerable<RMWorkflowStepConfiguration>> LoadWorkflowStepConfigurationByPager(int pageIndex, int pageSize);
        Task<long> MultiGeoInsertWorkflowDefinitionTableAsync(IEnumerable<RMWorkflowDefinition> workflowDefinitions);
        Task<long> MultiGeoDeleteAllWorkflowDefinitionAsync();
        Task<long> MultiGeoInsertWorkflowStepConfigurationTableAsync(IEnumerable<RMWorkflowStepConfiguration> workflowStepConfigurations);
        Task<long> MultiGeoDeleteAllWorkflowStepConfigurationAsync();
    }
}
