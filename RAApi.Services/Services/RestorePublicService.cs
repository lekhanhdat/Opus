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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Api.Contract;
using AvePoint.RA.Api.Contract.Services;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.Extension;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestoreConversationType = AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object.RestoreConversationType;

namespace AvePoint.RA.Api.Services.Services
{
    public class RestorePublicService : IRestorePublicService
    {
        private const int SiteCollectionDataSource = 1;
        private const int TeamsDataSource = 3;
        private const int DefaultSearchPageIndex = 1;
        private const int DefaultSearchPageSize = 1;
        private const long MaxDeleteArchivedDataDaysAfterRestore = 365;

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RestorePublicService));

        private IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        private ICommonSiteMasterIndexDao CommonSiteMasterIndexDao => PlatformWindsorManager.GetService<ICommonSiteMasterIndexDao>();
        private static IRestoredSitesInfoDao RestoredSitesInfoDao => PlatformWindsorManager.GetService<IRestoredSitesInfoDao>();
        private IRestoreSearchService RestoreSearchService => PlatformWindsorManager.GetService<IRestoreSearchService>();
        private IJobMonitorDao JobMonitorDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
        private IAccountWrapperService AccountWrapperService => PlatformWindsorManager.GetService<IAccountWrapperService>();
        private IRMArchiverSettingDao ArchiverSettingDao => PlatformWindsorManager.GetService<IRMArchiverSettingDao>();
        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private IAuditCommonService AuditCommonService => PlatformWindsorManager.GetService<IAuditCommonService>();

        public async Task<RestoreExecutionResponse> RestoreSiteCollectionAsync(RestoreExecutionRequest request)
        {
            if (string.IsNullOrEmpty(request.Scope))
            {
                return CreateRestoreFailureResponse(I18NEntity.GetString("RM_RESTORE_PUB_ScopeRequired"), RestoreErrorType.ScopeIsRequired);
            }

            var archivedCheck = await HasArchivedSiteCollectionDataAsync(request.Scope);
            if (archivedCheck == null || !archivedCheck.Success)
            {
                return CreateRestoreFailureResponse(archivedCheck?.Message ?? I18NEntity.GetString("RM_RESTORE_PUB_ArchivedDataCheckFailed"), RestoreErrorType.UnknowError);
            }

            if (!archivedCheck.HasArchivedData)
            {
                return CreateRestoreFailureResponse(I18NEntity.GetString("RM_RESTORE_PUB_ScopeNotFound_ArchivedData"), RestoreErrorType.ScopeNotFound);
            }
            //-------------------------
            // var siteNode = await GetSiteCollectionNodeAsync(request.Scope);
            // if (siteNode == null)
            // {
            //     return CreateRestoreFailureResponse("RM_RESTORE_PUB_ScopeNotFound_ArchivedData");
            // }

            // var restoreNode = await GetRestoreNodeAsync(BuildSiteCollectionSearchRequest(siteNode), false);
            // if (restoreNode == null)
            // {
            //     return CreateRestoreFailureResponse("RM_RESTORE_PUB_ScopeArchivedNode_CannotBeRestored");
            // }
            //----------------------------------

            var specifiedUserValidation = ValidateAndResolveSpecifiedUser(request?.SiteAdministratorUserPrincipalName);
            if (specifiedUserValidation.Error != null)
            {
                return specifiedUserValidation.Error;
            }

            var restoreInfo = BuildRestoreInfo(request, SiteCollectionDataSource, specifiedUserValidation.User, false);
            var result = RestoreSearchService.SaveAndRunRestoreJob(restoreInfo, GCommon.Contract.StorageOptimization.Object.RestoreType.InPlace);
            if (result?.MessageType == RAMessageType.Successful && !string.IsNullOrWhiteSpace(result.Extension))
            {
                Logger.Info($"Public restore service queued site collection restore successfully. Scope:[{request.Scope}]. JobId:[{result.Extension}].");
                return new RestoreExecutionResponse
                {
                    JobId = result.Extension,
                    Success = true,
                    Message = string.Empty
                };
            }
            Logger.Warn($"Public restore service failed to queue site collection restore. Scope:[{request.Scope}]. Error:[{result?.ErrorMessage}].");
            return CreateRestoreFailureResponse(result?.ErrorMessage ?? I18NEntity.GetString("RM_RESTORE_PUB_RestoreJobQueueFailed"), RestoreErrorType.UnknowError);
        }

        public async Task<RestoreExecutionResponse> RestoreTeamsGroupAsync(RestoreExecutionRequest request)
        {
            if (string.IsNullOrEmpty(request.Scope))
            {
                return CreateRestoreFailureResponse(I18NEntity.GetString("RM_RESTORE_PUB_ScopeRequired"), RestoreErrorType.ScopeIsRequired);
            }

            var archivedCheck = await HasArchivedTeamsGroupDataAsync(request.Scope);
            if (archivedCheck == null || !archivedCheck.Success)
            {
                return CreateRestoreFailureResponse(archivedCheck?.Message ?? I18NEntity.GetString("RM_RESTORE_PUB_ArchivedDataCheckFailed"), RestoreErrorType.UnknowError);
            }

            if (!archivedCheck.HasArchivedData)
            {
                return CreateRestoreFailureResponse(I18NEntity.GetString("RM_RESTORE_PUB_ScopeNotFound_ArchivedData"), RestoreErrorType.ScopeNotFound);
            }

            var teamsNode = await GetTeamsNodeAsync(request.Scope);
            if (teamsNode == null)
            {
                return CreateRestoreFailureResponse(I18NEntity.GetString("RM_RESTORE_PUB_ScopeNotFound_ArchivedData"), RestoreErrorType.ScopeNotFound);
            }

            var restoreNode = await GetRestoreNodeAsync(BuildTeamsSearchRequest(teamsNode), true);
            if (restoreNode == null)
            {
                return CreateRestoreFailureResponse(I18NEntity.GetString("RM_RESTORE_PUB_ScopeArchivedNode_CannotBeRestored"), RestoreErrorType.UnknowError);
            }

            var specifiedUserValidation = ValidateAndResolveSpecifiedUser(request?.SiteAdministratorUserPrincipalName);
            if (specifiedUserValidation.Error != null)
            {
                return specifiedUserValidation.Error;
            }

            var restoreInfo = BuildRestoreInfo(request, TeamsDataSource, specifiedUserValidation.User, true, restoreNode);
            var result = RestoreSearchService.SaveAndRunTeamsRestoreJob(restoreInfo, GCommon.Contract.StorageOptimization.Object.RestoreType.InPlace);
            if (result?.MessageType == RAMessageType.Successful && !string.IsNullOrWhiteSpace(result.Extension))
            {
                Logger.Info($"Public restore service queued Teams/Group restore successfully. Scope:[{request.Scope}]. JobId:[{result.Extension}].");
                return new RestoreExecutionResponse
                {
                    JobId = result.Extension,
                    Success = true,
                    Message = string.Empty
                };
            }

            Logger.Warn($"Public restore service failed to queue Teams/Group restore. Scope:[{request.Scope}]. Error:[{result?.ErrorMessage}].");
            return CreateRestoreFailureResponse(result?.ErrorMessage ?? I18NEntity.GetString("RM_RESTORE_PUB_RestoreJobQueueFailed"), RestoreErrorType.UnknowError);
        }

        public RestoreJobStatusResponse GetRestoreJobStatus(string jobId)
        {
            if (string.IsNullOrEmpty(jobId))
            {
                return new RestoreJobStatusResponse
                {
                    Success = false,
                    ErrorType = RestoreErrorType.JobIdIsRequired,
                    Message = I18NEntity.GetString("RM_RESTORE_PUB_JobIdRequired")
                };
            }

            RMJobMonitor job = JobMonitorDao.GetJobById(jobId);
            if (job == null)
            {
                Logger.Warn($"Public restore service cannot find the restore job. JobId:[{jobId}].");
                return new RestoreJobStatusResponse
                {
                    Success = false,
                    ErrorType = RestoreErrorType.JobNotFound,
                    Message = I18NEntity.GetString("RM_RESTORE_PUB_RestoreJobNotFound")
                };
            }

            var generalSetting = GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult();

            return new RestoreJobStatusResponse
            {
                Job = new JobDto
                {
                    Id = job.Id,
                    Status = job.Status,
                    Progress = job.Progress,
                    StartTime = FormatJobStartTime(generalSetting, job.StartTime),
                    FinishTime = FormatJobFinishTime(generalSetting, job.EndTime)
                },
                Success = true,
                Message = string.Empty
            };
        }

        public async Task<RestoreCommonResponse> SetRestoreGracePeriodSiteCollection(RestoreExecutionRequest request)
        {
            const string apiName = nameof(SetRestoreGracePeriodSiteCollection);
            string validationResult = "Passed";
            string oldValue = string.Empty;
            int affectedSiteCount = 0;

            RestoreCommonResponse ReturnWithAudit(RestoreCommonResponse response)
            {
                WriteRestoreGracePeriodAudit(apiName, request, response, validationResult, oldValue, affectedSiteCount);
                return response;
            }

            if (string.IsNullOrEmpty(request?.Scope))
            {
                Logger.Error($"Skip to add or update restore site, because SiteURL is empty.");
                validationResult = I18NEntity.GetString("RM_RESTORE_PUB_ScopeRequired");
                return ReturnWithAudit(new RestoreCommonResponse
                {
                    Success = false,
                    ErrorType = RestoreErrorType.ScopeIsRequired,
                    Message = I18NEntity.GetString("RM_RESTORE_PUB_ScopeRequired")
                });
            }

            var siteGracePeriodValidation = ValidateRestoreGracePeriodRequest(request, request.Scope);
            if (siteGracePeriodValidation != null)
            {
                validationResult = siteGracePeriodValidation.Message;
                return ReturnWithAudit(siteGracePeriodValidation);
            }

            var deleteArchivedDataDaysAfterRestore = request.DeleteArchivedDataDaysAfterRestore.Value;
            oldValue = GetRestoreGracePeriodOldValue(request.Scope);

            if (!IsDeleteArchivedDataEnabledForSite(request.Scope))
            {
                Logger.Warn($"Skip to set restore grace period because delete restored data is not enabled for the scope. Scope:[{request.Scope}].");
                validationResult = I18NEntity.GetString("RM_RESTORE_PUB_DeleteArchivedDataFeatureDisabled");
                return ReturnWithAudit(CreateFailureCommonResponse(I18NEntity.GetString("RM_RESTORE_PUB_DeleteArchivedDataFeatureDisabled"), RestoreErrorType.DoNotHavePermission));
            }

            if (!ArchiverSiteMasterIndexDao.ExistsRestoringSiteCollectionByUrl(request.Scope))
            {
                Logger.Warn($"Skip to add or update restore site, because the site is not in restoring state. Scope:[{request.Scope}].");
                validationResult = I18NEntity.GetString("RM_RESTORE_PUB_ScopeNotInRestoringState");
                return ReturnWithAudit(new RestoreCommonResponse
                {
                    Success = false,
                    ErrorType = RestoreErrorType.ScopeNotFound,
                    Message = I18NEntity.GetString("RM_RESTORE_PUB_ScopeNotInRestoringState")
                });
            }

            SaveRestoreGracePeriodBySiteUrl(request.Scope, deleteArchivedDataDaysAfterRestore);
            affectedSiteCount = 1;

            return ReturnWithAudit(new RestoreCommonResponse
            {
                Success = true,
                Message = string.Empty
            });
        }

        public async Task<RestoreCommonResponse> SetRestoreGracePeriodTeamsGroup(RestoreExecutionRequest request)
        {
            const string apiName = nameof(SetRestoreGracePeriodTeamsGroup);
            string validationResult = "Passed";
            string oldValue = string.Empty;
            int affectedSiteCount = 0;

            RestoreCommonResponse ReturnWithAudit(RestoreCommonResponse response)
            {
                WriteRestoreGracePeriodAudit(apiName, request, response, validationResult, oldValue, affectedSiteCount);
                return response;
            }

            if (string.IsNullOrWhiteSpace(request.Scope))
            {
                Logger.Error("Skip to add or update restore sites for Teams/Group, because groupMailbox is empty.");
                validationResult = I18NEntity.GetString("RM_RESTORE_PUB_ScopeRequired");
                return ReturnWithAudit(new RestoreCommonResponse
                {
                    Success = false,
                    ErrorType = RestoreErrorType.ScopeIsRequired,
                    Message = I18NEntity.GetString("RM_RESTORE_PUB_ScopeRequired")
                });
            }

            var teamsGracePeriodValidation = ValidateRestoreGracePeriodRequest(request, request.Scope);
            if (teamsGracePeriodValidation != null)
            {
                validationResult = teamsGracePeriodValidation.Message;
                return ReturnWithAudit(teamsGracePeriodValidation);
            }

            var deleteArchivedDataDaysAfterRestore = request.DeleteArchivedDataDaysAfterRestore.Value;

            if (!CommonSiteMasterIndexDao.ExistsTeamsGroupIndex())
            {
                Logger.Warn($"Skip to add or update restore sites for Teams/Group, because no Teams/Group index is found. Scope:[{request.Scope}].");
                validationResult = I18NEntity.GetString("RM_RESTORE_PUB_TeamsGroupIndexNotFound");
                return ReturnWithAudit(new RestoreCommonResponse
                {
                    Success = false,
                    ErrorType = RestoreErrorType.ScopeNotFound,
                    Message = I18NEntity.GetString("RM_RESTORE_PUB_TeamsGroupIndexNotFound")
                });
            }

            var allRelatedSiteUrls = GetAllRelatedTeamsSiteUrls(request.Scope);
            if (allRelatedSiteUrls.Count == 0)
            {
                Logger.Warn($"Skip to add or update restore sites for Teams/Group, because no related sites are found. Scope:[{request.Scope}].");
                validationResult = I18NEntity.GetString("RM_RESTORE_PUB_RelatedSitesNotFound");
                return ReturnWithAudit(new RestoreCommonResponse
                {
                    Success = false,
                    ErrorType = RestoreErrorType.ScopeNotFound,
                    Message = I18NEntity.GetString("RM_RESTORE_PUB_RelatedSitesNotFound")
                });
            }

            oldValue = GetRestoreGracePeriodOldValueForTeams(allRelatedSiteUrls);

            var sitesWithoutDeleteArchivedData = allRelatedSiteUrls.Where(siteUrl => !IsDeleteArchivedDataEnabledForSite(siteUrl)).ToList();
            if (sitesWithoutDeleteArchivedData.Count > 0)
            {
                Logger.Warn($"Skip to add or update restore sites for Teams/Group, because delete restored archived data is not enabled for one or more related sites. Scope:[{request.Scope}], Sites:[{string.Join(",", sitesWithoutDeleteArchivedData)}].");
                validationResult = I18NEntity.GetString("RM_RESTORE_PUB_DeleteArchivedDataFeatureDisabled");
                return ReturnWithAudit(CreateFailureCommonResponse(I18NEntity.GetString("RM_RESTORE_PUB_DeleteArchivedDataFeatureDisabled"), RestoreErrorType.DoNotHavePermission));
            }

            foreach (var siteUrl in allRelatedSiteUrls)
            {
                SaveRestoreGracePeriodBySiteUrl(siteUrl, deleteArchivedDataDaysAfterRestore);
            }

            Logger.Info($"Saved restore grace period for Teams/Group related sites successfully. Scope:[{request.Scope}], SiteCount:[{allRelatedSiteUrls.Count}], DayNum:[{deleteArchivedDataDaysAfterRestore}].");
            affectedSiteCount = allRelatedSiteUrls.Count;
            return ReturnWithAudit(new RestoreCommonResponse
            {
                Success = true,
                Message = string.Empty
            });
        }

        private void WriteRestoreGracePeriodAudit(string apiName, RestoreExecutionRequest request, RestoreCommonResponse response, string validationResult, string oldValue, int affectedSiteCount)
        {
            try
            {
                var auditAction = apiName == nameof(SetRestoreGracePeriodSiteCollection)
                    ? AuditAction.SetRestoreGracePeriodSiteCollectionApi
                    : AuditAction.SetRestoreGracePeriodTeamsGroupApi;
                var newValue = request?.DeleteArchivedDataDaysAfterRestore?.ToString() ?? string.Empty;
                var audit = new RMAuditInfo
                {
                    Module = AuditModule.RestoreCenter,
                    Category = AuditCategory.RestoreCenter,
                    Action = auditAction,
                    Status = response?.Success == true ? (int)AuditStatus.Successful : (int)AuditStatus.Failed,
                    ExecuteOn = DateTime.UtcNow,
                    Object = request?.Scope ?? string.Empty,
                    UserName = !string.IsNullOrWhiteSpace(TenantLocalValue.ClientName) ? TenantLocalValue.ClientName : TenantLocalValue.PartnerUser,
                    Role = "Administrator",
                    ModifyContent = new List<AuditItem>
                    {
                        new AuditItem
                        {
                            Id = Guid.NewGuid(),
                            TargetSetting = "RestoreGracePeriod_DeleteArchivedDataDaysAfterRestore",
                            OldValue = oldValue,
                            NewValue = newValue
                        }
                    }
                };

                AuditCommonService.AddAudits(new List<RMAuditInfo> { audit });
            }
            catch (Exception exception)
            {
                Logger.Warn($"Failed to write restore grace period admin audit. Api:[{apiName}], Scope:[{request?.Scope}], Exception:[{exception}].");
            }
        }

        private string GetRestoreGracePeriodOldValue(string siteUrl)
        {
            if (string.IsNullOrWhiteSpace(siteUrl))
            {
                return string.Empty;
            }

            try
            {
                var info = RestoredSitesInfoDao.GetInfoByUrl(siteUrl);
                return GetOldDayNumFromRestoredSiteInfo(info);
            }
            catch (Exception exception)
            {
                Logger.Warn($"Failed to load old restore grace period value. Scope:[{siteUrl}], Exception:[{exception}].");
                return string.Empty;
            }
        }

        private string GetRestoreGracePeriodOldValueForTeams(List<string> siteUrls)
        {
            if (siteUrls == null || siteUrls.Count == 0)
            {
                return string.Empty;
            }

            var values = new List<string>();
            foreach (var siteUrl in siteUrls)
            {
                var value = GetRestoreGracePeriodOldValue(siteUrl);
                values.Add(string.IsNullOrWhiteSpace(value) ? "Empty" : value);
            }

            var distinctValues = values.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (distinctValues.Count == 1)
            {
                return distinctValues[0] == "Empty" ? string.Empty : distinctValues[0];
            }

            return string.Join(",", distinctValues);
        }

        private string GetOldDayNumFromRestoredSiteInfo(RestoredSitesInfo restoredSiteInfo)
        {
            if (restoredSiteInfo == null || string.IsNullOrWhiteSpace(restoredSiteInfo.DeleteRestoredArchivedDataSettings))
            {
                return string.Empty;
            }

            try
            {
                var settings = SerializerHelper.DeserializeByDataContractSerializer<DeleteRestoredArchivedDataSettings>(restoredSiteInfo.DeleteRestoredArchivedDataSettings);
                return settings?.DayNum.ToString() ?? string.Empty;
            }
            catch (Exception exception)
            {
                Logger.Warn($"Failed to parse old restore grace period value. RestoredSiteInfoId:[{restoredSiteInfo.Id}], Exception:[{exception}].");
                return string.Empty;
            }
        }

        private void SaveRestoreGracePeriodBySiteUrl(string siteUrl, long dayNum)
        {
            var restoredSiteInfo = RestoredSitesInfoDao.GetInfoByUrl(siteUrl);
            var deleteRestoredArchivedDataSettings = new DeleteRestoredArchivedDataSettings
            {
                DayNum = dayNum
            };
            var deleteRestoredArchivedDataSettingsString = SerializerHelper.SerializeByDataContractSerializer(deleteRestoredArchivedDataSettings);

            if (restoredSiteInfo != null)
            {
                Logger.Warn($"user modification the DayNum of RestoredSitesInfo Id:[{restoredSiteInfo.Id}] to [{dayNum}].");
                restoredSiteInfo.DeleteRestoredArchivedDataSettings = deleteRestoredArchivedDataSettingsString;
                RestoredSitesInfoDao.Update(restoredSiteInfo, info => info.DeleteRestoredArchivedDataSettings);
            }
            else
            {
                var newId = Guid.NewGuid();
                Logger.Warn($"The user added a record with ID:[{newId}] and DayNum:[{dayNum}].");
                var insertRestoredSiteInfo = new RestoredSitesInfo();
                insertRestoredSiteInfo.Id = newId;
                insertRestoredSiteInfo.SiteUrl = siteUrl;
                insertRestoredSiteInfo.DeleteRestoredArchivedDataSettings = deleteRestoredArchivedDataSettingsString;
                RestoredSitesInfoDao.Create(insertRestoredSiteInfo);
            }
        }

        private RestoreCommonResponse ValidateRestoreGracePeriodRequest(RestoreExecutionRequest request, string scope)
        {
            if (request == null)
            {
                Logger.Warn("Skip to set restore grace period because request is null.");
                return CreateFailureCommonResponse(I18NEntity.GetString("RM_RESTORE_PUB_ScopeRequired"), RestoreErrorType.ScopeIsRequired);
            }

            if (!request.DeleteArchivedDataDaysAfterRestore.HasValue)
            {
                Logger.Warn($"Skip to set restore grace period because DeleteArchivedDataDaysAfterRestore is missing. Scope:[{scope}].");
                return CreateFailureCommonResponse(I18NEntity.GetString("RM_RESTORE_PUB_DeleteArchivedDataDaysRequired"), RestoreErrorType.UnvalidDeleteArchivedData);
            }

            if (request.DeleteArchivedDataDaysAfterRestore.Value < 0)
            {
                Logger.Warn($"Skip to set restore grace period because DeleteArchivedDataDaysAfterRestore is negative. Scope:[{scope}], DayNum:[{request.DeleteArchivedDataDaysAfterRestore}].");
                return CreateFailureCommonResponse(I18NEntity.GetString("RM_RESTORE_PUB_DeleteArchivedDataDaysNegative"), RestoreErrorType.UnvalidDeleteArchivedData);
            }

            if (request.DeleteArchivedDataDaysAfterRestore.Value > MaxDeleteArchivedDataDaysAfterRestore)
            {
                Logger.Warn($"Skip to set restore grace period because DeleteArchivedDataDaysAfterRestore exceeds the limit. Scope:[{scope}], DayNum:[{request.DeleteArchivedDataDaysAfterRestore}].");
                return CreateFailureCommonResponse(I18NEntity.GetString("RM_RESTORE_PUB_DeleteArchivedDataDaysLimit"), RestoreErrorType.UnvalidDeleteArchivedData);
            }

            if (!LicenseHelperService.IsEnableDeleteRestoreDataFeature())
            {
                Logger.Warn($"Skip to set restore grace period because delete restored data feature is disabled. Scope:[{scope}].");
                return CreateFailureCommonResponse(I18NEntity.GetString("RM_RESTORE_PUB_DeleteArchivedDataFeatureDisabled"), RestoreErrorType.DoNotHavePermission);
            }

            return null;
        }

        private bool IsDeleteArchivedDataEnabledForSite(string siteUrl)
        {
            if (string.IsNullOrWhiteSpace(siteUrl))
            {
                return false;
            }

            try
            {
                var archiverSetting = ArchiverSettingDao.LoadSiteArchiverSettingByUrl(siteUrl);
                if (archiverSetting == null || string.IsNullOrWhiteSpace(archiverSetting.CleanRestoredOption))
                {
                    return false;
                }

                var cleanRestoredOption = SerializerHelper.DeserializeByDataContractSerializer<CleanRestoredItemsExtension>(archiverSetting.CleanRestoredOption);
                return cleanRestoredOption?.EnableDelArchivedData == true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to validate delete restored data setting. SiteUrl:[{siteUrl}], Error:[{ex}].");
                return false;
            }
        }

        private List<string> GetAllRelatedTeamsSiteUrls(string groupMailbox)
        {
            var allRelatedSiteUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var allIndex = CommonSiteMasterIndexDao.GetAllSiteCollectionNodsInfoByUrl(groupMailbox);

            foreach (var index in allIndex)
            {
                if (string.IsNullOrWhiteSpace(index?.Extension))
                {
                    continue;
                }

                try
                {
                    var extObj = SerializerHelper.DeserializeByDataContractSerializer<ArchiverGroupSiteMasterIndexExtension>(index.Extension);
                    if (string.IsNullOrWhiteSpace(extObj?.SPGroupSiteURL))
                    {
                        continue;
                    }

                    var normalizedGroupSiteUrl = NormalizeSiteUrl(extObj.SPGroupSiteURL);
                    allRelatedSiteUrls.Add(normalizedGroupSiteUrl);

                    var rootSiteUrl = new Uri(extObj.SPGroupSiteURL).GetLeftPart(UriPartial.Authority);
                    if (extObj.ChannelSiteRelativeURLs != null)
                    {
                        foreach (var relativeUrl in extObj.ChannelSiteRelativeURLs)
                        {
                            if (string.IsNullOrWhiteSpace(relativeUrl))
                            {
                                continue;
                            }

                            allRelatedSiteUrls.Add(NormalizeSiteUrl(rootSiteUrl + relativeUrl));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to deserialize ArchiverGroupSiteMasterIndexExtension. GroupMailbox:[{groupMailbox}], IndexId:[{index?.Id}], Error:[{ex.Message}].");
                }
            }

            return allRelatedSiteUrls.ToList();
        }

        public async Task<RestoreArchivedDataCheckResponse> HasArchivedSiteCollectionDataAsync(string scope)
        {
            if (string.IsNullOrWhiteSpace(scope))
            {
                return CreateArchivedDataCheckFailure(scope, I18NEntity.GetString("RM_RESTORE_PUB_ScopeRequired"), RestoreErrorType.ScopeIsRequired);
            }

            if (!ArchiverSiteMasterIndexDao.ExistsRestoringSiteCollectionByUrl(scope))
            {
                return CreateArchivedDataCheckFailure(scope, I18NEntity.GetString("RM_RESTORE_PUB_ArchivedDataNotFound"), RestoreErrorType.ScopeNotFound);
            }
            return new RestoreArchivedDataCheckResponse
            {
                Scope = scope,
                HasArchivedData = true,
                Success = true,
                Message = string.Empty
            };
        }

        public async Task<RestoreArchivedDataCheckResponse> HasArchivedTeamsGroupDataAsync(string scope)
        {
            if (string.IsNullOrWhiteSpace(scope))
            {
                return CreateArchivedDataCheckFailure(scope, I18NEntity.GetString("RM_RESTORE_PUB_ScopeRequired"), RestoreErrorType.ScopeIsRequired);
            }

            List<CommonSiteMasterIndex> index = CommonSiteMasterIndexDao.GetAllSiteCollectionNodsInfoByUrl(scope);
            if (index == null || index.Count == 0)
            {
                return CreateArchivedDataCheckFailure(scope, I18NEntity.GetString("RM_RESTORE_PUB_ArchivedDataNotFound"), RestoreErrorType.ScopeNotFound);
            }

            return new RestoreArchivedDataCheckResponse
            {
                Scope = scope,
                HasArchivedData = true,
                Success = true,
                Message = string.Empty
            };
        }

        private async Task<SiteCollectionNodesInfo> GetSiteCollectionNodeAsync(string siteUrl)
        {
            var nodes = await RestoreSearchService.GetSiteCollectionNodesByUrlAsync(siteUrl);
            return nodes?.FirstOrDefault(node => IsSameSiteUrl(node?.SiteUrl, siteUrl));
        }

        private async Task<SiteCollectionNodesInfo> GetTeamsNodeAsync(string siteUrl)
        {
            var normalizedSiteUrl = NormalizeSiteUrl(siteUrl);
            if (string.IsNullOrWhiteSpace(normalizedSiteUrl))
            {
                return null;
            }

            var teamsIndex = CommonSiteMasterIndexDao
                .GetAllSiteCollectionNodsInfoByUrl(normalizedSiteUrl)
                ?.FirstOrDefault();

            if (teamsIndex == null)
            {
                return null;
            }

            return await Task.FromResult(new SiteCollectionNodesInfo
            {
                MasterIndexId = teamsIndex.Id,
                SiteUrl = teamsIndex.SiteURL,
                SiteGroupId = teamsIndex.SiteGroupId,
                SPObjectId = teamsIndex.SiteId,
                TeamsId = teamsIndex.TeamId,
                PermissionLevel = (int)FunctionSubPermission.RestoreCenterFullControl
            });
        }

        private async Task<ArchiverRestoreSerchResult> GetRestoreNodeAsync(ArchiverRestoreResult searchRequest, bool isTeams)
        {
            if (searchRequest == null)
            {
                return null;
            }

            var searchResult = isTeams
                ? await RestoreSearchService.GetSearchTeamsTreeResultAsync(searchRequest, false)
                : await RestoreSearchService.GetSearchTreeResultAsync(searchRequest);

            return searchResult?.RestoreSerchNodes?.FirstOrDefault(node => node != null && !string.IsNullOrWhiteSpace(node.TreeNode));
        }

        private static ArchiverRestoreResult BuildSiteCollectionSearchRequest(SiteCollectionNodesInfo searchNode)
        {
            return BuildSearchRequest(searchNode, SiteCollectionDataSource);
        }

        private static ArchiverRestoreResult BuildTeamsSearchRequest(SiteCollectionNodesInfo searchNode)
        {
            return BuildSearchRequest(searchNode, TeamsDataSource);
        }

        private static ArchiverRestoreResult BuildSearchRequest(SiteCollectionNodesInfo searchNode, int dataSource)
        {
            if (searchNode == null)
            {
                return null;
            }

            return new ArchiverRestoreResult
            {
                PageIndex = DefaultSearchPageIndex,
                PageSize = DefaultSearchPageSize,
                SerchContract = new BackupDataSearchContract
                {
                    SearchNode = searchNode,
                    FilterPolicy = new ArchiverRestoreFilter
                    {
                        FilterDeleteType = FilterDeletedType.All,
                        DataSource = dataSource,
                        Level = PolicyLevel.SiteCollection,
                        FilterName = string.Empty
                    }
                }
            };
        }

        private RestoreInfo BuildRestoreInfo(RestoreExecutionRequest request, int dataSource, ToExportUserInfo specifiedUser, bool isTeams, ArchiverRestoreSerchResult restoreNode = null)
        {
            var restoreOption = (RestoreOption)request.ConflictResolution;
            var appsRestoreOption = (RestoreOption)request.AppsConflictResolution;
            var restoreVersionOption = (RestoreDocumentVersionsOption)request.RestoreVersionOption;
            var jobPriority = (JobPriority)request.Priority;
            var specifyUsers = specifiedUser == null ? new List<ToExportUserInfo>() : new List<ToExportUserInfo> { specifiedUser };

            var restoreInfo = new RestoreInfo
            {
                DataSource = dataSource,
                IsEndUserJob = true,
                RestoreTypeSelect = GCommon.Contract.Server.Common.BackupDataSearch.RestoreType.InPlace,
                RestoreOption = restoreOption,
                RestoreAPPOption = appsRestoreOption,
                IsSpecifyUser = specifiedUser != null,
                SpecifyUserList = specifyUsers,
                IncludeWorkflowDefinition = request.IncludeWorkflowDefinition,
                IncludeSharingLink = request.IncludeSharingLink,
                IsSkipRestoreConversation = request.RestoreConversationType == (int)RestoreConversationType.Skip || request.IsSkipRestoreConversation,
                RestoreConversationType = (RestoreConversationType)request.RestoreConversationType,
                RestoreVersionOption = restoreVersionOption,
                KeepVersionsNumber = restoreVersionOption == RestoreDocumentVersionsOption.AllVersions ? 1 : request.KeepVersionsNumber,
                JobPriority = jobPriority,
                IsPublicRestoreApiRequest = request?.IsPublicRestoreApiRequest == true,
                IsSupportLockedSite = request?.IsSupportLockedSite == true,
            };
            if (restoreNode != null)
            {
                restoreInfo.NodeObjects = new List<ArchiverRestoreSerchResult> { restoreNode };
            }
            if (!isTeams)
            {
                restoreInfo.RestoreExecutionRequest = request;
            }
            return restoreInfo;
        }

        private (ToExportUserInfo User, RestoreExecutionResponse Error) ValidateAndResolveSpecifiedUser(string userPrincipalName)
        {
            if (string.IsNullOrWhiteSpace(userPrincipalName))
            {
                return (null, null);
            }

            var normalizedUpn = userPrincipalName.Trim();
            var matchedAccount = AccountWrapperService
                .SearchAccounts(TenantLocalValue.LogonGroupId, normalizedUpn, 20, true)
                .FirstOrDefault(account =>
                    account != null
                    && account.InviteType == AvePoint.RA.Contract.Object.AccountType.User
                    && (
                        string.Equals(account.UserPrincipalName, normalizedUpn, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(account.Mail, normalizedUpn, StringComparison.OrdinalIgnoreCase)
                    ));

            if (matchedAccount == null)
            {
                return (null, CreateRestoreFailureResponse(I18NEntity.GetString("RM_RESTORE_PUB_SpecifiedSiteAdministratorNotFound"), RestoreErrorType.UserNotFound));
            }

            var resolvedUser = new ToExportUserInfo
            {
                UserId = string.IsNullOrWhiteSpace(matchedAccount.AccountId) ? matchedAccount.Id : matchedAccount.AccountId,
                Id = matchedAccount.Id,
                UserName = matchedAccount.DisplayName,
                UserPrincipalName = matchedAccount.UserPrincipalName ?? matchedAccount.Mail,
                Email = matchedAccount.Mail ?? matchedAccount.UserPrincipalName,
                DisplayName = matchedAccount.DisplayName,
                InviteType = AvePoint.GCommon.Contract.Server.Common.BackupDataSearch.AccountType.User,
                SurName = matchedAccount.SurName,
                GivenName = matchedAccount.GivenName,
                TenantId = matchedAccount.TenantId
            };

            return (resolvedUser, null);
        }

        // private static RestoreExecutionResponse ValidateRestoreRequest(RestoreExecutionRequest request)
        // {
        //     if (request == null)
        //     {
        //         return CreateRestoreFailureResponse("InvalidRequest", "The request body is required.");
        //     }

        //     if (string.IsNullOrWhiteSpace(request.SiteUrl))
        //     {
        //         return CreateRestoreFailureResponse("InvalidSiteUrl", "The siteUrl is required.");
        //     }

        //     if (!Enum.IsDefined(typeof(RestoreOption), request.ConflictResolution))
        //     {
        //         return CreateRestoreFailureResponse("InvalidConflictResolution", "The conflict resolution is invalid.");
        //     }

        //     if (!Enum.IsDefined(typeof(RestoreOption), request.AppsConflictResolution))
        //     {
        //         return CreateRestoreFailureResponse("InvalidAppsConflictResolution", "The apps conflict resolution is invalid.");
        //     }

        //     if (!Enum.IsDefined(typeof(RestoreDocumentVersionsOption), request.RestoreVersionOption))
        //     {
        //         return CreateRestoreFailureResponse("InvalidRestoreVersionOption", "The restore version option is invalid.");
        //     }

        //     if (!Enum.IsDefined(typeof(JobPriority), request.Priority))
        //     {
        //         return CreateRestoreFailureResponse("InvalidPriority", "The priority is invalid.");
        //     }

        //     if ((RestoreDocumentVersionsOption)request.RestoreVersionOption == RestoreDocumentVersionsOption.SpecifyVersions &&
        //         (request.KeepVersionsNumber <= 0 || request.KeepVersionsNumber > MaxKeepVersionsNumber))
        //     {
        //         return CreateRestoreFailureResponse("InvalidKeepVersionsNumber", $"The keepVersionsNumber must be between 1 and {MaxKeepVersionsNumber} when specifying versions.");
        //     }

        //     return null;
        // }

        private static RestoreExecutionResponse CreateRestoreFailureResponse(string message, RestoreErrorType errorType = RestoreErrorType.None)
        {
            return new RestoreExecutionResponse
            {
                Success = false,
                Message = message,
                ErrorType = errorType 
            };
        }

        private static RestoreArchivedDataCheckResponse CreateArchivedDataCheckFailure(string siteUrl, string message, RestoreErrorType errorType = RestoreErrorType.None) 
        {
            return new RestoreArchivedDataCheckResponse
            {
                Scope = siteUrl,
                Success = false,
                ErrorType = errorType,
                Message = message
            };
        }

        private static RestoreCommonResponse CreateSuccessCommonResponse()
        {
            return new RestoreCommonResponse
            {
                Success = true,
                Message = string.Empty
            };
        }

        private static RestoreCommonResponse CreateFailureCommonResponse(string message, RestoreErrorType errorType = RestoreErrorType.None)
        {
            return new RestoreCommonResponse
            {
                Success = false,
                Message = message,
                ErrorType = errorType
            };
        }

        private static bool IsSameSiteUrl(string left, string right)
        {
            return string.Equals(NormalizeSiteUrl(left), NormalizeSiteUrl(right), StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeSiteUrl(string siteUrl)
        {
            return string.IsNullOrWhiteSpace(siteUrl) ? string.Empty : siteUrl.Trim().TrimEnd('/');
        }

        private string FormatJobStartTime(AvePoint.RA.Contract.RMWeb.CP.GeneralSettingModel generalSetting, long ticks)
        {
            return ticks == 0 ? string.Empty : GeneralSettingService.ConvertTiksToDateTime(generalSetting, ticks, true).SimplifyFormatTime;
        }

        private string FormatJobFinishTime(AvePoint.RA.Contract.RMWeb.CP.GeneralSettingModel generalSetting, long ticks)
        {
            return ticks == 0
                ? I18NEntity.GetString("RM_JS_JM_EndTimePending")
                : GeneralSettingService.ConvertTiksToDateTime(generalSetting, ticks, true).SimplifyFormatTime;
        }
    }
}
