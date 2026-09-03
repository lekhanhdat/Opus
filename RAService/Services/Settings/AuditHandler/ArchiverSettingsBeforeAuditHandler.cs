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
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Server.EndUserRestoreSetting;
using AvePoint.GCommon.Contract.Server.StubSetting;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.RMSharePointSettings.AuditHandler;
using Google.Apis.Storage.v1;
using RAArchiverCommon.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StubSettingDto = AvePoint.GCommon.Contract.Server.StubSetting.StubSettingDto;

namespace AvePoint.RA.Service.Services.Settings.AuditHandler
{
    public class ArchiverSettingsBeforeAuditHandler : IBeforeAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(ArchiverSettingsBeforeAuditHandler));
        private IRMMiscProfileDao StubSettingDao => PlatformWindsorManager.GetService<IRMMiscProfileDao>();
        private IRMStorageDeviceInfoDao StorageDeviceDao => PlatformWindsorManager.GetService<IRMStorageDeviceInfoDao>();
        private ISettingProfilesDao SettingProfileDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private IGlobalSettingService GlobalSettingService => PlatformWindsorManager.GetService<IGlobalSettingService>();
        private IRMRestoreSiteMappingDao RMRestoreSiteMappingDao => PlatformWindsorManager.GetService<IRMRestoreSiteMappingDao>();
        private IKeyValueService KeyValueService => PlatformWindsorManager.GetService<IKeyValueService>();

        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            RMAuditInfo info = new RMAuditInfo();
            info.Module = (AuditModule)model;
            info.Category = (AuditCategory)category;
            info.Action = (AuditAction)action;
            try
            {
                if (action == (int)AuditAction.StubSettingCreate)
                {
                    StubSettingDto stub = (StubSettingDto)args[0];
                    info.Object = stub.Name;
                }
                else if (action == (int)AuditAction.StubSettingUpdate)
                {
                    var stub = (StubSettingDto)args[0];

                    if (stub?.Id == null)
                    {
                        logger.Warn("The stub setting id is null, cannot find the existing record in database.");
                        return info;
                    }

                    var profile = StubSettingDao.Load(stub.Id);
                    if (profile == null)
                    {
                        logger.Warn("Cannot find the stub setting with id {0} in database, maybe it has been hard deleted.", stub.Id);
                        return info;
                    }

                    stub = MiscProfileConvert.ConvertRMMiscProfileToStubSettingDto(profile);
                    if (stub?.IsRemoved == true)
                    {
                        logger.Warn("The stub setting with id {0} has been removed, cannot get the existing record in database.", stub.Id);
                        return info;
                    }

                    info.Object = stub.Name;
                    info.ModifyContent ??= [];

                    AuditHelper.SaveOldAuditItem(info, "RM_AR_CP_Stub_ColName_Name", stub.Name);

                    var stubTypeI18Nstr = (LeaveStubType)stub.StubType switch
                    {
                        LeaveStubType.Aspx => "RM_AR_CP_Stub_Type_Aspx",
                        LeaveStubType.Txt => "RM_AR_CP_Stub_Type_Txt",
                        LeaveStubType.Html => "RM_AR_CP_Stub_Type_Html",
                        LeaveStubType.Link => "RM_AR_CP_Stub_Type_RestoreLink",
                        _ => "Unknown Stub Type"
                    };

                    AuditHelper.SaveOldAuditItem(info, "RM_AR_CP_Stub_Panel_StubType", stubTypeI18Nstr);
                    AuditHelper.SaveOldAuditItem(info, RMConstants.STUBCONTENT, stub.StubContent);
                    AuditHelper.SaveOldAuditItem(info, "RM_AR_CP_Stub_Panel_ConfigStubRetention", stub.IsEnabledRetention ? "RM_JS_Common_Yes" : "RM_JS_Common_No");
                    if (stub.IsEnabledRetention)
                    {
                        var retentionPeriod = stub.RetentionValue + " " + stub.RetentionUnit switch
                        {
                            DateUnit.Day => "RM_JS_RDM_CreateRule_Unit_Days",
                            DateUnit.Month => "RM_JS_RDM_CreateRule_Unit_Months",
                            DateUnit.Week => "RM_JS_ScheduleSetting_Weeks",
                            DateUnit.Year => "RM_JS_RDM_CreateRule_Unit_Years",
                            _ => ""
                        };
                        AuditHelper.SaveOldAuditItem(info, RMConstants.STUBRETENTIONPERIOD, retentionPeriod + " ");
                    }

                    AuditHelper.SaveOldAuditItem(info, AccountUtility.IsSupportRecordLabel() ? "RM_Audit_Stub_RecordsLabel" : "RM_AR_CP_Stub_Panel_Declare", stub.IsDeclareStubAsRecords ? "RM_JS_Common_Yes" : "RM_JS_Common_No");
                }
                else if (action == (int)AuditAction.StubSettingDelete)
                {
                    List<string> ids = (List<string>)args[0];
                    var stubNames = (await StubSettingDao.FindListAsync(r => ids.Contains(r.Id.ToString()))).Select(r => r.Name).ToList();
                    info.Object = String.Join(";", stubNames);
                }
                else if (action == (int)AuditAction.StorageDeviceCreate)
                {
                    var tempStorage = (StorageDeviceDto)args[0];
                    StorageDeviceDto storageDevice = new StorageDeviceDto();
                    if (tempStorage.Id != null)
                    {
                        info.Action = AuditAction.StorageDeviceUpdate;
                        storageDevice = StorageDeviceService.GetStorageDeviceById(tempStorage.Id);
                    }
                    else
                    {
                        return info;
                    }
                    info.Object = storageDevice.Name;
                    info.ModifyContent ??= [];

                    AuditHelper.SaveOldAuditItem(info, "RM_AR_CP_GSS_Name", storageDevice.Name);
                    AuditHelper.SaveOldAuditItem(info, "RM_AR_CP_GSS_Description", storageDevice.Description);
                    AuditHelper.SaveOldAuditItem(info, "Gui.Common_Storage Type", null);

                    switch ((StorageDeviceType)storageDevice.Type)
                    {
                        case StorageDeviceType.CloudAmazon:
                            AuditHelper.ReSaveOldAuditItem(info, "Gui.Common_Storage Type", "MediaStorage_Amazon_Amazon_S3");
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_Amazon_Bucket_Name", RAStorageUtil.GetStorageConfigValue(storageDevice, "bucketname"));
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_Amazon_Access_Key_ID", RAStorageUtil.GetStorageConfigValue(storageDevice, "name"));
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_Amazon_Secret_Access_Key", RAStorageUtil.SKP);
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_Amazon_Storage_Region", RAStorageUtil.GetI18NRegion(RAStorageUtil.GetStorageConfigValue(storageDevice, "region")));
                            break;

                        case StorageDeviceType.S3Compatible:
                            AuditHelper.ReSaveOldAuditItem(info, "Gui.Common_Storage Type", "MediaStorage_S3Compatible_Compatible_Amazon_S3");
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_S3Compatible_Bucket_Name", RAStorageUtil.GetStorageConfigValue(storageDevice, "bucketname"));
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_S3Compatible_Access_Key_ID", RAStorageUtil.GetStorageConfigValue(storageDevice, "name"));
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_S3Compatible_Secret_Access_Key", RAStorageUtil.SKP);
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_S3Compatible_Endpoint", RAStorageUtil.GetStorageConfigValue(storageDevice, "endpoint"));
                            break;

                        case StorageDeviceType.Dropbox:
                            AuditHelper.ReSaveOldAuditItem(info, "Gui.Common_Storage Type", "MediaStorage_Dropbox_Dropbox");
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_Dropbox_Root_folder", RAStorageUtil.GetStorageConfigValue(storageDevice, "containername"));
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_Dropbox_TokenSecret", RAStorageUtil.SKP);
                            break;

                        case StorageDeviceType.FTP:
                            AuditHelper.ReSaveOldAuditItem(info, "Gui.Common_Storage Type", "MediaStorage_FTP_FTP");
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_FTP_Host", RAStorageUtil.GetStorageConfigValue(storageDevice, "host"));
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_FTP_Port", RAStorageUtil.GetStorageConfigValue(storageDevice, "port"));
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_FTP_Root_Folder", RAStorageUtil.GetStorageConfigValue(storageDevice, "ftprootfolder"));
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_FTP_Username", RAStorageUtil.GetStorageConfigValue(storageDevice, "name"));
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_FTP_Password", RAStorageUtil.SKP);
                            break;

                        case StorageDeviceType.Google:
                            AuditHelper.ReSaveOldAuditItem(info, "Gui.Common_Storage Type", "MediaStorage_Google");
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_Google_ClientEmail", RAStorageUtil.GetStorageConfigValue(storageDevice, "name"));
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_Google_PrivateID", RAStorageUtil.SKP);
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_Google_ProjectID", RAStorageUtil.GetStorageConfigValue(storageDevice, "accesspoint"));
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_Google_BucketName", RAStorageUtil.GetStorageConfigValue(storageDevice, "containername"));
                            break;

                        case StorageDeviceType.CloudAzure:
                            AuditHelper.ReSaveOldAuditItem(info, "Gui.Common_Storage Type", "MediaStorage_Azure_Windows_Azure_Storage");
                            var accessPoint = RAStorageUtil.GetStorageConfigValue(storageDevice, "accesspoint");
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_Azure_Access_Point", string.IsNullOrEmpty(accessPoint) ? "https://blob.core.windows.net" : accessPoint);
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_Azure_Container_Name", RAStorageUtil.GetStorageConfigValue(storageDevice, "containername"));
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_Azure_Account_Name", RAStorageUtil.GetStorageConfigValue(storageDevice, "name"));
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_Azure_Account_Key", RAStorageUtil.SKP);
                            break;

                        case StorageDeviceType.SFTP:
                            AuditHelper.ReSaveOldAuditItem(info, "Gui.Common_Storage Type", "MediaStorage_SFTP_SFTP");
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_SFTP_Host", RAStorageUtil.GetStorageConfigValue(storageDevice, "host"));
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_SFTP_Port", RAStorageUtil.GetStorageConfigValue(storageDevice, "port"));
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_SFTP_Root_Folder", RAStorageUtil.GetStorageConfigValue(storageDevice, "sftprootfolder"));
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_SFTP_Username", RAStorageUtil.GetStorageConfigValue(storageDevice, "name"));
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_SFTP_Password", RAStorageUtil.SKP);
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_SFTP_PrivateKeyFile", RAStorageUtil.GetStorageConfigValue(storageDevice, "privatekeyfile"));
                            AuditHelper.SaveOldAuditItem(info, "MediaStorage_SFTP_PrivateKeyPassword", RAStorageUtil.SKP);
                            break;

                        default:
                            logger.Warn("This storage type may not be supported yet. Storage type: {0}", (StorageDeviceType)storageDevice.Type);
                            break;
                    }

                    AuditHelper.SaveOldAuditItem(info, "Gui.Common_Advanced", RAStorageUtil.GetStorageConfigValue(storageDevice, "advanced").ToBoolean(false) ? "RM_JS_Common_Yes" : "RM_JS_Common_No");
                    AuditHelper.SaveOldAuditItem(info, "Gui.Common_5514307E-E936-44C9-811D-7D1DDA6667A4", RAStorageUtil.GetStorageConfigValue(storageDevice, "extendedparameters"));

                    if (storageDevice.ArchiveRetentionRules != null && storageDevice.ArchiveRetentionRules.Count > 0)
                    {
                        foreach (var retentionRule in storageDevice.ArchiveRetentionRules)
                        {
                            if (retentionRule.SetupDataRetention)
                            {
                                string retentionBy = retentionRule.RetentionDataTimeType == KeepDateType.ModifiedTime ? "RM_AR_CP_GSS_Retention_ByModifiedTime" : "RM_AR_CP_GSS_Retention_ByArchivedTime";
                                AuditItem retentionAudit = new AuditItem()
                                {
                                    TargetSetting = retentionBy,
                                    OldValue = I18NEntity.GetString("Gui.Common_Keep the last") + " " + retentionRule.KeepValue + " " + retentionRule.ArchiveDateUnit switch
                                    {
                                        DateUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                        DateUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                        DateUnit.Week => I18NEntity.GetString("RM_JS_ScheduleSetting_Weeks"),
                                        DateUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                        _ => ""
                                    },
                                };
                                info.ModifyContent.Add(retentionAudit);
                                AuditItem retentionAction = new AuditItem()
                                {
                                    TargetSetting = "RM_AR_CP_GSS_OperateDataTitle",
                                };
                                if (retentionRule.IsMarkDataTier)
                                {
                                    retentionAction.OldValue = "RM_AR_CP_GSS_Retention_MarkDataTier" + " " + retentionRule.TierType switch
                                    {
                                        (int)Storage.AccessTierType.Cold => I18NEntity.GetString("RM_JS_Rule_DetailValue_ColdTier"),
                                        (int)Storage.AccessTierType.Archive => I18NEntity.GetString("RM_JS_Rule_DetailValue_ArchiveTier"),
                                    };
                                    info.ModifyContent.Add(retentionAction);
                                }
                                else if (retentionRule.IsMove)
                                {
                                    var moveStorageDevice = StorageDeviceService.GetStorageDeviceById(retentionRule.MoveDeviceId);
                                    retentionAction.OldValue = "RM_AR_CP_GSS_Retention_MoveDataRadio" + " " + moveStorageDevice.Name;
                                    info.ModifyContent.Add(retentionAction);
                                }
                                else if (retentionRule.DeleteTheData)
                                {
                                    retentionAction.OldValue = "Gui.Common_Delete the data";
                                    info.ModifyContent.Add(retentionAction);
                                    AuditItem deleteStub = new AuditItem()
                                    {
                                        TargetSetting = "RM_AR_CP_GSS_Retention_RemoveStub",
                                        OldValue = retentionRule.RemoveOrphanedStub ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                                    };
                                    info.ModifyContent.Add(deleteStub);
                                    AuditItem deleteJob = new AuditItem()
                                    {
                                        TargetSetting = "RM_AR_CP_GSS_Retention_RemoveJob",
                                        OldValue = retentionRule.RemoveTheJob ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                                    };
                                    info.ModifyContent.Add(deleteJob);
                                    if (KeyValueService.IsEnableSoftDeleteSetting())
                                    {
                                        AuditItem softDelete = new AuditItem()
                                        {
                                            TargetSetting = "RM_AR_CP_GSS_Retention_SoftDelete",
                                            OldValue = retentionRule.IsSoftDelete ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                                        };
                                        info.ModifyContent.Add(softDelete);
                                        if (retentionRule.IsSoftDelete)
                                        {
                                            AuditItem softDeleteTime = new AuditItem()
                                            {
                                                TargetSetting = "",
                                                OldValue = string.Format(I18NEntity.GetString("RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"), retentionRule.SoftDeleteKeepValue + " " + retentionRule.SoftDeleteDateUnit switch
                                                {
                                                    DateUnit.Day => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days"),
                                                    DateUnit.Month => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"),
                                                    DateUnit.Week => I18NEntity.GetString("RM_JS_ScheduleSetting_Weeks"),
                                                    DateUnit.Year => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years"),
                                                    _ => ""
                                                })
                                            };
                                            info.ModifyContent.Add(softDeleteTime);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (action == (int)AuditAction.StorageDeviceDelete)
                {
                    List<string> ids = (List<string>)args[0];
                    var storageNames = (await StorageDeviceDao.FindListAsync(r => ids.Contains(r.Id.ToString()))).Select(r => r.Name).ToList();
                    info.Object = String.Join(";", storageNames);
                }
                else if (action == (int)AuditAction.StorageDeviceSetIndexDevice)
                {
                    if ((SettingProfilesType)args[1] == SettingProfilesType.IndexDevice)
                    {
                        var storageId = new Guid(args[0].ToString());
                        var indexDevice = (await StorageDeviceDao.FindListAsync(r => r.Id.Equals(storageId))).FirstOrDefault();
                        info.Action = AuditAction.StorageDeviceSetIndexDevice;
                        info.Object = indexDevice?.Name;
                    }
                    else if ((SettingProfilesType)args[1] == SettingProfilesType.ExportLocationDevice)
                    {
                        var oldStorageName = string.Empty;
                        info.ModifyContent = new List<AuditItem>();
                        SettingProfileDto mDto = new SettingProfileDto()
                        {
                            Type = (int)SettingProfilesType.ExportLocationDevice,
                            Name = "UsingExportLocationDevice"
                        };
                        var dto = SettingProfileDao.Load(mDto);
                        if (dto != null)
                        {
                            var tempDto = StorageDeviceConvert.ConvertSettingProfileToIndexDeviceDto(dto);
                            var currentExportLocationId = tempDto.Settings;
                            var storageDto = StorageDeviceService.GetStorageDeviceById(currentExportLocationId);
                            if (storageDto != null)
                            {
                                oldStorageName = storageDto.Name;
                            }
                        }
                        AuditItem exportLocationAudit = new AuditItem();
                        exportLocationAudit.TargetSetting = "RM_Audit_ExportLocation";
                        exportLocationAudit.OldValue = oldStorageName;
                        info.ModifyContent.Add(exportLocationAudit);
                        info.Category = AuditCategory.ExportSettings;
                        if ((bool)args[3])
                        {
                            info.Action = AuditAction.CompliantExport;
                        }
                        else
                        {
                            info.Action = AuditAction.ConfigureExportSetting;
                        }
                    }
                }
                else if (action == (int)AuditAction.ConfigureExportSetting || action == (int)AuditAction.CompliantExport)
                {
                    var oldStorageName = string.Empty;
                    var allExportLocations = await GlobalSettingService.GetAllExportLocationAsync();
                    info.ModifyContent = new List<AuditItem>();
                    var exportLocationId = GlobalSettingService.GetCurrentExportLocationId();
                    var gssInfosTemp = allExportLocations.FirstOrDefault(l => l.Id == exportLocationId);
                    oldStorageName = gssInfosTemp?.Name;
                    AuditItem exportLocationAudit = new AuditItem();
                    exportLocationAudit.TargetSetting = "RM_Audit_ExportLocation";
                    exportLocationAudit.OldValue = oldStorageName;
                    info.ModifyContent.Add(exportLocationAudit);
                }
                else if (action == (int)AuditAction.ConfigureEndUserRestoreSetting)
                {
                    EndUserRestoreSettingDto settingDto = (EndUserRestoreSettingDto)args[0];
                    info.Object = string.Empty;
                }
                else if (action == (int)AuditAction.SaveRestoreSiteMapping)
                {
                    info.ModifyContent = new List<AuditItem>();
                }
                else if (action == (int)AuditAction.SaveRestoreSiteWhitelist || action == (int)AuditAction.SaveRestoreSiteBlacklist)
                {
                    info.ModifyContent = new List<AuditItem>();
                    AuditItem sourceMappings = new AuditItem();
                    sourceMappings.TargetSetting = "RM_AR_RC_TableCol_AddSiteCollectionUrlList";                 
                    List<WhitelistInfo> list = (List<WhitelistInfo>)args[0];
                    sourceMappings.NewValue = string.Join(";\r\n", list.Select(m => m.SiteCollectionUrl.Trim('/', '\\', ' ')).ToList());
                    info.ModifyContent.Add(sourceMappings);
                }
                else if (action == (int)AuditAction.DeleteRestoreSiteWhitelist || action == (int)AuditAction.DeleteRestoreSiteBlacklist)
                {
                    info.ModifyContent = new List<AuditItem>();
                    AuditItem sourceMappings = new AuditItem();
                    sourceMappings.TargetSetting = "RM_AR_RC_TableCol_RemoveSiteCollectionUrlList";
                    List<string> list =  (List<string>)args[0];
                    var mappings = RMRestoreSiteMappingDao.GetRecordsByIds(list);
                    sourceMappings.NewValue = string.Join(";\r\n", mappings.Select(m => m.SourceSiteUrl.Trim('/', '\\', ' ')).ToList());
                    info.ModifyContent.Add(sourceMappings);
                }
                else if (action == (int)AuditAction.SwitchFullTextIndexType)
                {
                    SwitchFullTextIndexParam param = (SwitchFullTextIndexParam)args[0];
                    info.ModifyContent = new List<AuditItem>();
                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_RC_Audit_Action_FullTextIndexType",
                        OldValue = param.Type == FullTextIndexType.WhiteList ? "RM_RC_Audit_Action_FullTextIndexBlacklistType" : "RM_RC_Audit_Action_FullTextIndexWhitelistType",
                        NewValue = param.Type == FullTextIndexType.WhiteList ? "RM_RC_Audit_Action_FullTextIndexWhitelistType" : "RM_RC_Audit_Action_FullTextIndexBlacklistType",
                    });
                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_AR_Audit_Action_CleanFullTextIndex",
                        OldValue = param.CleanSCList ? "RM_JS_Common_Yes" : "RM_JS_Common_No",
                    });
                }
                else if (action == (int)AuditAction.DeleteRestoreSiteMapping)
                {
                    List<string> ids = (List<string>)args[0];
                    info.ModifyContent = new List<AuditItem>();
                    var siteMappings = RMRestoreSiteMappingDao.GetMappingsById(ids);
                    AuditItem sourceMappings = new AuditItem();
                    sourceMappings.TargetSetting = "RM_AR_RC_SiteMapping";
                    sourceMappings.OldValue = string.Join("  \r\n",
                        siteMappings.Select(map => map.SourceSiteUrl + " : " + map.TargetSiteUrl).ToArray()
                        );
                    info.ModifyContent.Add(sourceMappings);
                }
            }
            catch (Exception e)
            {
                logger.Warn("Archiver setting before Audit handler,message detail {0}", e.ToString());
            }

            return info;
        }
    }
}
