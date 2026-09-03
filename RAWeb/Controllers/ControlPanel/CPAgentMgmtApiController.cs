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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Hybrid.Contract.Object;
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Cryptography;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Certficate;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Extension;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.Service.Services.AccountManager;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using AvePoint.Wrapper.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Web;


namespace AvePoint.RA.Web.Controllers.ControlPanel
{
    [RMApiAuthorize(RMPermissionMasks.FSAdmin | RMPermissionMasks.SPOnPremAdmin, RMDiscoveryFileSystemPermissionMask.AccessAll, PermissionJoinType.Any, PermissionJoinType.Any, preferred: false)]
    public class CPAgentMgmtApiController : BaseApiController
    {
        /// <summary>
        /// default agent certification valid duration in year
        /// </summary>
        private const int DefaultCertificationDurationInYear = 2;
        private RALogger logger = RALogger.GetInstance(typeof(CPAgentMgmtApiController));
        private IAgentMgmtService _AgentMgmtService;
        private IAgentMgmtService AgentMgmtService => PlatformWindsorManager.GetService(ref _AgentMgmtService);
        private ICertificateService _CertificateService;
        private ICertificateService CertificateService => PlatformWindsorManager.GetService(ref _CertificateService);
        private IKeyValueService _KeyValueService;
        private IKeyValueService KeyValueService => PlatformWindsorManager.GetService(ref _KeyValueService);
        private IRMKeyValueDao _RMKeyValueDao;
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService(ref _RMKeyValueDao);
        private IMultiGeoSettingService MultiGeoSettingService => PlatformWindsorManager.GetService<IMultiGeoSettingService>();
        private IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();


        /// <summary>
        /// save client id
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public Task<string> SaveClientId([FromBody] string clientId)
        {
            return RouteMultiGeoApiActionAsync(
                clientId,
                MultiGeoOperationType.SaveClientId,
                async request =>
                {
                    var result = await AgentMgmtService.SaveClientIdAsync(request);
                    return result ? "0" : "1";
                },
                _ => "-1");
        }

        [HttpPost]
        public string GetClientId()
        {
            var clientId = KeyValueService.Get(KeyNameCollection.AppManagementClientId, RMNameValueType.AppManagementClientId)?.Value;
            return clientId;
        }

        [HttpPost]
        public string GetAppRegisterURL()
        {
            return AveUrlUtility.CombineUrl(RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_URL], "#/administration/app-registrations");
        }

        [HttpPost]
        public string GetAgentInstallerURL()
        {
            return $"{RMGlobalConfiguration.AppConfig[RMAppSettingKey.AGENT_INSTALLER_URL]}?v={RMGlobalConfiguration.EnvSetting.ProductVersion}";
        }

        [HttpPost]
        public string GetAgentLatestVersion()
        {
            return $"{RMGlobalConfiguration.AppConfig[RMAppSettingKey.AGENT_LATEST_VERSION]}";
        }

        [HttpPost]
        public Task<bool> SetupNotify()
        {
            return KeyValueService.SaveAsync(
                    new RMNameValueDto
                    {
                        Name = TenantLocalValue.LogonUserId,
                        Value = bool.TrueString,
                        Type = RMNameValueType.AppManagementDoNotShowNotify
                    });
        }

        [HttpPost]
        public bool IsSetupNotify()
        {
            return KeyValueService.Get(TenantLocalValue.LogonUserId, RMNameValueType.AppManagementDoNotShowNotify) != null;
        }
        #region certificate
        //[HttpPost]
        //public string ImportCertificate()
        //{
        //    try
        //    {
        //        HttpRequest request = HttpContext.Current.Request;
        //        HttpPostedFile file = request.Files["recordsFileUp"];
        //        Logger.Info("tm import file,file name :{0}", file.FileName);
        //        string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
        //        DateTime dt = DateTime.Now;
        //        string fileName = "ImportRecord" + dt.Ticks.ToString() + "." + extension.ToLower();
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.Message;
        //    }
        //    return "ok";
        //}

        /// <summary>
        /// get all valid certificates without content
        /// </summary>
        /// <param name="includeExpired">indicate if including the expired certificate in the results</param>
        /// <returns></returns>
        [HttpPost]
        public Task<List<RMCertificateDto>> GetAllCertificatesInfo([FromBody] bool includeExpired)
        {
            return GetAllCertificatesAsync(includeExpired: includeExpired);
        }

