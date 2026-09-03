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
using AvePoint.RA.Contract.Object;
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
    public class LocationManagementAfterAuditHandler : IAfterAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(LocationManagementAfterAuditHandler));
        private ILocationManagementService LocationManagementService => PlatformWindsorManager.GetService<ILocationManagementService>();
        private ITemplateManagementService TemplateManagementService => PlatformWindsorManager.GetService<ITemplateManagementService>();
        private IRMLocationDao LocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();

        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            RMAuditInfo auditInfo = info != null ? info : new RMAuditInfo();
            auditInfo.Module = (AuditModule)model;
            auditInfo.Category = (AuditCategory)category;
            auditInfo.Action = (AuditAction)action;
            try
            {
                switch ((AuditAction)action)
                {
                    case AuditAction.CreateLocation:
                        var isCreateSucceed = !string.IsNullOrEmpty((string)returnValue);
                        if (isCreateSucceed)
                        {
                            var createdLocation = JsonConvert.DeserializeObject<RMLocation>((string)returnValue);
                            auditInfo.Object = LocationManagementService.GetLocationPathById(createdLocation.UniqueId);
                        }
                        else
                        {
                            var parentLocation = LocationDao.GetLocationById((int)args[1]);
                            auditInfo.Object = LocationManagementService.GetLocationPathById(parentLocation.UniqueId) + $"/{(string)args[0]}";
                            auditInfo.Status = (int)AuditStatus.Failed;
                        }
                        break;
                    case AuditAction.RenameLocation:
                        var locationObject = LocationDao.GetLocationById((int)args[0]);
                        auditInfo.Object = LocationManagementService.GetLocationPathById(locationObject.UniqueId);

                        var renameResultObj = JsonConvert.DeserializeObject<RMLocation>((string)returnValue);
                        bool isRenameSucceed = renameResultObj != null && renameResultObj.UniqueId != Guid.Empty;
                        if (isRenameSucceed)
                        {
                            AuditItem auditItem = auditInfo.ModifyContent.FirstOrDefault();
                            auditItem.NewValue = (string)args[1];
                        }
                        else
                        {
                            auditInfo.Status = (int)AuditStatus.Failed;
                        }
                        break;
                    case AuditAction.DeleteLocation:
                        auditInfo.Status = (bool)returnValue ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                        break;
                    case AuditAction.EditLocationSetting:
                        var editResult = (RAReturnMessage)returnValue;
                        if (editResult.MessageType == RAMessageType.Failed)
                        {
                            auditInfo.Status = (int)AuditStatus.Failed;
                        }
                        else
                        {
                            LocationInfo locationInfo = (LocationInfo)args[0];
                            var editLocationObj = LocationDao.GetLocationById(locationInfo.LocationId);
                            var locationSuiteAssociationIds = LocationDao.GetLocationSuiteAssociationIds(editLocationObj.UniqueId);
                            var selectedSuite = new List<string>();
                            var allSuite = TemplateManagementService.LoadAllSuites();
                            foreach (var suite in allSuite)
                            {
                                if (locationSuiteAssociationIds.Contains(suite.UniqueId))
                                {
                                    selectedSuite.Add(I18NEntity.GetString(suite.Name));
                                }
                            }

                            var descAuditItem = auditInfo.ModifyContent.Where(c => c.TargetSetting == I18NEntity.GetString("RM_TM_TermDescLabel")).First();
                            descAuditItem.NewValue = editLocationObj.Description;

                            var spaceAuditItem = auditInfo.ModifyContent.Where(c => c.TargetSetting == I18NEntity.GetString("RM_LM_LocationSettingTotalSpace")).First();
                            spaceAuditItem.NewValue = editLocationObj.AvailableSpace.ToString();

                            var isBottomAuditItem = auditInfo.ModifyContent.Where(c => c.TargetSetting == I18NEntity.GetString("RM_LM_MinimumLocationSettingDesc")).First();
                            isBottomAuditItem.NewValue = (editLocationObj.NodeType == (int)RMNodeType.PhysicalBottomLocation).ToString();
                            if (editLocationObj.NodeType == (int)RMNodeType.PhysicalBottomLocation)
                            {
                                var locationSuiteAssociationAuditItem = auditInfo.ModifyContent.Where(c => c.TargetSetting == I18NEntity.GetString("RM_SPS_LM_SelectedSuites4Location")).FirstOrDefault();
                                if (locationSuiteAssociationAuditItem == null)
                                {
                                    locationSuiteAssociationAuditItem = new AuditItem();
                                    locationSuiteAssociationAuditItem.TargetSetting = I18NEntity.GetString("RM_SPS_LM_SelectedSuites4Location");
                                    locationSuiteAssociationAuditItem.NewValue = string.Join(";", selectedSuite);
                                    info?.ModifyContent.Add(locationSuiteAssociationAuditItem);
                                }
                                locationSuiteAssociationAuditItem.NewValue = string.Join(";", selectedSuite);
                            }
                        }
                        break;
                    case AuditAction.PhysicalLocationImport:
                        string resultVal = returnValue as string;
                        bool isSuccess = resultVal != null && resultVal == "ok";
                        auditInfo.Status = isSuccess ? (int)AuditStatus.Successful :(int)AuditStatus.Failed;
                        break;
                    case AuditAction.PhysicalBulkUpdateExport:
                    case AuditAction.PhysicalBulkUpdateImport:
                        var jobId = returnValue as string;
                        auditInfo.Object = jobId;
                        break;

                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                logger.Error($"LocationManagementAfterAuditHandler error:{e.ToString()}");
            }
            return auditInfo;
        }
    }
}
