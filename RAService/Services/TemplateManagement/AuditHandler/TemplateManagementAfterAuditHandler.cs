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
using AvePoint.RA.Common.Global.Utils;
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
    public class TemplateManagementAfterAuditHandler : IAfterAuditHandler
    {
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

        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            RMAuditInfo auditInfo = info != null ? info : new RMAuditInfo();
            //auditInfo.Module = (AuditModule)model;
            //auditInfo.Category = (AuditCategory)category;
            //auditInfo.Action = (AuditAction)action;

            if (action == (int)AuditAction.EditTemplate)
            {
                TemplateDto dto = (TemplateDto)args[0];
                auditInfo.Object = I18NEntity.GetString(dto.name);
                if (dto.id == 0)
                {
                    switch (dto.type)
                    {
                        case Contract.TemplateManagement.TemplateType.Box:
                            auditInfo.Action = AuditAction.CreateBoxTemplate;
                            break;
                        case Contract.TemplateManagement.TemplateType.Folder:
                            auditInfo.Action = AuditAction.CreateFolderTemplate;
                            break;
                        case Contract.TemplateManagement.TemplateType.Records:
                            auditInfo.Action = AuditAction.CreateRecordTemplate;
                            break;
                        default:
                            auditInfo.Action = AuditAction.CreateTemplate;
                            break;
                    }
                }
                else
                {
                    switch (dto.type)
                    {
                        case Contract.TemplateManagement.TemplateType.Box:
                            auditInfo.Action = AuditAction.EditBoxTemplate;
                            break;
                        case Contract.TemplateManagement.TemplateType.Folder:
                            auditInfo.Action = AuditAction.EditFolderTemplate;
                            break;
                        case Contract.TemplateManagement.TemplateType.Records:
                            auditInfo.Action = AuditAction.EditRecordTemplate;
                            break;
                        default:
                            auditInfo.Action = AuditAction.EditTemplate;
                            break;
                    }
                }
            }
            else if (action == (int)AuditAction.CreateSuite)
            {
                SuiteDto dto = (SuiteDto)args[0];
                auditInfo.Object = I18NEntity.GetString(dto.Name);

                AuditItem suiteNameAuditItem = new AuditItem();
                suiteNameAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_Suite_Name");
                suiteNameAuditItem.NewValue = dto.Name;

                AuditItem descriptAuditItem = new AuditItem();
                descriptAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_TemplateDesc");
                descriptAuditItem.NewValue = dto.Description;

                AuditItem startFromAuditItem = new AuditItem();
                startFromAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_Suite_StartFromTitle");
                switch (dto.StartFromType)
                {
                    case Contract.TemplateManagement.SuiteStartFromType.Box:
                        startFromAuditItem.NewValue = I18NEntity.GetString("RM_PRM_TM_Suite_StartFromType_Box");
                        break;
                    case Contract.TemplateManagement.SuiteStartFromType.Folder:
                        startFromAuditItem.NewValue = I18NEntity.GetString("RM_PRM_TM_Suite_StartFromType_Folder");
                        break;
                    case Contract.TemplateManagement.SuiteStartFromType.Custom:
                        startFromAuditItem.NewValue = I18NEntity.GetString("RM_PRM_TM_Suite_StartFromType_Custom");
                        break;
                }

                auditInfo.ModifyContent.Add(suiteNameAuditItem);
                auditInfo.ModifyContent.Add(descriptAuditItem);
                auditInfo.ModifyContent.Add(startFromAuditItem);
            }
            else if (action == (int)AuditAction.UpdateSuite)
            {

            }
            else if (action == (int)AuditAction.ToggleGlobalUniqueId)
            {
                bool isGlobal = (bool)args[0];
                AuditItem toggleGlobalUniqueIdAuditItem = new AuditItem();
                toggleGlobalUniqueIdAuditItem.TargetSetting = I18NEntity.GetString("RM_EditTemplate_Auditor_PhysicalUniqueIdSettingsTitle");
                toggleGlobalUniqueIdAuditItem.NewValue = isGlobal ? I18NEntity.GetString("RM_EditTemplate_Auditor_PhysicalUniqueIdSettingsOptionValue_Global") : I18NEntity.GetString("RM_EditTemplate_Auditor_PhysicalUniqueIdSettingsOptionValue_Each");
                auditInfo.ModifyContent.Add(toggleGlobalUniqueIdAuditItem);
            }
            else if (action == (int)AuditAction.UpdateGlobalUniqueId)
            {
                GlobalUniqueIdSettingsDto settingsDto = (GlobalUniqueIdSettingsDto)args[0];

                ArgumentCheck.NotNull(info, nameof(info));
                AuditItem boxGlobalUniqueIdAuditItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(I18NEntity.GetString("RM_EditTemplate_GlobalBoxUniqueIdSettingsTitle"))).FirstOrDefault();
                if (boxGlobalUniqueIdAuditItem == null)
                {
                    boxGlobalUniqueIdAuditItem = new AuditItem();
                    boxGlobalUniqueIdAuditItem.TargetSetting = I18NEntity.GetString("RM_EditTemplate_GlobalBoxUniqueIdSettingsTitle");
                    auditInfo.ModifyContent.Add(boxGlobalUniqueIdAuditItem);
                }
                boxGlobalUniqueIdAuditItem.NewValue = $" {I18NEntity.GetString("RM_EditTemplate_Prefix")} : {settingsDto.BoxTemplatePrefix}<br>{I18NEntity.GetString("RM_EditTemplate_NumberofDigits")} : {settingsDto.BoxTemplateNumberOfDigits}";

                AuditItem folderGlobalUniqueIdAuditItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(I18NEntity.GetString("RM_EditTemplate_GlobalFileUniqueIdSettingsTitle"))).FirstOrDefault();
                if (folderGlobalUniqueIdAuditItem == null)
                {
                    folderGlobalUniqueIdAuditItem = new AuditItem();
                    folderGlobalUniqueIdAuditItem.TargetSetting = I18NEntity.GetString("RM_EditTemplate_GlobalFileUniqueIdSettingsTitle");
                    auditInfo.ModifyContent.Add(folderGlobalUniqueIdAuditItem);
                }
                folderGlobalUniqueIdAuditItem.NewValue = $" {I18NEntity.GetString("RM_EditTemplate_Prefix")} : {settingsDto.FolderTemplatePrefix}<br>{I18NEntity.GetString("RM_EditTemplate_NumberofDigits")} : {settingsDto.FolderTemplateNumberOfDigits}";

                AuditItem recordGlobalUniqueIdAuditItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(I18NEntity.GetString("RM_EditTemplate_GlobalRecordUniqueIdSettingsTitle"))).FirstOrDefault();
                if (recordGlobalUniqueIdAuditItem == null)
                {
                    recordGlobalUniqueIdAuditItem = new AuditItem();
                    recordGlobalUniqueIdAuditItem.TargetSetting = I18NEntity.GetString("RM_EditTemplate_GlobalRecordUniqueIdSettingsTitle");
                    auditInfo.ModifyContent.Add(recordGlobalUniqueIdAuditItem);
                }
                recordGlobalUniqueIdAuditItem.NewValue = $" {I18NEntity.GetString("RM_EditTemplate_Prefix")} : {settingsDto.RecordTemplatePrefix}<br>{I18NEntity.GetString("RM_EditTemplate_NumberofDigits")} : {settingsDto.RecordTemplateNumberOfDigits}";
            }
            else if (action == (int)AuditAction.CreateBarcodeTemplate || action == (int)AuditAction.UpdateBarcodeTemplate)
            {
                BarcodeTemplateDto dto = (BarcodeTemplateDto)args[0];
                if (dto.Type == BarcodeTemplateType.Box)
                {
                    auditInfo.Object = "RM_PRM_BarcodeTemp_BoxTab";
                }
                else
                {
                    auditInfo.Object = "RM_PRM_BarcodeTemp_FolderTab";
                }
            }
            else if (action == (int)AuditAction.ImportTemplate)
            {
                info.Object = returnValue.ToString();
            }
            else if (action == (int)AuditAction.CreateCustomBarcodeTemplate)
            {
                BarcodeCustomTemplateDto dto = (BarcodeCustomTemplateDto)args[0];
                auditInfo.Object = dto.Name;
                AuditItem templateNameAuditItem = new AuditItem();
                templateNameAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_Barcode_Template_Name");
                templateNameAuditItem.NewValue = dto.Name;
                auditInfo.ModifyContent.Add(templateNameAuditItem);
                AuditItem templateDescAuditItem = new AuditItem();
                templateDescAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_Barcode_Template_Description");
                templateDescAuditItem.NewValue = dto.Description;
                auditInfo.ModifyContent.Add(templateDescAuditItem);
                AuditItem templateLabelTypeAuditItem = new AuditItem();
                templateLabelTypeAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_Barcode_Template_LabelSize");
                templateLabelTypeAuditItem.NewValue = GetCustomBarcodeLabelTypeDisplay(dto.LabelType);
                auditInfo.ModifyContent.Add(templateLabelTypeAuditItem);
                foreach (var template in dto.Templates)
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
                    auditInfo.ModifyContent.Add(templateTypeAuditItem);
                    if (template.LogoProperties != null)
                    {
                        AuditItem logoPropertiesAuditItem = new AuditItem();
                        logoPropertiesAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_Barcode_Template_LogoOrIcon");
                        logoPropertiesAuditItem.NewValue = template.LogoProperties.LogoImgName;
                        auditInfo.ModifyContent.Add(logoPropertiesAuditItem);
                        AuditItem logoPositionAuditItem = new AuditItem();
                        logoPositionAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_Barcode_Template_LogoOrIconArea");
                        logoPositionAuditItem.NewValue = template.LogoProperties.Position == BarcodeTemplatePosition.Above
                            ? I18NEntity.GetString("RM_PRM_TM_Barcode_Template_LogoOrIconArea_Above") 
                            : I18NEntity.GetString("RM_PRM_TM_Barcode_Template_LogoOrIconArea_Under");
                    }
                    if (template.Properties != null && template.Properties.Count > 0)
                    {
                        StringBuilder propertiesBuilder = new StringBuilder();
                        var index = 0;
                        foreach (var property in template.Properties)
                        {
                            index++;
                            var propetyName = BuildInColumnName.TryGetValue(property.Name, out var columnName) ? I18NEntity.GetString(columnName) : I18NEntity.GetString(property.Name);
                            propertiesBuilder.AppendLine($"{index}. {I18NEntity.GetString("RM_PRM_TM_Barcode_Template_FieldName")}: {propetyName}");
                            propertiesBuilder.AppendLine($"{I18NEntity.GetString("RM_PRM_TM_Barcode_Template_Size")}: {property.FontSize} px");
                            propertiesBuilder.AppendLine($"{I18NEntity.GetString("RM_PRM_TM_Barcode_Template_Area")}: {property.Position}");
                        }
                        AuditItem propertiesAuditItem = new AuditItem();
                        propertiesAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_Barcode_Template_Properties");
                        propertiesAuditItem.NewValue = propertiesBuilder.ToString();
                        auditInfo.ModifyContent.Add(propertiesAuditItem);
                    }
                }
            }
            else if (action == (int)AuditAction.PreviewCustomBarcodeTemplate)
            {
                BarcodeCustomTemplateDto dto = (BarcodeCustomTemplateDto)args[0];
                auditInfo.Object = dto.Name;
                AuditItem templateNameAuditItem = new AuditItem();
                templateNameAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_Barcode_Template_Name");
                templateNameAuditItem.NewValue = dto.Name;
                auditInfo.ModifyContent.Add(templateNameAuditItem);
                AuditItem templateDescAuditItem = new AuditItem();
                templateDescAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_Barcode_Template_Description");
                templateDescAuditItem.NewValue = dto.Description;
                auditInfo.ModifyContent.Add(templateDescAuditItem);
                AuditItem templateLabelTypeAuditItem = new AuditItem();
                templateLabelTypeAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_Barcode_Template_LabelSize");
                templateLabelTypeAuditItem.NewValue = GetCustomBarcodeLabelTypeDisplay(dto.LabelType);
                auditInfo.ModifyContent.Add(templateLabelTypeAuditItem);
                foreach (var template in dto.Templates)
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
                    auditInfo.ModifyContent.Add(templateTypeAuditItem);
                    if (template.LogoProperties != null)
                    {
                        AuditItem logoPropertiesAuditItem = new AuditItem();
                        logoPropertiesAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_Barcode_Template_LogoOrIcon");
                        logoPropertiesAuditItem.NewValue = template.LogoProperties.LogoImgName;
                        auditInfo.ModifyContent.Add(logoPropertiesAuditItem);
                        AuditItem logoPositionAuditItem = new AuditItem();
                        logoPositionAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_Barcode_Template_LogoOrIconArea");
                        logoPositionAuditItem.NewValue = template.LogoProperties.Position == BarcodeTemplatePosition.Above
                            ? I18NEntity.GetString("RM_PRM_TM_Barcode_Template_LogoOrIconArea_Above")
                            : I18NEntity.GetString("RM_PRM_TM_Barcode_Template_LogoOrIconArea_Under");
                    }
                    if (template.Properties != null && template.Properties.Count > 0)
                    {
                        StringBuilder propertiesBuilder = new StringBuilder();
                        var index = 0;
                        foreach (var property in template.Properties)
                        {
                            index++;
                            var propetyName = BuildInColumnName.TryGetValue(property.Name, out var columnName) ? I18NEntity.GetString(columnName) : I18NEntity.GetString(property.Name);
                            propertiesBuilder.AppendLine($"{index}. {I18NEntity.GetString("RM_PRM_TM_Barcode_Template_FieldName")}: {propetyName}");
                            propertiesBuilder.AppendLine($"{I18NEntity.GetString("RM_PRM_TM_Barcode_Template_Size")}: {property.FontSize} px");
                            propertiesBuilder.AppendLine($"{I18NEntity.GetString("RM_PRM_TM_Barcode_Template_Area")}: {property.Position}");
                        }
                        AuditItem propertiesAuditItem = new AuditItem();
                        propertiesAuditItem.TargetSetting = I18NEntity.GetString("RM_PRM_TM_Barcode_Template_Properties");
                        propertiesAuditItem.NewValue = propertiesBuilder.ToString();
                        auditInfo.ModifyContent.Add(propertiesAuditItem);
                    }
                }
            }
            return auditInfo;
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