        [HttpPost]
        public async Task<bool> SetAsDefaultCertificate([FromBody] Guid certificateId)
        {
            var certificate = GetCertificate(certificateId);
            if (certificate == null || certificate.IsExpired)
            {
                logger.Warn($"Can't get certificate info with id {certificateId} or the certificate is expired.");
                return false;
            }
            return await SetDefaultCertificateAsync(certificateId);
        }

        private Task<bool> SetDefaultCertificateAsync(Guid certificateId)
        {
            return CertificateService.SetAsDefaultCertificateAsync(certificateId);
            //return KeyValueService.Save(new RMNameValueDto
            //{
            //    Name = KeyNameCollection.DefaultCertificateId,
            //    Type = RMNameValueType.DefaultCertificate,
            //    Value = certificateId.ToString().ToLower()
            //});
        }

        [HttpPost]
        public Task<string> CreateCertificate([FromBody] bool setAsDefault)
        {
            RMCertificateDto certificate = null;

            return RouteMultiGeoApiActionAsync(
                new RMCertificateCreateRequest
                {
                    SetAsDefault = setAsDefault,
                },
                MultiGeoOperationType.CreateCertificate,
                async request =>
                {
                    var dt = DateTime.UtcNow.ToString("yyyyMMdd");
                    certificate = new RMCertificateDto
                    {
                        Id = Guid.NewGuid(),
                        Name = $"AvePoint_Cloud_Records_App_Certificate_{dt}.pfx",
                        ValidFrom = DateTimeOffset.UtcNow.UtcDateTime,
                        ValidTo = DateTimeOffset.UtcNow.AddYears(DefaultCertificationDurationInYear).UtcDateTime,
                        PWD = GenerateRandomString()
                    };

                    var newId = CertificateService.Create(certificate);
                    if (Guid.Empty != newId && request.SetAsDefault)
                    {
                        certificate.Id = newId;
                        await SetDefaultCertificateAsync(newId);
                    }

                    if (Guid.Empty != newId)
                    {
                        certificate.Id = newId;
                    }

                    return newId != Guid.Empty ? "0" : "1";
                },
                (request, response) =>
                {
                    if (response == "0")
                    {
                        request.Certificate = certificate;
                    }

                    return Task.CompletedTask;
                },
                _ => "-1");
        }

        [HttpPost]
        public async Task<bool> DeleteCertificate([FromBody] Guid id)
        {
            if ((await GetAllAgentsByCertificateIdAsync(id)).Any()) return false;
            var isDefault = id == GetDefaultCertificateId();
            if (isDefault) KeyValueService.Delete($"{KeyNameCollection.DefaultCertificateId}{RMNameValueDto.Seprator}{RMNameValueType.DefaultCertificate}");

            return CertificateService.Delete(id);
        }


        /// <summary>
        /// check if can update the default certificate to active/activewithexception agents.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> CanUpdateCertificate2Agents()
        {
            var certificateId = GetDefaultCertificateId();
            if (certificateId == Guid.Empty) return false;

            var cert = GetCertificate(certificateId);
            if (cert == null || cert.IsExpired) return false;

            return await CertificateService.NeedUpdateCertificate2AgentsAsync(certificateId);
        }

        /// <summary>
        /// update the default certificate to active/active with exception agents.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<CertificateUpdateResult> UpdateCertificate2Agents()
        {
            var result = new CertificateUpdateResult { ResultCode = CertificateUpdateResultEnum.AllFailed };
            try
            {
                var certificateId = GetDefaultCertificateId();
                if (certificateId == Guid.Empty) return new CertificateUpdateResult { ResultCode = CertificateUpdateResultEnum.NoDefaultCertificate };

                var cert = GetCertificate(certificateId);
                if (cert == null || cert.IsExpired) return new CertificateUpdateResult { ResultCode = CertificateUpdateResultEnum.CertificateExpired };

                var updateResult = await CertificateService.UpdateCertificate2AgentsAsync(certificateId);
                if (updateResult == null) return new CertificateUpdateResult { ResultCode = CertificateUpdateResultEnum.NoActiveAgent };

                await AssembleCertificateUpdateResultAsync(result, updateResult);
                await UpdateCertificateId2Agents(certificateId, updateResult);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while UpdateCertificate2Agents, error: {e.ToString()}");
            }
            return result;
        }

