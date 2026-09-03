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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.RMMachineLearning;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.Service.Services.RMMachineLearning.AuditHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static ICSharpCode.SharpZipLib.Zip.ExtendedUnixData;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Common.Retrying;
using AvePoint.GCommon.Utility;
using Azure.Storage.Blobs;
using AvePoint.GCommon.Utility.Cloud;
using Cloud.Sdk.Core;
using Util.AI.Text.Extractor;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.Contract.RMWeb.Tree.Base;

namespace AvePoint.RA.Service.Services.RMMachineLearning
{
    [Audit]
    public class TrainingScopeService : RMServiceBase, ITrainingScopeService
    {
        private static readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private IRMMLTermDao trainingTermDao => PlatformWindsorManager.GetService<IRMMLTermDao>();
        private ITermDao termDao => PlatformWindsorManager.GetService<ITermDao>();
        private static IRMMLTrainingModelDao trainingModelDao => PlatformWindsorManager.GetService<IRMMLTrainingModelDao>();
        private static IRMRemoteNodeDao remoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private static IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private static IExplorerQueryService ExplorerQueryService => PlatformWindsorManager.GetService<IExplorerQueryService>();
        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private static IRMScopeDao RMScopeDao => PlatformWindsorManager.GetService<IRMScopeDao>();
        private static ITenantInfoDao TenantDao => PlatformWindsorManager.GetService<ITenantInfoDao>();
        private readonly IExplorerDao explorerDao = new ExplorerDao();
        private static Dictionary<string, string> SiteUrlCache = new();
        public async Task<MLTrainingScopeResult> QueryAsync(MLTrainingScopeQueryParam param)
        {
            try
            {
                param.PageSize = param.PageSize == 0 ? 10 : param.PageSize;
                var dto = new ExplorerQueryV3Dto()
                {
                    QueryOption = new ExplorerQueryOptionV3()
                    {
                        Values = new List<ExplorerSearchOptionV3>()
                    },
                    PagingInfo = new ExplorerPagingInfo
                    {
                        PageIndex = param.PageIndex,
                        PageSize = param.PageSize,
                    }
                };
                if (!string.IsNullOrEmpty(param.SearchValue))
                {
                    dto.QueryOption.Values.Add(new ExplorerSearchOptionV3
                    {
                        Column = new ExplorerQueryColumn { Id = RecordBuildInColumnIds.NameOrTitle },
                        Value = JsonConvert.SerializeObject(param.SearchValue)
                    });
                }
                var hasStatusFilter = false;
                if (param.Filters != null)
                {
                    foreach (var filter in param.Filters)
                    {
                        if (filter.ColumnValues != null && filter.ColumnValues.Count > 0)
                        {
                            if (filter.Column == TrainingFilterColumn.TrainingTerm)
                            {
                                dto.QueryOption.Values.Add(new ExplorerSearchOptionV3
                                {
                                    Column = new ExplorerQueryColumn { Id = QueryCloumnIds.TrainingTermId },
                                    Value = JsonConvert.SerializeObject(filter.ColumnValues.Select(v => new Guid(v)).ToList()),
                                });
                            }
                        }
                        if (filter.Column == TrainingFilterColumn.Status)
                        {
                            hasStatusFilter = true;
                            dto.QueryOption.Values.Add(new ExplorerSearchOptionV3
                            {
                                Column = new ExplorerQueryColumn { Id = QueryCloumnIds.TrainingScope },
                                Value = JsonConvert.SerializeObject(filter.ColumnValues.Select(v => (MLFileStatus)int.Parse(v)).ToList())
                            });
                        }
                    }
                }
                if (!hasStatusFilter)
                {
                    dto.QueryOption.Values.Add(new ExplorerSearchOptionV3
                    {
                        Column = new ExplorerQueryColumn { Id = QueryCloumnIds.TrainingScope },
                        Value = JsonConvert.SerializeObject(new List<MLFileStatus> { MLFileStatus.NotTrain, MLFileStatus.Training, MLFileStatus.Trained })
                    });
                }
                var columnName = param.SortBy switch
                {
                    "Name" => CosmosConst.C_LeafName,
                    "TermName" => CosmosConst.C_TermName,
                    "TrainingScope" => CosmosConst.C_TrainingScope,
                    _ => CosmosConst.C_LeafName
                };
                dto.QueryOption.OrderColumn = new ExplorerQueryOrderColumn() { Column = new ExplorerQueryColumn { Name = columnName }, OrderAsc = param.IsAscending };

                var queryResult = explorerDao.SearchRecordsV3(dto, null);
                var totalCount = explorerDao.QueryCountV3(dto, null);
                var pageResult = new MLTrainingScopeResult
                {
                    TotalCount = totalCount,
                    PageIndex = queryResult.Item2,
                    TrainingScopes = new List<MLTrainScopeDto>()
                };
                var allTermIds = queryResult.Item1.Select(r => r.TrainingTermId).ToList();
                //var allTermDic = termDao.GetRMTermsByTermIds(allTermIds).ToDictionary(t => t.UniqueId, t => t.Name);
                var allTermDic = (await termDao.FindListAsync(tm => allTermIds.Contains(tm.UniqueId))).ToDictionary(t => t.UniqueId, t => t.Name);
                foreach (var item in queryResult.Item1)
                {
                    pageResult.TrainingScopes.Add(new MLTrainScopeDto()
                    {
                        Id = item.Id,
                        FileName = item.LeafName,
                        TermName = allTermDic.ContainsKey(item.TrainingTermId) ? allTermDic[item.TrainingTermId] : "",
                        TermId = item.TrainingTermId,
                        Status = (MLFileStatus)item.TrainingScope,
                        SourceFlag = item.SourceFlag,
                        FullPath = GetRecordFullPath(item)
                    });
                }
                return pageResult;
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred in QueryAsync: {ex}");
                return new MLTrainingScopeResult
                {
                    TotalCount = 0,
                    PageIndex = param.PageIndex,
                    TrainingScopes = new List<MLTrainScopeDto>()
                };
            }
        }

