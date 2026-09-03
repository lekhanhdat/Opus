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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;

namespace RAGoogle.GoogleExplorer
{
    public class GoogleFolderReclassifier : GoogleReclassifyBaseProcessor
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(GoogleFolderReclassifier));
        //private int _failedCount = 0;
        //private bool _jobHasStopped = false;
        //private int _succeedCount = 0;
        private int _classificationLevel = 0;
        private bool _mChangeAllFile = false;
        private ChangeTermDto _jobContextDto;
        private IJobInfoUpdater _jobInfoUpdater;
        private List<RMGoogleSetting> _googleSettings = new List<RMGoogleSetting>();
        private IRMFunctionSettingDao FunctionSettingDao = PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
        //private List<Guid> rootFolders = new List<Guid>();      
        protected IJobInfoUpdater JobInfoUpdater
        {
            get
            {
                if (_jobInfoUpdater == null)
                {
                    _jobInfoUpdater = (IJobInfoUpdater)PlatformWindsorManager.GetService(typeof(IJobInfoUpdater));
                }
                return _jobInfoUpdater;
            }
        }
        private IRMSubJobDao _subJobDao;
        public IRMSubJobDao SubJobDao
        {
            get
            {
                if (_subJobDao == null)
                {
                    _subJobDao = (IRMSubJobDao)PlatformWindsorManager.GetService(typeof(IRMSubJobDao));
                }
                return _subJobDao;
            }
        }

        private IRMGoogleSettingDao _GoogleSettingDao { get; set; }
        public IRMGoogleSettingDao GoogleSettingDao
        {
            get
            {
                if (_GoogleSettingDao == null)
                {
                    _GoogleSettingDao = (IRMGoogleSettingDao)PlatformWindsorManager.GetService(typeof(IRMGoogleSettingDao));
                }
                return _GoogleSettingDao;
            }
        }

        public GoogleFolderReclassifier(string jobId, ChangeTermDto dto)
        {
            _jobId = jobId;
            RMSubJob subJobWithContext = SubJobDao.GetSubJob(jobId, true);
            _jobContextDto = dto;
            ChangeLabelDtoInfo = dto;
            TenantLocalValue.LogonUserId = _jobContextDto.UserId;
            JobInfoUpdater.UpdateJobState(_jobId, (int)JobStatus.InProgress);
            JobInfoUpdater.UpdateJobProgress(_jobId, 1);
            ReportManager.StartUpdateJobProgress();
            logger.Info("Object count from the message is :{0}", _jobContextDto.GoogleDriveRecordIds.Count);
            _classificationLevel = this.GetClassificationLevel();
            _mChangeAllFile = _jobContextDto.OverWriteSubFiles;
            _googleSettings = GoogleSettingDao.GetAllSettings();
            _googleSettings.ForEach(o => { o.FullPath = EncodeUtil.DecryptByCommunicationKey(o.FullPath); });
        }

        public int GetClassificationLevel()
        {
            RMFunctionSetting setting;
            FunctionSettingDao.TryGet(AvePoint.RA.Contract.FunctionSetting.FunctionSettingType.ClassificationLevelSetting, out setting);
            RMNodeLevel result;
            if (setting == null)
            {
                return (int)RMNodeLevel.GoogleFile;
            }
            if (Enum.TryParse<RMNodeLevel>(setting.SettingInfo, out result))
            {
                return (int)result;
            }
            return (int)RMNodeLevel.GoogleFolder;
        }
        public void Dispose()
        {

        }

        public async Task RunForGlobalSearchActionAsync(List<Record> folderRecords, Dictionary<Guid, List<Record>> allFolderFiles)
        {
            var fileList = allFolderFiles.Values.SelectMany(files => files).ToList();
            if (fileList.IsNullOrEmpty())
            {
                FailedCount++;
                FailedItems.AddRange(folderRecords);
                RecordsHistoryService.AddRecordsHistory(FailedItems.Select(item => item.Id).ToList(), "RM_JS_Audit_ChangeLabelNoFileErrorMessage", _jobContextDto.Comment);

                foreach (var record in folderRecords)
                {
                    ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
                    {
                        ObjectName = record.LeafName,
                        FullPath = GetFullPath(record),
                        Action = "RM_JS_BCM_Explorer_ChangeLabel",
                        Status = JobDetailsStatus.Failed,
                        Comment = "RM_JS_Audit_ChangeLabelNoFileErrorMessage",
                        Type = "RM_JS_Rule_ObjectLevel_Document"
                    });
                }

                logger.Warn($"No files found in folder, skipping folder reclassification process.");
                return;
            }

            await InitAsync(_jobContextDto.TermInfo.UniqueId.ToString());
            ReportManager.IncreaseBase(fileList.Count);
            ReportManager.Increase();

            var recordsNoLabel = fileList.Where(r => string.IsNullOrEmpty(r.TermName)).ToList();
            var records = _jobContextDto.OverWriteSubFiles ? fileList : recordsNoLabel;

            if (_jobContextDto.OverWriteSubFiles && !records.Any())
            {
                FailedCount++;
                FailedItems.AddRange(folderRecords);
                RecordsHistoryService.AddRecordsHistory(FailedItems.Select(item => item.Id).ToList(), "RM_JS_Audit_ChangeLabelNoFileErrorOverwriteMessage", _jobContextDto.Comment);

                foreach (var record in folderRecords)
                {
                    ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
                    {
                        ObjectName = record.LeafName,
                        FullPath = GetFullPath(record),
                        Action = "RM_JS_BCM_Explorer_ChangeLabel",
                        Status = JobDetailsStatus.Failed,
                        Comment = "RM_JS_Audit_ChangeLabelNoFileErrorOverwriteMessage",
                        Type = "RM_JS_Rule_ObjectLevel_Document"
                    });
                }

                logger.Warn($"There are no files to overwrite in the folder, skipping folder reclassification process.");
                return;
            }

            AllFolderFiles = allFolderFiles;
            await HandleRecords(records, ChangeLabelDtoInfo.TermInfo.UniqueId, ChangeLabelDtoInfo.TermInfo.Name);

            AddProcessReclassifyItemsToHistory(_jobContextDto);
            logger.Info("Finished processing Google folders for reclassification.");
        }
        public override async Task JobReportSuccessfulAction(Record record, Guid sourceTermId)
        {
            AddSucceedDetail(record, sourceTermId);
            SucceedItems.Add(record);
            await Task.CompletedTask;
        }
        public override async Task JobReportFailedAction(Record record, Exception ex)
        {
            AddFailedDetail(record, ex);
            FailedItems.Add(record);
            await Task.CompletedTask;
        }
    }
}
