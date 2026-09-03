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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.I18N.Core;
using DocumentFormat.OpenXml.Vml.Office;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ControlPanel.AuditHandler
{
    public class GlobalSettingAfterAuditHandler : IAfterAuditHandler
    {
        public IExportSettingService exportSettingService => PlatformWindsorManager.GetService<IExportSettingService>();
        public ISecurityGroupManagementService SecurityGroupManagementService => PlatformWindsorManager.GetService<ISecurityGroupManagementService>();
        private IRMKeyValueDao  RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private ISettingProfilesDao SettingProfileDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();
        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            RMAuditInfo auditInfo = new RMAuditInfo();
            auditInfo.Object = string.Empty;
            auditInfo.Module = (AuditModule)model;
            auditInfo.Category = (AuditCategory)category;
            auditInfo.Action = (AuditAction)action;
            if (action == (int)AuditAction.ConfigureDocAveConnection)
            {
                bool returnMessage = (bool)returnValue;
                auditInfo.Status = returnMessage ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;

                DovAveConnSetting newDovAveConnSetting = args[0] as DovAveConnSetting;
                if (info.ModifyContent != null && info.ModifyContent.Count != 0)
                {
                    AuditItem hostItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("DocAveControlServiceHost")).FirstOrDefault();
                    if (hostItem != null) { hostItem.NewValue = newDovAveConnSetting.DocAve_Host; }
                    AuditItem userNameItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("UserName")).FirstOrDefault();
                    if (userNameItem != null) { userNameItem.NewValue = newDovAveConnSetting.DocAve_Username; }
                    AuditItem passwordItem = new AuditItem();
                    passwordItem.TargetSetting = "Password Changed";
                    passwordItem.NewValue = newDovAveConnSetting.PasswordChanged ? "Yes" : "No";
                    info.ModifyContent.Add(passwordItem);
                }
            }
            else if (action == (int)AuditAction.ConfigureGlobalsettings)
            {
                bool returnMessage = (bool)returnValue;
                auditInfo.Status = returnMessage ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;

                string auditorNone = "RM_JS_RDM_CreateRule_ExportType_None";

                GlobalStorageSetting newGlobalSettings = args[0] as GlobalStorageSetting;
                if (info.ModifyContent != null && info.ModifyContent.Count != 0)
                {
                    AuditItem dataStoreLocation = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_CP_GSS_Audit_DataStoreLocation")).FirstOrDefault();
                    if (dataStoreLocation != null)
                    {
                        dataStoreLocation.NewValue = newGlobalSettings != null && newGlobalSettings.CurrentStoragePolicy != null ? newGlobalSettings.CurrentStoragePolicy.Name : string.Empty;
                    }
                    AuditItem exportLocation = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_CP_GSS_Audit_ExportLocation")).FirstOrDefault();
                    if (exportLocation != null)
                    {
                        exportLocation.NewValue = newGlobalSettings != null && newGlobalSettings.CurrentExportLocation != null ? newGlobalSettings.CurrentExportLocation.Name : string.Empty;
                    }
                    //AuditItem processingPool = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("Processing Pool")).FirstOrDefault();
                    //if (processingPool != null)
                    //{
                    //    processingPool.NewValue = newGlobalSettings != null && newGlobalSettings.CurrentProcessingPool != null ? newGlobalSettings.CurrentProcessingPool.Name : string.Empty;
                    //}

                    AuditItem dataCompression = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_CP_GSS_DataHandle_Compression")).FirstOrDefault();
                    if (dataCompression != null)
                    {
                        var compressionMethod = "RM_CP_GSS_DataHandle_Compression";
                        dataCompression.NewValue = newGlobalSettings != null && newGlobalSettings.UseCompression ? compressionMethod : auditorNone;
                    }
                    AuditItem compressionLevel = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_CP_GSS_Audit_CompressionLevel")).FirstOrDefault();
                    if (compressionLevel != null)
                    {
                        var compressionSpeed = newGlobalSettings != null ? newGlobalSettings.CompressionSpeed.ToString() : auditorNone;
                        compressionLevel.NewValue = newGlobalSettings != null && newGlobalSettings.UseCompression ? compressionSpeed : auditorNone;
                    }

                    AuditItem dataEncryption = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_CP_GSS_DataHandle_Encryption")).FirstOrDefault();
                    if (dataEncryption != null)
                    {
                        var encryptionMethod = "RM_CP_GSS_DataHandle_Encryption";
                        dataEncryption.NewValue = newGlobalSettings != null && newGlobalSettings.UseEncryption ? encryptionMethod : auditorNone;
                    }
                    AuditItem encryptionSecurityProfileItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_CP_GSS_SecurityProfile")).FirstOrDefault();
                    if (encryptionSecurityProfileItem != null)
                    {
                        var securityProfile = newGlobalSettings != null && newGlobalSettings.CurrentSecurityProfile != null ? newGlobalSettings.CurrentSecurityProfile.Name : string.Empty;
                        encryptionSecurityProfileItem.NewValue = newGlobalSettings != null && newGlobalSettings.UseEncryption ? securityProfile : auditorNone;
                    }

                }
            }
            else if (action == (int)AuditAction.ConfigureExportSetting || action == (int)AuditAction.CompliantExport)
            {
                var newValue = new StringBuilder();

                var veofilename = exportSettingService.GetConfigureFileName(ExportSettingType.VEO);
                if (!string.IsNullOrEmpty(veofilename))
                {
                    newValue.AppendFormat("VEO:{0}", veofilename);
                }

                var nnafilename = exportSettingService.GetConfigureFileName(ExportSettingType.NAA);
                if (!string.IsNullOrEmpty(nnafilename))
                {
                    if (!string.IsNullOrEmpty(newValue.ToString()))
                    {
                        newValue.AppendFormat("<br>NAA:{0}", nnafilename);
                    }
                    else
                    {
                        newValue.AppendFormat("NAA:{0}", nnafilename);
                    }
                }
                var narafilename = exportSettingService.GetConfigureFileName(ExportSettingType.NARA);
                if (!string.IsNullOrEmpty(narafilename))
                {
                    if (!string.IsNullOrEmpty(newValue.ToString()))
                    {
                        newValue.AppendFormat("<br>NARA:{0}", narafilename);
                    }
                    else
                    {
                        newValue.AppendFormat("NARA:{0}", narafilename);
                    }
                }

                var exportEncryptionEnabled = RMKeyValueDao.IsExportDataEncryptionEnabled();
                var enableStatusStr = exportEncryptionEnabled ? I18NEntity.GetString("RM_JS_Common_Enabled") : I18NEntity.GetString("RM_JS_Common_Disabled");
                if (!string.IsNullOrEmpty(newValue.ToString()))
                {
                    newValue.AppendFormat("<br>{0}", string.Format(I18NEntity.GetString("RM_RC_Audit_Action_ExportEncryptionEnabled"), enableStatusStr));
                }
                else
                {
                    newValue.AppendFormat(string.Format(I18NEntity.GetString("RM_RC_Audit_Action_ExportEncryptionEnabled"), enableStatusStr));
                }
                var enableCheckSum = EnableCheckSum() ? I18NEntity.GetString("RM_JS_Common_Enabled") : I18NEntity.GetString("RM_JS_Common_Disabled");
                newValue.AppendFormat("<br>{0}", string.Format(I18NEntity.GetString("RM_RC_Audit_Action_ExportDataCheckSumEnabled"), enableCheckSum));
                if (info.ModifyContent != null && info.ModifyContent.Count != 0)
                {
                    AuditItem exportconfiguration = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_RC_Audit_Configuration_File")).FirstOrDefault();
                    if (exportconfiguration != null)
                    {
                        exportconfiguration.NewValue = newValue.ToString();
                    }
                }
                if(returnValue is RAReturnMessage)
                {
                    var returnMessage = returnValue as RAReturnMessage;
                    auditInfo.Status = returnMessage.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                }                
                //info.Object = newValue.ToString();
            }
            else if (action == (int)AuditAction.ConfigureDedupScheduleJob)
            {
                var auditItem = info.ModifyContent.FirstOrDefault();
                if(auditItem != null)
                {
                    string fileName = args[0]?.ToString();
                    var stream = args[1] as Stream;
                    if(stream != null)
                    {
                        auditItem.NewValue = $"{fileName} {(stream.Length / 1024.0).ToString("f2")} (KB)";
                    }
                }
            }
            else if (action == (int)AuditAction.GenerateExportEncryptionKey)
            {
                var message = (RAReturnMessage)returnValue;
                auditInfo.Status = message != null && message.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
            }
            else if (action == (int)AuditAction.DeleteExportSetting)
            {
                var filePath = returnValue as string;
                if (!string.IsNullOrEmpty(filePath))
                {
                    auditInfo.Object = Path.GetFileName(filePath);
                }
            }
            else if (action == (int)AuditAction.DownloadTemplate)
            {
                HttpResponseMessage result = new HttpResponseMessage();
                result = (HttpResponseMessage)returnValue;
                auditInfo.Status = result.ReasonPhrase == "OK" ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                string fileName = args[0].ToString();
                if (fileName.StartsWith("VEO"))
                {
                    auditInfo.Object = I18NEntity.GetString("RM_ES_ExportType_VEO");
                }
                else if (fileName.StartsWith("NAA"))
                {
                    auditInfo.Object = I18NEntity.GetString("RM_ES_ExportType_NAA");
                }
                else if (fileName.StartsWith("NARA"))
                {
                    auditInfo.Object = I18NEntity.GetString("RM_ES_ExportType_NARA");
                }
            }
            else if (action == (int)AuditAction.CreateSecurityGroup)
            {
                var groupDto = args[0] as SecurityGroupDto;
                auditInfo.Object = groupDto.Name;
                var returnMessage = returnValue as RAReturnMessage;
                auditInfo.Status = returnMessage.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
            }
            else if (action == (int)AuditAction.EditSecurityGroup)
            {
                var groupDto = args[0] as SecurityGroupDto;
                auditInfo.Object = groupDto.Name;
                var returnMessage = returnValue as RAReturnMessage;
                auditInfo.Status = returnMessage.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
            }
            else if (action == (int)AuditAction.DeleteSecurityGroup)
            {
                bool returnMessage = (bool)returnValue;
                auditInfo.Status = returnMessage ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                var groupId = Convert.ToInt32(args[0]);
                var groupInfo = SecurityGroupManagementService.GetSimpleGroup(groupId);
                auditInfo.Object = groupInfo.Name;
            }

            if (info != null && info.ModifyContent != null)
            {
                auditInfo.ModifyContent = info.ModifyContent;
                if (!string.IsNullOrEmpty(info.Object))
                {
                    auditInfo.Object = info.Object;
                }
            }
            return auditInfo;
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
    }
}
