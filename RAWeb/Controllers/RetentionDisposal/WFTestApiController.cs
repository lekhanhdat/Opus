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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.Workflow;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RecordManager.Controllers.DisposalActivity
{
    public class ApprovalResult
    {
        public string CorrelationId { get; set; }
        public DisposalReviewActionEnum Result { get; set; }

    }
    [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess)]
    public class WFTestApiController : BaseApiController
    {
#if DEBUG
        private static readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        //public IDisposalReviewWFService DisposalReviewWFService { get; set; }

        private IAccountWrapperService _UserWrapperService;
        private IAccountWrapperService UserWrapperService => PlatformWindsorManager.GetService(ref _UserWrapperService);
        private IRMScopePermissionDao _RMScopePermissionDao;
        private IRMScopePermissionDao RMScopePermissionDao => PlatformWindsorManager.GetService(ref _RMScopePermissionDao);

        [AllowAnonymous]
        [HttpPost]
        public bool UpdateApprovalResult([FromBody] ApprovalResult result)
        {
            logger.Info($"Manual approve CorrelationId : {result.CorrelationId}, approve result : {result.Result}");
            return true;
        }

        [HttpGet]
        public string SearchUserOrGroup(string name)
        {
            var accounts = UserWrapperService.SearchAccounts(TenantLocalValue.LogonGroupId, name);
            return JsonConvert.SerializeObject(accounts);
            //return accounts.Select(o => o.DisplayName).ToList();
        }

        [HttpGet]
        public string CheckPermission(string scope, string scopePath, int user, int group)
        {
            var result = RMScopePermissionDao.GetExcludeScopes(scope, new List<int>() { user, group});
            return string.Join(",", result.ToArray());
        }

        [HttpGet]
        public DisposalReviewRequestInfo StartNew()
        {
            var request = new DisposalReviewRequestInfo()
            {
                RequestId = Guid.NewGuid(),
                DefinitionId = Guid.NewGuid(),
            };

            //request.InstanceId =  DisposalReviewWFService.StartWorkflow(request, Build());

            return new DisposalReviewRequestInfo() 
            { 
                InstanceId = request.InstanceId, 
                DefinitionId = request.DefinitionId, 
                RequestId = request.RequestId 
            };
        }

        [HttpGet]
        public int Starts(int total=1000)
        {
            for(int i=0; i < total; i++)
            {
                StartNew();
            }

            return total;
        }

        private WorkflowDefinitionDto GetData()
        {
            var definition = new WorkflowDefinitionDto()
            {
                Name = "Desposal Review workflow",
                Type = AvePoint.RA.Contract.RMWeb.RMWorkflowType.DisposalReview
            };

            var content = new RMWorkflowContentDto() { WorkflowNodes = new System.Collections.Generic.List<RMWorkflowStepNode>()};
            definition.Content = content;

            var destory1 = new RMWorkflowStepNode()
            {
                Id = Guid.NewGuid(),
                Name = "DestoryNode1",
                Status = WorkflowNodeStatus.Approve,
                NodeType = WorkflowNodeType.Destroy
            };

            content.WorkflowNodes.Add(destory1);

            var destory2 = new RMWorkflowStepNode()
            {
                Id = Guid.NewGuid(),
                Name = "DestoryNode2",
                Status = WorkflowNodeStatus.Approve,
                NodeType = WorkflowNodeType.Destroy
            };

            content.WorkflowNodes.Add(destory2);

            var notDestory1 = new RMWorkflowStepNode()
            {
                Id = Guid.NewGuid(),
                Name = "NotDestoryNode1",
                Status = WorkflowNodeStatus.Reject,
                NodeType = WorkflowNodeType.NotDestroy
            };

            content.WorkflowNodes.Add(notDestory1);

            var notDestory2 = new RMWorkflowStepNode()
            {
                Id = Guid.NewGuid(),
                Name = "NotDestoryNode2",
                Status = WorkflowNodeStatus.Reject,
                NodeType = WorkflowNodeType.NotDestroy
            };

            content.WorkflowNodes.Add(notDestory2);


            var reviewNode2 = new RMWorkflowStepNode()
            {
                Id = Guid.NewGuid(),
                Name = "ReviewNode2",
                Status = WorkflowNodeStatus.Reject,
                NodeType = WorkflowNodeType.DisposalReview,
                ChildrenIds = new System.Collections.Generic.List<Guid>() { destory1.Id, notDestory1.Id }
            };

            content.WorkflowNodes.Add(reviewNode2);

            var reviewNode3 = new RMWorkflowStepNode()
            {
                Id = Guid.NewGuid(),
                Name = "ReviewNode3",
                Status = WorkflowNodeStatus.ApproveOrReject,
                NodeType = WorkflowNodeType.DisposalReview,
                ChildrenIds = new System.Collections.Generic.List<Guid>() { destory2.Id, notDestory2.Id }
            };

            content.WorkflowNodes.Add(reviewNode3);

            var reviewNode1 = new RMWorkflowStepNode()
            {
                Id = Guid.NewGuid(),
                Name = "ReviewNode1",
                Status = WorkflowNodeStatus.Approve,
                NodeType = WorkflowNodeType.DisposalReview,
                ChildrenIds = new System.Collections.Generic.List<Guid>() { reviewNode2.Id, reviewNode3.Id }
            };

            content.WorkflowNodes.Add(reviewNode1);


            var beginReviewNode = new RMWorkflowStepNode()
            {
                Id = Guid.NewGuid(),
                Name = "StartReviewNode",
                Status = WorkflowNodeStatus.None,
                NodeType = WorkflowNodeType.BeginDisposalReview,
                ChildrenIds = new System.Collections.Generic.List<Guid>() { reviewNode1.Id}
            };

            content.WorkflowNodes.Add(beginReviewNode);

            return definition;
        }

        //[HttpGet]
        //public string Build()
        //{
        //    var definition = GetData();
        //    return XamlBuilder.BuildXaml(definition);
        //}

        //[HttpGet]
        //public string Validate()
        //{
        //    var xamlStr = Build();
        //    var errorList = XamlBuilder.ValidateXaml(xamlStr);
        //    var sb = new StringBuilder();
        //    foreach(var error in errorList)
        //    {
        //        sb.AppendLine(error);
        //    }

        //    if (sb.Length > 0) return sb.ToString();

        //    return "Validate succeed.";
        //}

        //[HttpGet]
        //public void Resume(Guid requestId, Guid instanceId, string bookmark, bool approve)
        //{
        //    var request = new DisposalReviewRequestInfo()
        //    {
        //        RequestId = requestId,
        //        InstanceId = instanceId,
        //        Action = approve? DisposalReviewActionEnum.Approve : DisposalReviewActionEnum.Reject
        //    };


        //    DisposalReviewWFService.Resume(request, Build(), bookmark);
        //}

        //[HttpGet]
        //public void Cancel(Guid instanceId)
        //{
        //    var request = new DisposalReviewRequestInfo()
        //    {
        //        InstanceId = instanceId,
        //    };

        //    DisposalReviewWFService.Cancel(request, Build());
        //}
#endif
    }
}
