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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Core.Upgrade;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.Presentation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.TenantMigrations.Upgrade.Impl
{
    public class RMTemplateManagementUpgradgeDao : BaseDao<RMTemplate>, IDbUpgradeDao
    {
        private static readonly Guid BOX_TEMPLATE_ID = new Guid(DefaultTemplateIds.BOX_TEMPLATE_ID);
        private static readonly Guid FOLDER_TEMPLATE_ID = new Guid(DefaultTemplateIds.FOLDER_TEMPLATE_ID);
        private static readonly Guid RECORD_TEMPLATE_ID = new Guid(DefaultTemplateIds.RECORD_TEMPLATE_ID);

        private static readonly Guid BOX_BASIC_INFORMATION_CATEGORY_ID = new Guid("11D303D8-D6FB-4A2B-A87D-3A18E2AC2D9A");

        private static readonly Guid FOLDER_BASIC_INFORMATION_CATEGORY_ID = new Guid("D192C525-4A1E-48A2-9C00-F864A26571CF");
        private static readonly Guid FOLDER_CLASSIFICATION_INFORMATION_CATEGORY_ID = new Guid("2D7D5D51-A541-4C18-BD5C-AE5FA633D5CF");
        private static readonly Guid FOLDER_STATEMENT_INFORMATION_CATEGORY_ID = new Guid("5C1875AE-0F81-4249-A036-64F91B29B02D");

        private static readonly Guid RECORD_BASIC_INFORMATION_CATEGORY_ID = new Guid("5815D70C-1E9D-404F-89BB-933E365A057C");
        private static readonly Guid RECORD_CLASSIFICATION_INFORMATION_CATEGORY_ID = new Guid("A6FA9703-0CFA-43F0-953B-F22858CB5124");
        private static readonly Guid RECORD_STATEMENT_INFORMATION_CATEGORY_ID = new Guid("9A10FB34-79DF-4D45-9EB1-6DF44B7A8D4C");
       
        private static readonly Guid SUITE_DEFAULT_BOX_SUIT = new Guid(DefaultSuiteIds.RECORD_SUITE_DEFAULT_BOX_SUITE_ID);
        private static readonly Guid SUITE_DEFAULT_FOLDER_SUITE_ID = new Guid(DefaultSuiteIds.RECORD_SUITE_DEFAULT_FOLDER_SUITE_ID);
        private IRMTemplateDao _TemplateDao;
        public IRMTemplateDao TemplateDao
        {
            get { return _TemplateDao ?? (IRMTemplateDao)PlatformWindsorManager.GetService(typeof(IRMTemplateDao)); }
            set { _TemplateDao = value; }
        }
        private List<Guid> upgradeTemplateList = new List<Guid>()
        {
            BOX_TEMPLATE_ID,
            FOLDER_TEMPLATE_ID,
            RECORD_TEMPLATE_ID
        };
        private List<Guid> upgradeSuiteList = new List<Guid>()
        {
            SUITE_DEFAULT_BOX_SUIT,
            SUITE_DEFAULT_FOLDER_SUITE_ID
        };
        private RALogger logger = RALogger.GetInstance(typeof(RMTemplateManagementUpgradgeDao));
        public async Task UpgradeAsync(RMDbContext context)
        {
            try
            {

                if (context.Template.Count(item => upgradeTemplateList.Contains(item.UniqueId)) == upgradeTemplateList.Count 
                    && context.Suite.Count(item => upgradeSuiteList.Contains(item.UniqueId)) == upgradeSuiteList.Count)
                {
                    logger.Info($"The tenant: [{TenantLocalValue.LogonGroupId}] has init physical template.");
                    return;
                }
                logger.Info("begin to upgrade tempalte info ");
                using (var tran = context.Database.BeginTransaction())
                {
                    CheckDefaultBoxTemplate(context);

                    CheckDefaultFileTemplate(context);

                    CheckDefaultRecordsTemplate(context);

                    CheckDefaultSuit(context);
                   
                    tran.Commit();
                }
                //move from RMSuiteMembershipUpgradgeDao
                using (var tran = context.Database.BeginTransaction())
                {
                    UpgradeData(context);
                    tran.Commit();
                }
                logger.Info("success to upgrade tempalte info");
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while upgrade TemplateManagement:{0}", ex.ToString());
            }
        }
        public async Task UpgradeTemplateColumn(RMDbContext context)
        {
            logger.Info($"Begin upgrade RMTempate.");
            try 
            {
                var Templates = context.Template.Where(t => t.Type == TemplateType.Folder || t.Type == TemplateType.Box).ToList();
                var needUpgradeTemplates = new List<RMTemplate>();
                foreach (var template in Templates)
                {
                    try
                    {
                        var columnSchema = template.ColumnSchema;
                        var schemaTemp = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(columnSchema);
                        var dbCategories = TemplateDao.LoadCategories(template.UniqueId);
                        var basicCategorie = dbCategories.Where(d => d.Name == "RM_Template_Cagegory_Name_Basic").First();
                        bool isExits = schemaTemp.Columns.Any(t => t.UniqueId == new Guid(DefaultColumnIDs.Barcode));
                        if (!isExits)
                        {
                            schemaTemp.Columns.Add(new ColumnXmlSchema()
                            {
                                CategoryId = basicCategorie.UniqueId,
                                UniqueId = new Guid(DefaultColumnIDs.Barcode),
                                ColumnType = ColumnType.SingleText,
                                Name = "RM_PRM_PRE_Column_Barcode",
                                OptionsJSON = JsonConvert.SerializeObject(this.GetDefaultColumnData(new Guid(DefaultColumnIDs.Barcode))),
                                Required = false,
                                ShowInEditForm = false,
                                AllowEdit = false
                            });
                            var schemaTempStr = SerializerHelper.SerializeByDataContractSerializer(schemaTemp);
                            template.ColumnSchema = schemaTempStr;
                            needUpgradeTemplates.Add(template);
                            logger.Info("Template {[0]} need to upgrade.", template.Name);
                        }
                        else
                        {
                            logger.Info("Template Column Barcode is Exits.");
                            continue;
                        }
                    }
                    catch(Exception e)
                    {
                        logger.Error($"Template [{template.Name}] upgrade failed.Error : {e}");
                    }
                }
                var result = this.BatchUpdate(needUpgradeTemplates);
                context.SaveChanges();
                logger.Info($"Upgrade template successful, upgrade count {result}");
            }
            catch(Exception e)
            {
                logger.Error("Upgrade template failed,error :{0}", e.ToString());
            }
        }

        private void CheckDefaultBoxTemplate(RMDbContext context)
        {
            if (!context.Template.Any(a => a.UniqueId == BOX_TEMPLATE_ID))
            {
                var entity = new RMTemplate()
                {
                    UniqueId = BOX_TEMPLATE_ID,
                    Name = "RM_Template_Template_Name_Box",
                    Prefix = null,
                    NumberOfDigits = null,
                    Creater = -1,
                    Modifier = -1,
                    CreatedOn = DateTime.UtcNow,
                    LastModifiedOn = DateTime.UtcNow,
                    Type = TemplateType.Box,
                };
                var xml = new TemplateColumnsSchema
                {
                    Columns = new List<ColumnXmlSchema>()
                };
                #region Basic Information
                var categoryUniqueId = BOX_BASIC_INFORMATION_CATEGORY_ID;
                if (!context.TemplateCategory.Any(a => a.UniqueId == categoryUniqueId))
                {
                    RMTemplateCategory t = new RMTemplateCategory();
                    t.UniqueId = categoryUniqueId;
                    t.IsDefault = true;
                    t.Name = "RM_Template_Cagegory_Name_Basic";
                    t.TemplateUniqueId = BOX_TEMPLATE_ID;
                    t.LastModifiedOn = DateTime.UtcNow;
                    context.TemplateCategory.Add(t);
                }

                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.NameOrTitle),
                    ColumnType = ColumnType.SingleText,
                    Name = "RM_Template_Column_Name_Title",
                    Required = true,
                    ShowInEditForm = true,
                    AllowEdit = false
                });
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.Description),
                    ColumnType = ColumnType.MultipleText,
                    Name = "RM_Template_Column_Name_Description",
                    Required = false,
                    ShowInEditForm = true,
                    AllowEdit = true
                });
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.Capability),
                    ColumnType = ColumnType.Number,
                    Name = "RM_Template_Column_Name_Capability",
                    Required = true,
                    ShowInEditForm = true,
                    AllowEdit = false
                });
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.HomeLocation),
                    ColumnType = ColumnType.Taxonomy,
                    Name = "RM_Template_Column_Name_HomeLocation",
                    Required = true,
                    ShowInEditForm = true,
                    AllowEdit = false
                });
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.Status),
                    ColumnType = ColumnType.SingleChoice,
                    Name = "RM_Template_Column_Name_Status",
                    OptionsJSON = JsonConvert.SerializeObject(this.GetDefaultColumnData(new Guid(DefaultColumnIDs.Status))),
                    Required = true,
                    ShowInEditForm = false,
                    AllowEdit = false
                });
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.Classification),
                    ColumnType = ColumnType.Taxonomy,
                    Name = "RM_Template_Column_Name_Classification",
                    Required = true,
                    ShowInEditForm = true,
                    AllowEdit = false
                });
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.LoanedBy),
                    ColumnType = ColumnType.PeopleOrGroup,
                    Name = "RM_PRM_PRE_Column_LoanBy",
                    Required = false,
                    ShowInEditForm = false,
                    AllowEdit = false
                });
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.Barcode),
                    ColumnType = ColumnType.SingleText,
                    Name = "RM_PRM_PRE_Column_Barcode",
                    Required = false,
                    ShowInEditForm = false,
                    AllowEdit = false
                });
                #endregion

                foreach (var column in xml.Columns)
                {
                    column.TemplateInheritSetting = (int)TemplateInheritSettingEnum.None;
                }
                entity.ColumnSchema = SerializerHelper.SerializeByDataContractSerializer(xml);
                context.Template.Add(entity);
                context.SaveChanges();
            }
        }

        private void CheckDefaultFileTemplate(RMDbContext context)
        {
            if (!context.Template.Any(a => a.UniqueId == FOLDER_TEMPLATE_ID))
            {
                var entity = new RMTemplate()
                {
                    UniqueId = FOLDER_TEMPLATE_ID,
                    Name = "RM_Template_Template_Name_Folder",
                    Prefix = null,
                    NumberOfDigits = null,
                    Creater = -1,
                    Modifier = -1,
                    CreatedOn = DateTime.UtcNow,
                    LastModifiedOn = DateTime.UtcNow,
                    Type = TemplateType.Folder,
                    ParentUniqueId = BOX_TEMPLATE_ID
                };
                var xml = new TemplateColumnsSchema
                {
                    Columns = new List<ColumnXmlSchema>()
                };

                #region Basic Information
                var categoryUniqueId = FOLDER_BASIC_INFORMATION_CATEGORY_ID;
                if (!context.TemplateCategory.Any(a => a.UniqueId == categoryUniqueId))
                {
                    RMTemplateCategory t = new RMTemplateCategory();
                    t.UniqueId = categoryUniqueId;
                    t.IsDefault = true;
                    t.Name = "RM_Template_Cagegory_Name_Basic";
                    t.TemplateUniqueId = FOLDER_TEMPLATE_ID;
                    t.LastModifiedOn = DateTime.UtcNow;
                    context.TemplateCategory.Add(t);
                }
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.NameOrTitle),
                    ColumnType = ColumnType.SingleText,
                    Name = "RM_Template_Column_Name_Title",
                    Required = true,
                    ShowInEditForm = true,
                }); ;
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.Description),
                    ColumnType = ColumnType.MultipleText,
                    Name = "RM_Template_Column_Name_Description",
                    Required = false,
                    ShowInEditForm = true,
                    AllowEdit = true
                });
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.Status),
                    ColumnType = ColumnType.SingleChoice,
                    Name = "RM_Template_Column_Name_Status",
                    OptionsJSON = JsonConvert.SerializeObject(this.GetDefaultColumnData(new Guid(DefaultColumnIDs.Status))),
                    Required = true,
                    ShowInEditForm = false,
                    AllowEdit = false
                });
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.Classification),
                    ColumnType = ColumnType.Taxonomy,
                    Name = "RM_Template_Column_Name_Classification",
                    Required = true,
                    ShowInEditForm = true,
                    AllowEdit = false
                });
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.LoanedBy),
                    ColumnType = ColumnType.PeopleOrGroup,
                    Name = "RM_PRM_PRE_Column_LoanBy",
                    OptionsJSON = JsonConvert.SerializeObject(this.GetDefaultColumnData(new Guid(DefaultColumnIDs.LoanedBy))),
                    Required = false,
                    ShowInEditForm = false,
                    AllowEdit = false
                });
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.Barcode),
                    ColumnType = ColumnType.SingleText,
                    Name = "RM_PRM_PRE_Column_Barcode",
                    Required = false,
                    ShowInEditForm = false,
                    AllowEdit = false
                });
                #endregion

                #region Classification Information
                categoryUniqueId = FOLDER_CLASSIFICATION_INFORMATION_CATEGORY_ID;
                if (!context.TemplateCategory.Any(a => a.UniqueId == categoryUniqueId))
                {
                    RMTemplateCategory t = new RMTemplateCategory();
                    t.UniqueId = categoryUniqueId;
                    t.IsDefault = false;
                    t.Name = "RM_Template_Cagegory_Name_Classification";
                    t.TemplateUniqueId = FOLDER_TEMPLATE_ID;
                    t.LastModifiedOn = DateTime.UtcNow;
                    context.TemplateCategory.Add(t);
                }
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.Format),
                    ColumnType = ColumnType.SingleChoice,
                    Name = "RM_Template_Column_Name_Format",
                    OptionsJSON = JsonConvert.SerializeObject(this.GetDefaultColumnData(new Guid(DefaultColumnIDs.Format))),
                    Required = true,
                    ShowInEditForm = true,
                    AllowEdit = true
                });
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.ProtectiveMarking),
                    ColumnType = ColumnType.SingleChoice,
                    Name = "RM_Template_Column_Name_ProtectiveMarking",
                    OptionsJSON = JsonConvert.SerializeObject(this.GetDefaultColumnData(new Guid(DefaultColumnIDs.ProtectiveMarking))),
                    Required = true,
                    ShowInEditForm = true,
                    AllowEdit = true
                });
                #endregion

                #region Statement Information
                categoryUniqueId = FOLDER_STATEMENT_INFORMATION_CATEGORY_ID;
                if (!context.TemplateCategory.Any(a => a.UniqueId == categoryUniqueId))
                {
                    RMTemplateCategory t = new RMTemplateCategory();
                    t.UniqueId = categoryUniqueId;
                    t.IsDefault = true;
                    t.Name = "RM_Template_Cagegory_Name_Statement";
                    t.TemplateUniqueId = FOLDER_TEMPLATE_ID;
                    t.LastModifiedOn = DateTime.UtcNow;
                    context.TemplateCategory.Add(t);
                }
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.Rights),
                    ColumnType = ColumnType.MultipleText,
                    Name = "RM_Template_Column_Name_Rights",
                    Required = false,
                    ShowInEditForm = true,
                    AllowEdit = true
                });
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.Coverage),
                    ColumnType = ColumnType.MultipleText,
                    Name = "RM_Template_Column_Name_Coverage",
                    Required = false,
                    ShowInEditForm = true,
                    AllowEdit = true
                });
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.DateClosed),
                    ColumnType = ColumnType.DateTime,
                    Name = "RM_Template_Column_Name_DataClosed",
                    Required = false,
                    ShowInEditForm = true,
                    AllowEdit = true
                });
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.HomeLocation),
                    ColumnType = ColumnType.Taxonomy,
                    Name = "RM_Template_Column_Name_HomeLocation",
                    Required = false,
                    ShowInEditForm = true,
                });
                #endregion

                foreach (var column in xml.Columns)
                {
                    column.TemplateInheritSetting = (int)TemplateInheritSettingEnum.None;
                }
                entity.ColumnSchema = SerializerHelper.SerializeByDataContractSerializer(xml);
                context.Template.Add(entity);
                context.SaveChanges();
            }
        }

        private void CheckDefaultRecordsTemplate(RMDbContext context)
        {
            if (!context.Template.Any(a => a.UniqueId == RECORD_TEMPLATE_ID))
            {
                var entity = new RMTemplate()
                {
                    UniqueId = RECORD_TEMPLATE_ID,
                    Name = "RM_Template_Template_Name_Record",
                    Prefix = null,
                    NumberOfDigits = null,
                    Creater = -1,
                    Modifier = -1,
                    CreatedOn = DateTime.UtcNow,
                    LastModifiedOn = DateTime.UtcNow,
                    Type = TemplateType.Records,
                    ParentUniqueId = FOLDER_TEMPLATE_ID
                };
                var xml = new TemplateColumnsSchema
                {
                    Columns = new List<ColumnXmlSchema>()
                };

                #region Basic Information
                var categoryUniqueId = RECORD_BASIC_INFORMATION_CATEGORY_ID;
                if (!context.TemplateCategory.Any(a => a.UniqueId == categoryUniqueId))
                {
                    RMTemplateCategory t = new RMTemplateCategory();
                    t.UniqueId = categoryUniqueId;
                    t.IsDefault = true;
                    t.Name = "RM_Template_Cagegory_Name_Basic";
                    t.TemplateUniqueId = RECORD_TEMPLATE_ID;
                    t.LastModifiedOn = DateTime.UtcNow;
                    context.TemplateCategory.Add(t);
                }
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.NameOrTitle),
                    ColumnType = ColumnType.SingleText,
                    Name = "RM_Template_Column_Name_Title",
                    Required = true,
                    ShowInEditForm = true,
                });
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.Description),
                    ColumnType = ColumnType.MultipleText,
                    Name = "RM_Template_Column_Name_Description",
                    Required = false,
                    ShowInEditForm = true,
                    AllowEdit = true
                });
                #endregion

                #region Classification Information
                categoryUniqueId = RECORD_CLASSIFICATION_INFORMATION_CATEGORY_ID;
                if (!context.TemplateCategory.Any(a => a.UniqueId == categoryUniqueId))
                {
                    RMTemplateCategory t = new RMTemplateCategory();
                    t.UniqueId = categoryUniqueId;
                    t.IsDefault = false;
                    t.Name = "RM_Template_Cagegory_Name_Classification";
                    t.TemplateUniqueId = RECORD_TEMPLATE_ID;
                    t.LastModifiedOn = DateTime.UtcNow;
                    context.TemplateCategory.Add(t);
                }
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.Format),
                    ColumnType = ColumnType.SingleChoice,
                    Name = "RM_Template_Column_Name_Format",
                    OptionsJSON = JsonConvert.SerializeObject(this.GetDefaultColumnData(new Guid(DefaultColumnIDs.Format))),
                    Required = true,
                    ShowInEditForm = true,
                    AllowEdit = true
                });
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.ProtectiveMarking),
                    ColumnType = ColumnType.SingleChoice,
                    Name = "RM_Template_Column_Name_ProtectiveMarking",
                    OptionsJSON = JsonConvert.SerializeObject(this.GetDefaultColumnData(new Guid(DefaultColumnIDs.ProtectiveMarking))),
                    Required = true,
                    ShowInEditForm = true,
                    AllowEdit = true
                });
                #endregion

                #region Statement Information
                categoryUniqueId = RECORD_STATEMENT_INFORMATION_CATEGORY_ID;
                if (!context.TemplateCategory.Any(a => a.UniqueId == categoryUniqueId))
                {
                    RMTemplateCategory t = new RMTemplateCategory();
                    t.UniqueId = categoryUniqueId;
                    t.IsDefault = false;
                    t.Name = "RM_Template_Cagegory_Name_Statement";
                    t.TemplateUniqueId = RECORD_TEMPLATE_ID;
                    t.LastModifiedOn = DateTime.UtcNow;
                    context.TemplateCategory.Add(t);
                }
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.Rights),
                    ColumnType = ColumnType.MultipleText,
                    Name = "RM_Template_Column_Name_Rights",
                    Required = false,
                    ShowInEditForm = true,
                    AllowEdit = true
                });
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.Coverage),
                    ColumnType = ColumnType.MultipleText,
                    Name = "RM_Template_Column_Name_Coverage",
                    Required = false,
                    ShowInEditForm = true,
                    AllowEdit = true
                });
                xml.Columns.Add(new ColumnXmlSchema()
                {
                    CategoryId = categoryUniqueId,
                    UniqueId = new Guid(DefaultColumnIDs.HomeLocation),
                    ColumnType = ColumnType.Taxonomy,
                    Name = "RM_Template_Column_Name_HomeLocation",
                    Required = false,
                    ShowInEditForm = true,
                });
                #endregion

                foreach (var column in xml.Columns)
                {
                    column.TemplateInheritSetting = (int)TemplateInheritSettingEnum.None;
                }
                entity.ColumnSchema = SerializerHelper.SerializeByDataContractSerializer(xml);
                context.Template.Add(entity);
                context.SaveChanges();
            }
        }

        private Dictionary<int, string> GetDefaultColumnData(Guid columnGuid)
        {
            var optionsValue = new Dictionary<int, string>();

            if (columnGuid == new Guid(DefaultColumnIDs.Status))
            {
                optionsValue.Add(1, "RM_Template_Column_Value_Status_Open");
                optionsValue.Add(2, "RM_Template_Column_Value_Status_Destroyed");
                optionsValue.Add(6, "RM_Template_Column_Value_Status_Closed");
                optionsValue.Add(7, "RM_Template_Column_Value_Status_Missing");
            }
            else if (columnGuid == new Guid(DefaultColumnIDs.Format))
            {
                optionsValue.Add(1, "RM_Template_Column_Value_Format_Document");
                optionsValue.Add(2, "RM_Template_Column_Value_Format_Cassette");
                optionsValue.Add(3, "RM_Template_Column_Value_Format_Map");
                optionsValue.Add(4, "RM_Template_Column_Value_Format_Play");
                optionsValue.Add(5, "RM_Template_Column_Value_Format_DVD");
            }
            else if (columnGuid == new Guid(DefaultColumnIDs.ProtectiveMarking))
            {
                optionsValue.Add(1, "RM_Template_Column_Value_ProtectiveMarking_InternalUsedOnly");
                optionsValue.Add(2, "RM_Template_Column_Value_ProtectiveMarking_Public");
                optionsValue.Add(3, "RM_Template_Column_Value_ProtectiveMarking_Confidential");
                optionsValue.Add(4, "RM_Template_Column_Value_ProtectiveMarking_HighlyConfidential");
            }
            return optionsValue;
        }

        private void CheckDefaultSuit(RMDbContext context)
        {
            var defaultBoxSuiteId = new Guid(DefaultSuiteIds.RECORD_SUITE_DEFAULT_BOX_SUITE_ID);
            var defaultFolderSuiteId = new Guid(DefaultSuiteIds.RECORD_SUITE_DEFAULT_FOLDER_SUITE_ID);
            if (!context.Suite.Any(a => a.UniqueId == defaultBoxSuiteId))
            {
                var entity = new RMSuite()
                {
                    UniqueId = defaultBoxSuiteId,
                    Name = "RM_Template_Default_Box_Suite_Name",
                    Description = "",
                    StartFromType = SuiteStartFromType.Box,
                    Creater = -1,
                    Modifier = -1,
                    CreatedOn = DateTime.UtcNow,
                    LastModifiedOn = DateTime.UtcNow,
                };
                context.Suite.Add(entity);
                context.SaveChanges();
            }
            if (!context.Suite.Any(a => a.UniqueId == defaultFolderSuiteId))
            {
                var entity = new RMSuite()
                {
                    UniqueId = defaultFolderSuiteId,
                    Name = "RM_Template_Default_Folder_Suite_Name",
                    Description = "",
                    StartFromType = SuiteStartFromType.Folder,
                    Creater = -1,
                    Modifier = -1,
                    CreatedOn = DateTime.UtcNow,
                    LastModifiedOn = DateTime.UtcNow,
                };
                context.Suite.Add(entity);
                context.SaveChanges();
            }
            if (!context.SuiteMembership.Any(a => a.SuiteUniqueId == defaultBoxSuiteId))
            {
                var entity = new RMSuiteMembership()
                {
                    SuiteUniqueId = defaultBoxSuiteId,
                    RootTemplateUniqueId = BOX_TEMPLATE_ID
                };
                context.SuiteMembership.Add(entity);
                context.SaveChanges();
            }
            if (!context.SuiteMembership.Any(a => a.SuiteUniqueId == defaultFolderSuiteId))
            {
                var entity = new RMSuiteMembership()
                {
                    SuiteUniqueId = defaultFolderSuiteId,
                    RootTemplateUniqueId = FOLDER_TEMPLATE_ID
                };
                context.SuiteMembership.Add(entity);
                context.SaveChanges();
            }

            if (!context.SuiteMembership.Any(a => a.SuiteUniqueId == defaultBoxSuiteId && a.BoxTemplateUniqueId == BOX_TEMPLATE_ID && a.FolderTemplateUniqueId == FOLDER_TEMPLATE_ID && a.RecordTemplateUniqueId == Guid.Empty)) {
                var entity = new RMSuiteMembership()
                {
                    SuiteUniqueId = defaultBoxSuiteId,
                    BoxTemplateUniqueId = BOX_TEMPLATE_ID,
                    FolderTemplateUniqueId = FOLDER_TEMPLATE_ID
                };
                context.SuiteMembership.Add(entity);
                context.SaveChanges();
            }

            if (!context.SuiteMembership.Any(a => a.SuiteUniqueId == defaultBoxSuiteId && a.BoxTemplateUniqueId == BOX_TEMPLATE_ID && a.FolderTemplateUniqueId == FOLDER_TEMPLATE_ID && a.RecordTemplateUniqueId == RECORD_TEMPLATE_ID))
            {
                var entity = new RMSuiteMembership()
                {
                    SuiteUniqueId = defaultBoxSuiteId,
                    BoxTemplateUniqueId = BOX_TEMPLATE_ID,
                    FolderTemplateUniqueId = FOLDER_TEMPLATE_ID,
                    RecordTemplateUniqueId = RECORD_TEMPLATE_ID

                };
                context.SuiteMembership.Add(entity);
                context.SaveChanges();
            }

            if (!context.SuiteMembership.Any(a => a.SuiteUniqueId == defaultFolderSuiteId && a.FolderTemplateUniqueId == FOLDER_TEMPLATE_ID && a.RecordTemplateUniqueId == RECORD_TEMPLATE_ID))
            {
                var entity = new RMSuiteMembership()
                {
                    SuiteUniqueId = defaultFolderSuiteId,
                    FolderTemplateUniqueId = FOLDER_TEMPLATE_ID,
                    RecordTemplateUniqueId = RECORD_TEMPLATE_ID
                };
                context.SuiteMembership.Add(entity);
                context.SaveChanges();
            }
        }
        private void UpgradeData(RMDbContext context)
        {
            var relationships = new List<RMTemplateRelationship>();
            var sms = context.SuiteMembership.Where(o => o.UpgradeStatus == Contract.Common.RMDataUpgradeStatus.NotUpgrade).ToList();
            if (sms.Count == 0) return;
            var suites = context.Suite.Select(o => new { o.Id, o.UniqueId });
            var templates = context.Template.Select(o => new { o.Id, o.UniqueId, o.Type });
            var groups = sms.GroupBy(o => o.SuiteUniqueId);
            foreach (var group in groups)
            {
                var suiteUniqueId = group.Key;
                var suite = suites.FirstOrDefault(o => o.UniqueId == suiteUniqueId);
                if (suite == null)
                {
                    logger.Warn($"Can't find the suite with unique id: {suiteUniqueId}");
                    continue;
                }
                var suiteIdPath = suiteUniqueId.ToString() + TemplateUtil.IdPathSeprator;
                Add2List(relationships, suiteIdPath, suiteUniqueId, suiteUniqueId, TemplateType.Suite, 0);  //suite

                foreach (var member in group.OrderByDescending(o => o.RootTemplateUniqueId).ThenByDescending(o => o.BoxTemplateUniqueId).ThenByDescending(o => o.FolderTemplateUniqueId))
                {
                    member.UpgradeStatus = Contract.Common.RMDataUpgradeStatus.Upgraded; //mark upgraded

                    if (member.RootTemplateUniqueId != Guid.Empty)
                    {
                        var rootTemplate = templates.FirstOrDefault(o => o.UniqueId == member.RootTemplateUniqueId);
                        if (rootTemplate != null)
                        {
                            string rootTemplateIdPath = suiteIdPath + rootTemplate.Id.ToString() + TemplateUtil.IdPathSeprator;
                            //Add2List(relationships, rootTemplateIdPath, rootTemplate.UniqueId, rootTemplate.UniqueId, rootTemplate.Type, 0);
                            Add2List(relationships, rootTemplateIdPath, suiteUniqueId, rootTemplate.UniqueId, rootTemplate.Type, 1);
                        }
                        else
                        {
                            logger.Warn($"Root template with unique id : {member.RootTemplateUniqueId} isn't upgraded because it can't be found in template table");
                        }
                        continue;
                    }

                    if (member.BoxTemplateUniqueId != Guid.Empty) // start from box
                    {
                        var boxTemplate = templates.FirstOrDefault(o => o.UniqueId == member.BoxTemplateUniqueId);
                        if (boxTemplate == null) continue;
                        string boxTemplateIdPath = suiteIdPath + boxTemplate.Id.ToString() + TemplateUtil.IdPathSeprator;

                        //Add2List(relationships, boxTemplateIdPath, boxTemplate.UniqueId, boxTemplate.UniqueId, TemplateType.Box, 0);
                        Add2List(relationships, boxTemplateIdPath, suiteUniqueId, boxTemplate.UniqueId, TemplateType.Box, 1);
                        if (member.FolderTemplateUniqueId != Guid.Empty)  //folder
                        {
                            var folderTemplate = templates.FirstOrDefault(o => o.UniqueId == member.FolderTemplateUniqueId);
                            if (folderTemplate == null) continue;
                            string folderTemplateIdPath = boxTemplateIdPath + folderTemplate.Id.ToString() + TemplateUtil.IdPathSeprator;

                            //Add2List(relationships, folderTemplateIdPath, folderTemplate.UniqueId, folderTemplate.UniqueId, TemplateType.Folder, 0);
                            Add2List(relationships, folderTemplateIdPath, boxTemplate.UniqueId, folderTemplate.UniqueId, TemplateType.Folder, 1);
                            Add2List(relationships, folderTemplateIdPath, suiteUniqueId, folderTemplate.UniqueId, TemplateType.Folder, 2);
                            if (member.RecordTemplateUniqueId != Guid.Empty)  //record
                            {
                                var recordTemplate = templates.FirstOrDefault(o => o.UniqueId == member.RecordTemplateUniqueId);
                                if (recordTemplate == null) continue;
                                string recordTemplateIdPath = folderTemplateIdPath + recordTemplate.Id.ToString() + TemplateUtil.IdPathSeprator;

                                //Add2List(relationships, recordTemplateIdPath, recordTemplate.UniqueId, recordTemplate.UniqueId, TemplateType.Records, 0);
                                Add2List(relationships, recordTemplateIdPath, folderTemplate.UniqueId, recordTemplate.UniqueId, TemplateType.Records, 1);
                                Add2List(relationships, recordTemplateIdPath, boxTemplate.UniqueId, recordTemplate.UniqueId, TemplateType.Records, 2);
                                Add2List(relationships, recordTemplateIdPath, suiteUniqueId, recordTemplate.UniqueId, TemplateType.Records, 3);
                            }
                        }
                    }
                    else if (member.FolderTemplateUniqueId != Guid.Empty)// start from folder
                    {
                        var folderTemplate = templates.FirstOrDefault(o => o.UniqueId == member.FolderTemplateUniqueId);
                        if (folderTemplate == null) continue;
                        string folderTemplateIdPath = suiteIdPath + folderTemplate.Id.ToString() + TemplateUtil.IdPathSeprator;

                        //Add2List(relationships, folderTemplateIdPath, folderTemplate.UniqueId, folderTemplate.UniqueId, TemplateType.Folder, 0);
                        Add2List(relationships, folderTemplateIdPath, suiteUniqueId, folderTemplate.UniqueId, TemplateType.Folder, 1);
                        if (member.RecordTemplateUniqueId != Guid.Empty)  //record
                        {
                            var recordTemplate = templates.FirstOrDefault(o => o.UniqueId == member.RecordTemplateUniqueId);
                            if (recordTemplate == null) continue;
                            string recordTemplateIdPath = folderTemplateIdPath + recordTemplate.Id.ToString() + TemplateUtil.IdPathSeprator;

                            //Add2List(relationships, recordTemplateIdPath, recordTemplate.UniqueId, recordTemplate.UniqueId, TemplateType.Records, 0);
                            Add2List(relationships, recordTemplateIdPath, folderTemplate.UniqueId, recordTemplate.UniqueId, TemplateType.Records, 1);
                            Add2List(relationships, recordTemplateIdPath, suiteUniqueId, recordTemplate.UniqueId, TemplateType.Records, 2);
                        }
                    }
                }
            }

            UpgradeData(context, relationships);
        }

        private void Add2List(List<RMTemplateRelationship> relationships, string idPath, Guid ancestor, Guid descendant, TemplateType type, int distance)
        {
            if (!relationships.Exists(o => o.IdPath == idPath && o.Distance == distance))
            {
                relationships.Add(new RMTemplateRelationship
                {
                    IdPath = idPath,
                    Ancestor = ancestor,
                    Descendant = descendant,
                    TemplateType = type,
                    Distance = distance
                });
            }
        }

        private void UpgradeData(RMDbContext context, List<RMTemplateRelationship> relationships)
        {
            context.TemplateRelationship.AddRange(relationships);
            context.SaveChanges();
        }
    }
}
