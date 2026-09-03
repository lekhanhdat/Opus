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
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Explorer.AuditHandler
{
    public class ExplorerAfterAuditHandler : IAfterAuditHandler
    {
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
        public IRecordsHistoryService RecordsHistoryService => PlatformWindsorManager.GetService<IRecordsHistoryService>();
        public ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        public ITenantService tenantService => PlatformWindsorManager.GetService<ITenantService>();
        private RALogger logger = RALogger.GetInstance(typeof(ExplorerAfterAuditHandler));

        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            try
            {
                //RecordHistoryXml xml = new RecordHistoryXml()
                //{
                //    HistoryList = new List<RecordHistory>()
                //};

                if (action == (int)AuditAction.ChangeTerm || action == (int)AuditAction.ChangeLabel)
                {
                    var result = returnValue as RecordsReturnMessage;
                    if (result.ResultType == ResultType.Failed)
                    {
                        info.Status = (int)RAMessageType.Failed;
                    }

                    var changeTermOption = args[0] as ChangeTermOption;

                    switch (changeTermOption?.ChangeTermOrigin)
                    {
                        case ChangeTermOrigin.Manual:
                            info.Category = AuditCategory.ManualApprovalTimer;
                            break;
                        case ChangeTermOrigin.Search:
                            info.Category = AuditCategory.Explorer;
                            break;
                        default:
                            info.Category = AuditCategory.ManualApprovalTimer;
                            break;
                    }
                    //info.Object = args[1] as string;
                }
                else if (action == (int)AuditAction.RunGlobalSearchActionJob)
                {
                    var jobId = returnValue as string;
                    var param = args[0] as string;
                    var globalSearchDto = SerializerHelper.DeserializeByDataContractSerializer<GlobalSearchActionDto>(param);
                    switch (globalSearchDto?.ChangeTermOrigin)
                    {
                        case ChangeTermOrigin.Manual:
                            info.Category = AuditCategory.ManualApprovalTimer;
                            break;
                        case ChangeTermOrigin.Search:
                        default:
                            info.Category = AuditCategory.Explorer;
                            break;
                    }
                    info.Module = AuditModule.BusinessClassificationManagement;
                    //var node = ((RMSPTreeNode)args[0]);
                    info.Object = jobId;
                }
                else if (action == (int)AuditAction.ExportSearchResult)
                {
                    var exportPath = returnValue as string;
                    info.Category = AuditCategory.Explorer;
                    info.Module = AuditModule.BusinessClassificationManagement;
                    //var node = ((RMSPTreeNode)args[0]);
                    info.Status = !string.IsNullOrWhiteSpace(exportPath) ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                }
                else if (action == (int)AuditAction.RunExportSearchResultJob)
                {
                    var jobId = returnValue as string;
                    info.Category = AuditCategory.Explorer;
                    info.Module = AuditModule.BusinessClassificationManagement;
                    //var node = ((RMSPTreeNode)args[0]);
                    info.Object = jobId;
                }
                else if (action == (int)AuditAction.ExportHoldRecords)
                {
                    var jobId = returnValue as string;
                    info.Category = AuditCategory.ManageHold;
                    info.Module = AuditModule.PhysicalRecordManagement;
                    info.Object = jobId;
                }
                else if (action == (int)AuditAction.ImportHoldRecords)
                {
                    var jobId = returnValue as string;
                    info.Category = AuditCategory.ManageHold;
                    info.Module = AuditModule.PhysicalRecordManagement;
                    info.Object = jobId;

                }
                else if (action == (int)AuditAction.RunFSReclassicfyJob || action == (int)AuditAction.RunFSManageHoldJob)
                {
                    var jobId = returnValue as string;
                    info.Category = AuditCategory.Explorer;
                    info.Module = AuditModule.BusinessClassificationManagement;
                    //var node = ((RMSPTreeNode)args[0]);
                    info.Object = jobId;
                }
                else if (info.Action == AuditAction.ManageRelatedRecords || info.Action == AuditAction.FSManageRelatedRecords)
                {
                    var result = returnValue as RAReturnMessage;
                    //info.Status = (int)result.MessageType;

                    if (!string.IsNullOrEmpty(result.Extension) && Convert.ToInt32(result.Extension) == (int)Contract.Explorer.SourceFlag.Physical)
                    {
                        info.Module = AuditModule.PhysicalRecordManagement;
                        info.Category = AuditCategory.PhysicalRecordsExplorer;
                        info.Action = AuditAction.PhysicalManageRelatedRecords;
                    }
                    info.Status = result.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                    if (result.MessageType == RAMessageType.Successful)
                    {
                        Guid currentId = Guid.Parse(args[0].ToString());
                        List<Guid> addIds = args[4] as List<Guid>;
                        List<Guid> delIds = args[2] as List<Guid>;
                        var idNameDict = args[3] as Dictionary<Guid, string>;
                        addIds ??= new List<Guid>();
                        delIds ??= new List<Guid>();

                        Func<List<Guid>, string> resolveNames = ids =>
                        {
                            if (ids == null || ids.Count == 0)
                            {
                                return string.Empty;
                            }

                            if (idNameDict != null && idNameDict.Count > 0)
                            {
                                return string.Join(",", ids.Select(id =>
                                    idNameDict.TryGetValue(id, out var name) && !string.IsNullOrWhiteSpace(name)
                                        ? name
                                        : ExplorerDao.GetRecordByIds(new List<Guid> { id }).FirstOrDefault()?.LeafName ?? id.ToString()));
                            }

                            return string.Join(",", ExplorerDao.GetRecordByIds(ids).Select(s => s.LeafName));
                        };

                        var addNames = resolveNames(addIds);
                        var delNames = resolveNames(delIds);
                        //var currRecord = CollectionDataDao.GetRecordByIds(new List<int>() { currentId }).FirstOrDefault();
                        var historyAction = string.Empty;
                        if (addIds.Count > 0)
                        {
                            if (delIds.Count > 0)
                            {
                                historyAction = I18NEntity.GetString("RM_BCM_History_AddRelatedRecords") + " " + addNames
                                    + "; " + I18NEntity.GetString("RM_BCM_History_DeleteRelatedRecords") + " " + delNames;
                            }
                            else
                            {
                                historyAction = I18NEntity.GetString("RM_BCM_History_AddRelatedRecords") + " " + addNames;
                            }
                        }
                        else
                        {
                            if (delIds.Count > 0)
                            {
                                historyAction = I18NEntity.GetString("RM_BCM_History_DeleteRelatedRecords") + " " + delNames;
                            }
                        }
                        //xml.HistoryList.Add(new RecordHistory()
                        //{
                        //    Action = historyAction,
                        //    TimeUTC = DateTime.UtcNow.Ticks,
                        //    User = userName
                        //});
                        if (addIds.Count > 0 || delIds.Count > 0)
                        {
                            //ExplorerDao.AddReocrdHistory(new List<Guid>() { currentId }, xml);
                            AddRecordsHistory(new List<Guid>() { currentId }, historyAction);
                        }

                        //TODO 被添加的item history
                    }
                }
                else if (info.Action == AuditAction.SpfxManageRelatedRecords)
                {
                    RAReturnMessage returnMessage = (RAReturnMessage)returnValue;
                    if (returnMessage.MessageType == RAMessageType.Successful)
                    {
                        var changeInfos = returnMessage.Extsion1 as (string, List<RMRelatedItemInfo>, List<RMRelatedItemInfo>)?;
                        if (changeInfos.HasValue)
                        {
                            var (fileName, orgins, news) = changeInfos.Value;

                            info.Object = fileName;
                            info.ModifyContent.Add(new AuditItem()
                            {
                                TargetSetting = "RM_JS_MA_Grid_RelatedRecords",
                                OldValue = string.Join(",", orgins.Select(s => s.SourceFlag == (int)SourceFlag.Physical ? s.recId : s.name)),
                                NewValue = string.Join(",", news.Select(s => s.SourceFlag == (int)SourceFlag.Physical ? s.recId : s.name))
                            });
                        }
                    }
                    else
                    {
                        info.NotNeedRecordAudit = true;
                    }
                }
                else if (info.Action == AuditAction.DeleteArchivedContent)
                {
                    var result = returnValue as RAReturnMessage;
                    info.Category = AuditCategory.DownloadCenter;
                    info.Module = AuditModule.DownloadCenter;
                    //var node = ((RMSPTreeNode)args[0]);
                    info.Status = result.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                    info.Action = AuditAction.DeleteArchivedContent;
                    if (!string.IsNullOrWhiteSpace(result.Extension))
                    {
                        var names = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(result.Extension);
                        info.Object = string.Join(",", names);
                    }
                }
                else if (info.Action == AuditAction.StartDownloadArchivedContentJob)
                {
                    var result = returnValue as RAReturnMessage;
                    info.Category = AuditCategory.DownloadCenter;
                    info.Module = AuditModule.DownloadCenter;
                    //var node = ((RMSPTreeNode)args[0]);
                    info.Status = result.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                    info.Action = AuditAction.StartDownloadArchivedContentJob;
                    info.Object = result.Extension;
                }
                else if (info.Action == AuditAction.DeclareAsRecord || info.Action == AuditAction.UndeclareAsRecord || info.Action == AuditAction.DeclareSPOOnPreAsRecord || info.Action == AuditAction.UndeclareSPOOnPreAsRecord)
                {
                    var isSupportRecordLabel = !DataCenterUtil.Is21V() && IsNewOpusTenant() && info.Action != AuditAction.DeclareSPOOnPreAsRecord && info.Action != AuditAction.UndeclareSPOOnPreAsRecord;
                    List<Guid> ids = args[0] as List<Guid>;
                    bool isDeclared = info.Action == AuditAction.DeclareAsRecord || info.Action == AuditAction.DeclareSPOOnPreAsRecord;
                    var result = returnValue as RecordsReturnMessage;
                    if (result == null)
                    {
                        var res = ((RecordsReturnMessage, string))returnValue;
                        result = res.Item1 as RecordsReturnMessage;
                    }
                    info.Status = result.ResultType == ResultType.Success ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                    if (result.ResultType == ResultType.Success)
                    {
                        //xml.HistoryList.Add(new RecordHistory()
                        //{
                        //    Action = isDeclared ? "RM_BCM_History_DeclareAsRecord" : "RM_BCM_History_UndeclareAsRecord",
                        //    TimeUTC = DateTime.UtcNow.Ticks,
                        //    User = userName
                        //});
                        //ExplorerDao.AddReocrdHistory(ids, xml);
                        AddRecordsHistory(ids, isSupportRecordLabel ? (isDeclared ? "RM_BCM_History_AddRecordLabel" : "RM_BCM_History_RemoveRecordLabel") : (isDeclared ? "RM_BCM_History_DeclareAsRecord" : "RM_BCM_History_UndeclareAsRecord"));
                    }
                    if (isSupportRecordLabel)
                    {
                        info.Action = isDeclared ? AuditAction.AddRecordLabel : AuditAction.RemoveRecordLabel;
                    }
                    if (info.Action == AuditAction.DeclareSPOOnPreAsRecord) info.Action = AuditAction.DeclareAsRecord;
                    if (info.Action == AuditAction.UndeclareSPOOnPreAsRecord) info.Action = AuditAction.UndeclareSPOOnPreAsRecord;
                }
                else if (info.Action == AuditAction.CreateHoldTypeWithRecord || info.Action == AuditAction.CreateAppendHoldTypeWithRecord)
                {
                    UpdateHoldDto dto = args[0] as UpdateHoldDto;
                    List<Guid> ids = dto.ReletedIds;
                    var result = returnValue as RAReturnMessage;
                    //info.Status = (int)result.MessageType;
                    info.Status = result.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                    if (result.MessageType == RAMessageType.Successful)
                    {
                        AddRecordsHistory(ids, dto.HoldAction == RecordsConstants.HOLD_ACTION_APPEND ? "RM_BCM_Audit_Action_CreateAppendHoldTypeWithRecord" : "RM_BCM_Audit_Action_CreateHoldTypeWithRecord");
                    }
                    //xml.HistoryList.Add(new RecordHistory()
                    //{
                    //    Action = "RM_BCM_Audit_Action_CreateHoldTypeWithRecord",
                    //    TimeUTC = DateTime.UtcNow.Ticks,
                    //    User = userName
                    //});
                    //ExplorerDao.AddReocrdHistory(ids, xml);
                }
                else if (info.Action == AuditAction.EditHold)
                {
                    UpdateHoldDto dto = args[0] as UpdateHoldDto;
                    List<Guid> ids = dto.ReletedIds;
                    var result = returnValue as RAReturnMessage;
                    info.Status = result.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;

                    if (info.ModifyContent != null && info.ModifyContent.Count != 0)
                    {
                        AuditItem dateItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_JS_BCM_Explorer_Details_HoldUntil")).FirstOrDefault();
                        if(dateItem != null)
                        {
                            if (dto.HoldSetting.Type == 0)
                            {
                                dateItem.NewValue = "RM_BCM_Audit_Action_EnterDuration" + " ";
                                dateItem.NewValue += dto.HoldSetting.Number.ToString() + " ";
                                dateItem.NewValue += dto.HoldSetting.Unit.ToString();
                            }
                            else
                            {
                                DateTime calendarTime = DateTime.Parse(dto.HoldSetting.CalenderTime);
                                string time = calendarTime.ToString(JSDateTimeFormat.DEFAULT_TIME_FORMAT);
                                dateItem.NewValue = "RM_BCM_Audit_Action_Calender" + " " + time;
                            }
                        }

                        AuditItem commentItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(I18NEntity.GetString("RM_JM_Comment"))).FirstOrDefault();
                        if (commentItem != null)
                        {
                            commentItem.NewValue = dto.HoldSetting.Description;
                        }
                        AuditItem emailNotificationAudit = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(I18NEntity.GetString("RM_MA_Setting_Email_Notification"))).FirstOrDefault();
                        if (emailNotificationAudit != null)
                        {
                            var emailNotification = dto.HoldSetting.EmailNotification;
                            if (dto.HoldSetting.Type == HoldDateType.Calendar && dto.HoldSetting.EmailNotification == null)
                            {
                                emailNotification = new HoldEmailNotification
                                {
                                    IsEnabled = false
                                };
                            }
                            emailNotificationAudit.NewValue = ExplorerAuditUtil.GetEmailNotificationInfo(emailNotification);
                        }

                    }
                    if (result.MessageType == RAMessageType.Successful)
                    {
                        AddRecordsHistory(ids, "RM_BCM_Audit_Action_EditHold");
                    }
                    //xml.HistoryList.Add(new RecordHistory()
                    //{
                    //    Action = "RM_BCM_Audit_Action_EditHold",
                    //    TimeUTC = DateTime.UtcNow.Ticks,
                    //    User = userName
                    //});
                    //ExplorerDao.AddReocrdHistory(ids, xml);

                }
                else if (info.Action == AuditAction.ReuseHoldTypeWithRecord || info.Action == AuditAction.ReuseAppendHoldTypeWithRecord)
                {
                    UpdateHoldDto dto = args[0] as UpdateHoldDto;
                    List<Guid> ids = dto.ReletedIds;
                    var result = returnValue as RAReturnMessage;
                    //info.Status = (int)result.MessageType;
                    info.Status = result.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                    if (result.MessageType == RAMessageType.Successful)
                    {
                        AddRecordsHistory(ids, dto.HoldAction == RecordsConstants.HOLD_ACTION_APPEND ? "RM_BCM_Audit_Action_ReuseAppendHoldTypeWithRecord" : "RM_BCM_Audit_Action_ReuseHoldTypeWithRecord");
                    }
                    //xml.HistoryList.Add(new RecordHistory()
                    //{
                    //    Action = "RM_BCM_Audit_Action_ReuseHoldTypeWithRecord",
                    //    TimeUTC = DateTime.UtcNow.Ticks,
                    //    User = userName
                    //});
                    //ExplorerDao.AddReocrdHistory(ids, xml);
                }
                else if (info.Action == AuditAction.ChangeHoldCreate)
                {
                    UpdateHoldDto dto = args[0] as UpdateHoldDto;
                    List<Guid> ids = dto.ReletedIds;
                    var result = returnValue as RAReturnMessage;
                    //info.Status = (int)result.MessageType;
                    info.Status = result.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                    if (result.MessageType == RAMessageType.Successful)
                    {
                        AddRecordsHistory(ids, "RM_BCM_Audit_Action_ChangeHoldCreate");
                    }
                    //xml.HistoryList.Add(new RecordHistory()
                    //{
                    //    Action = "RM_BCM_Audit_Action_ChangeHoldCreate",
                    //    TimeUTC = DateTime.UtcNow.Ticks,
                    //    User = userName
                    //});
                    //ExplorerDao.AddReocrdHistory(ids, xml);

                }
                else if (info.Action == AuditAction.ChangeHoldReuse)
                {
                    UpdateHoldDto dto = args[0] as UpdateHoldDto;
                    List<Guid> ids = dto.ReletedIds;
                    var result = returnValue as RAReturnMessage;
                    //info.Status = (int)result.MessageType;
                    info.Status = result.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                    if (result.MessageType == RAMessageType.Successful)
                    {
                        AddRecordsHistory(ids, "RM_BCM_Audit_Action_ChangeHoldReuse");
                    }
                    //xml.HistoryList.Add(new RecordHistory()
                    //{
                    //    Action = "RM_BCM_Audit_Action_ChangeHoldReuse",
                    //    TimeUTC = DateTime.UtcNow.Ticks,
                    //    User = userName
                    //});
                    //ExplorerDao.AddReocrdHistory(ids, xml);

                }
                else if (info.Action == AuditAction.CancelHoldByRecords || info.Action == AuditAction.RemovePersonalHold || info.Action == AuditAction.MobileReturn)
                {
                    List<Guid> ids = args[0] as List<Guid>;
                    var result = returnValue as RAReturnMessage;
                    //info.Status = (int)result.MessageType;
                    bool startJob = false;
                    info.Status = result.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                    if (result.MessageType == RAMessageType.Successful)
                    {
                        if (bool.TryParse(result.Extension, out startJob))
                        {
                            info.NotNeedRecordAudit = true;
                        }
                        AddRecordsHistory(ids, "RM_BCM_Audit_Action_CancelHoldByRecords");
                    }
                    //xml.HistoryList.Add(new RecordHistory()
                    //{
                    //    Action = "RM_BCM_Audit_Action_CancelHoldByRecords",
                    //    TimeUTC = DateTime.UtcNow.Ticks,
                    //    User = userName
                    //});
                    //ExplorerDao.AddReocrdHistory(ids, xml);

                }
                else if (info.Action == AuditAction.SusPendRecords)
                {
                    UpdateHoldDto dto = args[0] as UpdateHoldDto;
                    List<Guid> ids = dto.ReletedIds;
                    var result = returnValue as RAReturnMessage;
                    //info.Status = (int)result.MessageType;
                    info.Status = result.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                    if (result.MessageType == RAMessageType.Successful)
                    {
                       AddRecordsHistory(ids, "RM_BCM_Audit_Action_SusPendRecords");
                    }
                    //xml.HistoryList.Add(new RecordHistory()
                    //{
                    //    Action = "RM_BCM_Audit_Action_SusPendRecords",
                    //    TimeUTC = DateTime.UtcNow.Ticks,
                    //    User = userName
                    //});
                    //ExplorerDao.AddReocrdHistory(ids, xml);

                }
                else if (info.Action == AuditAction.CreateHold)
                {
                    var result = returnValue as RAReturnMessage;
                    //info.Status = (int)result.MessageType;
                    info.Status = result.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                    UpdateHoldDto dto = args[0] as UpdateHoldDto;
                    if (info.ModifyContent == null) { info.ModifyContent = new List<AuditItem>(); }

                    AuditItem dateItem = new AuditItem();
                    dateItem.TargetSetting = "RM_JS_BCM_Explorer_Details_HoldUntil";
                    if (dto.HoldSetting.Type == 0)
                    {
                        dateItem.NewValue = "RM_BCM_Audit_Action_EnterDuration" + " ";
                        dateItem.NewValue += dto.HoldSetting.Number.ToString() + " ";
                        dateItem.NewValue += dto.HoldSetting.Unit.ToString();
                    }
                    else
                    {
                        DateTime calendarTime = DateTime.Parse(dto.HoldSetting.CalenderTime);
                        string time = calendarTime.ToString(JSDateTimeFormat.DEFAULT_TIME_FORMAT);
                        dateItem.NewValue = "RM_BCM_Audit_Action_Calender" + " " + time;
                    }
                    info.ModifyContent.Add(dateItem);


                    AuditItem commentItem = new AuditItem();
                    commentItem.TargetSetting = "RM_JM_Comment";
                    commentItem.NewValue = dto.HoldSetting.Description;
                    info.ModifyContent.Add(commentItem);

                    var emailNotification = dto.HoldSetting.EmailNotification;
                    if (dto.HoldSetting.Type == HoldDateType.Calendar && dto.HoldSetting.EmailNotification == null)
                    {
                        emailNotification = new HoldEmailNotification
                        {
                            IsEnabled = false
                        };
                    }
                    AuditItem emailNotificationAudit = new AuditItem();
                    emailNotificationAudit.TargetSetting = "RM_MA_Setting_Email_Notification";
                    emailNotificationAudit.NewValue = ExplorerAuditUtil.GetEmailNotificationInfo(emailNotification);
                    info.ModifyContent.Add(emailNotificationAudit);
                }
                else if (info.Action == AuditAction.CancelHold)
                {
                    List<string> holdIds = args[0] as List<string>;
                    //List<int> ids = new List<int>();
                    //var records = CollectionDataDao.GetRecordByHoldIds(holdIds);
                    //foreach (var rec in records)
                    //{
                    //    ids.Add(rec.Id);
                    //}
                    var result = returnValue as RAReturnMessage;
                    //info.Status = (int)result.MessageType;
                    info.Status = result.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                    //xml.HistoryList.Add(new RecordHistory()
                    //{
                    //    Action = I18NEntity.GetString("Cancel Holds"),
                    //    TimeUTC = DateTime.UtcNow.Ticks,
                    //    User = userName
                    //});
                    //ExplorerDao.AddReocrdHistory(ids, xml);
                }
                else if (info.Action == AuditAction.SuspendHold)
                {
                    UpdateHoldDto dto = args[0] as UpdateHoldDto;
                    List<string> holdIds = dto.HoldIds;
                    List<Guid> ids = new List<Guid>();
                    var records = ExplorerDao.GetRecordbyHoldIds(holdIds);
                    foreach (var item in records)
                    {
                        ids.Add(item.Id);
                    }
                    var result = returnValue as RAReturnMessage;
                    info.Status = result.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                    if (result.MessageType == RAMessageType.Successful)
                    {
                        AddRecordsHistory(ids, "RM_BCM_Audit_Action_SuspendHold");
                    }
                    //xml.HistoryList.Add(new RecordHistory()
                    //{
                    //    Action = "RM_BCM_Audit_Action_SuspendHold",
                    //    TimeUTC = DateTime.UtcNow.Ticks,
                    //    User = userName
                    //});
                    //ExplorerDao.AddReocrdHistory(ids, xml);

                }
                else if (info.Action == AuditAction.DeleteHold)
                {
                    List<string> holdIds = args[0] as List<string>;
                    //List<int> ids = new List<int>();
                    //var records = CollectionDataDao.GetRecordByHoldIds(holdIds);
                    //foreach (var rec in records)
                    //{
                    //    ids.Add(rec.Id);
                    //}
                    var result = returnValue as RAReturnMessage;
                    //info.Status = (int)result.MessageType;
                    info.Status = result.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                    //xml.HistoryList.Add(new RecordHistory()
                    //{
                    //    Action = I18NEntity.GetString("Delete Holds"),
                    //    TimeUTC = DateTime.UtcNow.Ticks,
                    //    User = userName
                    //});
                    //ExplorerDao.AddReocrdHistory(ids, xml);
                }
                else if (info.Action == AuditAction.ExplorerRecordsMove)
                {
                    RMExplorerMoveJobMessage msg = GCommon.Utility.SerializerHelper.DeserializeFromXmlString<RMExplorerMoveJobMessage>(args[1].ToString());
                    var jobId = returnValue as string;
                    info.Object = jobId;
                    var location = string.Empty;
                    if (msg.MoveDestination.DestMode == Contract.Explorer.DestMode.TreeMode)
                    {
                        location = WebUtil.MakeFullUrl(msg.MoveDestination.RootSiteUrl, msg.MoveDestination.SPTreeNode.FullPath);
                    }
                    else
                    {
                        location = msg.MoveDestination.SPUrl;
                    }
                    var nameConflictOption = string.Empty;
                    switch (msg.MoveSetting.ItemLevelConflictOption)
                    {
                        case ConflictOption.Skip:
                            nameConflictOption = "RM_JS_BCM_Explorer_Move_FileConflictOption_Skip";
                            break;
                        case ConflictOption.AppendByName:
                            nameConflictOption = "RM_JS_BCM_Explorer_Move_FileConflictOption_Rename";
                            break;
                        case ConflictOption.Overwrite:
                            nameConflictOption = "RM_JS_BCM_Explorer_Move_FileConflictOption_Overwrite";
                            break;
                        default:
                            break;
                    }

                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_BCM_Audit_MoveToDestination",
                        NewValue = location
                    });

                    info.ModifyContent.Add(new AuditItem()
                    {
                        TargetSetting = "RM_BCM_Audit_NameConflictOption",
                        NewValue = nameConflictOption
                    });
                }
                else if (info.Action == AuditAction.MoveCheckSPUrl || info.Action == AuditAction.RuleCheckSPUrl
                    || info.Action == AuditAction.MoveCheckFSUNCLocation || info.Action == AuditAction.RuleCheckFSUNCLocation)
                {
                    var checkObj = returnValue as CheckLocationObject;
                    if (checkObj == null || string.IsNullOrEmpty(checkObj.DestRootPath) || Guid.Empty == checkObj.AveSiteId)
                    {
                        info.Status = (int)AuditStatus.Failed;
                    }
                }
                else if (info.Action == AuditAction.PhysicalExplorerMove || info.Action == AuditAction.MobileMove)
                {
                    var result = returnValue as RecordsReturnMessage;
                    if (result.ResultType == ResultType.Failed)
                    {
                        var moveDto = args[0] as PhysicalMoveOption;
                        List<Record> sourceRecords = null;
                        if (result.FailedIds != null && result.FailedIds.Count > 0)
                        {

                            var failedIds = result.FailedIds;
                            sourceRecords = ExplorerDao.QueryAll(r => failedIds.Contains(r.Id)).ToList();
                        }
                        else if (!string.IsNullOrEmpty(moveDto.FolderId))
                        {
                            sourceRecords = ExplorerDao.QueryAll(
                                 r => moveDto.SourcePhyRecordIds.Contains(r.Id) &&
                                 r.FileId != new Guid(moveDto.FolderId)).ToList();
                        }
                        if (sourceRecords != null && sourceRecords.Count > 0)
                        {
                            info.Object = string.Join("; ", sourceRecords.Select(r => r.LeafName));
                        }
                        info.Status = (int)RAMessageType.Failed;
                    }
                }
                else if (info.Action == AuditAction.MobileChangeStatus)
                {
                    var result = returnValue as RAReturnMessage;
                    info.Status = result == null ? (int)AuditStatus.Failed : result.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                }
                else if (info.Action == AuditAction.DownLoadPhysicalExportBarcodeReport)
                {
                    ExportBarcodeDto barcodeInfo = args[0] as ExportBarcodeDto;
                    var result = returnValue as ExportResultDto;
                    if (!string.IsNullOrEmpty(result.FileName))
                    {
                        info.Status = (int)AuditStatus.Successful;
                    }
                    else
                    {
                        info.Status = (int)AuditStatus.Failed;
                    }
                    info.Object = barcodeInfo.FullPath;
                }
                else if ((AuditAction)action == AuditAction.RunPhysicalExportBarcodeJob)
                {
                    string reValue = Convert.ToString(returnValue);
                    if (string.IsNullOrEmpty(reValue))
                    {
                        info.Status = (int)AuditStatus.Failed;
                    }
                    else
                    {
                        info.Status = (int)AuditStatus.Successful;
                    }
                    info.Object = reValue;
                }
                else if ((AuditAction)action == AuditAction.SavelocationPermission)
                {
                    var result = returnValue as RAReturnMessage;
                    info.Status = result.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                }
                else if ((AuditAction)action == AuditAction.RunPhysicalSetPermissionJob)
                {
                    string reValue = Convert.ToString(returnValue);
                    if (string.IsNullOrEmpty(reValue))
                    {
                        info.Status = (int)AuditStatus.Failed;
                    }
                    else
                    {
                        info.Status = (int)AuditStatus.Successful;
                    }
                    info.Object = reValue;
                }
                else if ((AuditAction)action == AuditAction.MLChangeTerm)
                {
                    ChangeTerm(info, args, returnValue);
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while collect explorer after audits,ERROR:{0}", ex.ToString());

            }
            return info;
        }

        private bool IsNewOpusTenant()
        {
            return tenantService.IsNewOpusTenant();
        }

        private void AddRecordsHistory(List<Guid> currentIds, string historyAction)
        {
            try
            {
                Task.Run(() =>
                {
                    RecordsHistoryService.AddRecordsHistory(currentIds, historyAction);
                });
            }
            catch(Exception e)
            {
                logger.Error("Error occurred while add record history,ERROR:{0}", e.ToString());
            }

        }

        private void ChangeTerm(RMAuditInfo info, object[] args, object returnValue)
        {
            string objName = string.Empty;
            ChangeTermType changeTermType = (ChangeTermType)args[0];
            info.Action = changeTermType switch
            {
                ChangeTermType.AIMAChangeTerm => AuditAction.MLChangeTerm,
                ChangeTermType.AIMADirectlyApprove => AuditAction.MLReviewApprove,
                _ => info.Action
            };
            var changeOpt = args[1] as ChangeTermOption;
            List<Guid> recIds = new List<Guid>();
            if (!changeOpt.SourceRecordIds.IsNullOrEmpty())
            {
                recIds.AddRange(changeOpt.SourceRecordIds);
            }
            if (!changeOpt.SourceOneDriveRecordIds.IsNullOrEmpty())
            {
                recIds.AddRange(changeOpt.SourceOneDriveRecordIds);
            }
            if (!changeOpt.GoogleDriveRecordIds.IsNullOrEmpty())
            {
                recIds.AddRange(changeOpt.GoogleDriveRecordIds);
            }


            var records = ExplorerDao.GetRecordByIds(recIds);

            var termsDic = TermDao.GetRMTermsByTermIds(records.Select(r => r.PredictTermId).ToList()).ToDictionary(t => t.UniqueId, t => t.Name);
            foreach (var rec in records)
            {
                info.ModifyContent.Add(new AuditItem()
                {
                    Id = rec.Id,
                    TargetSetting = "RM_JS_RC_RUR_TermName",
                    OldValue = (termsDic.ContainsKey(rec.PredictTermId) ? termsDic[rec.PredictTermId] : ""),
                    NewValue = changeOpt.TargetTermName,
                });
            }
            if ( records.Count() > 0 ) 
            {
                info.Object = records.Count() == 1 ? records[0].LeafName : string.Join("; ", records.Select(item => item.LeafName)) + "; ";
            }
            var result = returnValue as RecordsReturnMessage;
            if (result.ResultType == ResultType.Failed)
            {
                info.Status = (int)RAMessageType.Failed;
            }
        }
    }
}
