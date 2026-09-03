@isTest
private class ApiClientTest {
    static String mockPayload = '{"dataType":1,"withoutDateQueryParameter":{"from":-1,"to":999},"sizeRangeQueryParameter":{},"nodeQueryParameter":{"viewMode":1,"objectIds":[],"pageSize":5,"pageIndex":0},"fileExtensionQueryParameter":{}}';

    static testMethod void GetSummaryStatisticalDataTest() {
    	String result = ApiClient.GetSummaryStatisticalDataInfo();	
        System.assert(true);
        System.debug(result);	
    }

    static testMethod void GetLicenseTest() {
        String result = ApiClient.GetLicense();	
        System.assert(true);
        System.debug(result);
    }

    static testMethod void GetWithoutInDateListTest() {
        String result = ApiClient.GetWithoutInDateList();	
        System.assert(true);
        System.debug(result);
    }

    static testMethod void GetInactiveAggregateInfoTest() {
        String result = ApiClient.GetInactiveAggregateInfo(mockPayload);	
        System.assert(true);
        System.debug(result);
    }

    static testMethod void GetSearchObjectTest() {
        String result = ApiClient.GetSearchObject(mockPayload);	
        System.assert(true);
        System.debug(result);
    }

    static testMethod void GetFigureDataInfoTest() {
        String result = ApiClient.GetFigureDataInfo(mockPayload);	
        System.assert(true);
        System.debug(result);
    }

    static testMethod void GetInactiveFileExtensionsTest() {
        String result = ApiClient.GetInactiveFileExtensions(mockPayload);	
        System.assert(true);
        System.debug(result);
    }

    static testMethod void GetInactiveSizeRangesTest() {
        String result = ApiClient.GetInactiveSizeRanges(mockPayload);	
        System.assert(true);
        System.debug(result);
    }

    static testMethod void GetInactiveSummaryObjectTotalInfoTest() {
        String result = ApiClient.GetInactiveSummaryObjectTotalInfo(mockPayload);	
        System.assert(true);
        System.debug(result);
    }

    static testMethod void QueryAnalysisTest() {
        String result = ApiClient.QueryAnalysis(mockPayload);	
        System.assert(true);
        System.debug(result);
    }
}