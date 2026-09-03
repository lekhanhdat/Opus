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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Services.RMGeneralSetting.AuditHandler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Common;
using System.Linq;
using System.Text.RegularExpressions;
using AvePoint.RA.Contract.Object;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.GraphApi.App;
using AvePoint.RA.Contract.AAD;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.Common.Portal;
using AvePoint.RA.Contract;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Contract.GranularBackup.Object;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.RADataBroker;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.Configurations;
using System.Globalization;
using RATeams;
using TimeZoneConverter;

namespace AvePoint.RA.Service.Services.RMGeneralSetting
{
    [Audit]
    public class GeneralSettingService : RMServiceBase, IGeneralSettingService
    {
        private static RALogger logger = RALogger.GetInstance(typeof(GeneralSettingService));

        public IGeneralSettingDao GeneralSettingDao => PlatformWindsorManager.GetService<IGeneralSettingDao>();
        private IRMScheduleDao RMScheduleDao => PlatformWindsorManager.GetService<IRMScheduleDao>();
        private IGlobalStorageSettingDao GlobalStorageSettingDao => PlatformWindsorManager.GetService<IGlobalStorageSettingDao>();
        public IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();


        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private static IAccountWrapperService AccountWrapperService => PlatformWindsorManager.GetService<IAccountWrapperService>();
        //private static IRMCache Cache => PlatformWindsorManager.GetService<IRMCache>();

        private ISettingProfilesDao SettingProfileDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();

        public static int SessionTimeout { get; set; }
        public readonly string DEFAULTMASTKEYSECURITYPROFILE = "DefaultMastkeySecurityProfile";

        private readonly string ENVIRONMENT_NAME = "21V China North";

        /// <summary>
        /// 查询 general setting设置
        /// </summary>
        /// <returns>general seting 表单信息 </returns>
        [RACodeReview("Allen Yin")]
        public async Task<GeneralSettingModel> GetGeneralSettingAsync()
        {
            async Task<GeneralSettingModel> GetGeneralSettingAsyncInternal()
            {
                try
                {
                    RMCPGeneralSetting model = null;
                    //TenantLocalValue.LogonGroupId
                    var securityPro = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
                    if (!string.IsNullOrWhiteSpace(TenantLocalValue.LogonGroupId))
                    {
                        logger.Info($"Get general setting from TenantLocalValue.TenantId:[{TenantLocalValue.LogonGroupId}]");
                        model = await GeneralSettingDao.GetGeneralSettingByUserAsync(TenantLocalValue.LogonGroupId);
                    }
                    if (model != null)
                    {
                        var res = new GeneralSettingModel()
                        {
                            GeneralSetingId = model.Id,
                            DataFormatId = model.DataFormat,
                            TimeFormatId = model.TimeFormat,
                            SessionTime = model.SessionTime,
                            TimeZoneId = model.TimeZone,
                            DayLight = model.DayLight,
                            SessionTimeUnitId = model.SessionTimeUnit,
                            isShowDayLight = GeneralSettingConfig.GetTimeZoneInforById(model.TimeZone).SupportsDaylightSavingTime,
                            EmailSenderDefinition = new EmailSenderDefinition
                            {
                                EmailSenderType = EmailSenderType.Default,
                                AppProfileId = string.Empty,
                                EmailSender = null,
                            }
                        };
                        if (securityPro != null)
                        {
                            res.SecurityProfileId = securityPro.SecurityProfileId.ToString();
                            res.SecurityProfileName = securityPro.SecurityProfileName;
                        }
                        else
                        {
                            var securityProfile = await SaveUsingSecurityProfileAsync();
                            res.SecurityProfileId = securityProfile.DefaultSecurityProfileId;
                            res.SecurityProfileName = securityProfile.SecurityProfiles.Where(a => a.Id == securityProfile.DefaultSecurityProfileId).Select(a => a.Name).FirstOrDefault();
                        }
                        if (!string.IsNullOrWhiteSpace(model.EmailSenderDefinition))
                        {
                            res.EmailSenderDefinition = JsonConvert.DeserializeObject<EmailSenderDefinition>(model.EmailSenderDefinition);
                        }

                        return res;
                    }
                    else
                    {
                        //用户从未在界面上保存过setting时
                        return GeneralSettingModel.DefaultSetting;
                    }
                }
                catch (Exception e)
                {
                    logger.Error(string.Format("get general setting fail : {0}", e.ToString()));
                    //throw;
                    return GeneralSettingModel.DefaultSetting;
                }
            } 

            var generalSetting = await RMCacheManager.Cache.TryGetAsync(IRMCache.Keys.GeneralSettingService_GetGeneralSettingAsync, async () =>
            {
                return await GetGeneralSettingAsyncInternal();
            });
            var recordLabelSetting = await SettingProfileDao.LoadByTypeAsync((int)SettingProfilesType.RecordsLabelSetting);
            if (recordLabelSetting != null)
            {
                generalSetting.RecordsLabel = recordLabelSetting.Settings;
            }

            return generalSetting;
        }
        public async Task<SecurityProfileResult> SaveUsingSecurityProfileAsync()
        {
            if (TenantService.IsNewOpusTenant())
            {
                logger.Info("this is NewOpusTenant ,save Using Security Profile");
                var setting = await VerifyAndCreateDefaultSecurityProfileAsync();
                SecurityProfileResult result = new SecurityProfileResult() { SecurityProfiles = new List<SecurityProfileNameAndId>() };
                result.DefaultSecurityProfileId = setting.Item1.ToString();
                result.SecurityProfiles.Add(new SecurityProfileNameAndId() { Id = setting.Item1.ToString(), Name = setting.Item2 });
                return result;
            }
            else
            {
                SecurityProfileResult result = new SecurityProfileResult() { SecurityProfiles = new List<SecurityProfileNameAndId>() };
                var client = new DAOAPIClientV1();
                var profile = client.GetAllSecurityProfile();
                foreach (var item in profile)
                {
                    result.SecurityProfiles.Add(new SecurityProfileNameAndId()
                    {
                        Name = item.Name,
                        Id = item.Guid
                    });
                }
                var usingSecurityProfile = GlobalStorageSettingDao.FindAll().FirstOrDefault();
                if (usingSecurityProfile != null)
                {
                    result.DefaultSecurityProfileId = usingSecurityProfile.SecurityProfileId.ToString();
                }
                else
                {
                    result.DefaultSecurityProfileId = Guid.Empty.ToString();
                    RMCPGlobalStorageSetting dataEncryptionDto = new RMCPGlobalStorageSetting();
                    dataEncryptionDto.StoragePolicyId = Guid.Empty;
                    dataEncryptionDto.ExportLocationId = Guid.Empty;
                    dataEncryptionDto.SecurityProfileId = Guid.Empty;
                    dataEncryptionDto.SecurityProfileName = string.Empty;
                    dataEncryptionDto.UseCompression = true;
                    dataEncryptionDto.UseEncryption = false;
                    dataEncryptionDto.CompressionSpeed = 5;
                    dataEncryptionDto.CompressionMethod = GCommon.Contract.GranularBackup.Object.DataSecurity.CompressionMedia;
                    dataEncryptionDto.EncryptionMethod = GCommon.Contract.GranularBackup.Object.DataSecurity.EncryptionMedia;
                    dataEncryptionDto.Extentions = string.Empty;
                    await GlobalStorageSettingDao.SaveOrUpdateAsync(dataEncryptionDto);
                }
                return result;
            }
        }
        public async Task<bool> CheckEmailSenderDefinition(EmailSenderDefinition definition)
        {

            try
            {
                if (((int)definition.EmailSenderType) < 0 || ((int)definition.EmailSenderType) > 1)
                {
                    return false;
                }

                if (definition.EmailSenderType == EmailSenderType.Default)
                {
                    definition.EmailSender = null;
                    definition.AppProfileId = string.Empty;
                    return true;
                }

                var appProfile = await RMAosApiClient.GetProfileById(definition.AppProfileId);
                var graphAppManager = new RMGraphAppManager(appProfile);
                if(!graphAppManager.HasSendEmailPermission())
                {
                    return false;
                }

                var account = AccountWrapperService.GetAccount(appProfile, definition.EmailSender.UserPrincipalName);
                if(account == null)
                {
                    return false;
                }
                
                return true;
            }
            catch(Exception e)
            {
                logger.Error($"An error occurred while check email sender definition. Error: {e}");
                return false;
            }
        }