        private static string GetRecordFullPath(Record record)
        {
            try
            {
                var siteUrl = GetSiteUrl(record.AveSiteId);
                var fullPath = WebUtil.MakeFullUrl(siteUrl, record.DirPath);
                if (record.ExtensionForFile == "RM_RDM_RecordDetails_DataType_SPItem")
                {
                    fullPath = WebUtil.GetListItemRealPath(fullPath);
                }
                return fullPath;
            }
            catch (Exception ex)
            {
                Logger.Warn($"An error while get record full path, record id: {record?.Id} message: {ex}");
                return string.Empty;
            }
        }

        private static string GetSiteUrl(string siteId)
        {
            if (!SiteUrlCache.TryGetValue(siteId, out var siteUrl))
            {
                siteUrl = remoteNodeDao.GetRemoteSiteCollectionById(siteId)?.url;
                if (!SiteUrlCache.TryAdd(siteId, siteUrl))
                {
                    Logger.Warn($"An error while add site url, site is:{siteId}");
                }
            }
            return siteUrl;
        }

        public List<MLTermDto> GetAllMLTerm()
        {
            var terms = trainingTermDao.GetAllMLTerm();
            return terms;
        }

        public MLModelStatus GetTrainingModelStatus()
        {
            var trainingJobs = RMJobService.GetRunningJobs(JobType.MachineLearningTraining);
            var analyseJobs = RMJobService.GetRunningJobs(JobType.MachineLearningAnalyse);
            if (trainingJobs.Any() || analyseJobs.Any())
            {
                return MLModelStatus.Running;
            }
            var traingingModel = trainingModelDao.GetDefaultModel();
            if (traingingModel == null)
            {
                return MLModelStatus.None;
            }
            var trainStatus = (MLModelStatus)traingingModel.TrainStatus;
            var publishStatus = (MLModelStatus)traingingModel.PublishStatus;
            if (trainStatus == MLModelStatus.Succeeded)
            {
                return publishStatus;
            }
            else
            {
                return trainStatus;
            }
        }

