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
using AvePoint.Api.Contract;
using AvePoint.Common.RemoteNode.Impl;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Archiver;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Restore;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Stub;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.Service.Services;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Audit;
using AvePoint.RA.Service.Services.JobQueue;
using AvePoint.RA.Service.Services.Settings.AuditHandler;
using AvePoint.StorageOptimization.Archiver.Service.Impl;
//using global::GCommon.CryptoUtility.Cryptography;
using DocAveOnline.WebApi.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace AvePoint.Common.Api.Services
{
    public interface IArchiverService
    {
        Task<JobResult> RunArchiverEndUserRestoreJobAsync(EndUserRestoreConfig config);
        Task<StubParseResult> ParseStubStringAsync(string stubString, string user, string tenantId);
        JobResult RunArchiverContentDownloadJob(ArchivedContentRestoreConfig config);
        JobResult RunArchivedContentExportJob(ExportArchivedContentConfig config);
        JobResult RunExportSearchResultJob(EndUserRestoreConfig config);
        Task<EndUserRestoreSettingResult> GetEndUserSettingAsync();
    }
    [Audit]
    public class ArchiverService : RMServiceBase, IArchiverService
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(ArchiverService));
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        public IMArchiverService DaoArchiverService { get => new MArchiverService(); set { } }
        //public ICommonService CommonService { get; set; }
        public IMArchiverRestoreService RestoreService { get; set; }
        private RA.Contract.RMWeb.ArchiverRestore.IRestoreSearchService RestoreSearchService => PlatformWindsorManager.GetService<RA.Contract.RMWeb.ArchiverRestore.IRestoreSearchService>();
        public IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexService => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        public GCommon.Contract.Server.Common.RemoteNode.IRemoteNodeService RemoteNodeService { get => new RemoteNodeService(); set { } }
        private IRMRestoreSiteMappingDao RMRestoreSiteMappingDao => PlatformWindsorManager.GetService<IRMRestoreSiteMappingDao>();
        private IRMRestoreSiteMappingDao RestoreSiteMappingDao => PlatformWindsorManager.GetService<IRMRestoreSiteMappingDao>();
        public IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        [Audit(Module = AuditModule.RestoreCenter, Category = AuditCategory.RestoreCenter, Action = AuditAction.RunArchiverRestoreJob, AfterHandler = typeof(NewArchiverJobAfterAuditHandler))]
        public async Task<JobResult> RunArchiverEndUserRestoreJobAsync(EndUserRestoreConfig config)
        {
            logger.Info($"RunArchiverEndUserRestoreJob.RunJobUser:{config.Office365User}.ModuleType:{config.ModuleType}.");
            JobResult jobResult = new JobResult();
            try
            {
                //经与Recenter确认:Run Restore Job首先check DAO Setting，如果不允许Restore & Export，则直接返回Error Code.
                //1.Allow end users to restore/export archived data,总开关直接关闭时，直接返回Error Code
                //2.各个Source的 restore/export Setting，单独判断其打开关闭，如果不允许Restore & Export，则直接返回Error Code.
                #region Check DAO End User Restore Setting
                var endUserRestoreSetting = await DaoArchiverService.GetEndUserRestoreSettingAsync();
                if (!endUserRestoreSetting.IsAllowRestore)
                {
                    jobResult.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportTotalError;
                    logger.Error("RunArchiverEndUserRestoreJob:DAODoesNotAllowUserRestoreAndExportTotalError:IsAllowRestore[False].");
                    return jobResult;
                }
                switch (config.ModuleType)
                {
                    case DocAveOnline.WebApi.Contracts.ModuleType.None:
                        if (!endUserRestoreSetting.PermissionSetting.IsRestoreStubLink)
                        {
                            jobResult.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportServiceError;
                            logger.Error("RunArchiverEndUserRestoreJob:DAODoesNotAllowUserRestoreOrExportServiceError:IsRestoreStubLink[False].");
                            return jobResult;
                        }
                        break;
                    case DocAveOnline.WebApi.Contracts.ModuleType.SharePointOnline:
                        if (!endUserRestoreSetting.PermissionSetting.IsRestoreSiteCollection)
                        {
                            jobResult.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportServiceError;
                            logger.Error("RunArchiverEndUserRestoreJob:DAODoesNotAllowUserRestoreOrExportServiceError.IsRestoreSiteCollection[False]");
                            return jobResult;
                        }
                        break;
                    case DocAveOnline.WebApi.Contracts.ModuleType.Microsoft365Groups:
                    case DocAveOnline.WebApi.Contracts.ModuleType.MicrosoftTeams:
                        if (!endUserRestoreSetting.PermissionSetting.IsRestoreGroupTeamSite)
                        {
                            jobResult.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportServiceError;
                            logger.Error("RunArchiverEndUserRestoreJob:DAODoesNotAllowUserRestoreOrExportServiceError.IsRestoreGroupTeamSite[False]");
                            return jobResult;
                        }
                        break;
                    case DocAveOnline.WebApi.Contracts.ModuleType.OneDriveForBusiness:
                    default:
                        jobResult.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportServiceError;
                        logger.Error($"RunArchiverEndUserRestoreJob:DAODoesNotAllowUserRestoreOrExportServiceError.ModuleType[Error].ModuleType:{config.ModuleType}.");
                        return jobResult;
                }
                #endregion

                EndUserRestoreJobConfig endUserRestoreJobConfig = new EndUserRestoreJobConfig();

                endUserRestoreJobConfig.RunJobUser = config.Office365User;
                endUserRestoreJobConfig.IntegrationModule = ArchiveIntegrationModules.Recenter;
                endUserRestoreJobConfig.Items = new List<AdvanceSearchResult>();
                endUserRestoreJobConfig.OopStubUrl = config.OopStubUrl;
                endUserRestoreJobConfig.O365TenantId = config.Office365TenantID;
                if (!string.IsNullOrEmpty(config.OopStubUrl))
                {
                    if(endUserRestoreSetting.PermissionSetting?.StubOopRestoreSetting != null && !endUserRestoreSetting.PermissionSetting.StubOopRestoreSetting.IsEnableStubOopRestore)
                    {
                        jobResult.ErrorCode = ErrorCode.DAODoesNotAllowUserStubOopError;
                        logger.Error($"RunArchiverEndUserRestoreJob:DAODoesNotAllowUserStubOopError.ModuleType[Error].");
                        return jobResult;
                    }

                    endUserRestoreJobConfig.RestoreType = RestoreType.StubOop;
                }
                else
                {
                    endUserRestoreJobConfig.RestoreType = RestoreType.InPlace;
                }

                if (config.StubJobInfo != null)
                {
                    logger.Info($"RunArchiverEndUserRestoreJob  StubJobInfo : BackUpJobId:{config.StubJobInfo.BackUpJobId},PathMD5:{config.StubJobInfo.AdvanceSearchResult.PathMD5} site url:{config.StubJobInfo.SiteUrl}");
                    AdvanceSearchResult item = new AdvanceSearchResult();
                    item.FullPath = config.StubJobInfo.AdvanceSearchResult.FullPath;
                    item.Name = config.StubJobInfo.AdvanceSearchResult.Name;
                    item.PathMD5 = config.StubJobInfo.AdvanceSearchResult.PathMD5;
                    endUserRestoreJobConfig.SiteUrl = config.StubJobInfo.SiteUrl;
                    endUserRestoreJobConfig.StubType = config.StubJobInfo.StubType;
                    endUserRestoreJobConfig.Items.Add(item);
                    endUserRestoreJobConfig.PermissionCheckType = CheckPermissionType.StubRestoreLink;
                    endUserRestoreJobConfig.BackUpJobId = config.StubJobInfo.BackUpJobId;
                    if (endUserRestoreJobConfig.RestoreType == RestoreType.StubOop)
                    {
                        logger.Info($"RunArchiverEndUserRestoreJob StubJobInfo before url decode: OopStubUrl:{config.OopStubUrl}");
                        string checkUrl = config.OopStubUrl;
                        if (checkUrl.Contains("/"))
                        {
                            string stubName = checkUrl.Substring(checkUrl.LastIndexOf("/") + 1);
                            if (stubName.Contains("."))
                            {
                                string fileName = stubName.Substring(0, stubName.LastIndexOf("."));
                                string decodeFileName = string.IsNullOrEmpty(fileName) ? string.Empty : Uri.UnescapeDataString(fileName);
                                if (decodeFileName != item.Name)
                                {
                                    logger.Warn($"this oop stub url is not current stub file,so return,undecode fileName:{fileName},uri decode fileName :{decodeFileName},item.Name:{item.Name}");
                                    jobResult.ErrorCode = ErrorCode.StubNameNotMatch;
                                    return jobResult;
                                }
                            }
                        }
                    }
                }
                else if (config.SearchJobInfo != null)
                {
                    endUserRestoreJobConfig.SiteUrl = config.SearchJobInfo.SiteUrl;
                    endUserRestoreJobConfig.Items = config.SearchJobInfo.AdvanceSearchResults;
                    if (config.ModuleType == DocAveOnline.WebApi.Contracts.ModuleType.SharePointOnline || config.ModuleType == DocAveOnline.WebApi.Contracts.ModuleType.OneDriveForBusiness)
                    {
                        endUserRestoreJobConfig.PermissionCheckType = CheckPermissionType.SharePointSite;
                    }
                    else if (config.ModuleType == DocAveOnline.WebApi.Contracts.ModuleType.Microsoft365Groups || config.ModuleType == DocAveOnline.WebApi.Contracts.ModuleType.MicrosoftTeams)
                    {
                        endUserRestoreJobConfig.PermissionCheckType = CheckPermissionType.GroupOrTeams;
                        endUserRestoreJobConfig.GroupID = config.Office365GroupInfo.Id;
                        endUserRestoreJobConfig.Mail = config.Office365GroupInfo.Name;
                    }
                }
                else
                {
                    logger.Error("Cannot find restore items in EndUserRestoreConfig.");
                    return new JobResult() { ErrorCode = ErrorCode.UnExpectedException };
                }

                var jobMessage = await DaoArchiverService.RunEndUserRestoreNow(endUserRestoreJobConfig);
                if (jobMessage.IsReadOnlySite)
                {
                    jobMessage.MessageType = SOMessageType.Failed;
                    jobMessage.FailedType = FailedType.SiteCollectionReadOnly;
                }
                if (jobMessage.MessageType == SOMessageType.Successful)
                {
                    jobResult.Jobs = new List<JobDto>();
                    JobDto jobDto = new JobDto() { Id = jobMessage.ReturnId, NodeType = (RemoveNodeType)Enum.Parse(typeof(RemoveNodeType), jobMessage.ReturnName) };
                    logger.Info($"restore job id:{jobMessage.ReturnId}");
                    jobResult.Jobs.Add(jobDto);
                }
                else
                {
                    if (jobMessage.FailedType == FailedType.InsufficientPrivilegesForStub)
                    {
                        jobResult.ErrorCode = ErrorCode.InsufficientPrivileges4StubView;
                        logger.Error("CheckPermission:InsufficientPrivileges4StubView.");
                    }
                    else if (jobMessage.FailedType == FailedType.InsufficientPrivilegesForSite)
                    {
                        jobResult.ErrorCode = ErrorCode.InsufficientPrivileges4SiteOwner;
                        logger.Error("CheckPermission:InsufficientPrivileges4SiteOwner.");
                    }
                    else if (jobMessage.FailedType == FailedType.SecurityTrimingException)
                    {
                        jobResult.ErrorCode = ErrorCode.SCNotExistOrAccessDenied;
                        logger.Error("CheckPermission:SCNotExistOrAccessDenied.");
                    }
                    else if (jobMessage.FailedType == FailedType.SiteCollectionLocked)
                    {
                        jobResult.ErrorCode = ErrorCode.SiteLockedError;
                        logger.Error("CheckPermission:SiteCollectionLocked.");
                    }
                    else if (jobMessage.FailedType == FailedType.UserNotGroupOwner)
                    {
                        jobResult.ErrorCode = ErrorCode.UserNotInOwnerGroup;
                        logger.Error("CheckPermission:SiteCollectionLocked.");
                    }
                    else if (jobMessage.FailedType == FailedType.SiteNotRegistered)
                    {
                        jobResult.ErrorCode = ErrorCode.NoArchiveHistory;
                        logger.Error("CheckPermission:NoArchiveHistory.");
                    }
                    else if (jobMessage.FailedType == FailedType.RequestResourceNotFound)
                    {
                        jobResult.ErrorCode = ErrorCode.GroupNotFound;
                        logger.Error("CheckPermission:GroupNotFound.");
                    }
                    else if (jobMessage.FailedType == FailedType.UserNotGroupOwnerOrMember)
                    {
                        jobResult.ErrorCode = ErrorCode.UserNotInOwnerOrMemberGroup;
                        logger.Error("CheckPermission:UserNotInOwnerOrMemberGroup.");
                    }
                    else if (jobMessage.FailedType == FailedType.UserNotOwnerForSharePointSite)
                    {
                        jobResult.ErrorCode = ErrorCode.UserNotInOwnerGroup;
                        logger.Error("CheckPermission:UserNotInOwnerGroup.");
                    }
                    else if (jobMessage.FailedType == FailedType.UserNotOwnerOrMemberForSharePointSite)
                    {
                        jobResult.ErrorCode = ErrorCode.UserNotInOwnerOrMemberGroup;
                        logger.Error("CheckPermission:UserNotInOwnerOrMemberGroup.");
                    }
                    else if (jobMessage.FailedType == FailedType.UserNotOwnerOrMemberOrVisitorForSharePointSite)
                    {
                        jobResult.ErrorCode = ErrorCode.UserNotInOwnerOrMemberOrVisitorGroup;
                        logger.Error("CheckPermission:UserNotInOwnerOrMemberOrVisitorGroup.");
                    }
                    else if (jobMessage.FailedType == FailedType.UserNotOwnerOrSpecifiedGroupForSharePointSite)
                    {
                        jobResult.ErrorCode = ErrorCode.UserNotInOwnerOrSpecificGroup;
                        logger.Error("CheckPermission:UserNotOwnerOrSpecifiedGroupForSharePointSite.");
                    }
                    else if (jobMessage.FailedType == FailedType.SiteCollectionReadOnly)
                    {
                        jobResult.ErrorCode = ErrorCode.SiteReadOnlyError;
                        logger.Error("CheckPermission:SiteCollectionReadOnly.");
                    }
                    else if (jobMessage.FailedType == FailedType.SiteTypeNotSupport)
                    {
                        jobResult.ErrorCode = ErrorCode.SiteTypeNotSupport;
                        logger.Error("CheckPermission:SiteTypeNotSupport.");
                    }
                    else if (jobMessage.FailedType == FailedType.PermissionError)
                    {
                        jobResult.ErrorCode = ErrorCode.UserPermissionError;
                        logger.Error("CheckPermission:UserPermissionError.");
                    }
                    else if (jobMessage.FailedType == FailedType.StubFileNotExsit)
                    {
                        jobResult.ErrorCode = ErrorCode.NotFound;
                        logger.Error("CheckPermission:NotFound.");
                    }
                    else
                    {
                        jobResult.ErrorCode = ErrorCode.UnExpectedException;
                        logger.Error("CheckPermission:UnExpectedException.");
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Can not find site in the remote node."))
                {
                    jobResult.ErrorCode = ErrorCode.RemoveFromAos;
                    jobResult.ErrorMessage = e.Message;
                    logger.Error("Can not find site in the remote node, {0}", e.ToString());
                }
                else if (e.Message.Contains("Can not find the restore node,it has retained."))
                {
                    jobResult.ErrorCode = ErrorCode.NoArchiveHistory;
                    jobResult.ErrorMessage = e.Message;
                    logger.Error("Can not find the restore node,it has retained, {0}", e.ToString());
                }
                else
                {
                    jobResult.ErrorCode = ErrorCode.UnExpectedException;
                    jobResult.ErrorMessage = e.Message;
                    logger.Error("RunArchiver EndUserRestoreJob job failed, {0}", e.ToString());
                }
            }
            return jobResult;
        }
        public JobResult RunExportSearchResultJob(EndUserRestoreConfig config)
        {
            config.JobId = GenerateJobId();
            config.RunJobUser = config.Office365User;
            JobResult jobResult = new JobResult() { Jobs = new List<JobDto>()};
            if (string.IsNullOrEmpty(config.RunJobUser))
            {
                jobResult.ErrorCode = ErrorCode.UserPermissionError;
                return jobResult;
            }
            var endUserRestoreSetting = DaoArchiverService.GetEndUserRestoreSetting();
            logger.Info($"RunExportSearchResultJob.RunJobUser:{config.RunJobUser}.ModuleType:{config.ModuleType}.config.ModuleType:{config.ModuleType.ToString()}");
            switch (config.ModuleType)
            {
                case DocAveOnline.WebApi.Contracts.ModuleType.SharePointOnline:
                    if (endUserRestoreSetting.PermissionSetting.IsSearchSiteCollection != null && !(bool)endUserRestoreSetting.PermissionSetting.IsSearchSiteCollection)
                    {
                        jobResult.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportServiceError;
                        logger.Error("AdvanceSearch:DAODoesNotAllowUserRestoreOrExportServiceError.IsRestoreSiteCollection[False].IsExportSiteCollection[False].");
                        return jobResult;
                    }
                    break;
                case DocAveOnline.WebApi.Contracts.ModuleType.Microsoft365Groups:
                case DocAveOnline.WebApi.Contracts.ModuleType.MicrosoftTeams:
                    if (endUserRestoreSetting.PermissionSetting.IsSearchGroupTeamSite != null && !(bool)endUserRestoreSetting.PermissionSetting.IsSearchGroupTeamSite)
                    {
                        jobResult.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportServiceError;
                        logger.Error("AdvanceSearch:DAODoesNotAllowUserRestoreOrExportServiceError.IsRestoreGroupTeamSite[False].IsExportGroupTeamSite[False].");
                        return jobResult;
                    }
                    break;
            }
            JobQueueDto jqDto = new JobQueueDto()
            {
                JobType = JobType.ExportAdvanceSeachResult,
                //JobRunType = jobRunBy,
                TenantGroupId = TenantLocalValue.LogonGroupId,
                JobRunByUser = "RM_TS_RunSchedule",
                JobRunType = JobRunBy.Schedule,
                Parameters = SerializerHelper.SerializeByDataContractSerializer(config),
            };
            JobDto jobDto = new JobDto() { Id = config.JobId };
            logger.Info($"restore job id:{config.JobId}");
            JobQueueService.AddToDBJobQueue(jqDto);
            jobResult.Jobs.Add(jobDto);
            return jobResult;
        }
        private string GenerateJobId()
        {
            DateTime lastGeneratedDate = DateTime.MinValue;
            string jobId = "";
            try
            {
                DateTime now = DateTime.Now;
                while ((now - lastGeneratedDate) < TimeSpan.FromSeconds(1))
                {
                    Thread.Sleep(1000);
                    now = DateTime.Now;
                }
                lastGeneratedDate = now;
                string prefix = "EASR";
                jobId = prefix + DateTime.Now.ToString("yyyyMMddHHmmss") + GenerateRandomNumber(6);
            }
            catch (Exception ex)
            {
                logger.Warn("Generating job ID failed: " + ex.ToString());
            }
            return jobId;
        }
        private string GenerateRandomNumber(int count)
        {
            Random ran = new Random((int)DateTime.Now.Ticks);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                /* Fortify Issue Type: Insecure Randomness 
                * Sink Details: this class StartJobWithRetryAsync 
                * Ignore Reason: random用于 从传入列表中随机选一个值，传入列表值不是固定的，所以是安全的 
                */
                sb.Append(ran.Next(0, 9)).ToString();
            }
            return sb.ToString();
        }
        public async Task<StubParseResult> ParseStubStringAsync(string stubString, string user, string tenantId)
        {
            StubParseResult re = new StubParseResult();
            try
            {
                var settings = await DaoArchiverService.GetEndUserRestoreSettingAsync();
                if (settings != null)
                {
                    re.Footer = settings.Footer;
                    re.IsCustomizeStubRestorePage = settings.IsCustomizeStubRestorePage;
                    re.IsRestoreArchivedTier = settings.IsRestoreArchivedTier;
                    re.Logo = settings.Logo;
                    re.Message = settings.Message;
                    if (settings.IsAllowRestore == true)
                    {
                        re.IsExportStubLink = settings.PermissionSetting.IsExportStubLink == true;
                        re.IsRestoreStubLink = settings.PermissionSetting.IsRestoreStubLink == true;
                    }
                    else
                    {
                        re.IsExportStubLink = false;
                        re.IsRestoreStubLink = false;
                    }
                }
                else
                {
                    logger.Info("no end user restore setting");
                }
            }
            catch (Exception e)
            {
                logger.Error("Get end user restore setting failed, exception is {0}", e.ToString());
            }
            ArchiverStubLink link = null;
            try
            {
                //ReCenter will decode 
                //logger.Info($"Out Log For Debug stubString:{stubString}");
                //stubString = System.Web.HttpUtility.UrlDecode(stubString);
                link = await DaoArchiverService.ParseStubStringAsync(stubString);
            }
            catch (Exception e)
            {
                logger.Error("Parse stub string error, exception is {0}", e.ToString());
                re.ErrorCode = ErrorCode.ParseError;
                re.ErrorMessage = e.ToString();
            }

            if (link != null)
            {
                bool existMapping = false;
                var mappingInfo = await RMRestoreSiteMappingDao.GetMappingBySourceSiteUrlAsync(link.SiteUrl);
                if (mappingInfo!=null)
                {
                    existMapping = true;
                    logger.Info($"have sitemapping.link.SPTenantID:{link.TenantID} , ReCenter.SPTenantID:{tenantId}");
                }
                else
                {
                    logger.Info($"not have sitemapping.link.SPTenantID:{link.TenantID} , ReCenter.SPTenantID:{tenantId}");
                    //check logic
                    if (!tenantId.Equals(link.TenantID, StringComparison.InvariantCultureIgnoreCase))
                    {
                        re.ErrorCode = ErrorCode.TenantIDMismatchError;
                        logger.Info($"link.SPTenantID:{link.TenantID} , ReCenter.SPTenantID:{tenantId}.StubOriginalURL:{link.FileServerRelativeUrl}.StubSiteURL:{link.SiteUrl}.EndUser:{user}.");
                        return re;
                    }
                    if (!string.IsNullOrEmpty(link.User) && !user.Equals(link.User, StringComparison.InvariantCultureIgnoreCase))
                    {
                        re.ErrorCode = ErrorCode.UserPermissionError;
                        logger.Warn($"link.User:UserPermissionError.StubOriginalURL:{link.FileServerRelativeUrl}.StubSiteURL:{link.SiteUrl}.EndUser:{user}.OriginalUser:{link.User}.");
                        return re;
                    }
                    if (!link.HasArchiveHistory)
                    {
                        re.ErrorCode = ErrorCode.NoArchiveHistory;
                        logger.Warn($"NoArchiveHistory error.StubOriginalURL:{link.FileServerRelativeUrl}.StubSiteURL:{link.SiteUrl}.EndUser:{user}.");
                        return re;
                    }
                }
                try
                {
                    re.AdvanceSearchResult = new AdvanceSearchResult();
                    re.AdvanceSearchResult.FullPath = MakeFullUrl(link.SiteUrl, link.FileServerRelativeUrl);
                    re.AdvanceSearchResult.Name = link.FileServerRelativeUrl.Substring(link.FileServerRelativeUrl.LastIndexOf('/') + 1);
                    re.AdvanceSearchResult.PathMD5 = link.PathMD5;
                    re.SiteUrl = link.SiteUrl;
                    re.BackUpJobId = link.JobID;
                    re.StubType = link.StubType;
                    re.StubId = link.StubId;
                    re.FileSize = link.FileSize;
                    re.StubProductSource = (int)link.StubProductSource;
                    re.IsArchiveTier = await RMCacheManager.Cache.TryGetAsync<bool>(
                        IRMCache.Keys.Job_ArchivedDataTier + link.JobID,
                        CheckIsArchiverTier,
                        TimeSpan.FromHours(1)); 

                    if (existMapping)
                    {
                        try
                        {
                            logger.Info($"exist site mapping,source:{mappingInfo.SourceSiteUrl},target:{mappingInfo.TargetSiteUrl}");
                            re.AdvanceSearchResult.FullPath = re.AdvanceSearchResult.FullPath.Replace(link.SiteUrl, mappingInfo.TargetSiteUrl);
                            //re.SiteUrl = mappingInfo.TargetSiteUrl;
                        }
                        catch (Exception e)
                        {
                            logger.Error($"some thing went wrong when set maping to restore,error:{e}");
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Parse stub string error, exception is {0}", e.ToString());
                    re.ErrorCode = ErrorCode.ParseError;
                    re.ErrorMessage = e.ToString();
                }
            }
            else
            {
                logger.Error("Parse stub string error, link is null");
                re.ErrorCode = ErrorCode.ParseError;
            }
            logger.Info($"Finished ParseStubString.StubParseResult.IsExportStubLink:{re.IsExportStubLink}.StubParseResult.IsRestoreStubLink:{re.IsRestoreStubLink}.");
            return re;


            async Task<bool> CheckIsArchiverTier()
            {
                var isArchiveTier = IsArchiveTier(link.JobID, link.SiteUrl, link.PathMD5, out bool failSearch);
                if (failSearch)
                {
                    StubRebuildMd5Configs stubRebuildMd5Configs = null;
                    logger.Info($"unable found isArchiverTier, sc:{link.SiteUrl},link job id:{link.JobID}, pathMd5:{link.PathMD5}");
                    foreach (var site in await RestoreSiteMappingDao.GetSourceSCUrlsByTargetSCUrlAsync(link.SiteUrl))
                    {
                        logger.Info($"try found isArchiverTier, sc:{site},link job id:{link.JobID}, pathMd5:{link.PathMD5},failSearch:{failSearch}");
                        isArchiveTier = IsArchiveTier(link.JobID, site, link.PathMD5, out failSearch);
                        if (failSearch)
                        {
                            try
                            {
                                if (stubRebuildMd5Configs == null)
                                {
                                    stubRebuildMd5Configs = GetStubRebuildMd5Configs();
                                }

                                StubRebuildMd5Config stubRebuildMd5Config = stubRebuildMd5Configs?.Configs?.FirstOrDefault(c => c != null && site.Equals(c?.SiteCollectionUrl));
                                if (stubRebuildMd5Config == null)
                                {
                                    continue;
                                }

                                if (IsArchiveTier(link.JobID, site, HashCodeHelper.ToMD5HashCode(BuildDestinationPath(site, link.FileServerRelativeUrl, stubRebuildMd5Config.LibPathMapping)), out failSearch))
                                {
                                    logger.Info($"Success found isArchiverTier by change path md5, sc:{site},link job id:{link.JobID}, pathMd5:{link.PathMD5}");
                                    isArchiveTier = true;
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error($"IsArchiveTier error:{ex}");
                            }
                        }
                        else
                        {
                            logger.Info($"Success found isArchiverTier, sc:{site},link job id:{link.JobID}, pathMd5:{link.PathMD5}");
                            break;
                        }
                    }
                }

                return isArchiveTier;
            }
        }

        private StubRebuildMd5Configs GetStubRebuildMd5Configs()
        {
            StubRebuildMd5Configs result = new StubRebuildMd5Configs();
            try
            {
                var key = RMKeyValueDao.GetValueByKey("StubRebuildMd5Configs");
                if (string.IsNullOrWhiteSpace(key?.Value))
                {
                    result = new StubRebuildMd5Configs();
                }
                else
                {
                    result = SerializerHelper.DeserializeByJsonConvert<StubRebuildMd5Configs>(key.Value);
                    logger.Info($"GetStubRebuildMd5Configs success：{key.Value}");
                }

            }
            catch (Exception ex)
            {
                logger.Warn($"GetStubRebuildMd5Configs failed: {ex}");
            }
            return result;
        }

        public string BuildDestinationPath(string siteCollectionUrl, string fileRelativeUrl, Dictionary<string, string> libPathMapping)
        {
            try
            {
                string fileSubPath = fileRelativeUrl.Trim('/');

                foreach (var item in libPathMapping)
                {
                    var libPath = item.Key.Trim('/');
                    var index = fileSubPath.IndexOf(libPath, StringComparison.OrdinalIgnoreCase);
                    if (index >= 0)
                    {
                        fileSubPath = $"{item.Value}{fileSubPath.Substring(index + libPath.Length).Replace('/', '\\')}";
                        break;
                    }
                }

                return $"{siteCollectionUrl.TrimEnd('/')}\\{fileSubPath}";
            }
            catch (Exception ex)
            {
                logger.Error($"Error in BuildDestinationPath: {ex.Message}");
                return "";
            }
        }

        private bool IsArchiveTier(string backUpJobId,string siteUrl,string pathMd5, out bool failSearch)
        {
            failSearch = false;
            try
            {
                var sites = RemoteNodeService.GetRemoteSiteCollectionByParam(new List<string> { siteUrl });
                var siteIndex = ArchiverSiteMasterIndexService.GetAllSiteCollectionNodsInfoByUrl(siteUrl).First();
                GCommon.Contract.Server.Common.BackupDataSearch.ArchiverRestoreResult searchResult = RestoreSearchService.GetSearchTreeResultAsync(new GCommon.Contract.Server.Common.BackupDataSearch.ArchiverRestoreResult()
                {
                    PageSize = -1,
                    SerchContract = new GCommon.Contract.Server.Common.BackupDataSearch.BackupDataSearchContract()
                    {
                        SearchNode = new GCommon.Contract.Server.Common.BackupDataSearch.SiteCollectionNodesInfo() { SiteUrl = siteUrl, SiteGroupId = siteIndex.SiteGroupId, SPObjectId = sites!=null&& sites.Count>0? sites[0].ObjectId: ""},
                        FilterPolicy = new GCommon.Contract.CommonFilter.ArchiverRestoreFilter() { FilterName = "", Level = AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion, PathMD5List = new List<string>() { pathMd5 } },
                        BackupJobId = backUpJobId
                    }
                }, false).GetAwaiter().GetResult();
                if (searchResult != null && searchResult.RestoreSerchNodes != null && searchResult.RestoreSerchNodes.Count > 0)
                {
                    bool result = searchResult.RestoreSerchNodes[0].IsArchiveTier;
                    logger.Info("IsArchiveTier check result is result");
                    return result;
                }
                else
                {
                    failSearch = true;
                    logger.Info("IsArchiveTier error, searchResult is null");
                    return false;
                }
            }
            catch (Exception e)
            {
                failSearch = true;
                logger.Error("IsArchiveTier error, exception is {0}", e.ToString());
                return false;
            }
        }

        public async Task<EndUserRestoreSettingResult> GetEndUserSettingAsync()
        {
            try
            {
                var settings = await DaoArchiverService.GetEndUserRestoreSettingAsync();
                if (settings != null)
                {
                    return new EndUserRestoreSettingResult()
                    {
                        Footer = settings.Footer,
                        IsCustomizeStubRestorePage = settings.IsCustomizeStubRestorePage,
                        IsRestoreArchivedTier = settings.IsRestoreArchivedTier,
                        Logo = settings.Logo,
                        Message = settings.Message,
                        IsAllowRestore = settings.IsAllowRestore==true,
                        PermissionSetting = new DocAveOnline.WebApi.Contracts.EndUserPermissionSetting()
                        {
                            TeamsAndGroup = (DocAveOnline.WebApi.Contracts.TeamsPermissionSetting)(int)settings.PermissionSetting.TeamsAndGroup,
                            SiteCollection = (DocAveOnline.WebApi.Contracts.SP365PermissionLevel)(int)settings.PermissionSetting.SiteCollection,
                            SiteCollectionSpecialGroupNames = settings.PermissionSetting.SiteCollectionSpecialGroupNames,
                            IsExportGroupTeamSite = settings.PermissionSetting.IsExportGroupTeamSite == true,
                            IsRestoreGroupTeamSite = settings.PermissionSetting.IsRestoreGroupTeamSite == true,
                            IsExportSiteCollection = settings.PermissionSetting.IsExportSiteCollection == true,
                            IsRestoreSiteCollection = settings.PermissionSetting.IsRestoreSiteCollection == true,
                            IsExportStubLink = settings.PermissionSetting.IsExportStubLink == true,
                            IsRestoreStubLink = settings.PermissionSetting.IsRestoreStubLink == true,
                            IsSearchGroupTeamSite = settings.PermissionSetting.IsSearchGroupTeamSite == null?true: settings.PermissionSetting.IsSearchGroupTeamSite == true,
                            IsSearchSiteCollection = settings.PermissionSetting.IsSearchSiteCollection == null ? true : settings.PermissionSetting.IsSearchSiteCollection == true,
                            StubOopRestoreSetting = new StubOopRestoreSetting
                            {
                                IsEnableManualInputDesStubLocation = settings.PermissionSetting.StubOopRestoreSetting.IsEnableManualInputDesStubLocation,
                                IsEnableSearchStubLocation = settings.PermissionSetting.StubOopRestoreSetting.IsEnableSearchStubLocation,
                                IsEnableStubOopRestore = settings.PermissionSetting.StubOopRestoreSetting.IsEnableStubOopRestore
                            }
                        }
                    };
                }
                else
                {
                    logger.Info("no end user restore setting");
                    return new EndUserRestoreSettingResult() { ErrorCode = ErrorCode.NotFound, ErrorMessage = "Can not find end user setting." };
                }
            }
            catch (Exception e)
            {
                var message = $"Get end user restore setting failed, exception is {e.ToString()}";
                logger.Error(message);
                return new EndUserRestoreSettingResult() { ErrorCode = ErrorCode.UnExpectedException, ErrorMessage = message };
            }
        }

        public JobResult RunArchiverContentDownloadJob(ArchivedContentRestoreConfig config)
        {
            logger.Info("RunArchiverContentDownloadJob.");
            JobResult jobResult = new JobResult();
            try
            {
                EndUserRestoreJobConfig endUserRestoreJobConfig = null;
                if (config.ArchivedContentInfos != null && config.ArchivedContentInfos.Count > 0)
                {
                    var contentInfo = config.ArchivedContentInfos[0];
                    var record = JsonConvert.DeserializeObject<Record>(contentInfo.ExtensionString);
                    logger.Info($"RunArchiverContentDownloadJob  StubJobInfo : BackUpJobId:{contentInfo.BackUpJobId},PathMD5:{contentInfo.PathMD5} site url:{config.SiteUrl}");
                    AdvanceSearchResult item = new AdvanceSearchResult();
                    //item.BackUpJobId = contentInfo.BackUpJobId;
                    item.PathMD5 = contentInfo.PathMD5;
                    item.FullPath = contentInfo.FileUrl;
                    item.Name = record.LeafName;
                    item.ItemId = record.Id;
                    //item.TreeNode = contentInfo.TreeNode;
                    //item.IndexString = contentInfo.ExtensionString;
                    endUserRestoreJobConfig = new EndUserRestoreJobConfig();
                    endUserRestoreJobConfig.SiteUrl = config.SiteUrl;
                    endUserRestoreJobConfig.RestoreStorage = config.RestoreStorage;
                    endUserRestoreJobConfig.Items = new List<AdvanceSearchResult> { item };
                    endUserRestoreJobConfig.PermissionCheckType = CheckPermissionType.None;
                    endUserRestoreJobConfig.IntegrationModule = ArchiveIntegrationModules.Records;
                    endUserRestoreJobConfig.RestoreType = RestoreType.OutPlace;
                    endUserRestoreJobConfig.RunJobUser = TenantLocalValue.LogonUserEmail;
                }
                else
                {
                    logger.Error("Cannot find restore items in ArchivedContentRestoreConfig.");
                    return new JobResult() { ErrorCode = ErrorCode.UnExpectedException };
                }

                var jobMessage = DaoArchiverService.RunEndUserRestoreNow(endUserRestoreJobConfig, true).GetAwaiter().GetResult();
                if (jobMessage.MessageType == SOMessageType.Successful)
                {
                    jobResult.Jobs = new List<JobDto>();
                    JobDto jobDto = new JobDto() { Id = jobMessage.ReturnId, NodeType = (RemoveNodeType)Enum.Parse(typeof(RemoveNodeType), jobMessage.ReturnName) };
                    logger.Info($"restore job id:{jobMessage.ReturnId}");
                    jobResult.Jobs.Add(jobDto);
                }
                else
                {
                    if (jobMessage.FailedType == FailedType.InsufficientPrivilegesForStub)
                    {
                        jobResult.ErrorCode = ErrorCode.InsufficientPrivileges4StubView;
                        logger.Error("CheckPermission:InsufficientPrivilegesForStub.");
                    }
                    else if (jobMessage.FailedType == FailedType.InsufficientPrivilegesForSite)
                    {
                        jobResult.ErrorCode = ErrorCode.InsufficientPrivileges4SiteOwner;
                        logger.Error("CheckPermission:InsufficientPrivilegesForSite.");
                    }
                    else if (jobMessage.FailedType == FailedType.SecurityTrimingException)
                    {
                        jobResult.ErrorCode = ErrorCode.SCNotExistOrAccessDenied;
                        logger.Error("CheckPermission:SCNotExistOrAccessDenied.");
                    }
                    else if (jobMessage.FailedType == FailedType.SiteCollectionLocked)
                    {
                        jobResult.ErrorCode = ErrorCode.SiteLockedError;
                        logger.Error("CheckPermission:SiteCollectionLocked.");
                    }
                    else if (jobMessage.FailedType == FailedType.UserNotGroupOwner)
                    {
                        jobResult.ErrorCode = ErrorCode.UserNotInOwnerGroup;
                        logger.Error("CheckPermission:SiteCollectionLocked.");
                    }
                    else if (jobMessage.FailedType == FailedType.UserNotOwnerForSharePointSite)
                    {
                        jobResult.ErrorCode = ErrorCode.UserNotInOwnerGroup;
                        logger.Error("CheckPermission:UserNotInOwnerGroup.");
                    }
                    else if (jobMessage.FailedType == FailedType.UserNotOwnerOrMemberForSharePointSite)
                    {
                        jobResult.ErrorCode = ErrorCode.UserNotInOwnerOrMemberGroup;
                        logger.Error("CheckPermission:UserNotInOwnerOrMemberGroup.");
                    }
                    else if (jobMessage.FailedType == FailedType.UserNotOwnerOrMemberOrVisitorForSharePointSite)
                    {
                        jobResult.ErrorCode = ErrorCode.UserNotInOwnerOrMemberOrVisitorGroup;
                        logger.Error("CheckPermission:UserNotInOwnerOrMemberOrVisitorGroup.");
                    }
                    else if (jobMessage.FailedType == FailedType.UserNotOwnerOrSpecifiedGroupForSharePointSite)
                    {
                        jobResult.ErrorCode = ErrorCode.UserNotInOwnerOrSpecificGroup;
                        logger.Error("CheckPermission:UserNotInOwnerOrSpecificGroup.");
                    }
                    else
                    {
                        jobResult.ErrorCode = ErrorCode.UnExpectedException;
                        logger.Error("CheckPermission:UnExpectedException.");
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Can not find site in the remote node."))
                {
                    jobResult.ErrorCode = ErrorCode.RemoveFromAos;
                    jobResult.ErrorMessage = e.Message;
                    logger.Error("Can not find site in the remote node, {0}", e.ToString());
                }
                else if (e.Message.Contains("Can not find the restore node,it has retained."))
                {
                    jobResult.ErrorCode = ErrorCode.NoArchiveHistory;
                    jobResult.ErrorMessage = e.Message;
                    logger.Error("Can not find the restore node,it has retained, {0}", e.ToString());
                }
                else
                {
                    jobResult.ErrorCode = ErrorCode.UnExpectedException;
                    jobResult.ErrorMessage = e.Message;
                    logger.Error("RunArchiver EndUserRestoreJob job failed, {0}", e.ToString());
                }
            }
            return jobResult;
        }

        [Audit(Module = AuditModule.RestoreCenter, Category = AuditCategory.RestoreCenter, Action = AuditAction.RunArchiverOutPlaceRestoreJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public JobResult RunArchivedContentExportJob(ExportArchivedContentConfig config)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            logger.Info($"RunArchivedContentExportJob.RunJobUser:{config.Office365UserMail}.ModuleType:{config.ModuleType}.");
            JobResult jobResult = new JobResult();
            try
            {
                if (DaoArchiverService.IsExportSizeReachLimited())
                {
                    jobResult.ErrorCode = ErrorCode.ExportSizeLimitReached;
                    jobResult.LimitSize = 100 * 1024 * 1024 * 1024L;
                    logger.Error("current tenant export data size limit reached");
                    return jobResult;
                }
                //经与Recenter确认:Run Restore Job首先check DAO Setting，如果不允许Restore & Export，则直接返回Error Code.
                //1.Allow end users to restore/export archived data,总开关直接关闭时，直接返回Error Code
                //2.各个Source的 restore/export Setting，单独判断其打开关闭，如果不允许Restore & Export，则直接返回Error Code.
                #region Check DAO End User Restore Setting
                var endUserRestoreSetting = DaoArchiverService.GetEndUserRestoreSetting();
                if (!endUserRestoreSetting.IsAllowRestore)
                {
                    jobResult.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportTotalError;
                    logger.Error("RunArchivedContentExportJob:DAODoesNotAllowUserRestoreAndExportTotalError:IsAllowRestore[False].[");
                    return jobResult;
                }
                switch (config.ModuleType)
                {
                    case DocAveOnline.WebApi.Contracts.ModuleType.None:
                        if (!endUserRestoreSetting.PermissionSetting.IsExportStubLink)
                        {
                            jobResult.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportServiceError;
                            logger.Error("RunArchivedContentExportJob:DAODoesNotAllowUserRestoreOrExportServiceError:IsExportStubLink[False].");
                            return jobResult;
                        }
                        break;
                    case DocAveOnline.WebApi.Contracts.ModuleType.SharePointOnline:
                        if (!endUserRestoreSetting.PermissionSetting.IsExportSiteCollection)
                        {
                            jobResult.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportServiceError;
                            logger.Error("RunArchivedContentExportJob:DAODoesNotAllowUserRestoreOrExportServiceError.IsExportSiteCollection[False]");
                            return jobResult;
                        }
                        break;
                    case DocAveOnline.WebApi.Contracts.ModuleType.Microsoft365Groups:
                    case DocAveOnline.WebApi.Contracts.ModuleType.MicrosoftTeams:
                        if (!endUserRestoreSetting.PermissionSetting.IsExportGroupTeamSite)
                        {
                            jobResult.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportServiceError;
                            logger.Error("RunArchivedContentExportJob:DAODoesNotAllowUserRestoreOrExportServiceError.IsExportGroupTeamSite[False]");
                            return jobResult;
                        }
                        break;
                    case DocAveOnline.WebApi.Contracts.ModuleType.OneDriveForBusiness:
                    default:
                        jobResult.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportServiceError;
                        logger.Error($"RunArchivedContentExportJob:DAODoesNotAllowUserRestoreOrExportServiceError.ModuleType[Error].ModuleType:{config.ModuleType}.");
                        return jobResult;
                }
                #endregion
                EndUserRestoreJobConfig endUserRestoreJobConfig = null;
                if (config != null && config.ExportContentInfos != null && config.ExportContentInfos.Count > 0)
                {
                    endUserRestoreJobConfig = new EndUserRestoreJobConfig();
                    endUserRestoreJobConfig.SiteUrl = config.SiteUrl;
                    endUserRestoreJobConfig.IntegrationModule = ArchiveIntegrationModules.Recenter;
                    endUserRestoreJobConfig.RestoreType = RestoreType.OutPlace;
                    endUserRestoreJobConfig.RunJobUser = config.Office365UserMail;
                    endUserRestoreJobConfig.Items = new List<AdvanceSearchResult>();
                    endUserRestoreJobConfig.O365TenantId = config.Office365TenantID;
                    endUserRestoreJobConfig.IsExportJob = true;
                    foreach (var inf in config.ExportContentInfos)
                    {
                        endUserRestoreJobConfig.Items.Add(ConvertToSearchResult(inf));
                    }
                    if (!string.IsNullOrEmpty(config.StubType))
                    {
                        endUserRestoreJobConfig.StubType = config.StubType;
                    }
                    if (config.IsSearchResultExport)
                    {
                        logger.Info($"RunArchivedContentExportJob site url:{config.SiteUrl}");
                        endUserRestoreJobConfig.PermissionCheckType = CheckPermissionType.SharePointSite;
                    }
                    else if (!config.IsSearchResultExport && config.ExportContentInfos.Count == 1)
                    {
                        var contentInfo = config.ExportContentInfos[0];
                        logger.Info($"RunArchivedContentExportJob  StubJobInfo : BackUpJobId:{contentInfo.BackUpJobId},PathMD5:{contentInfo.PathMD5} site url:{config.SiteUrl}");
                        EndUserRestoreItem item = new EndUserRestoreItem();
                        item.BackUpJobId = contentInfo.BackUpJobId;
                        item.PathMD5 = contentInfo.PathMD5;
                        //item.FullPath = contentInfo.FullPath;

                        //endUserRestoreJobConfig.Items.Add(item);
                        endUserRestoreJobConfig.PermissionCheckType = CheckPermissionType.StubRestoreLink;
                        endUserRestoreJobConfig.BackUpJobId = contentInfo.BackUpJobId;
                    }
                    else
                    {
                        logger.Error("find restore items in ExportArchivedContentConfig error.");
                        return new JobResult() { ErrorCode = ErrorCode.UnExpectedException };
                    }
                    if (config.ModuleType == DocAveOnline.WebApi.Contracts.ModuleType.Microsoft365Groups || config.ModuleType == DocAveOnline.WebApi.Contracts.ModuleType.MicrosoftTeams)
                    {
                        endUserRestoreJobConfig.PermissionCheckType = CheckPermissionType.GroupOrTeams;
                        endUserRestoreJobConfig.GroupID = config.Office365GroupInfo.Id;
                        endUserRestoreJobConfig.Mail = config.Office365GroupInfo.Name;
                    }

                }
                else
                {
                    logger.Error("Cannot find restore items in ExportArchivedContentConfig.");
                    return new JobResult() { ErrorCode = ErrorCode.UnExpectedException };
                }
                var jobMessage = DaoArchiverService.RunEndUserRestoreNow(endUserRestoreJobConfig).GetAwaiter().GetResult();

                if (jobMessage.MessageType == SOMessageType.Successful)
                {
                    jobResult.Jobs = new List<JobDto>();
                    JobDto jobDto = new JobDto() { Id = jobMessage.ReturnId, NodeType = (RemoveNodeType)Enum.Parse(typeof(RemoveNodeType), jobMessage.ReturnName) };
                    logger.Info($"restore job id:{jobMessage.ReturnId}");
                    jobResult.Jobs.Add(jobDto);
                }
                else
                {
                    if (jobMessage.FailedType == FailedType.InsufficientPrivilegesForStub)
                    {
                        jobResult.ErrorCode = ErrorCode.InsufficientPrivileges4StubView;
                        logger.Error("CheckPermission:InsufficientPrivilegesForStub.");
                    }
                    else if (jobMessage.FailedType == FailedType.InsufficientPrivilegesForSite)
                    {
                        jobResult.ErrorCode = ErrorCode.InsufficientPrivileges4SiteOwner;
                        logger.Error("CheckPermission:InsufficientPrivilegesForSite.");
                    }
                    else if (jobMessage.FailedType == FailedType.SecurityTrimingException || jobMessage.FailedType == FailedType.NodeNotExisting)
                    {
                        jobResult.ErrorCode = ErrorCode.SCNotExistOrAccessDenied;
                        logger.Error("CheckPermission:SCNotExistOrAccessDenied.");
                    }
                    else if (jobMessage.FailedType == FailedType.SiteCollectionLocked)
                    {
                        jobResult.ErrorCode = ErrorCode.SiteLockedError;
                        logger.Error("CheckPermission:SiteCollectionLocked.");
                    }
                    else if (jobMessage.FailedType == FailedType.UserNotGroupOwner)
                    {
                        jobResult.ErrorCode = ErrorCode.UserNotInOwnerGroup;
                        logger.Error("CheckPermission:SiteCollectionLocked.");
                    }
                    else if (jobMessage.FailedType == FailedType.RequestResourceNotFound)
                    {
                        jobResult.ErrorCode = ErrorCode.GroupNotFound;
                        logger.Error("CheckPermission:GroupNotFound.");
                    }
                    else if (jobMessage.FailedType == FailedType.UserNotGroupOwnerOrMember)
                    {
                        jobResult.ErrorCode = ErrorCode.UserNotInOwnerOrMemberGroup;
                        logger.Error("CheckPermission:UserNotInOwnerOrMemberGroup.");
                    }
                    else if (jobMessage.FailedType == FailedType.UserNotOwnerForSharePointSite)
                    {
                        jobResult.ErrorCode = ErrorCode.UserNotInOwnerGroup;
                        logger.Error("CheckPermission:UserNotInOwnerGroup.");
                    }
                    else if (jobMessage.FailedType == FailedType.UserNotOwnerOrMemberForSharePointSite)
                    {
                        jobResult.ErrorCode = ErrorCode.UserNotInOwnerOrMemberGroup;
                        logger.Error("CheckPermission:UserNotInOwnerOrMemberGroup.");
                    }
                    else if (jobMessage.FailedType == FailedType.UserNotOwnerOrMemberOrVisitorForSharePointSite)
                    {
                        jobResult.ErrorCode = ErrorCode.UserNotInOwnerOrMemberOrVisitorGroup;
                        logger.Error("CheckPermission:UserNotInOwnerOrMemberOrVisitorGroup.");
                    }
                    else if (jobMessage.FailedType == FailedType.UserNotOwnerOrSpecifiedGroupForSharePointSite)
                    {
                        jobResult.ErrorCode = ErrorCode.UserNotInOwnerOrSpecificGroup;
                        logger.Error("CheckPermission:UserNotInOwnerOrSpecificGroup.");
                    }
                    else if (jobMessage.FailedType == FailedType.SiteTypeNotSupport)
                    {
                        jobResult.ErrorCode = ErrorCode.SiteTypeNotSupport;
                        logger.Error("CheckPermission:SiteTypeNotSupport.");
                    }
                    else if (jobMessage.FailedType == FailedType.ActiveAppProfileNotFound)
                    {
                        jobResult.ErrorCode = ErrorCode.ActiveAppProfileNotFound;
                        logger.Error("CheckPermission:ActiveAppProfileNotFound.");
                    }
                    else
                    {
                        jobResult.ErrorCode = ErrorCode.UnExpectedException;
                        logger.Error("CheckPermission:UnExpectedException.");
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Can not find site in the remote node."))
                {
                    jobResult.ErrorCode = ErrorCode.RemoveFromAos;
                    jobResult.ErrorMessage = e.Message;
                    logger.Error("Can not find site in the remote node, {0}", e.ToString());
                }
                else if (e.Message.Contains("Can not find the restore node,it has retained."))
                {
                    jobResult.ErrorCode = ErrorCode.NoArchiveHistory;
                    jobResult.ErrorMessage = e.Message;
                    logger.Error("Can not find the restore node,it has retained, {0}", e.ToString());
                }
                else
                {
                    jobResult.ErrorCode = ErrorCode.UnExpectedException;
                    jobResult.ErrorMessage = e.Message;
                    logger.Error("RunArchivedContentExportJob failed, {0}", e.ToString());
                }
            }
            sw.Stop();
            logger.Info($"linkRestoreReport run export job check permission and start job cost:{sw.ElapsedMilliseconds}");
            return jobResult;
        }

        private AdvanceSearchResult ConvertToSearchResult(ExportArchivedDataInfo info)
        {
            AdvanceSearchResult item = new AdvanceSearchResult();
            item.FullPath = info.FullPath;
            item.PathMD5 = info.PathMD5;
            //item.TreeNode = info.TreeNode;
            return item;
        }
        private string MakeFullUrl(string siteUrl, string strUrl)
        {
            if (siteUrl == null || strUrl == null)
            {
                throw new ArgumentNullException("strUrl");
            }
            if (siteUrl == strUrl)
            {
                return siteUrl;
            }
            if (strUrl.StartsWith("http:") || strUrl.StartsWith("https:"))
            {
                return strUrl;
            }
            strUrl = strUrl.Trim();
            StringBuilder stringBuilder = new StringBuilder(512);
            if (strUrl.StartsWith("/"))
            {
                var siteUri = new Uri(siteUrl);
                var protocol = siteUri.Scheme + ":";
                stringBuilder.Append(protocol);
                stringBuilder.Append("//");
                stringBuilder.Append(siteUri.Host);
                if ((StsCompareStrings(protocol, "http:") && siteUri.Port != 80) || (StsCompareStrings(protocol, "https:") && siteUri.Port != 443))
                {
                    stringBuilder.Append(":");
                    stringBuilder.Append(siteUri.Port);
                }
                stringBuilder.Append(strUrl);
            }
            else
            {
                stringBuilder.Append(siteUrl);
                if (strUrl != "")
                {
                    stringBuilder.Append("/");
                    stringBuilder.Append(strUrl);
                }
            }
            if (stringBuilder[stringBuilder.Length - 1] == '/')
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            return stringBuilder.ToString();
        }

        private bool StsCompareStrings(string str1, string str2)
        {
            System.Globalization.CompareInfo compareInfo = System.Globalization.CultureInfo.InvariantCulture.CompareInfo;
            return 0 == compareInfo.Compare(str1, str2, System.Globalization.CompareOptions.IgnoreCase);
        }
    }
}