        /// <summary>
        /// 更新 general setting 设置
        /// </summary>
        /// <param name="generalSettingModel">新的设置</param>
        /// <returns>更新是否成功</returns>
        [RACodeReview("Allen Yin")]
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.GeneralSettings, Action = AuditAction.ConfigureGeneralSetting, BeforeHandler = typeof(GeneralSettingBeforeAuditHandler), AfterHandler = typeof(GeneralSetingAfterAuditHandler))]
        public async Task<bool> SaveOrUpdateGeneralSettingAsync(GeneralSettingModel generalSettingModel)
        {
            try
            {
                if (!await SaveOrUpdateRecordLabelAsync(generalSettingModel.RecordsLabel, false))
                {
                    return false;
                }

                if (TenantService.IsNewOpusTenant())
                {
                    await VerifyAndCreateDefaultSecurityProfileAsync();
                }
                else if (!string.IsNullOrEmpty(generalSettingModel.SecurityProfileId))
                {
                    var setting = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
                    setting.SecurityProfileId = new Guid(generalSettingModel.SecurityProfileId);
                    setting.SecurityProfileName = generalSettingModel.SecurityProfileName;
                    setting.UseEncryption = !string.IsNullOrEmpty(generalSettingModel.SecurityProfileId) && new Guid(generalSettingModel.SecurityProfileId) != Guid.Empty;
                    GlobalStorageSettingDao.SaveOrUpdateAsync(setting);
                }
                bool isChanageSessionTimeOut = true;
                var tenantId = TenantLocalValue.LogonGroupId;
                var originalGS = await GeneralSettingDao.GetGeneralSettingByUserAsync(tenantId);
                if (originalGS != null)
                {
                    isChanageSessionTimeOut = originalGS.SessionTime != generalSettingModel.SessionTime || originalGS.SessionTimeUnit != generalSettingModel.SessionTimeUnitId;
                }
                if (generalSettingModel.SessionTime <= 0)
                {
                    throw new Exception(string.Format("general seting SessionTime error: {0}", generalSettingModel.SessionTime));
                }
                if (GeneralSettingConfig.GetTimeZoneInforById(generalSettingModel.TimeZoneId) == null)
                {
                    throw new Exception(string.Format("general seting TimeZoneId error: {0}", generalSettingModel.TimeZoneId));
                }
                RMCPGeneralSetting generalSetting = new RMCPGeneralSetting()
                {
                    Id = generalSettingModel.GeneralSetingId,
                    SessionTime = generalSettingModel.SessionTime,
                    SessionTimeUnit = generalSettingModel.SessionTimeUnitId,
                    DataFormat = generalSettingModel.DataFormatId,
                    TimeFormat = generalSettingModel.TimeFormatId,
                    TimeZone = generalSettingModel.TimeZoneId,
                    DayLight = generalSettingModel.DayLight,
                    TenantId = tenantId,
                    EmailSenderDefinition = JsonConvert.SerializeObject(generalSettingModel.EmailSenderDefinition),
                };
                bool result = await GeneralSettingDao.UpdateOrSaveGeneralSettingByUserAsync(generalSetting, tenantId);
              

                if (result && isChanageSessionTimeOut)
                {
                    await PlatformWindsorManager.GetService<ILoginService>().UpdateSessionTimeoutSettingAsync(CaculateTimeByUnit(generalSettingModel.SessionTime, generalSettingModel.SessionTimeUnitId));
                }
                if (result)
                {
                    var environmentName = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME];
                    await ScheduleService.CreateCustomScheduleAsync(true, ScheduleType.UniqueIDSettingSchedule);
                    await ScheduleService.CreateCustomScheduleAsync(true, ScheduleType.TeamsUniqueIDSettingSchedule);
                    await ScheduleService.CreateCustomScheduleAsync(true, ScheduleType.ManualApprovalScheduleTimer);
                    if (!environmentName.Equals(ENVIRONMENT_NAME, StringComparison.OrdinalIgnoreCase))
                    {
                        await ScheduleService.CreateCustomScheduleAsync(true, ScheduleType.EnforceRetention);
                        await ScheduleService.CreateCustomScheduleAsync(true, ScheduleType.EXOEnforceRetention);
                        await ScheduleService.CreateCustomScheduleAsync(true, ScheduleType.OneDriveEnforceRetention);
                        if(TeamsPermissionHelper.HasUpgradeTeamsFeature())
                            await ScheduleService.CreateCustomScheduleAsync(true, ScheduleType.TeamsEnforceRetention);
                    }
                    await ScheduleService.CreateCustomScheduleAsync(true, ScheduleType.PRExplorerTimer);
                    await ScheduleService.CreateCustomScheduleAsync(true, ScheduleType.ConnectorExplorerTimer);
                    await ScheduleService.CreateCustomScheduleAsync(true, ScheduleType.JobNotificationSchedule);
                    await ScheduleService.UpdateDashboardNextRunTimeAsync();
                    await ScheduleService.UpdateManualApprovalEmailScheduleNextRunTimeAsync();
                    var schedule = RMScheduleDao.GetScheduleByType(ScheduleType.CollectionDataSchedule).FirstOrDefault();
                    if (schedule != null)
                    {
                        schedule.TimeZoneId = generalSettingModel.TimeZoneId;
                        await RMScheduleDao.UpdateScheduleAsync(schedule);
                    }
                    await RMCacheManager.Cache.RemoveAsync(IRMCache.Keys.GeneralSettingService_GetGeneralSettingAsync);
				}
                return result;
            }
            catch (Exception e)
            {
                logger.Error(string.Format("general seting update fail: {0}",e.Message));
                throw;
            }
        }
      

        public async Task<string> VerfiyHasMastkeySecurityProfileAsync()
        {
            var encrptionProfiles = SettingProfileDao.LoadAllByType(SettingProfilesType.DataEncrptionProfile);
            foreach (var profile in encrptionProfiles)
            {
                if (profile.Name == DEFAULTMASTKEYSECURITYPROFILE)
                {
                    var encryptionProfile = (DataEncryptionProfile)SerializerHelper.DeserializeByDataContractSerializer(profile.Settings, typeof(DataEncryptionProfile));

                    return encryptionProfile.Guid;
                }

            }
            return string.Empty;
        }

        private async System.Threading.Tasks.Task<string> SaveDefaultMastkeySecurityProfileAsync()
        {
            try
            {
                var id = await VerfiyHasMastkeySecurityProfileAsync();
                if (string.IsNullOrEmpty(id))
                {
                    logger.Info("Start saving the DefaultMastkeySecurityProfile.");

                    DataEncryptionProfile profile = new DataEncryptionProfile();
                    profile.Name = DEFAULTMASTKEYSECURITYPROFILE;
                    profile.AccessControl = 0;
                    profile.AlgorithmType = (int)Cryptography.EncryptionAlgorithm.AES_ENCRYPTION;
                    profile.Guid = Guid.NewGuid().ToString();
                    profile.IsDefault = false;
                    profile.KeyLength = 256;
                    profile.Scope = 0;
                    profile.CurrentProtectionAlgorithm = new ProtectionAlgorithm();
                    profile.CurrentProtectionAlgorithm.Guid = Guid.NewGuid().ToString();
                    profile.CurrentProtectionAlgorithm.AlgorithmType = (int)Cryptography.EncryptionAlgorithm.AES_ENCRYPTION;
                    profile.CurrentProtectionAlgorithm.AosSecurityProfileId = Guid.Empty.ToString();
                    profile.CurrentProtectionAlgorithm.KeyLength = 256;
                    profile.CurrentProtectionAlgorithm.ProtectionKey = string.Empty;
                    profile.CurrentProtectionAlgorithm.ProviderKeyRestore = false;
                    profile.CurrentProtectionAlgorithm.SaveTime = 0;
                    profile.CurrentProtectionAlgorithm.Type = ProtectionAlgorithmType.TenantMasterKeyEncryptionService;



                    SettingProfiles dataEncryptionDto = new SettingProfiles();
                    dataEncryptionDto.Name = profile.Name;
                    dataEncryptionDto.Type = (int)SettingProfilesType.DataEncrptionProfile;
                    dataEncryptionDto.Settings = SerializerHelper.SerializeByDataContractSerializer(profile);
                    dataEncryptionDto.Id = new Guid(profile.Guid);


                    SettingProfileDao.Create(dataEncryptionDto);
                    logger.Info(string.Format("Saved the security profile {0} successfully.", profile.Name));
                    return profile.Guid;
                }
                else
                {
                    logger.Info("DefaultMastkeySecurityProfile already exist");
                    return id;
                }
            }
            catch (Exception e)
            {
                logger.Error($"Saved the DefaultMastkeySecurityProfile failed. {e}");
                throw;
            }
        }
        public async Task EnsureDefaultMastkeySecurityProfileAsync(Guid securityProfileGuid)
        {
            try
            {
                var id = await VerfiyHasMastkeySecurityProfileAsync();
                if (string.IsNullOrEmpty(id))
                {
                    logger.Info("Start saving the DefaultMastkeySecurityProfile.");

                    DataEncryptionProfile profile = new DataEncryptionProfile();
                    profile.Name = DEFAULTMASTKEYSECURITYPROFILE;
                    profile.AccessControl = 0;
                    profile.AlgorithmType = (int)Cryptography.EncryptionAlgorithm.AES_ENCRYPTION;
                    profile.Guid = securityProfileGuid.ToString();
                    profile.IsDefault = false;
                    profile.KeyLength = 256;
                    profile.Scope = 0;
                    profile.CurrentProtectionAlgorithm = new ProtectionAlgorithm();
                    profile.CurrentProtectionAlgorithm.Guid = Guid.NewGuid().ToString();
                    profile.CurrentProtectionAlgorithm.AlgorithmType = (int)Cryptography.EncryptionAlgorithm.AES_ENCRYPTION;
                    profile.CurrentProtectionAlgorithm.AosSecurityProfileId = Guid.Empty.ToString();
                    profile.CurrentProtectionAlgorithm.KeyLength = 256;
                    profile.CurrentProtectionAlgorithm.ProtectionKey = string.Empty;
                    profile.CurrentProtectionAlgorithm.ProviderKeyRestore = false;
                    profile.CurrentProtectionAlgorithm.SaveTime = 0;
                    profile.CurrentProtectionAlgorithm.Type = ProtectionAlgorithmType.TenantMasterKeyEncryptionService;

                    SettingProfiles dataEncryptionDto = new SettingProfiles();
                    dataEncryptionDto.Name = profile.Name;
                    dataEncryptionDto.Type = (int)SettingProfilesType.DataEncrptionProfile;
                    dataEncryptionDto.Settings = SerializerHelper.SerializeByDataContractSerializer(profile);
                    dataEncryptionDto.Id = new Guid(profile.Guid);

                    SettingProfileDao.Create(dataEncryptionDto);
                    logger.Info(string.Format("Saved the security profile in setting profile {0} successfully.", profile.Name));

                    var global = GlobalStorageSettingDao.FindAll().First();
                    global.SecurityProfileId = dataEncryptionDto.Id;
                    global.SecurityProfileName = DEFAULTMASTKEYSECURITYPROFILE;
                    await GlobalStorageSettingDao.UpdateAsync(global);
                    logger.Info(string.Format("Saved the security profile id in global setting {0} successfully.", profile.Name));
                }
                else
                {
                    logger.Info("DefaultMastkeySecurityProfile already exist");
                }
            }
            catch (Exception e)
            {
                logger.Error($"Saved the DefaultMastkeySecurityProfile failed. {e}");
                throw;
            }
        }

        public async Task<Tuple<Guid,string>> VerifyAndCreateDefaultSecurityProfileAsync()
        {
            Guid SecurityProfileId = Guid.Empty;
            string SecurityProfileName = string.Empty;
            var global = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
            if (global != null)
            {
                if (global.SecurityProfileId.Equals(Guid.Empty))
                {
                    var securityId = await SaveDefaultMastkeySecurityProfileAsync();
                    global.SecurityProfileId = new Guid(securityId);
                    global.SecurityProfileName = DEFAULTMASTKEYSECURITYPROFILE;
                    global.UseEncryption = true;
                    await GlobalStorageSettingDao.UpdateAsync(global);
                }
                SecurityProfileId = global.SecurityProfileId;
                SecurityProfileName = global.SecurityProfileName;
            }
            else
            {
                logger.Warn("VerifyAndCreateDefaultSecurityProfileAsync:GlobalSettingInfo is null");
            }
            return new Tuple<Guid, string>(SecurityProfileId, SecurityProfileName);
        }

     

        public bool DeleteCurrentUserGeneralSetting() {
            return GeneralSettingDao.DeleteGeneralSettingByUser(TenantLocalValue.LogonGroupId);
        }

        /// <summary>
        /// get items of Audit collection
        /// </summary>
        /// <param name="gsm">general setting设置</param>
        /// <returns>get items of Audit collection</returns>
        public Dictionary<AuditItems, string> GetAuditItems(GeneralSettingModel gsm)
        {
            try
            {
                Dictionary<AuditItems, string> result = new Dictionary<AuditItems, string>();
                result.Add(AuditItems.RecordsLabel, gsm.RecordsLabel);
                result.Add(AuditItems.SessionTimeOut, string.Format("{0} {1}", gsm.SessionTime, GetI18NSessionTimeUnit(gsm.SessionTimeUnitId)));
                string timezoneMsg;
                if(gsm.isShowDayLight&&gsm.DayLight){
                    timezoneMsg = string.Format("{0} <br> {1}", GeneralSettingConfig.GetTimeZoneInforById(gsm.TimeZoneId).DisplayName, "RM_Audit_WithDaylight ");
                }else{
                    timezoneMsg = string.Format("{0} <br> {1}", GeneralSettingConfig.GetTimeZoneInforById(gsm.TimeZoneId).DisplayName, "RM_Audit_WithoutDaylight ");
                }
                result.Add(AuditItems.TimeZone, timezoneMsg);
                result.Add(AuditItems.TimeFormat, GeneralSettingConfig.TimeFormats[(TimeFormat)Enum.Parse(typeof(TimeFormat), Enum.GetName(typeof(TimeFormat), gsm.TimeFormatId), true)]);
                result.Add(AuditItems.DataFormat, GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), gsm.DataFormatId), true)]);
                return result;
            }
            catch (Exception e)
            {
                logger.Error(string.Format("get items of Audit collection fail : {0} ",e.Message));
                throw;
            }
        }

        private string GetI18NSessionTimeUnit(int sessionTimeUnitId)
        {
            string sessionTimeUnit = string.Empty;
            switch (sessionTimeUnitId)
            {
                case (int)SessionTimeUnit.hours:
                    sessionTimeUnit = "RM_GS_SessionTime_Hour ";
                    break;
                case (int)SessionTimeUnit.minutes:
                    sessionTimeUnit = "RM_GS_SessionTime_Minute ";
                    break;
                default:
                    break;
            }
            return sessionTimeUnit;
        }

        /// <summary>
        /// 获得时间DateTime格式
        /// </summary>
        /// <returns>格式字符串</returns>
        public async Task<string> GetDateTimeFormatAsync() {
            try
            {
                string timeFormat = null;
                string dateFormat = null;
                RMCPGeneralSetting rcg = null;
                if (!string.IsNullOrEmpty(TenantLocalValue.LogonGroupId))
                {
                    rcg = await GeneralSettingDao.GetGeneralSettingByUserAsync(TenantLocalValue.LogonGroupId);
                }
                if (rcg != null)
                {
                    timeFormat = GeneralSettingConfig.TimeFormats[(TimeFormat)Enum.Parse(typeof(TimeFormat), Enum.GetName(typeof(TimeFormat), rcg.TimeFormat), true)];
                    dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), rcg.DataFormat), true)];
                }
                else
                {
                    //如果没有设置，那么取默认值
                    timeFormat = GeneralSettingConfig.TimeFormats[(TimeFormat)Enum.Parse(typeof(TimeFormat), Enum.GetName(typeof(TimeFormat), GeneralSettingModel.DefaultSetting.TimeFormatId), true)];
                    dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), GeneralSettingModel.DefaultSetting.DataFormatId), true)];
                }
                return string.Format("{0} {1}", dateFormat, timeFormat);
            }
            catch (Exception e)
            {
                logger.Error("GetDateTimeFormat failed:{0}", e.Message);
                throw;
            }
        }
        public string GetDateTimeFormat(GeneralSettingModel gls)
        {
            try
            { 
                string timeFormat = null;
                string dateFormat = null; 
                if (gls != null)
                {
                    timeFormat = GeneralSettingConfig.TimeFormats[(TimeFormat)Enum.Parse(typeof(TimeFormat), Enum.GetName(typeof(TimeFormat), gls.TimeFormatId), true)];
                    dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), gls.DataFormatId), true)];
                }
                else
                {
                    //如果没有设置，那么取默认值
                    timeFormat = GeneralSettingConfig.TimeFormats[(TimeFormat)Enum.Parse(typeof(TimeFormat), Enum.GetName(typeof(TimeFormat), GeneralSettingModel.DefaultSetting.TimeFormatId), true)];
                    dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), GeneralSettingModel.DefaultSetting.DataFormatId), true)];
                }
                return string.Format("{0} {1}", dateFormat, timeFormat);
            }
            catch (Exception e)
            {
                logger.Error("GetDateTimeFormat failed:{0}", e.Message);
                throw;
            }
        }

        /// <summary>
        /// 获得日期格式
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetDateFormatAsync()
        {
            try
            {
                RMCPGeneralSetting rcg = null;
                if (!string.IsNullOrEmpty(TenantLocalValue.LogonGroupId))
                {
                    rcg = await GeneralSettingDao.GetGeneralSettingByUserAsync(TenantLocalValue.LogonGroupId);
                }
                return GetDateFormat(rcg);
            }
            catch (Exception e)
            {
                logger.Error("GetDateFormat failed: {0}" , e.Message);
                throw;
            }
        
        }
        private string GetDateFormat(RMCPGeneralSetting rcg)
        {
            string dateFormat = null;
            if (rcg != null)
            {
                var key = (DateFormat)Enum.Parse(
                    typeof(DateFormat), 
                    Enum.GetName(typeof(DateFormat), rcg.DataFormat), 
                    true);
                dateFormat = GeneralSettingConfig.DateFormats[key];
            }
            else
            {
                var key = (DateFormat)Enum.Parse(
                    typeof(DateFormat),
                    Enum.GetName(typeof(DateFormat), GeneralSettingModel.DefaultSetting.DataFormatId),
                    true);
                //如果没有设置，那么取默认值
                dateFormat = GeneralSettingConfig.DateFormats[key];
            }
            return dateFormat;
        }
        private string GetTimeFormat(RMCPGeneralSetting rcg)
        {
            string timeFormat = null;
            if (rcg != null)
            {
                var key = (TimeFormat)Enum.Parse(
                        typeof(TimeFormat),
                        Enum.GetName(typeof(TimeFormat), rcg.TimeFormat), 
                        true);
                timeFormat = GeneralSettingConfig.TimeFormats[key];
            }
            else
            {
                var key = (TimeFormat)Enum.Parse(
                        typeof(TimeFormat),
                        Enum.GetName(typeof(TimeFormat), GeneralSettingModel.DefaultSetting.TimeFormatId), 
                        true);
                //如果没有设置，那么取默认值
                timeFormat = GeneralSettingConfig.TimeFormats[key];
            }
            return timeFormat;
        }

        #region 查询时间设置
        /// <summary>
        /// 根据ID获得general setting 中time setting设置，用于前台时间控件
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>时间控件需要的设置</returns>
        public async Task<TimeSettingModel> GetTimeSettingModelAsync(string tenantId)
        {
            TimeSettingModel result = new TimeSettingModel();
            try
            {
                result.TimeZoneInfo = GeneralSettingConfig.TimeZoneInfoes;
                RMCPGeneralSetting gl = await GeneralSettingDao.GetGeneralSettingByUserAsync(tenantId);
                if (gl != null)
                {
                    var zoneInfo = GeneralSettingConfig.GetTimeZoneInforById(gl.TimeZone);
                    //if(zoneInfo == null)
                    //{
                    //    logger.Warn("Cannot get timezone " + gl.TimeZone + "\r\nList Zones:" +
                    //        String.Join("\r\n", GeneralSettingConfig.TimeZones.Select(t => t.Id + "|" + t.StandardName + "|" + t.DisplayName)));
                    //    zoneInfo = TimeZoneInfo.Local;
                    //}
                    result.TimeZoneId = zoneInfo.Id;
                    result.offsetHours = zoneInfo.BaseUtcOffset.Hours;
                    result.offsetMinutes = zoneInfo.BaseUtcOffset.Minutes;
                    result.isSupportDayLight = zoneInfo.SupportsDaylightSavingTime;
                    //如果支持夏令时, 默认选中
                    result.isSetDayLight = gl.DayLight;
                    result.sessionTime = CaculateTimeByUnit(gl.SessionTime, gl.SessionTimeUnit);
                }
                else
                {
                    result.TimeZoneId = TimeZoneInfo.Local.Id;
                    result.offsetHours = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).Hours;
                    result.offsetMinutes = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).Minutes;
                    result.isSupportDayLight = TimeZoneInfo.Local.SupportsDaylightSavingTime;
                    //如果支持夏令时, 默认选中
                    result.isSetDayLight = TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now);
                    result.sessionTime = 30;
                }
                result.DateFormat = GetDateFormat(gl);
                result.TimeFormat = GetTimeFormat(gl);

                return result;
            }
            catch (Exception e)
            {
                logger.Error($"get TimeSeting of GeneralSeting fail. {e.Message}");
                throw;
            }
        }
        #endregion

        #region 将Utc转设定的timezone时间
        /// <summary>
        /// 将UTC tick转成对应时区时间
        /// </summary>
        /// <param name="tiks">Utc ticks</param>
        /// <returns>TimeModel包含格式化后字符串(该)和转化后时间</returns>
        public async Task<TimeModel> ConvertTiksToDateTimeAsync(long tiks,bool isIncludeTimeZoneInFormat, bool isControlPlus = false)
        {            
             GeneralSettingModel gls = await GetGeneralSettingAsync();
            if (isControlPlus) gls.TimeZoneId = TenantLocalValue.TimezoneId;
            return ConvertTiksToDateTime(gls, tiks, isIncludeTimeZoneInFormat);  
        }
        public TimeModel ConvertTiksToDateTime(GeneralSettingModel gls, long tiks, bool isIncludeTimeZoneInFormat)
        { 
            string timeFormat = GeneralSettingConfig.TimeFormats[(TimeFormat)Enum.Parse(typeof(TimeFormat), Enum.GetName(typeof(TimeFormat), gls.TimeFormatId), true)];
            string dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), gls.DataFormatId), true)];
            var tiz = GeneralSettingConfig.GetTimeZoneInforById(gls.TimeZoneId);
            var systemTiz = GeneralSettingConfig.FindSystemTimeZoneById(gls.TimeZoneId);
            DateTime currentDate = DateTimeUtil.ConvertTimeFromUtc(tiks, systemTiz, !gls.DayLight);
            string formaTime = DateTimeUtil.ConvertDateTimeToString(currentDate, string.Format("{0} {1} ", dateFormat, timeFormat));
            string formaDate = DateTimeUtil.ConvertDateTimeToString(currentDate, dateFormat);
            string simplifyFormatTime = formaTime;
            if (isIncludeTimeZoneInFormat)
            {
                formaTime = string.Format("{0} {1}", formaTime, DateTimeUtil.GetAllStaticTimeZones().Where(x => x.Id == tiz.Id).FirstOrDefault()?.DisplayName/*tiz.DisplayName*/);
                Regex reg = new Regex(@"\(.*?\)");
                var matchResult =  reg.Match(tiz.DisplayName);
                simplifyFormatTime = string.Format("{0} {1}", simplifyFormatTime, matchResult.Value);
                //"(UTC hh:mm)"
            }
            TimeModel model = new TimeModel()
            {
                FormaTime = formaTime,
                FormaDate= formaDate,
                DataTime = currentDate,
                SimplifyFormatTime = simplifyFormatTime
            };
            return model;
        }
 
        public TimeModel ConvertTiksToDateTime(GeneralSettingModel gls, long tiks, bool isIncludeTimeZoneInFormat, int timeZoneId, bool isDaylight, string dateFormat)
        {
            string timeFormat = GeneralSettingConfig.TimeFormats[(TimeFormat)Enum.Parse(typeof(TimeFormat), Enum.GetName(typeof(TimeFormat), gls.TimeFormatId), true)];
            var realTimeZoneId = DateTimeUtil.AllTimeZones[timeZoneId];
            var tiz = GeneralSettingConfig.GetTimeZoneInforById(realTimeZoneId ?? gls.TimeZoneId);
            var systemTiz = GeneralSettingConfig.FindSystemTimeZoneById(realTimeZoneId ?? gls.TimeZoneId);
            DateTime currentDate = DateTimeUtil.ConvertTimeFromUtc(tiks, systemTiz, !isDaylight);
            string formaTime = DateTimeUtil.ConvertDateTimeToString(currentDate, string.Format("{0} {1} ", dateFormat, timeFormat));
            string simplifyFormatTime = formaTime;
            if (isIncludeTimeZoneInFormat)
            {
                formaTime = string.Format("{0} {1}", formaTime, DateTimeUtil.GetAllStaticTimeZones().Where(x => x.Id == tiz.Id).FirstOrDefault()?.DisplayName/*tiz.DisplayName*/);
                Regex reg = new Regex(@"\(.*?\)");
                var matchResult = reg.Match(tiz.DisplayName);
                simplifyFormatTime = string.Format("{0} {1}", simplifyFormatTime, matchResult.Value);
                //"(UTC hh:mm)"
            }
            TimeModel model = new TimeModel()
            {
                FormaTime = formaTime,
                DataTime = currentDate,
                SimplifyFormatTime = simplifyFormatTime
            };
            return model;
        }

        public long ConvertTiksToTimeZoneTicks(int timeZoneId, bool isDaylight, long tiks)
        {
            string targetTimeZoneId = null;
            if (DateTimeUtil.AllTimeZones != null && timeZoneId >= 0 && timeZoneId < DateTimeUtil.AllTimeZones.Count)
            {
                targetTimeZoneId = DateTimeUtil.AllTimeZones[timeZoneId];
            }
            else
            {
                logger.Warn($"ConvertTiksToTimeZoneTicks: invalid timeZoneId {timeZoneId}, fallback to tenant timezone.");
            }

            if (string.IsNullOrEmpty(targetTimeZoneId))
            {
                targetTimeZoneId = TimeZoneInfo.Utc.Id;
            }

            var systemTimeZone = GeneralSettingConfig.FindSystemTimeZoneById(targetTimeZoneId) ?? TimeZoneInfo.Utc;
            DateTime converted = DateTimeUtil.ConvertTimeFromUtc(tiks, systemTimeZone, !isDaylight);
            return converted.Ticks;
        }
        //发邮件只用年月日。这个是属于发邮件的方法。
        public string ConvertTiksToDateNoTime(GeneralSettingModel gls, long tiks)
        {
            string dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), gls.DataFormatId), true)];
            var systemTiz = GeneralSettingConfig.FindSystemTimeZoneById(gls.TimeZoneId);
            DateTime currentDate = DateTimeUtil.ConvertTimeFromUtc(tiks, systemTiz, !gls.DayLight);
            string formaTime = DateTimeUtil.ConvertDateTimeToString(currentDate, string.Format("{0}", dateFormat));
            return formaTime;
        }

        //不按照GeneralSetting时区显示，直接返回UTC的时间
        public TimeModel ConvertTiksToUTCDateTime(GeneralSettingModel gls, long tiks)
        {
            string timeFormat = GeneralSettingConfig.TimeFormats[(TimeFormat)Enum.Parse(typeof(TimeFormat), Enum.GetName(typeof(TimeFormat), gls.TimeFormatId), true)];
            string dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), gls.DataFormatId), true)];            
            DateTime currentDate = new DateTime(tiks, DateTimeKind.Utc);
            string formaTime = DateTimeUtil.ConvertDateTimeToString(currentDate, string.Format("{0} {1} ", dateFormat, timeFormat));
            string simplifyFormatTime = formaTime;
           
            TimeModel model = new TimeModel()
            {
                FormaTime = formaTime,
                DataTime = currentDate,
                SimplifyFormatTime = simplifyFormatTime
            };
            return model;
        }

        public TimeModel ConvertTiksToUTCDateTime(GeneralSettingModel gls, long tiks, bool isIncludeTimeZoneInFormat)
        {
            string timeFormat = GeneralSettingConfig.TimeFormats[(TimeFormat)Enum.Parse(typeof(TimeFormat), Enum.GetName(typeof(TimeFormat), gls.TimeFormatId), true)];
            string dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), gls.DataFormatId), true)];
            DateTime currentDate = new DateTime(tiks, DateTimeKind.Utc);
            var tiz = GeneralSettingConfig.GetTimeZoneInforById(gls.TimeZoneId);
            string formaTime = DateTimeUtil.ConvertDateTimeToString(currentDate, string.Format("{0} {1} ", dateFormat, timeFormat));
            string simplifyFormatTime = formaTime;
            if (isIncludeTimeZoneInFormat)
            {
                formaTime = string.Format("{0} {1}", formaTime, DateTimeUtil.GetAllStaticTimeZones().Where(x => x.Id == tiz.Id).FirstOrDefault()?.DisplayName/*tiz.DisplayName*/);
                Regex reg = new Regex(@"\(.*?\)");
                var matchResult = reg.Match(tiz.DisplayName);
                simplifyFormatTime = string.Format("{0} {1}", simplifyFormatTime, matchResult.Value);
                //"(UTC hh:mm)"
            }
            TimeModel model = new TimeModel()
            {
                FormaTime = formaTime,
                DataTime = currentDate,
                SimplifyFormatTime = simplifyFormatTime
            };
            return model;
        }

        public async Task<TimeModel> ConverTiksToDateTimeAsync(string timeZoneId, long tiks) {
            GeneralSettingModel gls = await GetGeneralSettingAsync();
            return ConverTiksToDateTime(gls, timeZoneId, tiks);
        }
        public TimeModel ConverTiksToDateTime(GeneralSettingModel gls, string timeZoneId, long tiks)
        { 
            string timeFormat = GeneralSettingConfig.TimeFormats[(TimeFormat)Enum.Parse(typeof(TimeFormat), Enum.GetName(typeof(TimeFormat), gls.TimeFormatId), true)];
            string dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), gls.DataFormatId), true)];
            var tiz = GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId) ?? GeneralSettingConfig.FindSystemTimeZoneById(gls.TimeZoneId);
            DateTime currentDate = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(tiks, DateTimeKind.Utc), tiz);
            TimeModel model = new TimeModel()
            {
                FormaTime = DateTimeUtil.ConvertDateTimeToString(currentDate, string.Format("{0} {1}", dateFormat, timeFormat)),
                DataTime = currentDate
            };
            return model;
        }
        #endregion

        #region 将时间转化成UTC
        /// <summary>
        /// 将时间转化成UTC
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public async Task<DateTime> ConvertDateTimeToUtcAsync(DateTime dt)
        {
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
            TimeZoneInfo sourceTimeZone = await GetTimeZoneInforAsync();
            return TimeZoneInfo.ConvertTimeToUtc(dt, sourceTimeZone);
        }
        public async Task<DateTime> ConvertDateTimeToUtcAsync(DateTime dt,string timeZoneId)
        {
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
            var realTimeZoneId = DateTimeUtil.AllTimeZones[Convert.ToInt32(timeZoneId)];
            var timeZone = GeneralSettingConfig.FindSystemTimeZoneById(realTimeZoneId);
            if (string.IsNullOrEmpty(timeZoneId))
                timeZone = await GetTimeZoneInforAsync();
            return TimeZoneInfo.ConvertTimeToUtc(dt, timeZone);
        }
        private async Task<TimeZoneInfo> GetTimeZoneInforAsync()
        {
            GeneralSettingModel gl = await GetGeneralSettingAsync();
            return GeneralSettingConfig.FindSystemTimeZoneById(gl.TimeZoneId);
        }

        public async Task<DateTime> ConvertDateTimeToUtcAsync(string dateTimeStr, GeneralSettingModel gls)
        {
            if(string.IsNullOrEmpty(dateTimeStr))
            {
                return DateTime.MinValue;
            }

            if (gls == null)
            {
                gls = await GetGeneralSettingAsync();
            }

            if(dateTimeStr.IndexOf('(') > 0)
            {
                dateTimeStr = dateTimeStr[..dateTimeStr.IndexOf('(')].TrimEnd();
            }

            string timeFormat = GeneralSettingConfig.TimeFormats[(TimeFormat)Enum.Parse(typeof(TimeFormat), Enum.GetName(typeof(TimeFormat), gls.TimeFormatId), true)];
            string dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), gls.DataFormatId), true)];

            var format = new DateTimeFormatInfo { ShortDatePattern = dateFormat, LongTimePattern = timeFormat };
            var dateTimeOffset = DateTimeOffset.ParseExact(dateTimeStr, format.ShortDatePattern + " " + format.LongTimePattern, format);
            var timeZone = GeneralSettingConfig.FindSystemTimeZoneById(gls.TimeZoneId);
            var convertedDateTimeOffset = TimeZoneInfo.ConvertTime(dateTimeOffset, timeZone);
            return convertedDateTimeOffset.UtcDateTime;
        }
        #endregion

        /// <summary>
        /// 将时间单位转成分钟
        /// </summary>
        /// <param name="sessionTime">时间</param>
        /// <param name="sessionTimeUnit">单位</param>
        /// <returns></returns>
        public static int CaculateTimeByUnit(int sessionTime, int sessionTimeUnit)
        {
            int session;
            try
            {
                if (sessionTimeUnit == (int)SessionTimeUnit.hours)
                {
                    session = sessionTime * 60;
                }
                else if (sessionTimeUnit == (int)SessionTimeUnit.minutes)
                {
                    session = sessionTime;
                }
                else
                {
                    session = SessionTimeout;
                }
            }
            catch (Exception e)
            {
                logger.Error("CaculateTimeByUnit fail: {0}" , e.Message);
                throw;
            }
            
            return session;
        }

        public async Task<string> ConvertToUTCDateTimeAsync(string startTime, string format = null)
        {
            var gls = await GetGeneralSettingAsync();
            DateTime dt = DateTime.Parse(startTime);
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
            dt = DateTimeUtil.ConvertTimeToUtcDate(dt, gls);
            var returnFormat = JSDateTimeFormat.DEFAULT_TIME_FORMAT;
            if (!string.IsNullOrEmpty(format))
            {
                returnFormat = format;
            }
            return dt.ToString(returnFormat);
        }

        public string ConvertToUTCDateTime(string startTime, GeneralSettingModel gls, string format = null)
        {
            DateTime dt = DateTime.Parse(startTime);
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
            dt = DateTimeUtil.ConvertTimeToUtcDate(dt, gls);
            var returnFormat = JSDateTimeFormat.DEFAULT_TIME_FORMAT;
            if (!string.IsNullOrEmpty(format))
            {
                returnFormat = format;
            }
            return dt.ToString(returnFormat);
        }

        public async Task<string> ConvertFromUTCDateTimeAsync(string startTime, string format = null)
        {
            var gls = await GetGeneralSettingAsync();
            return DateTimeUtil.ConvertFromUTCDateTime(startTime, gls, format);
        }

        public string ConvertBrowserTimeZoneToWindows(string timezoneId)
        {
            if (TZConvert.TryIanaToWindows(timezoneId, out var windowsTz))
            {
                return TimeZoneInfo.FindSystemTimeZoneById(windowsTz).Id;
            }
            return timezoneId;
        }

        public async Task<bool> SaveOrUpdateRecordLabelAsync(string recordsLabel, bool isRequired = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(recordsLabel))
                {
                    if (isRequired) return false;
                    await SettingProfileDao.DeleteProfileByType((int)SettingProfilesType.RecordsLabelSetting);
                    return true;
                }
                await SettingProfileDao.UpdateAsync(new SettingProfileDto()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = SettingProfilesType.RecordsLabelSetting.ToString(),
                    Type = (int)SettingProfilesType.RecordsLabelSetting,
                    Settings = recordsLabel
                });
                return true;
            }
            catch (Exception e)
            {
                logger.Error($"SaveOrUpdateRecordLabelAsync failed: {e}");
                return false;
            }
        }
    }
}
