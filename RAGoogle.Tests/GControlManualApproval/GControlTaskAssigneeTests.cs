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
using System.Collections.Concurrent;
using AvePoint.RA.Contract.Google.GControlPlatform;
using AvePoint.RA.Contract.Google.Model;
using AvePoint.RA.Service.Services.Google.GControlPlatform;
using RAGoogleTests.GControlPlatformTests;
using RAGoogleTests.Stubs;
using RAManualApprovalCommon;

namespace RAGoogleTests.GControlManualApproval;

public class GControlTaskAssigneeTests : GControlPlatformInitializeTest
{
    private readonly IGControlPlatformApprovalProcessService _gControlPlatformApprovalProcessService = new GControlPlatformApprovalProcessServiceStub();
    
    [Fact]
    public async Task CacheWorkflowForUserTaskMapping_AvoidDuplicateStageId_GetAllDistinctItem_Concurrent()
    {
        // Arrange
        List<(Guid, Guid, string)> workflowForUserTaskMappings =
        [
            (Guid.Parse("11111111-1111-1111-1111-111111111111"), Guid.Parse("11111111-1111-1111-1111-111111111111"), "113436469886373527515"),
            (Guid.Parse("22222222-2222-2222-2222-222222222222"), Guid.Parse("22222222-2222-2222-2222-222222222222"), "113436469886373527515"),
            (Guid.Parse("22222222-2222-2222-2222-222222222222"), Guid.Parse("11111111-1111-1111-1111-111111111111"), "02bn6wsx3clh9n2"), // Duplicate WorkflowId with different StageId
            (Guid.Parse("33333333-3333-3333-3333-333333333333"), Guid.Parse("33333333-3333-3333-3333-333333333333"), "113436469886373527515"),
            (Guid.Parse("33333333-3333-3333-3333-333333333333"), Guid.Parse("33333333-3333-3333-3333-333333333333"), "113436469886373527515"),
        ];
        // Act
        await Parallel.ForEachAsync(workflowForUserTaskMappings, async (workflowForUserTaskMapping, _) =>
        {
            await ManualApprovalWorkflowManager.CacheGControlWorkflowIdAndStageIdMapping(new GControlWorkflowDto()
            {
                WorkflowId = workflowForUserTaskMapping.Item1,
                StageId = workflowForUserTaskMapping.Item2,
                ApproverId = workflowForUserTaskMapping.Item3
            });
        });
        // Assert
        var cachedList = ManualApprovalWorkflowManager.GetCachedGControlWorkflowForUserTaskMappings();
        Assert.True(cachedList.Count == 4);
    }
    
    [Fact]
    public void GetNewUserTaskMapping_GetUserFromStageId_OnlyAddNotExistAccount()
    {
        // Arrange
        List<GControlWorkflowDto> data =
        [
            new GControlWorkflowDto { WorkflowId = Guid.Parse("11111111-1111-1111-1111-111111111111"), StageId = Guid.Parse("11111111-1111-1111-1111-111111111111"), ApproverId = "113436469886373527515" },
            new GControlWorkflowDto { WorkflowId = Guid.Parse("22222222-2222-2222-2222-222222222222"), StageId = Guid.Parse("22222222-2222-2222-2222-222222222222"), ApproverId = "02bn6wsx3clh9n2" },
            new GControlWorkflowDto { WorkflowId = Guid.Parse("22222222-2222-2222-2222-222222222222"), StageId = Guid.Parse("11111111-1111-1111-1111-111111111111"), ApproverId = "113436469886373527515" },
            new GControlWorkflowDto { WorkflowId = Guid.Parse("33333333-3333-3333-3333-333333333333"), StageId = Guid.Parse("33333333-3333-3333-3333-333333333333"), ApproverId = "113436469886373527515"  },
        ];
        // Act
        var getUsers = ManualApprovalWorkflowManager.GetNewUserTaskMapping(data);
        // Assert
        Assert.Contains("113436469886373527515", getUsers);
        Assert.Contains("02bn6wsx3clh9n2", getUsers);
    }
    
}