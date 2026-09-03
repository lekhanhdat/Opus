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
using AvePoint.GCommon.Utility.TransientFault;
using AvePoint.Hybrid.Contract.Object;
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos.Util;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Cryptography;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Certficate;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Extension;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Cert;
using AvePoint.RA.Service.Services.ControlPanel.AuditHandler;
using CommonModel.DataModel;
using HybirdProxy.Implement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ControlPanel
{
    [Audit]
    public class CertificateService : RMServiceBase, ICertificateService
    {
        private RALogger logger = RALogger.GetInstance(typeof(CertificateService));
        private IRMCertificateDao  RMCertificateDao => PlatformWindsorManager.GetService<IRMCertificateDao>();
        public ISignalRService SignalRService => PlatformWindsorManager.GetService<ISignalRService>();
        public IRMAgentDao RMAgentDao => PlatformWindsorManager.GetService<IRMAgentDao>();
        public IKeyValueService KeyValueService => PlatformWindsorManager.GetService<IKeyValueService>();

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AgentManagement, Action = AuditAction.CreateCertificate, BeforeHandler = typeof(AppManagementBeforeAuditHandler), AfterHandler = typeof(AppManagementAfterAuditHandler))]
        public Guid Create(RMCertificateDto dto)
        {
            try
            {
                //var ecdsa = ECDsa.Create();// generate asymmectric key pair
                //var req = new CertificateRequest("cn=avepoint", ecdsa, HashAlgorithmName.SHA256);
                //var cert = req.CreateSelfSigned(dto.ValidFrom.Value, dto.ValidTo.Value);
                var cert = CreateCert.CreateSelfSignedCertificate(dto.ValidTo.Value);

                var certBytes = cert.Export(X509ContentType.Pfx, dto.PWD);

                var encryptedPWD = AESEncriptionHelper.Encrypt(Encoding.UTF8.GetBytes(dto.PWD), ReadEncyptKey());
                var brinaryContent = AESEncriptionHelper.Encrypt(certBytes, ReadEncyptKey());
                var entity = new RMCertificate
                {
                    Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                    EncryptedPWD = Convert.ToBase64String(encryptedPWD),
                    Name = dto.Name,
                    Thumbprint = cert.Thumbprint,
                    ValidFrom = dto.ValidFrom.Value,
                    ValidTo = dto.ValidTo.Value,
                    BinaryContent = brinaryContent,
                };

                RMCertificateDao.Create(entity);
                dto.Thumbprint = entity.Thumbprint;
                dto.BinaryContent = brinaryContent;
                return entity.Id;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while create self signed certificate. error: {e.ToString()}");
            }

            return Guid.Empty;

        }

        public async Task<Guid> CreateReplicaCertificateAsync(RMCertificateDto dto)
        {
            try
            {
                var cert = CreateCert.CreateSelfSignedCertificate(dto.ValidTo.Value);
                var certBytes = cert.Export(X509ContentType.Pfx, dto.PWD);

                var encryptedPWD = AESEncriptionHelper.Encrypt(Encoding.UTF8.GetBytes(dto.PWD), ReadEncyptKey());
                var entity = new RMCertificate
                {
                    Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                    EncryptedPWD = Convert.ToBase64String(encryptedPWD),
                    Name = dto.Name,
                    Thumbprint = dto.Thumbprint,
                    ValidFrom = dto.ValidFrom.Value,
                    ValidTo = dto.ValidTo.Value,
                    BinaryContent = dto.BinaryContent,
                };

                await RMCertificateDao.CreateReplicaCertificateAsync(entity);

                return entity.Id;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while create replica self signed certificate. error: {e.ToString()}");
            }

            return Guid.Empty;
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AgentManagement, Action = AuditAction.SetAsDefaultCertificate, BeforeHandler = typeof(AppManagementBeforeAuditHandler), AfterHandler = typeof(AppManagementAfterAuditHandler))]
        public Task<bool> SetAsDefaultCertificateAsync(Guid certificateId)
        {
            return KeyValueService.SaveAsync(new RMNameValueDto
            {
                Name = KeyNameCollection.DefaultCertificateId,
                Type = RMNameValueType.DefaultCertificate,
                Value = certificateId.ToString().ToLower()
            });
        }
        public RMCertificateDto Get(Guid id, bool includeBinaryData = true)
        {
            try
            {
                var entity = RMCertificateDao.Find(o => o.Id == id);
                if (entity != null)
                {
                    return new RMCertificateDto
                    {
                        Id = entity.Id,
                        Name = entity.Name,
                        Thumbprint = entity.Thumbprint,
                        ValidFrom = entity.ValidFrom,
                        ValidTo = entity.ValidTo,
                        PWD = includeBinaryData? Encoding.UTF8.GetString(AESEncriptionHelper.Decrypt(Convert.FromBase64String(entity.EncryptedPWD), ReadEncyptKey())): null,
                        BinaryContent = includeBinaryData? AESEncriptionHelper.Decrypt(entity.BinaryContent, ReadEncyptKey()) : null
                    };
                }

                logger.Warn($"Not found with id : {id}");
            }
            catch(Exception e)
            {
                logger.Error($"An error occurred while get certificate. error: {e.ToString()}");

            }

            return null;
        }

        public async Task<IList<RMCertificateDto>> GetAllWithoutBinaryDataAsync(bool includeExpired = false)
        {
            try
            {
                var dt = DateTime.UtcNow;
                var entities = !includeExpired ? await RMCertificateDao.FindListWithColumnsAsync(o => new {o.Id, o.Name, o.Thumbprint, o.ValidFrom, o.ValidTo}, o => o.ValidTo > dt)
                    : await RMCertificateDao.FindListWithColumnsAsync(o => new { o.Id, o.Name, o.Thumbprint, o.ValidFrom, o.ValidTo });

                if (entities != null)
                {
                    return entities.Select(entity => new RMCertificateDto
                    {
                        Id = entity.Id,
                        Name = entity.Name,
                        Thumbprint = entity.Thumbprint,
                        ValidFrom = entity.ValidFrom,
                        ValidTo = entity.ValidTo
                    }).ToList();
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while get certificate. error: {e.ToString()}");

            }

            return new List<RMCertificateDto>();
        }

        public string GetCertificatePulicKeyString(Guid id)
        {
            try
            {
                var dto = Get(id);
                if (dto != null)
                {
                    var cert = new X509Certificate2(dto.BinaryContent, dto.PWD);
                    return Convert.ToBase64String(cert.Export(X509ContentType.Cert));
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while get certificate public key string. error: {e.ToString()}");
            }

            return string.Empty;
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AgentManagement, Action = AuditAction.DeleteCertificate, BeforeHandler = typeof(AppManagementBeforeAuditHandler), AfterHandler = typeof(AppManagementAfterAuditHandler))]
        public bool Delete(Guid id)
        {
            try
            {
                return RMCertificateDao.DeleteByKey(id);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while delete certificate with id {id}. error: {e.ToString()}");

            }
            return false;
        }

        public async Task<bool> NeedUpdateCertificate2AgentsAsync(Guid certificateId)
        {
            return (await GetNeedCertificateUpdatedAgentsAsync(certificateId)).Count > 0;
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AgentManagement, Action = AuditAction.UpdateCertificate2Agents, BeforeHandler = typeof(AppManagementBeforeAuditHandler), AfterHandler = typeof(AppManagementAfterAuditHandler))]
        public async Task<List<AgentCertificateUpdateResult>> UpdateCertificate2AgentsAsync(Guid certificateId)
        {

            var agents = await GetNeedCertificateUpdatedAgentsAsync(certificateId);
            if (agents.Count == 0) return null;

            var proxy = GetAgentProxy();
            var result = agents.Select(o => new AgentCertificateUpdateResult { AgentId = new Guid(o.AgentId), AgentName = o.AgentName, Result = AgentCertificateUpdateResultEnum.Failed }).ToList();

            var certificate = Get(certificateId);
            AveTenantTasks.RunAndWaitTasks(agents, new System.Threading.CancellationTokenSource(), agent =>
            {
                var methodArgs = GetUpdateArgs(certificate, new Guid(agent.AgentId));
                var oneResult = System.Threading.Tasks.Task.Run(() => proxy.InvokeOneAgentAysnc<SAgentCertificateUpdateExecute, AgentCertificateUpdateArgs, AgentCertificateUpdateResult>(agent, new SAgentCertificateUpdateExecute() { MethodArgs = methodArgs })).Result;
                result.RemoveAll(o => o.AgentId == new Guid(agent.AgentId));
                result.Add(oneResult);
            });

            return result;
        }

        private AgentCertificateUpdateArgs GetUpdateArgs(RMCertificateDto cert, Guid agentId)
        {
            var agent = RMAgentDao.Find(o => o.Id == agentId);
            var conf = new AgentConfigurtion
            {
                Id = agentId.ToString(),
                CustomerId = TenantLocalValue.LogonGroupId,
                CertificateContent = Convert.ToBase64String(cert.BinaryContent),
                CertificatePWD = cert.PWD,
            };
            var confJson = JsonConvert.SerializeObject(conf);

            var confBytes = System.Text.Encoding.UTF8.GetBytes(confJson);
            var encryptBytes = AESEncriptionHelper.Encrypt(confBytes, new RMAgentDto { InstallationCode = agent.InstallationCode}.GetAESEncryptKey());

            var args = new AgentCertificateUpdateArgs() { AgentId = agentId, AgentName = agent.Name, AgentConfigurtionContent = Convert.ToBase64String(encryptBytes) };
            return args;
        }
        /// <summary>
        /// get agents need to update the certificate
        /// </summary>
        /// <param name="certificateId">certificate id</param>
        /// <returns></returns>
        private async Task<List<AgentInformation>> GetNeedCertificateUpdatedAgentsAsync(Guid certificateId)
        {
            var agents = (await SignalRService.GetAgentsAsync(TenantLocalValue.LogonGroupId)).ToList();
            var agentIds = (await RMAgentDao.FindListAsync(o => o.CertificateId != certificateId && (o.Status == ServiceStatus.Active || o.Status == ServiceStatus.ActiveException)))
                .Select(o => o.Id.ToString());
            agents.RemoveAll(o => !agentIds.Contains(o.AgentId));
            return agents;
        }

        private AgentProxy GetAgentProxy()
        {
            var retryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(12, TimeSpan.FromSeconds(10)));
            var proxy = retryPolicy.ExecuteAction(() => RACommonUtility.RASignalRAgentProxy.GetProxy());
            proxy.ConfigureProxy(config =>
            {
                config.InvokeTimeout = 60;
            });
            logger.Info("Finish to get proxy.");
            return proxy;
        }

        public string ReadEncyptKey() 
        {
            try
            {
                logger.Info("Starting to read encryption key.");

                var encryptedKey = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENCRYPTION_FSCERTIFICATE_KEY];

                string key = RMGlobalConfiguration.EnvSetting.IsDevEnvironment
                    ? encryptedKey 
                    : CipherEncryptionUtil.CipherDecrypt(encryptedKey); 

                logger.Info("Finished reading encryption key.");

                return key;
            }
            catch (Exception e)
            {
                logger.Error($"ReadEncyptKey Error  {e}");
                return string.Empty;
            }
           
        }
    }
}
