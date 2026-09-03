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
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.Server.UserRegister;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Import;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.LocationManagement;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.Tree;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services;
using AvePoint.RA.Service.Services.LocationManagement.AuditHandler;
using AvePoint.RA.Service.Services.TermManagement.AuditHandler;
using Google.Api.Gax.ResourceNames;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
//using AvePoint.RA.DB.Explorer.Model;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.LocationManagement
{
    [Audit]
    public class LocationManagementService : RMServiceBase, ILocationManagementService
    {
        private RALogger logger = RALogger.GetInstance(typeof(LocationManagementService));
        //public ITermGroupDao TermGroupDao => PlatformWindsorManager.GetService<ITermGroupDao>();
        private ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        private ITermSetDao TermSetDao => PlatformWindsorManager.GetService<ITermSetDao>();
        private ITermGroupDao TermGroupDao => PlatformWindsorManager.GetService<ITermGroupDao>();
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private IRMLocationDao LocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();
        private IExplorerDao mExplorerDao;
        public IPhysicalRecordSettingDao PhysicalRecordSettingsDao => PlatformWindsorManager.GetService<IPhysicalRecordSettingDao>();
        private IRecordImportSettingDao ImportSettingDao => PlatformWindsorManager.GetService<IRecordImportSettingDao>();
        public IExplorerDao ExplorerDao
        {
            get
            {
                if (mExplorerDao == null)
                {
                    mExplorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
                }
                return mExplorerDao;
            }
        }
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IGeneralSettingService mGeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private IJobQueueService mJobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();
        private IRMPhysicalRecordSettingsService RMPhysicalRecordSettingsService => PlatformWindsorManager.GetService<IRMPhysicalRecordSettingsService>();
        private ITemplateManagementService TemplateManagementService => PlatformWindsorManager.GetService<ITemplateManagementService>();
        private ISecurityGroupManagementService SecurityGroupManagementService => PlatformWindsorManager.GetService<ISecurityGroupManagementService>();
        private IRMSecurityTrimmingHelper RMSecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();

        private static readonly int MaxAvailableSpace = 100000000;
        //private static List<string> currentProcessRunJobIds = new List<string>();
        //private BaseJobDto baseJobDto;
        // <summary>
        /// 获取tree children nodes
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="treeNodeId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <returns>Jason字符串</returns> 
        //public string GetTaxonomyTreeData(string typeName, string treeNodeId, int pageIndex, int pageCount)
        //{
        //    logger.Debug(string.Format("type:[{0}],nodeId:[{1}],pageIndex:[{2}],pageCount:[{3}]", typeName, treeNodeId, pageIndex, pageCount));

        //    string strResult = string.Empty;
        //    switch (typeName)
        //    {
        //        case "TermGroup":
        //            strResult = GetJsonStrByObj(TermSetDao.LoadTermSet(TermSetType.Physical, Guid.Empty));
        //            break;
        //        case "TermSet":
        //            strResult = GetJsonStrByObj(TermDao.GetTermFromTermSet(Convert.ToInt32(treeNodeId), pageIndex, pageCount));
        //            break;
        //        case "Term":
        //            strResult = GetJsonStrByObj(TermDao.GetTermFromParentTerm(Convert.ToInt32(treeNodeId), pageIndex, pageCount));
        //            break;
        //        default:
        //            strResult = GetJsonStrByObj(TermGroupDao.LoadLocationSet());
        //            break;
        //    }
        //    return strResult;
        //}



        //[Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.LocationManagement, Action = AuditAction.DeleteLocationTerm, AfterHandler = typeof(TermManagementAfterAuditHandler))]
        //public bool DeleteTerm(Guid termId)
        //{
        //    //try
        //    //{
        //    //    int result = CheckTermCanbeDelete(termId.ToString());
        //    //    if (result == 0)
        //    //    {
        //    //        TermDao.DeleteLocationTerm(termId);
        //    //    }
        //    //    return result == 0;
        //    //}
        //    //catch (Exception e)
        //    //{
        //    //    logger.Warn("DeleteTerm error,error detail {0}", e.Message);
        //    //    return false;
        //    //}
        //    try
        //    {
        //        TermDao.CheckIsDefaultTerm(0, termId);
        //        TermDao.DeleteLocationTerm(termId);
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Warn("DeleteTerm error,error detail {0}", e.Message);
        //        return false;
        //    }
        //}



        //public string Search(int termSetId, string termLabel)
        //{
        //    return GetJsonStrByObj(TermDao.GetLocationTermsBySearch(termLabel));
        //}

        public string RunImportPhysicalFilesAndRecords(JobRunBy jobRunBy, string upFilePath, int settingId, int customId = 0)
        {

            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ImportPhysicalRecords,
                    Parameters = string.Format("{0} {1} {2}", upFilePath, settingId, customId),
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                id = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while RunImportPhysicalFilesAndRecords,ERROR:{0}", ex.ToString());
            }

            return id;

        }

        /// <summary>
        /// 导入Zip
        /// </summary>
        /// <param name="jobRunBy"></param>
        /// <param name="upFilePath"></param>
        /// <param name="settingId"></param>
        /// <returns></returns>
        public string RunImportPhysicalZipFilesAndRecords(JobRunBy jobRunBy, string upFilePath, int settingId)
        {

            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.PhysicalBulkInsertExport,
                    Parameters = string.Format("{0} {1} ", upFilePath, settingId),
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                id = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while RunImportPhysicalZipFilesAndRecords,ERROR:{0}", ex.ToString());
            }

            return id;

        }

        /// <summary>
        /// 导出Zip
        /// </summary>
        /// <param name="jobRunBy"></param>
        /// <param name="templateIds"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string RunExportPhysicalZipFilesAndRecords(JobRunBy jobRunBy, string templateIds)
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.PhysicalBulkEditExport,
                    Parameters = string.Format("{0}", templateIds),
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                id = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while RunExportPhysicalZipFilesAndRecords,ERROR:{0}", ex.ToString());
            }

            return id;
        }


        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.PhysicalRecordsExplorer, Action = AuditAction.PhysicalItemImportReport, AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public async Task<string> RealRunImportPhysicalFilesAndRecordsAsync(JobRunBy jobRunBy, string jobRunByUser, string upFilePath, int settingId, int customId = 0)
        {
            string id = string.Empty;
            if (jobRunBy == JobRunBy.Control)
            {
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                id = JobMonitorService.CreateJob(JobType.ImportPhysicalRecords, jobRunByUser, account.UserId);
                logger.Info("Begin control Import physical records Job {0}", id);
            }
            //BaseJobDto baseJobDto = new BaseJobDto() { Id = id, JobType = (int)JobType.ImportPhysicalRecords };
            //查询当前还没有结束的Term Sync Job
            List<string> runningImportPhyscailRecrodsJobs = JobMonitorService.GetRunningJobs(JobType.ImportPhysicalRecords);

            //Import Term Job一次只能同时运行一个，所以判断当前起的Job是否要Skip掉
            bool isSkip = false;
            if (runningImportPhyscailRecrodsJobs.Any(j => j != id))
            {
                //isSkip = true;
            }
            //if (!isSkip)
            {
                //新起线程起Job
                await StartImportPhysicalFilesAndRecordsAsync(id, upFilePath, settingId, customId);
            }
            //else
            //{
            //    logger.Info(I18NEntity.GetString("Skipped this job. A physical files and records import job is already running."));
            //    JobMonitorService.UpdateJobStatus(id, Contract.RMWeb.JobMonitor.JobStatus.Skipped, "Skipped this job. A physical files and records import job is already running.");
            //}

            return id;
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.PhysicalRecordsExplorer, Action = AuditAction.PhysicalBulkUpdateImport, AfterHandler = typeof(LocationManagementAfterAuditHandler))]
        public async Task<string> RealRunImportPhysicalZipFilesAndRecordsAsync(JobRunBy jobRunBy, string jobRunByUser, string upFilePath, int settingId)
        {
            string id = string.Empty;
            if (jobRunBy == JobRunBy.Control)
            {
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                id = JobMonitorService.CreateJob(JobType.PhysicalBulkInsertExport, jobRunByUser, account.UserId);
                logger.Info("Begin control Import physical records Job {0}", id);
            }
            //BaseJobDto baseJobDto = new BaseJobDto() { Id = id, JobType = (int)JobType.ImportPhysicalRecords };
            //查询当前还没有结束的Term Sync Job
            List<string> runningImportPhyscailRecrodsJobs = JobMonitorService.GetRunningJobs(JobType.PhysicalBulkInsertExport);

            //Import Term Job一次只能同时运行一个，所以判断当前起的Job是否要Skip掉
            bool isSkip = false;
            if (runningImportPhyscailRecrodsJobs.Any(j => j != id))
            {
                isSkip = true;
            }
            if (!isSkip)
            {
                await StartImportPhysicalZipFilesAndRecordsAsync(id, upFilePath, settingId);
            }
            else
            {
                logger.Info(I18NEntity.GetString("Skipped this job. A job for this profile is already running."));
                JobMonitorService.UpdateJobStatus(id, Contract.RMWeb.JobMonitor.JobStatus.Skipped, "RM_SYNC_JobSkip");
            }
            return id;
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.PhysicalRecordsExplorer, Action = AuditAction.PhysicalBulkUpdateExport, AfterHandler = typeof(LocationManagementAfterAuditHandler))]
        public async Task<string> RealRunExportPhysicalZipFilesAndRecordsAsync(JobRunBy jobRunBy, string jobRunByUser, string templateIds)
        {
            string id = string.Empty;
            if (jobRunBy == JobRunBy.Control)
            {
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                id = JobMonitorService.CreateJob(JobType.PhysicalBulkEditExport, jobRunByUser, account.UserId);
                logger.Info("Begin control Import physical records Job {0}", id);
            }
            //BaseJobDto baseJobDto = new BaseJobDto() { Id = id, JobType = (int)JobType.ImportPhysicalRecords };
            //查询当前还没有结束的Term Sync Job
            List<string> runningImportPhyscailRecrodsJobs = JobMonitorService.GetRunningJobs(JobType.PhysicalBulkEditExport);

            //Import Term Job一次只能同时运行一个，所以判断当前起的Job是否要Skip掉
            bool isSkip = false;
            if (runningImportPhyscailRecrodsJobs.Any(j => j != id))
            {
                //isSkip = true;
            }
            //if (!isSkip)
            {
                //新起线程起Job
                await StartExportPhysicalZipFilesAndRecordsAsync(id, templateIds);
            }
            return id;
        }

        private async System.Threading.Tasks.Task StartImportPhysicalFilesAndRecordsAsync(string jobId, string upFilePath, int settingId, int customId)
        {

            //IJobMessageService jobMsgSender = new JobMessageService();
            upFilePath = "\"" + upFilePath + "\"";
            mJobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = jobId,
                JobType = JobType.ImportPhysicalRecords,
                CommandLine = string.Format("{0} {1} {2} {3} {4} {5}", JobType.ImportPhysicalRecords, jobId, upFilePath, settingId, (await mGeneralSettingService.GetGeneralSettingAsync()).TimeZoneId.Replace(" ", "_"), customId),
            });

        }

        private async System.Threading.Tasks.Task StartImportPhysicalZipFilesAndRecordsAsync(string jobId, string upFilePath, int settingId)
        {

            //IJobMessageService jobMsgSender = new JobMessageService();
            upFilePath = "\"" + upFilePath + "\"";
            mJobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = jobId,
                JobType = JobType.PhysicalBulkInsertExport,
                CommandLine = string.Format("{0} {1} {2} {3} {4}", JobType.PhysicalBulkInsertExport, jobId, upFilePath, settingId, (await mGeneralSettingService.GetGeneralSettingAsync()).TimeZoneId.Replace(" ", "_")),
            });

        }

        private async System.Threading.Tasks.Task StartExportPhysicalZipFilesAndRecordsAsync(string jobId, string templateIds)
        {
            mJobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = jobId,
                JobType = JobType.PhysicalBulkEditExport,
                CommandLine = string.Format("{0} {1} {2} {3}", JobType.PhysicalBulkEditExport, jobId, templateIds, (await mGeneralSettingService.GetGeneralSettingAsync()).TimeZoneId.Replace(" ", "_")),
            });

        }

        //public Dictionary<string, int> GetPhysicalSettingsInfo()
        //{
        //    RMLocationManagement location = new RMLocationManagement();
        //    return location.GetPhysicalLibrarysInfo();
        //}

        //[Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.LocationManagement, Action = AuditAction.CreateLocationTerm, AfterHandler = typeof(TermManagementAfterAuditHandler))]
        //public string CreateTerm(string termName, int parentTermId, int termSetId)
        //{
        //    try
        //    {
        //        TermDao.CheckIsDefaultTerm(parentTermId, Guid.Empty);
        //        return GetJsonStrByObj(TermDao.CreateTerm(termName, parentTermId, termSetId));
        //    }
        //    catch
        //    {
        //        return string.Empty;
        //    }
        //}

        //[Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.LocationManagement, Action = AuditAction.RenameLocationTerm, BeforeHandler = typeof(TermManagementBeforeAuditHandler), AfterHandler = typeof(TermManagementAfterAuditHandler))]
        //public string RenameTerm(int termId, string termName, int termSetId)
        //{
        //    try
        //    {
        //        TermDao.CheckIsDefaultTerm(termId, Guid.Empty);
        //        return GetJsonStrByObj(TermDao.RenameTerm(termId, termName, termSetId));
        //    }
        //    catch
        //    {
        //        return GetJsonStrByObj(new { message = "-1" });
        //    }
        //}

        //[Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.LocationManagement, Action = AuditAction.RenameLocationTermSet, BeforeHandler = typeof(TermManagementBeforeAuditHandler), AfterHandler = typeof(TermManagementAfterAuditHandler))]
        //public string UpdateTermSet(int termSetId, string termSetName, string des)
        //{
        //    try
        //    {
        //        return GetJsonStrByObj(TermSetDao.UpdateTermSet(termSetId, termSetName, des));
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error("Save TermSet Error.Term Set Id:{0}, Message:{1}.", termSetId, e.ToString());
        //        return string.Empty;
        //    }
        //}
        #region Import Location from file
        private ImportGeneralSetting GetImportSetting()
        {
            try
            {
                RMMiscProfile profile = ImportSettingDao.GetProfileByType((int)ImportProfileType.GeneralSetting);
                ImportGeneralSetting setting = GCommon.Utility.SerializerHelper.DeserializeByDataContractSerializer<ImportGeneralSetting>(profile.Extension);
                return setting;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
            return null;
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.LocationManagement, Action = AuditAction.PhysicalLocationImport, AfterHandler = typeof(LocationManagementAfterAuditHandler))]
        public async Task<string> ImportXlsFileAsync(List<string[]> xlsFileContent)
        {
            try
            {
                await InnerImportXlsFileAsync(xlsFileContent);
                return "ok";
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                return e.Message;
            }
        }

        public async System.Threading.Tasks.Task InnerImportXlsFileAsync(List<string[]> xlsFileContent)
        {
            logger.Debug("import file info \r\n {0}", xlsFileContent);
            var suites = TemplateManagementService.LoadAllSuites();
            int locationDepth = 0;
            for (int i = 0; i < xlsFileContent[0].Length; i++)
            {
                if (xlsFileContent[0][i].StartsWith("location level", StringComparison.OrdinalIgnoreCase))
                {
                    locationDepth++;
                }
            }
            ImportGeneralSetting importSetting = GetImportSetting();
            RMLocation rootLocation = LocationDao.GetRootLocation();
            int rootId = rootLocation.Id;
            RMLocation tempParent = null;
            int depth = 0;
            List<string[]> temp = new List<string[]>();
            var userPermission = await SecurityGroupManagementService.GetUserScopePermissionsAsync(TenantLocalValue.LogonUserId);
            bool isAdmin = userPermission.IsAdmin;
            var phyLocationPermission = userPermission.ScopePermissionInfo.FirstOrDefault(s => s.DataSourceType == SourceFlag.Physical)?.ScopeIds ?? new List<Guid>();
            bool hasPermission = true;
            for (int i = 0; i < xlsFileContent.Count; i++)
            {
                if (i == 0)
                {
                    continue;  //skip header line
                }
                if (xlsFileContent[i][depth] != null && xlsFileContent[i][depth].Trim() != string.Empty)
                {
                    if (temp.Count > 0)
                    {
                        await ProcessLocationWithDepthAsync(tempParent, temp, depth + 1, locationDepth, importSetting, suites);   //处理第一层.

                        temp.Clear();
                    }
                    var termIndex = locationDepth + 3;
                    if (xlsFileContent[0].Length > termIndex && xlsFileContent[i][termIndex] != null)
                    {
                          CheckTerm(xlsFileContent[i][termIndex]);
                    }
                    (RMLocation rMLocation, hasPermission) = await ProcessOneLocationAsync(rootLocation, xlsFileContent[i], depth, locationDepth, importSetting, suites, !isAdmin, phyLocationPermission);   //这步骤是存入了数据库，这里我需要提前判断下 term到底合不合理
                    tempParent = rMLocation;
                    //第一层如果 有Term Path, 自动设置Physical Settings,  顶层使用第一层TermSet, Default使用Path最后的Term
                    if (!hasPermission) continue;
                     if (xlsFileContent[0].Length > termIndex && xlsFileContent[i][termIndex] != null)
                      {
                         this.AutoApplyPhysicalSetting(rMLocation, xlsFileContent[i][termIndex]);
                      }
                   
                }
                if ((xlsFileContent[i][depth] == null || xlsFileContent[i][depth].Trim() == string.Empty) && hasPermission)
                {
                    temp.Add(xlsFileContent[i]);
                }
            }
            if (temp.Count > 0)
            {
                await ProcessLocationWithDepthAsync(tempParent, temp, depth + 1, locationDepth, importSetting, suites);
                temp.Clear();
            }
        }

        //这里重新弄加一个类 先对term进行判断是否合法，然后在进入数据库
        public void CheckTerm(string termPath)
        {
            string errorMessage = "";
            if (termPath.Contains('|'))
            {
                string[] termNames = termPath.Split('|');
                if (termNames.Length > 2)
                {
                    string termGroupName = termNames[0].Trim();
                    RMTermGroup termGroup = TermGroupDao.GetTermGroupByName(termGroupName?.Trim());
                    if (termGroup == null)
                    {
                        logger.Warn("No term group by name {0}", termGroupName);
                        errorMessage = $"No term group by name '{termGroupName}'";
                        throw new Exception(errorMessage);
                    }
                    string termSetName = termNames[1].Trim();
                    RMTermSet termSet = TermSetDao.GetRMTermSetsByGroupUniqueIdAndTermSetName(termGroup.UniqueId, termSetName).FirstOrDefault();
                    if (termSet == null)
                    {
                        logger.Warn("No term set by name {0}", termSetName);
                        errorMessage = $"No term set by name  '{termSetName}'";
                        throw new Exception(errorMessage);
                    }
                    List<RMTerm> terms = TermDao.GetTermFromTermSetWithoutDeletedTerm(termSet.Id);
                    RMTerm topTerm = terms.FirstOrDefault(a => a.IsRootTerm && !a.IsRemoved && a.Name.Equals(termNames[2].Trim(), StringComparison.OrdinalIgnoreCase));
                    string path = termNames[0] + "/" + termNames[1] + "/" + termNames[2];
                    if (topTerm == null)
                    {
                        logger.Warn("No term  by name {0}", termNames[2]);
                        errorMessage = $"No term  by name  '{termNames[2]}'";
                        throw new Exception(errorMessage);
                    }
                    if (topTerm != null && termNames.Length > 2)
                    {
                        for (int i = 3; i < termNames.Length; i++)
                        {
                            path = path + "/" + termNames[i];
                            List<RMTerm> subTerms = TermDao.GetTermFromParentTerm(topTerm);
                            RMTerm subTerm = subTerms.FirstOrDefault(a => a.Name.Equals(termNames[i].Trim(), StringComparison.OrdinalIgnoreCase));
                            if (subTerm == null)
                            {
                                logger.Error("No term found {0} ", path);
                                errorMessage = $"No term found '{path}'";
                                throw new Exception(errorMessage);
                            }
                        }

                    }
                }
                else
                {
                    logger.Warn("Invalid term path {0}", termPath);
                    errorMessage = $"Invalid term path'{termPath}'";
                    throw new Exception(errorMessage);
                }
               
            }
        }


        private void AssociationSuites(LocationInfo tempLocation, string[] locationRow, int columnIndex, List<SimplifySuiteDto> suites)
        {
            if (locationRow.Length > columnIndex && !string.IsNullOrEmpty(locationRow[columnIndex]))
            {
                var suiteNames = locationRow[columnIndex].Split(';').Select(s => s.TrimStart());
                var suiteIds = suites.Where(s => suiteNames.Contains(s.Name.Trim()) || suiteNames.Contains(I18NEntity.GetString(s.Name.Trim()))).Select(s => s.UniqueId).ToList();
                if (suiteIds.Count > 0) tempLocation.AssociationSuites = suiteIds;
            }
        }
        private async Task<(RMLocation currentLocation, bool hasPermission)> ProcessOneLocationAsync(RMLocation parent, string[] location, int depth, int totalLocationDepth, ImportGeneralSetting importSetting, List<SimplifySuiteDto> suites, bool isNeedCheckPermission = false, List<Guid> phyLocationPermission = null)
        {
            logger.Debug("process one line, parent id {0}, depth {1}, location info {2}", parent.Id, depth, string.Join("|", location));


            int locationDeepCount = totalLocationDepth;
            string locationName = location[depth].Trim();
            if (parent.NodeType == (int)RMNodeType.PhysicalBottomLocation)
            {
                logger.Error("Can not add new location {0} to the bottom level location {1}", locationName, parent.Name);
                throw new Exception(string.Format("Can not add new location {0} to the bottom level location {1}", locationName, parent.Name));
            }
            RMLocation tempLocation = null;
            if (!isNeedCheckPermission)
            {
                if (!LocationDao.HasSameName(locationName, parent.Id))
                {
                    tempLocation = LocationDao.CreateLocation(locationName, parent.Id);
                }
                else
                {
                    tempLocation = LocationDao.GetByName(locationName, parent.Id);
                }
            }
            else
            {
                if (!LocationDao.HasSameName(locationName, parent.Id))
                {
                    return (null, false);
                }
                else
                {
                    tempLocation = LocationDao.GetByName(locationName, parent.Id);
                    if(!phyLocationPermission.Contains(tempLocation.UniqueId))
                    {
                        logger.Info("user does not have permission to location {0}, location id {1}", tempLocation.Name, tempLocation.Id);
                        return (null, false);
                    }
                    logger.Info("user has permission to location {0}, location id {1}", tempLocation.Name, tempLocation.Id);
                }
            }
            LocationInfo info = new LocationInfo();
            AssociationSuites(info, location, totalLocationDepth + 4, suites); 
            info.Description = location[locationDeepCount];
            info.AvailableSpace = GetDouble(location[locationDeepCount + 1], importSetting);
            info.LocationId = tempLocation.Id;
            info.ParentId = parent.Id;
            info.Name = locationName;
            bool allowContainer = GetBool(location[locationDeepCount + 2]);
            if(tempLocation.NodeType == (int)RMNodeType.PhysicalBottomLocation && !allowContainer)
            {
                logger.Info("try to change allow container creation from Yes to No, location {0}, parent {1}", locationName, parent.Name);
                if (CheckDatainBottom(tempLocation))
                {
                    allowContainer = true;
                }
            }
            info.NodeType = allowContainer ? (int)RMNodeType.PhysicalBottomLocation : (int)RMNodeType.PhysicalNormalLocation;
            tempLocation.NodeType = info.NodeType;
            await LocationDao.SaveLocationSettingAsync(info);
            logger.Info("Save or update location success {0}", info.Name);
            //this.SaveLocationSetting(info);
            return (tempLocation, true);
        }
        private async System.Threading.Tasks.Task ProcessLocationWithDepthAsync(RMLocation parent, List<string[]> locations, int depth, int totalLocationDepth, ImportGeneralSetting importSetting, List<SimplifySuiteDto> suites)
        {
            logger.Debug("Process child in parent id {0}", parent.Id);
            List<string[]> temp = new List<string[]>();
            RMLocation tempParent = null;
            for (int i = 0; i < locations.Count; i++)
            {
                logger.Debug(string.Join("|", locations[i]));
                if (locations[i][depth] != null && locations[i][depth].Trim() != string.Empty)
                {
                    if (temp.Count > 0)
                    {
                        await ProcessLocationWithDepthAsync(tempParent, temp, depth + 1, totalLocationDepth, importSetting, suites);
                        temp.Clear();
                    }
                    (RMLocation rMLocation, bool hasPermission) = await ProcessOneLocationAsync(parent, locations[i], depth, totalLocationDepth, importSetting, suites);
                    tempParent = rMLocation;
                }
                else if (locations[i][depth] == null || locations[i][depth].Trim() == string.Empty)
                {
                    temp.Add(locations[i]);
                }
            }
            if (temp.Count > 0)
            {
                await ProcessLocationWithDepthAsync(tempParent, temp, depth + 1, totalLocationDepth, importSetting, suites);
                temp.Clear();
            }
        }
        
        private bool CheckDatainBottom(RMLocation location)
        {
            int[] availableStatus = new int[] { (int)RMRecordStatus.Active, (int)RMRecordStatus.Closed, (int)RMRecordStatus.Destroyed, (int)RMRecordStatus.Missing };
            bool existsData = ExplorerDao.Exist(a => a.LocationId == location.UniqueId && Enumerable.Contains(availableStatus, a.RecordStatus));
            if (existsData)
            {
                logger.Error("There are records in location {0}, can not cancel 'Allow container creation'.", location.Name);
               // throw new Exception(string.Format("There are records in location {0}, can not cancel 'Allow container creation'.", location.Name));
            }
            return existsData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <param name="termPath">termgroup/termset/term1/term2</param>
        private void AutoApplyPhysicalSetting(RMLocation location, string termPath)
        {
            string errorMessage = "";
            try
            {
                string locationName = location.Name;
                int locationid = location.Id;
                string[] termNames = termPath.Split('|');
                string termGroupName = termNames[0].Trim();
                RMTermGroup termGroup = TermGroupDao.GetTermGroupByName(termGroupName?.Trim());
                string termSetName = termNames[1].Trim();
                RMTermSet termSet = TermSetDao.GetRMTermSetsByGroupUniqueIdAndTermSetName(termGroup.UniqueId, termSetName).First();
                List<RMTerm> terms = TermDao.GetTermFromTermSetWithoutDeletedTerm(termSet.Id);
                RMTerm topTerm = terms.First(a => a.IsRootTerm && !a.IsRemoved && a.Name.Equals(termNames[2].Trim(), StringComparison.OrdinalIgnoreCase));
                if (topTerm != null && termNames.Length > 2)
                {
                    for (int i = 3; i < termNames.Length; i++)
                    {
                        List<RMTerm> subTerms = TermDao.GetTermFromParentTerm(topTerm);
                        RMTerm subTerm = subTerms.FirstOrDefault(a => a.Name.Equals(termNames[i].Trim(), StringComparison.OrdinalIgnoreCase));
                        topTerm = subTerm;
                    }
                    RMPRSaveTermDto saveTermDto = new RMPRSaveTermDto();
                    saveTermDto.UniqueId = location.UniqueId;
                    saveTermDto.TermSetId = termSet.UniqueId;
                    saveTermDto.TermSetName = termSet.Name;
                    saveTermDto.DefaultTermId = topTerm?.UniqueId ?? Guid.Empty;
                    saveTermDto.DefaultTermName = topTerm?.Name;
                    saveTermDto.DeployTermMethod = DeployTermMethod.UseDefaultTerm;
                    saveTermDto.IsTopLevelSetting = true;
                    PhysicalRecordSettingsDao.SaveTerm(saveTermDto);
                    logger.Info("location {0}, default term {1}", location.Name, topTerm?.Name);
                }
                
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                errorMessage = I18NEntity.GetString("RM_LM_FileContentFormatError");
                throw new Exception(errorMessage);
            }
        }
        private double GetDouble(string size, ImportGeneralSetting importSetting)
        {
            double precision = 1e-6;
            double defaultSize = GetSizeFromSetting(importSetting);
            if (size == null || size == string.Empty)
            {
                return defaultSize;
            }
            double result;
            if(double.TryParse(size.Trim(), out result) && result > 0)
            { 
                if(result > MaxAvailableSpace)
                {
                    logger.Warn($"Invalid size {size}, use default value 1000");
                    return defaultSize;
                }
                return Math.Abs(result) <= precision ? defaultSize : result;
            }
            logger.Warn($"Invalid size {size}, use default value 1000");
            return defaultSize;
            
        }

        private double GetSizeFromSetting(ImportGeneralSetting importSetting)
        {
            if(importSetting != null)
            {
                return importSetting.DefaultLocaionSize;
            }
            return 1000;
        }
        private bool GetBool(string allowContainer)
        {
            if ("true".Equals(allowContainer, StringComparison.OrdinalIgnoreCase) || "Yes".Equals(allowContainer, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else if ("false".Equals(allowContainer, StringComparison.OrdinalIgnoreCase) || "No".Equals(allowContainer, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return false;
        }
        #endregion

        #region New Physical Logic
        public async Task<PhysicalObjectDto> GetPhysicalObjectByIdAsync(int id)
        {
            var result = new PhysicalObjectDto();
            try
            {
                var data = LocationDao.GetLocationById(id);
                result = ConvertUtil.ConvertLocationObjToPhysicalObj(data, true);
                var createByUser = await AccountDao.GetUserByUserIdAsync(data.CreatedUserId);
                var modifiedByUser = await AccountDao.GetUserByUserIdAsync(data.ModifiedUserId);
                result.CreatedBy = createByUser?.DisplayName;
                result.ModifiedBy = modifiedByUser?.DisplayName;
            }
            catch (Exception ex)
            {
                logger.Error($"Get GetPhysicalObject by id: [{id}], error: [{ex.ToString()}]");
            }
            return result;
        }

        public async Task<PhysicalResultInfo> QueryPhysicalNodesAsync(PhysicalExplorerQueryDto dto)
        {
            var result = new PhysicalResultInfo();
            try
            {
                if (dto == null)
                {
                    throw new Exception("query dto is null.");
                }
                var resultDatas = new List<PhysicalObjectDto>();
                if (dto != null)
                {
                    if (dto.PagingInfo != null)
                    {
                        result.PagingInfo = dto.PagingInfo;
                    }
                    else
                    {
                        result.PagingInfo = new PhysicalExplorerPagingInfo()
                        {
                            PageIndex = 0,
                            PageSize = 5
                        };
                    }
                }
                result.Datas = resultDatas;

                var nodeId = 0;
                if (int.TryParse(dto.NodeId, out nodeId))
                {
                    if (nodeId != 0)
                    {
                        var tempLocations = dto.CurrentNodeType == RMNodeLevel.PhysicalRootLocation ? await GetTopLocationWithPermission(nodeId, result) : LocationDao.GetSubLocationByParentId(nodeId, result.PagingInfo.PageIndex, result.PagingInfo.PageSize);
                        if (tempLocations != null && tempLocations.Count > 0)
                        {
                            result.PagingInfo.Total = LocationDao.CountSubLocation(nodeId);
                        }
                        ArgumentCheck.NotNull(tempLocations, nameof(tempLocations));
                        tempLocations.ForEach(a =>
                        {
                            a.SubLocationCount = LocationDao.CountSubLocation(a.Id);
                        });
                        await tempLocations.ForEachAsync(async a =>
                        {
                            try
                            {
                                var tempPhysical = ConvertUtil.ConvertLocationObjToPhysicalObj(a);
                                var createByUser = await AccountDao.GetUserByUserIdAsync(a.CreatedUserId);
                                var modifiedByUser = await AccountDao.GetUserByUserIdAsync(a.ModifiedUserId);
                                tempPhysical.CreatedBy = createByUser?.DisplayName;
                                tempPhysical.ModifiedBy = modifiedByUser?.DisplayName;
                                resultDatas.Add(tempPhysical);
                            }
                            catch(Exception ex)
                            {
                                logger.Warn(ex.ToString());
                            }
                        });
                    }
                }
                else
                {
                    logger.Error($"Load location list info, current id seems is not in correct format, id value: [{dto.NodeId}].");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"ERROR: [{ex.ToString()}]");
            }
            return result;
        }

        private async Task<List<RMLocation>> GetTopLocationWithPermission(int nodeId, PhysicalResultInfo result)
        {
            var userPermission = await SecurityGroupManagementService.GetUserScopePermissionsAsync(TenantLocalValue.LogonUserId);
            var phyPermission = userPermission.ScopePermissionInfo.Where(_ => _.DataSourceType == SourceFlag.Physical).FirstOrDefault();
            var permissionLocationTopIds = phyPermission?.ScopeIds ?? new List<Guid>();
            return userPermission.IsAdmin ? LocationDao.GetSubLocationByParentId(nodeId, result.PagingInfo.PageIndex, result.PagingInfo.PageSize) : LocationDao.GetTopLocationByParentIdAndId(nodeId, result.PagingInfo.PageIndex, result.PagingInfo.PageSize, permissionLocationTopIds);
        }

        public async Task<string> GetLocationTreeAsync(string treeNodeId, int pageIndex, int pageCount, bool iconStatus)
        {
            logger.Debug(string.Format($"nodeId:[{treeNodeId}],pageIndex:[{pageIndex}],pageCount:[{pageCount}]"));

            string strResult = string.Empty;

            var userPermission = await SecurityGroupManagementService.GetUserScopePermissionsAsync(TenantLocalValue.LogonUserId);
            var isHoldManager = await RMSecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.ManageHold);
            var phyPermission = isHoldManager ? null : userPermission.ScopePermissionInfo.Where(_ => _.DataSourceType == SourceFlag.Physical).FirstOrDefault();
            var isPhysicalAdmin = phyPermission?.SubPermission == Contract.RMWeb.CP.SubPermissionType.Admin;
            var permissionLocationTopIds = phyPermission?.ScopeIds ?? new List<Guid>();
            if (treeNodeId.Equals("Root", StringComparison.CurrentCultureIgnoreCase))
            {
                var rootLocations = LocationDao.LoadRootNode(pageIndex, pageCount);
                foreach (var rootlocation in rootLocations)
                {
                    var subLocations = userPermission.IsAdmin ? LocationDao.GetSubLocationByParentId(rootlocation.Id, pageIndex, pageCount) : LocationDao.GetTopLocationByParentIdAndId(rootlocation.Id, pageIndex, pageCount, permissionLocationTopIds);
                    foreach (var subLocation in subLocations)
                    {
                        var subLocationCount = LocationDao.CountSubLocation(subLocation.Id);
                        subLocation.SubLocationCount = subLocationCount;
                    }
                    if (iconStatus)
                    {
                        await LoadSettingIconAsync(subLocations);
                    }
                    rootlocation.SubLocations = subLocations;
                    rootlocation.SubLocationCount = userPermission.IsAdmin ? LocationDao.CountSubLocation(rootlocation.Id) : LocationDao.CountSubLocationByLocationIds(rootlocation.Id, permissionLocationTopIds);
                }
                strResult = GetJsonStrByObj(rootLocations);
            }
            else
            {
                int nodeId;
                if (int.TryParse(treeNodeId, out nodeId))
                {
                    var currentLocation = LocationDao.GetLocationWitPathById(nodeId, false);
                    if(currentLocation != null && currentLocation.NodeType == (int)RMNodeLevel.PhysicalRootLocation
                        && !userPermission.IsAdmin && isPhysicalAdmin)
                    {
                        return await LoadTopLocationWithPermission(pageIndex, pageCount, iconStatus, permissionLocationTopIds, nodeId);
                    }
                    var subLocations = LocationDao.GetSubLocationByParentId(nodeId, pageIndex, pageCount);
                    if (iconStatus)
                    {
                        await LoadSettingIconAsync(subLocations);
                    }
                    foreach (var subLocation in subLocations)
                    {
                        var subLocationCount = LocationDao.CountSubLocation(subLocation.Id);
                        subLocation.SubLocationCount = subLocationCount;
                    }
                    strResult = GetJsonStrByObj(subLocations);
                }
            }
            return strResult;
        }

        private async Task<string> LoadTopLocationWithPermission(int pageIndex, int pageCount, bool iconStatus, List<Guid> permissionLocationTopIds, int nodeId)
        {
            var subLocations = LocationDao.GetTopLocationByParentIdAndId(nodeId, pageIndex, pageCount, permissionLocationTopIds);
            if (iconStatus)
            {
                await LoadSettingIconAsync(subLocations);
            }
            foreach (var subLocation in subLocations)
            {
                var subLocationCount = LocationDao.CountSubLocation(subLocation.Id);
                subLocation.SubLocationCount = subLocationCount;
            }
            return GetJsonStrByObj(subLocations);
        }

        public async System.Threading.Tasks.Task LoadSettingIconAsync(List<RMLocation> nodes)
        {
            try
            {
                if (nodes.Count > 0)
                {
                    var tempNode = nodes[0];
                    bool isTopLevelLocation;
                    Guid topLevelLocationUniqueId;
                    List<string> locationDirPathIds;
                    RMPhysicalRecordSettingsService.CheckIsTopLevelSetting(tempNode.DirPath, out isTopLevelLocation, out topLevelLocationUniqueId, out locationDirPathIds);

                    if (!isTopLevelLocation)
                    {
                        var gsSetting = PhysicalRecordSettingsDao.GetPhysicalRecordSetting(topLevelLocationUniqueId);
                        var allSchedules = await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.PRDisposalSchedule);
                        List<string> allSchedulesProfilesId = new List<string>();
                        if (allSchedules != null && allSchedules.Count != 0)
                        {
                            allSchedulesProfilesId = allSchedules.Select(s => s.ProfileId).ToList();
                        }

                        var allSettings = new Dictionary<string, RMPhysicalRecordSetting>();
                        var settings = PhysicalRecordSettingsDao.GetPhysicalRecordSetting(nodes.Select(n => n.UniqueId).ToList()).OrderBy(item => item.Id);
                        foreach (var setting in settings)
                        {
                            var key = setting.LocationUniqueId.ToString();
                            if (!allSettings.ContainsKey(key))
                            {
                                allSettings.Add(key, setting);
                            }
                        }
                        foreach (var node in nodes)
                        {
                            RMPhysicalRecordSetting csSetting = null;
                            ArgumentCheck.NotNull(node, nameof(node));
                            var settingKey = node?.UniqueId.ToString();
                            if (allSettings.TryGetValue(settingKey, out csSetting))
                            {
                                node.IconStatus = IconStatus.Break;
                                continue;
                            }
                            var profileId = RMPhysicalRecordSettingsService.GetProfileId(node.UniqueId);
                            if (allSchedulesProfilesId.Contains(profileId))
                            {
                                node.IconStatus = IconStatus.Break;
                                continue;
                            }
                            if (gsSetting != null)
                            {
                                node.IconStatus = IconStatus.Inhert;
                                continue;
                            }
                            node.IconStatus = IconStatus.NoSet;
                        }
                    }
                    else
                    {
                        foreach (var selfGroupNode in nodes)
                        {
                            var selfGSSetting = PhysicalRecordSettingsDao.GetPhysicalRecordSetting(selfGroupNode.UniqueId);
                            if (selfGSSetting == null)
                            {
                                selfGroupNode.IconStatus = IconStatus.NoSet;
                            }
                            else
                            {
                                selfGroupNode.IconStatus = IconStatus.Break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred when load PRSetting Icon.Error:{0}", e.ToString());
            }
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.LocationManagement, Action = AuditAction.RenameLocation, BeforeHandler = typeof(LocationManagementBeforeAuditHandler), AfterHandler = typeof(LocationManagementAfterAuditHandler))]
        public async Task<string> RenameLocationAsync(int locationId, string name, RMNodeLevel nodeType)
        {
            var result = string.Empty;
            try
            {
                switch (nodeType)
                {
                    case RMNodeLevel.PhysicalRootLocation:
                        result = GetJsonStrByObj(await LocationDao.RenameLocationAsync(locationId, name, false));
                        break;
                    case RMNodeLevel.PhysicalNormalLocation:
                        result = GetJsonStrByObj(await LocationDao.RenameLocationAsync(locationId, name, true));
                        break;
                    case RMNodeLevel.PhysicalBottomLocation:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                result = GetJsonStrByObj(new { message = "-1" });
            }
            return result;
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.LocationManagement, Action = AuditAction.CreateLocation, AfterHandler = typeof(LocationManagementAfterAuditHandler))]
        public string CreateLocation(string name, int parentId)
        {
            var result = string.Empty;
            try
            {
                result = GetJsonStrByObj(LocationDao.CreateLocation(name, parentId));
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                result = "1";
            }
            return result;
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.LocationManagement, Action = AuditAction.DeleteLocation, BeforeHandler = typeof(LocationManagementBeforeAuditHandler), AfterHandler = typeof(LocationManagementAfterAuditHandler))]
        public async Task<bool> DeleteLocationAsync(int locationId)
        {
            var result = false;
            try
            {
                var canDel = PreparedDeleteLocation(locationId);
                if (!canDel)
                {
                    return false;
                }
                result = await LocationDao.DeleteLocationAsync(locationId);
            }
            catch (Exception e)
            {
                logger.Warn("Delete location error,error detail {0}", e.ToString());
                result = false;
            }
            return result;
        }

        public bool PreparedDeleteLocation(int locationId)
        {
            try
            {
                //目前只有成功和失败的验证返回值，以后可以支持是因为有sub location失败或者physical data失败
                var location = LocationDao.GetLocationById(locationId);
                if (location.NodeType == (int)RMNodeLevel.PhysicalBottomLocation)
                {
                    var hasPhysical = ExplorerDao.QueryByPage(r =>
                        (r.NodeType == (int)RMNodeLevel.PhysicalBox || r.NodeType == (int)RMNodeLevel.PhysicalFile)
                        && r.LocationId == location.UniqueId && r.BoxId == Guid.Empty
                        && r.SourceFlag == (int)SourceFlag.Physical
                        && r.RecordStatus != (int)RMRecordStatus.RMDeleted 
                        && r.RecordStatus != (int)RMRecordStatus.MoveOverwrite, 1).Item1.Count() > 0;
                    return !hasPhysical;
                }
                else
                {
                    var hasSubLocation = LocationDao.HasSubLocation(locationId);
                    return !hasSubLocation;
                }
            }
            catch (Exception e)
            {
                logger.Warn("Prepared delete location error, error detail {0}", e.ToString());
            }
            return false;
        }

        public string SearchLocation(string locationStr)
        {
            return GetJsonStrByObj(LocationDao.GetLocationsBySearch(locationStr));
        }

        public RMLocationProfileNode SearchLocationTree(string searchKey)
        {
            return LocationDao.SearchLocationTree(searchKey);
        }

        public async Task<RMLocationProfileNode> GetLocationChildren(RMLocationProfileNode node)
        {
            logger.Debug(string.Format($"nodeId:[{node.Id}],pageIndex:[{node.PagerIndex}],pageCount:[{node.PagerSize}]"));
            var userPermission = await SecurityGroupManagementService.GetUserScopePermissionsAsync(TenantLocalValue.LogonUserId);
            if (!userPermission.IsAdmin && node.NodeType == (int)RMNodeLevel.PhysicalRootLocation)
            {
                var phyPermissionIds = userPermission.ScopePermissionInfo.FirstOrDefault(_ => _.DataSourceType == SourceFlag.Physical)?.ScopeIds ?? new List<Guid>();
                return LocationDao.GetRootLocationChildrenWithPermission(node, phyPermissionIds);
            }
            return LocationDao.GetLocationChildren(node);
        }
        public RMLocationProfileNode Convert2ProfileNode(int locationId, bool widthChildIDs = false, bool isChecked = false)
        {
            return LocationDao.Convert2ProfileNode(locationId, widthChildIDs, isChecked);
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.LocationManagement, Action = AuditAction.EditLocationSetting, BeforeHandler = typeof(LocationManagementBeforeAuditHandler), AfterHandler = typeof(LocationManagementAfterAuditHandler))]
        public async Task<RAReturnMessage> SaveLocationSettingAsync(LocationInfo locationSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                if (locationSetting.AvailableSpace < 0)
                {
                    throw new Exception(I18NEntity.GetString("RM_LM_SpaceValueInvalid"));
                }
                if (locationSetting.NodeType == (int)RMNodeLevel.PhysicalBottomLocation)
                {
                    var hasSubLocation = LocationDao.CountSubLocation(locationSetting.LocationId) > 0 ? true : false;
                    if (hasSubLocation)
                    {
                        throw new Exception(I18NEntity.GetString("RM_LM_CannotBeMinimumLocation"));
                    }
                }
                else if (locationSetting.NodeType == (int)RMNodeLevel.PhysicalNormalLocation)
                {
                    var locationOldInfo = LocationDao.GetLocationById(locationSetting.LocationId);
                    IDBInfoDao dao = new DB.Dao.Impl.DBInfoDao();
                    if (!string.IsNullOrEmpty(dao.GetDBNameByTenantId(TenantLocalValue.LogonGroupId)))
                    {
                        if (locationOldInfo.NodeType == (int)RMNodeLevel.PhysicalBottomLocation)
                        {
                            //Location取消勾选"Minimum Location Unit", 并且原来是勾选状态，如果location下有box或者file(查询Cosmos DB)，则不能取消勾选
                            var queryResult = ExplorerDao.QueryByPage(r =>
                            (r.NodeType == (int)RMNodeLevel.PhysicalBox || r.NodeType == (int)RMNodeLevel.PhysicalFile)
                            && r.LocationId == locationOldInfo.UniqueId && r.BoxId == Guid.Empty
                            && r.SourceFlag == (int)SourceFlag.Physical
                            && r.RecordStatus != (int)RMRecordStatus.RMDeleted
                            && r.RecordStatus != (int)RMRecordStatus.MoveOverwrite, 1);
                            if (queryResult.Item1.Count() > 0)
                            {
                                throw new Exception(I18NEntity.GetString("RM_LM_CannotUncheckMinimumLocation"));
                            }
                        }
                    }    
                }
                var locationJson = GetJsonStrByObj(await LocationDao.SaveLocationSettingAsync(locationSetting));

                result.MessageType = RAMessageType.Successful;
            }
            catch (CancelSuiteAssociationInUsingExcetion e)
            {
                logger.Error($"Save location suite association error. Location Id: [{locationSetting.LocationId}], Message: [{e.ToString()}.]");
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = e.Message;
            }
            catch (Exception e)
            {
                logger.Error($"Save location error. Location Id: [{locationSetting.LocationId}], Message: [{e.ToString()}.]");
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = e.Message;
            }
            return result;
        }

        public string GetLocationPathById(Guid id, bool isReplaceI18NKey = true)
        {
            var result = string.Empty;
            try
            {
                var tempLocation = LocationDao.GetLocationByUniqueId(id, isReplaceI18NKey);
                if (tempLocation != null)
                {
                    result = string.Format($"{tempLocation.PathForDisplay}/{tempLocation.Name}");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Get Location Path by id: [{id}], error: [{ex.ToString()}]");
            }
            return result;
        }

        private string GetJsonStrByObj(object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        public bool CheckPhysicalRootLocation(string treeNode)
        {
            var isValid = true;
            try
            {
                RMLocationProfileNode profileRootNode = JsonConvert.DeserializeObject<RMLocationProfileNode>(treeNode);
                var dbRootNode = LocationDao.GetLocationByUniqueId(profileRootNode.UniqueId);
                if(null == dbRootNode)
                {
                    isValid = false;
                }
            }
            catch (Exception)
            {
                isValid = false;
            }
            return isValid;
        }

        


        #endregion

    }
}
