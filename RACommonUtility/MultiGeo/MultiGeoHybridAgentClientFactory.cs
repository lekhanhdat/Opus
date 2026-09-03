using AvePoint.Hybrid.ClientCore;
using AvePoint.Hybrid.ClientLibrary;
using AvePoint.Hybrid.ClientLibrary.SDK;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Tenant;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace AvePoint.RA.RACommonUtility.MultiGeo;

internal static class MultiGeoHybridAgentClientFactory
{
    public static HybridAgentApiClient Create(string apiUrl)
    {
        var identityServer = RMGlobalConfiguration.AppConfig[RMAppSettingKey.IDENTITY_SERVICE_URL];
        var indentityClientId = RMGlobalConfiguration.AppConfig[RMAppSettingKey.CLIENT_ID_IN_IDENTITY_SERVICE];
        var certificate = RMCertificateHelper.GetCertificate(RMCertNames.AvePointRecords);

        var services = new ServiceCollection();
        services.AddPublicApiCloudSdk(RecordsConstants.RECORDS_APPLICATION_NAME, certificate)
            .ConfigureIdentityServer(identityServer, indentityClientId, HBContractConstants.HybridInernalScope, true)
            .ConfigureDefaultHttpClient("RAMultiGeoClient", client =>
            {
                client.ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new HttpClientHandler()
                    {
#if DEBUG
                        ServerCertificateCustomValidationCallback = (msg, cert, chain, err) => true
#endif
                    };
                });
            })
            .AddHybridAgentApi(apiUrl);

        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetService<ICloudSdkHybridAgentClientFactory>();
        return factory.CreateHybridAgentClient(TenantLocalValue.LogonGroupId, HBContractConstants.HybridInernalScope);
    }
}