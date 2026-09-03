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
using Cloud.Sdk.Data.Nexus.Common;

namespace RADiscoveryUnitTest.GControlPlatformTests.GControlApprovalProcess;

[TestClass]
public class GControlApprovalProcessUnitTests : GControlPlatformInitializeTest
{
    [TestMethod]
    public async Task GetApprovalProcess_ById_ShouldBeSuccessful()
    {
        var approvalProcess = await GControlPlatformApprovalProcessService.GetPlatformApprovalProcess(new Guid("ea0c79ab-cf8e-4e81-a1dc-09ec19992295"));
        Assert.IsNotNull(approvalProcess);
    }
    
    [TestMethod]
    public async Task GetApprovalProcess_ByRequestDto_ShouldBeSuccessful()
    {
        var request = new CommonRequest()
        {
            Paging = new PagingClause()
            {
                Top = 100,
                Skip = 0
            },
            OrderBy = new()
            {
                ColumnName = "Name",
                Ascending = true
            }
        };
        var approvalProcess = await GControlPlatformApprovalProcessService.SearchPlatformApprovalProcesses(request);
        Assert.IsTrue(approvalProcess.Count > 0);
    }
}