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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.RAPhysical.ConfiguePermission.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.ConfiguePermission
{
    public class PhysicalPermissionProccessor : IPhysicalPermissionProccessor
    {
        #region fields
        private static readonly RALogger logger = RALogger.GetInstance(typeof(PhysicalPermissionProccessor));
        public IExplorerDao ExplorerDao { set; get; } = new ExplorerDao(true);

        private IRMLocationDao mLocationDao { get; set; }
        public IRMLocationDao LocationDao
        {
            get
            {
                if (mLocationDao == null)
                {
                    mLocationDao = (IRMLocationDao)PlatformWindsorManager.GetService(typeof(IRMLocationDao));
                }
                return mLocationDao;
            }
        }

        public IExplorerService ExplorerService { get; set; }

        private PermissionOption _options;

        private IRMReportManager mReportManger;
        private List<Guid> boxIdsCache = null;
        private List<Guid> containerCache = null;
        private Dictionary<Guid, RMLocation> locationDicCache = null;
        public IRMReportManager ReportManager
        {
            get
            {
                if (mReportManger == null)
                {
                    mReportManger = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManger;
            }
        }

        #region 子job更新进度和状态的接口
        private IJobInfoUpdater _jobInfoUpdater;
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
        #endregion

        public IRMScopePermissionDao ScopePermissionDao { get; set; }

        private IExplorerQueryService mExplorerQueryService;
        public IExplorerQueryService ExplorerQueryService
        {
            get
            {
                if (mExplorerQueryService == null)
                {
                    mExplorerQueryService = (IExplorerQueryService)PlatformWindsorManager.GetService(typeof(IExplorerQueryService));
                }
                return mExplorerQueryService;
            }
        }

        private IPermissionManagementService mPermissionManagementService;
        public IPermissionManagementService PermissionManagementService
        {
            get
            {
                if (mPermissionManagementService == null)
                {
                    mPermissionManagementService = (IPermissionManagementService)PlatformWindsorManager.GetService(typeof(IPermissionManagementService));
                }
                return mPermissionManagementService;
            }
        }

        private IUserService mUserService;
        public IUserService UserService
        {
            get
            {
                if (mUserService == null)
                {
                    mUserService = (IUserService)PlatformWindsorManager.GetService(typeof(IUserService));
                }
                return mUserService;
            }
        }

        private IRMSecurityTrimmingHelper mSecurityTrimmingHelper;
        public IRMSecurityTrimmingHelper SecurityTrimmingHelper
        {
            get
            {
                if (mSecurityTrimmingHelper == null)
                {
                    mSecurityTrimmingHelper = (IRMSecurityTrimmingHelper)PlatformWindsorManager.GetService(typeof(IRMSecurityTrimmingHelper));
                }
                return mSecurityTrimmingHelper;
            }
        }

        public bool HasSuccessNode { get; set; }

        public bool HasErrorNode { get; set; }

        #endregion
        private readonly object mCacheLock = new object();
        private Dictionary<Guid, object> mProcessDataIdCache = new Dictionary<Guid, object>();
        private void Init(PermissionOption options)
        {
            _options = options;
            boxIdsCache = new List<Guid>();
            containerCache = new List<Guid>();
            locationDicCache = new Dictionary<Guid, RMLocation>();
            JobInfoUpdater.UpdateJobState(_options.JobId, (int)JobStatus.InProgress);
            JobInfoUpdater.UpdateJobProgress(_options.JobId, 1);
            ReportManager.StartUpdateJobProgress();
        }

        public void Process(PermissionOption options)
        {
            Init(options);
            Process();
        }

        private void Process()
        {
            var dic = ScopePermissionDao.GetScopePermissionIds(_options.Scopes.Select(o => o.ScopeFullPath).ToList());
            var failedScopeIds = new List<string>();
            var successedScopeIds = new List<string>();
            foreach (var scope in _options.Scopes)
            {
                try
                {
                    logger.Info($"Start to process scope:  {scope.ScopeId}");
                    int permissionId;
                    if (dic.TryGetValue(scope.ScopeId, out permissionId))
                    {
                        int locationId;
                        int.TryParse(scope.ScopeId, out locationId);
                        if (locationId > 0)
                        {
                            //process location node
                            var locationInfo = LocationDao.GetLocationById(locationId);
                            if (locationInfo.NodeType == (int)RMNodeType.PhysicalBottomLocation)
                            {
                                ProcessPhysicalLocation(new PhysicalLocation(locationInfo.UniqueId), permissionId);
                            }
                            else
                            {
                                List<IPhysicalLocation> allSubBottomLocation = new List<IPhysicalLocation>();
                                List<IPhysicalLocation> notBreakBottomLocation = new List<IPhysicalLocation>();
                                IPhysicalLocation normalLocation = new PhysicalLocation(locationInfo.UniqueId);
                                GetSubBottomLocation(normalLocation, allSubBottomLocation);
                                notBreakBottomLocation = GetNotBreakBottomLocation(allSubBottomLocation, scope.ScopeFullPath);
                                foreach (var bottomLocation in notBreakBottomLocation)
                                {
                                    ProcessPhysicalLocation(new PhysicalLocation(bottomLocation.UniqueId), permissionId);
                                }
                            }
                        }
                        else
                        {
                            //process box/folder node
                            var record = GetRecord(scope.ScopeId);
                            if (record != null)
                            {
                                ProcessItem(record, permissionId);
                            }
                            else
                            {
                                logger.Warn($"Can't find the record with id : {scope.ScopeId}");
                            }
                        }
                    }
                    else
                    {
                        logger.Warn($"Can't find the scope permission with scope : {scope.ScopeId}");
                    }
                    successedScopeIds.Add(scope.ScopeId);
                }
                catch (Exception ex)
                {
                    failedScopeIds.Add(scope.ScopeId);
                }
            }

            if (failedScopeIds.Count > 0)
            {
                ScopePermissionDao.AddOrUpdatePermissionjobInfo(failedScopeIds, _options.JobId);
            }
            if (successedScopeIds.Count > 0)
            {
                ScopePermissionDao.DeletePermissionJobInfo(successedScopeIds);
            }
        }

        public void GetSubBottomLocation(IPhysicalLocation location, List<IPhysicalLocation> allSubBottomLocation)
        {
            List<IPhysicalLocation> subLocations = location.AllSubLocations;
            foreach (IPhysicalLocation subLocation in subLocations)
            {
                if (subLocation.IsBottomLocation)
                {
                    allSubBottomLocation.Add(subLocation);
                }
                else
                {
                    GetSubBottomLocation(subLocation, allSubBottomLocation);
                }
            }
        }

        public List<IPhysicalLocation> GetNotBreakBottomLocation(List<IPhysicalLocation> bottomLocations, string normalFullPath)
        {
            List<IPhysicalLocation> notBreakInherit = new List<IPhysicalLocation>();
            foreach (IPhysicalLocation bottomLocation in bottomLocations)
            {
                var scopePath = GetScopeIdFullPath(bottomLocation.UniqueId.ToString());
                bool isBreakInheit = ScopePermissionDao.IsBottomLocationBreakInherForNormalNode(scopePath, normalFullPath);
                if (!isBreakInheit)
                {
                    notBreakInherit.Add(bottomLocation);
                }
            }
            return notBreakInherit;
        }

        public string GetScopeIdFullPath(string scopeId)
        {
            var idFullPath = "";
            try
            {
                RMLocation location = null;
                var locationPath = "";
                int locationId;
                int.TryParse(scopeId, out locationId);
                var isLocationNode = false;
                #region 判断是不是location节点，如果是获取location信息
                if (locationId > 0)
                {
                    location = LocationDao.GetLocationById(locationId);
                    idFullPath = $"{location.DirPath}{locationId}/";
                    isLocationNode = true;
                }
                else
                {
                    location = LocationDao.GetLocationInfo(new Guid(scopeId));
                    if (location != null)
                    {
                        idFullPath = location.DirPath;
                        isLocationNode = true;
                    }
                }
                #endregion

                if (!isLocationNode)
                {
                    #region 获取Box/Folder/Record的Id Full Path
                    var node = ExplorerDao.QueryAll(o => o.Id == new Guid(scopeId)).FirstOrDefault();
                    //var id = node.NodeId;
                    //location = LocationDao.GetLocationInfo(node.LocationId);
                    //locationPath = location.DirPath;
                    switch (node?.NodeType)
                    {

                        //case (int)RMNodeType.PhyBox:
                        //    idFullPath = $"{locationPath}{id}/";
                        //    break;
                        //case (int)RMNodeType.PhyFile:
                        //    if (node.BoxId != Guid.Empty)
                        //    {
                        //        idFullPath = $"{locationPath}{node.BoxId}/{id}/";
                        //    }
                        //    else
                        //    {
                        //        //location下的folder
                        //        idFullPath = $"{locationPath}{id}/";
                        //    }
                        //    break;
                        //case (int)RMNodeType.PhyRecord:
                        //    if (node.BoxId != Guid.Empty)
                        //    {
                        //        idFullPath = $"{locationPath}{node.BoxId}/{node.FileId}/{id}/";
                        //    }
                        //    else
                        //    {
                        //        idFullPath = $"{locationPath}{node.FileId}/{id}/";
                        //    }
                        case (int)RMNodeType.PhyBox:
                        case (int)RMNodeType.PhyCustom:
                        case (int)RMNodeType.PhyFile:
                        case (int)RMNodeType.PhyRecord:
                            idFullPath = ExplorerService.GetPhysicalScopeIdFullPath(new Guid(scopeId));
                            break;
                        default:
                            break;
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred when GetScopeIdFullPath, message:{ex.ToString()}");
            }
            return idFullPath;
        }

        private void ProcessItem(Record item, int permissionId)
        {
            using (new CheckJobStopScope())
            {
                ReportManager.IncreaseBase(1);
                if (item.NodeType == (int)RMNodeLevel.PhysicalBox)
                {
                    ProcessBox(new PhysicalBox(item), permissionId);
                }
                else if (item.NodeType == (int)RMNodeLevel.PhysicalFile)
                {
                    ProcessFile(new PhysicalFile(item), permissionId);
                }
                else if (item.NodeType == (int)RMNodeLevel.PhysicalCustom)
                {
                    ProcessCustom(new PhysicalCustom(item), permissionId);
                }
            }
        }

        private void ProcessPhysicalLocation(PhysicalLocation location, int permissionId)
        {
            logger.Info($"process location: {location.UniqueId}");
            //location不需要更新cosmosdb
            ProcessDetail(location, JobDetailsStatus.Successful);
            var boxes = location.Boxes;
            ReportManager.IncreaseBase(boxes.Count);

            foreach (var box in boxes)
            {
                ProcessBox(box as PhysicalBox, permissionId);
            }

            var files = location.Files;
            ReportManager.IncreaseBase(files.Count);
            foreach (var file in files)
            {
                ProcessFile(file as PhysicalFile, permissionId);
            }

            var containers = location.Containers;
            ReportManager.IncreaseBase(containers.Count);
            foreach (var container in containers)
            {
                ProcessCustom(container as PhysicalCustom, permissionId);
            }
        }

        private void ProcessBox(PhysicalBox box, int permissionId)
        {
            try
            {
                if (!HasPermission(box.Id, permissionId))
                {
                    //如果对于当前box没有权限,不需要处理
                    logger.Warn($"Has no permission, ignore box : {box.Id}");
                    ProcessDetail(box, JobDetailsStatus.Skipped, "RM_JM_Skip_UpdatePermission");
                    return;
                }

                logger.Info($"Process box : {box.Id}");
                if (permissionId == box.Record.ScopePermissionId)
                {
                    logger.Warn($"Permission is the same with existing one, ignore box : {box.Id}");
                    ProcessDetail(box, JobDetailsStatus.Skipped);
                }
                else
                {
                    UpdateScopePermission(box.Record, permissionId);
                    ProcessDetail(box, JobDetailsStatus.Successful);
                    HasSuccessNode = true;
                }
                //var files = box.Files;
                ProcessBoxFilesByPage(box, permissionId);
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred update box Id : {box.Id}. error : {ex.ToString()}");
                ProcessDetail(box, JobDetailsStatus.Failed, "RM_JM_Failed_UpdatePermission");
                HasErrorNode = true;
                throw;
            }
        }

        private void ProcessCustom(PhysicalCustom custom, int permissionId)
        {
            try
            {
                if (!HasPermission(custom.Id, permissionId))
                {
                    //如果对于当前box没有权限,不需要处理
                    logger.Warn($"Has no permission, ignore container : {custom.Id}");
                    ProcessDetail(custom, JobDetailsStatus.Skipped, "RM_JM_Skip_UpdatePermission");
                    return;
                }

                logger.Info($"Process container : {custom.Id}");
                if (permissionId == custom.Record.ScopePermissionId)
                {
                    logger.Warn($"Permission is the same with existing one, ignore box : {custom.Id}");
                    ProcessDetail(custom, JobDetailsStatus.Skipped);
                }
                else
                {
                    UpdateScopePermission(custom.Record, permissionId);
                    ProcessDetail(custom, JobDetailsStatus.Successful);
                    HasSuccessNode = true;
                }

                var files = custom.Files;
                ReportManager.IncreaseBase(files.Count);
                foreach (var file in files)
                {
                    ProcessFile(file as PhysicalFile, permissionId);
                }

                var boxes = custom.Boxes;
                ReportManager.IncreaseBase(boxes.Count);
                foreach (var box in boxes)
                {
                    ProcessBox(box as PhysicalBox, permissionId);
                }

                var subCustomContainers = custom.CustomContainers;
                ReportManager.IncreaseBase(subCustomContainers.Count);
                foreach (var container in subCustomContainers)
                {
                    ProcessCustom(container as PhysicalCustom, permissionId);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred update box Id : {custom.Id}. error : {ex.ToString()}");
                ProcessDetail(custom, JobDetailsStatus.Failed, "RM_JM_Failed_UpdatePermission");
                HasErrorNode = true;
                throw;
            }
        }

        private void ProcessingFile(PhysicalFile file, int permissionId, Dictionary<Guid, bool> FilesPermission, List<Guid> scopeIdsUpdate)
        {
            try
            {
                if (FilesPermission.TryGetValue(file.Id, out var isValid) && !isValid)
                {
                    logger.Warn($"Has no permission, ignore file : {file.Id}");
                    ProcessDetail(file, JobDetailsStatus.Skipped, "RM_JM_Skip_UpdatePermission");
                    return;
                }
                logger.Info($"Process file : {file.Id}");
                if (permissionId == file.Record.ScopePermissionId)
                {
                    logger.Warn($"Permission is the same with existing one, ignore file : {file.Id}");
                    ProcessDetail(file, JobDetailsStatus.Skipped);
                }
                else
                {
                    //UpdateScopePermission(file.Record, permissionId);
                    AddScopePermission(file.Record, permissionId);
                    scopeIdsUpdate.Add(file.Record.Id);
                    ProcessDetail(file, JobDetailsStatus.Successful);
                    HasSuccessNode = true;
                }
                var records = file.Records;
                if (records.Any())
                {
                    ProcessRecords(records, permissionId);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred update box Id : {file.Id}. error : {ex.ToString()}");
                ProcessDetail(file, JobDetailsStatus.Failed, "RM_JM_Failed_UpdatePermission");
                HasErrorNode = true;
                throw;
            }
        }

        private void ProcessFile(PhysicalFile file, int permissionId)
        {
            try
            {
                if (!HasPermission(file.Id, permissionId))
                {
                    logger.Warn($"Has no permission, ignore file : {file.Id}");
                    ProcessDetail(file, JobDetailsStatus.Skipped, "RM_JM_Skip_UpdatePermission");
                    //如果对于当前File没有权限,不需要处理
                    return;
                }

                logger.Info($"Process file : {file.Id}");
                if (permissionId == file.Record.ScopePermissionId)
                {
                    logger.Warn($"Permission is the same with existing one, ignore file : {file.Id}");
                    ProcessDetail(file, JobDetailsStatus.Skipped);
                }
                else
                {
                    UpdateScopePermission(file.Record, permissionId);
                    ProcessDetail(file, JobDetailsStatus.Successful);
                    HasSuccessNode = true;
                }
                ProcessRecords(file.Records, permissionId);  //此处最好不要取所有的records，而是分页取。一页处理完毕后，再取下一页数据?
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred update box Id : {file.Id}. error : {ex.ToString()}");
                ProcessDetail(file, JobDetailsStatus.Failed, "RM_JM_Failed_UpdatePermission");
                HasErrorNode = true;
                throw;
            }

        }

        /// <summary>
        /// 检查当前节点是否符合权限
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="permissionId"></param>
        /// <returns></returns>
        private bool HasPermission(Guid scope, int permissionId)
        {
            var existPermissionId = ScopePermissionDao.GetScopePermissionId(scope.ToString());
            if (existPermissionId == 0) return true;  //继承权限

            return existPermissionId == permissionId;
        }

        private Dictionary<Guid, bool> GetMultipleFilesPermission(List<Guid> scopes, int permissionId)
        {
            var result = new Dictionary<Guid, bool>();
            var scopeIds = scopes.Select(x => x.ToString()).ToList();
            var dic = ScopePermissionDao.GetScopesPermissionWithIds(scopeIds);

            foreach (var item in scopes)
            {
                var key = item.ToString();

                if (!dic.TryGetValue(key, out var exist))
                {
                    result[item] = true;
                    continue;
                }
                result[item] = (exist == permissionId);

            }
            return result;
        }

        private void ProcessRecords(IList<IPhysicalRecord> records, int permissionId)
        {
            ReportManager.IncreaseBase(records.Count);
            foreach (var record in records)
            {
                ProcessRecord(record as PhysicalRecord, permissionId);
            }
        }

        private void ProcessRecord(PhysicalRecord record, int permissionId)
        {
            try
            {
                using (new CheckJobStopScope())
                {
                    logger.Info($"Process record: {record.Id}");
                    if (permissionId == record.Record.ScopePermissionId)
                    {
                        logger.Warn($"Permission is the same with existing one, ignore record : {record.Id}");
                        ProcessDetail(record, JobDetailsStatus.Skipped);
                        return;
                    }
                    UpdateScopePermission(record.Record, permissionId);
                    ProcessDetail(record, JobDetailsStatus.Successful);
                    HasSuccessNode = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred update record Id : {record.Id}. error : {ex.ToString()}");
                ProcessDetail(record, JobDetailsStatus.Failed, "RM_JM_Failed_UpdatePermission");
                HasErrorNode = true;
            }
        }

        private void ProcessDetailForGlobalSearch(PhysicalCustom record, JobDetailsStatus status, string comment = "")
        {
            var dirPath = GetDirPath(record.DirPath);

            ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
            {
                ObjectName = record.Name,
                FullPath = dirPath,
                Type = "RM_PRM_PRE_TableItemType_Container",
                Action = "RM_JM_GlobalSearch_AccessControlAction",
                Status = status,
                Comment = comment
            });
            ReportManager.Increase(1);
        }

        private void ProcessDetailForGlobalSearch(PhysicalRecord record, JobDetailsStatus status, string comment = "")
        {
            var dirPath = GetDirPath(record.DirPath);

            ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
            {
                ObjectName = record.Name,
                FullPath = dirPath,
                Type = "RM_JS_Rule_ObjectLevel_PhysicalRecord",
                Action = "RM_JM_GlobalSearch_AccessControlAction",
                Status = status,
                Comment = comment
            });
            ReportManager.Increase(1);
        }

        private void ProcessDetailForGlobalSearch(PhysicalFile file, JobDetailsStatus status, string comment = "")
        {
            var dirPath = GetDirPath(file.DirPath);

            ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
            {
                ObjectName = file.Name,
                FullPath = dirPath,
                Type = "RM_Common_ObjectLevel_PhysicalFile",
                Action = "RM_JM_GlobalSearch_AccessControlAction",
                Status = status,
                Comment = comment
            }); ;
            ReportManager.Increase(1);
        }

        private void ProcessDetailForGlobalSearch(PhysicalBox box, JobDetailsStatus status, string comment = "")
        {
            var dirPath = GetDirPath(box.DirPath);

            ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
            {
                ObjectName = box.Name,
                FullPath = dirPath,
                Type = "RM_Common_ObjectLevel_PhysicalBox",
                Action = "RM_JM_GlobalSearch_AccessControlAction",
                Status = status,
                Comment = comment
            });
            ReportManager.Increase(1);
        }



        private string GetDirPath(string dirPath)
        {
            var i18nRootName = I18NEntity.GetString("RM_SPS_Location_RootNode");
            if (!string.IsNullOrEmpty(dirPath) && dirPath.StartsWith(i18nRootName))
            {
                dirPath = "RM_SPS_Location_RootNode" + dirPath.Substring(i18nRootName.Length);
            }
            return dirPath;
        }

        private void ProcessDetail(PhysicalRecord record, JobDetailsStatus status, string comment = "")
        {
            var dirPath = GetDirPath(record.DirPath);
            ReportManager.SendJobDetail(new JMSetPermissionJobDetails()
            {
                ObjectName = record.Name,
                FullPath = dirPath,
                ItemType = "RM_JS_Rule_ObjectLevel_PhysicalRecord",
                Status = status,
                Comment = comment
            });
            ReportManager.Increase(1);
        }

        private void ProcessDetail(PhysicalFile file, JobDetailsStatus status, string comment = "")
        {
            var dirPath = GetDirPath(file.DirPath);
            ReportManager.SendJobDetail(new JMSetPermissionJobDetails()
            {
                ObjectName = file.Name,
                FullPath = dirPath,
                ItemType = "RM_Common_ObjectLevel_PhysicalFile",
                Status = status,
                Comment = comment
            });
            ReportManager.Increase(1);
        }

        private void ProcessDetail(PhysicalBox box, JobDetailsStatus status, string comment = "")
        {
            var dirPath = GetDirPath(box.DirPath);
            ReportManager.SendJobDetail(new JMSetPermissionJobDetails()
            {
                ObjectName = box.Name,
                FullPath = dirPath,
                ItemType = "RM_Common_ObjectLevel_PhysicalBox",
                Status = status,
                Comment = comment
            });
            ReportManager.Increase(1);
        }

        private void ProcessDetail(PhysicalCustom custom, JobDetailsStatus status, string comment = "")
        {
            var dirPath = GetDirPath(custom.DirPath);
            ReportManager.SendJobDetail(new JMSetPermissionJobDetails()
            {
                ObjectName = custom.Name,
                FullPath = dirPath,
                ItemType = "RM_PRM_PRE_TableItemType_Container",
                Status = status,
                Comment = comment
            });
            ReportManager.Increase(1);
        }

        private void ProcessDetail(PhysicalLocation location, JobDetailsStatus status, string comment = "")
        {

            var dirPath = GetDirPath(location.DirPath);

            ReportManager.SendJobDetail(new JMSetPermissionJobDetails()
            {
                ObjectName = location.Name,
                FullPath = dirPath,
                ItemType = "RM_Common_ObjectLevel_PhysicalLocation",
                Status = status,
                Comment = comment
            });
            ReportManager.Increase(1);
        }
        private void UpdateScopePermission(Record record, int permissionId)
        {
            record.ScopePermissionId = permissionId;
            ExplorerDao.UpdateAll(a => a.Id == record.Id, o => o.ScopePermissionId = permissionId);
        }

        private void UpdateRecordsScopePermission(List<Guid> recordIds, int permissionId)
        {
            ExplorerDao.UpdateAll(a => recordIds.Contains(a.Id), o => o.ScopePermissionId = permissionId);
        }

        private void AddScopePermission(Record record, int permissionId)
        {
            record.ScopePermissionId = permissionId;
        }

        private void ProcessBoxFilesByPage(PhysicalBox box, int permissionId)
        {
            const int pageSize = 2000;
            const int updateBatchSize = 500;
            var continuation = string.Empty;
            bool hasMore;
            do
            {
                var page = ExplorerDao.QueryByPage(
                    r => r.SourceFlag == (int)SourceFlag.Physical
                         && r.RecordStatus != 3
                         && r.BoxId == box.Record.Id
                         && r.NodeType == (int)RMNodeLevel.PhysicalFile,
                    pageSize,
                    continuation);

                var batch = page.Item1?.ToList() ?? new List<Record>();
                continuation = page.Item2;
                hasMore = !string.IsNullOrEmpty(continuation);

                if (batch.Count == 0)
                {
                    break;
                }

                ReportManager.IncreaseBase(batch.Count);

                var files = batch.Select(x => new PhysicalFile(box.ParentLocation, box, x)).ToList();
                var scopeIdsUpdate = new List<Guid>();
                var dicFilesPermission = GetMultipleFilesPermission(files.Select(f => f.Id).ToList(), permissionId);

                foreach (var file in files)
                {
                    ProcessingFile(file, permissionId, dicFilesPermission, scopeIdsUpdate);
                }

                if (scopeIdsUpdate.Any())
                {
                    foreach (var chunk in scopeIdsUpdate.Chunk(updateBatchSize))
                    {
                        var ids = chunk.ToList();
                        UpdateRecordsScopePermission(ids, permissionId);
                    }
                }
            }
            while (hasMore);
        }

        private Record GetRecord(string id)
        {
            return ExplorerDao.GetPhysicalRecordById(new Guid(id));
        }

        #region Set Permission From Global Search 
        public Task ProcessByGlobalSearch(PermissionOption options)
        {
            Init(options);
            if (_options.GSJobContext.QueryDto != null || _options.GSJobContext.QueryV3Dto != null)
            {
                return ProcessBySearchResultAsync();
            }
            else
            {
                return ProcessBySelectedNodesAsync();
            }
        }

        private async Task ProcessBySearchResultAsync()
        {
            logger.Info("set permission by search result.");
            ITenantInfoDao tenantInfoDao = PlatformWindsorManager.GetService<ITenantInfoDao>();
            //int isDataMoved = 1; //tenantInfoDao.CheckIfExplorerDataMoved(Contract.Tenant.TenantLocalValue.LogonGroupId);
            ///目前所有Tenant数据已经升级完成, Search页面也只能进入新页面, 这里也更新为只使用新Search;  CI in Sep 
            //if (isDataMoved == 1)
            //{
            logger.Info("Cosmos Data moved, use new search, user id {0}", _options.GSJobContext.UserId);
            Contract.Tenant.TenantLocalValue.LogonUserId = _options.GSJobContext.UserId;
            var v2Dto = _options.GSJobContext.QueryV3Dto;
            v2Dto.PagingInfo = new ExplorerPagingInfo()
            {
                PageIndex = string.Empty,
                PageSize = 500
            };
            await ProcessNodesByPageV3Async(v2Dto);
            //}
            //else
            //{
            //    logger.Warn("Old search logic");
            //    var dto = _options.GSJobContext.QueryDto;
            //    dto.PagingInfo = new ExplorerPagingInfo()
            //    {
            //        PageIndex = string.Empty,
            //        PageSize = 500
            //    };
            //    bool hasNext;
            //    hasNext = await ProcessNodesByPageAsync(dto);
            //}
        }

        private async Task ProcessBySelectedNodesAsync()
        {
            logger.Info("set permission by selected nodes.");
            var ids = _options.GSJobContext.NodeIds;
            List<int> nonePermissionIds = new List<int>();

            if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.PhysicalAdmin)))
            {
                var nodePermissionIds = ExplorerService.GetPhysicalObjectPermissionIds(ids);
                if (nodePermissionIds.Count > 0)
                {
                    var userAndGroupIds = await UserService.GetUserAndGroupIdsAsync(TenantLocalValue.LogonUserId);
                    var scopePermissionIds = PermissionManagementService.GetScopePermissionIds(userAndGroupIds);
                    nonePermissionIds = nodePermissionIds.Where(o => !scopePermissionIds.Contains(o) && o != 0).ToList();
                    logger.Info($"None permision ids:{string.Join(",", nonePermissionIds)}");
                }
            }
            //按照NodeType排序，先处理高级别节点
            var nodes = ExplorerDao.QueryAll(o => ids.Contains(o.Id)).OrderBy(o => o.NodeType).ThenBy(o => o.CreateDate).ToList();
            if (nonePermissionIds.Count > 0)
            {
                var nonePermissionRecords = nodes.Where(r => nonePermissionIds.Contains(r.ScopePermissionId)).ToList();
                foreach (var record in nonePermissionRecords)
                {
                    if (record.NodeType == (int)RMNodeLevel.PhysicalFile)
                    {
                        var folder = new PhysicalFile(record);
                        ProcessDetailForSkipData(folder.Name, folder.DirPath, "RM_Common_ObjectLevel_PhysicalFile", "RM_JS_SPS_UniqueIdDisplay_DelegateWarning");
                    }
                    else if (record.NodeType == (int)RMNodeLevel.PhysicalBox)
                    {
                        var box = new PhysicalBox(record);
                        ProcessDetailForSkipData(box.Name, box.DirPath, "RM_Common_ObjectLevel_PhysicalBox", "RM_JS_SPS_UniqueIdDisplay_DelegateWarning");
                    }
                    HasSuccessNode = true;
                }
                nodes = nodes.Where(r => !nonePermissionIds.Contains(r.ScopePermissionId)).ToList();
            }
            await ProcessNodesAsync(nodes);
        }

        private void ProcessDetailForSkipData(string name, string path, string type, string comment)
        {
            var dirPath = GetDirPath(path);
            ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
            {
                ObjectName = name,
                FullPath = dirPath,
                Type = type,
                Action = "RM_JM_GlobalSearch_AccessControlAction",
                Status = JobDetailsStatus.Skipped,
                Comment = comment
            }); ;
            ReportManager.Increase(1);
        }
       

        private async Task<bool> ProcessNodesByPageV3Async(ExplorerQueryV3Dto dto)
        {
            //IExplorerQueryParamProcesser explorerqurys = PlatformWindsorManager.GetService<IExplorerQueryParamProcesser>();          
            //explorerqurys.ProcessV3(dto.QueryOption);

            //var queryResult = ExplorerDao.SearchRecordsV3(dto, filterOptionV2); 

            //access control使用created时间正序查询，先处理上层节点
            bool hasNext = false;
            if (dto.QueryOption.OrderColumn == null)
            {
                dto.QueryOption.OrderColumn = new ExplorerQueryOrderColumn()
                {
                    Column = new ExplorerQueryColumn() { Name = CosmosConst.C_NodeType },
                    OrderAsc = true
                };
            }
            var queryResult = await ExplorerQueryService.QueryDataListWithoutTotalAsync(dto);
            if (queryResult?.PagingInfo != null)
            {
                dto.PagingInfo = queryResult?.PagingInfo;
            }
            ArgumentCheck.NotNull(queryResult, nameof(queryResult));
            var records = queryResult.Datas.ToList();
            if (records.Count > 0)
            {
                var ids = records.Select(r => r.Id).ToList();
                var nodes = ExplorerDao.QueryAll(o => ids.Contains(o.Id)).OrderBy(o => o.NodeType).ThenBy(o => o.CreateDate).ToList();
                await ProcessNodesAsync(nodes);
            }
            hasNext = queryResult?.PagingInfo != null ? queryResult.PagingInfo.HasNextPage : false;
            if (hasNext)
            {
                hasNext = await ProcessNodesByPageV3Async(dto);
            }

            return hasNext;
        }

        private async Task ProcessNodesAsync(List<Record> nodes)
        {
            if (nodes.Count > 0)
            {
                var permissionDto = GenScopePermissionDto(nodes);
                //保存节点权限信息到Sql
                await ScopePermissionDao.SaveLocationPermissionAsync(permissionDto);
                var dic = ScopePermissionDao.GetScopePermissionIds(permissionDto.ScopeInfos.Select(o => o.ScopeFullPath).ToList());
                foreach (var item in nodes)
                {
                    ReportManager.IncreaseBase(1);
                    try
                    {
                        int permissionId = 0;
                        dic.TryGetValue(item.Id.ToString(), out permissionId);
                        switch (item.NodeType)
                        {
                            case (int)RMNodeLevel.PhysicalCustom:
                                //处理查询结果中container时，加入判断1.如果container打破继承 或者2.container及container的parent没有处理过，则需要处理该container
                                if (IfDataNeedProcess(item))
                                {
                                    ProcessCustomForGS(new PhysicalCustom(item), permissionId);
                                }
                                break;
                            case (int)RMNodeLevel.PhysicalBox:
                                if (IfDataNeedProcess(item))
                                {
                                    ProcessBoxForGS(new PhysicalBox(item), permissionId);
                                }
                                break;
                            case (int)RMNodeLevel.PhysicalFile:
                                if ((item.BoxId == Guid.Empty && item.Ancestors == null) || IfDataNeedProcess(item))
                                {
                                    ProcessFileForGS(new PhysicalFile(item), permissionId);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"An error when set per,id:{item?.Id}, message:{ex.ToString()}");
                    }
                }
            }
        }

        private bool IfDataNeedProcess(Record container)
        {
            lock (mCacheLock)
            {
                //if (mProcessDataIdCache.ContainsKey(container.Id) || (container.Ancestors != null && container.Ancestors.Any(n => mProcessDataIdCache.ContainsKey(n))))
                //{
                //    return true;
                //}
                //else
                //{
                //    return false;
                //}


                if (mProcessDataIdCache.ContainsKey(container.NodeId))
                {
                    return false;
                }

                //数据本身是打破继承的,
                if (IsBreakInherPermission(container.NodeId))
                {
                    return true;
                }

                //for new data
                List<Guid> parentIds = new List<Guid>();
                parentIds.AddRange(container.Ancestors);
                if (parentIds != null && parentIds.Count > 0)
                {
                    //获取当前节点所有打破继承父节点
                    var parentBreakInherPermissionNodeIds = GetParentBreakInherPermissionNodeIds(parentIds.Select(l => l.ToString()).ToList());
                    if (parentBreakInherPermissionNodeIds.Count > 0)
                    {
                        //找到最近的打破继承父节点,检查job是否已经处理该节点，如果没有，则处理当前节点
                        parentIds.Reverse();
                        Guid firstParent = Guid.Empty;
                        foreach (var parentId in parentIds)
                        {
                            if (parentBreakInherPermissionNodeIds.Contains(parentId))
                            {
                                firstParent = parentId;
                                break;
                            }
                        }
                        if (!mProcessDataIdCache.ContainsKey(firstParent))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        //如果父节点都没有打破继承，则检查是否某个父节点已经被处理.如果没有，则处理当前节点
                        if (!mProcessDataIdCache.Keys.Any(a => parentIds.Contains(a)))
                        {
                            return true;
                        }
                    }


                    ////从缓存中找到已经处理的父节点
                    //var ancestorIds = mProcessDataIdCache.Keys.Where(a => parentIds.Contains(a)).ToList();
                    //if (ancestorIds != null && ancestorIds.Count > 0)
                    //{
                    //    //找到最近的父节点
                    //    parentIds.Reverse();
                    //    Guid firstBreakInhertParentNodeId = Guid.Empty;
                    //    foreach (var parentId in parentIds)
                    //    {
                    //        if (ancestorIds.Contains(parentId))
                    //        {
                    //            firstBreakInhertParentNodeId = parentId;
                    //            break;
                    //        }
                    //    }

                    //    //检查最近父节点到当前节点之间是否有打破继承的节点，如果有，则认为当前节点应该处理
                    //    int startIndex = parentIds.IndexOf(firstBreakInhertParentNodeId);
                    //    if (startIndex > 0)
                    //    {
                    //        List<Guid> parentIdList = new List<Guid>();
                    //        parentIdList.AddRange(parentIds.Take(startIndex));
                    //        if (parentIdList.Count > 0)
                    //        {
                    //            var parentBreakInherPermissionNodeIds = GetParentBreakInherPermissionNodeIds(parentIdList.Select(l => l.ToString()).ToList());
                    //            if (parentBreakInherPermissionNodeIds.Count > 0)
                    //            {
                    //                return true;
                    //            }
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    //上层节点不在缓存中，则需要处理
                    //    return true;
                    //}
                }

                return false;
            }
        }

        private void AddToCache(Guid id)
        {
            lock (mCacheLock)
            {
                if (!mProcessDataIdCache.ContainsKey(id))
                {
                    mProcessDataIdCache.Add(id, null);
                }
                if (mProcessDataIdCache.Count % 1000 == 0)
                {
                    logger.Info("Cached Data Id Count:" + mProcessDataIdCache.Count);
                }
            }
        }

        private void ProcessBoxForGS(PhysicalBox box, int permissionId)
        {
            try
            {
                logger.Info($"Process box, id: {box.Id}");
                if (permissionId == box.Record.ScopePermissionId)
                {
                    logger.Info($"Permission id does not change: {box.Id}");
                    ProcessDetailForGlobalSearch(box, JobDetailsStatus.Successful);
                }
                else
                {
                    UpdateScopePermission(box.Record, permissionId);
                    logger.Info($"Successed to update scope permission: {box.Id}");
                    ProcessDetailForGlobalSearch(box, JobDetailsStatus.Successful);
                }
                HasSuccessNode = true;

                var files = box.Files;
                ReportManager.IncreaseBase(files.Count);
                foreach (var file in files)
                {
                    if (IsBreakInherPermission(file.Id))
                    {
                        continue;
                    }
                    ProcessFileForGS(file as PhysicalFile, permissionId);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred update box Id : {box.Id}. error : {ex.ToString()}");
                ProcessDetailForGlobalSearch(box, JobDetailsStatus.Failed, "RM_JM_Failed_UpdatePermission");
                HasErrorNode = true;
                throw;
            }
            finally
            {
                AddToCache(box.Id);
            }
        }

        private void ProcessCustomForGS(PhysicalCustom custom, int permissionId)
        {
            try
            {
                logger.Info($"Process custom container, id: {custom.Id}");
                if (permissionId == custom.Record.ScopePermissionId)
                {
                    logger.Info($"Permission id does not change: {custom.Id}");
                    ProcessDetailForGlobalSearch(custom, JobDetailsStatus.Successful);
                }
                else
                {
                    UpdateScopePermission(custom.Record, permissionId);
                    logger.Info($"Successed to update scope permission: {custom.Id}");
                    ProcessDetailForGlobalSearch(custom, JobDetailsStatus.Successful);
                }
                HasSuccessNode = true;

                var files = custom.Files;
                ReportManager.IncreaseBase(files.Count);
                foreach (var file in files)
                {
                    if (IsBreakInherPermission(file.Id))
                    {
                        continue;
                    }
                    ProcessFileForGS(file as PhysicalFile, permissionId);
                }

                var boxes = custom.Boxes;
                ReportManager.IncreaseBase(boxes.Count);
                foreach (var box in boxes)
                {
                    if (IsBreakInherPermission(box.Id))
                    {
                        continue;
                    }
                    ProcessBoxForGS(box as PhysicalBox, permissionId);
                }

                var customContainers = custom.CustomContainers;
                ReportManager.IncreaseBase(customContainers.Count);
                foreach (var container in customContainers)
                {
                    //如果子container打破继承则不处理
                    if (IsBreakInherPermission(container.Id))
                    {
                        continue;
                    }
                    ProcessCustomForGS(container as PhysicalCustom, permissionId);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred update custom container Id : {custom.Id}. error : {ex.ToString()}");
                ProcessDetailForGlobalSearch(custom, JobDetailsStatus.Failed, "RM_JM_Failed_UpdatePermission");
                HasErrorNode = true;
                throw;
            }
            finally
            {
                AddToCache(custom.Id);
            }
        }

        //private void ProcessSubCustomForGS(PhysicalCustom custom, int permissionId)
        //{
        //    try
        //    {
        //        logger.Info($"Process custom container : {custom.Name}, id: {custom.Id}");
        //        if (permissionId == custom.Record.ScopePermissionId)
        //        {
        //            logger.Info($"Permission id does not change: {custom.Name}");
        //            ProcessDetailForGlobalSearch(custom, JobDetailsStatus.Successful);
        //        }
        //        else
        //        {
        //            UpdateScopePermission(custom.Record, permissionId);
        //            logger.Info($"Successed to update scope permission: {custom.Name}");
        //            ProcessDetailForGlobalSearch(custom, JobDetailsStatus.Successful);
        //        }
        //        HasSuccessNode = true;

        //        var files = custom.Files;
        //        ReportManager.IncreaseBase(files.Count);
        //        foreach (var file in files)
        //        {
        //            if (IsBreakInherPermission(file.Id))
        //            {
        //                continue;
        //            }
        //            ProcessFileForGS(file as PhysicalFile, permissionId);
        //        }

        //        var boxes = custom.Boxes;
        //        ReportManager.IncreaseBase(boxes.Count);
        //        foreach (var box in boxes)
        //        {
        //            if (IsBreakInherPermission(box.Id))
        //            {
        //                continue;
        //            }
        //            ProcessBoxForGS(box as PhysicalBox, permissionId);
        //        }

        //        var customContainers = custom.CustomContainers;
        //        ReportManager.IncreaseBase(customContainers.Count);
        //        foreach (var container in customContainers)
        //        {
        //            //如果子container打破继承则不处理
        //            if (IsBreakInherPermission(container.Id))
        //            {
        //                continue;
        //            }
        //            ProcessSubCustomForGS(container as PhysicalCustom, permissionId);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error($"An error occurred update custom container: {custom.Name}, Id : {custom.Id}. error : {ex.ToString()}");
        //        ProcessDetailForGlobalSearch(custom, JobDetailsStatus.Failed, "RM_JM_Failed_UpdatePermission");
        //        HasErrorNode = true;
        //        throw;
        //    }
        //    finally
        //    {
        //        AddToCache(custom.Id);
        //    }
        //}

        private void ProcessFileForGS(PhysicalFile file, int permissionId)
        {
            try
            {
                logger.Info($"Process file, id: {file.Id}");
                if (permissionId == file.Record.ScopePermissionId)
                {
                    logger.Info($"Permission id does not change: {file.Id}");
                    ProcessDetailForGlobalSearch(file, JobDetailsStatus.Successful);
                }
                else
                {
                    UpdateScopePermission(file.Record, permissionId);
                    logger.Info($"Successed to update scope permission: {file.Id}");
                    ProcessDetailForGlobalSearch(file, JobDetailsStatus.Successful);
                }
                HasSuccessNode = true;
                ProcessRecordsForGS(file.Records, permissionId);
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred update box Id : {file.Id}. error : {ex.ToString()}");
                ProcessDetailForGlobalSearch(file, JobDetailsStatus.Failed, "RM_JM_Failed_UpdatePermission");
                HasErrorNode = true;
                throw;
            }

        }

        private void ProcessRecordsForGS(IList<IPhysicalRecord> records, int permissionId)
        {
            ReportManager.IncreaseBase(records.Count);
            foreach (var record in records)
            {
                ProcessRecordForGS(record as PhysicalRecord, permissionId);
            }
        }

        private void ProcessRecordForGS(PhysicalRecord record, int permissionId)
        {
            try
            {
                using (new CheckJobStopScope())
                {
                    logger.Info($"Process record, id: {record.Id}");
                    if (permissionId == record.Record.ScopePermissionId)
                    {
                        logger.Info($"Permission id does not change: {record.Id}");
                        ProcessDetailForGlobalSearch(record, JobDetailsStatus.Successful);
                        return;
                    }
                    UpdateScopePermission(record.Record, permissionId);
                    logger.Info($"Successed to update scope permission: {record.Id}");
                    ProcessDetailForGlobalSearch(record, JobDetailsStatus.Successful);
                    HasSuccessNode = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred update record Id : {record.Id}. error : {ex.ToString()}");
                ProcessDetailForGlobalSearch(record, JobDetailsStatus.Failed, "RM_JM_Failed_UpdatePermission");
                HasErrorNode = true;
            }
        }

        private ScopePermissionDto GenScopePermissionDto(List<Record> nodes)
        {
            ScopePermissionDto permissionDto = null;
            if (nodes.Count > 0)
            {
                var jobContext = _options.GSJobContext;
                permissionDto = new ScopePermissionDto
                {
                    ScopeInfos = GenScopeInfoDto(nodes),
                    AccountIds = jobContext.AccountIds,
                    IsInheritSave = jobContext.IsInheritSave,
                    Permission = jobContext.PermissionType,
                    UserConflictOption = jobContext.UserConflictOption,
                };
            }
            return permissionDto;
        }

        private List<ScopeInfoDto> GenScopeInfoDto(List<Record> nodes)
        {
            var scopeInfos = new List<ScopeInfoDto>();
            foreach (var node in nodes)
            {
                try
                {
                    var location = SetLocationCache(node);
                    var id = node.NodeId;
                    var parentId = "";
                    var scopeIdFullPath = "";
                    var locationId = location.Id.ToString();
                    var locationPath = location.DirPath;
                    bool isNewData = node.Ancestors != null;
                    switch (node.NodeType)
                    {
                        //TODO Derek
                        case (int)RMNodeType.PhyCustom:
                            if (DataNeedProcess(node))
                            {
                                scopeIdFullPath = $"{locationPath}{node.GetScopeIdPath()}/";
                                parentId = node.ParentId == node.LocationId ? locationId : node.ParentId.ToString();
                                SetContainerIdCache(id);
                            }
                            else
                            {
                                continue;
                            }
                            break;
                        case (int)RMNodeType.PhyBox:
                            if (isNewData)
                            {
                                if (DataNeedProcess(node))
                                {
                                    scopeIdFullPath = $"{locationPath}{node.GetScopeIdPath()}/";
                                    parentId = node.ParentId == node.LocationId ? locationId : node.ParentId.ToString();
                                    SetContainerIdCache(id);
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                scopeIdFullPath = $"{locationPath}{id}/";
                                parentId = locationId;
                                SetBoxIdsCache(id);
                            }
                            break;
                        case (int)RMNodeType.PhyFile:
                            if (isNewData)
                            {
                                if (DataNeedProcess(node))
                                {
                                    scopeIdFullPath = $"{locationPath}{node.GetScopeIdPath()}/";
                                    parentId = node.ParentId == node.LocationId ? locationId : node.ParentId.ToString();
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                if (boxIdsCache.Contains(node.BoxId) && !IsBreakInherPermission(id))
                                {
                                    continue;//继承状态的folder,同时所在box也在处理节点当中，则此folder不做处理
                                }
                                else
                                {
                                    if (node.BoxId != Guid.Empty)
                                    {
                                        scopeIdFullPath = $"{locationPath}{node.BoxId}/{id}/";
                                        parentId = node.BoxId.ToString();
                                    }
                                    else
                                    {
                                        scopeIdFullPath = $"{locationPath}{id}/";
                                        parentId = locationId;
                                    }
                                }
                            }
                            break;
                        default:
                            continue;
                    }
                    scopeInfos.Add(new ScopeInfoDto
                    {
                        ScopeId = id.ToString(),
                        ParentScopeId = parentId,
                        ScopeFullPath = scopeIdFullPath
                    });
                }
                catch (Exception ex)
                {
                    logger.Warn($"An error when GenScopeInfoDto, message:{ex.ToString()}");
                }
            }
            return scopeInfos;
        }

        private bool DataNeedProcess(Record record)
        {
            if (containerCache.Contains(record.NodeId))
            {
                return false;
            }

            //数据本身是打破继承的,
            if (IsBreakInherPermission(record.NodeId))
            {
                return true;
            }

            List<Guid> parentIds = new List<Guid>();
            parentIds.AddRange(record.Ancestors);
            if (parentIds != null && parentIds.Count > 0)
            {
                //获取当前节点所有打破继承父节点
                var parentBreakInherPermissionNodeIds = GetParentBreakInherPermissionNodeIds(parentIds.Select(l => l.ToString()).ToList());
                if (parentBreakInherPermissionNodeIds.Count > 0)
                {
                    //找到最近的打破继承父节点,检查job是否已经处理该节点，如果没有，则处理当前节点
                    parentIds.Reverse();
                    Guid firstParent = Guid.Empty;
                    foreach (var parentId in parentIds)
                    {
                        if (parentBreakInherPermissionNodeIds.Contains(parentId))
                        {
                            firstParent = parentId;
                            break;
                        }
                    }
                    if (!containerCache.Contains(firstParent))
                    {
                        return true;
                    }
                }
                else
                {
                    //如果父节点都没有打破继承，则检查是否某个父节点已经被处理.如果没有，则处理当前节点
                    if (!containerCache.Any(a => parentIds.Contains(a)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private RMLocation SetLocationCache(Record node)
        {
            RMLocation location = null;
            var locationGuid = node.LocationId;
            if (!locationDicCache.ContainsKey(locationGuid))
            {
                try
                {
                    location = LocationDao.GetLocationInfo(locationGuid);
                    if (location == null)
                    {
                        throw new Exception("locaiton is not found");
                    }
                    locationDicCache.Add(locationGuid, location);
                }
                catch (Exception ex)
                {
                    logger.Error($"An error when get location, location id:{node?.LocationId},node id:{node?.Id}, message:{ex.ToString()}");
                    throw;
                }
            }
            else
            {
                location = locationDicCache[locationGuid];
            }
            return location;
        }

        private void SetBoxIdsCache(Guid id)
        {
            if (!boxIdsCache.Contains(id))
            {
                boxIdsCache.Add(id);
            }
        }

        private void SetContainerIdCache(Guid id)
        {
            if (!containerCache.Contains(id))
            {
                containerCache.Add(id);
            }
        }


        private bool IsBreakInherPermission(Guid scope)
        {
            var existPermissionId = ScopePermissionDao.GetScopePermissionId(scope.ToString());
            return existPermissionId > 0;
        }

        private List<Guid> GetParentBreakInherPermissionNodeIds(List<string> parentScopeIds)
        {
            return ScopePermissionDao.GetParentBreakInherPermissionNodeIds(parentScopeIds);
        }
        #endregion
    }
}
