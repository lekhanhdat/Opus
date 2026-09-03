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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using AvePoint.RA.Service.Services.TemplateManagement.AuditHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.Common;
using AvePoint.RA.DB.Model.Extension;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.Contract.CustomizeConnector.Model.Columns;
using AngleSharp.Common;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Service.Services.CommonExtension;
using AvePoint.RA.Contract.SharePoint.CustomIndexMetadata;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.DB.Dao.Impl;
//using AvePoint.RA.Contract.Explorer;

namespace AvePoint.RA.Service.Services.TemplateManagement
{
    [Audit]
    public class TemplateManagementService : RMServiceBase, ITemplateManagementService
    {
        private RALogger logger = RALogger.GetInstance(typeof(TemplateManagementService));
        private IGeneralSettingService mGeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private IRMTemplateDao TemplateDao => PlatformWindsorManager.GetService<IRMTemplateDao>();

        private IRMCustomizeConnectorTemplateDao CustomizeConnectorTemplateDao => PlatformWindsorManager.GetService<IRMCustomizeConnectorTemplateDao>();

        private IRMSuiteDao SuiteDao => PlatformWindsorManager.GetService<IRMSuiteDao>();
        private IRMLocationDao LocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();
        private IRMTemplateRelationshipDao TemplateRelationshipDao => PlatformWindsorManager.GetService<IRMTemplateRelationshipDao>();
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private IPhysicalUniqueIdSettingDao PhysicalUniqueIdSettingDao => PlatformWindsorManager.GetService<IPhysicalUniqueIdSettingDao>();
		private IRMSecurityTrimmingHelper trimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();

        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();

        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();

