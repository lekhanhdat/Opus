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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder.Extension;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.TenantMigrations.Upgrade.Impl;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using SecurityUtils = AvePoint.GCommon.Utility.SecurityUtils;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMTemplateDao : BaseDao<RMTemplate>, IRMTemplateDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMTemplateDao));

        public List<RMTemplateCategory> LoadCategories(Guid templateId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.TemplateCategory.AsNoTracking().Where(t => t.TemplateUniqueId == templateId).ToList();
            }
        }
        public async Task<int> ResetDefaultDataAsync()
        {
            using (var context = GetNewContext())
            {
                //for test
                using (var tran = context.Database.BeginTransaction())
                {
                    var truncate = "truncate table {0}.[RMTemplateCategories] truncate table {0}.[RMTemplates]";
                    var sql = string.Format(truncate, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName));
                    var result = context.Database.ExecuteSqlCommand(sql);
                    tran.Commit();
                }
                try
                {
                    using (var tran = context.Database.BeginTransaction())
                    {
                        var truncate = "truncate table {0}.[RMSuites] truncate table {0}.[RMSuiteMemberships]";
                        var sql = string.Format(truncate, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName));
                        var result = context.Database.ExecuteSqlCommand(sql);
                        tran.Commit();
                    }
                    using (var tran = context.Database.BeginTransaction())
                    {
                        var truncate = "truncate table {0}.[RMLocationSuiteAssociations]";
                        var sql = string.Format(truncate, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName));
                        var result = context.Database.ExecuteSqlCommand(sql);
                        tran.Commit();
                    }

                    using (var tran = context.Database.BeginTransaction())
                    {
                        var truncate = "truncate table {0}.[RMPhysicalUniqueIdSettings]";
                        var sql = string.Format(truncate, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName));
                        var result = context.Database.ExecuteSqlCommand(sql);
                        tran.Commit();
                    }

                    using (var tran = context.Database.BeginTransaction())
                    {
                        var truncate = "truncate table {0}.[RMTemplateRelationships]";
                        var sql = string.Format(truncate, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName));
                        var result = context.Database.ExecuteSqlCommand(sql);
                        tran.Commit();
                    }
                }
                catch (Exception e)
                {
                    logger.Warn($"An error occurred when ResetDefaultData, message:{e.Message}");
                }
                await new RMTemplateManagementUpgradgeDao().UpgradeAsync(context);
            }

            //CreateOneDemo();
            return 1;
        }

        public async Task<int> InitDefaultDataAsync()
        {
            using (var context = GetNewContext())
            {
                await new RMTemplateManagementUpgradgeDao().UpgradeAsync(context);
                await new RMSuiteMembershipUpgradgeDao().UpgradeAsync(context); //do not change the order with above
            }
            return 1;
        }

        #region remove code
        //private void CreateOneDemo()
        //{
        //    var entity = new RMTemplate()
        //    {
        //        Name = "test3",
        //        Prefix = "UniqueId",
        //        NumberOfDigits = 8,

        //        CreatedOn = DateTime.UtcNow,
        //        LastModifiedOn = DateTime.UtcNow,
        //        Type = (int)TemplateType.Box,
        //    };
        //    var xml = new TemplateColumnsSchema
        //    {
        //        Columns = new List<ColumnXmlSchema>()
        //    };
        //    xml.Columns.Add(new ColumnXmlSchema()
        //    {
        //        CategoryId = 1,
        //        ColumnType = ColumnType.SingleText,
        //        Name = "demo test31",
        //        Required = true,
        //        TemplateInheritSetting = (int)(TemplateInheritSettingEnum.PushToChild)
        //    });
        //    xml.Columns.Add(new ColumnXmlSchema()
        //    {
        //        CategoryId = 1,
        //        ColumnType = ColumnType.SingleText,
        //        Name = "demo test32",
        //        Required = true,
        //        TemplateInheritSetting = (int)(TemplateInheritSettingEnum.ChildInheritsValue | TemplateInheritSettingEnum.PushToChild)
        //    });
        //    xml.Columns.Add(new ColumnXmlSchema()
        //    {
        //        CategoryId = 2,
        //        ColumnType = ColumnType.SingleText,
        //        Name = "demo test33",
        //        Required = true,
        //        TemplateInheritSetting = (int)(TemplateInheritSettingEnum.AllowModifyValue | TemplateInheritSettingEnum.ChildInheritsValue | TemplateInheritSettingEnum.PushToChild)
        //    });
        //    xml.Columns.Add(new ColumnXmlSchema()
        //    {
        //        CategoryId = 2,
        //        ColumnType = ColumnType.SingleText,
        //        Name = "demo test34",
        //        Required = true,
        //        TemplateInheritSetting = (int)(TemplateInheritSettingEnum.AllowModifyValue | TemplateInheritSettingEnum.ChildInheritsValue | TemplateInheritSettingEnum.PushToChild)
        //    });
        //    foreach (var column in xml.Columns)
        //    {
        //        column.UniqueId = Guid.NewGuid();
        //    }
        //    entity.ColumnSchema = SerializerHelper.SerializeByDataContractSerializer(xml);

        //    using (var ctx = GetNewContext())
        //    {
        //        ctx.Template.Add(entity);
        //        ctx.SaveChanges();
        //    }
        //}
        #endregion

        public RMTemplate GetTemplateByTemplateType(TemplateType type)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.Template.AsNoTracking().First(t => t.Type == type);

            }
        }

        public RMTemplate GetTemplateById(int id)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.Template.AsNoTracking().First(t => t.Id == id);
            }
        }

        public RMTemplate GetTemplateByUniqueId(Guid uniquIid)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.Template.AsNoTracking().First(t => t.UniqueId == uniquIid);
            }
        }
        public List<RMTemplate> GetChildrenTemplateByParentID(Guid parentID)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.Template.AsNoTracking().Where(t => t.ParentUniqueId == parentID).ToList();
            }
        }


        public bool SaveTemplateWithColumns(TemplateDto templateDto)
        {
            //TODO xwwang move logic to service
            var hasError = false;
            var templateEntity = new RMTemplate();
            var xml = new TemplateColumnsSchema
            {
                Columns = new List<ColumnXmlSchema>()
            };

            using (var ctx = GetNewContext())
            {
                var isCreateTemplate = true;
                RMTemplate dbTemplate = null;
                Dictionary<Guid, ColumnChangeType> changedColumns = null;

                if (templateDto.uniqueId != Guid.Empty)
                {
                    isCreateTemplate = false;
                    dbTemplate = ctx.Template.First(t => t.UniqueId == templateDto.uniqueId);
                    templateEntity = dbTemplate;
                    //If we support push column, we just need to unlock the code here
                    changedColumns = CompareColumn(dbTemplate, templateDto);
                }

                var templateUniqueId = isCreateTemplate ? Guid.NewGuid() : dbTemplate.UniqueId;
                templateDto.uniqueId = templateUniqueId;
                templateEntity.Name = templateDto.name;
                templateEntity.Description = templateDto.description;
                templateEntity.Prefix = templateDto.prefix;
                templateEntity.NumberOfDigits = templateDto.numberOfDigits;
                templateEntity.Type = templateDto.type;
                templateEntity.UniqueId = templateUniqueId;
                templateEntity.LastModifiedOn = DateTime.UtcNow;

                var allDBCategoryIds = this.LoadCategories(templateUniqueId).Select(c => c.UniqueId).ToList();
                var userid = TenantLocalValue.LogonUserId;
                var user = ctx.Account.Where(a => a.UserId == userid).FirstOrDefault();
                if (user != null)
                {
                    templateEntity.Modifier = user.Id;
                    if (isCreateTemplate)
                    {
                        templateEntity.ParentUniqueId = templateDto.parentUniqueId;
                        templateEntity.Creater = user.Id;
                        templateEntity.CreatedOn = DateTime.UtcNow;
                    }
                }
                using (DbContextTransaction tran = ctx.Database.BeginTransaction())
                {
                    for (int i = 0; i < templateDto.categories.Count; i++)
                    {
                        var categoryDto = templateDto.categories[i];
                        if (string.IsNullOrEmpty(categoryDto.name.Trim()))
                        {
                            hasError = true;
                            break;
                        }
                        if (allDBCategoryIds.Contains(categoryDto.id))
                        {
                            var editEntity = ctx.TemplateCategory.Where(c => c.UniqueId == categoryDto.id && c.TemplateUniqueId == templateUniqueId).FirstOrDefault();
                            if (editEntity != null)
                            {
                                editEntity.Name = categoryDto.name;
                                //ctx.SaveChanges();
                            }
                        }
                        else
                        {
                            var categoryEntity = new RMTemplateCategory()
                            {
                                UniqueId = categoryDto.id,
                                Name = categoryDto.name,
                                IsDefault = !categoryDto.allowEdit,
                                LastModifiedOn = DateTime.UtcNow,
                                TemplateUniqueId = templateUniqueId,
                            };
                            ctx.TemplateCategory.Add(categoryEntity);
                            //ctx.SaveChanges();
                        }
                       

                        var columns = categoryDto.columns;
                        for (int j = 0; j < columns.Count; j++)
                        {
                            var column = columns[j];
                            if (string.IsNullOrEmpty(column.columnName.Trim()))
                            {
                                hasError = true;
                                break;
                            }
                            if (xml.Columns.Select(c => c.Name).Contains(column.columnName))
                            {
                                continue;
                            }

                            TemplateInheritSettingEnum inheritFrom = TemplateInheritSettingEnum.None;
                            if (column.inheritFromParentFolder)
                            {
                                inheritFrom = TemplateInheritSettingEnum.InheritFromParentFolder;
                            }
                            else if (column.inheritFromParent)
                            {
                                inheritFrom = TemplateInheritSettingEnum.InheritFromParentBox;
                            }
                            xml.Columns.Add(new ColumnXmlSchema()
                            {

                                CategoryId = column.categoryId,
                                UniqueId = column.uniqueId,
                                ColumnType = (ColumnType)column.typeId,
                                Name = column.columnName,
                                Required = column.required,
                                TemplateInheritSetting = (int)(column.pushToChild ? TemplateInheritSettingEnum.PushToChild : TemplateInheritSettingEnum.None)
                                | (int)(column.childInheritsValue ? TemplateInheritSettingEnum.ChildInheritsValue : TemplateInheritSettingEnum.None)
                                | (int)(column.allowModifyValue ? TemplateInheritSettingEnum.AllowModifyValue : TemplateInheritSettingEnum.None)
                                | (int)inheritFrom,
                                AllowEdit = column.allowEdit,
                                AllowSort = column.allowSort,
                                OptionsJSON = column.optionsJSON,
                                OptionsMaxIdReachedValue = column.optionsMaxIdReachedValue,
                                pushFoldTemplateCategoriesId = column.pushFoldTemplateCategoriesId,
                                pushRecordTemplateCategoriesId = column.pushRecordTemplateCategoriesId,
                            });
                        }
                    }
                    if (hasError)
                    {
                        throw new Exception("The category name or the column name is empty.");
                    }
                    var allDtoCategoryIds = templateDto.categories.Select(c => c.id).ToList();
                    for (int i = 0; i < allDBCategoryIds.Count; i++)
                    {
                        if (!allDtoCategoryIds.Contains(allDBCategoryIds[i]))
                        {
                            var guid = allDBCategoryIds[i];
                            var removeEntity = ctx.TemplateCategory.Where(c => c.UniqueId == guid && c.TemplateUniqueId == templateUniqueId).FirstOrDefault();
                            if (removeEntity != null)
                            {
                                ctx.TemplateCategory.Remove(removeEntity);
                                //ctx.SaveChanges();
                            }
                        }
                    }

                    templateEntity.ColumnSchema = SerializerHelper.SerializeByDataContractSerializer(xml);
                    if (isCreateTemplate)
                    {
                        ctx.Template.Add(templateEntity);
                        ctx.SaveChanges();
                    }
                    if (changedColumns != null && changedColumns.Count > 0)
                    {
                        this.AddChangedInfoIntoChangeDB(ctx, templateDto.id, changedColumns);
                        //PushColumnAndCatagory(ctx, templateDto, changedColumns, dbTemplate.UniqueId);
                    }
                    
                    if (isCreateTemplate)
                    {
                        AddTemplateRelatonship(templateDto, templateEntity.Id, ctx);
                    }

                    tran.Commit();
                }
                templateDto.id = templateEntity.Id;
                return ctx.SaveChanges() > 0;
            }
        }

        public void AddTemplateRelatonship(List<string> ancestorTemplateIdList, int templateId)
        {
            using (var ctx = GetNewContext())
            {
                var template = ctx.Template.First(o => o.Id == templateId);
                TemplateDto templateDto = new TemplateDto 
                {
                    ParentTemplateIdList = ancestorTemplateIdList,
                    type = template.Type,
                    uniqueId = template.UniqueId
                };
                using (DbContextTransaction tran = ctx.Database.BeginTransaction())
                {
                    AddTemplateRelatonship(templateDto, templateId, ctx);
                    tran.Commit();
                }
                ctx.SaveChanges();
            }
        }

        /// <summary>
        /// 更新闭包表，添加节点到所有祖先节点的关系
        /// </summary>
        /// <param name="templateDto"></param>
        /// <param name="templateId"></param>
        /// <param name="ctx"></param>
        private void AddTemplateRelatonship(TemplateDto templateDto, int templateId, AvePoint.RA.DB.Core.RMDbContext ctx)
        {
            var parentTemplateIdList = templateDto.ParentTemplateIdList;
            var distance = parentTemplateIdList.Count;
            var isFirstOne = true;
            var idPath = TemplateUtil.Convert2Path(parentTemplateIdList) + templateId.ToString() + TemplateUtil.IdPathSeprator;
            foreach (var parent in parentTemplateIdList) //first one is suite unique id
            {
                Guid parentUniqueId = isFirstOne ? Guid.Parse(parent) : GetTemplateUniqueIdById(int.Parse(parent), ctx);
                AddOneTemplateRelatonship(ctx, idPath, parentUniqueId, templateDto.uniqueId, distance, templateDto.type);
                distance--;
                isFirstOne = false;
            }
            //AddOneTemplateRelatonship(ctx, templateDto.id, templateDto.id, 0, templateDto.type);
        }

        private Guid GetTemplateUniqueIdById(int templateId, AvePoint.RA.DB.Core.RMDbContext ctx)
        {
            return ctx.Template.First(o => o.Id == templateId).UniqueId;
        }

        private void AddOneTemplateRelatonship(AvePoint.RA.DB.Core.RMDbContext ctx, string idPath, Guid ancestor, Guid descendant, int distance, TemplateType type)
        {
            if (!ctx.TemplateRelationship.Any(o => o.IdPath == idPath && o.Distance == distance))
            {
                ctx.TemplateRelationship.Add(new RMTemplateRelationship
                {
                    IdPath = idPath,
                    Ancestor = ancestor,
                    Descendant = descendant,
                    Distance = distance,
                    TemplateType = type
                });
            }
        }

        public Dictionary<Guid, ColumnChangeType> CompareColumn(RMTemplate dbTemplate, TemplateDto uiTemplate)
        {
            var result = new Dictionary<Guid, ColumnChangeType>();
            var dbColumns = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(dbTemplate.ColumnSchema).Columns;
            var dbColumnsUniqueIds = dbColumns.Select(c => c.UniqueId);
            foreach (var category in uiTemplate.categories)
            {
                foreach (var column in category.columns)
                {
                    if (dbColumnsUniqueIds.Contains(column.uniqueId))
                    {
                        //Exist in db, need to double check in both ui and db to see if the pushtochild is changed
                        var dbColumn = dbColumns.Where(c => c.UniqueId == column.uniqueId).FirstOrDefault();
                        //如果DB 跟GUI 的pushToChild 不一样了，就要添加到Result中，并且说明变化
                        if (column.pushToChild ^ ((dbColumn?.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild))
                        {
                            //push to child is changed
                            result.Add(column.uniqueId, column.pushToChild ? ColumnChangeType.UncheckToChecked : ColumnChangeType.CheckToUncheck);
                        }
                        //GUI 是pushToChild，如果AllowEdit 或者push 到指定的category 变化了，也需要记录成变化
                        else if (column.pushToChild && ColumnPropertiesChanged(column, dbColumn))
                        {
                            result.Add(column.uniqueId, ColumnChangeType.ColumnPropertiesChanged);
                        }
                        //处理过的column 需要从collection 移除，最终用来查看是否DB 中有未处理的column，来查看GUI 中删除了哪个column
                        dbColumns.Remove(dbColumn);
                    }
                    else
                    {
                        if (column.pushToChild)
                        {
                            //表示DB 中没有这个column， 但是GUI 上有了，说明Column 是新创建，并且是push 的
                            result.Add(column.uniqueId, ColumnChangeType.NewAdded);
                        }
                    }
                }
            }
            if (dbColumns.Count > 0)
            {
                foreach (var dbColumn in dbColumns)
                {
                    if ((dbColumn.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)
                    {
                        //表示DB 里面有，但是GUI 没有，说明GUI 上删除了Push column
                        result.Add(dbColumn.UniqueId, ColumnChangeType.Deleted);
                    }
                }
            }
            return result;
        }

        private bool ColumnPropertiesChanged(TemplateColumnDto uiColumn, ColumnXmlSchema dbColumn)
        {
            bool changed = false;
            changed = ColumnNameChanged(uiColumn, dbColumn)
                || ColumnTypeChanged(uiColumn, dbColumn)
                || ColumnRequiredChanged(uiColumn, dbColumn)
                || ColumnAllowEditChanged(uiColumn, dbColumn)
                || AllowModifiedValueChanged(uiColumn, dbColumn)
                || PushCategordChanged(uiColumn, dbColumn)
                || ChoiceOptionChanged(uiColumn, dbColumn);
            return changed;
        }

        private bool ColumnNameChanged(TemplateColumnDto uiColumn, ColumnXmlSchema dbColumn)
        {
            bool nameChanged = false;
            nameChanged = !uiColumn.columnName.Equals(dbColumn.Name, StringComparison.OrdinalIgnoreCase);
            return nameChanged;
        }

        private bool ColumnTypeChanged(TemplateColumnDto uiColumn, ColumnXmlSchema dbColumn)
        {
            bool columnTypeChanged;
            columnTypeChanged = uiColumn.typeId != (int)dbColumn.ColumnType;
            return columnTypeChanged;
        }

        private bool ColumnRequiredChanged(TemplateColumnDto uiColumn, ColumnXmlSchema dbColumn)
        {
            bool requiredChanged = false;
            requiredChanged = uiColumn.required ^ dbColumn.Required;
            return requiredChanged;
        }

        private bool ColumnAllowEditChanged(TemplateColumnDto uiColumn, ColumnXmlSchema dbColumn)
        {
            bool allowEditChanged = false;
            allowEditChanged = uiColumn.allowEdit ^ dbColumn.AllowEdit;
            return allowEditChanged;
        }

        private bool AllowModifiedValueChanged(TemplateColumnDto uiColumn, ColumnXmlSchema dbColumn)
        {
            bool allowModifiedValueChanged = false;
            allowModifiedValueChanged = uiColumn.allowModifyValue ^ ((dbColumn.TemplateInheritSetting & (int)TemplateInheritSettingEnum.AllowModifyValue) == (int)TemplateInheritSettingEnum.AllowModifyValue);
            return allowModifiedValueChanged;
        }

        private bool PushCategordChanged(TemplateColumnDto uiColumn, ColumnXmlSchema dbColumn)
        {
            //这里会有问题
            bool pushColumnChanged = false;
            //pushColumnChanged = (uiColumn.pushFoldTemplateCategoriesId != dbColumn.pushFoldTemplateCategoriesId) || (uiColumn.pushRecordTemplateCategoriesId != dbColumn.pushRecordTemplateCategoriesId);
            if (uiColumn.pushFoldTemplateCategoriesId != null && uiColumn.pushFoldTemplateCategoriesId.Count > 0)
            {
                if (dbColumn.pushFoldTemplateCategoriesId == null || dbColumn.pushFoldTemplateCategoriesId.Count == 0)
                {
                    pushColumnChanged = true;
                    return pushColumnChanged;
                }
                else
                {
                    foreach (TemplateIdAndCategoryId foldInfo in uiColumn.pushFoldTemplateCategoriesId)
                    {
                        TemplateIdAndCategoryId dbfoldInfo = dbColumn.pushFoldTemplateCategoriesId.Find(f => f.tempalteId == foldInfo.tempalteId);
                        if (dbfoldInfo == null || dbfoldInfo.categoryId != foldInfo.categoryId)
                        {
                            pushColumnChanged = true;
                            return pushColumnChanged;
                        }
                    }
                }
            }
            if (uiColumn.pushRecordTemplateCategoriesId != null && uiColumn.pushRecordTemplateCategoriesId.Count > 0)
            {
                if (dbColumn.pushRecordTemplateCategoriesId == null || dbColumn.pushRecordTemplateCategoriesId.Count == 0)
                {
                    pushColumnChanged = true;
                    return pushColumnChanged;
                }
                else
                {
                    foreach (TemplateIdAndCategoryId foldInfo in uiColumn.pushRecordTemplateCategoriesId)
                    {
                        TemplateIdAndCategoryId dbfoldInfo = dbColumn.pushRecordTemplateCategoriesId.Find(f => f.tempalteId == foldInfo.tempalteId);
                        if (dbfoldInfo == null || dbfoldInfo.categoryId != foldInfo.categoryId)
                        {
                            pushColumnChanged = true;
                            return pushColumnChanged;
                        }
                    }
                }
            }

            return pushColumnChanged;
        }

        private bool ChoiceOptionChanged(TemplateColumnDto uiColumn, ColumnXmlSchema dbColumn)
        {
            bool choiceOptionChanged = false;
            if (dbColumn.ColumnType == ColumnType.SingleChoice || dbColumn.ColumnType == ColumnType.MultipleChoice)
            {
                //Choice 类型的option 目前没有忽略大小写，所以比较的时候也不能忽略大小写
                choiceOptionChanged = !uiColumn.optionsJSON.Equals(dbColumn.OptionsJSON);
            }
            return choiceOptionChanged;
        }

        /*private void PushColumnAndCatagory(Core.RMDbContext ctx, TemplateDto uiTemplate, Dictionary<Guid, ColumnChangeType> changedColumns, Guid templateId)
        {
            //We only have one template in each level, so we can get children tempate below, if we have multiple template, we nee to change the logic
            var templates = new List<RMTemplate>();
            TemplateInheritSettingEnum inheritFrom = TemplateInheritSettingEnum.InheritFromParentBox;
            if (uiTemplate.type == TemplateType.Box)
            {
                inheritFrom = TemplateInheritSettingEnum.InheritFromParentBox;
                List<RMTemplate> foldTemplates = new List<RMTemplate>();
                foldTemplates = ctx.Template.Where(t => t.ParentUniqueId == uiTemplate.uniqueId).ToList();
                templates.AddRange(foldTemplates);
                foreach (RMTemplate foldTemplate in foldTemplates)
                {
                    List<RMTemplate> recordTemplates = new List<RMTemplate>();
                    recordTemplates = ctx.Template.Where(t => t.ParentUniqueId == foldTemplate.UniqueId).ToList();
                    templates.AddRange(recordTemplates);
                }
                //templates = ctx.Template.Where(t=> t.Type == TemplateType.Folder || t.Type == TemplateType.Records).ToList();
            }
            else if (uiTemplate.type == TemplateType.Folder)
            {
                inheritFrom = TemplateInheritSettingEnum.InheritFromParentFolder;
                templates = ctx.Template.Where(t => t.ParentUniqueId == uiTemplate.uniqueId).ToList();
                //templates = ctx.Template.Where(t => t.Type == TemplateType.Records).ToList();
            }
            var allUIColumns = new List<TemplateColumnDto>();
            uiTemplate.categories.ForEach(c => allUIColumns.AddRange(c.columns));
            foreach (var template in templates)
            {
                var dbColumns = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(template.ColumnSchema).Columns;
                foreach (var changeColumnId in changedColumns.Keys)
                {
                    switch (changedColumns[changeColumnId])
                    {
                        case ColumnChangeType.NewAdded:
                            {
                                //Get categoryid from GUI later, we just add test code here
                                var column = allUIColumns.Where(c => c.uniqueId == changeColumnId).First();
                                var categoryId = Guid.Empty;
                                if (template.Type == TemplateType.Folder)
                                {
                                    if (column?.pushFoldTemplateCategoriesId != null)
                                    {
                                        foreach (TemplateIdAndCategoryId templateAndCategory in column.pushFoldTemplateCategoriesId)
                                        {
                                            if (templateAndCategory.tempalteId == template.UniqueId.ToString())
                                            {
                                                categoryId = new Guid(templateAndCategory.categoryId);
                                                break;
                                            }
                                        }
                                    }
                                }
                                else if (template.Type == TemplateType.Records)
                                {
                                    if (column?.pushRecordTemplateCategoriesId != null)
                                    {
                                        foreach (TemplateIdAndCategoryId templateAndCategory in column.pushRecordTemplateCategoriesId)
                                        {
                                            if (templateAndCategory.tempalteId == template.UniqueId.ToString())
                                            {
                                                categoryId = new Guid(templateAndCategory.categoryId);
                                                break;
                                            }
                                        }
                                    }
                                }
                                ArgumentNullException.ThrowIfNull(column);
                                dbColumns.Add(new ColumnXmlSchema()
                                {
                                    CategoryId = categoryId,
                                    UniqueId = column.uniqueId,
                                    ColumnType = (ColumnType)column.typeId,
                                    Name = column.columnName,
                                    Required = column.required,
                                    TemplateInheritSetting = (int)(column.pushToChild ? TemplateInheritSettingEnum.PushToChild : TemplateInheritSettingEnum.None)
                                    | (int)(column.childInheritsValue ? TemplateInheritSettingEnum.ChildInheritsValue : TemplateInheritSettingEnum.None)
                                    | (int)(column.allowModifyValue ? TemplateInheritSettingEnum.AllowModifyValue : TemplateInheritSettingEnum.None)
                                    | (int)inheritFrom,
                                    AllowEdit = false,
                                    OptionsJSON = column.optionsJSON,
                                    OptionsMaxIdReachedValue = column.optionsMaxIdReachedValue
                                });
                                break;
                            }
                        case ColumnChangeType.UncheckToChecked:
                        //目前column changed 逻辑也在UncheckToCheck 中cover 了，因为两种现象可能同时存在
                        case ColumnChangeType.ColumnPropertiesChanged:
                            {
                                var column = allUIColumns.Where(c => c.uniqueId == changeColumnId).First();
                                var dbColumn = dbColumns.FirstOrDefault(c => c.UniqueId == changeColumnId);
                                //通常在UncheckToChecked 的情况下，DB column 不会出现dbColumn！=null 的情况，保留此部分逻辑为了代码健壮
                                if (dbColumn != null)
                                {
                                    if (!column.pushToChild)
                                    {
                                        dbColumns.RemoveAll(c => c.UniqueId == changeColumnId);
                                    }
                                    else
                                    {
                                        var categoryId = Guid.Empty;
                                        if (template.Type == TemplateType.Folder)
                                        {
                                            if (column?.pushFoldTemplateCategoriesId != null)
                                            {
                                                foreach (TemplateIdAndCategoryId templateAndCategory in column.pushFoldTemplateCategoriesId)
                                                {
                                                    if (templateAndCategory.tempalteId == template.UniqueId.ToString())
                                                    {
                                                        categoryId = new Guid(templateAndCategory.categoryId);
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        else if (template.Type == TemplateType.Records)
                                        {
                                            if (column?.pushRecordTemplateCategoriesId != null)
                                            {
                                                foreach (TemplateIdAndCategoryId templateAndCategory in column.pushRecordTemplateCategoriesId)
                                                {
                                                    if (templateAndCategory.tempalteId == template.UniqueId.ToString())
                                                    {
                                                        categoryId = new Guid(templateAndCategory.categoryId);
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        dbColumn.Name = column.columnName;
                                        dbColumn.Required = column.required;
                                        dbColumn.TemplateInheritSetting = (int)(column.pushToChild ? TemplateInheritSettingEnum.PushToChild : TemplateInheritSettingEnum.None)
                                            | (int)(column.childInheritsValue ? TemplateInheritSettingEnum.ChildInheritsValue : TemplateInheritSettingEnum.None)
                                            | (int)(column.allowModifyValue ? TemplateInheritSettingEnum.AllowModifyValue : TemplateInheritSettingEnum.None)
                                            | (int)inheritFrom;
                                        dbColumn.CategoryId = categoryId;
                                        dbColumn.ColumnType = (ColumnType)column.typeId;
                                        dbColumn.OptionsJSON = column.optionsJSON;
                                        dbColumn.OptionsMaxIdReachedValue = column.optionsMaxIdReachedValue;
                                    }
                                }
                                else
                                {
                                    var categoryId = Guid.Empty;
                                    if (template.Type == TemplateType.Folder)
                                    {
                                        if (column?.pushFoldTemplateCategoriesId != null)
                                        {
                                            foreach (TemplateIdAndCategoryId templateAndCategory in column.pushFoldTemplateCategoriesId)
                                            {
                                                if (templateAndCategory.tempalteId == template.UniqueId.ToString())
                                                {
                                                    categoryId = new Guid(templateAndCategory.categoryId);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else if (template.Type == TemplateType.Records)
                                    {
                                        if (column?.pushRecordTemplateCategoriesId != null)
                                        {
                                            foreach (TemplateIdAndCategoryId templateAndCategory in column.pushRecordTemplateCategoriesId)
                                            {
                                                if (templateAndCategory.tempalteId == template.UniqueId.ToString())
                                                {
                                                    categoryId = new Guid(templateAndCategory.categoryId);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    ArgumentNullException.ThrowIfNull(column);
                                    dbColumns.Add(new ColumnXmlSchema()
                                    {
                                        CategoryId = categoryId,
                                        UniqueId = column.uniqueId,
                                        ColumnType = (ColumnType)column.typeId,
                                        Name = column.columnName,
                                        Required = column.required,
                                        TemplateInheritSetting = (int)(column.pushToChild ? TemplateInheritSettingEnum.PushToChild : TemplateInheritSettingEnum.None)
                                        | (int)(column.childInheritsValue ? TemplateInheritSettingEnum.ChildInheritsValue : TemplateInheritSettingEnum.None)
                                        | (int)(column.allowModifyValue ? TemplateInheritSettingEnum.AllowModifyValue : TemplateInheritSettingEnum.None)
                                        | (int)inheritFrom,
                                        AllowEdit = false,
                                        OptionsJSON = column.optionsJSON,
                                        OptionsMaxIdReachedValue = column.optionsMaxIdReachedValue
                                    });
                                }
                                break;
                            }
                        case ColumnChangeType.CheckToUncheck:
                        case ColumnChangeType.Deleted:
                            dbColumns.RemoveAll(c => c.UniqueId == changeColumnId);
                            break;
                        default:
                            break;
                    }
                }
                var xml = new TemplateColumnsSchema
                {
                    Columns = new List<ColumnXmlSchema>()
                };
                xml.Columns.AddRange(dbColumns);
                template.ColumnSchema = SerializerHelper.SerializeByDataContractSerializer(xml);
                #region no use code
                //Find all categorys in current templates
                //var templateCatagorys = ctx.TemplateCategory.Where(c => c.TemplateUniqueId == template.UniqueId).ToList();
                //foreach (var pushCategory in pushCollection.Keys)
                //{
                //    var category = templateCatagorys.First(t => string.Equals(t.Name, pushCategory.name, StringComparison.OrdinalIgnoreCase));
                //    //If current template do not have the category, we need to add a new one.
                //    if(category ==null)
                //    {
                //        ctx.TemplateCategory.Add(new RMTemplateCategory()
                //        {
                //            UniqueId = Guid.NewGuid(),
                //            Name = pushCategory.name,
                //            IsDefault = false,
                //            LastModifiedOn = DateTime.UtcNow,
                //            TemplateUniqueId = template.UniqueId,
                //        });
                //        var xml = pushCategory.columns;
                //        template.ColumnSchema = SerializerHelper.SerializeByDataContractSerializer(xml);
                //    }
                //    else
                //    {
                //        var dbColumnSchema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(template.ColumnSchema);
                //        var xml = new TemplateColumnsSchema { Columns = new List<ColumnXmlSchema>() };
                //        xml.Columns = dbColumnSchema.Columns;
                //        var dbColumnNames = dbColumnSchema.Columns.Select(c => c.Name);
                //        foreach(var pushColumn in pushCollection[pushCategory].Columns)
                //        {
                //            if(dbColumnNames.Contains(pushColumn.Name))
                //            {
                //                var xmlColumn = xml.Columns.First(c => string.Equals(c.Name, pushColumn.Name, StringComparison.OrdinalIgnoreCase));
                //                xmlColumn.TemplateInheritSetting = pushColumn.TemplateInheritSetting| (int)TemplateInheritSettingEnum.InheritFromParent;
                //            }
                //            else
                //            {
                //                //DB do not have this column, add it xml
                //                pushColumn.TemplateInheritSetting |= (int)TemplateInheritSettingEnum.InheritFromParent;
                //                xml.Columns.Add(pushColumn);
                //            }
                //        }
                //        //foreach(var dbColumn in dbColumnSchema.Columns)
                //        //{
                //        //    var column = pushCollection[pushCategory].Columns.Find(p => string.Equals(p.Name, dbColumn.Name, StringComparison.OrdinalIgnoreCase));
                //        //    if (column == null)
                //        //    {
                //        //        //DB do not have this push column, add it xml
                //        //        xml.Columns.Add(dbColumn);
                //        //    }
                //        //    else
                //        //    {
                //        //        xml.Columns.Add(column);
                //        //    }
                //        //}
                //        template.ColumnSchema = SerializerHelper.SerializeByDataContractSerializer(xml);
                //    }
                //}
                #endregion
            }
            this.AddChangedInfoIntoChangeDB(ctx, uiTemplate.id, changedColumns);
        }*/

        private void AddChangedInfoIntoChangeDB(Core.RMDbContext ctx, int tempalteId, Dictionary<Guid, ColumnChangeType> changedColumns)
        {
            foreach (var changeColumnId in changedColumns.Keys)
            {
                RMPhysicalColumnChangeLog columnChangeLog = new RMPhysicalColumnChangeLog();
                columnChangeLog.ColumnUniqueId = changeColumnId;
                columnChangeLog.TemplateId = tempalteId;
                columnChangeLog.ActionTime = DateTime.UtcNow.Ticks;
                columnChangeLog.Action = (int)changedColumns[changeColumnId];
                columnChangeLog.ModifiedBy = TenantLocalValue.LogonUserEmail;
                ctx.RMPhysicalColumnChangeLog.Add(columnChangeLog);
            }
        }

        public bool CheckColumnsSameName(string columnName, int templateId)
        {
            using (var ctx = GetNewContext())
            {
                var template = ctx.Template.AsNoTracking().FirstOrDefault(t => t.Id == templateId);
                if (template == null)
                {
                    return false;
                }
                var schema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(template.ColumnSchema);
                return schema.Columns.Select(c => c.Name).Contains(columnName);
            }
        }


        public RMTemplate GetTemplateByName(string templateName)
        {
            using var ctx = GetNewContext();
            var defaultTmplate = new Dictionary<string,string>() { 
                { "RM_Template_Template_Name_Box", I18NEntity.GetString("RM_Template_Template_Name_Box") },
                { "RM_Template_Template_Name_Folder",I18NEntity.GetString("RM_Template_Template_Name_Folder")},
                { "RM_Template_Template_Name_Record",I18NEntity.GetString("RM_Template_Template_Name_Record")}
            };

            var template = ctx.Template.AsNoTracking().Where(t => t.Name.Equals(templateName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if(template == null && defaultTmplate.ContainsValue(templateName))
            {
                var templateNameKey = defaultTmplate.Where(kv => kv.Value.Equals(templateName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault().Key;
                template = ctx.Template.AsNoTracking().Where(t => t.Name.Equals(templateNameKey,StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            }

            return template;
        }

        public List<RMTemplate> GetTemplate()
        {
            List<RMTemplate> result = new List<RMTemplate>();
            using (var ctx = GetNewContext())
            {
                result = ctx.Template.AsNoTracking().ToList();
            }
            return result;
        }

        public List<SimplifyTemplateDto> GetAllSimplifyTemplates()
        {
            using (var ctx = GetNewContext())
            {
                return ctx.Template.AsNoTracking().Select(o => new SimplifyTemplateDto { Id = o.Id, Name = o.Name, UniqueId = o.UniqueId, Type = o.Type }).ToList();
            }
        }

        public List<RMTemplate> GetTemplateByType(TemplateType type)
        {
            List<RMTemplate> result = new List<RMTemplate>();
            using (var ctx = GetNewContext())
            {
                result = ctx.Template.AsNoTracking().Where(t => t.Type == type).ToList();
            }
            return result;
        }

        public List<RMTemplate> GetTemplateByIds(List<int> ids)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.Template.AsNoTracking().Where(t => ids.Contains(t.Id)).ToList();

            }
        }

        public List<RMTemplate> GetTemplateByUniqueIds(List<Guid> uinqueIds)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.Template.AsNoTracking().Where(t => uinqueIds.Contains(t.UniqueId)).ToList();

            }
        }

        public void DeleteTemplate(Guid templateId, List<string> idPathList)
        {
            var idPath = TemplateUtil.Convert2Path(idPathList);
            using (var context = GetNewContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    var membership = context.TemplateRelationship.Where(t => t.IdPath == idPath);
                    context.TemplateRelationship.RemoveRange(membership);

                    var isExistingInOther = context.TemplateRelationship.Any(o => (o.Descendant == templateId || o.Ancestor == templateId) && idPath != o.IdPath);
                    var entity = context.Template.Where(s => s.UniqueId == templateId).First();
                    var templateName = entity.Name;
                    if (!isExistingInOther)
                    {
                        context.Template.Remove(entity);
                    }
                    context.SaveChanges();
                    tran.Commit();
                    if (!isExistingInOther)
                    {
                        logger.Info($"The box template is really deleted, name:[{templateName}], uniquedId:[{templateId}]");
                    }
                    else
                    {
                        logger.Info($"The box template is not really deleted, name:[{templateName}], uniquedId:[{templateId}]");
                    }
                }
            }
        }

        public void DeleteSuite(Guid suiteId)
        {
            using (var context = GetNewContext())
            {
                var hasChild = context.TemplateRelationship.Any(o => o.Ancestor == suiteId && o.Distance > 0);

                if (hasChild) return;
                using (var tran = context.Database.BeginTransaction())
                {
                    var entity = context.Suite.Where(s => s.UniqueId == suiteId).FirstOrDefault();
                    context.Suite.Remove(entity);
                    var relationship = context.TemplateRelationship.Where(o => o.Ancestor == suiteId && o.Descendant == suiteId && o.Distance == 0).FirstOrDefault();
                    if (relationship != null) context.TemplateRelationship.Remove(relationship);
                    context.SaveChanges();
                    tran.Commit();
                }
            }
        }

      
        public List<RMTemplate> GetAllSubTemplateBySuiteId(Guid suiteId)
        {
            using (var context = GetNewContext())
            {
                List<RMTemplate> allSubTemplates = new List<RMTemplate>();
                var subTemplateIds = context.TemplateRelationship.Where(o => o.Ancestor == suiteId && o.Distance > 0).Select(o => o.Descendant).Distinct();
                allSubTemplates = context.Template.AsNoTracking().Where(sub => subTemplateIds.Contains(sub.UniqueId)).ToList();
               
                return allSubTemplates;
            }
        }
    }

    public enum ColumnChangeType
    {
        None = 0,
        Deleted = 1,
        NewAdded = 2,
        CheckToUncheck = 4,
        UncheckToChecked = 8,
        ColumnPropertiesChanged = 16,
    }
}