        public async Task<bool> DeleteAIRelatedResourcesAsync(string tenantId)
        {
            bool result = true;
            try
            {
                if (!string.IsNullOrEmpty(GCommonRoleConfiguration.ICS_API_URL))
                {
                    RMRetryer retryer = RMRetryerBuilder.CreateBuilder().Build();
                    var icsClient = AosApiUtility.GetIcsClient(tenantId);
                    var trainingModel = trainingModelDao.GetDefaultModel();
                    if (trainingModel != null)
                    {
                        if (!trainingModel.ExpiredResourcesDeleted)
                        {
                            var modelId = trainingModel.Id;
                            result &= await icsClient.EndpointService.DeleteAsync(modelId);

                            // Check if it's GCP environment and use appropriate deletion method
                            if (!RMGlobalConfiguration.EnvSetting.IsGCPEnvironment)
                            {
                                // For non-GCP environments, use the old BlobClient deletion method
                                retryer.Retry(() =>
                                {
                                    var storageInfo = icsClient.StorageService.GetStorageInfoForDeleteAsync(modelId, new Cloud.Sdk.Data.Amls.Ics.Contracts.StorageRequest() { TsvFileName = RecordsConstants.TrainingData_FileName }).GetAwaiter().GetResult();
                                    var blobClient = new BlobClient(new Uri(storageInfo.SasToken));
                                    blobClient.DeleteIfExists();
                                    Logger.Info($"delete data.tsv success using BlobClient.");
                                    result &= true;
                                });
                            }
                            else
                            {
                                retryer.Retry(() =>
                                {
                                    var storageInfo = icsClient.StorageService.GetStorageInfoForDeleteAsync(modelId, new Cloud.Sdk.Data.Amls.Ics.Contracts.StorageRequest() { TsvFileName = RecordsConstants.TrainingData_FileName }).GetAwaiter().GetResult();
                                    using (var httpClient = new HttpClient())
                                    {
                                        using (var response = httpClient.DeleteAsync(storageInfo.SasToken).GetAwaiter().GetResult())
                                        {
                                            if (response.StatusCode == HttpStatusCode.NotFound)
                                            {
                                                Logger.Warn($"data.tsv not found in GCP environment, skip delete. modelId: {modelId}");
                                            }
                                            else
                                            {
                                                response.EnsureSuccessStatusCode();
                                            }
                                        }
                                    }
                                    Logger.Info($"delete data.tsv success in GCP environment.");
                                    result &= true;
                                });
                            }
                            if (result)
                            {
                                trainingModel.ExpiredResourcesDeleted = true;
                                await trainingModelDao.UpdateAsync(trainingModel);
                            }
                        }
                    }
                }
                else
                {
                    Logger.Info($"No need to DeleteAIRelated, the ics api url is null");
                }
            }
            catch (CloudApiException e)
            {
                if (e.ErrorCode == 404)
                {
                    Logger.Warn($"Ics service not found, it means do not need delete: {e}");
                    result = true;
                }
                else
                {
                    Logger.Error($"DeleteAIRelated error: {e}");
                    result = false;
                }
            }
            catch (Exception e)
            {
                Logger.Error($"DeleteAIRelated error: {e}");
                result = false;
            }
            return result;
        }

