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
using AvePoint.RA.Contract.LocationManagement;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.LocationManagement.AuditHandler
{
    public class LocationManagementBeforeAuditHandler: IBeforeAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(LocationManagementBeforeAuditHandler));
        private ILocationManagementService LocationManagementService => PlatformWindsorManager.GetService<ILocationManagementService>();
        private ITemplateManagementService TemplateManagementService => PlatformWindsorManager.GetService<ITemplateManagementService>();
        private IRMLocationDao LocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();

        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            var info = new RMAuditInfo();
            info.ModifyContent = new List<AuditItem>();

            try
            {
                switch ((AuditAction)action)
                {
                    case AuditAction.RenameLocation:
                        var locationObject = LocationDao.GetLocationById((int)args[0]);
                        AuditItem auditItem = new AuditItem();
                        auditItem.OldValue = locationObject.Name;

                        info.ModifyContent.Add(auditItem);
                        break;
                    case AuditAction.DeleteLocation:
                        var deleteLocationObj = LocationDao.GetLocationById((int)args[0]);
                        info.Object = LocationManagementService.GetLocationPathById(deleteLocationObj.UniqueId);
                        break;
                    case AuditAction.EditLocationSetting:
                        LocationInfo locationInfo = (LocationInfo)args[0];
                        var oldLocationObj = LocationDao.GetLocationById(locationInfo.LocationId);
                        var locationSuiteAssociationIds = LocationDao.GetLocationSuiteAssociationIds(oldLocationObj.UniqueId);
                        var selectedSuite = new List<string>();
                        var allSuite = TemplateManagementService.LoadAllSuites();
                        foreach (var suite in allSuite)
                        {
                            if (locationSuiteAssociationIds.Contains(suite.UniqueId))
                            {
                                selectedSuite.Add(I18NEntity.GetString(suite.Name));
                            }
                        }

                        AuditItem descriptionAuditItem = new AuditItem();
                        descriptionAuditItem.TargetSetting = I18NEntity.GetString("RM_TM_TermDescLabel");
                        descriptionAuditItem.OldValue = oldLocationObj.Description;

                        AuditItem spaceAuditItem = new AuditItem();
                        spaceAuditItem.TargetSetting = I18NEntity.GetString("RM_LM_LocationSettingTotalSpace");
                        spaceAuditItem.OldValue = oldLocationObj.AvailableSpace.ToString();

                        AuditItem isBottomAuditItem = new AuditItem();
                        isBottomAuditItem.TargetSetting = I18NEntity.GetString("RM_LM_MinimumLocationSettingDesc");
                        isBottomAuditItem.OldValue = (oldLocationObj.NodeType == (int)RMNodeType.PhysicalBottomLocation).ToString();

                        AuditItem locationSuiteAssociationAuditItem = new AuditItem();
                        locationSuiteAssociationAuditItem.TargetSetting = I18NEntity.GetString("RM_SPS_LM_SelectedSuites4Location");
                        locationSuiteAssociationAuditItem.OldValue = string.Join(";", selectedSuite);

                        info.Object= LocationManagementService.GetLocationPathById(oldLocationObj.UniqueId);
                        info.ModifyContent.Add(descriptionAuditItem);
                        info.ModifyContent.Add(spaceAuditItem);
                        info.ModifyContent.Add(isBottomAuditItem);
                        if (oldLocationObj.NodeType == (int)RMNodeType.PhysicalBottomLocation)
                        {
                            info.ModifyContent.Add(locationSuiteAssociationAuditItem);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                logger.Error($"LocationManagementBeforeAuditHandler error:{e.Message}");
            }
            return info;
        }
    }
}
