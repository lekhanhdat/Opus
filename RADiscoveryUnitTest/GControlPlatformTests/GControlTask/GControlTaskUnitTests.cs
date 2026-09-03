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
using AvePoint.RA.Contract.Tenant;
using AvePoint.Records.Core.Utilities.Extensions;
using Cloud.Sdk.Data.Nexus.Common;
using Cloud.Sdk.Data.Nexus.Governance;

namespace RADiscoveryUnitTest.GControlPlatformTests.GControlTask;

[TestClass]
public class GControlTaskUnitTests : GControlPlatformInitializeTest
{
    [TestMethod]
    public async Task CreateTask_ShouldBeSuccessful()
    {
        var result = await GControlPlatformTaskService.CreateOpusTask();
        Assert.IsTrue(result);
    }
    
    [TestMethod]
    public async Task GetTask_ShouldBeSuccessful()
    {
        Guid taskId = $"{TenantLocalValue.LogonGroupId}_{TaskType.InformationLifecycleManualApproval}".ToMd5();
        var result = await GControlPlatformTaskService.GetPlatformTask(taskId);
        Assert.IsNotNull(result);
    }
    
    [TestMethod]
    public async Task TaskId_FromTenantId_ShouldBeHash()
    {
        Guid taskId = $"{TenantLocalValue.LogonGroupId}_{TaskType.InformationLifecycleManualApproval}".ToMd5();
        Assert.AreEqual(taskId, GControlPlatformTaskService.GetTaskId());
    }

    [TestMethod]
    public async Task GetTasks_ShouldBeSuccessful()
    {
        var request = new CommonRequest()
        {
            Paging = new PagingClause()
            {
                Top = 100,
                Skip = 0
            },
            OrderBy = new OrderByClause()
            {
                ColumnName = "DataObjectName",
                Ascending = true
            }
        };
        var result = await GControlPlatformTaskService.SearchPlatformTasks(request);
        Assert.IsTrue(result.Count > 0);
    }
}