        public async Task<MLTrainingScopeResult> LoadUsageScope(MLTrainingScopeQueryParam param)
        {
            try
            {
                MLTrainingScopeResult result = new MLTrainingScopeResult()
                {
                    TrainingScopes = new List<MLTrainScopeDto>(),
                };
                List<SourceFlag> sourceFlagQueries = GetSourceFlagByLicense();
                IExtract extractor = new Extractor();
                var support = extractor.GetAllSupportTypes();
                var smartTermIds = trainingTermDao.GetAllMLTermIds();
                var query = new ExplorerQueryV3Dto()
                {
                    QueryOption = new ExplorerQueryOptionV3()
                    {
                        Values = new List<ExplorerSearchOptionV3>()
                        {
                            new ExplorerSearchOptionV3
                            {
                                Column = new ExplorerQueryColumn { Id = QueryCloumnIds.SourceFlag },
                                Value = JsonConvert.SerializeObject(sourceFlagQueries)
                            },
                            new ExplorerSearchOptionV3
                            {
                                Column = new ExplorerQueryColumn{Id = QueryCloumnIds.FileExtension},
                                Value = JsonConvert.SerializeObject(support.ToList())
                            },
                            new ExplorerSearchOptionV3
                            {
                                Column = new ExplorerQueryColumn { Id = QueryCloumnIds.TrainingScope },
                                Value = JsonConvert.SerializeObject(new List<MLFileStatus> { MLFileStatus.None })
                            },
                            new ExplorerSearchOptionV3
                            {
                                Column = new ExplorerQueryColumn { Id = QueryCloumnIds.TermId },
                                Value = JsonConvert.SerializeObject(smartTermIds)
                            }
                        }
                    },
                    PagingInfo = new ExplorerPagingInfo
                    {
                        PageIndex = param.PageIndex,
                        PageSize = param.PageSize == 0 ? 10 : param.PageSize,
                    }
                };
                if (!string.IsNullOrEmpty(param.SearchValue))
                {
                    query.QueryOption.Values.Add(new ExplorerSearchOptionV3
                    {
                        Column = new ExplorerQueryColumn { Id = RecordBuildInColumnIds.NameOrTitle },
                        Value = JsonConvert.SerializeObject(param.SearchValue)
                    });
                }

                var columnName = param.SortBy switch
                {
                    "Name" => CosmosConst.C_LeafName,
                    "TermName" => CosmosConst.C_TermName,
                    _ => CosmosConst.C_LeafName
                };
                query.QueryOption.OrderColumn = new ExplorerQueryOrderColumn() { Column = new ExplorerQueryColumn { Name = columnName }, OrderAsc = param.IsAscending };


                var queryResult = await ExplorerQueryService.QueryDataListWithTotalAsync(query);
                result.TotalCount = queryResult.PagingInfo.Total;
                result.PageIndex = queryResult.PagingInfo.PageIndex;
                result.TrainingScopes = new List<MLTrainScopeDto>();
                foreach (var data in queryResult.Datas)
                {
                    if(data.SourceFlag == (int)SourceFlag.SharePoint || data.SourceFlag == (int)SourceFlag.OneDrive
                        || data.SourceFlag == (int)SourceFlag.Teams)
                    {
                        var dicMap = RMScopeDao.GetScopeInfoByIds(new List<Guid>() { data.ScopeId });
                        if (dicMap.ContainsKey(data.ScopeId))
                        {
                            var sPath = dicMap[data.ScopeId];
                            data.FullPath = WebUtil.MakeFullUrl(sPath?.FullPath, data.DirPath);
                        }
                        else
                        {
                            SharePointSettingUtility SPUtility = new SharePointSettingUtility();
                            var site = SPUtility.GetRemoteSiteCollection(data.AveSiteId.ToString());
                            data.FullPath = site == null ? string.Empty : WebUtil.MakeFullUrl(site.url, data.DirPath);
                            Logger.Info("get site info from dao:siteId:{0}, siteUrl:{1},path:{2}", data.AveSiteId.ToString(), site?.url, data.FullPath);
                            if (site != null)
                            {
                                RMScopeDao.AddOrUpateSiteScope(new RMScope()
                                {
                                    FullPath = site.url,
                                    ScopeId = data.ScopeId,
                                    ScopeName = site.Name,
                                    IsRemoved = false,
                                });
                            }
                        }
                    }
                    result.TrainingScopes.Add(new MLTrainScopeDto
                    {
                        Id = data.Id,
                        FileName = data.LeafName,
                        TermId = data.TermId,
                        TermName = data.TermName,
                        SourceFlag = data.SourceFlag,
                        FullPath = data.SourceFlag == (int)SourceFlag.SharePoint || data.SourceFlag == (int)SourceFlag.OneDrive
                        || data.SourceFlag == (int)SourceFlag.Teams ? data.FullPath : data.DirPath,
                    });
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while loading usage scope: {ex}");
                return new MLTrainingScopeResult()
                {
                    TrainingScopes = new List<MLTrainScopeDto>(),
                };
            }
        }

        private List<SourceFlag> GetSourceFlagByLicense()
        {
            var isSPOLicense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.SharePointOnPrem);
            var isGoogleLicense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, PaidForModule.Google);
            var isILLicense = TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, PaidForProduct.OpusIL);
            List<SourceFlag> sources = new List<SourceFlag>();
            if (isGoogleLicense)
            {
                sources.Add(SourceFlag.Google);
            }
            if (isILLicense)
            {
                sources.AddRange(new List<SourceFlag> { SourceFlag.SharePoint, SourceFlag.OneDrive, SourceFlag.Teams });
            }
            return sources;
        }

        [Audit(Module = AuditModule.MachineLearning, Category = AuditCategory.MachineLearning, Action = AuditAction.AddTrainingFileManual, BeforeHandler = typeof(MLTermBeforeAuditHandler), AfterHandler = typeof(MLTermAfterAuditHandler))]
        public async Task<RAReturnMessage> AddTrainingScopeManuallyAsync(List<MLTrainScopeDto> datas)
        {
            try
            {
                bool hasRunningJob = RMJobService.GetRunningJobsCount(JobType.MachineLearningTraining) > 0;
                if (hasRunningJob)
                {
                    RAReturnMessage message = new RAReturnMessage();
                    message.MessageType = RAMessageType.Failed;
                    message.ErrorMessage = I18NEntity.GetString("RM_ML_HasRunningJob_AddTrainingScopeManual_Message");
                    return message;
                }
                var sourceFlags = GetSourceFlagByLicense().Select(s => (int)s).ToList();
                var groupTermData = datas.GroupBy(_ => _.TermId).ToDictionary(g => g.Key, g => g.Count());
                var trainingScopeTerms = explorerDao.QueryAll(r => sourceFlags.Contains(r.SourceFlag) && groupTermData.Keys.Contains(r.TermId) && r.TrainingScope != (int)MLFileStatus.None)
                        .GroupBy(_ => _.TermId)
                        .ToDictionary(g => g.Key, g => g.Count());
                var termsCanAddTrainingData = new List<Guid>();
                foreach(var term in groupTermData)
                {
                    int value = 0;
                    if(!trainingScopeTerms.TryGetValue(term.Key, out value) || value + term.Value <= RecordsConstants.TrainingFile_MaximumNumberPerTerm)
                    {
                        termsCanAddTrainingData.Add(term.Key);
                    }
                }
                if (termsCanAddTrainingData.Count < groupTermData.Count)
                {
                    var errorTerms = groupTermData.Where(g => !termsCanAddTrainingData.Any(t => t == g.Key)).Select(g => g).ToList();
                    var errorTermsNames = datas.Where(d => errorTerms.Any(e => e.Key == d.TermId))
                        .Select(d => d.TermName)
                        .Distinct()
                        .ToList();
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = string.Format(I18NEntity.GetString("RM_ML_TS_AddTrainScopeTermErrors"), string.Join(", ", errorTermsNames))
                    };
                }
                var updateDataIds = datas.ToDictionary(g => g.Id, k => k.TermId);
                explorerDao.UpdateAll(r => updateDataIds.Keys.Contains(r.Id),
                    r =>
                    {
                        r.TrainingScope = (int)MLFileStatus.NotTrain;
                        r.TrainingAddType = (int)TrainingAddType.AddManually;
                        r.TrainingTermId = updateDataIds[r.Id];
                    });
                foreach(var term in groupTermData)
                {
                    await trainingTermDao.AddTermTrainingScopeCountValueAsync(term.Key, term.Value);
                }
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Successful,
                };
            }
            catch(Exception ex)
            {
                Logger.Error($"An error occurred while adding training scope manually: {ex}");
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_ML_TS_AddTrainScopeTermFailed")
                };
            }
        }

        [Audit(Module = AuditModule.MachineLearning, Category = AuditCategory.MachineLearning, Action = AuditAction.ChangeTrainingScopeOption, BeforeHandler = typeof(MLTermBeforeAuditHandler), AfterHandler = typeof(MLTermAfterAuditHandler))]
        public async Task<RAReturnMessage> ChangeTrainingScopeOption(MLTrainingScopeManage manage)
        {
            try
            {   
                bool hasRunningJob = RMJobService.GetRunningJobsCount(JobType.MachineLearningTraining) > 0;
                if (hasRunningJob)
                {
                    RAReturnMessage message = new RAReturnMessage();
                    message.MessageType = RAMessageType.Failed;
                    message.ErrorMessage = I18NEntity.GetString("RM_ML_HasRunningJob_ChangeScopeOption_Message");
                    return message;
                }

                if (manage != null && manage.TrainingScopeOption == (int)TrainingScopeOption.FromLocation && manage.SourceFlag == MTSSourceFlag.SPO)
                {
                    string message = await ValidSiteCollectionAndLibraryExistInCosmos(manage.Location);
                    if (!string.IsNullOrEmpty(message))
                    {
                        return new RAReturnMessage
                        {
                            MessageType = RAMessageType.Failed,
                            ErrorMessage = message
                        };
                    }
                }

                trainingModelDao.ChangeTrainingScopeOption(manage);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Successful,
                };
            }
            catch (Exception e)
            { 
                Logger.Error($"An error occurred while selecting training scope option: {e}");
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                };
            }
        }

        private async Task<string> ValidSiteCollectionAndLibraryExistInCosmos(string location)
        {
            try
            {
                var node = remoteNodeDao.GetRemoteSiteCollectionByListUrl(location);
                if (node != null)
                {

                    Record record = explorerDao.GetRecordsByContainerAndNodeType(new Guid(node.ObjectId), node.parentId, new List<int> { (int)NodeType.Web, (int)NodeType.List }, location, string.Empty, 1)?.Item1.FirstOrDefault() ?? null;
                    if (record != null && await CheckLocationHaveDataToTraining(record))
                    {
                        return string.Empty;
                    }
                }
                return I18NEntity.GetString("RM_ML_NotExistedSyncJob_ChangeScopeOption_Message");
            }
            catch(Exception e)
            {
                throw;
            }
        }

        private async Task<bool> CheckLocationHaveDataToTraining(Record record)
        {
            IExtract extractor = new Extractor();
            var support = extractor.GetAllSupportTypes();
            var query = new ExplorerQueryV3Dto()
            {
                QueryOption = new ExplorerQueryOptionV3()
                {
                    Values = new List<ExplorerSearchOptionV3>()
                            {
                                new ExplorerSearchOptionV3
                                {
                                    Column = new ExplorerQueryColumn { Id = QueryCloumnIds.SourceFlag },
                                    Value = JsonConvert.SerializeObject(GetSourceFlagByLicense())
                                },
                                new ExplorerSearchOptionV3
                                {
                                    Column = new ExplorerQueryColumn{Id = QueryCloumnIds.FileExtension},
                                    Value = JsonConvert.SerializeObject(support.ToList())
                                }
                            }
                },
                PagingInfo = new ExplorerPagingInfo
                {
                    PageIndex = string.Empty,
                    PageSize = 1,
                }
            };
            switch(record.NodeType)
            {
                case (int)RMNodeLevel.List:
                    query.QueryOption.Values.Add(new ExplorerSearchOptionV3
                    {
                        Column = new ExplorerQueryColumn { Id = QueryCloumnIds.ListId },
                        Value = record.ListId.ToString()
                    });
                    break;
                case (int)RMNodeLevel.Site:
                    {
                        var webIds = new List<string>() { record.WebId.ToString() };
                        var subSiteUnderCurrentSite = explorerDao.QueryAll(_ => _.ContainerId == record.ContainerId && _.ScopeId == record.ScopeId && _.DirPath.StartsWith(record.DirPath + "/"));
                        webIds.AddRange(subSiteUnderCurrentSite?.Select(_ => _.WebId.ToString()).ToList() ?? []);
                        query.QueryOption.Values.Add(new ExplorerSearchOptionV3
                        {
                            Column = new ExplorerQueryColumn { Id = QueryCloumnIds.WebIds },
                            Value = JsonConvert.SerializeObject(webIds)
                        });
                    }
                    break;
                default:
                    return false;
            }
            var queryResult = await ExplorerQueryService.QueryDataListWithTotalAsync(query);
            if (queryResult.Datas.Count() > 0)
                return true;
            return false;
        }

        public MLTrainingScopeManage GetTrainingScopeOption()
        {
            var manage = trainingModelDao.GetTrainingScopeOption();
            var hasGoogleLicense = TenantDao.CheckAdditionalProduct(TenantLocalValue.LogonGroupId, (long)PaidForProduct.OpusGoogle);
            var hasILLicense = TenantDao.CheckAdditionalProduct(TenantLocalValue.LogonGroupId, (long)PaidForProduct.OpusIL);
            manage.SourceFlag = (!hasGoogleLicense, !hasILLicense) switch
            {
                (true, true) => MTSSourceFlag.None,
                (true, false) => MTSSourceFlag.SPO,
                (false, true) => MTSSourceFlag.Google,
                _ => manage.SourceFlag
            };
            return manage;
        }

        [Audit(Module = AuditModule.MachineLearning, Category = AuditCategory.MachineLearning, Action = AuditAction.DeleteTrainingScopeFile, BeforeHandler = typeof(MLTermBeforeAuditHandler), AfterHandler = typeof(MLTermAfterAuditHandler))]
        public async Task<RAReturnMessage> DeleteTrainingScopeManuallyAsync(List<MLTrainScopeDto> datas)
        {
            try
            {
                var groupTerm = datas.GroupBy(_ => _.TermId).ToDictionary(_ => _.Key, _ => _.Count());
                var ids = datas.Select(_ => _.Id).ToList();
                bool hasRunningJob = RMJobService.GetRunningJobsCount(JobType.MachineLearningTraining) > 0;
                if (hasRunningJob)
                {
                    RAReturnMessage message = new RAReturnMessage();
                    message.MessageType = RAMessageType.Failed;
                    message.ErrorMessage = I18NEntity.GetString("RM_ML_HasRunningJob_DeleteTrainingScopeManual_Message");
                    return message;
                }
                explorerDao.UpdateAll(r => ids.Contains(r.Id),
                    r =>
                    {
                        r.TrainingScope = (int)MLFileStatus.None;
                        r.TrainingAddType = (int)TrainingAddType.None;
                        r.TrainingTermId = Guid.Empty;
                    });
                foreach (var term in groupTerm)
                {
                    await trainingTermDao.SubTermTrainingScopeCountValueAsync(term.Key, term.Value);
                }
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Successful,
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while delete training scope manually: {ex}");
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                };
            }
        }

        public async Task<Dictionary<string,string>> GetAllGoogleDriveName(string searchKey)
        {
            List<string> scopeIds = await explorerDao.DistinctQueryAsync(r => r.ScopeId.ToString(), r => r.SourceFlag == (int)SourceFlag.Google);
            return remoteNodeDao.GetAllGoogleDriveName(searchKey, scopeIds);
        }
    }
}
