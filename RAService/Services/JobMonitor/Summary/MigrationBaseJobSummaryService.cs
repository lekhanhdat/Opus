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
using AvePoint.Common;
using AvePoint.GCommon.Contract.Server.Common.TimeZone;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Vault.Message;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.I18N.Core.DaoMigration;
using AvePoint.RA.Service.Services.JobMonitor.Detail;
using AvePoint.RA.Service.Services.JobMonitor.Summary.MigrationWorkerHanlder.Interface;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchiverJobStatus = AvePoint.Common.JobState;

namespace AvePoint.RA.Service.Services.JobMonitor.Summary
{
    public class MigrationBaseJobSummaryService : IMigrationJobSummaryService
    {
        protected RALogger logger = RALogger.GetInstance(typeof(MigrationBaseJobSummaryService));

        #region const

        public const string JOBINFORMATION = "Job Information";
        public const string STATISTICS = "Statistics";
        public const string LOGCOLLECTIONINFORMATION = "Log Collection Information";

        public const string PLANNAME = "PlanName";
        public const string SCOPE = "Scope";
        public const string DESCRIPTION = "Description";
        public const string JOBID = "JobId";
        public const string MAINTENANCDJOBID = "Maintenance Job ID";
        public const string BACKUPOPTION = "BackupOption";
        public const string BACKUPMETHOD = "BackupMethod";
        public const string CREATEPERSISTENTSNAPSHOT = "CreatePersistentSnapshot";
        public const string VDBENABLED = "VDBEnabled";
        public const string RESTOREGRANULARITYLEVEL = "RestoreGranularityLevel";
        public const string RESTORETYPE = "RestoreType";
        public const string RESTOREMODE = "RestoreMode";
        public const string CONCORDANCEFORMAT = "ConcordanceFormat";
        public const string RESTOREOPTIONS = "RestoreOptions";
        public const string STORAGEPOLICY = "StoragePolicy";
        public const string LOGICALDEVICE = "LogicalDevice";
        public const string PHYSICALDEVICE = "PhysicalDevice";
        public const string SOTRAGEDEVICE = "StorageDevice";

        public const string TIMEZONE = "TimeZone";
        public const string STARTTIME = "StartTime";
        public const string ENDTIME = "EndTime";
        public const string PLANMODIFIEDBY = "PlanModifiedBy";
        public const string JOBOPERATEDBY = "JobOperatedBy";
        public const string SOURCEFARM = "SourceFarm";
        public const string SOURCEFARMS = "SourceFarmS";    //实际上只有一个farm, 这里用S区别, 是送sub job里取的farm还是从main job里取的farm
        public const string PRSOURCEFARM = "PlatformSourceFarm";
        public const string PRTARGETFARM = "PlatformTargetFarm";
        public const string TARGETFARM = "TargetFarm";
        public const string TARGETFARMS = "TargetFarmS";    //实际上只有一个farm, 这里用S区别, 是送sub job里取的farm还是从main job里取的farm
        public const string TARGETFARMSO = "TargetFarmSO";
        public const string SOURCEAGENT = "SourceAgent";
        public const string HIDESOURCEAGENT = "HideSourceAgent";
        public const string SOURCEAGENTS = "SourceAgents";
        public const string HIDESOURCEAGENTS = "HideSourceAgents";
        public const string TARGETAGENT = "TargetAgent";
        public const string HIDETARGETAGENT = "HideTargetAgent";
        public const string TARGETAGENTS = "TargetAgents";
        public const string HIDETARGETAGENTS = "HideTargetAgents";
        public const string MANAGERVERSION = "ManagerVersion";
        public const string SOURCEAGENTVERSION = "SourceAgentVersion";
        public const string TARGETAGENTVERSION = "TargetAgentVersion";
        public const string MEDIASERVER = "MediaServer";

