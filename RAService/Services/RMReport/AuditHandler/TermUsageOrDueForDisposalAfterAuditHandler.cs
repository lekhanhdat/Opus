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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Common.Global.Utils;

namespace AvePoint.RA.Service.Services.RMReport.AuditHandler
{

    public class TermUsageOrDueForDisposalAfterAuditHandler : IAfterAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(TermUsageOrDueForDisposalAfterAuditHandler));

        public async Task<RMAuditInfo> CollectAsync(Contract.RMWeb.Audit.RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            RMAuditInfo auditInfo = info != null ? info : new RMAuditInfo();
            try
            {
                //auditInfo = new RMAuditInfo();
                switch ((AuditAction)action)
                {
                    case AuditAction.DeleteJobNotificationProfile:
                        auditInfo.Module = AuditModule.ControlPanel;
                        auditInfo.Category = AuditCategory.JobNotification;
                        auditInfo.Action = AuditAction.DeleteJobNotificationProfile;
                        auditInfo.Status = (int)AuditStatus.Successful;
                        break;
                    case AuditAction.CreateProfile:
                    case AuditAction.EditProfile:
                        RMProfile profile = this.ConvertProfile(((RMProfileDto)args[0]));
                        if ((AuditAction)action == AuditAction.CreateProfile)
                        {
                            //auditInfo.Status = int.Parse(returnValue.ToString()) > 0 ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                            auditInfo.Object = profile != null ? profile.Name : string.Empty;
                        }
                        else
                        {
                            //profile = profileDAO.GetProfileById(((RMProfileDto)args[0]).Id);
                            //auditInfo.Object = info != null ? info.Object : string.Empty;
                            //auditInfo.Status = Boolean.Parse(returnValue.ToString()) ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                        }
                        if (returnValue != null)
                        {
                            RAReturnMessage returnMessage = (RAReturnMessage)returnValue;
                            auditInfo.Status = (int)returnMessage.MessageType == 0 ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                        }
                        else
                        {
                            auditInfo.Status = (int)AuditStatus.Failed;
                        }
                        ArgumentCheck.NotNull(profile, nameof(profile));
                        switch ((AvePoint.RA.Contract.JobMonitor.JobType)profile.Type)
                        {
                            case AvePoint.RA.Contract.JobMonitor.JobType.ItemsFilesDueDisposal:
                            case AvePoint.RA.Contract.JobMonitor.JobType.EXOItemsFilesDueDisposalReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.PhysicalItemsFilesDueDisposalReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.FSItemsFilesDueDisposal:
                            case AvePoint.RA.Contract.JobMonitor.JobType.OneDriveItemsFilesDueDisposalReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.SPOnPremItemsFilesDueDisposal:
                            case AvePoint.RA.Contract.JobMonitor.JobType.BoxItemsFilesDueDisposalReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.GoogleItemsFilesDueDisposalReport:
                            case JobType.TeamsItemsFilesDueDisposalReport:
                                auditInfo.Category = AuditCategory.ReportCenter;
                                auditInfo.Action = (AuditAction)action == AuditAction.CreateProfile ? AuditAction.CreateDueForDisposalProfile : AuditAction.EditDueForDisposalProfile;
                                break;
                            case AvePoint.RA.Contract.JobMonitor.JobType.BCSTermUsageReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.EXOTermUsageReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.PhysicalTermUsageReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.FSBCSTermUsageReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.OneDriveTermUsageReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.SPOnPremBCSTermUsageReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.BoxBCSTermUsageReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.GoogleBCSTermUsageReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.TeamsBCSTermUsageReport:
                                auditInfo.Category = AuditCategory.ReportCenter;
                                auditInfo.Action = (AuditAction)action == AuditAction.CreateProfile ? AuditAction.CreateTermUsageProfile : AuditAction.EditTermUsageProfile;
                                break;
                            case AvePoint.RA.Contract.JobMonitor.JobType.OrphanedTermReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.EXOOrphanedTermUsageReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.PhysicalOrphanedTermUsageReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.FSOrphanedTermReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.OneDriveOrphanedTermUsageReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.SPOnPremOrphanedTermReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.BoxOrphanedTermUsageReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.GoogleOrphanedTermUsageReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.TeamsOrphanedTermUsageReport:
                                auditInfo.Category = AuditCategory.ReportCenter;
                                auditInfo.Action = (AuditAction)action == AuditAction.CreateProfile ? AuditAction.CreateOrphanTermProfile : AuditAction.EditOrphanTermProfile;
                                break;
                            case AvePoint.RA.Contract.JobMonitor.JobType.RetiredTermReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.EXORetiredTermUsageReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.PhysicalRetiredTermUsageReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.FSRetiredTermReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.OneDriveRetiredTermUsageReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.SPOnPremRetiredTermReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.BoxRetiredTermUsageReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.GoogleRetiredTermUsageReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.TeamsRetiredTermUsageReport:
                                auditInfo.Category = AuditCategory.ReportCenter;
                                auditInfo.Action = (AuditAction)action == AuditAction.CreateProfile ? AuditAction.CreateRetiredTermProfile : AuditAction.EditRetiredTermProfile;
                                break;
                            case AvePoint.RA.Contract.JobMonitor.JobType.CreateAndDestroyedFileReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.EXOCreateAndDestroyedFileReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.PhysicalCreateAndDestroyedFileReport:
                            case JobType.FSCreateAndDestroyedFileReport:
                            case JobType.OneDriveCreateAndDestroyedFileReport:
                            case JobType.SPOnPremCreateAndDestroyedFileReport:
                            case JobType.BoxCreateAndDestroyedFileReport:
                            case JobType.GoogleCreateAndDestroyedFileReport:
                            case JobType.TeamsCreateAndDestroyedFileReport:
                                auditInfo.Category = AuditCategory.ReportCenter;
                                auditInfo.Action = (AuditAction)action == AuditAction.CreateProfile ? AuditAction.CreateCreationAndDestructionReport : AuditAction.EditCreationAndDestructionReport;
                                break;
                            case AvePoint.RA.Contract.JobMonitor.JobType.AvailableSpaceReport:
                                auditInfo.Category = AuditCategory.ReportCenter;
                                auditInfo.Action = (AuditAction)action == AuditAction.CreateProfile ? AuditAction.CreateAvailableSpaceReportProfile : AuditAction.EditAvailableSpaceReportProfile;
                                break;
                            case AvePoint.RA.Contract.JobMonitor.JobType.SPOActionAuditReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.OneDriveActionAuditReport:
                            case JobType.TeamsActionAuditReport:
                                auditInfo.Category = AuditCategory.ReportCenter;
                                auditInfo.Action = (AuditAction)action == AuditAction.CreateProfile ? AuditAction.CreateActionAuditReport : AuditAction.EditActionAuditReport;
                                break;
                            case AvePoint.RA.Contract.JobMonitor.JobType.RestoreReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.OneDriverRestoreReport:
                            case AvePoint.RA.Contract.JobMonitor.JobType.TeamsRestoreReport:
                            case JobType.GoogleRestoreReport:
                                auditInfo.Category = AuditCategory.ReportCenter;
                                auditInfo.Action = (AuditAction)action == AuditAction.CreateProfile ? AuditAction.CreateRestoreReportProfile : AuditAction.EditRestoreReportProfile;
                                break;
                            case AvePoint.RA.Contract.JobMonitor.JobType.JobNotification:
                                auditInfo.Module = AuditModule.ControlPanel;
                                auditInfo.Category = AuditCategory.JobNotification;
                                auditInfo.Action = (AuditAction)action == AuditAction.CreateProfile ? AuditAction.CreateJobNotificationProfile : AuditAction.EditJobNotificationProfile;
                                break;
                            default:
                                break;
                        }

                        break;
                    case AuditAction.DeleteProfile:
                        DelProfileInfo dpi = (DelProfileInfo)args[0];
                        if (dpi.Type == JobType.BCSTermUsageReport || dpi.Type == JobType.EXOTermUsageReport || dpi.Type == JobType.PhysicalTermUsageReport || dpi.Type == JobType.FSBCSTermUsageReport || dpi.Type == JobType.OneDriveTermUsageReport || dpi.Type == JobType.SPOnPremBCSTermUsageReport || dpi.Type == JobType.BoxBCSTermUsageReport || dpi.Type == JobType.TeamsBCSTermUsageReport)
                        {
                            auditInfo.Action = AuditAction.DeleteTermUsageProfile;
                            auditInfo.Category = AuditCategory.ReportCenter;
                        }
                        else if (dpi.Type == JobType.RetiredTermReport || dpi.Type == JobType.EXORetiredTermUsageReport || dpi.Type == JobType.PhysicalRetiredTermUsageReport || dpi.Type == JobType.FSRetiredTermReport || dpi.Type == JobType.OneDriveRetiredTermUsageReport || dpi.Type == JobType.SPOnPremRetiredTermReport || dpi.Type == JobType.BoxRetiredTermUsageReport || dpi.Type == JobType.TeamsRetiredTermUsageReport)
                        {
                            auditInfo.Action = AuditAction.DeleteRetiredTermProfile;
                            auditInfo.Category = AuditCategory.ReportCenter;
                        }
                        else if (dpi.Type == JobType.OrphanedTermReport || dpi.Type == JobType.EXOOrphanedTermUsageReport || dpi.Type == JobType.PhysicalOrphanedTermUsageReport || dpi.Type == JobType.FSOrphanedTermReport || dpi.Type == JobType.OneDriveOrphanedTermUsageReport || dpi.Type == JobType.SPOnPremOrphanedTermReport || dpi.Type == JobType.BoxOrphanedTermUsageReport || dpi.Type == JobType.TeamsOrphanedTermUsageReport)
                        {
                            auditInfo.Action = AuditAction.DeleteOrphanTermProfile;
                            auditInfo.Category = AuditCategory.ReportCenter;
                        }
                        else if (dpi.Type == JobType.ItemsFilesDueDisposal || dpi.Type == JobType.EXOItemsFilesDueDisposalReport
                            || dpi.Type == JobType.PhysicalItemsFilesDueDisposalReport || dpi.Type == JobType.FSItemsFilesDueDisposal
                            || dpi.Type == JobType.OneDriveItemsFilesDueDisposalReport || dpi.Type == JobType.SPOnPremItemsFilesDueDisposal
                            || dpi.Type == JobType.BoxItemsFilesDueDisposalReport || dpi.Type == JobType.GoogleItemsFilesDueDisposalReport
                            || dpi.Type == JobType.TeamsItemsFilesDueDisposalReport)
                        {
                            auditInfo.Action = AuditAction.DeleteDueForDisposalProfile;
                            auditInfo.Category = AuditCategory.ReportCenter;
                        }
                        else if (dpi.Type == JobType.CreateAndDestroyedFileReport 
                            || dpi.Type == JobType.EXOCreateAndDestroyedFileReport
                            || dpi.Type == JobType.PhysicalCreateAndDestroyedFileReport 
                            || dpi.Type == JobType.FSCreateAndDestroyedFileReport
                            || dpi.Type == JobType.OneDriveCreateAndDestroyedFileReport
                            || dpi.Type == JobType.SPOnPremCreateAndDestroyedFileReport
                            || dpi.Type == JobType.BoxCreateAndDestroyedFileReport
                            || dpi.Type == JobType.GoogleCreateAndDestroyedFileReport
                            || dpi.Type == JobType.TeamsCreateAndDestroyedFileReport)
                        {
                            auditInfo.Action = AuditAction.DeleteCreationAndDestructionReport;
                            auditInfo.Category = AuditCategory.ReportCenter;
                        }
                        else if (dpi.Type == JobType.AvailableSpaceReport)
                        {
                            auditInfo.Action = AuditAction.DeleteAvailableSpaceReportProfile;
                            auditInfo.Category = AuditCategory.ReportCenter;
                        }
                        else if (dpi.Type == JobType.SPOActionAuditReport
                            || dpi.Type == JobType.OneDriveActionAuditReport
                            || dpi.Type == JobType.TeamsActionAuditReport)
                        {
                            auditInfo.Action = AuditAction.DeleteActionAuditReport;
                            auditInfo.Category = AuditCategory.ReportCenter;
                        }
                        else if (dpi.Type == JobType.OneDriverRestoreReport || dpi.Type == JobType.RestoreReport || dpi.Type == JobType.TeamsRestoreReport || dpi.Type == JobType.GoogleRestoreReport)
                        {
                            auditInfo.Action = AuditAction.DeleteRestoreReportProfile;
                            auditInfo.Category = AuditCategory.ReportCenter;
                        }
                        string profileNames = string.Empty;

                        if (dpi.Names != null)
                        {
                            profileNames = string.Join(",", dpi.Names.ToArray());
                        }
                        auditInfo.Object = profileNames;
                        var result = ((bool, List<string>))returnValue;
                        auditInfo.Status = result.Item1 ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                        break;
                    case AuditAction.GenerateReport:
                        var jobType = (JobType)args[0];
                        var isOrphanTerm = args.Length > 4 ? Convert.ToBoolean(args[3]) : args.Length > 3 ? Convert.ToBoolean(args[2]) : false;
                        var isRetiredTerm = args.Length > 4 ? Convert.ToBoolean(args[4]) : args.Length > 3 ? Convert.ToBoolean(args[3]) : false;
                        if (jobType == JobType.BCSTermUsageReport || jobType == JobType.EXOTermUsageReport || jobType == JobType.PhysicalTermUsageReport
                            || jobType == JobType.FSBCSTermUsageReport || jobType == JobType.OneDriveTermUsageReport || jobType == JobType.SPOnPremBCSTermUsageReport 
                            || jobType == JobType.BoxBCSTermUsageReport || jobType == JobType.GoogleBCSTermUsageReport || jobType == JobType.TeamsBCSTermUsageReport)
                        {
                            auditInfo.Category = AuditCategory.ReportCenter;
                            if (isOrphanTerm)
                            {
                                auditInfo.Action = AuditAction.GenerateOrphanTermReport;
                            }
                            else if (isRetiredTerm)
                            {
                                auditInfo.Action = AuditAction.GenerateRetiredTermReport;
                            }
                            else
                            {
                                auditInfo.Action = AuditAction.GenerateBCSTermUsageReport;
                            }
                        }
                        else if (jobType == JobType.ItemsFilesDueDisposal || jobType == JobType.EXOItemsFilesDueDisposalReport
                            || jobType == JobType.PhysicalItemsFilesDueDisposalReport || jobType == JobType.FSItemsFilesDueDisposal
                            || jobType == JobType.OneDriveItemsFilesDueDisposalReport || jobType == JobType.SPOnPremItemsFilesDueDisposal
                            || jobType == JobType.BoxItemsFilesDueDisposalReport || jobType == JobType.GoogleItemsFilesDueDisposalReport
                            || jobType == JobType.TeamsItemsFilesDueDisposalReport)
                        {
                            auditInfo.Category = AuditCategory.ReportCenter;
                            auditInfo.Action = AuditAction.GenerateContentDueDisposalReport;
                        }
                        else if (jobType == JobType.CreateAndDestroyedFileReport 
                            || jobType == JobType.EXOCreateAndDestroyedFileReport 
                            || jobType == JobType.PhysicalCreateAndDestroyedFileReport 
                            || jobType == JobType.FSCreateAndDestroyedFileReport
                            || jobType == JobType.OneDriveCreateAndDestroyedFileReport
                            || jobType == JobType.SPOnPremCreateAndDestroyedFileReport
                            || jobType == JobType.BoxCreateAndDestroyedFileReport
                            || jobType == JobType.GoogleCreateAndDestroyedFileReport
                            || jobType == JobType.TeamsCreateAndDestroyedFileReport)
                        {
                            auditInfo.Category = AuditCategory.ReportCenter;
                            auditInfo.Action = AuditAction.GenerateCreationAndDestructionReport;
                        }
                        else if (jobType == JobType.AvailableSpaceReport)
                        {
                            auditInfo.Category = AuditCategory.ReportCenter;
                            auditInfo.Action = AuditAction.GenerateAvailableSpaceReport;
                        }
                        else if (jobType == JobType.SPOActionAuditReport
                            || jobType == JobType.OneDriveActionAuditReport
                            || jobType == JobType.TeamsActionAuditReport)
                        {
                            auditInfo.Category = AuditCategory.ReportCenter;
                            auditInfo.Action = AuditAction.GenerateActionAuditReport;
                        }
                        else if (jobType == JobType.RestoreReport || jobType == JobType.OneDriverRestoreReport || jobType == JobType.TeamsRestoreReport || jobType == JobType.GoogleRestoreReport)
                        {
                            auditInfo.Category = AuditCategory.ReportCenter;
                            auditInfo.Action = AuditAction.GenerateRestoreReport;
                        }
                        else if (jobType == JobType.ExportSiteMetrics)
                        {
                            auditInfo.Category = AuditCategory.ReportCenter;
                            auditInfo.Action = AuditAction.GenerateExportSiteMetricsReport;
                        }
                        auditInfo.Object = returnValue != null ? returnValue.ToString() : string.Empty;
                        auditInfo.Status = !string.IsNullOrEmpty(returnValue?.ToString()) ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                        break;
                    case AuditAction.ExportReport:
                        BaseJobDto jobDto = (BaseJobDto)args[0];
                        var isOrphanTermReport = (Boolean)args[1];
                        var isRetiredTermReport = (Boolean)args[2];
                        if ((JobType)jobDto.JobType == JobType.BCSTermUsageReport || (JobType)jobDto.JobType == JobType.EXOTermUsageReport 
                            || (JobType)jobDto.JobType == JobType.PhysicalTermUsageReport|| (JobType)jobDto.JobType == JobType.FSBCSTermUsageReport
                            || (JobType)jobDto.JobType == JobType.OneDriveTermUsageReport || (JobType)jobDto.JobType == JobType.SPOnPremBCSTermUsageReport
                            || (JobType)jobDto.JobType == JobType.BoxBCSTermUsageReport || (JobType)jobDto.JobType == JobType.GoogleBCSTermUsageReport 
                            || (JobType)jobDto.JobType == JobType.TeamsBCSTermUsageReport)
                        {
                            auditInfo.Category = AuditCategory.ReportCenter;
                            if(isOrphanTermReport)
                            {
                                auditInfo.Action = AuditAction.ExportOrphanTermReport;
                            }
                            else if (isRetiredTermReport)
                            {
                                auditInfo.Action = AuditAction.ExportRetiredTermReport;
                            }
                            else
                            {
                                auditInfo.Action = AuditAction.ExportBCSTermUsageReport;
                            }
                        }
                        else if ((JobType)jobDto.JobType == JobType.ItemsFilesDueDisposal || (JobType)jobDto.JobType == JobType.EXOItemsFilesDueDisposalReport 
                            || (JobType)jobDto.JobType == JobType.PhysicalItemsFilesDueDisposalReport|| (JobType)jobDto.JobType == JobType.FSItemsFilesDueDisposal
                            || (JobType)jobDto.JobType == JobType.OneDriveItemsFilesDueDisposalReport || (JobType)jobDto.JobType == JobType.SPOnPremItemsFilesDueDisposal
                            || (JobType)jobDto.JobType == JobType.BoxItemsFilesDueDisposalReport || (JobType)jobDto.JobType == JobType.GoogleItemsFilesDueDisposalReport
                            || (JobType)jobDto.JobType == JobType.TeamsItemsFilesDueDisposalReport)
                        {
                            auditInfo.Category = AuditCategory.ReportCenter;
                            auditInfo.Action = AuditAction.ExportContentDueDisposalReport;
                        }
                        else if ((JobType)jobDto.JobType == JobType.CreateAndDestroyedFileReport 
                            || (JobType)jobDto.JobType == JobType.EXOCreateAndDestroyedFileReport 
                            || (JobType)jobDto.JobType == JobType.PhysicalCreateAndDestroyedFileReport 
                            || (JobType)jobDto.JobType == JobType.FSCreateAndDestroyedFileReport
                            || (JobType)jobDto.JobType == JobType.OneDriveCreateAndDestroyedFileReport
                            || (JobType)jobDto.JobType == JobType.SPOnPremCreateAndDestroyedFileReport
                            || (JobType)jobDto.JobType == JobType.BoxCreateAndDestroyedFileReport
                            || (JobType)jobDto.JobType == JobType.GoogleCreateAndDestroyedFileReport
                            || (JobType)jobDto.JobType == JobType.TeamsCreateAndDestroyedFileReport)
                        {
                            auditInfo.Category = AuditCategory.ReportCenter;
                            auditInfo.Action = AuditAction.ExportCreationAndDestructionReport;
                        }
                        else if ((JobType)jobDto.JobType == JobType.AvailableSpaceReport)
                        {
                            auditInfo.Category = AuditCategory.ReportCenter;
                            auditInfo.Action = AuditAction.ExportAvailableSpaceReport;
                        }
                        else if ((JobType)jobDto.JobType == JobType.SPOActionAuditReport
                            || (JobType)jobDto.JobType == JobType.OneDriveActionAuditReport
                            || (JobType)jobDto.JobType == JobType.TeamsActionAuditReport)
                        {
                            auditInfo.Category = AuditCategory.ReportCenter;
                            auditInfo.Action = AuditAction.ExportActionAuditReport;
                        }
                        else if ((JobType)jobDto.JobType == JobType.RestoreReport || (JobType)jobDto.JobType == JobType.OneDriverRestoreReport || (JobType)jobDto.JobType == JobType.TeamsRestoreReport || (JobType)jobDto.JobType == JobType.GoogleRestoreReport)
                        {
                            auditInfo.Category = AuditCategory.ReportCenter;
                            auditInfo.Action = AuditAction.ExportRestoreReport;
                        }
                        auditInfo.Object = jobDto.ProfileName;
                        auditInfo.Status = (bool)returnValue ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                        break;
                    case AuditAction.ExportAuditorReport:
                        auditInfo.Category = AuditCategory.AuditorReport;
                        auditInfo.Action = AuditAction.ExportAuditorReport;
                        RAReturnMessage message = (RAReturnMessage)returnValue;
                        auditInfo.Status = message.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                        break;
                    case AuditAction.ExportReportDetailsJob:
                        auditInfo.Category = AuditCategory.ReportCenter;
                        auditInfo.Action = AuditAction.ExportReportDetailsJob;
                        auditInfo.Object = returnValue.ToString();
                        break;
                    default:
                        break;
                }
                auditInfo.Module = (AuditModule)model;

                return auditInfo;
            }
            catch (Exception e)
            {
                auditInfo.Status = (int)AuditStatus.Failed;
                logger.Error(e.Message);
                throw;
            }

        }

        private RMProfile ConvertProfile(RMProfileDto dto)
        {
            RMProfile profile = new RMProfile()
            {
                Id = dto.Id,
                Name = dto.ProfileName,
                Description = dto.Description,
                Type = (int)dto.Type,
                Extension1 = dto.Extension1,
                Extension2 = dto.Extension2,
            };
            return profile;
        }

    }
}
