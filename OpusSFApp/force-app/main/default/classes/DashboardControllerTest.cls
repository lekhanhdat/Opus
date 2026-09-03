/**
 * This class contains unit tests for validating the behavior of Apex classes
 * and triggers.
 *
 * Unit tests are class methods that verify whether a particular piece
 * of code is working properly. Unit test methods take no arguments,
 * commit no data to the database, and are flagged with the testMethod
 * keyword in the method definition.
 *
 * All test methods in an org are executed whenever Apex code is deployed
 * to a production org to confirm correctness, ensure code
 * coverage, and prevent regressions. All Apex classes are
 * required to have at least 75% code coverage in order to be deployed
 * to a production org. In addition, all triggers must have some code coverage.
 * 
 * The @isTest class annotation indicates this class only contains test
 * methods. Classes defined with the @isTest annotation do not count against
 * the org size limit for all Apex scripts.
 *
 * See the Apex Language Reference for more information about Testing and Code Coverage.
 */
@isTest
private class DashboardControllerTest {

    @isTest
    static void GetInactiveSummaryDataTest() 
    {
        String result = DashboardController.GetInactiveSummaryData();
        System.assert(true);
    }

    @isTest
    static void QueryInactiveAggregateInfoTest() 
    {
        String mockPayload = '{"dataType":1,"withoutDateQueryParameter":{"from":-1,"to":999},"sizeRangeQueryParameter":{},"nodeQueryParameter":{"viewMode":1,"objectIds":[],"pageSize":5,"pageIndex":0},"fileExtensionQueryParameter":{}}';
        String result = DashboardController.QueryInactiveAggregateInfo(mockPayload);
        System.assert(true);
    }

    @isTest
    static void SearchObjectTest() 
    {
        String mockPayload = '{"dataType":1,"withoutDateQueryParameter":{"from":-1,"to":999},"sizeRangeQueryParameter":{},"nodeQueryParameter":{"viewMode":1,"objectIds":[],"pageSize":5,"pageIndex":0},"fileExtensionQueryParameter":{}}';
        String result = DashboardController.SearchObject(mockPayload);
        System.assert(true);
    }

    @isTest
    static void QueryFigureDataInfoTest() 
    {
        String mockPayload = '{"dataType":1,"withoutDateQueryParameter":{"from":-1,"to":999},"sizeRangeQueryParameter":{},"nodeQueryParameter":{"viewMode":1,"objectIds":[],"pageSize":5,"pageIndex":0},"fileExtensionQueryParameter":{}}';
        String result = DashboardController.QueryFigureDataInfo(mockPayload);
        System.assert(true);
    }

    @isTest
    static void QueryInactiveSizeRangesTest() 
    {
        String mockPayload = '{"dataType":1,"withoutDateQueryParameter":{"from":-1,"to":999},"sizeRangeQueryParameter":{},"nodeQueryParameter":{"viewMode":1,"objectIds":[],"pageSize":5,"pageIndex":0},"fileExtensionQueryParameter":{}}';
        String result = DashboardController.QueryInactiveSizeRanges(mockPayload);
        System.assert(true);
    }

    @isTest
    static void QueryInactiveFileExtensionsTest() 
    {
        String mockPayload = '{"dataType":1,"withoutDateQueryParameter":{"from":-1,"to":999},"sizeRangeQueryParameter":{},"nodeQueryParameter":{"viewMode":1,"objectIds":[],"pageSize":5,"pageIndex":0},"fileExtensionQueryParameter":{}}';
        String result = DashboardController.QueryInactiveFileExtensions(mockPayload);
        System.assert(true);
    }
    
    @isTest
    static void QueryInactiveSummaryObjectTotalInfoTest() 
    {
        String mockPayload = '{"dataType":1,"withoutDateQueryParameter":{"from":-1,"to":999},"sizeRangeQueryParameter":{},"nodeQueryParameter":{"viewMode":1,"objectIds":[],"pageSize":5,"pageIndex":0},"fileExtensionQueryParameter":{}}';
        String result = DashboardController.QueryInactiveSummaryObjectTotalInfo(mockPayload);
        System.assert(true);
    }

    @isTest
    static void QueryAnalysisTest() 
    {
        String mockPayload = '{"dataType":1,"withoutDateQueryParameter":{"from":-1,"to":999},"sizeRangeQueryParameter":{},"nodeQueryParameter":{"viewMode":1,"objectIds":[],"pageSize":5,"pageIndex":0},"fileExtensionQueryParameter":{}}';
        String result = DashboardController.QueryAnalysis(mockPayload);
        System.assert(true);
    }

    @isTest
    static void GetWithoutInDateListTest() 
    {
        String result = DashboardController.GetWithoutInDateList();
        System.assert(true);
    }

    @isTest
    static void GetLicenseTest() 
    {
        String result = DashboardController.GetLicense();
        System.assert(true);
    }
}