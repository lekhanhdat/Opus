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
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.ManualApproval.Queriers;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using Newtonsoft.Json;
using RAManualApproval.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Directory = System.IO.Directory;
using Workbook = DocumentFormat.OpenXml.Spreadsheet.Workbook;
using Worksheet = DocumentFormat.OpenXml.Spreadsheet.Worksheet;
using Cell = DocumentFormat.OpenXml.Spreadsheet.Cell;
using Column = DocumentFormat.OpenXml.Spreadsheet.Column;
using Row = DocumentFormat.OpenXml.Spreadsheet.Row;
using Text = DocumentFormat.OpenXml.Spreadsheet.Text;
using Columns = DocumentFormat.OpenXml.Spreadsheet.Columns;
using DataValidation = DocumentFormat.OpenXml.Spreadsheet.DataValidation;
using System.Xml;
using AvePoint.RA.Contract.CustomizeConnector.Enums;
using AvePoint.RA.Contract.Explorer;
using DocumentFormat.OpenXml.Vml.Office;
using DocumentFormat.OpenXml.Wordprocessing;
using OpenNLP.Tools.Util;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using AvePoint.RA.Contract.SharePoint.CustomIndexMetadata;

namespace RAManualApproval.ExportAction.UnderReview
{
    public class ManualUnderReviewExportProcessor : ManualApprovalExportProcess
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ManualUnderReviewExportProcessor));

        private static readonly IGeneralSettingService GeneralSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();

        private static readonly IRMSharePointSettingsService s_spSettingsService = PlatformWindsorManager.GetService<IRMSharePointSettingsService>();

        private static readonly GeneralSettingModel GeneralSetting = GeneralSettingService.GetGeneralSettingAsync().Result;

        private readonly List<CustomMetadataColumnInfo> CustomMetadataColumnInfos = new();
        protected override string ExportName => I18NEntity.GetString("RM_DAM_ManualUnderReviewReport");

        public ManualUnderReviewExportProcessor() 
        {
            CustomMetadataColumnInfos = s_spSettingsService.GetInUsedCustomMetadataColumnInfoAsync().GetAwaiter().GetResult();
        }

        protected override List<string> AssembleMaReviewInfoHeaderTittleForCsv()
        {
            var headerList = new List<string>(){
                 I18NEntity.GetString("RM_JS_MA_Grid_ApprovalStatus") + "("
                 + I18NEntity.GetString("RM_MA_Approve") + ";"
                 + I18NEntity.GetString("RM_MA_Reject") + ";"
                 + I18NEntity.GetString("RM_JS_MA_ApproveStatus_WaitingApprove") + ")",
                 I18NEntity.GetString("RM_JS_BCM_Explorer_Datagrid_Source"),
                 I18NEntity.GetString("RM_MA_QuickReason"),
                 I18NEntity.GetString("RM_MA_ApprovalComment"),
                  I18NEntity.GetString("RM_MA_ExportExtendDisposalDate"),
                 I18NEntity.GetString("RM_JS_MA_Grid_Title"),
                 I18NEntity.GetString("RM_JS_MA_Grid_FullPath"),
                 I18NEntity.GetString("RM_JS_MA_Grid_FolderPath"),
                 I18NEntity.GetString("RM_PRM_PRE_Column_ID"),
                 I18NEntity.GetString("RM_JS_JMD_Grid_Type"),
                 I18NEntity.GetString("RM_JS_MA_Grid_Rule"),
                 I18NEntity.GetString("RM_MA_ExportReasonforRejection"),
                 I18NEntity.GetString("RM_MA_JS_LastApproveRejectComment"),
                 I18NEntity.GetString("RM_JS_Rule_DisposalClass_Title"),
                 I18NEntity.GetString("RM_MA_Grid_EscalateOrReassignFrom"),
                 I18NEntity.GetString("RM_JS_BCM_Explorer_Datagrid_RecordsOwner"),
                 I18NEntity.GetString("RM_JS_MA_Grid_Reassigned_Comment"),
                 I18NEntity.GetString("RM_MA_JS_LastReviewedBy"),
                 I18NEntity.GetString("RM_MA_JS_LastReviewTime"),
                 I18NEntity.GetString("RM_JS_MA_Grid_ModifiedBy"),
                 I18NEntity.GetString("RM_JS_MA_Grid_CreatedBy"),
                 I18NEntity.GetString("RM_JS_MA_Grid_CreatedTime"),
                 I18NEntity.GetString("RM_JS_MA_Grid_Id"),
                 I18NEntity.GetString("RM_JS_MA_Grid_ActionTime"),
                 I18NEntity.GetString("RM_JS_MA_Grid_ModifiedTime"),
                 I18NEntity.GetString("RM_JS_MA_Grid_TermName"),
                 I18NEntity.GetString("RM_JS_MA_Grid_DisposalDueDate"),
            };
            CustomMetadataColumnInfos.OrderBy(column => column.UniqueId).ForEach(column => headerList.Add(column.ColumnName));
            return headerList;
        }



        protected override async Task<List<string>> GenerateRecordItemStringForCsvAsync(List<ManualApprovalRecord> manualItems, Dictionary<int, string> ContentSourceInfoes, Dictionary<int, string> UserDisplayNameCache)
        {
            var res = new List<string>();

            foreach (var item in manualItems)
            {
                try
                {
                    var isArchived = item.ManualRetentionStatus == 1 ? $"({I18NEntity.GetString("RM_MA_Extended_RetentionStatus")})" : string.Empty;
                    var fields = new List<string>
                        {
                            I18NEntity.GetString($"RM_JS_MA_ApproveStatus_{(SOApproveDBStatus)item.ManualApprovedStatus}") ?? string.Empty ,
                            ManualApprovalExportJobManager.GetI18NOfSourceFlag((SourceFlag)item.SourceFlag,ContentSourceInfoes) ,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            item.LeafName ?? string.Empty,
                            item.ManualFullPath ?? string.Empty ,
                            item.ManualFolderPath ?? string.Empty,
                            item.RecordsId ?? string.Empty ,
                            !string.IsNullOrEmpty(item.ExtensionForFile) ? I18NEntity.GetString(item.ExtensionForFile) + isArchived : string.Empty ,
                            item.ManualRuleName ?? string.Empty ,
                            item.ManualLastReasonForRejection ?? string.Empty ,
                            item.ManualLastApproveRejectComment ?? string.Empty,
                            item.ManualRuleDisposalClass ?? string.Empty ,
                            await ManualApprovalExportJobManager.GetUserDisplayNameAsync(item.ManualEscalateFrom,UserDisplayNameCache) ,
                            String.Join(" ,",ManualApprovalExportJobManager.GetReviewers(item.ManualReviewer)) ?? string.Empty ,
                            item.ManualEscalatedComment ?? string.Empty ,
                            item.ManualLastReviewedBy ?? string.Empty ,
                            item.ManualLastlReviewTime > 0 ? GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, item.ManualLastlReviewTime, true).SimplifyFormatTime : string.Empty,
                            item.ModifiedBy ?? string.Empty ,
                            item.CreatedBy ?? string.Empty ,
                            GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, item.ManualCollectionTime, true).SimplifyFormatTime ?? string.Empty ,
                            item.Id.ToString() ?? string.Empty ,
                            item.ManualActionTime == 0 ? "0" : GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, item.ManualActionTime, true).SimplifyFormatTime,
                            item.ManualModifiedTime > 0 ? GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, item.ManualModifiedTime, true).SimplifyFormatTime  : string.Empty ,
                            item.TermName ?? string.Empty ,
                            item.ManualDisposalDueDate > 0 ? GeneralSettingService.ConvertTiksToDateTime(GeneralSetting,  item.ManualDisposalDueDate, true).SimplifyFormatTime  : string.Empty ,
                        };
                    if(CustomMetadataColumnInfos.Count > 0)
                    {
                        foreach(var column in CustomMetadataColumnInfos.OrderBy(column => column.UniqueId))
                        {
                            if(item.CustomColumnDic != null && item.CustomColumnDic.TryGetValue(column.UniqueId.ToString(), out var value))
                            {
                                try
                                {
                                    if (column.ColumnType == CustomColumnType.SingleText || column.ColumnType == CustomColumnType.YesOrNo || column.ColumnType == CustomColumnType.Number)
                                    {
                                        fields.Add(value.Value);
                                    }
                                    else if (column.ColumnType == CustomColumnType.DateTime)
                                    {
                                        var dateTime = value.Date;
                                        var currentTime = GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, dateTime.Ticks, true).SimplifyFormatTime;
                                        fields.Add(currentTime);
                                    }
                                }
                                catch
                                {
                                    fields.Add("");
                                }
                            }
                            else
                            {
                                fields.Add("");
                            }
                        }
                    }
                    var dataLine = StringUtils.ToCSVString(fields.ToArray());
                    res.Add(dataLine);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Convert history to cell failed,history item id {item.Id},{ex}");
                    ManualApprovalExportJobManager.AddFailedJobDetail(item, ManualApprovalAction.Export, ex.Message);
                }
            }

            return res;
        }

        protected override void BuildStatusFilter(ManualApprovalQueryDefinition queryDefinition)
        {
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ApprovalStatus,
                Value = JsonConvert.SerializeObject(new List<SOApproveDBStatus> { SOApproveDBStatus.WaitingApprove })
            });
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ExtendTime,
                Value = "false"
            });
        }


    }

}
