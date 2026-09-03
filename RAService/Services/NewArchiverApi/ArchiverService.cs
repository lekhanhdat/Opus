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
using System;
using System.Collections.Generic;
using System.Text;
using AvePoint.Api.Contract;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.StorageOptimization.Archiver;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Restore;
using AvePoint.GCommon.Contract.Tree.Object;
using DocAveOnline.WebApi.Contracts;

namespace AvePoint.Common.Api.Services
{
    public interface IArchiverService
    {
        JobResult RunArchiverEndUserRestoreJob(EndUserRestoreConfig config);
        StubParseResult ParseStubString(string stubString, string user, string tenantId);
        JobResult RunArchiverContentDownloadJob(ArchivedContentRestoreConfig config);
        JobResult RunArchivedContentExportJob(ExportArchivedContentConfig config);

        EndUserRestoreSettingResult GetEndUserSetting();
    }
    public class ArchiverService : IArchiverService
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(ArchiverService));

        public IMArchiverService DaoArchiverService { get; set; }
        public IMArchiverRestoreService RestoreService { get; set; }

        public JobResult RunArchiverEndUserRestoreJob(EndUserRestoreConfig config)
        {
            logger.Info("RunArchiverEndUserRestoreJob.");
            JobResult jobResult = new JobResult();
            try
            {
                //经与Recenter确认:Run Restore Job首先check DAO Setting，如果不允许Restore & Export，则直接返回Error Code.
                //1.Allow end users to restore/export archived data,总开关直接关闭时，直接返回Error Code
                //2.各个Source的 restore/export Setting，单独判断其打开关闭，如果不允许Restore & Export，则直接返回Error Code.
                #region Check DAO End User Restore Setting
                var endUserRestoreSetting = DaoArchiverService.GetEndUserRestoreSetting();
                if (endUserRestoreSetting.IsAllowRestore == Status.False)
                {
                    jobResult.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportTotalError;
                    logger.Error("RunArchiverEndUserRestoreJob:DAODoesNotAllowUserRestoreAndExportTotalError:IsAllowRestore[False].");
                    return jobResult;
                }
                switch (config.ModuleType)
                {
                    case DocAveOnline.WebApi.Contracts.ModuleType.None:
                        if (endUserRestoreSetting.PermissionSetting.IsRestoreStubLink == Status.False)
                        {
                            jobResult.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportServiceError;
                            logger.Error("RunArchiverEndUserRestoreJob:DAODoesNotAllowUserRestoreOrExportServiceError:IsRestoreStubLink[False].");
                            return jobResult;
                        }
                        break;
                    case DocAveOnline.WebApi.Contracts.ModuleType.SharePointOnline:
                        if (endUserRestoreSetting.PermissionSetting.IsRestoreSiteCollection == Status.False)
                        {
                            jobResult.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportServiceError;
                            logger.Error("RunArchiverEndUserRestoreJob:DAODoesNotAllowUserRestoreOrExportServiceError.IsRestoreSiteCollection[False]");
                            return jobResult;
                        }
                        break;
                    case DocAveOnline.WebApi.Contracts.ModuleType.Microsoft365Groups:
                    case DocAveOnline.WebApi.Contracts.ModuleType.MicrosoftTeams:
                        if (endUserRestoreSetting.PermissionSetting.IsRestoreGroupTeamSite == Status.False)
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
                endUserRestoreJobConfig.RestoreType = RestoreType.InPlace;
                endUserRestoreJobConfig.IntegrationModule = ArchiveIntegrationModules.Recenter;
                endUserRestoreJobConfig.Items = new List<EndUserRestoreItem>();

                if (config.StubJobInfo != null)
                {
                    logger.Info($"RunArchiverEndUserRestoreJob  StubJobInfo : BackUpJobId:{config.StubJobInfo.BackUpJobId},PathMD5:{config.StubJobInfo.AdvanceSearchResult.PathMD5} site url:{config.StubJobInfo.SiteUrl}");
                    EndUserRestoreItem item = new EndUserRestoreItem();
                    item.BackUpJobId = config.StubJobInfo.BackUpJobId;
                    item.FullPath = config.StubJobInfo.AdvanceSearchResult.FullPath;
                    item.Name = config.StubJobInfo.AdvanceSearchResult.Name;
                    item.PathMD5 = config.StubJobInfo.AdvanceSearchResult.PathMD5;
                    endUserRestoreJobConfig.SiteUrl = config.StubJobInfo.SiteUrl;
                    endUserRestoreJobConfig.StubType = config.StubJobInfo.StubType;
                    endUserRestoreJobConfig.Items.Add(item);
                    endUserRestoreJobConfig.PermissionCheckType = CheckPermissionType.StubRestoreLink;
                }
                else if (config.SearchJobInfo != null)
                {
                    endUserRestoreJobConfig.SiteUrl = config.SearchJobInfo.SiteUrl;
                    foreach (var i in config.SearchJobInfo.AdvanceSearchResults)
                    {
                        EndUserRestoreItem item = new EndUserRestoreItem();
                        item.FullPath = i.FullPath;
                        item.Name = i.Name;
                        item.PathMD5 = i.PathMD5;
                        endUserRestoreJobConfig.Items.Add(item);
                    }
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

                var jobMessage = DaoArchiverService.RunEndUserRestoreNow(endUserRestoreJobConfig);
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
                else
                {
                    jobResult.ErrorCode = ErrorCode.UnExpectedException;
                    jobResult.ErrorMessage = e.Message;
                    logger.Error("RunArchiver EndUserRestoreJob job failed, {0}", e.ToString());
                }
            }
            return jobResult;
        }

        public StubParseResult ParseStubString(string stubString, string user, string tenantId)
        {
            StubParseResult re = new StubParseResult();
            try
            {
                var settings = RestoreService.GetEndUserRestoreSetting();
                if (settings != null)
                {
                    re.Footer = settings.Footer;
                    re.IsCustomizeStubRestorePage = settings.IsCustomizeStubRestorePage;
                    re.IsRestoreArchivedTier = settings.IsRestoreArchivedTier;
                    re.Logo = settings.Logo;
                    re.Message = settings.Message;
                    if (settings.IsAllowRestore == Status.True)
                    {
                        re.IsExportStubLink = settings.PermissionSetting.IsExportStubLink == Status.True;
                        re.IsRestoreStubLink = settings.PermissionSetting.IsRestoreStubLink == Status.True;
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
                link = DaoArchiverService.ParseStubString(stubString);
            }
            catch (Exception e)
            {
                logger.Error("Parse stub string error, exception is {0}", e.ToString());
                re.ErrorCode = ErrorCode.ParseError;
                re.ErrorMessage = e.ToString();
            }

            if (link != null)
            {
                //check logic
                if (!tenantId.Equals(link.TenantID, StringComparison.InvariantCultureIgnoreCase))
                {
                    re.ErrorCode = ErrorCode.TenantIDMismatchError;
                    logger.Info($"link.SPTenantID:{link.TenantID} , ReCenter.SPTenantID:{tenantId}");
                    return re;
                }
                if (!string.IsNullOrEmpty(link.User) && !user.Equals(link.User, StringComparison.InvariantCultureIgnoreCase))
                {
                    re.ErrorCode = ErrorCode.UserPermissionError;
                    logger.Warn($"link.User:UserPermissionError");
                    return re;
                }
                if (!link.HasArchiveHistory)
                {
                    re.ErrorCode = ErrorCode.NoArchiveHistory;
                    logger.Warn($"NoArchiveHistory error");
                    return re;
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
        }


        public EndUserRestoreSettingResult GetEndUserSetting()
        {
            try
            {
                var settings = RestoreService.GetEndUserRestoreSetting();
                if (settings != null)
                {
                    return new EndUserRestoreSettingResult()
                    {
                        Footer = settings.Footer,
                        IsCustomizeStubRestorePage = settings.IsCustomizeStubRestorePage,
                        IsRestoreArchivedTier = settings.IsRestoreArchivedTier,
                        Logo = settings.Logo,
                        Message = settings.Message,
                        IsAllowRestore = settings.IsAllowRestore == Status.True,
                        PermissionSetting = new DocAveOnline.WebApi.Contracts.EndUserPermissionSetting()
                        {
                            TeamsAndGroup = (DocAveOnline.WebApi.Contracts.TeamsPermissionSetting)(int)settings.PermissionSetting.TeamsAndGroup,
                            SiteCollection = (DocAveOnline.WebApi.Contracts.SP365PermissionLevel)(int)settings.PermissionSetting.SiteCollection,
                            SiteCollectionSpecialGroupNames = settings.PermissionSetting.SiteCollectionSpecialGroupNames,
                            IsExportGroupTeamSite = settings.PermissionSetting.IsExportGroupTeamSite == Status.True,
                            IsRestoreGroupTeamSite = settings.PermissionSetting.IsRestoreGroupTeamSite == Status.True,
                            IsExportSiteCollection = settings.PermissionSetting.IsExportSiteCollection == Status.True,
                            IsRestoreSiteCollection = settings.PermissionSetting.IsRestoreSiteCollection == Status.True,
                            IsExportStubLink = settings.PermissionSetting.IsExportStubLink == Status.True,
                            IsRestoreStubLink = settings.PermissionSetting.IsRestoreStubLink == Status.True,
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
                    logger.Info($"RunArchiverContentDownloadJob  StubJobInfo : BackUpJobId:{contentInfo.BackUpJobId},PathMD5:{contentInfo.PathMD5} site url:{config.SiteUrl}");
                    EndUserRestoreItem item = new EndUserRestoreItem();
                    item.BackUpJobId = contentInfo.BackUpJobId;
                    item.PathMD5 = contentInfo.PathMD5;
                    item.FullPath = contentInfo.FileUrl;
                    item.IndexString = contentInfo.ExtensionString;
                    endUserRestoreJobConfig = new EndUserRestoreJobConfig();
                    endUserRestoreJobConfig.SiteUrl = config.SiteUrl;
                    endUserRestoreJobConfig.RestoreStorage = config.RestoreStorage;
                    endUserRestoreJobConfig.Items = new List<EndUserRestoreItem> { item };
                    endUserRestoreJobConfig.PermissionCheckType = CheckPermissionType.None;
                    endUserRestoreJobConfig.IntegrationModule = ArchiveIntegrationModules.Records;
                    endUserRestoreJobConfig.RestoreType = RestoreType.ToFileSystem;
                }
                else
                {
                    logger.Error("Cannot find restore items in ArchivedContentRestoreConfig.");
                    return new JobResult() { ErrorCode = ErrorCode.UnExpectedException };
                }

                var jobMessage = DaoArchiverService.RunEndUserRestoreNow(endUserRestoreJobConfig);
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
                else
                {
                    jobResult.ErrorCode = ErrorCode.UnExpectedException;
                    jobResult.ErrorMessage = e.Message;
                    logger.Error("RunArchiver EndUserRestoreJob job failed, {0}", e.ToString());
                }
            }
            return jobResult;
        }

        public JobResult RunArchivedContentExportJob(ExportArchivedContentConfig config)
        {
            logger.Info("RunArchivedContentExportJob.");
            JobResult jobResult = new JobResult();
            try
            {
                if (DaoArchiverService.IsExportSizeReachLimited())
                {
                    jobResult.ErrorCode = ErrorCode.ExportSizeLimitReached;
                    jobResult.LimitSize = 100;
                    logger.Error("current tenant export data size limit reached");
                    return jobResult;
                }
                //经与Recenter确认:Run Restore Job首先check DAO Setting，如果不允许Restore & Export，则直接返回Error Code.
                //1.Allow end users to restore/export archived data,总开关直接关闭时，直接返回Error Code
                //2.各个Source的 restore/export Setting，单独判断其打开关闭，如果不允许Restore & Export，则直接返回Error Code.
                #region Check DAO End User Restore Setting
                var endUserRestoreSetting = DaoArchiverService.GetEndUserRestoreSetting();
                if (endUserRestoreSetting.IsAllowRestore == Status.False)
                {
                    jobResult.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportTotalError;
                    logger.Error("RunArchivedContentExportJob:DAODoesNotAllowUserRestoreAndExportTotalError:IsAllowRestore[False].[");
                    return jobResult;
                }
                switch (config.ModuleType)
                {
                    case DocAveOnline.WebApi.Contracts.ModuleType.None:
                        if (endUserRestoreSetting.PermissionSetting.IsExportStubLink == Status.False)
                        {
                            jobResult.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportServiceError;
                            logger.Error("RunArchivedContentExportJob:DAODoesNotAllowUserRestoreOrExportServiceError:IsExportStubLink[False].");
                            return jobResult;
                        }
                        break;
                    case DocAveOnline.WebApi.Contracts.ModuleType.SharePointOnline:
                        if (endUserRestoreSetting.PermissionSetting.IsExportSiteCollection == Status.False)
                        {
                            jobResult.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportServiceError;
                            logger.Error("RunArchivedContentExportJob:DAODoesNotAllowUserRestoreOrExportServiceError.IsExportSiteCollection[False]");
                            return jobResult;
                        }
                        break;
                    case DocAveOnline.WebApi.Contracts.ModuleType.Microsoft365Groups:
                    case DocAveOnline.WebApi.Contracts.ModuleType.MicrosoftTeams:
                        if (endUserRestoreSetting.PermissionSetting.IsExportGroupTeamSite == Status.False)
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
                    endUserRestoreJobConfig.RestoreType = RestoreType.ToFileSystem;
                    endUserRestoreJobConfig.RunJobUser = config.Office365UserMail;
                    endUserRestoreJobConfig.Items = new List<EndUserRestoreItem>();
                    if (!string.IsNullOrEmpty(config.StubType))
                    {
                        endUserRestoreJobConfig.StubType = config.StubType;
                    }
                    if (config.IsSearchResultExport)
                    {
                        logger.Info($"RunArchivedContentExportJob site url:{config.SiteUrl}");
                        endUserRestoreJobConfig.PermissionCheckType = CheckPermissionType.SharePointSite;

                        foreach (var info in config.ExportContentInfos)
                        {
                            EndUserRestoreItem item = new EndUserRestoreItem();
                            item.PathMD5 = info.PathMD5;
                            endUserRestoreJobConfig.Items.Add(item);
                        }
                    }
                    else if (!config.IsSearchResultExport && config.ExportContentInfos.Count == 1)
                    {
                        var contentInfo = config.ExportContentInfos[0];
                        logger.Info($"RunArchivedContentExportJob  StubJobInfo : BackUpJobId:{contentInfo.BackUpJobId},PathMD5:{contentInfo.PathMD5} site url:{config.SiteUrl}");
                        EndUserRestoreItem item = new EndUserRestoreItem();
                        item.BackUpJobId = contentInfo.BackUpJobId;
                        item.PathMD5 = contentInfo.PathMD5;
                        item.FullPath = contentInfo.FullPath;

                        endUserRestoreJobConfig.Items.Add(item);
                        endUserRestoreJobConfig.PermissionCheckType = CheckPermissionType.StubRestoreLink;
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
                var jobMessage = DaoArchiverService.RunEndUserRestoreNow(endUserRestoreJobConfig);

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
                else
                {
                    jobResult.ErrorCode = ErrorCode.UnExpectedException;
                    jobResult.ErrorMessage = e.Message;
                    logger.Error("RunArchivedContentExportJob failed, {0}", e.ToString());
                }
            }
            return jobResult;
        }


        private void ValidateSPObjectNodeLevel(SharePointOnlineObject spObj)
        {
            switch (spObj.Level)
            {
                case SPObjectNodeLevel.SiteCollection:
                case SPObjectNodeLevel.Site:
                case SPObjectNodeLevel.List:
                    break;
                default:
                    throw new Exception(string.Format("Unsupported SharePoint object type. {0}", spObj.Level));
            }
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
