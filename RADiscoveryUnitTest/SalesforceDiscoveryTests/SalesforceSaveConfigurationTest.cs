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

using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Salesforce;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Salesforce.Model;
using AvePoint.RA.Service.Services.Discovery.Salesforce.Configuration;
using AvePoint.RA.Service.Services.Discovery.Salesforce.Work.Trigger;

namespace RADiscoveryUnitTest.SalesforceDiscoveryTests;

[TestClass]
public class SalesforceSaveConfigurationTest : SalesforceInitializeTest
{
    [TestMethod]
    public async Task SizeRangeInfosShouldNotBeEmptyOrNull()
    {
        var configuration = await ConfigurationService.GetConfigurationInfoAsync();
        Assert.IsTrue(configuration.SizeRangeInfoes.IsNotNullOrEmpty());
    }
    
    [TestMethod]
    public async Task DateRangeInfosShouldNotBeEmptyOrNull()
    {
        var configuration = await ConfigurationService.GetConfigurationInfoAsync();
        Assert.IsTrue(configuration.DateRangeInfoes.IsNotNullOrEmpty());
    }
    
    [TestMethod]
    public async Task AddConfigurationShouldBeSuccessful()
    {
        //var aosOrganizations = await DataQueryService.GetAllOrganizations();
        /*List<RMDiscoverySalesforceOrgnization> organizations = aosOrganizations.Select(org => new RMDiscoverySalesforceOrgnization
        {
            Id = org.Id,
            Name = org.Name,
            Email = org.Email,
        }).ToList();*/
        List<RMDiscoverySalesforceOrgnization> organizations = [new()
        {
            Id = "00DNS000006GOpJ2AW",
            Name = "AvePoint",
            Email = "dereknguyen@avepoint.com"
        }];
        RMDiscoverySalesforceConfigurationInfo configurationInfo = new()
        {
            ScopeInfo = new()
            {
                Organizations = organizations
            },
            DateRangeInfoes = RMDiscoverySalesforceDefaultConfigurationInfo.DEFAULT_DATE_RANGE_INFOES,
            SizeRangeInfoes = RMDiscoverySalesforceDefaultConfigurationInfo.DEFAULT_SIZE_RANGE_INFOES
        };
        var responseMessage = await ConfigurationService.AddOrUpdateConfigurationInfoAsync(configurationInfo);
        Assert.AreEqual(responseMessage.MessageType, RAMessageType.Successful);
    }
    
    [TestMethod]
    public async Task RunTriggerJobShouldBeSuccessful()
    {
        var triggerJob = new RMDiscoverySalesforceJobTrigger();
        await triggerJob.TriggerAsync();
    }
    
    [TestMethod]
    public async Task GetSalesforceTenantShouldBeSuccessful()
    {
        var organizations = await DataQueryService.GetAllOrganizations();
        Assert.IsTrue(organizations.Any());
    }
    
    [TestMethod]
    public async Task LatestSalesforceJobStatusShouldBeNotNoneStatus()
    {
        var job = await JobManagementService.GetLatestAsync();
        Assert.IsTrue(job.Status != RMDiscoveryJobStatus.None);
    }
}