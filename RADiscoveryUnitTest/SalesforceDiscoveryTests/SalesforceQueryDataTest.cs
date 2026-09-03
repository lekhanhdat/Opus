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
using AvePoint.RA.Contract.Salesforce.Model;

namespace RADiscoveryUnitTest.SalesforceDiscoveryTests;

[TestClass]
public class SalesforceQueryDataTest : SalesforceInitializeTest
{
    [TestMethod]
    public async Task GetSummaryStaticDataInfoShouldBeSuccessful()
    {
        var summaryStaticDataInfo = await DataQueryService.GetSummaryStaticalDataInfoAsync(null);
        Assert.IsTrue(summaryStaticDataInfo.ObjectTotalCount > 0);
    }
    
    [TestMethod]
    public async Task GetInactiveAggregateInfoShouldBeSuccessful()
    {
        RMDiscoverySalesforceQueryParameter salesforceQueryParameter = new()
        {
            OrganizationId = "00DNS000006GOpJ2AW",
            WithoutDateQueryParameter = new()
            {
                From = -1,
                To = 999
            },
            NodeQueryParameter = new()
            {
                ViewMode = 0
            }
        };
        var inactiveAggregateInfo = await DataQueryService.QueryInactiveAggregateInfo(salesforceQueryParameter);
        Assert.IsTrue(inactiveAggregateInfo.RecordsTotalCount > 0);
    }
    
    [TestMethod]
    public async Task GetInactiveFileExtensionsShouldBeSuccessful()
    {
        RMDiscoverySalesforceQueryParameter salesforceQueryParameter = new()
        {
            OrganizationId = "00DNS000006GOpJ2AW",
            WithoutDateQueryParameter = new()
            {
                From = -1,
                To = 999
            },
            NodeQueryParameter = new()
            {
                ViewMode = RMSFDiscoveryNodeViewMode.File
            }
        };
        var inactiveFileExtensions = await DataQueryService.QueryInactiveFileExtensionsAsync(salesforceQueryParameter);
        Assert.IsTrue(inactiveFileExtensions.Count > 0);
    }
    
    [TestMethod]
    public async Task GetInactiveSizeRangesShouldBeSuccessful()
    {
        RMDiscoverySalesforceQueryParameter salesforceQueryParameter = new()
        {
            OrganizationId = "00DNS000006GOpJ2AW",
            WithoutDateQueryParameter = new()
            {
                From = -1,
                To = 999
            },
            NodeQueryParameter = new()
            {
                ViewMode = RMSFDiscoveryNodeViewMode.File
            }
        };
        var sizeRanges = await DataQueryService.QueryInactiveSizeRangesAsync(salesforceQueryParameter);
        Assert.IsTrue(sizeRanges.Count > 0);
    }
    
    [TestMethod]
    public async Task GetAnalysisForFileShouldBeSuccessful()
    {
        RMDiscoverySalesforceQueryParameter salesforceQueryParameter = new()
        {
            OrganizationId = "00DNS000006GOpJ2AW",
            WithoutDateQueryParameter = new()
            {
                From = -1,
                To = 999
            },
            NodeQueryParameter = new()
            {
                ViewMode = RMSFDiscoveryNodeViewMode.File,
                PageIndex = 0,
                PageSize = 10,
            }
        };
        var queryAnalysis = await DataQueryService.QueryAnalysis(salesforceQueryParameter);
        Assert.IsTrue(queryAnalysis.Items.Count > 0);
    }
    
    [TestMethod]
    public async Task GetAnalysisForDataShouldBeSuccessful()
    {
        RMDiscoverySalesforceQueryParameter salesforceQueryParameter = new()
        {
            OrganizationId = "00DNS000006GOpJ2AW",
            WithoutDateQueryParameter = new()
            {
                From = -1,
                To = 999
            },
            NodeQueryParameter = new()
            {
                ViewMode = RMSFDiscoveryNodeViewMode.Data,
                PageIndex = 0,
                PageSize = 10,
            }
        };
        var queryAnalysis = await DataQueryService.QueryAnalysis(salesforceQueryParameter);
        Assert.IsTrue(queryAnalysis.Items.Count > 0);
    }
    
    [TestMethod]
    public async Task GetInactiveSummaryObjectTotalInfoShouldBeSuccessful()
    {
        RMDiscoverySalesforceQueryParameter salesforceQueryParameter = new()
        {
            OrganizationId = "00DNS000006GOpJ2AW",
            WithoutDateQueryParameter = new()
            {
                From = -1,
                To = 999
            },
            NodeQueryParameter = new()
            {
                ViewMode = RMSFDiscoveryNodeViewMode.None,
            }
        };
        var inactiveSummaryObjectTotalInfo = await DataQueryService.QueryInactiveSummaryObjectTotalInfo(salesforceQueryParameter);
        Assert.IsTrue(inactiveSummaryObjectTotalInfo.Count > 0);
    }
    
    [TestMethod]
    public async Task GetFigureDataInfoShouldBeSuccessful()
    {
        RMDiscoverySalesforceQueryParameter salesforceQueryParameter = new()
        {
            OrganizationId = "00DNS000006GOpJ2AW",
            WithoutDateQueryParameter = new()
            {
                From = -1,
                To = 999
            },
            NodeQueryParameter = new()
            {
                ViewMode = RMSFDiscoveryNodeViewMode.Data,
            }
        };
        var dataInfo = await DataQueryService.QueryFigureDataInfo(salesforceQueryParameter);
        Assert.IsTrue(dataInfo.Count > 0);
    }
}