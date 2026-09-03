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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json;
using OpenNLP.Tools.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAManualApproval.ExportAction.RelatedRecords
{
    public class ManualRelatedRecordsExportProcessor : ManualApprovalExportProcess
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ManualRelatedRecordsExportProcessor));

        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private static readonly GeneralSettingModel GeneralSetting = GeneralSettingService.GetGeneralSettingAsync().Result;
        protected override string ExportName => I18NEntity.GetString("RM_DAM_ManualRelatedRecordsReport");

        public ManualRelatedRecordsExportProcessor() { }

        protected override List<string> AssembleMaReviewInfoHeaderTittleForCsv()
        {
            return new List<string>
            {
                I18NEntity.GetString("RM_JS_BCM_Explorer_Datagrid_Source"),    //source
                 I18NEntity.GetString("RM_JS_MA_Grid_Title"),       //record name
                 I18NEntity.GetString("RM_PRM_PRE_Column_ID"),    //record id
                 I18NEntity.GetString("RM_JS_MA_Grid_FullPath"),  // full path
                 I18NEntity.GetString("RM_JS_JMD_Grid_Type"),  // type
                 I18NEntity.GetString("RM_JS_MA_Grid_Rule"),  // rule
                 I18NEntity.GetString("RM_JS_Rule_DisposalClass_Title"),  //  disposal class     
                 I18NEntity.GetString("RM_JS_MA_Grid_RelatedRecords"),      // related records
                 I18NEntity.GetString("RM_JS_MA_Grid_RelatedRecordsAction"),  //disposal action
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
                            !string.IsNullOrEmpty(item.ExtensionForFile) ? I18NEntity.GetString(item.ExtensionForFile) + isArchived : string.Empty ,   // type
                            item.ManualRuleName ?? string.Empty ,  //rule
                            item.ManualRuleDisposalClass ?? string.Empty ,   //  disposal class     
                            string.IsNullOrEmpty(item.ManualRelatedRecords) ?
                                   string.Empty :
                                   String.Join(" ,",  SerializerHelper.DeserializeFromXmlString<List<ReportRelatedRecords>>(item.ManualRelatedRecords).Select(r=>r.Name)),   // related records

                            item.ManualIsRelatedRecords ?  item.ManualRelatedRecordsAction == 0 ? I18NEntity.GetString("RM_JS_RDM_RelatedRecordsAction_None") :I18NEntity.GetString("RM_JS_RDM_RelatedRecordsAction_Both") : "",    //disposal action
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
                Value = JsonConvert.SerializeObject(new List<SOApproveDBStatus> { SOApproveDBStatus.WaitingApprove })
            });
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.IsRelatedRecords,
                Value = "true"
            });
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ExtendTime,
                Value = "false"
            });
        }


    }

}
