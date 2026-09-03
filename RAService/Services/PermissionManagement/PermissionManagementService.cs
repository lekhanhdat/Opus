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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.Service.Services.Explorer;
using AvePoint.RA.Service.Services.Explorer.AuditHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.PermissionManagement
{
    [Audit]
    public class PermissionManagementService : RMServiceBase, IPermissionManagementService
    {
        private RALogger logger = RALogger.GetInstance(typeof(PermissionManagementService));
        private IRMScopePermissionDao RMScopePermissionDao => PlatformWindsorManager.GetService<IRMScopePermissionDao>();

        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
                }
                return _explorerDao;
            }
        }
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IRMLocationDao LocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        private IJobQueueService mJobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        protected IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();

        private IExplorerQueryParamProcesser ExplorerQueryParamProcesser => PlatformWindsorManager.GetService<IExplorerQueryParamProcesser>();

        public IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();

        private IRecordsHistoryService RecordsHistoryService => PlatformWindsorManager.GetService<IRecordsHistoryService>();

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.PhysicalRecordsExplorer, Action = AuditAction.SavelocationPermission, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<RAReturnMessage> SaveLocationPermissionAsync(ScopePermissionDto dto)
        {
            RAReturnMessage returnMessage = new RAReturnMessage();
            try
            {
                logger.Info($"There are [{dto.ScopeInfos.Count}] nodes that need to set permissions");
                var scopeIds = dto.ScopeInfos.Select(o => o.ScopeId).ToList();
                await RecordsHistoryService.AddPhysicalPermissionAudtisAsync(dto);
                var oldScopeBreakStatusDic = RMScopePermissionDao.GetScopeBreakInherMapping(scopeIds);
                //保存权限信息到records db
                var scopeIdWithPermissionDic = await RMScopePermissionDao.SaveLocationPermissionAsync(dto);
                if (scopeIds.Count == 1 && await ExplorerQueryParamProcesser.IsPhysicalEndUserAsync())
                {
                    returnMessage.Extsion1 = HasCurrentScopePermission(GetScopeIdFullPath(scopeIds[0]), await UserService.GetUserAndGroupIdsAsync(TenantLocalValue.LogonUserId));
                }
                if (IsRunJobForSetPermission(dto, oldScopeBreakStatusDic))
                {
                    //run job
                    var jobContextDto = new ScopePermissionJobContextDto
                    {
                        Scopes = dto.ScopeInfos
                    };
                    var runJobResult = RunSetPermissionJob(jobContextDto);
                    if (runJobResult.MessageType == RAMessageType.Successful)
                    {
                        returnMessage.Extension = runJobResult.Extension;
                    }
                    else
                    {
                        returnMessage.MessageType = RAMessageType.Failed;
                    }
                }
                else
                {
                    //不起Job情况下，更新CosmosDB逻辑(location节点除外)
                    if (dto.ScopeInfos.Any(o => (o.NodeType != (int)RMNodeLevel.PhysicalBottomLocation && o.NodeType != (int)RMNodeLevel.PhysicalNormalLocation)))
                    {
                        if (dto.IsInheritSave)
                        {
                            //继承父级PermissionId
                            var firstScopeInfo = dto.ScopeInfos.FirstOrDefault();
                            var parentPerId = RMScopePermissionDao.GetInheritPermissionId(firstScopeInfo?.ScopeFullPath);
                            UpdateToParentPermissionId(dto.ScopeInfos, parentPerId);
                        }
                        else
                        {
                            //打破继承
                            UpdateToSelfPermissionId(dto.ScopeInfos, scopeIdWithPermissionDic);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured when save location permission, message:{ex.ToString()}");
                returnMessage.ErrorMessage = I18NEntity.GetString("RM_PRM_PRE_SavePermission_ErrorMessage");
                returnMessage.MessageType = RAMessageType.Failed;
            }
            return returnMessage;
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.PhysicalRecordsExplorer, Action = AuditAction.SavelocationPermission, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<RAReturnMessage> SavePermissionForNewPhysicalAsync(ScopePermissionDto dto, PhysicalObjectDto obj)
        {
            RAReturnMessage returnMessage = new RAReturnMessage();
            try
            {
                var scopeInfo = dto.ScopeInfos.FirstOrDefault();
                if (!dto.IsInheritSave)
                {
                    //打破继承
                    var scopeIdPermissionDic = await RMScopePermissionDao.SaveLocationPermissionAsync(dto);
                    if (scopeIdPermissionDic.TryGetValue(scopeInfo?.ScopeId, out int permissionId))
                    {
                        obj.ScopePermissionId = permissionId;
                    }
                }
                else
                {
                    //继承父级PermissionId
                    var inherPerId = RMScopePermissionDao.GetInheritPermissionId(scopeInfo?.ScopeFullPath);
                    obj.ScopePermissionId = inherPerId;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured when save permission for new physical, message:{ex.ToString()}");
                returnMessage.ErrorMessage = I18NEntity.GetString("RM_PRM_PRE_SavePermission_ErrorMessage");
                returnMessage.MessageType = RAMessageType.Failed;
            }
            return returnMessage;
        }

        public List<string> GetlocationPathsCanBeViewed(List<int> userAndGroupIds)
        {
            var scopeFullPaths = RMScopePermissionDao.GetLocationPathsWithPermission(userAndGroupIds);
            var nameFullPath = new List<string>();
            foreach (var fullPath in scopeFullPaths)
            {
                var path = GetPermissionLocationNameFullPath(fullPath);
                if (!string.IsNullOrEmpty(path))
                {
                    nameFullPath.Add(path);
                }
            }
            return nameFullPath;
        }

        public ScopePermissionDto ConvertToScopePermissionDto(ScopePermissionSimpleDto simpleDto)
        {
            try
            {
                var uiAccounts = simpleDto.Accounts;
                var accountIds = new List<int>();
                if (uiAccounts != null && uiAccounts.Count > 0)
                {
                    accountIds = uiAccounts.Select(o => o.RMUserId).Distinct().ToList();
                }
                var nodeIds = simpleDto.ScopeIds;

                var dto = new ScopePermissionDto
                {
                    IsInheritSave = simpleDto.IsInherit,
                    AccountIds = accountIds,
                    Permission = simpleDto.Permission
                };
                if (IsSetLocationPermission(nodeIds))
                {
                    dto.ScopeInfos = GetLocationScopeInfos(nodeIds);
                }
                else
                {
                    List<Record> phyNodes = ExplorerDao.QueryAll(o => nodeIds.Contains(o.Id.ToString())).ToList();
                    dto.ScopeInfos = GetScopeInfos(phyNodes);
                }
                return dto;
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured when ConvertToScopePermissionDto, message:{ex.ToString()}");
                return null;
            }
        }

        public ScopePermissionDto ConvertToScopePermissionDto(PhysicalObjectDto obj)
        {
            var uiAccounts = obj.ScopePerDto.Accounts;
            var accountIds = new List<int>();
            if (uiAccounts != null && uiAccounts.Count > 0)
            {
                accountIds = uiAccounts.Select(o => o.RMUserId).Distinct().ToList();
            }
            var dto = new ScopePermissionDto
            {
                IsInheritSave = obj.ScopePerDto.IsInheritSave,
                AccountIds = accountIds,
                Permission = obj.ScopePerDto.Permission
            };

            if (obj.NodeType == RMNodeType.PhyRecord)
            {
                var parentNode = ExplorerDao.GetPhysicalRecordById(obj.FileId);
                if (parentNode != null)
                {
                    var parentMetaInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(parentNode.MetaInfo);
                    parentMetaInfo.TryGetValue(DefaultColumnIDs.Status, out string statusObj);
                    obj.MetaInfo[DefaultColumnIDs.Status] = statusObj;
                }
            }
            var record = ConvertUtil.ConvertPhysicalToRMBaseRecord(obj);
            obj.Id = record.Id;
            dto.ScopeInfos = GetScopeInfos(new List<Record> { record });
            return dto;
        }

        public bool IsSetLocationPermission(List<string> nodeIds)
        {
            var result = false;
            foreach (var nodeId in nodeIds)
            {
                var locaiton = LocationDao.GetLocationByUniqueId(new Guid(nodeId));
                if (locaiton != null)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public List<ScopeInfoDto> GetScopeInfos(List<Record> phyNodes)
        {
            //操作相同location下的physical数据
            var locationUniqueId = Guid.Empty;
            var locationInfo = new RMLocation();
            var scopeInfos = new List<ScopeInfoDto>();
            Dictionary<Guid, Record> cacheBoxsInfo = new Dictionary<Guid, Record>();
            foreach (var node in phyNodes)
            {
                if (locationUniqueId == Guid.Empty)
                {
                    locationUniqueId = node.LocationId;
                    locationInfo = LocationDao.GetLocationInfo(locationUniqueId);//Throw exception
                }
                var nodeId = node.Id;
                var nodeName = node.LeafName;
                var dto = new ScopeInfoDto();
                dto.ScopeId = nodeId.ToString();
                bool isNewData = node.Ancestors != null;
                switch (node.NodeType)
                {
                    //TODO Derek
                    case (int)RMNodeType.PhyCustom:
                        dto.ParentScopeId = node.ParentId == node.LocationId ? locationInfo.Id.ToString() : node.ParentId.ToString();
                        dto.ScopeFullPath = $"{locationInfo.DirPath}{node.GetScopeIdPath()}/";
                        var parentPath = ExplorerService.GetPhysicalObjectFullPath(node.NodeId);
                        dto.ScopeNameFullPath = $"{parentPath}/{nodeName}";
                        dto.NodeType = (int)RMNodeLevel.PhysicalCustom;
                        scopeInfos.Add(dto);
                        break;
                    case (int)RMNodeType.PhyBox:
                        if (isNewData)
                        {
                            dto.ParentScopeId = node.ParentId == node.LocationId ? locationInfo.Id.ToString() : node.ParentId.ToString();
                            dto.ScopeFullPath = $"{locationInfo.DirPath}{node.GetScopeIdPath()}/";
                            var boxParentPath = ExplorerService.GetPhysicalObjectFullPath(node.NodeId);
                            dto.ScopeNameFullPath = $"{boxParentPath}/{nodeName}";
                        }
                        else
                        {
                            dto.ParentScopeId = locationInfo.Id.ToString();
                            dto.ScopeFullPath = $"{locationInfo.DirPath}{nodeId}/";
                            dto.ScopeNameFullPath = $"{locationInfo.PathForDisplay}{nodeName}";
                        }
                        dto.NodeType = (int)RMNodeLevel.PhysicalBox;
                        scopeInfos.Add(dto);
                        break;
                    case (int)RMNodeType.PhyFile:
                        if (isNewData)
                        {
                            dto.ParentScopeId = node.ParentId == node.LocationId ? locationInfo.Id.ToString() : node.ParentId.ToString();
                            dto.ScopeFullPath = $"{locationInfo.DirPath}{node.GetScopeIdPath()}/";
                            var folderParentPath = ExplorerService.GetPhysicalObjectFullPath(node.NodeId);
                            dto.ScopeNameFullPath = $"{folderParentPath}/{nodeName}";
                        }
                        else
                        {
                            var boxId = node.BoxId;
                            if (boxId != Guid.Empty)
                            {
                                var boxName = "";
                                if (!cacheBoxsInfo.ContainsKey(boxId))
                                {
                                    var parentBox = ExplorerDao.QueryAll(o => o.Id == boxId).FirstOrDefault();
                                    cacheBoxsInfo.Add(boxId, parentBox);
                                    boxName = parentBox?.LeafName;
                                }
                                else
                                {
                                    boxName = cacheBoxsInfo[boxId]?.LeafName;
                                }
                                dto.ParentScopeId = boxId.ToString();
                                dto.ScopeFullPath = $"{locationInfo.DirPath}{boxId}/{nodeId}/";
                                dto.ScopeNameFullPath = $"{locationInfo.PathForDisplay}{boxName}/{nodeName}";
                            }
                            else
                            {
                                dto.ParentScopeId = locationInfo.Id.ToString();
                                dto.ScopeFullPath = $"{locationInfo.DirPath}{nodeId}/";
                                dto.ScopeNameFullPath = $"{locationInfo.PathForDisplay}{nodeName}";
                            }
                        }
                        dto.NodeType = (int)RMNodeLevel.PhysicalFile;
                        scopeInfos.Add(dto);
                        break;
                    default:
                        break;
                }
            }
            return scopeInfos;
        }

        //private string GetScopeNameParentPath(List<Guid> ancestorIds)
        //{
        //    if (ancestorIds != null && ancestorIds.Count > 0)
        //    {
        //        var records = ExplorerDao.QueryAll(o => ancestorIds.Contains(o.NodeId)).ToList();
        //        if (records != null && records.Count > 0)
        //        {
        //            return string.Join("/", records.Select(r => r.LeafName).ToList());
        //        }
        //    }

        //    return string.Empty;
        //}

        public List<ScopeInfoDto> GetLocationScopeInfos(List<string> ids)
        {
            var scopeInfos = new List<ScopeInfoDto>();
            var dto = new ScopeInfoDto();
            foreach (var id in ids)
            {
                var location = LocationDao.GetLocationInfo(new Guid(id));
                dto.ScopeId = location.Id.ToString();
                dto.ParentScopeId = location.ParentId.ToString();
                dto.ScopeFullPath = location.DirPath;
                dto.ScopeNameFullPath = $"{location.PathForDisplay}";
                dto.NodeType = location.NodeType;
                scopeInfos.Add(dto);
            }
            return scopeInfos;
        }

        public async Task<List<AOSUserDto>> GetUsersWithPermissionAsync(string scopeId)
        {
            var userIds = RMScopePermissionDao.GetUserIdsWithPermission(scopeId);
            if (userIds != null && userIds.Count > 0)
            {
                return await UserService.GetUsersByIdsAsync(userIds);
            }
            return null;
        }

        public async Task<UsersAndBreakInheritStatus> GetBreakOrInheritPermissionAsync(string scopeId, bool includeSelf)
        {
            var result = new UsersAndBreakInheritStatus();
            var scopePath = GetScopeIdFullPath(scopeId);
            var dic = RMScopePermissionDao.GetUserIdsAndBreakInheritStatus(scopePath, includeSelf); ;
            if (dic.Count > 0)
            {
                var item = dic.FirstOrDefault();
                result.Accounts = await UserService.GetUsersByIdsAsync(item.Key);
                result.BreakInheritStatus = item.Value;
            }
            return result;
        }

        public List<int> GetUserIdsWithPermission(string scopeId)
        {
            return RMScopePermissionDao.GetUserIdsWithPermission(scopeId);
        }
       
        public List<int> GetExcludeScopePermissionIds(string scopeId, List<int> accountAndGroupIds)
        {
            var scopePermissionIds = RMScopePermissionDao.GetExcludeScopes(scopeId, accountAndGroupIds);
            return scopePermissionIds.ToList();
        }

        public List<int> GetIncludeScopePermissionIds(string scopeId, List<int> accountAndGroupIds)
        {
            var scopePermissionIds = RMScopePermissionDao.GetInclueScopes(scopeId, accountAndGroupIds);
            return scopePermissionIds.ToList();
        }

        public List<int> GetExcludeScopePermissionIdsForSearch(string scopePath, List<int> accountAndGroupIds)
        {
            return RMScopePermissionDao.GetExcludeScopePermissions(scopePath, accountAndGroupIds);
        }

        public List<int> GetIncludeScopePermissionIdsForSearch(string scopePath, List<int> accountAndGroupIds)
        {
            return RMScopePermissionDao.GetIncludeScopePermissions(scopePath, accountAndGroupIds);
        }

        public bool HasCurrentScopePermission(string scopePath, List<int> accountAndGroupIds)
        {
            return RMScopePermissionDao.HasScopePermission(scopePath, accountAndGroupIds);
        }

        public Dictionary<string, bool> GetScopeBreakInherMapping(List<string> scopeIds)
        {
            return RMScopePermissionDao.GetScopeBreakInherMapping(scopeIds);
        }

        public int GetScopePermissionId(string scopeId)
        {
            return RMScopePermissionDao.GetScopePermissionId(scopeId);
        }

        public string GetScopeIdFullPath(string scopeId)
        {
            var idFullPath = "";
            try
            {
                RMLocation location = null;
                var locationPath = "";
                int.TryParse(scopeId, out int locationId);
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
                        //    case (int)RMNodeType.PhyBox:
                        //        idFullPath = $"{locationPath}{id}/";
                        //        break;
                        //    case (int)RMNodeType.PhyFile:
                        //        if (node.BoxId != Guid.Empty)
                        //        {
                        //            idFullPath = $"{locationPath}{node.BoxId}/{id}/";
                        //        }
                        //        else
                        //        {
                        //            //location下的folder
                        //            idFullPath = $"{locationPath}{id}/";
                        //        }
                        //        break;
                        //    case (int)RMNodeType.PhyRecord:
                        //        if (node.BoxId != Guid.Empty)
                        //        {
                        //            idFullPath = $"{locationPath}{node.BoxId}/{node.FileId}/{id}/";
                        //        }
                        //        else
                        //        {
                        //            idFullPath = $"{locationPath}{node.FileId}/{id}/";
                        //        }
                        //        break;
                        case (int)RMNodeType.PhyCustom:
                        case (int)RMNodeType.PhyBox:
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

        public string GetScopeIdFullPath(PhysicalObjectDto node)
        {
            var scopeFullPath = "";
            var locationInfo = LocationDao.GetLocationInfo(node.LocationId);
            var locationPath = locationInfo.DirPath;
            var nodeId = node.Id;
            if (node.Ancestors != null && node.Ancestors.Count > 0)
            {
                List<Guid> parentIds = new List<Guid>();
                parentIds.AddRange(node.Ancestors);
                parentIds.RemoveAt(0);
                var parentIdPath = string.Join("/", parentIds);
                scopeFullPath = string.IsNullOrWhiteSpace(parentIdPath) ? $"{locationPath}/{nodeId}/" : $"{locationPath}{parentIdPath}/{nodeId}/";
            }
            else
            {
                switch (node.NodeType)
                {
                    case RMNodeType.PhyBox:
                        scopeFullPath = $"{locationPath}{nodeId}/";
                        break;
                    case RMNodeType.PhyFile:
                        scopeFullPath = node.BoxId != Guid.Empty ? $"{locationPath}{node.BoxId}/{nodeId}/" : $"{locationInfo.DirPath}{nodeId}/";
                        break;
                    case RMNodeType.PhyRecord:
                        scopeFullPath = node.BoxId != Guid.Empty ? $"{locationPath}{node.BoxId}/{node.FileId}/{nodeId}/" : $"{locationPath}{node.FileId}/{nodeId}/";
                        break;
                    default:
                        break;
                }
            }
            return scopeFullPath;
        }

        //[Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.PhysicalRecordsExplorer, Action = AuditAction.RunPhysicalSetPermissionJob, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public RAReturnMessage RunSetPermissionJob(ScopePermissionJobContextDto dto)
        {
            RAReturnMessage returnMessage = new RAReturnMessage();
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.PhysicalSetPermission,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(dto),
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = ex.Message;
            }
            return returnMessage;
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.PhysicalRecordsExplorer, Action = AuditAction.RunPhysicalSetPermissionJob, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public string RealRunSetPermissionJob(JobRunBy JobRunType, string param)
        {
            logger.Info($"Start Run RealRunSetPermission Job");
            string jobId = string.Empty;
            string jobRunByUser = JobRunType == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
            try
            {
                jobId = RMJobService.CreateJob(JobType.PhysicalSetPermission, jobRunByUser);
                SubJobDao.UpdateSubJobCount(jobId, 1);
                SetJobSettings(jobId, JobType.PhysicalSetPermission, param, 1);
            }
            catch (Exception ex)
            {
                logger.Error($"Error in RealRunSetPermission Job, reason : {ex.ToString()}.");
            }
            return jobId;
        }

        private string GetPermissionLocationNameFullPath(string scopePath)
        {
            scopePath = scopePath.TrimEnd('/');
            List<string> ids = scopePath.Split('/').ToList();
            var localtionDirPath = "";
            var phyNodeIds = new List<Guid>();
            foreach (var id in ids)
            {
                bool result = int.TryParse(id, out int locationId);
                if (result)
                {
                    localtionDirPath += $"{locationId}/";
                }
                else
                {
                    phyNodeIds.Add(new Guid(id));
                }
            }
            var nameFullPath = LocationDao.GetLocationPath(localtionDirPath);
            var recordStatus = new int[] { (int)RMRecordStatus.Active, (int)RMRecordStatus.Closed, (int)RMRecordStatus.Missing, (int)RMRecordStatus.Destroyed };
            var phyNodes = ExplorerDao.QueryAll(o => phyNodeIds.Contains(o.Id)).ToList();
            if (phyNodes.Count > 0)
            {
                var checkDeleteObject = phyNodes.Where(o => recordStatus.Contains(o.RecordStatus)).FirstOrDefault();
                if (checkDeleteObject == null)
                {
                    return "";
                }

                var boxNode = phyNodes.Where(o => o.NodeType == (int)RMNodeType.PhyBox).FirstOrDefault();
                if (boxNode != null)
                {
                    nameFullPath = $"{nameFullPath}/{boxNode.LeafName}";
                }
                var folderNode = phyNodes.Where(o => o.NodeType == (int)RMNodeType.PhyFile).FirstOrDefault();
                if (folderNode != null)
                {
                    nameFullPath = $"{nameFullPath}/{folderNode.LeafName}";
                }
            }
            return nameFullPath;
        }

        private void SetJobSettings(string jobId, JobType jobType, string content, int subJobCount)
        {
            var runningJobIds = SubJobDao.GetRunningSetPermissionJobIds();
            var subJobId = $"{jobId}_000";
            var subJob = new RMSubJob()
            {
                Id = subJobId,
                ParentId = jobId,
                StartTime = DateTime.UtcNow.Ticks,
                JobType = (int)jobType,
                Runable = !runningJobIds.IsNullOrEmpty() ? RecordsConstants.SubJob_Runnable_Waiting : RecordsConstants.SubJob_Runnable_CanRun,
                Progress = 0,
                Status = (int)JobStatus.Wait,
                Weight = 100d / subJobCount,
                //String1 = cmdLine
            };
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = content };
            SubJobDao.CreateJob(subJob);
        }

        private bool HasSubNodes(ScopeInfoDto node, List<string> excludeScopeIds)
        {
            ExplorerQueryV2Dto queryDtoV2 = new ExplorerQueryV2Dto()
            {
                PagingInfo = new ExplorerPagingInfo()
                {
                    PageSize = 1
                },
                QueryOption = new ExplorerQueryOptionV2()
                {
                    FilterOption = new ExplorerFilterOptionV2()
                    {
                        ExceptIds = excludeScopeIds != null ? excludeScopeIds.Select(i => new Guid(i)).ToList() : null,
                        SourceFlags = new List<SourceFlag>() { SourceFlag.Physical },
                    }
                }
            };

            Guid nodeId = Guid.Empty;
            if (node.NodeType == (int)RMNodeLevel.PhysicalBottomLocation)
            {
                var locationNode = LocationDao.GetLocationById(int.Parse(node.ScopeId));
                nodeId = locationNode.UniqueId;
            }
            else
            {
                nodeId = new Guid(node.ScopeId);
            }
            PhysicalExplorerQueryDtoExtension.GenerateShallowQueryExpression((RMNodeLevel)node.NodeType, nodeId, queryDtoV2.QueryOption.FilterOption);
            var builder = DB.Explorer.Dao.CosmosImp.Builder.SqlQuerySpecBuilderFactory.CreatePhysicalExplorerBuilder();
            var queryData = ExplorerDao.SearchRecordsV2(queryDtoV2, builder);
            return queryData != null && queryData.Item1 != null && queryData.Item1.Count() > 0 ? true : false;
        }

        /// <summary>
        /// 存在没有打破继承的直接子节点
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        private bool IsExistNotBreakSubNode(ScopePermissionDto dto)
        {
            var hasSubNodes = false;
            var nodes = dto.ScopeInfos;
            var scopeIds = nodes.Select(o => o.ScopeId).ToList();
            foreach (var node in nodes)
            {
                if (node.NodeType == (int)RMNodeType.PhysicalNormalLocation)
                {
                    //normalLocation要取bottom location，如果有一个bottom location存在没有打破继承的子节点就起job
                    int locationId;
                    int.TryParse(node.ScopeId, out locationId);
                    if (locationId > 0)
                    {
                        List<IPhysicalLocation> allSubBottomLocation = new List<IPhysicalLocation>();
                        List<IPhysicalLocation> notBreakBottomLocation = new List<IPhysicalLocation>();
                        var locationInfo = LocationDao.GetLocationById(locationId);
                        IPhysicalLocation normalLocation = new PhysicalLocation(locationInfo.UniqueId);
                        GetSubBottomLocation(normalLocation, allSubBottomLocation);
                        notBreakBottomLocation = GetNotBreakBottomLocation(allSubBottomLocation, node.ScopeFullPath);
                        foreach (IPhysicalLocation bottomLocation in notBreakBottomLocation)
                        {
                            ScopeInfoDto tempScope = new ScopeInfoDto();
                            tempScope.ScopeId = bottomLocation.IntId.ToString();
                            tempScope.NodeType = (int)RMNodeLevel.PhysicalBottomLocation;
                            var excludeScopeIds = RMScopePermissionDao.GetBreakSubScopeIds(tempScope.ScopeId);
                            //if (ExplorerDao.Exist(GetSubNodesLambda(tempScope, excludeScopeIds)))
                            if(HasSubNodes(tempScope, excludeScopeIds))
                            {
                                hasSubNodes = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    var excludeScopeIds = RMScopePermissionDao.GetBreakSubScopeIds(node.ScopeId);
                    // if (ExplorerDao.Exist(GetSubNodesLambda(node, excludeScopeIds)))
                    if (HasSubNodes(node, excludeScopeIds))
                    {
                        hasSubNodes = true;
                        break;
                    }
                }
            }
            return hasSubNodes;
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
                bool isBreakInheit= RMScopePermissionDao.IsBottomLocationBreakInherForNormalNode(scopePath, normalFullPath);
                if (!isBreakInheit)
                {
                    notBreakInherit.Add(bottomLocation);
                }
            }
            return notBreakInherit;
        }

        private void UpdateToSelfPermissionId(List<ScopeInfoDto> scopeInfos, Dictionary<string, int> scopePermissionIdDic)
        {
            if (scopeInfos != null && scopeInfos.Count > 0)
            {
                scopeInfos.ForEach(item =>
                {
                    if (scopePermissionIdDic.TryGetValue(item.ScopeId, out int permissionId))
                    {
                        ExplorerDao.UpdateAll(o => o.Id == new Guid(item.ScopeId), r => { r.ScopePermissionId = permissionId; });
                    }
                });
            }
        }
        private void UpdateToParentPermissionId(List<ScopeInfoDto> scopeInfos, int permissionId)
        {
            var scopeIds = scopeInfos.Select(o => new Guid(o.ScopeId)).ToList();
            ExplorerDao.UpdateAll(o => scopeIds.Contains(o.Id), r => { r.ScopePermissionId = permissionId; });
        }

        /// <summary>
        /// //当前节点权限从无到有，或者从有到无，并且存在没有单独设置权限的子节点，需要Run Job
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        private bool IsRunJobForSetPermission(ScopePermissionDto dto, Dictionary<string, bool> oldScopeBreakPermissionDic)
        {
            var isRun = false;
            var changeStatus = false;
            if (!dto.IsInheritSave)
            {
                //打破继承加权限
                if (oldScopeBreakPermissionDic.Values.Any(o => o == false))
                {
                    //之前权限状态是无，即将变成有权限
                    changeStatus = true;
                }
            }
            else
            {
                //恢复继承
                if (oldScopeBreakPermissionDic.Values.Any(o => o == true))
                {
                    //之前权限状态是有，即将变成无状态
                    changeStatus = true;
                }
            }
            var scopeIds = dto.ScopeInfos.Select(o => o.ScopeId).ToList();
            var existsFailedJobInfo = RMScopePermissionDao.ExistsFailedJobForScopes(scopeIds);
            if (IsExistNotBreakSubNode(dto) && (changeStatus || existsFailedJobInfo))
            {
                isRun = true;
            }
            else
            {
                isRun = false;
            }
            return isRun;
        }

        public List<int> GetScopePermissionIds(List<int> accountAndGroupIds)
        {
            return RMScopePermissionDao.GetPermissionScopes(accountAndGroupIds).ToList();
        }

        public async Task<RAReturnMessage> SyncADUsersAsync(List<AOSUserDto> users)
        {
            var returnMessage = new RAReturnMessage();
            try
            {
                if (users != null && users.Count > 0)
                {
                    await UserService.SyncUsersAsync(TenantLocalValue.LogonGroupId, users, groupType: DefaultAddedSecurityGroupType.BuiltInEndUserGroup);
                }
            }
            catch (Exception ex)
            {
                returnMessage.ErrorMessage = I18NEntity.GetString("RM_RegisterUser_Error_Message");
                returnMessage.MessageType = RAMessageType.Failed;
            }
            return returnMessage;
        }

        public void DeletePermissionInfo(string scopeId)
        {
            try
            {
                RMScopePermissionDao.DeleteScopePermission(scopeId);
            }
            catch (Exception ex)
            {
                logger.Warn($"An error when DeletePermissionInfo, scopeid:[{scopeId}], message:[{ex.ToString()}]");
            }
        }
        public int GetScopePermissionId(string scopeIdPath, bool includeSelf)
        {
            return RMScopePermissionDao.GetInheritPermissionId(scopeIdPath, includeSelf);
        }
    }
}
