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
using Cloud.Sdk.Data.Nexus.Foundation;
using OpusJobType = AvePoint.RA.Contract.JobMonitor.JobType;

namespace RADiscoveryUnitTest.GControlPlatformTests.GControlJob;

[TestClass]
public class GControlJobUnitTests : GControlPlatformInitializeTest
{
    
    [TestMethod]
    public async Task CreateJob_ShouldBeSuccessful()
    {
        var result = await GControlPlatformJobService.CreatePlatformJob("","Test Google Apply Setting Job", OpusJobType.GoogleApplySettings, TenantLocalValue.LogonUserEmail);
        var platformJob = await GControlPlatformJobService.GetPlatformJobHistory(result);
        Assert.AreEqual(result, platformJob?.Id ?? Guid.Empty);
    }

    [TestMethod]
    public async Task GetJob_ShouldBeSuccessful()
    {
        var result = await GControlPlatformJobService.GetPlatformJobHistory(new Guid("ff525068-c9fa-4c9c-a2f4-722457b76215"));
        Assert.IsNotNull(result);
    }
    
    [TestMethod]
    public async Task UpdateJob_ShouldBeSuccessful()
    {
        var result = await GControlPlatformJobService.UpdatePlatformJob(new Guid("ff525068-c9fa-4c9c-a2f4-722457b76215"), JobStatus.RanToCompletion, DateTime.UtcNow);
        Assert.IsTrue(result);
    }
}