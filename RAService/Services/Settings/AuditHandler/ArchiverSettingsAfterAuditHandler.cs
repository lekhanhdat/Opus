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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Archiver.Export;
using AvePoint.RA.Service.Services.RMSharePointSettings.AuditHandler;
using RAArchiverCommon.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StubSettingDto = AvePoint.GCommon.Contract.Server.StubSetting.StubSettingDto;

namespace AvePoint.RA.Service.Services.Settings.AuditHandler
{
    public class ArchiverSettingsAfterAuditHandler : IAfterAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(ArchiverSettingsAfterAuditHandler));
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private IGlobalSettingService GlobalSettingService => PlatformWindsorManager.GetService<IGlobalSettingService>();
        private IKeyValueService KeyValueService => PlatformWindsorManager.GetService<IKeyValueService>();
        private IRMRestoreSiteMappingDao _restoreSiteMappingDao => PlatformWindsorManager.GetService<IRMRestoreSiteMappingDao>();

        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            if (action == (int)AuditAction.StubSettingCreate || action == (int)AuditAction.StubSettingUpdate || action == (int)AuditAction.StubSettingDelete ||
                action == (int)AuditAction.StorageDeviceCreate || action == (int)AuditAction.StorageDeviceUpdate || action == (int)AuditAction.StorageDeviceDelete ||
                action == (int)AuditAction.StorageDeviceSetIndexDevice)
            {
                if (info.Action == AuditAction.ConfigureExportSetting || info.Action == AuditAction.CompliantExport)
                {
                    var newStorageName = string.Empty;
                    var storageDto = StorageDeviceService.GetStorageDeviceById(args[0].ToString());
                    var exportLocationTarget = "RM_Audit_ExportLocation";
                    if (storageDto != null)
                    {
                        newStorageName = storageDto.Name;
                    }
                    AuditItem exportLocation = info.ModifyContent.FirstOrDefault(a => exportLocationTarget.Equals(a.TargetSetting));
                    if (exportLocation != null) { exportLocation.NewValue = newStorageName; }
                }
                else if (action == (int)AuditAction.StubSettingCreate || action == (int)AuditAction.StubSettingUpdate)
                {
                    StubSettingDto stub = (StubSettingDto)args[0];
                    info.Object = stub.Name;

                    info.ModifyContent ??= [];
                    AuditHelper.SaveNewAuditItem(info, "RM_AR_CP_Stub_ColName_Name", stub.Name);

                    var stubTypeI18Nstr = (LeaveStubType)stub.StubType switch
                    {
                        LeaveStubType.Aspx => "RM_AR_CP_Stub_Type_Aspx",
                        LeaveStubType.Txt => "RM_AR_CP_Stub_Type_Txt",
                        LeaveStubType.Html => "RM_AR_CP_Stub_Type_Html",
                        LeaveStubType.Link => "RM_AR_CP_Stub_Type_RestoreLink",
                        _ => "Unknown Stub Type"
                    };

                    AuditHelper.SaveNewAuditItem(info, "RM_AR_CP_Stub_Panel_StubType", stubTypeI18Nstr);
                    AuditHelper.SaveNewAuditItem(info, RMConstants.STUBCONTENT, stub.StubContent);
                    AuditHelper.SaveNewAuditItem(info, "RM_AR_CP_Stub_Panel_ConfigStubRetention", stub.IsEnabledRetention ? "RM_JS_Common_Yes" : "RM_JS_Common_No");

                    AuditItem tempEnabledRetentionItem = 
                        action == (int)AuditAction.StubSettingUpdate 
                        ? info.ModifyContent.FirstOrDefault(a => "RM_AR_CP_Stub_Panel_ConfigStubRetention".Equals(a.TargetSetting)) 
                        : null;

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
                        AuditHelper.SaveNewAuditItem(info, RMConstants.STUBRETENTIONPERIOD, retentionPeriod + " ");
                    }
                    AuditHelper.SaveNewAuditItem(info, AccountUtility.IsSupportRecordLabel() ? "RM_Audit_Stub_RecordsLabel" : "RM_AR_CP_Stub_Panel_Declare", stub.IsDeclareStubAsRecords ? "RM_JS_Common_Yes" : "RM_JS_Common_No");

                    if (tempEnabledRetentionItem != null && "RM_JS_Common_No".Equals(tempEnabledRetentionItem.OldValue) && stub.IsEnabledRetention)
                    {
                        var retentionPeriodItem = info.ModifyContent.FirstOrDefault(a => RMConstants.STUBRETENTIONPERIOD.Equals(a.TargetSetting));
                        if (retentionPeriodItem != null)
                        {
                            var insertIndex = info.ModifyContent.IndexOf(tempEnabledRetentionItem) + 1;
                            var currentIndex = info.ModifyContent.IndexOf(retentionPeriodItem);
                            if (currentIndex != insertIndex)
                            {
                                info.ModifyContent.RemoveAt(currentIndex);
                                info.ModifyContent.Insert(insertIndex, retentionPeriodItem);
                            }
                        }
                    }
                }
                else if (info.Action == AuditAction.StorageDeviceCreate || info.Action == AuditAction.StorageDeviceUpdate)
                {
                    StorageDeviceDto storageDevice = (StorageDeviceDto)args[0];
                    if (storageDevice.AuditId != null)
                    {
                        info.Action = AuditAction.StorageDeviceUpdate;
                    }
                    info.Object = storageDevice.Name;
                    info.ModifyContent ??= [];

                    AuditHelper.SaveNewAuditItem(info, "RM_AR_CP_GSS_Name", storageDevice.Name);
                    AuditHelper.SaveNewAuditItem(info, "RM_AR_CP_GSS_Description", storageDevice.Description);

                    switch ((StorageDeviceType)storageDevice.Type)
                    {
                        case StorageDeviceType.CloudAmazon:
                            AuditHelper.SaveNewAuditItem(info, "Gui.Common_Storage Type", "MediaStorage_Amazon_Amazon_S3");
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_Amazon_Bucket_Name", RAStorageUtil.GetStorageConfigValue(storageDevice, "bucketname"));
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_Amazon_Access_Key_ID", RAStorageUtil.GetStorageConfigValue(storageDevice, "name"));
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_Amazon_Secret_Access_Key", RAStorageUtil.SKP);
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_Amazon_Storage_Region", RAStorageUtil.GetI18NRegion(RAStorageUtil.GetStorageConfigValue(storageDevice, "region")));
                            break;

                        case StorageDeviceType.S3Compatible:
                            AuditHelper.SaveNewAuditItem(info, "Gui.Common_Storage Type", "MediaStorage_S3Compatible_Compatible_Amazon_S3");
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_S3Compatible_Bucket_Name", RAStorageUtil.GetStorageConfigValue(storageDevice, "bucketname"));
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_S3Compatible_Access_Key_ID", RAStorageUtil.GetStorageConfigValue(storageDevice, "name"));
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_S3Compatible_Secret_Access_Key", RAStorageUtil.SKP);
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_S3Compatible_Endpoint", RAStorageUtil.GetStorageConfigValue(storageDevice, "endpoint"));
                            break;

                        case StorageDeviceType.Dropbox:
                            AuditHelper.SaveNewAuditItem(info, "Gui.Common_Storage Type", "MediaStorage_Dropbox_Dropbox");
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_Dropbox_Root_folder", RAStorageUtil.GetStorageConfigValue(storageDevice, "containername"));
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_Dropbox_TokenSecret", RAStorageUtil.SKP);
                            break;

                        case StorageDeviceType.FTP:
                            AuditHelper.SaveNewAuditItem(info, "Gui.Common_Storage Type", "MediaStorage_FTP_FTP");
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_FTP_Host", RAStorageUtil.GetStorageConfigValue(storageDevice, "host"));
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_FTP_Port", RAStorageUtil.GetStorageConfigValue(storageDevice, "port"));
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_FTP_Root_Folder", RAStorageUtil.GetStorageConfigValue(storageDevice, "ftprootfolder"));
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_FTP_Username", RAStorageUtil.GetStorageConfigValue(storageDevice, "name"));
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_FTP_Password", RAStorageUtil.SKP);
                            break;

                        case StorageDeviceType.Google:
                            AuditHelper.SaveNewAuditItem(info, "Gui.Common_Storage Type", "MediaStorage_Google");
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_Google_ClientEmail", RAStorageUtil.GetStorageConfigValue(storageDevice, "name"));
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_Google_PrivateID", RAStorageUtil.SKP);
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_Google_ProjectID", RAStorageUtil.GetStorageConfigValue(storageDevice, "accesspoint"));
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_Google_BucketName", RAStorageUtil.GetStorageConfigValue(storageDevice, "containername"));
                            break;

                        case StorageDeviceType.CloudAzure:
                            AuditHelper.SaveNewAuditItem(info, "Gui.Common_Storage Type", "MediaStorage_Azure_Windows_Azure_Storage");
                            var accessPoint = RAStorageUtil.GetStorageConfigValue(storageDevice, "accesspoint");
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_Azure_Access_Point", string.IsNullOrEmpty(accessPoint) ? "https://blob.core.windows.net" : accessPoint);
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_Azure_Container_Name", RAStorageUtil.GetStorageConfigValue(storageDevice, "containername"));
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_Azure_Account_Name", RAStorageUtil.GetStorageConfigValue(storageDevice, "name"));
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_Azure_Account_Key", RAStorageUtil.SKP);
                            break;

                        case StorageDeviceType.SFTP:
                            AuditHelper.SaveNewAuditItem(info, "Gui.Common_Storage Type", "MediaStorage_SFTP_SFTP");
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_SFTP_Host", RAStorageUtil.GetStorageConfigValue(storageDevice, "host"));
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_SFTP_Port", RAStorageUtil.GetStorageConfigValue(storageDevice, "port"));
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_SFTP_Root_Folder", RAStorageUtil.GetStorageConfigValue(storageDevice, "sftprootfolder"));
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_SFTP_Username", RAStorageUtil.GetStorageConfigValue(storageDevice, "name"));
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_SFTP_Password", RAStorageUtil.SKP);
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_SFTP_PrivateKeyFile", RAStorageUtil.GetStorageConfigValue(storageDevice, "privatekeyfile"));
                            AuditHelper.SaveNewAuditItem(info, "MediaStorage_SFTP_PrivateKeyPassword", RAStorageUtil.SKP);
                            break;

                        default:
                            logger.Warn("This storage type may not be supported yet. Storage type: {0}", (StorageDeviceType)storageDevice.Type);
                            break;
                    }

                    AuditHelper.SaveNewAuditItem(info, "Gui.Common_Advanced", RAStorageUtil.GetStorageConfigValue(storageDevice, "advanced").ToBoolean(false) ? "RM_JS_Common_Yes" : "RM_JS_Common_No");
                    AuditHelper.SaveNewAuditItem(info, "Gui.Common_5514307E-E936-44C9-811D-7D1DDA6667A4", RAStorageUtil.GetStorageConfigValue(storageDevice, "extendedparameters"));


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
                                    NewValue = I18NEntity.GetString("Gui.Common_Keep the last") + " " + retentionRule.KeepValue + " " + retentionRule.ArchiveDateUnit switch
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
                                    retentionAction.NewValue = "RM_AR_CP_GSS_Retention_MarkDataTier" + " " + retentionRule.TierType switch
                                    {
                                        (int)Storage.AccessTierType.Cold => I18NEntity.GetString("RM_JS_Rule_DetailValue_ColdTier"),
                                        (int)Storage.AccessTierType.Archive => I18NEntity.GetString("RM_JS_Rule_DetailValue_ArchiveTier"),
                                    };
                                    info.ModifyContent.Add(retentionAction);
                                }
                                else if (retentionRule.IsMove)
                                {
                                    var moveStorageDevice = StorageDeviceService.GetStorageDeviceById(retentionRule.MoveDeviceId);
                                    retentionAction.NewValue = "RM_AR_CP_GSS_Retention_MoveDataRadio" + " " + moveStorageDevice.Name;
                                    info.ModifyContent.Add(retentionAction);
                                }
                                else if (retentionRule.DeleteTheData)
                                {
                                    retentionAction.NewValue = "Gui.Common_Delete the data";
                                    info.ModifyContent.Add(retentionAction);
                                    AuditItem deleteStub = new AuditItem()
                                    {
                                        TargetSetting = "RM_AR_CP_GSS_Retention_RemoveStub",
                                        NewValue = retentionRule.RemoveOrphanedStub ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                                    };
                                    info.ModifyContent.Add(deleteStub);
                                    AuditItem deleteJob = new AuditItem()
                                    {
                                        TargetSetting = "RM_AR_CP_GSS_Retention_RemoveJob",
                                        NewValue = retentionRule.RemoveTheJob ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                                    };
                                    info.ModifyContent.Add(deleteJob);
                                    if (KeyValueService.IsEnableSoftDeleteSetting())
                                    {
                                        AuditItem softDelete = new AuditItem()
                                        {
                                            TargetSetting = "RM_AR_CP_GSS_Retention_SoftDelete",
                                            NewValue = retentionRule.IsSoftDelete ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                                        };
                                        info.ModifyContent.Add(softDelete);
                                        if (retentionRule.IsSoftDelete)
                                        {
                                            AuditItem softDeleteTime = new AuditItem()
                                            {
                                                TargetSetting = "",
                                                NewValue = string.Format(I18NEntity.GetString("RM_AR_CP_GSS_Retention_SoftDelete_KeepTheLast"), retentionRule.SoftDeleteKeepValue + " " + retentionRule.SoftDeleteDateUnit switch
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
                else
                {
                    if (info.Object == null && returnValue != null)
                    {
                        info.Object = returnValue.ToString();
                    }
                }

                RAReturnMessage msg = (RAReturnMessage)returnValue;
                if (msg != null)
                {
                    info.Status = (int)msg.MessageType;
                }
            }
            else if (action == (int)AuditAction.ConfigureExportSetting || action == (int)AuditAction.CompliantExport)
            {
                var newStorageName = string.Empty;
                var allExportLocations = await GlobalSettingService.GetAllExportLocationAsync();
                var gssInfosTemp = allExportLocations.FirstOrDefault(l => l.Id == (string)args[0]);
                newStorageName = gssInfosTemp?.Name;
                var exportLocationTarget = "RM_Audit_ExportLocation";
                AuditItem exportLocation = info.ModifyContent.FirstOrDefault(a => exportLocationTarget.Equals(a.TargetSetting));
                if (exportLocation != null) { exportLocation.NewValue = newStorageName; }
            }
            else if (action == (int)AuditAction.ConfigureEndUserRestoreSetting)
            {
                info.Status = (int)returnValue;
            }
            else if (action == (int)AuditAction.SaveRestoreSiteMapping)
            {
                RAReturnMessage msg = (RAReturnMessage)returnValue;
                info.Status = (int)msg.MessageType;
                List<SiteMappingInfo> mappingInfo = (List<SiteMappingInfo>)args[0];

                AuditItem sourceMappings = new AuditItem();
                sourceMappings.TargetSetting = "RM_AR_RC_SiteMapping";
                sourceMappings.NewValue = string.Join("  \r\n",
                    mappingInfo.Select(map => map.SourceSiteUrl + " : " + map.TargetSiteUrl).ToArray()
                    );
                info.ModifyContent.Add(sourceMappings);
            }
            else if (action == (int)AuditAction.DeleteRestoreSiteMapping)
            {
                RAReturnMessage msg = (RAReturnMessage)returnValue;
                info.Status = (int)msg.MessageType;
            }
            else if (action == (int)AuditAction.ImportRestoreSiteMapping || action == (int)AuditAction.ExportRestoreSiteMapping)
            {
                info.Object = returnValue?.ToString();
                if (string.IsNullOrWhiteSpace(info.Object))
                {
                    info.Status = (int)RAMessageType.Exception;
                }
            }
            else if (action == (int)AuditAction.SaveRestoreSiteWhitelist || action == (int)AuditAction.DeleteRestoreSiteWhitelist
                || action == (int)AuditAction.SaveRestoreSiteBlacklist || action == (int)AuditAction.DeleteRestoreSiteBlacklist)
            {
                RAReturnMessage msg = (RAReturnMessage)returnValue;
                info.Status = (int)msg.MessageType;
            }
            else if (action == (int)AuditAction.SwitchFullTextIndexType)
            {
                RAReturnMessage msg = (RAReturnMessage)returnValue;
                info.Status = (int)msg.MessageType;
            }
            else if (action == (int)AuditAction.ImportRestoreSiteWhitelist
                || action == (int)AuditAction.ExportRestoreSiteWhitelist
                || action == (int)AuditAction.ImportRestoreSiteBlacklist
                || action == (int)AuditAction.ExportRestoreSiteBlacklist)
            {
                var jobId = returnValue as string;
                info.Category = AuditCategory.RestoreCenter;
                info.Module = AuditModule.RestoreCenter;
                info.Object = jobId;
            }
            return info;
        }
    }
}