        private RMCertificateDto GetCertificate(Guid certificateId, bool includeBinaryData = false)
        {
            return CertificateService.Get(certificateId, includeBinaryData);
        }
        private async System.Threading.Tasks.Task AssembleCertificateUpdateResultAsync(CertificateUpdateResult result, List<AgentCertificateUpdateResult> updateResult)
        {
            result.Agents = updateResult;
            var agentIds = updateResult.Select(o => o.AgentId).ToList();
            var allAgents = await GetAllAgentsInfoAsync();

            updateResult.ForEach(r => r.AgentName = allAgents.FirstOrDefault(o => o.Id == r.AgentId)?.Name);

            var updateStatus = updateResult.Select(o => o.Result).Distinct();
            result.ResultCode = updateStatus.Count() > 1 ? CertificateUpdateResultEnum.HasFailed : updateStatus.First() == AgentCertificateUpdateResultEnum.Succeed ? CertificateUpdateResultEnum.AllSucceed : CertificateUpdateResultEnum.AllFailed;
        }

        private async System.Threading.Tasks.Task UpdateCertificateId2Agents(Guid certificateId, List<AgentCertificateUpdateResult> updateResult)
        {
            var agentIds = updateResult.Where(o => o.Result == AgentCertificateUpdateResultEnum.Succeed).Select(o => o.AgentId).ToList();
            if (agentIds.Count > 0) await AgentMgmtService.UpdateCertificateIdAsync(agentIds, certificateId);
        }

