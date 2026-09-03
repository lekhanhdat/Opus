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
using AvePoint.GCommon.Contract.Server.UserRegister;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Bulk;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.UniqueId;
using AvePoint.RA.RAPhysical.Common;
using AvePoint.RA.RAPhysical.User;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.Import
{
    public class PhysicalBulkImportWork : IDisposable
    {
        private RALogger logger = RALogger.GetInstance(typeof(PhysicalBulkImportWork));
        #region Job Param
        private JobType jobType;
        private string jobRunBy;
        private string mCurrentJobId;
        private string mGlobalTimeZoneId; 
        private string physicalRecordsCSVPath;
        private JobResult Result;
        private GeneralSettingModel GeneralSetting;

        private string commomErrorMessage = "RM_TS_SS_Summary";
        private int TotalItemCount = 0;
        private int FailedItemCount = 0;
        private int SuccessItemCount = 0;
        private bool isCosmosBulkOperationEnabled = false;
        private int bulkSize = 0;
        private static readonly int splitTasksNumber = 5;

        private readonly string Column_Name = "Name";
        private readonly string Column_Desc = "Description";
        private readonly string Column_Classification = "Classification";
        private readonly string Column_UniqueId = "UniqueId";
        private readonly string Column_Status = "Status";
        private readonly string Column_ParentId = "ParentId";
        private readonly string Column_HomeLocation = "Home Location";
        private readonly string Column_Barcode = "Barcode";

        private readonly string Column_CreatedTime = "Build in Created time";
        private readonly string Column_ModifiedTime = "Build in Modified time";

        private readonly Regex REGEX_DIGIT_AND_CHAR_AND_SPECIAL_CHAR = new(@"^[\sA-Za-z0-9!""#$%&'()*+,./:;<=>?@[\\\]^_`{|}~-]+$");

        private HashSet<Guid> PhysicalLocationPermission = new HashSet<Guid>();
        private bool IsAdmin = false;
        protected IRMReportManager mReportManager;
        protected IRMReportManager ReportManager
        {
            get
            {
                if (mReportManager == null)
                {
                    mReportManager = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManager;
            }
        }
        #endregion
        protected IRMSecurityGroupDao SecurityGroupDao => PlatformWindsorManager.GetService<IRMSecurityGroupDao>();
        protected UserService userService = new UserService();

        #region IOC
        public IAccountWrapperService AccountWrapperService { get; set; } = PlatformWindsorManager.GetService<IAccountWrapperService>();
        private IRMLocationDao LocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();
        public ITemplateManagementService TemplateManagementService { set; get; } = PlatformWindsorManager.GetService<ITemplateManagementService>();
        public IExplorerDao ExplorerDao { set; get; } = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao(true);
        public IPhysicalRecordSettingDao PhysicalRecordSettingDao { set; get; } = PlatformWindsorManager.GetService<IPhysicalRecordSettingDao>();
        public IRecordLoanAllianceDao RecordLoanAllianceDao { set; get; } = PlatformWindsorManager.GetService<IRecordLoanAllianceDao>();
        public IRecordImportSettingDao ImportSettingDao { set; get; } = PlatformWindsorManager.GetService<IRecordImportSettingDao>();
        public IRMManagedRecordRelatedDao recordRelatedDao { set; get; } = PlatformWindsorManager.GetService<IRMManagedRecordRelatedDao>();
        public IAccountDao accountDao { set; get; } = PlatformWindsorManager.GetService<IAccountDao>();
        public ITermGroupDao TermGroupDao { set; get; } = PlatformWindsorManager.GetService<ITermGroupDao>();
        public ITermDao TermDao { get; set; } = PlatformWindsorManager.GetService<ITermDao>();
        public ITermSetDao TermSetDao { set; get; } = PlatformWindsorManager.GetService<ITermSetDao>();
        public ITermSetMembershipDao TermSetMembershipDao { get; set; } = PlatformWindsorManager.GetService<ITermSetMembershipDao>();

        public IGeneralSettingService GeneralSettingService { set; get; } = PlatformWindsorManager.GetService<IGeneralSettingService>();
        public IRMTemplateDao TemplateDao { get; set; } = PlatformWindsorManager.GetService<IRMTemplateDao>();

        public IPhysicalUniqueIdSettingDao PhysicalUniqueIdSettingDao { get; set; } = PlatformWindsorManager.GetService<IPhysicalUniqueIdSettingDao>();

        public IPermissionManagementService PermissionManagementService { get; set; } = PlatformWindsorManager.GetService<IPermissionManagementService>();
        public IRMPhysicalPushColumnDao RMPhysicalPushColumnDao => PlatformWindsorManager.GetService<IRMPhysicalPushColumnDao>();
        public IRMKeyValueDao RMKeyValueDao => (IRMKeyValueDao)PlatformWindsorManager.GetService(typeof(IRMKeyValueDao));

        private IRecordsHistoryService RecordsHistoryService => PlatformWindsorManager.GetService<IRecordsHistoryService>();

        #endregion

        #region Global Import Physical Record Param 
        private string ConflictedResolution = "skip";
        private bool EnableCustomTime = true;
        public string DateTimeFormat = "d/MM/yyyy h:mm tt";
        public string DateFormat = "d/MM/yyyy";
        public string DefaultDateTimeFormat = "d/MM/yyyy h:mm:ss tt";
        public string TimeZoneId = "AUS Eastern Standard Time";
        private TimeZoneInfo _timeZone;
        public TimeZoneInfo GTimeZoneInfo
        {
            get
            {
                if (_timeZone == null)
                {
                    try
                    {
                        _timeZone = GeneralSettingConfig.FindSystemTimeZoneById(this.TimeZoneId);
                    }
                    catch
                    {
                        _timeZone = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(a => a.DisplayName == TimeZoneId);
                    }
                }
                return _timeZone;
            }
        }
        public double DefaultLocationSize = 1000.0;
        public double DefaultBoxSize = 1.0;

        private static int BoundedCapacity = 2000;//10
        private int BulkQueryDbRecordsSize = 100;//2
        private BlockingCollection<WrapperRecord> RecordCaches = new(BoundedCapacity);
        private Dictionary<int,RMTemplate> AllTemplates;

        #endregion


        #region Init

        public PhysicalBulkImportWork(RMImportJobMessage msg)
        {
            this.jobType = msg.JobType;
            this.jobRunBy = msg.JobRunBy;
            mCurrentJobId = msg.JobID;
            mGlobalTimeZoneId = msg.GlobalTimeZoneId;
            ReportMangerFactory.Instance.Init(mCurrentJobId, this.jobType);
            AllTemplates = TemplateDao.FindAll().ToDictionary(t => t.Id, t => t);
            Result = new JobResult();
            GeneralSetting = GeneralSettingService.GetGeneralSettingAsync().Result;
            if (GeneralSetting != null)
            {
                TimeZoneId = GeneralSetting.TimeZoneId;
                DateTimeFormat = GeneralSettingService.GetDateTimeFormat(GeneralSetting);
                logger.Info($"Init time zone id {TimeZoneId}, datetime format {DateTimeFormat}");
            }
            else
            {
                logger.Warn("Can not get general setting from control panel.");
            }
            ConflictedResolution = msg.SharePointSettingID == 1 ? "skip" : "override";
            logger.Info($"Init conflict resoltion {ConflictedResolution}");
            EnableCustomTime = msg.EnableCustomTimeId == 1;
            logger.Info($"Init enable custom time [{EnableCustomTime}]");
            try
            {
                physicalRecordsCSVPath = JobReportUtility.GetImportJobCSVFile(msg.PhysicalRecordsCSVPath);
            }
            catch (Exception e)
            {
                logger.Error("can not download file:{0},error:{1}", msg.PhysicalRecordsCSVPath, e.ToString());
                throw;
            }


            //默认初始化 进度为2
            ReportManager.Increase(2);
            ReportManager.StartUpdateJobProgress();
        }

        private async Task InitUserPermission()
        {
            try
            {
                var userIds = await userService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                var userPermission = SecurityGroupDao.GetUserScopePermissions(userIds);
                IsAdmin = userPermission.IsAdmin;
                if (!IsAdmin)
                {
                    logger.Info("start load Physical permission location ids");
                    var phyPermission = userPermission.ScopePermissionInfo?.Where(_ => _.DataSourceType == SourceFlag.Physical).FirstOrDefault() ?? new();
                    var locationScopeIds = phyPermission?.ScopeIds ?? new List<Guid>();
                    var physicalBottomPermissionIds = LocationDao.LoadAllLocationBottomIdUnderTopLocation(locationScopeIds);
                    PhysicalLocationPermission = new HashSet<Guid>(physicalBottomPermissionIds);
                }
            }
            catch(Exception e)
            {
                logger.Error($"InitUserPermission have error: {e}");
                IsAdmin = false;
            }
        }

        private Dictionary<string, List<string[]>> ReadExcel()
        {
            Dictionary<string, List<string[]>> datas = new Dictionary<string, List<string[]>>();
            try
            {
                using (FileStream fs = new FileStream(physicalRecordsCSVPath, FileMode.Open))
                {
                    if (physicalRecordsCSVPath.EndsWith("csv"))
                    {
                        List<string[]> temp = new List<string[]>();
                        using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                        {
                            while (!sr.EndOfStream)
                            {
                                string csvLine = sr.ReadLine();
                                if(csvLine != null) temp.Add(CSVHelper.AnalyseCSVRow2Array(csvLine));
                            }
                        }
                        datas.Add("csv", temp);
                    }
                    else if (physicalRecordsCSVPath.EndsWith("xlsx"))
                    {
                        int contentBeforeHeaderRowsCount = 1;
                        datas = ExcelUtil.ReadExcelWithHeader(fs, contentBeforeHeaderRowsCount);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw new Exception("Failed to read file conntent");
            }
            return datas;
        }
        #endregion

        #region Generate or Fake UNIQUE ID
        private async Task BulkValidateNameAndSaveAsync()
        {
            int[] availableStatus = [(int)RMRecordStatus.Active, (int)RMRecordStatus.Closed, (int)RMRecordStatus.Destroyed, (int)RMRecordStatus.Missing];
            var bulkWrapperRecords = new List<WrapperRecord>();
            
            foreach (var batchCacheRecord in RecordCaches.GetConsumingEnumerable().Batch(BulkQueryDbRecordsSize))
            {
                try
                {
                    logger.Debug($"Take one batch record to bulk list");

                    bulkWrapperRecords.AddRange(batchCacheRecord);
                    if (bulkWrapperRecords.Count >= BulkQueryDbRecordsSize || RecordCaches.IsAddingCompleted)
                    {
                        logger.Info($"Bulk operate records count: {bulkWrapperRecords.Count}");
                        var existUniqueIds = new List<string>();
                        var existBarcodes = new List<string>();
                        var actionAuditList = new List<PhysicalRecordActionAudit>();

                        async Task SaveOneRecord(WrapperRecord wRecord)
                        {
                            var detail = wRecord.Detail;
                            try
                            {
                                var rec = wRecord.Record;

                                if (!string.IsNullOrEmpty(wRecord.Barcode))
                                {
                                    ValidateBarcode(rec.RecordsId, wRecord.Barcode);
                                }

                                if (existUniqueIds.Contains(rec.RecordsId))
                                {
                                    throw new Exception("RM_Phy_Import_UniqueIdDuplicateError");
                                }

                                if (existBarcodes.Contains(rec.RecordsId))
                                {
                                    throw new Exception("RM_Phy_Import_UniqueIdUsedByBarcode");
                                }

                                if(existBarcodes.Contains(wRecord.Barcode))
                                {
                                    throw new Exception("RM_Phy_Import_BarcodeDuplicateError");
                                }

                                if (existUniqueIds.Contains(wRecord.Barcode))
                                {
                                    throw new Exception("RM_Phy_Import_BarcodeUsedByUniqueId");
                                }

                                using (PerformanceScope scope = new PerformanceScope("Save one record --Add to bulk queue", addToStatistics: true))
                                {
                                    await SaveOrUpdateAsync(rec, wRecord.IsUpdate, wRecord.GenerateNewId, wRecord.TemplateDto, detail);
                                }
                                rec.LeafName_Array = rec.LeafName.ExplorerAnalyzeBuiltInColumn();
                                existUniqueIds.Add(rec.RecordsId);
                                if (!string.IsNullOrEmpty(wRecord.Barcode)) existBarcodes.Add(wRecord.Barcode);
                                if(wRecord.ActionAudit != null)
                                {
                                    actionAuditList.Add(wRecord.ActionAudit);
                                }
                                logger.Info("Add physical record successfully, id {0}, unique id {1}", rec?.Id, rec?.RecordsId);
                            }
                            catch (JobStopException)
                            {
                                throw new JobStopException("This Job is stopped.");
                            }
                            catch (InputParameterException ex)
                            {
                                detail.Status = JobDetailsStatus.Failed;
                                detail.Comment = ex.Message;
                                Result.HasFailed = true;
                                Interlocked.Increment(ref FailedItemCount);
                                logger.Warn(ex.ToString());
                            }
                            catch (SkipItemException ex)
                            {
                                detail.Status = JobDetailsStatus.Skipped;
                                detail.Comment = ex.Message;
                            }
                            catch (GCommon.Utility.AveException ae)
                            {
                                detail.Status = JobDetailsStatus.Failed;
                                detail.Comment = ae.Message;
                                Result.HasFailed = true;
                                Interlocked.Increment(ref FailedItemCount);
                                logger.Error(@"Update physical record failed.Line:[{0}],Error:{1}", wRecord.RowNumber + 1, ae);
                            }
                            catch (Exception e)
                            {
                                detail.Status = JobDetailsStatus.Failed;
                                detail.Comment = e.Message;
                                Result.HasFailed = true;
                                Interlocked.Increment(ref FailedItemCount);
                                logger.Error(@"Update physical record failed.Line:[{0}],Error:{1}", wRecord.RowNumber + 1, e);
                            }
                            finally
                            {
                                ReportManager.Increase();
                                if (!CheckJobStatusUtility.isStopping)
                                {
                                    logger.Debug("Write success report");
                                    if (detail.Status != JobDetailsStatus.Successful)
                                    {
                                        ReportManager.SendJobDetail(detail);
                                    }
                                }
                            }
                        }
                        foreach (var rowData in bulkWrapperRecords)
                        {
                            using (new CheckJobStopScope()) { }
                            await SaveOneRecord(rowData);
                        }
                        RecordsHistoryService.AddPhysicalAudit(actionAuditList);
                        bulkWrapperRecords.Clear();
                    }
                }
                catch (JobStopException)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    logger.Error($"Bulk operate error:{e}");
                }
            }
        }

        private void ValidateBarcode(string uniqueId, string barcode)
        {
            if (barcode.Length > 26 || !REGEX_DIGIT_AND_CHAR_AND_SPECIAL_CHAR.IsMatch(barcode))
            {
                throw new Exception("RM_Phy_Import_BarcodeFormatError");
            }

            var barcodeRecords = new List<Record>();

            barcodeRecords.AddRange(ExplorerDao.QueryAll(r => barcode.Equals(r.CustomColumnDic[DefaultColumnIDs.Barcode].Value, StringComparison.InvariantCultureIgnoreCase)
                || barcode.Equals(r.RecordsId, StringComparison.InvariantCultureIgnoreCase), false).ToList());

            if (barcodeRecords.Count == 0)
            {
                return;
            }

            if (barcodeRecords.Any(a => ((a.CustomColumnDic.ContainsKey(DefaultColumnIDs.Barcode) && a.CustomColumnDic[DefaultColumnIDs.Barcode].Value == barcode) || a.RecordsId == barcode)
                && a.RecordsId != uniqueId))
            {
                throw new Exception("RM_Phy_Import_BarcodeDuplicateError");
            }
        }

        private void InitCosmosBulkOperation()
        {
            isCosmosBulkOperationEnabled = RMKeyValueDao.IsCosmosBulkOperationEnabled();
            bulkSize = RMKeyValueDao.GetCosmosBulkInsertOperationBufferSize();
            if (bulkSize == default(int)) bulkSize = CosmosBulkOperator.DefualtBufferSize;
            logger.Info($"Cosmos bulk operation enabled, bulk size: {bulkSize}");
        }

        private async Task UpdateProcessSucceedRecord(Record record)
        {
            RMTemplate template = null;
            if (AllTemplates.ContainsKey(record.TemplateId))
            {
                template = AllTemplates[record.TemplateId];
            }
            logger.Info($"[TemplateName:{template?.Name}] Update record to db success, the item id:{record?.Id}");
            JMImportPhysicalRecordsJobDetail detail = new JMImportPhysicalRecordsJobDetail()
            {
                SrcRecordType = "N/A",
                DestRecordType = template?.Type.ToString(),
                TemplateName = template?.Name,
                UniqueId = record.RecordsId,
                Barcode = record.CustomColumnDic.TryGetValue(DefaultColumnIDs.Barcode, out var barcode) ? barcode.Value : ((template.Type == TemplateType.Custom || template.Type == TemplateType.Records) ? string.Empty :  record.RecordsId),
                Title = record.LeafName,
                SrcLocation = record.DirPath,
                LocationFullPath = record.DirPath,
                Status = JobDetailsStatus.Successful,
                Comment = record.SpecialComment
            };
            ReportManager.SendJobDetail(detail);
            Result.HasSuccessful = true;
            Interlocked.Increment(ref SuccessItemCount);
        }

        private void UpdateProcessFailedRecord(Record record, Exception ex)
        {
            RMTemplate template = null;
            if (AllTemplates.ContainsKey(record.TemplateId))
            {
                template = AllTemplates[record.TemplateId];
            }
            logger.Warn($"[TemplateName:{template?.Name}] Update record to db failed, the item id:{record?.Id}, error: {ex}");
            JMImportPhysicalRecordsJobDetail detail = new JMImportPhysicalRecordsJobDetail()
            {
                SrcRecordType = "N/A",
                DestRecordType = template?.Type.ToString(),
                TemplateName = template?.Name,
                UniqueId = record.RecordsId,
                Barcode = record.CustomColumnDic.TryGetValue(DefaultColumnIDs.Barcode, out var barcode) ? barcode.Value : ((template.Type == TemplateType.Custom || template.Type == TemplateType.Records) ? string.Empty : record.RecordsId),
                Title = record.LeafName,
                SrcLocation = record.DirPath,
                LocationFullPath = record.DirPath,
                Status = JobDetailsStatus.Failed,
                Comment = ex.Message
            };
            ReportManager.SendJobDetail(detail);
            Interlocked.Increment(ref FailedItemCount);
        }

        private async Task<(bool,string)> ValidateUniqueIdAsync(string uniqueId, TemplateDto template)
        {
            string newId;
            if (string.IsNullOrEmpty(uniqueId))
            {
                newId = await this.GeneratePhysicalObjectUniqueIdAsync(template.type, template.id.ToString(), template.prefix, template.numberOfDigits);
                logger.Info("unique id is empty, generate new {0}", newId);
                return (true, newId);
            }
            newId = uniqueId;
            return (false, newId);
        }
        private async Task GenerateFakeUniqueIdAsync(TemplateDto template, bool hasGenerateNewId)
        {
            if (!hasGenerateNewId)
            {
                string Id = await this.GeneratePhysicalObjectUniqueIdAsync(template.type, template.id.ToString(), template.prefix, template.numberOfDigits);
                logger.Info("Generate fake Id in import, {0}", Id);
            }
        }

        public async Task<string> GeneratePhysicalObjectUniqueIdAsync(TemplateType type, string templateId, string prefix, int digit)
        {
            var physicalUniqueIdSetting = PhysicalUniqueIdSettingDao.LoadingUniqueIdSetting();
            var isGlobalUniqueId = physicalUniqueIdSetting == null ? false : physicalUniqueIdSetting.IsGlobalSetting;
            if (isGlobalUniqueId)
            {
                var defaultTemplateIds = new Guid[] { new Guid(DefaultTemplateIds.BOX_TEMPLATE_ID), new Guid(DefaultTemplateIds.FOLDER_TEMPLATE_ID), new Guid(DefaultTemplateIds.RECORD_TEMPLATE_ID) };
                var defaultTemplates = await TemplateDao.FindListAsync(t => Enumerable.Contains(defaultTemplateIds, t.UniqueId));
                switch (type)
                {
                    case TemplateType.Box:
                        templateId = defaultTemplates.First(t => t.UniqueId == new Guid(DefaultTemplateIds.BOX_TEMPLATE_ID)).Id.ToString();
                        prefix = physicalUniqueIdSetting.BoxTemplatePrefix;
                        digit = physicalUniqueIdSetting.BoxTemplateNumberOfDigits;
                        break;
                    case TemplateType.Folder:
                        templateId = defaultTemplates.First(t => t.UniqueId == new Guid(DefaultTemplateIds.FOLDER_TEMPLATE_ID)).Id.ToString();
                        prefix = physicalUniqueIdSetting.FolderTemplatePrefix;
                        digit = physicalUniqueIdSetting.FolderTemplateNumberOfDigits;
                        break;
                    case TemplateType.Records:
                        templateId = defaultTemplates.First(t => t.UniqueId == new Guid(DefaultTemplateIds.RECORD_TEMPLATE_ID)).Id.ToString();
                        prefix = physicalUniqueIdSetting.RecordTemplatePrefix;
                        digit = physicalUniqueIdSetting.RecordTemplateNumberOfDigits;
                        break;
                    case TemplateType.Custom:
                        prefix = physicalUniqueIdSetting.CustomTemplatePrefix;
                        digit = physicalUniqueIdSetting.CustomTemplateNumberOfDigits;
                        break;
                    default:
                        break;
                }
            }
            var uid = await UniqueIdGenerator.GenerateUniqueIdAsync(templateId, prefix, digit);
            return uid;
        }

        #endregion

        public async Task ImportPhysicalRecordsAsync()
        {
            logger.Info("Begin to import physical records!");
            JobStatus status = JobStatus.None;
            try
            {
                await InitUserPermission();
                InitCosmosBulkOperation();
                await this.InitMetaAsync();
                logger.Info($"Before read excel memory used: {ProcessUtil.GetProcessMemoryMB()}");
                Dictionary<string, List<string[]>> datas = this.ReadExcel();
                logger.Info($"After read excel memory used: {ProcessUtil.GetProcessMemoryMB()}");

                foreach (KeyValuePair<string, List<string[]>> keyValue in datas)
                {
                    var tempName = keyValue.Value?[0]?[0];
                    try
                    {
                        if (isCosmosBulkOperationEnabled)
                        {
                            logger.Debug($"[TemplateName:{tempName}] CosmosBulkOperator.Instance.Start");
                            CosmosBulkOperator.Instance.Start(bulkSize, UpdateProcessSucceedRecord, UpdateProcessFailedRecord);
                        }
                        RecordCaches = new(BoundedCapacity);
                        Task bulkTask = AveTenantTasks.ExecuteAction(BulkValidateNameAndSaveAsync);
                        logger.Info($"[TemplateName:{tempName}] Process sheet {keyValue.Key}, row count {keyValue.Value.Count}");
                        await ImportPhysicalRecordAsync(keyValue.Key, keyValue.Value);
                        RecordCaches.CompleteAdding();
                        logger.Debug($"[TemplateName:{tempName}] CompleteAdding");
                        await bulkTask;
                        logger.Debug("End Add to bulk queue");
                        using (new CheckJobStopScope()) { }
                    }
                    catch (JobStopException)
                    {
                        throw new JobStopException("This Job is stopped.");
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Process sheet {keyValue.Key} error: {e}");
                    }
                    finally
                    {
                        if (isCosmosBulkOperationEnabled)
                        {
                            CosmosBulkOperator.Instance.Complete();
                            CosmosBulkOperator.Instance.Reset();
                            logger.Debug($"[TemplateName:{tempName}] CosmosBulkOperator.Instance.Complete");
                        }
                    }
                }
                status = Result.HasFailed
                    ? Result.HasSuccessful
                        ? JobStatus.FinishWithException
                        : JobStatus.Failed
                    : JobStatus.Finished;
                System.IO.File.Delete(physicalRecordsCSVPath);
            }
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                status = JobStatus.Failed;
                logger.Error($"Import physical records failed , error {e}");
                throw;
            }
            finally
            {
                string jobComment = (status == JobStatus.FinishWithException || status == JobStatus.Failed)
                    ? commomErrorMessage
                    : string.Empty;
                ReportManager.SetJobFinished(status, jobComment);
                logger.Debug("SetJobFinished");
            }
        }

        #region Location And Templated Dictionary
        Dictionary<string, RMLocation> locationDic;
        List<RMLocation> locationList;
        private void InitLocationDic()
        {
            if (locationDic == null || locationDic.Count == 0)
            {
                locationDic = new Dictionary<string, RMLocation>();
                List<RMLocation> allLocation = LocationDao.GetAllLocations();
                locationList = allLocation;
                foreach (RMLocation lo in allLocation)
                {
                    string fullpath = this.getLocationFullPath(lo.UniqueId);
                    if (!locationDic.ContainsKey(fullpath))
                    {
                        locationDic.Add(fullpath.ToLower(), lo);
                    }
                }
                logger.Debug(string.Join("\n", locationDic.Keys.ToArray()));
            }
        }

        private void InitColumnIndexDic(string[] header, TemplateDto template)
        {
            columnIndexDic.Clear();
            TemplateColumnDto title = this.getColumn(new Guid(DefaultColumnIDs.NameOrTitle), template);
            TemplateColumnDto desc = this.getColumn(new Guid(DefaultColumnIDs.Description), template);
            TemplateColumnDto classification = this.getColumn(new Guid(DefaultColumnIDs.Classification), template);
            TemplateColumnDto status = this.getColumn(new Guid(DefaultColumnIDs.Status), template);
            TemplateColumnDto home = this.getColumn(new Guid(DefaultColumnIDs.HomeLocation), template);
            TemplateColumnDto barcode = this.getColumn(new Guid(DefaultColumnIDs.Barcode), template);

            var headerLength = header.Length;
            if(header[headerLength - 1] == "Modified time")
            {
                columnIndexDic.Add(Column_ModifiedTime, headerLength - 1);
                headerLength--;
            }

            if(header[headerLength - 1] == "Created time")
            {
                columnIndexDic.Add(Column_CreatedTime, headerLength - 1);
                headerLength--;
            }

            for (int i = 0; i < headerLength; i++)
            {
                if(title != null && header[i] == I18NEntity.GetString(title.columnName))
                {
                    columnIndexDic.Add(Column_Name, i);
                }else if (desc != null && header[i] == I18NEntity.GetString(desc.columnName))
                {
                    columnIndexDic.Add(Column_Desc, i);
                }
                else if (status != null && header[i] == I18NEntity.GetString(status.columnName))
                {
                    columnIndexDic.Add(Column_Status, i);
                }
                else if (classification != null && header[i] == I18NEntity.GetString(classification.columnName))
                {
                    columnIndexDic.Add(Column_Classification, i);
                } 
                else if (home != null && header[i] == I18NEntity.GetString(home.columnName))
                {
                    columnIndexDic.Add(Column_HomeLocation, i);
                }
                else if (barcode != null && header[i] == I18NEntity.GetString(barcode.columnName))
                {
                    columnIndexDic.Add(Column_Barcode, i);
                }
                else if (header[i] == I18NEntity.GetString("RM_Template_Column_Name_HomeLocation"))
                {
                    columnIndexDic.Add(Column_HomeLocation, i);
                }
                else if (header[i] == "Unique ID")
                {
                    columnIndexDic.Add(Column_UniqueId, i);
                }
                else if (header[i] == "Parent ID")
                {
                    columnIndexDic.Add(Column_ParentId, i);
                }
                else
                {
                    columnIndexDic.Add(header[i], i);
                }
            }
        }

        #endregion

        #region Import Records
        public async Task ImportPhysicalRecordAsync(string sheetName, List<string[]> sheetData)
        {
            logger.Info("Import physical record sheet {0}, row count {1}", sheetName, sheetData.Count);
            if (sheetData.Count < 3)
            {
                logger.Warn("There is no data in this sheet {0}", sheetName);
                return;
            }
            TotalItemCount += sheetData.Count - 2;
            //ReportManager.IncreaseBase(sheetData.Count - 1);
            string templateName = sheetData[0][0];
            TemplateDto templateDto = await TemplateManagementService.GetTemplateDtosByNameAsync(templateName);
            if(templateDto == null)
            {
                logger.Warn("No Template match the name {0}", templateName);
                return;
            }
            string[] header = sheetData[1];
            this.InitColumnIndexDic(header, templateDto);
            // 行Index
            int rowIndex = 0;
            // 每次循环的次数
            int forCount = 100;
            List<KeyValuePair<int, string[]>> rowDataList = new();
            JMImportPhysicalRecordsJobDetail detail = new JMImportPhysicalRecordsJobDetail();
            detail.SrcRecordType = "N/A";
            detail.TemplateName = templateName;
            detail.DestRecordType = templateDto.type.ToString();
            do
            {
                for (int i = 0; i < forCount; i++)
                {
                    if (rowIndex < 2)
                    {
                        rowIndex++;
                        continue;
                    }
                    if (rowIndex == sheetData.Count)
                    {
                        break;
                    }
                    rowDataList.Add(new(rowIndex, sheetData[rowIndex]));
                    rowIndex++;
                }
                using PerformanceScope scope = new PerformanceScope("PhysicalBulkImportWork.BulkImportItem", $"Import {rowDataList.Count} Items", true);
                if (templateDto.type == TemplateType.Box)
                {
                    await ProcessBoxAsync(rowDataList, templateDto, detail);
                }
                else if (templateDto.type == TemplateType.Folder)
                {
                    await ProcessFolderAsync(rowDataList, templateDto, detail);
                }
                else if (templateDto.type == TemplateType.Records)
                {
                    await ProcessRecordAsync(rowDataList, templateDto, detail);
                }
                else if (templateDto.type == TemplateType.Custom)
                {
                    await ProcessCustomAsync(rowDataList, templateDto, detail);
                }
                // 每次完成清空
                rowDataList.Clear();
            } while (rowIndex < sheetData.Count);
        }

        private async Task<bool> ProcessCustomAsync(List<KeyValuePair<int, string[]>> rowDataList, TemplateDto template, JMImportPhysicalRecordsJobDetail oldDetail)
        {
            RMNodeType nodeType = RMNodeType.PhyCustom;
            async Task ProcessOneCustom(TemplateDto template, int rowNumber, JMImportPhysicalRecordsJobDetail oldDetail, string[] rowData)
            {
                using var scope = new PerformanceScope($"Import One Custom Prepare Save", addToStatistics: true);
                JMImportPhysicalRecordsJobDetail detail = new JMImportPhysicalRecordsJobDetail()
                {
                    DestRecordType = oldDetail.DestRecordType,
                    SrcRecordType = oldDetail.SrcRecordType,
                    TemplateName = oldDetail.TemplateName
                };
                try
                {
                    logger.Debug("Process {0},line number: {1}, {2}", nodeType, rowNumber + 1, string.Join("][", rowData));
                    string uniqueId = rowData[columnIndexDic[Column_UniqueId]];
                    string leafName = rowData[columnIndexDic[Column_Name]];
                    bool generateNewId = string.IsNullOrEmpty(uniqueId); //ValidateUniqueId(rowData[columnIndexDic[Column_UniqueId]], template, out uniqueId);
                    detail.UniqueId = uniqueId;

                    Record rec = null;
                    if (!generateNewId)
                    {
                        rec = ExplorerDao.GetPhysicalRecordByRecordsId(uniqueId);
                    }
                    else
                    {
                        logger.Warn("Unique id is empty, number {0}", rowNumber + 1);
                    }
                    var homeLocation = rowData[columnIndexDic[Column_HomeLocation]];
                    bool isUpdate = this.ValidateConflictUniqueID(ref rec, uniqueId, detail, homeLocation, leafName);
                    rec.RecordStatus = (int)RMRecordStatus.Active;

                    await AssembleBasicColumnAsync(rowData, nodeType, template, rec, detail, isUpdate);

                    await AssembleAdditionalInfoAsync(rowData, nodeType, template, rec, detail);
                    if (!IsAdmin && !PhysicalLocationPermission.Contains(rec.LocationId))
                    {
                        detail.Comment = "RM_Phy_Import_NoPermissionForLocation";
                        detail.Status = JobDetailsStatus.Failed;
                        Result.HasFailed = true;
                        Interlocked.Increment(ref FailedItemCount);
                        return;
                    }
                    logger.Info($"[TemplateName:{template?.name}] Will add physical record to cache, id {rec?.Id} successfully, unique id {rec?.RecordsId}");
                    RecordCaches.Add(new WrapperRecord(rec, isUpdate, generateNewId, template, detail, rowNumber, null));
                    logger.Info($"[TemplateName:{template?.name}] Add physical record to cache, id {rec?.Id} successfully, unique id {rec?.RecordsId}");
                }
                catch (JobStopException)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (InputParameterException ex)
                {
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = ex.Message;
                    Result.HasFailed = true;
                    Interlocked.Increment(ref FailedItemCount);
                    logger.Warn(ex.ToString());
                }
                catch (SkipItemException ex)
                {
                    detail.Status = JobDetailsStatus.Skipped;
                    detail.Comment = ex.Message;
                }
                catch (GCommon.Utility.AveException ae)
                {
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = ae.Message;
                    Result.HasFailed = true;
                    Interlocked.Increment(ref FailedItemCount);
                    logger.Error(@"Update physical record failed.Line:[{0}],Error:{1}", rowNumber + 1, ae);
                }
                catch (Exception e)
                {
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message);
                    Result.HasFailed = true;
                    Interlocked.Increment(ref FailedItemCount);
                    logger.Error(@"Update physical record failed.Line:[{0}],Error:{1}", rowNumber + 1, e);
                }
                finally
                {
                    if (!CheckJobStatusUtility.isStopping)
                    {
                        //If the status is successful, process and log in the task
                        if (detail.Status != JobDetailsStatus.Successful)
                        {
                            ReportManager.SendJobDetail(detail);
                        }
                    }
                }
            }
            using var scope = new PerformanceScope($"Import {rowDataList.Count} Customs Prepare Save", addToStatistics: true);
            int existingItemsPerTask = rowDataList.Count / (splitTasksNumber - 1);
            if (rowDataList.Count > splitTasksNumber)
            {
                AveTenantTasks.RunParallel(rowDataList, existingItemsPerTask, new CancellationTokenSource(), rowData =>
                {
                    ProcessOneCustom(template, rowData.Key, oldDetail, rowData.Value).GetAwaiter().GetResult();
                });
            }
            else
            {
                foreach (var rowData in rowDataList)
                {
                    await ProcessOneCustom(template, rowData.Key, oldDetail, rowData.Value);
                }
            }
            return true;
        }

        private async Task<bool> ProcessRecordAsync(List<KeyValuePair<int, string[]>> rowDataList, TemplateDto template, JMImportPhysicalRecordsJobDetail oldDetail)
        {
            //Size, Classification, CreatedBy, ModifyBy Excel里没有需要自己处理值RMNodeType nodeType = RMNodeType.PhyFile; 
            RMNodeType nodeType = RMNodeType.PhyRecord;
            async Task ProcessOneRecord(TemplateDto template, int rowNumber, JMImportPhysicalRecordsJobDetail oldDetail, string[] rowData)
            {
                using var scope = new PerformanceScope($"Import One Record Prepare Save", addToStatistics: true);
                JMImportPhysicalRecordsJobDetail detail = new JMImportPhysicalRecordsJobDetail()
                {
                    DestRecordType = oldDetail.DestRecordType,
                    SrcRecordType = oldDetail.SrcRecordType,
                    TemplateName = oldDetail.TemplateName
                };
                try
                {
                    logger.Debug("Process {0},line number: {1}, {2}", nodeType, rowNumber + 1, string.Join("][", rowData));
                    string uniqueId = rowData[columnIndexDic[Column_UniqueId]];
                    string leafName = rowData[columnIndexDic[Column_Name]];
                    bool generateNewId = string.IsNullOrEmpty(uniqueId); //ValidateUniqueId(rowData[columnIndexDic[Column_UniqueId]], template, out uniqueId);
                    detail.UniqueId = uniqueId;

                    Record rec = null;
                    if (!generateNewId)
                    {
                        rec = ExplorerDao.GetPhysicalRecordByRecordsId(uniqueId);
                    }
                    else
                    {
                        logger.Warn("Unique id is empty, number {0}", rowNumber + 1);
                    }
                    var homeLocation = rowData[columnIndexDic[Column_HomeLocation]];
                    bool isUpdate = this.ValidateConflictUniqueID(ref rec, uniqueId, detail, homeLocation, leafName);

                    await this.AssembleBasicColumnAsync(rowData, nodeType, template, rec, detail, isUpdate);

                    string containedFolder = null;
                    Record folder = null;
                    if (columnIndexDic.ContainsKey(Column_ParentId))
                    {
                        containedFolder = rowData[columnIndexDic[Column_ParentId]];
                        detail.Container = containedFolder;
                        if (!string.IsNullOrEmpty(containedFolder))
                        {
                            folder = this.GetParentFolderWithRetry(containedFolder);
                        }
                        if (folder == null)
                        {
                            throw new GCommon.Utility.AveException("No folder found with unique id {0}", containedFolder);
                        }
                        // RMNodeType.PhyFile = physical folder, not a record, the RMNodeType.Record is record.
                        if (folder.NodeType != (int)RMNodeType.PhyFile)
                        {
                            throw new GCommon.Utility.AveException("The parent id {0} is not a folder()", containedFolder);
                        }
                    }
                    else
                    {
                        throw new GCommon.Utility.AveException("No Parent ID column found in import file");
                    }
                    rec.LocationId = folder.LocationId;
                    rec.BoxId = folder.BoxId;
                    rec.FileId = folder.Id;
                    rec.ParentId = folder.Id;
                    rec.RecordStatus = folder.RecordStatus;
                    rec.ScopePermissionId = folder.ScopePermissionId;
                    if (folder.Ancestors != null)
                    {
                        var ancesstors = new List<Guid>();
                        ancesstors.AddRange(folder.Ancestors);
                        ancesstors.Add(folder.Id);
                        rec.Ancestors = ancesstors;
                    }
                    detail.LocationFullPath = this.getLocationFullPath(folder.LocationId);
                    rec.DirPath = this.getLocationFullPath(folder.LocationId);
                    if (!IsAdmin && !PhysicalLocationPermission.Contains(rec.LocationId))
                    {
                        detail.Comment = "RM_Phy_Import_NoPermissionForLocation";
                        detail.Status = JobDetailsStatus.Failed;
                        Result.HasFailed = true;
                        Interlocked.Increment(ref FailedItemCount);
                        return;
                    }
                    Dictionary<string, string> metaInfo = await this.AssembleColumnInTemplateAsync(template, rec, rowData, folder.Id, folder.LeafName);
                    rec.MetaInfo = JsonConvert.SerializeObject(metaInfo);
                    var actionAudit = RecordsHistoryService.BuildPhysicalActionAuditForJob(rec.Id, isUpdate ? PhysicalActionType.ImportEdit : PhysicalActionType.ImportCreate, !isUpdate);
                    logger.Info($"[TemplateName:{template?.name}] Will add physical record to cache, id {rec?.Id} successfully, unique id {rec?.RecordsId}");
                    RecordCaches.Add(new WrapperRecord(rec, isUpdate, generateNewId, template, detail, rowNumber, actionAudit));
                    logger.Info($"[TemplateName:{template?.name}] Add physical record to cache, id {rec?.Id} successfully, unique id {rec?.RecordsId}");
                }
                catch (JobStopException)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (InputParameterException ex)
                {
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = ex.Message;
                    Result.HasFailed = true;
                    Interlocked.Increment(ref FailedItemCount);
                    logger.Warn(ex.ToString());
                }
                catch (SkipItemException ex)
                {
                    detail.Status = JobDetailsStatus.Skipped;
                    detail.Comment = ex.Message;
                }
                catch (GCommon.Utility.AveException ae)
                {
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = ae.Message;
                    Result.HasFailed = true;
                    Interlocked.Increment(ref FailedItemCount);
                    logger.Error(@"Update physical record failed.Line:[{0}],Error:{1}", rowNumber + 1, ae);
                }
                catch (Exception e)
                {
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message);
                    Result.HasFailed = true;
                    Interlocked.Increment(ref FailedItemCount);
                    logger.Error(@"Update physical record failed.Line:[{0}],Error:{1}", rowNumber + 1, e);
                }
                finally
                {
                    if (!CheckJobStatusUtility.isStopping)
                    {
                        //If the status is successful, process and log in the task
                        if (detail.Status != JobDetailsStatus.Successful)
                        {
                            ReportManager.SendJobDetail(detail);
                        }
                    }
                }
            }
            using var scope = new PerformanceScope($"Import {rowDataList.Count} Records Prepare Save", addToStatistics: true);
            int existingItemsPerTask = rowDataList.Count / (splitTasksNumber - 1);
            if (rowDataList.Count > splitTasksNumber)
            {
                AveTenantTasks.RunParallel(rowDataList, existingItemsPerTask, new CancellationTokenSource(), rowData =>
                {
                    ProcessOneRecord(template, rowData.Key, oldDetail, rowData.Value).GetAwaiter().GetResult();
                });
            }
            else
            {
                foreach (var rowData in rowDataList)
                {
                    await ProcessOneRecord(template, rowData.Key, oldDetail, rowData.Value);
                }
            }
            return true;
        }

        /// <summary>
        /// Cosmos存入马上查询， 会得不到数据， 因此增加retry操作
        /// </summary>
        /// <param name="recordsId"></param>
        /// <returns></returns>
        private Record GetParentFolderWithRetry(string recordsId)
        {
            Record folder = ExplorerDao.GetPhysicalRecordByRecordsId(recordsId);
            int count = 0;
            while (folder == null && count < 3)
            {
                count++;
                logger.Warn("Get parent folder failed, retry,  count {0}", count);
                Thread.Sleep(1000);
                folder = ExplorerDao.GetPhysicalRecordByRecordsId(recordsId);
            }
            return folder;
        }
        /// <summary>
        /// Cosmos存入马上查询， 会得不到数据， 因此增加retry操作
        /// </summary>
        /// <param name="recordsId"></param>
        /// <returns></returns>
        /*private Record GetParentFolderWithRetry(Guid Id)
        {
            Record folder = ExplorerDao.GetPhysicalRecordById(Id); ;
            int count = 0;
            while (folder == null && count < 3)
            {
                count++;
                logger.Warn("Get parent folder failed, retry,  count {0}", count);
                Thread.Sleep(1000);
                folder = ExplorerDao.GetPhysicalRecordById(Id);
            }
            return folder;
        }*/
        #region Folder

        private TemplateColumnDto getColumn(Guid columnId, TemplateDto template)
        {
            foreach(var cat in template.categories)
            {
                if(cat.columns.Any(a=>a.uniqueId == columnId))
                {
                    return cat.columns.First(a => a.uniqueId == columnId);
                }
            }
            return null;
        }

        #region Location

        private string getLocationFullPath(Guid LocationId)
        {
            if (this.locationList.Any(a => a.UniqueId == LocationId))
            {
                RMLocation location = locationList.First(a => a.UniqueId == LocationId);
                return getLocationFullPath(location);
            }
            logger.Error("No location found with id {0}", LocationId);
            return null;
        }

        private string getLocationFullPath(RMLocation location)
        {
            string dirPath = GetLocationPath(location.DirPath);
            return string.Format("{0}/{1}", dirPath, location.Name);
        }
        private string GetLocationPath(string dirPath)
        {
            var result = string.Empty;
            if (!string.IsNullOrEmpty(dirPath))
            {
                try
                {
                    dirPath = dirPath.TrimEnd('/');
                    List<string> locationIds = dirPath.Split('/').ToList();
                    for (int i = 0; i < locationIds.Count; i++)
                    {
                        int tempId = Convert.ToInt32(locationIds[i]);
                        if (locationList.Any(a => a.Id == tempId))
                        {
                            RMLocation tempLocation = locationDic.Values.First(a => a.Id == tempId);
                            string tempPath = I18NEntity.GetString(tempLocation.Name);
                            if (i == 0)
                            {
                                result = tempPath;
                            }
                            else
                            {
                                result = result + "/" + tempPath;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.Message, e);
                }
            }
            return result;
        }

        #endregion


        private async Task<bool> ProcessFolderAsync(List<KeyValuePair<int, string[]>> rowDataList, TemplateDto template, JMImportPhysicalRecordsJobDetail oldDetail)
        {
            //Size, Classification, CreatedBy, ModifyBy Excel里没有需要自己处理值
            RMNodeType nodeType = RMNodeType.PhyFile;
            async Task ProcessOneFolder(TemplateDto template, int rowNumber, JMImportPhysicalRecordsJobDetail oldDetail, string[] rowData)
            {
                using var scope = new PerformanceScope($"Import One Folder Prepare Save", addToStatistics: true);
                JMImportPhysicalRecordsJobDetail detail = new JMImportPhysicalRecordsJobDetail()
                {
                    DestRecordType = oldDetail.DestRecordType,
                    SrcRecordType = oldDetail.SrcRecordType,
                    TemplateName = oldDetail.TemplateName
                };
                try
                {
                    logger.Debug("Process {0},line number: {1}, {2}", nodeType, rowNumber + 1, string.Join("][", rowData));
                    string uniqueId = rowData[columnIndexDic[Column_UniqueId]];
                    string leafName = rowData[columnIndexDic[Column_Name]];
                    var barcode = rowData[columnIndexDic[Column_Barcode]];
                    bool generateNewId = string.IsNullOrEmpty(uniqueId); //ValidateUniqueId(rowData[columnIndexDic[Column_UniqueId]], template, out uniqueId);
                    detail.UniqueId = uniqueId;

                    if (string.IsNullOrEmpty(barcode))
                    {
                        barcode = uniqueId;
                    }
                    detail.Barcode = barcode;
                    Record rec = null;
                    if (!generateNewId)
                    {
                        rec = ExplorerDao.GetPhysicalRecordByRecordsId(uniqueId);
                    }
                    else
                    {
                        logger.Warn("Unique id is empty, number {0}", rowNumber + 1);
                    }
                    var homeLocation = rowData[columnIndexDic[Column_HomeLocation]];
                    bool isUpdate = this.ValidateConflictUniqueID(ref rec, uniqueId, detail, homeLocation, leafName);

                    await AssembleBasicColumnAsync(rowData, nodeType, template, rec, detail, isUpdate);

                    await AssembleAdditionalInfoAsync(rowData, nodeType, template, rec, detail);
                    if (!IsAdmin && !PhysicalLocationPermission.Contains(rec.LocationId))
                    {
                        detail.Comment = "RM_Phy_Import_NoPermissionForLocation";
                        detail.Status = JobDetailsStatus.Failed;
                        Result.HasFailed = true;
                        Interlocked.Increment(ref FailedItemCount);
                        return;
                    }
                    var actionAudit = RecordsHistoryService.BuildPhysicalActionAuditForJob(rec.Id, isUpdate ? PhysicalActionType.ImportEdit : PhysicalActionType.ImportCreate, !isUpdate);
                    logger.Info($"[TemplateName:{template?.name}] Will add physical record to cache, id {rec?.Id} successfully, unique id {rec?.RecordsId}");
                    RecordCaches.Add(new WrapperRecord(rec, isUpdate, generateNewId, template, detail, rowNumber, actionAudit, barcode));
                    logger.Info($"[TemplateName:{template?.name}] Add physical record to cache, id {rec?.Id} successfully, unique id {rec?.RecordsId}");
                }
                catch (JobStopException)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (InputParameterException ex)
                {
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = ex.Message;
                    Result.HasFailed = true;
                    Interlocked.Increment(ref FailedItemCount);
                    logger.Warn(ex.ToString());
                }
                catch (SkipItemException ex)
                {
                    detail.Status = JobDetailsStatus.Skipped;
                    detail.Comment = ex.Message;
                }
                catch (GCommon.Utility.AveException ae)
                {
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = ae.Message;
                    Result.HasFailed = true;
                    Interlocked.Increment(ref FailedItemCount);
                    logger.Error(@"Update physical record failed.Line:[{0}],Error:{1}", rowNumber + 1, ae);
                }
                catch (Exception e)
                {
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message);
                    Result.HasFailed = true;
                    Interlocked.Increment(ref FailedItemCount);
                    logger.Error(@"Update physical record failed.Line:[{0}],Error:{1}", rowNumber + 1, e);
                }
                finally
                {
                    if (!CheckJobStatusUtility.isStopping)
                    {
                        //If the status is successful, process and log in the task
                        if (detail.Status != JobDetailsStatus.Successful)
                        {
                            ReportManager.SendJobDetail(detail);
                        }
                    }
                }
            }
            using var scope = new PerformanceScope($"Import {rowDataList.Count} Folders Prepare Save", addToStatistics: true);
            int existingItemsPerTask = rowDataList.Count / (splitTasksNumber - 1);
            if (rowDataList.Count > splitTasksNumber)
            {
                AveTenantTasks.RunParallel(rowDataList, existingItemsPerTask, new CancellationTokenSource(), async rowData =>
                {
                    await ProcessOneFolder(template, rowData.Key, oldDetail, rowData.Value);
                });
            }
            else
            {
                foreach (var rowData in rowDataList)
                {
                    await ProcessOneFolder(template, rowData.Key, oldDetail, rowData.Value);
                }
            }
            return true;
        }

        private int GetCreateDate(long timeCreated)
        {
            DateTime date = new DateTime(timeCreated, DateTimeKind.Utc);
            return int.Parse(date.ToString("yyyyMMdd"));
        }
         
      
        #endregion

        #region column Index dictionary
        Dictionary<string, int> columnIndexDic = new Dictionary<string, int>();
        #endregion

        #region Common Assemble
        private bool ValidateConflictUniqueID(ref Record rec, string uniqueId, JMImportPhysicalRecordsJobDetail detail, string homeLocation,string leafName = null)
        {
            if (rec != null)
            {
                detail.Title = rec.LeafName;
                if (!this.IsConflictedOverride())
                {
                    detail.Title = leafName;
                    logger.Warn("Record with UniqueId {0},  already exist, skip.", rec.RecordsId);
                    //add skip report
                    throw new SkipItemException(string.Format("Record with unique ID {0} already exist", rec.RecordsId));
                }
                return true;
            }
            else
            {
                rec = new Record();
                rec.Id = Guid.NewGuid();
                rec.RecordsId = uniqueId;
                rec.DirPath = homeLocation;
                return false;
            }
        }

        private async Task AssembleBasicColumnAsync(string[] rowData, RMNodeType nodeType, TemplateDto template, Record rec, JMImportPhysicalRecordsJobDetail detail, bool isUpdate = false)
        {
            using var scope = new PerformanceScope($"AssembleBasicColumnAsync", addToStatistics: true);
            if (string.IsNullOrEmpty(rowData[columnIndexDic[Column_Name]]))
            {
                throw new InputParameterException("RM_Phy_Import_LeafNameEmpty");
            }
            rec.NodeId = rec.Id;
            rec.LeafName = rowData[columnIndexDic[Column_Name]];
            detail.Title = rec.LeafName;
            rec.NodeType = (int)nodeType;
            //获取Title之后再取Template 
            rec.TemplateId = template.id;
            rec.SourceFlag = (int)SourceFlag.Physical;
            rec.ModifiedBy = await GetAccountDisplayNameAsync(this.jobRunBy);
            rec.CreatedBy = await GetAccountDisplayNameAsync(this.jobRunBy);

            var createdTime = columnIndexDic.TryGetValue(Column_CreatedTime, out var created) ? rowData[created] : string.Empty;
            var modifiedTime = columnIndexDic.TryGetValue(Column_ModifiedTime, out var modified) ? rowData[modified] : string.Empty;
            await AssembleCreatedTimeAndModifiedTime(createdTime, modifiedTime, isUpdate, rec);
        }

        private async Task AssembleCreatedTimeAndModifiedTime(string createdTime, string modifiedTime, bool isUpdate, Record rec)
        {
            var currentTimeTicks = DateTime.UtcNow.Ticks;

            if (isUpdate)
            {
                if (EnableCustomTime)
                {
                    logger.Info("Enable custom time while update record, Record {0}, create time {1} , modified time {2}", rec.RecordsId, rec.TimeCreated, rec.TimeModified);
                    rec.TimeCreated = !string.IsNullOrEmpty(createdTime)
                        ? await GetCreatedAndModifiedTime(createdTime, true)
                        : rec.TimeCreated;
                    rec.TimeModified = !string.IsNullOrEmpty(modifiedTime)
                        ? await GetCreatedAndModifiedTime(modifiedTime,false)
                        : currentTimeTicks;
                    logger.Info("Enable custom time after update record, Record {0}, create time {1} , modified time {2}", rec.RecordsId, rec.TimeCreated, rec.TimeModified);
                }
                else
                {
                    rec.TimeModified = currentTimeTicks;
                    logger.Info("Disable custom time while update record, set time modified to current,Record {0}, create time {1} , modified time {2}", rec.RecordsId, rec.TimeCreated, rec.TimeModified);
                }
            }
            else
            {
                if (EnableCustomTime)
                {
                    logger.Info("Enable custom time while create record, Record {0}, create time {1} , modified time {2}", rec.RecordsId, rec.TimeCreated, rec.TimeModified);
                    rec.TimeCreated = !string.IsNullOrEmpty(createdTime)
                        ? await GetCreatedAndModifiedTime(createdTime, true)
                        : currentTimeTicks;
                    rec.TimeModified = !string.IsNullOrEmpty(modifiedTime)
                        ? await GetCreatedAndModifiedTime(modifiedTime, false)
                        : currentTimeTicks;
                    logger.Info("Enable custom time after create record, Record {0}, create time {1} , modified time {2}", rec.RecordsId, rec.TimeCreated, rec.TimeModified);
                }
                else
                {
                    rec.TimeCreated = currentTimeTicks;
                    rec.TimeModified = currentTimeTicks;
                    logger.Info("Disable custom time while create record, set time created and modified to current,Record {0}, create time {1} , modified time {2}", rec.RecordsId, rec.TimeCreated, rec.TimeModified);
                }
            }

            async Task<long> GetCreatedAndModifiedTime(string timeString, bool isCreatedTime)
            {
                try
                {
                    return await GetCreatedAndModifiedTimeTicks(timeString);
                }
                catch
                {
                    if (isCreatedTime)
                    {
                        throw new InputParameterException("RM_Phy_Import_CreatedTimeError");
                    }

                    throw new InputParameterException("RM_Phy_Import_ModifiedTimeError");
                }
            }
        }

        private Task AssembleAdditionalInfoAsync(string[] rowData, RMNodeType nodeType, TemplateDto template, Record rec, JMImportPhysicalRecordsJobDetail detail)
        {
            using var scope = new PerformanceScope($"AssembleAdditionalInfoAsync", addToStatistics: true);
            string parentNumber = null;
            Record parentRecord = null;
            RMLocation location = null;

            RMTerm mTerm = null;
            if (columnIndexDic.ContainsKey(Column_Classification) && !string.IsNullOrEmpty(rowData[columnIndexDic[Column_Classification]]))
            {
                //从import文件的路径中获取Term
                mTerm = this.getTermByPath(rowData[columnIndexDic[Column_Classification]]);
                if (mTerm == null)
                {
                    detail.Comment = "Failed to analyse term path, inherit from parent.";
                }
                else
                {
                    rec.TermId = mTerm.UniqueId;
                    rec.TermName = mTerm.Name;
                }
            }
            if (columnIndexDic.ContainsKey(Column_ParentId))
            {
                parentNumber = rowData[columnIndexDic[Column_ParentId]];
                if (string.IsNullOrEmpty(parentNumber) && columnIndexDic.ContainsKey(Column_HomeLocation))
                {
                    //Location下的
                    string homeLocation = rowData[columnIndexDic[Column_HomeLocation]];
                    detail.SrcLocation = homeLocation;
                    if (!string.IsNullOrEmpty(homeLocation) && this.locationDic.ContainsKey(homeLocation.ToLower()))
                    {
                        location = locationDic[homeLocation.ToLower()];
                        detail.LocationFullPath = homeLocation;
                        rec.DirPath = homeLocation;
                        if (location.NodeType != (int)RMNodeType.PhysicalBottomLocation)
                        {
                            throw new GCommon.Utility.AveException(string.Format("Location {0} is not bottom level location", location.Name));
                        }
                    }
                    else
                    {
                        throw new GCommon.Utility.AveException(string.Format("The record does not have parent ID, and invalid location {0}", homeLocation));
                    }
                    if (location.NodeType != (int)RMNodeType.PhysicalBottomLocation)
                    {
                        throw new GCommon.Utility.AveException(string.Format("Location {0} is not bottom level location", location.Name));
                    }
                    AssembleFromLocation(rec, location, mTerm);
                }
                else
                {
                    //Parent Record下的
                    detail.Container = parentNumber;
                    parentRecord = ExplorerDao.GetPhysicalRecordByRecordsId(parentNumber);
                    if (parentRecord == null)
                    {
                        throw new GCommon.Utility.AveException("No Record found with unique id {0}", parentNumber);
                    }
                    if(nodeType != RMNodeType.PhyCustom && parentRecord.NodeType != (int)RMNodeType.PhyCustom && parentRecord.NodeType >= (int)nodeType)
                    {
                        throw new Exception("Records cannot be imported to nodes of the same level");
                    }
                    detail.LocationFullPath = this.getLocationFullPath(parentRecord.LocationId);
                    rec.DirPath = this.getLocationFullPath(parentRecord.LocationId);
                    AssembleFromParent(rec, parentRecord, mTerm);
                }
            }
            else
            {
                throw new GCommon.Utility.AveException("No Parent ID column found.");
            }

            if(rec.TermId == Guid.Empty || string.IsNullOrEmpty(rec.TermName))
            {
                throw new Exception("The Classificaion of Records is empty or cannot be found");
            }

            return AssembleMetaInfoAsync(rowData, template, rec, location, parentRecord);
        }

        private async Task AssembleMetaInfoAsync(string[] rowData, TemplateDto template, Record rec, RMLocation location, Record parentRecord)
        {
            if (location != null)
            {
                Dictionary<string, string> metaInfo = await this.AssembleColumnInTemplateAsync(template, rec, rowData, location.UniqueId, location.Name);
                rec.MetaInfo = JsonConvert.SerializeObject(metaInfo);
            }
            else
            {
                Dictionary<string, string> metaInfo = await this.AssembleColumnInTemplateAsync(template, rec, rowData, parentRecord.Id, parentRecord.LeafName);
                rec.MetaInfo = JsonConvert.SerializeObject(metaInfo);
            }
        }

        private async Task SaveOrUpdateAsync(Record rec, bool isUpdate, bool generateNewId, TemplateDto template, JMImportPhysicalRecordsJobDetail detail)
        {
            (var validated, string uniqueId) = await ValidateUniqueIdAsync(rec.RecordsId, template);
            if (validated)
            {
                rec.RecordsId = uniqueId;
            }
            detail.UniqueId = uniqueId;
            if (!isUpdate)
            {
                await GenerateFakeUniqueIdAsync(template, generateNewId);
            }
            else
            {
                if (rec.CreateDate != GetCreateDate(rec.TimeCreated))
                {
                    ExplorerDao.Delete(rec.CreateDate, rec.Id);
                    rec.CreateDate = 0;
                }
            }

            if (isCosmosBulkOperationEnabled)
            {
                rec.SpecialComment = detail.Comment;
                CosmosBulkOperator.Instance.Add(rec);
            }
            else
            {
                ExplorerDao.Upsert(rec);
            }
        }
        /// <summary>
        /// 从parent中获取Ancestors等信息，如果当前term没有值，那么也会从parent获取term
        /// </summary>
        /// <param name="rec"></param>
        /// <param name="parent"></param>
        /// <param name="mTerm"></param>
        private void AssembleFromParent(Record rec, Record parent, RMTerm mTerm)
        {
            rec.LocationId = parent.LocationId;    
            rec.ParentId = parent.Id;
            rec.ScopePermissionId = parent.ScopePermissionId;

            if (parent.NodeType == (int)RMNodeType.PhyBox)
            {
                rec.BoxId = parent.Id;
            }
            if (parent.Ancestors != null)
            {
                var ancesstors = new List<Guid>();
                ancesstors.AddRange(parent.Ancestors);
                ancesstors.Add(parent.Id);
                rec.Ancestors = ancesstors;
            }
            if (mTerm == null)
            {
                AssembleTermFromParent(rec, parent);
            }
        }
        private void AssembleTermFromLocation(Record rec, RMLocation location)
        {
            TaxonomyColumnValue termInfo = this.GetDefaultTermId(location);
            rec.TermId = new Guid(termInfo.Id);
            rec.TermName = termInfo.Name;
        }

        private void AssembleTermFromParent(Record rec, Record parent)
        {
            rec.TermId = parent.TermId;
            rec.TermName = parent.TermName;
        }
        /// <summary>
        /// 从location中获取location id， parent id， ancestors， 如果mTerm为空，那么会尝试从location中获取term
        /// </summary>
        /// <param name="rec"></param>
        /// <param name="location"></param>
        /// <param name="mTerm"></param>
        private void AssembleFromLocation(Record rec, RMLocation location, RMTerm mTerm)
        {
            rec.ParentId = location.UniqueId;
            rec.LocationId = location.UniqueId;
            rec.BoxId = Guid.Empty;
            rec.Ancestors = new List<Guid> { location.UniqueId };
            rec.ScopePermissionId = PermissionManagementService.GetScopePermissionId(location.Id.ToString());
            if (mTerm == null)
            {
                AssembleTermFromLocation(rec, location);
            } 
        }
        #endregion

        #region Box

        private async Task<bool> ProcessBoxAsync(List<KeyValuePair<int, string[]>> rowDataList, TemplateDto template, JMImportPhysicalRecordsJobDetail oldDetail)
        {
            async Task ProcessOneBox(TemplateDto template, int rowNumber, JMImportPhysicalRecordsJobDetail oldDetail, string[] rowData)
            {
                using var scope = new PerformanceScope($"Import One Box Prepare Save", addToStatistics: true);
                JMImportPhysicalRecordsJobDetail detail = new JMImportPhysicalRecordsJobDetail()
                {
                    DestRecordType = oldDetail.DestRecordType,
                    SrcRecordType = oldDetail.SrcRecordType,
                    TemplateName = oldDetail.TemplateName
                };
                try
                {
                    RMNodeType nodeType = RMNodeType.PhyBox;
                    string uniqueId = rowData[columnIndexDic[Column_UniqueId]];
                    string leafName = rowData[columnIndexDic[Column_Name]];
                    var barcode = rowData[columnIndexDic[Column_Barcode]];
                    logger.Debug("Process {0},line number: {1}, {2}", nodeType, rowNumber + 1, string.Join("][", rowData));
                    bool generateNewId = string.IsNullOrEmpty(uniqueId); //ValidateUniqueId(rowData[columnIndexDic[Column_UniqueId]], template, out uniqueId);
                    detail.UniqueId = uniqueId;

                    if (string.IsNullOrEmpty(barcode))
                    {
                        barcode = uniqueId;
                    }
                    detail.Barcode = barcode;
                    Record rec = null;
                    if (!generateNewId)
                    {
                        using PerformanceScope scope1 = new($"Get One Physical Record from DB", addToStatistics: true);
                        rec = ExplorerDao.GetPhysicalRecordByRecordsId(uniqueId);
                    }
                    else
                    {
                        logger.Warn("Unique id is empty, number {0}", rowNumber + 1);
                    }
                    var homeLocation = rowData[columnIndexDic[Column_HomeLocation]];
                    bool isUpdate = this.ValidateConflictUniqueID(ref rec, uniqueId, detail, homeLocation, leafName);

                    await AssembleBasicColumnAsync(rowData, nodeType, template, rec, detail, isUpdate);
                    await AssembleAdditionalInfoAsync(rowData, nodeType, template, rec, detail);
                    if (!IsAdmin && !PhysicalLocationPermission.Contains(rec.LocationId))
                    {
                        detail.Comment = "RM_Phy_Import_NoPermissionForLocation";
                        detail.Status = JobDetailsStatus.Failed;
                        Result.HasFailed = true;
                        Interlocked.Increment(ref FailedItemCount);
                        return;
                    }
                    var actionAudit = RecordsHistoryService.BuildPhysicalActionAuditForJob(rec.Id, isUpdate ? PhysicalActionType.ImportEdit : PhysicalActionType.ImportCreate, !isUpdate);
                    logger.Info($"[TemplateName:{template?.name}] Will add physical record to cache, id {rec?.Id} successfully, unique id {rec?.RecordsId}");
                    RecordCaches.Add(new WrapperRecord(rec, isUpdate, generateNewId, template, detail, rowNumber, actionAudit, barcode));
                    logger.Info($"[TemplateName:{template?.name}] Add physical record to cache, id {rec?.Id} successfully, unique id {rec?.RecordsId}");
                }
                catch (JobStopException)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (InputParameterException ex)
                {
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = ex.Message;
                    Result.HasFailed = true;
                    Interlocked.Increment(ref FailedItemCount);
                    logger.Warn(ex.ToString());
                }
                catch (SkipItemException ex)
                {
                    detail.Status = JobDetailsStatus.Skipped;
                    detail.Comment = ex.Message;
                }
                catch (GCommon.Utility.AveException ae)
                {
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = ae.Message;
                    Result.HasFailed = true;
                    Interlocked.Increment(ref FailedItemCount);
                    logger.Error(@"Update physical record failed.Line:[{0}],Error:{1}", rowNumber + 1, ae);
                }
                catch (Exception e)
                {
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message);
                    Result.HasFailed = true;
                    Interlocked.Increment(ref FailedItemCount);
                    logger.Error(@"Update physical record failed.Line:[{0}],Error:{1}", rowNumber + 1, e);
                }
                finally
                {
                    if (!CheckJobStatusUtility.isStopping)
                    {
                        //If the status is successful, process and log in the task
                        if (detail.Status != JobDetailsStatus.Successful)
                        {
                            ReportManager.SendJobDetail(detail);
                        }
                    }
                }
            }

            using var scope = new PerformanceScope($"Import {rowDataList.Count} Boxes Prepare Save", addToStatistics: true);
            int existingItemsPerTask = rowDataList.Count / (splitTasksNumber - 1);
            if (rowDataList.Count > splitTasksNumber)
            {
                AveTenantTasks.RunParallel(rowDataList, existingItemsPerTask, new CancellationTokenSource(), rowData =>
                {
                    ProcessOneBox(template, rowData.Key, oldDetail, rowData.Value).GetAwaiter().GetResult();
                });
            }
            else
            {
                foreach (var rowData in rowDataList)
                {
                    await ProcessOneBox(template, rowData.Key, oldDetail, rowData.Value);
                }
            }
            return true;
        }

        #endregion

        private bool IsConflictedOverride()
        {
            if ("override".Equals(this.ConflictedResolution, StringComparison.OrdinalIgnoreCase) || "overwrite".Equals(this.ConflictedResolution, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
         
        private DateTime GetDateClosedTimeDate(string[] rowData)
        {
            if (columnIndexDic.ContainsKey(I18NEntity.GetString("RM_Template_Column_Name_DataClosed")))
            {
                string modifiedTime = rowData[columnIndexDic[I18NEntity.GetString("RM_Template_Column_Name_DataClosed")]];
                return this.GetTimeLocal(modifiedTime);
            }
            return DateTime.MinValue;
        }
        private ChoiceColumnValue AssemleRecordFormat(string formatStr, TemplateColumnDto template)
        {
            ChoiceColumnValue colValue = null;
            try
            {
                Dictionary<int, string> options = JsonConvert.DeserializeObject<Dictionary<int, string>>(template.optionsJSON);
                if (options.Any(a => a.Value.Equals(formatStr, StringComparison.OrdinalIgnoreCase) || I18NEntity.GetString(a.Value) == formatStr))
                {
                    KeyValuePair<int, string> option = options.First(a => a.Value.Equals(formatStr, StringComparison.OrdinalIgnoreCase) || I18NEntity.GetString(a.Value) == formatStr);
                    colValue = new ChoiceColumnValue() { Value = option.Key.ToString(), Name = I18NEntity.GetString(option.Value) };
                    return colValue;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
            logger.Warn("Convert format:{0} failed, use default Document", formatStr);
            return new ChoiceColumnValue() { Name = "Document", Value = "1" };
        }

        private async Task<Dictionary<string, string>> AssembleColumnInTemplateAsync(TemplateDto template, Record rec, string[] rowData, Guid locationId, string locationName)
        {
            Dictionary<string, string> metaInfo = new Dictionary<string, string>();
            foreach (TemplateCategoryDto cat in template.categories)
            {
                foreach (TemplateColumnDto col in cat.columns)
                {
                    if ("RM_Template_Column_Name_Title" == col.columnName || I18NEntity.GetString("RM_Template_Column_Name_Title").Equals(col.columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        metaInfo.Add(col.uniqueId.ToString(), this.ReplaceEnterInExcel(rec.LeafName));   //TRIM TITLE SUPPORT BREAK ROW
                    }
                    else if ("RM_Template_Column_Name_Capability" == col.columnName || I18NEntity.GetString("RM_Template_Column_Name_Capability").Equals(col.columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (columnIndexDic.ContainsKey(I18NEntity.GetString("RM_Template_Column_Name_Capability")))
                        {
                            metaInfo.Add(col.uniqueId.ToString(), rowData[columnIndexDic[I18NEntity.GetString("RM_Template_Column_Name_Capability")]]);
                        }
                    }
                    else if ("RM_Template_Column_Name_HomeLocation" == col.columnName || I18NEntity.GetString("RM_Template_Column_Name_HomeLocation").Equals(col.columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(new TaxonomyColumnValue() { Id = locationId.ToString(), Name = locationName })); //RM_Template_Column_Name_Classification
                    }
                    else if ("RM_Template_Column_Name_Classification" == col.columnName)
                    {
                        metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(new TaxonomyColumnValue() { Id = rec.TermId.ToString(), Name = rec.TermName }));
                    }
                    else if ("RM_Template_Column_Name_Status" == col.columnName && columnIndexDic.ContainsKey(Column_Status))
                    {
                        string recordsStatus = rowData[columnIndexDic[Column_Status]];
                        int statusInt = GetStauts(recordsStatus);
                        rec.RecordStatus = statusInt;
                        ChoiceColumnValue statusFiled = new ChoiceColumnValue()
                        {
                            Value = statusInt.ToString(),
                            Name = GetStautsName(statusInt)
                        };
                        metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(statusFiled));
                    }
                    else if ("RM_Template_Column_Name_Format" == col.columnName || I18NEntity.GetString("RM_Template_Column_Name_Format").Equals(col.columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (columnIndexDic.ContainsKey(I18NEntity.GetString("RM_Template_Column_Name_Format")))
                        {
                            string formatStr = rowData[columnIndexDic[I18NEntity.GetString("RM_Template_Column_Name_Format")]];
                            ChoiceColumnValue formatFiled = this.AssemleRecordFormat(formatStr, col);
                            metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(formatFiled));
                        }
                    }
                    else if ("RM_Template_Column_Name_DataClosed" == col.columnName || I18NEntity.GetString("RM_Template_Column_Name_DataClosed").Equals(col.columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        DateTime closedTime = GetDateClosedTimeDate(rowData);
                        if (closedTime != DateTime.MinValue)
                        {
                            DateTimeColumnValue timeColumn = new DateTimeColumnValue() { Date = closedTime, TimeZoneId = this.TimeZoneId, IsSetDayLight = true };
                            metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(timeColumn));
                        }
                    }
                    else if ("RM_PRM_PRE_Column_Barcode" == col.columnName || I18NEntity.GetString("RM_PRM_PRE_Column_Barcode").Equals(col.columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        if(template.type == TemplateType.Box || template.type == TemplateType.Folder)
                        {
                            string barcode = rowData[columnIndexDic[Column_Barcode]];
                            metaInfo.Add(col.uniqueId.ToString(), barcode ?? rowData[columnIndexDic[Column_UniqueId]]);
                        }
                    }
                    else if (col.allowEdit && this.columnIndexDic.ContainsKey(I18NEntity.GetString(col.columnName)))  //allow edit说明不是默认Column
                    {
                        string colValue = rowData[columnIndexDic[I18NEntity.GetString(col.columnName)]];                       
                        if (col.typeId == (int)AvePoint.RA.Contract.TemplateManagement.ColumnType.SingleText
                            || col.typeId == (int)AvePoint.RA.Contract.TemplateManagement.ColumnType.MultipleText)
                        {
                            if (string.IsNullOrEmpty(colValue))
                            {
                                logger.Warn($"Value of column {col.columnName} is null or empty");
                                continue;
                            }
                            metaInfo.Add(col.uniqueId.ToString(), this.ReplaceEnterInExcel(colValue));
                        }
                        else if (col.typeId == (int)AvePoint.RA.Contract.TemplateManagement.ColumnType.Number)
                        {
                            double tempDouble;
                            if(double.TryParse(colValue, out tempDouble))
                            {
                                metaInfo.Add(col.uniqueId.ToString(), colValue);
                            }
                            else
                            {
                                logger.Warn("Invalid number {0}", colValue);
                                if (col.required)
                                {
                                    logger.Warn($"required number column {col.columnName}, add default value 0");
                                    metaInfo.Add(col.uniqueId.ToString(), "0");
                                }
                            }
                        }
                        else if (col.typeId == (int)AvePoint.RA.Contract.TemplateManagement.ColumnType.DateTime)
                        {
                            DateTime localTime = this.GetTimeLocal(colValue);
                            if (localTime != DateTime.MinValue)
                            {
                                DateTimeColumnValue timeColumn = new DateTimeColumnValue() { Date = localTime, TimeZoneId = this.TimeZoneId, IsSetDayLight = true };
                                metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(timeColumn));
                            } 
                            else if (col.required)
                            {
                                var localNow = DateTimeUtil.ConvertTimeFromUtc(DateTime.UtcNow, GeneralSetting);
                                logger.Warn($"required datetime column {col.columnName}, add default value {DateTime.Now} / {localNow}");
                                DateTimeColumnValue timeColumn = new DateTimeColumnValue() { Date = localNow, TimeZoneId = this.TimeZoneId, IsSetDayLight = true };
                                metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(timeColumn));
                            }
                        }
                        else if (col.typeId == (int)AvePoint.RA.Contract.TemplateManagement.ColumnType.PeopleOrGroup)
                        {
                            if (string.IsNullOrEmpty(colValue))
                            {
                                logger.Warn($"Value of column {col.columnName} is null or empty");
                                continue;
                            }
                            string[] tempUsers = colValue.Split(';');
                            List<PeopleColumnValue> accounts = new List<PeopleColumnValue>();
                            foreach(string temp in tempUsers)
                            {
                                RMAccount account = await GetAccountFromDicAsync(temp);
                                if (account == null)
                                {
                                    logger.Error("No user found in Records with princple name or display name {0}", account.Id);
                                } 
                                PeopleColumnValue people = GetAosUser(account, temp);
                                accounts.Add(people);

                            }
                            metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(accounts)); 
                        }
                        else if (col.typeId == (int)AvePoint.RA.Contract.TemplateManagement.ColumnType.SingleChoice)
                        {
                            bool sucess = false;
                            Dictionary<int, string> options = JsonConvert.DeserializeObject<Dictionary<int, string>>(col.optionsJSON);
                            logger.Debug($"Diagnose log. optionsJSON:{col.optionsJSON} columnName:{col.columnName}");
                            var columnName = I18NEntity.GetString(col.columnName);
                            if (columnIndexDic.ContainsKey(columnName))
                            {
                                string optionVal = rowData[columnIndexDic[columnName]];
                                logger.Debug($"Diagnose log. optionVal:{optionVal}");
                                foreach (KeyValuePair<int, string> option in options)
                                {
                                    if (optionVal != null && (option.Value.Equals(optionVal) || I18NEntity.GetString(option.Value) == optionVal))
                                    {
                                        ChoiceColumnValue formatFiled = new ChoiceColumnValue() { Value = option.Key.ToString(), Name = option.Value };
                                        metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(formatFiled));
                                        sucess = true;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                logger.Debug($"Diagnose log. columnName not found:{string.Join(",", columnIndexDic.Keys.ToList())}");
                            }

                            if(col.required && !sucess)
                            {
                                ChoiceColumnValue formatFiled = new ChoiceColumnValue() { Value = options.First().Key.ToString(), Name = options.First().Value };
                                metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(formatFiled));
                                logger.Warn($"required single choice column {col.columnName}, add default value {options.First().Value}");
                            }
                        }
                        else if (col.typeId == (int)AvePoint.RA.Contract.TemplateManagement.ColumnType.MultipleChoice)
                        {
                            bool sucess = false;
                            Dictionary<int, string> options = JsonConvert.DeserializeObject<Dictionary<int, string>>(col.optionsJSON);
                            var columnName = I18NEntity.GetString(col.columnName);
                            if (columnIndexDic.ContainsKey(columnName))
                            {
                                string optionVal = rowData[columnIndexDic[columnName]];
                                List<ChoiceColumnValue> choiceList = new List<ChoiceColumnValue>();
                                foreach (KeyValuePair<int, string> option in options)
                                {
                                    string[] optionsVal = optionVal != null ? optionVal.Split(';') : new string[0];
                                    foreach(string temp in optionsVal)
                                    {
                                        if (temp!= string.Empty && (option.Value.Equals(temp) || I18NEntity.GetString(option.Value) == temp))
                                        {
                                            ChoiceColumnValue formatFiled = new ChoiceColumnValue() { Value = option.Key.ToString(), Name = option.Value };
                                            choiceList.Add(formatFiled);
                                        }
                                    }
                                }
                                if(choiceList.Count > 0)
                                {
                                    sucess = true;
                                    metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(choiceList));
                                }
                            }
                            if (col.required && !sucess)
                            {
                                ChoiceColumnValue formatFiled = new ChoiceColumnValue() { Value = options.First().Key.ToString(), Name = options.First().Value };
                                metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(new List<ChoiceColumnValue>() { formatFiled }));
                                logger.Warn($"required multi choice column {col.columnName}, add default value {options.First().Value}");
                            }
                        }
                        else
                        {
                            logger.Debug("Not mapped column type {0}", (Contract.Explorer.ColumnType)col.typeId);
                        }
                    }
                    else
                    {
                        logger.Debug("Record File or column mapping file does not contains column {0}", col.columnName);
                    }
                }
            }
            // 增加PushColumn逻辑
            AddPushColumnToDB(template, rec, metaInfo);
            return metaInfo;
        }

        /// <summary>
        /// 修改DB PushColumn 数据
        /// </summary>
        /// <param name="template"></param>
        /// <param name="record"></param>
        /// <param name="metaInfo"></param>
        private void AddPushColumnToDB(TemplateDto template, Record record, Dictionary<string, string> metaInfo)
        {
            var pushColumnCollection = new Dictionary<Guid, TemplateColumnDto>();
            template.categories.ForEach(cat =>
            {
                cat.columns.ForEach(col =>
                {
                    logger.Info($"foreach column, {col.columnName}, {col.pushToChild}, {col.inheritFromParent}, {col.inheritFromParentFolder}");
                    if (col.pushToChild || col.inheritFromParent || col.inheritFromParentFolder)
                    {
                        logger.Info($"add push column to db, {col.columnName}, {col.pushToChild}, {col.inheritFromParent}, {col.inheritFromParentFolder}");
                        pushColumnCollection[col.uniqueId] = col;
                    }
                });
            });
            if (pushColumnCollection.Count > 0)
            {
                foreach (var pushColumn in pushColumnCollection)
                {
                    string pushColumnValue;
                    metaInfo.TryGetValue(pushColumn.Key.ToString(), out pushColumnValue);
                    logger.Info($"get push column value, {pushColumn.Key}, {pushColumnValue}");
                    var physicalObjectId = record.Id;
                    if (pushColumn.Value.inheritFromParent)
                    {
                        physicalObjectId = record.BoxId;
                    }
                    else if (pushColumn.Value.inheritFromParentFolder)
                    {
                        physicalObjectId = record.FileId;
                    }

                    RMPhysicalPushColumnDao.AddOrUpdate(new RMPhysicalPushColumn()
                    {
                        ColumnUniqueId = pushColumn.Key,
                        TemplateId = template.id,
                        ColumnValue = pushColumnValue,
                        PhysicalObjectId = physicalObjectId,
                    });
                    logger.Info($"finsh to update push column, {pushColumn.Key}, {template.id}, {pushColumnValue}, {physicalObjectId}");
                }
            }
        }

        /// <summary>
        /// 将Excel单元格中的回车符， 替换成文本中的回车符
        /// </summary>
        private string ReplaceEnterInExcel(string value)
        {
            if (value != null)
            {
                return value.Replace("_x000D_", "\r");
            }
            return value;
        }

        private PeopleColumnValue GetAosUser(RMAccount account, string notFoundUserName)
        {
            if (account != null)
            {
                return new PeopleColumnValue()
                {
                    DisplayName = account.DisplayName,
                    RMUserId = account.Id,
                    Email = account.UserPrincipalName,
                    UserName = account.UserPrincipalName,
                    UserId = account.UserId,
                    UserPrincipalName = account.UserPrincipalName
                };
            }
            else
            {
                return new PeopleColumnValue()
                {
                    DisplayName = notFoundUserName,
                };
            }
        }
        private readonly object locker = new object();
        private Dictionary<string, RMAccount> accountDictionary = new Dictionary<string, RMAccount>();
        private async Task<RMAccount> GetAccountFromDicAsync(string recordUserName)
        {
            RMAccount account = null;
            lock (locker)
            {
                if (accountDictionary.ContainsKey(recordUserName))
                {
                    account = accountDictionary[recordUserName];
                }
                else
                {
                    account = accountDao.GetUserForImportAsync(recordUserName).Result;
                    if (account != null)
                    {
                        accountDictionary.Add(recordUserName, account);
                    }
                    else
                    { 
                        Contract.Object.AADAccount aadUser = AccountWrapperService.SearchAccounts(TenantLocalValue.LogonGroupId, recordUserName, 1).OrderBy(o => o.DisplayName).FirstOrDefault(); 
                        if (aadUser != null)
                        {
                            account = new RMAccount() {
                                AADId = aadUser.Id,
                                UserId = aadUser.InviteType == AccountType.Group ? aadUser.Id : aadUser.AccountId,//change this for loginuser under azure ad group can't get account id in aos.
                                UserPrincipalName = aadUser.UserPrincipalName ?? aadUser.Mail,
                                DisplayName = aadUser.DisplayName,
                                ObjectType = aadUser.InviteType == AccountType.Group ? RMActiveDirectoryObjectType.Group : RMActiveDirectoryObjectType.User,
                            };
                        }
                        else
                        {
                            account = new RMAccount { DisplayName = recordUserName, UserPrincipalName = recordUserName };
                        }
                        accountDictionary.Add(recordUserName, account);
                    }
                }
            }
            return account;
        }

        private async Task<long> GetCreatedAndModifiedTimeTicks(string time)
        {
            if (!DateTime.TryParse(time, out DateTime temp))
            {
                if (!DateTime.TryParseExact(time, this.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out temp))
                {
                    if (!DateTime.TryParseExact(time, this.DefaultDateTimeFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out temp))
                    {
                        throw new Exception();
                    }
                }
            }
            return DateTimeUtil.ConvertTimeToUtc(temp, this.TimeZoneId, false);
        }

        private async Task<string> GetAccountDisplayNameAsync(string princpleName)
        {
            RMAccount account = await this.GetAccountFromDicAsync(princpleName);
            if (account == null)
            {
                logger.Warn("Can no found account {0} in aos accounts", princpleName);
                return princpleName;
            }
            else
            {
                return account.DisplayName;
            }
        }
     
        private int GetStauts(string statusStr)
        {
            if ("Open".Equals(statusStr, StringComparison.OrdinalIgnoreCase))
            {
                return (int)RMRecordStatus.Active;
            }
            else if ("Closed".Equals(statusStr, StringComparison.OrdinalIgnoreCase) || "Close".Equals(statusStr, StringComparison.OrdinalIgnoreCase))
            {
                return (int)RMRecordStatus.Closed;
            }
            else if ("Destroyed".Equals(statusStr, StringComparison.OrdinalIgnoreCase) || "Destroy".Equals(statusStr, StringComparison.OrdinalIgnoreCase))
            {
                return (int)RMRecordStatus.Destroyed;
            }
            else if ("Missing".Equals(statusStr, StringComparison.OrdinalIgnoreCase) || "Miss".Equals(statusStr, StringComparison.OrdinalIgnoreCase))
            {
                return (int)RMRecordStatus.Missing;
            }
            return (int)RMRecordStatus.Active;
        }
        private string GetStautsName(int statusInt)
        {
            RMRecordStatus status = (RMRecordStatus)statusInt;
            if (status == RMRecordStatus.Active)
            {
                return I18NEntity.GetString("RM_PRM_PRE_Column_Status_Open");
            }
            else if (status == RMRecordStatus.Closed)
            {
                return I18NEntity.GetString("RM_PRM_PRE_Column_Status_Closed");
            }
            else if (status == RMRecordStatus.Destroyed)
            {
                return I18NEntity.GetString("RM_PRM_PRE_Column_Status_Destroyed");
            }
            else if (status == RMRecordStatus.Missing)
            {
                return I18NEntity.GetString("RM_PRM_PRE_Column_Status_Missing");
            }
            return "None";
        }

       
        private DateTime GetTimeLocal(string time)
        {
            DateTime temp = DateTime.MinValue;
            if (string.IsNullOrEmpty(time))
            {
                return temp;
            }
            if (!DateTime.TryParseExact(time, this.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out temp))
            {
                if (!DateTime.TryParseExact(time, this.DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out temp))
                {
                    if (!DateTime.TryParse(time, out temp))
                    {
                        logger.Error("Parse time failed, {0}", time);
                        return temp;
                    }
                }
            }
            return temp;
        }

        private readonly Dictionary<int, RMPhysicalRecordSetting> GlocalSettingDic = new Dictionary<int, RMPhysicalRecordSetting>();
        private readonly object _globalPhysicalSettingLock = new object();
        private TaxonomyColumnValue GetDefaultTermId(RMLocation location)
        {
            RMLocation temp = location;
            RMLocation parent = this.locationDic.Values.First(a => a.Id == temp.ParentId);
            while (parent.NodeType != (int)RMNodeType.PhysicalRootLocation)
            {
                temp = parent;
                parent = this.locationDic.Values.First(a => a.Id == temp.ParentId);
            }
            logger.Info("Home Location is {0}", temp?.UniqueId);
            if (!GlocalSettingDic.TryGetValue(temp.Id, out RMPhysicalRecordSetting setting))
            {
                lock (_globalPhysicalSettingLock)
                {
                    if (!GlocalSettingDic.TryGetValue(temp.Id, out setting))
                    {
                        RMPhysicalRecordSetting topLevelSetting = PhysicalRecordSettingDao.GetPhysicalRecordSetting(temp.UniqueId);
                        if (topLevelSetting == null)
                        {
                            logger.Error("Location {0} does not have physcial setting , get default term failed.", temp?.UniqueId);
                            throw new GCommon.Utility.AveException("No physical setting found on location {0}", temp.Name);
                        }
                        GlocalSettingDic[temp.Id] = topLevelSetting;
                        setting = topLevelSetting;
                    }
                }
            }

            if (setting != null)
            {
                return new TaxonomyColumnValue() { Id = setting.DefaultTermId.ToString(), Name = setting.DefaultTermName };
            }

            logger.Error("No Global physcial setting on location {0}", temp?.UniqueId);
            throw new Exception(string.Format("Term is invalid and no Global physcial setting on location {0}", temp.Name));
        }

        #endregion

        #region  Term Specify

        private async Task InitTermCacheAsync()
        {
            if (gTermGroup.IsNullOrEmpty())
            {
                gTermGroup = TermGroupDao.LoadTermGroup(false);
            }
            if (gTermSet.IsNullOrEmpty())
            {
                gTermSet = await TermSetDao.FindListAsync(a => a.IsRemoved == false);
            }
            if (gCachedTerm.IsNullOrEmpty())
            {
                gCachedTerm = await TermDao.FindListAsync(a => a.IsRemoved == false);
            }
        }
        private RMTerm getTermByPath(string termPath)
        {
            try
            {
                logger.Info("Start to analyze term path:{0}", termPath);
                string[] temp = termPath?.Split('|');
                if (temp == null || temp.Length < 3)
                {
                    logger.Warn("Invalid term path {0}", termPath);
                    return null;
                }
                string termGroupName = temp[0];
                string termSetName = temp[1];
                RMTermGroup termGroup = gTermGroup.FirstOrDefault(a => string.Equals(a.Name, termGroupName, StringComparison.OrdinalIgnoreCase));
                if (termGroup == null)
                {
                    logger.Warn("Can not find term group {0}", termGroupName);
                    return null;
                }
                RMTermSet termSet = gTermSet.FirstOrDefault(a => a.TermGroupId == termGroup.UniqueId && string.Equals(a.Name, termSetName, StringComparison.OrdinalIgnoreCase));
                if (termSet == null)
                {
                    logger.Warn("Can not find term set:{0} in term group: {1}", termSetName, termGroupName);
                    return null;
                }
                string[] termArray = new string[temp.Length - 2];
                for (int i = 0; i < temp.Length; i++)
                {
                    if (i > 1)
                    {
                        termArray[i - 2] = temp[i];
                    }
                }
                return getTermByArrary(termSet, termGroup, termArray);
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
            }
            return null;
        }
        List<RMTermGroup> gTermGroup = new List<RMTermGroup>();
        List<RMTermSet> gTermSet = new List<RMTermSet>();
        List<RMTerm> gCachedTerm = new List<RMTerm>();
        Dictionary<Guid, List<RMTermSetMembership>> gTermMembership = new Dictionary<Guid, List<RMTermSetMembership>>();

        private RMTerm getTermByArrary(RMTermSet termSet, RMTermGroup termGroup, string[] termArray)
        {
            logger.Info("Term path after anylyse:{0}", string.Join("/", termArray));
            RMTerm tempTerm = null;
            Guid parentUniqueId = termSet.UniqueId;
            int parentId = termSet.Id;
            for (int i = 0; i < termArray.Length; i++)
            {
                List<RMTermSetMembership> memberships = GetMembership(parentUniqueId, parentId, i == 0);
                tempTerm = gCachedTerm.FirstOrDefault(a => termNameEquals(a.Name, termArray[i]) && memberships.Any(m => m.TermId == a.Id));
                if (tempTerm == null)
                {
                    logger.Error("Can not find term {0} in termset {1}, group {2}", termArray[i], termSet.Name, termGroup.Name);
                    return tempTerm;
                }
                logger.Debug("Get term by name {0}", termArray[i]);
                parentUniqueId = tempTerm.UniqueId;
                parentId = tempTerm.Id;
            }
            return tempTerm;
        }
        private List<RMTermSetMembership> GetMembership(Guid uniqueId, int parentId, bool isRootTerm)
        {
            if (gTermMembership.ContainsKey(uniqueId))
            {
                return gTermMembership[uniqueId];
            }
            else
            {
                List<RMTermSetMembership> list = null;
                if (isRootTerm)
                {
                    list = TermSetMembershipDao.GetSubTermMembershipsByTermSetId(parentId);
                }
                else
                {
                    list = TermSetMembershipDao.GetSubTermMembershipByTermId(parentId);
                }
                gTermMembership[uniqueId] = list;
                return list;
            }
        }
        /// <summary>
        /// TermName中&符， 存入数据库再取出， 会变成全角的， 替换之后再比较
        /// </summary> 
        private bool termNameEquals(string t1, string t2)
        {
            string newT1 = t1.Replace('＆', '&');
            string newT2 = t2.Replace('＆', '&');
            return string.Equals(newT1, newT2);
        }
        #endregion

        #region Init Mapping Meta before import record
      

        public async Task<bool> InitMetaAsync()
        {
            InitLocationDic();
            await InitTermCacheAsync();
            
            return true;
        }
        #endregion
         


        #region Dispose method
        public void Dispose()
        {

        }
        #endregion
    }

    class JobResult
    {
        public bool HasFailed { get; set; }

        public bool HasSuccessful { get; set; }

        public int FolderCount { get; set; }

        public int FileCount { get; set; }
    }

    enum BuildInColumnType
    {
        UniqueId,
        ParentId,

    }
}
