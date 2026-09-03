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
using AvePoint.Common.Portal;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup.Object;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object;
using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography.Encryption.KeyVault;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RACommonUtility.Encryption;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.SharePoint.Client;
using RAArchiverCommon;
using Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M365GroupTeam
{
    public class JobManagementService
    {
        protected static readonly AveLogger logger = AveLogger.GetInstance(typeof(JobManagementService));
        protected IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        protected RMAesEncryptorWrapper AesEncryptorWrapper => new();
        protected IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        public ERMessage GetRestoreMessage(string jobId, JobType _jobType, RestoreInfo setting, SPTreeNodeDto teamsNode, string groupSiteUrl, string m365TenantId)
        {
            var message = new ERMessage()
            {
                JobId = jobId,
                EmailBposInfoMap = new Dictionary<string, BposInfo>(),
                //TreeNodes = new List<ExchangeOnlineTreeNodeDto>(),
            };

            var resolveConflictType = setting.RestoreOption switch
            {
                RestoreOption.NotOverWrite => EOConflictResolutionType.Skip,
                RestoreOption.OverWrite => EOConflictResolutionType.Merge,
                RestoreOption.Append => EOConflictResolutionType.Merge,
                _ => EOConflictResolutionType.None,
            };

            //var restoreType = setting.RestoreTypeSelect switch
            //{
            //    AvePoint.GCommon.Contract.Server.Common.BackupDataSearch.RestoreType.InPlace => EORestoreType.InPlace,
            //    AvePoint.GCommon.Contract.Server.Common.BackupDataSearch.RestoreType.OutOfPlace => EORestoreType.OutOfPlace,
            //    //EORestoreType.ToStorage ??
            //    _ => EORestoreType.InPlace
            //};

            message.Config = new EORestoreConfig()
            {
                JobType = (int)_jobType,
                ContainerConflictResolution = resolveConflictType,
                ContentConflictResolution = resolveConflictType,

                //JobCategory =null;
                RestoreType = setting.RestoreTypeSelect == AvePoint.GCommon.Contract.Server.Common.BackupDataSearch.RestoreType.InPlace?EORestoreType.InPlace: EORestoreType.OutOfPlace,
                NotificationUsers = setting.NotificationUsers,
                IsMicrosoftTeams = teamsNode.Type == NodeType.O365TeamSites,
                IsO365Group = teamsNode.Type == NodeType.O365GroupSites,
                IsYammerGroup = false,
                
                //MailboxType = AvePoint.RA.Contract.Global.Object.MailboxType.Teams,

                IsSkipRestoreConversation = setting.RestoreConversationType == RestoreConversationType.Skip || setting.IsSkipRestoreConversation,
                RestoreConversationType = setting.RestoreConversationType,
                ReportOnlyHighLevel = false,
                NeedMergeConversation = false,
                SkippedErrorCodeList = new List<string>(),

                // Specify users when original teams's owner is deleted
                IsSpecifyUser = setting.IsSpecifyUser,
                SpecifyUserList = setting.SpecifyUserList,

                //For specify restore version number when choose Teams oop restore on restore center
                RestoreVersionOption = setting.RestoreVersionOption,
                KeepVersionsNumber = setting.KeepVersionsNumber,

            };

            var needUseImportApi = RService.RMKeyValueDao.GetValueByKey("UseTeamworkMigrationMode");
            if(needUseImportApi != null && bool.TryParse(needUseImportApi.Value, out var result) && result)
            {
                message.Config.UseImportApi = true;
                logger.Info("Use teamwork import API for teams restore.");
            }

            if (setting.StorageDeviceDto != null)
            {
                var storage = StorageDeviceService.GetStorageDeviceById(setting.StorageDeviceDto.Id, needDecryptSecert: true);
                if (storage != null)
                {
                    var storagePolicy = ConvertStorageDeviceDtoToPhysicalDeviceDto(storage);
                    if (storagePolicy != null)
                    {
                        message.Config.DestinationFSDevice = storagePolicy;
                        logger.Info("Set outplace physical device: {0}.", storagePolicy.Name);
                        var zipPassword = GeneratePassword(13, true, false, true, true);
                        message.Config.ZipFilePassword = zipPassword;
                        var encryptPassword = AesEncryptorWrapper.Encrypt(zipPassword);
                        DownloadDataInfoDao.CreateZipPasswordInfo(new RMDownloadDataInfo() { Name = encryptPassword, JobId = jobId, FileDownloadTime = DateTime.UtcNow.Ticks, DownloadType = DownloadContentType.ZipPasswordInfo });
                    }
                }
            }
            var exoTreeNode = RMDtoConverter.ConvertRMSPTree2EXOTreeNodeDto(RMDtoConverter.ConvertSPTree2RMTree(teamsNode));
            exoTreeNode.Level = NodeLevel.ExchangeOnlineMailbox;

            ArchiverSiteMasterIndexContract index = new ArchiverSiteMasterIndexContract()
            {
                SiteURL = teamsNode.Name
            };

            var site = new RemoteSiteCollection()
            {
                url = groupSiteUrl,
                TenantId = m365TenantId,
            };

            message.BposInfo = CommonPoolUserUtil.GetBPOSInfoForTeams(site, true);
            message.ConfigForMedia = AssembleRestoreRequest(index, _jobType == JobType.TeamsOutPlaceRestore);
            message.ConfigForMedia.CacheLocation = CreateCacheSettingDto();
            message.ConfigForMedia.TreeRoot = exoTreeNode;

            return message;
        }
        private string GeneratePassword(int intLength, bool booNumber, bool booSign, bool booSmallword, bool booBigword)
        {
            //定义
            int intResultRound = 0;
            string strB = "";
            while (intResultRound < intLength)
            {
                //生成随机数A，表示生成类型
                //1=数字，2=符号，3=小写字母，4=大写字母
                int intA = SecurityUtils.GetRandomNumber(1, 5);
                //如果随机数A=1，则运行生成数字
                //生成随机数A，范围在0-10
                //把随机数A，转成字符
                //生成完，位数+1，字符串累加，结束本次循环
                if (intA == 1 && booNumber)
                {
                    intA = SecurityUtils.GetRandomNumber(0, 10);
                    strB = intA.ToString(CultureInfo.InvariantCulture) + strB;
                    intResultRound = intResultRound + 1;
                    continue;
                }
                //如果随机数A=2，则运行生成符号
                //生成随机数A，表示生成值域
                //1：33-47值域，2：58-64值域，3：91-96值域，4：123-126值域
                if (intA == 2 && booSign)
                {
                    intA = SecurityUtils.GetRandomNumber(1, 5);

                    //如果A=1
                    //生成随机数A，33-47的Ascii码
                    //把随机数A，转成字符
                    //生成完，位数+1，字符串累加，结束本次循环
                    if (intA == 1)
                    {
                        intA = SecurityUtils.GetRandomNumber(33, 48);
                        strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }

                    //如果A=2
                    //生成随机数A，58-64的Ascii码
                    //把随机数A，转成字符
                    //生成完，位数+1，字符串累加，结束本次循环
                    if (intA == 2)
                    {
                        intA = SecurityUtils.GetRandomNumber(58, 65);
                        strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }

                    //如果A=3
                    //生成随机数A，91-96的Ascii码
                    //把随机数A，转成字符
                    //生成完，位数+1，字符串累加，结束本次循环
                    if (intA == 3)
                    {
                        intA = SecurityUtils.GetRandomNumber(91, 97);
                        strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }

                    //如果A=4
                    //生成随机数A，123-126的Ascii码
                    //把随机数A，转成字符
                    //生成完，位数+1，字符串累加，结束本次循环
                    if (intA == 4)
                    {
                        intA = SecurityUtils.GetRandomNumber(123, 127);
                        strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }
                }
                //如果随机数A=3，则运行生成小写字母
                //生成随机数A，范围在97-122
                //把随机数A，转成字符
                //生成完，位数+1，字符串累加，结束本次循环
                if (intA == 3 && booSmallword)
                {
                    intA = SecurityUtils.GetRandomNumber(97, 123);
                    strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                    intResultRound = intResultRound + 1;
                    continue;
                }

                //如果随机数A=4，则运行生成大写字母
                //生成随机数A，范围在65-90
                //把随机数A，转成字符
                //生成完，位数+1，字符串累加，结束本次循环
                if (intA == 4 && booBigword)
                {
                    intA = SecurityUtils.GetRandomNumber(65, 89);
                    strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                    intResultRound = intResultRound + 1;
                }
            }
            return strB;
        }
        private PhysicalDeviceDto ConvertStorageDeviceDtoToPhysicalDeviceDto(StorageDeviceDto storageDevice)
        {
            var physical = new PhysicalDeviceDto()
            {
                Id = storageDevice.Id,
                ConnectionString = storageDevice.ConnectionString,
                ModifyTime = storageDevice.ModifyTime,
                Type = storageDevice.Type,
            };
            return physical;
        }
        private ExchangeOnlineTreeNodeDto Build4RootNode(ExchangeOnlineTreeNodeDto node)
        {
            if (node.Level == NodeLevel.ExchangeOnlineFarm)
            {
                var root = new ExchangeOnlineTreeNodeDto
                {
                    Level = NodeLevel.Root,
                    Name = "Root",
                    Children = new List<ExchangeOnlineTreeNodeDto> { node },
                    ChildrenCount = 1,
                    ChildrenLoaded = true
                };
                node.Parent = root;
                return root;
            }
            return node;
        }

        private void GetBPOSInfo(ExchangeOnlineTreeNodeDto treeNode, Dictionary<string, BposInfo> bposInfo)
        {
            using (var performance = new PerformanceScope("EXO.TreeManagement.GetBPOSInfo", "", true))
            {
                if (treeNode.Level == NodeLevel.ExchangeOnlineMailbox)
                {
                    var info = RABrowserClient.GetBPOSInfoByEXONode(treeNode);
                    bposInfo[treeNode.Name] = info;
                }
                else
                {
                    GetBPOSInfo(treeNode.Parent, bposInfo);
                }
            }
        }

        public ExchangeRestoreRequest AssembleRestoreRequest(ArchiverSiteMasterIndexContract queryIndex, bool isLoadSiteData)
        {
            var indexes = RService.CommonSiteMasterIndexService.GetSiteCollectionWithSubInfos(queryIndex);
            logger.Info($"get masterIndex from CommonSiteMasterIndexService,indexes is null?{indexes==null},count:{indexes?.Count}");
            if (!this.ValidateDataForRestore(indexes))
            {
                throw new AveException("The Archiver data has already been deleted by the specified Archiver Retention rules.");
            }

            if (indexes.Count > 1)
            {
                logger.Warn($"There are multiple indexes found for the same jobId: {string.Join(',', indexes.Select(i => $"{i.JobId}({i.ArchiverTime})"))}. Only the most recent one will be used for restore.");
            }


            var index = indexes.FirstOrDefault();

            if (isLoadSiteData)
            {
                var siteData = RService.CommonSiteMasterIndexService.LoadSiteMasterIndexByJobIdOrTeamsGroup(queryIndex.SiteURL, index.JobId);
                if(siteData != null && siteData.Count > 0)
                {
                    indexes.AddRange(siteData);
                }
            }

            ExchangeRestoreRequest request = new ExchangeRestoreRequest
            {
                JobId = index.JobId,
                BackupCycleId = index.JobId,
                BackupJobId = index.JobId,
                BackupPlanId = index.PlanId,
                BackupTime = index.ArchiverTime,
                OnlyOneJob = false,
                IsSoftDeleted= index.IsSoftDeleted,
                IsIncludeDeletedContents = false,
                FromSentDate = null,
                ToSentDate = null,
                TreeRoot = new ExchangeOnlineTreeNodeDto()
                {

                },
                IndexStorageInfoMap = new Dictionary<string, string>()

                //SourceDataType = plan.SourceDataType,
            };

            var indexDeviceDto = RService.StorageDeviceService.GetIndexDevice();
            if (indexDeviceDto == null)
            {
                throw new Exception("index device not exist");
            }
            request.IndexDBLogicalDevice = RAStorageUtil.ConvertStorageDeviceDtoToLogicalDeviceDto(indexDeviceDto);

            var dataDeviceList = GetAllStorageLogicalDevices(indexes)
                .Select(i => RAStorageUtil.ConvertStorageDeviceDtoToLogicalDeviceDto(RService.StorageDeviceService.GetStorageDeviceById(i, needDecryptSecert: true)));
            request.LogicalDevice = new LogicalDeviceDto();
            dataDeviceList.ForEach(logicalDevice =>
            {
                logicalDevice.PhysicalDrives.ForEach(physicalDevice =>
                {
                    request.LogicalDevice.PhysicalDrives.Add(physicalDevice);
                });
            });

            HashSet<string> GetAllStorageLogicalDevices(List<ArchiverSiteMasterIndexContract> indexes)
            {
                HashSet<string> logicalDeviceIdList = new HashSet<string>();
                foreach (var index in indexes)
                {
                    foreach (var subInfo in index.SubInfo)
                    {
                        logicalDeviceIdList.Add(string.IsNullOrEmpty(subInfo.CurrentStorageId) ? subInfo.StorageInfo : subInfo.CurrentStorageId);
                    }
                }
                return logicalDeviceIdList;
            }


            request.RestoreSecurityInfos = (new DataEncryptionHelper()).GetRestoreSecurityInfoList(indexes);
            

            return request;
        }


        public ExchangeOnlineMessage GetBackupMessage(string jobId, JobType jobType, RMSPTreeNode teamsTreeNode)
        {
            var backupMessage = new ExchangeOnlineMessage()
            {
                JobId = jobId,
            };

            try
            {
                //if (AvePoint.RA.Common.JobService.JobServiceUtility.IsSubJob(jobId))
                //{
                //    IRMSubJobDao SubJobDao = new RMSubJobDao();
                //    IJobMonitorDao JobMonitorDao = new JobMonitorDao();
                //    //从子job的Context中获取当前需要处理的节点.
                //    RMSubJob subJobWithContext = SubJobDao.GetSubJob(jobId, true);
                //    //if (!onlyStorageInSubJobTable.Contains(jobType))
                //    //{
                //    //    MainJobId = subJobWithContext.ParentId;
                //    //    MainJobStartTime = JobMonitorDao.GetJob(subJobWithContext.ParentId).StartTime;
                //    //}

                //    var JobContextSetting = subJobWithContext.JobContext?.Settings;
                //    var JobContextContent = subJobWithContext.JobContext?.Content;

                //    backupMessage.PlanId = subJobWithContext.ParentId;

                //    foreach (var tempNode in tempNodes)
                //    {
                //        var nodeDto = RMDtoConverter.ConvertRMExchangeTree2TreeNodeDto(tempNode);
                //        GetBPOSInfo(nodeDto, backupMessage.EmailBposInfoMap);
                //        backupMessage.TreeNodes.Add(nodeDto);
                //    }
                //}

                //backupMessage.TreeNode = backupMessage.TreeNodes.FirstOrDefault();
                //backupMessage.BposInfo = backupMessage.EmailBposInfoMap.FirstOrDefault().Value;
                //logger.Info("Start to receive request object from control.");
                //var jobMessage = JobQueueMessageManager.Get(jobId);
                //IdentityManager.IdentityMode = IdentityMode.Process;
                //IdentityManager.IdentityType = MicroKernelConstant.IdentityTypeGroupId;
                //IdentityManager.IdentityContent = jobMessage.JobTenantInfo.TenantId;
                //backupMessage = EBJobManagementService.GetExchangeOnlineBackupJobMessage(System.IO.Path.Combine(AveEnv.GetAgentTempFolder(ContextLevel.Process),jobMessage.SubJobId), jobMessage);
                //OverSizeIndexCacheParameter.Setup(backupMessage?.ConfigForMedia?.OversizeIndexDBCacheInfo, jobMessage.JobTenantInfo.TenantId);

                backupMessage.TreeNode = RMDtoConverter.ConvertRMSPTree2EXOTreeNodeDto(teamsTreeNode);
                (var remoteSiteCollection, _) = PlatformWindsorManager.GetService<IRMRemoteNodeDao>().GetTeamsGroupAndChannelsCollectionByTeamsId(teamsTreeNode.TeamsId);
                if (remoteSiteCollection == null)
                {
                    throw new Exception("Teams site not exists");
                }

                if (backupMessage.TreeNode.Level == NodeLevel.Office365GroupEntire)
                {
                    backupMessage.TreeNode.MailboxType = remoteSiteCollection.SiteCollectionType switch
                    {
                        SiteCollectionType.Teams => MailboxType.Teams,
                        SiteCollectionType.Group => MailboxType.Group,
                        _ => MailboxType.None
                    };
                }

                backupMessage.Config = new ExchangeOnlineBackupConfig()
                {
                    JobType = (int)jobType,
                    BackupType = EOBackupType.Full,
                    BackupLevel = EOBackupLevel.MailBox,
                    BackupPrivateChannel = true,
                    BackupSharedChannel = false,
                    BackupPlanner = false,
                    GenerateFullTextIndex = false,
                    IsTestRun = false,
                    PreviousFBJobId = "",
                    UserArchiverImportFile = teamsTreeNode.UserArchiverImportFile,
                    SupportLockedSite = teamsTreeNode.SupportLockedSite,
                    SupportArchivedTeams = teamsTreeNode.SupportArchivedTeams,
                    //CompressionType = "",
                    //DataSecurity = "",
                    //EncryptionMethods = EOEncryptionMethods.BLOWFISH_ENCRYPTION,
                };

                backupMessage.ConfigForMedia = new AvePoint.GCommon.Contract.Media.TCPRequest.Backup.ExchangeBackupRequest()
                {

                };

                bool isLimitAppTypes = false;
                if (jobType == JobType.TeamsArchiverBackup || jobType == JobType.TeamsPreScan || jobType == JobType.SpecifyTeamsArchiverBackup)
                {
                    try
                    {
                        var ruleManagement = new Office365GroupBackup.RuleManagement(backupMessage);
                        if (ruleManagement.AppliedRules.Where(r => r.PolicyLevel == AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.Teams).Count() > 0)
                        {
                            isLimitAppTypes = true;
                            logger.Info($"Current job type:{jobType} contains teams level rule so it will used Delegated app.Rule Count:{ruleManagement.AppliedRules.Count}.");
                        }
                        else
                        {
                            logger.Info($"Current job type:{jobType} does contains teams level rule.Rule Count:{ruleManagement.AppliedRules.Count}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        isLimitAppTypes = true;
                        logger.Warn($"Failed get teams rule.Message:{ex}");
                    }
                }
                else if (jobType == JobType.SpecifyTeamsArchiverBackup)
                {
                    isLimitAppTypes = true;
                    logger.Info($"Current job type:{jobType} is GAO teams level rule. so use delegate app profile.");

                }
                backupMessage.BposInfo = CommonPoolUserUtil.GetBPOSInfoForTeams(remoteSiteCollection, isLimitAppTypes);
                logger.Info("Receive request object successfully.");
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while receiving request object from control. Reason: {0}", ex.ToString());
                throw;
            }



            return backupMessage;
        }

        private CacheSettingDto CreateCacheSettingDto()
        {
            var cacheSetting = new CacheSettingDto { Extension = new CacheSettingExtension { Path = new List<PathMap>() } };
            cacheSetting.LimitFreeSpace = 1024 * 1024 * 1024;//1 GB
            cacheSetting.Extension.Path.Add(new PathMap()
            {
                DiskInfo = new DiskInfoDto()
                {
                    Path = BackgroundSettings.GetInstance().ArchiveCache,
                    Type = DeviceType.LocalPath,
                    Password = string.Empty,
                    UserName = string.Empty,
                    Usage = null
                }
            });
            return cacheSetting;
        }


        protected bool ValidateDataForRestore(List<ArchiverSiteMasterIndexContract>? indexWithSubInfos)
        {
            if (indexWithSubInfos == null || indexWithSubInfos.Count == 0)
            {
                return false;
            }
            foreach (ArchiverSiteMasterIndexContract index in indexWithSubInfos)
            {
                if (!index.SubInfo.IsNullOrEmpty())
                {
                    return true;
                }
            }
            return false;
        }
    }
}
