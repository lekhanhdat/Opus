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
using Aspose.Email.Clients.Exchange.WebService.Schema_2016;
using AvePoint.Api.Contract.Job;
using AvePoint.Archiver.Media;
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.AveModuleContract;
using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;
using AvePoint.Media.Common;
using AvePoint.Media.Service;
using AvePoint.Media.Service.ArchiverBackup.Backup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.GraphApi.GroupMailAndCalendar;
using AvePoint.RA.Common.GraphApi.GroupSite;
using AvePoint.RA.Common.GraphApi.Mail;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RAExchange.Common;
using AvePoint.RA.RAExchange.Discover;
using AvePoint.RA.RAExchange.Discover.DiscoverImpl;
using AvePoint.RA.RAExchange.Disposal.Action;
using AvePoint.RA.RAExchange.Disposal.Common;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Records.Core.Utilities.Extensions;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Restore;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Spreadsheet;
using ExchangeBackupUtility;
using Google.Apis;
using LS.SPWorkflowProcessor;
using Media.Service.ArchiverBackup.LogicBackup;
using Microsoft.Exchange.WebServices.Data;
using Newtonsoft.Json;
using PnP.Framework.Diagnostics;
using PnP.Framework.Entities;
using PnP.Framework.Extensions;
using RAArchiverCommon;
using RAExportCommon;
using RAManualApprovalCommon.Model;
using RazorEngine.Compilation.ImpromptuInterface.InvokeExt;
using Storage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using ActionTab = AvePoint.RA.Contract.RMWeb.JobMonitor.ActionTab;
using JobDetailsStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobDetailsStatus;

namespace AvePoint.RA.RAExchange.Disposal
{
    public class EXOBackupAction:RMEXODiscoverBase
    {
        //public List<ExchangeOnlineTreeNodeDto> ExoNodes { get; set; }
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(EXOBackupAction));
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private IEXOArchiverIndexSubInfoDao EXOArhciverSubInfo => PlatformWindsorManager.GetService<IEXOArchiverIndexSubInfoDao>();
        private IRMRemoteNodeDao RemoteNodeDao = PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private const string EXO = "EXO";
        protected IRMReportManager JobManagement = null;
        private IBatchDiscover discover = null;
        private RMEXODiscoverHelper discoverHelper = null;
        private bool isNullClassfication = false;
        protected Guid GroupId = Guid.Empty;
        protected Guid AOSMailboxId = Guid.Empty;
        private Microsoft.Exchange.WebServices.Data.SearchFilter mSearchFilter = null;
        //private List<Rule> allRulesList = null;
        //private Dictionary<Guid, RMRuleItemCollection> TermAndRulesMapping;
        //private Dictionary<Guid, string> ReviewedUserIdsAndNodeIdMapping;
        //private Dictionary<Guid, string> TermIdAndNameMapping;
        //private Dictionary<Guid, RMRule> mRuleCache = new Dictionary<Guid, RMRule>();
        //private RuleCollection mRuleCollection = null;
        private EXOConfiguration mConfiguration = null;
        private IBackupController backupController;
        private EXOExportBeforeArcInfo EXOExportBefArcInfo = null;
        private IEXOExport EXOExport = null;
        private bool skipRemoveAction = false;
        private int mThreadCount = 3;
        private long currentUtcTime;
        private List<Guid> rejectEmailId = new List<Guid>();
        private EXOBackupInfoSender exoSender = null;
        private List<ItemId> needToDeletedItems = new List<ItemId>();
        private Dictionary<string,List<string>> NeedToDeleteConversations = new Dictionary<string, List<string>>();
        private List<string> NeedToDeleteCalendarEvents = new List<string>();
        public bool ConversationHasError = false;
        public bool CalendarEventHasError = false;
        public AvePoint.GCommon.Contract.StorageOptimization.Object.Rule CurrentRule;
        public string SubjobId;
        public int SubjobNum;
        public string SubSubjobId;
        public GraphConversationAndCalendarManager GroupManager = null;
        public EWSManagement EWSManage = null;
        public string CurrentRuleId;
        public string CurrentRuleName;
        public long totalSize = 0;
        public EXOReportCenter _report;
        private bool mIsSupportGraphApi;