        public const string APPS = "Apps";
        public const string FOLDER = "Folder";
        public const string STATUS = "Status";
        public const string COMMENTS = "COMMENTS";
        public const string DELETIONCOMMENTS = "DeletionComments";
        public const string NUMBEROFSUCCEEDEDOBJECTS = "NumberOfSucceededObjects";
        public const string NUMBEROFFAILEDOBJECTS = "NumberOfFailedObjects";
        public const string NUMBEROFSKIPPEDOBJECTS = "NumberOfSkippedObjects";
        public const string NUMBER_OF_FILTERED_DOBJECTS = "NumberOFFilteredObjects";
        public const string NUMBEROFCONTENTDB = "NumberOfContentDB";
        //public const string NUMBEROFWEBAPP = "NumberOfWebapp";
        public const string TOTALSIZE = "TotalSize";
        public const string TransferredSize = GConstants.JobSummaryKey.TransferredSize;
        public const string COUNTOFPRUNEDJOBRECORDS = "CountOfPrunedJobRecords";
        public const string SUCCESSFULSOLUTIONCOUNT = "SuccessfulSolutionCount";
        public const string FAILEDSOLUTIONCOUNT = "FailedSolutionCount";
        public const string SOLUTIONSIZE = "SolutionSize";


        public const string STATISTICSFOREXPORT = SOConstants.StatisticsForExport;
        public const string STATISTICSFORBACKUP = SOConstants.StatisticsForBackup;
        public const string STATISTICSFORDELETION = SOConstants.StatisticsForDeletion;
        public const string STATISTICSFORRECORDMANAGER = SOConstants.StatisticsForRecordManager;
        public const string STATISTICSFOTAG = SOConstants.StatisticsForTag;



        public const string MAINTENANCEACTIONS = "Maintenance Action";
        public const string COPYDATASTATUS = "Copy Data Status";
        public const string VDBMAPPINGSTATUS = "InstaMount Mapping Status";
        public const string INDEXSTATUS = "Index Status";
        public const string IMPORTFROM = "Import From";
        public const string DATATYPE = "Data Type";

        //ebs stub upgrade
        public const string EBSDisabled = "EBS Disabled";
        public const string ScheduledRuleEnabled = "Scheduled Rule Enabled";
        public const string WebappRealtimeRuleEnabled = "Web Application Realtime Rule Enabled";

        public const string None = "None";
        public const string Yes = "Yes";
        public const string No = "No";

        public const string JOBSETTINGS = "Job Settings";
        public const string DELETESOURCE = "Delete Source Contents";
        public const string NOTIFICATION = "Notification";
        public const string TRANSFERJOBID = "Transfer JobId";

        // retention job
        public const string SOURCELOGICALDEVICE = "Source Logical Device";
        public const string DeleteGroupId = "Delete Group ID";
        public const string EXPORTREPORTLOCATION = "Report Location";
        public const string TOTALSIZEBEFORE = "BeforeTotalSize";
        public const string TOTALSIZEAFTER = "AfterTotalSize";
        #endregion

        #region interface

        protected IGeneralSettingService GeneralSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();
        IArchiverJobDao ArhciverJobDao => PlatformWindsorManager.GetService<IArchiverJobDao>();
        ITenantInfoDao TenantInfoDao = PlatformWindsorManager.GetService<ITenantInfoDao>();
        public IJobMonitorDao JMDao => PlatformWindsorManager.GetService<IJobMonitorDao>();

        IMigrationJobDetailWorkerHanlder workerHanlder = PlatformWindsorManager.GetService<IMigrationJobDetailWorkerHanlder>();
        #endregion

        public virtual string[] GetSummaryAttributes()
        {
            return Array.Empty<string>();
        }

        public virtual RMJobSummaryInfos GetJobSummaryInfo(BaseJobDto job, GeneralSettingModel gsm)
        {
            throw new NotImplementedException();
        }

        public virtual (JMJobSummary, SOJob) GetSummaryBasicInfo(string jobId, GeneralSettingModel gsm)
        {
            JMJobSummary summary = new();
            SOJob jobInfo = new();
            var archiverJob = ArhciverJobDao.GetJobByID(jobId);
            if (archiverJob != null)
            {
                summary = new JMJobSummary()
                {
                    JobId = jobId,
                    JobType = (Contract.JobMonitor.JobType)archiverJob.JobType,
                    Scope = archiverJob.Scope,
                    StartTime = GeneralSettingService.ConvertTiksToDateTime(gsm, archiverJob.StartTime, true).SimplifyFormatTime,
                    EndTime = GeneralSettingService.ConvertTiksToDateTime(gsm, archiverJob.EndTime, true).SimplifyFormatTime,
                    JobRunBy = archiverJob.UserName
                };
                jobInfo = new()
                {
                    Id = archiverJob.Id,
                    Category = archiverJob.JobCategory,
                    PlanId = archiverJob.PlanId,
                    Type = archiverJob.JobType,
                    State = archiverJob.StatusFromDAOL,
                    StartTime = archiverJob.StartTime,
                    FinishTime = archiverJob.EndTime,
                    Scope = archiverJob.Scope,
                    UserName = archiverJob.UserName,
                    Detail = archiverJob.Comment,
                };
            }
            return (summary, jobInfo);
        }