        private Guid CreateNewCertificate()
        {
            var dt = DateTime.UtcNow.ToString("yyyyMMdd");
            var name = $"AvePoint_Cloud_Records_App_Certificate_{dt}.pfx";
            var dto = new RMCertificateDto
            {
                Id = Guid.NewGuid(),
                Name = name,
                ValidFrom = DateTimeOffset.UtcNow.UtcDateTime,
                ValidTo = DateTimeOffset.UtcNow.AddYears(DefaultCertificationDurationInYear).UtcDateTime,
                PWD = GenerateRandomString()
            };

            return CertificateService.Create(dto);

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
        /// <summary>
        /// get all valid certificates info without binary content, ordered by ValidateTo field in descending order
        /// </summary>
        /// <param name="markDefault">indicate if mark which one is the default certificate in the results</param>
        /// <param name="includeExpired">indicate if including the expired certificate in the results</param>
        /// <returns></returns>
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
        #endregion

        //[HttpPost]
        //public string GetCertificatePulicKeyString([FromBody]Guid id)
        //{
        //    return CertificateService.GetCertificatePulicKeyString(id);
        //}

        [HttpPost]
        public async Task<string> ReGenerateInstallationCode([FromBody] Guid agentId)
        {
            var newCode = GenerateRandomString();
            var updated = await AgentMgmtService.UpdateInstallationCodeAsync(agentId, newCode);

            return updated ? newCode : null;
        }

        [HttpPost]
        public string GetInstallationCode([FromBody] Guid agentId)
        {
            var dto = AgentMgmtService.Get(agentId);
            return dto?.InstallationCode;
        }

        private string GenerateRandomString(int num = 11)
        {
            Thread.Sleep(100);
            string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ!@#$%^&*()abcdefghijklmnopqrstuvwxyz";

            string str = "";
            for (int i = 0; i < num; i++)
            {
                str += chars[SecurityUtils.GetRandomNumber(0,chars.Length)];
            }

            return str;
        }

        [HttpPost]
        public async Task<RMAgentCreateResult> CreateAgent([FromBody] RMAgentDto dto)
        {
            if (dto == null || !IsValidAgentType(dto))
            {
                return RMAgentCreateResult.Failed;
            }

            if (await CheckSameAgentNameAsync(dto))
            {
                return RMAgentCreateResult.SameNameExist;
            }

            var certs = await GetAllCertificatesAsync(markDefault: true);
            var installationCode = string.Empty;
            var authCode = string.Empty;
            var clientId = GetClientId();
            var certificateId = Guid.Empty;
            var createdAgentId = Guid.Empty;

            if (string.IsNullOrEmpty(clientId))
            {
                return RMAgentCreateResult.NoClientId;
            }

            if (certs.Count == 0)
            {
                return RMAgentCreateResult.NoCertificate;
            }

            installationCode = GenerateRandomString();
            authCode = GenerateRandomString();
            certificateId = certs.First(o => o.IsDefault).Id;

            return await RouteMultiGeoApiActionAsync(
                dto,
                MultiGeoOperationType.CreateAgent,
                async request =>
                {
                    request.InstallationCode = installationCode;
                    request.AuthCode = authCode;
                    request.ClientId = clientId;
                    request.CertificateId = certificateId;

                    var createdAgent = AgentMgmtService.CreateAgentAndGetId(request);
                    if (createdAgent.HasValue)
                    {
                        createdAgentId = createdAgent.Value;
                        request.Id = createdAgentId;
                        return RMAgentCreateResult.Succeed;
                    }

                    return RMAgentCreateResult.Failed;
                },
                (request, response) =>
                {
                    if (response == RMAgentCreateResult.Succeed)
                    {
                        request.Id = createdAgentId;
                        request.InstallationCode = installationCode;
                        request.AuthCode = authCode;
                        request.ClientId = clientId;
                        request.CertificateId = certificateId;
                    }

                    return Task.CompletedTask;
                },
                _ => RMAgentCreateResult.UpdateCommonDataFailed);
        }

        [HttpPost]
        public async Task<string> UpdateAgent([FromBody] RMAgentDto dto)
        {
            if (dto == null || !IsValidAgentType(dto) || await CheckSameAgentNameAsync(dto))
            {
                return "1";
            }

            return await RouteMultiGeoApiActionAsync(
                dto,
                MultiGeoOperationType.UpdateAgent,
                async request =>
                {
                    return await AgentMgmtService.UpdateAgentAsync(request);
                },
                _ => "-1");
        }

        [HttpPost]
        public Task<bool> UpdateAgentResourceUsage([FromBody] RMAgentDto dto)
        {
            return AgentMgmtService.UpdateAgentResourceUsageAsync(dto);
        }

        [HttpPost]
        public Task<string> DeleteAgent([FromBody] Guid id)
        {
            return RouteMultiGeoApiActionAsync(
                id,
                MultiGeoOperationType.DeleteAgent,
                async request =>
                {
                    var result = await AgentMgmtService.DeleteAsync(request);
                    return result ? "0" : "1";
                },
                _ => "-1");
        }

        [HttpPost]
        public Task<string> DisableAgent([FromBody] Guid id)
        {
            return RouteMultiGeoApiActionAsync(
                id,
                MultiGeoOperationType.DisableAgent,
                async request =>
                {
                    var result = await AgentMgmtService.DisableAsync(request);
                    return result ? "0" : "1";
                },
                _ => "-1");
        }

        [HttpPost]
        public Task<string> EnableAgent([FromBody] Guid id)
        {
            return RouteMultiGeoApiActionAsync(
                id,
                MultiGeoOperationType.EnableAgent,
                async request =>
                {
                    var result = await AgentMgmtService.EnableAsync(request);
                    return result ? "0" : "1";
                },
                _ => "-1");
        }

        [HttpPost]
        public async Task<IList<RMAgentDto>> GetAllAgents()
        {
            var agents = await GetAllAgentsInfoAsync();
            var groups = agents.GroupBy(o => o.CertificateId);
            foreach (var group in groups)
            {
                var certificate = GetCertificate(group.Key);
                if (certificate == null) continue;
                foreach (var agent in group)
                {
                    agent.CertificateThumbprint = certificate.Thumbprint;
                    agent.CertificateStatus = certificate.Status;
                }
            }
            return agents.OrderBy(o => o.Name).ToList();
        }

        [HttpPost]
        public async Task<IList<RMAgentDto>> GetAllHasFileSystemSouceTypeAgents()
        {
            return (await GetAllAgents()).Where(item => item.SourceType.HasFlag(SourceType.FileSystem)).ToList();
        }

        [HttpPost]
        public async Task<IList<NameAndIdDto>> GetAllAgentsByCertificate([FromBody] Guid certificateId)
        {
            var agents = await GetAllAgentsByCertificateIdAsync(certificateId);
            return agents?.Count() > 0 ? agents.Select(o => new NameAndIdDto { Id = o.Id.ToString(), Name = o.Name }).OrderBy(o => o.Name).ToList() : null;
        }

        [HttpPost]
        public bool CheckAgentIsUnderGroup([FromBody] Guid id)
        {
            return AgentMgmtService.CheckAgentIsUnderGroup(id);
        }

        /// <summary>
        /// get all agents which use the certificate
        /// </summary>
        /// <param name="certificateId"></param>
        /// <returns></returns>
        private async Task<IEnumerable<RMAgentDto>> GetAllAgentsByCertificateIdAsync(Guid certificateId)
        {
            return (await GetAllAgentsInfoAsync()).Where(o => o.CertificateId == certificateId);
        }

        private Task<IList<RMAgentDto>> GetAllAgentsInfoAsync()
        {
            return AgentMgmtService.GetAllAsync();
        }

        private async Task<bool> CheckSameAgentNameAsync([FromBody] RMAgentDto dto)
        {
            return (await GetAllAgentsInfoAsync()).Any(o => o.Id != dto.Id && o.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsValidAgentType([FromBody] RMAgentDto dto)
        {
            return dto.SourceType == SourceType.SharePoint || dto.SourceType == SourceType.FileSystem || (dto.SourceType == (SourceType.SharePoint | SourceType.FileSystem));
        }

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
        public async Task<IActionResult> DownloadAgentConfig([FromForm] string agentId)
        {
            try
            {
                var agent = AgentMgmtService.Get(Guid.Parse(agentId), true);
                var conf = AgentMgmtService.DownloadConfig(agent);

                if (conf == null)
                {
                    return NoContent();
                }

                AssemblePublicKey(conf);

                await PopulatePublicUrlsAsync(agent, conf);

                var confJson = JsonConvert.SerializeObject(conf);
                var confBytes = System.Text.Encoding.UTF8.GetBytes(confJson);
                Stream stream = new MemoryStream(AESEncriptionHelper.Encrypt(confBytes, agent.GetAESEncryptKey()));

                return new FileStreamResult(stream, "application/octet-stream")
                {
                    FileDownloadName = $"CloudAgentConfigurationFile_{DateTime.UtcNow.ToFileTime()}"
                };
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while downloading agent config file, error : {e}");
                throw;
            }
        }

        /// <summary>
        /// download the default certificate which only include public key
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        //[Microsoft.AspNetCore.Mvc.TypeFilter(typeof(ValidateAntiForgeryTokenFilterAttribute))]
        ////[FileDownloadFilter]
        public async Task<IActionResult> DownloadCert()
        {
            var certs = await GetAllCertificatesAsync(markDefault: true);
            var certId = certs.Count == 0 ? CreateNewCertificate() : certs.First(o => o.IsDefault).Id;

            var dto = AgentMgmtService.DownloadCert(certId);

            var cert = new X509Certificate2(dto.BinaryContent, dto.PWD);

            var certBytes = cert.Export(X509ContentType.Cert);

            Stream stream = new MemoryStream(certBytes);
            stream.Position = 0;
            return File(stream, GetContentType(dto.Name.Replace(".pfx", "") + ".cer"), dto.Name.Replace(".pfx", "") + ".cer");
        }

        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }

        /// <summary>
        /// download the certificate which only include public key
        /// </summary>
        /// <param name="certId"></param>
        /// <returns></returns>
        [HttpPost]
        //[Microsoft.AspNetCore.Mvc.TypeFilter(typeof(ValidateAntiForgeryTokenFilterAttribute))]
        public IActionResult DownloadCertById([FromForm] string certId)
        {
            try
            {
                var certGuidId = Guid.Parse(certId);
                var dto = AgentMgmtService.DownloadCert(certGuidId);
                var cert = new X509Certificate2(dto.BinaryContent, dto.PWD);
                var certBytes = cert.Export(X509ContentType.Cert);
                Stream stream = new MemoryStream(certBytes);
                return new FileStreamResult(stream, "application/octet-stream")
                {
                    FileDownloadName = dto.Name.Replace(".pfx", "") + ".cer"
                };
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while download agent cert. Error: {e}");
                throw;
            }
        }

        [RMApiAuthorize(RMPermissionMasks.ContentRepositoyAdmin)]
        [HttpPost]
        public async Task<Contract.Object.RMAgentUpgradeResult> UpgradeCloudAgent([FromBody] RMAgentUpgradeDto dto)
        {
            var key = RMKeyValueDao.GetValueByKey("ENABLE_JPMC_FILE_SYSTEM_FEATURE");
            bool.TryParse(key?.Value, out bool result);
            if (result == false)
            {
                return Contract.Object.RMAgentUpgradeResult.Failed;
            }
            return (await AgentMgmtService.UpgradeCloudAgentAsync(dto)).Item2;
        }

        [HttpPost]
        public Task<AgentQueryResult> QueryAgents([FromBody] AgentQueryParams queryDto)
        {
            queryDto.DataCenterName = RMSSOHelper.CurrentDCName;
            return AgentMgmtService.QueryAgentsAsync(queryDto);
        }

        [HttpPost]
        public Task<AgentQueryResult> FilterAgentsByDC([FromBody] AgentQueryParams queryDto)
        {
            return AgentMgmtService.FilterAgentsByDCAsync(queryDto);
        }
    }
}