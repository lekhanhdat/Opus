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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Util;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.TemplateManagement.Barcode;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.TemplateManagement.AuditHandler;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.TemplateManagement
{
    [Audit]
    public class BarcodeTemplateService : RMServiceBase, IBarcodeTemplateService
    {
        private const int MaxLogoSizeBytes = 1 * 1024 * 1024;
        private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47 };
        private static readonly byte[] JpegSignature = { 0xFF, 0xD8, 0xFF };
        private RALogger logger = RALogger.GetInstance(typeof(IBarcodeTemplateService));
        private IRMTemplateDao TemplateDao => PlatformWindsorManager.GetService<IRMTemplateDao>();
        private IRMBarcodeTemplateDao BarcodeTemplateDao => PlatformWindsorManager.GetService<IRMBarcodeTemplateDao>();
        private IRMBarcodeTemplateColumnMembershipDao BarcodeTemplateColumnMembershipDao => PlatformWindsorManager.GetService<IRMBarcodeTemplateColumnMembershipDao>();
        private IRMCustomBarcodeTemplateSuiteDao CustomBarcodeTemplateSuiteDao => PlatformWindsorManager.GetService<IRMCustomBarcodeTemplateSuiteDao>();
        private IRMCustomBarcodeTemplateDao CustomBarcodeTemplateDao => PlatformWindsorManager.GetService<IRMCustomBarcodeTemplateDao>();
        private IRMCustomBarcodeTemplatePropertyDao CustomBarcodeTemplatePropertyDao => PlatformWindsorManager.GetService<IRMCustomBarcodeTemplatePropertyDao>();

        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        public async Task<TemplateColumnInfo> GetAllTemplateColumnAsync()
        {
            var boxTemplateColumns = new Dictionary<string, List<string>>()
            {
                {BuildInColumnIDs.RecordsId.ToString(), new List<string>()},
                {BuildInColumnIDs.CreatedBy.ToString(), new List<string>()},
                {BuildInColumnIDs.CreatedTime.ToString(), new List<string>()},
                {BuildInColumnIDs.ModifiedBy.ToString(), new List<string>()},
                {BuildInColumnIDs.ModifiedTime.ToString(), new List<string>()}
            };
            var foldTemplateColumns = new Dictionary<string, List<string>>()
            {
                {BuildInColumnIDs.RecordsId.ToString(), new List<string>()},
                {BuildInColumnIDs.CreatedBy.ToString(), new List<string>()},
                {BuildInColumnIDs.CreatedTime.ToString(), new List<string>()},
                {BuildInColumnIDs.ModifiedBy.ToString(), new List<string>()},
                {BuildInColumnIDs.ModifiedTime.ToString(), new List<string>()}
            };
            var boxTemplates = await TemplateDao.FindListAsync(t => t.Type == TemplateType.Box);
            GetBoxTemplateColumns(boxTemplates, boxTemplateColumns);
            var foldTemplates = await TemplateDao.FindListAsync(t => t.Type == TemplateType.Folder);
            GetFoldTemplateColumns(boxTemplates, foldTemplates, foldTemplateColumns);
            return new TemplateColumnInfo
            {
                BoxTemplateColumns = boxTemplateColumns,
                FolderTemplateColumns = foldTemplateColumns,
            };
        }

        public void GetBoxTemplateColumns(List<RMTemplate> boxTemplates, Dictionary<string, List<string>> columnsWithTemplateName)
        {
            foreach (RMTemplate template in boxTemplates)
            {
                var schemaTemp = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(template.ColumnSchema);
                foreach (ColumnXmlSchema column in schemaTemp.Columns)
                {
                    if (!columnsWithTemplateName.ContainsKey(column.Name))
                    {
                        List<string> templatesName = new List<string>();
                        templatesName.Add(template.Name);
                        columnsWithTemplateName[column.Name] = templatesName;
                    }
                    else
                    {
                        List<string> templatesName = columnsWithTemplateName[column.Name];
                        if (!templatesName.Contains(template.Name))
                        {
                            templatesName.Add(template.Name);
                            columnsWithTemplateName[column.Name] = templatesName;
                        }
                    }
                }
            }
        }

        public void GetFoldTemplateColumns(List<RMTemplate> boxTemplates, List<RMTemplate> foldTemplates, Dictionary<string, List<string>> columnsWithTemplateName)
        {
            List<string> foldUniqueId = new List<string>();
            Dictionary<string, string> foldUniqueIdAndName = new Dictionary<string, string>();
            foreach (RMTemplate template in foldTemplates)
            {
                foldUniqueId.Add(template.UniqueId.ToString());
                if (!foldUniqueIdAndName.ContainsKey(template.UniqueId.ToString()))
                {
                    foldUniqueIdAndName.Add(template.UniqueId.ToString(), template.Name);
                }
                var schemaTemp = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(template.ColumnSchema);
                foreach (ColumnXmlSchema column in schemaTemp.Columns)
                {
                    //过滤掉老数据的pushcolumn(最初版本的pushcolumn记录在子template)
                    if ((column.TemplateInheritSetting & (int)TemplateInheritSettingEnum.InheritFromParentFolder) == (int)TemplateInheritSettingEnum.InheritFromParentFolder)
                    {
                        continue;
                    }
                    if ((column.TemplateInheritSetting & (int)TemplateInheritSettingEnum.InheritFromParentBox) == (int)TemplateInheritSettingEnum.InheritFromParentBox)
                    {
                        continue;
                    }

                    if (!columnsWithTemplateName.ContainsKey(column.Name))
                    {
                        List<string> templatesName = new List<string>();
                        templatesName.Add(template.Name);
                        columnsWithTemplateName[column.Name] = templatesName;
                    }
                    else
                    {
                        List<string> templatesName = columnsWithTemplateName[column.Name];
                        if (!templatesName.Contains(template.Name))
                        {
                            templatesName.Add(template.Name);
                            columnsWithTemplateName[column.Name] = templatesName;
                        }
                    }
                }
            }
            //handle pushcolumn
            foreach (RMTemplate template in boxTemplates)
            {
                var schemaTemp = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(template.ColumnSchema);
                foreach (ColumnXmlSchema column in schemaTemp.Columns)
                {
                    if ((column.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)
                    {
                        if (column.pushFoldTemplateCategoriesId == null)
                        {
                            continue;
                        }
                        foreach (TemplateIdAndCategoryId temp in column.pushFoldTemplateCategoriesId)
                        {
                            if (foldUniqueIdAndName.ContainsKey(temp.tempalteId))
                            {
                                if (columnsWithTemplateName.ContainsKey(column.Name))
                                {
                                    List<string> templatesName = columnsWithTemplateName[column.Name];
                                    if (!templatesName.Contains(foldUniqueIdAndName[temp.tempalteId]))
                                    {
                                        templatesName.Add(foldUniqueIdAndName[temp.tempalteId]);
                                        columnsWithTemplateName[column.Name] = templatesName;
                                    }
                                }
                                else
                                {
                                    List<string> templatesName = new List<string>();
                                    templatesName.Add(foldUniqueIdAndName[temp.tempalteId]);
                                    columnsWithTemplateName[column.Name] = templatesName;
                                }
                            }
                        }
                    }
                }
            }


        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.TemplateManagement, Action = AuditAction.CreateBarcodeTemplate, BeforeHandler = typeof(TemplateManagementBeforeAuditHandler), AfterHandler = typeof(TemplateManagementAfterAuditHandler))]
        public RAReturnMessage CreateBarcodeTemplate(BarcodeTemplateDto dto)
        {
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            try
            {
                if (dto.ColumnD != null && dto.ColumnD.Count > 5)
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_PRM_BarcodeTemp_Valid_AreaDLimit");
                    return msg;
                }
                List<string> imageType = new List<string> { "png", "jpg", "pjp", "jpeg", "jfif", "pjpeg" };
                if (!string.IsNullOrEmpty(dto.ImageType) && !imageType.Contains(dto.ImageType.ToLower()))
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_PRM_BarcodeTemp_Valid_TypeNotSupport");
                    return msg;
                }
                RMBarcodeTemplate rmBarcode = ConvertUtil.ConvertToRMBarcodeTemplate(dto);

                if (BarcodeTemplateDao.CheckBarcodeTemplateExist((int)dto.Type))
                {
                    logger.Error("Create barcode template error: This type template is exist");
                    msg.MessageType = RAMessageType.Failed;
                    msg.FaildType = RAFailedType.NameExisting;
                    msg.ErrorMessage = I18NEntity.GetString("RM_PRM_BarcodeTemp_CreateFailed");
                    return msg;
                }
                bool isSaveSuccess = BarcodeTemplateDao.SaveBarcodeTemplate(rmBarcode);
                if (!isSaveSuccess)
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_PRM_BarcodeTemp_CreateFailed");
                    return msg;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Create barcode template error:{0}", ex.ToString());
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = ex.Message;
            }
            return msg;
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.TemplateManagement, Action = AuditAction.UpdateBarcodeTemplate, BeforeHandler = typeof(TemplateManagementBeforeAuditHandler), AfterHandler = typeof(TemplateManagementAfterAuditHandler))]
        public async Task<RAReturnMessage> UpdateBarcodeTemplateAsync(BarcodeTemplateDto dto)
        {
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            try
            {
                if (dto.ColumnD != null && dto.ColumnD.Count > 5)
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_PRM_BarcodeTemp_Valid_AreaDLimit");
                    return msg;
                }
                List<string> imageType = new List<string> { "png", "jpg", "pjp", "jpeg", "jfif", "pjpeg" };
                if (!string.IsNullOrEmpty(dto.ImageType) && !imageType.Contains(dto.ImageType.ToLower()))
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_PRM_BarcodeTemp_Valid_TypeNotSupport");
                    return msg;
                }
                RMBarcodeTemplate rmBarcodeTemplate = ConvertUtil.ConvertToRMBarcodeTemplate(dto);
                bool isSaveSuccess = await BarcodeTemplateDao.UpdateBarcodeTemplateAsync(rmBarcodeTemplate);
                if (!isSaveSuccess)
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_PRM_BarcodeTemp_UpdateFailed");
                    return msg;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Update barcode template error:{0}", ex.ToString());
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = ex.Message;
            }
            return msg;
        }

        public async Task<BarcodeTemplateDto> GetDefaultBarcodeTemplateByTypeAsync(AvePoint.RA.Contract.TemplateManagement.BarcodeTemplateType type)
        {
            BarcodeTemplateDto dto = new BarcodeTemplateDto();
            
            // Get default suite first
            var defaultSuite = await CustomBarcodeTemplateSuiteDao.GetDefaultAsync();
            if (defaultSuite != null)
            {
                // Convert the type to the barcode contract type
                var barcodeType = (AvePoint.RA.Contract.TemplateManagement.BarcodeTemplateType)type;
                
                // Get default template for this suite and type
                var customTemplate = await CustomBarcodeTemplateDao.GetDefaultTemplateAsync(defaultSuite.UniqueId, barcodeType);
                if (customTemplate != null)
                {
                    var columnMemberships = await BarcodeTemplateColumnMembershipDao.GetByTypeAsync((int)type);
                    dto = ConvertUtil.ConvertCustomBarcodeTemplateToDto(customTemplate, columnMemberships);
                }
            }
            
            // If no custom template found, fall back to legacy template
            if (string.IsNullOrEmpty(dto.Id))
            {
                RMBarcodeTemplate rmBarcodeTemplate = BarcodeTemplateDao.GetTemplateByType((int)type);
                if (rmBarcodeTemplate != null)
                {
                    dto = ConvertUtil.ConvertToBarcodeTemplateDto(rmBarcodeTemplate);
                }
            }
            
            return dto;
        }

        public async Task<List<BarcodeTemplateSuiteDto>> GetAllBarcodeTemplateSuitesAsync()
        {
            var suites = await CustomBarcodeTemplateSuiteDao.GetAllAsync();
            if (suites != null && suites.Count > 0)
            {
                return suites.ConvertAll(ConvertUtil.ConvertToBarcodeTemplateSuiteDto);
            }
            return [];
        }

        public async Task<PagedBarcodeTemplateSuiteResult> GetPagedBarcodeTemplateSuitesAsync(PagedBarcodeTemplateSuiteRequest request)
        {
            try
            {
                // Validate and normalize request parameters
                if (request == null)
                {
                    request = new PagedBarcodeTemplateSuiteRequest();
                }

                // Ensure valid page parameters
                if (request.PageIndex < 0) request.PageIndex = 0;
                if (request.PageSize <= 0) request.PageSize = 20;
                if (request.PageSize > 100) request.PageSize = 100; // Limit max page size

                var searchName = string.IsNullOrWhiteSpace(request.SearchName) ? null : request.SearchName.Trim();

                // Call DAO to get paged results
                var pagedResult = await CustomBarcodeTemplateSuiteDao.GetPagedAsync(
                    request.PageIndex, 
                    request.PageSize, 
                    searchName);

                // Convert to DTOs
                var suiteDtos = pagedResult.Suites?.ConvertAll(ConvertUtil.ConvertToBarcodeTemplateSuiteDto) ?? new List<BarcodeTemplateSuiteDto>();

                // Create result
                return new PagedBarcodeTemplateSuiteResult
                {
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize,
                    TotalCount = pagedResult.TotalCount,
                    Suites = suiteDtos,
                    SearchName = searchName,
                };
            }
            catch (Exception ex)
            {
                logger.Error($"Error getting paged barcode template suites: {ex.Message}", ex);
                
                // Return empty result on error
                return new PagedBarcodeTemplateSuiteResult
                {
                    PageIndex = request?.PageIndex ?? 0,
                    PageSize = request?.PageSize ?? 20,
                    TotalCount = 0,
                    Suites = new List<BarcodeTemplateSuiteDto>(),
                    SearchName = request?.SearchName
                };
            }
        }
        
        public async Task<BarcodeTemplateSuiteDto> GetBarcodeTemplateBySuiteIdAsync(Guid uniqueId)
        {
            var suite = await CustomBarcodeTemplateSuiteDao.GetByUniqueIdAsync(uniqueId);
            if (suite != null)
            {
                var customTemplates = await CustomBarcodeTemplateDao.GetBySuiteIdAsync(uniqueId);
                if (suite.IsDefault)
                {
                    var defaultTemplates = customTemplates.ConvertAll(
                        item => SerializerHelper.DeserializeByJsonConvert<RMBarcodeTemplate>(item.PropertiesJson)
                    );
                    var defaultTempalteColumns = BarcodeTemplateColumnMembershipDao.GetAll();
                    var defaultTemplateDtos = defaultTemplates.Select(item =>
                    {
                        var result = ConvertUtil.ConvertToBarcodeTemplateDto(item);
                        result.ColumnD = defaultTempalteColumns
                                        .Where(col => col.Type == item.Type)
                                        .Select(col => col.ColumnName)
                                        .ToList();
                        return result;
                    }).ToList();

                    return ConvertUtil.ConvertToBarcodeDefaultTemplateDto(suite, defaultTemplateDtos);
                }

                var customTemplateProperties = await CustomBarcodeTemplatePropertyDao.GetByTemplateIdsAsync(customTemplates.Select(item => item.Id).ToList());
                var customTemplateDtos = customTemplates.Select(item =>
                {
                    var result = new BarcodeCustomTemplateInfo
                    {
                        TemplateId = item.Id,
                        Type = item.Type,
                        LogoProperties = !string.IsNullOrEmpty(item.PropertiesJson) ? SerializerHelper.DeserializeByJsonConvert<BarcodeTemplateLogoProperties>(item.PropertiesJson) : new BarcodeTemplateLogoProperties(),
                        Properties = customTemplateProperties.Where(prop => prop.TemplateId == item.Id).ConvertAll(ConvertUtil.ConvertToBarcodeTemplatePropertyDto).ToList(),
                    };
                    return result;
                }).ToList();

                return ConvertUtil.ConvertToBarcodeCustomTemplateDto(suite, customTemplateDtos);
            }
            return null;
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.TemplateManagement, Action = AuditAction.CreateCustomBarcodeTemplate, BeforeHandler = typeof(TemplateManagementBeforeAuditHandler), AfterHandler = typeof(TemplateManagementAfterAuditHandler))]
        public async Task<RAReturnMessage> CreateCustomBarcodeTemplateAsync(BarcodeCustomTemplateDto dto)
        {
            try
            {
                dto.Name = dto.Name.Trim();

                if (dto.IsDefault)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                    };
                }

                var existingSuite = await CustomBarcodeTemplateSuiteDao.GetByNameAsync(dto.Name);
                if (existingSuite != null)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = I18NEntity.GetString("RM_PRM_BarcodeTemp_SuiteNameExist"),
                    };
                }

                if (dto.Templates != null && dto.Templates.Any())
                {
                    foreach (var templateInfo in dto.Templates)
                    {
                        // Validate image base64 before creating template 
                        if (templateInfo.LogoProperties?.LogoImgBase64Str != null)
                        {
                            if (!ValidateBase64Image(templateInfo.LogoProperties.LogoImgBase64Str, out var errorMessage))
                            {
                                return new RAReturnMessage
                                {
                                    MessageType = RAMessageType.Failed,
                                    ErrorMessage = I18NEntity.GetString(errorMessage)
                                };
                            }
                        }
                    }
                }

                var currentTime = DateTime.UtcNow.Ticks;
                
                var newSuite = new RMCustomBarcodeTemplateSuite
                {
                    UniqueId = Guid.NewGuid(),
                    IsDefault = false,
                    Name = dto.Name,
                    Description = dto.Description,
                    LabelType = dto.LabelType,
                    CreatedTime = currentTime,
                    ModifiedTime = currentTime
                };

                try
                {
                    await CustomBarcodeTemplateSuiteDao.CreateAsync(newSuite);
                }
                catch (DbUpdateException)
                {
                    if (await CustomBarcodeTemplateSuiteDao.GetByNameAsync(dto.Name) != null)
                    {
                        return new RAReturnMessage
                        {
                            MessageType = RAMessageType.Failed,
                            ErrorMessage = I18NEntity.GetString("RM_PRM_BarcodeTemp_SuiteNameExist"),
                        };
                    }

                    throw;
                }

                var templatesToCreate = new List<RMCustomBarcodeTemplate>();
                var allPropertiesToCreate = new List<RMCustomBarcodeTemplateProperty>();

                if (dto.Templates != null && dto.Templates.Any())
                {
                    foreach (var templateInfo in dto.Templates)
                    {
                        // Prepare template for batch creation
                        var newTemplate = new RMCustomBarcodeTemplate
                        {
                            SuiteId = newSuite.UniqueId,
                            Name = newSuite.Name + "_" + templateInfo.Type,
                            Type = templateInfo.Type,
                            IsDefault = false,
                            PropertiesJson = SerializerHelper.SerializeByJsonConvert(templateInfo.LogoProperties)
                        };
                        templatesToCreate.Add(newTemplate);
                    }

                    // Batch create templates
                    var createdTemplateIds = await CustomBarcodeTemplateDao.BatchCreateAsync(templatesToCreate);

                    // Prepare properties for batch creation
                    for (int i = 0; i < dto.Templates.Count && i < createdTemplateIds.Count; i++)
                    {
                        var templateInfo = dto.Templates[i];
                        var templateId = createdTemplateIds[i];

                        if (templateInfo.Properties != null && templateInfo.Properties.Any())
                        {
                            var newProperties = templateInfo.Properties.Select(propertyDto => new RMCustomBarcodeTemplateProperty
                            {
                                TemplateId = templateId,
                                Name = propertyDto.Name,
                                FontSize = propertyDto.FontSize,
                                Position = propertyDto.Position,
                                // SortOrder = propertyDto.SortOrder,
                                CreatedTime = currentTime,
                                ModifiedTime = currentTime
                            }).ToList();

                            allPropertiesToCreate.AddRange(newProperties);
                        }
                    }

                    // Batch create properties
                    if (allPropertiesToCreate.Any())
                    {
                        await CustomBarcodeTemplatePropertyDao.BatchCreateAsync(allPropertiesToCreate);
                    }
                }

                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Successful,
                };
            }
            catch (Exception ex)
            {
                logger.Error($"Error creating custom barcode template: {ex.Message}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Exception,
                };
            }
        }

        public async Task<RAReturnMessage> UpdateDefaultBarcodeTemplateAsync(BarcodeDefaultTemplateDto dto)
        {
            try
            {
                var existingSuite = await CustomBarcodeTemplateSuiteDao.GetByUniqueIdAsync(dto.SuiteId);
                if (existingSuite == null)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                    };
                }

                var currentTime = DateTime.UtcNow.Ticks;

                existingSuite.Name = dto.Name;
                existingSuite.Description = dto.Description;
                existingSuite.LabelType = dto.LabelType;
                existingSuite.ModifiedTime = currentTime;

                var suiteUpdated = await CustomBarcodeTemplateSuiteDao.UpdateAsync(existingSuite);
                if (!suiteUpdated)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                    };
                }

                var existingTemplates = await CustomBarcodeTemplateDao.GetBySuiteIdAsync(dto.SuiteId);
                var existingTemplateDict = existingTemplates.ToDictionary(t => t.Type, t => t);
                var templatesToUpdate = new List<RMCustomBarcodeTemplate>();

                var defaultTemplates = dto.Templates ?? new List<BarcodeTemplateDto>();
                foreach (var templateInfo in defaultTemplates)
                {
                    if (existingTemplateDict.TryGetValue(templateInfo.Type, out var existingTemplate))
                    {
                        var rmBarcodeTemplate = ConvertUtil.ConvertToRMBarcodeTemplate(templateInfo);
                        if (rmBarcodeTemplate.ColumnDList != null)
                        {
                            BarcodeTemplateColumnMembershipDao.CreateOrUpdateTemplateColumnMemberShips(rmBarcodeTemplate.Type, rmBarcodeTemplate.ColumnDList);
                        }
                        existingTemplate.PropertiesJson = SerializerHelper.SerializeByJsonConvert(rmBarcodeTemplate);
                        templatesToUpdate.Add(existingTemplate);
                    }
                }

                if (templatesToUpdate.Count != 0)
                    await CustomBarcodeTemplateDao.BatchUpdateAsync(templatesToUpdate);

                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Successful,
                };
            }
            catch (Exception ex)
            {
                logger.Error($"Error updating default barcode template: {ex.Message}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Exception,
                };
            }
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.TemplateManagement, Action = AuditAction.UpdateCustomBarcodeTemplate, BeforeHandler = typeof(TemplateManagementBeforeAuditHandler), AfterHandler = typeof(TemplateManagementAfterAuditHandler))]
        public async Task<RAReturnMessage> UpdateCustomBarcodeTemplateAsync(BarcodeCustomTemplateDto dto)
        {
            try
            {
                // Get existing suite
                var existingSuite = await CustomBarcodeTemplateSuiteDao.GetByUniqueIdAsync(dto.SuiteId);
                if (existingSuite == null)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                    };
                }

                var suiteWithSameName = await CustomBarcodeTemplateSuiteDao.GetByNameAsync(dto.Name);
                if (suiteWithSameName != null && suiteWithSameName.UniqueId != dto.SuiteId)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = I18NEntity.GetString("RM_PRM_BarcodeTemp_SuiteNameExist"),
                    };
                }

                if (dto.Templates != null && dto.Templates.Any())
                {
                    foreach (var templateInfo in dto.Templates)
                    {
                        // Validate image base64 before creating template 
                        if (templateInfo.LogoProperties?.LogoImgBase64Str != null)
                        {
                            if (!ValidateBase64Image(templateInfo.LogoProperties.LogoImgBase64Str, out var errorMessage))
                            {
                                return new RAReturnMessage
                                {
                                    MessageType = RAMessageType.Failed,
                                    ErrorMessage = I18NEntity.GetString(errorMessage)
                                };
                            }
                        }
                    }
                }

                var currentTime = DateTime.UtcNow.Ticks;

                existingSuite.Name = dto.Name;
                existingSuite.Description = dto.Description;
                existingSuite.LabelType = dto.LabelType;
                existingSuite.ModifiedTime = currentTime;

                var suiteUpdated = await CustomBarcodeTemplateSuiteDao.UpdateAsync(existingSuite);
                if (!suiteUpdated)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = "Failed to update template suite."
                    };
                }

                var customDto = dto as BarcodeCustomTemplateDto;
                var customTemplates = customDto?.Templates ?? new List<BarcodeCustomTemplateInfo>();
                var allPropertiesToUpdate = new List<RMCustomBarcodeTemplateProperty>();
                var allPropertiesToCreate = new List<RMCustomBarcodeTemplateProperty>();
                var propertyIdsToDelete = new List<int>();

                var existingTemplates = await CustomBarcodeTemplateDao.GetBySuiteIdAsync(dto.SuiteId);
                var existingTemplateDict = existingTemplates.ToDictionary(t => t.Type, t => t);
                var templatesToUpdate = new List<RMCustomBarcodeTemplate>();

                foreach (var templateInfo in customTemplates)
                {
                    if (existingTemplateDict.TryGetValue(templateInfo.Type, out var existingTemplate))
                    {
                        existingTemplate.PropertiesJson = SerializerHelper.SerializeByJsonConvert(templateInfo.LogoProperties);
                        templatesToUpdate.Add(existingTemplate);

                        // Prepare properties for this template
                        await PreparePropertiesForUpdate(existingTemplate.Id, templateInfo.Properties, currentTime,
                            allPropertiesToUpdate, allPropertiesToCreate, propertyIdsToDelete);
                    }
                }

                // 批量删除、更新、创建属性
                if (propertyIdsToDelete.Count != 0)
                    await CustomBarcodeTemplatePropertyDao.BatchDeleteAsync(propertyIdsToDelete);
                if (allPropertiesToUpdate.Count != 0)
                    await CustomBarcodeTemplatePropertyDao.BatchUpdateAsync(allPropertiesToUpdate);
                if (allPropertiesToCreate.Count != 0)
                    await CustomBarcodeTemplatePropertyDao.BatchCreateAsync(allPropertiesToCreate);

                // 批量更新模板
                if (templatesToUpdate.Count != 0)
                    await CustomBarcodeTemplateDao.BatchUpdateAsync(templatesToUpdate);

                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Successful,
                };
            }
            catch (Exception ex)
            {
                logger.Error($"Error updating custom barcode template: {ex.Message}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Exception,
                };
            }
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.TemplateManagement, Action = AuditAction.DeleteCustomBarcodeTemplates, BeforeHandler = typeof(TemplateManagementBeforeAuditHandler), AfterHandler = typeof(TemplateManagementAfterAuditHandler))]
        public async Task<RAReturnMessage> BatchDeleteCustomBarcodeTemplateSuitesAsync(List<Guid> suiteIds)
        {
            if (suiteIds == null || suiteIds.Count == 0)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                };
            }
            try
            {
                foreach (var suiteId in suiteIds)
                {
                    var templates = await CustomBarcodeTemplateDao.GetBySuiteIdAsync(suiteId);
                    if (templates?.Count > 0)
                    {
                        var templateIds = templates.Select(t => t.Id).ToList();
                        if (templateIds.Count > 0)
                        {
                            foreach (var tid in templateIds)
                            {
                                var props = await CustomBarcodeTemplatePropertyDao.GetByTemplateIdAsync(tid);
                                if (props?.Count > 0)
                                {
                                    await CustomBarcodeTemplatePropertyDao.BatchDeleteAsync(props.Select(p => p.Id).ToList());
                                }
                            }
                            await CustomBarcodeTemplateDao.BatchDeleteAsync(templateIds);
                        }
                    }
                    await CustomBarcodeTemplateSuiteDao.DeleteByUniqueIdAsync(suiteId);
                }
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Successful,
                };
            }
            catch (Exception ex)
            {
                logger.Error($"Error batch deleting barcode template suites: {ex.Message}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Exception,
                };
            }
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.TemplateManagement, Action = AuditAction.PreviewCustomBarcodeTemplate, BeforeHandler = typeof(TemplateManagementBeforeAuditHandler), AfterHandler = typeof(TemplateManagementAfterAuditHandler))]
        public async Task<ExportResultDto> DownLoadPrivewBarcodeTemplateAsync(BarcodeCustomTemplateDto dto)
        {
            var result = new ExportResultDto();
            var labelType = dto.LabelType;
            var templatePath = GetTemplatePath(labelType);
            var nowTime = DateTime.UtcNow;
            var nowTimeStr = (await GeneralSettingService.ConvertTiksToDateTimeAsync(nowTime.Ticks, false)).DataTime.ToString(AveDateTimeUtility.DATETYPE022);
            var fileName = I18NEntity.GetString("RM_DAM_ExportBarcodesReport") + "_" + "Preview items" + "_" + nowTimeStr;
            var folderPath = SecurityUtils.SafeCombinePath(JobReportUtility.GetDownloadBarcodeInfoReportTempleFolder("Temple"), fileName);
            string reportFilePath = folderPath + Path.DirectorySeparatorChar + fileName + ".docx";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var roundRects = ReportWordUtil.GetRoundRectCount(templatePath);
            var labelItems = await BuildPreviewLabelItems(dto, roundRects);
            ReportWordUtil.CopyTemplateAndFillVml(templatePath, reportFilePath, labelItems);
            result.FileContent = StreamUtl.ReadFile(reportFilePath);
            result.FileName = $"{fileName}.docx";
            logger.Info("Finish export barcode");
            return result;
        }

        private async Task<List<LabelItem>> BuildPreviewLabelItems(BarcodeCustomTemplateDto dto, int roundRects)
        {
            var labelItems = new List<LabelItem>();
            if (roundRects <= 0) return labelItems;
            if (dto.Templates == null || !dto.Templates.Any()) return labelItems;

            var boxTemplate = dto.Templates.FirstOrDefault(t => t.Type == BarcodeTemplateType.Box) ?? dto.Templates.First();
            var folderTemplate = dto.Templates.FirstOrDefault(t => t.Type == BarcodeTemplateType.Folder) ?? boxTemplate;

            for (int i = 0; i < roundRects; i++)
            {
                var template = i == 0 ? boxTemplate : folderTemplate;
                LogoItem logo = null;
                var logoProp = template.LogoProperties;
                if (logoProp != null)
                {
                    var isEnableLogo = !string.IsNullOrEmpty(logoProp.LogoImgBase64Str);
                    var imageBytes = new byte[0];
                    var width = 50;
                    var height = 50;
                    if (!string.IsNullOrEmpty(logoProp.LogoImgBase64Str) && logoProp.LogoImgBase64Str.Contains(","))
                    {
                        var idx = logoProp.LogoImgBase64Str.IndexOf(",");
                        if (idx >= 0 && idx + 1 < logoProp.LogoImgBase64Str.Length)
                            logoProp.LogoImgBase64Str = logoProp.LogoImgBase64Str[(idx + 1)..];
                    }
                    if (isEnableLogo)
                    {
                        imageBytes = Convert.FromBase64String(logoProp.LogoImgBase64Str);
                        var imageInfo = BarcodeUtil.GetImageInfo(imageBytes);
                        if (imageInfo != null)
                        {
                            width = imageInfo.Width > 0 ? imageInfo.Width : 50;
                            height = imageInfo.Height > 0 ? imageInfo.Height : 50;
                        }
                    }
                    logo = new LogoItem
                    {
                        Enabled = isEnableLogo,
                        Position = logoProp.Position,
                        ImageBytes = imageBytes,
                        FileName = string.IsNullOrEmpty(logoProp.LogoImgName) ? "logo" : logoProp.LogoImgName,
                        Mime = string.IsNullOrEmpty(logoProp.LogoImgType) ? "image/png" : logoProp.LogoImgType,
                        Width = width,
                        Height = height
                    };
                }

                var barcode = i == 0 ? "PREVIEWBOX" : $"PREVIEWFOLDER{i}";
                var properties = template.Properties != null
                    ? template.Properties.Select(p => new PropertyItem
                    {
                        Name = p.Name,
                        Value = p.DisplayName,
                        Position = p.Position,
                        FontSize = p.FontSize > 0 ? p.FontSize : 10
                    }).ToList()
                    : new List<PropertyItem>();

                labelItems.Add(new LabelItem
                {
                    Barcode = barcode.ToUpperInvariant(),
                    Properties = properties,
                    Logo = logo
                });
            }
            return labelItems;
        }

        private static string GetTemplatePath(BarcodeTemplateLabelType labelType)
        {
            return labelType switch
            {
                BarcodeTemplateLabelType.Label_200x93 => Path.Combine(RA.Common.Util.WebUtil.GetInstallPath(), "Config", "BarcodeTemplate", "Label_200x93-R_Word_Template.docx"),
                BarcodeTemplateLabelType.Label_135x95 => Path.Combine(RA.Common.Util.WebUtil.GetInstallPath(), "Config", "BarcodeTemplate", "Label_135x95-R_Word_Template.docx"),
                BarcodeTemplateLabelType.Label_95x65 => Path.Combine(RA.Common.Util.WebUtil.GetInstallPath(), "Config", "BarcodeTemplate", "Label_95x65-R_Word_Template.docx"),
                BarcodeTemplateLabelType.Label_99x67 => Path.Combine(RA.Common.Util.WebUtil.GetInstallPath(), "Config", "BarcodeTemplate", "Label_99x67-R_Word_Template.docx"),
                BarcodeTemplateLabelType.Label_72x63 => Path.Combine(RA.Common.Util.WebUtil.GetInstallPath(), "Config", "BarcodeTemplate", "Label_72x63-R_Word_Template.docx"),
                _ => throw new ArgumentException($"Unsupported label type: {labelType}"),
            };
        }

        private async Task PreparePropertiesForUpdate(int templateId, List<BarcodeTemplatePropertyDto> newProperties, long currentTime,
            List<RMCustomBarcodeTemplateProperty> propertiesToUpdate,
            List<RMCustomBarcodeTemplateProperty> propertiesToCreate,
            List<int> propertyIdsToDelete)
        {
            var existingProperties = await CustomBarcodeTemplatePropertyDao.GetByTemplateIdAsync(templateId) 
                                     ?? new List<RMCustomBarcodeTemplateProperty>();
            var existingById = existingProperties.ToDictionary(p => p.Id, p => p);

            var seenNewIds = new HashSet<int>();

            if (newProperties != null && newProperties.Any())
            {
                foreach (var dtoProp in newProperties)
                {
                    if (dtoProp.Id > 0 && existingById.TryGetValue(dtoProp.Id, out var existingEntity))
                    {
                        if (!seenNewIds.Add(dtoProp.Id))
                        {
                            continue;
                        }
                        existingEntity.Name = dtoProp.Name;
                        existingEntity.FontSize = dtoProp.FontSize;
                        existingEntity.Position = dtoProp.Position;
                        existingEntity.ModifiedTime = currentTime;
                        propertiesToUpdate.Add(existingEntity);
                    }
                    else
                    {
                        var newEntity = new RMCustomBarcodeTemplateProperty
                        {
                            TemplateId = templateId,
                            Name = dtoProp.Name,
                            FontSize = dtoProp.FontSize,
                            Position = dtoProp.Position,
                            CreatedTime = currentTime,
                            ModifiedTime = currentTime
                        };
                        propertiesToCreate.Add(newEntity);
                        if (dtoProp.Id > 0)
                        {
                            seenNewIds.Add(dtoProp.Id);
                        }
                    }
                }
            }

            var idsToDelete = existingProperties
                .Where(p => !seenNewIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToList();
            if (idsToDelete.Count > 0)
            {
                propertyIdsToDelete.AddRange(idsToDelete);
            }
        }

        private bool ValidateBase64Image(string base64Image, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(base64Image))
                return true;

            var commaIndex = base64Image.IndexOf(',');
            if (commaIndex >= 0)
            {
                base64Image = base64Image[(commaIndex + 1)..];
            }

            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(base64Image);
            }
            catch (FormatException)
            {
                errorMessage = "RM_PRM_BarcodeTemp_FileTypeNotSupported";
                return false;
            }

            if (imageBytes.Length > MaxLogoSizeBytes)
            {
                errorMessage = "RM_PRM_BarcodeTemp_FileSizeExceeded";
                return false;
            }

            if (!IsSupportedImageType(imageBytes))
            {
                errorMessage = "RM_PRM_BarcodeTemp_FileTypeNotSupported";
                return false;
            }

            return true;
        }

        private bool IsSupportedImageType(byte[] bytes)
        {
            if (bytes.Length < 4)
                return false;

            // PNG
            if (bytes.Take(4).SequenceEqual(PngSignature))
                return true;

            // JPEG family: JPG, JPEG, JFIF, PJPEG, PJP
            if (bytes.Take(3).SequenceEqual(JpegSignature))
                return true;

            return false;
        }
    }
}