        private IRMCustomMetadataColumnDao CustomMetadataColumnDao => PlatformWindsorManager.GetService<IRMCustomMetadataColumnDao>();

        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
                }
                return _explorerDao;
            }
        }
        public async Task<int> ResetDefaultDataAsync()
        {
            try
            {
                return await TemplateDao.ResetDefaultDataAsync();
            }
            catch (Exception e)
            {
                logger.Error("ResetDefaultData error {0}", e.ToString());
                return -1;
            }
        }

        public async Task<int> InitDefaultDataAsync()
        {
            try
            {
                return await TemplateDao.InitDefaultDataAsync();
            }
            catch (Exception e)
            {
                logger.Error("InitDefaultData error {0}", e.ToString());
                return -1;
            }
        }

        public async Task<TemplateDto> Convert2TemplateDtoAsync(RMTemplate template, bool forBulkUpdate = false)
        {
            var rstDto = new TemplateDto()
            {
                categories = new List<TemplateCategoryDto>()
            };
            GeneralSettingModel gls = await mGeneralSettingService.GetGeneralSettingAsync();

            rstDto.id = template.Id;
            rstDto.uniqueId = template.UniqueId;
            rstDto.name = template.Name;
            rstDto.description = template.Description;
            rstDto.prefix = template.Prefix;
            rstDto.numberOfDigits = template.NumberOfDigits.HasValue ? template.NumberOfDigits.Value : 0;
            rstDto.type = template.Type;
            rstDto.createdOn = template.CreatedOn;
            rstDto.lastModifiedOn = template.LastModifiedOn;
            rstDto.createdOnStr = mGeneralSettingService.ConvertTiksToDateTime(gls, rstDto.createdOn.Ticks, true).DataTime.ToString("MM/dd/yyyy HH:mm:ss");
            rstDto.lastModifiedOnStr = mGeneralSettingService.ConvertTiksToDateTime(gls, rstDto.lastModifiedOn.Ticks, true).DataTime.ToString("MM/dd/yyyy HH:mm:ss");
            if (template.Creater != -1)
            {
                var account = await AccountDao.GetUserByIdAsync(template.Creater);
                //var account = ctx.Account.Where(a => a.Id == template.Creater).FirstOrDefault();
                if (account != null)
                {
                    rstDto.creater = new ToUserInfo()
                    {
                        UserId = account.UserId,
                        DisplayName = account.DisplayName,
                        UserPrincipalName = account.UserPrincipalName,
                    };
                }
            }
            else
            {
                rstDto.creater = new ToUserInfo()
                {
                    UserId = "-1",
                    DisplayName = "Built-in",
                    UserPrincipalName = "Built-in",
                };
            }

            var dbCategories = TemplateDao.LoadCategories(template.UniqueId).ToList();
            var columnSchema = template.ColumnSchema;
            rstDto.categories = AssembleCategory(columnSchema, dbCategories, false, false, forBulkUpdate);
            return rstDto;
        }
        /// <summary>
        /// 当前方法有多个需求，即有界面Template显示，也有Save Template调用，因此需要不同的Template内容。对于页面显示，Category需要国际化后的内容。对于Save Template存储到Records DB中的内容，需要存储国际化Key，因此添加新的参数支持不同的需求。
        /// </summary>
        /// <param name="needI18NKey">true返回的是国际化key，false返回的是国际化value</param>
        private List<TemplateCategoryDto> AssembleCategory(string columnSchema, List<RMTemplateCategory> dbCategories, bool needI18NKey, bool createNewTemplate = false, bool forBulkUpdate = false)
        {
            var schemaTemp = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(columnSchema);
            var schema = new TemplateColumnsSchema();
            schema.Columns = new List<ColumnXmlSchema>();
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
                if (column.UniqueId == new Guid(DefaultColumnIDs.Capability) && column.AllowEdit == false)
                {
                    column.AllowEdit = true;
                }
                schema.Columns.Add(column);
            }
            Dictionary<Guid, List<ColumnXmlSchema>> groups = null;
            if (forBulkUpdate)
            {
                groups = schema.Columns.Where(c => 
                    !DefaultColumnIDs.HideForBulkUpdateIDs.Contains(c.UniqueId.ToString().ToLower()) 
                    && !((c.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild))
                        .GroupBy(c => c.CategoryId).ToDictionary(key => key.Key, c => c.ToList());
            }
            else
            {
                groups = schema.Columns.GroupBy(c => c.CategoryId).ToDictionary(key => key.Key, c => c.ToList());
            }
            var categories = new List<TemplateCategoryDto>();
            foreach (var category in dbCategories)
            {
                var newCategoryUniqueID = Guid.NewGuid();
                var templateColumns = new List<TemplateColumnDto>();
                categories.Add(new TemplateCategoryDto()
                {
                    id = createNewTemplate ? newCategoryUniqueID : category.UniqueId,
                    name = category.Name,
                    allowEdit = !category.IsDefault,
                    columns = templateColumns,
                });
                if (groups.ContainsKey(category.UniqueId))
                {
                    var list = groups[category.UniqueId];
                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        var columnDto = new TemplateColumnDto()
                        {
                            categoryId = createNewTemplate ? newCategoryUniqueID : item.CategoryId,
                            columnName = item.Name,
                            uniqueId = item.UniqueId,
                            required = item.Required,
                            typeId = (int)item.ColumnType,
                            showInEditForm = item.ShowInEditForm,
                            allowEdit = item.AllowEdit,
                            allowSort = item.AllowSort,
                            allowEditSort = item.AllowEditSort(),
                            inheritFromParent = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.InheritFromParentBox) == (int)TemplateInheritSettingEnum.InheritFromParentBox,
                            inheritFromParentFolder = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.InheritFromParentFolder) == (int)TemplateInheritSettingEnum.InheritFromParentFolder,
                            pushToChild = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild,
                            childInheritsValue = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.ChildInheritsValue) == (int)TemplateInheritSettingEnum.ChildInheritsValue,
                            allowModifyValue = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.AllowModifyValue) == (int)TemplateInheritSettingEnum.AllowModifyValue,
                            //pushCategoryId = item.PushToRecordCategoryId,
                            //pushFolderCategoryId = item.PushToFolderCategoryId,
                            pushFoldTemplateCategoriesId = item.pushFoldTemplateCategoriesId,
                            pushRecordTemplateCategoriesId = item.pushRecordTemplateCategoriesId,
                        };
                        //支持老数据
                        if (item.PushToFolderCategoryId != null && item.PushToFolderCategoryId != Guid.Empty && (item.pushFoldTemplateCategoriesId == null || item.pushFoldTemplateCategoriesId.Count == 0))
                        {
                            List<TemplateIdAndCategoryId> pushFoldTemplateCategoriesId = new List<TemplateIdAndCategoryId>();
                            pushFoldTemplateCategoriesId.Add(new TemplateIdAndCategoryId { tempalteId = DefaultTemplateIds.FOLDER_TEMPLATE_ID, categoryId = item.PushToFolderCategoryId.ToString() });
                            columnDto.pushFoldTemplateCategoriesId = pushFoldTemplateCategoriesId;
                        }
                        if (item.PushToRecordCategoryId != null && item.PushToRecordCategoryId != Guid.Empty && (item.pushRecordTemplateCategoriesId == null || item.pushRecordTemplateCategoriesId.Count == 0))
                        {
                            List<TemplateIdAndCategoryId> pushRecordTemplateCategoriesId = new List<TemplateIdAndCategoryId>();
                            pushRecordTemplateCategoriesId.Add(new TemplateIdAndCategoryId { tempalteId = DefaultTemplateIds.RECORD_TEMPLATE_ID, categoryId = item.PushToRecordCategoryId.ToString() });
                            columnDto.pushRecordTemplateCategoriesId = pushRecordTemplateCategoriesId;
                        }

                        //RECO-4254
                        if (item.UniqueId == new Guid(DefaultColumnIDs.Description))
                        {
                            columnDto.allowEdit = true;
                        }
                        switch (item.ColumnType)
                        {
                            case ColumnType.SingleText:
                            case ColumnType.MultipleText:
                            case ColumnType.DateTime:
                            case ColumnType.PeopleOrGroup:
                            case ColumnType.Number:
                                break;
                            case ColumnType.Taxonomy:
                                break;
                            case ColumnType.SingleChoice:
                            case ColumnType.MultipleChoice:
                                columnDto.optionsJSON = needI18NKey ? item.OptionsJSON : GetI18NForOptionsJSON(item.OptionsJSON);
                                columnDto.optionsMaxIdReachedValue = item.OptionsMaxIdReachedValue;
                                break;
                            default:
                                break;
                        }
                        templateColumns.Add(columnDto);
                    }
                }
            }
            if (forBulkUpdate)
            {
                categories = categories.Where(c => c.columns.Count > 0).ToList();
            }
            return categories;
        }

        private string GetI18NForOptionsJSON(string optionsJSON)
        {
            string I18Nson = string.Empty;
            Dictionary<int, string> jsons = new Dictionary<int, string>();
            try
            {
                var oldJsons = JsonConvert.DeserializeObject<Dictionary<int, string>>(optionsJSON);
                foreach (var item in oldJsons)
                {
                    var i18NKeyStatus = item.Value.MapI18NKeyPhysicalOptionsJSON();
                    jsons[item.Key] = i18NKeyStatus;
                }
                I18Nson = JsonConvert.SerializeObject(jsons);
            }
            catch (Exception ex)
            {
                logger.Warn("GetI18NForOptionsJSON error {0}.JSON:{1}.", ex.ToString(), optionsJSON);
                I18Nson = optionsJSON;
            }
            return I18Nson;
        }

        public List<TemplateCategoryDto> GetTemplateCategoryDetail(RMTemplate template)
        {
            var schema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(template.ColumnSchema);
            var groups = schema.Columns.GroupBy(c => c.CategoryId).ToDictionary(key => key.Key, c => c.ToList());
            var dbCategories = TemplateDao.LoadCategories(template.UniqueId).ToList();
            List<TemplateCategoryDto> childCategories = new List<TemplateCategoryDto>();
            foreach (var category in dbCategories)
            {
                var templateColumns = new List<TemplateColumnDto>();

                childCategories.Add(new TemplateCategoryDto()
                {
                    id = category.UniqueId,
                    name = category.Name,
                    allowEdit = !category.IsDefault,
                    columns = templateColumns,
                });
                if (groups.ContainsKey(category.UniqueId))
                {
                    var list = groups[category.UniqueId];
                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        var columnDto = new TemplateColumnDto()
                        {
                            categoryId = item.CategoryId,
                            columnName = item.Name,
                            uniqueId = item.UniqueId,
                            required = item.Required,
                            typeId = (int)item.ColumnType,
                            showInEditForm = item.ShowInEditForm,
                            allowEdit = item.AllowEdit,
                            allowSort = item.AllowSort,
                            allowEditSort = item.AllowEditSort(),
                            inheritFromParent = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.InheritFromParentBox) == (int)TemplateInheritSettingEnum.InheritFromParentBox,
                            inheritFromParentFolder = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.InheritFromParentFolder) == (int)TemplateInheritSettingEnum.InheritFromParentFolder,
                            pushToChild = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild,
                            childInheritsValue = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.ChildInheritsValue) == (int)TemplateInheritSettingEnum.ChildInheritsValue,
                            allowModifyValue = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.AllowModifyValue) == (int)TemplateInheritSettingEnum.AllowModifyValue,
                            pushFoldTemplateCategoriesId = item.pushFoldTemplateCategoriesId,
                            pushRecordTemplateCategoriesId = item.pushRecordTemplateCategoriesId,
                        };

                        if (columnDto.inheritFromParent || columnDto.inheritFromParentFolder)
                        {
                            continue;
                        }
                        //RECO-4254
                        if (item.UniqueId == new Guid(DefaultColumnIDs.Description))
                        {
                            columnDto.allowEdit = true;
                        }
                        switch (item.ColumnType)
                        {
                            case ColumnType.SingleText:
                            case ColumnType.MultipleText:
                            case ColumnType.DateTime:
                            case ColumnType.PeopleOrGroup:
                            case ColumnType.Number:
                                break;
                            case ColumnType.Taxonomy:
                                break;
                            case ColumnType.SingleChoice:
                            case ColumnType.MultipleChoice:
                                columnDto.optionsJSON = item.OptionsJSON;
                                break;
                            default:
                                break;
                        }
                        templateColumns.Add(columnDto);
                    }
                }
            }
            return childCategories;
        }

        public async Task<TemplateDto> LoadTemplateDtoAsync(int id, bool forBulkUpdate = false)
        {
            try
            {
                var template = TemplateDao.GetTemplateById(id);
                return await ConvertTemplate2DtoAsync(template, forBulkUpdate);
            }
            catch (Exception e)
            {
                logger.Error("LoadTemplateDto error {0}", e.ToString());
                return null;
            }
        }

        public async Task<TemplateDto> LoadTemplateDtoAsync(int id, Contract.Explorer.PhysicalObjectDto dto)
        {
            try
            {
                var template = TemplateDao.GetTemplateById(id);
                return await ConvertTemplate2DtoAsync(template, dto);
            }
            catch (Exception e)
            {
                logger.Error("LoadTemplateDto error {0}", e.ToString());
                return null;
            }
        }
        public async Task<TemplateDto> LoadTemplateDtoAsync(Guid uniqueId)
        {
            try
            {
                var template = TemplateDao.GetTemplateByUniqueId(uniqueId);
                return await ConvertTemplate2DtoAsync(template);
            }
            catch (Exception e)
            {
                logger.Error("LoadTemplateDto error {0}", e.ToString());
                return null;
            }
        }

        public async Task<TemplateDto> LoadTemplateDtoAsync(Guid uniqueId, Contract.Explorer.PhysicalObjectDto dto)
        {
            try
            {
                var template = TemplateDao.GetTemplateByUniqueId(uniqueId);
                return await ConvertTemplate2DtoAsync(template, dto);
            }
            catch (Exception e)
            {
                logger.Error("LoadTemplateDto error {0}", e.ToString());
                return null;
            }
        }

        public async Task<TemplateDto> LoadTemplateDtoAsync(SuiteTemplateQueryDto queryDto)
        {
            try
            {
                Guid uniqueId = queryDto.TemplateIdUniqueId;
                var template = TemplateDao.GetTemplateByUniqueId(uniqueId);
                return await ConvertTemplate2DtoAsync(template, queryDto);
            }
            catch (Exception e)
            {
                logger.Error("LoadTemplateDto error {0}", e.ToString());
                return null;
            }
        }

        private async Task<TemplateDto> ConvertTemplate2DtoAsync(RMTemplate template, bool forBulkUpdate = false)
        {
            try
            {
                TemplateDto resultDto = new TemplateDto();
                resultDto = await Convert2TemplateDtoAsync(template, forBulkUpdate);
                return resultDto;
            }
            catch (Exception e)
            {
                logger.Error("LoadTemplateDto error {0}", e.ToString());
                return null;
            }
        }

        private async Task<TemplateDto> ConvertTemplate2DtoAsync(RMTemplate template, Contract.Explorer.PhysicalObjectDto dto)
        {
            try
            {
                TemplateDto resultDto = new TemplateDto();
                resultDto = await Convert2TemplateDtoAsync(template);
                if (dto.NodeType == RMNodeType.PhyFile && dto.BoxId != Guid.Empty)
                {
                    AddPushColumnToFold(resultDto, template, dto);
                }
                else if (dto.NodeType == RMNodeType.PhyRecord)
                {
                    AddPushColumnToRecord(resultDto, template, dto);
                }
                return resultDto;
            }
            catch (Exception e)
            {
                logger.Error("LoadTemplateDto error {0}", e.ToString());
                return null;
            }
        }

        public void AddPushColumnToFold(TemplateDto resultDto ,RMTemplate template, Contract.Explorer.PhysicalObjectDto dto)
        {
            Record box = ExplorerDao.GetPhysicalRecordById(dto.BoxId);
            RMTemplate boxTemplate = TemplateDao.GetTemplateById(box.TemplateId);
            var columnSchema = boxTemplate.ColumnSchema;
            TemplateColumnsSchema schema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(columnSchema);
            List<ColumnXmlSchema> columns = schema.Columns;
            for (int i = 0; i < columns.Count; i++)
            {
                var item = columns[i];
                if ((item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)
                {
                    List<TemplateIdAndCategoryId> pushFoldTemplateCategoriesId = item.pushFoldTemplateCategoriesId;
                    if (pushFoldTemplateCategoriesId != null && pushFoldTemplateCategoriesId.Count > 0)
                    {
                        logger.Info($"get push column, {item.Name}, {item.TemplateInheritSetting}, {item.CategoryId}");
                        TemplateIdAndCategoryId templateCategoryId = pushFoldTemplateCategoriesId.Find(t => t.tempalteId.ToLower() == template.UniqueId.ToString().ToLower());
                        if (templateCategoryId != null)
                        {
                            foreach (var category in resultDto.categories)
                            {
                                if (category.id.ToString().ToLower() == templateCategoryId.categoryId.ToLower())
                                {
                                    bool isInheritBox = true;
                                    TemplateColumnDto columnDto = ConvertToPageColumnDto(item, isInheritBox);
                                    category.columns.Add(columnDto);
                                    logger.Info($"add push column to category {category.name}, {item.Name}");
                                }
                            }
                        }
                        //如果没有存储当前sub template的信息,则把push column add到默认category里 即第一个
                        else
                        {
                            bool isInheritBox = true;
                            TemplateColumnDto columnDto = ConvertToPageColumnDto(item, isInheritBox);
                            resultDto.categories[0].columns.Add(columnDto);
                            logger.Info($"add push column to default category, {item.Name}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// edit template的时候 把parent template的pushcolumn显示到子template上
        /// </summary>
        /// <param name="resultDto"></param>
        /// <param name="template"></param>
        /// <param name="queryDto"></param>
        public void AddPushColumnToFoldTemplate(TemplateDto resultDto, SuiteTemplateQueryDto queryDto)
        {
            //if (queryDto.BoxTemplateUniqueId == Guid.Empty)
            //{
            //    //如果fold是suite的root template 直接返回
            //    return;
            //}
            //RMTemplate boxTemplate = TemplateDao.GetTemplateByUniqueId(queryDto.TemplateIdUniqueId);
            if (queryDto.TemplateIdList.Count <= 2) return;
            if (!int.TryParse(queryDto.TemplateIdList.ElementAt(queryDto.TemplateIdList.Count - 2), out int parentTemplateId)) return;
            RMTemplate boxTemplate = TemplateDao.GetTemplateById(parentTemplateId);
            if (boxTemplate.Type != TemplateType.Box) return;

            var columnSchema = boxTemplate.ColumnSchema;
            TemplateColumnsSchema schema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(columnSchema);
            List<ColumnXmlSchema> columns = schema.Columns;
            for (int i = 0; i < columns.Count; i++)
            {
                var item = columns[i];
                if ((item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)
                {
                    List<TemplateIdAndCategoryId> pushFoldTemplateCategoriesId = item.pushFoldTemplateCategoriesId;
                    if (pushFoldTemplateCategoriesId != null && pushFoldTemplateCategoriesId.Count > 0)
                    {
                        TemplateIdAndCategoryId templateCategoryId = pushFoldTemplateCategoriesId.Find(t => t.tempalteId == resultDto.uniqueId.ToString());
                        if (templateCategoryId != null)
                        {
                            foreach (var category in resultDto.categories)
                            {
                                if (category.id.ToString() == templateCategoryId.categoryId)
                                {
                                    bool isDisplayTempalte = true;
                                    bool isInheritBox = true;
                                    TemplateColumnDto columnDto = ConvertToPageColumnDto(item, isInheritBox, isDisplayTempalte);
                                    category.columns.Add(columnDto);
                                }
                            }
                        }
                        //如果没有存储当前sub template的信息,则把push column add到默认category里 即第一个
                        else
                        {
                            bool isDisplayTempalte = true;
                            bool isInheritBox = true;
                            TemplateColumnDto columnDto = ConvertToPageColumnDto(item, isInheritBox, isDisplayTempalte);
                            resultDto.categories[0].columns.Add(columnDto);
                        }
                    }
                }
            }
        }

        public void AddPushColumnToRecord(TemplateDto resultDto, RMTemplate template, Contract.Explorer.PhysicalObjectDto dto)
        {
            if (dto.FileId != Guid.Empty)
            {
                Record fold = ExplorerDao.GetPhysicalRecordById(dto.FileId);
                RMTemplate foldTemplate = TemplateDao.GetTemplateById(fold.TemplateId);
                var columnSchema = foldTemplate.ColumnSchema;
                TemplateColumnsSchema schema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(columnSchema);
                List<ColumnXmlSchema> columns = schema.Columns;
                for (int i = 0; i < columns.Count; i++)
                {
                    var item = columns[i];
                    if ((item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)
                    {
                        List<TemplateIdAndCategoryId> pushRecordTemplateCategoriesId = item.pushRecordTemplateCategoriesId;
                        if (pushRecordTemplateCategoriesId != null && pushRecordTemplateCategoriesId.Count > 0)
                        {
                            TemplateIdAndCategoryId templateCategoryId = pushRecordTemplateCategoriesId.Find(t => t.tempalteId == template.UniqueId.ToString());
                            if (templateCategoryId != null)
                            {
                                foreach (var category in resultDto.categories)
                                {
                                    if (category.id.ToString() == templateCategoryId.categoryId)
                                    {
                                        bool isInheritBox = false;
                                        TemplateColumnDto columnDto = ConvertToPageColumnDto(item, isInheritBox);
                                        category.columns.Add(columnDto);
                                    }
                                }
                            }
                            //如果没有存储当前sub template的信息,则把push column add到默认category里 即第一个
                            else
                            {
                                bool isInheritBox = false;
                                TemplateColumnDto columnDto = ConvertToPageColumnDto(item, isInheritBox);
                                resultDto.categories[0].columns.Add(columnDto);
                            }
                        }
                    }
                }
            }
            if (dto.BoxId != Guid.Empty)
            {
                Record box = ExplorerDao.GetPhysicalRecordById(dto.BoxId);
                RMTemplate boxTemplate = TemplateDao.GetTemplateById(box.TemplateId);
                var columnSchema = boxTemplate.ColumnSchema;
                TemplateColumnsSchema schema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(columnSchema);
                List<ColumnXmlSchema> columns = schema.Columns;
                for (int i = 0; i < columns.Count; i++)
                {
                    var item = columns[i];
                    if ((item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)
                    {
                        List<TemplateIdAndCategoryId> pushRecordTemplateCategoriesId = item.pushRecordTemplateCategoriesId;
                        if (pushRecordTemplateCategoriesId != null && pushRecordTemplateCategoriesId.Count > 0)
                        {
                            TemplateIdAndCategoryId templateCategoryId = pushRecordTemplateCategoriesId.Find(t => t.tempalteId == template.UniqueId.ToString());
                            if (templateCategoryId != null)
                            {
                                foreach (var category in resultDto.categories)
                                {
                                    if (category.id.ToString() == templateCategoryId.categoryId)
                                    {
                                        bool isInheritBox = true;
                                        TemplateColumnDto columnDto = ConvertToPageColumnDto(item, isInheritBox);
                                        category.columns.Add(columnDto);
                                    }
                                }
                            }
                            //如果没有存储当前sub template的信息,则把push column add到默认category里 即第一个
                            else
                            {
                                bool isInheritBox = true;
                                TemplateColumnDto columnDto = ConvertToPageColumnDto(item, isInheritBox);
                                resultDto.categories[0].columns.Add(columnDto);
                            }
                        }
                    }
                }
            }
        }

        public void AddPushColumnToRecordTemplate(TemplateDto resultDto, SuiteTemplateQueryDto queryDto)
        {
            //if (queryDto.FolderTemplateUniqueId != Guid.Empty)
            if (queryDto.TemplateIdList.Count > 2 && int.TryParse(queryDto.TemplateIdList.ElementAt(queryDto.TemplateIdList.Count -2), out int folderTemplateId))  //parent template id
            {
                //RMTemplate foldTemplate = TemplateDao.GetTemplateByUniqueId(queryDto.FolderTemplateUniqueId);
                RMTemplate foldTemplate = TemplateDao.GetTemplateById(folderTemplateId);
                if (foldTemplate.Type == TemplateType.Folder)
                {
                    var columnSchema = foldTemplate.ColumnSchema;
                    TemplateColumnsSchema schema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(columnSchema);
                    List<ColumnXmlSchema> columns = schema.Columns;
                    for (int i = 0; i < columns.Count; i++)
                    {
                        var item = columns[i];
                        if ((item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)
                        {
                            List<TemplateIdAndCategoryId> pushRecordTemplateCategoriesId = item.pushRecordTemplateCategoriesId;
                            if (pushRecordTemplateCategoriesId != null && pushRecordTemplateCategoriesId.Count > 0)
                            {
                                TemplateIdAndCategoryId templateCategoryId = pushRecordTemplateCategoriesId.Find(t => t.tempalteId == resultDto.uniqueId.ToString());
                                if (templateCategoryId != null)
                                {
                                    foreach (var category in resultDto.categories)
                                    {
                                        if (category.id.ToString() == templateCategoryId.categoryId)
                                        {
                                            bool isDisplayTempalte = true;
                                            bool isInheritBox = false;
                                            TemplateColumnDto columnDto = ConvertToPageColumnDto(item, isInheritBox, isDisplayTempalte);
                                            category.columns.Add(columnDto);
                                        }
                                    }
                                }
                                //如果没有存储当前sub template的信息,则把push column add到默认category里 即第一个
                                else
                                {
                                    bool isDisplayTempalte = true;
                                    bool isInheritBox = false;
                                    TemplateColumnDto columnDto = ConvertToPageColumnDto(item, isInheritBox, isDisplayTempalte);
                                    resultDto.categories[0].columns.Add(columnDto);
                                }
                            }
                            #region
                            //支持老数据
                            //else
                            //{
                            //    if (item.PushToRecordCategoryId != null && item.PushToRecordCategoryId != Guid.Empty && (item.pushRecordTemplateCategoriesId == null || item.pushRecordTemplateCategoriesId.Count == 0))
                            //    {
                            //        List<TemplateIdAndCategoryId> pushRecordTemplateCategoriesId1= new List<TemplateIdAndCategoryId>();
                            //        pushRecordTemplateCategoriesId1.Add(new TemplateIdAndCategoryId { tempalteId = DefaultTemplateIds.RECORD_TEMPLATE_ID, categoryId = item.PushToRecordCategoryId.ToString() });
                            //        //columnDto.pushRecordTemplateCategoriesId = pushRecordTemplateCategoriesId;
                            //        TemplateIdAndCategoryId templateCategoryId = pushRecordTemplateCategoriesId.Find(t => t.tempalteId == resultDto.uniqueId.ToString());
                            //        if (templateCategoryId != null)
                            //        {
                            //            foreach (var category in resultDto.categories)
                            //            {
                            //                if (category.id.ToString() == templateCategoryId.categoryId)
                            //                {
                            //                    bool isDisplayTempalte = true;
                            //                    bool isInheritBox = false;
                            //                    TemplateColumnDto columnDto = ConvertToPageColumnDto(item, isInheritBox, isDisplayTempalte);
                            //                    category.columns.Add(columnDto);
                            //                }
                            //            }
                            //        }
                            //        //如果没有存储当前sub template的信息,则把push column add到默认category里 即第一个
                            //        else
                            //        {
                            //            bool isDisplayTempalte = true;
                            //            bool isInheritBox = false;
                            //            TemplateColumnDto columnDto = ConvertToPageColumnDto(item, isInheritBox, isDisplayTempalte);
                            //            resultDto.categories[0].columns.Add(columnDto);
                            //        }
                            //    }
                            //}
                            #endregion
                        }
                    }
                }
            }
            //if (queryDto.BoxTemplateUniqueId != Guid.Empty)
            if (queryDto.TemplateIdList.Count > 3 && int.TryParse(queryDto.TemplateIdList.ElementAt(queryDto.TemplateIdList.Count - 3), out int boxTemplateId))  //grand father template id
            {
                //RMTemplate boxTemplate = TemplateDao.GetTemplateByUniqueId(queryDto.BoxTemplateUniqueId);
                RMTemplate boxTemplate = TemplateDao.GetTemplateById(boxTemplateId);
                if (boxTemplate.Type == TemplateType.Box)
                {
                    var columnSchema = boxTemplate.ColumnSchema;
                    TemplateColumnsSchema schema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(columnSchema);
                    List<ColumnXmlSchema> columns = schema.Columns;
                    for (int i = 0; i < columns.Count; i++)
                    {
                        var item = columns[i];
                        if ((item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)
                        {
                            List<TemplateIdAndCategoryId> pushRecordTemplateCategoriesId = item.pushRecordTemplateCategoriesId;
                            if (pushRecordTemplateCategoriesId != null && pushRecordTemplateCategoriesId.Count > 0)
                            {
                                TemplateIdAndCategoryId templateCategoryId = pushRecordTemplateCategoriesId.Find(t => t.tempalteId == resultDto.uniqueId.ToString());
                                if (templateCategoryId != null)
                                {
                                    foreach (var category in resultDto.categories)
                                    {
                                        if (category.id.ToString() == templateCategoryId.categoryId)
                                        {
                                            bool isDisplayTempalte = true;
                                            bool isInheritBox = true;
                                            TemplateColumnDto columnDto = ConvertToPageColumnDto(item, isInheritBox, isDisplayTempalte);
                                            category.columns.Add(columnDto);
                                        }
                                    }
                                }
                                //如果没有存储当前sub template的信息,则把push column add到默认category里 即第一个
                                else
                                {
                                    bool isDisplayTempalte = true;
                                    bool isInheritBox = true;
                                    TemplateColumnDto columnDto = ConvertToPageColumnDto(item, isInheritBox, isDisplayTempalte);
                                    resultDto.categories[0].columns.Add(columnDto);
                                }
                            }
                        }
                    }
                }
            }
        }

        public TemplateColumnDto ConvertToPageColumnDto(ColumnXmlSchema item,bool isInheritBox,bool isDisplayTemplate = false)
        {
            var columnDto = new TemplateColumnDto()
            {
                categoryId = item.CategoryId,
                columnName = item.Name,
                uniqueId = item.UniqueId,
                required = item.Required,
                typeId = (int)item.ColumnType,
                showInEditForm = item.ShowInEditForm,
                allowEdit = item.AllowEdit,
                allowSort = item.AllowSort,
                allowEditSort = item.AllowEditSort(),
                inheritFromParent = isInheritBox,
                inheritFromParentFolder = !isInheritBox,
                //pushToChild = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild,
                //childInheritsValue = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.ChildInheritsValue) == (int)TemplateInheritSettingEnum.ChildInheritsValue,
                allowModifyValue = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.AllowModifyValue) == (int)TemplateInheritSettingEnum.AllowModifyValue,
                pushFoldTemplateCategoriesId = item.pushFoldTemplateCategoriesId,
                pushRecordTemplateCategoriesId = item.pushRecordTemplateCategoriesId,
            };
            //RECO-4254
            if (item.UniqueId == new Guid(DefaultColumnIDs.Description))
            {
                columnDto.allowEdit = true;
            }
            if (isDisplayTemplate)
            {
                //如果是edit template，则把push的column设置成不可编辑
                columnDto.allowEdit = false;
            }
            switch (item.ColumnType)
            {
                case ColumnType.SingleText:
                case ColumnType.MultipleText:
                case ColumnType.DateTime:
                case ColumnType.PeopleOrGroup:
                case ColumnType.Number:
                    break;
                case ColumnType.Taxonomy:
                    break;
                case ColumnType.SingleChoice:
                case ColumnType.MultipleChoice:
                    columnDto.optionsJSON = item.OptionsJSON;
                    columnDto.optionsMaxIdReachedValue = item.OptionsMaxIdReachedValue;
                    break;
                default:
                    break;
            }
            return columnDto;
        }

        private async Task<TemplateDto> ConvertTemplate2DtoAsync(RMTemplate template, SuiteTemplateQueryDto queryDto)
        {
            try
            {
                TemplateDto resultDto = new TemplateDto();
                resultDto = await Convert2TemplateDtoAsync(template);
                if (template != null)
                {
                    if (template.Type == TemplateType.Box)
                    {
                        //queryDto.BoxTemplateUniqueId = queryDto.TemplateIdUniqueId;
                        //List<RMTemplate> foldTemplates = TemplateDao.GetChildTemplatesByParent(queryDto);
                        List<RMTemplate> foldTemplates = GetChildTemplatesByParent(queryDto.TemplateIdUniqueId, queryDto.TemplateIdList);
                        foreach (RMTemplate foldTemplate in foldTemplates)
                        {
                            List<TemplateCategoryDto> categories = GetTemplateCategoryDetail(foldTemplate);
                            TemplatesContentCategoriesDto childrenTemplateCategories = new TemplatesContentCategoriesDto();
                            childrenTemplateCategories.uniqueId = foldTemplate.UniqueId;
                            childrenTemplateCategories.templateName = foldTemplate.Name;
                            childrenTemplateCategories.type = foldTemplate.Type;
                            childrenTemplateCategories.currentCategories = categories;

                            //queryDto.FolderTemplateUniqueId = foldTemplate.UniqueId;
                            //List<RMTemplate> recordTemplates = TemplateDao.GetChildTemplatesByParent(queryDto);
                            var folderTemplateIdList = new List<string>();
                            folderTemplateIdList.AddRange(queryDto.TemplateIdList);
                            folderTemplateIdList.Add(foldTemplate.Id.ToString());
                            List<RMTemplate> recordTemplates = GetChildTemplatesByParent(foldTemplate.UniqueId, folderTemplateIdList);
                            foreach (RMTemplate recordTemplate in recordTemplates)
                            {
                                List<TemplateCategoryDto> secondChildrenCategories = GetTemplateCategoryDetail(recordTemplate);
                                TemplatesContentCategoriesDto secondTemplateCategories = new TemplatesContentCategoriesDto();
                                secondTemplateCategories.uniqueId = recordTemplate.UniqueId;
                                secondTemplateCategories.templateName = recordTemplate.Name;
                                secondTemplateCategories.type = recordTemplate.Type;
                                secondTemplateCategories.currentCategories = secondChildrenCategories;
                                if (childrenTemplateCategories.childrenCategories == null)
                                {
                                    childrenTemplateCategories.childrenCategories = new List<TemplatesContentCategoriesDto>();
                                }
                                childrenTemplateCategories.childrenCategories.Add(secondTemplateCategories);
                            }
                            if (resultDto.childTemplateCategories == null)
                            {
                                resultDto.childTemplateCategories = new List<TemplatesContentCategoriesDto>();
                            }
                            resultDto.childTemplateCategories.Add(childrenTemplateCategories);
                        }
                    }
                    else if (template.Type == TemplateType.Folder)
                    {
                        //queryDto.FolderTemplateUniqueId = queryDto.TemplateIdUniqueId;
                        //List<RMTemplate> recordTemplates = TemplateDao.GetChildTemplatesByParent(queryDto, true);
                        List<RMTemplate> recordTemplates = GetChildTemplatesByParent(queryDto.TemplateIdUniqueId, queryDto.TemplateIdList);

                        foreach (RMTemplate recordTemplate in recordTemplates)
                        {
                            List<TemplateCategoryDto> categories = GetTemplateCategoryDetail(recordTemplate);
                            TemplatesContentCategoriesDto childrenTemplateCategories = new TemplatesContentCategoriesDto();
                            childrenTemplateCategories.uniqueId = recordTemplate.UniqueId;
                            childrenTemplateCategories.templateName = recordTemplate.Name;
                            childrenTemplateCategories.type = recordTemplate.Type;
                            childrenTemplateCategories.currentCategories = categories;
                            if (resultDto.childTemplateCategories == null)
                            {
                                resultDto.childTemplateCategories = new List<TemplatesContentCategoriesDto>();
                            }
                            resultDto.childTemplateCategories.Add(childrenTemplateCategories);
                        }
                    }
                }
                ArgumentCheck.NotNull(template, nameof(template));
                if (template.Type == TemplateType.Folder)
                {
                    AddPushColumnToFoldTemplate(resultDto, queryDto);
                }
                else if (template.Type == TemplateType.Records)
                {
                    AddPushColumnToRecordTemplate(resultDto, queryDto);
                }
                return resultDto;
            }
            catch (Exception e)
            {
                logger.Error("LoadTemplateDto error {0}", e.ToString());
                return null;
            }
        }

        private List<RMTemplate> GetChildTemplatesByParent(Guid templateUniqueId, List<string> templateIdList)
        {
            var templateIds = TemplateRelationshipDao.GetAllByParent(templateUniqueId, templateIdList);
            return TemplateDao.GetTemplateByUniqueIds(templateIds);
        }

        public async Task<TemplateDto> GetTemplateByNodeTypeAsync(RMNodeLevel nodeType)
        {
            try
            {
                var templateType = TemplateType.Records;
                switch (nodeType)
                {
                    case RMNodeLevel.PhysicalRecord:
                        templateType = TemplateType.Records;
                        break;
                    case RMNodeLevel.PhysicalFile:
                        templateType = TemplateType.Folder;
                        break;
                    case RMNodeLevel.PhysicalBox:
                        templateType = TemplateType.Box;
                        break;
                    default:
                        break;
                }
                var template = TemplateDao.GetTemplateByTemplateType(templateType);
                return await Convert2TemplateDtoAsync(template);
            }
            catch (Exception e)
            {
                logger.Error("GetTemplateByNodeType error {0}", e.ToString());
                return null;
            }
        }

        public async Task<string> LoadTemplateDatasAsync(int id, bool forBulkUpdate = false)
        {
            return JsonConvert.SerializeObject(await LoadTemplateDtoAsync(id, forBulkUpdate));
        }

        public async Task<string> LoadTemplateDatasAsync(Guid uniqueId)
        {
            return JsonConvert.SerializeObject(await LoadTemplateDtoAsync(uniqueId));
        }

        public async Task<string> LoadTemplateDatasAsync(SuiteTemplateQueryDto queryDto)
        {
            return JsonConvert.SerializeObject(await LoadTemplateDtoAsync(queryDto));
        }

        public bool ValidateDuplicateColumn(int typeId, string columnName)
        {
            try
            {
                List<RMTemplate> templates = TemplateDao.GetTemplate();
                foreach (RMTemplate template in templates)
                {
                    //template.ColumnSchema;
                    TemplateColumnsSchema schemaTemp = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(template.ColumnSchema);
                    if (schemaTemp != null && schemaTemp.Columns != null)
                    {
                        foreach (ColumnXmlSchema column in schemaTemp.Columns)
                        {
                            ///客户的旧数据存在同名不同类型，  Multi 和Single Line Text两种类型允许创建同名， 避免两种类型都无法创建  from CI Sep
                            if (!isAllTextColumn(typeId, (int)column.ColumnType) && (int)column.ColumnType != typeId && string.Equals(I18NEntity.GetString(column.Name), columnName, StringComparison.OrdinalIgnoreCase))
                            {
                                logger.Info("Duplicate column {0} in template {1}, with different type", column.Name, template.Name);
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn(e.Message);
            }
            return true;
        }

        private bool isAllTextColumn(int type1, int type2)
        {
            if((type1 == (int)ColumnType.SingleText || type1 == (int)ColumnType.MultipleText) 
                && (type2 == (int)ColumnType.SingleText || type2 == (int)ColumnType.MultipleText))
            {
                return true;
            }
            return false;
        }

        public string LoadChildTemplateCategory(int id)
        {
            return JsonConvert.SerializeObject(LoadTemplateAsync(id));
        }

        public async Task<TemplateDto> LoadTemplateAsync(int id)
        {
            try
            {
                RMTemplate template = TemplateDao.GetTemplateById(id);
                RMTemplate childTemplate = new RMTemplate();
                if (template != null)
                {
                    if (template.Type == TemplateType.Box)
                    {
                        childTemplate = TemplateDao.GetTemplateByTemplateType(TemplateType.Folder);
                    }
                    else if (template.Type == TemplateType.Folder)
                    {
                        childTemplate = TemplateDao.GetTemplateByTemplateType(TemplateType.Records);
                    }
                }
                if (childTemplate != null)
                {
                    return await Convert2TemplateDtoAsync(childTemplate);
                }
                return await Convert2TemplateDtoAsync(childTemplate);
            }
            catch (Exception e)
            {
                logger.Error("LoadTemplateDto error {0}", e.ToString());
                return null;
            }
        }


        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.TemplateManagement, Action = AuditAction.EditTemplate, BeforeHandler = typeof(TemplateManagementBeforeAuditHandler), AfterHandler = typeof(TemplateManagementAfterAuditHandler))]
        public (TemplateDto dto, bool result) SaveTemplateWithColumns(TemplateDto dto)
        {
            try
            {
                var hasErrorMessage = "Please check whether or not the template configurations are correct.";

                var setting = PhysicalUniqueIdSettingDao.LoadingUniqueIdSetting();
                if (!(setting?.IsGlobalSetting).GetValueOrDefault())
                {
                    if (!CheckTemplateData(dto))
                    {
                        throw new Exception(hasErrorMessage);
                    }
                }
                var result = TemplateDao.SaveTemplateWithColumns(dto);
                return (dto, result);
            }
            catch (Exception e)
            {
                logger.Error("SaveTemplateWithColumns error {0}", e.ToString());
                return (new(), false);
            }
        }

        #region remove code
        //public bool CheckColumnsSameName(string columnName, int templateId)
        //{
        //    try
        //    {
        //        return TemplateDao.CheckColumnsSameName(columnName, templateId);
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error("CheckColumnsSameName error {0}", e.ToString());
        //        return false;
        //    }
        //}
        //public int CreateCategory(int parentTemplateId, string name, TemplateType type)
        //{
        //    try
        //    {
        //        return TemplateDao.CreateCategory(parentTemplateId, name, type);
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error("CreateCategory error {0}", e.ToString());
        //        return -1;
        //    }
        //}

        //public bool CreateCategory(string name)
        //{
        //    try
        //    {
        //        return TemplateDao.CreateCategory(name);
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error("CreateCategory error {0}", e.ToString());
        //        return false;
        //    }
        //}
        #endregion

        public async Task<string> GetAllTemplateDatasAsync()
        {
            GeneralSettingModel gls = await mGeneralSettingService.GetGeneralSettingAsync();
            string dtFormat = mGeneralSettingService.GetDateTimeFormat(gls);
            List<TemplateDto> results = new List<TemplateDto>();
            List<RMTemplate> templates = TemplateDao.GetTemplate();
            foreach (RMTemplate template in templates)
            {
                TemplateDto dto = new TemplateDto();
                dto.id = template.Id;
                dto.name = template.Name;
                dto.description = template.Description;
                dto.prefix = template.Prefix;
                dto.numberOfDigits = template.NumberOfDigits.HasValue ? template.NumberOfDigits.Value : 0;
                dto.type = template.Type;
                dto.createdOn = template.CreatedOn;
                dto.lastModifiedOn = template.LastModifiedOn;
                dto.createdOnStr = mGeneralSettingService.ConvertTiksToDateTime(gls, template.CreatedOn.Ticks, true).DataTime.ToString(dtFormat);
                dto.lastModifiedOnStr = mGeneralSettingService.ConvertTiksToDateTime(gls, template.LastModifiedOn.Ticks, true).DataTime.ToString(dtFormat);
                if (template.Modifier != -1)
                {
                    var account = await AccountDao.GetUserByIdAsync(template.Modifier);
                    if (account != null)
                    {
                        dto.modifier = new ToUserInfo()
                        {
                            UserId = account.UserId,
                            DisplayName = account.DisplayName,
                            UserPrincipalName = account.UserPrincipalName,
                        };
                    }
                }
                else
                {
                    dto.modifier = new ToUserInfo()
                    {
                        UserId = "-1",
                        DisplayName = "Built-in",
                        UserPrincipalName = "Built-in",
                    };
                }

                results.Add(dto);
            }
            return JsonConvert.SerializeObject(results);

        }

        public async Task<TemplateDto> GetTemplateDtosByNameAsync(string templateName)
        {
            var template = TemplateDao.GetTemplateByName(templateName);
            return await Convert2TemplateDtoAsync(template);
        }

        public async Task<List<TemplateDto>> GetAllTemplateDtosAsync()
        {
            List<TemplateDto> results = new List<TemplateDto>();
            List<RMTemplate> templates = TemplateDao.GetTemplate();
            foreach (RMTemplate rm in templates)
            {
                results.Add(await Convert2TemplateDtoAsync(rm));
            }
            return results;
        }

        public async System.Threading.Tasks.Task UpdateIndexPolicyAsync()
        {
            var columns = await GetAllColumnsAsync();
            var indexPathList = CosmosIndexPolicyUtil.GetBuiltinPhysicalColumnIndexPolicyPath();

            foreach (var column in columns)
            {
                if (!(column.AllowSort.HasValue && column.AllowSort.Value)) continue;
                var path = CosmosIndexPolicyUtil.GetDynamicCustomColumnIndexPolicyPath(column.ColumnType, column.UniqueId);
                if (string.IsNullOrEmpty(path) || indexPathList.Contains(path)) continue;
                indexPathList.Add(path);
            }
            if (indexPathList.Count > 0)
            {
                ExplorerDao.AddPath2IndexPolicy(indexPathList);
            }
        }
        public async Task<List<TemplateColumn4Display>> GetAllColumnsAsync()
        {
            var results = new List<TemplateColumn4Display>();
            List<RMTemplate> templates = TemplateDao.GetTemplate();
            Dictionary<string, string> templateDic = new Dictionary<string, string>(); //key : template unique id, value: template name
            foreach (RMTemplate rm in templates)
            {
                templateDic[rm.UniqueId.ToString().ToLower()] = I18NEntity.GetString(rm.Name); //template id and name dic
                List<TemplateColumn4Display> tempColumns = rm.GetColumnList4Display();
                foreach (var displyColumn in tempColumns)
                {
                    //Combine single & multi line text column, if the same column name, from CI Sep 2021
                    var loadedColumn = results.FirstOrDefault(r => r.UniqueId == displyColumn.UniqueId
                    || (string.Equals(r.ColumnName, displyColumn.ColumnName, StringComparison.OrdinalIgnoreCase) && (r.ColumnType == displyColumn.ColumnType || IsBothTextColumn(r.ColumnType, displyColumn.ColumnType))));

                    if (loadedColumn == null)
                    {
						displyColumn.NameHash = displyColumn.GetNameHash();
						results.Add(displyColumn);
                        continue;
                    }
                    if (displyColumn.AllowSort.HasValue && displyColumn.AllowSort.Value)
                    {
                        //as long as the column is allow sort in one template, set value to 'true'
                        loadedColumn.AllowSort = true; 
                    }
                    if (displyColumn.UniqueId != loadedColumn.UniqueId)
                    {
                        //same name, different id
                        loadedColumn.AllowSort = false; //do not allow sort when there are multipule columns with same name but different id.
                        loadedColumn.IdsWithDuplicateName.Add(displyColumn.UniqueId);
                        if (loadedColumn.ColumnType == ColumnType.SingleChoice || loadedColumn.ColumnType == ColumnType.MultipleChoice)
                        {
                            loadedColumn.OptionsJSON = this.MixOptionsJson(loadedColumn.OptionsJSON, displyColumn.OptionsJSON);
                        }
                    }
                    loadedColumn.NameHash = loadedColumn.GetNameHash();
					//assign template id
					var relatedTemplateIds = displyColumn.Templates.Select(o => o.Id);
                    var exceptionTemplateIds = relatedTemplateIds.Except(loadedColumn.Templates.Select(o => o.Id));
                    if (exceptionTemplateIds != null && exceptionTemplateIds.Count() > 0)
                    {
                        loadedColumn.Templates.AddRange(exceptionTemplateIds.Select(o =>
                        new NameAndIdDto { Id = o }));
                    }
                }
            }
            //assign template name
            foreach(var r in results)
            {
                //Get real column name for sorting, from CI Sep 2021
                r.ColumnName = I18NEntity.GetString(r.ColumnName);
                foreach (var template in r.Templates)
                {
                    if (templateDic.ContainsKey(template.Id))
                    {
                        template.Name = templateDic[template.Id];
                    }
                }
            }

			if (trimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.ControlPanelAdmin).GetAwaiter().GetResult())
			{
				var customizeConnectorColumns = await GetCustomizeConnectorColumnsAsync();                
                results.AddRange(customizeConnectorColumns);
			}

            if(trimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOEnduser).GetAwaiter().GetResult() 
                || trimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsEndUser).GetAwaiter().GetResult())
            {
                var customMetadataColumns = await GetCustomMetadataColumnsAsync();
                results.AddRange(customMetadataColumns);
            }

			//Sort column by name, from CI Sep 2021
			return results.OrderBy(a => a.ColumnName).ToList();
            //return results;
        }

        private async Task<List<TemplateColumn4Display>> GetCustomizeConnectorColumnsAsync()
        {
            var res = new List<TemplateColumn4Display>();
            var customizeConnectorTemplates = await CustomizeConnectorTemplateDao.GetAll();
            foreach(var customizeConnectorTemplate in customizeConnectorTemplates)
            {
                foreach(var column in customizeConnectorTemplate.Columns)
                {
                    if(column.Origin == Contract.CustomizeConnector.Enums.CustomizeConnectorOrigin.BuildIn)
                    {
                        continue;
                    }

                    var templateColumn = new TemplateColumn4Display
                    {
                        ColumnType = column.Type,
                        ColumnName = column.Name + $"({customizeConnectorTemplate.Name} Connector)",
                        IdsWithDuplicateName = new List<Guid> { column.Id },
                        NameHash = column.Id,
                        UniqueId = column.Id,
                        AllowSort = false,
                    };

                    if(column.Type == ColumnType.SingleChoice || column.Type == ColumnType.MultipleChoice)
                    {
                        var options = JsonConvert.DeserializeObject<List<CustomizeConnectorChoiceColumnOption>>(column.Extention);
                        var optionDic = options.ToDictionary(item => item.Value, item => item.Name);
                        templateColumn.OptionsJSON = JsonConvert.SerializeObject(optionDic);
                    }

                    res.Add(templateColumn);
                }
            }

            return res;
        }

        public async Task<List<TemplateColumn4Display>> GetCustomMetadataColumnsAsync()
        {
            var res = new List<TemplateColumn4Display>();
            _ = RMKeyValueDao.TryGetBoolValue(KeyNameCollection.IsEnableCustomIndexMetadata, out var isEnable);
            if (isEnable)
            {
                var customMetadataColumns = await CustomMetadataColumnDao.GetInUsedCustomMetadataColumnsAsync();
                foreach (var column in customMetadataColumns)
                {
                    var templateColumn = new TemplateColumn4Display
                    {
                        ColumnType = GetColumnType(column.ColumnType),
                        ColumnName = column.ColumnName,
                        IdsWithDuplicateName = new List<Guid> { column.UniqueId },
                        NameHash = column.UniqueId,
                        UniqueId = column.UniqueId,
                        AllowSort = column.EnableSort,
                    };

                    res.Add(templateColumn);

                }
            }

            return res;
        }

        private ColumnType GetColumnType(CustomColumnType columnType)
        {
            return columnType switch
            {
                CustomColumnType.SingleText => ColumnType.SingleText,
                CustomColumnType.Number => ColumnType.Number,
                CustomColumnType.DateTime => ColumnType.DateTime,
                CustomColumnType.YesOrNo => ColumnType.YesOrNo,
                _ => throw new Exception(),
            };
        }

        private bool IsBothTextColumn(ColumnType type1, ColumnType type2)
        {
            if ((type1 == ColumnType.SingleText || type1 == ColumnType.MultipleText) && (type2 == ColumnType.SingleText || type2 == ColumnType.MultipleText))
            {
                return true;
            }
            return false;
        }

        private string MixOptionsJson(string optionsJson1, string optionsJson2)
        {
            try
            {
                Dictionary<int, string> choices1 = JsonConvert.DeserializeObject<Dictionary<int, string>>(optionsJson1);
                Dictionary<int, string> choices2 = JsonConvert.DeserializeObject<Dictionary<int, string>>(optionsJson2);
                int maxIndex = choices1.Keys.Max();
                foreach (KeyValuePair<int, string> ch in choices2)
                {
                    if (!choices1.Any(a => a.Value == ch.Value))
                    {
                        choices1.Add(++maxIndex, ch.Value);
                    }
                }
                return JsonConvert.SerializeObject(choices1);
            }
            catch (Exception e)
            {
                logger.Warn("Mix options error, {0}", e);
            }
            return optionsJson1;
        }
        public string GetColumnOptions(TemplateColumn4Query param)
        {
            List<RMTemplate> templates = TemplateDao.GetTemplateByIds(param.TemplateIds);
            foreach(var template in templates)
            {
                var json = template.GetColumnOptionsJson(param.UniqueId);
                if (!string.IsNullOrEmpty(json)) return json;
            }

            return string.Empty;
        }

        public Dictionary<int, int> GetNodeTypeAndTemplateIdMapping()
        {
            var mapping = new Dictionary<int, int>();
            List<RMTemplate> templates = TemplateDao.GetTemplate();
            foreach (RMTemplate template in templates)
            {
                var nodelevel = RMNodeLevel.PhysicalRecord;
                switch (template.Type)
                {
                    case TemplateType.Records:
                        nodelevel = RMNodeLevel.PhysicalRecord;
                        break;
                    case TemplateType.Folder:
                        nodelevel = RMNodeLevel.PhysicalFile;
                        break;
                    case TemplateType.Box:
                        nodelevel = RMNodeLevel.PhysicalBox;
                        break;
                    default:
                        break;
                }
                mapping[(int)nodelevel] = template.Id;
            }
            return mapping;
        }

        public int ValidHasUniqueIdSettings(Guid templateId)
        {
            var setting = PhysicalUniqueIdSettingDao.LoadingUniqueIdSetting();
            var template = TemplateDao.GetTemplateByUniqueId(templateId);
            if ((setting?.IsGlobalSetting).GetValueOrDefault())
            {
                ArgumentCheck.NotNull(setting, nameof(setting));
                switch (template.Type)
                {
                    case TemplateType.Box:
                        return (!string.IsNullOrEmpty(setting.BoxTemplatePrefix) && setting.BoxTemplateNumberOfDigits > 0) ? 
                            (int)ValidHasUniqueIdSettingsEnum.UniqueIdSettingsConfigured : (int)ValidHasUniqueIdSettingsEnum.GlobaleUniqueIdSettingsMissing;
                    case TemplateType.Folder:
                        return (!string.IsNullOrEmpty(setting.FolderTemplatePrefix) && setting.FolderTemplateNumberOfDigits > 0) ?
                            (int)ValidHasUniqueIdSettingsEnum.UniqueIdSettingsConfigured : (int)ValidHasUniqueIdSettingsEnum.GlobaleUniqueIdSettingsMissing;
                    case TemplateType.Records:
                        return (!string.IsNullOrEmpty(setting.RecordTemplatePrefix) && setting.RecordTemplateNumberOfDigits > 0) ?
                            (int)ValidHasUniqueIdSettingsEnum.UniqueIdSettingsConfigured : (int)ValidHasUniqueIdSettingsEnum.GlobaleUniqueIdSettingsMissing;
                    case TemplateType.Custom:
                        return (!string.IsNullOrEmpty(setting.CustomTemplatePrefix) && setting.CustomTemplateNumberOfDigits > 0) ?
                            (int)ValidHasUniqueIdSettingsEnum.UniqueIdSettingsConfigured : (int)ValidHasUniqueIdSettingsEnum.GlobaleUniqueIdSettingsMissing;
                    default:
                        return 1;
                }
            }
            else
            {
                return (!string.IsNullOrEmpty(template.Prefix) && template.NumberOfDigits != null && template.NumberOfDigits > 0) ? (int)ValidHasUniqueIdSettingsEnum.UniqueIdSettingsConfigured : (int)ValidHasUniqueIdSettingsEnum.TemplateUniqueIdSettingsMissing;
            }
        }

        public bool CheckTemplateData(TemplateDto dto)
        {
            var isValid = true;
            Regex regForNumberOfDigits = new Regex("(^[2-9]$)|(^1[0-5]$)", RegexOptions.None, RecordsConstants.REGEX_DEFAULT_MATCH_TIMEOUT);//2-15
            if (!regForNumberOfDigits.IsMatch(dto.numberOfDigits.ToString()))
            {
                isValid = false;
            }
            var prefix = dto.prefix.Trim();
            if (string.IsNullOrEmpty(prefix) || prefix.Length > 10)//max length 10
            {
                isValid = false;
            }
            return isValid;
        }

        /// <summary>
        /// 检查custom 的template的层次是否超出了最大设置
        /// </summary>
        /// <param name="idPathList"></param>
        /// <returns></returns>
        private bool CheckCustomTemplateDepthValid(List<string> idPathList)
        {
            return TemplateRelationshipDao.GetAncesstorCount(idPathList, TemplateType.Custom) < TemplateConstants.MaxCustomTemplateDepth;
        }

        public async Task<SaveTemplateResult> CheckTemplateBeforeSavingAsync(Guid uniqueId, string prefix, string name, TemplateType templateType, List<string> idPathList)
        {
            var isNewMode = uniqueId == Guid.Empty;
            if (isNewMode && templateType == TemplateType.Custom && !CheckCustomTemplateDepthValid(idPathList))
            {
                return SaveTemplateResult.CustomTeplateExceedMaxDepth;
            }
            var uniqueIdSetting = PhysicalUniqueIdSettingDao.LoadingUniqueIdSetting();
            if (isNewMode && uniqueIdSetting == null)
            {
                return SaveTemplateResult.MissUniqueIdSettingMode;
            }

            var allOtherTemplates = await TemplateDao.FindListAsync(t => t.UniqueId != uniqueId);
            var templatePrefixs = allOtherTemplates.Select(t => t.Prefix);
            var templateNames = allOtherTemplates.Select(t => I18NEntity.GetString(t.Name));
            if (templateNames.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                return SaveTemplateResult.NameDuplicate;
            }            
            else if (templatePrefixs.Contains(prefix) && (uniqueIdSetting == null || !uniqueIdSetting.IsGlobalSetting))
            {
                return SaveTemplateResult.PrefixDuplicate;
            }
            else
            {
                return SaveTemplateResult.None;
            }

        }

        public void CheckCategoriesAndColumnsData(TemplateDto template)
        {
            var hasError = false;
            List<TemplateCategoryDto> uiCategories = template.categories;
            RMTemplate dbTemplate = template.uniqueId == Guid.Empty ? null : TemplateDao.GetTemplateByUniqueId(template.uniqueId);
            var defaultTemplate = GetDefaultCategoryAndColumn((int)template.type);
            var xmlDefaultCategories = defaultTemplate.categories;
            var defaultColumns = new List<TemplateColumnDto>();
            var defaultCategoriesId = new List<Guid>();
            var isEditTemplate = template.uniqueId != Guid.Empty ? true: false;
            if (uiCategories != null && uiCategories.Count > 0)
            {
                if (isEditTemplate)
                {
                    var dbCategories = TemplateDao.LoadCategories(template.uniqueId);
                    var dbDefaultCategries = dbCategories.Where(d => d.IsDefault && d.TemplateUniqueId == template.uniqueId).ToList();
                    //验证default category 信息是否被非法修改
                    foreach (var item in dbDefaultCategries)
                    {
                        defaultCategoriesId.Add(item.UniqueId);
                        //ui和db对比 只check 不允许编辑的default category
                        var categoryDto = uiCategories.Where(c => c.id == item.UniqueId && c.name == item.Name && !c.allowEdit).FirstOrDefault();
                        if (categoryDto == null)
                        {
                            hasError = true;
                            break;
                        }
                    }

                    if (!hasError) {
                        foreach (var uiCategory in uiCategories)
                        {
                            if (!uiCategory.allowEdit && !defaultCategoriesId.Contains(uiCategory.id))
                            {
                                //非默认 category allowEdit属性不能是false
                                hasError = true;
                                break;
                            }
                            //验证column 信息是否被非法修改
                            ArgumentCheck.NotNull(dbTemplate, nameof(dbTemplate));
                            InvalidColumnOfEditTemplate(uiCategory, dbTemplate.ColumnSchema);
                        }
                    }
                }
                else
                {
                    //只能从xml中获取category信息，category id不准确不能作为比较条件
                    foreach (var item in xmlDefaultCategories)
                    {
                        
                        //只check 不允许编辑的default category
                        var categoryDto = uiCategories.Where(c => c.name == item.name && !item.allowEdit).FirstOrDefault();
                        if (categoryDto == null)
                        {
                            hasError = true;
                            break;
                        }
                        else {
                            defaultCategoriesId.Add(categoryDto.id);
                        }
                        defaultColumns.AddRange(item.columns);
                    }
                    if (!hasError) {
                        foreach (var uiCategory in uiCategories)
                        {
                            if (!uiCategory.allowEdit && !defaultCategoriesId.Contains(uiCategory.id))
                            {
                                //非默认 category allowEdit属性不能是false
                                hasError = true;
                                break;
                            }
                        }
                        //验证column 信息是否被非法修改
                        InvalidColumnOfCreateTemplate(uiCategories, defaultColumns);
                    }
                }
            }
            else
            {
                hasError = true;
            }
            if (hasError)
            {
                throw new Exception("Incorrect categories info.");
            }
        }

        public void InvalidColumnOfEditTemplate(TemplateCategoryDto categoryDto, string dbColumnsXMl)
        {
            var hasError = false;
            var uiColumns = categoryDto.columns;
            var defaultColumnIds = new List<Guid>();
            if (!string.IsNullOrEmpty(dbColumnsXMl))
            {
                var dbColumnsSchema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(dbColumnsXMl);
                if (dbColumnsSchema != null && dbColumnsSchema.Columns != null && dbColumnsSchema.Columns.Count > 0)
                {
                    var dbColumns = dbColumnsSchema.Columns.Where(c => c.CategoryId == categoryDto.id && (c.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) != (int)TemplateInheritSettingEnum.PushToChild).ToList();
                    foreach (var dbColumn in dbColumns)
                    {
                        //第二个参数为了排除box template的size column的老数据 (size 的老数据AllowEdit为false)
                        if (!dbColumn.AllowEdit && dbColumn.UniqueId != new Guid(DefaultColumnIDs.Capability))
                        {
                            //验证不允许编辑的column信息是否被篡改
                            var columnDto = uiColumns.Where(c =>c.required == dbColumn.Required && c.uniqueId == dbColumn.UniqueId && c.categoryId == dbColumn.CategoryId && c.columnName == dbColumn.Name && c.typeId == (int)dbColumn.ColumnType && !c.allowEdit).FirstOrDefault();
                            if (columnDto == null)
                            {
                                hasError = true;
                                logger.Error($"Invalid column [{dbColumn.Name}], ID:[{dbColumn.UniqueId}], column information has been tampered with.");
                                break;
                            }
                        }
                        else
                        {
                            //编辑column时不可以更改type
                            var columnDto = uiColumns.Where(c => c.uniqueId == dbColumn.UniqueId && c.categoryId == dbColumn.CategoryId).FirstOrDefault();
                            if (columnDto != null)
                            {
                                if (columnDto.typeId != (int)dbColumn.ColumnType) {
                                    hasError = true;
                                    logger.Error($"Invalid column [{dbColumn.Name}], ID:[{dbColumn.UniqueId}], column type has been tampered with.");
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (hasError)
            {
                throw new Exception("Invalid column exists.");
            }
        }


        public void InvalidColumnOfCreateTemplate(List<TemplateCategoryDto> uiCategories, List<TemplateColumnDto> defaultColumns) {
            var hasError = false;
            var uiColumns = new List<TemplateColumnDto>();
            foreach (var uiCategory in uiCategories)
            {
                if (uiCategory.columns != null && uiCategory.columns.Count > 0) {
                    uiColumns.AddRange(uiCategory.columns);
                }
            }

            foreach (var item in defaultColumns)
            {
                if (!item.allowEdit)
                {
                    //验证不允许编辑的column信息是否被篡改
                    var columnDto = uiColumns.Where(c => c.required == item.required && c.uniqueId == item.uniqueId  && c.columnName == item.columnName && c.typeId == item.typeId && !c.allowEdit).FirstOrDefault();
                    if (columnDto == null)
                    {
                        hasError = true;
                        break;
                    }
                }
                else {
                    //default column不可以更改type
                    var columnDto = uiColumns.Where(c => c.uniqueId == item.uniqueId).FirstOrDefault();
                    if (columnDto != null)
                    {
                        if (columnDto.typeId != item.typeId)
                        {
                            hasError = true;
                            break;
                        }
                    }
                }
            }
            if (hasError)
            {
                throw new Exception("Invalid column exists.");
            }
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.TemplateManagement, Action = AuditAction.CreateSuite, BeforeHandler = typeof(TemplateManagementBeforeAuditHandler), AfterHandler = typeof(TemplateManagementAfterAuditHandler))]
        public bool CreateSuite(SuiteDto dto)
        {
            try
            {
                return SuiteDao.CreateSuite(dto);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while create suite [{1}], ERROR:{0}", ex.ToString(), dto.Name);
                return false;
            }
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.TemplateManagement, Action = AuditAction.UpdateSuite, BeforeHandler = typeof(TemplateManagementBeforeAuditHandler), AfterHandler = typeof(TemplateManagementAfterAuditHandler))]
        public bool UpdateSuite(SuiteDto dto)
        {
            try
            {
                return SuiteDao.UpdateSuite(dto);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while update suite [{1}][{2}], ERROR:{0}", ex.ToString(), dto.Name, dto.UniqueId);
                return false;
            }
        }
        
        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.TemplateManagement, Action = AuditAction.DeleteSuite, BeforeHandler = typeof(TemplateManagementBeforeAuditHandler), AfterHandler = typeof(TemplateManagementAfterAuditHandler))]
        public RAReturnMessage DeleteSuite(Guid suiteId)
        {
            RAReturnMessage returnMessage = new RAReturnMessage();
            try
            {
                var allSubTemplates = TemplateDao.GetAllSubTemplateBySuiteId(suiteId);
                if (allSubTemplates.Count > 0)
                {
                    returnMessage.MessageType = RAMessageType.Failed;
                    returnMessage.FaildType = RAFailedType.DeleteUsingSuite;
                    return returnMessage;
                }
                var allSubTemplateIds = allSubTemplates.Select(t => t.Id).ToList();
                if (allSubTemplateIds.Count > 0 && ExplorerDao.QueryByPage(d => allSubTemplateIds.Contains(d.TemplateId) && d.RecordStatus != (int)Contract.Explorer.RMRecordStatus.RMDeleted, 1).Item1.Any())
                {
                    returnMessage.MessageType = RAMessageType.Failed;
                    returnMessage.FaildType = RAFailedType.DeleteUsingSuite;
                    return returnMessage;
                }
                TemplateDao.DeleteSuite(suiteId);
            }
            catch (Exception e)
            {
                logger.Warn("delete suite {0} error: {1}", suiteId, e.ToString());
                returnMessage.MessageType = RAMessageType.Failed;
            }
            return returnMessage;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="idPathList"></param>
        /// <returns></returns>
        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.TemplateManagement, Action = AuditAction.DeleteTemplate, BeforeHandler = typeof(TemplateManagementBeforeAuditHandler), AfterHandler = typeof(TemplateManagementAfterAuditHandler))]
        public RAReturnMessage DeleteTemplate(Guid templateId, List<string> idPathList)
        {
            RAReturnMessage returnMessage = new RAReturnMessage();
            try
            {
                var hasSubTemplates = TemplateRelationshipDao.GetChildrenCount(templateId, idPathList) > 0;
                
                if (hasSubTemplates)
                {
                    returnMessage.MessageType = RAMessageType.Failed;
                    returnMessage.FaildType = RAFailedType.DeleteUningTemplate;
                    return returnMessage;
                }
                var template = TemplateDao.Find(d => templateId == d.UniqueId);
                if (ExplorerDao.QueryByPage(d => d.TemplateId == template.Id && d.RecordStatus != (int)Contract.Explorer.RMRecordStatus.RMDeleted, 1).Item1.Any())
                {
                    returnMessage.MessageType = RAMessageType.Failed;
                    returnMessage.FaildType = RAFailedType.DeleteUningTemplate;
                    return returnMessage;
                }
                else
                {
                    TemplateDao.DeleteTemplate(templateId, idPathList);
                }
                //var subTemplates = new List<RMSuiteMembership>();
                //var template = TemplateDao.Find(d => templateId == d.UniqueId);
                //var delBoxTemplate = false;

                //if (delBoxTemplate)
                //{
                //    if (ExplorerDao.QueryByPage(d => d.TemplateId == template.Id && d.RecordStatus != (int)Contract.Explorer.RMRecordStatus.RMDeleted, 1).Item1.Any())
                //    {
                //        returnMessage.MessageType = RAMessageType.Failed;
                //        returnMessage.FaildType = RAFailedType.DeleteUningTemplate;
                //        return returnMessage;
                //    }
                //    else
                //    {
                //        TemplateDao.DeleteTemplateWithRelationship(templateId);
                //    }
                //}
            }
            catch (Exception e)
            {
                logger.Warn("delete template {0} error: {1}", templateId, e.ToString());
                returnMessage.MessageType = RAMessageType.Failed;
            }
            return returnMessage;
        }

        public SuiteDto LoadSuite(Guid id)
        {
            var entity = SuiteDao.Find(s => s.UniqueId == id);
            var rootRelationship = TemplateRelationshipDao.Find(s => s.Ancestor == id && s.Distance == 1);

            var rootTempalteName = "";
            if (rootRelationship != null)
            {
                var template = TemplateDao.Find(t => t.UniqueId == rootRelationship.Descendant);
                if (template != null)
                {
                    rootTempalteName = template.Name;
                }
            }
            else {
                entity.RootTemplateCreateType = SuiteRootTemplateCreateType.New;
            }

            return new SuiteDto()
            {
                Id = entity.Id,
                UniqueId = entity.UniqueId,
                Name = entity.Name,
                Description = entity.Description,
                StartFromType = entity.StartFromType,
                RootTemplateCreateType = entity.RootTemplateCreateType,
                RootTemplateUniqueId = rootRelationship != null ? rootRelationship.Descendant : Guid.Empty,
                RootTemplateName = rootTempalteName
            };
        }

        public SuiteTemplateBrowserResultDto GetSuitesV2ByPage(SuiteTemplateBrowserDto browserDto)
        {
            var queryDto = new SuiteTemplateQueryDto
            {
                PagingInfo = browserDto.PagingInfo,
            };
            using (PerformanceScope scope = new PerformanceScope("template.getAllSuites"))
            {
                var result = new SuiteTemplateBrowserResultDto();
                result.Children = new List<SuiteTemplateTreeNode>();
                var totalCount = 0;
                var allSuites = SuiteDao.GetAllSuite(queryDto, out totalCount);
                result.ChildrenCount = totalCount;

                foreach (var suite in allSuites)
                {
                    var childrenCount = TemplateRelationshipDao.GetChildrenCount(suite.UniqueId, new List<string>() { suite.UniqueId.ToString()});

                    var suiteDto = new SuiteTemplateTreeNode()
                    {
                        UniqueId = suite.UniqueId,
                        Name = suite.Name,
                        Type = TemplateType.Suite,
                        StartFromType = suite.StartFromType,
                        TemplateIdList = new List<string> {suite.UniqueId.ToString() },
                        ChildrenCount = childrenCount
                    };

                    result.Children.Add(suiteDto);
                }
                return result;
            }
        }

        public async Task<SuiteTemplateBrowserResultDto> GetTemplatesByParentV2ByPageAsync(SuiteTemplateBrowserDto browserDto)
        {
            var result = new SuiteTemplateBrowserResultDto() { Children = new List<SuiteTemplateTreeNode>()};
            int totalCount = 0;
            var children = TemplateRelationshipDao.GetByParent(browserDto.Node.UniqueId, browserDto.Node.TemplateIdList, browserDto.PagingInfo.PageIndex, browserDto.PagingInfo.PageSize, out totalCount);
            result.ChildrenCount = totalCount;
            var templates = (await TemplateDao.FindListAsync(o => children.Contains(o.UniqueId))).OrderBy(o => o.Name);
            var isUnderDefaultSuite = browserDto.Node.UniqueId.ToString().Equals(DefaultSuiteIds.RECORD_SUITE_DEFAULT_FOLDER_SUITE_ID, StringComparison.OrdinalIgnoreCase)
                || browserDto.Node.UniqueId.ToString().Equals(DefaultSuiteIds.RECORD_SUITE_DEFAULT_BOX_SUITE_ID, StringComparison.OrdinalIgnoreCase)
                || browserDto.Node.IsUnderDefaultSuite;
            foreach (var template in templates)
            {
                var idPathList = browserDto.Node.TemplateIdList.Select(o => o).ToList();
                idPathList.Add(template.Id.ToString());
                var childrenCount = TemplateRelationshipDao.GetChildrenCount(template.UniqueId, idPathList);
                var suiteDto = new SuiteTemplateTreeNode()
                {
                    UniqueId = template.UniqueId,
                    Name = template.Name,
                    Type = template.Type,
                    TemplateIdList = idPathList,
                    ChildrenCount = childrenCount,
                    IsUnderDefaultSuite = isUnderDefaultSuite,
                };

                result.Children.Add(suiteDto);
            }
            return result;
        }

        public List<SimplifySuiteDto> LoadAllSuites()
        {
            List<SimplifySuiteDto> resultLists = new List<SimplifySuiteDto>();
            var allSuites = SuiteDao.FindAll().OrderBy(o => o.Name);
            foreach (var suite in allSuites)
            {
                resultLists.Add(new SimplifySuiteDto()
                {
                    UniqueId = suite.UniqueId,
                    Name = suite.Name,
                    StartFrom = suite.StartFromType
                });
            }
            return resultLists;
        }

        public async Task<List<SimplifyTemplateDto>> GetAllTemplatesByLocationId4ExplorerAsync(Guid locationId)
        {
            List<SimplifyTemplateDto> result = new List<SimplifyTemplateDto>();
            try
            {
                LocationDao.UpgradeBottomLocationAssociation();
                var suiteIds = SuiteDao.GetSuiteIdsByLocationID(locationId);
                var teplates = (await TemplateRelationshipDao.FindListAsync(m => suiteIds.Contains(m.Ancestor) && m.Distance == 1))
                    .Select(m => new { IdPath = m.IdPath, UniqueId = m.Descendant }).ToList();
                var rootTemplateUniqueIds = teplates.Select(m => m.UniqueId).Distinct().ToList();
                result = await GetTemplatesByUniqueIdsAsync(rootTemplateUniqueIds);
            }
            catch (Exception e)
            {
                logger.Error("get template by location {0}, error: {1}", locationId, e.ToString());
                return null;
            }
            return result;
        }

        private async Task<List<SimplifyTemplateDto>> GetTemplatesByUniqueIdsAsync(List<Guid> uniqueIds)
        {
            List<SimplifyTemplateDto> result = new List<SimplifyTemplateDto>();
            var templates = await TemplateDao.FindListAsync(t => uniqueIds.Contains(t.UniqueId));
            foreach (var template in templates)
            {
                AssembleI18nNameForTemplate(template);
                var rootTemplateDto = new SimplifyTemplateDto() { UniqueId = template.UniqueId, Name = template.Name, Type = template.Type };
                
                if (!result.Contains(rootTemplateDto))
                {
                    result.Add(rootTemplateDto);
                }
            }
            return result;
        }

        private void AssembleI18nNameForTemplate(RMTemplate template)
        {
            if(template.Name == "Default box template" && template.Type == TemplateType.Box)
            {
                template.Name = "RM_Template_Template_Name_Box";
            }
            else if (template.Name == "Default folder template" && template.Type == TemplateType.Folder)
            {
                template.Name = "RM_Template_Template_Name_Folder";
            }
        }

        private List<List<string>> GetAllPossibleTemplateIdPath(AvePoint.RA.Contract.Explorer.PhysicalObjectDto phyObjDto)
        {
            var result = new List<List<string>>();
            var suiteIds = SuiteDao.GetSuiteIdsByLocationID(phyObjDto.LocationId);
            var parentIds = AvePoint.RA.Contract.Explorer.PhysicalObjectDtoExtension.GetParentsIdList(phyObjDto);
            parentIds.RemoveAt(0); //first one is location id
            var templateIds = new List<string>();
            if (parentIds.Count > 0)
            {
                var parents = ExplorerDao.GetRecordByIds(parentIds);
                foreach (var p in parentIds)
                {
                    templateIds.Add(parents.First(o => o.Id == p).TemplateId.ToString());
                }
            }
            templateIds.Add(phyObjDto.TemplateId.ToString());

            foreach (var suiteId in suiteIds)
            {
                var t = new List<string>() { suiteId.ToString() };
                t.AddRange(templateIds);
                result.Add(t);
            }

            return result;
        }

        public async Task<string> GetTemplateIdPathAsync(AvePoint.RA.Contract.Explorer.PhysicalObjectDto phyObjDto)
        {
            var templateIdPaths = GetAllPossibleTemplateIdPath(phyObjDto);

            var idPaths = templateIdPaths.Select(idPathList => TemplateUtil.Convert2Path(idPathList));
            var relationship = (await TemplateRelationshipDao.FindListAsync(o => idPaths.Contains(o.IdPath))).FirstOrDefault();
            return relationship?.IdPath;
        }

        public async Task<List<SimplifyTemplateDto>> GetTemplatesByPhysicalObject4ExplorerAsync(AvePoint.RA.Contract.Explorer.PhysicalObjectDto phyObjDto)
        {
            List<SimplifyTemplateDto> result = new List<SimplifyTemplateDto>();
            try
            {
                var templateIdPaths = GetAllPossibleTemplateIdPath(phyObjDto);
                var subTemplateTypes = TemplateTypeExtension.GetSubTemplatesType(phyObjDto.Template.type);
                var childTemplateIds = new List<Guid>();
                foreach (var templateIdPath in templateIdPaths)
                {
                    var tmp = TemplateRelationshipDao.GetAllByParent(phyObjDto.Template.uniqueId, templateIdPath, subTemplateTypes);
                    if (tmp.Count > 0)
                    {
                        childTemplateIds = tmp;
                        break;
                    }
                }
                var allTemplates = await TemplateDao.FindListAsync(f => childTemplateIds.Contains(f.UniqueId));
                foreach (var t in allTemplates)
                {
                    result.Add(new SimplifyTemplateDto() { Name = I18NEntity.GetString(t.Name), UniqueId = t.UniqueId, Type = t.Type });
                }
            }
            catch (Exception e)
            {
                logger.Error("get template by phyObjDto error: {0}", e.ToString());
                return null;
            }
            return result;
        }

        public async Task<List<SimplifyTemplateDto>> GetTemplatesByIdPathAsync(Guid templateId, string templateIdPath, List<TemplateType> types)
        {
            var templateUniqueIds = (await TemplateRelationshipDao.FindListAsync(o => o.Ancestor == templateId && o.IdPath.StartsWith(templateIdPath) && o.Distance == 1 && types.Contains(o.TemplateType)))
                .Select(o => o.Descendant).Distinct().ToList();
            return await GetTemplatesByUniqueIdsAsync(templateUniqueIds);
        }

        public TemplateDto GetDefaultCategoryAndColumn(int type)
        {
            TemplateDto dto = new TemplateDto();
            List<RMTemplateCategory> dbCategories = new List<RMTemplateCategory>();

            TemplateType typeEnum = (TemplateType)type;
            switch (typeEnum)
            {
                case TemplateType.Box:
                    dbCategories.Add(new RMTemplateCategory()
                    {
                        Name = "RM_Template_Cagegory_Name_Basic",
                        UniqueId = new Guid("11D303D8-D6FB-4A2B-A87D-3A18E2AC2D9A"),
                        IsDefault = true
                    });
                    dto.categories = this.AssembleCategory(DefaultTemplateData.DEFAULT_DATA_BOX_TEMPLATE_XML, dbCategories, true, true);
                    break;
                case TemplateType.Folder:
                    dbCategories.Add(new RMTemplateCategory()
                    {
                        Name = "RM_Template_Cagegory_Name_Basic",
                        UniqueId = new Guid("D192C525-4A1E-48A2-9C00-F864A26571CF"),
                        IsDefault = true
                    });
                    dbCategories.Add(new RMTemplateCategory()
                    {
                        Name = "RM_Template_Cagegory_Name_Classification",
                        UniqueId = new Guid("2D7D5D51-A541-4C18-BD5C-AE5FA633D5CF"),
                        IsDefault = true
                    });
                    dbCategories.Add(new RMTemplateCategory()
                    {
                        Name = "RM_Template_Cagegory_Name_Statement",
                        UniqueId = new Guid("5C1875AE-0F81-4249-A036-64F91B29B02D"),
                        IsDefault = true
                    });
                    dto.categories = this.AssembleCategory(DefaultTemplateData.DEFAULT_DATA_FOLDER_TEMPLATE_XML, dbCategories, true, true);
                    break;
                case TemplateType.Records:
                    dbCategories.Add(new RMTemplateCategory()
                    {
                        Name = "RM_Template_Cagegory_Name_Basic",
                        UniqueId = new Guid("5815D70C-1E9D-404F-89BB-933E365A057C"),
                        IsDefault = true
                    });
                    dbCategories.Add(new RMTemplateCategory()
                    {
                        Name = "RM_Template_Cagegory_Name_Classification",
                        UniqueId = new Guid("A6FA9703-0CFA-43F0-953B-F22858CB5124"),
                        IsDefault = true
                    });
                    dbCategories.Add(new RMTemplateCategory()
                    {
                        Name = "RM_Template_Cagegory_Name_Statement",
                        UniqueId = new Guid("9A10FB34-79DF-4D45-9EB1-6DF44B7A8D4C"),
                        IsDefault = true
                    });
                    dto.categories = this.AssembleCategory(DefaultTemplateData.DEFAULT_DATA_RECORD_TEMPLATE_XML, dbCategories, true, true);
                    break;
                case TemplateType.Custom:
                    dbCategories.Add(new RMTemplateCategory()
                    {
                        Name = "RM_Template_Cagegory_Name_Basic",
                        UniqueId = new Guid("28CB3865-F492-47CD-8D0C-BD26E87ED5FC"),
                        IsDefault = true
                    });
                    dto.categories = this.AssembleCategory(DefaultTemplateData.DEFAULT_DATA_CUSTOM_TEMPLATE_XML, dbCategories, true, true);
                    break;
                default:
                    break;
            }
            return dto;
        }

        public TemplateInfoOfBreadCrumbs GetTemplateInfoOfBreadCrumbs(SuiteTemplateQueryDto queryDto) {
            try
            {
                TemplateInfoOfBreadCrumbs info = new TemplateInfoOfBreadCrumbs();
                if (queryDto.BoxTemplateUniqueId != Guid.Empty) {
                    RMTemplate boxTemplate = TemplateDao.GetTemplateByUniqueId(queryDto.BoxTemplateUniqueId);
                    info.BoxTemplateName = I18NEntity.GetString(boxTemplate.Name);
                    info.BoxTemplateId = boxTemplate.UniqueId;
                }

                if (queryDto.FolderTemplateUniqueId != Guid.Empty) {
                    RMTemplate folderTemplate = TemplateDao.GetTemplateByUniqueId(queryDto.FolderTemplateUniqueId);
                    info.FolderTemplateName = I18NEntity.GetString(folderTemplate.Name);
                    info.FolderTemplateId = folderTemplate.UniqueId;
                }
                return info;
            }
            catch (Exception e)
            {
                logger.Error("GetTemplateInfoOfBreadCrumbs error {0}", e.ToString());
                return null;
            }
        }

        public async Task<ExistingTemplatesInfo> GetExistingFolderTemplatesInfoAsync(Guid suiteId) {
            ExistingTemplatesInfo info = new ExistingTemplatesInfo();
            var otherSuites = (await SuiteDao.FindListAsync(s => s.UniqueId != suiteId)).Select(s => s.UniqueId);
            var boundToOtherSuiteRootTemplateIds = (await TemplateRelationshipDao.FindListAsync(s => otherSuites.Contains(s.Ancestor) && s.Distance == 1 && s.TemplateType == TemplateType.Folder)).Select(s => s.Descendant).ToList();
            
            List<RMTemplate> folderTemplates = await TemplateDao.FindListAsync(t => t.Type == TemplateType.Folder && !boundToOtherSuiteRootTemplateIds.Contains(t.UniqueId));
            if (folderTemplates.Count > 0)
            {
                info.FolderTemplates = folderTemplates.ConvertAll(t => { return new SimplifyTemplateDto() { Name = I18NEntity.GetString(t.Name), UniqueId = t.UniqueId, Type = t.Type }; });
            }
            return info;
        }

        public async Task<ExistingTemplatesInfo> GetExistingTemplatesInfoAsync(QueryExistingTemplatesDto queryDto) {
            ExistingTemplatesInfo info = new ExistingTemplatesInfo();
            if (!ValidateId(queryDto.TemplateIdList)) return info;

            var template = TemplateDao.GetTemplateById(int.Parse(queryDto.TemplateIdList.Last()));
            if (template == null)
            {
                logger.Warn($"Can't get template with id : {queryDto.TemplateIdList.Last()}");
                return info;
            }
            if (queryDto.TemplateTypes == null || queryDto.TemplateTypes.Count == 0)
            {
                queryDto.TemplateTypes = GetTemplateTypes(template);
            }

            if (queryDto.TemplateTypes == null || queryDto.TemplateTypes.Count == 0)
            {
                return info;
            }
            var boundToSuiteTemplateIds = await GetBoundedTemplateIdsAsync(queryDto.TemplateTypes, template.UniqueId, queryDto.TemplateIdList);
            boundToSuiteTemplateIds.Add(template.UniqueId);
            var templates = await TemplateDao.FindListAsync(t => queryDto.TemplateTypes.Contains(t.Type) && !boundToSuiteTemplateIds.Contains(t.UniqueId));
            if (templates.Count > 0)
            {
                info.Templates = templates
                    .ConvertAll(t => { return new SimplifyTemplateDto() { Name = I18NEntity.GetString(t.Name), UniqueId = t.UniqueId, Type = t.Type }; })
                    .OrderBy(o => o.Type)
                    .ToList();
            }
            return info;
        }


        public List<SimplifyTemplateDto> GetAllExistingSimplifyTemplates()
        {
            try
            {
                var templates = TemplateDao.GetAllSimplifyTemplates();
                return templates.ConvertAll(t => { return new SimplifyTemplateDto() { Name = I18NEntity.GetString(t.Name), Id = t.Id, UniqueId = t.UniqueId, Type = t.Type }; });
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while GetAllExistingSimplifyTemplates");
                return null;
            }
        }

        private bool ValidateId(List<string> idPathList)
        {
            if (!int.TryParse(idPathList.Last(), out int templateId))
            {
                logger.Warn($"Template id {idPathList.Last()} in template id list is wrong");
                return false;
            }

            return true;
        }

        private List<TemplateType> GetTemplateTypes(RMTemplate template)
        {
            var result = new List<TemplateType>();
            
            switch (template.Type)
            {
                case TemplateType.Custom:
                    result.Add(TemplateType.Custom);
                    result.Add(TemplateType.Box);
                    result.Add(TemplateType.Folder);
                    break;
                case TemplateType.Box:
                    result.Add(TemplateType.Folder);
                    break;
                case TemplateType.Folder:
                    result.Add(TemplateType.Records);
                    break;
                default:
                    logger.Warn($"Wrong template type : {template.Type}");
                    break;
            }

            return result;
        }

        private async Task<List<Guid>> GetBoundedTemplateIdsAsync(List<TemplateType> types, Guid templateUniqueId, List<string> idPathList)
        {
            var idPath = TemplateUtil.Convert2Path(idPathList);
            var boundedIds = (await TemplateRelationshipDao.FindListAsync(o => o.Ancestor == templateUniqueId && types.Contains(o.TemplateType) && o.IdPath.StartsWith(idPath) && o.Distance == 1))
                .Select(o => o.Descendant).Distinct().ToList();
            return boundedIds;
        }

        public string LoadingUniqueIdSetting()
        {
            var setting = PhysicalUniqueIdSettingDao.LoadingUniqueIdSetting();
            return Newtonsoft.Json.JsonConvert.SerializeObject(setting);
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.TemplateManagement, Action = AuditAction.ToggleGlobalUniqueId, BeforeHandler = typeof(TemplateManagementBeforeAuditHandler), AfterHandler = typeof(TemplateManagementAfterAuditHandler))]
        public async Task<bool> ToggleGlobalUniqueIdSettingsAsync(bool isGlobal)
        {
            var defaultBox = TemplateDao.Find(t => t.UniqueId == new Guid(DefaultTemplateIds.BOX_TEMPLATE_ID));
            var defaultFolder = TemplateDao.Find(t => t.UniqueId == new Guid(DefaultTemplateIds.FOLDER_TEMPLATE_ID));
            var defaultRecord = TemplateDao.Find(t => t.UniqueId == new Guid(DefaultTemplateIds.RECORD_TEMPLATE_ID));
            var setting = PhysicalUniqueIdSettingDao.LoadingUniqueIdSetting();
            if (setting != null)
            {
                return false;
            }
            RMPhysicalUniqueIdSetting saveSetting = null;
            if (isGlobal)
            {
                saveSetting = new RMPhysicalUniqueIdSetting()
                {
                    IsGlobalSetting = true,
                    BoxTemplatePrefix = defaultBox?.Prefix,
                    BoxTemplateNumberOfDigits = (defaultBox?.NumberOfDigits).GetValueOrDefault(),
                    FolderTemplatePrefix = defaultFolder?.Prefix,
                    FolderTemplateNumberOfDigits = (defaultFolder?.NumberOfDigits).GetValueOrDefault(),
                    RecordTemplatePrefix = defaultRecord?.Prefix,
                    RecordTemplateNumberOfDigits = (defaultRecord?.NumberOfDigits).GetValueOrDefault()
                };
            }
            else
            {
                saveSetting = new RMPhysicalUniqueIdSetting()
                {
                    IsGlobalSetting = isGlobal,
                };
            }
            return await PhysicalUniqueIdSettingDao.UpdateUniqueIdSettingAsync(saveSetting);
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.TemplateManagement, Action = AuditAction.UpdateGlobalUniqueId, BeforeHandler = typeof(TemplateManagementBeforeAuditHandler), AfterHandler = typeof(TemplateManagementAfterAuditHandler))]
        public Task<bool> UpdateGlobalUniqueIdSettingsAsync(GlobalUniqueIdSettingsDto settingsDto)
        {
            RMPhysicalUniqueIdSetting saveSetting = new RMPhysicalUniqueIdSetting()
            {
                IsGlobalSetting = true,
                BoxTemplatePrefix = settingsDto.BoxTemplatePrefix,
                BoxTemplateNumberOfDigits = settingsDto.BoxTemplateNumberOfDigits,
                FolderTemplatePrefix = settingsDto.FolderTemplatePrefix,
                FolderTemplateNumberOfDigits = settingsDto.FolderTemplateNumberOfDigits,
                RecordTemplatePrefix = settingsDto.RecordTemplatePrefix,
                RecordTemplateNumberOfDigits = settingsDto.RecordTemplateNumberOfDigits,
                CustomTemplatePrefix = settingsDto.CustomTemplatePrefix,
                CustomTemplateNumberOfDigits = settingsDto.CustomTemplateNumberOfDigits
            };

            var prefixList = new List<string>() { settingsDto.BoxTemplatePrefix, settingsDto.FolderTemplatePrefix, settingsDto.RecordTemplatePrefix, settingsDto.CustomTemplatePrefix };
            var hasDuplicates = prefixList.GroupBy(x => x).Any(g => g.Count() > 1);
            if (hasDuplicates)
            {
                return Task.FromResult(false) ;
            }

            return PhysicalUniqueIdSettingDao.UpdateUniqueIdSettingAsync(saveSetting);
        }


        public bool AddExistingTemplates(AddExistingTemplatesDto dto) 
        {
            var ancestors = new List<Guid> { };
            var isFirst = true;
            foreach(var ancestor in dto.TemplateIdList)
            {
                if (isFirst) //suite unique id
                {
                    ancestors.Add(Guid.Parse(dto.TemplateIdList.First()));
                    isFirst = false;
                    continue;
                }
                var template = TemplateDao.GetTemplateById(int.Parse(ancestor));
                ancestors.Add(template.UniqueId);
            }

            var entities = BuildRelationships(ancestors, dto.Ids, dto.TemplateIdList);
            return TemplateRelationshipDao.AddRelationships(entities);
        }

        private List<RMTemplateRelationship> BuildRelationships(List<Guid> ancestors, List<Guid> descendants, List<string> idPathList)
        {
            var entities = new List<RMTemplateRelationship>();
            var parentIdPath = TemplateUtil.Convert2Path(idPathList);
            foreach (var descendant in descendants)
            {
                var template = TemplateDao.GetTemplateByUniqueId(descendant);
                entities.AddRange(BuildRelationships4One(ancestors, template, parentIdPath));
            }

            return entities;
        }

        private List<RMTemplateRelationship> BuildRelationships4One(List<Guid> ancestors, RMTemplate descendant, string parentIdPath)
        {
            var entities = new List<RMTemplateRelationship>();
            int distance = ancestors.Count;
            foreach (var ancestor in ancestors)
            {
                entities.Add(new RMTemplateRelationship
                {
                    IdPath = parentIdPath + descendant.Id + TemplateUtil.IdPathSeprator,
                    Ancestor = ancestor,
                    Descendant = descendant.UniqueId,
                    TemplateType = descendant.Type,
                    Distance = distance
                });
                distance--;
            }

            return entities;
        }

        public List<RMTemplate> GetAllTemplateByType(TemplateType type)
        {
            List<RMTemplate> templates = new List<RMTemplate>();
            templates = TemplateDao.GetTemplateByType(type);
            return templates;

        }

        public string RunPhysicalTemplateImportJob(string blobName)
        {
            var id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.PhysicalTemplateImport,
                    Parameters = blobName,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while run import template job,ERROR:{0}", ex.ToString());
            }

            return id;
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.TemplateManagement, Action = AuditAction.ImportTemplate, BeforeHandler = typeof(TemplateManagementBeforeAuditHandler), AfterHandler = typeof(TemplateManagementAfterAuditHandler))]
        public string RealRunPhysicalTemplateImportJob(JobRunBy jobRunBy, string jobRunByUser, string blobName)
        {
            var id = string.Empty;

            id = JobMonitorService.CreateJob(JobType.PhysicalTemplateImport, TenantLocalValue.LogonUserEmail);
            logger.Info("Begin control Import template Job {0}", id);

            var importJobs = JobMonitorService.GetRunningJobs(JobType.PhysicalTemplateImport);

            bool isSkip = false;
            if (importJobs != null && importJobs.Count > 0)
            {
                var otherImportJobs = importJobs.Where(j => !j.Equals(id)).ToList();
                if (otherImportJobs != null && otherImportJobs.Count > 0)
                {
                    isSkip = true;
                }
            }
            if (!isSkip)
            {
                string content = "\"" + blobName + "\"";
                JobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = id,
                    JobType = JobType.PhysicalTemplateImport,
                    CommandLine = string.Format("{0} {1} {2}", JobType.PhysicalTemplateImport, id, content),
                });
            }
            else
            {
                JobMonitorService.UpdateJobStatus(id, JobStatus.Skipped, "RM_PTI_JobSkipped");
                logger.Info(I18NEntity.GetString("RM_PTI_JobSkipped"));
            }

            return id;
        }

        #region Google One
        public bool IsExcludeTemplateColumn(Guid columnId)
        {
            HashSet<Guid> excludeColumnIds = new HashSet<Guid>()
            {
            new Guid(DefaultColumnIDs.Barcode),
            new Guid(DefaultColumnIDs.Coverage),
            new Guid(DefaultColumnIDs.Capability),
            new Guid(DefaultColumnIDs.DateClosed),
            new Guid(DefaultColumnIDs.Rights),
            new Guid(DefaultColumnIDs.LoanedBy),
            new Guid(DefaultColumnIDs.LoanedBy_Old),
            new Guid(DefaultColumnIDs.HomeLocation),
            new Guid(DefaultColumnIDs.Description),
            new Guid(DefaultColumnIDs.ProtectiveMarking),
            };
            return excludeColumnIds.Contains(columnId);
        }
        #endregion
    }

    public enum ValidHasUniqueIdSettingsEnum
    {
        UniqueIdSettingsConfigured = 0,
        TemplateUniqueIdSettingsMissing = 1,
        GlobaleUniqueIdSettingsMissing = 2
    }
}
