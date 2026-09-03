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
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Core.Upgrade;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BarcodeTemplateType = AvePoint.RA.Contract.TemplateManagement.BarcodeTemplateType;

namespace AvePoint.RA.DB.TenantMigrations.Upgrade.Impl
{
    public class RMBarcodeTemplateUpgradgeDao : BaseDao<RMBarcodeTemplate>, IDbUpgradeDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMBarcodeTemplateUpgradgeDao));
        private List<int> upgradeTemplateList = new List<int>() 
        {
            (int)BarcodeTemplateType.Box,
            (int)BarcodeTemplateType.Folder
        };
        public async Task UpgradeAsync(RMDbContext context)
        {
            try
            {
                ChackDefaultBarcodeTemplate(context);
                CheckDefaultCustomBarcodeTemplate(context);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while upgrade TemplateManagement:{0}", ex.ToString());
            }
        }

        private void ChackDefaultBarcodeTemplate(RMDbContext context)
        {
            try
            {
                if (context.BarcodeTemplate.Count(item => upgradeTemplateList.Contains(item.Type)) == upgradeTemplateList.Count)
                {
                    logger.Info($"The tenant: [{TenantLocalValue.LogonGroupId}] has init barcode template.");
                }
                logger.Info("begin to upgrade barcode tempalte info ");
                using (var tran = context.Database.BeginTransaction())
                {
                    CheckDefaultBoxBarcodeTemplate(context);
                    CheckDefaultFolderBarcodeTemplate(context);
                    tran.Commit();
                }
                logger.Info("success to upgrade tempalte info");
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while upgrade TemplateManagement:{0}", ex.ToString());
            }
        }

        private void CheckDefaultCustomBarcodeTemplate(RMDbContext context)
        {
            try
            {
                if (context.RMCustomBarcodeTemplateSuites.Any(item => item.IsDefault))
                {
                    logger.Info($"The tenant: [{TenantLocalValue.LogonGroupId}] has init custom barcode template suite.");
                }
                else
                {
                    logger.Info($"The tenant: [{TenantLocalValue.LogonGroupId}] has not init custom barcode template suite, begin to create default custom barcode template suite.");
                    CheckDefaultCustomBarcodeTemplateSuite(context);
                }

                if (context.RMCustomBarcodeTemplates.Count(item => item.IsDefault) == upgradeTemplateList.Count)
                {
                    logger.Info($"The tenant: [{TenantLocalValue.LogonGroupId}] has init custom barcode template.");
                }
                else
                {
                    logger.Info($"The tenant: [{TenantLocalValue.LogonGroupId}] has not init custom barcode template, begin to create default custom barcode template.");
                    CheckDefaultCustomBoxBarcodeTemplate(context);
                    CheckDefaultCustomFolderBarcodeTemplate(context);
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while upgrade TemplateManagement:{0}", ex.ToString());
            }
        }

        private void CheckDefaultCustomBarcodeTemplateSuite(RMDbContext context)
        {
            if (!context.RMCustomBarcodeTemplateSuites.Any(a => a.IsDefault))
            {
                var entity = new RMCustomBarcodeTemplateSuite()
                {
                    UniqueId = Guid.NewGuid(),
                    IsDefault = true,
                    Name = I18NEntity.GetString("RM_Custom_Barcode_Template_Suite_Default"),
                    LabelType = BarcodeTemplateLabelType.None,
                    CreatedTime = DateTime.UtcNow.Ticks,
                    ModifiedTime = DateTime.UtcNow.Ticks,
                };
                context.RMCustomBarcodeTemplateSuites.Add(entity);
                context.SaveChanges();
            }
        }

        private void CheckDefaultCustomBoxBarcodeTemplate(RMDbContext context)
        {
            if (!context.RMCustomBarcodeTemplates.Any(a => a.IsDefault && a.Type == BarcodeTemplateType.Box))
            {
                var defaultBoxTemplate = context.BarcodeTemplate.FirstOrDefault(a => a.Type == (int)BarcodeTemplateType.Box);
                var entity = new RMCustomBarcodeTemplate()
                {
                    Type = BarcodeTemplateType.Box,
                    Name = "RM_Custom_Barcode_Template_Suite_Default",
                    SuiteId = context.RMCustomBarcodeTemplateSuites.FirstOrDefault(a => a.IsDefault).UniqueId,
                    IsDefault = true,
                    PropertiesJson = defaultBoxTemplate == null ? string.Empty : SerializerHelper.SerializeByJsonConvert(defaultBoxTemplate),
                };
                context.RMCustomBarcodeTemplates.Add(entity);
                context.SaveChanges();
            }
        }

        private void CheckDefaultCustomFolderBarcodeTemplate(RMDbContext context)
        {
            if (!context.RMCustomBarcodeTemplates.Any(a => a.IsDefault && a.Type == BarcodeTemplateType.Folder))
            {
                var defaultFolderTemplate = context.BarcodeTemplate.FirstOrDefault(a => a.Type == (int)BarcodeTemplateType.Folder);
                var entity = new RMCustomBarcodeTemplate()
                {
                    Type = BarcodeTemplateType.Folder,
                    Name = "RM_Custom_Barcode_Template_Suite_Default",
                    SuiteId = context.RMCustomBarcodeTemplateSuites.FirstOrDefault(a => a.IsDefault).UniqueId,
                    IsDefault = true,
                    PropertiesJson = defaultFolderTemplate == null ? string.Empty : SerializerHelper.SerializeByJsonConvert(defaultFolderTemplate),
                };
                context.RMCustomBarcodeTemplates.Add(entity);
                context.SaveChanges();
            }
        }

        private void CheckDefaultBoxBarcodeTemplate(RMDbContext context)
        {
            if (!context.BarcodeTemplate.Any(a => a.Type == (int)BarcodeTemplateType.Box))
            {
                var entity = new RMBarcodeTemplate()
                {
                    Type = (int)BarcodeTemplateType.Box,
                    ColumnB = "RM_Template_Column_Name_Title",
                    ColumnC = BuildInColumnIDs.RecordsId.ToString(),
                    ColumnE = "RM_Template_Column_Name_Classification",
                    ColumnF = BuildInColumnIDs.ModifiedTime.ToString(),
                    ModifyTime = DateTime.UtcNow.Ticks,
                };
                context.BarcodeTemplate.Add(entity);
                context.SaveChanges();
                var columnMembership = new RMBarcodeTemplateColumnMembership()
                {
                    Type = (int)BarcodeTemplateType.Box,
                    ColumnName = "RM_Template_Column_Name_Description"
                };
                context.BarcodeTemplateColumnMembership.Add(columnMembership);
                context.SaveChanges();
            }
        }

        private void CheckDefaultFolderBarcodeTemplate(RMDbContext context)
        {
            if (!context.BarcodeTemplate.Any(a => a.Type == (int)BarcodeTemplateType.Folder))
            {
                var entity = new RMBarcodeTemplate()
                {
                    Type = (int)BarcodeTemplateType.Folder,
                    ColumnB = "RM_Template_Column_Name_Title",
                    ColumnC = BuildInColumnIDs.RecordsId.ToString(),
                    ColumnE = "RM_Template_Column_Name_Classification",
                    ColumnF = BuildInColumnIDs.ModifiedTime.ToString(),
                    ModifyTime = DateTime.UtcNow.Ticks,
                };
                context.BarcodeTemplate.Add(entity);
                context.SaveChanges();
                var columnMembership = new RMBarcodeTemplateColumnMembership()
                {
                    Type = (int)BarcodeTemplateType.Folder,
                    ColumnName = "RM_Template_Column_Name_Description"
                };
                context.BarcodeTemplateColumnMembership.Add(columnMembership);
                context.SaveChanges();
            }
        }
    }
}
