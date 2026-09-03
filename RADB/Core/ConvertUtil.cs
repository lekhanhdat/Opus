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
using AvePoint.GCommon.Contract.ContentManager.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.TemplateManagement.Barcode;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.DataIngestion;
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Core
{
    public class ConvertUtil
    {
        static RALogger logger = RALogger.GetInstance(typeof(ConvertUtil));

        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        public static BaseRecordDto ConvertToBaseRecordDto(Record dbRecord, Dictionary<int, RMAccount> accountMap = null)
        {
            var result = new BaseRecordDto()
            {
                Id = dbRecord.Id,
                CreateDate = dbRecord.CreateDate,
                RecordsId = dbRecord.RecordsId,
                ScopeId = dbRecord.ScopeId,
                NodeId = dbRecord.NodeId,
                DirPath = dbRecord.DirPath,
                NodeType = dbRecord.NodeType,
                LeafName = dbRecord.LeafName,
                ExtensionForFile = dbRecord.ExtensionForFile,
                RecordStatus = dbRecord.RecordStatus,
                TermId = dbRecord.TermId,
                TermName = dbRecord.TermName,
                RuleId = dbRecord.RuleId,
                RuleLevel = dbRecord.RuleLevel,
                HoldStatus = dbRecord.HoldStatus,
                RelatedRecords = dbRecord.RelatedRecords,
                RelatedRecordsCount = dbRecord.RelatedRecordsCount,
                SourceFlag = dbRecord.SourceFlag,
                CreatedBy = dbRecord.CreatedBy,
                ModifiedBy = dbRecord.ModifiedBy,
                DisposalDueDate = dbRecord.DestroyedTime > 0 ? string.Empty : DueDateUtil.ConvertLongDueDate2String(dbRecord.DisposalDueDate),
                PreviosDisposalDueDate = DueDateUtil.ConvertLongDueDate2String(dbRecord.PreviosDisposalDueDate),
                TimeCreated = dbRecord.TimeCreated,
                TimeLastModified = dbRecord.TimeModified,
                TimeArchived = dbRecord.DestroyedTime,
                CollectionTime = dbRecord.CollectTime,
                AveSiteId = dbRecord.AveSiteId,
                WebId = dbRecord.WebId,
                ListId = dbRecord.ListId,
                BoxId = dbRecord.BoxId,
                FileId = dbRecord.FileId,
                FolderId = dbRecord.FolderId,
                ItemId = dbRecord.ItemId,
                ItemRowId = dbRecord.ItemRowId,
                FullPath = dbRecord.FullPath,
                MetaInfo = dbRecord.MetaInfo,
                CustomColumnDic = dbRecord.CustomColumnDic,
                DeclareAsRecord = dbRecord.DeclareAsRecord,
                LockedByRecordLabel = dbRecord.LockedByRecordLabel,
                HoldBy = ConvertHoldBy(dbRecord, accountMap),
                Ancestors = dbRecord.Ancestors,
                ParentId = dbRecord.ParentId,
                LocationId = dbRecord.LocationId,
                TemplateId = dbRecord.TemplateId,
                EmailAddress = dbRecord.EmailAddress,
                ExternalId = dbRecord.ExternalId,
                ContainerId = dbRecord.ContainerId,
                HoldId = dbRecord.HoldId,
                HoldReleaseTime = dbRecord.HoldReleaseTime,
                AppendHolds_Array = dbRecord.AppendHolds_Array,
                HoldByUsers = ConvertHoldByUsers(dbRecord,accountMap),
                DestryoedTime = dbRecord.DestroyedTime,
                LoanPickStatus = dbRecord.LoanPickStatus,
                DestructionPickStatus = dbRecord.DestructionPickStatus,
                ManualApprovedBy = dbRecord.ManualApprovedBy,
                PredictTermId = dbRecord.PredictTermId,
                PredictTime = dbRecord.PredictTime,
                MLApprovalStatus = dbRecord.MLApprovalStatus
            };
            if(dbRecord.ManualReviewer != null && dbRecord.ManualReviewer.Length > 0)
            {
                try
                {
                    result.RecordOwner = string.Join("; ", dbRecord.ManualReviewer.Select(a => accountMap[a].DisplayName));
                    result.RecordOwnerPrincipalName = string.Join("; ", dbRecord.ManualReviewer.Select(a => accountMap[a].UserPrincipalName));
                }
                catch(Exception e)
                {
                    logger.Warn($"Convert record owner {string.Join(", ", dbRecord.ManualReviewer)} error: {e}");
                }
            }

            if (dbRecord.IsGControlRecord)
            {
                try
                {
                    var gControlReviewer = accountMap.Values.FirstOrDefault(acc => acc.AADId == dbRecord.GControlCurrentApproverId);
                    dbRecord.GControlManualReviewers ??= [];
                    var manualReviewerNames = dbRecord.GControlManualReviewers.Select(reviewerId => accountMap[reviewerId].DisplayName).Union([gControlReviewer?.DisplayName ?? string.Empty]);
                    var manualReviewerPrincipleNames = dbRecord.GControlManualReviewers.Select(reviewerId => accountMap[reviewerId].UserPrincipalName).Union([gControlReviewer?.UserPrincipalName ?? string.Empty]);
                    result.RecordOwner = string.Join("; ", manualReviewerNames.Where(name => !string.IsNullOrEmpty(name)));
                    result.RecordOwnerPrincipalName = string.Join("; ", manualReviewerPrincipleNames.Where(name => !string.IsNullOrEmpty(name)));
                }
                catch(Exception e)
                {
                    logger.Warn($"Convert google record owner {string.Join(", ", dbRecord.GControlManualReviewers ?? [])} or {dbRecord.GControlCurrentApproverId} error: {e}");
                }
            }
            
            //if (!string.IsNullOrEmpty(dbRecord.RecordOwner) && accountMap != null)
            //{
            //    try
            //    {
            //        if (dbRecord.RecordOwner != I18N.Core.I18NEntity.GetString("RM_JS_JM_EndTimePending"))
            //        {
            //            var accountIds = dbRecord.RecordOwner.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            //            result.RecordOwner = string.Join(";", accountIds.Select(a => accountMap[int.Parse(a)].DisplayName));
            //            result.RecordOwnerPrincipalName = string.Join(";", accountIds.Select(a => accountMap[int.Parse(a)].UserPrincipalName));
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        logger.Warn("convert record owner {0} error: {1}", dbRecord.RecordOwner, e.ToString());
            //    }
            //}
            return result;
        }

        private static string ConvertHoldBy(Record dbRecord, Dictionary<int, RMAccount> accountMap)
        {
            string name = null;
            if (!string.IsNullOrEmpty(dbRecord.HoldBy) && accountMap != null)
            {
                name = accountMap.Values.Where(o => dbRecord.HoldBy.Equals(o.UserPrincipalName)).Select(o => o.DisplayName).FirstOrDefault();
            }

            return name ?? dbRecord.HoldBy;
        }
        private static List<HoldUser> ConvertHoldByUsers(Record dbRecord, Dictionary<int, RMAccount> accountMap)
        {
            List<HoldUser> holdByUsers = string.IsNullOrEmpty(dbRecord.HoldByUsers) ? new List<HoldUser>() : JsonConvert.DeserializeObject<List<HoldUser>>(dbRecord.HoldByUsers);
            foreach (var holdBy in holdByUsers)
            {
                if (!string.IsNullOrEmpty(holdBy.HoldBy) && accountMap != null)
                {
                    holdBy.HoldBy = accountMap.Values.Where(o => holdBy.HoldBy.Equals(o.UserPrincipalName)).Select(o => o.DisplayName).FirstOrDefault();
                }
            }
            return holdByUsers;
        }

        public static Record ConvertToRMBaseRecord(BaseRecordDto dbRecord)
        {
            return new Record()
            {
                Id = dbRecord.Id,
                CreateDate = dbRecord.CreateDate,
                RecordsId = dbRecord.RecordsId,
                ScopeId = dbRecord.ScopeId,
                NodeId = dbRecord.NodeId,
                DirPath = dbRecord.DirPath,
                NodeType = dbRecord.NodeType,
                LeafName = dbRecord.LeafName,
                ExtensionForFile = dbRecord.ExtensionForFile,
                TermId = dbRecord.TermId,
                TermName = dbRecord.TermName,
                RuleId = dbRecord.RuleId,
                HoldStatus = dbRecord.HoldStatus,
                RelatedRecords = dbRecord.RelatedRecords,
                RelatedRecordsCount = dbRecord.RelatedRecordsCount,
                SourceFlag = dbRecord.SourceFlag,
                CreatedBy = dbRecord.CreatedBy,
                RecordOwner = dbRecord.RecordOwner,
                DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(dbRecord.DisposalDueDate),
                PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(dbRecord.PreviosDisposalDueDate),
                TimeCreated = dbRecord.TimeCreated,
                TimeModified = dbRecord.TimeLastModified,
                CollectTime = dbRecord.CollectionTime,
                AveSiteId = dbRecord.AveSiteId,
                WebId = dbRecord.WebId,
                ListId = dbRecord.ListId,
                FolderId = dbRecord.FolderId,
                ItemId = dbRecord.ItemId,
                ItemRowId = dbRecord.ItemRowId,
                MetaInfo = dbRecord.MetaInfo,
                HoldId = dbRecord.HoldId,
                EmailAddress = dbRecord.EmailAddress,
                ExternalId = dbRecord.ExternalId,
                ContainerId = dbRecord.ContainerId
            };
        }

        public static RMHold ConvertToRMHold(HoldSetting setting, GeneralSettingModel gls)
        {
            RMHold rmhold = new RMHold();
            rmhold.Id = setting.Id;
            rmhold.Type = (int)setting.ProfileType;
            rmhold.Name = setting.Name;
            rmhold.CreateTime = DateTime.UtcNow.Ticks;
            rmhold.HoldDateType = (int)setting.Type;
            rmhold.Number = setting.Number;
            rmhold.HoldUnit = (int)setting.Unit;
            rmhold.Description = setting.Description;
            //(setting.ProfileType == HoldProfileType.Normal || setting.ProfileType == HoldProfileType.Physical) && 
            if (setting.CalenderTime != null)
            {
                DateTime holdReleaseTime = DateTime.Parse(setting.CalenderTime);
                holdReleaseTime = DateTime.SpecifyKind(holdReleaseTime, DateTimeKind.Unspecified);
                DateTime utcCalendarTime = DateTimeUtil.ConvertTimeToUtcDate(holdReleaseTime, gls);
                rmhold.CalendarTime = utcCalendarTime.Ticks;
                rmhold.TimeZoneId = gls.TimeZoneId;
                rmhold.IsDaylightSaving = gls.DayLight;
            }
            //if (setting.ProfileType == HoldProfileType.Physical && setting.CalenderTime != null)
            //{
            //    rmhold.CalendarTime = setting.CalendarDate.Ticks;
            //    //rmhold.TimeZoneId = TimeZoneInfo.FindSystemTimeZoneById(setting.TimeZoneId).Id;//此方法可验证UI传过来的TimeZoneId是否合法
            //    //rmhold.IsDaylightSaving = setting.IsDayLightSaving;
            //}
            if (setting.EmailNotification != null)
            {
                rmhold.IsEmailNotificationEnabled = setting.EmailNotification.IsEnabled;
                rmhold.ReminderDurationDays = setting.EmailNotification.ReminderDurationDays;
                rmhold.EmailRecipients = setting.EmailNotification.EmailRecipients != null
                    ? JsonConvert.SerializeObject(setting.EmailNotification.EmailRecipients)
                    : null;
            }
            return rmhold;
        }

        public static HoldSetting ConvertToHoldSetting(RMHold rmHold, GeneralSettingModel gls)
        {
            HoldSetting setting = new HoldSetting();
            setting.Id = rmHold.Id;
            setting.ProfileType = (HoldProfileType)rmHold.Type;
            setting.Name = rmHold.Name;
            setting.Type = (HoldDateType)rmHold.HoldDateType;
            setting.Number = rmHold.Number;
            setting.Unit = (HoldDateUnit)rmHold.HoldUnit;
            setting.Description = rmHold.Description;
            if (setting.Type == HoldDateType.Calendar)
            {
                DateTime localCalendarTime = DateTimeUtil.ConvertTimeFromUtc(rmHold.CalendarTime, gls);
                setting.CalenderTime = localCalendarTime.ToString(JSDateTimeFormat.DEFAULT_TIME_FORMAT);
                //if (setting.ProfileType == HoldProfileType.Physical)
                //{
                //    setting.CalendarDate = new DateTime(rmHold.CalendarTime, DateTimeKind.Utc);
                //}
                setting.TimeZoneId = gls.TimeZoneId;
                setting.IsDayLightSaving = gls.DayLight;
            }
            setting.EmailNotification = new HoldEmailNotification
            {
                IsEnabled = rmHold.IsEmailNotificationEnabled,
                ReminderDurationDays = rmHold.ReminderDurationDays,
                EmailRecipients = !string.IsNullOrEmpty(rmHold.EmailRecipients)
                    ? JsonConvert.DeserializeObject<List<AOSUserDto>>(rmHold.EmailRecipients)
                    : new List<AOSUserDto>()
            };
            setting.IsHoldManagerEmailNotificationEnabled = rmHold.IsHoldManagerEmailNotificationEnabled;

            return setting;
        }

        public static List<RMRecordAlliance> ConvertToRMRecordAlliance(List<Guid> ids, string holdSettingId, int type, long releaseTime, string holdBy)
        {
            List<RMRecordAlliance> alliances = new List<RMRecordAlliance>();
            foreach (Guid id in ids)
            {
                RMRecordAlliance temp = new RMRecordAlliance();
                temp.RecordsId = id;
                temp.HoldId = holdSettingId;
                temp.AllianceType = type;
                temp.HoldReleaseTime = releaseTime;
                temp.HoldBy = holdBy;
                alliances.Add(temp);
            }
            return alliances;
        }


        public static RMScope ConvertToRMScope(ScopeDto dto)
        {
            return new RMScope()
            {
                //Id = dto.Id,
                ScopeId = dto.ScopeId,
                ScopeName = dto.ScopeName,
                FullPath = dto.FullPath,
                IsRemoved = dto.IsRemoved
            };
        }

        public static RMSubJobDto ConvertSubJob2Dto(RMSubJob subJob)
        {
            if (subJob == null)
            {
                return null;
            }
            RMSubJobDto dto = new RMSubJobDto();
            dto.Id = subJob.Id;
            dto.JobType = subJob.JobType;
            dto.ParentId = subJob.ParentId;
            dto.Status = subJob.Status;
            dto.LastUpdateTime = subJob.LastUpdateTime;
            dto.Weight = subJob.Weight;
            dto.Runable = subJob.Runable;
            dto.StartTime = subJob.StartTime;
            dto.String1 = subJob.String1;
            dto.FarmId = subJob.FarmId;
            dto.O365TenantId = subJob.O365TenantId;
            return dto;
        }

        #region Physical
        public static Record ConvertPhysicalToRMBaseRecord(PhysicalObjectDto uiRecord)
        {
            var jsonStr = string.Empty;
            using (new RA.Common.PerformanceScope("PhysicalRecord.RA.DB.Core.ConvertUtil.ConvertPhysicalToRMBaseRecord.SerializeMetaInfo"))
            {
                jsonStr = JsonConvert.SerializeObject(uiRecord.MetaInfo);
            }
            var tempId = uiRecord.Id == Guid.Empty ? Guid.NewGuid() : uiRecord.Id;
            int recordStatus = 1;

            //recordStatus = int.Parse(uiRecord.MetaInfo.FirstOrDefault(r => r.Key.Equals(DefaultColumnIDs.Status, StringComparison.OrdinalIgnoreCase)).Value.ToString().Split('|')[1]);
            string statusObj;
            uiRecord.MetaInfo.TryGetValue(DefaultColumnIDs.Status, out statusObj);
            if (!string.IsNullOrEmpty(statusObj))
            {
                var statusDic = JsonConvert.DeserializeObject<ChoiceColumnValue>(statusObj);
                recordStatus = int.Parse(statusDic.Value);
            }

            return new Record()
            {
                Id = tempId,
                NodeId = tempId,
                CreateDate = uiRecord.CreateDate,
                SourceFlag = (int)SourceFlag.Physical,
                RecordStatus = recordStatus,
                LeafName = uiRecord.Name,
                NodeType = (int)uiRecord.NodeType,
                TermId = uiRecord.TermId,
                TermName = uiRecord.TermName,
                RecordsId = uiRecord.UniqueId,
                LocationId = uiRecord.LocationId,
                BoxId = uiRecord.BoxId,
                FileId = uiRecord.FileId,
                IsLocked = uiRecord.IsLocked,
                TemplateId = uiRecord.TemplateId,
                MetaInfo = jsonStr,
                DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(uiRecord.DisposalDueDate),
                //System Fields
                CreatedBy = uiRecord.CreatedBy.IsNullOrWhiteSpace() ? TenantLocalValue.DisplayName : uiRecord.CreatedBy,
                ModifiedBy = TenantLocalValue.DisplayName,
                TimeCreated = DateTime.UtcNow.Ticks,
                TimeModified = DateTime.UtcNow.Ticks,
                ExportToRECO = uiRecord.ExportToRECO,
                ParentId = uiRecord.ParentId,
                Ancestors = uiRecord.Ancestors,
                PhysicalActionAudit = uiRecord.PhysicalActionAudit,
            };
        }

        public static PhysicalObjectDto ConvertRMBaseRecordToPhysical(Record dbRecord, Dictionary<int, RMAccount> accountMap = null)
        {
            var metaInfo = new Dictionary<string, string>();
            var barcode = dbRecord.RecordsId;
            var gls = GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult();
            using (new RA.Common.PerformanceScope("PhysicalRecord.RA.DB.Core.ConvertUtil.ConvertRMBaseRecordToPhysical.ReerializeMetaInfo"))
            {
                metaInfo = string.IsNullOrEmpty(dbRecord.MetaInfo) ? null : JsonConvert.DeserializeObject<Dictionary<string, string>>(dbRecord.MetaInfo);
                if (metaInfo != null)
                {
                    ReplaceExcelEnter(metaInfo);
                    if (!metaInfo.TryGetValue(DefaultColumnIDs.Barcode, out string value))
                    {
                        metaInfo[DefaultColumnIDs.Barcode] = dbRecord.RecordsId;
                    }
                    else
                    {
                        barcode = value;
                    }
                }
            }

            var result = new PhysicalObjectDto()
            {
                Id = dbRecord.Id,
                CreateDate = dbRecord.CreateDate,
                Name = ReplaceEnterInExcel(dbRecord.LeafName),
                NodeType = (RMNodeType)dbRecord.NodeType,
                UniqueId = dbRecord.RecordsId,
                LocationId = dbRecord.LocationId,
                BoxId = dbRecord.BoxId,
                FileId = dbRecord.FileId,
                IsLocked = dbRecord.IsLocked,
                TemplateId = dbRecord.TemplateId,
                MetaInfo = metaInfo,
                CreatedBy = dbRecord.CreatedBy,
                CreateTime = dbRecord.TimeCreated,
                CreateTimeStr = GeneralSettingService.ConvertTiksToDateTime(gls, dbRecord.TimeCreated, true).SimplifyFormatTime,
                ModifiedBy = dbRecord.ModifiedBy,
                ModifiedTime = dbRecord.TimeModified,
                ModifiedTimeStr = GeneralSettingService.ConvertTiksToDateTime(gls, dbRecord.TimeModified, true).SimplifyFormatTime,
                Status = dbRecord.RecordStatus,
                DisposalDueDate = DueDateUtil.ConvertLongDueDate2String(dbRecord.DisposalDueDate),
                //RecordOwner = dbRecord.RecordOwner ?? I18N.Core.I18NEntity.GetString("RM_JS_PRM_PRE_UserIsNull"),//RECO-4872
                HoldType = dbRecord.HoldType,
                HoldBy = dbRecord.HoldBy ?? I18N.Core.I18NEntity.GetString("RM_JS_PRM_PRE_UserIsNull"),
                HoldReleaseTime = dbRecord.HoldReleaseTime,
                HoldReleaseTimeStr = GeneralSettingService.ConvertTiksToDateTime(gls, dbRecord.HoldReleaseTime, true).SimplifyFormatTime,
                TermId = dbRecord.TermId,
                TermName = dbRecord.TermName,
                RuleId = dbRecord.RuleId,
                SourceFlag = dbRecord.SourceFlag,
                RelatedRecordsCount = dbRecord.RelatedRecordsCount,
                ExportToRECO = dbRecord.ExportToRECO,
                ScopePermissionId = dbRecord.ScopePermissionId,
                ParentId = dbRecord.ParentId,
                Ancestors = dbRecord.Ancestors,
                HoldProfileId = dbRecord.HoldId,
                BarcodeId = barcode,
            };
            if (!string.IsNullOrEmpty(dbRecord.PhysicalActionAudit))
            {
                var audits = JsonConvert.DeserializeObject<List<PhysicalAudit>>(dbRecord.PhysicalActionAudit);
                foreach(var audit in audits)
                {
                    try
                    {
                        audit.ActionTimeStr = GeneralSettingService.ConvertTiksToDateTime(gls, audit.ActionTime, true).SimplifyFormatTime;
                        audit.ActionUser = I18NEntity.GetString(audit.ActionUser);
                        if (audit.ModifyContent != null && audit.ModifyContent.Count > 0)
                        {
                            audit.ModifyContent.ForEach(content => content.TargetSetting = I18NEntity.GetString(content.TargetSetting));
                            if (audit.ActionType == PhysicalActionType.Move)
                            {
                                var tempPath = I18NEntity.GetString("RM_SPS_Location_RootNode");
                                audit.ModifyContent[0].OldValue = tempPath + audit.ModifyContent[0].OldValue[audit.ModifyContent[0].OldValue.IndexOf('/')..];
                                audit.ModifyContent[0].NewValue = tempPath + audit.ModifyContent[0].NewValue[audit.ModifyContent[0].NewValue.IndexOf('/')..];
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        logger.Error($"Convert record [{dbRecord.LeafName}] action audit failed, error : {e}");
                    }
                }
                result.PhysicalActionAudit = JsonConvert.SerializeObject(audits);
            }
                
            if (dbRecord.ManualReviewer != null && dbRecord.ManualReviewer.Length > 0 && accountMap != null)
            {
                try
                {
                    result.RecordOwner = string.Join("; ", dbRecord.ManualReviewer.Select(a => accountMap[a].DisplayName));
                }
                catch (Exception e)
                {
                    logger.Warn($"Convert record owner {string.Join(", ", dbRecord.ManualReviewer)} error: {e}");
                }
            }

            return result;
        }

        public static PhysicalObjectDto ConvertLocationObjToPhysicalObj(RMLocation data, bool needDetail = false)
        {
            var result = new PhysicalObjectDto();
            var gls = GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult();
            result.Id = data.UniqueId;
            result.Name = data.Name;
            result.CreateTime = data.CreatedTime;
            result.CreateTimeStr = GeneralSettingService.ConvertTiksToDateTime(gls, data.CreatedTime, true).SimplifyFormatTime;
            result.ModifiedTime = data.ModifiedTime;
            result.ModifiedTimeStr = GeneralSettingService.ConvertTiksToDateTime(gls, data.ModifiedTime, true).SimplifyFormatTime;
            result.Capacity = data.AvailableSpace;
            result.NodeType = (RMNodeType)data.NodeType;
            result.MetaInfo = new Dictionary<string, string>();
            result.MetaInfo.Add(DefaultColumnIDs.NameOrTitle, data.Name);
            result.MetaInfo.Add(DefaultColumnIDs.Description, data.Description);
            result.MetaInfo.Add(DefaultColumnIDs.Capability, data.AvailableSpace.ToString());
            if (needDetail)
            {
                result.MetaInfo.Add(DefaultColumnIDs.Path, data.PathForDisplay);
            }
            return result;
        }
        /// <summary>
        /// Import的数据可能包含Excel中的换行符，替换成正常的换行符， 临时方案
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string ReplaceEnterInExcel(string value)
        {
            //nsw Service完成之后再恢复
            //if (value != null)
            //{
            //    return value.Replace("_x000D_", "\r");
            //}
            return value;
        }

        private static void ReplaceExcelEnter(Dictionary<string, string> meta)
        {
            if (meta.ContainsKey("de5e99cb-4fb4-4e25-b732-a1dce71dd048"))
            {
                meta["de5e99cb-4fb4-4e25-b732-a1dce71dd048"] = ReplaceEnterInExcel(meta["de5e99cb-4fb4-4e25-b732-a1dce71dd048"]);
            }
        }


        public static RMBarcodeTemplate ConvertToRMBarcodeTemplate(BarcodeTemplateDto dto)
        {
            RMBarcodeTemplate rmBarcodeTemplate = new RMBarcodeTemplate();
            string prifix = string.Empty;
            if (!string.IsNullOrEmpty(dto.ImgBase64Str))
            {
                prifix = dto.ImgBase64Str.Substring(0, dto.ImgBase64Str.IndexOf(",") + 1);
                dto.ImgBase64Str = dto.ImgBase64Str.Substring(dto.ImgBase64Str.IndexOf(",") + 1);
                rmBarcodeTemplate.ImageColumnA = Convert.FromBase64String(dto.ImgBase64Str);
            }
            rmBarcodeTemplate.Prefix = prifix;
            rmBarcodeTemplate.ImageType = dto.ImageType;
            rmBarcodeTemplate.ImageName = dto.ImageName;
            rmBarcodeTemplate.ColumnB = dto.ColumnB;
            rmBarcodeTemplate.ColumnC = dto.ColumnC;
            rmBarcodeTemplate.ColumnDList = dto.ColumnD;
            rmBarcodeTemplate.ColumnE = dto.ColumnE;
            rmBarcodeTemplate.ColumnF = dto.ColumnF;
            rmBarcodeTemplate.Type = (int)dto.Type;
            return rmBarcodeTemplate;
        }

        public static BarcodeTemplateDto ConvertToBarcodeTemplateDto(RMBarcodeTemplate rmBarcodeTemplate)
        {
            BarcodeTemplateDto barcodeTemplateDto = new BarcodeTemplateDto();
            if (rmBarcodeTemplate.ImageColumnA != null)
            {
                barcodeTemplateDto.ImgBase64Str = rmBarcodeTemplate.Prefix + Convert.ToBase64String(rmBarcodeTemplate.ImageColumnA);
            }
            barcodeTemplateDto.Id = rmBarcodeTemplate.Id.ToString();
            barcodeTemplateDto.ImageName = rmBarcodeTemplate.ImageName;
            barcodeTemplateDto.ImageType = rmBarcodeTemplate.ImageType;
            barcodeTemplateDto.ColumnB = rmBarcodeTemplate.ColumnB;
            barcodeTemplateDto.ColumnC = rmBarcodeTemplate.ColumnC;
            barcodeTemplateDto.ColumnD = rmBarcodeTemplate.ColumnDList;
            barcodeTemplateDto.ColumnE = rmBarcodeTemplate.ColumnE;
            barcodeTemplateDto.ColumnF = rmBarcodeTemplate.ColumnF;
            barcodeTemplateDto.Type = (BarcodeTemplateType)rmBarcodeTemplate.Type;
            //barcodeTemplateDto.lastModifiedOn = mGeneralSettingService.ConvertTiksToDateTime(rmBarcodeTemplate.ModifyTime, true).SimplifyFormatTime;
            return barcodeTemplateDto;
        }

        public static BarcodeTemplateSuiteDto ConvertToBarcodeTemplateSuiteDto(RMCustomBarcodeTemplateSuite cusomtBarcodeTemplateSuite)
        {
            var gls = GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult();
            var dto = new BarcodeTemplateSuiteDto
            {
                Id = cusomtBarcodeTemplateSuite.Id,
                SuiteId = cusomtBarcodeTemplateSuite.UniqueId,
                IsDefault = cusomtBarcodeTemplateSuite.IsDefault,
                Name = cusomtBarcodeTemplateSuite.Name,
                Description = cusomtBarcodeTemplateSuite.Description,
                LabelType = cusomtBarcodeTemplateSuite.LabelType
            };
            if (dto.Name.Equals("RM_Custom_Barcode_Template_Suite_Default"))
            {
                dto.Name = I18NEntity.GetString("RM_Custom_Barcode_Template_Suite_Default");
            }
            return dto;
        }

        public static BarcodeDefaultTemplateDto ConvertToBarcodeDefaultTemplateDto(RMCustomBarcodeTemplateSuite cusomtBarcodeTemplateSuite, List<BarcodeTemplateDto> defaultTemplates)
        {
            BarcodeDefaultTemplateDto dto = new BarcodeDefaultTemplateDto
            {
                Id = cusomtBarcodeTemplateSuite.Id,
                SuiteId = cusomtBarcodeTemplateSuite.UniqueId,
                IsDefault = cusomtBarcodeTemplateSuite.IsDefault,
                Name = cusomtBarcodeTemplateSuite.Name,
                Description = cusomtBarcodeTemplateSuite.Description,
                LabelType = cusomtBarcodeTemplateSuite.LabelType,
                Templates = defaultTemplates
            };
            return dto;
        }

        public static BarcodeCustomTemplateDto ConvertToBarcodeCustomTemplateDto(RMCustomBarcodeTemplateSuite cusomtBarcodeTemplateSuite, List<BarcodeCustomTemplateInfo> customTemplates)
        {
            var dto = new BarcodeCustomTemplateDto
            {
                Id = cusomtBarcodeTemplateSuite.Id,
                SuiteId = cusomtBarcodeTemplateSuite.UniqueId,
                IsDefault = cusomtBarcodeTemplateSuite.IsDefault,
                Name = cusomtBarcodeTemplateSuite.Name,
                Description = cusomtBarcodeTemplateSuite.Description,
                LabelType = cusomtBarcodeTemplateSuite.LabelType,
                Templates = customTemplates
            };
            return dto;
        }

        public static BarcodeTemplatePropertyDto ConvertToBarcodeTemplatePropertyDto(RMCustomBarcodeTemplateProperty cusomtBarcodeTemplateProperty)
        {
            var gls = GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult();
            BarcodeTemplatePropertyDto dto = new BarcodeTemplatePropertyDto
            {
                Id = cusomtBarcodeTemplateProperty.Id,
                TemplateId = cusomtBarcodeTemplateProperty.TemplateId,
                FontSize = cusomtBarcodeTemplateProperty.FontSize,
                Name = cusomtBarcodeTemplateProperty.Name,
                Position = cusomtBarcodeTemplateProperty.Position,
            };
            return dto;
        }

        public static BarcodeTemplateDto ConvertCustomBarcodeTemplateToDto(RMCustomBarcodeTemplate customTemplate, List<RMBarcodeTemplateColumnMembership> columnMemberships)
        {
            BarcodeTemplateDto dto = new BarcodeTemplateDto();
            if (customTemplate != null)
            {
                dto.Id = customTemplate.Id.ToString();
                dto.Type = (BarcodeTemplateType)customTemplate.Type;
                
                if (!string.IsNullOrEmpty(customTemplate.PropertiesJson))
                {
                    try
                    {
                        var properties = SerializerHelper.DeserializeByDataContractSerializer<RMBarcodeTemplate>(customTemplate.PropertiesJson);
                        dto.ImgBase64Str = properties.Prefix + Convert.ToBase64String(properties.ImageColumnA);
                        dto.ImageType = properties.ImageType;
                        dto.ImageName = properties.ImageName;
                        dto.ColumnB = properties.ColumnB;
                        dto.ColumnC = properties.ColumnC;
                        dto.ColumnE = properties.ColumnE;
                        dto.ColumnF = properties.ColumnF;
                        dto.ColumnD = columnMemberships.Select(item => item.ColumnName).ToList();
                    }
                    catch
                    {
                        // If JSON parsing fails, keep default values
                    }
                }
            }
            return dto;
        }

        public static RMBarcodeTemplate ConvertCustomBarcodeTemplateToDefault(RMCustomBarcodeTemplate customTemplate, List<RMBarcodeTemplateColumnMembership> columnMemberships)
        {
            var dto = new RMBarcodeTemplate();
            if (customTemplate != null)
            {

                if (!string.IsNullOrEmpty(customTemplate.PropertiesJson))
                {
                    try
                    {
                        var properties = SerializerHelper.DeserializeByJsonConvert<RMBarcodeTemplate>(customTemplate.PropertiesJson);
                        dto.Id = customTemplate.Id;
                        dto.Type = (int)customTemplate.Type;
                        dto.ImageColumnA = properties.ImageColumnA;
                        dto.Prefix = properties.Prefix;
                        dto.ImageType = properties.ImageType;
                        dto.ImageName = properties.ImageName;
                        dto.ColumnB = properties.ColumnB;
                        dto.ColumnC = properties.ColumnC;
                        dto.ColumnE = properties.ColumnE;
                        dto.ColumnF = properties.ColumnF;
                        dto.ColumnDList = columnMemberships.Select(item => item.ColumnName).ToList();
                    }
                    catch
                    {
                        // If JSON parsing fails, keep default values
                    }
                }
            }
            return dto;
        }

        public static List<RMPhysicalRequest> ConvertDto2Domain(List<PhysicalRequestDto> physicals)
        {
            List<RMPhysicalRequest> result = new();
            if (physicals == null || physicals.Count == 0)
            {
                return result;
            }
            foreach (PhysicalRequestDto dto in physicals)
            {
                result.Add(ConvertGeneralInfomationPhysicalRequest(dto));
            }
            return result;
        }

        public static List<(PhysicalObjectDto, RMPhysicalRequest)> ConvertDto2Domain(PhysicalRequestDto physical)
        {
            if (physical == null)
            {
                return null;
            }
            var physicalRequests = new List<(PhysicalObjectDto, RMPhysicalRequest)>();
            if (physical.PhysicalFileInfos != null)
            {
                foreach (var physicalFileInfo in physical.PhysicalFileInfos)
                {
                    RMPhysicalRequest domain = ConvertGeneralInfomationPhysicalRequest(physical);
                    domain.Title = physicalFileInfo.Name;
                    domain.PhysicalFileId = physicalFileInfo.UniqueId;
                    if (physicalFileInfo != null)
                    {
                        if (physicalFileInfo.Id == Guid.Empty)
                        {
                            physicalFileInfo.Id = Guid.NewGuid();
                        }
                        domain.MetaData = SerializerHelper.SerializeByDataContractSerializer(physicalFileInfo);

                        if (physicalFileInfo.ScopePerDto != null)
                        {
                            domain.ScopePermissionInfo = SerializerHelper.SerializeByDataContractSerializer(physicalFileInfo.ScopePerDto);
                        }

                        if (physical.MoveDto != null)
                        {
                            domain.MoveInfo = SerializerHelper.SerializeByDataContractSerializer(physical.MoveDto);
                        }
                        physicalRequests.Add((physicalFileInfo ,domain));
                    }
                }
            }
            else if(physical.PhysicalFileInfo != null)
            {
                RMPhysicalRequest domain = ConvertGeneralInfomationPhysicalRequest(physical);
                domain.Title = physical.PhysicalFileInfo.Name;
                domain.PhysicalFileId = physical.PhysicalFileInfo.UniqueId;
                if (physical.PhysicalFileInfo.Id == Guid.Empty)
                {
                    physical.PhysicalFileInfo.Id = Guid.NewGuid();
                }
                domain.MetaData = SerializerHelper.SerializeByDataContractSerializer(physical.PhysicalFileInfo);

                if (physical.PhysicalFileInfo.ScopePerDto != null)
                {
                    domain.ScopePermissionInfo = SerializerHelper.SerializeByDataContractSerializer(physical.PhysicalFileInfo.ScopePerDto);
                }
                if (physical.MoveDto != null)
                {
                    domain.MoveInfo = SerializerHelper.SerializeByDataContractSerializer(physical.MoveDto);
                }
                physicalRequests.Add((physical.PhysicalFileInfo ,domain));
            }
            else
            {
                RMPhysicalRequest domain = ConvertGeneralInfomationPhysicalRequest(physical);
                if (physical.MoveDto != null)
                {
                    domain.MoveInfo = SerializerHelper.SerializeByDataContractSerializer(physical.MoveDto);
                }
                physicalRequests.Add((physical.PhysicalFileInfo, domain));
            }
            return physicalRequests;
        }

        private static RMPhysicalRequest ConvertGeneralInfomationPhysicalRequest(PhysicalRequestDto physical)
        {
            RMPhysicalRequest domain = new RMPhysicalRequest();
            domain.Id = physical.Id;
            domain.GroupRequestId = physical.GroupRequestId;
            domain.Type = (int)physical.Type;
            domain.Status = (int)physical.Status;
            domain.CreatedTime = physical.CreatedTime;
            domain.ModifiedTime = physical.ModifiedTime;
            domain.CreatedUserId = physical.CreatedUserId;
            domain.ManagerUserId = physical.ManagerUserId;
            domain.HoldUserId = physical.HoldUserId;
            domain.HoldByDisplayName = physical.HoldUserDisplay;
            domain.Comment = physical.Comment;
            if (physical.DisposalClass != null)
            {
                domain.HoldCategory = (int)physical.DisposalClass.HoldCategory;
                domain.HoldNumber = physical.DisposalClass.HoldNumber;
                domain.HoldUnit = (int)physical.DisposalClass.HoldUnit;
                domain.TimeZoneId = physical.DisposalClass.TimeZoneId;
                domain.EndTimeStr = physical.DisposalClass.EndTimeStr;
                domain.IsDaylightSavingTime = physical.DisposalClass.IsDaylightSavingTime;
                domain.EndTime = CalculateEndtime(physical.DisposalClass, physical.CreatedTime);
                domain.ReviewComment = physical.DisposalClass.ReviewComment;
            }
            return domain;
        }

        private static long CalculateEndtime(PhysicalRequestDisposal disposal, long createTime)
        {
            if (disposal.HoldCategory == HoldCategory.Before)
            {
                if (!string.IsNullOrEmpty(disposal.EndTimeStr))
                {
                    return ConvertTimeToUtc(disposal.EndTimeStr, disposal.TimeZoneId, disposal.IsDaylightSavingTime);
                }
            }
            else if (disposal.HoldCategory == HoldCategory.Last)
            {
                DateTime temp = new DateTime(createTime, DateTimeKind.Utc);
                if (disposal.HoldUnit == HoldUnit.Year)
                {
                    temp.AddYears(disposal.HoldNumber);
                }
                else if (disposal.HoldUnit == HoldUnit.Month)
                {
                    temp.AddMonths(disposal.HoldNumber);
                }
                else if (disposal.HoldUnit == HoldUnit.Day)
                {
                    temp.AddDays(disposal.HoldNumber);
                }
                else
                {
                    return 0;
                }
                return temp.Ticks;
            }
            return 0;
        }


        private static long ConvertTimeToUtc(string timeStr, string timeZoneId, bool isDaylightSaving)
        {
            DateTime temp = DateTime.Parse(timeStr);
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;

            try
            {
                timeZoneInfo = GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId);
            }
            catch (Exception e)
            {
                logger.Error("Get time zone failed by timezoneid {0}", timeZoneId);
                logger.Error(e.Message, e);
            }
            DateTime dest = DateTimeUtil.ConvertTimeToUtcDate(temp, timeZoneInfo, !isDaylightSaving);
            return dest.Ticks;
        }
        #endregion

        #region fs
        public static Record ConvertFSDtoToRMBaseRecord(FileSystemRecordDto dto)
        {
            var tempId = dto.NodeId == Guid.Empty ? Guid.NewGuid() : dto.NodeId;
            return new Record()
            {
                Id = tempId,
                NodeId = tempId,
                SourceFlag = (int)SourceFlag.FileSystem,
                RecordStatus = dto.RecordStatus,
                DestroyedTime = dto.DestroyedTime,
                LeafName = dto.LeafName,
                NodeType = (int)dto.NodeType,
                TermId = dto.TermId,
                TermName = dto.TermName,
                ItemId = dto.ItemId,
                ListId = dto.ListId,
                ItemRowId = dto.ItemRowId,
                ParentId = dto.ParentId,
                AveSiteId = dto.AveSiteId,
                CollectTime = DateTime.UtcNow.Ticks,
                DirPath = dto.DirPath,
                FolderId = dto.FolderId,
                RecordHistory = dto.RecordHistory,
                RecordOwner = dto.RecordOwner,
                RuleId = dto.RuleId,
                ScopeId = dto.ScopeId,
                WebId = dto.WebId,
                RuleLevel = dto.RuleLevel,
                MetaInfo = dto.MetaInfo,
                Extsion1 = dto.Extsion1,
                FullPath = dto.FullPath,
                ExtensionForFile = dto.ExtensionForFile,
                HoldStatus = dto.HoldStatus,
                HoldReleaseTime = dto.HoldReleaseTime,
                HoldBy = dto.HoldBy,
                HoldId = dto.HoldId,
                HoldType = dto.HoldType,
                PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(dto.PreviosDisposalDueDate),
                DeclareAsRecord = dto.DeclareAsRecord,
                RecordsId = dto.RecordsId,
                //LocationId = dto.LocationId,
                //BoxId = dto.BoxId,
                //FileId = dto.FileId,
                //TemplateId = dto.TemplateId,
                DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(dto.DisposalDueDate),
                //System Fields
                CreatedBy = dto.CreatedBy,
                ModifiedBy = dto.ModifiedBy,
                DeclaredBy = dto.DeclaredBy,
                DeleteRelatedRecords = dto.DeleteRelatedRecords,
                RelatedRecords = dto.RelatedRecords,
                RelatedRecordsCount = dto.RelatedRecordsCount,
                TimeCreated = dto.TimeCreated1.Ticks,
                TimeModified = dto.TimeLastModified,
                SortTicks = dto.SortTicks,
                CreateDate = dto.CreateDate,
                IsManualSynced = dto.IsManualSynced,
                ManualActionTime = dto.ManualActionTime,
                ManualApprovedBy = dto.ManualApprovedBy,
                ManualEscalatedComment = dto.ManualEscalatedComment,
                ManualApprovedStatus = dto.ManualApprovedStatus,
                ManualInternalApprovedStatus = dto.ManualInternalApprovedStatus,
                ManualArchiveStatus = dto.ManualArchiveStatus,
                ManualFullPath = dto.ManualFullPath,
                ManualFolderPath = dto.ManualFolderPath, 
                ManualSiteUrl = dto.ManualSiteUrl,
                ManualEscalateFrom = dto.ManualEscalateFrom,
                ManualExtendTime = dto.ManualExtendTime,
                ManualExtendComment = dto.ManualExtendComment,
                ManualCollectionTime = dto.ManualCollectionTime,
                ManualAudits = dto.ManualAudits,
                ManualArchivedTime = dto.ManualArchivedTime,
                ManualPartitionKey = dto.ManualPartitionKey,
                ManualRowKey = dto.ManualRowKey,
                ManualRuleName = dto.ManualRuleName,
                ManualRuleCriteria = dto.ManualRuleCriteria,
                ManualRuleDisposalClass = dto.ManualRuleDisposalClass,
                ManualVersion = dto.ManualVersion,
                ManualReviewer = dto.ManualReviewer,
                ManualRelatedRecordsAction = dto.ManualRelatedRecordsAction,
                ManualRelatedRecords = dto.ManualRelatedRecords,
                ManualIsRelatedRecords = dto.ManualIsRelatedRecords,
                ManualWorkflowInstanceId = dto.ManualWorkflowInstanceId,
                ManualWorkflowDefinitionId = dto.ManualWorkflowDefinitionId,
                ManualWorkflowStepId = dto.ManualWorkflowStepId,
                ManualExtendCount = dto.ManualExtendCount,
                ManualEmailNotificationCount = dto.ManualEmailNotificationCount,
                ManualEmailNotificationLastTime = dto.ManualEmailNotificationLastTime,
                ManualNeedEmailNotification = dto.ManualNeedEmailNotification,
                ManualIsAutoReassigned = dto.ManualIsAutoReassigned,
                HoldByUsers = dto.HoldByUsers,
                HoldUntilTimes = dto.HoldUntilTimes,
                AppendHolds_Array = dto.AppendHolds_Array,
                Depth = dto.Depth,
                JPMCFSFileSize = dto.JPMCFSFileSize,
                JPMCFSFileCount = dto.JPMCFSFileCount,
                ClassCode = dto.ClassCode,
                CountryCode = dto.CountryCode,
                RetentionType = dto.RetentionType.ToString(),
                StartDate = dto.StartDate,
                EndTime = dto.EndTime,
                PolicyValueUnit = dto.PolicyValueUnit.ToString(),
                PolicyValueNumber = dto.PolicyValueNumber.ToString(),
            };
        }

        public static FileSystemRecordDto ConvertRMBaseRecordToFSDto(Record record)
        {
            if (record == null)
            {
                return null;
            }
            return new FileSystemRecordDto()
            {
                NodeId = record.NodeId,
                SourceFlag = (int)SourceFlag.FileSystem,
                RecordStatus = record.RecordStatus,
                DestroyedTime = record.DestroyedTime,
                LeafName = record.LeafName,
                NodeType = (int)record.NodeType,
                TermId = record.TermId,
                TermName = record.TermName,
                ItemId = record.ItemId,
                ListId = record.ListId,
                ItemRowId = record.ItemRowId,
                ParentId = record.ParentId,
                AveSiteId = record.AveSiteId,
                CollectionTime = record.CollectTime,
                DirPath = record.DirPath,
                FolderId = record.FolderId,
                RecordHistory = record.RecordHistory,
                RecordOwner = record.RecordOwner,
                RuleId = record.RuleId,
                ScopeId = record.ScopeId,
                WebId = record.WebId,
                RuleLevel = record.RuleLevel,
                MetaInfo = record.MetaInfo,
                Extsion1 = record.Extsion1,
                FullPath = record.FullPath,
                ExtensionForFile = record.ExtensionForFile,
                HoldStatus = record.HoldStatus,
                HoldReleaseTime = record.HoldReleaseTime,
                HoldBy = record.HoldBy,
                HoldId = record.HoldId,
                HoldType = record.HoldType,
                PreviosDisposalDueDate = DueDateUtil.ConvertLongDueDate2String(record.PreviosDisposalDueDate),
                DeclareAsRecord = record.DeclareAsRecord,
                RecordsId = record.RecordsId,
                LocationId = record.LocationId,
                BoxId = record.BoxId,
                FileId = record.FileId,
                TemplateId = record.TemplateId,
                DisposalDueDate = DueDateUtil.ConvertLongDueDate2String(record.DisposalDueDate),
                //System Fields
                CreatedBy = record.CreatedBy,
                ModifiedBy = record.ModifiedBy,
                DeclaredBy = record.DeclaredBy,
                DeleteRelatedRecords = record.DeleteRelatedRecords,
                RelatedRecords = record.RelatedRecords,
                RelatedRecordsCount = record.RelatedRecordsCount,
                TimeCreated1 = new DateTime(record.TimeCreated), //DateTime.UtcNow,
                TimeLastModified = record.TimeModified, // DateTime.UtcNow.Ticks,
                SortTicks = record.SortTicks,
                CreateDate = record.CreateDate,

                IsManualSynced = record.IsManualSynced,
                ManualActionTime = record.ManualActionTime,
                ManualApprovedBy = record.ManualApprovedBy,
                ManualEscalatedComment = record.ManualEscalatedComment,
                ManualApprovedStatus = record.ManualApprovedStatus,
                ManualInternalApprovedStatus = record.ManualInternalApprovedStatus,
                ManualArchiveStatus = record.ManualArchiveStatus,
                ManualFullPath = record.ManualFullPath,
                ManualEscalateFrom = record.ManualEscalateFrom,
                ManualExtendTime = record.ManualExtendTime,
                ManualExtendComment = record.ManualExtendComment,
                ManualCollectionTime = record.ManualCollectionTime,
                ManualAudits = record.ManualAudits,
                ManualArchivedTime = record.ManualArchivedTime,
                ManualPartitionKey = record.ManualPartitionKey,
                ManualRowKey = record.ManualRowKey,
                ManualRuleName = record.ManualRuleName,
                ManualRuleCriteria = record.ManualRuleCriteria,
                ManualRuleDisposalClass = record.ManualRuleDisposalClass,
                ManualVersion = record.ManualVersion,
                ManualReviewer = record.ManualReviewer,
                ManualRelatedRecordsAction = record.ManualRelatedRecordsAction,
                ManualRelatedRecords = record.ManualRelatedRecords,
                ManualIsRelatedRecords = record.ManualIsRelatedRecords,
                ManualWorkflowInstanceId = record.ManualWorkflowInstanceId,
                ManualWorkflowDefinitionId = record.ManualWorkflowDefinitionId,
                ManualWorkflowStepId = record.ManualWorkflowStepId,
                ManualExtendCount = record.ManualExtendCount,
                ManualEmailNotificationCount = record.ManualEmailNotificationCount,
                ManualEmailNotificationLastTime = record.ManualEmailNotificationLastTime,
                ManualNeedEmailNotification = record.ManualNeedEmailNotification,
                ManualIsAutoReassigned = record.ManualIsAutoReassigned,

                HoldByUsers = record.HoldByUsers,
                HoldUntilTimes = record.HoldUntilTimes,
                AppendHolds_Array = record.AppendHolds_Array,
                JPMCFSFileSize = record.JPMCFSFileSize,
                ClassCode = record.ClassCode,
                CountryCode = record.CountryCode,
                StartDate = record.StartDate,
                RetentionType = string.IsNullOrEmpty(record.RetentionType)?0:Convert.ToInt32(record.RetentionType),
                EndTime = record.EndTime,
                PolicyValueNumber = string.IsNullOrEmpty(record.PolicyValueNumber) ? 0 : Convert.ToInt32(record.PolicyValueNumber),
                PolicyValueUnit = string.IsNullOrEmpty(record.PolicyValueUnit) ? 0 : Convert.ToInt32(record.PolicyValueUnit)
            };
        }

        /// <summary>
        /// Converts a Record to the lightweight FsRecordProcessDto,
        /// containing only the properties required for ADS processing.
        /// </summary>
        public static FsRecordProcessDto ConvertRecordToFsRecordProcessDto(Record record)
        {
            return new FsRecordProcessDto
            {
                RecordsId = record.RecordsId,
                NodeId = record.NodeId,
                NodeType = (int)record.NodeType,
                CreateDate = record.CreateDate,
                MetaInfo = record.MetaInfo,
                HoldStatus = record.HoldStatus,
                HoldType = record.HoldType,
                HoldReleaseTime = record.HoldReleaseTime,
                HoldId = record.HoldId,
                HoldBy = record.HoldBy,
                HoldByUsers = record.HoldByUsers,
                HoldUntilTimes = record.HoldUntilTimes,
                AppendHolds_Array = record.AppendHolds_Array,
                DisposalDueDate = DueDateUtil.ConvertLongDueDate2String(record.DisposalDueDate),
                FullPath = $@"{record.DirPath}\{record.LeafName}"
            };
        }

        public static FileSystemTableEntity ConvertFSDto2ArchiverTableEntity(FSAzureTableEntityDto dto)
        {
            FileSystemTableEntity entity = new FileSystemTableEntity()
            {
                PartitionKey = dto.ConnectionId.ToString(),
                RowKey = dto.FilePathMd5.ToString(),
                CreateTime = dto.CreateTime.Equals(DateTime.MinValue) ? (DateTime)SqlDateTime.MinValue : dto.CreateTime,
                DisposalAction = dto.DisposalAction,
                HighName = dto.HighName,
                KeepDataOption = dto.KeepDataOption,
                LowName = dto.LowName,
                LastModifiedTme = dto.LastModifiedTme.Equals(DateTime.MinValue) ? (DateTime)SqlDateTime.MinValue : dto.LastModifiedTme,
                ParentID = dto.ParentID,
                Property = dto.Property,
                RuleId = dto.RuleId,
                MovedToApprovalTable = dto.MovedToApprovalTable,
                ScanTime = dto.ScanTime.Equals(DateTime.MinValue) ? (DateTime)SqlDateTime.MinValue : dto.ScanTime,
                NodeLevel = dto.NodeLevel,
                ScopeID = dto.ScopeID,
                AchiveTime = dto.AchiveTime.Equals(DateTime.MinValue) ? (DateTime)SqlDateTime.MinValue : dto.AchiveTime,
                SortTicks = dto.SortTicks,
                RuleAction = dto.RuleAction,
                Status = dto.Status,
                FullPath = string.IsNullOrEmpty(dto.FullPath) ? string.Empty : dto.FullPath,
                RelatedRecordInfo = string.IsNullOrEmpty(dto.RelatedRecordInfo) ? string.Empty : dto.RelatedRecordInfo,
                HasRelatedDocument = dto.HasRelatedDocument,
                CurrentSettingId = dto.CurrentSettingId,
                InternalStatus = dto.InternalApprovedStatus,
                ApprovalBy = dto.ManualApprovedBy,
                ManualEscalateFrom = dto.ManualEscalateFrom,
                RecordStatus = dto.RecordStatus,
                DestroyedTime = dto.DestroyedTime,
                ConnectionId = dto.InternalConnectionId,
            };
            return entity;
        }

        public static OnPremiseSPTableEntity ConvertOnPremiseSPDto2ArchiverTableEntity(OnPremiseSPAzureTableEntityDto dto)
        {
            OnPremiseSPTableEntity entity = new OnPremiseSPTableEntity();
            try
            {
                entity.PartitionKey = ReplaceCharacter(dto.ScopePath);
                //entity.PartitionKey = string.Format("{0}{1}", entity.PartitionKey, "Manual");//Manual Job PartitionKey需要加前缀
                //entity.RowKey = dto.JobID + "_" + Snowflake.Instance().GetTicks();
                entity.RowKey = dto.NodeID + "_" + dto.UIVersion;
                entity.ArchiveLevel = dto.ArchiveLevel;
                entity.CacheNodeType = dto.CacheNodeType;
                entity.NodeID = dto.NodeID;
                entity.ParentID = dto.ParentID;
                entity.RuleID = dto.RuleID;
                entity.RuleAction = dto.RuleAction;
                entity.ScanJobID = dto.JobID;
                entity.ScopeID = dto.ScopeID;
                entity.SiteId = new Guid(dto.SiteId);
                entity.ListId = dto.ListId;
                entity.Status = dto.Status;
                entity.MovedToApprovalTable = dto.MovedToApprovalTable;
                entity.UIVersion = dto.UIVersion;
                entity.SourceFlag = (int)SOSourceFlag.OnPremSP;
                entity.SortTicks = Snowflake.Instance().GetTicks();
                entity.HasRelatedDocument = dto.HasRelatedDocument;
                entity.DeleteRelatedRecords = dto.DeleteRelatedRecords;
                entity.RelatedRecordInfo = dto.RelatedRecordInfo;
                entity.ArchivedTime = dto.ArchivedTime;
                #region Json Meta
                OnPremiseArchiverSharePointDto spDataSource = new OnPremiseArchiverSharePointDto();
                spDataSource.ScopeID = dto.ScopeID;
                spDataSource.ScanJobID = dto.JobID;
                spDataSource.NodeID = dto.NodeID;
                spDataSource.ParentID = dto.ParentID;
                spDataSource.UIVersion = dto.UIVersion;
                spDataSource.CacheNodeType = dto.CacheNodeType;
                spDataSource.ArchiveLevel = dto.ArchiveLevel;
                spDataSource.KeepDataStatus = 0;
                spDataSource.RuleID = dto.RuleID;
                spDataSource.LastModifiedTime = dto.LastModifiedTime;
                spDataSource.LeafName = dto.LeafName;
                spDataSource.Level = dto.Level;
                spDataSource.ExpireTime = dto.ScanTime;
                spDataSource.LibRowID = dto.LibRowID;
                spDataSource.ListId = dto.ListId;
                spDataSource.NodeType = dto.NodeType;
                spDataSource.Path = dto.Path;
                spDataSource.Property = dto.Property;
                spDataSource.SPNodeLevel = dto.SPNodeLevel;
                spDataSource.ScanItemID = 0;
                spDataSource.ScanTime = dto.ScanTime;
                spDataSource.SiteUrl = dto.SiteUrl;
                spDataSource.SiteId = dto.SiteId;
                spDataSource.RegistedSiteId = dto.RegistedSiteId;
                spDataSource.WebId = dto.WebId;
                spDataSource.Metadata = dto.Metadata ?? string.Empty;
                spDataSource.SiteGroupId = dto.SiteGroupId;
                spDataSource.SiteTitle = dto.SiteTitle;
                #endregion
                string jsonMeta = JsonConvert.SerializeObject(spDataSource);
                entity.JsonMeta = jsonMeta;
            }
            catch (Exception ex)
            {
                logger.Warn("An error occur when ConvertOnPremiseSPDto2ArchiverTableEntity.Message:{0}.", ex.ToString());
            }
            return entity;
        }

        //此方法用来获取scope full path
        /// <summary>
        /// no use for now
        /// </summary>
        /// <param name="scopeFullPath"></param>
        /// <returns></returns>
        private static string ReplaceCharacter(string scopePath)
        {
            scopePath = scopePath.Replace("/", "_").Replace(@"\", "_");
            return scopePath;
        }

        public static FSFolderCacheDto ConvertExplorerData2FSFolderCacheDto(Record record)
        {
            FSFolderCacheDto dto = new FSFolderCacheDto()
            {
                Id = record.NodeId,
                RuleId = record.RuleId,
                TermId = record.TermId,
                TermName = record.TermName,
                RelatedInfo = record.RelatedRecords,
                SortTicks = record.SortTicks,
                LastModifiedTime = record.TimeModified,
                LastAccessTime = GetAccessTime(record.MetaInfo),
                HoldStatus = record.HoldStatus,
                HoldReleaseTime = record.HoldReleaseTime
            };
            return dto;
        }

        public static OnPremiseSPListCacheDto ConvertExplorerData2OnPremiseSPListCacheDto(Record record)
        {
            OnPremiseSPListCacheDto dto = new OnPremiseSPListCacheDto()
            {
                Id = record.NodeId,
                RuleId = record.RuleId,
                TermId = record.TermId,
                TermName = record.TermName,
                RelatedInfo = record.RelatedRecords,
                SortTicks = record.SortTicks,
                LastModifiedTime = record.TimeModified,
                HoldStatus = record.HoldStatus,
                HoldReleaseTime = record.HoldReleaseTime,
                ManualExtendTime = record.ManualExtendTime,
            };
            return dto;
        }

        private static long GetAccessTime(string metainfo)
        {
            var meta = JsonConvert.DeserializeObject<RecordMetaInfo>(metainfo);
            return meta.LastAccessTime;
        }

        public static FSFolderCacheDto ConvertAzureData2FSFolderCacheDto(FileSystemTableEntity entity)
        {
            FSFolderCacheDto dto = new FSFolderCacheDto()
            {
                Id = new Guid(entity.RowKey),
                RuleId = new Guid(entity.RuleId),
                Status = entity.Status,
                SortTicks = entity.SortTicks,
                RelatedInfo = entity.RelatedRecordInfo,
                MovedToApprovalTable = entity.MovedToApprovalTable
            };
            return dto;
        }

        public static OnPremiseSPListCacheDto ConvertAzureData2OnPremiseSPListCacheDto(OnPremiseSPTableEntity entity)
        {
            OnPremiseSPListCacheDto dto = new OnPremiseSPListCacheDto()
            {
                Id = entity.NodeID,
                RuleId = entity.RuleID,
                Status = entity.Status,
                SortTicks = entity.SortTicks,
                RelatedInfo = entity.RelatedRecordInfo,
                MovedToApprovalTable = entity.MovedToApprovalTable,
                DeleteRelatedRecords = entity.DeleteRelatedRecords
            };
            return dto;
        }

        #endregion

        #region sp on premise
        public static Record ConvertRecordDto2Record(RecordDto dto)
        {
            Record record = new Record()
            {
                ApproveUsers = dto.ApproveUsers,
                AveSiteId = dto.AveSiteId,
                CollectTime = dto.CollectionTime,
                ContainerId = dto.ContainerId,
                CreateDate = dto.CreateDate,
                CreatedBy = dto.CreatedBy,
                DeclareAsRecord = dto.DeclareAsRecord,
                DestroyedTime = dto.DestroyedTime,
                DeclaredBy = dto.DeclaredBy,
                DeleteRelatedRecords = dto.DeleteRelatedRecords,
                RelatedRecords = dto.RelatedRecords,
                RelatedRecordsCount = dto.RelatedRecordsCount,
                DirPath = dto.DirPath,
                DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(dto.DisposalDueDate),
                FolderId = dto.FolderId,
                FullPath = dto.FullPath,
                HoldBy = dto.HoldBy,
                HoldId = dto.HoldId,
                HoldReleaseTime = dto.HoldReleaseTime,
                FileId = dto.FileId,
                HoldStatus = dto.HoldStatus,
                HoldType = dto.HoldType,
                Id = dto.Id,
                ItemId = dto.ItemId,
                ItemRowId = dto.ItemRowId,
                LeafName = dto.LeafName,
                ListId = dto.ListId,
                ModifiedBy = dto.ModifiedBy,
                MetaInfo = dto.MetaInfo,
                NodeId = dto.NodeId,
                NodeType = dto.NodeType,
                ParentId = dto.ParentId,
                RecordHistory = dto.RecordHistory,
                LocationId = dto.LocationId,
                BoxId = dto.BoxId,
                PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(dto.PreviosDisposalDueDate),
                Extsion1 = dto.Extsion1,
                RecordOwner = dto.RecordOwner,
                RecordsId = dto.RecordsId,
                RecordStatus = dto.RecordStatus,
                RuleId = dto.RuleId,
                RuleLevel = dto.RuleLevel,
                ScopeId = dto.ScopeId,
                ExtensionForFile = dto.ExtensionForFile,
                SortTicks = dto.SortTicks,
                SourceFlag = dto.SourceFlag,
                TermId = dto.TermId,
                TermName = dto.TermName,
                TimeCreated = dto.TimeCreated,
                TimeModified = dto.TimeLastModified,
                WebId = dto.WebId,
                RecordOwner_Array = dto.RecordOwner_Array
            };
            return record;
        }


        public static RecordDto ConvertRecord2RecordDto(Record record)
        {
            RecordDto dto = new RecordDto()
            {
                ApproveUsers = record.ApproveUsers,
                AveSiteId = record.AveSiteId,
                CollectionTime = record.CollectTime,
                ContainerId = record.ContainerId,
                CreateDate = record.CreateDate,
                CreatedBy = record.CreatedBy,
                DeclareAsRecord = record.DeclareAsRecord,
                LockedByRecordLabel = record.LockedByRecordLabel,
                DestroyedTime = record.DestroyedTime,
                DeclaredBy = record.DeclaredBy,
                ApplyRecordLabelBy = record.ApplyRecordLabelBy,
                DeleteRelatedRecords = record.DeleteRelatedRecords,
                DirPath = record.DirPath,
                DisposalDueDate = DueDateUtil.ConvertLongDueDate2String(record.DisposalDueDate),
                FolderId = record.FolderId,
                FullPath = record.FullPath,
                HoldBy = record.HoldBy,
                HoldId = record.HoldId,
                HoldReleaseTime = record.HoldReleaseTime,
                FileId = record.FileId,
                HoldStatus = record.HoldStatus,
                HoldType = record.HoldType,
                Id = record.Id,
                ItemId = record.ItemId,
                ItemRowId = record.ItemRowId,
                LeafName = record.LeafName,
                ListId = record.ListId,
                ModifiedBy = record.ModifiedBy,
                MetaInfo = record.MetaInfo,
                NodeId = record.NodeId,
                NodeType = record.NodeType,
                ParentId = record.ParentId,
                RecordHistory = record.RecordHistory,
                LocationId = record.LocationId,
                BoxId = record.BoxId,
                PreviosDisposalDueDate = DueDateUtil.ConvertLongDueDate2String(record.PreviosDisposalDueDate),
                Extsion1 = record.Extsion1,
                RecordOwner = record.RecordOwner,
                RecordsId = record.RecordsId,
                RecordStatus = record.RecordStatus,
                RuleId = record.RuleId,
                RuleLevel = record.RuleLevel,
                ScopeId = record.ScopeId,
                ExtensionForFile = record.ExtensionForFile,
                SortTicks = record.SortTicks,
                SourceFlag = record.SourceFlag,
                TermId = record.TermId,
                TermName = record.TermName,
                TimeCreated = record.TimeCreated,
                TimeLastModified = record.TimeModified,
                WebId = record.WebId,
                RecordOwner_Array = record.RecordOwner_Array
            };
            return dto;
        }


        public static RecordDto ConvertBaseRecordDto2RecordDto(BaseRecordDto record)
        {
            RecordDto dto = new RecordDto()
            {
                //ApproveUsers = record.ApproveUsers,
                AveSiteId = record.AveSiteId,
                CollectionTime = record.CollectionTime,
                //ContainerId = record.ContainerId,
                CreateDate = record.CreateDate,
                CreatedBy = record.CreatedBy,
                DeclareAsRecord = record.DeclareAsRecord,
                // DestroyedTime = record.DestroyedTime,
                // DeclaredBy = record.DeclaredBy,
                //DeleteRelatedRecords = record.DeleteRelatedRecords,
                DirPath = record.DirPath,
                DisposalDueDate = record.DisposalDueDate,
                FolderId = record.FolderId,
                FullPath = record.FullPath,
                HoldBy = record.HoldBy,
                HoldId = record.HoldId,
                //// HoldReleaseTime = record.HoldReleaseTime,
                FileId = record.FileId,
                HoldStatus = record.HoldStatus,
                //HoldType = record.HoldType,
                Id = record.Id,
                ItemId = record.ItemId,
                ItemRowId = record.ItemRowId,
                LeafName = record.LeafName,
                ListId = record.ListId,
                ModifiedBy = record.ModifiedBy,
                MetaInfo = record.MetaInfo,
                NodeId = record.NodeId,
                NodeType = record.NodeType,
                //ParentId = record.ParentId,
                RecordHistory = record.RecordHistory,
                //LocationId = record.LocationId,
                BoxId = record.BoxId,
                PreviosDisposalDueDate = record.PreviosDisposalDueDate,
                //Extsion1 = record.Extsion1,
                RecordOwner = record.RecordOwner,
                RecordsId = record.RecordsId,
                RecordStatus = record.RecordStatus,
                RuleId = record.RuleId,
                RuleLevel = record.RuleLevel,
                ScopeId = record.ScopeId,
                ExtensionForFile = record.ExtensionForFile,
                //SortTicks = record.SortTicks,
                SourceFlag = record.SourceFlag,
                TermId = record.TermId,
                TermName = record.TermName,
                TimeCreated = record.TimeCreated,
                //TimeLastModified = record.TimeModified,
                WebId = record.WebId,
                //RecordOwner_Array = record.RecordOwner_Array
            };
            return dto;
        }

        //public static List<T> ConvertStringToListObject<T>(string value)
        //    where T : class
        //{
        //    if (string.IsNullOrEmpty(value))
        //        return null;
        //    var dto = new List<T>();
        //    try
        //    {
        //        List<string> infoes = new List<string>();
        //        try
        //        {
        //            infoes = SerializerHelper.DeserializeByDataContractJsonSerializer<List<string>>(value);
        //        }
        //        catch
        //        {
        //            dto.Add(SerializerHelper.DeserializeByDataContractSerializer<T>(value));
        //            return dto;
        //        }
        //        foreach (var info in infoes)
        //        {
        //            if (string.IsNullOrEmpty(info))
        //                dto.Add(null);
        //            else
        //                dto.Add(SerializerHelper.DeserializeByDataContractSerializer<T>(info));
        //        }
        //    }
        //    catch (Exception e) 
        //    {
        //        logger.Warn($"deserialize physical file info error, {e}");
        //        return null;
        //    }
        //    return dto;
        //}

        //public static string ConvertTitileOrRecordIdToJson(string value)
        //{
        //    string result = string.Empty;
        //    List<string> names = new List<string>();
        //    try
        //    {
        //        try
        //        {
        //            names = SerializerHelper.DeserializeByDataContractJsonSerializer<List<string>>(value);
        //        }
        //        catch
        //        {
        //            names = new List<string> { value };
        //        }
        //        result = SerializerHelper.SerializeByDataContractJsonSerializer(names);
        //    }
        //    catch(Exception e)
        //    {
        //        result = string.Empty;
        //        logger.Warn($"convert title or record id to json error, {e}");
        //    }
        //    return result;
        //}
        #endregion

        #region Data Ingestion
        public static RMDataIngestionMessage ConvertDataIngestionMessageDtoToModel(RMDataIngestionMessageDto dto)
        {
            return new RMDataIngestionMessage()
            {
                Id = dto.Id,
                UniqueId = dto.UniqueId,
                SourceBlobName = dto.SourceBlobName,
                IngestionType = dto.IngestionType,
                OperationType = dto.OperationType,
                Status = dto.Status,
                CreatedTime = dto.CreatedTime,
                Extension = dto.Extension
            };
        }

        public static RMDataIngestionMessageTableEntity ConvertDataIngestionMessageDtoToAzureEntity(RMDataIngestionMessageDto dto)
        {
            return new RMDataIngestionMessageTableEntity()
            {
                RowKey = string.Format("{0:D19}", DateTime.MaxValue.Ticks - new DateTime(dto.CreatedTime, DateTimeKind.Utc).Ticks),
                PartitionKey = dto.UniqueId,
                SourceBlobName = dto.SourceBlobName,
                IngestionType = (int)dto.IngestionType,
                OperationType = (int)dto.OperationType,
                Status = (int)dto.Status,
                CreatedTime = DateTime.UtcNow.Ticks,
                Extension = dto.Extension
            };
        }

        public static RMDataIngestionMessage ConvertDataIngestionMessageAzureEntityToModel(RMDataIngestionMessageTableEntity entity)
        {
            return new RMDataIngestionMessage()
            {
                UniqueId = entity.PartitionKey,
                SourceBlobName = entity.SourceBlobName,
                IngestionType = (RMDataIngestionType)entity.IngestionType,
                OperationType = (RMDataIngestionOperationType)entity.OperationType,
                Status = (RMDataIngestionMessageStatus)entity.Status,
                CreatedTime = entity.CreatedTime,
                Extension = entity.Extension
            };
        }

        #endregion

        #region Job Progress
        public static JMArchiverJobProgressDetails ConvertToProgressJobDetails(RMJobProgress tableEntity)
        {
            var gls = GeneralSettingService.GetGeneralSettingAsync().ExecuteAsyncTask();
            var result = new JMArchiverJobProgressDetails();
            result.SubJobID = tableEntity.SubJobID;
            result.Scope = tableEntity.Scope;
            result.JobType = (JobType)tableEntity.JobType;
            result.Status = (JobStatus)tableEntity.Status;
            result.SuccessfulCount = tableEntity.Successful;
            result.FailedCount = tableEntity.Failed;
            result.SkippedCount = tableEntity.Skipped;
            result.IsSavedJobDetails = tableEntity.IsSavedJobDetails;

            result.ProgressStatus = (ProgressStatus)tableEntity.ProgressStatus;
            result.StartTime = tableEntity.StartTime == 0 ? DateTime.MinValue : new DateTime(tableEntity.StartTime);
            result.StartTimeStr = tableEntity.StartTime == 0 ? string.Empty : GeneralSettingService.ConvertTiksToDateTime(gls, tableEntity.StartTime, true).SimplifyFormatTime;
            result.FinishTime = tableEntity.FinishTime == 0 ? DateTime.MinValue : new DateTime(tableEntity.FinishTime);
            result.FinishTimeStr = tableEntity.FinishTime == 0 ? string.Empty : GeneralSettingService.ConvertTiksToDateTime(gls, tableEntity.FinishTime, true).SimplifyFormatTime;

            result.TotalFiles = tableEntity.TotalFiles;

            result.TotalMatchedRuleFilesForExport = tableEntity.TotalMatchedRuleFilesForExport;
            result.TotalMatchedRuleFilesForArchive = tableEntity.TotalMatchedRuleFilesForArchive;
            result.TotalMatchedRuleFilesForOtherActions = tableEntity.TotalMatchedRuleFilesForOtherActions;

            var processedItemsInfoList = JsonConvert.DeserializeObject<List<ProcessedItemsInfoDto>>(tableEntity.ProcessedItemsInfos);
            if (processedItemsInfoList is not null && processedItemsInfoList.Count > 0)
            {
                result.ProcessedScannedItemsInfo = processedItemsInfoList.FirstOrDefault(x => x.Action == ActionTab.Scan) ?? new ProcessedItemsInfoDto { Action = ActionTab.Scan };
                result.ProcessedExportedItemsInfo = processedItemsInfoList.FirstOrDefault(x => x.Action == ActionTab.Export) ?? new ProcessedItemsInfoDto { Action = ActionTab.Export };
                result.ProcessedArchivedItemsInfo = processedItemsInfoList.FirstOrDefault(x => x.Action == ActionTab.Backup) ?? new ProcessedItemsInfoDto { Action = ActionTab.Backup };
                result.ProcessedOtherItemsInfo = processedItemsInfoList.FirstOrDefault(x => x.Action == ActionTab.Action) ?? new ProcessedItemsInfoDto { Action = ActionTab.Action };
            }

            result.StartScanTime = tableEntity.StartScanTime == 0 ? DateTime.MinValue : new DateTime(tableEntity.StartScanTime);
            result.EstimatedScanFinishedTime = tableEntity.EstimatedScanFinishedTime == 0 ? DateTime.MinValue : new DateTime(tableEntity.EstimatedScanFinishedTime);
            result.EstimatedScanFinishedTimeStr = result.EstimatedScanFinishedTime == DateTime.MinValue ? string.Empty : GeneralSettingService.ConvertTiksToDateTime(gls, result.EstimatedScanFinishedTime.Ticks, true).SimplifyFormatTime;

            result.StartExportTime = tableEntity.StartExportTime == 0 ? DateTime.MinValue : new DateTime(tableEntity.StartExportTime);
            result.EstimatedExportFinishedTime = tableEntity.EstimatedExportFinishedTime == 0 ? DateTime.MinValue : new DateTime(tableEntity.EstimatedExportFinishedTime);
            result.EstimatedExportFinishedTimeStr = result.EstimatedExportFinishedTime == DateTime.MinValue ? string.Empty : GeneralSettingService.ConvertTiksToDateTime(gls, result.EstimatedExportFinishedTime.Ticks, true).SimplifyFormatTime;

            result.StartArchivedTime = tableEntity.StartArchivedTime == 0 ? DateTime.MinValue : new DateTime(tableEntity.StartArchivedTime);
            result.EstimatedArchivedFinishedTime = tableEntity.EstimatedArchivedFinishedTime == 0 ? DateTime.MinValue : new DateTime(tableEntity.EstimatedArchivedFinishedTime);
            result.EstimatedArchivedFinishedTimeStr = result.EstimatedArchivedFinishedTime == DateTime.MinValue ? string.Empty : GeneralSettingService.ConvertTiksToDateTime(gls, result.EstimatedArchivedFinishedTime.Ticks, true).SimplifyFormatTime;

            result.StartOtherTime = tableEntity.StartOtherTime == 0 ? DateTime.MinValue : new DateTime(tableEntity.StartOtherTime);
            result.EstimatedOtherFinishedTime = tableEntity.EstimatedOtherFinishedTime == 0 ? DateTime.MinValue : new DateTime(tableEntity.EstimatedOtherFinishedTime);
            result.EstimatedOtherFinishedTimeStr = result.EstimatedOtherFinishedTime == DateTime.MinValue ? string.Empty : GeneralSettingService.ConvertTiksToDateTime(gls, result.EstimatedOtherFinishedTime.Ticks, true).SimplifyFormatTime;

            result.LastUpdatedTime = tableEntity.LastUpdatedTime == 0 ? DateTime.MinValue : new DateTime(tableEntity.LastUpdatedTime);
            result.LastUpdatedTimeStr = result.LastUpdatedTime == DateTime.MinValue ? string.Empty : GeneralSettingService.ConvertTiksToDateTime(gls, result.LastUpdatedTime.Ticks, true).SimplifyFormatTime;

            result.Comment = string.Empty;
            if (!string.IsNullOrEmpty(tableEntity.Comment))
            {
                result.Comment = I18NEntity.GetString(tableEntity.Comment);
            }
            return result;
        }

        public static JMMainJobDetails ConvertToMainJobDetails(RMJobProgress tableEntity)
        {
            return new JMMainJobDetails
            {
                SubJobID = tableEntity.SubJobID,
                Status = (JobStatus)tableEntity.Status,
                Scope = tableEntity.Scope,
                SuccessfulCount = tableEntity.Successful,
                FailedCount = tableEntity.Failed,
                SkippedCount = tableEntity.Skipped,
            };
        }

        public static RMJobProgress ConvertToJobProgressTableEntity(JMArchiverJobProgressDetails progressDetails)
        {
            return new RMJobProgress
            {
                SubJobID = progressDetails.SubJobID,
                Status = (int)progressDetails.Status,
                ProgressStatus = (int)progressDetails.ProgressStatus,

                JobType = (int)progressDetails.JobType,
                Scope = string.IsNullOrEmpty(progressDetails.Scope) ? string.Empty : progressDetails.Scope,
                Comment = progressDetails.Comment,
                IsSavedJobDetails = progressDetails.IsSavedJobDetails,

                Successful = progressDetails.SuccessfulCount,
                Failed = progressDetails.FailedCount,
                Skipped = progressDetails.SkippedCount,

                StartTime = progressDetails.StartTime.Ticks,
                FinishTime = progressDetails.FinishTime.Ticks,
                LastUpdatedTime = progressDetails.LastUpdatedTime.Ticks,

                TotalFiles = progressDetails.TotalFiles,

                TotalMatchedRuleFilesForExport = progressDetails.TotalMatchedRuleFilesForExport,
                TotalMatchedRuleFilesForArchive = progressDetails.TotalMatchedRuleFilesForArchive,
                TotalMatchedRuleFilesForOtherActions = progressDetails.TotalMatchedRuleFilesForOtherActions,

                ProcessedItemsInfos = JsonConvert.SerializeObject(new List<ProcessedItemsInfoDto>
                {
                    progressDetails.ProcessedScannedItemsInfo,
                    progressDetails.ProcessedExportedItemsInfo,
                    progressDetails.ProcessedArchivedItemsInfo,
                    progressDetails.ProcessedOtherItemsInfo
                }),

                StartScanTime = progressDetails.StartScanTime.Ticks,
                EstimatedScanFinishedTime = progressDetails.EstimatedScanFinishedTime.Ticks,

                StartExportTime = progressDetails.StartExportTime.Ticks,
                EstimatedExportFinishedTime = progressDetails.EstimatedExportFinishedTime.Ticks,

                StartArchivedTime = progressDetails.StartArchivedTime.Ticks,
                EstimatedArchivedFinishedTime = progressDetails.EstimatedArchivedFinishedTime.Ticks,

                StartOtherTime = progressDetails.StartOtherTime.Ticks,
                EstimatedOtherFinishedTime = progressDetails.EstimatedOtherFinishedTime.Ticks
            };
        }
        #endregion
    }

    public class Snowflake
    {
        private static long machineId;//机器ID
        private static long datacenterId = 0L;//数据ID
        private static long sequence = 0L;//计数从零开始

        private static long twepoch = 687888001020L; //唯一时间随机量

        private static long machineIdBits = 5L; //机器码字节数
        private static long datacenterIdBits = 5L;//数据字节数
        public static long maxMachineId = -1L ^ -1L << (int)machineIdBits; //最大机器ID

        private static long sequenceBits = 12L; //计数器字节数，12个字节用来保存计数码        
        private static long machineIdShift = sequenceBits; //机器码数据左移位数，就是后面计数器占用的位数
        private static long datacenterIdShift = sequenceBits + machineIdBits;
        private static long timestampLeftShift = sequenceBits + machineIdBits + datacenterIdBits; //时间戳左移动位数就是机器码+计数器总字节数+数据字节数
        public static long sequenceMask = -1L ^ -1L << (int)sequenceBits; //一微秒内可以产生计数，如果达到该值则等到下一微妙在进行生成
        private static long lastTimestamp = -1L;//最后时间戳

        private readonly static object syncRoot = new object();//加锁对象
        static Snowflake snowflake;

        public static Snowflake Instance()
        {
            if (snowflake == null)
                snowflake = new Snowflake();
            return snowflake;
        }

        public Snowflake()
        {
            //Snowflakes(0L, -1);
        }

        /*private void Snowflakes(long machineId, long datacenterId)
        {
            if (machineId >= 0)
            {
                if (machineId > maxMachineId)
                {
                    throw new Exception("the machineId is invalid");
                }
                Snowflake.machineId = machineId;
            }
            if (datacenterId >= 0)
            {
                if (datacenterId > maxDatacenterId)
                {
                    throw new Exception("the datacenterId is invalid");
                }
                Snowflake.datacenterId = datacenterId;
            }
        }*/

        /// <summary>
        /// 生成当前时间戳
        /// </summary>
        /// <returns>毫秒</returns>
        private static long GetTimestamp()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        /// <summary>
        /// 获取下一微秒时间戳
        /// </summary>
        /// <param name="lastTimestamp"></param>
        /// <returns></returns>
        private static long GetNextTimestamp(long lastTimestamp)
        {
            long timestamp = GetTimestamp();
            if (timestamp <= lastTimestamp)
            { 
                timestamp = GetTimestamp();
            }
            return timestamp;
        }

        /// <summary>
        /// 获取长整形的ID
        /// </summary>
        /// <returns></returns>
        public long GetId()
        {
            lock (syncRoot)
            {
                long timestamp = GetTimestamp();
                if (Snowflake.lastTimestamp == timestamp)
                { //同一微秒中生成ID
                    sequence = (sequence + 1) & sequenceMask; //用&运算计算该微秒内产生的计数是否已经到达上限
                    if (sequence == 0)
                    {
                        //一微秒内产生的ID计数已达上限，等待下一微秒
                        timestamp = GetNextTimestamp(Snowflake.lastTimestamp);
                    }
                }
                else
                {
                    //不同微秒生成ID
                    sequence = 0L;
                }
                if (timestamp < lastTimestamp)
                {
                    throw new Exception("the timestamp less than lastTimestamp, error");
                }
                Snowflake.lastTimestamp = timestamp; //把当前时间戳保存为最后生成ID的时间戳
                long Id = ((timestamp - twepoch) << (int)timestampLeftShift)
                    | (datacenterId << (int)datacenterIdShift)
                    | (machineId << (int)machineIdShift)
                    | sequence;
                return Id;
            }
        }


        public long GetTicks()
        {
            long nowTicks = 0L;
            bool flag = false;
            do
            {
                nowTicks = DateTime.UtcNow.Ticks;
                if (nowTicks > lastTimestamp)
                {
                    lastTimestamp = nowTicks;
                    flag = true;
                }
            } while (!flag);

            return nowTicks;
        }
    }
}
