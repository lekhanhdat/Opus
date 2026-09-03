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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Security;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Model;
using RADownloadCentre.SettingExport.Base;

namespace RADownloadCentre.SettingExport
{
    public class FSExportSettingProcessor : FSExportSettingProcessorBase
    {
        public FSExportSettingProcessor(RMExportSettingJobMessage jobMsg) : base(jobMsg.JobID, jobMsg.JobType)
        {
            FilePath = JobReportUtility.GetDownloadReportDetailTempleFolder(BaseJobDto, BaseJobDto.Id.Replace("ESFS", StartFileName), ".xlsx");
            DeactiveUNCPath = FileSystemSettingDAO.GetAllDeactiveId();
        }

        protected override async Task<string[][]> ConvertSettingToArrayAsync(List<RMFileSystemSetting> settings, string[][] datas)
        {
            int rowCount = 2;
            if (datas[0] == null) rowCount = 0;
            foreach (RMFileSystemSetting setting in settings)
            {
                int colCount = 0;
                try
                {
                    datas[rowCount] = new string[9];
                    for (int i = 0; i < 9; i++) { datas[rowCount][i] = string.Empty; }

                    datas[rowCount][colCount++] = setting.FullPath;
                    string termScope = setting.TermId != Guid.Empty
                        ? _termDAO.GetTermNamesPathByTermId(setting.TermId)
                        : _termDAO.GetTermSetNamesPathByTermSetId(setting.TermSetId);
                    datas[rowCount][colCount++] = termScope.Replace('/', PathSeparator);
                    string defaultTerm = setting.DefaultTermId == Guid.Empty ? string.Empty
                        : _termDAO.GetTermNamesPathByTermId(setting.DefaultTermId).Replace(termScope, "").Replace('/', PathSeparator);
                    datas[rowCount][colCount++] = defaultTerm.TrimStart(PathSeparator);
                    datas[rowCount][colCount++] = setting.NeedCheckDefaultValue ? "TRUE" : "";
                    datas[rowCount][colCount++] = setting.ApplyExistType == (int)ApplyExistingTermType.OverWrite && setting.NeedCheckDefaultValue ? "TRUE" : "";
                    if (setting.ApprovalType == ApprovalType.None)
                    {
                        datas[rowCount][colCount++] = NoManualSetting;
                        datas[rowCount][colCount++] = "";
                    }
                    else if (setting.ApprovalType == ApprovalType.ApprovalProcess)
                    {
                        var workflow = RMWorkflowDefinitionDAO.GetWorkflowByReferenceId(new Guid(setting.WorkflowReferenceId));
                        if (workflow == null)
                        {
                            GenerateJobDetail(setting.FullPath.Substring(setting.FullPath.LastIndexOf(@"\") + 1), setting.FullPath, "RM_JS_BCM_ExportSetting_WorkFlowProcessEmpty", false);
                            continue;
                        }
                        datas[rowCount][colCount++] = WorkflowProcess;
                        datas[rowCount][colCount++] = workflow.Name;
                    }
                    else if (setting.ApprovalType == ApprovalType.RecordOwners)
                    {
                        datas[rowCount][colCount++] = RecordOwner;
                        var usernames = (await RecordOwnerDao.GetRecordOwnerAccountsAsync(setting.Id, RecordOwnerSettingType.FileSystem)).Select(_ => _.UserPrincipalName);
                        datas[rowCount][colCount++] = string.Join(PathSeparator, usernames);
                    }
                    else
                    {
                        datas[rowCount][colCount++] = AutoApprove;
                        datas[rowCount][colCount++] = "";
                    }
                    datas[rowCount][colCount++] = setting.EMailToRecordOwner ? "TRUE" : "FALSE";
                    if (setting.DeployTermMethod == (int)DeployTermMethod.NoDefaultTerm)
                    {
                        datas[rowCount][colCount++] = ManuallyChooseATerm;
                    }
                    else if (setting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm)
                    {
                        datas[rowCount][colCount++] = SetADefaultTerm;
                    }
                    rowCount++;
                }
                catch (Exception e)
                {
                    Logger.Error($"Convert File system setting to array failed: {e}");
                    rowCount++;
                    throw;
                }
            }
            return datas;
        }

        protected override string[][] AssembleSettingHeaderTittle(string[][] datas, string connectionName)
        {
            datas[0] = new string[2];
            datas[0][0] = ConnectionNameColumn;
            datas[0][1] = connectionName;
            datas[1] = new string[9];
            datas[1][0] = UNCPathColumn;
            datas[1][1] = TermScopeColumn;
            datas[1][2] = DefaultTermColumn;
            datas[1][3] = ApplyToExistingDocumentsColumn;
            datas[1][4] = OverwriteTheExistingTermColumn;
            datas[1][5] = ManualApprovalTypeColumn;
            datas[1][6] = SendEmailForPersonColumn;
            datas[1][7] = SendEmailNotificationColumn;
            datas[1][8] = ApplyTermByColumn;
            return datas;
        }
    }
}