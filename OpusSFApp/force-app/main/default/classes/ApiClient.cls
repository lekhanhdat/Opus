public class ApiClient 
{
  public static String GetSummaryStatisticalDataInfo()
  {
    String result = '';
    String apiPath = '/api/discovery/salesforce/GetSummaryStatisticalDataInfo';
    Map<String, Object> params = new Map<String,Object>();
    try
    {
        result = ApiProxy.CalloutInvoke('GET', params, apiPath);
    }
    catch(Exception e){
        System.debug('Failed to get all objects, error:' + e);
    }
    return result;
  }

  public static String GetInactiveAggregateInfo(String QueryParameter)
  {
    String result = '';
    String apiPath = '/api/discovery/salesforce/QueryInactiveAggregateInfo';
    try
    {
        result = ApiProxy.PostInvokeWithJson(QueryParameter, apiPath);
    }
    catch(Exception e){
        System.debug('Failed to QueryInactiveAggregateInfo objects, error:: ' + e);
    }
    return result;
  }

  public static String GetSearchObject(String QueryParameter)
  {
    String result = '';
    String apiPath = '/api/discovery/salesforce/SearchObject';
    try
    {
        result = ApiProxy.PostInvokeWithJson(QueryParameter, apiPath);
    }
    catch(Exception e){
        System.debug('Failed to SearchObject objects, error:: ' + e);
    }
    return result;
  }

  public static String GetFigureDataInfo(String QueryParameter)
  {
    String result = '';
    String apiPath = '/api/discovery/salesforce/QueryFigureDataInfo';
    try
    {
        result = ApiProxy.PostInvokeWithJson(QueryParameter, apiPath);
    }
    catch(Exception e){
        System.debug('Failed to QueryFigureDataInfo objects, error:: ' + e);
    }
    return result;
  }

  public static String GetInactiveFileExtensions(String QueryParameter)
  {
    String result = '';
    String apiPath = '/api/discovery/salesforce/QueryInactiveFileExtensions';
    try
    {
        result = ApiProxy.PostInvokeWithJson(QueryParameter, apiPath);
    }
    catch(Exception e){
        System.debug('Failed to QueryInactiveFileExtensions objects, error:: ' + e);
    }
    return result;
  }

  public static String GetInactiveSizeRanges(String QueryParameter) 
  {
    String result = '';
    String apiPath = '/api/discovery/salesforce/QueryInactiveSizeRanges';
    try
    {
        result = ApiProxy.PostInvokeWithJson(QueryParameter, apiPath);
    }
    catch(Exception e){
        System.debug('Failed to QueryInactiveSizeRanges objects, error:: '+ e);
    }
    return result;
  }
  
  public static String GetInactiveSummaryObjectTotalInfo(String QueryParameter) 
  {
    String result = '';
    String apiPath = '/api/discovery/salesforce/QueryInactiveSummaryObjectTotalInfo';
    try
    {
      result = ApiProxy.PostInvokeWithJson(QueryParameter, apiPath);
    }
    catch(Exception e){
      System.debug('Failed to QueryInactiveSummaryObjectTotalInfo objects, error:: '+ e);
    }
    return result;
  }

  public static String QueryAnalysis(String QueryParameter) 
  {
    String result = '';
    String apiPath = '/api/discovery/salesforce/QueryAnalysis';
    try
    {
      result = ApiProxy.PostInvokeWithJson(QueryParameter, apiPath);
    }
    catch(Exception e){
      System.debug('Failed to QueryAnalysis objects, error:: ' + e);
    }
    return result;
  }

  public static String GetWithoutInDateList() 
  {
    String result = '';
    String apiPath = '/api/discovery/salesforce/GetWithoutInDateList';
    Map<String, Object> params = new Map<String,Object>();
    try
    {
      result = ApiProxy.CalloutInvoke('GET', params, apiPath);
    }
    catch(Exception e){
      System.debug('Failed to GetWithoutInDateList objects, error:: ' + e);
    }
    return result;
  }

  public static String GetLicense()
  {
    String result = '';
    String apiPath = '/api/discovery/salesforce/GetLicense';
    Map<String, Object> params = new Map<String,Object>();
    try
    {
      result = ApiProxy.LicenseCalloutInvoke('GET', params, apiPath);
      System.debug('getLicense >> result:: ' + result);
    }
    catch(Exception e){
      System.debug('Failed to GetLicense, error:: ' + e);
    }
    return result;
  }
}