        protected Contract.JobMonitor.BaseJobDto GetRABaseJobDto(BaseJobDto jobDto)
        {
            var tenantGroupEmail = TenantInfoDao.GetTenantInfo(TenantLocalValue.LogonGroupId)?.RegisterEmail;
            var raJobDto = new Contract.JobMonitor.BaseJobDto()
            {
                Id = jobDto.Id,
                JobType = jobDto.Type,
                PlanId = jobDto.PlanId,
                Category = jobDto.Category,
                TenantGroupEmail = tenantGroupEmail
            };
            return raJobDto;
        }

        protected AbstractDaoMigrationJobDetailWorker GetDetailWorker(BaseJobDto jobDto)
        {
            return workerHanlder.GetDetailWorker(jobDto);
        }

        protected Dictionary<string, string> AssembleSummaryMap(List<JobSummary> jobSummaries, List<SubJobDto> inCorrectSubJobs = null)
        {
            Dictionary<string, string> jobSummaryMap = new Dictionary<string, string>();

            foreach (var item in jobSummaries)
            {
                if (item.Key.Contains("Count"))
                {
                    if (!jobSummaryMap.ContainsKey(item.Key))
                    {
                        jobSummaryMap[item.Key] = "" + Convert.ToInt32(item.Value);
                    }
                    else
                    {
                        jobSummaryMap[item.Key] = "" + (Convert.ToInt32(item.Value) + Convert.ToInt32(jobSummaryMap[item.Key]));
                    }
                }
                else if (item.Key.Equals("DataSize", StringComparison.OrdinalIgnoreCase) || item.Key.Equals(TransferredSize, StringComparison.OrdinalIgnoreCase))
                {
                    if (!jobSummaryMap.ContainsKey(item.Key))
                    {
                        jobSummaryMap[item.Key] = "" + Convert.ToDouble(item.Value);
                    }
                    else
                    {
                        jobSummaryMap[item.Key] = "" + (Convert.ToDouble(item.Value) + Convert.ToDouble(jobSummaryMap[item.Key]));
                    }
                }
                else if (item.Key.Equals("Comments", StringComparison.OrdinalIgnoreCase))
                {
                    if (inCorrectSubJobs != null && inCorrectSubJobs.Count > 0)
                    {
                        if (inCorrectSubJobs.Select(s => s.Id).Contains(item.SubJobId))
                        {
                            jobSummaryMap[item.Key] = item.Value.ToString();
                        }
                    }
                    else
                    {
                        jobSummaryMap[item.Key] = item.Value.ToString();
                    }
                }
                else
                {
                    jobSummaryMap[item.Key] = item.Value.ToString();
                }
            }
            return jobSummaryMap;
        }

        protected List<RMJobSummaryRow> GetJobInfoRows(BaseJobDto job, GeneralSettingModel gsm)
        {
            string[] attributes = GetSummaryAttributes();
            List<RMJobSummaryRow> result = null;
            if (attributes != null && attributes.Length > 0)
            {
                result = new List<RMJobSummaryRow>(attributes.Length);
                foreach (string attributeKey in attributes)
                {
                    RMJobSummaryRow row = GetJobInfoRow(attributeKey, job, gsm);
                    if (row != null)
                    {
                        result.Add(row);
                    }
                }
            }
            return result;
        }

