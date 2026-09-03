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
using Aspose.Email.Storage.Pst;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Import;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RAPhysical.User;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public class RMTrimImport : IDisposable
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMTrimImport));
        #region Job Param
        private JobType jobType;
        private string jobRunBy;
        private string mCurrentJobId;
        private string mGlobalTimeZoneId;
        private AvePoint.RA.SharePoint.Object.JobResult Result;
        private string physicalRecordsCSVPath;

        private string commomErrorMessage = "RM_TS_SS_Summary";
        private int TotalItemCount = 0;
        private int FailedItemCount = 0;
        private int SuccessItemCount = 0;


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

        #region IOC
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        public IAccountWrapperService AccountWrapperService { get; set; } = PlatformWindsorManager.GetService<IAccountWrapperService>();
        private IRMLocationDao LocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();
        private ITemplateManagementService TemplateManagementService => PlatformWindsorManager.GetService<ITemplateManagementService>();
        public IExplorerDao ExplorerDao { set; get; } = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao(true);
        private IPhysicalRecordSettingDao PhysicalRecordSettingDao => PlatformWindsorManager.GetService<IPhysicalRecordSettingDao>();
        private IRecordLoanAllianceDao RecordLoanAllianceDao => PlatformWindsorManager.GetService<IRecordLoanAllianceDao>();
        public IRMManagedRecordRelatedDao recordRelatedDao { set; get; } = PlatformWindsorManager.GetService<IRMManagedRecordRelatedDao>();
        public IAccountDao accountDao => PlatformWindsorManager.GetService<IAccountDao>();
        public ITermGroupDao TermGroupDao => PlatformWindsorManager.GetService<ITermGroupDao>();
        public ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        public ITermSetDao TermSetDao => PlatformWindsorManager.GetService<ITermSetDao>();
        public ITermSetMembershipDao TermSetMembershipDao => PlatformWindsorManager.GetService<ITermSetMembershipDao>();

        private IRecordsHistoryService RecordsHistoryService => PlatformWindsorManager.GetService<IRecordsHistoryService>();
        #endregion

        #region Global Import Physical Record Param
        public List<RecordTypeMapping> RecordTypeMappings;
        public List<ColumnValueMapping> ColumnValueMappings;
        public List<UserMapping> UserMappings;
        public ColumnMapping CustomTeplateColumnMapping;
        public ColumnMapping BoxColumnMapping;
        public ColumnMapping FolderColumnMapping;
        public ColumnMapping RecordColumnMapping;
        private string ConflictedResolution = "skip";
        public string DateTimeFormat = "d/MM/yyyy h:mm tt";
        public string DateFormat = "d/MM/yyyy";
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
        private int ClassificationClumnIndex = -1;

        private const int ActionAuditBatchSize = 100;
        private List<PhysicalRecordActionAudit> ActionAuditList = new();
        #endregion

        private HashSet<Guid> PhysicalLocationPermission = new HashSet<Guid>();
        private bool IsAdmin = false;

        protected IRMSecurityGroupDao SecurityGroupDao => PlatformWindsorManager.GetService<IRMSecurityGroupDao>();
        protected UserService userService = new UserService();

        public RMTrimImport(RMImportJobMessage msg)
        {
            this.jobType = msg.JobType;
            this.jobRunBy = msg.JobRunBy;
            mCurrentJobId = msg.JobID;
            mGlobalTimeZoneId = msg.GlobalTimeZoneId;
            ReportMangerFactory.Instance.Init(mCurrentJobId, this.jobType);

            //reportManager.BaseJobDto = new BaseJobDto() { Id = mCurrentJobId, JobType = (int)jobType };
            Result = new AvePoint.RA.SharePoint.Object.JobResult();
            //InitMapping();
            switch (jobType)
            {
                case JobType.ImportPhysicalRecords:
                    #region ImportPhysicalRecords
                    //physicalRecordsCSVPath = msg.PhysicalRecordsCSVPath;

                    try
                    {
                        physicalRecordsCSVPath = JobReportUtility.GetImportJobCSVFile(msg.PhysicalRecordsCSVPath);
                    }
                    catch (Exception e)
                    {
                        logger.Error("can not download file:{0},error:{1}", msg.PhysicalRecordsCSVPath, e.ToString());
                        throw;
                    }

                    #endregion
                    break;
                default:
                    break;
            }

            //默认初始化 进度为2
            ReportManager.Increase(2);
            ReportManager.StartUpdateJobProgress();
        }

        private Dictionary<string,List<string[]>> ReadExcel()
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
                        datas = ExcelUtil.ReadExcelWithHeader(fs);
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

        public async Task ImportPhysicalRecordsAsync()
        {
            await this.InitUserPermission();
            await this.InitMetaAsync();
            if (this.RecordTypeMappings == null)
            {
                throw new GCommon.Utility.AveException("Please import mapping infomation first.");
            }
            JobStatus status = JobStatus.None;
            try
            {
                Dictionary<string, List<string[]>> datas = this.ReadExcel();
                foreach (KeyValuePair<string, List<string[]>> keyValue in datas)
                {
                    logger.Info("Process sheet {0}, row count {1}", keyValue.Key, keyValue.Value.Count);
                    await ImportPhysicalRecordAsync(keyValue.Key, keyValue.Value);
                }
                status = Result.HasFailed
                    ? Result.HasSuccessful
                        ? JobStatus.FinishWithException
                        : JobStatus.Failed
                    : JobStatus.Finished;
                System.IO.File.Delete(physicalRecordsCSVPath);
            }
            catch (Exception e)
            { 
                throw e;
            }
            finally
            {

                string jobComment = (status == JobStatus.FinishWithException || status == JobStatus.Failed)
                    ? commomErrorMessage
                    : string.Empty;
                ReportManager.SetJobFinished(status, jobComment);

            }
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
            catch (Exception e)
            {
                logger.Error($"InitUserPermission have error: {e}");
                IsAdmin = false;
            }
        }
         #region Import Records

        #region Location And Templated Dictionary
                /// <summary>
                /// key为一个path，与上传文件中的location path一致, 不包含根节点My Registered Locations，例如Test Store/Area 1/Row 1/Bay 1/Shelf 1，
                /// </summary>
                Dictionary<string, RMLocation> locationDic;
        private void InitLocationDic()
        {
            if (locationDic?.Count > 0) return;

            locationDic = new Dictionary<string, RMLocation>();
            List<RMLocation> allLocation = LocationDao.GetAllLocations();
            var rootLocation = allLocation.First(l => l.ParentId == 0);
            locationDic.Add(rootLocation.Name, rootLocation);
            var firstLevelLocations = allLocation.Where(l => l.ParentId == rootLocation.Id).ToList();
            foreach (RMLocation lo in firstLevelLocations)
            {
                AssembleLocationDic(allLocation, lo, string.Empty);
            }
            //foreach (RMLocation lo in allLocation)
            //{
            //    if (!locationDic.ContainsKey(lo.Name))
            //    {
            //        locationDic.Add(lo.Name, lo);
            //    }
            //}
        }

        private void AssembleLocationDic(List<RMLocation> allLocation, RMLocation location, string dirPath)
        {
            var path = string.IsNullOrEmpty(dirPath) ? location.Name : $"{dirPath}/{location.Name}";
            if (!locationDic.ContainsKey(path)) locationDic[path] = location;
            foreach (RMLocation lo in allLocation.Where(l => l.ParentId == location.Id))
            {
                AssembleLocationDic(allLocation, lo, path);
            }
        }

        /// <summary>
        /// 每个级别默认的Template
        /// </summary>
        Dictionary<RMNodeType, TemplateDto> templateDic = new Dictionary<RMNodeType, TemplateDto>();
        private async Task InitTemplateDicAsync()
        {
            if (templateDic == null || templateDic.Count == 0)
            {
                List<TemplateDto> templates = await TemplateManagementService.GetAllTemplateDtosAsync();
                foreach (TemplateDto temp in templates)
                {
                    RMNodeType nodeType = convertTemplateType2NodeType(temp.type);
                    if (isDefault(nodeType, temp.uniqueId) && !templateDic.ContainsKey(nodeType))
                    {
                        logger.Info("Get default template for node type {0},  {1}", nodeType, temp.name);
                        templateDic.Add(nodeType, temp);
                    }
                }
            }
        }
        private bool isDefault(RMNodeType nodeType, Guid uniqueId)
        {
            if(nodeType == RMNodeType.PhyBox && uniqueId == new Guid(DefaultTemplateIds.BOX_TEMPLATE_ID))
            {
                return true;
            }
            else if (nodeType == RMNodeType.PhyFile && uniqueId == new Guid(DefaultTemplateIds.FOLDER_TEMPLATE_ID))
            {
                return true;
            }
            else if (nodeType == RMNodeType.PhyRecord && uniqueId == new Guid(DefaultTemplateIds.RECORD_TEMPLATE_ID))
            {
                return true;
            }
            return false;
        }
        private RMNodeType convertTemplateType2NodeType(TemplateType templateType)
        {
            switch (templateType)
            {
                case TemplateType.Custom:
                    return RMNodeType.PhyCustom;
                case TemplateType.Box:
                    return RMNodeType.PhyBox;
                case TemplateType.Folder:
                    return RMNodeType.PhyFile;
                case TemplateType.Records:
                    return RMNodeType.PhyRecord;
                default:
                    return RMNodeType.PhyRecord;
            }
        }
        #endregion

        private int GetNodeTypeAndClassificationIndex(string[] header)
        {
            int nodeTypeIndex = 0;
            for (int i = 0; i < header.Length; i++)
            {
                if ("Record Type".Equals(header[i], StringComparison.OrdinalIgnoreCase))
                {
                    nodeTypeIndex = i;
                }
                else if ("RMTermPath".Equals(header[i], StringComparison.OrdinalIgnoreCase))
                {
                    this.ClassificationClumnIndex = i;
                    logger.Info("RMTermPath column index is {0}", i);
                }
            }
            return nodeTypeIndex;
        }

        private Dictionary<string, int> AssembleColumnIndexNumber(string[] header, string destTemplateType)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            ColumnMapping columnMapping = BoxColumnMapping;
            
            if (NameConstants.PhysicalBox.Equals(destTemplateType, StringComparison.OrdinalIgnoreCase))
            {
                columnMapping = this.BoxColumnMapping;
            }
            else if (NameConstants.PhysicalFolder.Equals(destTemplateType, StringComparison.OrdinalIgnoreCase))
            {
                columnMapping = this.FolderColumnMapping;
            }
            else if (NameConstants.PhysicalRecord.Equals(destTemplateType, StringComparison.OrdinalIgnoreCase))
            {
                columnMapping = this.RecordColumnMapping;
            }
            else if (NameConstants.CustomTemplate.Equals(destTemplateType, StringComparison.OrdinalIgnoreCase))
            {
                columnMapping = this.CustomTeplateColumnMapping;
            }
            for (int i = 0; i < header.Length; i++)
            {
                ColumnMappingDetail columnMappingDetail = columnMapping.Details.FirstOrDefault(a => a.SrcName.Equals(header[i]));
                if (columnMappingDetail != null)
                {
                    dictionary.Add(columnMappingDetail.DestName, i);
                }
            }
            return dictionary;
        }

        private RecordTypeMapping GetRecordTypeMapping(string trimRecordType)
        { 
            RecordTypeMapping current = this.RecordTypeMappings.FirstOrDefault(a => a.SrcRecordType.Equals(trimRecordType, StringComparison.OrdinalIgnoreCase));
            if(current == null)
            {
                throw new GCommon.Utility.AveException("TRIM record type {0}, not found in mapping file.", trimRecordType);
            }
            return current;
        }

        public async Task ImportPhysicalRecordAsync(string sheetName, List<string[]> sheetData)
        { 
            logger.Info("Import physical record sheet {0}, row count {1}", sheetName, sheetData.Count);
            if (sheetData.Count < 2)
            {
                logger.Warn("There is no data in this sheet {0}", sheetName);
                return;
            } 
            TotalItemCount += sheetData.Count - 1;
            //ReportManager.IncreaseBase(sheetData.Count - 1);
            string[] header = sheetData[0];
            int recordTypeIndex = this.GetNodeTypeAndClassificationIndex(header);
            RecordTypeMapping current = this.RecordTypeMappings.FirstOrDefault(a => a.SrcRecordType.Equals(sheetData[1][recordTypeIndex], StringComparison.OrdinalIgnoreCase));
            ArgumentNullException.ThrowIfNull(current);
            this.columnIndexDic = AssembleColumnIndexNumber(header, current?.DestTemplateType);
            int rowIndex = 0;
            foreach (string[] rowData in sheetData)
            {
                if (rowIndex == 0)
                {
                    rowIndex++;
                    continue;
                }

                if (rowData.All(string.IsNullOrEmpty))
                {
                    rowIndex++;
                    continue;
                }

                rowIndex++;
                JMImportPhysicalRecordsJobDetail detail = new JMImportPhysicalRecordsJobDetail();
                try
                {
                    logger.Debug("Process line number: {0}, {1}", rowIndex+1, string.Join("][", rowData));  
                    //Size, Classification, CreatedBy, ModifyBy Excel里没有需要自己处理值
                    string trimRecordType = rowData[recordTypeIndex];
                    RecordTypeMapping recordTypeMapping = this.GetRecordTypeMapping(trimRecordType);
                    logger.Info("Start to process record, trim record type {0}, dest record type {1}, template name {2}, rowNumber {3}",
                        recordTypeMapping.SrcRecordType, recordTypeMapping.DestTemplateType, recordTypeMapping.DestTemplateName, rowIndex + 1);
                    detail.SrcRecordType = recordTypeMapping.SrcRecordType;
                    detail.TemplateName = recordTypeMapping.DestTemplateName;
                    detail.DestRecordType = recordTypeMapping.DestTemplateType;
                    if (NameConstants.CustomTemplate.Equals(current?.DestTemplateType, StringComparison.OrdinalIgnoreCase))
                    {
                        await ProcessCustomContainerAsync(rowData, recordTypeMapping, rowIndex, detail);
                    }
                    if (NameConstants.PhysicalBox.Equals(current.DestTemplateType, StringComparison.OrdinalIgnoreCase))
                    {
                        await ProcessBoxAsync(rowData, recordTypeMapping, rowIndex, detail);
                    }
                    else if (NameConstants.PhysicalFolder.Equals(current.DestTemplateType, StringComparison.OrdinalIgnoreCase))
                    {
                        await ProcessFolderAsync(rowData, recordTypeMapping, rowIndex, detail);
                    }
                    else if (NameConstants.PhysicalRecord.Equals(current.DestTemplateType, StringComparison.OrdinalIgnoreCase))
                    {
                        await ProcessRecordAsync(rowData, recordTypeMapping, rowIndex, detail);
                    }

                    FlushActionAuditsIfNeeded();
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
                    FailedItemCount++;
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
                    FailedItemCount++;
                    logger.Error(@"Update physical record failed.Line:[{0}],Error:{1}", rowIndex + 1, ae);
                }
                catch (Exception e)
                {
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message);
                    Result.HasFailed = true;
                    FailedItemCount++;
                    logger.Error(@"Update physical record failed.Line:[{0}],Error:{1}", rowIndex+1, e);
                }
                finally
                {
                    ReportManager.Increase();
                    if (!CheckJobStatusUtility.isStopping)
                    {
                        //ReportManager.Increase();
                        ReportManager.SendJobDetail(detail);
                    }
                }
            }

            FlushActionAudits();
        }

        private void FlushActionAuditsIfNeeded()
        {
            if (ActionAuditList.Count >= ActionAuditBatchSize)
            {
                FlushActionAudits();
            }
        }

        private void FlushActionAudits()
        {
            if (ActionAuditList.Count == 0)
            {
                return;
            }

            var batchCount = ActionAuditList.Count;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                RecordsHistoryService.AddPhysicalAudit(ActionAuditList);
                stopwatch.Stop();
                logger.Info($"Action audit batch saved, count {batchCount}, elapsed {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                stopwatch.Stop();
                Result.HasFailed = true;
                logger.Error($"Action audit batch save failed, count {batchCount}, elapsed {stopwatch.ElapsedMilliseconds} ms, error: {e}");
            }
            finally
            {
                ActionAuditList.Clear();
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
            rec.LocationId = parent.LocationId;   //subFolde可能需要置成Empty
            rec.ParentId = parent.Id;
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
            rec.Ancestors = new List<Guid> { location.UniqueId};
            if (mTerm == null)
            {
                AssembleTermFromLocation(rec, location);
            }
        }

        /// <summary>
        /// 获取Location和Container的id
        /// </summary>
        /// <param name="rowData"></param>
        /// <param name="nodeType"></param>
        /// <param name="parentUniqueId"></param>
        /// <param name="homeLocation"></param>
        private void AssembleLocationAndParentContainer(string[] rowData, RMNodeType nodeType, out string parentUniqueId, out string homeLocation)
        {
            parentUniqueId = default;
            homeLocation = default;
            if (columnIndexDic.ContainsKey("File (Container)"))
            {
                parentUniqueId = rowData[columnIndexDic["File (Container)"]];
            }
            if (columnIndexDic.ContainsKey("Home Location"))
            {
                homeLocation = validateHomeLocation(rowData[columnIndexDic["Home Location"]], nodeType);
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
        /// 如果import的文件中配置了term，那么使用这个term
        /// </summary>
        /// <param name="rec"></param>
        /// <param name="rowData"></param>
        /// <param name="detail"></param>
        /// <param name="mTerm"></param>
        private void AssembleTermFromFile(Record rec, string[] rowData, JMImportPhysicalRecordsJobDetail detail, out RMTerm mTerm)
        {
            mTerm = null;
            if (ClassificationClumnIndex != -1 && !string.IsNullOrEmpty(rowData[ClassificationClumnIndex]))
            {
                mTerm = this.getTermByPath(rowData[ClassificationClumnIndex]);
                if (mTerm == null)
                {
                    detail.Comment = "Failed to analyse term path, inherit from parent.";
                }
            }
            if (mTerm != null)
            {
                logger.Debug("Get term {0}, {1}", mTerm.UniqueId, mTerm.Name);
                rec.TermId = mTerm.UniqueId;
                rec.TermName = mTerm.Name;
            }
        }

        private async Task<(string, bool)> AssembleAssigneeAsync(Record rec, string[] rowData)
        {
            string assignee = default;
            bool hasAssignee = default;
            if (columnIndexDic.ContainsKey("Where is it? (Assignee)"))
            {
                assignee = rowData[columnIndexDic["Where is it? (Assignee)"]];
                var assigneeName = assignee;
                if (this.UserMappings.Any(a => a.SrcUserName.Equals(assigneeName, StringComparison.OrdinalIgnoreCase)))
                {
                    string recordUserName = this.UserMappings.First(a => a.SrcUserName.Equals(assigneeName, StringComparison.OrdinalIgnoreCase)).DestEmailAddress;
                    if (!string.IsNullOrEmpty(assignee) && !assignee.StartsWith("In file") && !assignee.StartsWith("At home") && !this.locationDic.Values.Any(a => a.Name == assigneeName))
                    {
                        //可能是在个人手上.
                        logger.Info("Assign is {0}, try to add loan info", assignee);
                        rec.HoldType = (int)HoldType.PersonalHold;
                        hasAssignee = true;
                        //add loan alliance
                        //add to custom column
                        await AssembleLoanedByAsync(rec, recordUserName);
                    }
                }
                else
                {
                    logger.Warn("{0} is not a user or the user does not has a mapping.", assignee);
                }
                
            }
            return (assignee, hasAssignee);
        }

        private async Task AssembleLoanedByAsync(Record rec, string recordUserName)
        {
            var users = new List<AOSUserDto> { await GetUserFromDicAsync(recordUserName) };
            var metaInfoDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(rec.MetaInfo);
            metaInfoDic[DefaultColumnIDs.LoanedBy] = JsonConvert.SerializeObject(users);
            rec.MetaInfo = JsonConvert.SerializeObject(metaInfoDic);
        }
        /// <summary>
        /// set modified time and created time
        /// </summary>
        /// <param name="rec"></param>
        /// <param name="rowData"></param>
        /// <param name="isUpdate"></param>
        /// <param name="importedCreateTime"></param>
        private void AssembleTime(Record rec, string[] rowData, bool isUpdate, out long importedCreateTime)
        {
            if (columnIndexDic.ContainsKey("Created Time"))
            {
                string createdTime = rowData[columnIndexDic["Created Time"]];
                //rec.TimeCreated = this.GetTimeLong(createdTime);
                importedCreateTime = this.GetTimeLong(createdTime);
            }
            else
            {
                if (isUpdate)
                {
                    //override,  no createtime column,  不更新CreateTime
                    importedCreateTime = rec.TimeCreated;
                }
                else
                {
                    importedCreateTime = DateTime.UtcNow.Ticks;
                }
            }
            rec.TimeCreated = importedCreateTime;
            //rec.TimeCreated = this.GetTimeLong(createdTime);
            if (columnIndexDic.ContainsKey("Modified Time"))
            {
                string modifiedTime = rowData[columnIndexDic["Modified Time"]];
                rec.TimeModified = this.GetTimeLong(modifiedTime);
            }
            else
            {
                rec.TimeModified = DateTime.UtcNow.Ticks;
            }
        }

        /// <summary>
        /// assemble basic info
        /// </summary>
        /// <param name="rowData"></param>
        /// <param name="recordTypeMapping"></param>
        /// <param name="nodeType"></param>
        /// <param name="detail"></param>
        /// <param name="template"></param>
        /// <param name="isUpdate"></param>
        /// <returns></returns>
        private async Task<(Record, TemplateDto, bool)> AssembleRecordAsync(string[] rowData, RecordTypeMapping recordTypeMapping, RMNodeType nodeType, JMImportPhysicalRecordsJobDetail detail)
        {
            TemplateDto template = null;
            bool isUpdate = false;
            string uniqueId = rowData[columnIndexDic["Unique ID"]];
            detail.UniqueId = uniqueId;
            isUpdate = false;
            Record rec = ExplorerDao.GetPhysicalRecordByRecordsId(uniqueId);
            if (rec != null)
            {
                detail.Title = rec.LeafName;
                if (!this.IsConflictedOverride())
                {
                    logger.Warn($"Record with UniqueId {uniqueId}, NodeType: {nodeType} already exist, skip.");
                    //add skip report
                    throw new SkipItemException(string.Format("Record with unique Id {0} already exist", uniqueId));
                }
                isUpdate = true;
            }
            else
            {
                rec = new Record();
                rec.Id = Guid.NewGuid();
            }
            rec.NodeId = rec.Id;
            rec.LeafName = rowData[columnIndexDic["Title"]];
            detail.Title = rec.LeafName;
            //获取Title之后再取Template 防止出错识别不出来是哪条数据
            template = await this.GetTemplateAsync(recordTypeMapping.DestTemplateName, nodeType);//templateDic[nodeType];
            rec.TemplateId = template.id;

            rec.NodeType = (int)nodeType;
            rec.SourceFlag = (int)SourceFlag.Physical;
            rec.ModifiedBy = await GetAccountDisplayNameAsync(this.jobRunBy);
            rec.CreatedBy = await GetAccountDisplayNameAsync(this.jobRunBy);
            rec.RecordsId = uniqueId;

            return (rec, template, isUpdate);
        }
        private async Task<bool> ProcessRecordAsync(string[] rowData, RecordTypeMapping recordTypeMapping, int rowNumber, JMImportPhysicalRecordsJobDetail detail)
        {
            //Size, Classification, CreatedBy, ModifyBy Excel里没有需要自己处理值RMNodeType nodeType = RMNodeType.PhyFile; 
            RMNodeType nodeType = RMNodeType.PhyRecord;
            
            TemplateDto template;
            bool isUpdate = false;
            long importedCreateTime = 0L;

            (Record rec,template,isUpdate) = await AssembleRecordAsync(rowData, recordTypeMapping, nodeType, detail);

            string containedFolder = null;
            Record folder = null;
            bool inSubFolder = false;
            if (columnIndexDic.ContainsKey("File (Container)"))
            {
                containedFolder = rowData[columnIndexDic["File (Container)"]];
                detail.Container = containedFolder;
                if (!string.IsNullOrEmpty(containedFolder))
                {
                    folder = this.GetParentFolderWithRetry(containedFolder);
                    while(folder != null && folder.SendTo == "sub folder")
                    {
                        inSubFolder = true;
                        folder = ExplorerDao.GetPhysicalRecordById(folder.ParentId);  //非Sub Folder的 ParentId是Empty
                    }
                }
                if (folder == null)
                {
                    throw new GCommon.Utility.AveException("No root folder found with unique id {0}", containedFolder);
                }
            }
            else
            {
                throw new GCommon.Utility.AveException("No 'Contained Within (HPRM Container)' column found in import file");
            }
            if (!IsAdmin && !PhysicalLocationPermission.Contains(folder.LocationId))
            {
                throw new GCommon.Utility.AveException("RM_Phy_Import_NoPermissionForLocation");
            }
            rec.LocationId = folder.LocationId;
            rec.BoxId = folder.BoxId;
            rec.FileId = folder.Id;
            rec.RecordStatus = folder.RecordStatus; 
            detail.LocationFullPath = this.getLocationFullPath(folder.LocationId); 
            //rec.TermId = folder.TermId;
            //rec.TermName = folder.TermName;
            if (inSubFolder)
            {
                detail.Comment = string.Format("This record is moved up from {0} to {1}", detail.Container, folder.RecordsId);
            }
            Dictionary<string, string> metaInfo = await this.AssembleColumnInTemplateAsync(template, rec, rowData, folder.Id, folder.LeafName);
            rec.MetaInfo = JsonConvert.SerializeObject(metaInfo);

            AssembleTime(rec, rowData, isUpdate, out importedCreateTime);

            if (isUpdate && rec.CreateDate != GetCreateDate(importedCreateTime))
            {
                logger.Info("Record {0}, create time change from {1} to {2}", rec.RecordsId, rec.TimeCreated, importedCreateTime);
                ExplorerDao.Delete(rec.CreateDate, rec.Id);
                rec.CreateDate = 0;
            }
            var actionAudit = RecordsHistoryService.BuildPhysicalActionAuditForJob(rec.Id, isUpdate ? PhysicalActionType.ImportEdit : PhysicalActionType.ImportCreate, !isUpdate);
            ActionAuditList.Add(actionAudit);
            ExplorerDao.Upsert(rec);
            logger.Info($"Updated data successfully, row number {rowNumber + 1}");
            Result.HasSuccessful = true;
            SuccessItemCount++;
            this.AddRelated2DB(rec, rowData);
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
            while(folder == null && count < 3)
            {
                count++;
                logger.Warn("Get parent folder failed, retry,  count {0}", count);
                Thread.Sleep(1000);
                folder = ExplorerDao.GetPhysicalRecordByRecordsId(recordsId);
            }
            return folder;
        }

        #region Folder

        private string getLocationFullPath(Guid LocationId)
        {
            if(this.locationDic.Values.Any(a=>a.UniqueId == LocationId))
            {
                RMLocation location = locationDic.Values.First(a => a.UniqueId == LocationId);
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
                        if (locationDic.Values.Any(a => a.Id == tempId))
                        {
                            RMLocation tempLocation = locationDic.Values.First(a => a.Id == tempId);
                            string tempPath = tempLocation.Name;
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
        private async Task<bool> ProcessFolderAsync(string[] rowData, RecordTypeMapping recordTypeMapping, int rowNumber, JMImportPhysicalRecordsJobDetail detail)
        {
            //Size, Classification, CreatedBy, ModifyBy Excel里没有需要自己处理值
            RMNodeType nodeType = RMNodeType.PhyFile; 

            TemplateDto template;
            bool isUpdate = false;
            long importedCreateTime = 0L;

            (Record rec, template, isUpdate) = await AssembleRecordAsync(rowData, recordTypeMapping, nodeType, detail);


            string parentUniqueId = null;
            Record parent = null;
            RMLocation location = null;
            if (columnIndexDic.ContainsKey("File (Container)"))
            {
                parentUniqueId = rowData[columnIndexDic["File (Container)"]];
                if (string.IsNullOrEmpty(parentUniqueId) && columnIndexDic.ContainsKey("Home Location"))  //说明是Location下的
                {
                    string homeLocation = validateHomeLocation(rowData[columnIndexDic["Home Location"]], nodeType);
                    detail.SrcLocation = homeLocation;
                    if (!string.IsNullOrEmpty(homeLocation) && this.locationDic.ContainsKey(homeLocation))
                    {
                        location = locationDic[homeLocation];
                        if (!IsAdmin && !PhysicalLocationPermission.Contains(location.UniqueId))
                        {
                            throw new GCommon.Utility.AveException("RM_Phy_Import_NoPermissionForLocation");
                        }
                        detail.LocationFullPath = this.getLocationFullPath(location);
                        if (location.NodeType != (int)RMNodeType.PhysicalBottomLocation)
                        {
                            throw new GCommon.Utility.AveException(string.Format("Location {0} is not bottom level location", location.Name));
                        }
                    }
                    else
                    {
                        throw new GCommon.Utility.AveException(string.Format("The folder has no box info, and invalid location {0}", homeLocation));
                    }
                }
                else
                {
                    detail.Container = parentUniqueId;
                    parent = ExplorerDao.GetPhysicalRecordByRecordsId(parentUniqueId);
                    //if(box == null || box.NodeType == (int)RMNodeType.PhyFile)
                    //{
                    //    logger.Info("Folder {0} is sub folder.", uniqueId);
                    //    box = ExplorerDao.GetPhysicalRecordById(box.BoxId);
                    //} 
                    if (parent != null && parent.NodeType == (int)RMNodeType.PhyFile)
                    {
                        logger.Info("folder {0} located in another folder {1}", rec?.Id, parent?.Id);
                        AvePoint.GCommon.Utility.ArgumentCheck.NotNull(rec, nameof(rec));
                        rec.SendTo = "sub folder";  //Parent是Folder的, 标记是SubFolder
                        detail.Comment = "Sub folder";
                    }
                    if (parent == null)
                    {
                        throw new GCommon.Utility.AveException("No Box or folder found with unique id {0}", parentUniqueId);
                    }
                    if (!IsAdmin && !PhysicalLocationPermission.Contains(parent.LocationId))
                    {
                        throw new GCommon.Utility.AveException("RM_Phy_Import_NoPermissionForLocation");
                    }
                    detail.LocationFullPath = this.getLocationFullPath(parent.LocationId);
                }
            }
            else
            {
                throw new GCommon.Utility.AveException("No File(Container) information found.");
            }
            if(columnIndexDic.ContainsKey("Home Location"))
            {
                detail.SrcLocation = rowData[columnIndexDic["Home Location"]];
            }
            RMTerm mTerm = null;
            AssembleTermFromFile(rec, rowData, detail, out mTerm);
            Guid parentId = Guid.Empty;
            if (location == null)  //Folder 在Box或者custom conatiner下
            {
                parentId = parent.Id;
                if (parent.NodeType == (int)RMNodeType.PhyBox) rec.BoxId = parent.Id;
                AssembleFromParent(rec, parent, mTerm);
                Dictionary<string, string> metaInfo = await this.AssembleColumnInTemplateAsync(template, rec, rowData, parent.Id, parent.LeafName);
                rec.MetaInfo = JsonConvert.SerializeObject(metaInfo);
            }
            else
            {
                parentId = location.UniqueId;
                AssembleFromLocation(rec, location, mTerm);
                Dictionary<string, string> metaInfo = await this.AssembleColumnInTemplateAsync(template, rec, rowData, location.UniqueId, location.Name);
                rec.MetaInfo = JsonConvert.SerializeObject(metaInfo);
            }
            detail.Title = rec.LeafName;

            
            (var assignee, var hasAssignee) = await AssembleAssigneeAsync(rec, rowData);
            AssembleTime(rec, rowData, isUpdate, out importedCreateTime);

            //rec.DestroyedTime = this.GetDateClosedTimeLong(rowData);
            if (isUpdate && rec.CreateDate != GetCreateDate(importedCreateTime))
            {
                logger.Info("Folder {0}, create time change from {1} to {2}", rec.RecordsId, rec.TimeCreated, importedCreateTime);
                ExplorerDao.Delete(rec.CreateDate, rec.Id);
                rec.CreateDate = 0;
            }
            var actionAudit = RecordsHistoryService.BuildPhysicalActionAuditForJob(rec.Id, isUpdate ? PhysicalActionType.ImportEdit : PhysicalActionType.ImportCreate, !isUpdate);
            ActionAuditList.Add(actionAudit);
            ExplorerDao.Upsert(rec);
            logger.Info($"Updated data successfully, row number {rowNumber + 1}");
            Result.HasSuccessful = true;
            SuccessItemCount++;
            this.AddRelated2DB(rec, rowData);
            if (hasAssignee)
            {
                await this.AddLoanInfoForFolderAsync(assignee, rec.Id, parentId, rec.RecordsId);
            }
            else
            {
                this.CheckNeedClearLoan(rec.Id, parentId, rec.RecordsId);
            }
            return true;
        }

        private int GetCreateDate(long timeCreated)
        {
            DateTime date = new DateTime(timeCreated, DateTimeKind.Utc);
            return int.Parse(date.ToString("yyyyMMdd"));
        }

        private async Task AddLoanInfoForFolderAsync(string assignee, Guid recordId, Guid parentId, string uniqueId)
        {
            try
            {
                if (this.UserMappings.Any(a => a.SrcUserName.Equals(assignee, StringComparison.OrdinalIgnoreCase)))
                {
                    string recordUserName = this.UserMappings.First(a => a.SrcUserName.Equals(assignee, StringComparison.OrdinalIgnoreCase)).DestEmailAddress;
                    string holdBy = await GetAccountDisplayNameAsync(recordUserName);
                    List<RMRecordLoanAlliance> loanInfos = RecordLoanAllianceDao.GetPhyRecordAllianceById(recordId);
                    if (loanInfos.IsNullOrEmpty())
                    {
                        RecordLoanAllianceDao.CreateOrUpdateLoanAlliance(new RMRecordLoanAlliance() { RecordsId = recordId, HoldBy = holdBy, HoldReleaseTime = DateTime.MaxValue.Ticks, ParentId = parentId });
                    }
                    else
                    {
                        if (IsConflictedOverride())
                        {
                            logger.Info("Override loan info for {0}, loan {1}", uniqueId, holdBy);
                            for(int i = 0; i < loanInfos.Count; i++)
                            { 
                                RecordLoanAllianceDao.UpdateLoanedBy(loanInfos[i].RecordsId, holdBy);
                            }
                        }
                        else
                        {
                            logger.Warn("Record {0} has already loan by {1}", uniqueId, loanInfos[0].HoldBy);
                        }
                    }
                }
                else
                {
                    logger.Warn("{0} is not a user or the user does not has a mapping.", assignee);
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw new GCommon.Utility.AveException("Import record successfull, but failed to add Loan infomation."); ;
            }
        }
        /// <summary>
        /// Override状态下， 需要清空原有的Loan信息， from NSW
        /// </summary> 
        private void CheckNeedClearLoan(Guid recordId, Guid parentId, string uniqueId)
        {
            if (IsConflictedOverride())
            {
                try
                {
                    List<RMRecordLoanAlliance> loanInfos = RecordLoanAllianceDao.GetPhyRecordAllianceById(recordId);
                    if (!loanInfos.IsNullOrEmpty())
                    {
                        logger.Info("Start to clear loan info on {0}", uniqueId);
                        foreach (RMRecordLoanAlliance loan in loanInfos)
                        {
                            RecordLoanAllianceDao.DeleteByKey(loan.Id);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn(e.Message, e);
                }
            }
        }
        #endregion

        Dictionary<string, int> columnIndexDic = new Dictionary<string, int>();

        #region Box
        private async Task<TemplateDto> GetTemplateAsync(string templateName, RMNodeType nodeType)
        {
            if (string.IsNullOrEmpty(templateName))
            {
                logger.Info("No template name, return default");
                return templateDic[nodeType];
            }
            TemplateDto templateDto = await TemplateManagementService.GetTemplateDtosByNameAsync(templateName);
            if(templateDto == null)
            {
                logger.Error("No template found with name {0}", templateName);
                throw new GCommon.Utility.AveException("No template found with name {0}", templateName); 
            }
            return templateDto;
        }

        private async Task<bool> ProcessBoxAsync(string[] rowData, RecordTypeMapping recordTypeMapping, int rowNumber, JMImportPhysicalRecordsJobDetail detail)
        {
            var rec = await CreateRecordAsync(RMNodeType.PhyBox, rowData, recordTypeMapping, rowNumber, detail);
            this.AddRelated2DB(rec, rowData);  //box not support related yet--May.  but need to show report --July
            return true;
        }

        private async Task<Record> CreateRecordAsync(RMNodeType nodeType, string[] rowData, RecordTypeMapping recordTypeMapping, int rowNumber, JMImportPhysicalRecordsJobDetail detail)
        {
            string containerType = nodeType == RMNodeType.PhyCustom ? NameConstants.CustomContainer : nodeType == RMNodeType.PhyBox ? NameConstants.Box : NameConstants.File;
            TemplateDto template;
            //bool isUpdate = false;
            //long importedCreateTime = 0L;
            //string parentUniqueId = null;
            //string homeLocation = null;
            (Record rec, template, bool isUpdate) = await AssembleRecordAsync(rowData, recordTypeMapping, nodeType, detail);
            Record parent = null;
            RMLocation location = null;
            AssembleLocationAndParentContainer(rowData, nodeType, out string parentUniqueId, out string homeLocation);
            if (string.IsNullOrEmpty(parentUniqueId) && string.IsNullOrEmpty(homeLocation))
            {
                throw new GCommon.Utility.AveException($"No Home Location or File (Container) found for {containerType} {rec.LeafName}");
            }
            //RMTerm mTerm = null;
            AssembleTermFromFile(rec, rowData, detail, out RMTerm mTerm);

            if (!string.IsNullOrEmpty(homeLocation)) //在Location下
            {
                if (!this.locationDic.ContainsKey(homeLocation))
                {
                    logger.Warn("No location found with name {0}", homeLocation);
                    throw new GCommon.Utility.AveException(string.Format("No location found with name {0}", homeLocation));
                }
                detail.SrcLocation = homeLocation;
                location = locationDic[homeLocation];

                if(!IsAdmin && !PhysicalLocationPermission.Contains(location.UniqueId))
                {
                    throw new GCommon.Utility.AveException("RM_Phy_Import_NoPermissionForLocation");
                }

                detail.LocationFullPath = this.getLocationFullPath(location);
                if (location.NodeType != (int)RMNodeType.PhysicalBottomLocation)
                {
                    throw new GCommon.Utility.AveException(string.Format("Location {0} is not bottom level location", location.Name));
                }
                AssembleFromLocation(rec, location, mTerm);
                Dictionary<string, string> metaInfo = await this.AssembleColumnInTemplateAsync(template, rec, rowData, location.UniqueId, location.Name);
                rec.MetaInfo = JsonConvert.SerializeObject(metaInfo);
            }
            else  //在Container下
            {
                detail.Container = parentUniqueId;
                parent = ExplorerDao.GetPhysicalRecordByRecordsId(parentUniqueId);
                if (parent == null)
                {
                    throw new GCommon.Utility.AveException("No Container found with unique id {0}", parentUniqueId);
                }

                if (!IsAdmin && !PhysicalLocationPermission.Contains(parent.LocationId))
                {
                    throw new GCommon.Utility.AveException("RM_Phy_Import_NoPermissionForLocation");
                }

                AssembleFromParent(rec, parent, mTerm);
                detail.LocationFullPath = this.getLocationFullPath(parent.LocationId);
                Dictionary<string, string> metaInfo = await this.AssembleColumnInTemplateAsync(template, rec, rowData, parent.Id, parent.LeafName);
                rec.MetaInfo = JsonConvert.SerializeObject(metaInfo);
            }

            AssembleTime(rec, rowData, isUpdate, out long importedCreateTime);
           
            //rec.DestroyedTime = this.GetDateClosedTimeLong(rowData);
            if (isUpdate && rec.CreateDate != GetCreateDate(importedCreateTime))
            {
                logger.Info($"{containerType} {rec.RecordsId}, create time change from {rec.TimeCreated} to {importedCreateTime}");
                ExplorerDao.Delete(rec.CreateDate, rec.Id);
                rec.CreateDate = 0;
            }

            if (nodeType == RMNodeType.PhyBox)
            {
                var actionAudit = RecordsHistoryService.BuildPhysicalActionAuditForJob(rec.Id, isUpdate ? PhysicalActionType.ImportEdit : PhysicalActionType.ImportCreate, !isUpdate);
                ActionAuditList.Add(actionAudit);
            }
            ExplorerDao.Upsert(rec);
            Result.HasSuccessful = true;
            SuccessItemCount++;
            logger.Info("Add physical record successfully, id {0}, unique id {1}", rec?.Id, rec.RecordsId);
            return rec;
        }

        #endregion

        #region Custom container
        private async Task<bool> ProcessCustomContainerAsync(string[] rowData, RecordTypeMapping recordTypeMapping, int rowNumber, JMImportPhysicalRecordsJobDetail detail)
        {
            var rec = await CreateRecordAsync(RMNodeType.PhyCustom, rowData, recordTypeMapping, rowNumber, detail);
            return true;
        }
        #endregion
        private bool IsConflictedOverride()
        {
            if("override".Equals(this.ConflictedResolution, StringComparison.OrdinalIgnoreCase) || "overwrite".Equals(this.ConflictedResolution, StringComparison.OrdinalIgnoreCase))
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
            if (columnIndexDic.ContainsKey("Date Closed"))
            {
                string modifiedTime = rowData[columnIndexDic["Date Closed"]];
                return this.GetTimeLocal(modifiedTime);
            }
            return DateTime.MinValue;
        }
        private ChoiceColumnValue AssemleRecordFormat(string formatStr, TemplateColumnDto template)
        {
            ChoiceColumnValue colValue = null;
            if (this.ColumnValueMappings.Any(a=>a.RecordType == "Physical Record" && a.DescColumn == "Format" && a.SrcValue == formatStr))
            {
                ColumnValueMapping map = this.ColumnValueMappings.First(a => a.RecordType == "Physical Record" && a.DescColumn == "Format" && a.SrcValue == formatStr);
                logger.Info("Column value mapping, Format, src:{0}, desc:{1}", map.SrcValue, map.DestValue);
                formatStr = map.DestValue;
            }
            ;
            try
            {
                Dictionary<int, string> options = JsonConvert.DeserializeObject<Dictionary<int, string>>(template.optionsJSON);
                if(options.Any(a=>a.Value.Equals(formatStr, StringComparison.OrdinalIgnoreCase)))
                {
                    KeyValuePair<int, string> option = options.First(a => a.Value.Equals(formatStr, StringComparison.OrdinalIgnoreCase));
                    colValue = new ChoiceColumnValue() { Value = option.Key.ToString(), Name = option.Value }; 
                    return colValue;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
            logger.Warn("Convert format:{0} failed, use default Document", formatStr);
            return new ChoiceColumnValue() { Name = "1", Value = "Document" };
        }

        private void AssembleStatus(Record rec, string[] rowData, Dictionary<string, string> metaInfo)
        {
            if (columnIndexDic.ContainsKey("Status"))
            {
                string status = rowData[columnIndexDic["Status"]];
                ColumnValueMapping map = this.ColumnValueMappings.FirstOrDefault(a => a.DescColumn == "Status" && a.SrcValue == status);
                if (map == null)
                {
                    throw new GCommon.Utility.AveException("No value mapping for Status:{0}", status);
                }
                string recordsStatus = map.DestValue;
                int statusInt = GetStauts(recordsStatus);
                rec.RecordStatus = statusInt;
            }

            ChoiceColumnValue statusFiled = new ChoiceColumnValue()
            {
                Value = rec.RecordStatus.ToString(),
                Name = GetStautsName(rec.RecordStatus)
            };
            metaInfo[DefaultColumnIDs.Status] = JsonConvert.SerializeObject(statusFiled);
        }
        private async Task<Dictionary<string, string>> AssembleColumnInTemplateAsync(TemplateDto template, Record rec, string[] rowData, Guid locationId, string locationName)
        {
            Dictionary<string, string> metaInfo = new Dictionary<string, string>();
            foreach (TemplateCategoryDto cat in template.categories)
            {
                foreach (TemplateColumnDto col in cat.columns)
                {
                    if ("RM_Template_Column_Name_Title" == col.columnName)
                    {
                        metaInfo.Add(col.uniqueId.ToString(), this.ReplaceEnterInExcel(rec.LeafName));   //TRIM TITLE SUPPORT BREAK ROW
                    }
                    else if ("RM_Template_Column_Name_Capability" == col.columnName)
                    {
                        metaInfo.Add(col.uniqueId.ToString(), this.DefaultBoxSize.ToString());
                    }
                    else if ("RM_Template_Column_Name_HomeLocation" == col.columnName)
                    {
                        metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(new TaxonomyColumnValue() { Id = locationId.ToString(), Name = locationName })); //RM_Template_Column_Name_Classification
                    }
                    else if ("RM_Template_Column_Name_Classification" == col.columnName)
                    {
                        metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(new TaxonomyColumnValue() { Id = rec.TermId.ToString(), Name = rec.TermName }));
                    }
                    //else if ("RM_Template_Column_Name_Status" == col.columnName && columnIndexDic.ContainsKey("Status"))
                    //{
                    //    AssembleStatus(rec, rowData, metaInfo);
                    //} 
                    else if ("RM_Template_Column_Name_Format" == col.columnName || "Format".Equals(col.columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (columnIndexDic.ContainsKey("Format"))
                        {
                            string formatStr = rowData[columnIndexDic["Format"]];
                            ChoiceColumnValue formatFiled = this.AssemleRecordFormat(formatStr, col);
                            metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(formatFiled));
                        }
                    } else if ("RM_Template_Column_Name_DataClosed" == col.columnName)
                    {
                        DateTime closedTime = GetDateClosedTimeDate(rowData);
                        if (closedTime != DateTime.MinValue)
                        {
                            DateTimeColumnValue timeColumn = new DateTimeColumnValue() { Date = closedTime, TimeZoneId = this.TimeZoneId, IsSetDayLight = true };
                            metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(timeColumn));
                        }
                    }
                    else if (col.allowEdit && this.columnIndexDic.ContainsKey(col.columnName))  //allow edit说明不是默认Column
                    {
                        string colValue = rowData[columnIndexDic[col.columnName]];
                        if (col.typeId == (int)AvePoint.RA.Contract.TemplateManagement.ColumnType.SingleText
                            || col.typeId == (int)AvePoint.RA.Contract.TemplateManagement.ColumnType.MultipleText
                            || col.typeId == (int)AvePoint.RA.Contract.TemplateManagement.ColumnType.Number)
                        {
                            metaInfo.Add(col.uniqueId.ToString(), this.ReplaceEnterInExcel(colValue));
                        }
                        else if (col.typeId == (int)(int)AvePoint.RA.Contract.TemplateManagement.ColumnType.DateTime)
                        {
                            DateTime localTime = this.GetTimeLocal(colValue);
                            if (localTime != DateTime.MinValue)
                            {
                                DateTimeColumnValue timeColumn = new DateTimeColumnValue() { Date = localTime, TimeZoneId = this.TimeZoneId, IsSetDayLight = true };
                                metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(timeColumn));
                            }
                        }
                        else if (col.typeId == (int)(int)AvePoint.RA.Contract.TemplateManagement.ColumnType.PeopleOrGroup)
                        {
                            string[] tempUsers = colValue.Split(';');
                            List<PeopleColumnValue> accounts = new List<PeopleColumnValue>();
                            foreach (string temp in tempUsers)
                            {
                                string recordUserName = temp;
                                if (this.UserMappings.Any(a => a.SrcUserName.Equals(temp, StringComparison.OrdinalIgnoreCase)))
                                {
                                    recordUserName = this.UserMappings.First(a => a.SrcUserName.Equals(temp, StringComparison.OrdinalIgnoreCase)).DestEmailAddress; 
                                }
                                RMAccount account = await GetAccountFromDicAsync(recordUserName);
                                if (account == null)
                                {
                                    logger.Error("No user found in Records with princple name or display name {0}", recordUserName);
                                }
                                PeopleColumnValue people = GetAosUser(account, recordUserName);
                                accounts.Add(people);

                            }
                            metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(accounts));

                        }
                        else if (col.typeId == (int)(int)AvePoint.RA.Contract.TemplateManagement.ColumnType.SingleChoice)
                        {
                            if (columnIndexDic.ContainsKey(col.columnName))
                            {
                                Dictionary<int, string> options = JsonConvert.DeserializeObject<Dictionary<int, string>>(col.optionsJSON);
                                string optionVal = rowData[columnIndexDic[col.columnName]];
                                foreach (KeyValuePair<int, string> option in options)
                                {
                                    if (option.Value.Equals(optionVal))
                                    {
                                        ChoiceColumnValue formatFiled = new ChoiceColumnValue() { Value = option.Key.ToString(), Name = option.Value };
                                        metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(formatFiled));
                                        break;
                                    }
                                }
                            }
                        }
                        else if (col.typeId == (int)AvePoint.RA.Contract.TemplateManagement.ColumnType.MultipleChoice)
                        {
                            if (columnIndexDic.ContainsKey(col.columnName))
                            {
                                Dictionary<int, string> options = JsonConvert.DeserializeObject<Dictionary<int, string>>(col.optionsJSON);
                                string optionVal = rowData[columnIndexDic[col.columnName]];
                                List<ChoiceColumnValue> choiceList = new List<ChoiceColumnValue>();
                                foreach (KeyValuePair<int, string> option in options)
                                {
                                    string[] optionsVal = optionVal.Split(';');
                                    foreach (string temp in optionsVal)
                                    {
                                        if (temp != string.Empty && option.Value.Equals(temp))
                                        {
                                            ChoiceColumnValue formatFiled = new ChoiceColumnValue() { Value = option.Key.ToString(), Name = option.Value };
                                            choiceList.Add(formatFiled);
                                        }
                                    }
                                }
                                if (choiceList.Count > 0)
                                {
                                    metaInfo.Add(col.uniqueId.ToString(), JsonConvert.SerializeObject(choiceList));
                                }
                            }
                        }
                        else
                        {
                            logger.Debug("Not mapping column type {0}", (Contract.Explorer.ColumnType)col.typeId);
                        }
                    }
                    else
                    {
                        logger.Debug("Record File or column mapping file does not contains column {0}", col.columnName);
                    }
                }
            }

            AssembleStatus(rec, rowData, metaInfo);
            return metaInfo;
        }
        /// <summary>
        /// 将Excel单元格中的回车符， 替换成文本中的回车符
        /// </summary>
        private string ReplaceEnterInExcel(string value)
        {
            if(value != null)
            {
                return value.Replace("_x000D_", "\r");
            }
            return value;
        }

        private PeopleColumnValue GetAosUser(RMAccount account, string notFoundUserName)
        {
            if(account != null)
            {
                return new PeopleColumnValue() { DisplayName = account.DisplayName,  RMUserId = account.Id, Email = account.UserPrincipalName, UserName = account.UserPrincipalName,
                    UserId = account.UserId, UserPrincipalName = account.UserPrincipalName};
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
                logger.Info("Get account from AAD, key {0}", recordUserName);
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
                } 
            }
            return account;
        }

        private ConcurrentDictionary<string, AOSUserDto> userDictionary = new ConcurrentDictionary<string, AOSUserDto>();

        /// <summary>
        /// first get user from DB, if not exists then search from AAD, if search no result, then return a fake user only contains display name/upn
        /// </summary>
        /// <param name="recordUserName"></param>
        /// <returns></returns>
        private async Task<AOSUserDto> GetUserFromDicAsync(string recordUserName)
        {
            if (!userDictionary.ContainsKey(recordUserName))
            {
                var user = (await UserService.SearchUsersAsync(TenantLocalValue.LogonGroupId, recordUserName)).OrderBy(o => o.DisplayName).FirstOrDefault();
                if (user == null)
                {
                    var aadUser = AccountWrapperService.SearchAccounts(TenantLocalValue.LogonGroupId, recordUserName, 1).OrderBy(o => o.DisplayName).FirstOrDefault();
                    if (aadUser != null)
                    {
                        user = AADAccount.Convert2AOSUserDto(aadUser);
                    }
                    else
                    {
                        user = new AOSUserDto { DisplayName = recordUserName, UserPrincipalName = recordUserName };
                    }
                    logger.Info("Get user from AAD, key {0}", recordUserName);
                }
                userDictionary[recordUserName] = user;
                accountDictionary[recordUserName] = new RMAccount()
                {
                    //Id = user.Id,
                    UserId = user.UserId,
                    UserPrincipalName = user.UserPrincipalName,
                    DisplayName = user.DisplayName,
                    ObjectType = user.InviteType == AccountType.Group ? RMActiveDirectoryObjectType.Group : RMActiveDirectoryObjectType.User,
                };
            }

            return userDictionary[recordUserName];
        }


       /* private async Task<string> GetAssignneAsync(string srcName)
        {
            if (this.UserMappings.Any(a => a.SrcUserName.Equals(srcName, StringComparison.OrdinalIgnoreCase)))
            {
                string recordUserName = this.UserMappings.First(a => a.SrcUserName.Equals(srcName, StringComparison.OrdinalIgnoreCase)).DestEmailAddress;
                return await GetAccountDisplayNameAsync(recordUserName);
            }
            else
            {
                logger.Warn("Can not found user {0}, in user mappings");
                return srcName;
            }
        }*/

        private async Task<string> GetAccountDisplayNameAsync(string princpleName)
        {
            RMAccount account = await this.GetAccountFromDicAsync(princpleName);
            if(account == null)
            {
                logger.Warn("Can no found account {0} in aos accounts", princpleName);
                return princpleName;
            }
            else
            {
                return account.DisplayName;
            }
        }
        private void AddRelated2DB(Record rec, string[] rowData)
        {
            try
            {
                //不再过滤Box的RElated  20200622 July release
                if (columnIndexDic.ContainsKey("Related Record") && !string.IsNullOrEmpty(rowData[columnIndexDic["Related Record"]]))
                {
                    string relatedInfo = rowData[columnIndexDic["Related Record"]];
                    logger.Info("Record {0} : {1} has related info: {2}", rec.RecordsId, rec?.Id, relatedInfo);
                    if (relatedInfo.Contains('\n'))
                    {
                        string[] infos = relatedInfo.Split('\n');
                        foreach (string info in infos)
                        {
                            string relatedNumber = GetRelatedNumber(info);
                            if (relatedNumber != null && !recordRelatedDao.IsRelatedExist(rec.RecordsId, relatedNumber))
                            {
                                RMManagedRecordRelated related = new RMManagedRecordRelated();
                                related.CurrentRecordId1 = rec.Id;
                                related.SrcUniqueId = rec.RecordsId;
                                related.RelatedUniqueId = relatedNumber;
                                related.Type = 1;
                                recordRelatedDao.AddImportTRIMRelate(related);
                            }
                            else
                            {
                                logger.Info("Related info with src {0}, dest {1}, already exist.", rec.RecordsId, relatedNumber);
                            }
                        }
                    }
                    else
                    {
                        string relatedNumber = GetRelatedNumber(relatedInfo);
                        if (relatedNumber != null && !recordRelatedDao.IsRelatedExist(rec.RecordsId, relatedNumber))
                        {
                            RMManagedRecordRelated related = new RMManagedRecordRelated();
                            related.CurrentRecordId1 = rec.Id;
                            related.SrcUniqueId = rec.RecordsId;
                            related.RelatedUniqueId = relatedNumber;
                            related.Type = 1;
                            recordRelatedDao.AddImportTRIMRelate(related);
                        }
                        else
                        {
                            logger.Info("Related info with src {0}, dest {1}, already exist.", rec.RecordsId, relatedNumber);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw new Exception(string.Format("Process related failed. {0}", e.Message));
            }
        }

        private string GetRelatedNumber(string relatedInfo)
        {
            if (relatedInfo.Contains(':'))
            {
                string[] infos = relatedInfo.Split(':');
                if (!string.IsNullOrEmpty(infos[1]))
                {
                    logger.Info("Get related Number:[{0}] from string {1}", infos[1], relatedInfo);
                    return infos[1].Trim();
                }
                else
                {
                    logger.Warn("Related record number is empty.");
                }
            }
            else
            {
                logger.Warn("invalid related info {0}", relatedInfo);
            }
            return null;
        }

        private string validateHomeLocation(string homeLocation, RMNodeType nodeType)
        {
            if (homeLocation == null || homeLocation == string.Empty)
            {
                return homeLocation;
            }
            if (nodeType == RMNodeType.PhyBox)
            {
                if (homeLocation.StartsWith("At home:"))
                {
                    return homeLocation.Substring(8, homeLocation.Length - 8);
                }
            }
            return homeLocation;
        }

        private int GetStauts(string statusStr)
        {
            if ("Open".Equals(statusStr, StringComparison.OrdinalIgnoreCase))
            {
                return (int)RMRecordStatus.Active;
            }
            else if ("Closed".Equals(statusStr, StringComparison.OrdinalIgnoreCase)|| "Close".Equals(statusStr, StringComparison.OrdinalIgnoreCase))
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
            return (int)RMRecordStatus.None;
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

        private long GetTimeLong(string time)
        {
            if(string.IsNullOrEmpty(time))
            {
                return 0;
            }
            //TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(this.TimeZoneId);
            DateTime temp = new DateTime();
            if (!DateTime.TryParseExact(time, this.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out temp))
            {
                if(!DateTime.TryParseExact(time, this.DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out temp))
                {
                    if (!DateTime.TryParse(time, out temp))
                    {
                        logger.Error("Parse time failed, {0}", time);
                        return 0;
                    }
                }
            }
            return DateTimeUtil.ConvertTimeToUtc(temp, this.TimeZoneId, false);  //NSW完成之后要把true变成False， 将夏令时算在内
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

        private Dictionary<int, RMPhysicalRecordSetting> GlocalSettingDic = new Dictionary<int, RMPhysicalRecordSetting>();
        private TaxonomyColumnValue GetDefaultTermId(RMLocation location)
        {
            RMLocation temp = location;
            RMLocation parent = this.locationDic.Values.FirstOrDefault(a => a.Id == temp.ParentId);
            while (parent?.NodeType != (int)RMNodeType.PhysicalRootLocation)
            {
                temp = parent;
                parent = this.locationDic.Values.FirstOrDefault(a => a.Id == temp.ParentId);
            }
            logger.Info("Home Location is {0}", temp.Name);
            if (!GlocalSettingDic.ContainsKey(temp.Id))
            {
                RMPhysicalRecordSetting topLevelSetting = PhysicalRecordSettingDao.GetPhysicalRecordSetting(temp.UniqueId);
                if(topLevelSetting == null)
                {
                    logger.Error("Location {0} does not have physcial setting , get default term failed.", temp.Name);
                    throw new GCommon.Utility.AveException("No physical setting found on location {0}", temp.Name);
                }
                GlocalSettingDic.Add(temp.Id, topLevelSetting);
            }
            if (GlocalSettingDic.ContainsKey(temp.Id))
            {
                return new TaxonomyColumnValue() { Id = GlocalSettingDic[temp.Id].DefaultTermId.ToString(), Name = GlocalSettingDic[temp.Id].DefaultTermName };
            }
            else
            {
                logger.Error("No Global physcial setting on location {0}", temp.Name);
                throw new Exception(string.Format("No Global physcial setting on location {0}", temp.Name));
            }
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
                gCachedTerm = await TermDao.FindListAsync(a=>a.IsRemoved== false);
            }
        }
        private RMTerm getTermByPath(string termPath)
        {
            logger.Info("Start to analyze term path:{0}", termPath);
            string[] temp = termPath.Split('|');
            if(temp.Length < 3)
            {
                logger.Warn("Invalid term path {0}", termPath);
                return null;
            }
            string termGroupName = temp[0];
            string termSetName = temp[1];
            try
            {
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
                for(int i = 0; i < temp.Length; i++)
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
                logger.Error(e.Message, e);
            }
            return null;
        }
        List<RMTermGroup> gTermGroup = new List<RMTermGroup>();
        List<RMTermSet> gTermSet = new List<RMTermSet>();
        List<RMTerm> gCachedTerm = new List<RMTerm>();
        Dictionary<Guid, List<RMTermSetMembership>> gTermMembership = new Dictionary<Guid, List<RMTermSetMembership>>();
        
        private RMTerm getTermByArrary(RMTermSet termSet, RMTermGroup termGroup, string[] termArray)
        {
            logger.Info("Term path after anylyse:{0}", string.Join("|", termArray));
            RMTerm tempTerm = null;
            Guid parentUniqueId = termSet.UniqueId;
            int parentId = termSet.Id;
            for(int i = 0; i < termArray.Length; i++)
            {
                List<RMTermSetMembership> memberships = GetMembership(parentUniqueId, parentId, i == 0);
                tempTerm = gCachedTerm.FirstOrDefault(a=> termNameEquals(a.Name, termArray[i]) && memberships.Any(m=>m.TermId == a.Id));
                if(tempTerm == null)
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
        private void InitMapping()
        { 
            DateTime dt = DateTime.Now;
            string fileName = "ImportRecordMeta" + ".xlsx";
            var blobName = Path.Combine(JobReportUtility.GetTenantIdentity(), JobReportUtility.ImportCSVFile, fileName);  
            string metaFileName = JobReportUtility.GetImportJobMetaFileWithoutDeletion(blobName);
           
            Dictionary<string, List<string[]>> sheetDatas = new Dictionary<string, List<string[]>>();
            using (FileStream fs = new FileStream(metaFileName, FileMode.Open))
            {
                try
                {
                    sheetDatas = ExcelUtil.ReadExcelWithHeader(fs); 
                }
                catch (Exception e)
                {
                    logger.Error(e.Message, e); 
                }
            }
            InitMetaFromFile(sheetDatas); 
        }
         
        public async Task<bool> InitMetaAsync()
        {
            InitLocationDic();
            await InitTemplateDicAsync();
            InitMapping();
            await InitTermCacheAsync();
            if (this.RecordTypeMappings == null)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region  Init Setting and Mapping from file

        public void InitMetaFromFile(Dictionary<string, List<string[]>> datas)
        {
            foreach (KeyValuePair<string, List<string[]>> pair in datas)
            {
                if (pair.Key.StartsWith("record type", StringComparison.OrdinalIgnoreCase))
                {
                    this.ImportRecordTypeMapping(pair.Value);
                }
                else if (pair.Key.StartsWith("custom template column", StringComparison.OrdinalIgnoreCase))
                {
                    this.ImportColunMapping(RMNodeType.PhyCustom, pair.Value);
                }
                else if (pair.Key.StartsWith("physical box column", StringComparison.OrdinalIgnoreCase))
                {
                    this.ImportColunMapping(RMNodeType.PhyBox, pair.Value);
                }
                else if (pair.Key.StartsWith("physical folder column", StringComparison.OrdinalIgnoreCase))
                {
                    this.ImportColunMapping(RMNodeType.PhyFile, pair.Value);
                }
                else if (pair.Key.StartsWith("physical record column", StringComparison.OrdinalIgnoreCase))
                {
                    this.ImportColunMapping(RMNodeType.PhyRecord, pair.Value);
                }
                else if (pair.Key.StartsWith("column value", StringComparison.OrdinalIgnoreCase))
                {
                    this.ImportColumnValueMapping(pair.Value);
                }
                else if (pair.Key.StartsWith("user mapping", StringComparison.OrdinalIgnoreCase))
                {
                    this.ImportUserMapping(pair.Value);
                }
                else if (pair.Key.Contains("Setting"))
                {
                    //Dealwith general setting
                    this.ImportSystemSetting(pair.Value);
                }
            }
        }

        private void ImportRecordTypeMapping(List<string[]> datas)
        {
            int index = 0;
            List<RecordTypeMapping> mappings = new List<RecordTypeMapping>();
            foreach (string[] data in datas)
            {
                if (index != 0 && data.Length > 1)
                {
                    if (data[0] != null && data[0] != string.Empty && data[1] != null)
                    {
                        RecordTypeMapping map = new RecordTypeMapping();
                        map.SrcRecordType = data[0].Trim();
                        map.DestTemplateType = data[1].Trim();
                        if(data.Length > 2 && !string.IsNullOrEmpty(data[2]))
                        { 
                            map.DestTemplateName = data[2].Trim();
                        }
                        else
                        {
                            logger.Warn("No template name assigned, import will use default template.");
                        }
                        mappings.Add(map);
                        logger.Info("Import datatype mapping {0}", string.Join(":", data));
                    }
                }
                index++;
            }
            this.RecordTypeMappings = mappings;
        }
        private void ImportColumnValueMapping(List<string[]> datas)
        {
            int index = 0;
            List<ColumnValueMapping> mappings = new List<ColumnValueMapping>();
            foreach (string[] data in datas)
            {
                if (index != 0 && data.Length > 4)
                {
                    if (data[0] != null && data[1] != null && data[2] != null && data[3] != null && data[4] != null)
                    {
                        ColumnValueMapping map = new ColumnValueMapping();
                        map.RecordType = data[0].Trim();
                        map.SrcColumn = data[1].Trim();
                        map.DescColumn = data[2].Trim();
                        map.SrcValue = data[3].Trim();
                        map.DestValue = data[4].Trim();
                        mappings.Add(map);
                        logger.Info("Import column value mapping  {0}", string.Join(" | ", data));
                    }
                }
                index++;
            }
            this.ColumnValueMappings = mappings;

        }
        private void ImportUserMapping(List<string[]> datas)
        {
            int index = 0;
            List<UserMapping> mappings = new List<UserMapping>();
            foreach (string[] data in datas)
            {
                if (index != 0 && data.Length > 1)
                {
                    if (data[0] != null && data[1] != null)
                    {
                        UserMapping map = new UserMapping();
                        map.SrcUserName = data[0].Trim();
                        map.DestEmailAddress = data[1].Trim();
                        mappings.Add(map);
                        logger.Info("Import user mapping  {0}", string.Join(":", data));
                    }
                }
                index++;
            }
            this.UserMappings = mappings;
        }
        private void ImportSystemSetting(List<string[]> datas)
        {
            int index = 0;
            ImportGeneralSetting setting = new ImportGeneralSetting();
            foreach (string[] data in datas)
            {
                if (index != 0 && data.Length > 1)
                {
                    if (data[0] != null && data[1] != null)
                    {
                        if (data[0].Equals("Default Box Size", StringComparison.OrdinalIgnoreCase))
                        {
                            double temp = 0.0;
                            if (!double.TryParse(data[1].Trim(), out temp))
                            {
                                temp = 1;
                            }
                            setting.DefaultBoxSize = temp;
                        }
                        else if (data[0].Equals("Default Location Size", StringComparison.OrdinalIgnoreCase))
                        {

                            double temp = 0.0;
                            if (!double.TryParse(data[1].Trim(), out temp))
                            {
                                temp = 1;
                            }
                            setting.DefaultLocaionSize = temp;
                        }
                        else if (data[0].Equals("Date Time Format", StringComparison.OrdinalIgnoreCase))
                        {
                            setting.DateTimeFormate = data[1];
                        }
                        else if (data[0].Equals("Date Format", StringComparison.OrdinalIgnoreCase))
                        {
                            setting.DateFormate = data[1];
                        }
                        else if (data[0].Equals("Time Zone Id", StringComparison.OrdinalIgnoreCase))
                        {
                            setting.TimeZone = data[1].Trim();
                        }else if (data[0].Equals("conflicted resolution", StringComparison.OrdinalIgnoreCase))
                        {
                            this.ConflictedResolution = string.IsNullOrEmpty(data[1]) ? "skip" : data[1].Trim();
                        }
                    }
                }
                index++;
            }
            this.DefaultBoxSize = setting.DefaultBoxSize;
            this.DefaultLocationSize = setting.DefaultLocaionSize;
            this.TimeZoneId = setting.TimeZone;
            this.DateTimeFormat = setting.DateTimeFormate;
            this.DateFormat = setting.DateFormate;
            logger.Info("Init setting,  default box size:{0}, location size{1}, timezone: {2}, datetime format: {3}, date format: {4}, conflicted:{5}.", 
                this.DefaultBoxSize, this.DefaultLocationSize, this.TimeZoneId, this.DateTimeFormat, this.DateFormat, this.ConflictedResolution);
        }

        private void ImportColunMapping(RMNodeType nodeType, List<string[]> datas)
        {
            int index = 0;
            logger.Info("Import column mapping at level {0}", nodeType);
            ColumnMapping mappings = new ColumnMapping() { RecordType = (int)nodeType, Details = new List<ColumnMappingDetail>() };
            foreach (string[] data in datas)
            {
                if (index != 0 && data.Length > 1)
                {
                    if (data[0] != null && data[1] != null)
                    {
                        ColumnMappingDetail map = new ColumnMappingDetail();
                        map.SrcName = data[0].Trim();
                        map.DestName = data[1].Trim();
                        map.ColumnType = data[2].Trim();
                        map.MustHave = data[3].Trim();
                        mappings.Details.Add(map);
                        logger.Info("Import Column mapping detail  {0}", string.Join("--", data));
                    }
                }
                index++;
            }
            RMMiscProfile profile = new RMMiscProfile();
            profile.Id = Guid.NewGuid().ToString();
            switch (nodeType)
            {
                case RMNodeType.PhyCustom:
                    this.CustomTeplateColumnMapping = mappings;
                    break;
                case RMNodeType.PhyBox:
                    this.BoxColumnMapping = mappings;
                    break;
                case RMNodeType.PhyFile:
                    this.FolderColumnMapping = mappings;
                    break;
                case RMNodeType.PhyRecord:
                    this.RecordColumnMapping = mappings;
                    break;
                default: break;
            }
        } 
        #endregion


        #region Dispose method
        public void Dispose()
        {

        } 
        #endregion
    }

    class NameConstants
    {
        public const string CustomContainer = "Custom Container";
        public const string Box = "Box";
        public const string File = "File";
        public const string CustomTemplate = "Custom Template";
        public const string PhysicalBox = "Physical Box";
        public const string PhysicalFolder = "Physical Folder";
        public const string PhysicalRecord = "Physical Record";
    }
}
