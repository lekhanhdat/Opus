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
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Cache.Services;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.CustomizeConnector.Enums;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object.Extension;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Lite;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RAPhysical.Disposal.PhysicalDisposalActionImps;
using AvePoint.RA.SharePoint.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Explorer
{
    public class ExplorerQueryService : RMServiceBase, IExplorerQueryService
    {
        private RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        #region interface

        private static IRMCustomizeConnectorContentSourceDao CustomizeConnectorContentSourceDao => PlatformWindsorManager.GetService<IRMCustomizeConnectorContentSourceDao>();

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

        private IRMScopeDao RMScopeDao => PlatformWindsorManager.GetService<IRMScopeDao>();
        private IRMRuleDao RMRuleDao => PlatformWindsorManager.GetService<IRMRuleDao>();
        private IRecordLoanAllianceDao RecordLoanAllianceDao => PlatformWindsorManager.GetService<IRecordLoanAllianceDao>();
        private IGeneralSettingService  GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private IExplorerQueryParamProcesser ExplorerQueryParamProcesser => PlatformWindsorManager.GetService<IExplorerQueryParamProcesser>();
        //private IRMSecurityGroupDao RMSecurityGroupDao => PlatformWindsorManager.GetService<IRMSecurityGroupDao>();

        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();

        private ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();

        private IHoldDao HoldDao => PlatformWindsorManager.GetService<IHoldDao>();

        private IOfflineSearchDao OfflineSearchDao => PlatformWindsorManager.GetService<IOfflineSearchDao>();

        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private IRMFunctionSettingDao FunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        #endregion

        #region Advanced Search
        public Task<ExplorerResultInfo> QueryDataListWithoutTotalAsync(ExplorerQueryV3Dto dto)
        {
            return QueryDataListAsync(dto, false);
        }
        public Task<ExplorerResultInfo> QueryDataListWithTotalAsync(ExplorerQueryV3Dto dto)
        {
            return QueryDataListAsync(dto, true);
        }
        public Task<ExplorerResultInfo> QueryAdvancedDataListWithTotalAsync(ExplorerQueryV3Dto dto, bool suggestSearch = false)
        {
            return QueryDataListAsync(dto, true, false, suggestSearch);
        }

        public async Task<ExplorerResultInfo> QueryDataListWithoutTotalCustomAsync(
            ExplorerQueryV3Dto dto, ExplorerFilterOptionV2 builtinFilterOption = null, bool returnTotalCount = false,
            bool convertMetaInfo = false)
        {
            var rst = new ExplorerResultInfo
            {
                PagingInfo = dto.PagingInfo
            };
            try
            {
                await ExplorerQueryParamProcesser.ProcessV3Async(dto.QueryOption);
                var recT = ExplorerDao.SearchRecordsV3(dto, builtinFilterOption);

                await ProcessResultAsync(rst, recT, convertMetaInfo);

                if (returnTotalCount)
                {
                    rst.PagingInfo.Total = ExplorerDao.QueryCountV3(dto, builtinFilterOption);
                }
            }
            catch (ExplorerQueryNoPermissionException e)
            {
                return GetNoPermissionResult(dto.PagingInfo, e);
            }
            return rst;
        }

        public async Task<ExplorerResultInfo> QueryOfflineSearchDataAsync(ExplorerOfflineResultQueryDto dto)
        {
            ExplorerResultInfo resultInfo = new ExplorerResultInfo();
            Tuple<List<Record>,int> datas = OfflineSearchDao.Query(dto);
            resultInfo.Datas = await Convert2BaseDtoAsync(datas.Item1);
            resultInfo.PagingInfo = new ExplorerPagingInfo() { Total = datas.Item2, PageIndex = dto.PagingInfo.PageIndex.ToString(), PageSize = dto.PagingInfo.PageSize };
            return resultInfo;
        }

        /// <summary>
        /// for offline search
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<ExplorerFilterOptionV2> PrepareFilterV2Async(ExplorerQueryV3Dto dto)
        {
            var filterOptionV2 = await AttachBuiltinFilterAsync(dto); 
            await ExplorerQueryParamProcesser.ProcessV3Async(dto.QueryOption);
            return filterOptionV2;
        }

        private async Task<ExplorerResultInfo> QueryDataListAsync(ExplorerQueryV3Dto dto, bool returnTotalCount = false, bool convertMetaInfo = false, bool isSuggestionSearch = false)
        {
            var rst = new ExplorerResultInfo
            {
                PagingInfo = dto.PagingInfo
            };
            try
            {

                //需要处理一些built in的权限控制逻辑（比如SourceFlag，Permission/ContainerId等），然后把这些条件这个AND到查询
                var filterOptionV2 = await AttachBuiltinFilterAsync(dto);
                if (isSuggestionSearch)
                {
                    filterOptionV2.NodeTypes = [RMNodeLevel.Folder, RMNodeLevel.Library, RMNodeLevel.List, RMNodeLevel.SiteCollection];
                    filterOptionV2.SourceFlags = [SourceFlag.SharePoint, SourceFlag.OneDrive];
                    filterOptionV2.Status = [RMRecordStatus.Active];
                }

                await ExplorerQueryParamProcesser.ProcessV3Async(dto.QueryOption);
                var recT = ExplorerDao.SearchRecordsV3(dto, filterOptionV2, suggestSearch : isSuggestionSearch);

                await ProcessResultAsync(rst, recT, convertMetaInfo);

                if (returnTotalCount)
                {
                    rst.PagingInfo.Total = ExplorerDao.QueryCountV3(dto, filterOptionV2, suggestSearch: isSuggestionSearch);
                }
            }
            catch (ExplorerQueryNoPermissionException e)
            {
                return GetNoPermissionResult(dto.PagingInfo, e);
            }
            return rst;
        }

        /// <summary>
        /// 对于一些built-in的filter，比如term/permission/containerId等，使用V2的处理方式
        /// </summary>
        /// <param name="dto"></param>
        private async Task<ExplorerFilterOptionV2> AttachBuiltinFilterAsync(ExplorerQueryV3Dto dto)
        {
            using (var performance = new PerformanceScope("ExportQueryService.AttachBuiltinFilterAsync"))
            {
                //需要处理一些built in的权限控制逻辑（比如SourceFlag，Permission/ContainerId等），然后把这些条件这个AND到查询

                ExplorerQueryV2Dto dtoV2 = new ExplorerQueryV2Dto()
                {
                    QueryOption = new ExplorerQueryOptionV2()
                    {
                        FilterOption = new ExplorerFilterOptionV2
                        {
                            //SourceFlags = SourceFlagHelper.GetAllSourceFlags(),
                            SourceFlags = await SecurityTrimmingHelper.GetAllAvailableSourceFlagsFromDbAsync(),
                            QueryArchivedData = QueryArchivedData(dto),
                            FSFolderLevelEnabled = GetClassificationLevel() == (int)NodeLevel.FSFolder,
                        }
                    }
                };

                if (dtoV2.QueryOption.FilterOption.QueryArchivedData == true)
                {
                    foreach (var value in dto.QueryOption.Values)
                    {
                        if (value.Column.Id == Contract.TemplateManagement.QueryCloumnIds.SourceFlag)
                        {
                            int[] arr = SerializerHelper.DeserializeByJsonSerializer<int[]>(value.Value);
                            arr = arr.Where(a => a != 4).ToArray();
                            value.Value = SerializerHelper.SerializeByJsonSerializer(arr);
                        }
                    }
                }

                await PreProcessAsync(dtoV2);

                return dtoV2.QueryOption.FilterOption;
            }
        }

        public int GetClassificationLevel()
        {
            RMFunctionSetting setting;
            FunctionSettingDao.TryGet(AvePoint.RA.Contract.FunctionSetting.FunctionSettingType.ClassificationLevelSetting, out setting);
            NodeLevel result;
            if (setting == null)
            {
                return (int)NodeLevel.FSFile;
            }
            if (Enum.TryParse<NodeLevel>(setting.SettingInfo, out result))
            {
                if (RMKeyValueDao.TryGetBoolValue(KeyNameCollection.EnableJPMCFileSystemFeature, out var enabled) && enabled && result == NodeLevel.FSFolder)
                {
                    return (int)NodeLevel.FSFile;
                }
                return (int)result;
            }
            return (int)NodeLevel.FSFolder;
        }


        private bool? QueryArchivedData(ExplorerQueryV3Dto dto)
        {
            bool onlyQueryArchived = false;
            var column = dto.QueryOption.Values.Where(c => c.Column.Id == Contract.TemplateManagement.QueryCloumnIds.ContentArchived).FirstOrDefault();
            if (column == null)
            {
                return null;
            }
            else
            {
                if (dto.QueryOption.Values.Count == 1)
                {
                    if (dto.QueryOption.Values[0].Value.ToLower().Equals("true"))
                    {
                        onlyQueryArchived = true;
                        //dto.QueryOption.Values.RemoveAt(0);
                        dto.QueryOption.Values.Add(new ExplorerSearchOptionV3()
                        {
                            Column = new ExplorerQueryColumn()
                            {
                                Id = RecordBuildInColumnIds.SourceFlag
                            },
                            Value = "[1,6,11]",
                            ColumnsLogic = ExplorerSearchKeyOperationLogic.AND
                        });
                    }
                }
                else
                {
                    List<int> index = new List<int>();
                    for (int i = 0; i < dto.QueryOption.Values.Count; i++)
                    {
                        if (dto.QueryOption.Values[i].Column.Id == Contract.TemplateManagement.QueryCloumnIds.ContentArchived)
                        {
                            if (i != dto.QueryOption.Values.Count - 1 && dto.QueryOption.Values[i].ColumnsLogic == ExplorerSearchKeyOperationLogic.AND)
                            {
                                index.Add(i);
                                if (dto.QueryOption.Values[i].Value.ToLower().Equals("true"))
                                {
                                    onlyQueryArchived = true;
                                }
                            }
                            if (i > 0 && dto.QueryOption.Values[i - 1].ColumnsLogic == ExplorerSearchKeyOperationLogic.AND)
                            {
                                index.Add(i);
                                if (dto.QueryOption.Values[i].Value.ToLower().Equals("true"))
                                {
                                    onlyQueryArchived = true;
                                }
                            }
                        }
                    }

                    if (index.Count > 0)
                    {
                        //for (int i = dto.QueryOption.Values.Count - 1; i >= 0; i--)
                        //{
                        //    if (index.Contains(i))
                        //    {
                        //        dto.QueryOption.Values.RemoveAt(i);
                        //    }
                        //}
                        if (dto.QueryOption.Values.Count == 0 && onlyQueryArchived)
                        {
                            dto.QueryOption.Values.Add(new ExplorerSearchOptionV3()
                            {
                                Column = new ExplorerQueryColumn()
                                {
                                    Id = RecordBuildInColumnIds.SourceFlag
                                },
                                Value = "[1,6,11]",
                                ColumnsLogic = ExplorerSearchKeyOperationLogic.AND
                            });
                        }
                    }
                }
            }
            return onlyQueryArchived;
        }

        
        #endregion
        public Task<ExplorerResultInfo> QueryDataListWithoutTotalAsync(ExplorerQueryV2Dto dto)
        {
            return QueryDataListAsync(dto);
        }

        public Task<ExplorerResultInfo> QueryDataListWithTotalAsync(ExplorerQueryV2Dto dto, bool convertMetaInfo = false)
        {
            return QueryDataListAsync(dto, true, convertMetaInfo);
        }

        /// <summary>
        /// will get data without checking permission
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="convertMetaInfo"></param>
        /// <returns></returns>
        public Task<ExplorerResultInfo> QueryDataListWithoutTotalDirectlyAsync(ExplorerQueryV2Dto dto, bool convertMetaInfo = false)
        {
            return QueryDataListAsync(dto, false, convertMetaInfo, false);
        }

        private async Task<ExplorerResultInfo> QueryDataListAsync(ExplorerQueryV2Dto dto, bool returnTotalCount = false, bool convertMetaInfo = false, bool needPreProcess = true)
        {
            var rst = new ExplorerResultInfo
            {
                PagingInfo = dto.PagingInfo
            };
            try
            {
                LogQueryOption(dto.QueryOption);
                if (needPreProcess)
                {
                    await PreProcessAsync(dto);
                }
                
                var recT = ExplorerDao.SearchRecordsV2(dto);

                await ProcessResultAsync(rst, recT, convertMetaInfo);

                //if (convertMetaInfo)
                //{
                //    foreach (Record rec in recT.Item1)
                //    {
                //        rec.AppendMetaInfoForOldLogic();
                //    }
                //}

                //rst.Datas = Convert2BaseDto(recT.Item1.ToList());
                //rst.PagingInfo.HasNextPage = !string.IsNullOrEmpty(recT.Item2);
                //rst.PagingInfo.PageIndex = recT.Item2;

                if (returnTotalCount)
                {
                    rst.PagingInfo.Total = ExplorerDao.QueryCount(dto);
                }
            }
            catch (ExplorerQueryNoPermissionException e)
            {
                return GetNoPermissionResult(dto.PagingInfo, e);
            }
            catch (Exception ex)
            {
                rst.Datas = new List<BaseRecordDto>();
                logger.Error("error occurred while query data for explorer ,ERROR:{0}", ex.ToString());
            }

            return rst;
        }

        private async System.Threading.Tasks.Task ProcessResultAsync(ExplorerResultInfo rst, Tuple<IEnumerable<Record>, string> recT, bool convertMetaInfo)
        {
            using (var performance = new PerformanceScope("ExplorerDAOV2.ProcessResultAsync"))
            {
                if (convertMetaInfo)
                {
                    foreach (Record rec in recT.Item1)
                    {
                        rec.AppendMetaInfoForOldLogic();
                    }
                }

                rst.Datas = await Convert2BaseDtoAsync(recT.Item1.ToList());
                rst.PagingInfo.HasNextPage = !string.IsNullOrEmpty(recT.Item2);
                rst.PagingInfo.PageIndex = recT.Item2;
            }
        }

        private System.Threading.Tasks.Task PreProcessAsync(ExplorerQueryV2Dto dto)
        {
            //LogQueryOption(dto.QueryOption);
            //remove term permission filter
            //AssembleTermPermissionCondition(dto.QueryOption);
            return ExplorerQueryParamProcesser.ProcessAsync(dto.QueryOption);
            //ProcessWithoutNodeTypeParam(dto.QueryOption.FilterOption);
        }


        private void LogQueryOption(ExplorerQueryOptionV2 option)
        {
            if(option.SearchOption != null)
            {
                logger.Info("Search key {0}", option.SearchOption.Key);
                if(!option.SearchOption.Columns.IsNullOrEmpty())
                {
                    StringBuilder builder = new StringBuilder();
                    foreach(ExplorerQueryColumn column in option.SearchOption.Columns)
                    {
                        builder.Append(string.Format("Name:{0}, Id:{1}, IdsWithDuplicateName:{2}", column.Name, column.Id, column.IdsWithDuplicateName.IsNullOrEmpty() ? "" : string.Join(",", column.IdsWithDuplicateName)));
                        builder.Append("\n");
                    }
                    logger.Debug(builder.ToString());
                }
            }
        }
        //[Obsolete("remove term permission filter")]
        //public void ProcessWithoutNodeTypeParam(ExplorerFilterOptionV2 filterOption)
        //{
        //    if (filterOption.SourceFlags != null && filterOption.SourceFlags.Contains(SourceFlag.Physical))
        //    {
        //        var termPermissionDto = await GetSecurityTermDtoAsync();
        //        if (termPermissionDto.TermPermissionType != TermPermissionMethod.All)
        //        {
        //            if (filterOption.WithoutNodeTypes == null)
        //            {
        //                filterOption.WithoutNodeTypes = new List<RMNodeLevel>() { RMNodeLevel.PhysicalRecord };
        //            }
        //            else if (!filterOption.WithoutNodeTypes.Contains(RMNodeLevel.PhysicalRecord))
        //            {
        //                filterOption.WithoutNodeTypes.Add(RMNodeLevel.PhysicalRecord);
        //            }
        //        }
        //    }
        //}

        public Task<SecurityTermPermissionDto> GetSecurityTermDtoAsync()
        {
            return SecurityTrimmingHelper.GetSecurityTermDtoAsync();
        }

        public List<Guid> GetSecurityTerms(SecurityTermPermissionDto termPremDto)
        {
            var result = new List<Guid>();
            switch (termPremDto.TermPermissionType)
            {
                case TermPermissionMethod.All:
                    break;
                case TermPermissionMethod.SpecifyScope:
                    result = termPremDto.TermObjIds;
                    result.Add(Guid.Empty);
                    break;
                case TermPermissionMethod.None:
                    result.Add(Guid.Empty);
                    break;
                default:
                    break;
            }
            return result;
        }

        //private void AssembleTermPermissionCondition(ExplorerQueryOptionV2 queryOption)
        //{
        //    try
        //    {
        //        var termPermDto = await GetSecurityTermDtoAsync();
        //        if (termPermDto.TermPermissionType != Contract.RMWeb.CP.TermPermissionMethod.All)
        //        {
        //            var securityTerms = GetSecurityTerms(termPermDto);
        //            var selectedTermIds = queryOption.FilterOption.TermIds;
        //            if (selectedTermIds == null || selectedTermIds.Count == 0)
        //            {
        //                var showUnclassifiedContent = queryOption.FilterOption.WithOutTerms;
        //                if (showUnclassifiedContent.HasValue && showUnclassifiedContent.Value)
        //                {
        //                    securityTerms.Remove(Guid.Empty);
        //                }
        //                queryOption.FilterOption.TermIds = securityTerms;
        //            }
        //            else
        //            {
        //                queryOption.FilterOption.TermIds = securityTerms.Intersect(selectedTermIds).ToList();
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error($"Global search get permission term error: {e.ToString()}");
        //    }
        //}

        private ExplorerResultInfo GetNoPermissionResult(ExplorerPagingInfo pagingInfo, ExplorerQueryNoPermissionException ex)
        {
            logger.Warn("No permission to access data ,ERROR:{0}", ex.ToString());

            return new ExplorerResultInfo()
            {
                Datas = new List<BaseRecordDto>(),
                PagingInfo = pagingInfo
            };
        }

        public async Task<List<BaseRecordDto>> Convert2BaseDtoAsync(List<Record> queryList, string extensionValue = null)
        {
            var scopeIds = queryList.Select(q => q.ScopeId).Distinct().ToList();
            var pathDic = RMScopeDao.GetScopeInfoByIds(scopeIds);
            List<RMRule> allRules = RMRuleDao.GetAvailableRules();
            Dictionary<Guid, string> termIdNameMapping = TermDao.GetTermUniqueIdAndNameMapping();
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            var accountMap = AccountDao.FindAll().ToDictionary(k => k.Id, v => v);
            List<string> invalidSiteIds = new List<string>();
            var contentSourceInfoes = (await CustomizeConnectorContentSourceDao.GetAllSimpleInfoes(CustomizeConnectorOrigin.BuildIn, CustomizeConnectorOrigin.ExternalCustomize))
            
            .ToDictionary(item => item.Flag, item => I18NEntity.GetString(item.Name));
            var resultList = queryList.ConvertAll(e =>
           {
               var record = ConvertUtil.ConvertToBaseRecordDto(e, accountMap);
               record.SourceName = contentSourceInfoes.ContainsKey(e.SourceFlag) ? contentSourceInfoes[e.SourceFlag] : I18NEntity.GetString("RM_CP_Connector");
               MakeSPObjectFullPath(pathDic, record, invalidSiteIds);
               SetRuleInfos(record, allRules);
               SetTermInfo(record, termIdNameMapping);
               SetPredictInfo(record, termIdNameMapping);
               SetObjectType(record);
               SetCustomColumnDateTimeValue(record, gls);
               SetSPObjectDisposalDueDate(DateTime.UtcNow.Ticks, gls, record);
               if (record.SourceFlag == (int)SourceFlag.Physical)
               {
                   var physicalObjectDto = ConvertUtil.ConvertRMBaseRecordToPhysical(e);
                   SetPhysicalObjectDisposalDueDateByCalculate(gls, record, physicalObjectDto);
               }
               if(!string.IsNullOrWhiteSpace(extensionValue))
               {
                   record.ExtensionValue = extensionValue;
               }
               return record;
           });

            InheritRuleInfoFromParent(resultList, allRules);
            AppendTermInfoForRecordLevel(resultList);
            await AppendHoldReleaseTimeInfoAsync(resultList);
            await AppendPhyPersonalHoldInfoAsync(resultList);
            await AppendTimeInfoAsync(resultList, gls);
            return resultList;
        }

        private async Task AppendTimeInfoAsync(List<BaseRecordDto> baseRecords, GeneralSettingModel gls)
        {
            if (baseRecords.IsNullOrEmpty())
            {
                return;
            }

            foreach (var result in baseRecords)
            {
                result.TimeArchivedStr = result.TimeArchived > 0 ? GeneralSettingService.ConvertTiksToDateTime(gls, result.TimeArchived, true).SimplifyFormatTime : "";
                result.TimeCreatedStr = result.TimeCreated > 0 ? GeneralSettingService.ConvertTiksToDateTime(gls, result.TimeCreated, true).SimplifyFormatTime : "";
                result.TimeLastModifiedStr = result.TimeLastModified > 0 ? GeneralSettingService.ConvertTiksToDateTime(gls, result.TimeLastModified, true).SimplifyFormatTime : "";
            }
        }

        private void SetPhysicalObjectDisposalDueDateByCalculate(GeneralSettingModel gls, BaseRecordDto record, PhysicalObjectDto physicalObjectDto)
        {
            using (new RA.Common.PerformanceScope("PhysicalRecord.RA.DB.Core.SetPhysicalObjectDisposalDueDateByCalculate"))
            {
                this.CalculateDisposalDueDateNormal(physicalObjectDto, gls, 0);
                if (physicalObjectDto.NodeType == RMNodeType.PhyFile && physicalObjectDto.BoxId != Guid.Empty
                    && physicalObjectDto.DisposalHold == true && physicalObjectDto.HoldStatus == HoldStatus.Inherit)
                {
                    Record box = ExplorerDao.GetPhysicalRecordById(physicalObjectDto.BoxId);
                    this.CalculateDisposalDueDateNormal(physicalObjectDto, gls, box.DisposalDueDate);
                }

                if (physicalObjectDto.NodeType == RMNodeType.PhyRecord)
                {
                    List<Guid> parentIds = new List<Guid>() { physicalObjectDto.FileId, physicalObjectDto.BoxId };
                    List<Record> parentRecs = ExplorerDao.QueryAll(a => parentIds.Contains(a.Id) && a.ScopeId == Guid.Empty).OrderBy(a => a.NodeType).ToList();
                    Record file = parentRecs.FirstOrDefault(a => a.NodeType == (int)RMNodeType.PhyFile);
                    if (file != null && file.RuleId != Guid.Empty)
                    {
                        physicalObjectDto.RuleId = file.RuleId;
                        if (file.DisposalDueDate > DateTime.MinValue.Ticks)
                        {
                            physicalObjectDto.DisposalDueDate = this.GetDisposalDueDateStr(file.DisposalDueDate, (RMRecordStatus)file.RecordStatus, gls, false);
                        }
                        this.AppendPhysicalRuleAction(physicalObjectDto, RMRuleDao.GetRuleById(file.RuleId));
                    }
                    else
                    {
                        this.AppendPhysicalRuleAction(physicalObjectDto, RMRuleDao.GetRuleById(physicalObjectDto.RuleId));
                    }
                    this.CalculateDisposalDueDateNormal(physicalObjectDto, gls, 0);
                }

                record.DisposalDueDate = physicalObjectDto.DisposalDueDate;
            }
        }

        private void AppendPhysicalRuleAction(PhysicalObjectDto record, RMRule tempRule)
        {
            if (tempRule != null)
            {
                record.RuleName = tempRule?.RuleName;
                record.RuleAction = record.SourceFlag == 4 ? (int)tempRule.PhysicalDisposalAction : (int)tempRule.DisposalAction;
            }
            else
            {
                record.RuleName = string.Empty;
                record.RuleAction = (int)RMContentDisposalAction.None;
            }
        }

        private void CalculateDisposalDueDateNormal(PhysicalObjectDto record, GeneralSettingModel gls, long parentDueDate)
        {
            long tempTicks;
            if (record.DisposalDueDate == "RDM_RecordsExporer_Status_NextJob" && parentDueDate > DateTime.UtcNow.Ticks)
            {
                record.DisposalDueDate = GeneralSettingService.ConvertTiksToDateTime(gls, parentDueDate, true).SimplifyFormatTime;
            }
            else if (long.TryParse(record.DisposalDueDate, out tempTicks))
            {
                tempTicks = tempTicks > parentDueDate ? tempTicks : parentDueDate;
                var minDate = DateTime.MinValue;
                if (tempTicks > minDate.Ticks)
                {
                    record.DisposalDueDate = this.GetDisposalDueDateStr(tempTicks, (RMRecordStatus)record.Status, gls);
                }
            }
            else
            {
                record.DisposalDueDate = I18NEntity.GetString(record.DisposalDueDate);
            }
        }
        private string GetDisposalDueDateStr(long dueDateLong, RMRecordStatus recordStatus, GeneralSettingModel gls, bool isForGUI = true)
        {
            return GeneralSettingService.ConvertTiksToDateTime(gls, dueDateLong, true).SimplifyFormatTime;
            ////RECO-4643 Destroyed状态的数据，due date 会显示destroyed 的时间，所以不会遵循： 与当前时间判断显示NextJob 的逻辑
            //if (dueDateLong > DateTime.UtcNow.Ticks || recordStatus == RMRecordStatus.Destroyed)
            //{
            //    return GeneralSettingService.ConvertTiksToDateTime(gls, dueDateLong, true).SimplifyFormatTime;
            //}
            //else
            //{
            //    if (isForGUI)
            //    {

            //        return I18NEntity.GetString("RDM_RecordsExporer_Status_NextJob");
            //    }
            //    else
            //    {
            //        return "RDM_RecordsExporer_Status_NextJob";
            //    }
            //}
        }

        private void MakeSPObjectFullPath(Dictionary<Guid, RMScope> pathDic, BaseRecordDto record, List<string> invalidSiteIds)
        {
            try
            {
                if (pathDic.ContainsKey(record.ScopeId))
                {
                    // get full path
                    var sPath = pathDic.ContainsKey(record.ScopeId) ? pathDic[record.ScopeId]?.FullPath : record.DirPath;
                    record.FullPath = string.IsNullOrWhiteSpace(sPath) ? string.IsNullOrWhiteSpace(record.DirPath) ? string.Empty : record.DirPath : WebUtil.MakeFullUrl(sPath, record.DirPath);
                }
                else
                {

                    if ((record.SourceFlag == (int)SourceFlag.SharePoint || record.SourceFlag == (int)SourceFlag.OneDrive)
                        && !string.IsNullOrWhiteSpace(record.AveSiteId) && !invalidSiteIds.Contains(record.AveSiteId))
                    {
                        var site = RMRemoteNodeDao.GetRemoteSiteCollectionById(record.AveSiteId.ToString());
                        record.FullPath = site == null ? string.IsNullOrWhiteSpace(record.DirPath) ? string.Empty : record.DirPath : WebUtil.MakeFullUrl(site.url, record.DirPath);
                        if (site != null)
                        {
                            var siteScope = new RMScope()
                            {
                                FullPath = site.url,
                                ScopeId = record.ScopeId,
                                ScopeName = site.Name,
                                IsRemoved = false,
                            };
                            if (!pathDic.ContainsKey(record.ScopeId) && !string.IsNullOrWhiteSpace(record.FullPath))
                            {
                                pathDic.Add(record.ScopeId, siteScope);
                            }
                            logger.Info("get site info from dao:siteId:{0}, siteUrl:{1},path:{2}", record.AveSiteId.ToString(), site?.url, record.FullPath);
                            RMScopeDao.AddOrUpateSiteScope(siteScope);
                        }
                        else
                        {
                            logger.Warn($"Site is null, cannot get site scope. SiteId:{record.AveSiteId}");
                            if (!invalidSiteIds.Contains(record.AveSiteId))
                            {
                                invalidSiteIds.Add(record.AveSiteId);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while MakeSPObjectFullPath. Error:{e.ToString()}");
            }
        }

        private void SetRuleInfos(BaseRecordDto record, List<RMRule> rules = null)
        {
            if (record.RuleId != Guid.Empty)
            {
                RMRule rule = null;
                if (rules != null)
                {
                    rule = rules.FirstOrDefault(a => a.RuleId == record.RuleId);
                }
                else
                {

                    rule = RMRuleDao.GetRuleById(record.RuleId);
                }
                record.RuleName = rule?.RuleName;
                if (rule == null)
                {
                    record.DisposalAction = (int)RMContentDisposalAction.None;
                }
                else
                {
                    if (record.SourceFlag == (int)SourceFlag.Physical)
                    {
                        record.DisposalAction = (int)rule.PhysicalDisposalAction;
                    }
                    else if (record.SourceFlag == (int)SourceFlag.FileSystem)
                    {
                        record.DisposalAction = (int)rule.FSDisposalAction;
                    }
                    else if (record.SourceFlag == (int)SourceFlag.SharePointOnPrem)
                    {
                        record.DisposalAction = (int)rule.SPLocalDisposalAction;
                    }
                    else if (record.SourceFlag == (int)SourceFlag.OneDrive)
                    {
                        record.DisposalAction = (int)rule.OneDriveDisposalAction;
                    }
                    else if (record.SourceFlag == (int)SourceFlag.AzureFileShare)
                    {
                        record.DisposalAction = (int)rule.AzureFileDisposalAction;
                    }
                    else if (record.SourceFlag == (int)SourceFlag.Box)
                    {
                        record.DisposalAction = (int)rule.BoxDisposalAction;
                    }
                    else if (record.SourceFlag == (int)SourceFlag.Google)
                    {
                        record.DisposalAction = (int)rule.GoogleDriveDisposalAction;
                    }
                    else if (record.SourceFlag > (int)SourceFlag.Connector)
                    {
                        record.DisposalAction = (int)rule.ConnectorDisposalAction;
                    }
                    else
                    {
                        record.DisposalAction = (int)rule.DisposalAction;
                    }
                }

                record.ExchangeDisposalAction = rule == null ? (int)RMContentDisposalAction.None : rule.ExchangeDisposalAction;
            }
            else
            {
                record.DisposalAction = (int)RMContentDisposalAction.None;
                record.ExchangeDisposalAction = (int)RMContentDisposalAction.None;
            }
        }

        private void SetTermInfo(BaseRecordDto record, Dictionary<Guid, string> idNameMapping)
        {
            if (record.TermId != Guid.Empty && idNameMapping != null && idNameMapping.ContainsKey(record.TermId))
            {
                record.TermName = idNameMapping[record.TermId];
            }
        }

        private void SetPredictInfo(BaseRecordDto record, Dictionary<Guid, string> idNameMapping)
        {
            if (record.PredictTermId != Guid.Empty && idNameMapping != null && idNameMapping.ContainsKey(record.PredictTermId))
            {
                record.PredictTermName = idNameMapping[record.PredictTermId];
            }
        }

        private void SetObjectType(BaseRecordDto record)
        {
            if (record.ExtensionForFile == "RM_RDM_RecordDetails_DataType_FileNull")
            {
                record.ExtensionForFile = I18NEntity.GetString("RM_RDM_RecordDetails_DataType_FileNull");
            }
            if (record.ExtensionForFile == "RM_RDM_RecordDetails_DataType_SPItem")
            {
                record.ExtensionForFile = I18NEntity.GetString("RM_RDM_RecordDetails_DataType_SPItem");
            }
            if (record.NodeType == (int)RMNodeLevel.FSFolder && string.IsNullOrEmpty(record.ExtensionForFile))
            {
                record.ExtensionForFile = I18NEntity.GetString("RM_RDM_RecordDetails_DataType_FSFolder");
            }
            if (record.NodeType == (int)RMNodeLevel.PhysicalBox)
            {
                record.ExtensionForFile = I18NEntity.GetString("RM_PRM_PRE_Filter_PhysicalBox");
            }
            if (record.NodeType == (int)RMNodeLevel.PhysicalFile)
            {
                record.ExtensionForFile = I18NEntity.GetString("RM_PRM_PRE_Filter_PhysicalFile");
            }
            if (record.NodeType == (int)RMNodeLevel.PhysicalRecord)
            {
                record.ExtensionForFile = I18NEntity.GetString("RM_PRM_PRE_Filter_PhysicalRecord");
            }
            if (record.NodeType == (int)RMNodeLevel.PhysicalCustom)
            {
                record.ExtensionForFile = I18NEntity.GetString("RM_PRM_PRE_TableItemType_Container");
            }
            if (record.NodeType == (int)RMNodeLevel.AzureFileShareDirectory)
            {
                record.ExtensionForFile = I18NEntity.GetString("RM_RDM_RecordDetails_DataType_AzureFileDirectory");
            }
            if (record.NodeType == (int)RMNodeLevel.CustomizeConnectorItem)
            {
                record.ExtensionForFile = I18NEntity.GetString("RM_Connector_ItemLevel_Item");
            }
            if (record.NodeType == (int)RMNodeLevel.BoxFolder)
            {
                record.ExtensionForFile = I18NEntity.GetString("RM_RDM_RecordDetails_DataType_BoxFolder");
            }
            if (record.NodeType == (int)RMNodeLevel.Folder)
            {
                record.ExtensionForFile = I18NEntity.GetString("RM_RDM_RecordDetails_DataType_SPFolder");
            }
        }

        private void InheritRuleInfoFromParent(List<BaseRecordDto> baseRecords, List<RMRule> allRules)
        {
            List<Guid> parentId = new List<Guid>();
            foreach (BaseRecordDto dto in baseRecords)
            {
                if (dto.RuleId == Guid.Empty)
                {
                    //self no rule
                    if (dto.NodeType == (int)RMNodeType.PhyRecord)
                    {
                        parentId.Add(dto.BoxId);
                        parentId.Add(dto.FileId);
                    }
                    else if (dto.NodeType == (int)RMNodeType.PhyFile)
                    {
                        parentId.Add(dto.BoxId);
                    }
                }
            }
            List<Guid> tempDistinct = parentId.Where(a => a != Guid.Empty).Distinct().ToList();
            if (tempDistinct.IsNullOrEmpty())
            {
                logger.Debug("No need to check parent Rules");
                return;
            }
            List<Record> parents = ExplorerDao.QueryAll(a => tempDistinct.Contains(a.Id) && a.ScopeId == Guid.Empty && a.RuleId != Guid.Empty).OrderBy(a => a.NodeType).ToList();
            logger.Info("query record count {0}, parent with rules count {1}", baseRecords.Count, parents.Count);
            foreach (BaseRecordDto dto in baseRecords)
            {
                if (dto.RuleId == Guid.Empty)
                {
                    if (dto.NodeType == (int)RMNodeType.PhyRecord)
                    {
                        Record parentFolder = parents.FirstOrDefault(a => a.Id == dto.FileId);
                        if (parentFolder != null && parentFolder.RuleId != null)
                        {
                            dto.TermName = parentFolder.TermName;
                            dto.TermId = parentFolder.TermId;
                            dto.RuleId = parentFolder.RuleId;
                            SetRuleInfos(dto, allRules);
                            continue;
                        }
                        Record parentBox = parents.FirstOrDefault(a => a.Id == dto.BoxId);
                        if (parentBox != null && parentBox.RuleId != null)
                        {
                            dto.TermName = parentBox.TermName;
                            dto.TermId = parentBox.TermId;
                            dto.RuleId = parentBox.RuleId;
                            SetRuleInfos(dto, allRules);
                            continue;
                        }
                    }
                    else if (dto.NodeType == (int)RMNodeType.PhyFile)
                    {
                        Record parentBox = parents.FirstOrDefault(a => a.Id == dto.BoxId);
                        if (parentBox != null && parentBox.RuleId != null)
                        {
                            dto.RuleId = parentBox.RuleId;
                            SetRuleInfos(dto, allRules);
                            continue;
                        }
                    }
                }
            }
        }

        private void AppendTermInfoForRecordLevel(List<BaseRecordDto> baseRecords)
        {
            List<BaseRecordDto> records = baseRecords.Where(a => a.NodeType == (int)RMNodeType.PhyRecord && string.IsNullOrEmpty(a.TermName)).ToList();
            List<BaseRecordDto> tempList = new List<BaseRecordDto>();
            foreach (BaseRecordDto rec in records)
            {
                BaseRecordDto folder = baseRecords.FirstOrDefault(a => a.Id == rec.FileId);
                if (folder == null)
                {
                    tempList.Add(rec);
                }
                else if (folder.CustomColumnDic != null)
                {
                    //rec.TermName = folder.TermName;
                    //Dictionary<string, string> metaInfo = string.IsNullOrEmpty(folder.MetaInfo) ? null : JsonConvert.DeserializeObject<Dictionary<string, string>>(folder.MetaInfo);
                    //string termName = metaInfo == null ? "" : JsonConvert.DeserializeObject<TaxonomyColumnValue>(metaInfo[MetaInfo.Classification])?.Name;
                    //rec.TermName = termName;

                    rec.TermName = folder.CustomColumnDic.ContainsKey(MetaInfo.Classification) ? folder.CustomColumnDic[MetaInfo.Classification]?.GetTaxonomyColumnValue()?.Name : null;
                    string termId = folder.CustomColumnDic.ContainsKey(MetaInfo.Classification) ? folder.CustomColumnDic[MetaInfo.Classification]?.GetTaxonomyColumnValue()?.Id : null;
                    rec.TermId = !string.IsNullOrEmpty(termId) ? new Guid(termId) : Guid.Empty;
                }
            }
            if (tempList.Count == 0)
            {
                return;
            }
            List<Guid> folderIds = tempList.Select(a => a.FileId).Distinct().ToList();
            List<Record> folders = new List<Record>();
            if (folderIds.Count < 1000)
            {
                folders = ExplorerDao.QueryAll(a => a.NodeType == (int)RMNodeType.PhyFile && folderIds.Contains(a.Id) && a.ScopeId == Guid.Empty).ToList();
            }
            else
            {
                int index = 0;
                while (index * 1000 < folderIds.Count)
                {
                    List<Guid> tempIds = folderIds.Skip(index * 1000).Take(1000).ToList();

                    folders.AddRange(ExplorerDao.QueryAll(a => a.NodeType == (int)RMNodeType.PhyFile && folderIds.Contains(a.Id) && a.ScopeId == Guid.Empty));
                    index++;
                }
            }
            foreach (BaseRecordDto rec in tempList)
            {
                Record folder = folders.FirstOrDefault(a => a.Id == rec.FileId);
                if (folder != null)
                {
                    string termName = GetPhysicalTermNameFromMetaInfo(folder)?.Name;
                    rec.TermName = termName;
                    string termId = GetPhysicalTermNameFromMetaInfo(folder)?.Id;
                    rec.TermId = !string.IsNullOrEmpty(termId) ? new Guid(termId) : Guid.Empty;
                }
            }
        }

        /// <summary>
        /// TODO Need Move To CosmosDB
        /// </summary>
        /// <param name="baseRecords"></param>
        private async System.Threading.Tasks.Task AppendPhyPersonalHoldInfoAsync(List<BaseRecordDto> baseRecords)
        {
            if (baseRecords.IsNullOrEmpty())
            {
                return;
            }
            List<Guid> tempParam = new List<Guid>();
            foreach (var result in baseRecords)
            {
                if (result.NodeType == (int)RMNodeType.PhyBox)
                {
                    tempParam.Add(result.Id);
                }
                else if (result.NodeType == (int)RMNodeType.PhyFile)
                {
                    tempParam.Add(result.Id);
                    tempParam.Add(result.BoxId);
                }
                else if (result.NodeType == (int)RMNodeType.PhyRecord)
                {
                    tempParam.Add(result.FileId);
                    tempParam.Add(result.BoxId);
                }
            }
            tempParam = tempParam.Distinct().ToList();

            List<RMRecordLoanAlliance> loanAls = RecordLoanAllianceDao.GetPhyRecordAllianceByIds(tempParam);
            var allHoldRelatedRecords = ExplorerDao.GetHoldRecordsByIds(tempParam);
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            Dictionary<int, RMAccount> accountMap = AccountDao.FindAll().ToDictionary(k => k.Id, v => v);
            foreach (var result in baseRecords)
            {
                if (result.NodeType == (int)RMNodeType.PhyFile || result.NodeType == (int)RMNodeType.PhyBox)
                {
                    //RECO-10110, multipule personal hold by user
                    var al = loanAls.Where(a => a.RecordsId == result.Id);
                    if (al != null && al.Count() > 0)
                    {
                        result.PersonHold = true;
                        result.PersonHoldBy = string.Join("; ", al.Select(o => o.HoldBy));
                        var holdReleaseTime = al.FirstOrDefault()?.HoldReleaseTime;
                        result.PersonHoldReleaseTime = holdReleaseTime > 0 && holdReleaseTime != DateTime.MaxValue.Ticks ? 
                            GeneralSettingService.ConvertTiksToDateTime(gls, al.First().HoldReleaseTime, true).SimplifyFormatTime : "";
                    }
                   
                }
                else if(result.NodeType == (int)RMNodeType.PhyRecord)
                {
                    var recordAl = loanAls.Where(a => a.RecordsId == result.FileId);
                    if (recordAl == null || recordAl.Count() == 0)
                    {
                        recordAl = loanAls.Where(a => a.RecordsId == result.BoxId);
                    }
                    if(recordAl != null && recordAl.Count() > 0)
                    {
                        result.PersonHold = true;
                        result.PersonHoldBy = string.Join("; ", recordAl.Select(o => o.HoldBy));
                        var holdReleaseTime = recordAl.FirstOrDefault()?.HoldReleaseTime;
                        result.PersonHoldReleaseTime = holdReleaseTime > 0 && holdReleaseTime != DateTime.MaxValue.Ticks ?
                            GeneralSettingService.ConvertTiksToDateTime(gls, recordAl.First().HoldReleaseTime, true).SimplifyFormatTime : "";
                        if (!string.IsNullOrEmpty(result.DisposalDueDate))
                        {
                            result.DisposalDueDate = holdReleaseTime > 0 && holdReleaseTime != DateTime.MaxValue.Ticks ?
                                GeneralSettingService.ConvertTiksToDateTime(gls, recordAl.First().HoldReleaseTime, true).SimplifyFormatTime : "";
                        }
                    }
                }
                AssemblePhysicalHoldStatus(result, allHoldRelatedRecords, accountMap, gls);
            }
        }

        /// <summary>
        /// 如果hold Status是true，那么不用处理；
        /// 如果是false，那么需要检查继承关系，检查其所属的box/folder，看看是否是被hold
        /// </summary>
        /// <param name="result"></param>
        /// <param name="allRelatedRecords"></param>
        private void AssemblePhysicalHoldStatus(BaseRecordDto result, List<Record> allHoldRelatedRecords, Dictionary<int, RMAccount> accountMap, GeneralSettingModel gls)
        {
            var allRelatedHoldIds = new List<string>();
            foreach (var relatedHold in allHoldRelatedRecords)
            {
                allRelatedHoldIds.AddRange(GetAllExistHoldIds(relatedHold));
            }
            List<RMHold> allRelatedHolds = HoldDao.GetHoldByIds(allRelatedHoldIds);

            if (result.NodeType == (int)RMNodeType.PhyBox)
            {
                var box = allHoldRelatedRecords.FirstOrDefault(a => a.Id == result.Id);
                if (box != null)
                {
                    result.HoldStatus = true;
                    result.HoldBy = this.AssembleAccountDisplayName(box.HoldBy, accountMap.Values);
                    result.ReleaseTime = GeneralSettingService.ConvertTiksToDateTime(gls, box.HoldReleaseTime, true).SimplifyFormatTime;
                    result.HoldTitle = GetHoldTitle(allRelatedHolds, box);
                }
            }
            else if (result.NodeType == (int)RMNodeType.PhyFile)
            {
                var file = allHoldRelatedRecords.FirstOrDefault(a => a.Id == result.Id);
                var box = result.BoxId == Guid.Empty ? null : allHoldRelatedRecords.FirstOrDefault(a => a.Id == result.BoxId);
                if (file != null)
                {
                    result.HoldStatus = true;
                    result.HoldBy = this.AssembleAccountDisplayName(file.HoldBy, accountMap.Values);
                    result.ReleaseTime = GeneralSettingService.ConvertTiksToDateTime(gls, file.HoldReleaseTime, true).SimplifyFormatTime;
                    result.HoldTitle = GetHoldTitle(allRelatedHolds, file);
                }
                else if (box != null)
                {
                    result.HoldStatus = true;
                    result.HoldBy = this.AssembleAccountDisplayName(box.HoldBy, accountMap.Values);
                    result.ReleaseTime = GeneralSettingService.ConvertTiksToDateTime(gls, box.HoldReleaseTime, true).SimplifyFormatTime;
                    result.HoldTitle = GetHoldTitle(allRelatedHolds, box);
                    if (!string.IsNullOrEmpty(result.DisposalDueDate))
                    {
                        result.DisposalDueDate = GeneralSettingService.ConvertTiksToDateTime(gls, box.HoldReleaseTime, true).SimplifyFormatTime;
                    }
                }
            }
            else if (result.NodeType == (int)RMNodeType.PhyRecord)
            {
                var file = allHoldRelatedRecords.FirstOrDefault(a => a.Id == result.FileId);
                var box = result.BoxId == Guid.Empty ? null : allHoldRelatedRecords.FirstOrDefault(a => a.Id == result.BoxId);
                if (file != null)
                {
                    result.HoldStatus = true;
                    result.HoldBy = this.AssembleAccountDisplayName(file.HoldBy, accountMap.Values);
                    result.ReleaseTime = GeneralSettingService.ConvertTiksToDateTime(gls, file.HoldReleaseTime, true).SimplifyFormatTime;
                    result.HoldTitle = GetHoldTitle(allRelatedHolds, file);
                }
                else if (box != null)
                {
                    result.HoldStatus = true;
                    result.HoldBy = this.AssembleAccountDisplayName(box.HoldBy, accountMap.Values);
                    result.ReleaseTime = GeneralSettingService.ConvertTiksToDateTime(gls, box.HoldReleaseTime, true).SimplifyFormatTime;
                    result.HoldTitle = GetHoldTitle(allRelatedHolds, box);
                }
            }
            if (!result.HoldStatus)
            {
                result.ReleaseTime = "";
                result.HoldBy = null;
            }
        }

        private List<string> GetAllExistHoldIds(Record tempExplorerItem)
        {
            List<string> recordAllExistHoldIds = new List<string>();
            if (!string.IsNullOrEmpty(tempExplorerItem.HoldId))
            {
                recordAllExistHoldIds.Add(tempExplorerItem.HoldId);
            }
            if (tempExplorerItem.AppendHolds_Array != null)
            {
                recordAllExistHoldIds.AddRange(tempExplorerItem.AppendHolds_Array.ToList());
            }
            return recordAllExistHoldIds;
        }

        private string GetHoldTitle(List<RMHold> allRelatedHolds, Record box)
        {
            var existHoldIds = GetAllExistHoldIds(box);
            var existHolds = allRelatedHolds.Where(h => existHoldIds.Contains(h.Id)).OrderBy(h => h.Id, new HoldSpecialComparer(existHoldIds)).ToList();
            var HoldProfileTitle = string.Join("; ", existHolds.Select(h => h.Name));
            return HoldProfileTitle;
        }

        private string AssembleAccountDisplayName(string principalName, IEnumerable<RMAccount> accounts)
        {
            RMAccount temp = accounts.FirstOrDefault(a => a.UserPrincipalName == principalName);
            if (temp != null)
            {
                return temp.DisplayName;
            }
            return principalName;
        }


        private async System.Threading.Tasks.Task AppendHoldReleaseTimeInfoAsync(List<BaseRecordDto> baseRecords)
        {
            if (baseRecords.IsNullOrEmpty())
            {
                return;
            }
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            var allHoldIds = new List<string>();
            baseRecords.ForEach(r => allHoldIds.AddRange(GetAllExistHoldIds(r)));
            List<RMHold> allHolds = HoldDao.GetHoldByIds(allHoldIds);
            foreach (var result in baseRecords)
            {
                if (result.HoldStatus)
                {
                    var existHolds = GetAllExistHoldIds(result);
                    var holds = allHolds.Where(h => existHolds.Contains(h.Id)).OrderBy(h => h.Id, new HoldSpecialComparer(existHolds)).ToList();
                    result.ReleaseTime = GeneralSettingService.ConvertTiksToDateTime(gls, result.HoldReleaseTime, true).SimplifyFormatTime;
                    result.HoldTitle = string.Join("; ", holds.Select(h => h.Name).ToList());
                    if (result.HoldByUsers.Count > 0)
                    {
                        var distinctHoldByUsers = result.HoldByUsers.Select(h => h.HoldBy).Distinct();
                        if (distinctHoldByUsers.Count() == 1)
                        {
                            result.HoldBy = distinctHoldByUsers.FirstOrDefault();
                        }
                        else
                        {
                            result.HoldBy = string.Join("; ", result.HoldByUsers.OrderBy(h => h.HoldId, new HoldSpecialComparer(existHolds)).Select(h => h.HoldBy));
                        }
                    }
                }
            }
        }

        private List<string> GetAllExistHoldIds(BaseRecordDto baseRecord)
        {
            List<string> recordAllExistHoldIds = new List<string>();
            if (!string.IsNullOrEmpty(baseRecord.HoldId))
            {
                recordAllExistHoldIds.Add(baseRecord.HoldId);
            }
            if (baseRecord.AppendHolds_Array != null)
            {
                recordAllExistHoldIds.AddRange(baseRecord.AppendHolds_Array.ToList());
            }
            return recordAllExistHoldIds;
        }

        private TaxonomyColumnValue GetPhysicalTermNameFromMetaInfo(Record record)
        {
			//var termMataInfo = new PhysicalRecord(record)[MetaInfo.Classification];
			//return JsonConvert.DeserializeObject<TaxonomyColumnValue>(termMataInfo);
			var metaInfo = string.IsNullOrEmpty(record.MetaInfo) ? null : JsonConvert.DeserializeObject<Dictionary<string, string>>(record.MetaInfo);
            return metaInfo != null && metaInfo.ContainsKey(MetaInfo.Classification) ? JsonConvert.DeserializeObject < TaxonomyColumnValue > (metaInfo[MetaInfo.Classification]) : null;
        }

        private void SetCustomColumnDateTimeValue(BaseRecordDto record, GeneralSettingModel gls)
        {
            record.CustomColumnDic?.Values?.ToList().ForEach(a => 
            { 
                if (a.Date != default) 
                { 
                    a.Date = DateTimeUtil.ConvertTimeFromUtc(a.Date, gls); 
                } 
            });
        }

        private void SetSPObjectDisposalDueDate(long now, GeneralSettingModel gls, BaseRecordDto record)
        {
            if (record != null && !string.IsNullOrEmpty(record.DisposalDueDate))
            {
                long tempTicks;
                if (long.TryParse(record.DisposalDueDate, out tempTicks))
                {
                    var minDate = DateTime.MinValue;
                    if (tempTicks > minDate.Ticks)
                    {
                        //if (tempTicks > now)
                        //{
                        //    record.DisposalDueDate = GeneralSettingService.ConvertTiksToDateTime(gls, tempTicks, true).SimplifyFormatTime;
                        //}
                        //else
                        //{
                        //    record.DisposalDueDate = I18NEntity.GetString("RDM_RecordsExporer_Status_NextJob");
                        //}
                        record.DisposalDueDate = GeneralSettingService.ConvertTiksToDateTime(gls, tempTicks, true).SimplifyFormatTime;
                    }
                }
                else
                {
                    record.DisposalDueDate = I18NEntity.GetString(record.DisposalDueDate);
                }
            }
        }

    }
}
