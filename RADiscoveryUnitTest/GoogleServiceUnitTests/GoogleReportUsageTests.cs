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
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Service.RMTasks;
using RAGoogle.GoogleObjDiscover;
using RAGoogle.Services;

namespace RADiscoveryUnitTest.GoogleServiceUnitTests;

[TestClass]
public class GoogleReportUsageTests : GoogleServiceInitializeTest
{
    private RMAosGoogleAppProfile _googleAppProfile;
    [TestInitialize]
    public void InitGoogleDrive()
    {
         _googleAppProfile =
            RMAosApiClient.GetGoogleAppProfile(TenantLocalValue.LogonGroupId, "C04g1u0oi", true);
    }

    [TestMethod]
    public async Task GetCustomerReportUsage()
    {
        using GoogleActivityService service = new(_googleAppProfile);
        DateTime startTime = DateTime.UtcNow.AddDays(-3);
        var usageReport = await service.GetCustomerDriveReportUsageAsync(startTime) / 1024 / 1024;
        Assert.IsTrue(usageReport > 0, $"Customer usage report size is {usageReport}MB");
    }
    
    [TestMethod]
    public async Task StorageUsageTestAsync()
    {
        var executor = new UpdateAosStatisticsSizeExecutor();
        executor.UpdateSizeToAOS();
    }
}