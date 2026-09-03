public class ApiProxy {
    public static String SFNameSpace = 'AvePointOpus'; // dev deployment config
    public static String CalloutName = 'AvePointGraph';
    public static Integer HttpTimeout = 2*60*1000;
    public static String currentUserId;
    public static String currentUserName;
    public static String currentOrgId;

    public static void initializeUserInfo() {
        if (currentUserId == null) {
            currentUserId = UserInfo.getUserId();
            currentUserName = UserInfo.getUserName(); 
            currentOrgId = UserInfo.getOrganizationId();
        }
    }
    public static String CalloutInvoke(String method, Map<String,Object> paramArgsMap, String apiPath) {
        String result = '';
        try {
            initializeUserInfo();
            Http http = new Http();
            HttpRequest request = new HttpRequest();
            request.setMethod(method);
            request.setTimeout(HttpTimeout);
            request.setHeader('AppUserId', currentUserId);
            request.setHeader('AppUserName', currentUserName);
            request.setHeader('AppOrganizationId', currentOrgId);
            String requestUrl = RequestBuilder(method, paramArgsMap, apiPath);
            System.debug('Request Url:' + requestUrl);
            request.setEndpoint(requestUrl);
            if(method=='POST' && paramArgsMap.size() > 0){
                String jsonString = ConvertDictionaryToJson(paramArgsMap);              
                request.setHeader('Content-Type','application/json');               
                request.setBody(jsonString);
            }       
            HttpResponse response = http.send(request);
            result = response.getBody();
        }
        catch (Exception e)
        {
            System.debug('Faield to invoke the callout , err: ' + e);
            result = '{"statusCode":407,"response":""}'; // callout failed, not api failed
        }
        return result;
    }

    public static String PostInvokeWithJson(String jsonRequestBody, String apiPath) {
        String result = '';
        try {
            initializeUserInfo();
            Http http = new Http();
            HttpRequest request = new HttpRequest();
            request.setMethod('POST');
            request.setTimeout(HttpTimeout);
            request.setHeader('AppUserId', currentUserId);
            request.setHeader('AppUserName', currentUserName);
            request.setHeader('AppOrganizationId', currentOrgId);
            String requestUrl = 'callout:';
            if(SFNameSpace != '')
            {
                requestUrl = requestUrl + SFNameSpace + '__';
            }
            requestUrl = requestUrl + CalloutName + apiPath;
            System.debug('Request Url:' + requestUrl);
            request.setEndpoint(requestUrl);
            request.setHeader('Content-Type','application/json');               
                request.setBody(jsonRequestBody);
            HttpResponse response = http.send(request);
            result = response.getBody();
        }
        catch (Exception e)
        {
            System.debug('Faield to invoke the callout , err: ' + e);
            result = '{"statusCode":407,"response":""}'; // callout failed, not api failed
        }
        return result;
    }

    public static String RequestBuilder(String method, Map<String,Object> paramArgsMap, String apiPath) {
        String result = 'callout:';
        if(SFNameSpace != '')
        {
            result = result + SFNameSpace + '__';
        }
        result = result + CalloutName + apiPath;
        if( method=='GET' )
        {
            List<String> args = new List<String>();
            result = result + '?';
            for(String key :paramArgsMap.keyset()){
                Object value = paramArgsMap.get(key);
                args.add(key + '=' + value);
            }
            result+=String.join(args,'&');
        }
        return result;
    }

    private static String ConvertDictionaryToJson(Map<String,Object> paramArgsMap)
    {
        String result;
        for(String key :paramArgsMap.keyset()){
            Object value = paramArgsMap.get(key);
            result = Json.serialize(value);
        }
        return result;
    }

    public static String LicenseCalloutInvoke(String method, Map<String,Object> paramArgsMap, String apiPath) {
        initializeUserInfo();

        String result = '';
        HttpRequest request = new HttpRequest();
        String requestUrl = RequestBuilder(method, paramArgsMap, apiPath);

        request.setMethod(method);
        request.setTimeout(HttpTimeout);
        request.setHeader('AppUserId', currentUserId);
        request.setHeader('AppUserName', currentUserName);
        request.setHeader('AppOrganizationId', currentOrgId);
        request.setEndpoint(requestUrl);   

        System.debug('getLicense >> requestUrl:: ' + requestUrl);
        try {
            Http http = new Http();
            HttpResponse response = http.send(request);

            System.debug('getLicense >> response:: ' + response.getBody());
            Map<String, Object> resultMap = new Map<String, Object>();
            resultMap.put('statusCode', response.getStatusCode());
            resultMap.put('response', response.getBody());

            result = Json.serialize(resultMap);
        }
        catch (Exception e)
        {
            System.debug('getLicense >> error:: ' + e);
            if (result == '') {
                result = '{"statusCode":407,"response":""}';
            }
        }
        return result;
    }
}