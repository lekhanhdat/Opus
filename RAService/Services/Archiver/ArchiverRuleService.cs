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
using AvePoint.Cryptography;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.GranularBackup.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.RuleManagement;
using AvePoint.RA.Service.Services.StorageDevice;
using Newtonsoft.Json;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AvePoint.RA.Service.Services.Archiver
{
    public class ArchiverRuleService : RMServiceBase, IArchiverRuleService
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(ArchiverRuleService));
        #region interface     
        public IEXOSettingRuleDao EXOSettingRuleDao => (IEXOSettingRuleDao)PlatformWindsorManager.GetService(typeof(IEXOSettingRuleDao));
        public IRuleManagerService RuleManagerService => (IRuleManagerService)PlatformWindsorManager.GetService(typeof(IRuleManagerService));
        public IGlobalStorageSettingDao GlobalStorageSettingDao => (IGlobalStorageSettingDao)PlatformWindsorManager.GetService(typeof(IGlobalStorageSettingDao));
        public ITermRuleAssociationDao TermRuleInfos => (ITermRuleAssociationDao)PlatformWindsorManager.GetService(typeof(ITermRuleAssociationDao));
        public IGlobalSettingService GlobalSettingService => (IGlobalSettingService)PlatformWindsorManager.GetService(typeof(IGlobalSettingService));
        public ITermDao TermDao => (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
        public IRMKeyValueDao RMKeyValueDao => (IRMKeyValueDao)PlatformWindsorManager.GetService(typeof(IRMKeyValueDao));
        public IExportSettingsDao ExportSettingsDao => (IExportSettingsDao)PlatformWindsorManager.GetService(typeof(IExportSettingsDao));
        public IEXOSettingDao EXOSettingDao => (IEXOSettingDao)PlatformWindsorManager.GetService(typeof(IEXOSettingDao));
        public ITermSetDao TermSetDao => (ITermSetDao)PlatformWindsorManager.GetService(typeof(ITermSetDao));
        public IExportDataEncryptionSettingService ExportDataEncryptionSettingService => (IExportDataEncryptionSettingService)PlatformWindsorManager.GetService(typeof(IExportDataEncryptionSettingService));
        public IRMMailboxService MailBoxService => (IRMMailboxService)PlatformWindsorManager.GetService(typeof(IRMMailboxService));
        public IScheduleService ScheduleService => (IScheduleService)PlatformWindsorManager.GetService(typeof(IScheduleService));
        public IRMScheduleDao RMScheduleDao => (IRMScheduleDao)PlatformWindsorManager.GetService(typeof(IRMScheduleDao));
        public IJobMonitorDao JobMonitorDao => (IJobMonitorDao)PlatformWindsorManager.GetService(typeof(IJobMonitorDao));
        public IJobQueueService JobQueueService => (IJobQueueService)PlatformWindsorManager.GetService(typeof(IJobQueueService));
        public IJobMonitorService JobMonitorService => (IJobMonitorService)PlatformWindsorManager.GetService(typeof(IJobMonitorService));
        #endregion
        public Dictionary<int, Rule> GetEXORuleCollection(Guid GroupId, bool isNullClassification)
        {
            Dictionary<int, Rule> ruleResults = new Dictionary<int, Rule>();
            //var exportLocationTypes = GlobalSettingService.GetExportLocationTypes();
            var ruleAssembler = new EXORuleAssembler();
            SortedSet<string> invalidRule = new SortedSet<string>();
            SortedSet<string> invalidRuleWithFtpLocation = new SortedSet<string>();
            List<Rule> allRecordsEXORules = RuleManagerService.GetRulesFromRecords().AsQueryable().Where(r => r.EXORule != null && r.EXORule.SOFilters.Count != 0).ToList();
            if (isNullClassification)
            {
                List<RMSimpleRule> rMSimpleRules = EXOSettingRuleDao.GetMappingRules(GroupId).OrderBy(x => x.RuleOrder).ToList();
                logger.Info("Current EXO Setting is IsNullTermClassification:mailboxGroupId{0}.EXOSettingRuleDao count:{1}.", GroupId, rMSimpleRules.Count);
                #region Init column and global settings
               // RMCPGlobalStorageSetting rmSettings = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();

                //string policyId = rmSettings?.StoragePolicyId.ToString();
               // logger.Info("storage policy id:{0}", policyId);
                #endregion
                logger.Info("Init terminfo in rules");

                foreach (var exoRule in rMSimpleRules)
                {
                    logger.Info($"GetEXORuleCollection IsNullTermClassification.Apply Rule Name:{exoRule.RuleName}.Apply Rule ID:{exoRule.RuleId}.");
                    var ruleObj = allRecordsEXORules.AsQueryable().Where(r => r.Id.Equals(exoRule.RuleId.ToString())).FirstOrDefault();
                    var rule = ruleAssembler.CloneSameRuleObject(ruleObj);
                    rule = RuleManagerService.ConvertToEXORule(rule);
                    if (rule != null)
                    {
                        #region init global settings
                        //if (rmSettings != null)
                        {
                            //DataSecurity encryptionMethod = rmSettings.UseEncryption ? rmSettings.EncryptionMethod : DataSecurity.None;
                            //DataSecurity compressionMethod = rmSettings.UseCompression ? rmSettings.CompressionMethod : DataSecurity.None;
                            //rule.ArchiverDataSecurity = compressionMethod | encryptionMethod;
                            //rule.ArchiverCompressionType = (CompressionType)rmSettings.CompressionSpeed;
                            //rule.DataEncryptionProfileId = rmSettings.SecurityProfileId.ToString();
                            //rule.DataEncryptionProfileName = rmSettings.SecurityProfileName;
                            //rule.StoragePolicyId = rmSettings.StoragePolicyId.ToString();
                            //rule.StoragePolicyName = rmSettings.StoragePolicyName;
                            if (rule.ExportInfo != null && (rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportBeforeArchive || rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive) && !(rule.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
                            {
                                //if (rmSettings.ExportLocationId == Guid.Empty)
                                //{
                                //    invalidRule.Add(rule.Name);
                                //    continue;
                                //}

                                //if (exportLocationTypes.ContainsKey(rmSettings.ExportLocationId) && exportLocationTypes[rmSettings.ExportLocationId] == 1)
                                //{
                                //    invalidRuleWithFtpLocation.Add(rule.Name);
                                //    continue;
                                //}
                                //rule.ExportInfo.exportLocationId = rmSettings.ExportLocationId.ToString();
                                //rule.ExportInfo.exportLocationName = rmSettings.ExportLocationName;
                                GetExportConfiguration(rule, (int)SourceFlag.Exchange);
                            }
                        }
                        #endregion
                        #region init RuleDic
                        switch (rule.PolicyLevel)
                        {
                            case PolicyLevel.ExchangeOnlineItem:
                                ruleResults.Add(exoRule.RuleOrder, rule);
                                break;

                        }
                        #endregion
                    }
                }
            }
            else
            {
                List<RMTermRuleAssociation> termRules = TermRuleInfos.GetTermWithRuleLevel((int)NodeLevel.ExchangeOnlineItem, allRecordsEXORules);
                var exoTemp = termRules.GroupBy(term => term.Id).ToList();
                List<RMTermRuleAssociation> exoTermRules = new List<RMTermRuleAssociation>();
                foreach (var exoTermRuleObjList in exoTemp)
                {
                    int ruleorder = 0;
                    foreach (var exoTermRule in exoTermRuleObjList.ToList())
                    {
                        //Rebuild the RuleOrder
                        if (allRecordsEXORules.Any(t => t.Id == exoTermRule.RuleId.ToString()))
                        {
                            ruleorder++;
                            exoTermRule.RuleOrder = ruleorder;
                            exoTermRules.Add(exoTermRule);
                        }
                    }
                }
                var termIds = exoTermRules.Select(t => t.TermId).Distinct().ToList();
                termIds = GetTermIdsWithRule(termIds, GroupId);

                #region Init column and global settings
                //RMCPGlobalStorageSetting rmSettings = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();

                //string policyId = rmSettings?.StoragePolicyId.ToString();
                //logger.Info("storage policy id:{0}", policyId);

                #endregion
                logger.Info("Init terminfo in rules");
                foreach (var termId in termIds)
                {
                    List<RMTerm> terms = new List<RMTerm>();
                    TermDao.GetAllInheritTermsByRootTerm(termId, ref terms);
                    if (terms.Count == 0)
                    {
                        continue;
                    }

                    var termIdRules = exoTermRules.AsQueryable().Where(t => t.TermId.Equals(termId)).ToList();
                    #region rebuild ruleorder
                    Dictionary<int, Rule> docRuleDic = new Dictionary<int, Rule>(); int docOrder = 0;
                    #endregion
                    foreach (var termRule in termIdRules)
                    {
                        var ruleObj = allRecordsEXORules.AsQueryable().Where(r => r.Id.Equals(termRule.RuleId.ToString())).FirstOrDefault();
                        var rule = ruleAssembler.CloneSameRuleObject(ruleObj);
                        rule = RuleManagerService.ConvertToEXORule(rule);
                        if (rule != null)
                        {
                            #region init global settings
                           // if (rmSettings != null)
                            {
                                //DataSecurity encryptionMethod = rmSettings.UseEncryption ? rmSettings.EncryptionMethod : DataSecurity.None;
                                //DataSecurity compressionMethod = rmSettings.UseCompression ? rmSettings.CompressionMethod : DataSecurity.None;
                                //rule.ArchiverDataSecurity = compressionMethod | encryptionMethod;
                                //rule.ArchiverCompressionType = (CompressionType)rmSettings.CompressionSpeed;
                                //rule.DataEncryptionProfileId = rmSettings.SecurityProfileId.ToString();
                                //rule.DataEncryptionProfileName = rmSettings.SecurityProfileName;
                                //rule.StoragePolicyId = rmSettings.StoragePolicyId.ToString();
                                //rule.StoragePolicyName = rmSettings.StoragePolicyName;
                                if (rule.ExportInfo != null && (rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportBeforeArchive || rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive) && !(rule.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
                                {
                                    //if (rmSettings.ExportLocationId == Guid.Empty)
                                    //{
                                    //    invalidRule.Add(rule.Name);
                                    //    continue;
                                    //}

                                    //if (exportLocationTypes.ContainsKey(rmSettings.ExportLocationId) && exportLocationTypes[rmSettings.ExportLocationId] == 1)
                                    //{
                                    //    invalidRuleWithFtpLocation.Add(rule.Name);
                                    //    continue;
                                    //}
                                    //rule.ExportInfo.exportLocationId = rmSettings.ExportLocationId.ToString();
                                    //rule.ExportInfo.exportLocationName = rmSettings.ExportLocationName;
                                    GetExportConfiguration(rule, (int)SourceFlag.Exchange);
                                }
                            }


                            #endregion
                            #region init RuleDic
                            switch (rule.PolicyLevel)
                            {
                                case PolicyLevel.ExchangeOnlineItem:
                                    docOrder++;
                                    docRuleDic.Add(docOrder, rule);
                                    break;

                            }
                            #endregion
                        }
                    }
                    //Rule 分组
                    #region
                    foreach (var term in terms)
                    {
                        if (docRuleDic.Count > 0)
                        {
                            ruleAssembler.AddTermWithRule(term, docRuleDic, (int)PolicyLevel.ExchangeOnlineItem);
                        }

                    }
                    #endregion
                }
                ruleResults = ruleAssembler.GetRuleDicResult();
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
            return ruleResults;
        }

        public List<Guid> GetDisposalJobUsedRules(int dataSource, Guid groupId, bool isNullClassfication = false)
        {
            List<Guid> ruleIds = new List<Guid>();
            switch (dataSource)
            {
                case (int)SourceFlag.Exchange:
                    if (isNullClassfication)
                    {
                        List<RMSimpleRule> rMSimpleRules = EXOSettingRuleDao.GetMappingRules(groupId).OrderBy(x => x.RuleOrder).ToList();
                        ruleIds = rMSimpleRules.Select(x => x.RuleId).ToList();
                    }
                    else
                    {
                        ruleIds = GetRuleIdsByDataSource(SourceFlag.Exchange);
                    }
                    break;
                case (int)SourceFlag.Physical:
                    ruleIds = GetRuleIdsByDataSource(SourceFlag.Physical);
                    break;
                case (int)SourceFlag.SharePoint:
                    ruleIds = GetRuleIdsByDataSource(SourceFlag.SharePoint);
                    break;
                case (int)SourceFlag.OneDrive:
                    ruleIds = GetRuleIdsByDataSource(SourceFlag.OneDrive);
                    break;
                default:
                    break;
            }
            return ruleIds;
        }

        private List<Guid> GetRuleIdsByDataSource(SourceFlag sourceFlag)
        {
            List<Guid> ruleIds = new List<Guid>();
            Dictionary<Guid, Rule> ruleIdDic = new Dictionary<Guid, Rule>();
            switch (sourceFlag)
            {
                case SourceFlag.Physical:
                    ruleIdDic = RuleManagerService.GetRulesFromRecords().Where(r => r.PhysicalRule != null && r.PhysicalRule.SOFilters != null && r.PhysicalRule.SOFilters.Count != 0).ToDictionary(rule => new Guid(rule.Id));
                    break;
                case SourceFlag.SharePoint:
                    ruleIdDic = RuleManagerService.GetRulesFromRecords().Where(r => r.SOFilters != null && r.SOFilters.Count != 0).ToDictionary(rule => new Guid(rule.Id));
                    break;
                case SourceFlag.OneDrive:
                    ruleIdDic = RuleManagerService.GetRulesFromRecords().Where(r => r.OneDriveRule != null && r.OneDriveRule.SOFilters != null && r.OneDriveRule.SOFilters.Count != 0).ToDictionary(rule => new Guid(rule.Id));
                    break;
                case SourceFlag.Exchange:
                    ruleIdDic = RuleManagerService.GetRulesFromRecords().Where(r => r.EXORule != null && r.EXORule.SOFilters != null && r.EXORule.SOFilters.Count != 0).ToDictionary(rule => new Guid(rule.Id));
                    break;
            }
               
            List<RMTermRuleAssociation> trAssociations = TermRuleInfos.GetTermWithRule();
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
                Rule rule;
                var tempRuleIds = termRules[term.Id];
                for (int idx = 0; idx < tempRuleIds.Count; idx++)
                {
                    if (ruleIdDic.TryGetValue(tempRuleIds[idx], out rule))
                    {
                        var ruleId = new Guid(rule.Id);
                        if (rule.PolicyLevel != PolicyLevel.None && !ruleIds.Contains(ruleId))
                        {
                            ruleIds.Add(ruleId);
                        }
                    }
                }                             
            }
            return ruleIds;
        }


        private List<int> GetTermIdsWithRule(List<int> termIds, Guid agentGroupId)
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

        }
        private void GetExportConfiguration(Rule rule, int sourceFlag)
        {
            //#if DEBUG
            //            RADataBroker.DAOAPIClientV1 Client1 = new RADataBroker.DAOAPIClientV1();
            //            var connString = Client1.GetExportLocationbyId(rule.ExportInfo.exportLocationId);
            //            rule.PhysicalDeviceDto = new AvePoint.GCommon.Contract.Storage.Entity.PhysicalDeviceDto()
            //            {
            //                ConnectionString = connString,
            //                DeviceType = (int)AvePoint.GCommon.Contract.Storage.Entity.StorageDeviceType.CloudAzure
            //            };
            //#endif
            var hasUpgradeVEOV3 = VEOV3CommonMethod.HasUpgradedVEOV3();
            if (rule.ExportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.VEO)
            {
                try
                {
                    var condition = hasUpgradeVEOV3 ?
                        (Func<RMCPExportSetting, bool>)(s => s.VEOContent != null && s.VEOHistory != null && s.SourceFlag == sourceFlag)
                        : (s => s.VEOContent == null && s.VEOHistory == null && s.SourceFlag == sourceFlag);
                    RMCPExportSetting exportSetting = ExportSettingsDao.GetExportSettings((int)ExportSettingType.VEO).FirstOrDefault(condition);
                    if (exportSetting != null)
                    {
                        if (hasUpgradeVEOV3)
                        {
                            rule.VEOContent = exportSetting.VEOContent;
                            rule.VEOHistory = exportSetting.VEOHistory;
                        }
                        else
                        {
                            rule.FileVEO = exportSetting.FileVEO;
                            rule.RecordVEO = exportSetting.RecordVEO;
                            rule.ManifestVEO = exportSetting.ManifestVEO;
                        }

                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(exportSetting.ArchiverSetting);
                        rule.ArchiverSetting = new ArchiverSetting();
                        //rule.ArchiverSetting.NumberOfThreadSendingEmail = int.Parse(doc.SelectSingleNode("Configuration/numberOfThreadsSendingEmail").InnerXml);
                        if (hasUpgradeVEOV3)
                        {
                            rule.ArchiverSetting.FileNumber = int.Parse(doc.SelectSingleNode("Configuration/archiverVEOV3Setting/fileNumber").InnerXml);
                            rule.ArchiverSetting.FileSize = double.Parse(doc.SelectSingleNode("Configuration/archiverVEOV3Setting/fileSize").InnerXml);
                        }
                        else
                        {
                            rule.ArchiverSetting.EnableArchiverVEOMerge = bool.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge").Attributes["enable"].Value);
                            rule.ArchiverSetting.IsDeleteOldFile = bool.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/isDeleteOldFile").InnerXml);
                            rule.ArchiverSetting.FileNumber = int.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/fileNumber").InnerXml);
                            rule.ArchiverSetting.FileSize = double.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/fileSize").InnerXml);
                            rule.ArchiverSetting.FolderName = doc.SelectSingleNode("Configuration/archiverVEOMerge/folderName").InnerXml;

                            doc.LoadXml(exportSetting.ArchiverVEOSetting);
                            rule.ArchiverVEOSetting = new ArchiverVEOSetting();
                            rule.ArchiverVEOSetting.AgencyId = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/AgencyID").InnerXml;
                            rule.ArchiverVEOSetting.SeriesNumber = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/Series_Number").InnerXml;
                            rule.ArchiverVEOSetting.SeriesIdentifier = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/SeriesIdentifier").InnerXml;
                            rule.ArchiverVEOSetting.ConsignmentNumber = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/ConsignmentNumber").InnerXml;
                        }
                    }
                    else
                    {
                        //rule.FileVEO = null;
                        //rule.RecordVEO = null;
                        //rule.ManifestVEO = null;
                        //rule.ArchiverSetting = null;
                        //rule.ArchiverVEOSetting = null;
                        //RECO 自己提供配置文件
                        var filepath = Path.Combine(WebUtil.GetInstallPath(), "Config", hasUpgradeVEOV3 ? VEOV3CommonString.VEOV3TemplateZipFile : "VEO Configuration Files.zip");
                        var unZipFolder = Path.Combine(WebUtil.GetInstallPath(), "Temp", "Config", hasUpgradeVEOV3 ? Path.GetFileNameWithoutExtension(VEOV3CommonString.VEOV3TemplateZipFile) : "VEO Configuration Files");
                        GCommon.ZipUtil.UnZipFile(filepath, unZipFolder);
                        if (sourceFlag == (int)SourceFlag.SharePoint)
                        {
                            if (hasUpgradeVEOV3)
                            {
                                rule.VEOContent = GetMemoryStream(unZipFolder, VEOV3CommonString.VEOContent);
                                rule.VEOHistory = GetMemoryStream(unZipFolder, VEOV3CommonString.VEOHistory);
                            }
                            else
                            {
                                rule.FileVEO = GetMemoryStream(unZipFolder, "FileVEO.xml");
                                rule.RecordVEO = GetMemoryStream(unZipFolder, "RecordVEO.xml");
                                rule.ManifestVEO = GetMemoryStream(unZipFolder, "ManifestVEO.xml");
                            }
                        }
                        else
                        {
                            if (hasUpgradeVEOV3)
                            {
                                rule.VEOContent = GetMemoryStream(unZipFolder, VEOV3CommonString.EXOVEOContent);
                                rule.VEOHistory = GetMemoryStream(unZipFolder, VEOV3CommonString.EXOVEOHistory);
                            }
                            else
                            {
                                rule.FileVEO = GetMemoryStream(unZipFolder, "EXOFileVEO.xml");
                                rule.RecordVEO = GetMemoryStream(unZipFolder, "EXORecordVEO.xml");
                                rule.ManifestVEO = GetMemoryStream(unZipFolder, "EXOManifestVEO.xml");
                            }
                        }

                        using (FileStream fs = new FileStream(Path.Combine(unZipFolder, "ArchiverSettings.config"), FileMode.Open, FileAccess.Read))
                        {
                            using (StreamReader sr = new StreamReader(fs))
                            {
                                XmlDocument doc = new XmlDocument();
                                doc.LoadXml(sr.ReadToEnd());
                                rule.ArchiverSetting = new ArchiverSetting();
                                if (hasUpgradeVEOV3)
                                {
                                    rule.ArchiverSetting.FileNumber = int.Parse(doc.SelectSingleNode("Configuration/archiverVEOV3Setting/fileNumber").InnerXml);
                                    rule.ArchiverSetting.FileSize = double.Parse(doc.SelectSingleNode("Configuration/archiverVEOV3Setting/fileSize").InnerXml);
                                }
                                else
                                {
                                    //rule.ArchiverSetting.NumberOfThreadSendingEmail = int.Parse(doc.SelectSingleNode("Configuration/numberOfThreadsSendingEmail").InnerXml);
                                    rule.ArchiverSetting.EnableArchiverVEOMerge = bool.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge").Attributes["enable"].Value);
                                    rule.ArchiverSetting.IsDeleteOldFile = bool.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/isDeleteOldFile").InnerXml);
                                    rule.ArchiverSetting.FileNumber = int.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/fileNumber").InnerXml);
                                    rule.ArchiverSetting.FileSize = double.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/fileSize").InnerXml);
                                    rule.ArchiverSetting.FolderName = doc.SelectSingleNode("Configuration/archiverVEOMerge/folderName").InnerXml;
                                }
                            }
                        }

                        if (!hasUpgradeVEOV3)
                        {
                            using (FileStream fs = new FileStream(Path.Combine(unZipFolder, "ArchiverVEOSettings.config"), FileMode.Open, FileAccess.Read))
                            {
                                using (StreamReader sr = new StreamReader(fs))
                                {
                                    XmlDocument doc = new XmlDocument();
                                    doc.LoadXml(sr.ReadToEnd());
                                    rule.ArchiverVEOSetting = new ArchiverVEOSetting();
                                    rule.ArchiverVEOSetting.AgencyId = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/AgencyID").InnerXml;
                                    rule.ArchiverVEOSetting.SeriesNumber = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/Series_Number").InnerXml;
                                    rule.ArchiverVEOSetting.SeriesIdentifier = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/SeriesIdentifier").InnerXml;
                                    rule.ArchiverVEOSetting.ConsignmentNumber = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/ConsignmentNumber").InnerXml;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("set VEO export setting when run job error {0}", e.ToString());
                }
            }
            if (rule.ExportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NAA)
            {
                try
                {
                    var nnaExportSetting = ExportSettingsDao.GetExportSetting((int)ExportSettingType.NAA, sourceFlag);
                    if (nnaExportSetting != null)
                    {
                        rule.NAAConfigFile = nnaExportSetting.ExportConfig;
                    }
                    else
                    {
                        var filepath = Path.Combine(WebUtil.GetInstallPath(), "Config", "NAA Configuration File.zip");
                        var unZipFolder = Path.Combine(WebUtil.GetInstallPath(), "Temp", "Config", "NAA Configuration File");
                        GCommon.ZipUtil.UnZipFile(filepath, unZipFolder);
                        if (sourceFlag == (int)SourceFlag.SharePoint)
                        {
                            rule.NAAConfigFile = GetMemoryStream(unZipFolder, "NAA Configuration File.xml");
                        }
                        else
                        {
                            rule.NAAConfigFile = GetMemoryStream(unZipFolder, "EXO NAA Configuration File.xml");
                        }

                    }
                }
                catch (Exception e)
                {
                    logger.Warn("set NNA export setting when run job error {0}", e.ToString());
                }

            }
            //NARA
            if (rule.ExportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NARA)
            {
                try
                {
                    var nnaExportSetting = ExportSettingsDao.GetExportSetting((int)ExportSettingType.NARA, sourceFlag);
                    if (nnaExportSetting != null)
                    {
                        rule.NARAConfigFile = nnaExportSetting.ExportConfig;
                    }
                    else
                    {
                        var filepath = Path.Combine(WebUtil.GetInstallPath(), "Config", "NARA Configuration File.zip");
                        var unZipFolder = Path.Combine(WebUtil.GetInstallPath(), "Temp", "Config", "NARA Configuration File");
                        GCommon.ZipUtil.UnZipFile(filepath, unZipFolder);
                        if (sourceFlag == (int)SourceFlag.SharePoint)
                        {
                            rule.NARAConfigFile = GetMemoryStream(unZipFolder, "NARA Configuration File.xml");
                        }
                        else
                        {
                            rule.NARAConfigFile = GetMemoryStream(unZipFolder, "EXO NARA Configuration File.xml");
                        }

                    }
                }
                catch (Exception e)
                {
                    logger.Warn("set NARA export setting when run job error {0}", e.ToString());
                }

            }

            var exportEncryptionEnabled = RMKeyValueDao.IsExportDataEncryptionEnabled();
            if (exportEncryptionEnabled)
            {
                var keyIV = ExportDataEncryptionSettingService.GetCurrentAesKey().Extension;
                if (hasUpgradeVEOV3 && rule.ExportType == GCommon.Contract.StorageOptimization.Object.ExportTypeValue.VEO)
                {
                    rule.ExportDataEncryptionKey = keyIV;
                }
                else if (!string.IsNullOrWhiteSpace(keyIV) && keyIV.IndexOf("|") > 0)
                {
                    rule.ExportDataEncryptionKey = CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(Encoding.UTF8.GetBytes(keyIV.Split('|')[0]));
                    rule.ExportDataEncryptionIV = CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(Encoding.UTF8.GetBytes(keyIV.Split('|')[1]));
                }
                else
                {
                    throw new Exception("Export data encryption is enabled, but we cannot valid encryption key.");
                }
            }
        }
        private byte[] GetMemoryStream(string unZipFolder, string fileName)
        {
            using (FileStream fs = new FileStream(Path.Combine(unZipFolder, fileName), FileMode.Open,FileAccess.Read))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    fs.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        public List<RuleNodeContract> BuildBreakTreeNode(RMEXOTreeNode tree)
        {
            List<RuleNodeContract> breakInherting = new List<RuleNodeContract>();
            if (tree.Level != (int)NodeLevel.ExchangeOnlineMailbox)
            {
                try
                {
                    var parentId = ScheduleService.GetProfileId(tree) + "|";
                    var treeNodes = RMScheduleDao.GetDisposalBreakNodes(parentId);
                    foreach (var item in treeNodes)
                    {
                        var node = JsonConvert.DeserializeObject<RMEXOTreeNode>(item);
                        if (node.Level == (int)NodeLevel.ExchangeOnlineO365Group)
                        {
                            continue;
                        }
                        ExchangeOnlineTreeNodeDto exoTree = RMDtoConverter.ConvertRMExchangeTree2TreeNodeDto(node);
                        var breakNode = ConvertTreeNodeToRuleNodeConfig(exoTree, RuleNodeType.Archiver);
                        breakInherting.Add(breakNode);
                    }

                    var spsettings = EXOSettingDao.GetDescendantsDisableNodes(tree);
                    foreach (var item in spsettings)
                    {
                        var node = SerializerHelper.DeserializeByDataContractSerializer<RMEXOTreeNode>(item.NodeInfo);
                        if (node.Level == (int)NodeLevel.ExchangeOnlineO365Group)
                        {
                            continue;
                        }
                        ExchangeOnlineTreeNodeDto exoTree = RMDtoConverter.ConvertRMExchangeTree2TreeNodeDto(node);
                        var breakNode = ConvertTreeNodeToRuleNodeConfig(exoTree, RuleNodeType.Archiver);
                        breakInherting.Add(breakNode);
                    }

                    if (tree.IsNullClassificationSetting)
                    {
                        var nonNullClassificationSetting = EXOSettingDao.GetDescendantsBreakNodesForNullClassification(tree);
                        var groupMailboxs = MailBoxService.GetEmailsByEmailGroupIdForBrowse(tree.Id);
                        foreach (var item in nonNullClassificationSetting)
                        {
                            var node = SerializerHelper.DeserializeByDataContractSerializer<RMEXOTreeNode>(item.NodeInfo);
                            if (node.Level == (int)NodeLevel.ExchangeOnlineO365Group)
                            {
                                continue;
                            }
                            if (groupMailboxs != null && groupMailboxs.Where(mailbox => mailbox.Email == item.Name).FirstOrDefault() != null && groupMailboxs.Where(mailbox => mailbox.Email == item.Name).FirstOrDefault()?.Id != item.ScopeId.ToString())
                            {
                                logger.Warn("Current Mailbox:{0} has unique setting but ScopeId:{1} does not save as MailboxId:{2}.So skip it when check IsNullClassificationSetting.", item.Name, item.ScopeId, groupMailboxs.Where(mailbox => mailbox.Email == item.Name).FirstOrDefault()?.Id);
                                continue;
                            }
                            ExchangeOnlineTreeNodeDto exoTree = RMDtoConverter.ConvertRMExchangeTree2TreeNodeDto(node);
                            var breakNode = ConvertTreeNodeToRuleNodeConfig(exoTree, RuleNodeType.Archiver);
                            breakInherting.Add(breakNode);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("error occurred while build break tree node,ERROR:{0}", ex.ToString());
                }
            }
            return breakInherting;
        }

        private RuleNodeContract ConvertTreeNodeToRuleNodeConfig(ExchangeOnlineTreeNodeDto node, RuleNodeType type)
        {
            if (node == null)
            {
                return null;
            }
            RuleNodeContract result = new RuleNodeContract();
            result.Id = Guid.NewGuid().ToString();
            result.NodeId = node.ID;
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
                    result.ParentNodeId = node.Parent.Parent == null ? null : node.Parent.Parent.ID;
                    result.ParentNodeName = node.Parent.Parent == null ? null : node.Parent.Parent.Name;
                }
                else
                {
                    result.ParentNodeId = node.Parent.ID;
                    result.ParentNodeName = node.Parent.Name;
                }
            }
            result.NodeLevel = node.Level;
            result.Type = type;
            AssignSPObjectId(node, ref result);
            //在处理index的时候需要转换children
            if (node.Children != null && node.Children.Count > 0 && type == RuleNodeType.IndexDevice)
            {
                result.Children = new List<RuleNodeContract>();
                foreach (ExchangeOnlineTreeNodeDto child in node.Children)
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

        private static void AssignSPObjectId(ExchangeOnlineTreeNodeDto node, ref RuleNodeContract config)
        {
            if (node.Level == NodeLevel.ExchangeOnlineMailbox)
            {
                config.SiteId = node.ID;
                config.SiteUrl = node.FullPath;
                if (node.Parent != null)
                {
                    AssignSPObjectId(node.Parent, ref config);
                }
            }
            if (node.Level == NodeLevel.ExchangeOnlineMailboxGroup || node.Level == NodeLevel.ExchangeOnlineO365Group)
            {
                config.WebAppId = node.ID;
                config.WebAppUrl = node.FullPath;
            }
        }

        public string RealRunCloudArchiverMigrationJob(string jobSettings, string jobId)
        {
            logger.Info("Start run export under review data job.");
            try
            {
                JobMonitorDao.CreateJob(jobId, JobType.CloudArchiverMigration, TenantLocalValue.LogonUserEmail);
                JobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    JobType = JobType.CloudArchiverMigration,
                    CommandLine = $"{JobType.CloudArchiverMigration} {jobId}",
                    Extension = jobSettings
                });
                logger.Info($"Cloud Archiver Migration job created: {jobId}");
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while real run export archiver site info job. Error: {e}");
                if (!string.IsNullOrEmpty(jobId))
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                }
            }

            return jobId;
        }
    }
}
