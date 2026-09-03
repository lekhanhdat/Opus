using AvePoint.RA.Common;
using AvePoint.RA.Common.Helper;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Multi_Geo.AuditHandler;
using CommonModel.DataModel;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Exchange.WebServices.Data;
using PnP.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Multi_Geo
{
    [Audit]
    public class MultiGeoSettingService : IMultiGeoSettingService
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(MultiGeoSettingService));
        private readonly IRMCache Cache = PlatformWindsorManager.GetService<IRMCache>();
        private readonly IRMFunctionSettingDao RMFunctionSettingDao = PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
        private readonly IRMKeyValueDao RMKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private readonly IMultiGeoSettingDao MultiGeoSettingDao = PlatformWindsorManager.GetService<IMultiGeoSettingDao>();
        private readonly IMultiGeoDataCenterService MultiGeoDataCenterService = PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        private static readonly TimeSpan s_cacheDuration = TimeSpan.FromHours(2);
        public IFSConnectionGroupDao FSConnectionGroupDao => PlatformWindsorManager.GetService<IFSConnectionGroupDao>();
        private const string IS_ENABLE_MULTI_GEO_FEATURE_KEY = "IsEnableMultiGeoFeature";
        private const string MULTI_GEO_MAIN_DC = "MultiGeoMainDC";
        private const string MULTI_GEO_SUPPORTED_DC = "MultiGeoSupportedDC";

        public async Task<bool> IsEnableMultiGeoFeature()
        {
            return await RMFunctionSettingDao.IsEnableMultiGeoFeature(RMKeyValueDao);
        }

        [Audit(Module = AuditModule.MultiGeo, Category = AuditCategory.MultiGeo, Action = AuditAction.EnableMultiGeoFeature ,BeforeHandler = typeof(MultiGeoServiceBeforeAuditHandler) ,AfterHandler = typeof(MultiGeoServiceAfterAuditHandler))]
        public async Task<RAReturnMessage> EnableMultiGeoFeature()
        {
            try
            {
                if (!RMKeyValueDao.IsSupportMultipleGeoFeature())
                    throw new Exception("Current accounts don’t support the multiple GEO feature.");
                if (await RMFunctionSettingDao.IsEnableMultiGeoFeature(RMKeyValueDao))
                {
                    Logger.Info("Multi GEO feature has already been enabled, skip enabling process.");
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Skipped,
                        ErrorMessage = "Multi GEO feature has already been enabled, skip enabling process."
                    };
                }
                await RMFunctionSettingDao.AddOrUpdateSettingInfoAsync(Contract.FunctionSetting.FunctionSettingType.EnableMultiGEOFeature, "True");
                await Cache.SetAsync(IS_ENABLE_MULTI_GEO_FEATURE_KEY, true, s_cacheDuration);
                var mainDC = RMKeyValueDao.GetValueByKey(KeyNameCollection.JPMCMultiGEOMainDC)?.Value ?? string.Empty;
                await Cache.SetAsync(MULTI_GEO_MAIN_DC, mainDC, s_cacheDuration);
                var supportedDCs = await MultiGeoDataCenterService.GetDCsSupported();
                await Cache.SetAsync(MULTI_GEO_SUPPORTED_DC, supportedDCs, s_cacheDuration);
                Logger.Info("Start run sync common data job");
                await MultiGeoDataCenterService.RunMainDCSyncCommonDataJob(Contract.RMWeb.JobRunBy.Control);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Successful,
                };
            }
            catch(Exception e)
            {
                Logger.Error($"Enable Multi GEO Feature have error: {e.Message}");
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = e.Message
                };
            }
        }

        [Audit(Module = AuditModule.MultiGeo, Category = AuditCategory.MultiGeo, Action = AuditAction.SaveMultiGeoIPConfig,BeforeHandler = typeof(MultiGeoServiceBeforeAuditHandler) ,AfterHandler = typeof(MultiGeoServiceAfterAuditHandler))]
        public async Task<RAReturnMessage> SaveMultiGeoSettings(List<MultiGeoSettingInfoDto> multiGeoSettingInfo, bool isIgnoreAudit = false)
        {
            try
            {
                foreach(var setting in multiGeoSettingInfo)
                {
                    if (!ValidIPAddresses(setting.IPAddresses))
                    {
                        throw new Exception(I18NEntity.GetString("RM_GEO_IPAddresses_InValid"));
                    }

                    if (setting.Id == Guid.Empty)
                    {
                        setting.Id = Guid.NewGuid();
                    }
                }

                await MultiGeoSettingDao.AddOrUpdateMultipleGeoSettings(multiGeoSettingInfo.Select(dc => new DB.Model.MultiGeoSettingInfo
                {
                    Id = dc.Id,
                    DataCenter = dc.DCInternalName,
                    IPAddresses = dc.IPAddresses
                }).ToList());

                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Successful,
                };
            }
            catch(Exception e)
            {
                Logger.Error($"Save multi GEO setting have error: {e}");
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = e.Message
                };
            }
        }

        private bool ValidIPAddresses(string IPAddresses)
        {
            if (string.IsNullOrEmpty(IPAddresses)) return true;
            var splitIpAddresses = IPAddresses.Split(",");
            foreach(var ipAddress in splitIpAddresses)
            {
                if (!ValidEachIpAddress(ipAddress))
                {
                    return false;
                }
            }
            return true;
        }

        private bool ValidEachIpAddress(string ipAddress)
        {
            string currentIp = ipAddress;
            if (ipAddress.Contains('/'))
            {
                var range = ipAddress.Substring(ipAddress.LastIndexOf('.') + 1);
                string[] parts = range.Split('/');
                if (parts.Length != 2) return false;
                if (int.TryParse(parts[0], out int pre) && int.TryParse(parts[1], out int post))
                {
                    if (pre < 0 || pre > 255 || post < 0 || post > 255) return false;
                    if (pre > post) return false;
                }
                else return false;
                currentIp = ipAddress.Split('/')[0];
            }
            return IPHelper.ValidIPv4Format(currentIp);
        }

        public async Task<List<MultiGeoSettingInfoDto>> GetAllMultiGeoSetting()
        {
            try
            {
                var result = new List<MultiGeoSettingInfoDto>();
                var dicDCAndIpAddresses = MultiGeoSettingDao.GetDicDCAndIpAddresses();
                var supportedDCs = await MultiGeoDataCenterService.GetDCsSupported();
                var mainDC = MultiGeoDataCenterService.GetMainDC();
                string ipAddress = string.Empty;
                foreach (var dataCenter in supportedDCs)
                {
                    if(dataCenter.DCInternalName.Equals(mainDC, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    ipAddress = string.Empty;
                    dicDCAndIpAddresses.TryGetValue(dataCenter.DCInternalName, out ipAddress);
                    result.Add(new MultiGeoSettingInfoDto
                    {
                        DCInternalName = dataCenter.DCInternalName,
                        DCDisplayName = dataCenter.DCDisplayName,
                        IPAddresses = ipAddress
                    });
                }
                return result;
            }
            catch (Exception e)
            {
                Logger.Error($"Get All Multi Geo setting have errors: {e}");
            }
            return Enumerable.Empty<MultiGeoSettingInfoDto>().ToList();
        }

        public async Task<bool> ValidateLoginIPAsync(string ipAddress, string dataCenter)
        {
            try
            {
                var dicDCAndIpAddresses = MultiGeoSettingDao.GetDicDCAndIpAddresses();
                if (dicDCAndIpAddresses.TryGetValue(dataCenter, out string ipAddresses))
                {
                    if (string.IsNullOrEmpty(ipAddresses)) return true;
                    var splitIpAddresses = ipAddresses.Split(",");
                    foreach (var ip in splitIpAddresses)
                    {
                        Logger.Info($"ClientIP: {ipAddress} and ipConfig: {ip}");
                        if (ValidEachIpAddress(ip) && IPHelper.IsInSameSegment(ipAddress, ip))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    Logger.Info($"No IP address setting for data center {dataCenter}, skip IP validation.");
                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Validate login IP have error: {e}");
            }
            return false;
        }

        public ICollection<AgentInformation>  GetAvailableAgentForMultiGeoRedirect(ICollection<AgentInformation> agents)
        {
            var mainDC = MultiGeoDataCenterService.GetMainDC();
            var reservedAgentIds = FSConnectionGroupDao.LoadAllGroups()
                          .Where(group => !string.IsNullOrEmpty(group.DCInternalName) && !string.Equals(group.DCInternalName, mainDC))
                          .SelectMany(group => group.Agents ?? new List<RMAgent>())
                          .Select(agent => agent.Id)
                          .ToHashSet();

            return agents.Where(item => !reservedAgentIds.Contains(new Guid(item.AgentId))).ToList();
        }
    }
}
