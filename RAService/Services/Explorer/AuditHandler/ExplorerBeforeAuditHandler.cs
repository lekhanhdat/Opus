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
//using AvePoint.RA.RAPhysical.Disposal.PhysicalDisposalActionImps;
//using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RAPhysical.API;
using Google.Apis.Vault.v1.Data;
using Newtonsoft.Json;
using RAGoogle.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AvePoint.RA.Service.Services.Explorer.AuditHandler
{
    public class ExplorerBeforeAuditHandler : IBeforeAuditHandler
    {

        public IRMManagedRecordRelatedDao ManagedRecordRelatedDao => PlatformWindsorManager.GetService<IRMManagedRecordRelatedDao>();
        public IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        public IHoldDao HoldDao => PlatformWindsorManager.GetService<IHoldDao>();
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

        public IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();
        public ILocationManagementService LocationManagementService => PlatformWindsorManager.GetService<ILocationManagementService>();
        public IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        public IPermissionManagementService PermissionManagementService => PlatformWindsorManager.GetService<IPermissionManagementService>();
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private RALogger logger = RALogger.GetInstance(typeof(ExplorerBeforeAuditHandler));

        private IAccountWrapperService AccountWrapperService => PlatformWindsorManager.GetService<IAccountWrapperService>();

        private List<string> GetAllHoldIdsFromRecord(Record rec)
        {
            // Primary source: HoldUntilTimes contains all hold IDs reliably
            if (!string.IsNullOrEmpty(rec.HoldUntilTimes))
            {
                var holdUntilTimes = JsonConvert.DeserializeObject<List<HoldUntilTime>>(rec.HoldUntilTimes);
                if (holdUntilTimes != null && holdUntilTimes.Count > 0)
                {
                    return holdUntilTimes.Select(h => h.HoldId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
                }
            }

            // Fallback: HoldId + AppendHolds_Array
            List<string> allHoldIds = new List<string>();
            if (!string.IsNullOrEmpty(rec.HoldId))
            {
                allHoldIds.Add(rec.HoldId);
            }
            if (rec.AppendHolds_Array != null)
            {
                allHoldIds.AddRange(rec.AppendHolds_Array);
            }
            return allHoldIds.Distinct().ToList();
        }

        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            var info = new RMAuditInfo();
            try
            {
                info.Module = (AuditModule)model;
                info.Category = (AuditCategory)category;
                info.Action = (AuditAction)action;
                info.ModifyContent = new List<AuditItem>();
                string objName = string.Empty;
                if (info.Action == AuditAction.ChangeTerm)
                {
                    var changeOpt = args[0] as ChangeTermOption;
                    List<Guid> recIds = new List<Guid>();
                    if (!changeOpt.SourceRecordIds.IsNullOrEmpty())
                    {
                        recIds.AddRange(changeOpt.SourceRecordIds);
                    }
                    if (!changeOpt.SourceEXORecordIds.IsNullOrEmpty())
                    {
                        recIds.AddRange(changeOpt.SourceEXORecordIds);
                    }
                    if (!changeOpt.SourceFSRecordIds.IsNullOrEmpty())
                    {
                        recIds.AddRange(changeOpt.SourceFSRecordIds);
                    }
                    if (!changeOpt.SourceSPOnPremRecordIds.IsNullOrEmpty())
                    {
                        recIds.AddRange(changeOpt.SourceSPOnPremRecordIds);
                    }
                    if (!changeOpt.SourcePhyRecordIds.IsNullOrEmpty())
                    {
                        info.Module = AuditModule.PhysicalRecordManagement;
                        info.Category = AuditCategory.PhysicalRecordsExplorer;
                        recIds.AddRange(changeOpt.SourcePhyRecordIds);
                    }
                    if (!changeOpt.SourceOneDriveRecordIds.IsNullOrEmpty())
                    {
                        recIds.AddRange(changeOpt.SourceOneDriveRecordIds);
                    }

                    if (!changeOpt.SourceAzureFileShareRecordIds.IsNullOrEmpty())
                    {
                        recIds.AddRange(changeOpt.SourceAzureFileShareRecordIds);
                    }

                    if (!changeOpt.SourceCustomizeConnectorRecordIds.IsNullOrEmpty())
                    {
                        recIds.AddRange(changeOpt.SourceCustomizeConnectorRecordIds);
                    }
                    if (!changeOpt.GoogleDriveRecordIds.IsNullOrEmpty())
                    {
                        recIds.AddRange(changeOpt.GoogleDriveRecordIds);
                    }
                    if (!changeOpt.SourceTeamsRecordIds.IsNullOrEmpty())
                    {
                        recIds.AddRange(changeOpt.SourceTeamsRecordIds);
                    }
                    if (!changeOpt.SourceBoxRecordIds.IsNullOrEmpty())
                    {
                        recIds.AddRange(changeOpt.SourceBoxRecordIds);
                    }


                    var records = ExplorerDao.GetRecordByIds(recIds);

                    foreach (var rec in records)
                    {
                        if (rec.SourceFlag == (int)SourceFlag.Google)
                        {
                            var groupedByOldTerm = records.GroupBy(r => r.TermName);
                            List<string> oldTerm = [];
                            var recId = groupedByOldTerm.ToList()[0].First().Id;

                            foreach (var group in groupedByOldTerm)
                            {
                                oldTerm.Add(group.Key);
                            }
                            info.ModifyContent.Add(new AuditItem()
                            {
                                Id = recId,
                                TargetSetting = "RM_JS_RC_RUR_LabelName",
                                OldValue = string.Join(", ", oldTerm),
                            });
                            objName += rec.LeafName + ";";
                            break;
                        }
                        var dbTermName = rec.TermName;
                        if (rec.SourceFlag == (int)SourceFlag.Physical)
                        {
                            var meta = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(rec.MetaInfo);
                            var termField = meta?.Where(f => f.Key.Equals(DefaultColumnIDs.Classification, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                            var termJSONString = termField.HasValue ? termField.Value.Value : "";
                            if (!string.IsNullOrEmpty(termJSONString))
                            {
                                dbTermName = Newtonsoft.Json.JsonConvert.DeserializeObject<TaxonomyColumnValue>(termJSONString).Name;
                            }
                        }
                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = rec.Id,
                            TargetSetting = "RM_JS_RC_RUR_TermName",
                            OldValue = dbTermName,
                        });

                        objName += rec.LeafName + ";";
                    }
                    info.ModifyContent.Add(new AuditItem()
                    {
                        Id = Guid.NewGuid(),
                        TargetSetting = "RM_JS_RC_RUR_TermName",
                        NewValue = changeOpt.TargetTermName
                    });
                    info.ModifyContent.Add(new AuditItem()
                    {
                        Id = Guid.NewGuid(),
                        TargetSetting = "RM_JS_RC_RUR_ReclassifyComment",
                        NewValue = changeOpt.Comment
                    });
                    info.Object = objName.TrimEnd(';');

                    AddActionAuditForMyHub(info, changeOpt, Cloud.Sdk.Data.MyHub.AuditActionType.OpusReclassify);
                }
                else if (info.Action == AuditAction.ManageRelatedRecords)
                {
                    Guid currentId = (Guid)args[0];
                    List<Guid> addIds = args[1] as List<Guid>;
                    var idNameDict = args[3] as Dictionary<Guid, string>;

                    var currRecord = ExplorerDao.GetRecordByIds(new List<Guid>() { currentId }).FirstOrDefault();
                    if (currRecord != null)
                    {
                        var oldRelated = string.Empty;
                        if (!string.IsNullOrEmpty(currRecord.RelatedRecords))
                        {
                            List<RMRelatedItemInfo> infos = SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(currRecord.RelatedRecords);
                            if (infos != null && infos.Count > 0)
                            {
                                var oldRelatedIds = infos.Select(r => r.id).ToList();
                                oldRelated = string.Join(",", ExplorerDao.QueryAll(r => oldRelatedIds.Contains(r.NodeId)).Select(s => s.LeafName));
                            }
                        }
                        else
                        {
                            oldRelated = "";
                        }

                        info.Object = currRecord.LeafName;
                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = currentId,
                            TargetSetting = "RM_JS_MA_Grid_RelatedRecords",
                            OldValue = oldRelated,
                            NewValue = string.Join(",", ExplorerDao.GetRecordByIds(addIds).Select(s => s.LeafName))
                        });
                    }
                }
                else if (info.Action == AuditAction.FSManageRelatedRecords)
                {
                    Guid currentId = (Guid)args[0];
                    List<Guid> addIds = args[1] as List<Guid>;
                    var oldRelated = string.Empty;

                    var currRecord = ExplorerDao.GetRecordByIds(new List<Guid>() { currentId }).First();
                    var originalRelateds = ManagedRecordRelatedDao.GetRelatedRecords(currentId).ToList();
                    //TO DO (fpwang)
                    if (originalRelateds.Count > 0)
                    {
                        // oldRelated = string.Join(",", originalRelateds.Select(s => s.LeafName));
                    }
                    info.Object = currRecord.LeafName;
                    info.ModifyContent.Add(new AuditItem()
                    {
                        Id = currentId,
                        TargetSetting = "RM_JS_MA_Grid_RelatedRecords",
                        OldValue = oldRelated,
                        NewValue = string.Join(",", ExplorerDao.GetRecordByIds(addIds).Select(s => s.LeafName))
                    });
                }
                else if (info.Action == AuditAction.DeclareAsRecord || info.Action == AuditAction.UndeclareAsRecord || info.Action == AuditAction.DeclareSPOOnPreAsRecord || info.Action == AuditAction.UndeclareSPOOnPreAsRecord)
                {
                    var recIds = args[0] as List<Guid>;
                    var records = ExplorerDao.GetRecordByIds(recIds);
                    bool isDeclared = info.Action == AuditAction.DeclareAsRecord || info.Action == AuditAction.DeclareSPOOnPreAsRecord;
                    foreach (var rec in records)
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = rec.Id,
                            TargetSetting = isDeclared ? I18NEntity.GetString("Declare As Record") : "RM_BCM_History_UndeclareAsRecord",
                            OldValue = "",
                            NewValue = ""
                        });
                        objName += rec.LeafName + ";";
                    }
                    info.Object = objName.TrimEnd(';');
                }
                else if (info.Action == AuditAction.CancelHoldByRecords || info.Action == AuditAction.RemovePersonalHold)
                {
                    List<Guid> recordIds = args[0] as List<Guid>;
                    List<string> removeHoldIds = args.Length > 2 ? args[2] as List<string> : null;
                    var records = ExplorerDao.GetRecordByIds(recordIds);
                    foreach (var rec in records)
                    {
                        if (rec.SourceFlag == (int)SourceFlag.Physical)
                        {
                            info.Module = AuditModule.PhysicalRecordManagement;
                            info.Category = AuditCategory.PhysicalRecordsExplorer;
                        }

                        string holdNames = string.Empty;
                        if (removeHoldIds != null && removeHoldIds.Count > 0)
                        {
                            List<RMHold> holds = HoldDao.GetHoldByIds(removeHoldIds);
                            if (!holds.IsNullOrEmpty())
                            {
                                holdNames = string.Join(", ", holds.Select(h => h.Name));
                            }
                        }
                        else
                        {
                            List<string> allHoldIds = GetAllHoldIdsFromRecord(rec);
                            if (allHoldIds.Count > 0)
                            {
                                List<RMHold> holds = HoldDao.GetHoldByIds(allHoldIds);
                                if (!holds.IsNullOrEmpty())
                                {
                                    holdNames = string.Join(", ", holds.Select(h => h.Name));
                                }
                            }
                        }

                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = rec.Id,
                            TargetSetting = "RM_BCM_Audit_Action_CancelHoldByRecords",
                            OldValue = "",
                            NewValue = holdNames
                        });
                        objName += rec.LeafName + ";";
                    }
                    info.Object = objName.TrimEnd(';');
                }
                else if (info.Action == AuditAction.MobileReturn)
                {
                    List<Guid> recordIds = args[0] as List<Guid>;
                    var records = ExplorerDao.GetRecordByIds(recordIds);
                    foreach (var rec in records)
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = rec.Id,
                            TargetSetting = "RM_BCM_Audit_Action_CancelHoldByRecords",
                            OldValue = "",
                            NewValue = ""
                        });
                        objName += rec.LeafName + ";";
                    }
                    info.Object = objName.TrimEnd(';');
                }
                else if (info.Action == AuditAction.SusPendRecords)
                {
                    UpdateHoldDto dto = args[0] as UpdateHoldDto;
                    if (ExplorerService.IsPhysicalRecord(dto.ReletedIds.FirstOrDefault()))
                    {
                        info.Module = AuditModule.PhysicalRecordManagement;
                        info.Category = AuditCategory.PhysicalRecordsExplorer;
                    }
                    List<Guid> recordIds = dto.ReletedIds;
                    var records = ExplorerDao.GetRecordByIds(recordIds);

                    string extendDescription = "";
                    if (dto.HoldSetting != null)
                    {
                        var unit = dto.HoldSetting.Unit.ToString();
                        extendDescription = $"{dto.HoldSetting.Number} {unit}";
                    }

                    foreach (var rec in records)
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = rec.Id,
                            TargetSetting = "RM_BCM_Audit_Action_SusPendRecords",
                            OldValue = "",
                            NewValue = extendDescription
                        });
                        objName += rec.LeafName + ";";
                    }
                    info.Object = objName.TrimEnd(';');
                }
                else if (info.Action == AuditAction.CreateHoldTypeWithRecord || info.Action == AuditAction.ReuseHoldTypeWithRecord)
                {
                    UpdateHoldDto dto = args[0] as UpdateHoldDto;
                    if (ExplorerService.IsPhysicalRecord(dto.ReletedIds.FirstOrDefault()))
                    {
                        info.Module = AuditModule.PhysicalRecordManagement;
                        info.Category = AuditCategory.PhysicalRecordsExplorer;
                    }
                    if (dto.HoldAction == RecordsConstants.HOLD_ACTION_APPEND)
                    {
                        if (info.Action == AuditAction.CreateHoldTypeWithRecord)
                        {
                            info.Action = AuditAction.CreateAppendHoldTypeWithRecord;
                        }
                        if (info.Action == AuditAction.ReuseHoldTypeWithRecord)
                        {
                            info.Action = AuditAction.ReuseAppendHoldTypeWithRecord;
                        }
                    }
                    List<Guid> reletedIds = dto.ReletedIds;
                    var records = ExplorerDao.GetRecordByIds(reletedIds);
                    foreach (var rec in records)
                    {
                        objName += rec.LeafName + ";";
                    }
                    var targetSetting = "";
                    switch (info.Action)
                    {
                        case AuditAction.CreateHoldTypeWithRecord:
                            targetSetting = "RM_BCM_Audit_Action_CreateHoldTypeWithRecord";
                            break;
                        case AuditAction.ReuseHoldTypeWithRecord:
                            targetSetting = "RM_BCM_Audit_Action_ReuseHoldTypeWithRecord";
                            break;
                        case AuditAction.CreateAppendHoldTypeWithRecord:
                            targetSetting = "RM_BCM_Audit_Action_CreateAppendHoldTypeWithRecord";
                            break;
                        case AuditAction.ReuseAppendHoldTypeWithRecord:
                            targetSetting = "RM_BCM_Audit_Action_ReuseAppendHoldTypeWithRecord";
                            break;
                    }
                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = targetSetting,
                        OldValue = "",
                        NewValue = dto.HoldSetting.Name
                    });
                    info.Object = objName.TrimEnd(';');
                }
                else if (info.Action == AuditAction.ChangeHoldCreate || info.Action == AuditAction.ChangeHoldReuse)
                {
                    UpdateHoldDto dto = args[0] as UpdateHoldDto;
                    List<Guid> reletedIds = dto.ReletedIds;
                    if (ExplorerService.IsPhysicalRecord(dto.ReletedIds.FirstOrDefault()))
                    {
                        info.Module = AuditModule.PhysicalRecordManagement;
                        info.Category = AuditCategory.PhysicalRecordsExplorer;
                    }
                    var records = ExplorerDao.GetRecordByIds(reletedIds);
                    foreach (var rec in records)
                    {
                        string oldHoldNames = string.Empty;
                        if (rec != null)
                        {
                            List<string> allHoldIds = GetAllHoldIdsFromRecord(rec);
                            if (allHoldIds.Count > 0)
                            {
                                List<RMHold> holds = HoldDao.GetHoldByIds(allHoldIds);
                                if (!holds.IsNullOrEmpty())
                                {
                                    oldHoldNames = string.Join(", ", holds.Select(h => h.Name));
                                }
                            }
                            info.ModifyContent.Add(new AuditItem()
                            {
                                Id = rec.Id,
                                TargetSetting = "RM_JS_RDM_Hold_HoldName",
                                OldValue = oldHoldNames,
                                NewValue = dto.HoldSetting.Name
                            });
                            objName += rec.LeafName + ";";
                        }
                    }
                    info.Object = objName.TrimEnd(';');
                }
                else if (info.Action == AuditAction.CreateHold)
                {
                    UpdateHoldDto dto = args[0] as UpdateHoldDto;
                    //if (dto.HoldCategory == RecordsConstants.RecordHold_PhyProfile)
                    //{
                    //    info.Module = AuditModule.PhysicalRecordManagement;
                    //    info.Category = AuditCategory.PhysicalRecordsExplorer;
                    //}
                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_BCM_Audit_Action_CreateHold",
                        OldValue = "",
                        NewValue = ""
                    });
                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_JS_BCM_Explorer_Details_HoldManager",
                        OldValue = "",
                        NewValue = string.Join(", ", dto.HoldSetting.HoldUserManagers?.Select(u => u.DisplayName ?? u.UserPrincipalName))
                    });
                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_JS_BCM_Explorer_Enable_SendEmailTo_HoldManager",
                        OldValue = "",
                        NewValue = dto.HoldSetting.IsHoldManagerEmailNotificationEnabled ? "RM_JS_Common_Enabled" : "RM_JS_Common_Disabled"
                    });
                    objName += dto.HoldSetting.Name;
                    info.Object = objName;
                }
                else if (info.Action == AuditAction.EditHold)
                {
                    var gls = await GeneralSettingService.GetGeneralSettingAsync();
                    UpdateHoldDto dto = args[0] as UpdateHoldDto;
                    //if (dto.HoldCategory == RecordsConstants.RecordHold_PhyProfile)
                    //{
                    //    info.Module = AuditModule.PhysicalRecordManagement;
                    //    info.Category = AuditCategory.PhysicalRecordsExplorer;
                    //}
                    List<string> HoldId = new List<string>();
                    HoldId.Add(dto.HoldSetting.Id);
                    var holds = HoldDao.GetHoldByIds(HoldId);

                    AuditItem dateItem = new AuditItem();
                    dateItem.TargetSetting = "RM_JS_BCM_Explorer_Details_HoldUntil";
                    if (holds[0].HoldDateType == 0)
                    {
                        dateItem.OldValue = I18NEntity.GetString("RM_BCM_Audit_Action_EnterDuration") + " ";
                        dateItem.OldValue += holds[0].Number.ToString() + " ";
                        dateItem.OldValue += ((HoldDateUnit)holds[0].HoldUnit).ToString();
                    }
                    else
                    {
                        dateItem.OldValue = I18NEntity.GetString("RM_BCM_Audit_Action_Calender") + " ";
                        DateTime calendarTime = new DateTime(holds[0].CalendarTime);
                        calendarTime = DateTime.SpecifyKind(calendarTime, DateTimeKind.Unspecified);
                        calendarTime = DateTimeUtil.ConvertTimeFromUtc(calendarTime.Ticks, gls);
                        string time = calendarTime.ToString(JSDateTimeFormat.DEFAULT_TIME_FORMAT);
                        dateItem.OldValue += time;
                    }
                    info.ModifyContent.Add(dateItem);

                    AuditItem commentItem = new AuditItem();
                    commentItem.TargetSetting = I18NEntity.GetString("RM_JM_Comment");
                    commentItem.OldValue = holds[0].Description;
                    info.ModifyContent.Add(commentItem);

                    objName += dto.HoldSetting.Name;
                    info.Object = objName;
                    AuditItem emailNotification = new AuditItem();
                    emailNotification.TargetSetting = I18NEntity.GetString("RM_MA_Setting_Email_Notification");
                    emailNotification.OldValue = holds[0].HoldDateType == (int)HoldDateType.Custom
                        ? null
                        : ExplorerAuditUtil.GetEmailNotificationInfo(new HoldEmailNotification
                        {
                            IsEnabled = holds[0].IsEmailNotificationEnabled,
                            ReminderDurationDays = holds[0].ReminderDurationDays,
                        });
                    info.ModifyContent.Add(emailNotification);

                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_JS_BCM_Explorer_Enable_SendEmailTo_HoldManager",
                        OldValue = "",
                        NewValue = holds[0].IsHoldManagerEmailNotificationEnabled ? "RM_JS_Common_Enabled" : "RM_JS_Common_Disabled"
                    });
                    Dictionary<string, List<ToUserInfo>> holdUserDic = await HoldDao.GetUsersManageHold(holds.Select(a => a.Id).ToList());
                    holdUserDic.TryGetValue(holds[0].Id, out var managerHolds);

                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_JS_BCM_Explorer_Details_HoldManager",
                        OldValue = string.Join(", ", managerHolds?.Select(u => u.DisplayName ?? u.UserPrincipalName)),
                        NewValue = string.Join(", ", dto.HoldSetting.HoldUserManagers?.Select(u => u.DisplayName ?? u.UserPrincipalName))
                    });
                }
                else if (info.Action == AuditAction.CancelHold || info.Action == AuditAction.DeleteHold)
                {
                    List<string> holdIds = args[0] as List<string>;
                    var holds = HoldDao.GetHoldByIds(holdIds);
                    foreach (var hold in holds)
                    {
                        if (hold.Type == (int)HoldProfileType.Physical)
                        {
                            info.Module = AuditModule.PhysicalRecordManagement;
                            info.Category = AuditCategory.PhysicalRecordsExplorer;
                        }
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = info.Action == AuditAction.CancelHold ? "RM_BCM_Audit_Action_CancelHold" : I18NEntity.GetString("RM_BCM_Audit_Action_DeleteHold"),
                            OldValue = "",
                            NewValue = ""
                        });
                        objName += hold.Name + ";";
                    }
                    info.Object = objName.TrimEnd(';');
                }
                else if (info.Action == AuditAction.SuspendHold)
                {
                    UpdateHoldDto dto = args[0] as UpdateHoldDto;
                    //if (dto.HoldCategory == RecordsConstants.RecordHold_PhyProfile)
                    //{
                    //    info.Module = AuditModule.PhysicalRecordManagement;
                    //    info.Category = AuditCategory.PhysicalRecordsExplorer;
                    //}
                    List<string> holdIds = dto.HoldIds;
                    var holds = HoldDao.GetHoldByIds(holdIds);
                    foreach (var hold in holds)
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_BCM_Audit_Action_SuspendHold",
                            OldValue = "",
                            NewValue = ""
                        });
                        objName += hold.Name + ";";
                    }
                    info.Object = objName.TrimEnd(';');
                }
                else if (info.Action == AuditAction.MoveCheckSPUrl || info.Action == AuditAction.RuleCheckSPUrl)
                {
                    string locationPath = args[0] as string;
                    RMAccountProfileDto account = args[1] as RMAccountProfileDto;
                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_BCM_Audit_CheckSPUrl",
                        NewValue = locationPath
                    });
                }
                else if (info.Action == AuditAction.MoveCheckFSUNCLocation || info.Action == AuditAction.RuleCheckFSUNCLocation)
                {
                    string locationPath = args[0] as string;
                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_BCM_Audit_CheckFSUNCPath",
                        NewValue = locationPath
                    });
                }
                else if (info.Action == AuditAction.PhysicalExplorerMove || info.Action == AuditAction.MobileMove)
                {
                    var phyMoveOpt = args[0] as PhysicalMoveOption;
                    if (phyMoveOpt.FromModule == (int)AuditCategory.PhysicalRecordsGlobalSearch)
                    {
                        info.Category = AuditCategory.PhysicalRecordsGlobalSearch;
                    }
                    if (phyMoveOpt.FromModule == (int)AuditCategory.PhysicalExplorerMoveRequest)
                    {
                        info.Category = AuditCategory.PhyscialRequestManagement;
                    }

                    #region original value
                    var sourceRecords = ExplorerDao.QueryAll(r => phyMoveOpt.SourcePhyRecordIds.Contains(r.Id));
                    var record = sourceRecords.FirstOrDefault();
                    var oldlocation = new PhysicalLocation(record.LocationId);
                    var oldDirPath = oldlocation.DirPath;
                    if (oldDirPath.IndexOf('/') < 0)
                    {
                        var rootLocation = I18NEntity.GetString("RM_SPS_Location_RootNode");
                        oldDirPath = rootLocation + "/" + oldlocation.Name;
                    }
                    oldDirPath = oldDirPath.Replace("RM_SPS_Location_RootNode", I18NEntity.GetString("RM_SPS_Location_RootNode"));
                    if (record.BoxId != Guid.Empty)
                    {
                        var oldBox = ExplorerDao.GetPhysicalRecordById(record.BoxId);
                        if (oldBox != null)
                        {
                            oldDirPath += '/' + oldBox.LeafName;
                        }
                    }
                    if (record.FileId != Guid.Empty)
                    {
                        var folder = ExplorerDao.GetPhysicalRecordById(record.FileId);
                        oldDirPath += '/' + folder.LeafName;
                    }

                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_BCM_Audit_OriginalLocationMove",
                        OldValue = oldDirPath
                    });

                    #endregion orignal value

                    #region new value

                    var location = new PhysicalLocation(new Guid(phyMoveOpt.LocationId));
                    var dirPath = location.DirPath;
                    if(dirPath.IndexOf('/') < 0)
                    {
                        var rootLocation = I18NEntity.GetString("RM_SPS_Location_RootNode");
                        dirPath = rootLocation + "/" + location.Name;
                    }
                    dirPath = dirPath.Replace("RM_SPS_Location_RootNode", I18NEntity.GetString("RM_SPS_Location_RootNode"));
                    if (!string.IsNullOrEmpty(phyMoveOpt.BoxId))
                    {
                        var box = ExplorerDao.GetPhysicalRecordById(new Guid(phyMoveOpt.BoxId));
                        if (box != null)
                        {
                            dirPath += '/' + box.LeafName;
                        }
                    }
                    if (!string.IsNullOrEmpty(phyMoveOpt.FolderId))
                    {
                        var folder = ExplorerDao.GetPhysicalRecordById(new Guid(phyMoveOpt.FolderId));
                        dirPath += '/' + folder.LeafName;
                    }
                 
                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_BCM_Audit_MoveToDestination",
                        NewValue = dirPath
                    });
                    #endregion new value
                    //info.ModifyContent.Add(new AuditItem()
                    //{
                    //    TargetSetting = "RM_BCM_Audit_NameConflictOption",
                    //    NewValue = GetNameConflictOptionString(phyMoveOpt)
                    //});

                    List<Guid> recIds = new List<Guid>();
                    if (!phyMoveOpt.SourcePhyRecordIds.IsNullOrEmpty())
                    {
                        recIds.AddRange(phyMoveOpt.SourcePhyRecordIds);
                    }
                    var records = ExplorerDao.GetRecordByIds(recIds);
                    foreach (var rec in records)
                    {
                        objName += rec.LeafName + ";";
                    }
                    info.Object = objName.TrimEnd(';');
                }
                else if (info.Action == AuditAction.MobileChangeStatus)
                {
                    var requestDto = args[0] as MobileChangeStatusDto;
                    var recIds = requestDto.RecordIds.Select(r => r.Id).ToList();
                    var records = ExplorerDao.GetRecordByIds(recIds);
                    foreach (var recordInfo in records)
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = recordInfo.Id,
                            TargetSetting = "RM_BCM_Audit_ChangeStatus",
                            OldValue = GetStautsName(recordInfo.RecordStatus),
                            NewValue = GetStautsName((int)requestDto.PhysicalRecordStatus),
                        });
                        objName += recordInfo.LeafName + ";";
                    }
                    info.Object = objName.TrimEnd(';');
                }
                else if (info.Action == AuditAction.SavelocationPermission)
                {
                    ScopePermissionDto param = args[0] as ScopePermissionDto;
                    info.ModifyContent = new List<AuditItem>();
                    await AddPhysicalPermissionAudtisAsync(info, param);
                }
                else if (info.Action == AuditAction.DownloadArchivedContent)
                {
                    bool isMyhub = false;
                    if (args.Length > 1 && args[1] is bool boolVal)
                    {
                        isMyhub = boolVal;
                    }
                    if (isMyhub)
                    {
                        return null;
                    }

                    var ids = args[0] as List<Guid>;
                    var records = ExplorerDao.GetRecordByIds(ids);
                    var names = records.Select(r => r.LeafName).ToList();
                    info.Category = AuditCategory.DownloadCenter;
                    info.Module = AuditModule.DownloadCenter;
                    //var node = ((RMSPTreeNode)args[0]);
                    info.Action = AuditAction.DownloadArchivedContent;
                    var recordsId = records.Select(r => r.Id).ToList();
                    var jobs = DownloadDataInfoDao.GetDownloadDataInfos(ids).Where(job => !recordsId.Contains(job.RecordsId)).Select(job => job.JobId).ToList();
                    names.AddRange(jobs);
                    info.Object = string.Join(",", names);
                }
                else if (info.Action == AuditAction.ChangeLabel)
                {
                    var changeOpt = args[0] as ChangeLabelOption;
                    List<Guid> recIds = new List<Guid>();
                    if (!changeOpt.GoogleDriveRecordIds.IsNullOrEmpty())
                    {
                        recIds.AddRange(changeOpt.GoogleDriveRecordIds);
                    }

                    var records = ExplorerDao.GetRecordByIds(recIds);

                    foreach (var rec in records)
                    {
                        var dbTermName = rec.TermName;
                        info.ModifyContent.Add(new AuditItem()
                        {
                            Id = rec.Id,
                            TargetSetting = "RM_JS_RC_RUR_LabelName",
                            OldValue = dbTermName,
                            NewValue = changeOpt.TargetLabelName,
                        });
                        objName += rec.LeafName + ";";
                    }

                    info.Object = objName.TrimEnd(';');
                }
                else if (info.Action == AuditAction.RunGlobalSearchActionJob)
                {
                    GlobalSearchActionDto globalSearchActionDto = DeserializeFromXml<GlobalSearchActionDto>(args[0]?.ToString());
                    ChangeTermOption changeTermOption = DeserializeFromXml<ChangeTermOption>(globalSearchActionDto.ActionExtension.ToString());
                    var newTerm = changeTermOption.TargetTermName;

                    var records = ExplorerDao.GetRecordByIds(changeTermOption.GoogleDriveRecordIds);
                    var expandedRecords = new List<Record>();
                    foreach (var root in records)
                    {
                        if (root.NodeType == (int)RMNodeLevel.GoogleFolder)
                        {
                            var childFiles = ExplorerDao.GetAllGoogleFilesByBatchBFSAsync(root.ScopeId, root.Id).Result;
                            expandedRecords.AddRange(childFiles);
                        }
                        else
                        {
                            expandedRecords.Add(root);
                        }
                    }

                    var groupedByOldTerm = expandedRecords.GroupBy(r => r.TermName);
                    List<string> oldTerm = [];
                    var recId = groupedByOldTerm.ToList()[0].First().Id;

                    foreach (var group in groupedByOldTerm)
                    {
                        oldTerm.Add(group.Key);
                    }
                    AuditItem auditItem = new AuditItem
                    {
                        Id = recId,
                        TargetSetting = "RM_JS_RC_RUR_LabelName",
                        OldValue = string.Join(", ", oldTerm),
                        NewValue = newTerm
                    };
                    info.ModifyContent.Add(auditItem);
                    AddActionAuditForMyHub(info, changeTermOption, Cloud.Sdk.Data.MyHub.AuditActionType.OpusRunBulkReclassifyJob);
                }
            }
            catch (Exception ex)
            {
                logger.Error("collect explorer audit beforer error:{0}", ex.ToString());
            }
            return info;
        }

        private void AddActionAuditForMyHub(RMAuditInfo info, ChangeTermOption changeTermOption, Cloud.Sdk.Data.MyHub.AuditActionType action)
        {
            try
            {
                if (changeTermOption.ChangeTermOrigin == ChangeTermOrigin.MyHub)
                {
                    logger.Info($"Add {action} to MyHub");
                    var userName = TenantLocalValue.LogonUserEmail ?? (AccountDao.GetUserByUserIdAsync(TenantLocalValue.LogonUserId).GetAwaiter().GetResult())?.UserPrincipalName;
                    var account = AccountWrapperService.GetAccount(TenantLocalValue.LogonGroupId, userName);
                    new MyHubAuditHelper().SendMyHubAduit(info.Object, action, account).GetAwaiter().GetResult();// Need change after update sdk
                    logger.Info($"Add {action} to MyHub success.");
                }
            }
            catch (Exception ex)
            {
                logger.Error("Collect MyHub audit beforer do action error:{0}", ex.ToString());
            }
        }

        private string GetStautsName(int statusInt)
        {
            RMRecordStatus status = (RMRecordStatus)statusInt;
            if (status == RMRecordStatus.Active)
            {
                return "RM_PRM_PRE_Column_Status_Open";
            }
            else if (status == RMRecordStatus.Closed)
            {
                return "RM_PRM_PRE_Column_Status_Closed";
            }
            else if (status == RMRecordStatus.Destroyed)
            {
                return "RM_PRM_PRE_Column_Status_Destroyed";
            }
            else if (status == RMRecordStatus.Missing)
            {
                return "RM_PRM_PRE_Column_Status_Missing";
            }
            return "RM_RC_Audit_None";
        }

        private static string GetNameConflictOptionString(PhysicalMoveOption moveOption)
        {
            return "RM_JS_BCM_Explorer_Move_FileConflictOption_" + moveOption.NameConflictOption.ToString();
        }

        private async System.Threading.Tasks.Task AddPhysicalPermissionAudtisAsync(RMAuditInfo info, ScopePermissionDto dto)
        {
            var newAccountIds = dto.AccountIds;
            foreach (var scopeInfo in dto.ScopeInfos)
            {
                AuditItem item = new AuditItem();
                item.TargetSetting = I18NEntity.GetString(scopeInfo.ScopeNameFullPath).TrimEnd('/');
                var oldAccountIds = PermissionManagementService.GetUserIdsWithPermission(scopeInfo.ScopeId);
                item.OldValue = await GetUserNamesAsync(oldAccountIds);
                item.NewValue = await GetUserNamesAsync(newAccountIds);
                info.ModifyContent.Add(item);
            }
            var namePaths = dto.ScopeInfos.Select(o => o.ScopeNameFullPath.TrimEnd('/')).ToList();
            info.Object = string.Join(";", namePaths);
        }

        private async Task<string> GetUserNamesAsync(List<int> userIds)
        {
            var userNames = "";
            if (userIds.Count > 0)
            {
                var users = await AccountDao.GetUserByIdsAsync(userIds);
                var userNameList = users.Select(o => o.DisplayName).Distinct().ToList();
                userNames = string.Join(";", userNameList);
            }
            return userNames;
        }

        public static T DeserializeFromXml<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return default(T);

            var serializer = new DataContractSerializer(typeof(T));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            using (var xr = XmlReader.Create(ms))
            {
                return (T)serializer.ReadObject(xr);
            }
        }
    }
}