        public EXOBackupAction(ExchangeOnlineTreeNodeDto ndoe, IRMReportManager jobManagement,string ruleId,string ruleName) : base(ndoe)
        {
            CurrentRuleId = ruleId;
            CurrentRuleName = ruleName;
            _report = new EXOReportCenter(jobManagement);
            var mailboxAddress = TreeManagement.GetMailboxNode(ndoe)?.Name;
            var isSupportGraphApi = EXOGraphApiResolver.ShouldUseGraph(_keyValueDao, mailboxAddress, "", ndoe);
            mIsSupportGraphApi = isSupportGraphApi;
        }
        public string GetBaseClassEmailAddress()
        { 
            return MailboxAddress;
        }
        public override void Init()
        {
            GroupId = new Guid(TreeManagement.GetGroupNode(TreeNodeDto).ID);
            //AOSMailboxId = new Guid(base.MailboxGuid);
            if (discover is FullDiscover)
            {
                isNullClassfication = true;
            }
            TreeManagement tm = new TreeManagement();
            //var mailBoxStringId = tm.GetRealMailboxStringId(TreeNodeDto);
            mConfiguration = new EXOConfiguration();
            mConfiguration.HasUpgradeVEOV3 = VEOV3CommonMethod.HasUpgradedVEOV3();
            mConfiguration.CurrentRule = CurrentRule;
            mConfiguration.ContainerId = GroupId;
            mConfiguration.MailBoxTreeNodeId = TreeManagement.GetMailboxNode(TreeNodeDto).ID;
            mConfiguration.SubJobId = SubjobId;
            var archiveTemp = BackgroundSettings.GetInstance().ArchiveTemp;
            if (!System.IO.Directory.Exists(archiveTemp))
            {
                System.IO.Directory.CreateDirectory(archiveTemp);
            }
            AvePoint.Common.AveEnv.AgentJobFolder = archiveTemp;
            currentUtcTime = DateTime.UtcNow.Ticks;
            InitThreadCount();
            SubSubjobId = GenerageSubJobId(SubjobId, SubjobNum);
            GenerateFileSender();
        }
        private void GenerateFileSender()
        {
            IArchiverBackupDataWriter fileSender = null;
            try
            {
                MailboxAddress = TreeNodeDto.EmailAddress;
                //CommonPoolUserUtil.GetBPOSInfoForTeams(, true);
                var remoteEmail = RemoteNodeDao.GetO365TenantIdByName(TreeNodeDto.EmailAddress) ?? throw new Exception($"Cannot get remote email information by mailbox address:{TreeNodeDto.EmailAddress}");
                GroupManager = new GraphConversationAndCalendarManager(remoteEmail.TenantId, MailboxAddress, TreeNodeDto.ID);
                string groupOwnerEmail = GroupManager.GetGroupOwnersAsync().GetAwaiter().GetResult();
                if (!mIsSupportGraphApi)
                {
                    logger.Info("not enbale SupportGraphApi,still use ews to get items");
                    EWSManage = new EWSManagement(remoteEmail.TenantId, MailboxAddress, groupOwnerEmail);
                }
                logger.Info($"GenerateFileSender, mailbox address:{MailboxAddress}, isSupportGraphApi:{mIsSupportGraphApi}");
                MediaEnvironment.MediaServer = MediaServiceFactory.CreateMediaServer();//new MediaServer();
                MediaConfigInfo.CommonConfigInfo = MediaServiceFactory.CreateCommonConfigInfo(); //container.Resolve<CommonConfigInfo>("AvePoint.Media.Service.DomainModel.CommonConfigInfo");
                MediaConfigInfo.ArchiverConfigInfo = MediaServiceFactory.CreateArchiverConfigInfo(); //container.Resolve<ArchiverConfigInfo>("AvePoint.Media.Service.DomainModel.ArchiverConfigInfo");
                fileSender = new EXOArchiverBackupDataWriter();//container.Resolve<IArchiverBackupDataWriter>("AvePoint.Media.Service.ArchiverBackup.Backup.IArchiverBackupDataWriter");
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Can't initialize exo media information. Message:{0}", ex.ToString()));
                throw;
            }
            //ArchiverBackupRequest aRequest = mConfiguration.BackupRequest;
            ExchangeBackupRequest aRequest = new ExchangeBackupRequest();
            aRequest.RuleId = mConfiguration.CurrentRule.Id;
            //aRequest.SourceFlag = (int)sourceFlag;
            aRequest.JobId = SubSubjobId;
            StoragePolicyDto storage = mConfiguration.CurrentRule.StoragePolicyDto;
            aRequest.StoragePolicyId = storage.Id;
            aRequest.AchiverTime = mConfiguration.ArchiverUNCTime.Ticks;
            //set RetentionTimeSpan
            
            aRequest.LogicalDevice = storage.PrimaryStorage;

            var indexDeviceDto = StorageDeviceService.GetIndexDevice();
            aRequest.IndexLogicalDevice = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(indexDeviceDto);

            aRequest.CompressionType = mConfiguration.CurrentRule.ArchiverCompressionType;
            aRequest.EncryptionMethods = mConfiguration.CurrentRule.EncryptionMethods;
            aRequest.DataSecurity = mConfiguration.CurrentRule.ArchiverDataSecurity;                                                                                                          //if (aRequest.IncludeListView)   ////SAAS-12519 增加Archiver的Include List View功能
            aRequest.MailBoxAddress = MailboxAddress;                                                                                             //{
            if (mConfiguration.CurrentRule.DataEncryptionInfoWrapper != null)
            {
                aRequest.EncryptionInfo = mConfiguration.CurrentRule.DataEncryptionInfoWrapper.EncryptionInfo;
                DataEncryptionInfoManager.PutEncryptionInfo(mConfiguration.CurrentRule.DataEncryptionInfoWrapper.EncryptionInfo, mConfiguration.CurrentRule.DataEncryptionInfoWrapper.DynamicKey);
            }
            else
            {
                aRequest.EncryptionInfo = DataEncryptionInfoManager.DefaultEncryptionInfo;
            }
            logger.Info("ArchiverBackupRequest exo EncryptionInfo is:{0}.", aRequest.EncryptionInfo == null ? string.Empty : aRequest.EncryptionInfo.ToString());

            fileSender.OpenEXO(ConvertBackupRequestToJob(aRequest));
            GenerateSubInfo();
            exoSender = new EXOBackupInfoSender(fileSender);

        }
        private void GenerateSubInfo()
        {
            EXOArchiverIndexSubInfo subInfo = new EXOArchiverIndexSubInfo()
            {
                Id = Guid.NewGuid().ToString(),
                RetentionTime = mConfiguration.ArchiverUNCTime.Ticks,
                ArchiverTime = mConfiguration.ArchiverUNCTime.Ticks,
                SubSubJobId = this.SubSubjobId,
                StorageId = mConfiguration.CurrentRule.StoragePolicyDto.Id,
                MailBoxAddress = this.MailboxAddress,
                RuleId = CurrentRuleId,
                MergeIndexState = (int)MergeIndexState.None,
                KeepTime = 0,
                AgentDataSize = 0,
                SourceFlag = (int)SourceFlag.Exchange,
                SubJobId = this.SubjobId,
                DeletedStatus = (int)DeletedStatus.Normal,
                SoftDeleteTime = 0,
                DataFlag = (int)SourceFlag.Exchange,
                CurrentStorageId = mConfiguration.CurrentRule.StoragePolicyDto.Id,
                RetentionCount = 1,
                RetentionSource = (int)RetentionSourceFlag.None
            };
            EXOArhciverSubInfo.CreateEXOSubInfo(subInfo);
        }
        private string GenerageSubJobId(string parentJobId,int subJobNumber)
        {
            if (subJobNumber >= 1000)
            {
                return string.Format("{0}_{1:D4}", parentJobId, subJobNumber);
            }
            else
            {
                return string.Format("{0}_{1:D3}", parentJobId, subJobNumber);
            }
        }
        //private string GenerageSubJobId(string parentJobId)
        //{
        //    //subJobNumber++;
        //    //if (subJobNumber >= 1000)
        //    //{
        //    //    return string.Format("{0}_{1:D4}", parentJobId, subJobNumber);
        //    //}
        //    //else
        //    //{
        //    //    return string.Format("{0}_{1:D3}", parentJobId, subJobNumber);
        //    //}
        //}
        private ExchangeBackupJob ConvertBackupRequestToJob(ExchangeBackupRequest aRequest)
        {
            ExchangeBackupJob archiverBackupJob = new ExchangeBackupJob(aRequest);
            //if (this.mConfiguration.isRAMode)
            //{
            //    archiverBackupJob.IsRAJob = true;  //pass isRAJob to media message for new feature
            //    if (this.mConfiguration.BackgroundSettings.RecordsOutputStreamLevel == 0)
            //    {
            //        archiverBackupJob.OutFileLevelBlock = true;
            //    }
            //}
            //else
            //{
            //if (this.mConfiguration.BackgroundSettings.ArchiverOutputStreamLevel == 0)
            //{
            //    archiverBackupJob.OutFileLevelBlock = true;
            //}
            //}
            archiverBackupJob.CacheSetting = new CacheSettingDto { Extension = new CacheSettingExtension { Path = new List<PathMap>() } };
            DiskInfoDto disk = new DiskInfoDto()
            {
                Path = BackgroundSettings.GetInstance().ArchiveCache,
                Type = DeviceType.LocalPath,
                Password = string.Empty,
                UserName = string.Empty,
                Usage = null
            };
            archiverBackupJob.CacheSetting.Extension.Path.Add(new PathMap() { DiskInfo = disk });
            archiverBackupJob.CacheSetting.LimitFreeSpace = 1024 * 1024 * 1024;//1 GB
            archiverBackupJob.BackupTime = DateTime.UtcNow.Ticks;
            return archiverBackupJob;
        }
        private void InitThreadCount()
        {
            try
            {
                mThreadCount = int.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.EXO_DISCOVER_THREADS_LIMIT]);
            }
            catch (Exception e)
            {
                logger.Warn($"Error occurred while get max thread count, error:{e.ToString()}");
                mThreadCount = 3;
            }
        }
        public void Backup()
        {
            Init();
            logger.Info("Begin to scan mailbox.");
            bool GroupEnableMailBox = CheckGroupEnableMailBox();
            if (GroupEnableMailBox)
            {
                BackupMailbox();
                ProcessFolder();
                logger.Info("Finish scan mailbox.");
            }
            else
            {
                logger.Info($"current group not enable mail box,so no need to archiver it,address:{MailboxAddress}");
            }
        }
        private bool CheckGroupEnableMailBox()
        {
            return GroupManager.MailEnabled;
        }
        public void Close()
        {
            BackupCloseInfo closeInfo = new BackupCloseInfo()
            {
                ErrorMessage = "",
            };
            exoSender?.FileSender?.Close(closeInfo);
        }
        private void BackupMailbox()
        {
            logger.Info("Begin to backup mailbox.");
            exoSender.BackupEXOMailBoxHeader(TreeNodeDto);
            exoSender.BackupStream.BeginWriteMetadata();
            exoSender.BackupStream.WriteMetadata(AveMetadataType.ExchangeMailBox,"");
            exoSender.BackupStream.EndWriteMetadata();
            exoSender.BackupStream.FlushMetadata(0);
            exoSender.BackupTail();
            //AddMailBoxDetail(TreeNodeDto,JobDetailsStatus.Successful,"", ActionTab.Scan);
            AddMailBoxDetail(TreeNodeDto, JobDetailsStatus.Successful, "", ActionTab.Backup);
            logger.Info("Finish backup mailbox.");
        }
        private long ExportContent(IAveBackupStream stream,ThreadPost item)
        {
            try
            {
                var xBytes = Encoding.UTF8.GetBytes(SerializerHelper.SerializeByJsonConvert(item.Body));
                long size = xBytes.Length;
                stream.FlushMetadata(size);
                var buffer = stream.DataBuffer;
                stream.WriteContent(xBytes, 0, xBytes.Length);
                return size;
            }
            catch (Exception e)
            {
                logger.Warn("FS item backup \"ExportContent\" failed,Error:{0}", e);
                throw;
            }
        }
        private long ExportCalendarEventContent(IAveBackupStream stream, GroupCalendarEvent calendarEvent)
        {
            try
            {
                var xBytes = Encoding.UTF8.GetBytes(SerializerHelper.SerializeByJsonConvert(calendarEvent.Body));
                long size = xBytes.Length;
                stream.FlushMetadata(size);
                var buffer = stream.DataBuffer;
                stream.WriteContent(xBytes, 0, xBytes.Length);
                return size;
            }
            catch (Exception e)
            {
                logger.Warn("FS item backup \"ExportCalendarEventContent\" failed,Error:{0}", e);
                throw;
            }
        }
        private void ExportAttachmentContent(IAveBackupStream stream, PostAttachment attachment)
        {
            try
            {
                var xBytes = Encoding.UTF8.GetBytes(attachment.ContentBytes);
                long size = xBytes.Length;
                stream.FlushMetadata(size);
                var buffer = stream.DataBuffer;
                stream.WriteContent(xBytes, 0, xBytes.Length);
            }
            catch (Exception e)
            {
                logger.Warn("FS item backup \"ExportAttachmentContent\" failed,Error:{0}", e);
                throw;
            }
        }
        public void Delete()
        {

        }
        public void SetDiscoverObject(RMEXODiscoverHelper discoverHelper, IBatchDiscover discover)
        {
            this.discoverHelper = discoverHelper;
            this.discover = discover;
        }
        private void ProcessFolder()
        {

            logger.Info($"Begin processing mailbox : {MailboxAddress}.");
            //此处用GetItems 的值更合理，但是很多getitems是异步的，没有办法获取所有值

            using (var performance = new PerformanceScope("RMEXOEnforceRuleActionBase.ProcessFolder", "", true))
            {
                try
                {
                    BackupConversations();
                    BackupCalendar();
                }
                catch (Exception ex)
                {
                    //JobManagement.HasErrorNode = true;
                    logger.Error($"Error in process folder : {MailboxAddress}, reason : {ex.ToString()}.");
                    //JobManagement.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMArchiverActionJobDetails()
                    //{
                    //    SourceLocation = entity.DisplayPath,
                    //    FileSize = entity.Size,
                    //    Size = entity.Size.ToString(),
                    //    RuleName = ruleName,
                    //    Status = JobDetailsStatus.Successful,
                    //    FinishTime = DateTime.UtcNow.Ticks,
                    //    Level = ConvertStatisticsLevelToI18n(StatisticsLevel.TeamsGroup),
                    //    ActionTab = (int)actionTab,
                    //    Action = action,
                    //    Comment = errorMessage
                    //});
                    throw;
                }
            }
        }
        private void BackupConversations()
        {
            List<GroupConversation> conversations = null;
            int pageSize = 100;
            int skipSize = 0;
            using (var performance = new PerformanceScope("BackupConversations.ProcessConversationTotal", "", true))
            {
                do
                {
                    conversations = GroupManager.GetConversationsAsync(skipSize,pageSize).GetAwaiter().GetResult();
                    if (conversations != null && conversations.Count > 0)
                    {
                        logger.Info($"Begin processing conversations, current total count is : {conversations.Count}.skip size:{skipSize}");
                        _report._reportManager.IncreaseBase(conversations.Count);
                        foreach (var con in conversations)
                        {
                            using (var performance1 = new PerformanceScope("BackupConversations.ProcessOneConversation", "", true))
                            {
                                try
                                {
                                    exoSender.BackupEXOConversationHeader(con);
                                    exoSender.BackupStream.BeginWriteMetadata();
                                    exoSender.BackupStream.WriteMetadata(AveMetadataType.ExchangeFolder, SerializerHelper.SerializeByJsonConvert(con));
                                    exoSender.BackupStream.EndWriteMetadata();
                                    exoSender.BackupStream.FlushMetadata(0);
                                    exoSender.BackupTail();
                                    AddDeleteConversation(con.Id);
                                    ProcessConversationItems(con);
                                    AddConversationDetail(con, JobDetailsStatus.Successful, "");
                                }
                                catch (Exception e)
                                {
                                    ConversationHasError = true;
                                    logger.Error($"Error occurred while sync conversation:{con.Topic} Error:{e}");
                                    AddConversationDetail(con, JobDetailsStatus.Failed, "");
                                }
                            }
                        }
                    }
                    else
                    {
                        logger.Info("has process all conversations");
                    }
                    skipSize += pageSize;
                }
                while (conversations != null && conversations.Count == pageSize);
            }
        }
        private void BackupCalendar()
        {
            GroupCalendar calendar = null;
            try
            {
                calendar = GroupManager.GetCalendarAsync().GetAwaiter().GetResult();
                exoSender.BackupEXOCalendarHeader(calendar);
                exoSender.BackupStream.BeginWriteMetadata();
                exoSender.BackupStream.WriteMetadata(AveMetadataType.ExchangeCalendar, SerializerHelper.SerializeByJsonConvert(calendar));
                exoSender.BackupStream.EndWriteMetadata();
                exoSender.BackupStream.FlushMetadata(0);
                exoSender.BackupTail();
            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while get calendar,maybe calendar error:{e}");
                CalendarEventHasError = true;
                return;
            }
            using (var performance = new PerformanceScope("BackupCalendar.ProcessCalendarTotal", "", true))
            {
                int pageSize = 100;
                int skipSize = 0;
                List<GroupCalendarEvent> calendarEvents = null;
                do
                {
                    calendarEvents = GroupManager.GetCalendarEventsAsync(skipSize, pageSize).GetAwaiter().GetResult();
                    if (calendarEvents != null && calendarEvents.Count > 0)
                    {
                        logger.Info($"Begin processing calendar events, current total count is : {calendarEvents.Count}.skip size:{skipSize}");
                        _report._reportManager.IncreaseBase(calendarEvents.Count);
                        int processingItemIdx = 0;
                        foreach (var cal in calendarEvents)
                        {
                            processingItemIdx++;
                            logger.Info($"Process calendar event {processingItemIdx}/{calendarEvents.Count}, id:{cal.Id}");
                            Microsoft.Exchange.WebServices.Data.Appointment item = null;
                            if (!mIsSupportGraphApi)
                            {
                                item = EWSManage.GetCalendarEvent(cal.Subject);
                            }
                            using (var performance1 = new PerformanceScope("BackupCalendar.ProcessCalendarEvent", "", true))
                            {
                                try
                                {
                                    cal.CalendarId = calendar?.Id;
                                    try
                                    {
                                        if (!mIsSupportGraphApi)
                                        {
                                            cal.LegacyFreeBusyStatus = (int)item?.LegacyFreeBusyStatus;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Warn($"Error occurred while get calendar event legacy free busy status, calendar event id:{cal.Id}, error:{e}");
                                    }
                                    exoSender.BackupEXOEventHeader(cal);
                                    exoSender.BackupStream.BeginWriteMetadata();
                                    exoSender.BackupStream.WriteMetadata(AveMetadataType.ExchangeCalendarEvent, SerializerHelper.SerializeByJsonConvert(cal));
                                    exoSender.BackupStream.EndWriteMetadata();
                                    cal.TotalBodySize += ExportCalendarEventContent(exoSender.BackupStream, cal);
                                    //exoSender.BackupStream.FlushMetadata(0);
                                    exoSender.BackupTail();
                                    //if (cal.HasAttachments)
                                    //{
                                    //    ProcessCalendarEventAttachments(cal);
                                    //}
                                    AddDeleteEvent(cal.Id);
                                    AddEventDetail(cal, JobDetailsStatus.Successful, "");
                                }
                                catch (Exception e)
                                {
                                    CalendarEventHasError = true;
                                    logger.Error($"Error occurred while sync calendar event:{cal.Subject} Error:{e}");
                                    AddEventDetail(cal, JobDetailsStatus.Failed, "");
                                }
                            }
                        }
                    }
                    else
                    {
                        logger.Info("has process all conversations");
                    }

                    skipSize+= pageSize;
                }
                while (calendarEvents != null && calendarEvents.Count == pageSize);
            }
        }
        public IEnumerable<ExchangeFolder> GetFoldersInternal(ExchangeFolder folder)
        {
            using (var performance = new PerformanceScope("EXO.RMEXODataSync.GetSubFolders", "", true))
            {
                foreach (var f in folder.GetAllSubFolders())
                {
                    //在返回Folder 的时候需要计算一下当前Folder 的SyncState，来保证Folder 下次的Inc job 能根据Sync state 进行inc
                    f.GenerateCurrentSyncState();
                    yield return f;
                }
            }
        }
        private void ProcessConversationItems(GroupConversation conversation)
        {
            using (AveAppendableTaskExecutor taskExecutor = new AveAppendableTaskExecutor(MaxBackupItemsThreads))
            {
                taskExecutor.StartExecute();
                using (var performance = new PerformanceScope("RMEXOEnforceRuleActionBase.ProcessGroupedItems", "", true))
                {
                    List<ThreadPost> exchangeItems = null;
                    using (var performance1 = new PerformanceScope("RMEXOEnforceRuleActionBase.GetGroupItems", "", true))
                    {
                        exchangeItems = GroupManager.GetThreadPostsByConversationIdAsync(conversation.Id).GetAwaiter().GetResult();
                    }
                    bool jobNeedStop = false;
                    using (var performance1 = new PerformanceScope("BackupConversations.ProcessConversationItems", "", true))
                    {
                        List<Microsoft.Exchange.WebServices.Data.Item> items = null;
                        if (!mIsSupportGraphApi)
                        {
                            items = EWSManage.GetExchangeItems(conversation.Topic);
                        }
                        int processingItemIdx = 0;
                        foreach (var item in exchangeItems)
                        {
                            using (var performance2 = new PerformanceScope("BackupConversations.ProcessConversationOneItem", "", true))
                            {
                                try
                                {
                                    processingItemIdx++;
                                    logger.Info($"Begin processing item {processingItemIdx}/{exchangeItems.Count}, item is : {item.Id}.");
                                    item.Topic = conversation.Topic;
                                    try
                                    {
                                        if (!mIsSupportGraphApi)
                                        {
                                            var exchangeItem = items?.FirstOrDefault(i => i.Id.UniqueId.Replace('/', '-').Replace('+', '_') == item.Id);
                                            if (exchangeItem != null)
                                            {
                                                item.Importance = (Aspose.Email.MailPriority)exchangeItem.Importance;
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Warn($"Error occurred while get item importance, item id:{item.Id}, error:{e}");
                                    }
                                    try
                                    {
                                        ProcessItemsAsync(item).GetAwaiter().GetResult();
                                        conversation.TotalBodySize += item.TotalBodySize;
                                        AddDeleteConversationThread(item);
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Error($"error occured when ProcessConversationItems1,e: {e}");
                                        throw;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Error($"error occured when ProcessConversationItems,e: {ex}");
                                    throw;
                                }
                                finally
                                {
                                    if (jobNeedStop)
                                    {
                                        //JobManagement. = true;
                                        jobNeedStop = false;
                                    }
                                }
                            }
                        }
                    }
                }
                logger.Info($"Add items to task executor finished.");
            }
            logger.Info($"ProcessItems finish.");
        }
        public bool DeleteConversations()
        {
            try
            {
                if (!ConversationHasError)
                {
                    bool hasFailedItem = false;
                    foreach (var temp in NeedToDeleteConversations)
                    {
                        // TODO: Add delete conversation details ?
                        try
                        {
                            logger.Info($"delete conversation,id:{temp.Key}");
                            GroupManager.DeleteConversationById(temp);
                        }
                        catch (Exception ex)
                        {
                            hasFailedItem = true;
                            logger.Error($"delete conversation failed,id:{temp.Key}. {ex}");
                        }
                    }

                    if(hasFailedItem)
                    {
                        return false;
                    }
                    return true;
                }
                else
                {
                    logger.Error($"no need to delete conversation");
                }
            }
            catch (Exception e)
            {
                logger.Error($"delete conversation faied,e:{e}");
            }
            return false;
        }
        public bool DeleteCalendarEvents()
        {
            try
            {
                if (!CalendarEventHasError)
                {
                    bool hasFailedItem = false;
                    foreach (var temp in NeedToDeleteCalendarEvents)
                    {
                        try
                        {
                            logger.Info($"delete CalendarEvents,id:{temp}");
                            GroupManager.DeleteEventById(temp);
                        }
                        catch (Exception ex)
                        {
                            hasFailedItem = true;
                            logger.Error($"delete CalendarEvents failed,id:{temp}. {ex}");
                        }

                    }

                    if (hasFailedItem)
                    {
                        return false;
                    }
                    return true;
                }
                else
                {
                    logger.Error($"no need to delete CalendarEvents");
                }
            }
            catch (Exception e)
            {
                logger.Error($"delete CalendarEvents faied,e:{e}");
            }
            return false;
        }
        private void AddDeleteConversation(string conversationId)
        {
            if (!NeedToDeleteConversations.ContainsKey(conversationId))
            {
                NeedToDeleteConversations.Add(conversationId,new List<string>());
            }
        }
        private void AddDeleteConversationThread(ThreadPost threadPost)
        {
            if (!NeedToDeleteConversations[threadPost.ConversationId].Contains(threadPost.ThreadId))
            {
                NeedToDeleteConversations[threadPost.ConversationId].Add(threadPost.ThreadId);
            }
        }
        private void AddDeleteEvent(string eventId)
        {
            NeedToDeleteCalendarEvents.Add(eventId);
        }
        private void AddConversationDetail(GroupConversation item, JobDetailsStatus status, string action, string errorMessage = null)
        {
            JMArchiverActionJobDetails mArchiverActionJobDetails = new JMArchiverActionJobDetails
            {
                SourceLocation = item.Topic,
                //FileSize = entity.Size,
                Size = item.TotalBodySize.ToString(),//item.,
                RuleName = CurrentRuleName,
                Status = status,
                FinishTime = DateTime.UtcNow.Ticks,
                Level = ConvertStatisticsLevelToI18n(StatisticsLevel.Conversation),
                ActionTab = (int)ActionTab.Backup,
                Action = action,
                Comment = errorMessage
            };
            totalSize += item.TotalBodySize;
            _report.AddReportRecord(mArchiverActionJobDetails);
        }
        private void AddEventDetail(GroupCalendarEvent item, JobDetailsStatus status, string action, string errorMessage = null)
        {
            JMArchiverActionJobDetails mArchiverActionJobDetails = new JMArchiverActionJobDetails
            {
                SourceLocation = item.Subject,
                //FileSize = entity.Size,
                Size = item.TotalBodySize.ToString(),
                RuleName = CurrentRuleName,
                Status = status,
                FinishTime = DateTime.UtcNow.Ticks,
                Level = ConvertStatisticsLevelToI18n(StatisticsLevel.Event),
                ActionTab = (int)ActionTab.Backup,
                Action = action,
                Comment = errorMessage
            };
            totalSize += item.TotalBodySize;
            _report.AddReportRecord(mArchiverActionJobDetails);
        }
        private void AddMailBoxDetail(ExchangeOnlineTreeNodeDto item, JobDetailsStatus status, string action, ActionTab actionTab, string errorMessage = null)
        {
            JMArchiverActionJobDetails mArchiverActionJobDetails = new JMArchiverActionJobDetails
            {
                SourceLocation = item.EmailAddress,
                //FileSize = entity.Size,
                Size = "0",//item.,
                RuleName = CurrentRuleName,
                Status = status,
                FinishTime = DateTime.UtcNow.Ticks,
                Level =ConvertStatisticsLevelToI18n(StatisticsLevel.GroupMailbox),
                ActionTab = (int)actionTab,
                Action = action,
                Comment = errorMessage,
            };
            _report.AddReportRecord(mArchiverActionJobDetails);
        }
        private void AddPostAttachmentDetail(PostAttachment item, JobDetailsStatus status, string action,string topic, string errorMessage = null)
        {
            var attachmentName = string.IsNullOrWhiteSpace(item.Name) ? item.Id : item.Name;
            var sourceLocation = string.IsNullOrWhiteSpace(topic)
                ? attachmentName
                : string.IsNullOrWhiteSpace(attachmentName)
                    ? topic
                    : GCommon.Utility.SecurityUtils.SafeCombinePath(topic, attachmentName);

            JMArchiverActionJobDetails mArchiverActionJobDetails = new JMArchiverActionJobDetails
            {
                SourceLocation = sourceLocation ?? "Attachment",
                FileSize = item.Size,
                Size = item.Size.ToString(),
                RuleName = CurrentRuleName,
                Status = status,
                FinishTime = DateTime.UtcNow.Ticks,
                Level = ConvertStatisticsLevelToI18n(StatisticsLevel.Attachment),
                ActionTab = (int)ActionTab.Backup,
                Action = action,
                Comment = errorMessage
            };
            totalSize += item.Size;
            _report.AddReportRecord(mArchiverActionJobDetails);
        }
        public string ConvertStatisticsLevelToI18n(StatisticsLevel statisticsLevel)
        {
            var I18nStr = string.Empty;
            switch (statisticsLevel)
            {
                case StatisticsLevel.None:
                    break;
                case StatisticsLevel.TeamsGroup:
                    I18nStr = "RM_Archiver_JobDetailTeamsGroupLevel";
                    break;
                case StatisticsLevel.Channel:
                    I18nStr = "RM_Archiver_JobDetailChannelLevel";
                    break;
                case StatisticsLevel.ChannelConversation:
                    I18nStr = "RM_Archiver_JobDetailChannelConversationLevel";
                    break;
                case StatisticsLevel.GroupMailbox:
                    I18nStr = "RM_Archiver_JobDetailGroupMailboxLevel";
                    break;
                case StatisticsLevel.GroupMailboxItem:
                    I18nStr = "RM_Archiver_JobDetailGroupMailboxItemLevel";
                    break;
                case StatisticsLevel.Conversation:
                    I18nStr = "RM_Archiver_JobDetailConversationLevel";
                    break;
                case StatisticsLevel.Event:
                    I18nStr = "RM_Archiver_JobDetailEventLevel";
                    break;
                case StatisticsLevel.SiteCollection:
                    I18nStr = "RM_JS_Rule_ObjectLevel_SiteCollection";
                    break;
                case StatisticsLevel.Site:
                    I18nStr = "RM_JS_Rule_ObjectLevel_Site";
                    break;
                case StatisticsLevel.List:
                    I18nStr = "RM_JS_Rule_ObjectLevel_List";
                    break;
                case StatisticsLevel.Folder:
                    I18nStr = "RM_JS_Rule_ObjectLevel_Folder";
                    break;
                case StatisticsLevel.Item:
                    I18nStr = "RM_JS_Rule_ObjectLevel_Item";
                    break;
                case StatisticsLevel.Plan:
                    I18nStr = "RM_Archiver_JobDetailPlanLevel";
                    break;
                case StatisticsLevel.Task:
                    I18nStr = "RM_Archiver_JobDetailTaskLevel";
                    break;
                case StatisticsLevel.Attachment:
                    I18nStr = "RM_JS_Rule_ObjectLevel_Attachment";
                    break;
                case StatisticsLevel.Exception:
                    I18nStr = "RM_Archiver_JobDetailExceptionLevel";
                    break;
                default:
                    break;
            }

            return I18nStr;
        }
        private async System.Threading.Tasks.Task ProcessItemsAsync(ThreadPost item)
        {
            try
            {
                logger.Info($"Begin process grouped item.");
                try
                {
                    using (new RA.Common.PerformanceScope("RMEXOEnforceRuleActionBase.ProcessItem", addToStatistics: true))
                    {
                        exoSender.BackupEXOItemHeader(item);
                        exoSender.BackupStream.BeginWriteMetadata();
                        exoSender.BackupStream.WriteMetadata(AveMetadataType.ExchangeItem, SerializerHelper.SerializeByJsonConvert(item.GetProperties()));
                        exoSender.BackupStream.EndWriteMetadata();
                        item.TotalBodySize += ExportContent(exoSender.BackupStream, item);
                        //exoSender.BackupStream.FlushMetadata(0);
                        exoSender.BackupTail();
                        if (item.HasAttachments || (item.Body!=null && item.Body.Content.Contains("data-imagetype")))
                        {
                            using (new RA.Common.PerformanceScope("RMEXOEnforceRuleActionBase.ProcessAttachment", addToStatistics: true))
                            {
                                ProcessItemAttachments(item);
                            }
                        }
                        //AddDetail(item, JobDetailsStatus.Successful, "", "");
                    }
                }
                //catch (NotImplementedException ex)
                //{
                //    //JobManagement. = true;
                //    logger.Error($"An error occur in ProcessItem, item id {item?.Id}, reason : {ex.ToString()}.");
                //    //AddDetail(item, JobDetailsStatus.Failed, "", "RM_EXODisposal_Action_Scan", ex.Message);
                //}
                catch (Exception ex)
                {
                    logger.Error($"An error occur in ProcessItem, item id {item?.Id}, reason : {ex.ToString()}.");
                    string errorMessage = ex.Message;
                    //AddDetail(item, JobDetailsStatus.Failed, "", "RM_EXODisposal_Action_Scan", errorMessage);
                    throw;
                }
            }
            //catch (JobStopException)
            //{
            //    throw;
            //}
            catch (Exception e)
            {
                logger.Error($"Error occurred while sync item:{item.ConversationId} Error:{e.ToString()}");
                throw;
            }
            finally
            {
                _report._reportManager.Increase(1);
            }
        }
        private void ProcessItemAttachments(ThreadPost item)
        {
            var attachments = GroupManager.GetAttachmentByThreadPostAsync(item).GetAwaiter().GetResult();
            foreach (var atta in attachments)
            {
                atta.ParentItemId = item.Id;
                exoSender.BackupEXOItemAttachmentHeader(atta);
                exoSender.BackupStream.BeginWriteMetadata();
                exoSender.BackupStream.WriteMetadata(AveMetadataType.ExchangeAttachment, SerializerHelper.SerializeByJsonConvert(atta.GetProperties()));
                exoSender.BackupStream.EndWriteMetadata();
                ExportAttachmentContent(exoSender.BackupStream, atta);
                //exoSender.BackupStream.FlushMetadata(0);
                exoSender.BackupTail();
                AddPostAttachmentDetail(atta,JobDetailsStatus.Successful,"", item.Topic);
            }
        }
        private void ProcessCalendarEventAttachments(GroupCalendarEvent calendarEvent)
        {
            var attachments = GroupManager.GetCalendarEventAttachmentByEventAsync(calendarEvent).GetAwaiter().GetResult();
            foreach (var atta in attachments)
            {
                atta.ParentItemId = calendarEvent.Id;
                exoSender.BackupEXOItemAttachmentHeader(atta);
                exoSender.BackupStream.BeginWriteMetadata();
                exoSender.BackupStream.WriteMetadata(AveMetadataType.ExchangeAttachment, atta.GetProperties());
                ExportAttachmentContent(exoSender.BackupStream, atta);
                exoSender.BackupStream.EndWriteMetadata();
                exoSender.BackupStream.FlushMetadata(0);
                exoSender.BackupTail();
            }
        }
    }
}
