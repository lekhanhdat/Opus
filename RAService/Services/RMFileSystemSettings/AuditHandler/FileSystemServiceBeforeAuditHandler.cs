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
using Aspose.Pdf.Operators;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Common.Security;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Multi_Geo.Enum;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.Myhub.Items.Actions;
using AvePoint.RA.Contract.Myhub.Items.Views;
using AvePoint.RA.Contract.Myhub.Model.QueryRequest.Actions;
using AvePoint.RA.Contract.Myhub.Permission;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using AvePoint.RA.Service.Services.RMSharePointSettings.AuditHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RMFileSystemSettings.AuditHandler
{
    public class FileSystemServiceBeforeAuditHandler : IBeforeAuditHandler
    {
        private IFSConnectionGroupDao FSGroupDao => PlatformWindsorManager.GetService<IFSConnectionGroupDao>();
        private IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService<IFSConnectionDao>();
        private IRMFSConnectionAndOwnerRelationshipDao RMFSConnectionAndOwnerRelationshipDao => PlatformWindsorManager.GetService<IRMFSConnectionAndOwnerRelationshipDao>();
        private IFileSystemSettingDao FileSystemSettingDao => PlatformWindsorManager.GetService<IFileSystemSettingDao>();
        private IRecordOwnerDao RecordOwnerDao => PlatformWindsorManager.GetService<IRecordOwnerDao>();
        private ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private IRMAgentDao RMAgentDao => PlatformWindsorManager.GetService<IRMAgentDao>();
        private IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService<IManualProcessManagementService>();
        private IRMFunctionSettingDao RMFunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private IMultiGeoSettingService MultiGeoSettingService => PlatformWindsorManager.GetService<IMultiGeoSettingService>();
        private IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        private IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        private IFSConnectionGroupDao FSConnectionGroupDao => PlatformWindsorManager.GetService<IFSConnectionGroupDao>();
        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            var info = new RMAuditInfo();
            action = GetMultiGeoAction(args, action);
            info.ModifyContent = new List<AuditItem>();
            info.Action = (AuditAction)action;
            info.Category = (AuditCategory)category;
            info.Module = (AuditModule)model;
            var isEnableJPMCFeature = RMKeyValueDao.IsEnableJPMCFileSystemFeature();
            if (action == (int)AuditAction.CreateFSGroup)
            {
                ConnectionGroupDto dto = (ConnectionGroupDto)args[0];
                info.Object = dto.Name;
                info.ModifyContent.Add(new AuditItem()
                {
                    TargetSetting = "RM_FS_Register_GroupName",
                    NewValue = dto.Name,
                });
                info.ModifyContent.Add(new AuditItem()
                {
                    TargetSetting = "RM_FS_Register_Description",
                    NewValue = dto.Description,
                });
                info.ModifyContent.Add(new AuditItem()
                {
                    TargetSetting = "RM_FS_Register_SpecifyAgentAccessConn_Type",
                    NewValue = dto.AccessConnectionType == AccessConnectionType.All ? "RM_FS_Register_SpecifyAgentAccessConn_Type_All" : "RM_FS_Register_SpecifyAgentAccessConn_Type_Specify"
                });
                bool isEnableMultiGeoFeature = await MultiGeoSettingService.IsEnableMultiGeoFeature();
                if (isEnableMultiGeoFeature)
                {
                    if (CheckSkipAuditFSGroupMultiGeo(dto))
                    {
                        var skipInfo = new RMAuditInfo();
                        skipInfo.NotNeedRecordAudit = true;
                        return skipInfo;
                    }
                }
                if (isEnableMultiGeoFeature && dto.AccessConnectionType == AccessConnectionType.Specify)
                {
                    var DCSupporteds = await MultiGeoDataCenterService.GetDCsSupported();
                    bool newIsDefaultDC = string.IsNullOrEmpty(dto.DCInternalName);
                    info.ModifyContent.Add(new AuditItem
                    {
                        TargetSetting = "RM_FS_Register_DC_Type",
                        NewValue = newIsDefaultDC ? "RM_FS_Register_DC_Default" : "RM_FS_Register_DC_Specific",
                    });
                    if (!newIsDefaultDC)
                    {
                        info.ModifyContent.Add(new AuditItem
                        {
                            TargetSetting = "RM_FS_Register_DC_Selected",
                            NewValue = newIsDefaultDC ? string.Empty : (DCSupporteds.FirstOrDefault(dc => dc.DCInternalName == dto.DCInternalName)?.DCDisplayName ?? string.Empty),
                        });
                    }
                }
            }
            else if (action == (int)AuditAction.EditFSGroup)
            {
                ConnectionGroupDto dto = (ConnectionGroupDto)args[0];
                info.Object = dto.Name;
                var dbGroup = FSGroupDao.GetGroup(dto.Id);
                if (dbGroup.Name != dto.Name)
                {
                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_FS_Register_GroupName",
                        OldValue = dbGroup.Name,
                        NewValue = dto.Name,
                    });
                }
                if (dbGroup.Description != dto.Description)
                {
                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_FS_Register_Description",
                        OldValue = dbGroup.Description,
                        NewValue = dto.Description,
                    });
                }
                if (dbGroup.AccessConnectionType != dto.AccessConnectionType)
                {
                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_FS_Register_SpecifyAgentAccessConn_Type",
                        OldValue = dbGroup.AccessConnectionType == AccessConnectionType.All ? "RM_FS_Register_SpecifyAgentAccessConn_Type_All" : "RM_FS_Register_SpecifyAgentAccessConn_Type_Specify",
                        NewValue = dto.AccessConnectionType == AccessConnectionType.All ? "RM_FS_Register_SpecifyAgentAccessConn_Type_All" : "RM_FS_Register_SpecifyAgentAccessConn_Type_Specify"
                    });
                }
                if (dto.FSConnections != null)
                {
                    var oldConnsStr = string.Join(";", (dbGroup.FSConnections ?? new List<FSConnection>()).Select(c => c.Name).Where(name => !string.IsNullOrWhiteSpace(name)).OrderBy(name => name));
                    var newConnsStr = string.Join(";", FSConnectionDao.GetConnectionByIds(dto.FSConnections.Select(c => c.Id).ToList()).Select(c => c.Name).Where(name => !string.IsNullOrWhiteSpace(name)).OrderBy(name => name));
                    if (oldConnsStr != newConnsStr)
                    {
                        info.ModifyContent.Add(new AuditItem
                        {
                            TargetSetting = "RM_FS_Register_ConnectionName",
                            OldValue = oldConnsStr,
                            NewValue = newConnsStr
                        });
                    }
                }
                if (dto.Agents != null)
                {
                    var oldAgents = dbGroup.Agents ?? new List<RMAgent>();
                    var oldAgentIds = oldAgents.Select(a => a.Id).Where(id => id != Guid.Empty).OrderBy(id => id).ToList();
                    var newAgentIds = dto.Agents.Select(a => a.Id).Where(id => id != Guid.Empty).OrderBy(id => id).ToList();
                    var accessTypeChanged = dbGroup.AccessConnectionType != dto.AccessConnectionType;
                    var agentIdsChanged = !oldAgentIds.SequenceEqual(newAgentIds);
                    if (accessTypeChanged || agentIdsChanged)
                    {
                        var oldAgentsStr = dbGroup.AccessConnectionType == AccessConnectionType.All
                            ? string.Empty
                            : string.Join(";", oldAgents.Select(a => a.Name).Where(name => !string.IsNullOrWhiteSpace(name)).OrderBy(name => name));
                        var newAgents = newAgentIds.Any() ? (await RMAgentDao.FindListAsync(a => newAgentIds.Contains(a.Id))).ToList() : new List<RMAgent>();
                        var newAgentsStr = dto.AccessConnectionType == AccessConnectionType.All
                            ? string.Empty
                            : string.Join(";", newAgents.Select(a => a.Name).Where(name => !string.IsNullOrWhiteSpace(name)).OrderBy(name => name));
                        info.ModifyContent.Add(new AuditItem
                        {
                            TargetSetting = "RM_CP_Audit_AGM_Agent_Name",
                            OldValue = oldAgentsStr,
                            NewValue = newAgentsStr
                        });
                    }
                }
                bool isEnableMultiGeoFeature = await MultiGeoSettingService.IsEnableMultiGeoFeature();
                if (isEnableMultiGeoFeature)
                {
                    if (CheckSkipAuditFSGroupMultiGeo(dto))
                    {
                        var skipInfo = new RMAuditInfo();
                        skipInfo.NotNeedRecordAudit = true;
                        return skipInfo;
                    }
                }
                if (isEnableMultiGeoFeature && dbGroup.AccessConnectionType == AccessConnectionType.Specify && dbGroup.DCInternalName != dto.DCInternalName)
                {
                    var DCSupporteds = await MultiGeoDataCenterService.GetDCsSupported();                  
                    bool oldIsDefaultDC = string.IsNullOrEmpty(dbGroup.DCInternalName);
                    bool newIsDefaultDC = string.IsNullOrEmpty(dto.DCInternalName);
                    info.ModifyContent.Add(new AuditItem
                    {
                        TargetSetting = "RM_FS_Register_DC_Type",
                        OldValue = oldIsDefaultDC ? "RM_FS_Register_DC_Default" : "RM_FS_Register_DC_Specific",
                        NewValue = newIsDefaultDC ? "RM_FS_Register_DC_Default" : "RM_FS_Register_DC_Specific",
                    });
                    if (!oldIsDefaultDC || !newIsDefaultDC)
                    {
                        info.ModifyContent.Add(new AuditItem
                        {
                            TargetSetting = "RM_FS_Register_DC_Selected",
                            OldValue = oldIsDefaultDC ? string.Empty : (DCSupporteds.FirstOrDefault(dc => dc.DCInternalName == dbGroup.DCInternalName)?.DCDisplayName ?? string.Empty),
                            NewValue = newIsDefaultDC ? string.Empty : (DCSupporteds.FirstOrDefault(dc => dc.DCInternalName == dto.DCInternalName)?.DCDisplayName ?? string.Empty),
                        });
                    }
                }
            }
            else if (action == (int)AuditAction.CreateFSConnection)
            {
                ConnectionDto dto = (ConnectionDto)args[0];
                info.Object = dto.Name;
                info.ModifyContent.Add(new AuditItem()
                {
                    TargetSetting = "RM_FS_Register_ConnectionName",
                    NewValue = dto.Name,
                });
                info.ModifyContent.Add(new AuditItem()
                {
                    TargetSetting = "RM_FS_Register_Description",
                    NewValue = dto.Description,
                });
                if (isEnableJPMCFeature)
                {
                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_FS_Register_JPMCId",
                        NewValue = dto.JPMCConnectionId,
                    });
                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_FS_Register_Path",
                        NewValue = dto.UNCPath,
                    });
                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_FS_Register_Information_Owner",
                        NewValue = dto.InformationOwners != null ? string.Join("; ", dto.InformationOwners.Select(x => x.DisplayName)) : string.Empty,
                    });
                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_FS_Register_Records_Owner",
                        NewValue = dto.RecordOwners != null ? string.Join("; ", dto.RecordOwners.Select(x => x.DisplayName)) : string.Empty,
                    });
                }
                else
                {
                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_FS_Register_UNCPath",
                        NewValue = dto.UNCPath,
                    });
                }

                if (dto.GroupId != Guid.Empty)
                {
                    var dtoGroup = FSGroupDao.GetGroupById(dto.GroupId);
                    if (dtoGroup != null)
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_FS_Register_AddToConnectionGroup",
                            NewValue = dtoGroup.Name,
                        });
                    }
                }
                bool isEnableMultiGeoFeature = await MultiGeoSettingService.IsEnableMultiGeoFeature();
                if (isEnableMultiGeoFeature)
                {
                    var actionGeo = (AuditAction)action;
                    if (await CheckSkipAuditConnectionMultiGeo(dto,null, actionGeo, info))
                    {
                        var skipInfo = new RMAuditInfo();
                        skipInfo.NotNeedRecordAudit = true;
                        return skipInfo;
                    }
                }
            }
            else if (action == (int)AuditAction.EditFSConnection)
            {
                ConnectionDto dto = (ConnectionDto)args[0];
                info.Object = dto.Name;
                var dbConn = FSConnectionDao.GetConnectionById(dto.Id);
                if (dbConn.Name != dto.Name)
                {
                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_FS_Register_ConnectionName",
                        OldValue = dbConn.Name,
                        NewValue = dto.Name,
                    });
                }
                if (dbConn.Description != dto.Description)
                {
                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_FS_Register_Description",
                        OldValue = dbConn.Description,
                        NewValue = dto.Description,
                    });
                }
                if (dbConn.GroupId != dto.GroupId)
                {
                    var dbGroupName = "";
                    var dtoGroupName = "";
                    if (dbConn.GroupId != Guid.Empty)
                    {
                        var dbGroup = FSGroupDao.GetGroupById(dbConn.GroupId);
                        if (dbGroup != null)
                        {
                            dbGroupName = dbGroup.Name;
                        }
                    }
                    if (dto.GroupId != Guid.Empty)
                    {
                        var dtoGroup = FSGroupDao.GetGroupById(dto.GroupId);
                        if (dtoGroup != null)
                        {
                            dtoGroupName = dtoGroup.Name;
                        }
                    }
                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_FS_Register_AddToConnectionGroup",
                        OldValue = dbGroupName,
                        NewValue = dtoGroupName,
                    });
                }
                if (isEnableJPMCFeature)
                {
                    if (!dto.IsEditConnectionPage)
                    {
                        info.Action = AuditAction.PermissionChange;
                    }
                    if (dbConn.UNCPath != dto.UNCPath)
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_FS_Register_Path",
                            OldValue = dbConn.UNCPath,
                            NewValue = dto.UNCPath,
                        });
                    }
                    var dbOwners = RMFSConnectionAndOwnerRelationshipDao.GetOwnersByConnectionId(dto.Id);
                    var userIds = dbOwners.Select(r => r.UserIntId).Distinct().ToList();
                    var dbOwnersId = AccountDao.GetUserByIdsAsync(userIds).Result;

                    var dbInformationOwners = dbOwners.Where(r => r.Type == FSConnectionOwnerType.InformationOwner).Join(dbOwnersId, rel => rel.UserIntId, owner => owner.Id, (rel, owner) => owner.DisplayName).OrderBy(name => name).ToList();
                    var dbRecordOwners = dbOwners.Where(r => r.Type == FSConnectionOwnerType.RecordOwner).Join(dbOwnersId, rel => rel.UserIntId, owner => owner.Id, (rel, owner) => owner.DisplayName).OrderBy(name => name).ToList();
                    var dtoInformationOwners = dto.InformationOwners != null ? dto.InformationOwners.Select(x => x.DisplayName).OrderBy(name => name).ToList() : new List<string>();
                    var dtoRecordOwners = dto.RecordOwners != null ? dto.RecordOwners.Select(x => x.DisplayName).OrderBy(name => name).ToList() : new List<string>();

                    string oldInformationOwnersStr = string.Join("; ", dbInformationOwners);
                    string newInformationOwnersStr = string.Join("; ", dtoInformationOwners);

                    string oldRecordOwnersStr = string.Join("; ", dbRecordOwners);
                    string newRecordOwnersStr = string.Join("; ", dtoRecordOwners);
                    if (oldInformationOwnersStr != newInformationOwnersStr)
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_FS_Register_Information_Owner",
                            OldValue = oldInformationOwnersStr,
                            NewValue = newInformationOwnersStr,
                        });
                    }
                    if (oldRecordOwnersStr != newRecordOwnersStr)
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_FS_Register_Records_Owner",
                            OldValue = oldRecordOwnersStr,
                            NewValue = newRecordOwnersStr,
                        });
                    }
                }
                else
                {
                    if (dbConn.UNCPath != dto.UNCPath)
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_FS_Register_UNCPath",
                            OldValue = dbConn.UNCPath,
                            NewValue = dto.UNCPath,
                        });
                    }
                }
                bool isEnableMultiGeoFeature = await MultiGeoSettingService.IsEnableMultiGeoFeature();
                if (isEnableMultiGeoFeature)
                {
                    var actionGeo = (AuditAction)action;
                    if (await CheckSkipAuditConnectionMultiGeo(dto,null, actionGeo, info))
                    {
                        var skipInfo = new RMAuditInfo();
                        skipInfo.NotNeedRecordAudit = true;
                        return skipInfo;
                    }
                }
            }
            else if (action == (int)AuditAction.FSConnectionCorrelateGroup)
            {
                CorrelateConnectionDto dto = (CorrelateConnectionDto)args[0];
                var group = FSGroupDao.GetGroupById(dto.GroupId);
                info.Object = group.Name;
                var dbConns = FSConnectionDao.GetAllConnectionsByGroupId(dto.GroupId);
                var dtoConns = FSConnectionDao.GetConnectionByIds(dto.ConnectionIdList);

                info.ModifyContent.Add(new AuditItem()
                {
                    TargetSetting = "RM_FS_Register_EditCorrelateConnections",
                    OldValue = string.Join(";", dbConns.Select(c => c.Name)),
                    NewValue = string.Join(";", dtoConns.Select(c => c.Name)),
                });
            }
            else if (action == (int)AuditAction.FSConnectionValidationTest)
            {

            }
            else if (action == (int)AuditAction.DeleteFSGroup)
            {
                var groups = FSGroupDao.GetGroupByIds((List<Guid>)args[0]);
                bool isEnableMultiGeoFeature = await MultiGeoSettingService.IsEnableMultiGeoFeature();
                if (isEnableMultiGeoFeature)
                {
                    if (CheckSkipAuditDeleteGroupMultiGeo(groups, info))
                    {
                        var skipInfo = new RMAuditInfo();
                        skipInfo.NotNeedRecordAudit = true;
                        return skipInfo;
                    }
                }
                info.Object = string.Join(";", groups.Select(g => g.Name));
            }
            else if (action == (int)AuditAction.DeleteFSConnection)
            {
                var conns = FSConnectionDao.GetConnectionByIds((List<Guid>)args[0]);
                bool isEnableMultiGeoFeature = await MultiGeoSettingService.IsEnableMultiGeoFeature();
                if (isEnableMultiGeoFeature)
                {
                    var actionGeo = (AuditAction)action;
                    if (await CheckSkipAuditConnectionMultiGeo(null, conns, actionGeo, info))
                    {
                        var skipInfo = new RMAuditInfo();
                        skipInfo.NotNeedRecordAudit = true;
                        return skipInfo;
                    }
                    if (string.IsNullOrEmpty(info.Object))
                    {
                        info.Object = string.Join(";", conns.Select(c => c.Name));
                    }
                }
                else
                {
                    info.Object = string.Join(";", conns.Select(c => c.Name));
                }
            }


            if (action == (int)AuditAction.FSEditLocationOwnersSetting)
            {
                #region AuditAction.FSEditLocationOwnersSetting
                RMFSTreeNode node = (RMFSTreeNode)args[0];

                //info.Object = EncodeUtil.DecryptByCommunicationKey(node.FullPath);
                info.Object = node.FullPath;
                var dbSetting = FileSystemSettingDao.LoadFSSetting(node.Id, node.ConnGroupId);
                var enableApprovalAudit = new AuditItem { TargetSetting = "RM_BCM_ManualApproval_Title_EnableApproval" };
                var processAudit = new AuditItem { TargetSetting = "RM_JS_RDM_ManualApproval_ProcessName" };
                var emailAudit = new AuditItem { TargetSetting = "RM_JS_SPS_EditKey_EmailNotifiation" };
                var ownerAudit = new AuditItem { TargetSetting = "RM_SPS_RecordOwners" };
                string ownerOldValue = string.Empty, ownerNewValue = string.Empty;
                ownerNewValue = node.RecordOwner.Count > 0 ? string.Join(";", node.RecordOwner.Select(a => a.DisplayName)) : string.Empty;

                enableApprovalAudit.NewValue = YesOrNoString(node.ApprovalType != (int)ApprovalType.None);
                if (node.ApprovalType != (int)ApprovalType.None)
                {
                    if (node.ApprovalType == (int)ApprovalType.ApprovalProcess)
                    {
                        var workflow = ManualProcessManagementService.GetWorkflow(new Guid(node.WorkflowReferenceId));
                        if (!string.IsNullOrEmpty(workflow?.Name))
                        {
                            processAudit.NewValue = workflow?.Name;
                        }
                    }
                    emailAudit.NewValue = YesOrNoString(node.EMailToRecordOwner);
                    ownerAudit.NewValue = ownerNewValue;
                }

                if (dbSetting != null)
                {
                    enableApprovalAudit.OldValue = YesOrNoString(dbSetting.ApprovalType != ApprovalType.None);
                    List<string> recordOwnerIDs = RecordOwnerDao.GetRecordOwner(dbSetting.Id, RecordOwnerSettingType.FileSystem).Select(a => a.ObjectId).ToList();
                    List<string> recordOwners = (await AccountDao.FindListAsync(o => recordOwnerIDs.Contains(o.UserId))).Select(a => a.DisplayName).ToList();
                    ownerOldValue = recordOwners.Count > 0 ? string.Join(";", recordOwners) : string.Empty;

                    if (dbSetting.ApprovalType != ApprovalType.None)
                    {
                        if (dbSetting.ApprovalType == ApprovalType.ApprovalProcess)
                        {
                            var workflow = ManualProcessManagementService.GetWorkflow(new Guid(dbSetting.WorkflowReferenceId));
                            if (!string.IsNullOrEmpty(workflow?.Name))
                            {
                                processAudit.OldValue = workflow?.Name;
                            }
                        }

                        emailAudit.OldValue = YesOrNoString(dbSetting.EMailToRecordOwner);
                        ownerAudit.OldValue = ownerOldValue;
                    }
                }
                info.ModifyContent.Add(enableApprovalAudit);
                info.ModifyContent.Add(processAudit);
                info.ModifyContent.Add(ownerAudit);
                info.ModifyContent.Add(emailAudit);
                #endregion
            }
            else if (action == (int)AuditAction.FSEditDocLevelSetting)
            {
                #region AuditAction.FSEditDocLevelSetting
                RMFSTreeNode node = (RMFSTreeNode)args[0];

                //info.Object = EncodeUtil.DecryptByCommunicationKey(node.FullPath);
                info.Object = node.FullPath;
                var dbSetting = FileSystemSettingDao.LoadFSSetting(node.Id, node.ConnGroupId);
                if (dbSetting != null)
                {
                    bool oldApplyExistDocument = false;
                    string newSubsetPath = string.Empty;
                    string oldSubsetPath = string.Empty;
                    oldApplyExistDocument = dbSetting.NeedCheckDefaultValue;
                    if (node.TermId != Guid.Empty)
                    {
                        newSubsetPath = TermDao.GetTermNamesPathByTermId(node.TermId);
                    }
                    else if (node.TermSetId != Guid.Empty)
                    {
                        newSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(node.TermSetId);
                    }

                    if (dbSetting.TermId != Guid.Empty)
                    {
                        oldSubsetPath = TermDao.GetTermNamesPathByTermId(dbSetting.TermId);
                    }
                    else if (dbSetting.TermSetId != Guid.Empty)
                    {
                        oldSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(dbSetting.TermSetId);
                    }
                    string newPath = string.Empty;
                    string oldPath = string.Empty;
                    if (node.DefaultTermId != Guid.Empty)
                    {
                        newPath = TermDao.GetTermNamesPathByTermId(node.DefaultTermId);
                    }
                    else
                    {
                        newPath = "RM_SS_NoDefaultValue";
                    }
                    if (dbSetting.DefaultTermId != Guid.Empty)
                    {
                        oldPath = TermDao.GetTermNamesPathByTermId(dbSetting.DefaultTermId);
                    }
                    else
                    {
                        oldPath = "RM_SS_NoDefaultValue";
                    }
                    //if (node.Level != (int)NodeLevel.WebApplication)
                    //{
                    //}
                    if (!isEnableJPMCFeature)
                    {
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_SubsetTerm", NewValue = newSubsetPath });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_AutoClassification_DeployTermMethod",
                            NewValue = ContentRepositoryAuditUtil.GetApplyTermMethodString(node.DeployTermMethod)
                        });
                        if ((DeployTermMethod)dbSetting.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                        {
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_DefaultValue", OldValue = oldPath });
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_GS_ApplyExistingDoc", OldValue = GetApplyExistString(oldApplyExistDocument, dbSetting.ApplyExistType) });
                        }
                        if (node.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                        {
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_DefaultValue", NewValue = newPath });
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_GS_ApplyExistingDoc", NewValue = GetApplyExistString(node.NeedCheckDefaultValue, node.ApplyExistType) });
                        }
                        if ((DeployTermMethod)dbSetting.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                        {
                            var oldAutoRules = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(dbSetting.AutoClassificationRules);
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy", OldValue = ContentRepositoryAuditUtil.GetRulesCretiaString(oldAutoRules) });
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", OldValue = ContentRepositoryAuditUtil.GetSkipOverrideString((AutoJobOption)dbSetting.AutoJobOption) });
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_Auto_RunFullJob", OldValue = YesOrNoString(dbSetting.RunAutoFullJob) });
                        }
                        if (node.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                Id = ContentRepositoryAuditUtil.NeedReAuditorInAfter,//NeedReAuditorInAfter代表这条audit不完善，需要在AfterHandler里继续完善
                                TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy",
                                NewValue = ContentRepositoryAuditUtil.GetRulesCretiaString(node.AutoClassificationRules)
                            });
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(node.AutoJobOption) });
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(node.RunAutoFullJob) });
                        }
                    }
                    else
                    {
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_FS_EditKey_ClassCodeScope", NewValue = newSubsetPath, OldValue = oldSubsetPath });
                    }
                }
                else
                {
                    if (node.Level != (int)NodeLevel.WebApplication)
                    {
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "", OldValue = "RM_JS_TM_inherBreak" });
                    }
                    string newSubsetPath = string.Empty;
                    if (node.TermId != Guid.Empty)
                    {
                        newSubsetPath = TermDao.GetTermNamesPathByTermId(node.TermId);
                    }
                    else if (node.TermSetId != Guid.Empty)
                    {
                        newSubsetPath = TermDao.GetTermSetNamesPathByTermSetId(node.TermSetId);
                    }

                    string newPath = string.Empty;
                    if (node.DefaultTermId != Guid.Empty)
                    {
                        newPath = TermDao.GetTermNamesPathByTermId(node.DefaultTermId);
                    }
                    else
                    {
                        newPath = "RM_SS_NoDefaultValue";
                    }

                    //if (node.Level != (int)NodeLevel.WebApplication)
                    //{
                    //}
                    if (!isEnableJPMCFeature)
                    {
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_SubsetTerm", NewValue = newSubsetPath });
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_SPS_AutoClassification_DeployTermMethod",
                            NewValue = ContentRepositoryAuditUtil.GetApplyTermMethodString(node.DeployTermMethod)
                        });
                        if (node.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
                        {
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_DefaultValue", NewValue = newPath });
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_GS_ApplyExistingDoc", NewValue = GetApplyExistString(node.NeedCheckDefaultValue, node.ApplyExistType) });
                        }
                        if (node.DeployTermMethod == DeployTermMethod.UseAutoClassification)
                        {
                            info.ModifyContent.Add(new AuditItem()
                            {
                                Id = ContentRepositoryAuditUtil.NeedReAuditorInAfter,//NeedReAuditorInAfter代表这条audit不完善，需要在AfterHandler里继续完善
                                TargetSetting = "RM_JS_SPS_AutoClassification_ApplyPolicy",
                                NewValue = ContentRepositoryAuditUtil.GetRulesCretiaString(node.AutoClassificationRules)
                            });
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_AutoClassification_SkipOverrideOption", NewValue = ContentRepositoryAuditUtil.GetSkipOverrideString(node.AutoJobOption) });
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_SPS_Auto_RunFullJob", NewValue = YesOrNoString(node.RunAutoFullJob) });
                        }
                    }
                    else
                    {
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_FS_EditKey_ClassCodeScope", NewValue = newSubsetPath });
                    }
                }
                #endregion
            }
            else if (action == (int)AuditAction.FSEditGeneralSettingForJPMC)
            {
                RMFSTreeNode node = (RMFSTreeNode)args[0];

                //info.Object = EncodeUtil.DecryptByCommunicationKey(node.FullPath);
                info.Object = node.FullPath;
                var dbSetting = FileSystemSettingDao.LoadFSSetting(node.Id, node.ConnGroupId);
                if (dbSetting != null)
                {
                    bool oldEnableIL = dbSetting.EnableRecordManagement == (int)RMFSTreeNode.EnableRecordManagementSetting.Enable;
                    bool newEnableIL = node.EnableRecordManagement == (int)RMFSTreeNode.EnableRecordManagementSetting.Enable;
                    bool oldEnableDownloadRCCReport = dbSetting.IsAllowUserDownloadRCCReport == true;
                    bool newEnableDownloadRCCReport = node.IsAllowUserDownloadRCCReport == true;

                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_SPS_GS_ManagedScope",
                        OldValue = YesOrNoString(oldEnableIL),
                        NewValue = YesOrNoString(newEnableIL),
                    });

                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_JS_FS_DownloadRCCReport",
                        OldValue = YesOrNoString(oldEnableDownloadRCCReport),
                        NewValue = YesOrNoString(newEnableDownloadRCCReport)
                    });
                }
                else
                {
                    if (node.Level != (int)NodeLevel.WebApplication)
                    {
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "", OldValue = "RM_JS_TM_inherBreak" });
                    }
                    bool newEnableIL = node.EnableRecordManagement == (int)RMFSTreeNode.EnableRecordManagementSetting.Enable;
                    bool newEnableDownloadRCCReport = node.IsAllowUserDownloadRCCReport;

                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_SPS_GS_ManagedScope",
                        NewValue = YesOrNoString(newEnableIL),
                    });

                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_JS_FS_DownloadRCCReport",
                        NewValue = YesOrNoString(newEnableDownloadRCCReport)
                    });

                    //if (node.Level != (int)NodeLevel.WebApplication)
                    //{
                    //}
                }
            }
            else if (action == (int)AuditAction.FSEditInheritSetting)
            {
                #region AuditAction.FSEditInheritSetting
                RMFSTreeNode node = (RMFSTreeNode)args[0];
                //info.Object = EncodeUtil.DecryptByCommunicationKey(node.FullPath);
                info.Object = node.FullPath;
                #endregion
            }
            else if (action == (int)AuditAction.FSDeactiveSetting)
            {
                #region AuditAction.FSDeactiveSetting
                RMFSTreeNode node = (RMFSTreeNode)args[0];
                if (node.IsActive)
                {
                    info.Action = AuditAction.FSActiveSetting;
                }
                //info.Object = EncodeUtil.DecryptByCommunicationKey(node.FullPath);
                info.Object = node.FullPath;
                var dbSetting = FileSystemSettingDao.LoadFSSetting(node.Id, node.ConnGroupId);
                #endregion
            }
            else if (action == (int)AuditAction.FSClassificationSetting)
            {
                int classificationLevel = (int)args[0];
                int dbLevel = 0;
                RMFunctionSetting setting;
                RMFunctionSettingDao.TryGet(Contract.FunctionSetting.FunctionSettingType.ClassificationLevelSetting, out setting);
                NodeLevel result;
                if (setting == null)
                {
                    dbLevel = (int)NodeLevel.FSFile;
                }
                else if (Enum.TryParse<NodeLevel>(setting.SettingInfo, out result))
                {
                    dbLevel = (int)result;
                }

                info.ModifyContent.Add(new AuditItem()
                {
                    TargetSetting = "RM_RC_Audit_FSClassificationSetting_Edit",
                    OldValue = GetAuditItemValue(dbLevel),
                    NewValue = GetAuditItemValue(classificationLevel)
                });
            }
            else if (action == (int)AuditAction.ApplyClassCodeSettings4FS|| action == (int)AuditAction.MyhubClassify)
            {
                #region AuditAction.ApplyClassCodeSettings4FS
                ClassCodePolicyInfo classCodeInfo = (ClassCodePolicyInfo)args[0];

                //info.Object = EncodeUtil.DecryptByCommunicationKey(node.FullPath);
                info.Object = classCodeInfo.FSTreeNode.FullPath;
                var dbSetting = FileSystemSettingDao.LoadFSSetting(classCodeInfo.FSTreeNode.Id, classCodeInfo.FSTreeNode.ConnGroupId);
                var retentionType = classCodeInfo.RetentionScheduleType == RetentionScheduleType.Event ? RetentionScheduleType.Event : RetentionScheduleType.Flat;
                if (dbSetting != null)
                {

                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_FS_ClassCodePolicy_ClassCode", NewValue = classCodeInfo.ClassCode, OldValue = dbSetting.ClassCode });
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_FS_ClassCodePolicy_CountryCode", NewValue = classCodeInfo.CountryCode, OldValue = dbSetting.CountryCode });
                    if (dbSetting.ClassCode == null)
                    {
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_FS_ClassCodePolicy_RetentionType", NewValue = retentionType == RetentionScheduleType.Event ? "RM_FS_ClassCodePolicy_RetentionEventType" : "RM_FS_ClassCodePolicy_RetentionFlatType" });
                        var newEffectScopeValue = classCodeInfo.ApplyExistDocument ? "RM_FS_ClassCodePolicy_ApplyAllNodes" : "RM_FS_ClassCodePolicy_ApplySelectedNode";
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_FS_Export_EffectScopeColumn", NewValue = newEffectScopeValue });
                    }
                    if (dbSetting.ClassCode != null)
                    {
                        var newEffectScopeValue = classCodeInfo.ApplyExistDocument ? "RM_FS_ClassCodePolicy_ApplyAllNodes" : "RM_FS_ClassCodePolicy_ApplySelectedNode";
                        var oldEffectScopeValue = dbSetting.ApplyExistDocument ? "RM_FS_ClassCodePolicy_ApplyAllNodes" : "RM_FS_ClassCodePolicy_ApplySelectedNode";
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_FS_Export_EffectScopeColumn", NewValue = newEffectScopeValue, OldValue = oldEffectScopeValue });
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_FS_ClassCodePolicy_RetentionType", NewValue = retentionType == RetentionScheduleType.Event ? "RM_FS_ClassCodePolicy_RetentionEventType" : "RM_FS_ClassCodePolicy_RetentionFlatType", OldValue = dbSetting.RetentionScheduleType == RetentionScheduleType.Event ? "RM_FS_ClassCodePolicy_RetentionEventType" : "RM_FS_ClassCodePolicy_RetentionFlatType" });
                    }
                    string oldValue = null;
                    string newValue = null;

                    if (dbSetting.RetentionScheduleType == RetentionScheduleType.Event)
                    {
                        oldValue = dbSetting.StartDate != 0
                            ? (await GeneralSettingService.ConvertTiksToDateTimeAsync(dbSetting.StartDate, true)).SimplifyFormatTime
                            : string.Empty;
                    }

                    if (retentionType == RetentionScheduleType.Event)
                    {
                        var ticks = classCodeInfo.StartDate.Ticks;
                        newValue = ticks != 0
                            ? (await GeneralSettingService.ConvertTiksToDateTimeAsync(ticks, true)).SimplifyFormatTime
                            : string.Empty;
                    }
                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_FS_ClassCodePolicy_StartDate",
                        OldValue = oldValue,
                        NewValue = newValue
                    });
                }
                else
                {
                    if (classCodeInfo.FSTreeNode.Level != (int)NodeLevel.WebApplication)
                    {
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "", OldValue = "RM_JS_TM_inherBreak" });
                    }
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_FS_ClassCodePolicy_ClassCode", NewValue = classCodeInfo.ClassCode });
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_FS_ClassCodePolicy_CountryCode", NewValue = classCodeInfo.CountryCode });
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_FS_ClassCodePolicy_RetentionType", NewValue = retentionType == RetentionScheduleType.Event ? "RM_FS_ClassCodePolicy_RetentionEventType" : "RM_FS_ClassCodePolicy_RetentionFlatType" });
                    if (retentionType == RetentionScheduleType.Event)
                    {
                        var ticks = classCodeInfo.StartDate.Ticks;
                        var newValue = ticks != 0
                            ? (await GeneralSettingService.ConvertTiksToDateTimeAsync(ticks, true)).SimplifyFormatTime
                            : string.Empty;
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_FS_ClassCodePolicy_StartDate", NewValue = newValue });
                    }
                    var effectScopeValue = classCodeInfo.ApplyExistDocument ? "RM_FS_ClassCodePolicy_ApplyAllNodes" : "RM_FS_ClassCodePolicy_ApplySelectedNode";
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_FS_Export_EffectScopeColumn", NewValue = effectScopeValue });
                }
                #endregion
            }
            else if (action == (int)AuditAction.PermissionChange)
            {
                RMConnectionRecordOwnerUpdateModel updateModels = (RMConnectionRecordOwnerUpdateModel)args[0];
                var dbOwners = RMFSConnectionAndOwnerRelationshipDao.GetOwnersByConnectionId(updateModels.ConnectionId);
                var userIds = dbOwners.Select(r => r.UserIntId).Distinct().ToList();
                var dbOwnersId = AccountDao.GetUserByIdsAsync(userIds).Result;

                var dbRecordOwners = dbOwners.Where(r => r.Type == FSConnectionOwnerType.RecordOwner).Join(dbOwnersId, rel => rel.UserIntId, owner => owner.Id, (rel, owner) => owner.DisplayName).OrderBy(name => name).ToList();
                var dtoRecordOwners = updateModels.RecordOwners != null ? updateModels.RecordOwners.Select(x => x.DisplayName).OrderBy(name => name).ToList() : new List<string>();

                string oldRecordOwnersStr = string.Join("; ", dbRecordOwners);
                string newRecordOwnersStr = string.Join("; ", dtoRecordOwners);

                if (oldRecordOwnersStr != newRecordOwnersStr)
                {
                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_FS_Register_Records_Owner",
                        OldValue = oldRecordOwnersStr,
                        NewValue = oldRecordOwnersStr,
                    });
                }
            }
            else if (action == (int)AuditAction.GenerateRCCReport)
            {
                var isMyhub = JsonConvert.DeserializeObject<RCCReportRequest>(args[2].ToString()).IsMyHub;
                if (isMyhub)
                {
                    info.Category = AuditCategory.FSMyhub;
                }
            }
            else if (action == (int)AuditAction.DeleteRCCReport)
            {
                var reportType = (int)args[1];

                if (reportType == (int)MyhubReportJobType.HistoryContent)
                {
                    info.Action = AuditAction.DeleteHistoryReport;
                }
                var displayName = GetMyhubReportDisplayName(new RMMyhubReportQueryInfo
                {
                    Ids = (List<Guid>)args[0],
                    ReportType = (int)args[1]
                });
                info.Object = displayName;
            }
            else if (action == (int)AuditAction.DownloadRCCReport)
            {
                var queryInfo = (RMMyhubReportQueryInfo)args[0];
                var reportType = queryInfo.ReportType;

                if (reportType == (int)MyhubReportJobType.HistoryContent)
                {
                    info.Action = AuditAction.DownloadHistoryReport;
                }
                var displayName = GetMyhubReportDisplayName(queryInfo);
                info.Object = displayName;
            }

            return info;
        }
        private int GetMultiGeoAction(object[] args, int action)
        {
            if (args[0] is ConnectionGroupDto dto)
            {
                return dto.MultiGeoOperation switch
                {
                    MultiGeoOperation.MultiGeoCreateFSGroup => (int)AuditAction.CreateFSGroup,
                    MultiGeoOperation.MultiGeoEditFSGroup => (int)AuditAction.EditFSGroup,
                    MultiGeoOperation.None => action,
                    _ => action,
                };
            }
            if(args[0] is ConnectionDto connGeoDto)
            {
                return connGeoDto.MultiGeoOperation switch
                {
                    MultiGeoOperation.MultiGeoCreateFSConnection => (int)AuditAction.CreateFSConnection,
                    MultiGeoOperation.MultiGeoEditFSConnection => (int)AuditAction.EditFSConnection,
                    MultiGeoOperation.None => action,
                    _ => action,
                };
            }
            return action;
        }

        private string GetAuditItemValue(int level) => level switch
        {
            (int)NodeLevel.FSFolder => "RM_RC_Audit_FSClassificationSetting_FolderLevel",
            (int)NodeLevel.FSFile => "RM_RC_Audit_FSClassificationSetting_FileLevel",
            _ => string.Empty
        };

        private string YesOrNoString(bool boolValue)
        {
            return boolValue ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
        }
        private string GetApplyExistString(bool oldApplyExistDocument, int applyExistType)
        {
            if (oldApplyExistDocument)
            {
                if ((ApplyExistingTermType)applyExistType == ApplyExistingTermType.OverWrite)
                {
                    return "RM_JS_Common_Yes" + " " + "RM_JS_SPS_AutoClassification_ApplyOverwirteTerm ";
                }
                else if ((ApplyExistingTermType)applyExistType == ApplyExistingTermType.SkipAndKeep)
                {
                    return "RM_JS_Common_Yes" + " " + "RM_JS_SPS_AutoClassification_ApplySkipTerm ";
                }
                else
                {
                    return "RM_JS_Common_Yes";
                }
            }
            else
            {
                return "RM_JS_Common_No";
            }
        }
        private string GetMyhubReportDisplayName(RMMyhubReportQueryInfo queryInfo)
        {
            var auditItems = new List<RMMyhubReportAuditItem>();
            List<int> finalJobStatus = new List<int>()
                {
                    (int)DownloadContentJobStatus.None,
                    (int)DownloadContentJobStatus.Calculating,
                    (int)DownloadContentJobStatus.Failed,
                    (int)DownloadContentJobStatus.Finished,
                    (int)DownloadContentJobStatus.FinishWithException,
                    (int)DownloadContentJobStatus.Skipped,
                    (int)DownloadContentJobStatus.Stopped,
                    (int)DownloadContentJobStatus.Stopping
                };
            var contentInfoList = DownloadDataInfoDao.GetDownloadDataInfos(queryInfo.Ids, finalJobStatus);

            foreach (var contentInfo in contentInfoList)
            {
                if (queryInfo.ReportType == (int)MyhubReportJobType.HistoryContent)
                {
                    var historyInfo = JsonConvert.DeserializeObject<ManualApprovalHistoryOption>(contentInfo.ExtendString1 ?? string.Empty) ?? new ManualApprovalHistoryOption();
                    if (historyInfo != null)
                    {
                        return historyInfo.DisplayName;
                    }
                }
                else if (queryInfo.ReportType == (int)MyhubReportJobType.DownloadRCCReport)
                {
                    var rccInfos = JsonConvert.DeserializeObject<List<RCCReportContentDto>>(contentInfo.ExtendString1 ?? string.Empty) ?? new List<RCCReportContentDto>();
                    if (rccInfos != null && rccInfos.Count > 0)
                    {
                        return rccInfos[0].DisplayName;
                    }
                }
            }
            return string.Empty;
        }

        private  bool CheckSkipAuditFSGroupMultiGeo(ConnectionGroupDto dto)
        {
            string mainDCInternalName = MultiGeoDataCenterService.GetMainDC();
            string currentDCName = RMSSOHelper.CurrentDCName;
            if(mainDCInternalName == null || currentDCName == null)
            {
                return false;
            }
            var rs = dto.DataCenterType switch
            {
                DataCenterType.DefaultDC => !string.Equals(currentDCName, mainDCInternalName, StringComparison.OrdinalIgnoreCase),
                DataCenterType.SpecificDC => !string.Equals(currentDCName, mainDCInternalName, StringComparison.OrdinalIgnoreCase) 
                                            && !string.Equals(currentDCName, dto.DCInternalName, StringComparison.OrdinalIgnoreCase),
                _ => true // skip
            };
            return rs;
        }

        private bool CheckSkipAuditDeleteGroupMultiGeo(List<FSConnectionGroup> fsGroups, RMAuditInfo info)
        {
            string mainDCInternalName = MultiGeoDataCenterService.GetMainDC();
            string currentDCName = RMSSOHelper.CurrentDCName;
            var groupNames = new List<string>();
            if (mainDCInternalName == null || currentDCName == null)
            {
                return false;
            }
            if(string.Equals(currentDCName, mainDCInternalName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            foreach (var group in fsGroups)
            {
                if (!string.IsNullOrEmpty(group.DCInternalName) && string.Equals(group.DCInternalName, currentDCName, StringComparison.OrdinalIgnoreCase))
                {
                    groupNames.Add(group.Name);
                }
            }
            if (groupNames.Any())
            {
                info.Object = string.Join(";", groupNames);
                return false;
            }
            return true;
        }

        private async Task<bool> CheckSkipAuditConnectionMultiGeo(ConnectionDto dto, List<FSConnection> connections, AuditAction action, RMAuditInfo info)
        {
            string mainDCInternalName = MultiGeoDataCenterService.GetMainDC();
            string currentDCName = RMSSOHelper.CurrentDCName;
            var listConnectionNames = new List<string>();

            if (mainDCInternalName == null || currentDCName == null)
            {
                return false;
            }
            if (string.Equals(currentDCName, mainDCInternalName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if(action == AuditAction.EditFSConnection)
            {
                if(dto == null) return true;
                string selectedDCName = await FSConnectionGroupDao.GetGroupDCInternalNameByConnectionId(dto.Id);
                if (!string.IsNullOrEmpty(selectedDCName) && string.Equals(selectedDCName, currentDCName, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            if(action == AuditAction.DeleteFSConnection)
            {
                if(!connections.Any()) return true;
                foreach (var condto in connections)
                {
                    string DCInternalName = await FSConnectionGroupDao.GetGroupDCInternalNameByConnectionId(condto.Id);
                    if (!string.IsNullOrEmpty(DCInternalName) && string.Equals(DCInternalName, currentDCName, StringComparison.OrdinalIgnoreCase))
                    {
                        listConnectionNames.Add(condto.Name);
                    }
                }
                if (listConnectionNames.Any())
                {
                    info.Object = string.Join(";", listConnectionNames);
                    return false;
                }
            }
            return true;
        }
    }
}
