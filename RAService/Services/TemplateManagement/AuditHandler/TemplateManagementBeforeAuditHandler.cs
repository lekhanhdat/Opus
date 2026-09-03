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
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.TemplateManagement.Barcode;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.TemplateManagement.AuditHandler
{
    public class TemplateManagementBeforeAuditHandler : IBeforeAuditHandler
    {
        private ITemplateManagementService TemplateManagementService => PlatformWindsorManager.GetService<ITemplateManagementService>();

        private IBarcodeTemplateService BarcodeTemplateService => PlatformWindsorManager.GetService<IBarcodeTemplateService>();

        private IRMCustomBarcodeTemplateSuiteDao BarcodeTemplateSuiteDao => PlatformWindsorManager.GetService<IRMCustomBarcodeTemplateSuiteDao>();

        private Dictionary<string, string> BuildInColumnName = new Dictionary<string, string>()
        {
            {BuildInColumnIDs.RecordsId.ToString(), "RM_PRM_PRE_Column_ID"},
            {BuildInColumnIDs.CreatedBy.ToString(), "RM_PRM_PRE_Column_Creator"},
            {BuildInColumnIDs.CreatedTime.ToString(), "RM_PRM_PRE_Column_CreatedTime"},
            {BuildInColumnIDs.ModifiedBy.ToString(), "RM_PRM_PRE_Column_Modifier"},
            {BuildInColumnIDs.ModifiedTime.ToString(), "RM_PRM_PRE_Column_ModifiedTime"}
        };

        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            var info = new RMAuditInfo();
            info.ModifyContent = new List<AuditItem>();

            info.Module = (AuditModule)model;
            info.Category = (AuditCategory)category;
            info.Action = (AuditAction)action;
            if (action == (int)AuditAction.UpdateSuite)
            {
                SuiteDto dto = (SuiteDto)args[0];
                info.Object = I18NEntity.GetString(dto.Name);
                var oldSuite = TemplateManagementService.LoadSuite(dto.UniqueId);

                AuditItem suiteNameAuditItem = new AuditItem();
                suiteNameAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_Suite_Name");
                suiteNameAuditItem.NewValue = dto.Name;
                suiteNameAuditItem.OldValue = I18NEntity.GetString(oldSuite.Name);

                AuditItem descriptAuditItem = new AuditItem();
                descriptAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_TemplateDesc");
                descriptAuditItem.NewValue = dto.Description;
                descriptAuditItem.OldValue = oldSuite.Description;

                AuditItem startFromAuditItem = new AuditItem();
                startFromAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_Suite_StartFromTitle");
                string newStartFromTypeString = GetStartFromTypeString(dto.StartFromType);
                string oldStartFromTypeString = GetStartFromTypeString(oldSuite.StartFromType);
                startFromAuditItem.NewValue = newStartFromTypeString;
                startFromAuditItem.OldValue = oldStartFromTypeString;

                info.ModifyContent.Add(suiteNameAuditItem);
                info.ModifyContent.Add(descriptAuditItem);
                info.ModifyContent.Add(startFromAuditItem);
            }
            else if (action == (int)AuditAction.DeleteSuite)
            {
                Guid suiteUniqueId = (Guid)args[0];
                var suite = TemplateManagementService.LoadSuite(suiteUniqueId);
                info.Object = I18NEntity.GetString(suite.Name);
            }
            else if (action == (int)AuditAction.DeleteTemplate)
            {
                Guid templateUniqueId = (Guid)args[0];
                var template = await TemplateManagementService.LoadTemplateDtoAsync(templateUniqueId);
                info.Object = I18NEntity.GetString(template.name);
                switch (template.type)
                {
                    case Contract.TemplateManagement.TemplateType.Box:
                        info.Action = AuditAction.DeleteBoxTemplate;
                        break;
                    case Contract.TemplateManagement.TemplateType.Folder:
                        info.Action = AuditAction.DeleteFolderTemplate;
                        break;
                    case Contract.TemplateManagement.TemplateType.Records:
                        info.Action = AuditAction.DeleteRecordTemplate;
                        break;
                    default:
                        info.Action = AuditAction.DeleteTemplate;
                        break;
                }
            }
            else if (action == (int)AuditAction.UpdateGlobalUniqueId)
            {
                var dbSettings = TemplateManagementService.LoadingUniqueIdSetting();
                var dbSettingsObj = Newtonsoft.Json.JsonConvert.DeserializeObject<DB.Model.RMPhysicalUniqueIdSetting>(dbSettings);
                if (dbSettingsObj != null)
                {
                    if (!string.IsNullOrEmpty(dbSettingsObj.BoxTemplatePrefix) && dbSettingsObj.BoxTemplateNumberOfDigits > 0)
                    {
                        AuditItem boxGlobalUniqueIdAuditItem = new AuditItem();
                        boxGlobalUniqueIdAuditItem.TargetSetting = I18NEntity.GetString("RM_EditTemplate_GlobalBoxUniqueIdSettingsTitle");
                        boxGlobalUniqueIdAuditItem.OldValue = boxGlobalUniqueIdAuditItem.NewValue =
                            $" {I18NEntity.GetString("RM_EditTemplate_Prefix")} : {dbSettingsObj.BoxTemplatePrefix}<br>{I18NEntity.GetString("RM_EditTemplate_NumberofDigits")} : {dbSettingsObj.BoxTemplateNumberOfDigits}";
                        info.ModifyContent.Add(boxGlobalUniqueIdAuditItem);
                    }

                    if (!string.IsNullOrEmpty(dbSettingsObj.FolderTemplatePrefix) && dbSettingsObj.FolderTemplateNumberOfDigits > 0)
                    {
                        AuditItem folderGlobalUniqueIdAuditItem = new AuditItem();
                        folderGlobalUniqueIdAuditItem.TargetSetting = I18NEntity.GetString("RM_EditTemplate_GlobalFileUniqueIdSettingsTitle");
                        folderGlobalUniqueIdAuditItem.OldValue = folderGlobalUniqueIdAuditItem.NewValue =
                            $" {I18NEntity.GetString("RM_EditTemplate_Prefix")} : {dbSettingsObj.FolderTemplatePrefix}<br>{I18NEntity.GetString("RM_EditTemplate_NumberofDigits")} : {dbSettingsObj.FolderTemplateNumberOfDigits}";
                        info.ModifyContent.Add(folderGlobalUniqueIdAuditItem);
                    }

                    if (!string.IsNullOrEmpty(dbSettingsObj.RecordTemplatePrefix) && dbSettingsObj.RecordTemplateNumberOfDigits > 0)
                    {
                        AuditItem recordGlobalUniqueIdAuditItem = new AuditItem();
                        recordGlobalUniqueIdAuditItem.TargetSetting = I18NEntity.GetString("RM_EditTemplate_GlobalRecordUniqueIdSettingsTitle");
                        recordGlobalUniqueIdAuditItem.OldValue = recordGlobalUniqueIdAuditItem.NewValue =
                            $" {I18NEntity.GetString("RM_EditTemplate_Prefix")} : {dbSettingsObj.RecordTemplatePrefix}<br>{I18NEntity.GetString("RM_EditTemplate_NumberofDigits")} : {dbSettingsObj.RecordTemplateNumberOfDigits}";
                        info.ModifyContent.Add(recordGlobalUniqueIdAuditItem);
                    }
                }
            }
            else if (action == (int)AuditAction.UpdateCustomBarcodeTemplate)
            {
                BarcodeCustomTemplateDto dto = (BarcodeCustomTemplateDto)args[0];
                var oldDto = await BarcodeTemplateService.GetBarcodeTemplateBySuiteIdAsync(dto.SuiteId) as BarcodeCustomTemplateDto;
                info.Object = dto.Name;
                AuditItem templateNameAuditItem = new AuditItem();
                templateNameAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_Barcode_Template_Name");
                templateNameAuditItem.NewValue = dto.Name;
                templateNameAuditItem.OldValue = oldDto.Name;
                info.ModifyContent.Add(templateNameAuditItem);
                AuditItem templateDescAuditItem = new AuditItem();
                templateDescAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_Barcode_Template_Description");
                templateDescAuditItem.NewValue = dto.Description;
                templateDescAuditItem.OldValue = oldDto.Description;
                info.ModifyContent.Add(templateDescAuditItem);
                AuditItem templateLabelTypeAuditItem = new AuditItem();
                templateLabelTypeAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_Barcode_Template_LabelSize");
                templateLabelTypeAuditItem.NewValue = GetCustomBarcodeLabelTypeDisplay(dto.LabelType);
                templateLabelTypeAuditItem.OldValue = GetCustomBarcodeLabelTypeDisplay(oldDto.LabelType);
                info.ModifyContent.Add(templateLabelTypeAuditItem);
                foreach (var template in dto.Templates)
                {
                    var oldTemplate = oldDto.Templates.FirstOrDefault(t => t.TemplateId == template.TemplateId);
                    if (oldTemplate != null)
                    {
                        AuditItem templateTypeAuditItem = new AuditItem();
                        templateTypeAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_Barcode_Template_Type");
                        switch (template.Type)
                        {
                            case BarcodeTemplateType.Box:
                                templateTypeAuditItem.NewValue = I18NEntity.GetString("RM_PRM_TM_Barcode_Template_BoxTab");
                                break;
                            case BarcodeTemplateType.Folder:
                                templateTypeAuditItem.NewValue = I18NEntity.GetString("RM_PRM_TM_Barcode_Template_FolderTab");
                                break;
                            default:
                                templateTypeAuditItem.NewValue = template.Type.ToString();
                                break;
                        }
                        templateTypeAuditItem.OldValue = templateTypeAuditItem.NewValue;
                        info.ModifyContent.Add(templateTypeAuditItem);
                        AuditItem logoPropertiesAuditItem = new AuditItem();
                        AuditItem logoPositionAuditItem = new AuditItem();
                        logoPropertiesAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_Barcode_Template_LogoOrIcon");
                        logoPositionAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_Barcode_Template_LogoOrIconArea");
                        if (template.LogoProperties != null)
                        {
                            logoPropertiesAuditItem.NewValue = template.LogoProperties.LogoImgName;
                            logoPositionAuditItem.NewValue = template.LogoProperties.Position == BarcodeTemplatePosition.Above
                            ? I18NEntity.GetString("RM_PRM_TM_Barcode_Template_LogoOrIconArea_Above")
                            : I18NEntity.GetString("RM_PRM_TM_Barcode_Template_LogoOrIconArea_Under");
                        }
                        if (oldTemplate.LogoProperties != null)
                        {
                            logoPropertiesAuditItem.OldValue = oldTemplate.LogoProperties.LogoImgName;
                            logoPositionAuditItem.OldValue = oldTemplate.LogoProperties.Position == BarcodeTemplatePosition.Above
                                                       ? I18NEntity.GetString("RM_PRM_TM_Barcode_Template_LogoOrIconArea_Above")
                                                       : I18NEntity.GetString("RM_PRM_TM_Barcode_Template_LogoOrIconArea_Under");
                        }
                        info.ModifyContent.Add(logoPropertiesAuditItem);
                        info.ModifyContent.Add(logoPositionAuditItem);

                        AuditItem propertiesAuditItem = new AuditItem();
                        propertiesAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_Barcode_Template_Properties");

                        if (template.Properties != null && template.Properties.Count > 0)
                        {
                            StringBuilder propertiesBuilder = new StringBuilder();
                            var index = 0;
                            foreach (var property in template.Properties)
                            {
                                if (property != null)
                                {
                                    index++;
                                    var propetyName = BuildInColumnName.TryGetValue(property.Name, out var columnName) ? I18NEntity.GetString(columnName) : I18NEntity.GetString(property.Name);
                                    propertiesBuilder.AppendLine($"{index}. {I18NEntity.GetString("RM_PRM_TM_Barcode_Template_FieldName")}: {propetyName}");
                                    propertiesBuilder.AppendLine($"{I18NEntity.GetString("RM_PRM_TM_Barcode_Template_Size")}: {property.FontSize} px");
                                    propertiesBuilder.AppendLine($"{I18NEntity.GetString("RM_PRM_TM_Barcode_Template_Area")}: {property.Position}");
                                }
                            }

                            propertiesAuditItem.NewValue = propertiesBuilder.ToString();
                        }

                        if (oldTemplate.Properties != null && oldTemplate.Properties.Count > 0)
                        {
                            StringBuilder propertiesBuilder = new StringBuilder();
                            var index = 0;
                            foreach (var property in oldTemplate.Properties)
                            {
                                if (property != null)
                                {
                                    index++;
                                    var propetyName = BuildInColumnName.TryGetValue(property.Name, out var columnName) ? I18NEntity.GetString(columnName) : I18NEntity.GetString(property.Name);
                                    propertiesBuilder.AppendLine($"{index}. {I18NEntity.GetString("RM_PRM_TM_Barcode_Template_FieldName")}: {propetyName}");
                                    propertiesBuilder.AppendLine($"{I18NEntity.GetString("RM_PRM_TM_Barcode_Template_Size")}: {property.FontSize} px");
                                    propertiesBuilder.AppendLine($"{I18NEntity.GetString("RM_PRM_TM_Barcode_Template_Area")}: {property.Position}");
                                }
                            }

                            propertiesAuditItem.OldValue = propertiesBuilder.ToString();
                        }

                        info.ModifyContent.Add(propertiesAuditItem);
                    }
                }
            }
            else if (action == (int)AuditAction.DeleteCustomBarcodeTemplates)
            {
                var templateIds = (List<Guid>)args[0];
                var suites = await BarcodeTemplateSuiteDao.GetByUniqueIdsAsync(templateIds);
                var suiteNames = suites.Select(s => s.Name).ToList();
                info.Object = string.Join(", ", suiteNames);
            }
            return info;
        }

        private static string GetStartFromTypeString(Contract.TemplateManagement.SuiteStartFromType startFromType)
        {
            var startFromTypeString = "";
            switch (startFromType)
            {
                case Contract.TemplateManagement.SuiteStartFromType.Box:
                    startFromTypeString = I18NEntity.GetString("RM_PRM_TM_Suite_StartFromType_Box");
                    break;
                case Contract.TemplateManagement.SuiteStartFromType.Folder:
                    startFromTypeString = I18NEntity.GetString("RM_PRM_TM_Suite_StartFromType_Folder");
                    break;
                case Contract.TemplateManagement.SuiteStartFromType.Custom:
                    startFromTypeString = I18NEntity.GetString("RM_PRM_TM_Suite_StartFromType_Custom");
                    break;
            }

            return startFromTypeString;
        }

        private static string GetCustomBarcodeLabelTypeDisplay(BarcodeTemplateLabelType labelType)
        {
            switch (labelType)
            {
                case BarcodeTemplateLabelType.Label_200x93:
                    return "200.7 x 93.1mm";
                case BarcodeTemplateLabelType.Label_135x95:
                    return "135 x 95mm";
                case BarcodeTemplateLabelType.Label_95x65:
                    return "95.5 x 65mm";
                case BarcodeTemplateLabelType.Label_99x67:
                    return "99.1 x 67.7mm";
                case BarcodeTemplateLabelType.Label_72x63:
                    return "72 x 63.5mm";
                default:
                    return labelType.ToString();
            }
        }
    }
}