        protected static ArchiverJobStatus ConvertArchiverJobStatusToOpus(JobStatus state)
        {
            return state switch
            {
                JobStatus.None => ArchiverJobStatus.None,
                JobStatus.Wait => ArchiverJobStatus.Waiting,
                JobStatus.InProgress => ArchiverJobStatus.InProgress,
                JobStatus.Finished => ArchiverJobStatus.Finished,
                JobStatus.Failed => ArchiverJobStatus.Failed,
                JobStatus.FinishWithException => ArchiverJobStatus.FinishedException,
                JobStatus.Stopped => ArchiverJobStatus.Stopped,
                JobStatus.Skipped => ArchiverJobStatus.Skiped,
                JobStatus.Stopping => ArchiverJobStatus.Stopping,
                JobStatus.Calculating => ArchiverJobStatus.Started,
                JobStatus.Pending => ArchiverJobStatus.Pending,
                _ => ArchiverJobStatus.None,
            };
        }

        protected string ConvertJobStatusToString(JobState jobStatus)
        {
            string result = null;
            switch (jobStatus)
            {
                case JobState.Waiting:
                    result = I18NEntity.GetString("RM_JS_JM_Status_Wait");
                    break;
                case JobState.InProgress:
                    result = I18NEntity.GetString("RM_JS_JM_Status_InProgerss");
                    break;
                case JobState.Finished:
                    result = I18NEntity.GetString("RM_JS_JM_Status_Finished");
                    break;
                case JobState.Failed:
                    result = I18NEntity.GetString("RM_JS_JM_Status_Failed");
                    break;
                case JobState.FinishedException:
                    result = I18NEntity.GetString("RM_JS_JM_Status_FinishWithException");
                    break;
                case JobState.Stopped:
                    result = I18NEntity.GetString("RM_JS_JM_Status_Stopped");
                    break;
                case JobState.Skiped:
                    result = I18NEntity.GetString("RM_JS_JM_Status_Skipped");
                    break;
                case JobState.Stopping:
                    result = I18NEntity.GetString("RM_JS_JM_Status_Stopping");
                    break;
            }
            return result;
        }

