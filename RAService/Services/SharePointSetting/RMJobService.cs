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
using AvePoint.RA.CommonUtil;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.RuleManagement;
using AvePoint.RA.Service.Services.SharePointSetting.AuditHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.GranularBackup.Object;
using System.Xml;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Contract.Tenant;
using System.IO;
using Newtonsoft.Json;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.Contract.RMWeb.CP;
using System.Threading;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.DB.Core;
using AvePoint.GCommon.Utility.Cryptography;
using System.Text;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.Contract.Archiver;
using System.Threading.Tasks;
using AvePoint.RA.Service.Services;

namespace AvePoint.RA.Service.SharePointSetting
{
    [Audit]
    public class RMJobService : RMServiceBase, IRMJobService
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMJobService));

        //test 


        private IRuleManagerService RuleManagerService => PlatformWindsorManager.GetService<IRuleManagerService>();

        private IGlobalSettingService GlobalSettingService => PlatformWindsorManager.GetService<IGlobalSettingService>();

        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();

        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();

        private ITermRuleAssociationDao TermRuleInfos => PlatformWindsorManager.GetService<ITermRuleAssociationDao>();

        private IGlobalStorageSettingDao GlobalStorageSettingDao => PlatformWindsorManager.GetService<IGlobalStorageSettingDao>();
        private ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();

        private IExportSettingsDao ExportSettingsDao => PlatformWindsorManager.GetService<IExportSettingsDao>();

        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private IRMScheduleDao RMScheduleDao => PlatformWindsorManager.GetService<IRMScheduleDao>();
        public ISharePointSettingDao SharePointSettingDao => PlatformWindsorManager.GetService<ISharePointSettingDao>();

        public ITeamsSettingDao TeamsSettingDao => PlatformWindsorManager.GetService<ITeamsSettingDao>();
        public IOneDriveSettingDao OneDriveSettingDao => PlatformWindsorManager.GetService<IOneDriveSettingDao>();
        public IEXOSettingDao EXOSettingDao => PlatformWindsorManager.GetService<IEXOSettingDao>();
        public IEXOSettingRuleDao EXOSettingRuleDao => PlatformWindsorManager.GetService<IEXOSettingRuleDao>();
        private IJobQueueService mJobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private ISPSettingTreeService mSPTreeService => PlatformWindsorManager.GetService<ISPSettingTreeService>();
        private IRMOneDriveSettingsService RMOneDriveSettingsService => PlatformWindsorManager.GetService<IRMOneDriveSettingsService>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        public IExportDataEncryptionSettingService ExportDataEncryptionSettingService => PlatformWindsorManager.GetService<IExportDataEncryptionSettingService>();

        protected IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        protected IRMMailboxService MailBoxService => PlatformWindsorManager.GetService<IRMMailboxService>();
        private IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService<IFSConnectionDao>();
        private IFSConnectionGroupDao FSConnectionGroupDao => PlatformWindsorManager.GetService<IFSConnectionGroupDao>();
        private IArchiverRuleService ArchiverRuleService => PlatformWindsorManager.GetService<IArchiverRuleService>();       
        private IRMPhysicalRecordSettingsService PhysicalRecordSettingService => PlatformWindsorManager.GetService<IRMPhysicalRecordSettingsService>();
        private IRMMiscProfileDao StubSettingDao => PlatformWindsorManager.GetService<IRMMiscProfileDao>();

        /*private void SaveTermFilter(string columnName, List<string> termNames, Rule rule)
        {
            int seq = termNames.Count + 1;
            for (int i = 0; i < rule.SOFilters.Count; i++)
            {
                rule.SOFilters[i].SequenceNo = seq++;
            }
            int SequenceNo = termNames.Count;
            foreach (var termName in termNames)
            {
                ArchiverRuleFilter arFilter = new ArchiverRuleFilter();
                arFilter.CombineMode = ArchiverFilterCombineMode.Or;
                arFilter.Condition = ArchiverFilterCondition.Equals;
                if (termNames[0].Equals(termName))
                {
                    arFilter.CombineMode = ArchiverFilterCombineMode.And;
                }
                arFilter.RuleType = ArchiverFilterRuleType.TextColumn;
                arFilter.RuleName = columnName;
                arFilter.Value1 = termName;
                arFilter.SequenceNo = SequenceNo--;
                rule.SOFilters.Insert(0, arFilter.Dto);
            }
            RuleManagerService.ResetSOFilter(rule);
        }*/

        public async System.Threading.Tasks.Task RunDisposalJobNowAsync(List<RMSPTreeNode> nodes)
        {
            foreach (var node in nodes)
            {
               await RunNowAsync(node, JobRunBy.Control);
            }
        }
       /* public RMSPTreeNode GetGroupNode(RMSPTreeNode node)
        {
            while (node.Level != (int)NodeLevel.WebApplication)
            {
                node = node.Parent;
            }
            return node;
        }*/

        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.DisposalActivityManagement, Action = AuditAction.RunDisposalJob,
            AfterHandler = typeof(DisposalActivityManagementAfterAuditHandler))]
        [RACodeReview("Allen Yin", comment: "有多处可优化空间，包括：1.代码顺序 2.残留废弃代码 3.算法时间复杂度，一些查询应该直接封装到DAO层。等时间充裕的时候再做整理")]
        public async Task<RAReturnMessage> RunNowAsync(RMSPTreeNode tree, JobRunBy jobRunBy)
        {
            RAReturnMessage rm = new RAReturnMessage()
            {
                FaildType = RAFailedType.None,
                MessageType = RAMessageType.Successful,

            };
            try
            {
                if (!TenantService.CheckTenantIsAvailable(TenantLocalValue.LogonGroupId))
                {
                    rm.MessageType = RAMessageType.Failed;
                    rm.FaildType = RAFailedType.LicenseExpired;
                    rm.ErrorMessage = I18NEntity.GetString("RM_JM_Summary_LicenseNotAvailable");
                    return rm;
                }
                FipsModeUtil.InitControlCryptoMode();
                CspCommunicationWrapper.CommunicationEncryptionKey = CspCommunicationWrapper.staticCommunicationEncryptionKey;
                SPTreeNodeDto spTree = RMDtoConverter.ConvertRMTree2SPTree(tree);
                DAOAPIClientV1 client = null;
                #region Get ArchiverDB And Index info
                //IMArchiverService archiverService = DocAveServiceHelper.CreateServiceClient<IMArchiverService>();
                int setting = 0;
                try
                {
                    client = new DAOAPIClientV1();
                    DAOAPIClientV1 Client1 = new DAOAPIClientV1();
                    setting = Client1.GetArchiverDBAndIndexDeviceSetting();
                }
                catch (Exception ex)
                {
                    logger.Warn("Init setting error {0}", ex.ToString());
                }
                if (setting == 1)
                {
                    rm.MessageType = RAMessageType.Failed;
                    rm.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_RunJob_Failed_NoIndexDeviceSetting");
                    return rm;
                }
                else if (setting == 2)
                {
                    rm.MessageType = RAMessageType.Failed;
                    rm.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_RunJob_Failed_NoArchiverDBSetting");
                    return rm;
                }
                else if (setting == 3)
                {
                    rm.MessageType = RAMessageType.Failed;
                    rm.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_RunJob_Failed_NoIndexDeviceAndArchiverDBSetting");
                    return rm;
                }

                #endregion             

                #region Init column and global settings
                logger.Info("Init column and global settings");
                SOPlan plan = new SOPlan();
                plan.SOPlanExtension = new SOPlanExtension();
                //plan.SOPlanExtension.ProcessingPoolId = rmSettings.ProcessingPoolId.ToString();
                plan.ArchiverType = GCommon.Contract.StorageOptimization.Object.ArchiverType.Full;
                plan.Category = GCommon.Contract.Server.Common.PlanCategory.Archiver;
                plan.ModuleName = "archiver";
                plan.RunNow = true;
                var connectionstr = RMGlobalConfiguration.DBConfig[RMDatabaseSettingKey.RECO_CONTROL_SQL_CONNECTION_STRING_FULL];
                var recordHistoryStr = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECORDS_HISTORY_STORAGE_CONNECTION_STRING_FULL];
                var wrapConnectionStr = CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(Encoding.UTF8.GetBytes(connectionstr));
                var recordHistoryConnectionStr = CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(Encoding.UTF8.GetBytes(recordHistoryStr));
                plan.RecordWebDBConnectionString = wrapConnectionStr;
                plan.RecordsHistoryDBConnectionString = recordHistoryConnectionStr;
                plan.RunJobUser = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : I18NEntity.GetString("RM_TS_RunSchedule");
                plan.SkipRemoveContentAndDestroyAction = tree.SkipRemoveContentAndDestroyAction;
                AvePoint.RA.DB.Explorer.Dao.CosmosImp.CosmosConnectionInfo rmCosmosConnectionInfo = await RMDBContextManager.GetExplorerDBConnectionInfoAsync();
                AvePoint.GCommon.Contract.StorageOptimization.Object.CosmosConnectionInfo toSOCosmosConnectionInfo = null;
                if (rmCosmosConnectionInfo != null)
                {
                    toSOCosmosConnectionInfo = new AvePoint.GCommon.Contract.StorageOptimization.Object.CosmosConnectionInfo
                    {
                        CollectionId = rmCosmosConnectionInfo.CollectionId,
                        DatabaseId = rmCosmosConnectionInfo.DatabaseId,
                        Endpoint = rmCosmosConnectionInfo.Endpoint,
                        Key = rmCosmosConnectionInfo.Key
                    };
                }
                plan.RecordExplorerDB = toSOCosmosConnectionInfo;

                #endregion               
                Dictionary<Guid, List<Guid>> temrRuleMapping = null;
                try
                {
                    temrRuleMapping = await AssembleTermRuleMappingAsync(SourceFlag.SharePoint);
                }
                catch (Exception e)
                {
                    if (!string.IsNullOrWhiteSpace(e.Message) && (e.Message.Contains(I18NEntity.GetString("RM_JS_DAM_FaildRun_NoExportLocation")) || e.Message.Contains(I18NEntity.GetString("RM_JS_DAM_FaildRun_FTPExportLocationNotSupported"))))
                    {
                        rm.MessageType = RAMessageType.Failed;
                        rm.ErrorMessage = e.Message;
                        return rm;
                    }
                    else
                    {
                        throw;
                    }
                }
                var isAllSiteDisabled = false;
                if (temrRuleMapping != null && temrRuleMapping.Count > 0)
                {
                    var breakInherting = BuildBreakTreeNode(tree);
                    if (breakInherting != null && breakInherting.Count > 0)
                    {
                        foreach (var bNode in breakInherting)
                        {
                            logger.Info("breaking Inhert Node Id is :{0}, FullPath is {1}", bNode.Id, bNode.FullPath);
                        }
                        if (tree.Level == (int)NodeLevel.WebApplication || tree.Level == (int)NodeLevel.SiteCollection)
                        {
                            isAllSiteDisabled = await IsAllSiteDisabledAsync(tree.Clone(), breakInherting, jobRunBy, RMBrowseTreeNodeSourceType.SharepointOnline);
                        }
                    }

                    if (isAllSiteDisabled)
                    {
                        rm.Extension = TenantUtil.RunUnderTenant(TenantLocalValue.LogonGroupId, TenantLocalValue.LogonUserEmail, SaveSkippedFakeJobid, new List<object> { "RM_JS_JMD_DisableRecordManagement_Or_HasOwnSettingMessage", jobRunBy, GetRMSPTreeNodeContainerId(tree) });
                    }
                    else
                    {
                        RMDtoConverter.ConvertSPTreeBeforeToJSON(spTree);
                        spTree.Url = spTree.Name;
                        List<SPTreeNodeDto> spTrees = new List<SPTreeNodeDto>();
                        spTrees.Add(spTree);
                        SORuleInfoContract ruleInfo = new SORuleInfoContract();
                        //ruleInfo.Rules = ruleResults.Select(r => r.Value).ToList();
                        ruleInfo.SourceFlag = (int)SourceFlag.SharePoint;
                        ruleInfo.TermRuleMapping = temrRuleMapping;
                        ruleInfo.RecordsStorageInfo = await GetRecordsStorageInfoAsync((int)SourceFlag.SharePoint);
                        ruleInfo.Plan = plan;
                        ResetNodeLevel(spTrees[0]);
                        logger.Info("Disposal job running under scope:{0}", spTrees?[0]?.FullPath);
                        try
                        {
                            var key = client.RunNow(spTrees, ruleInfo, breakInherting);
                            rm.Extension = TenantUtil.RunUnderTenant(TenantLocalValue.LogonGroupId, TenantLocalValue.LogonUserEmail, SaveFakeJobid, new List<object> { key, jobRunBy, GetRMSPTreeNodeContainerId(tree) });
                        }
                        catch (Exception e)
                        {
                            logger.Error("error occurred while run job, ERROR:{0}", e.ToString());
                            rm.MessageType = RAMessageType.Failed;
                            rm.ErrorMessage = I18NEntity.GetString(e.Message);
                        }
                    }
                }
                else
                {
                    rm.MessageType = RAMessageType.Failed;
                    rm.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_RunJob_Failed_NoRules");
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while run job, ERROR:{0}", ex.ToString());
                rm.MessageType = RAMessageType.Failed;
                rm.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_RunJob_Failed");
            }
            return rm;
        }


        public async Task<Dictionary<Guid, List<Guid>>> AssembleTermRuleMappingAsync(SourceFlag sourceFlag)
        {
            logger.Debug("Begin to assemble term rules mappings to cache.");
            var termRuleMapping = new Dictionary<Guid, List<Guid>>();
            List<RMTermRuleAssociation> trAssociations = TermRuleInfos.GetTermWithRule();
            Dictionary<Guid, Rule> ruleIdDic = new Dictionary<Guid, Rule>();
            if (sourceFlag == SourceFlag.SharePoint)
            {
                ruleIdDic = RuleManagerService.GetRulesFromRecords().Where(r => r.SOFilters != null && r.SOFilters.Count != 0).ToDictionary(rule => new Guid(rule.Id));
            }
            else if (sourceFlag == SourceFlag.OneDrive)
            {
                ruleIdDic = RuleManagerService.GetRulesFromRecords().Where(r => r.OneDriveRule != null && r.OneDriveRule.SOFilters != null && r.OneDriveRule.SOFilters.Count != 0).ToDictionary(rule => new Guid(rule.Id));
            }
            else if (sourceFlag == SourceFlag.Physical)
            {
                ruleIdDic = RuleManagerService.GetRulesFromRecords().Where(r => r.PhysicalRule != null && r.PhysicalRule.SOFilters != null && r.PhysicalRule.SOFilters.Count != 0).Select(r => { r.PhysicalRule.Id = r.Id; return r.PhysicalRule; }).ToDictionary(rule => new Guid(rule.Id));
            }
            Dictionary<int, List<Guid>> termRules = new Dictionary<int, List<Guid>>();
            foreach (var termId in trAssociations.Select(a => a.TermId).Distinct())
            {
                var rules = trAssociations
                    .Where(a => a.TermId == termId)
                    .OrderBy(a => a.RuleOrder)
                    .Select(a => a.RuleId)
                    .ToList();
                if (rules.Count > 0)
                {
                    termRules.Add(termId, rules);
                }
            }

            var allHasRuleTerms = TermDao.GetRMTermsByTermIds(termRules.Keys.ToArray());
            foreach (var term in allHasRuleTerms)
            {
                if (term.IsRemoved)
                {
                    continue;
                }
                List<Guid> spoRuleIds = new List<Guid>();

                Rule rule;
                var ruleIds = termRules[term.Id];
                for (int idx = 0; idx < ruleIds.Count; idx++)
                {
                    if (ruleIdDic.TryGetValue(ruleIds[idx], out rule))
                    {
                        if (rule.PolicyLevel != PolicyLevel.None)
                        {
                            var ruleId = new Guid(rule.Id);
                            if (!spoRuleIds.Contains(ruleId))
                            {
                                spoRuleIds.Add(ruleId);
                            }

                        }
                    }
                }
                if (spoRuleIds.Count == 0)
                {
                    //no spo rule
                    continue;
                }
                if (!termRuleMapping.ContainsKey(term.UniqueId))
                {
                    termRuleMapping.Add(term.UniqueId, spoRuleIds);
                }

                var refTerms = new List<RMTerm>();
                TermDao.GetAllInheritTermsByRootTerm(term.Id, ref refTerms);
                foreach (var refTerm in refTerms)
                {
                    if (!termRuleMapping.TryGetValue(refTerm.UniqueId, out List<Guid> tempIds))
                    {
                        termRuleMapping.Add(refTerm.UniqueId, spoRuleIds);
                    }
                }
            }

            if (ruleIdDic != null && ruleIdDic.Count > 0)
            {
                SortedSet<string> invalidRule = new SortedSet<string>();
                SortedSet<string> invalidRuleWithFtpLocation = new SortedSet<string>();
                RMCPGlobalStorageSetting rmSettings = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
                var exportLocationTypes = await GlobalSettingService.GetExportLocationTypesAsync();
                foreach (var rule in ruleIdDic.Values)
                {
                    if (rule.ExportInfo != null && (rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportBeforeArchive || rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive) && !(rule.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
                    {
                        if (rmSettings.ExportLocationId == Guid.Empty)
                        {
                            invalidRule.Add(rule.Name);
                            continue;
                        }
                        if (exportLocationTypes.ContainsKey(rmSettings.ExportLocationId) && exportLocationTypes[rmSettings.ExportLocationId] == 1)
                        {
                            invalidRuleWithFtpLocation.Add(rule.Name);
                            continue;
                        }

                    }
                }

                if (invalidRule.Count > 0)
                {
                    string message = string.Join(";", invalidRule);
                    throw new Exception(I18NEntity.GetString("RM_JS_DAM_FaildRun_NoExportLocation") + "|" + message);

                }
                if (invalidRuleWithFtpLocation.Count > 0)
                {
                    string message = string.Join(";", invalidRuleWithFtpLocation);
                    throw new Exception(I18NEntity.GetString("RM_JS_DAM_FaildRun_FTPExportLocationNotSupported") + "|" + message);
                }
            }
            return termRuleMapping;
        }

        private async Task<RecordsStorageInfo> GetRecordsStorageInfoAsync(int sourceFlag)
        {
            RecordsStorageInfo recordsStorageInfo = new RecordsStorageInfo();
            RMCPGlobalStorageSetting rmSettings = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
            #region export info
            try
            {
                if (sourceFlag == (int)SourceFlag.OneDrive)
                {
                    sourceFlag = (int)SourceFlag.SharePoint;
                }

                var exportSetting = ExportSettingsDao.GetExportSetting((int)ExportSettingType.VEO, sourceFlag);
                if (exportSetting != null)
                {
                    recordsStorageInfo.FileVEO = exportSetting.FileVEO;
                    recordsStorageInfo.RecordVEO = exportSetting.RecordVEO;
                    recordsStorageInfo.ManifestVEO = exportSetting.ManifestVEO;

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(exportSetting.ArchiverSetting);
                    recordsStorageInfo.ArchiverSetting = new ArchiverSetting();
                    //rule.ArchiverSetting.NumberOfThreadSendingEmail = int.Parse(doc.SelectSingleNode("Configuration/numberOfThreadsSendingEmail").InnerXml);
                    recordsStorageInfo.ArchiverSetting.EnableArchiverVEOMerge = bool.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge").Attributes["enable"].Value);
                    recordsStorageInfo.ArchiverSetting.IsDeleteOldFile = bool.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/isDeleteOldFile").InnerXml);
                    recordsStorageInfo.ArchiverSetting.FileNumber = int.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/fileNumber").InnerXml);
                    recordsStorageInfo.ArchiverSetting.FileSize = double.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/fileSize").InnerXml);
                    recordsStorageInfo.ArchiverSetting.FolderName = doc.SelectSingleNode("Configuration/archiverVEOMerge/folderName").InnerXml;

                    doc.LoadXml(exportSetting.ArchiverVEOSetting);
                    recordsStorageInfo.ArchiverVEOSetting = new ArchiverVEOSetting();
                    recordsStorageInfo.ArchiverVEOSetting.AgencyId = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/AgencyID").InnerXml;
                    recordsStorageInfo.ArchiverVEOSetting.SeriesNumber = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/Series_Number").InnerXml;
                    recordsStorageInfo.ArchiverVEOSetting.SeriesIdentifier = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/SeriesIdentifier").InnerXml;
                    recordsStorageInfo.ArchiverVEOSetting.ConsignmentNumber = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/ConsignmentNumber").InnerXml;
                }
                else
                {
                    //rule.FileVEO = null;
                    //rule.RecordVEO = null;
                    //rule.ManifestVEO = null;
                    //rule.ArchiverSetting = null;
                    //rule.ArchiverVEOSetting = null;
                    //RECO 自己提供配置文件
                    var filepath = Path.Combine(WebUtil.GetInstallPath(), "Config", "VEO Configuration Files.zip");
                    var unZipFolder = Path.Combine(WebUtil.GetInstallPath(), "Temp", "Config", "VEO Configuration Files");
                    GCommon.ZipUtil.UnZipFile(filepath, unZipFolder);
                    if (sourceFlag == (int)SourceFlag.SharePoint || sourceFlag == (int)SourceFlag.OneDrive)
                    {
                        recordsStorageInfo.FileVEO = GetMemoryStream(unZipFolder, "FileVEO.xml");
                        recordsStorageInfo.RecordVEO = GetMemoryStream(unZipFolder, "RecordVEO.xml");
                        recordsStorageInfo.ManifestVEO = GetMemoryStream(unZipFolder, "ManifestVEO.xml");
                    }
                    else
                    {
                        recordsStorageInfo.FileVEO = GetMemoryStream(unZipFolder, "EXOFileVEO.xml");
                        recordsStorageInfo.RecordVEO = GetMemoryStream(unZipFolder, "EXORecordVEO.xml");
                        recordsStorageInfo.ManifestVEO = GetMemoryStream(unZipFolder, "EXOManifestVEO.xml");

                    }

                    using (FileStream fs = new FileStream(Path.Combine(unZipFolder, "ArchiverSettings.config"), FileMode.Open, FileAccess.Read))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(await sr.ReadToEndAsync());
                            recordsStorageInfo.ArchiverSetting = new ArchiverSetting();
                            //rule.ArchiverSetting.NumberOfThreadSendingEmail = int.Parse(doc.SelectSingleNode("Configuration/numberOfThreadsSendingEmail").InnerXml);
                            recordsStorageInfo.ArchiverSetting.EnableArchiverVEOMerge = bool.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge").Attributes["enable"].Value);
                            recordsStorageInfo.ArchiverSetting.IsDeleteOldFile = bool.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/isDeleteOldFile").InnerXml);
                            recordsStorageInfo.ArchiverSetting.FileNumber = int.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/fileNumber").InnerXml);
                            recordsStorageInfo.ArchiverSetting.FileSize = double.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/fileSize").InnerXml);
                            recordsStorageInfo.ArchiverSetting.FolderName = doc.SelectSingleNode("Configuration/archiverVEOMerge/folderName").InnerXml;
                        }
                    }
                    using (FileStream fs = new FileStream(Path.Combine(unZipFolder, "ArchiverVEOSettings.config"), FileMode.Open, FileAccess.Read))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(await sr.ReadToEndAsync());
                            recordsStorageInfo.ArchiverVEOSetting = new ArchiverVEOSetting();
                            recordsStorageInfo.ArchiverVEOSetting.AgencyId = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/AgencyID").InnerXml;
                            recordsStorageInfo.ArchiverVEOSetting.SeriesNumber = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/Series_Number").InnerXml;
                            recordsStorageInfo.ArchiverVEOSetting.SeriesIdentifier = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/SeriesIdentifier").InnerXml;
                            recordsStorageInfo.ArchiverVEOSetting.ConsignmentNumber = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/ConsignmentNumber").InnerXml;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("set VEO export setting when run job error {0}", e.ToString());
            }


            try
            {
                var nnaExportSetting = ExportSettingsDao.GetExportSetting((int)ExportSettingType.NAA, sourceFlag);
                if (nnaExportSetting != null)
                {
                    recordsStorageInfo.NAAConfigFile = nnaExportSetting.ExportConfig;
                }
                else
                {
                    var filepath = Path.Combine(WebUtil.GetInstallPath(), "Config", "NAA Configuration File.zip");
                    var unZipFolder = Path.Combine(WebUtil.GetInstallPath(), "Temp", "Config", "NAA Configuration File");
                    GCommon.ZipUtil.UnZipFile(filepath, unZipFolder);
                    if (sourceFlag == (int)SourceFlag.SharePoint || sourceFlag == (int)SourceFlag.OneDrive)
                    {
                        recordsStorageInfo.NAAConfigFile = GetMemoryStream(unZipFolder, "NAA Configuration File.xml");
                    }
                    else
                    {
                        recordsStorageInfo.NAAConfigFile = GetMemoryStream(unZipFolder, "EXO NAA Configuration File.xml");
                    }

                }
            }
            catch (Exception e)
            {
                logger.Warn("set NNA export setting when run job error {0}", e.ToString());
            }


            //NARA

            try
            {
                var nnaExportSetting = ExportSettingsDao.GetExportSetting((int)ExportSettingType.NARA, sourceFlag);
                if (nnaExportSetting != null)
                {
                    recordsStorageInfo.NARAConfigFile = nnaExportSetting.ExportConfig;
                }
                else
                {
                    var filepath = Path.Combine(WebUtil.GetInstallPath(), "Config", "NARA Configuration File.zip");
                    var unZipFolder = Path.Combine(WebUtil.GetInstallPath(), "Temp", "Config", "NARA Configuration File");
                    GCommon.ZipUtil.UnZipFile(filepath, unZipFolder);
                    if (sourceFlag == (int)SourceFlag.SharePoint || sourceFlag == (int)SourceFlag.OneDrive)
                    {
                        recordsStorageInfo.NARAConfigFile = GetMemoryStream(unZipFolder, "NARA Configuration File.xml");
                    }
                    else
                    {
                        recordsStorageInfo.NARAConfigFile = GetMemoryStream(unZipFolder, "EXO NARA Configuration File.xml");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("set NARA export setting when run job error {0}", e.ToString());
            }

            var exportEncryptionEnabled = RMKeyValueDao.IsExportDataEncryptionEnabled();
            if (exportEncryptionEnabled)
            {
                var keyIV = ExportDataEncryptionSettingService.GetCurrentAesKey().Extension;
                if (!string.IsNullOrWhiteSpace(keyIV) && keyIV.IndexOf("|") > 0)
                {
                    recordsStorageInfo.ExportDataEncryptionKey = CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(Encoding.UTF8.GetBytes(keyIV.Split('|')[0]));
                    recordsStorageInfo.ExportDataEncryptionIV = CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(Encoding.UTF8.GetBytes(keyIV.Split('|')[1]));
                }
                else
                {
                    throw new Exception("Export data encryption is enabled, but we cannot valid encryption key.");
                }
            }

            recordsStorageInfo.ExportLocationId = rmSettings.ExportLocationId.ToString();
            recordsStorageInfo.ExportLocationName = rmSettings.ExportLocationName;
            #endregion

            #region storage info

            string policyId = rmSettings.StoragePolicyId.ToString();
            logger.Info("storage policy id:{0}", policyId);
            DataSecurity encryptionMethod = rmSettings.UseEncryption ? rmSettings.EncryptionMethod : DataSecurity.None;
            recordsStorageInfo.ArchiverDataSecurity = DataSecurity.CompressionMedia | encryptionMethod;
            recordsStorageInfo.ArchiverCompressionType = CompressionType.Normal;
            recordsStorageInfo.DataEncryptionProfileId = rmSettings.SecurityProfileId == Guid.Empty ? null : rmSettings.SecurityProfileId.ToString();
            recordsStorageInfo.DataEncryptionProfileName = rmSettings.SecurityProfileName;
            recordsStorageInfo.StoragePolicyId = policyId;
            recordsStorageInfo.StoragePolicyName = rmSettings.StoragePolicyName;

            recordsStorageInfo.StubTemplatesList = new();
            var allStubs = await StubSettingDao.FindListAsync(s => s.Type == (int)AvePoint.GCommon.Contract.Server.Common.Profile.Object.ProfileType.StubSetting && !s.IsRemoved);
            foreach (var stub in allStubs)
            {
                var para = SerializerHelper.DeserializeByDataContractSerializer<AvePoint.GCommon.Contract.Server.ControlPanel.Object.StubSettingParaDto>(stub.Extension);
                recordsStorageInfo.StubTemplatesList.Add(new()
                {
                    Id = stub.Id,
                    Name = stub.Name,

                    StubType = para.StubType,
                    IsDeclareStubAsRecords = para.IsDeclareStubAsRecords,
                    StubContent = para.StubContent
                });
            }
            #endregion

            return recordsStorageInfo;
        }

        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.DisposalActivityManagement, Action = AuditAction.RunOneDriveDisposalJob,
            AfterHandler = typeof(DisposalActivityManagementAfterAuditHandler))]
        public async Task<RAReturnMessage> RunOneDriveNowAsync(RMSPTreeNode tree, JobRunBy jobRunBy)
        {
            RAReturnMessage rm = new RAReturnMessage()
            {
                FaildType = RAFailedType.None,
                MessageType = RAMessageType.Successful,
            };
            try
            {
                //RECO-11064 Security Testing
                bool isNullClassification = false; //group level开启IsNullClassificationSetting即认为当前节点使用IsNullClassificationSetting
                Guid groupId = Guid.Empty;
                Guid siteId = Guid.Empty;
                try
                {

                    string objectId = Guid.Empty.ToString();
                    if (tree.Level != (int)NodeLevel.WebApplication)
                    {
                        RMSPTreeNode siteCollectionNode = RMOneDriveSettingsService.GetSiteCollectionNode(tree);
                        objectId = siteCollectionNode.SPObjectId;
                        siteId = new Guid(objectId);
                    }
                    groupId = new Guid(tree.GetGroupNode().SPObjectId);
                    var groupSetting = OneDriveSettingDao.GetSettingInfoByAgentGroupId(groupId.ToString());
                    isNullClassification = groupSetting.IsNullClassificationSetting;
                    if (RMOneDriveSettingsService.CheckParentNodeDisable(tree, objectId) || (!groupSetting.IsNullClassificationSetting && groupSetting.TermSetId == Guid.Empty))
                    {
                        rm.MessageType = RAMessageType.Failed;
                        rm.FaildType = RAFailedType.MissingRequiredSettings;
                        return rm;
                    }
                }
                catch (Exception e)
                {
                    logger.Warn($"RunOneDriveNow verificate error: {e}");
                }

                if (!TenantService.CheckTenantIsAvailable(TenantLocalValue.LogonGroupId))
                {
                    rm.MessageType = RAMessageType.Failed;
                    rm.FaildType = RAFailedType.LicenseExpired;
                    rm.ErrorMessage = I18NEntity.GetString("RM_JM_Summary_LicenseNotAvailable");
                    return rm;
                }
                //目前One Drive Browser Tree传的NodeLevel不正确，Disposal Run Job时单独赋值正确的NodeLevel
                if (tree.NodeType == (int)NodeType.SkyDriveProSitesGroup && tree.Level == (int)NodeLevel.WebApplication)
                {
                    tree.Level = (int)NodeLevel.SkyDriveProGroup;
                }
                FipsModeUtil.InitControlCryptoMode();
                CspCommunicationWrapper.CommunicationEncryptionKey = CspCommunicationWrapper.staticCommunicationEncryptionKey;
                SPTreeNodeDto spTree = RMDtoConverter.ConvertRMTree2SPTree(tree);
                DAOAPIClientV1 client = null;
                #region Get ArchiverDB And Index info
                int setting = 0;
                try
                {
                    client = new DAOAPIClientV1();                    
                    setting = client.GetArchiverDBAndIndexDeviceSetting();
                }
                catch (Exception ex)
                {
                    logger.Warn("Init setting error {0}", ex.ToString());
                }
                if (setting == 1)
                {
                    rm.MessageType = RAMessageType.Failed;
                    rm.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_RunJob_Failed_NoIndexDeviceSetting");
                    return rm;
                }
                else if (setting == 2)
                {
                    rm.MessageType = RAMessageType.Failed;
                    rm.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_RunJob_Failed_NoArchiverDBSetting");
                    return rm;
                }
                else if (setting == 3)
                {
                    rm.MessageType = RAMessageType.Failed;
                    rm.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_RunJob_Failed_NoIndexDeviceAndArchiverDBSetting");
                    return rm;
                }

                #endregion

                #region Init column and global settings
                logger.Info("Init column and global settings");
                SOPlan plan = new SOPlan();
                RMCPGlobalStorageSetting rmSettings = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
                plan.SOPlanExtension = new SOPlanExtension();
                //plan.SOPlanExtension.ProcessingPoolId = rmSettings.ProcessingPoolId.ToString();
                plan.ArchiverType = GCommon.Contract.StorageOptimization.Object.ArchiverType.Full;
                plan.Category = GCommon.Contract.Server.Common.PlanCategory.Archiver;
                plan.ModuleName = "archiver";
                plan.RunNow = true;
                var connectionstr = RMGlobalConfiguration.DBConfig[RMDatabaseSettingKey.RECO_CONTROL_SQL_CONNECTION_STRING_FULL];
                var recordHistoryStr = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECORDS_HISTORY_STORAGE_CONNECTION_STRING_FULL];
                var wrapConnectionStr = CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(Encoding.UTF8.GetBytes(connectionstr));
                var recordHistoryConnectionStr = CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(Encoding.UTF8.GetBytes(recordHistoryStr));
                plan.RecordWebDBConnectionString = wrapConnectionStr;
                plan.RecordsHistoryDBConnectionString = recordHistoryConnectionStr;
                plan.RunJobUser = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : I18NEntity.GetString("RM_TS_RunSchedule");
                plan.SkipRemoveContentAndDestroyAction = tree.SkipRemoveContentAndDestroyAction;
                plan.IsRecordsOneDriveNode = true;
                AvePoint.RA.DB.Explorer.Dao.CosmosImp.CosmosConnectionInfo rmCosmosConnectionInfo = await RMDBContextManager.GetExplorerDBConnectionInfoAsync();
                AvePoint.GCommon.Contract.StorageOptimization.Object.CosmosConnectionInfo toSOCosmosConnectionInfo = null;
                if (rmCosmosConnectionInfo != null)
                {
                    toSOCosmosConnectionInfo = new AvePoint.GCommon.Contract.StorageOptimization.Object.CosmosConnectionInfo
                    {
                        CollectionId = rmCosmosConnectionInfo.CollectionId,
                        DatabaseId = rmCosmosConnectionInfo.DatabaseId,
                        Endpoint = rmCosmosConnectionInfo.Endpoint,
                        Key = rmCosmosConnectionInfo.Key
                    };
                }
                plan.RecordExplorerDB = toSOCosmosConnectionInfo;
                string policyId = rmSettings.StoragePolicyId.ToString();
                logger.Info("storage policy id:{0}.", policyId);
                #endregion

                Dictionary<Guid, List<Guid>> temrRuleMapping = null;
                try
                {
                    if (isNullClassification)
                    {
                        temrRuleMapping = new Dictionary<Guid, List<Guid>>();
                        var ruleIds = await GetNullClassificationRuleIdsAsync(tree, groupId, siteId);
                        if (ruleIds != null && ruleIds.Count > 0)
                        {
                            temrRuleMapping.Add(Guid.Empty, ruleIds);
                        }
                        else
                        {
                            logger.Warn($"Onedrive null classification disposal rule count is 0.");
                        }
                        plan.IsNullClassificationSetting = true;                         
                        logger.Info($"Run disposal job with null classification."); 
                    }
                    else
                    {
                        temrRuleMapping = await AssembleTermRuleMappingAsync(SourceFlag.OneDrive);
                    }
                }
                catch (Exception e)
                {
                    if (!string.IsNullOrWhiteSpace(e.Message) && (e.Message.Contains(I18NEntity.GetString("RM_JS_DAM_FaildRun_NoExportLocation")) || e.Message.Contains(I18NEntity.GetString("RM_JS_DAM_FaildRun_FTPExportLocationNotSupported"))))
                    {
                        rm.MessageType = RAMessageType.Failed;
                        rm.ErrorMessage = e.Message;
                        return rm;
                    }
                    else
                    {
                        throw;
                    }
                }
                var isAllSiteDisabled = false;
                if (temrRuleMapping != null && temrRuleMapping.Count > 0)
                {
                    var breakInherting = BuildOneDriveBreakTreeNode(tree);
                    if (breakInherting != null && breakInherting.Count > 0)
                    {
                        foreach (var bNode in breakInherting)
                        {
                            logger.Info("breaking Inhert Node Id is :{0}, FullPath is {1}.", bNode.Id, bNode.FullPath);
                        }
                        if (tree.Level == (int)NodeLevel.SkyDriveProGroup || tree.Level == (int)NodeLevel.SiteCollection)
                        {
                            isAllSiteDisabled = await IsAllSiteDisabledAsync(tree.Clone(), breakInherting, jobRunBy, RMBrowseTreeNodeSourceType.SkyDrivePro);
                        }
                    }

                    if (isAllSiteDisabled)
                    {
                        rm.Extension = TenantUtil.RunUnderTenant(TenantLocalValue.LogonGroupId, TenantLocalValue.LogonUserEmail, SaveSkippedFakeJobid, new List<object> { "RM_JS_JMD_DisableRecordManagement_Or_HasOwnSettingMessage", jobRunBy, GetRMSPTreeNodeContainerId(tree) });
                    }
                    else
                    {
                        RMDtoConverter.ConvertSPTreeBeforeToJSON(spTree);
                        spTree.Url = spTree.Name;
                        List<SPTreeNodeDto> spTrees = new List<SPTreeNodeDto>();
                        spTrees.Add(spTree);
                        SORuleInfoContract ruleInfo = new SORuleInfoContract();
                        //ruleInfo.Rules = ruleResults.Select(r => r.Value).ToList();
                        ruleInfo.TermRuleMapping = temrRuleMapping;
                        ruleInfo.RecordsStorageInfo = await GetRecordsStorageInfoAsync((int)SourceFlag.OneDrive);
                        ruleInfo.Plan = plan;
                        ruleInfo.SourceFlag = (int)SourceFlag.OneDrive;
                        ResetNodeLevel(spTrees[0]);
                        try
                        {
                            var key = client.RunNow(spTrees, ruleInfo, breakInherting);
                            rm.Extension = TenantUtil.RunUnderTenant(TenantLocalValue.LogonGroupId, TenantLocalValue.LogonUserEmail, SaveFakeJobid, new List<object> { key, jobRunBy, GetRMSPTreeNodeContainerId(tree) });
                        }
                        catch (Exception e)
                        {
                            logger.Error("error occurred while run job, ERROR:{0}", e.ToString());
                            rm.MessageType = RAMessageType.Failed;
                            rm.ErrorMessage = I18NEntity.GetString(e.Message);
                        }
                    }
                }
                else
                {
                    rm.MessageType = RAMessageType.Failed;
                    rm.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_RunJob_Failed_NoRules");
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while run job, ERROR:{0}.", ex.ToString());
                rm.MessageType = RAMessageType.Failed;
                rm.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_RunJob_Failed");
            }
            return rm;
        }

        private async Task<List<Guid>> GetNullClassificationRuleIdsAsync(RMSPTreeNode tree, Guid groupId, Guid siteId)
        {
            //add group level rules
            List<RMSimpleRule> rMSimpleRules = EXOSettingRuleDao.GetOneDriveMappingRules(groupId, siteId).OrderBy(x => x.RuleOrder).ToList();
            List<Guid> ruleIds = new List<Guid>();
            ruleIds.AddRange(rMSimpleRules.Select(s => s.RuleId));

            //add sc level rules
            if (tree.Level == (int)NodeLevel.SkyDriveProGroup)
            {
                var siteIds = GetOneDriveBreakInheritSiteIds(tree);
                var scSettings = (await OneDriveSettingDao.FindListAsync(s => s.SiteGroupId == groupId && s.SiteId == s.ScopeId && !siteIds.Contains(s.SiteId) && s.IsNullClassificationSetting && !s.IsRemoved)).ToList();
                if (scSettings != null && scSettings.Count > 0)
                {
                    var scRuleIds = EXOSettingRuleDao.GetSiteCollectionRuleIds(scSettings.Select(s => s.ScopeId).ToList());
                    foreach (var id in scRuleIds)
                    {
                        if (!ruleIds.Contains(id))
                        {
                            ruleIds.Add(id);
                        }
                    }
                }
            }
            return ruleIds;
        }

        /// <summary>
        /// Judge if all of the sites are disabled or broke inheritance
        /// </summary>
        /// <param name="webApplication"></param>
        /// <param name="breakInherting"></param>
        /// <param name="jobRunBy"></param>
        /// <param name="treeNodeSourceType"></param>
        /// <returns></returns>
        private async Task<bool> IsAllSiteDisabledAsync(RMSPTreeNode webApplication, List<RuleNodeContract> breakInherting, JobRunBy jobRunBy, RMBrowseTreeNodeSourceType treeNodeSourceType)
        {
            List<RMSPTreeNode> sites = await mSPTreeService.BrowseAsync(webApplication, jobRunBy == JobRunBy.Control, treeNodeSourceType);
            if (sites.Count == 0) return false;

            //var spObjectIds = breakInherting.Select(o => o.SiteId);
            //var nodeIds = breakInherting.Select(o => o.NodeId);
            //var siteUrls = breakInherting.Select(o => o.SiteUrl);
            //var exist = sites.Exists(o => !(spObjectIds.Contains(o.Id) || spObjectIds.Contains(o.SPObjectId) || nodeIds.Contains(o.Id) || nodeIds.Contains(o.SPObjectId) || siteUrls.Contains(o.FullPath)));
            var fullPaths = breakInherting.Select(o => o.FullPath);
            var exist = sites.Exists(o => !fullPaths.Contains(o.FullPath));

            return !exist;
        }

        private void ResetNodeLevel(SPTreeNodeDto dto)
        {
            logger.Debug("Current node level is: " + dto.Level.ToString());
            if (dto.Level == NodeLevel.WebApplication)
            {
                var webApp = RABrowserClient.GetWebApplicationById(dto.SPObjectId);
                logger.Debug($"Get web app by id:{dto.SPObjectId}, exists:{webApp != null}, node type:{webApp?.NodeType}");
                ArgumentCheck.NotNull(webApp, nameof(webApp));
                if (webApp.NodeType == GCommon.Contract.Server.ControlPanel.Office365.RemoveNodeType.O365GroupSites)
                {
                    dto.Level = NodeLevel.O365GroupSitesGroup;
                    logger.Info("Current node is O365GroupSitesGroup, reset node level.");
                }
                else if (webApp.NodeType == GCommon.Contract.Server.ControlPanel.Office365.RemoveNodeType.PrivateChannel)
                {
                    dto.Level = NodeLevel.PrivateChannelGroup;
                    logger.Info("Current node is PrivateChannelGroup, reset node level.");
                }
            }
        }

        private string GetRMSPTreeNodeContainerId(RMSPTreeNode treeNode)
        {
            if (treeNode.Level == (int)NodeLevel.WebApplication || treeNode.Level == (int)NodeLevel.SkyDriveProGroup)
            {
                return treeNode.Id;
            }
            else
            {
                return GetRMSPTreeNodeContainerId(treeNode.Parent);
            }
        }

        private string GetRMEXOTreeNodeContainerId(RMEXOTreeNode treeNode)
        {
            if (treeNode.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup)
            {
                return treeNode.Id;
            }
            else
            {
                return GetRMEXOTreeNodeContainerId(treeNode.Parent);
            }
        }

        public RAReturnMessage RunDeclaredOnly(RMSPTreeNode tree, JobRunBy jobRunBy)
        {
            RAReturnMessage msg = new RAReturnMessage()
            {
                FaildType = RAFailedType.None,
                MessageType = RAMessageType.Successful,

            };
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Schedule ? "RM_TS_RunSchedule" : TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ActionOnly,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = tree == null ? null : SerializerHelper.SerializeByDataContractSerializer(tree)
                };
                string id = mJobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg.MessageType = RAMessageType.Failed;
                }
                else
                {
                    logger.Info($"Successfully started decalre only job Id:[{id}].");
                }
            }
            catch (Exception e)
            {
                logger.Error($"Start declared only job failed: {e.ToString()}");
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = e.Message;
            }
            return msg;
        }

        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunDisposalJob,
    AfterHandler = typeof(DeclareOnlyJobAfterAuditHandler))]
        public async Task<string> RealRunDeclareOnlyJobAsync(JobRunBy jobRunBy, string jobRunByUser, RMSPTreeNode runJobNode)
        {
            string jobId = string.Empty;
            //起Job，判断是前台起Job还是Schedule起的Job
            if (jobRunBy == JobRunBy.Schedule)
            {
                jobId = JobMonitorService.CreateJobWithScopeId(JobType.ActionOnly, "RM_TS_RunSchedule", runJobNode.SPObjectId);
                logger.Info("Begin schedule action Job {0}", jobId);
            }
            else
            {
                jobId = JobMonitorService.CreateJobWithScopeId(JobType.ActionOnly, jobRunByUser, runJobNode.SPObjectId);
                logger.Info("Begin web action Job {0}", jobId);
            }
            var runningJobs = JobMonitorService.GetRunningJobs(new List<JobType>() { JobType.ActionOnly }, runJobNode.SPObjectId);
            bool needSkip = runningJobs.Any(j => !j.Id.Equals(jobId));
            if (needSkip)
            {
                JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JS_DAM_RunJob_Skip_SameNode");
                logger.Info($"A job with same node is already running. NodePath:[{runJobNode.FullPath}] NodeId:[{runJobNode.SPObjectId}]");
            }
            else
            {
                await StartDeclareJobAsync(jobId, runJobNode, jobRunBy, JobType.ActionOnly);
            }
            return jobId;
        }

        private async System.Threading.Tasks.Task StartDeclareJobAsync(string jobId, RMSPTreeNode node, JobRunBy runBy, JobType jobType)
        {
            var excludeNodes = BuildBreakTreeNode(node);
            List<Guid> excludeSiteIds = excludeNodes.Where(n => n.NodeLevel == NodeLevel.SiteCollection).Select(n => new Guid(n.NodeId)).ToList();
            List<RMSPTreeNode> availableSites = new List<RMSPTreeNode>();
            try
            {
                if (node == null)
                {
                    ArgumentCheck.NotNull(node, nameof(node));
                    logger.Warn("Node info in {0} is null or empty", node.FullPath);
                    return;
                }
                if (node.Level == (int)NodeLevel.WebApplication)
                {
                    List<RMSPTreeNode> sites = await mSPTreeService.BrowseAsync(node);
                    var totalSiteCount = sites.Count;
                    var hasCustomSiteCount = 0;

                    logger.Info("Group:{0} site collection count is {1}", node.Name, sites.Count);
                    foreach (RMSPTreeNode siteNode in sites)
                    {
                        if (excludeSiteIds.Contains(new Guid(siteNode.SPObjectId)))
                        {
                            logger.Info("Exclude SiteId {0}", siteNode.SPObjectId);
                            hasCustomSiteCount++;
                        }
                        else
                        {
                            availableSites.Add(siteNode);
                        }
                    }
                }
                else
                {
                    //TODO 找到SC节点加进去，不能直接加
                    availableSites.Add(node);
                }
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
            }
            int subJobCount = availableSites.Count;
            if (subJobCount == 0)
            {
                logger.Warn("No available sc to run");
                JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "No available sc to run");
                return;
            }
            SeperateSubJobForDeclareJob(availableSites, excludeNodes, jobId, runBy, jobType);
        }
        //SettingService
        private void SeperateSubJobForDeclareJob(List<RMSPTreeNode> availableSites, List<RuleNodeContract> excludeNodes, string jobId, JobRunBy runBy, JobType jobType)
        {
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();

            Dictionary<int, List<RMSPTreeNode>> subJobNodeDic = new Dictionary<int, List<RMSPTreeNode>>();
            int count = 0;
            foreach (RMSPTreeNode site in availableSites)
            {
                tempList.Add(site);
                if (tempList.Count >= RMGlobalConfiguration.AppConfig.NodeCountInSubJob)
                {
                    count++;
                    var temp = new List<RMSPTreeNode>();
                    temp.AddRange(tempList);
                    subJobNodeDic.Add(count, temp);
                    tempList.Clear();
                }
            }
            if (tempList.Count > 0)
            {
                count++;
                subJobNodeDic.Add(count, tempList);
            }
            SubJobDao.UpdateSubJobCount(jobId, count);
            logger.Info("Sub job count for [{0}] is [{1}]", jobId, count);

            int currentSubjobIndex = 0;
            foreach (KeyValuePair<int, List<RMSPTreeNode>> pa in subJobNodeDic)
            {
                string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, count, pa.Value, currentSubjobIndex < subJobCountInConfigFile, excludeNodes);
                logger.Debug("Create and queue sub job [{0}]", subJobId);
                if (currentSubjobIndex < subJobCountInConfigFile)
                {
                    mJobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = runBy,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType, subJobId),
                    });
                }
                currentSubjobIndex++;
            }
        }

        private string CreateSubJob(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMSPTreeNode> tempList, bool sendNow, List<RuleNodeContract> excludeNodes)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempList) };
            subJob.JobContext.Content = SerializerHelper.SerializeByDataContractSerializer(excludeNodes);
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job [{0}] sucessfull, type: [{1}], weight: [{2}]", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }

        public async Task<RAReturnMessage> NewOpusTenantRunPhysicalJobNowAsync(int locationID, JobRunBy jobRunBy, bool skipRemoveContentAndDestroyAction)
        {         
           return await PhysicalRecordSettingService.RunPhysicalRecordsDisposalJobAsync(locationID , jobRunBy, skipRemoveContentAndDestroyAction);
        }


        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.DisposalActivityManagement, Action = AuditAction.RunPRDisposalJob,
            AfterHandler = typeof(DisposalActivityManagementAfterAuditHandler))]
        public async Task<RAReturnMessage> OldOpusTenantRunPhysicalJobNowAsync(int locationID, JobRunBy jobRunBy, bool skipRemoveContentAndDestroyAction)
        {
            RAReturnMessage rm = new RAReturnMessage()
            {
                FaildType = RAFailedType.None,
                MessageType = RAMessageType.Successful,
            };
            try
            {
                if (!TenantService.CheckTenantIsAvailable(TenantLocalValue.LogonGroupId))
                {
                    rm.MessageType = RAMessageType.Failed;
                    rm.FaildType = RAFailedType.LicenseExpired;
                    rm.ErrorMessage = I18NEntity.GetString("RM_JM_Summary_LicenseNotAvailable");
                    return rm;
                }
                else if (TermRuleInfos.GetTermWithRule().Count == 0)
                {
                    logger.Error(I18NEntity.GetString("RM_JS_DAM_Physical_RunJob_Failed_NoRules"));
                    rm.MessageType = RAMessageType.Failed;
                    rm.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_Physical_RunJob_Failed_NoRules");
                    return rm;
                }
                FipsModeUtil.InitControlCryptoMode();
                CspCommunicationWrapper.CommunicationEncryptionKey = CspCommunicationWrapper.staticCommunicationEncryptionKey;
                DAOAPIClientV1 client = null;
                #region Get ArchiverDB And Index info
                //IMArchiverService archiverService = DocAveServiceHelper.CreateServiceClient<IMArchiverService>();
                int setting = 0;
                try
                {
                    client = new DAOAPIClientV1();
                    DAOAPIClientV1 Client1 = new DAOAPIClientV1();
                    setting = Client1.GetArchiverDBAndIndexDeviceSetting();
                }
                catch (Exception ex)
                {
                    logger.Warn("Init setting error {0}", ex.ToString());
                }
                if (setting == 1)
                {
                    rm.MessageType = RAMessageType.Failed;
                    rm.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_RunJob_Failed_NoIndexDeviceSetting");
                    return rm;
                }
                else if (setting == 2)
                {
                    rm.MessageType = RAMessageType.Failed;
                    rm.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_RunJob_Failed_NoArchiverDBSetting");
                    return rm;
                }
                else if (setting == 3)
                {
                    rm.MessageType = RAMessageType.Failed;
                    rm.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_RunJob_Failed_NoIndexDeviceAndArchiverDBSetting");
                    return rm;
                }

                #endregion
                #region Init column and global settings
                logger.Info("Init column and global settings");
                SOPlan plan = new SOPlan();
                RMCPGlobalStorageSetting rmSettings = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
                plan.SOPlanExtension = new SOPlanExtension();
                //plan.SOPlanExtension.ProcessingPoolId = rmSettings.ProcessingPoolId.ToString();
                plan.ArchiverType = GCommon.Contract.StorageOptimization.Object.ArchiverType.Full;
                plan.Category = GCommon.Contract.Server.Common.PlanCategory.Archiver;
                plan.ModuleName = "archiver";
                plan.RunNow = true;
                var connectionstr = RMGlobalConfiguration.DBConfig[RMDatabaseSettingKey.RECO_CONTROL_SQL_CONNECTION_STRING_FULL];
                var wrapConnectionStr = CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(Encoding.UTF8.GetBytes(connectionstr));
                plan.RecordWebDBConnectionString = wrapConnectionStr;
                plan.RunJobUser = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : I18NEntity.GetString("RM_TS_RunSchedule");
                plan.SkipRemoveContentAndDestroyAction = skipRemoveContentAndDestroyAction;
                AvePoint.RA.DB.Explorer.Dao.CosmosImp.CosmosConnectionInfo rmCosmosConnectionInfo = await RMDBContextManager.GetExplorerDBConnectionInfoAsync();
                AvePoint.GCommon.Contract.StorageOptimization.Object.CosmosConnectionInfo toSOCosmosConnectionInfo = null;
                if (rmCosmosConnectionInfo != null)
                {
                    toSOCosmosConnectionInfo = new AvePoint.GCommon.Contract.StorageOptimization.Object.CosmosConnectionInfo
                    {
                        CollectionId = rmCosmosConnectionInfo.CollectionId,
                        DatabaseId = rmCosmosConnectionInfo.DatabaseId,
                        Endpoint = rmCosmosConnectionInfo.Endpoint,
                        Key = rmCosmosConnectionInfo.Key
                    };
                }
                plan.RecordExplorerDB = toSOCosmosConnectionInfo;
                #region init global settings
                DataSecurity encryptionMethod = rmSettings.UseEncryption ? rmSettings.EncryptionMethod : DataSecurity.None;
                DataSecurity compressionMethod = rmSettings.UseCompression ? rmSettings.CompressionMethod : DataSecurity.None;
                plan.RecordsGlobalStorageSettings = new RecordsGlobalStorageSettings();
                plan.RecordsGlobalStorageSettings.UseCompression = rmSettings.UseCompression;
                plan.RecordsGlobalStorageSettings.UseEncryption = rmSettings.UseEncryption;
                plan.RecordsGlobalStorageSettings.CompressionMethod = compressionMethod;
                plan.RecordsGlobalStorageSettings.EncryptionMethod = encryptionMethod;
                plan.RecordsGlobalStorageSettings.CompressionSpeed = rmSettings.CompressionSpeed;
                plan.RecordsGlobalStorageSettings.SecurityProfileId = rmSettings.SecurityProfileId;
                plan.RecordsGlobalStorageSettings.SecurityProfileName = rmSettings.SecurityProfileName;
                plan.RecordsGlobalStorageSettings.StoragePolicyId = rmSettings.StoragePolicyId;
                plan.RecordsGlobalStorageSettings.StoragePolicyName = rmSettings.StoragePolicyName;
                plan.RecordsGlobalStorageSettings.ExportLocationId = rmSettings.ExportLocationId;
                plan.RecordsGlobalStorageSettings.ExportLocationName = rmSettings.ExportLocationName;
                //GetExportConfiguration(rule, (int)SourceFlag.SharePoint); if physical support export need finish the same method.
                #endregion
                #endregion
                SORuleInfoContract ruleInfo = new SORuleInfoContract();
                //List<Rule> rules = RuleManagerService.GetRulesFromDA();
                //List<Rule> physicalRules = rules.AsQueryable().Where(r => r.PhysicalRule != null && r.PhysicalRule.SOFilters.Count != 0).ToList();
                ruleInfo.Rules = new List<Rule>();
                ruleInfo.Plan = plan;
                try
                {
                    var key = client?.RunNowForPhysicalRecords(locationID, ruleInfo);
                    rm.Extension = TenantUtil.RunUnderTenant(TenantLocalValue.LogonGroupId, TenantLocalValue.LogonUserEmail, SaveFakeJobid, new List<object> { key, jobRunBy, Guid.Empty.ToString() });

                }
                catch (Exception e)
                {
                    logger.Error("error occurred while run job, ERROR:{0}", e.ToString());
                    rm.MessageType = RAMessageType.Failed;
                    rm.ErrorMessage = I18NEntity.GetString(e.Message);
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while run job, ERROR:{0}", ex.ToString());
                rm.MessageType = RAMessageType.Failed;
                rm.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_RunJob_Failed");
            }
            finally
            {
                // need to do next.
            }
            return rm;
        }

        public string SaveFakeJobid(List<object> args)
        {
            if (args.Count != 3)
            {
                throw new ArgumentException("parater is invalid");
            }

            var key = args[0] as string;
            JobRunBy jobRunBy = (JobRunBy)args[1];
            var containerId = args[2] as string;
            return JobMonitorService.CreateJobWithScopeId(JobType.DisposalActivityManagement, jobRunBy == JobRunBy.Schedule ? "RM_TS_RunSchedule" : TenantLocalValue.LogonUserEmail, key, containerId);
        }

        private string SaveSkippedFakeJobid(List<object> args)
        {
            if (args.Count != 3)
            {
                throw new ArgumentException("parater is invalid");
            }

            var reason = args[0] as string; //failed reason
            JobRunBy jobRunBy = (JobRunBy)args[1];
            var containerId = args[2] as string;
            return JobMonitorService.CreateJobWithScopeId(JobType.DisposalActivityManagement, JobStatus.Skipped, jobRunBy == JobRunBy.Schedule ? "RM_TS_RunSchedule" : TenantLocalValue.LogonUserEmail, null, containerId, reason);
        }


       


       
        public List<RuleNodeContract> BuildBreakTreeNode(RMSPTreeNode tree)
        {
            List<RuleNodeContract> breakInherting = new List<RuleNodeContract>();
            try
            {
                var parentId = ScheduleService.GetProfileId(tree) + "|";

                var treeNodes = RMScheduleDao.GetDisposalBreakNodes(parentId);
                foreach (var item in treeNodes)
                {

                    var node = JsonConvert.DeserializeObject<RMSPTreeNode>(item);
                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        continue;
                    }
                    SPTreeNodeDto spTree = RMDtoConverter.ConvertRMTree2SPTree(node);
                    var breakNode = ConvertTreeNodeToRuleNodeConfig(spTree, RuleNodeType.Archiver);
                    breakInherting.Add(breakNode);

                }

                if (tree.Type == ContentSourceType.Teams)
                {
                    var spsettings = TeamsSettingDao.GetDescendantsDisableNodes(tree);
                    foreach (var item in spsettings)
                    {
                        var node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(item.NodeInfo);
                        if (node.Level == (int)NodeLevel.WebApplication)
                        {
                            continue;
                        }
                        SPTreeNodeDto spTree = RMDtoConverter.ConvertRMTree2SPTree(node);
                        var breakNode = ConvertTreeNodeToRuleNodeConfig(spTree, RuleNodeType.Archiver);
                        breakInherting.Add(breakNode);
                    }
                }
                else
                {
                    var spsettings = SharePointSettingDao.GetDescendantsDisableNodes(tree);
                    foreach (var item in spsettings)
                    {
                        var node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(item.NodeInfo);
                        if (node.Level == (int)NodeLevel.WebApplication)
                        {
                            continue;
                        }
                        SPTreeNodeDto spTree = RMDtoConverter.ConvertRMTree2SPTree(node);
                        var breakNode = ConvertTreeNodeToRuleNodeConfig(spTree, RuleNodeType.Archiver);
                        breakInherting.Add(breakNode);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while build break tree node,ERROR:{0}", ex.ToString());
            }
            return breakInherting;
        }

        private List<Guid> GetOneDriveBreakInheritSiteIds(RMSPTreeNode tree)
        {
            List<Guid> ids = new List<Guid>();
            if (tree.Level == (int)NodeLevel.SkyDriveProGroup)
            {
                try
                {
                    var parentId = ScheduleService.GetProfileId(tree) + "|";
                    var treeNodes = RMScheduleDao.GetDisposalBreakNodes(parentId);
                    foreach (var item in treeNodes)
                    {
                        var node = JsonConvert.DeserializeObject<RMSPTreeNode>(item);
                        if (node.Level == (int)NodeLevel.SiteCollection)
                        {
                            Guid siteId = new Guid(node.SPObjectId);
                            if (!ids.Contains(siteId))
                            {
                                ids.Add(siteId);
                            }
                        }                        
                    }
                    var spsettings = OneDriveSettingDao.GetDescendantsDisableNodes(tree);
                    foreach (var item in spsettings)
                    {
                        var node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(item.NodeInfo);
                        if (node.Level == (int)NodeLevel.SiteCollection)
                        {
                            Guid siteId = new Guid(node.SPObjectId);
                            if (!ids.Contains(siteId))
                            {
                                ids.Add(siteId);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("error occurred while build break tree node,ERROR:{0}", ex.ToString());
                }
            }
            return ids;
        }
        private List<RuleNodeContract> BuildOneDriveBreakTreeNode(RMSPTreeNode tree)
        {
            List<RuleNodeContract> breakInherting = new List<RuleNodeContract>();
            try
            {
                var parentId = ScheduleService.GetProfileId(tree) + "|";
                var treeNodes = RMScheduleDao.GetDisposalBreakNodes(parentId);
                foreach (var item in treeNodes)
                {
                    var node = JsonConvert.DeserializeObject<RMSPTreeNode>(item);
                    if (node.Level == (int)NodeLevel.WebApplication || node.Level == (int)NodeLevel.SkyDriveProGroup)
                    {
                        logger.Info("BuildOneDriveBreakTreeNode RMSchedule break node is Container and skip it.:{0}.", node.FullPath);
                        continue;
                    }
                    SPTreeNodeDto spTree = RMDtoConverter.ConvertRMTree2SPTree(node);
                    var breakNode = ConvertTreeNodeToRuleNodeConfig(spTree, RuleNodeType.Archiver);
                    breakInherting.Add(breakNode);
                    logger.Info("BuildOneDriveBreakTreeNode RMSchedule break node:{0}.", breakNode.FullPath);
                }
                var spsettings = OneDriveSettingDao.GetDescendantsDisableNodes(tree);
                foreach (var item in spsettings)
                {
                    var node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(item.NodeInfo);
                    if (node.Level == (int)NodeLevel.WebApplication || node.Level == (int)NodeLevel.SkyDriveProGroup)
                    {
                        logger.Info("BuildOneDriveBreakTreeNode OneDriveSetting break node is Container and skip it.:{0}.", node.FullPath);
                        continue;
                    }
                    SPTreeNodeDto spTree = RMDtoConverter.ConvertRMTree2SPTree(node);
                    var breakNode = ConvertTreeNodeToRuleNodeConfig(spTree, RuleNodeType.Archiver);
                    breakInherting.Add(breakNode);
                    logger.Info("BuildOneDriveBreakTreeNode OneDriveSetting break node:{0}.", breakNode.FullPath);
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while build break tree node,ERROR:{0}", ex.ToString());
            }
            return breakInherting;
        }

        private RuleNodeContract ConvertTreeNodeToRuleNodeConfig(SPTreeNodeDto node, RuleNodeType type)
        {
            if (node == null)
            {
                return null;
            }
            RuleNodeContract result = new RuleNodeContract();
            result.Id = Guid.NewGuid().ToString();
            result.NodeId = node.SPObjectId;
            result.NodeName = node.Name;
            result.DisplayName = node.DisplayName;
            result.ManagerTreeId = node.ID;
            result.FullPath = node.FullPath;
            result.FarmId = node.FarmID;
            //result.SPType = node.SPType;
            if (node.NodeExtension != null && node.NodeExtension.BposInfo != null)
            {
                result.BposInfo = node.NodeExtension.BposInfo;
            }
            if (node.Parent != null)  //Farm 级别没有Parent
            {
                if (node.Parent.Level == NodeLevel.Sites || node.Parent.Level == NodeLevel.Lists || node.Parent.Level == NodeLevel.Folders)
                {
                    result.ParentNodeId = node.Parent.Parent == null ? null : node.Parent.Parent.SPObjectId;
                    result.ParentNodeName = node.Parent.Parent == null ? null : node.Parent.Parent.Name;
                }
                else
                {
                    result.ParentNodeId = node.Parent.SPObjectId;
                    result.ParentNodeName = node.Parent.Name;
                }
            }
            result.NodeLevel = node.Level;
            result.SPVersion = node.SPVersion;
            result.Type = type;
            AssignSPObjectId(node, ref result);
            //在处理index的时候需要转换children
            if (node.Children != null && node.Children.Count > 0 && type == RuleNodeType.IndexDevice)
            {
                result.Children = new List<RuleNodeContract>();
                foreach (SPTreeNodeDto child in node.Children)
                {
                    RuleNodeContract childRuleNode = new RuleNodeContract();
                    childRuleNode = ConvertTreeNodeToRuleNodeConfig(child, type);
                    if (childRuleNode != null)
                    {
                        childRuleNode.ParentNode = result;
                        result.Children.Add(childRuleNode);
                    }
                }
            }
            return result;
        }


        private static void AssignSPObjectId(SPTreeNodeDto node, ref RuleNodeContract config)
        {
            if (node.Level >= NodeLevel.Folder || node.Level == NodeLevel.Sites || node.Level == NodeLevel.Lists)
            {
                AssignSPObjectId(node.Parent, ref config);
            }
            if (node.Level == NodeLevel.List)
            {
                config.ListId = node.SPObjectId;
                config.ListTitle = node.Name;
                AssignSPObjectId(node.Parent, ref config);
            }
            if (node.Level == NodeLevel.Site)
            {
                if (string.IsNullOrEmpty(config.WebId))
                {
                    config.WebId = node.SPObjectId;
                }
                AssignSPObjectId(node.Parent, ref config);
            }
            if (node.Level == NodeLevel.SiteCollection)
            {
                config.SiteId = node.ID;
                config.SiteUrl = node.Url;
                if (node.Parent != null)
                {
                    AssignSPObjectId(node.Parent, ref config);
                }
            }
            if (node.Level == NodeLevel.WebApplication)
            {
                config.WebAppId = node.SPObjectId;
                config.WebAppUrl = node.FullPath;
            }
        }
        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.DisposalActivityManagement, Action = AuditAction.RunEXODisposalJob,
            AfterHandler = typeof(DisposalActivityManagementAfterAuditHandler))]
        public async Task<RAReturnMessage> RunEXONowAsync(RMEXOTreeNode tree, JobRunBy jobRunBy)
        {
            RAReturnMessage rm = new RAReturnMessage()
            {
                FaildType = RAFailedType.None,
                MessageType = RAMessageType.Successful,
            };
            if (!TenantService.CheckTenantIsAvailable(TenantLocalValue.LogonGroupId))
            {
                rm.MessageType = RAMessageType.Failed;
                rm.FaildType = RAFailedType.LicenseExpired;
                rm.ErrorMessage = I18NEntity.GetString("RM_JM_Summary_LicenseNotAvailable");
                return rm;
            }
            string jobids = "";
            List<RMEXOTreeNode> treeNodes = new List<RMEXOTreeNode>();
            treeNodes = new List<RMEXOTreeNode>();
            treeNodes.Add(tree);
            bool isHasRemovedSite = false;
            bool isHasSuccessRunJobNode = false;
            DAOAPIClientV1 client = new DAOAPIClientV1();
            int setting = 0;
            try
            {
                DAOAPIClientV1 Client1 = new DAOAPIClientV1();
                setting = Client1.GetArchiverDBAndIndexDeviceSetting();
            }
            catch (Exception ex)
            {
                logger.Warn("Init setting error {0}", ex.ToString());
            }

            if (setting == 2 || setting == 3)
            {
                rm.MessageType = RAMessageType.Failed;
                rm.FaildType = RAFailedType.NoDBSetting;
                rm.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_RunJob_Failed_NoArchiverDBSetting");
                logger.Warn("check archiver setting faild:{0}", rm.ErrorMessage);
                return rm;
            }

            foreach (var treeNode in treeNodes)
            {               
                try
                {
                    string jobKey = string.Empty;
                    RMEXOTreeNode treeClone = null;
                    var groupId = TreeNodeUtil.GetMailboxGroupNode(treeNode).Id;
                    Guid mailboxGroupId = Guid.Parse(groupId);
                    if (!CheckNode(treeNode))
                    {
                        isHasRemovedSite = true;
                        logger.Warn("{0}, Node Full Path :{1}", I18NEntity.GetString("RM_SS_FolderRemoved"), treeNode.FullPath);
                        continue;
                    }

                    ExchangeOnlineTreeNodeDto exoTree = RMDtoConverter.ConvertRMExchangeTree2TreeNodeDto(treeClone == null ? treeNode : treeClone, null);
                    logger.Info("Run job Node FullPath:[{0}]", exoTree?.FullPath);

                    Dictionary<int, Rule> ruleResults = new Dictionary<int, Rule>();                  
                    List<Rule> allRecordsRules = RuleManagerService.GetRulesFromRecords();
                    List<Rule> allRecordsEXORules = allRecordsRules.AsQueryable().Where(r => r.EXORule != null && r.EXORule.SOFilters.Count != 0).ToList();
                    var plan = await GetSOPlanAsync(GCommon.Contract.Server.Common.PlanCategory.ExchangeArchiver);
                    bool isNullClassificationSetting = CheckIsNullClassificationSetting(treeNode, mailboxGroupId);
                    #region Init column and global settings
                    RMCPGlobalStorageSetting rmSettings = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
                    plan.RunJobUser = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : I18NEntity.GetString("RM_TS_RunSchedule");
                    string policyId = rmSettings?.StoragePolicyId.ToString();
                    logger.Info("storage policy id:{0}", policyId);
                    var connectionstr = RMGlobalConfiguration.DBConfig[RMDatabaseSettingKey.RECO_CONTROL_SQL_CONNECTION_STRING_FULL];
                    var recordHistoryStr = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECORDS_HISTORY_STORAGE_CONNECTION_STRING_FULL];
                    var wrapConnectionStr = CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(Encoding.UTF8.GetBytes(connectionstr));
                    var recordHistoryConnectionStr = CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(Encoding.UTF8.GetBytes(recordHistoryStr));
                    plan.RecordWebDBConnectionString = wrapConnectionStr;
                    plan.RecordsHistoryDBConnectionString = recordHistoryConnectionStr;
                    plan.RunJobUser = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : I18NEntity.GetString("RM_TS_RunSchedule");
                    plan.SkipRemoveContentAndDestroyAction = tree.SkipRemoveContentAndDestroyAction;
                    plan.IsNullClassificationSetting = isNullClassificationSetting;
                    AvePoint.RA.DB.Explorer.Dao.CosmosImp.CosmosConnectionInfo rmCosmosConnectionInfo = await RMDBContextManager.GetExplorerDBConnectionInfoAsync();
                    AvePoint.GCommon.Contract.StorageOptimization.Object.CosmosConnectionInfo toSOCosmosConnectionInfo = null;
                    if (rmCosmosConnectionInfo != null)
                    {
                        toSOCosmosConnectionInfo = new AvePoint.GCommon.Contract.StorageOptimization.Object.CosmosConnectionInfo
                        {
                            CollectionId = rmCosmosConnectionInfo.CollectionId,
                            DatabaseId = rmCosmosConnectionInfo.DatabaseId,
                            Endpoint = rmCosmosConnectionInfo.Endpoint,
                            Key = rmCosmosConnectionInfo.Key
                        };
                    }
                    plan.RecordExplorerDB = toSOCosmosConnectionInfo;
                    #endregion

                    try
                    {
                        ruleResults = ArchiverRuleService.GetEXORuleCollection(mailboxGroupId, isNullClassificationSetting);
                    }
                    catch (Exception e)
                    {
                        if (!string.IsNullOrWhiteSpace(e.Message) && (e.Message.Contains(I18NEntity.GetString("RM_JS_DAM_FaildRun_NoExportLocation")) || e.Message.Contains(I18NEntity.GetString("RM_JS_DAM_FaildRun_FTPExportLocationNotSupported"))))
                        {
                            rm.MessageType = RAMessageType.Failed;
                            rm.ErrorMessage = e.Message;
                            return rm;
                        }
                        else
                        {
                            throw;
                        }
                    }                   
                    logger.Info("get term rule result");
                    //if (ruleResults.Count != 0 && !string.IsNullOrEmpty(metadataColumn))
                    if (ruleResults.Count != 0)
                    {
                        var breakInherting = ArchiverRuleService.BuildBreakTreeNode(tree);
                        if (breakInherting != null)
                        {
                            foreach (var bNode in breakInherting)
                            {
                                logger.Info("breaking Inhert Node Id is :{0}", bNode.Id);

                            }
                        }
                        string userName = string.Empty;

                        RMDtoConverter.ConvertEXOTreeBeforeToJSON(exoTree);
                        List<ExchangeOnlineTreeNodeDto> exoTrees = new List<ExchangeOnlineTreeNodeDto>();
                        exoTrees.Add(exoTree);
                        SORuleInfoContract ruleInfo = new SORuleInfoContract();
                        ruleInfo.Rules = ruleResults.Select(r => r.Value).ToList();
                        ruleInfo.Plan = plan;
                        ruleInfo.Plan.GroupBCSColumnDictionary = GetGroupBcsColumnDic();
                        try
                        {
                            jobKey = client.RunNowForExchange(exoTrees, ruleInfo, breakInherting);//TODO i18n
                            rm.Extension = TenantUtil.RunUnderTenant(TenantLocalValue.LogonGroupId, TenantLocalValue.LogonUserEmail, SaveFakeJobid, new List<object> { jobKey, jobRunBy, GetRMEXOTreeNodeContainerId(tree) });
                        }
                        catch (Exception e)
                        {
                            logger.Error("error occurred while run job, ERROR:{0}", e.ToString());
                            rm.MessageType = RAMessageType.Failed;
                            rm.ErrorMessage = I18NEntity.GetString(e.Message);
                        }
                    }
                    else
                    {
                        rm.MessageType = RAMessageType.Failed;
                        rm.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_RunJob_Failed_NoRules");//TODO i18n
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("error occurred while run job, ERROR:{0}", ex.ToString());
                    rm.MessageType = RAMessageType.Failed;
                    rm.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_RunJob_Failed");//TODO i18n
                }               
            }
            if (isHasRemovedSite && !isHasSuccessRunJobNode)
            {
                rm.MessageType = RAMessageType.Failed;
                rm.ErrorMessage = I18NEntity.GetString("RM_SS_FolderRemoved");//Remove from FS
            }
            rm.Extsion1 = jobids.TrimEnd(';');
            return rm;
        }

        /// <summary>
        /// 能进入NullClassificationSetting条件： 1 && 2：
        /// 1.当前Mailbox Group设置了IsNullClassificationSetting.
        /// 2.当前Mailbox没有单独的Term Setting.
        /// 3.Group节点Run Job不需要check#2，只有Mailbox节点Run Job需要check#2.
        /// </summary>
        private bool CheckIsNullClassificationSetting(RMEXOTreeNode treeNode, Guid groupId)
        {
            bool isNullClassificationSetting = false;
            RMExchangeOnlineSetting currentNodeTermSetting = null;
            if (treeNode.Level == (int)NodeLevel.ExchangeOnlineMailbox)
            {
                currentNodeTermSetting = EXOSettingDao.GetSettingInfoByScope(groupId, Guid.Empty, new Guid(treeNode.Id));
            }
            if (treeNode.IsNullClassificationSetting)
            {
                if (currentNodeTermSetting == null)
                {
                    isNullClassificationSetting = true;
                }
                else if (currentNodeTermSetting != null && currentNodeTermSetting.TermSetId == Guid.Empty)
                {
                    isNullClassificationSetting = true;
                }
            }
            return isNullClassificationSetting;
        }

        private Dictionary<Guid, Tuple<bool, string>> GetGroupBcsColumnDic()
        {
            Dictionary<Guid, Tuple<bool, string>> dic = new Dictionary<Guid, Tuple<bool, string>>();
            //var groupLevelSettings = SharepointSettingDao.GetAllGroupSettings();
            //foreach (var setting in groupLevelSettings)
            //{
            //    if (setting.IsUsingExistColumnName)
            //    {
            //        dic.Add(setting.ScopeId, new Tuple<bool, string>(true, setting.ExistColumnName));
            //    }
            //    else
            //    {
            //        dic.Add(setting.ScopeId, new Tuple<bool, string>(false, setting.ColumnName));
            //    }
            //}
            logger.Info("Group BCS Setting:{0}", string.Join(",", dic.Keys.ToList()));
            return dic;
        }

        private async Task<SOPlan> GetSOPlanAsync(GCommon.Contract.Server.Common.PlanCategory category)
        {
            logger.Info("Init column and global settings");
            SOPlan plan = new SOPlan();

            plan.SOPlanExtension = new SOPlanExtension();
            plan.ArchiverType = GCommon.Contract.StorageOptimization.Object.ArchiverType.Full;
            plan.Category = category;
            plan.ModuleName = "archiver";//TO DO confirm
            plan.RunNow = true;

            var connectionstr = RMGlobalConfiguration.DBConfig[RMDatabaseSettingKey.RECO_CONTROL_SQL_CONNECTION_STRING_FULL];
            var wrapConnectionStr = CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(Encoding.UTF8.GetBytes(connectionstr));
            plan.RecordWebDBConnectionString = wrapConnectionStr;


            AvePoint.RA.DB.Explorer.Dao.CosmosImp.CosmosConnectionInfo rmCosmosConnectionInfo = await RMDBContextManager.GetExplorerDBConnectionInfoAsync();
            AvePoint.GCommon.Contract.StorageOptimization.Object.CosmosConnectionInfo toSOCosmosConnectionInfo = null;
            if (rmCosmosConnectionInfo != null)
            {
                toSOCosmosConnectionInfo = new AvePoint.GCommon.Contract.StorageOptimization.Object.CosmosConnectionInfo
                {
                    CollectionId = rmCosmosConnectionInfo.CollectionId,
                    DatabaseId = rmCosmosConnectionInfo.DatabaseId,
                    Endpoint = rmCosmosConnectionInfo.Endpoint,
                    Key = rmCosmosConnectionInfo.Key
                };
            }
            plan.RecordExplorerDB = toSOCosmosConnectionInfo;

            return plan;
        }



        //private List<RuleNodeContract> BuildBreakTreeNode(RMEXOTreeNode tree)
        //{
        //    List<RuleNodeContract> breakInherting = new List<RuleNodeContract>();
        //    try
        //    {

        //        var parentId = GetParentProfileId(tree);

        //        var profileIds = EXOSettingDao.GetBreakNodeIds(parentId);

        //        var spsettings = EXOSettingDao.FindList(s => profileIds.Contains(s.IdPath));
        //        foreach (var item in spsettings)
        //        {
        //            var node = SerializerHelper.DeserializeByDataContractSerializer<RMEXOTreeNode>(item.NodeInfo);
        //            if (node.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup || node.Level == (int)NodeLevel.ExchangeOnlineO365Group)
        //            {
        //                continue;
        //            }
        //            ExchangeOnlineTreeNodeDto spTree = RMDtoConverter.ConvertRMExchangeTree2TreeNodeDto(node);
        //            var breakNode = ConvertTreeNodeToRuleNodeConfig(spTree, RuleNodeType.Archiver);
        //            breakInherting.Add(breakNode);

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("error occurred while build break tree node,ERROR:{0}", ex.ToString());
        //    }
        //    return breakInherting;
        //}

        //private List<RuleNodeContract> BuildBreakTreeNode(RMEXOTreeNode tree)
        //{
        //    List<RuleNodeContract> breakInherting = new List<RuleNodeContract>();
        //    if (tree.Level != (int)NodeLevel.ExchangeOnlineMailbox)
        //    {
        //        try
        //        {
        //            var parentId = ScheduleService.GetProfileId(tree) + "|";
        //            var treeNodes = RMScheduleDao.GetDisposalBreakNodes(parentId);
        //            foreach (var item in treeNodes)
        //            {
        //                var node = JsonConvert.DeserializeObject<RMEXOTreeNode>(item);
        //                if (node.Level == (int)NodeLevel.ExchangeOnlineO365Group)
        //                {
        //                    continue;
        //                }
        //                ExchangeOnlineTreeNodeDto exoTree = RMDtoConverter.ConvertRMExchangeTree2TreeNodeDto(node);
        //                var breakNode = ConvertTreeNodeToRuleNodeConfig(exoTree, RuleNodeType.Archiver);
        //                breakInherting.Add(breakNode);
        //            }

        //            var spsettings = EXOSettingDao.GetDescendantsDisableNodes(tree);
        //            foreach (var item in spsettings)
        //            {
        //                var node = SerializerHelper.DeserializeByDataContractSerializer<RMEXOTreeNode>(item.NodeInfo);
        //                if (node.Level == (int)NodeLevel.ExchangeOnlineO365Group)
        //                {
        //                    continue;
        //                }
        //                ExchangeOnlineTreeNodeDto exoTree = RMDtoConverter.ConvertRMExchangeTree2TreeNodeDto(node);
        //                var breakNode = ConvertTreeNodeToRuleNodeConfig(exoTree, RuleNodeType.Archiver);
        //                breakInherting.Add(breakNode);
        //            }

        //            if (tree.IsNullClassificationSetting)
        //            {
        //                var nonNullClassificationSetting = EXOSettingDao.GetDescendantsBreakNodesForNullClassification(tree);
        //                var groupMailboxs = MailBoxService.GetEmailsByEmailGroupIdForBrowse(tree.Id);
        //                foreach (var item in nonNullClassificationSetting)
        //                {
        //                    var node = SerializerHelper.DeserializeByDataContractSerializer<RMEXOTreeNode>(item.NodeInfo);
        //                    if (node.Level == (int)NodeLevel.ExchangeOnlineO365Group)
        //                    {
        //                        continue;
        //                    }
        //                    if (groupMailboxs != null && groupMailboxs.Where(mailbox => mailbox.Email == item.Name).FirstOrDefault() != null && groupMailboxs.Where(mailbox => mailbox.Email == item.Name).FirstOrDefault().Id != item.ScopeId.ToString())
        //                    {
        //                        logger.Warn("Current Mailbox:{0} has unique setting but ScopeId:{1} does not save as MailboxId:{2}.So skip it when check IsNullClassificationSetting.", item.Name, item.ScopeId, groupMailboxs.Where(mailbox => mailbox.Email == item.Name).FirstOrDefault().Id);
        //                        continue;
        //                    }
        //                    ExchangeOnlineTreeNodeDto exoTree = RMDtoConverter.ConvertRMExchangeTree2TreeNodeDto(node);
        //                    var breakNode = ConvertTreeNodeToRuleNodeConfig(exoTree, RuleNodeType.Archiver);
        //                    breakInherting.Add(breakNode);
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("error occurred while build break tree node,ERROR:{0}", ex.ToString());
        //        }
        //    }
        //    return breakInherting;
        //}

        //private RuleNodeContract ConvertTreeNodeToRuleNodeConfig(ExchangeOnlineTreeNodeDto node, RuleNodeType type)
        //{
        //    if (node == null)
        //    {
        //        return null;
        //    }
        //    RuleNodeContract result = new RuleNodeContract();
        //    result.Id = Guid.NewGuid().ToString();
        //    result.NodeId = node.ID;
        //    result.NodeName = node.Name;
        //    result.DisplayName = node.DisplayName;
        //    result.ManagerTreeId = node.ID;
        //    result.FullPath = node.FullPath;
        //    result.FarmId = node.FarmID;
        //    //result.SPType = node.SPType;
        //    if (node.NodeExtension != null && node.NodeExtension.BposInfo != null)
        //    {
        //        result.BposInfo = node.NodeExtension.BposInfo;
        //    }
        //    if (node.Parent != null)  //Farm 级别没有Parent
        //    {
        //        if (node.Parent.Level == NodeLevel.Sites || node.Parent.Level == NodeLevel.Lists || node.Parent.Level == NodeLevel.Folders)
        //        {
        //            result.ParentNodeId = node.Parent.Parent == null ? null : node.Parent.Parent.ID;
        //            result.ParentNodeName = node.Parent.Parent == null ? null : node.Parent.Parent.Name;
        //        }
        //        else
        //        {
        //            result.ParentNodeId = node.Parent.ID;
        //            result.ParentNodeName = node.Parent.Name;
        //        }
        //    }
        //    result.NodeLevel = node.Level;
        //    result.Type = type;
        //    AssignSPObjectId(node, ref result);
        //    //在处理index的时候需要转换children
        //    if (node.Children != null && node.Children.Count > 0 && type == RuleNodeType.IndexDevice)
        //    {
        //        result.Children = new List<RuleNodeContract>();
        //        foreach (ExchangeOnlineTreeNodeDto child in node.Children)
        //        {
        //            RuleNodeContract childRuleNode = new RuleNodeContract();
        //            childRuleNode = ConvertTreeNodeToRuleNodeConfig(child, type);
        //            if (childRuleNode != null)
        //            {
        //                childRuleNode.ParentNode = result;
        //                result.Children.Add(childRuleNode);
        //            }
        //        }
        //    }
        //    return result;
        //}

        //private static void AssignSPObjectId(ExchangeOnlineTreeNodeDto node, ref RuleNodeContract config)
        //{
        //    if (node.Level == NodeLevel.ExchangeOnlineMailbox)
        //    {
        //        config.SiteId = node.ID;
        //        config.SiteUrl = node.FullPath;
        //        if (node.Parent != null)
        //        {
        //            AssignSPObjectId(node.Parent, ref config);
        //        }
        //    }
        //    if (node.Level == NodeLevel.ExchangeOnlineMailboxGroup || node.Level == NodeLevel.ExchangeOnlineO365Group)
        //    {
        //        config.WebAppId = node.ID;
        //        config.WebAppUrl = node.FullPath;
        //    }
        //}

        private bool CheckNode(RMEXOTreeNode node)
        {
            bool isExists = false;
            var groupNode = TreeNodeUtil.GetMailboxGroupNode(node);
            switch (node.Level)
            {
                case (int)NodeLevel.ExchangeOnlineMailboxGroup:
                    isExists = ValidataEXONode(groupNode);
                    break;
                default:
                    if (ValidataEXONode(groupNode))
                    {
                        var siteNode = TreeNodeUtil.GetMailboxNode(node);
                        isExists = ValidataEXONode(siteNode);
                    }
                    break;
            }
            return isExists;
        }

        /*private List<int> GetTermIdsWithRule(List<int> termIds, Guid agentGroupId)
        {
            //filter the terms under other group
            var settingInfo = EXOSettingDao.GetSettingInfoByAgentGroupId(agentGroupId.ToString());
            List<int> alltermIds = null;
            List<int> resultIds = new List<int>();
            if (settingInfo != null)
            {
                var termSetId = settingInfo.TermSetId;
                if (!termSetId.Equals(Guid.Empty))
                {
                    RMTermSet termSet = TermSetDao.GetRMTermSetByGuid(termSetId);
                    if (termSet != null)
                    {
                        alltermIds = TermDao.GetAllTermIds();
                        if (alltermIds != null && alltermIds.Count > 0)
                        {
                            foreach (var termId in termIds)
                            {
                                if (alltermIds.Contains(termId))
                                {
                                    resultIds.Add(termId);
                                }
                            }
                        }
                    }
                }
            }
            if (resultIds != null && resultIds.Count > 0)
            {
                return resultIds;
            }
            else
            {
                return termIds;
            }

        }*/

        private bool ValidataEXONode(RMEXOTreeNode node)
        {
            bool result = false;
            try
            {
                //DAOAPIClientV1 client = new DAOAPIClientV1();
                //result = client.GetExchangeNodeById(node.Id) != null;
                result = RABrowserClient.GetExchangeNodeById(node.Id) != null;

            }
            catch (Exception ex)
            {
                logger.Error("get exoNode error:{0}", ex.ToString());
            }
            return result;
        }

       
        private byte[] GetMemoryStream(string unZipFolder, string fileName)
        {
            using (FileStream fs = new FileStream(Path.Combine(unZipFolder, fileName), FileMode.Open, FileAccess.Read))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    fs.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        public bool CheckIsRemoteSite(RMSPTreeNode treeNode)
        {
            if (treeNode.Level != (int)NodeLevel.WebApplication)
            {
                GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection remoteSite = RMRemoteNodeDao.GetRemoteSiteCollectionById(treeNode.GetSiteCollectionNode().Id);
                if (remoteSite == null)
                {
                    logger.Warn("Site collection not exist. Url:{0}", treeNode.Name);
                    return true;
                }
                else
                {
                    var groupNode = treeNode.GetGroupNode();
                    return !groupNode.Id.Equals(remoteSite.parentId, StringComparison.OrdinalIgnoreCase);
                }
            }
            else
            {
                var group = RMRemoteNodeDao.GetWebApplicationById(treeNode.Id);
                if (group == null)
                {
                    logger.Warn("Group not exist. Url:{0}", treeNode.Name);
                    return true;
                }
                return false;
            }
        }

        public bool CheckIsRemoteTeamsExisting(RMSPTreeNode treeNode)
        {
            if (treeNode.Level != (int)NodeLevel.WebApplication)
            {
                GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection teamsGroup = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(treeNode.GetTeamsNode().TeamsId).Item1;
                if (teamsGroup == null)
                {
                    logger.Warn("Teams group does not exist. Url:{0}", treeNode.Name);
                    return true;
                }
                else
                {
                    var groupNode = treeNode.GetGroupNode();
                    return !groupNode.Id.Equals(teamsGroup.parentId, StringComparison.OrdinalIgnoreCase);
                }
            }
            else
            {
                var group = RMRemoteNodeDao.GetWebApplicationById(treeNode.Id);
                if (group == null)
                {
                    logger.Warn("Group not exist. Url:{0}", treeNode.Name);
                    return true;
                }
                return false;
            }
        }


        public bool CheckEXONodeMoved(RMEXOTreeNode treeNode)
        {
            if (treeNode.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup)
            {
                return false;
            }
            else
            {
                var exoNode = MailBoxService.GetMailboxById(treeNode.Id);
                if (exoNode == null)
                {
                    logger.Warn("Mailbox not exists. Id:{0}", treeNode.Id);
                    return true;
                }
                var groupNode = treeNode.GetMailboxGroupNode();
                return !exoNode.ParentId.Equals(groupNode.Id, StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool IsFSConnectionDeleted(AvePoint.RA.Contract.Object.RMFSTreeNode treeNode)
        {
            if (treeNode.Level == (int)NodeLevel.WebApplication)
            {
                var group = FSConnectionGroupDao.GetGroupById(treeNode.Id);
                return group == null;
            }
            else
            {
                var topNodes = FindTop3LevelNodes(treeNode);
                var connectionId = topNodes.Item3.Id;
                if (connectionId == Guid.Empty)
                {
                    return true;
                }
                else
                {
                    var connection = FSConnectionDao.GetConnectionById(connectionId);
                    return connection == null;
                }
            }
        }

        //TODO Derek, add control for running disposal job in records
        public bool RunDisposalInRecords()
        {
            var key = RMKeyValueDao.GetValueByKey("RunDisposalInRecords");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }

        private System.Tuple<RMFSTreeNode, RMFSTreeNode, RMFSTreeNode> FindTop3LevelNodes(RMFSTreeNode node)
        {
            if (node.Parent == null)
            {
                throw new Exception("The level of current node is less then 3.");
            }
            if (node.Parent.Parent == null)
            {
                throw new Exception("The level of current node is less then 3.");
            }
            if (node.Parent.Parent.Parent == null)
            {
                return new System.Tuple<RMFSTreeNode, RMFSTreeNode, RMFSTreeNode>(node.Parent.Parent, node.Parent, node);
            }
            var tempNode = node;
            while (tempNode.Parent.Parent.Parent != null)
            {
                tempNode = tempNode.Parent;
            }
            return new System.Tuple<RMFSTreeNode, RMFSTreeNode, RMFSTreeNode>(tempNode.Parent.Parent, tempNode.Parent, tempNode);
        }

        public bool CheckIsOneDriveNode(RMSPTreeNode treeNode)
        {
            try
            {
                var webApp = RABrowserClient.GetWebApplicationById(treeNode.GetGroupNode().Id);
                return webApp != null && webApp.NodeType == GCommon.Contract.Server.ControlPanel.Office365.RemoveNodeType.SkyDrivePro ? true : false;

            }
            catch (Exception e)
            {
                logger.Info("An error occurred while checking if node is onedrive, path:{0} error:{1}", treeNode?.FullPath, e.ToString());
                return false;
            }
        }

        public bool CheckIsTeamsNode(RMSPTreeNode treeNode)
        {
            try
            {
                var webApp = RABrowserClient.GetWebApplicationById(treeNode.GetGroupNode().Id);
                return webApp != null && (webApp.NodeType == GCommon.Contract.Server.ControlPanel.Office365.RemoveNodeType.O365GroupSites || webApp.url.Equals("Default Private Channel Sites Container")) ? true : false;
            }
            catch (Exception e)
            {
                logger.Info("An error occurred while checking if node is teams, path:{0} error:{1}", treeNode?.FullPath, e.ToString());
                return false;
            }
        }

        public int GetTenantMainJobCount()
        {
            var result = RMKeyValueDaoExtension.GetMainJobCountFromDB(RMKeyValueDao);
            logger.Info($"Tenant max main job count is :[{result}], id: [{TenantLocalValue.LogonGroupId}]");
            return result;
        }
    }
}
