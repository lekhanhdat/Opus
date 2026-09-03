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
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.Media.Service.ArchiverBackup.Backup;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Encryption;
using AvePoint.RA.RAExchange.Common;
using AvePoint.RA.RAExchange.Discover;
using AvePoint.RA.RAExchange.Disposal.MergeIndex;
using AvePoint.RA.SharePoint.ArchiverCommon;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.Exchange.WebServices.Data;
using RAArchiverCommon.TeamsController;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rule = AvePoint.GCommon.Contract.StorageOptimization.Object.Rule;

namespace AvePoint.RA.RAExchange.Disposal
{
    public class EXOBackupProcessor
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(EXOBackupProcessor));
        private IRuleManagerService RuleManagerService => PlatformWindsorManager.GetService<IRuleManagerService>();
        private IGlobalStorageSettingDao GlobalStorageSettingDao => PlatformWindsorManager.GetService<IGlobalStorageSettingDao>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private ISettingProfilesDao SettingProfileDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();
        private IEXOArchiverIndexSubInfoDao EXOArhciverSubInfo => PlatformWindsorManager.GetService<IEXOArchiverIndexSubInfoDao>();
        private RMAesEncryptorWrapper AesEncryptorWrapper => new();
        private IRMSubJobDao mSubJobDao { set; get; }
        public IRMReportManager exoReportManager { set; get; }
        public IRMSubJobDao SubJobDao
        {
            get
            {
                if (mSubJobDao == null)
                {
                    mSubJobDao = (IRMSubJobDao)PlatformWindsorManager.GetService(typeof(IRMSubJobDao));
                }
                return mSubJobDao;
            }
        }
        public ActionStatistics ScanActionStatistics;
        public ActionStatistics BackupActionStatistics;
        public ActionStatistics OtherActionStatistics;
        public EXOBackupProcessor()
        {
            exoReportManager = ReportMangerFactory.Instance.ReportManager;
        }
        public void RunNow(string subJobId,string ruleId)
        {
            //ruleId = "b781ef9a-3b35-4f54-90a5-d74cc2c1b169";//DEV test
            //allRecordsRules = RuleManagerService.GetRulesFromDA();
            JobManagement jm = JobManagement.GetInstanceV2(subJobId, JobType.EXORecordsDisposal);
            List<Rule> currentRule = new List<Rule>();
            if (ruleId == RecordsConstants.FAKE_SPECIFY_TEAMS_RULE_ID)
            {
                currentRule.Add(RuleManagerService.GetSpecifyTeamsArchiverBackupRule());
            }
            else
            {
                currentRule = RuleManagerService.GetRulesByIds(new List<Guid> { new Guid(ruleId) });
            }
                
            using (var performance = new PerformanceScope("RMEXOEnforceRuleActionProcessor.RunNow", "", true))
            {
                try
                {
                    //ArchiverCommonStaticMethod.InitExchangeOnlineSetting();
                    List<ExchangeOnlineTreeNodeDto> nodes = new List<ExchangeOnlineTreeNodeDto>();
                    if (AvePoint.RA.Common.JobService.JobServiceUtility.IsSubJob(subJobId))
                    {
                        //从子job的Context中获取当前需要处理的节点.   更新进度和状态用JobInfoUpdater
                        RMSubJob subJobWithContext = SubJobDao.GetSubJob(subJobId, true);
                        List<RMEXOTreeNode> tempList = SerializerHelper.DeserializeByDataContractSerializer<List<RMEXOTreeNode>>(subJobWithContext.JobContext.Settings);
                        tempList.ForEach(node => nodes.Add(RMDtoConverter.ConvertRMExchangeTree2TreeNodeDto(node)));
                    }
                    var rule = currentRule.FirstOrDefault();
                    if (nodes != null && nodes.Count > 0)
                    {
                        int subjobNumber = 0;
                        foreach (var node in nodes)
                        {
                            using (new PerformanceScope("EnforceRuleActionProcessExoNodeAll"))
                            {
                                EXOBackupAction exoAction = null;
                                bool hasError = false;
                                logger.Info($"Process node : {node?.ObjectId}, node id : {node.ID}.");
                                try
                                {
                                    var teamsRule = rule.TeamsRule;
                                    RebuildStoragePolicyDto(teamsRule);
                                    exoAction = GenerateEnforceRuleActionObject(node, exoReportManager, teamsRule, subJobId, subjobNumber, ruleId, rule.Name);
                                    exoAction.Backup();
                                    subjobNumber++;
                                }
                                catch (ServiceRequestException ex)
                                {
                                    logger.Error($"exo RunNow excpetion,e:{ex}");
                                    //jm.HasErrorNode = true;
                                    //var comment = ex.ToString();
                                    //if (ex.Message.Contains("401"))
                                    //{
                                    //    comment = "RM_JS_Common_PasswordError";
                                    //}
                                    //jm.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXOEnforceRuleActionJobDetails()
                                    //{
                                    //    Action = "RM_EXODisposal_Action_Scan",
                                    //    ObjectName = node.Name,
                                    //    FullPath = node.EmailAddress + "\\" + node.FullPath,
                                    //    ItemType = JobReportUtility.ConvertItemTypeForDetails(node.Level),
                                    //    Comment = comment,
                                    //    Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed,
                                    //});
                                    //logger.Error($"Error in process node {node.Name}, reason : {ex.ToString()}.");
                                    hasError = true;
                                }
                                //catch (JobStopException)
                                //{
                                //    throw;
                                //}
                                catch (Exception ex)
                                {
                                    hasError = true;
                                    //if (ex.Message == "243")
                                    //{
                                    //    logger.Error($"[DirtyData] Mailbox: {node.Name}, id: {node.ID} is deleted, ErrorCode:[{ex.Message}]");
                                    //    return;
                                    //}

                                    //jm.HasErrorNode = true;
                                    //jm.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXOEnforceRuleActionJobDetails()
                                    //{
                                    //    Action = "RM_EXODisposal_Action_Scan",
                                    //    ObjectName = node.Name,
                                    //    FullPath = node.EmailAddress + "\\" + node.FullPath,
                                    //    ItemType = JobReportUtility.ConvertItemTypeForDetails(node.Level),
                                    //    Comment = ex.InnerException?.Message ?? ex.Message,
                                    //    Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed,
                                    //});
                                    logger.Error($"Error in process node {node.Name}, reason : {ex}.");
                                }
                                finally
                                {
                                    exoAction.Close();
                                    JMArchiverActionJobDetails mArchiverActionJobDetails = new JMArchiverActionJobDetails
                                    {
                                        SourceLocation = node.Name,
                                        //FileSize = entity.Size,
                                        Size = exoAction.totalSize.ToString(),//item.,
                                        RuleName = rule.Name,
                                        Status = JobDetailsStatus.Successful,
                                        FinishTime = DateTime.UtcNow.Ticks,
                                        Level = "RM_Archiver_JobDetailGroupMailboxLevel",
                                        ActionTab = (int)ActionTab.Action,
                                        Action = "SO_Action_Delete",
                                        //Comment = errorMessage,
                                    };
                                    try
                                    {
                                        logger.Info($"Start to merge index for {node.Name}.");
                                        var indexDeviceDto = StorageDeviceService.GetIndexDevice();
                                        var indexDevice = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(indexDeviceDto);
                                        EXOMergeIndexJobInfo jobInfo = new EXOMergeIndexJobInfo()
                                        {
                                            IndexLogicalDevice = indexDevice,
                                            JobDto = new GCommon.Contract.Server.ControlPanel.Object.BaseJobDto(),
                                        };
                                        EXOArchiverMergeIndexJobHandler mergeHandler = new EXOArchiverMergeIndexJobHandler();
                                        mergeHandler.PerformMergeIndexSubJob(jobInfo, exoAction.SubSubjobId, exoAction.GetBaseClassEmailAddress());
                                        EXOArhciverSubInfo.UpdateEXOSubInfoMergeStatusBySubSubJobId(exoAction.SubSubjobId, (int)MergeIndexState.Succeed);

                                        bool allConversationsDeleted = false;
                                        bool allCalendarEventsDeleted = false;
                                        if (!hasError)
                                        {
                                            try
                                            {
                                                logger.Info($"Delete conversations and calendar events for {node.Name}.");
                                                using (var performance1 = new PerformanceScope("DeleteConversationAndEvents", "", true))
                                                {
                                                    using (var performance2 = new PerformanceScope("DeleteConversationTotal", "", true))
                                                    {
                                                        allConversationsDeleted = exoAction.DeleteConversations();
                                                    }
                                                    using (var performance3 = new PerformanceScope("DeleteEventsTotal", "", true))
                                                    {
                                                        allCalendarEventsDeleted = exoAction.DeleteCalendarEvents();
                                                    }
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                logger.Error($"Error in delete conversations and calendar events for {node.Name}, reason : {e}.");
                                            }
                                        }

                                        //if(hasError || !allConversationsDeleted || !allCalendarEventsDeleted)
                                        if (hasError || exoAction.ConversationHasError || exoAction.CalendarEventHasError)
                                        {
                                            mArchiverActionJobDetails.Status = JobDetailsStatus.Failed;
                                            mArchiverActionJobDetails.Size = "0";
                                            mArchiverActionJobDetails.Comment = "StorageOptimization.Service_2E0C0627-857A-4FB1-AA89-A73C6E3859C1";
                                            logger.Warn("there exist error,will not delete");
                                        }
                                        else
                                        {
                                            TeamsDisposalState.IsExchangeDisposalSuccessful = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.Error($"Error in process node {node.Name}, reason : {ex.ToString()}.");
                                        EXOArhciverSubInfo.UpdateEXOSubInfoMergeStatusBySubSubJobId(exoAction.SubSubjobId, (int)MergeIndexState.Failed);
                                    }
                                    finally
                                    {
                                        exoAction._report.AddReportRecord(mArchiverActionJobDetails);
                                        ScanActionStatistics = exoAction._report.ScanActionStatistics;
                                        BackupActionStatistics = exoAction._report.BackupActionStatistics;
                                        OtherActionStatistics = exoAction._report.OtherActionStatistics;
                                    }
                                    EXOLiteDBWrapper.CreateInstance(EXOPathUtil.GetDisposalDueRecordDBPath(subJobId)).DeleteDBFile();
                                }
                            }
                        }
                    }
                    else
                    {
                        logger.Info("Tree node is null.");
                    }

                }
                //catch (JobStopException ex)
                //{
                //    logger.Warn($"Job stop {ex}");
                //    jm.JobHasStopped = true;
                //}
                catch (Exception exception)
                {
                    logger.Error($"Error in job level, reason : {exception.ToString()}");
                    //jm.HasErrorNode = true;
                    exoReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXOEnforceRuleActionJobDetails()
                    {
                        Comment = exception.ToString(),
                        Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed,
                    });
                }
                finally
                {
                    //CosmosDBManualDataUpdater.WaitComplete();
                    //exoReportManager.();
                    //jm.Finish();
                    jm.ReportManager.SetJobFinished(Contract.RMWeb.JobMonitor.JobStatus.Finished);
                }
            }
        }
        private void RebuildStoragePolicyDto(Rule rule, bool useDefaultStorageWhenNoStorage = false)
        {
            var globalSetting = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
            if (useDefaultStorageWhenNoStorage && (string.IsNullOrWhiteSpace(rule.StoragePolicyId) || rule.StoragePolicyId.Equals(Guid.Empty.ToString())))
            {
                rule.StoragePolicyId = globalSetting.StoragePolicyId.ToString();
            }

            if (!string.IsNullOrEmpty(rule.StoragePolicyId))
            {
                var storageDevice = StorageDeviceService.GetStorageDeviceById(rule.StoragePolicyId, needDecryptSecert: true);
                ArchiverCommonStaticMethod.VerifyAvePoint(storageDevice);
                var logical = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(storageDevice);
                rule.StoragePolicyDto = new StoragePolicyDto()
                {
                    Id = storageDevice.Id,
                    Name = rule.Id,
                    PrimaryStorage = logical,
                    Type = storageDevice.Type,
                };
                if (storageDevice.SetupDataRetention)
                {
                    rule.StoragePolicyDto.RetentionOption = StorageDeviceConvert.ConvertToRetentionRuleOption(storageDevice.ArchiveRetentionRules);
                }

                if (globalSetting != null)
                {
                    if (globalSetting.UseCompression)
                    {
                        rule.ArchiverCompressionType = (GCommon.Contract.GranularBackup.Object.CompressionType)globalSetting.CompressionSpeed;
                        rule.ArchiverDataSecurity = rule.ArchiverDataSecurity | GCommon.Contract.GranularBackup.Object.DataSecurity.CompressionMedia;
                    }
                    if (globalSetting.UseEncryption)
                    {
                        storageDevice.EncryptionProfileId = globalSetting.SecurityProfileId.ToString();
                        var encryptionInfo = SettingProfileDao.LoadById(new Guid(storageDevice.EncryptionProfileId));
                        DataEncryptionProfile mProfile = SerializerHelper.DeserializeByDataContractSerializer<DataEncryptionProfile>(encryptionInfo.Settings);

                        if (mProfile.CurrentProtectionAlgorithm != null && mProfile.CurrentProtectionAlgorithm.Type == ProtectionAlgorithmType.TenantMasterKeyEncryptionService)
                        {
                            rule.EncryptionMethods = GCommon.Contract.GranularBackup.Object.EncryptionMethods.AES_ENCRYPTION;
                            rule.ArchiverDataSecurity = rule.ArchiverDataSecurity | GCommon.Contract.GranularBackup.Object.DataSecurity.EncryptionMedia;
                            rule.DataEncryptionProfileId = storageDevice.EncryptionProfileId;
                            rule.DataEncryptionInfoWrapper = new GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper.DataEncryptionInfoWrapper();
                            var info = new GCommon.Contract.Server.ControlPanel.Cryptography.DataEncryptionInfo();
                            byte[] result;
                            result = KeyGenerateProviderFactory.CreateProvider().GenerateKeyBytes(mProfile.KeyLength / 8);
                            info.Checksum = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1).ComputeHash(result);
                            info.EncryptionType = mProfile.AlgorithmType;
                            info.ProfileGuid = storageDevice.EncryptionProfileId;
                            info.ProtectionGuid = storageDevice.EncryptionProfileId;
                            info.ProfileName = "Default Encryption Profile";
                            info.EncryptedDynamicKey = AesEncryptorWrapper.Encrypt(result);
                            rule.DataEncryptionInfoWrapper.EncryptionInfo = info;
                            rule.DataEncryptionInfoWrapper.DynamicKey = AvePoint.GCommon.Utility.Cryptography.CspCommunicationWrapper.WrapKeyToBase64String(result);
                        }
                        else
                        {
                            logger.Warn("Not the desired encryption method.");
                            if (mProfile.CurrentProtectionAlgorithm != null)
                            {
                                logger.Warn("CurrentProtectionAlgorithm is null.");
                            }
                            else
                            {
                                logger.Warn($"CurrentProtectionAlgorithm Type is {mProfile.CurrentProtectionAlgorithm.Type}.");
                            }
                        }

                    }
                }
            }
        }
        private EXOBackupAction GenerateEnforceRuleActionObject(ExchangeOnlineTreeNodeDto treeNode, IRMReportManager jobManagement, AvePoint.GCommon.Contract.StorageOptimization.Object.Rule currentRule,string subjobId,int subjobNumber,string currentRuleId,string currentRuleName)
        {
            EXOBackupAction enforceRuleActionBase = new EXOBackupAction(treeNode, jobManagement, currentRuleId, currentRuleName);
            var groupId = new Guid(TreeManagement.GetGroupNode(treeNode).ID);
            var discoverType = EXODiscoverType.Full;
            RMEXODiscoverHelper help = new RMEXODiscoverHelper();
            var factory = EXODiscoverFactory.CreateFactory(help, discoverType, NodeFlagType.ExplorerSync, groupId, null);
            enforceRuleActionBase.SetDiscoverObject(help, factory);
            enforceRuleActionBase.CurrentRule = currentRule;
            enforceRuleActionBase.SubjobId = subjobId;
            enforceRuleActionBase.SubjobNum = subjobNumber;
            return enforceRuleActionBase;
        }
    }
}