        protected RMJobSummaryRow GetComments(BaseJobDto jobDto, JobReportDetailEntityType[] entityTypes, Dictionary<string, string> jobSummaryMap)
        {
            string comments = string.Empty;
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.Comments)) { comments = jobSummaryMap[GConstants.JobSummaryKey.Comments]; }
            comments = ConvertXmlToI18NString(!string.IsNullOrEmpty(comments) ? comments : jobDto.Detail);
            return new RMJobSummaryRow { Key = I18NEntity.GetString("RM_JS_JM_Comment"), Value = comments };      
        }

        private string ConvertXmlToI18NString(string xmlString)
        {
            if (string.IsNullOrEmpty(xmlString)) return string.Empty;

            if (xmlString.StartsWith("<", StringComparison.Ordinal))
            {
                try
                {
                    List<PropertyItem> PropertyItems = SerializerHelper.DeserializeFromXmlString<List<PropertyItem>>(xmlString);
                    string iI8NStr = string.Empty;
                    foreach (PropertyItem item in PropertyItems)
                    {
                        if (GConstants.JobSummaryKey.Gui_NewLine.Equals(item.Key))//换行
                        {
                            iI8NStr += "\r\n";
                            continue;
                        }
                        try
                        {
                            iI8NStr += item.Args != null && item.Args.Length > 0 ? I18NEntity.GetComment(item.Key, item.DefaultValue, item.Args) : I18NEntity.GetComment(item.Key, item.DefaultValue);
                        }
                        catch (Exception e)
                        {
                            iI8NStr = item.DefaultValue;
                        }
                    }
                    return iI8NStr;
                }
                catch (Exception e)
                {
                    logger.Warn(xmlString + " Deserialize error: " + e.ToString());
                    //if (xmlString.ToLower(CultureInfo.CurrentCulture).Contains("jobsummarycommonts"))
                    //{
                    //    try
                    //    {
                    //        JobSummaryCommonts comments = SerializerHelper.DeserializeFromXmlString<JobSummaryCommonts>(xmlString);
                    //        return I18NResourceRespository.GetComment(comments.Message, comments.Message);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        logger.Error(xmlString + " Deserialize error: " + ex.ToString());
                    //    }
                    //}
                    //else
                    //{
                    //    try
                    //    {
                    //        return SerializerHelper.DeserializeFromXmlString<object>(xmlString).ToString();
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        logger.Error(xmlString + " Deserialize error: " + ex.ToString());
                    //    }
                    //}
                    return xmlString;
                }
            }
            return I18NEntity.GetComment(xmlString, xmlString);
        }

        private RMJobSummaryRow GetJobInfoRow(string attributeKey, BaseJobDto jobDto, GeneralSettingModel gsm)
        {
            try
            {
                if (PLANNAME.Equals(attributeKey))
                {
                    return new RMJobSummaryRow { Key = I18NEntity.GetString("ControlPanel.Service_Plan Name"), Value = (jobDto.PlanName == null ? "" : jobDto.PlanName) };
                }

                if (SCOPE.Equals(attributeKey))
                {
                    return GetScope(jobDto);
                }

                if (DESCRIPTION.Equals(attributeKey))
                {
                    return GetDescription(jobDto);
                }

                if (JOBID.Equals(attributeKey))
                {
                    if (jobDto.IsTestRun == RunJobMode.TestRun)
                    {
                        return new RMJobSummaryRow { Key = I18NEntity.GetString("RM_JS_JM_JobID"), Value = jobDto.Id + I18NEntity.GetString("ControlPanel.Service_(Test Run)") };
                    }
                    else
                    {
                        return new RMJobSummaryRow { Key = I18NEntity.GetString("RM_JS_JM_JobID"), Value = jobDto.Id };
                    }
                }

                if (IMPORTFROM.Equals(attributeKey))
                {
                    return GetImportDataVersion(jobDto);
                }
                if (DATATYPE.Equals(attributeKey))
                {
                    return GetEIDataType(jobDto);
                }

                if (MAINTENANCDJOBID.Equals(attributeKey))
                {
                    return GetMaintenanceJobID(jobDto);
                }

                if (BACKUPOPTION.Equals(attributeKey))
                {
                    return GetBackupOption(jobDto);
                }

                if (BACKUPMETHOD.Equals(attributeKey))
                {
                    return GetBackupMethod(jobDto);
                }

                if (CREATEPERSISTENTSNAPSHOT.Equals(attributeKey))
                {
                    return GetCreatePersistentSnapshot(jobDto);
                }

                if (VDBENABLED.Equals(attributeKey))
                {
                    return GetVDBEnalbed(jobDto);
                }

                if (RESTOREGRANULARITYLEVEL.Equals(attributeKey))
                {
                    return GetRestoreGranularityLevel(jobDto);
                }

                if (STORAGEPOLICY.Equals(attributeKey))
                {
                    return GetStoragePolicy(jobDto);
                }

                if (SOURCELOGICALDEVICE.Equals(attributeKey))
                {
                    return GetSourceLogicalDevice(jobDto);
                }

                if (LOGICALDEVICE.Equals(attributeKey))
                {
                    return GetLogicalDevice(jobDto);
                }

                if (PHYSICALDEVICE.Equals(attributeKey))
                {
                    return GetPhysicalDevice(jobDto);
                }

                if (RESTORETYPE.Equals(attributeKey))
                {
                    return GetRestoreType(jobDto);
                }
                if (RESTOREMODE.Equals(attributeKey))
                {
                    return GetRestoreMode(jobDto);
                }
                if (CONCORDANCEFORMAT.Equals(attributeKey))
                {
                    return GetRestoreConcordanceFormat(jobDto);
                }
                if (RESTOREOPTIONS.Equals(attributeKey))
                {
                    return GetRestoreOptions(jobDto);
                }
                if (EXPORTREPORTLOCATION.Equals(attributeKey))
                {
                    return GetExportLocation(jobDto);
                }

                if (STARTTIME.Equals(attributeKey))
                {
                    return new RMJobSummaryRow { Key = I18NEntity.GetString("RM_JS_JM_StartTime"), Value = this.GetTime(gsm , jobDto.StartTime) };
                }

                if (ENDTIME.Equals(attributeKey))
                {
                    return new RMJobSummaryRow { Key = I18NEntity.GetString("RM_JM_EndTime"), Value = (jobDto.FinishTime == 0) ? I18NEntity.GetString("RM_JS_JM_Status_Pending") : this.GetTime(gsm, jobDto.FinishTime) };
                }

                if (TIMEZONE.Equals(attributeKey))
                {
                    //AveTimeZone timeZone = TimeFormatUtil.GetTimeZoneById(jobDto.TimeZoneId);
                    AveTimeZone timeZone = GeneralSettingConfig.GetTimeZoneInforById(jobDto.TimeZoneId);
                    return new RMJobSummaryRow { Key = I18NEntity.GetString("ControlPanel.Service_Time Zone"), Value = timeZone == null ? "" : timeZone.DisplayName };
                }

                //if (PLANMODIFIEDBY.Equals(attributeKey))
                //{
                //    return GetPlanModifiedBy(jobDto);
                //}

                if (JOBOPERATEDBY.Equals(attributeKey))
                {
                    return new RMJobSummaryRow { Key = I18NEntity.GetString("RM_JM_JobRunBy"), Value = jobDto.UserName };
                }
                //if (TARGETFARMSO.Equals(attributeKey))
                //{
                //    string farmName = GetTargetFarmSO(jobDto);
                //    return new RMJobSummaryRow { Key = I18NEntity.GetString("ControlPanel.Service", "Target Farm"), Value = farmName };
                //}

                //if (TRANSFERJOBID.Equals(attributeKey))
                //{
                //    string transferJobId = GetTransferJobId(jobDto);
                //    return new RMJobSummaryRow { Key = I18NEntity.GetString("ControlPanel.Service_a1f28dff-68c1-40bb-98e3-7b7eb1a174ea", "Transfer Job Id"), Value = transferJobId };
                //}

                if (DeleteGroupId.Equals(attributeKey))
                {
                    var groupId = (jobDto as DeleteGroupJobDto).GroupId;
                    return new RMJobSummaryRow { Key = SOI18NResource.Get("ControlPanel.Service_e6b48b35-0029-4fbb-88cd-abf3542f1ac7", "Delete Group ID"), Value = groupId };
                }

                //Dictionary<string, Func<BaseJobDto, string>> actions = AssembleActions(jobDto);

                //if (actions.ContainsKey(attributeKey))
                //{
                //    return new RMJobSummaryRow { Key = I18NEntity.GetString("ControlPanel.Service", attributeKey), Value = actions[attributeKey](jobDto) };
                //}
                return null;
            }
            catch (Exception ex)
            {
                logger.Error("GetJobInfoRow error: {0}", ex.ToString());
                logger.Error("Current job time zone id: {0}", jobDto.TimeZoneId);
                return null;
            }

        }

        #region GetXXX
        private RMJobSummaryRow GetDescription(BaseJobDto jobDto)
        {
            return new RMJobSummaryRow { Key = I18NEntity.GetString("ControlPanel.Service_Description"), Value = jobDto.Detail };
        }
        private RMJobSummaryRow GetMaintenanceJobID(BaseJobDto jobDto)
        {
            return new RMJobSummaryRow { Key = I18NEntity.GetString("ControlPanel.Service_Maintenance Job ID"), Value = "" };
        }
        private RMJobSummaryRow GetBackupMethod(BaseJobDto jobDto)
        {
            return new RMJobSummaryRow { Key = I18NEntity.GetString("ControlPanel.Service_Backup Method"), Value = "" };
        }
        private RMJobSummaryRow GetRestoreGranularityLevel(BaseJobDto jobDto)
        {
            return new RMJobSummaryRow { Key = I18NEntity.GetString("ControlPanel.Service_Restore Granularity Level"), Value = "" };
        }
        private RMJobSummaryRow GetStoragePolicy(BaseJobDto jobDto)
        {
            return new RMJobSummaryRow { Key = I18NEntity.GetString("ControlPanel.Service_Storage Policy"), Value = "" };
        }
        private RMJobSummaryRow GetSourceLogicalDevice(BaseJobDto jobDto)
        {
            return new RMJobSummaryRow { Key = I18NEntity.GetString("ControlPanel.Service_Source Logical Device"), Value = "" };
        }
        private RMJobSummaryRow GetLogicalDevice(BaseJobDto jobDto)
        {
            return new RMJobSummaryRow { Key = I18NEntity.GetString("ControlPanel.Service_Logical Device"), Value = "" };
        }
        private RMJobSummaryRow GetPhysicalDevice(BaseJobDto jobDto)
        {
            return new RMJobSummaryRow { Key = I18NEntity.GetString("ControlPanel.Service_Physical Device"), Value = "" };
        }
        private RMJobSummaryRow GetVDBEnalbed(BaseJobDto jobDto)
        {
            return new RMJobSummaryRow { Key = I18NEntity.GetString("ControlPanel.Service_InstaMount Enabled"), Value = "" };
        }
        private RMJobSummaryRow GetCreatePersistentSnapshot(BaseJobDto jobDto)
        {
            return new RMJobSummaryRow { Key = I18NEntity.GetString("ControlPanel.Service_Create Persistent Snapshots"), Value = "" };
        }
        private RMJobSummaryRow GetRestoreType(BaseJobDto jobDto)
        {
            return new RMJobSummaryRow { Key = I18NEntity.GetString("ControlPanel.Service_Restore Type"), Value = GetRestoreType(((SOJob)jobDto).RestoreType) };
        }
        private RMJobSummaryRow GetRestoreMode(BaseJobDto jobDto)
        {
            return new RMJobSummaryRow { Key = I18NEntity.GetString("ControlPanel.Service_Restore Mode"), Value = "" };
        }
        private RMJobSummaryRow GetRestoreConcordanceFormat(BaseJobDto jobDto)
        {
            return new RMJobSummaryRow { Key = I18NEntity.GetString("ControlPanel.Service_With Concordance Format"), Value = "" };
        }
        private RMJobSummaryRow GetRestoreOptions(BaseJobDto jobDto)
        {
            return new RMJobSummaryRow { Key = I18NEntity.GetString("ControlPanel.Service_Restore Options"), Value = "" };
        }
        private RMJobSummaryRow GetBackupOption(BaseJobDto jobDto)
        {
            return (new RMJobSummaryRow { Key = I18NEntity.GetString("ControlPanel.Service_Backup Option"), Value = "" });
        }
        private RMJobSummaryRow GetImportDataVersion(BaseJobDto jobDto)
        {
            return (new RMJobSummaryRow { Key = I18NEntity.GetString("ControlPanel.Service_Import From"), Value = "" });
        }
        private RMJobSummaryRow GetEIDataType(BaseJobDto jobDto)
        {
            return (new RMJobSummaryRow { Key = I18NEntity.GetString("ControlPanel.Service_Data Type"), Value = "" });
        }
        /// <summary>
        /// 获取根据timezone获取time出现异常的情况
        /// </summary>
        /// <param name="date"></param>
        /// <param name="timeZoneId"></param>
        /// <returns></returns>
        private string GetTime(GeneralSettingModel gsm, long date)
        {
            return GeneralSettingService.ConvertTiksToDateTime(gsm, date, true).SimplifyFormatTime;
        }

        private RMJobSummaryRow GetExportLocation(BaseJobDto jobDto)
        {
            return new RMJobSummaryRow { Key = EXPORTREPORTLOCATION, Value = "" };
        }

        private string GetRestoreType(RestoreType type)
        {
            if (type == RestoreType.InPlace)
            {
                return I18NEntity.GetString("StorageOptimization.Service_In Place");
            }
            else if (type == RestoreType.OutPlace)
            {
                return I18NEntity.GetString("StorageOptimization.Service_Out of Place");
            }
            else if (type == RestoreType.ToFileSystem)
            {
                return I18NEntity.GetString("StorageOptimization.Service_1313f847-5350-48da-8615-94006bb68de4", "Restore to storage policy");
            }
            else
            {
                return I18NEntity.GetString("StorageOptimization.Service_In Place");
            }
        }
        #endregion

        private RMJobSummaryRow GetScope(BaseJobDto jobDto)
        {
            AvePoint.GCommon.Contract.StorageOptimization.Object.SOJob soJob = jobDto as AvePoint.GCommon.Contract.StorageOptimization.Object.SOJob;
            return new RMJobSummaryRow { Key = I18NEntity.GetString("RM_DAM_Scope"), Value = (soJob == null ? "" : soJob.Scope) };
        }

    }
}
