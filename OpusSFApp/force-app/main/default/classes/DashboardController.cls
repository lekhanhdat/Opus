public with sharing class DashboardController {
    @RemoteAction
    @AuraEnabled(cacheable=true)
    public static String GetInactiveSummaryData()
    {
        String result = '';
        try
        {
            result = ApiClient.GetSummaryStatisticalDataInfo();
        }
        catch(Exception e){
            System.debug('Failed to get summary data for inactive gadget:: ' + e);
        }
        return result;
    }

    @RemoteAction
    @AuraEnabled(cacheable=true)
    public static String QueryInactiveAggregateInfo(String payload)
    {
        String result = '';
        System.debug('payload11;; ' + payload);
        try
        {
            result = ApiClient.GetInactiveAggregateInfo(payload);
        }
        catch(Exception e){
            System.debug('Failed to get inactive aggregate info:: ' + e);
        }
        return result;
    }
    
    @RemoteAction
    @AuraEnabled(cacheable=true)
    public static String SearchObject(String payload)
    {
        String result = '';
        try
        {
            result = ApiClient.GetSearchObject(payload);
        }
        catch(Exception e){
            System.debug('Failed to get Search Object:: ' + e);
        }
        return result;
    }

    @RemoteAction
    @AuraEnabled(cacheable=true)
    public static String QueryFigureDataInfo(String payload)
    {
        String result = '';
        try
        {
            result = ApiClient.GetFigureDataInfo(payload);
        }
        catch(Exception e){
            System.debug('Failed to get Query Figure Data Info:: ' + e);
        }
        return result;
    }
    
    @RemoteAction
    @AuraEnabled(cacheable=true)
    public static String QueryInactiveSizeRanges(String payload)
    {
        String result = '';
        try
        {
            result = ApiClient.GetInactiveSizeRanges(payload);
        }
        catch(Exception e){
            System.debug('Failed to get inactive aggregate info:: ' + e);
        }
        return result;
    }

    @RemoteAction
    @AuraEnabled(cacheable=true)
    public static String QueryInactiveFileExtensions(String payload)
    {
        String result = '';
        try
        {
            result = ApiClient.GetInactiveFileExtensions(payload);
        }
        catch(Exception e){
            System.debug('Failed to get Inactive File Extensions:: ' + e);
        }
        return result;
    }

    @RemoteAction
    @AuraEnabled(cacheable=true)
    public static String QueryInactiveSummaryObjectTotalInfo(String payload)
    {
        String result = '';
        try
        {
            result = ApiClient.GetInactiveSummaryObjectTotalInfo(payload);
        }
        catch(Exception e){
            System.debug('Failed to get table analysis total data:: ' + e);
        }
        return result;
    }

    @RemoteAction
    @AuraEnabled(cacheable=true)
    public static String QueryAnalysis(String payload)
    {
        String result = '';
        try
        {
            result = ApiClient.QueryAnalysis(payload);
        }
        catch(Exception e){
            System.debug('Failed to get table analysis data:: ' + e);
        }
        return result;
    }
    
    @RemoteAction
    @AuraEnabled(cacheable=true)
    public static String GetWithoutInDateList()
    {
        String result = '';
        try
        {
            result = ApiClient.GetWithoutInDateList();
        }
        catch(Exception e){
            System.debug('Failed to get date range data:: ' + e);
        }
        return result;
    }

    @RemoteAction
    @AuraEnabled(cacheable=false)
    public static String GetLicense()
    {
        String result = '';
        try
        {
            result = ApiClient.GetLicense();
        }
        catch(Exception e){
            System.debug('Failed to get date range data:: ' + e);
        }
        return result;
    }
}