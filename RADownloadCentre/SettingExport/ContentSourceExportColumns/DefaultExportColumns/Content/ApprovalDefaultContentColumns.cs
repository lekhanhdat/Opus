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
using AvePoint.RA.DB.Model;
using RADownloadCentre.SettingExport.Base;
using RADownloadCentre.SettingExport.Model;

namespace RADownloadCentre.SettingExport.ContentSourceExportColumns.DefaultExportColumns.Content;

public class ApprovalDefaultContentColumns(BaseExportCsv exportCsv, ExportTeamsSettingData setting) : BaseDefaultExportColumns(exportCsv)
{
    public override List<string> GetExportColumns()
    {
        var result = AddApprovalInformation(BaseExportCsv.GetExportColumns());
        return result;
    }
    
    private List<string> AddApprovalInformation(List<string> result)
    {
        if (setting is { ApprovalType: ApprovalType.ApprovalProcess, WorkflowInfomation: null })
        {
            return result;
        }

        var (manualApprovalTypeColumn, sendEmailForPersonColumn) = setting.ApprovalType switch
        {
            ApprovalType.None =>
                (ProcessCol(NoManualSetting), ProcessCol("")),
            ApprovalType.ApprovalProcess =>
                (ProcessCol(WorkflowProcess),
                    ProcessCol(setting.WorkflowInfomation.Name.Replace(@"""", "\"\""))),
            ApprovalType.RecordOwners =>
                (ProcessCol(RecordOwner),
                    ProcessCol(string.Join(PathSeparator, setting.UserName).Replace(@"""", "\"\""))),
            _ => (ProcessCol(AutoApprove), ProcessCol(""))
        };
        var sendEmailNotificationColumn = ProcessCol(setting.EMailToRecordOwner ? "TRUE" : "FALSE");
        result.AddRange([manualApprovalTypeColumn, sendEmailForPersonColumn, sendEmailNotificationColumn]);
        return result;
    }
}