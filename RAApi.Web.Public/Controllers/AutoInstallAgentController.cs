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
using AvePoint.GCommon.Utility;
using AvePoint.Hybrid.Contract.Object;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Cryptography;
using AvePoint.RA.Common.Security;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Certficate;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Extension;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.RACommonUtility.MultiGeo;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [Route("api/AutoInstallAgent/[action]")]
    public class AutoInstallAgentController : RAWebApiBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(AutoInstallAgentController));
        private IAgentMgmtService _AgentMgmtService;
        private IAgentMgmtService AgentMgmtService => PlatformWindsorManager.GetService(ref _AgentMgmtService);
        private ICertificateService _CertificateService;
        private ICertificateService CertificateService => PlatformWindsorManager.GetService(ref _CertificateService);
        private IKeyValueService _KeyValueService;

        private IKeyValueService KeyValueService => PlatformWindsorManager.GetService(ref _KeyValueService);
        private IMultiGeoSettingService MultiGeoSettingService => PlatformWindsorManager.GetService<IMultiGeoSettingService>();
        private IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        private void AssemblePublicKey([FromBody] AgentConfigurtion config)
        {
            var cert = RMCertificateHelper.GetCertificate(RMCertNames.AvePointRecords);
            if (cert == null)
            {
                throw new Exception($"No certificate found for {RMCertNames.AvePointRecords}");
            }

            config.RecordsCertContent = Convert.ToBase64String(cert.Export(X509ContentType.Cert));
        }

        private async Task PopulatePublicUrlsAsync(RMAgentDto agent, AgentConfigurtion conf)
        {
            conf.RecordsApiUrl = RMGlobalConfiguration.AppConfig[RMAppSettingKey.PUBLIC_RECO_API_URL];
            conf.IdentityServiceUrl = RMGlobalConfiguration.AppConfig[RMAppSettingKey.PUBLIC_IDENTITY_SERVICE_URL];
            conf.SiginalRServiceUrl = RMGlobalConfiguration.AppConfig[RMAppSettingKey.PUBLIC_SIGNALR_SERVER_URL];

            if (!await MultiGeoSettingService.IsEnableMultiGeoFeature() || !MultiGeoDataCenterService.IsMainDC())
            {
                return;
            }

            var targetDCInternalName = string.IsNullOrWhiteSpace(agent?.DCInternalName)
                ? RMSSOHelper.CurrentDCName
                : agent.DCInternalName;

            if (string.IsNullOrWhiteSpace(targetDCInternalName))
            {
                return;
            }

            conf.CurrentDC = targetDCInternalName;

            var mainDCInternalName = MultiGeoDataCenterService.GetMainDC();
            if (targetDCInternalName.Equals(mainDCInternalName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var recoApiUrls = RMGlobalConfiguration.AppConfig.GetMultiGeoPublicRecoApiUrl();
            if (recoApiUrls.TryGetValue(targetDCInternalName, out var recoApiUrl) && !string.IsNullOrWhiteSpace(recoApiUrl))
            {
                conf.RecordsApiUrl = recoApiUrl;
            }

            var signalRUrls = RMGlobalConfiguration.AppConfig.GetMultiGeoPublicSignalRServerUrl();
            if (signalRUrls.TryGetValue(targetDCInternalName, out var signalRUrl) && !string.IsNullOrWhiteSpace(signalRUrl))
            {
                conf.SiginalRServiceUrl = signalRUrl;
            }
        }
        [HttpPost]
        //[Microsoft.AspNetCore.Mvc.TypeFilter(typeof(ValidateAntiForgeryTokenFilterAttribute))]
        ////[FileDownloadFilter]
        public async Task<byte[]> DownloadAgentConfig([FromBody] string agentId)
        {
            try
            {
                var agent = AgentMgmtService.Get(Guid.Parse(agentId), true);
                var conf = AgentMgmtService.DownloadConfig(agent);

                if (conf == null)
                {
                    return null;
                }

                AssemblePublicKey(conf);

                await PopulatePublicUrlsAsync(agent, conf);

                var confJson = JsonConvert.SerializeObject(conf);
                var confBytes = System.Text.Encoding.UTF8.GetBytes(confJson);
                Stream stream = new MemoryStream(AESEncriptionHelper.Encrypt(confBytes, agent.GetAESEncryptKey()));
                if (stream is MemoryStream memoryStream)
                {
                    return memoryStream.ToArray();
                }

                using var output = new MemoryStream();
                stream.CopyTo(output);
                return output.ToArray();
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while downloading agent config file, error : {e}");
                throw;
            }
        }
        [HttpPost]
        public string GetInstallationCode([FromBody] string agentId)
        {
            var dto = AgentMgmtService.Get(new Guid(agentId));
            return dto?.InstallationCode;
        }
        [HttpPost]
        public async Task<AgentResultDto> CreateAgent([FromBody] RMAgentDto dto)
        {
            if (dto == null || !IsValidAgentType(dto))
            {
                return new AgentResultDto
                {
                    AgentCreateResult = RMAgentCreateResult.Failed
                };
            }

            if (await CheckSameAgentNameAsync(dto))
            {
                return new AgentResultDto
                {
                    AgentCreateResult = RMAgentCreateResult.SameNameExist
                };
            }

            var certs = await GetAllCertificatesAsync(markDefault: true);
            var installationCode = string.Empty;
            var authCode = string.Empty;
            var clientId = GetClientId();
            var certificateId = Guid.Empty;
            var createdAgentId = Guid.Empty;

            if (string.IsNullOrEmpty(clientId))
            {
                return new AgentResultDto
                {
                    AgentCreateResult = RMAgentCreateResult.NoClientId
                };
            }

            if (certs.Count == 0)
            {
                return new AgentResultDto
                {
                    AgentCreateResult = RMAgentCreateResult.NoCertificate
                };
            }

            installationCode = GenerateRandomString();
            authCode = GenerateRandomString();
            certificateId = certs.First(o => o.IsDefault).Id;

            return await RouteMultiGeoApiActionAsync(dto,
                MultiGeoOperationType.CreateAgent,
                async request =>
                {
                    var result = new AgentResultDto();
                    request.InstallationCode = installationCode;
                    request.AuthCode = authCode;
                    request.ClientId = clientId;
                    request.CertificateId = certificateId;

                    var createdAgent = AgentMgmtService.CreateAgentAndGetId(request);
                    if (createdAgent.HasValue)
                    {
                        createdAgentId = createdAgent.Value;
                        request.Id = createdAgentId;
                        result.AgentCreateResult = RMAgentCreateResult.Succeed;
                        result.AgentId = createdAgentId.ToString();
                        return result;
                    }
                    else
                    {
                        result.AgentCreateResult = RMAgentCreateResult.Failed;
                        return result;
                    }
                },
                (request, response) =>
                {
                    if (response.AgentCreateResult == RMAgentCreateResult.Succeed)
                    {
                        request.Id = createdAgentId;
                        request.InstallationCode = installationCode;
                        request.AuthCode = authCode;
                        request.ClientId = clientId;
                        request.CertificateId = certificateId;
                    }

                    return Task.CompletedTask;
                },
                _ => new AgentResultDto
                {
                    AgentCreateResult = RMAgentCreateResult.Failed
                });
        }
        private string GenerateRandomString(int num = 11)
        {
            Thread.Sleep(100);
            string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ!@#$%^&*()abcdefghijklmnopqrstuvwxyz";

            string str = "";
            for (int i = 0; i < num; i++)
            {
                str += chars[SecurityUtils.GetRandomNumber(0, chars.Length)];
            }

            return str;
        }
        private async Task<List<RMCertificateDto>> GetAllCertificatesAsync(bool markDefault = true, bool includeExpired = false)
        {
            var certificates = (await CertificateService.GetAllWithoutBinaryDataAsync(includeExpired)).OrderByDescending(o => o.ValidTo).ToList();
            if (markDefault && certificates.Count > 0)
            {
                var defaultCert = certificates.FirstOrDefault(o => o.Id == GetDefaultCertificateId()) ?? certificates.First();
                defaultCert.IsDefault = true;
            }
            return certificates;
        }
        private Guid GetDefaultCertificateId()
        {
            var certificateId = KeyValueService.Get(KeyNameCollection.DefaultCertificateId, RMNameValueType.DefaultCertificate)?.Value;
            if (!string.IsNullOrEmpty(certificateId) && Guid.TryParse(certificateId, out Guid id))
            {
                return id;
            }

            return Guid.Empty;
        }
        private bool IsValidAgentType([FromBody] RMAgentDto dto)
        {
            return dto.SourceType == SourceType.SharePoint || dto.SourceType == SourceType.FileSystem || (dto.SourceType == (SourceType.SharePoint | SourceType.FileSystem));
        }
        private Task<IList<RMAgentDto>> GetAllAgentsInfoAsync()
        {
            return AgentMgmtService.GetAllAsync();
        }
        private string GetClientId()
        {
            var clientId = KeyValueService.Get(KeyNameCollection.AppManagementClientId, RMNameValueType.AppManagementClientId)?.Value;
            return clientId;
        }
        private async Task<bool> CheckSameAgentNameAsync([FromBody] RMAgentDto dto)
        {
            return (await GetAllAgentsInfoAsync()).Any(o => o.Id != dto.Id && o.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
