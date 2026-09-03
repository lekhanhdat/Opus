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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.ManualApproval.Queriers;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using OpenNLP.Tools.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RAManualApproval.ExportAction.Disposal_extensions
{
    public class ManualDisposalExtensionsExportProcessor : ManualApprovalExportProcess
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ManualDisposalExtensionsExportProcessor));

        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private static readonly GeneralSettingModel GeneralSetting = GeneralSettingService.GetGeneralSettingAsync().Result;
        protected override string ExportName => I18NEntity.GetString("RM_DAM_ManualDisposalExtensionsReport");
        public ManualDisposalExtensionsExportProcessor() { }

        protected override List<string> AssembleMaReviewInfoHeaderTittleForCsv()
        {
            return new List<string>
            {
                I18NEntity.GetString("RM_JS_BCM_Explorer_Datagrid_Source"),    //source
                 I18NEntity.GetString("RM_JS_MA_Grid_Title"),       //record name
                 I18NEntity.GetString("RM_PRM_PRE_Column_ID"),    //record id
                 I18NEntity.GetString("RM_JS_MA_Grid_FullPath"),  // full path
                 I18NEntity.GetString("RM_JS_MA_Grid_FolderPath"),  //folder path
                 I18NEntity.GetString("RM_JS_JMD_Grid_Type"),  // type
                 I18NEntity.GetString("RM_JS_MA_Grid_Rule"),  // rule
                 I18NEntity.GetString("RM_MA_LastReasonforRejection"),   // last rason for rejection
                 I18NEntity.GetString("RM_MA_JS_LastApproveRejectComment"),     //last comment
                 I18NEntity.GetString("RM_JS_Rule_DisposalClass_Title"),  //  disposal class     
                 I18NEntity.GetString("RM_JS_MA_Grid_ExtendTime"),      // Disposal extended to
                 I18NEntity.GetString("RM_JS_BCM_Explorer_Datagrid_RecordsOwner"),   // reocrd reviewer
                 I18NEntity.GetString("RM_MA_JS_LastReviewedBy"),  // last reviewed by
                 I18NEntity.GetString("RM_MA_JS_LastReviewTime"),   // last review time 
                 I18NEntity.GetString("RM_JS_MA_Grid_ModifiedTime"),   //modifiedTime
            };
        }

        protected override async Task<List<string>> GenerateRecordItemStringForCsvAsync(List<ManualApprovalRecord> manualItems, Dictionary<int, string> ContentSourceInfoes, Dictionary<int, string> UserDisplayNameCache)
        {
            var res = new List<string>();
            var generalSetting = await GeneralSettingService.GetGeneralSettingAsync();
            foreach (var item in manualItems)
            {
                try
                {
                    var isArchived = item.ManualRetentionStatus == 1 ? $"({I18NEntity.GetString("RM_MA_Extended_RetentionStatus")})" : string.Empty;
                    var fields = new List<string>
                        {
                            ManualApprovalExportJobManager.GetI18NOfSourceFlag((SourceFlag)item.SourceFlag,ContentSourceInfoes) ,   //source
                            item.LeafName ?? string.Empty,  //record name
                            item.RecordsId ?? string.Empty ,   //record id
                            item.ManualFullPath ?? string.Empty ,  // full path
                            item.ManualFolderPath ?? string.Empty,  //folder path
                            !string.IsNullOrEmpty(item.ExtensionForFile) ? I18NEntity.GetString(item.ExtensionForFile) + isArchived : string.Empty ,   // type
                            item.ManualRuleName ?? string.Empty ,  //rule
                            item.ManualLastReasonForRejection ?? string.Empty , // last rason for rejection
                            item.ManualLastApproveRejectComment ?? string.Empty,    //last comment
                            item.ManualRuleDisposalClass ?? string.Empty ,   //  disposal class     
                            item.ManualExtendTime > 0 ?  GeneralSettingService.ConvertTiksToDateTime(generalSetting, item.ManualExtendTime, true).SimplifyFormatTime : "0",             //disposal extend to
                            String.Join(" ,", ManualApprovalExportJobManager.GetReviewers(item.ManualReviewer)) ?? string.Empty ,   // reocrd reviewer
                            item.ManualLastReviewedBy ?? string.Empty ,   // last reviewed by
                            item.ManualLastlReviewTime > 0 ? GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, item.ManualLastlReviewTime, true).SimplifyFormatTime : string.Empty,  // last review time 
                            item.ManualModifiedTime > 0 ? GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, item.ManualModifiedTime, true).SimplifyFormatTime  : string.Empty , //modified time
                        };

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
                Value = JsonConvert.SerializeObject(new List<SOApproveDBStatus> { SOApproveDBStatus.Approved, SOApproveDBStatus.Rejected, SOApproveDBStatus.WaitingApprove })
            });


            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ExtendTime,
                Value = "true"
            });
        }


    }
}
