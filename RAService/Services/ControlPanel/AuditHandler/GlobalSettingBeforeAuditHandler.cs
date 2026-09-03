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
using AvePoint.GCommon.Contract.GranularBackup.Object;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.CSD.Service;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Graph;
using RAGoogle.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ControlPanel.AuditHandler
{
    public class GlobalSettingBeforeAuditHandler : IBeforeAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(GlobalSettingBeforeAuditHandler));
        public IGlobalSettingService globalSettingService => PlatformWindsorManager.GetService<IGlobalSettingService>();
        public IExportSettingService exportSettingService => PlatformWindsorManager.GetService<IExportSettingService>();
        public IRMArchiverSettingsService ArchiverSettingsService => PlatformWindsorManager.GetService<IRMArchiverSettingsService>();
        public ISecurityGroupManagementService SecurityGroupManagementService => PlatformWindsorManager.GetService<ISecurityGroupManagementService>();
        private IGeneralSettingService  GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService >();
        public ICSDApiKeyDao ApiKeyDao => PlatformWindsorManager.GetService<ICSDApiKeyDao>();
        private IRMKeyValueDao  RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private ISettingProfilesDao SettingProfileDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();

        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        public async System.Threading.Tasks.Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            var info = new RMAuditInfo();
            info.ModifyContent = new List<AuditItem>();
            try
            {
                //if (action == (int)AuditAction.ConfigureGlobalsettings)
                //{
                //    string auditorNone = "RM_JS_RDM_CreateRule_ExportType_None";

                //    GlobalStorageSetting oldGlobalSettings = globalSettingService.LoadGlobalSettingInfoFromRA();

                //    AuditItem auditItem2 = new AuditItem();
                //    auditItem2.TargetSetting = "RM_CP_GSS_Audit_DataStoreLocation";
                //    auditItem2.OldValue = oldGlobalSettings != null && oldGlobalSettings.CurrentStoragePolicy != null ? oldGlobalSettings.CurrentStoragePolicy.Name : auditorNone;
                //    info.ModifyContent.Add(auditItem2);

                //    AuditItem auditItem3 = new AuditItem();
                //    auditItem3.TargetSetting = "RM_CP_GSS_Audit_ExportLocation";
                //    auditItem3.OldValue = oldGlobalSettings != null && oldGlobalSettings.CurrentExportLocation != null ? oldGlobalSettings.CurrentExportLocation.Name : auditorNone;
                //    info.ModifyContent.Add(auditItem3);

                //    //AuditItem auditItem4 = new AuditItem();
                //    //auditItem4.TargetSetting = "Processing Pool";
                //    //auditItem4.OldValue = oldGlobalSettings != null && oldGlobalSettings.CurrentProcessingPool != null ? oldGlobalSettings.CurrentProcessingPool.Name : auditorNone;
                //    //info.ModifyContent.Add(auditItem4);

                //    AuditItem auditItem = new AuditItem();
                //    auditItem.TargetSetting = "RM_CP_GSS_DataHandle_Compression";
                //    var compressionMethod = "RM_CP_GSS_DataHandle_Compression";
                //    auditItem.OldValue = oldGlobalSettings != null && oldGlobalSettings.UseCompression ? compressionMethod : auditorNone;
                //    info.ModifyContent.Add(auditItem);

                //    AuditItem compressionSpeedItem = new AuditItem();
                //    compressionSpeedItem.TargetSetting = "RM_CP_GSS_Audit_CompressionLevel";
                //    var compressionSpeed = oldGlobalSettings != null ? oldGlobalSettings.CompressionSpeed.ToString() : auditorNone;
                //    compressionSpeedItem.OldValue = oldGlobalSettings != null && oldGlobalSettings.UseCompression ? compressionSpeed : auditorNone;
                //    info.ModifyContent.Add(compressionSpeedItem);

                //    AuditItem auditItem1 = new AuditItem();
                //    auditItem1.TargetSetting = "RM_CP_GSS_DataHandle_Encryption";
                //    var encryptionMethod = "RM_CP_GSS_DataHandle_Encryption";
                //    auditItem1.OldValue = oldGlobalSettings != null && oldGlobalSettings.UseEncryption ? encryptionMethod : auditorNone;
                //    info.ModifyContent.Add(auditItem1);

                //    AuditItem securityProfileItem = new AuditItem();
                //    securityProfileItem.TargetSetting = "RM_CP_GSS_SecurityProfile";
                //    var curSecurityProfileName = oldGlobalSettings.CurrentSecurityProfile != null ? oldGlobalSettings.CurrentSecurityProfile.Name : auditorNone;
                //    var securityProfile = oldGlobalSettings != null ? curSecurityProfileName : auditorNone;
                //    securityProfileItem.OldValue = oldGlobalSettings != null && oldGlobalSettings.UseEncryption ? securityProfile : auditorNone;
                //    info.ModifyContent.Add(securityProfileItem);


                //}
                if (action == (int)AuditAction.ConfigureExportSetting || action == (int)AuditAction.CompliantExport)
                {
                    var oldValue = new StringBuilder();

                    var veofilename = exportSettingService.GetConfigureFileName(ExportSettingType.VEO);
                    if (!string.IsNullOrEmpty(veofilename))
                    {
                        oldValue.AppendFormat("VEO:{0}", veofilename);
                    }

                    var nnafilename = exportSettingService.GetConfigureFileName(ExportSettingType.NAA);
                    if (!string.IsNullOrEmpty(nnafilename))
                    {
                        if (!string.IsNullOrEmpty(oldValue.ToString()))
                        {
                            oldValue.AppendFormat("<br>NAA:{0}", nnafilename);
                        }
                        else
                        {
                            oldValue.AppendFormat("NAA:{0}", nnafilename);
                        }

                    }

                    var narafilename = exportSettingService.GetConfigureFileName(ExportSettingType.NARA);
                    if (!string.IsNullOrEmpty(narafilename))
                    {
                        if (!string.IsNullOrEmpty(oldValue.ToString()))
                        {
                            oldValue.AppendFormat("<br>NARA:{0}", narafilename);
                        }
                        else
                        {
                            oldValue.AppendFormat("NARA:{0}", narafilename);
                        }

                    }

                    var exportEncryptionEnabled = RMKeyValueDao.IsExportDataEncryptionEnabled();
                    var enableStatusStr = exportEncryptionEnabled ? I18NEntity.GetString("RM_JS_Common_Enabled") : I18NEntity.GetString("RM_JS_Common_Disabled");
                    if (!string.IsNullOrEmpty(oldValue.ToString()))
                    {
                        oldValue.AppendFormat("<br>{0}", string.Format(I18NEntity.GetString("RM_RC_Audit_Action_ExportEncryptionEnabled"), enableStatusStr));
                    }
                    else
                    {
                        oldValue.AppendFormat(string.Format(I18NEntity.GetString("RM_RC_Audit_Action_ExportEncryptionEnabled"), enableStatusStr));
                    }
                    var enableCheckSum = EnableCheckSum() ? I18NEntity.GetString("RM_JS_Common_Enabled") : I18NEntity.GetString("RM_JS_Common_Disabled");
                    oldValue.AppendFormat("<br>{0}", string.Format(I18NEntity.GetString("RM_RC_Audit_Action_ExportDataCheckSumEnabled"), enableCheckSum));
                    AuditItem auditItem = new AuditItem();
                    auditItem.TargetSetting = "RM_RC_Audit_Configuration_File";
                    auditItem.OldValue = oldValue.ToString();
                    info.ModifyContent.Add(auditItem);
                }
                else if (action == (int)AuditAction.ConfigureDedupScheduleJob)
                {
                    AuditItem auditItem = new AuditItem();
                    var fileInfo = ArchiverSettingsService.GetSavedDedupFileInfo();
                    var fileName = fileInfo?.GetValue("FileName");
                    if (!string.IsNullOrEmpty(fileName) && double.TryParse(fileInfo?.GetValue("FileSize"), out var fileSize))
                    {
                        auditItem.OldValue = $"{fileName} {fileSize} (KB)";
                    }
                    auditItem.TargetSetting = "RM_RC_Audit_Configuration_File";
                    info.ModifyContent.Add(auditItem);
                }
                else if (action == (int)AuditAction.CreateSecurityGroup) {
                    var newGroup = args[0] as SecurityGroupDto;
                    #region group name item
                    AuditItem groupNameItem = new AuditItem
                    {
                        TargetSetting = "RM_CP_AM_Table_Column_GroupName",
                        NewValue = newGroup.Name
                    };
                    info.ModifyContent.Add(groupNameItem);
                    #endregion

                    #region group desc item
                    AuditItem groupDescItem = new AuditItem
                    {
                        TargetSetting = "RM_CP_AM_Table_Column_Desc",
                        NewValue = newGroup.Description
                    };
                    info.ModifyContent.Add(groupDescItem);
                    #endregion

                    #region group members item
                    AuditItem groupMemberItem = new AuditItem
                    {
                        TargetSetting = "RM_CP_AM_Table_Column_MembersName",
                        NewValue = GetGroupMemberNames(newGroup.Users)
                    };
                    info.ModifyContent.Add(groupMemberItem);
                    #endregion
                    AuditItem permissionSettings = new AuditItem
                    {
                        TargetSetting = "RM_CP_AM_Table_Column_PermissionGroupName",
                        NewValue = newGroup.SecurityGroupControlType switch 
                        {
                            SecurityGroupControlType.DataScope => "RM_CP_AM_Permission_DataScope",
                            SecurityGroupControlType.FunctionModule => "RM_CP_AM_Permission_FunctionMoudle",
                            _=>""
                        }
                    };
                    info.ModifyContent.Add(permissionSettings);
                    if (newGroup.SecurityGroupControlType == SecurityGroupControlType.DataScope)
                    {
                        #region group scope and containers
                        if (RMKeyValueDao.HasUpgradeTeams())
                        {
                            await AddScopeAndContainersAuditAsync(info, null, newGroup, SourceFlag.Teams, "RM_CP_AM_IsCheckeTeamsAndGroupScope");
                        }
                        await AddScopeAndContainersAuditAsync(info, null, newGroup, SourceFlag.SharePoint, "RM_CP_AM_IsCheckdSPScope");
                        await AddScopeAndContainersAuditAsync(info, null, newGroup, SourceFlag.OneDrive, "RM_CP_AM_IsCheckdOneDriveScope");
                        await AddScopeAndContainersAuditAsync(info, null, newGroup, SourceFlag.Exchange, "RM_CP_AM_IsCheckdEXOScope");
                        await AddScopeAndContainersAuditAsync(info, null, newGroup, SourceFlag.Physical, "RM_CP_AM_IsCheckdPhysicalScope");
                        await AddScopeAndContainersAuditAsync(info, null, newGroup, SourceFlag.FileSystem, "RM_CP_AM_IsCheckdFileSystemScope");
                        await AddScopeAndContainersAuditAsync(info, null, newGroup, SourceFlag.SharePointOnPrem, "RM_CP_AM_IsCheckdSPLocalScope");
                        await AddScopeAndContainersAuditAsync(info, null, newGroup, SourceFlag.AzureFileShare, "RM_CP_AM_IsCheckdAzureFileScope");
                        await AddScopeAndContainersAuditAsync(info, null, newGroup, SourceFlag.Box, "RM_CP_AM_IsCheckedBoxScope");
                        if (TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, Contract.RoleAssignments.PaidForProduct.OpusGoogle))
                        {
                            await AddScopeAndContainersAuditAsync(info, null, newGroup, SourceFlag.Google, "RM_CP_AM_IsCheckedGoogleScope");
                        }
                        #endregion
                        AuditItem enableTrimItem = new AuditItem
                        {
                            TargetSetting = I18NEntity.GetString("RM_CP_AM_Permission_DelegateTitle"),
                            NewValue = newGroup.IsEnableTrim ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No")
                        };
                        info.ModifyContent.Add(enableTrimItem);

                        AuditItem enableReportPermission = new AuditItem
                        {
                            TargetSetting = I18NEntity.GetString("RM_CP_AM_Report_Permission_SpecificReport"),
                            NewValue = newGroup.IsUseReportingPermissionControl ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No")
                        };
                        info.ModifyContent.Add(enableReportPermission);

                        AuditItem reportPermission = new AuditItem
                        {
                            TargetSetting = I18NEntity.GetString("RM_CP_AM_Report"),
                            NewValue = GenerateReportPermissionString(newGroup.ReportingPermission)
                        };
                        info.ModifyContent.Add(reportPermission);

                        #region group term permission settings
                        AddTermPermissionAudit(info, null, newGroup);
                        #endregion

                        #region rule permission settings
                        AddRulePermissionAudit(info, null, newGroup);
                        #endregion
                        if (LicenseHelperService.HasOpusILLicense || LicenseHelperService.HasOpusGoogleLicense)
                        {
                            AddManageHoldPermissionAudit(info, newGroup);

                            AddManualApprovalSettingPermissionAudit(info, newGroup);
                        }
                    }
                    else
                    {
                        AuditItem restoreCenter = new AuditItem
                        {
                            TargetSetting = "RM_RC_Audit_Module_RestoreCenter",
                            NewValue = newGroup.FunctionSubPermission switch
                            {
                                FunctionSubPermission.RestoreCenterSearch => "RM_CP_AM_SubPermission_SearchOnly",
                                FunctionSubPermission.RestoreCenterExport => "RM_CP_AM_SubPermission_SearchAndExport",
                                FunctionSubPermission.RestoreCenterFullControl => "RM_CP_AM_SubPermission_FullControl",
                                _ => ""
                            },
                        };
                        info.ModifyContent.Add(restoreCenter);
                    }


                }
                else if (action == (int)AuditAction.EditSecurityGroup)
                {
                    var newGroup = args[0] as SecurityGroupDto;
                    var oldGroup = await SecurityGroupManagementService.GetGroupAsync(newGroup.Id);
                    if (oldGroup.Id != (int)BuiltInGroupId.Admin && ((RMSOPermissionMasks)oldGroup.SOPermissionMasks).UserHasThisPermission(RMSOPermissionMasks.RestoreCenterSearch))
                    {
                        oldGroup.SecurityGroupControlType = SecurityGroupControlType.FunctionModule;
                        if (((RMSOPermissionMasks)oldGroup.SOPermissionMasks).UserHasThisPermission(RMSOPermissionMasks.RestoreCenterFullControl))
                        {
                            oldGroup.FunctionSubPermission = FunctionSubPermission.RestoreCenterFullControl;
                        }
                        else if (((RMSOPermissionMasks)oldGroup.SOPermissionMasks).UserHasThisPermission(RMSOPermissionMasks.RestoreCenterExport))
                        {
                            oldGroup.FunctionSubPermission = FunctionSubPermission.RestoreCenterExport;
                        }
                        else if (((RMSOPermissionMasks)oldGroup.SOPermissionMasks).UserHasThisPermission(RMSOPermissionMasks.RestoreCenterSearch))
                        {
                            oldGroup.FunctionSubPermission = FunctionSubPermission.RestoreCenterSearch;
                        }
                    }
                    bool showCommonInfo = (int)BuiltInGroupId.EndUser != newGroup.Id && !IsBuiltInReviewUserGroup(oldGroup);
                    if (showCommonInfo)
                    {
                        #region group name item
                        AuditItem groupNameItem = new AuditItem
                        {
                            TargetSetting = "RM_CP_AM_Table_Column_GroupName",
                            OldValue = oldGroup.Name,
                            NewValue = newGroup.Name
                        };
                        info.ModifyContent.Add(groupNameItem);
                        #endregion

                        #region group desc item
                        AuditItem groupDescItem = new AuditItem
                        {
                            TargetSetting = "RM_CP_AM_Table_Column_Desc",
                            OldValue = oldGroup.Description,
                            NewValue = newGroup.Description
                        };
                        info.ModifyContent.Add(groupDescItem);
                        #endregion
                    }

                    #region group members item
                    AuditItem groupMemberItem = new AuditItem
                    {
                        TargetSetting = "RM_CP_AM_Table_Column_MembersName",
                        OldValue = GetGroupMemberNames(oldGroup.Users),
                        NewValue = GetGroupMemberNames(newGroup.Users)
                    };
                    info.ModifyContent.Add(groupMemberItem);
                    #endregion

                    if (showCommonInfo)
                    {
                        AuditItem permissionSettings = new AuditItem
                        {
                            TargetSetting = "RM_CP_AM_Table_Column_PermissionGroupName",
                            NewValue = newGroup.SecurityGroupControlType switch
                            {
                                SecurityGroupControlType.DataScope => "RM_CP_AM_Permission_DataScope",
                                SecurityGroupControlType.FunctionModule => "RM_CP_AM_Permission_FunctionMoudle",
                                _ => ""
                            },
                            OldValue = oldGroup.SecurityGroupControlType switch
                            {
                                SecurityGroupControlType.DataScope => "RM_CP_AM_Permission_DataScope",
                                SecurityGroupControlType.FunctionModule => "RM_CP_AM_Permission_FunctionMoudle",
                                _ => ""
                            }
                        };
                        info.ModifyContent.Add(permissionSettings);
                        if (oldGroup.SecurityGroupControlType == SecurityGroupControlType.FunctionModule && newGroup.SecurityGroupControlType == SecurityGroupControlType.FunctionModule)
                        {
                            AuditItem restoreCenter = new AuditItem
                            {
                                TargetSetting = "RM_RC_Audit_Module_RestoreCenter",
                                NewValue = newGroup.FunctionSubPermission switch
                                {
                                    FunctionSubPermission.RestoreCenterSearch => "RM_CP_AM_SubPermission_SearchOnly",
                                    FunctionSubPermission.RestoreCenterExport => "RM_CP_AM_SubPermission_SearchAndExport",
                                    FunctionSubPermission.RestoreCenterFullControl => "RM_CP_AM_SubPermission_FullControl",
                                    _ => ""
                                },
                                OldValue = oldGroup.FunctionSubPermission switch
                                {
                                    FunctionSubPermission.RestoreCenterSearch => "RM_CP_AM_SubPermission_SearchOnly",
                                    FunctionSubPermission.RestoreCenterExport => "RM_CP_AM_SubPermission_SearchAndExport",
                                    FunctionSubPermission.RestoreCenterFullControl => "RM_CP_AM_SubPermission_FullControl",
                                    _ => ""
                                }
                            };
                            info.ModifyContent.Add(restoreCenter);
                        }
                        else
                        {
                            #region group scope and containers
                            SecurityGroupDto tempOldGroup = oldGroup.SecurityGroupControlType == SecurityGroupControlType.FunctionModule ? null: oldGroup;
                            SecurityGroupDto tempNewGroup = newGroup.SecurityGroupControlType == SecurityGroupControlType.FunctionModule ? null : newGroup;
                            if (RMKeyValueDao.HasUpgradeTeams())
                            {
                                await AddScopeAndContainersAuditAsync(info, tempOldGroup, tempNewGroup, SourceFlag.Teams, "RM_CP_AM_IsCheckeTeamsAndGroupScope");
                            }
                            await AddScopeAndContainersAuditAsync(info, tempOldGroup, tempNewGroup, SourceFlag.SharePoint, "RM_CP_AM_IsCheckdSPScope");
                            await AddScopeAndContainersAuditAsync(info, tempOldGroup, tempNewGroup, SourceFlag.OneDrive, "RM_CP_AM_IsCheckdOneDriveScope");
                            await AddScopeAndContainersAuditAsync(info, tempOldGroup, tempNewGroup, SourceFlag.Exchange, "RM_CP_AM_IsCheckdEXOScope");
                            await AddScopeAndContainersAuditAsync(info, tempOldGroup, tempNewGroup, SourceFlag.Physical, "RM_CP_AM_IsCheckdPhysicalScope");
                            await AddScopeAndContainersAuditAsync(info, tempOldGroup, tempNewGroup, SourceFlag.FileSystem, "RM_CP_AM_IsCheckdFileSystemScope");
                            await AddScopeAndContainersAuditAsync(info, tempOldGroup, tempNewGroup, SourceFlag.SharePointOnPrem, "RM_CP_AM_IsCheckdSPLocalScope");
                            await AddScopeAndContainersAuditAsync(info, tempOldGroup, tempNewGroup, SourceFlag.AzureFileShare, "RM_CP_AM_IsCheckdAzureFileScope");
                            await AddScopeAndContainersAuditAsync(info, tempOldGroup, tempNewGroup, SourceFlag.Box, "RM_CP_AM_IsCheckedBoxScope");
                            if(TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, Contract.RoleAssignments.PaidForProduct.OpusGoogle))
                            {
                                await AddScopeAndContainersAuditAsync(info, oldGroup, newGroup, SourceFlag.Google, "RM_CP_AM_IsCheckedGoogleScope");
                            }
                            #endregion
                            if (tempOldGroup == null)
                            {
                                AuditItem oldrestoreCenter = new AuditItem
                                {
                                    TargetSetting = "RM_RC_Audit_Module_RestoreCenter",
                                    OldValue = oldGroup.FunctionSubPermission switch
                                    {
                                        FunctionSubPermission.RestoreCenterSearch => "RM_CP_AM_SubPermission_SearchOnly",
                                        FunctionSubPermission.RestoreCenterExport => "RM_CP_AM_SubPermission_SearchAndExport",
                                        FunctionSubPermission.RestoreCenterFullControl => "RM_CP_AM_SubPermission_FullControl",
                                        _ => ""
                                    }
                                };
                                info.ModifyContent.Add(oldrestoreCenter);
                            }
                            if (tempNewGroup == null)
                            {
                                AuditItem newrestoreCenter = new AuditItem
                                {
                                    TargetSetting = "RM_RC_Audit_Module_RestoreCenter",
                                    NewValue = newGroup.FunctionSubPermission switch
                                    {
                                        FunctionSubPermission.RestoreCenterSearch => "RM_CP_AM_SubPermission_SearchOnly",
                                        FunctionSubPermission.RestoreCenterExport => "RM_CP_AM_SubPermission_SearchAndExport",
                                        FunctionSubPermission.RestoreCenterFullControl => "RM_CP_AM_SubPermission_FullControl",
                                        _ => ""
                                    },
                                };
                                info.ModifyContent.Add(newrestoreCenter);
                            }
                        }

                    }

                    if ((int)BuiltInGroupId.EndUser == newGroup.Id)
                    {
                        await AddScopeAndContainersAuditAsync(info, oldGroup, newGroup, SourceFlag.Physical, "RM_CP_AM_IsCheckdPhysicalScope");
                    }

                    bool showTermAndRule = !IsBuiltInReviewUserGroup(oldGroup);
                    if (showTermAndRule)
                    {
                        SecurityGroupDto tempOldGroup = oldGroup.SecurityGroupControlType == SecurityGroupControlType.FunctionModule ? null : oldGroup;
                        SecurityGroupDto tempNewGroup = newGroup.SecurityGroupControlType == SecurityGroupControlType.FunctionModule ? null : newGroup;
                        AuditItem enableTrimItem = new AuditItem
                        {
                            TargetSetting = I18NEntity.GetString("RM_CP_AM_Permission_DelegateTitle"),
                            NewValue = tempNewGroup == null ? "" : newGroup.IsEnableTrim ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No"),
                            OldValue = tempOldGroup == null ? "" : oldGroup.IsEnableTrim ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No")
                        };
                        info.ModifyContent.Add(enableTrimItem);

                        #region group term permission settings
                        AddTermPermissionAudit(info, oldGroup, newGroup);
                        #endregion

                        #region rule permission settings
                        AddRulePermissionAudit(info, oldGroup, newGroup);
                        #endregion
                    }
                    if (newGroup.SecurityGroupControlType == SecurityGroupControlType.DataScope || oldGroup.SecurityGroupControlType == SecurityGroupControlType.DataScope)
                    {
                        AuditItem enableReportPermission = new AuditItem
                        {
                            TargetSetting = I18NEntity.GetString("RM_CP_AM_Report_Permission_SpecificReport"),
                            NewValue = newGroup.IsUseReportingPermissionControl ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No"),
                            OldValue = oldGroup.IsUseReportingPermissionControl ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No"),
                        };
                        info.ModifyContent.Add(enableReportPermission);

                        AuditItem reportPermission = new AuditItem
                        {
                            TargetSetting = I18NEntity.GetString("RM_CP_AM_Report"),
                            NewValue = GenerateReportPermissionString(newGroup.ReportingPermission),
                            OldValue = GenerateReportPermissionString(oldGroup.ReportingPermission),
                        };
                        info.ModifyContent.Add(reportPermission);
                    }

                    if (showTermAndRule && (LicenseHelperService.HasOpusILLicense || LicenseHelperService.HasOpusGoogleLicense))
                    {
                        AddManageHoldPermissionAudit(info, newGroup, oldGroup);
                        AddManualApprovalSettingPermissionAudit(info, newGroup, oldGroup);
                    }
                }
                else if (category == (int)AuditCategory.CSDConfigApiKey)
                {
                    if (action == (int)AuditAction.CSDAddApiKey)
                    {
                        var keyName = args[0]?.ToString();
                        var keyExpired = await GeneralSettingService.ConvertDateTimeToUtcAsync(DateTime.Parse(args[1]?.ToString()));
                        var keyOperator = args[2]?.ToString();
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = I18NEntity.GetString("RM_JS_CP_CSDAK_KeyName"),
                            NewValue = keyName
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = I18NEntity.GetString("RM_JS_CP_CSDAK_KeyExpired"),
                            NewValue = (await GeneralSettingService.ConvertTiksToDateTimeAsync(keyExpired.Ticks, true)).FormaTime
                        });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = I18NEntity.GetString("RM_JS_CP_CSDAK_KeyOperator"),
                            NewValue = keyOperator
                        });
                    }
                    else if (action == (int)AuditAction.CSDEditApiKey)
                    {
                        var keyId = int.Parse(args[0]?.ToString());
                        var keyName = args[1]?.ToString();
                        var keyExpired = await GeneralSettingService.ConvertDateTimeToUtcAsync(DateTime.Parse(args[2]?.ToString()));
                        var keyOperator = args[3]?.ToString();
                        var oldKey = ApiKeyDao.GetApiKey(keyId);
                        var keyNameChanged = keyName != oldKey?.Name;
                        if (keyNameChanged)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = I18NEntity.GetString("RM_JS_CP_CSDAK_KeyName"),
                                OldValue = oldKey?.Name,
                                NewValue = keyName
                            });
                        }
                        else
                        {
                            info.Object = oldKey?.Name;
                        }
                        if (keyExpired.Ticks != oldKey?.Expired)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = I18NEntity.GetString("RM_JS_CP_CSDAK_KeyExpired"),
                                OldValue = oldKey == null ? "" : (await GeneralSettingService.ConvertTiksToDateTimeAsync(oldKey.Expired, true)).FormaTime,
                                NewValue = (await GeneralSettingService.ConvertTiksToDateTimeAsync(keyExpired.Ticks, true)).FormaTime
                            });
                        }
                        if (keyOperator != oldKey?.OperatorLoginName)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = I18NEntity.GetString("RM_JS_CP_CSDAK_KeyOperator"),
                                OldValue = oldKey == null ? "" : oldKey.OperatorLoginName,
                                NewValue = keyOperator
                            });
                        }
                    }
                    else if (action == (int)AuditAction.CSDDeleteApiKey)
                    {
                        IEnumerable<int> ids = args[0] as IEnumerable<int>;
                        var removeKeys = ApiKeyDao.GetApiKeys(ids);
                        info.Object = string.Join(", ", removeKeys.Select(k => k.Name));
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
            finally
            {

            }
            return info;
        }
        private string GenerateReportPermissionString(long permissionMask)
        {
            string result = string.Empty;

            if ((permissionMask & (int)RMReportPermissionMasks.ContentDueForActionEnduser)== (int)RMReportPermissionMasks.ContentDueForActionEnduser)
            {
                result = string.Concat(result, ";", I18NEntity.GetString("RM_CP_AM_Report_Permission_SpecificReport_Option01"));
            }
            if ((permissionMask & (int)RMReportPermissionMasks.TermUsageEnduser) == (int)RMReportPermissionMasks.TermUsageEnduser)
            {
                result = string.Concat(result, ";", I18NEntity.GetString("RM_CP_AM_Report_Permission_SpecificReport_Option02"));
            }
            if ((permissionMask & (int)RMReportPermissionMasks.RuleUsageEnduser) == (int)RMReportPermissionMasks.RuleUsageEnduser)
            {
                result = string.Concat(result, ";", I18NEntity.GetString("RM_CP_AM_Report_Permission_SpecificReport_Option03"));
            }
            if ((permissionMask & (int)RMReportPermissionMasks.CreationAndDestructionEnduser) == (int)RMReportPermissionMasks.CreationAndDestructionEnduser)
            {
                result = string.Concat(result, ";", I18NEntity.GetString("RM_CP_AM_Report_Permission_SpecificReport_Option04"));
            }
            if ((permissionMask & (int)RMReportPermissionMasks.ActionAuditEnduser) == (int)RMReportPermissionMasks.ActionAuditEnduser)
            {
                result = string.Concat(result, ";", I18NEntity.GetString("RM_CP_AM_Report_Permission_SpecificReport_Option05"));
            }
            if ((permissionMask & (int)RMReportPermissionMasks.RestoredDataEnduser) == (int)RMReportPermissionMasks.RestoredDataEnduser)
            {
                result = string.Concat(result, ";", I18NEntity.GetString("RM_CP_AM_Report_Permission_SpecificReport_Option06"));
            }
            if((permissionMask & (int)RMReportPermissionMasks.AvailableSpaceEndUser) == (int)RMReportPermissionMasks.AvailableSpaceEndUser)
            {
                result = string.Concat(result, ";", I18NEntity.GetString("RM_CP_AM_Report_Permission_SpecificReport_Option07"));
            }
            return result.TrimStart(';');
        }
        private bool EnableCheckSum()
        {
            var exportSignature = SettingProfileDao.LoadByType((int)SettingProfilesType.ExportSignatureInfo);
            if (exportSignature != null)
            {
                ExportSignatureInfo info = new ExportSignatureInfo();
                info = JsonSerializer.Deserialize<ExportSignatureInfo>(exportSignature.Settings);
                return info.EnableExportSignature;
            }
            else
            {
                return false;
            }
        }
        private string GetGroupMemberNames(List<AOSUserDto> users)
        {
            var groupMemberNames = "";
            if (users != null && users.Count > 0)
            {
                groupMemberNames = string.Join("; ", users.Select(o => o.DisplayName));
            }
            return groupMemberNames;
        }

        private async System.Threading.Tasks.Task AddScopeAndContainersAuditAsync(RMAuditInfo info, SecurityGroupDto oldGroup, SecurityGroupDto newGroup, SourceFlag source , string sourceTitle)
        {
            var oldScopeInfo = oldGroup?.DataSourceScopeInfo.Where(o => o.DataSourceType == source).FirstOrDefault();
            var newScopeInfo = newGroup?.DataSourceScopeInfo.Where(o => o.DataSourceType == source).FirstOrDefault();
            var oldScopeSelected = oldScopeInfo != null;
            var newScopeSelected = newScopeInfo != null;
            var isBuildInEndUserGroup = (int)BuiltInGroupId.EndUser == newGroup?.Id;
            AuditItem scopeItem = new AuditItem
            {
                TargetSetting = sourceTitle,
            };
            if (newGroup != null)
            {
                scopeItem.NewValue = newScopeSelected ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
            }
            if (oldGroup != null)
            {
                scopeItem.OldValue = oldScopeSelected ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
            }
            if (!isBuildInEndUserGroup) {
                info.ModifyContent.Add(scopeItem);
            }
            var allContainers = new List<SecurityContainerDto>();
            var oldSelectedContainerNames = "";
            var newSelectedContainerNames = "";
            switch (source)
            {
                case SourceFlag.SharePoint:
                case SourceFlag.Exchange:
                case SourceFlag.OneDrive:
                case SourceFlag.Teams:
                    if (oldScopeSelected || newScopeSelected)
                    {
                        allContainers = await SecurityGroupManagementService.GetContainersAsync(source);
                    }

                    if (oldScopeSelected)
                    {
                        if (oldScopeInfo.ScopeIds != null)
                        {
                            oldSelectedContainerNames = string.Join("; ", allContainers.Where(o => oldScopeInfo.ScopeIds.Contains(new Guid(o.Id))).Select(o => o.Name).ToList());
                        }
                    }

                    if (newScopeSelected)
                    {
                        if (newScopeInfo.ScopeIds != null)
                        {
                            newSelectedContainerNames = string.Join("; ", allContainers.Where(o => newScopeInfo.ScopeIds.Contains(new Guid(o.Id))).Select(o => o.Name).ToList());
                        }
                    }
                    AuditItem spContainersItem = new AuditItem
                    {
                        TargetSetting = "RM_CP_AM_Table_Column_PermissionScopeName",
                    };
                    if (newGroup != null)
                    {
                        spContainersItem.NewValue = !string.IsNullOrEmpty(newSelectedContainerNames) ? newSelectedContainerNames : "RM_RC_Audit_None";
                    }
                    if (oldGroup != null)
                    {
                        spContainersItem.OldValue = !string.IsNullOrEmpty(oldSelectedContainerNames) ? oldSelectedContainerNames : "RM_RC_Audit_None";
                    }
                    info.ModifyContent.Add(spContainersItem);
                    break;
               
                case SourceFlag.Physical:
                    if (!isBuildInEndUserGroup)
                    {
                        AuditItem phyContainersItem = new AuditItem
                        {
                            TargetSetting = "RM_CP_AM_Table_Column_PermissionScopeName",
                            
                        };
                        if (newGroup != null)
                        {
                            phyContainersItem.NewValue = newScopeSelected ? "RM_CP_AM_AllScope_Title" : "RM_RC_Audit_None";
                        }
                        if (oldGroup != null)
                        {
                            phyContainersItem.OldValue = oldScopeSelected ? "RM_CP_AM_AllScope_Title" : "RM_RC_Audit_None";
                        }

                        if (oldGroup != null && oldScopeSelected && oldScopeInfo.SubPermission == SubPermissionType.Admin)
                        {
                            if (oldScopeSelected)
                            {
                                allContainers = await SecurityGroupManagementService.GetContainersAsync(source);
                            }

                            oldSelectedContainerNames = "";

                            if (oldScopeSelected)
                            {
                                if (oldScopeInfo.ScopeIds != null)
                                {
                                    oldSelectedContainerNames = string.Join("; ", allContainers.Where(o => oldScopeInfo.ScopeIds.Contains(new Guid(o.Id))).Select(o => o.Name).ToList());
                                }
                            }

                            if (oldGroup != null)
                            {
                                phyContainersItem.OldValue = !string.IsNullOrEmpty(oldSelectedContainerNames) ? oldSelectedContainerNames : "RM_RC_Audit_None";
                            }
                        }

                        if(newGroup != null && newScopeSelected && newScopeInfo.SubPermission == SubPermissionType.Admin)
                        {
                            if (newScopeSelected && (allContainers == null || allContainers.Count == 0))
                            {
                                allContainers = await SecurityGroupManagementService.GetContainersAsync(source);
                            }

                            newSelectedContainerNames = "";
                            if (newScopeSelected)
                            {
                                if (newScopeInfo.ScopeIds != null)
                                {
                                    newSelectedContainerNames = string.Join("; ", allContainers.Where(o => newScopeInfo.ScopeIds.Contains(new Guid(o.Id))).Select(o => o.Name).ToList());
                                }
                            }

                            if (newGroup != null)
                            {
                                phyContainersItem.NewValue = !string.IsNullOrEmpty(newSelectedContainerNames) ? newSelectedContainerNames : "RM_RC_Audit_None";
                            }
                        }

                        info.ModifyContent.Add(phyContainersItem);

                        var oldPhyPermssionVal = "RM_RC_Audit_None";
                        if (oldScopeSelected)
                        {
                            oldPhyPermssionVal = oldScopeInfo.SubPermission == SubPermissionType.Admin ? "RM_CP_AM_PhysicalPermission_Admin" : "RM_CP_AM_PhysicalPermission_EndUser";
                        }
                        var newPhyPermssionVal = "RM_RC_Audit_None";
                        if (newScopeSelected)
                        {
                            newPhyPermssionVal = newScopeInfo.SubPermission == SubPermissionType.Admin ? "RM_CP_AM_PhysicalPermission_Admin" : "RM_CP_AM_PhysicalPermission_EndUser";
                        }
                        AuditItem phyPermissionItem = new AuditItem
                        {
                            TargetSetting = "RM_CP_AM_Table_Column_PermissionName",
                        };
                        if (oldGroup != null)
                        {
                            phyPermissionItem.OldValue = oldPhyPermssionVal;
                        }
                        if (newGroup != null)
                        {
                            phyPermissionItem.NewValue = newPhyPermssionVal;
                        }
                        info.ModifyContent.Add(phyPermissionItem);
                    }

                    if (oldGroup != null && oldScopeSelected && oldScopeInfo.SubPermission == SubPermissionType.EndUser)
                    {
                        AuditItem oldPhySubPermissionItem = new AuditItem
                        {
                            TargetSetting = "RM_CP_AM_Module_PhyExplorer_Permission_Title",
                            OldValue = GetSubPermissionNamesStr(oldScopeInfo.SubPermissions)
                        };
                        info.ModifyContent.Add(oldPhySubPermissionItem);
                    }
                    if (newScopeSelected && newScopeInfo.SubPermission == SubPermissionType.EndUser)
                    {
                        AuditItem phySubPermissionItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_CP_AM_Module_PhyExplorer_Permission_Title")).FirstOrDefault();
                        if (phySubPermissionItem != null)
                        {
                            phySubPermissionItem.NewValue = GetSubPermissionNamesStr(newScopeInfo.SubPermissions);
                        }
                        else
                        {
                            phySubPermissionItem = new AuditItem
                            {
                                TargetSetting = "RM_CP_AM_Module_PhyExplorer_Permission_Title",
                                NewValue = GetSubPermissionNamesStr(newScopeInfo.SubPermissions)
                            };
                            info.ModifyContent.Add(phySubPermissionItem);
                        }
                    }
                    break;
                case SourceFlag.FileSystem:
                case SourceFlag.SharePointOnPrem:
                case SourceFlag.AzureFileShare:
                case SourceFlag.Box:
                case SourceFlag.Google:
                    AuditItem containersItem = new AuditItem
                    {
                        TargetSetting = "RM_CP_AM_Table_Column_PermissionScopeName",
                    };
                    if (oldGroup != null)
                    {
                        containersItem.OldValue = oldScopeSelected ? "RM_CP_AM_AllScope_Title" : "RM_RC_Audit_None";
                    }
                    if (newGroup != null)
                    {
                        containersItem.NewValue = newScopeSelected ? "RM_CP_AM_AllScope_Title" : "RM_RC_Audit_None";
                    }
                    info.ModifyContent.Add(containersItem);
                    break;
            }
        }

        private string GetSubPermissionNamesStr(List<SubPermission> subPermissions)
        {
            var resultNames = "";
            List<string> names = new List<string>();
            foreach (var subPermission in subPermissions)
            {
                switch (subPermission)
                {
                    case SubPermission.None:
                        break;
                    case SubPermission.SetAccessControl:
                        names.Add(I18NEntity.GetString("RM_CP_AM_Phy_SubPermission_SetAccessControl"));
                        break;
                    case SubPermission.BoxCreationRequest:
                        names.Add(I18NEntity.GetString("RM_CP_AM_Phy_SubPermission_BoxCreationRequest"));
                        break;
                    case SubPermission.FolderCreationRequest:
                        names.Add(I18NEntity.GetString("RM_CP_AM_Phy_SubPermission_FolderCreationRequest"));
                        break;
                    case SubPermission.FolderLoanRequest:
                        names.Add(I18NEntity.GetString("RM_CP_AM_Phy_SubPermission_FolderLoanRequest"));
                        break;
                    case SubPermission.FolderLoanReturn:
                        names.Add(I18NEntity.GetString("RM_CP_AM_Phy_SubPermission_FolderLoanReturn"));
                        break;
                    default:
                        break;
                }
            }
            if (names.Count > 0)
            {
                resultNames = string.Join("\n", names);
            }
            return resultNames;
        }

        private void AddTermPermissionAudit(RMAuditInfo info, SecurityGroupDto oldGroup, SecurityGroupDto newGroup)
        {
            if (LicenseHelperService.HasOpusILLicense)
            {
                AuditItem item = new AuditItem
                {
                    TargetSetting = I18NEntity.GetString("RM_CP_AM_TermPermission_Title"),
                };
                if (newGroup.IsEnableTrim)
                {
                    item.NewValue = GetTermSettings(newGroup);
                }
                if (oldGroup != null && oldGroup.IsEnableTrim)
                {
                    item.OldValue = GetTermSettings(oldGroup);
                }

                info.ModifyContent.Add(item);
            }
        }
        private string GetTermSettings(SecurityGroupDto group)
        {
            var treeNodeInfo = group.TermTreeNodeInfo;
            var result = "";
            if (group.SetTermPermissionMethod == TermPermissionMethod.All)
            {

                result = I18NEntity.GetString("RM_CP_AM_TermPermission_AllPermission_Msg");
            }
            else if (group.SetTermPermissionMethod == TermPermissionMethod.SpecifyScope)
            {
                List<string> hasPermissionTermPath = new List<string>();
                var termGroupNodes = treeNodeInfo.SubTerms;
                if (termGroupNodes != null)
                {
                    foreach (var tGroup in termGroupNodes)
                    {
                        if (tGroup.IsChecked)
                        {
                            hasPermissionTermPath.Add(tGroup.Name);
                        }
                        else
                        {
                            var termSetNodes = tGroup.SubTerms;
                            if (termSetNodes != null)
                            {
                                foreach (var tSet in termSetNodes)
                                {
                                    if (tSet.IsChecked)
                                    {
                                        hasPermissionTermPath.Add($"{tGroup.Name}/{tSet.Name}");
                                    }
                                }
                            }
                        }
                    }
                    if (hasPermissionTermPath.Count > 0)
                    {
                        result = string.Join("\n", hasPermissionTermPath);
                    }
                }
            }
            else if (group.SetTermPermissionMethod == TermPermissionMethod.None)
            {
                result = result = I18NEntity.GetString("RM_CP_AM_TermPermission_NoPermission_Msg");
            }
            return result;
        }

        private void AddRulePermissionAudit(RMAuditInfo info, SecurityGroupDto oldGroup, SecurityGroupDto newGroup)
        {
            AuditItem item = new AuditItem
            {
                TargetSetting = I18NEntity.GetString("RM_CP_AM_RulePermission_RuleTitle"),
            };
            if (newGroup.IsEnableTrim)
            {
                item.NewValue = GetRuleSettings(newGroup);
            }
            if (oldGroup != null && oldGroup.IsEnableTrim)
            {
                item.OldValue = GetRuleSettings(oldGroup);
            }

            info.ModifyContent.Add(item);
        }

        private void AddManageHoldPermissionAudit(RMAuditInfo info, SecurityGroupDto newGroup, SecurityGroupDto oldGroup = null)
        {
            AuditItem item = new AuditItem
            {
                TargetSetting = I18NEntity.GetString("RM_CP_AM_ManageHolds"),
                NewValue = newGroup.IsEnableManageHold ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No")
            };
            if (oldGroup != null)
            {
                item.OldValue = oldGroup.IsEnableManageHold ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
            }
            info.ModifyContent.Add(item);
        }

        private void AddManualApprovalSettingPermissionAudit(RMAuditInfo info, SecurityGroupDto newGroup, SecurityGroupDto oldGroup = null)
        {
            AuditItem item = new AuditItem
            {
                TargetSetting = I18NEntity.GetString("RM_CP_AM_ManageApprovalSettings"),
                NewValue = newGroup.IsEnableApprovalSetting ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No")
            };
            if (oldGroup != null)
            {
                item.OldValue = oldGroup.IsEnableApprovalSetting ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
            }
            info.ModifyContent.Add(item);
        }

        private string GetRuleSettings(SecurityGroupDto group)
        {
            var treeNodeInfo = group.RuleTreeNodeInfo;
            var result = "";
            if (group.SetRulePermissionMethod == RulePermissionMethod.All)
            {

                result = I18NEntity.GetString("RM_CP_AM_RulePermission_AllRuleTitle");
            }
            else if (group.SetRulePermissionMethod == RulePermissionMethod.SpecifyScope)
            {
                List<string> hasPermissionRules = new List<string>();
                var ruleContainerNodes = treeNodeInfo.SubItems;
                if (ruleContainerNodes != null)
                {
                    foreach (var ruleContainer in ruleContainerNodes)
                    {
                        var ruleContainerName = I18NEntity.GetString(ruleContainer.Name);
                        if (ruleContainer.IsChecked)
                        {
                            hasPermissionRules.Add(ruleContainerName);
                        }
                        else
                        {
                            var ruleNodes = ruleContainer.SubItems;
                            if (ruleNodes != null)
                            {
                                foreach (var ruleNode in ruleNodes)
                                {
                                    if (ruleNode.IsChecked)
                                    {
                                        hasPermissionRules.Add($"{ruleContainerName}/{ruleNode.Name}");
                                    }
                                }
                            }
                        }
                    }
                    if (hasPermissionRules.Count > 0)
                    {
                        result = string.Join("\n", hasPermissionRules);
                    }
                }
            }
            else if (group.SetRulePermissionMethod == RulePermissionMethod.None)
            {
                //TODO Cyrus RM_CP_AM_TermPermission_NoPermission_Msg -> Rule
                result = result = I18NEntity.GetString("RM_CP_AM_TermPermission_NoPermission_Msg");
            }
            return result;
        }

        private bool IsBuiltInReviewUserGroup(SecurityGroupDto dto)
        {
            if (dto.IsBuiltInGroup && dto.Id != (int)BuiltInGroupId.EndUser && dto.Id != (int)BuiltInGroupId.Admin)
            {
                return true;
            }
            return false;
        }
    }
}
