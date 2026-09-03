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

namespace RASalesforce.APIs;

public class SalesforceRestAPIService : SalesforceAPIService
{

    public static string ConnectedApplicationQuery = "SELECT+Id,Name,CreatedDate,CreatedById,LastModifiedDate,LastModifiedById,SystemModstamp,OptionsAllowAdminApprovedUsersOnly,OptionsRefreshTokenValidityMetric,OptionsHasSessionLevelPolicy,OptionsIsInternal,OptionsFullContentPushNotifications,OptionsCodeCredentialGuestEnabled,OptionsAppIssueJwtTokenEnabled,OptionsTokenExchangeManageBitEnabled,MobileSessionTimeout,PinLength,StartUrl,MobileStartUrl,RefreshTokenValidityPeriod,DeveloperName,Version,SessionTimeout,SessionPolicyAction,ConnAppPluginClassId,ExecutionUserId,SamlLoginInformation,Description,ContactEmail,ContactPhone,LogoUrl,InfoUrl,IconUrl,UvidTimeout,NamedUserUvidTimeout+FROM+ConnectedApplication";
    private string restApiEndpoint = "/services/data/v61.0/tooling";
    private string baseUrl;

    public SalesforceRestAPIService(SalesforceAPIProvider provider) : base(provider) { }


    public async Task<OrganizationLimits?> GetOrganizationLimitsAsync()
    {
        string url = this.restApiEndpoint + "/limits/";
        //Dictionary<string, string> header = new Dictionary<string, string>();
        return await InvokerAsync(async () =>
        {
            Dictionary<string, string> header = new Dictionary<string, string>();
            var token = await GetTokenAsync();
            header.TryAdd("Authorization", "Bearer " + token.AccessToken);
            using (HttpClient client = new())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<OrganizationLimits>(jsonResponse);
                }
                else
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    throw new Exception(jsonResponse);
                }
            }
        }, 3, 1000);
    }

    public async Task<RecordCount?> GetRecordsCountAsync()
    {
        string url = this.restApiEndpoint + "/limits/recordCount";        
        return await InvokerAsync(async () =>
        {
            Dictionary<string, string> header = new Dictionary<string, string>();
            var token = await GetTokenAsync();
            header.Add("Authorization", "Bearer " + token.AccessToken);
            using (HttpClient client = new())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<RecordCount>(jsonResponse);
                }
                else
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    throw new Exception(jsonResponse);
                }
            }
        }, 3, 1000);
    }
    
    public async Task<RecordCount?> GetRecordsCountByObjectsAsync(List<string> objectNames)
    {
        string url = this.restApiEndpoint + $"/limits/recordCount?sObjects={string.Join(',', objectNames)}";
        Dictionary<string, string> header = new Dictionary<string, string>();
        return await InvokerAsync(async () =>
        {
            var token = await GetTokenAsync();
            header.Add("Authorization", "Bearer " + token.AccessToken);
            using (HttpClient client = new())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<RecordCount>(jsonResponse);
                }
                else
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    throw new Exception(jsonResponse);
                }
            }
        }, 3, 1000);
    }

    public async Task<RecordCount?> GetRecordsCountV1Async()
    {
        string url = @"https://nero2-dev-ed.develop.my.salesforce.com/setup/org/orgstorageusage.jsp?id=00DHs00000BCEM1";
        Dictionary<string, string> header = new Dictionary<string, string>();
        return await InvokerAsync(async () =>
        {
            var token = await GetTokenAsync();
            header.Add("Authorization", "Bearer " + token.AccessToken);
            using (HttpClient client = new())
            {
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
                client.DefaultRequestHeaders.Add("Accept", "text/html");
                client.DefaultRequestHeaders.Add("Cookie", $"sid={token.AccessToken}");
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    Stream chunkedContent = await response.Content.ReadAsStreamAsync();
                    using (StreamReader reader = new StreamReader(chunkedContent))
                    {
                        var jsonResponse = await reader.ReadToEndAsync();
                        return JsonConvert.DeserializeObject<RecordCount>(jsonResponse);
                    }
                    //string jsonResponse = await response.Content.ReadAsStringAsync();
                    //return JsonConvert.DeserializeObject<RecordCount>(jsonResponse);
                }
                else
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    throw new Exception(jsonResponse);
                }
            }
        }, 3, 1000);
    }
    //public async Task<T> QueryAllAsync<T>(string query)
    //{
    //    var token = await GetTokenAsync();
    //    using (HttpClient client = new())
    //    {
    //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
    //        HttpResponseMessage response = await client.GetAsync($"{token.InstanceUrl}{RestAPIEndpoint}/query?q={query}");
    //        if (response.IsSuccessStatusCode)
    //        {
    //            string jsonResponse = await response.Content.ReadAsStringAsync();
    //            return JsonConvert.DeserializeObject<T>(jsonResponse);
    //        }
    //    }
    //    return default;
    //}

    internal override void InitClientSetting(SFToken token)
    {
        this.baseUrl = string.Format("{0}/services/data/", token.InstanceUrl);
        this.restApiEndpoint = baseUrl + "v62.0";
    }
}