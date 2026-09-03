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
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Hybrid.AgentContract.Rule;
using AvePoint.Hybrid.Utility.Configuration;
using AvePoint.Hybrid.Utility.Cryptography;
using AvePoint.RA.Common.Global.Util;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Global.JobMessage;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Common.Threads;
using AvePoint.RA.SharePoint.Discover;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.RMSharePointColumn;
using AvePoint.RA.SharePointOnPrem.Common;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using log4net;
using RAFileSystem.SharePoint.Common;
using RAFileSystem.SharePoint.EnforceRuleAction.LeaveStub;
using RAFileSystem.SharePoint.Util;
using RAFileSystem.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xml;

namespace AvePoint.RA.SharePoint.EnforceRuleAction
{
    public abstract class BaseSPEnforceRuleActionProcessor : BaseActionProcessor
    {
        protected static readonly IAveLogger logger = AveLogger.GetInstance(typeof(BaseActionProcessor));
        protected SharePointSettingUtility SPUtility = new SharePointSettingUtility();
        public Dictionary<Guid, RMRuleItemCollection> TermAndRulesMapping { get; private set; }
        public Dictionary<Guid, Rule> AllRecordsRule;
        public List<FSTermDto> AllTerms;
        public List<AgentTermSetDto> AllTermSets;
        public List<AgentTermSetMembershipDto> AllTermSetMemberships;
        public Dictionary<Guid, int> TermWssidMappingsOfSite;
        public Dictionary<Guid, IAveTimeZone> TimeZones = new Dictionary<Guid, IAveTimeZone>();
        protected string BCSColumnName;
        protected AveObjectModelFactory ObjectModelFactory;
        protected AveObjectModelFactory ObjectModelFactoryForRelated;
        protected AveBPOSAccountInfo bposInfo;
        protected AveBPOSAccountInfo bposInfoForRelated;
        protected SPTreeNodeDto CurrentSiteColTreeNode;
        // Folder Scope run job need get Web/List Node.
        protected SPTreeNodeDto CurrentWebTreeNode;
        protected SPTreeNodeDto CurrentListTreeNode;
        protected SPTreeNodeDto CurrentNode;
        protected ActionUtility actionUtility;
        protected List<string> DesignLists = new List<string>();
        protected string ScopeId = string.Empty;
        protected string ScopePath = string.Empty;
        protected MemoryListCacheService<OnPremiseSPAzureTableEntityDto> WaitingApprovalItemCache;
        protected MemoryListCacheService<OnPremiseSPAzureTableEntityDto> RejectItemCache;
        protected MemoryListCacheService<OnPremiseSPAzureTableEntityDto> CosmosDBItemCache;
        protected Dictionary<string, AveBPOSAccountInfo> _bposCache = new Dictionary<string, AveBPOSAccountInfo>();
        protected List<string> RunningJobNodeUrls = new List<string>();
        protected List<string> BreakTreeNodeUrls = new List<string>();
        protected string PartSiteUrl = string.Empty;
        private static object mLock = new object();
        protected AveBPOSAccountInfo BposInfo = null;
        public GeneralSettingModel TimeSettingModel { get; set; }
        public string TimeFormat { get; set; }
        private const string DEFAULT_TIME_FORMAT = "MM/dd/yyyy HH:mm";
        private Char delimiter = (Char)0x12;
        private List<string> SkipItems = new List<string>() { ".aspx", ".css", ".js" };
        private static readonly string RelatedColumnInternalName = "RecordsRelated";
        private const string RELATEDFILENOTEXIST = "related file not exist";
        private const string ITEMNOTEXIST = "Item does not exist";
        #region stub field
        private byte[] StubFileContent;
        protected OnPremSPLeaveStubWrapperBackupCache onPremSPLeaveStubWrapperBackupCache = null;
        protected OnPremSPLeaveStubWrapperRestoreCache onPremSPLeaveStubWrapperRestoreCache = null;
        protected OnPremSPLeaveStubWrapperIAveObjectCache onPremSPLeaveStubWrapperIAveObjectCache = null;
        protected OnPremSPLeaveStubWrapperAveObjectInfo onPremSPLeaveStubWrapperAveObjectInfo = null;
        #endregion

        public BaseSPEnforceRuleActionProcessor() : base()
        {
            WrapperConfiguration.BPOS_S.IncludeVersionForPerformance = false;
        }
        public BaseSPEnforceRuleActionProcessor(SPTreeNodeDto CurrentTreeNode, EnforceRuleActionJobMessage mMessage) : base()
        {
            AllTerms = mMessage.AllTerms;
            AllTermSets = mMessage.AllTermSets;
            AllTermSetMemberships = mMessage.AllTermSetMemberships;
            WaitingApprovalItemCache = new MemoryListCacheService<OnPremiseSPAzureTableEntityDto>();
            RejectItemCache = new MemoryListCacheService<OnPremiseSPAzureTableEntityDto>();
            CosmosDBItemCache = new MemoryListCacheService<OnPremiseSPAzureTableEntityDto>();
            WrapperConfiguration.BPOS_S.IncludeVersionForPerformance = false;
            TermAndRulesMapping = DtoConverter.ConvertGlobalRuleTermMappingToAgentRuleTermMapping(mMessage.TermAndRulesMapping);
            CurrentNode = CurrentTreeNode;
            ScopeId = CurrentNode.ID;
            ScopePath = ReplaceCharacter(CurrentNode.FullPath);
            CurrentSiteColTreeNode = GetSiteCollectionNode(CurrentTreeNode);
            CurrentListTreeNode = GetListNode(CurrentTreeNode);
            CurrentWebTreeNode = GetWebNode(CurrentTreeNode);
            PartSiteUrl = new Uri(CurrentSiteColTreeNode.FullPath).Scheme + @"://" + new Uri(CurrentSiteColTreeNode.FullPath).Authority;
            BCSColumnName = GetBCSColumnName(mMessage, CurrentSiteColTreeNode);
            BposInfo = GetBposInfoBySite(CurrentSiteColTreeNode.FullPath);
            ObjectModelFactory = AveObjectModelFactory.CreateObjectModelFactory(CurrentSiteColTreeNode.FullPath, BposInfo, AveContextKind.ClientObjectModel);
            actionUtility = ActionUtility.GetInstance(ObjectModelFactory);
            onPremSPLeaveStubWrapperBackupCache = OnPremSPLeaveStubWrapperBackupCache.GetInstance(ObjectModelFactory, BposInfo);
            onPremSPLeaveStubWrapperRestoreCache = OnPremSPLeaveStubWrapperRestoreCache.GetInstance(ObjectModelFactory, BposInfo);
            onPremSPLeaveStubWrapperIAveObjectCache = OnPremSPLeaveStubWrapperIAveObjectCache.GetInstance(ObjectModelFactory, BposInfo);
            onPremSPLeaveStubWrapperAveObjectInfo = new OnPremSPLeaveStubWrapperAveObjectInfo();
            DesignLists = WebUtil.GetDesignLists();
            RunningJobNodeUrls = mMessage.RunningJobNodeUrls;
            BreakTreeNodeUrls = mMessage.BreakTreeNodeUrls;
            TimeSettingModel = SerializerHelper.DeserializeByDataContractSerializer<GeneralSettingModel>(mMessage.GeneralSettingModel);
            TimeFormat = mMessage.TimeFormat;
            using (var aveSite = ObjectModelFactory.CreateSite(CurrentSiteColTreeNode.FullPath))
            {
                InitRecordsFeature(aveSite);
            }
        }


        //此方法用来获取scope full path
        /// <summary>
        /// no use for now
        /// </summary>
        /// <param name="scopeFullPath"></param>
        /// <returns></returns>
        private static string ReplaceCharacter(string scopePath)
        {
            scopePath = scopePath.Replace("/", "_").Replace(@"\", "_");
            return scopePath;
        }

        private string GetBCSColumnName(EnforceRuleActionJobMessage mMessage, SPTreeNodeDto siteNode)
        {
            string bcsColumn = string.Empty;
            //只有Group节点才能够设置BCSColumn
            var groupSetting = mMessage.AllSettings.FirstOrDefault();
            if (groupSetting.IsUsingExistColumnName)
            {
                bcsColumn = groupSetting.ExistColumnName;
            }
            else
            {
                bcsColumn = groupSetting.ColumnName;
            }
            return bcsColumn;
        }

        private AveBPOSAccountInfo GetBposInfoBySite(string siteUrl)
        {
            lock (_bposCache)
            {
                if (_bposCache.ContainsKey(siteUrl))
                {
                    return _bposCache[siteUrl];
                }
                else
                {
                    //AveBPOSAccountInfo aveBPOSAccountInfo = new AveBPOSAccountInfo()
                    //{
                    //    //Domain = gcBposInfo.UserAccountInfo.Domain,
                    //    UserName = @"jt0\administrator",
                    //    Password = "2wsx3edcR"
                    //};
                    var account = AgentAccountUtil.Get();
                    AveBPOSAccountInfo aveBPOSAccountInfo = new AveBPOSAccountInfo()
                    {
                        Domain = account.Domain,
                        UserName = account.UserName,
                        Password = account.Password
                    };
                    _bposCache.Add(siteUrl, aveBPOSAccountInfo);
                    return aveBPOSAccountInfo;
                }
            }
        }

        private void InitRecordsFeature(IAveSite site)
        {
            try
            {
                var mRecordFeatureId = new Guid("da2e115b-07e4-49d9-bb2c-35e93bb9fca9");
                if (site.Features[mRecordFeatureId] == null)
                {
                    site.Features.Add(mRecordFeatureId, true);
                }
            }
            catch (InvalidOperationException ex)
            {
                logger.Warn("In Place Records Management Feature is not installed in this farm, cannot declare document as a record:{0}", ex.ToString());
                throw;
            }
            catch (Exception ex)
            {
                logger.Warn("Activate In Place Records Management feature error:{0}", ex.ToString());
            }
        }

        protected SPTreeNodeDto GetSiteCollectionNode(SPTreeNodeDto curnode)
        {
            var node = curnode;
            while (node.Level != NodeLevel.SiteCollection)
            {
                node = node.Parent;
            }
            return node;
        }

        protected SPTreeNodeDto GetWebNode(SPTreeNodeDto currentNode)
        {
            var node = currentNode;
            if (currentNode.Level >= NodeLevel.Folder && currentNode.Level < NodeLevel.Item)
            {
                while (!(node.Level >= NodeLevel.Site && node.Level < NodeLevel.Lists))
                {
                    node = node.Parent;
                }
            }
            else
            {
                return null;
            }
            return node;
        }

        protected SPTreeNodeDto GetListNode(SPTreeNodeDto currentNode)
        {
            var node = currentNode;
            if (currentNode.Level >= NodeLevel.Folder && currentNode.Level < NodeLevel.Item)
            {
                while (node.Level != NodeLevel.List)
                {
                    node = node.Parent;
                }
            }
            else
            {
                return null;
            }
            return node;
        }

        public virtual void ProcessSiteCollection(SPTreeNodeDto SiteNode)
        {
            ProgressService.Increase();
        }
        public virtual void ProcessSite(IAveWeb site)
        {
            ProgressService.Increase();
        }
        public virtual void ProcessSite(AveDiscoverWeb site, bool skipCheckBreakInherit = false)
        {
            ProgressService.Increase();
        }
        public virtual void ProcessList(AveDiscoverList list)
        {
            ProgressService.Increase();
        }
        public virtual void ProcessList(IAveList list)
        {
            ProgressService.Increase();
        }
        public virtual void ProcessFolder(AveDiscoverFolder folder)
        {
            ProgressService.Increase();
        }
        public virtual void ProcessFolder(IAveFolder folder)
        {
            ProgressService.Increase();
        }

        public bool NeedSkip(IAveListItem item, ref string skipReason)
        {
            foreach (var skipItem in SkipItems)
            {
                if (item.Name.EndsWith(skipItem, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            };
            return false;
        }

        public virtual void ProcessItem(IAveListItem item, string BCSColumnInternalName, Dictionary<Guid, OnPremiseSPListCacheDto> azureTableRecords, Dictionary<Guid, OnPremiseSPListCacheDto> exploreDBRecords)
        {
            Guid termId;
            string termName;
            RMRuleItemCollection rules;
            Rule resultRule = null;
            Guid itemId = item.UniqueId;
            long itemSize = GetItemSize(item);
            try
            {
                logger.Info("Process item {0}.", item.ID);
                string itemFullPath = WebUtil.MakeFullUrl(item.ParentList.ParentWeb.Url, item.Url);
                //string itemParentFolderPath = item.DirPath().Substring(0, item.DirPath().LastIndexOf('.'));
                if (item.Folder != null)
                {
                    logger.Info($"Skip folder content type {item.ID}.");
                    return;
                }
                //if (IsBreakInheritNode(itemParentFolderPath))
                //{
                //    logger.Info($"Current item IsBreakInheritNode {itemFullPath}.");
                //    return;
                //}
                string skipReason = string.Empty;
                if (NeedSkip(item, ref skipReason))
                {
                    logger.Info($"Skip documents in exclude file extensions list. Item Url:[{item.ID}].");
                    //SendReport(item.Name, itemFullPath, null, JobDetailsStatus.Skipped, itemSize, false, skipReason);
                    return;
                }
                if (!GetSingleTaxonomyFieldValue(item, BCSColumnInternalName, out termId, out termName))
                {
                    logger.Warn("can't get sigle item value {0}.", item.ID);
                    return;
                }
                
                if (TermAndRulesMapping.TryGetValue(termId, out rules))
                {
                    //if (!rules.HasUnCamlQueryableCondition && rules.Rules.Count == 1 && rules.Rules[0].RuleFilters.Any(r => r.RuleType != ArchiverFilterRuleType.LastAccessedTime))
                    //{
                    //    var ruleId = rules.Rules[0].RuleId;
                    //    resultRule = rules.CommonRules.Rules.Where(t => t.Value.Id.Equals(ruleId)).FirstOrDefault().Value;
                    //}
                    //else
                    {
                        RuleManagement ruleManagement = new RuleManagement(rules.CommonRules);
                        resultRule = ruleManagement.CheckItemCriteria(item.UniqueId, item);
                    }
                    if (resultRule != null)
                    {
                        if (IsRemoveRule(resultRule))
                        {
                            if (exploreDBRecords != null && exploreDBRecords.ContainsKey(item.UniqueId) && exploreDBRecords[item.UniqueId].HoldStatus && exploreDBRecords[item.UniqueId].HoldReleaseTime > DateTime.UtcNow.Ticks)
                            {
                                logger.Info($"This file in on hold and current rule is remove rule, will be skipped.Item Url:[{item.ID}].");
                                SendReport(item.Name, itemFullPath, null, JobDetailsStatus.Skipped, itemSize, false, "RM_JM_FSFileOnHold");
                                return;
                            }
                        }
                        if (!resultRule.DeleteRecords && item.IsRecord())
                        {
                            logger.Info($"Item {item.ID} fit rule:{resultRule.Name.LogBase64()}, current item is delcare, current rule DeleteRecords is false so skip current objecct.");
                            resultRule = null;
                            return;
                        }
                        else if (item.IsHoldOnly())
                        {
                            logger.Info($"Item {item.ID} fit rule:{resultRule.Name.LogBase64()}, current item is hold status so skip current objecct.");
                            resultRule = null;
                            SendReport(item.Name, itemFullPath, null, JobDetailsStatus.Skipped, itemSize, false, "RM_JM_SharePointHold");
                            return;
                        }
                    }
                    else if (resultRule == null)
                    {
                        logger.Info($"Item {item.ID} not fit rule.");
                        return;
                    }
                    logger.Info($"Start do action RuleName: {resultRule.Name.LogBase64()} , RuleAction: {resultRule.KeepDataOption}, IsManualRule:{resultRule.IsManualApproval}, DeleteRelatedRecordsOption: {GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both}.");
                    var dto = CreateAzureEntityDto(item, resultRule);
                    if (resultRule.IsManualApproval)
                    {
                        if (azureTableRecords.ContainsKey(itemId))
                        {
                            var azureTableRecord = azureTableRecords[itemId];
                            dto.MovedToApprovalTable = azureTableRecord.MovedToApprovalTable;
                            dto.Status = azureTableRecord.Status;
                            dto.ScanTime = DateTime.UtcNow;
                            dto.DeleteRelatedRecords = azureTableRecord.DeleteRelatedRecords;
                            if (azureTableRecord.RuleId.ToString().Equals(resultRule.Id, StringComparison.OrdinalIgnoreCase))
                            {
                                if (azureTableRecord.Status == (int)SOApproveDBStatus.Approved)
                                {
                                    //ToDoAction
                                    logger.Info($"Current item {item.ID} has approved in records review.");
                                    DoDisposalAction(item, resultRule, dto, itemFullPath, itemSize);
                                }
                                else if (azureTableRecord.Status == (int)SOApproveDBStatus.KeepData
                                    || azureTableRecord.Status == (int)SOApproveDBStatus.CheckOption
                                    || azureTableRecord.Status == (int)SOApproveDBStatus.WaitingApprove)
                                {
                                    //AddSkipReport(dto);
                                    logger.Info("Skip current status:{0}. File id:{1}.", azureTableRecord.Status, item?.ID);
                                }
                                else if (azureTableRecord.Status == (int)SOApproveDBStatus.Rejected)
                                {

                                    if (exploreDBRecords != null && exploreDBRecords.ContainsKey(item.UniqueId) && exploreDBRecords[item.UniqueId].ManualExtendTime >= DateTime.UtcNow.Ticks)
                                    {
                                       logger.Debug("item is manualsync and its extend");
                                        return;
                                    }
                                    logger.Info($"Current item {item.ID} has rejected in records review.");
                                    dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                                    dto.MovedToApprovalTable = false;
                                    dto.ScanTime = DateTime.UtcNow;
                                    dto.IsRejectData = true;
                                    OperationForBusinessLayer(dto);
                                    SendReport(item.Name, itemFullPath, resultRule, JobDetailsStatus.Successful, itemSize, true, "RM_JM_FSFileWaitingForApproval");
                                }
                                else
                                {
                                    logger.Warn("Invalid current status:{0}. File id:{1}.", azureTableRecord.Status, item?.ID);
                                }
                            }
                            else
                            {
                                logger.Info($"Current item {item.ID} fit rule different with previous job.Old RuleId:{azureTableRecord.RuleId}.NewRuleId:{resultRule.Id}.");
                                //only update ruleId in azure table
                                dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                                dto.MovedToApprovalTable = false;
                                dto.ScanTime = DateTime.UtcNow;
                                OperationForBusinessLayer(dto);
                                SendReport(item.Name, itemFullPath, resultRule, JobDetailsStatus.Successful, itemSize, true, "RM_JM_FSFileWaitingForApproval");
                            }
                        }
                        else
                        {
                            logger.Info($"Current item {item.ID} does not exist in azure table and add it to azure table.");
                            dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                            dto.ScanTime = DateTime.UtcNow;
                            OperationForBusinessLayer(dto);
                            SendReport(item.Name, itemFullPath, resultRule, JobDetailsStatus.Successful, itemSize, true, "RM_JM_FSFileWaitingForApproval");
                        }
                    }
                    else
                    {
                        //ToDoAction
                        DoDisposalAction(item, resultRule, dto, itemFullPath, itemSize);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn($"Process Item failed {e.ToString()}.");
                string exMsg = GetExceptionMessage(e);
                if (!exMsg.Contains(ITEMNOTEXIST))
                {
                    AddFailedDetail(item, exMsg);
                    JobHasErrorNode = true;
                }
            }
            finally
            {
                ProgressService.Increase();
            }
        }

        private bool IsRemoveRule(Rule resultRule)
        {
            if (
                (resultRule.KeepDataOption & (int)KeepDataOption.Remove) == (int)KeepDataOption.Remove
                || (resultRule.KeepDataOption & (int)KeepDataOption.NotBackup) == (int)KeepDataOption.NotBackup
                || resultRule.KeepDataOption == 0)
            {
                return true;
            }
            return false;
        }

        private long GetItemSize(IAveListItem item)
        {
            long itemSize = 0;
            if (item.Fields.ContainsField("File_x0020_Size"))
            {
                try
                {
                    itemSize = Convert.ToInt64(item["File_x0020_Size"]);
                }
                catch (Exception ex)
                {
                    logger.Info("Can not get item size.ItemUrl:{0}.Message:{1}.", item.Url.LogBase64(), ex.ToString());
                }
            }
            else
            {
                logger.Info("Current item does not contains File_x0020_Size.Item Url:{0}.", item.ID);
            }
            return itemSize;
        }

        private void DoDisposalAction(IAveListItem item, Rule resultRule, OnPremiseSPAzureTableEntityDto dto, string itemFullPath, long itemSize)
        {
            bool undeclared = false;
            string jobDetailComment = string.Empty;
            if ((resultRule.KeepDataOption & (int)KeepDataOption.TagContent) == (int)KeepDataOption.TagContent)
            {
                //add for manual declare and rule for add tag
                if (item.IsBlockDeleteOnlyRecord())
                {
                    logger.Info("This kind of records no need undeclared {0}.", item.ID);
                }
                else if (item.IsRecord())
                {
                    actionUtility.UndeclareItem(item);
                    undeclared = true;
                }
                item = item.ParentList.ParentWeb.GetListItem(item.Url, item.ParentList.ID, item.UniqueId);
                actionUtility.CreateTagContent(item, resultRule.TagContentInfo);
                logger.Info($"Current item {item.ID} Create Tag success.");
            }
            if (undeclared || (resultRule.KeepDataOption & (int)KeepDataOption.DeclareRecord) == (int)KeepDataOption.DeclareRecord)
            {
                if (item.IsRecord())
                {
                    logger.Info($"item is already being declared {item.ID}.");
                    SendReport(item.Name, itemFullPath, resultRule, JobDetailsStatus.Skipped, itemSize, false, "RM_UI_Detail_IsDeclared");
                    return;
                }
                try
                {
                    actionUtility.DeclareItem(item, item.Url, undeclared);
                    logger.Info($"Current item {item.ID} Declare success.");
                }
                catch (InvalidOperationException ex)
                {
                    logger.Warn("In Place Records Management Feature is not installed in this farm, cannot declare document as a record:{0}.", ex.ToString());
                }
            }
            else if (
                (resultRule.KeepDataOption & (int)KeepDataOption.Remove) == (int)KeepDataOption.Remove
                || (resultRule.KeepDataOption & (int)KeepDataOption.NotBackup) == (int)KeepDataOption.NotBackup
                || resultRule.KeepDataOption == 0)
            {
                LinkDocumentForStubRule(resultRule,item);
                bool needToDeleteRelatedData = false;
                if (resultRule.IsManualApproval)
                {
                    logger.Info($"current rule is manual rule,will check data record db setting,record db status:{dto.DeleteRelatedRecords}.");
                    if (dto.DeleteRelatedRecords == (int)AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both)
                    {
                        needToDeleteRelatedData = true;
                    }
                }
                else if (resultRule.RelatedRecordOption == AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both)
                {
                    logger.Info("current rule not manual rule,will check rule related setting.");
                    needToDeleteRelatedData = true;
                }
                if (needToDeleteRelatedData)
                {
                    using (var pc1 = new AgentPerformanceScope("SPOnprem.DeleteRelatedFile", addToStatistics: true))
                    {
                        logger.Info($"Current item {item.ID} has related date and need to process it.");
                        if (item.Fields.ContainsFieldWithStaticName("RecordsRelated"))
                        {
                            var metadata = item["RecordsRelated"];
                            if (metadata != null && !string.IsNullOrEmpty(metadata.ToString()))
                            {
                                logger.Info("Begin DisposeRelatedItemsForSPOnpremDeleteOnly.Url:{0}.RelatedInfo:{1}.", item.Url.LogBase64(), metadata.ToString().LogBase64());
                                var relatedString = metadata.ToString();
                                var relatedItems = SPCommonUtility.GetRelatedProperties(relatedString);
                                foreach (var reItem in relatedItems)
                                {
                                    if (reItem.SourceFlag == (int)SourceFlag.SharePointOnPrem)
                                    {
                                        logger.Info($"will delete related data for site :{reItem.SiteUrl.LogBase64()},reItem.id:{reItem.id}");
                                        Guid recordId = AvePoint.RA.SharePoint.ExplorerSync.Utils.IDGenerator.GetRecordId(reItem.SiteId, reItem.id);
                                        if (HybridApiClient.Instance.CheckIsHoldRecord(recordId.ToString()))
                                        {
                                            try
                                            {
                                                logger.Info($"this file is on hold will not delete related data for site :{reItem.SiteUrl.LogBase64()},reItem.id:{reItem.id}");
                                                var fileInfo = DeleteRelatedItem(reItem.ItemUrl, reItem.WebId, reItem.ListId, reItem.id, false, resultRule);
                                                SendReport(reItem.name, string.IsNullOrEmpty(fileInfo.Item2) ? reItem.ItemUrl : fileInfo.Item2, resultRule, JobDetailsStatus.Skipped, fileInfo.Item1, false, "RM_FS_ReportSkip_OnHold");
                                            }
                                            catch (Exception e)
                                            {
                                                if (e.Message.Equals(RELATEDFILENOTEXIST))
                                                {
                                                    logger.Error($"file not exist anymore :{reItem.SiteUrl.LogBase64()},reItem.id:{reItem.id}");
                                                }
                                                else
                                                {
                                                    logger.Error($"some thing went error when DeleteRelatedItem,error:{e} :{reItem.SiteUrl.LogBase64()},reItem.id:{reItem.id}");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Tuple<long, string> fileInfo = new Tuple<long, string>(0, "");
                                            string errorMessage = "";
                                            bool hasError = false;
                                            bool needSkipRelatedFile = false;
                                            try
                                            {
                                                InitRelatedObj(reItem.SiteUrl);
                                                fileInfo = DeleteRelatedItem(reItem.ItemUrl, reItem.WebId, reItem.ListId, reItem.id, true, resultRule);
                                            }
                                            catch (Exception ex)
                                            {
                                                if (ex.Message.Equals(RELATEDFILENOTEXIST))
                                                {
                                                    needSkipRelatedFile = true;
                                                }
                                                else
                                                {
                                                    errorMessage = ex.Message;
                                                    hasError = true;
                                                }
                                            }
                                            if (hasError)
                                            {
                                                SendReport(reItem.name, string.IsNullOrEmpty(fileInfo.Item2) ? reItem.ItemUrl : fileInfo.Item2, resultRule, JobDetailsStatus.Failed, fileInfo.Item1, false, errorMessage);
                                                return;
                                            }
                                            else
                                            {
                                                OnPremiseSPAzureTableEntityDto relatedDto = new OnPremiseSPAzureTableEntityDto()
                                                {
                                                    Id = recordId,
                                                    SiteId = reItem.SiteId.ToString(),
                                                    ExplorerStatus = (int)RMRecordStatus.Destroyed
                                                };
                                                CosmosDBItemCache.Add(relatedDto);
                                                if (!needSkipRelatedFile)
                                                {
                                                    SendReport(reItem.name, string.IsNullOrEmpty(fileInfo.Item2) ? reItem.ItemUrl : fileInfo.Item2, resultRule, JobDetailsStatus.Successful, fileInfo.Item1, false);
                                                }
                                            }
                                        }
                                    }
                                }
                                if (relatedItems != null && relatedItems.Any(a => a.SourceFlag == (int)SourceFlag.Physical))
                                {
                                    List<OnPremRelatedResult> deleteRelatedResult = JobContext.Current.ApiClient.DeleteRelatedPhysicalRecord(new OnPremRelatedDto()
                                    {
                                        CurrentRule = new Contract.Global.Object.Rule()
                                        {
                                            Id = resultRule.Id,
                                            Name = resultRule.Name,
                                        },
                                        RecordRelatedValue = relatedString,
                                        Jobid = JobContext.Current.JobId
                                    });
                                    logger.Info($"the delete related result count is:{deleteRelatedResult?.Count}");
                                    foreach (var re in deleteRelatedResult)
                                    {
                                        SendRelatedRecordReport(re.Name, re.DirPath, resultRule, re.DetailsStatus, 0, false, re.Message);
                                    }
                                    if (deleteRelatedResult.Any(a => a.DetailsStatus == JobDetailsStatus.Failed))
                                    {
                                        logger.Warn("the related physical data delete failed,skip delete source onprem");
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                DeleteItem(item, resultRule, itemSize);
                logger.Info($"Current item {item.ID} Delete success.");
                dto.ExplorerStatus = (int)RMRecordStatus.Destroyed;
            }
            else if ((resultRule.KeepDataOption & (int)KeepDataOption.UndeclaredRecord) == (int)KeepDataOption.UndeclaredRecord)
            {
                UndeclaredItem(item, resultRule, itemSize);
                logger.Info($"Current item {item.ID} Undeclare success.");
            }
            if (dto != null)
            {
                dto.Status = (int)SOApproveDBStatus.Archived;
                OperationForBusinessLayer(dto);
            }
            SendReport(item.Name, itemFullPath, resultRule, JobDetailsStatus.Successful, itemSize, false);
            logger.Info("Process item {0} success.", item.ID);
        }
        private void LinkDocumentForStubRule(Rule resultRule,IAveListItem item)
        {
            bool isLinkToDucument = (resultRule.KeepDataOption & (int)KeepDataOption.LinkDocument) == (int)KeepDataOption.LinkDocument;
            if (isLinkToDucument && item.File != null)
            {
                onPremSPLeaveStubWrapperIAveObjectCache.InitStubIAveObjectContainer(CurrentSiteColTreeNode.FullPath, item.ParentList.ParentWeb.ID, item.ParentList.ID);
                onPremSPLeaveStubWrapperBackupCache.InitStubAveBackupContainer(CurrentSiteColTreeNode.FullPath, item.ParentList.ParentWeb.ID, item.ParentList.ParentWeb.ServerRelativeUrl, item.ParentList.ID, item.ParentList.Title, onPremSPLeaveStubWrapperAveObjectInfo);
                onPremSPLeaveStubWrapperRestoreCache.InitStubAveRestoreContainer(CurrentSiteColTreeNode.FullPath, item.ParentList.ParentWeb.ID, item.ParentList.ParentWeb.ServerRelativeUrl, item.ParentList.ID, item.ParentList.Title, onPremSPLeaveStubWrapperAveObjectInfo);
                LinkDocument(item.File, resultRule);
            }
            else
            {
                logger.Warn($"can not link document because:isLinkToDucument:{isLinkToDucument},item file exist:{item?.File != null}");
            }
        }
        private void InitRelatedObj(string siteUrl)
        {
            bposInfoForRelated = GetBposInfoBySite(siteUrl);
            ObjectModelFactoryForRelated = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, BposInfo, AveContextKind.ClientObjectModel);
        }

        /// <summary>
        /// 更新Azure Table & CosmosDB
        /// </summary>
        /// <param name="dto"></param>
        private void OperationForBusinessLayer(OnPremiseSPAzureTableEntityDto dto)
        {
            //Waiting for Review 数据只需要插入Azure Table，CosmosDB不需要更新(由Data Sync Job更新)
            if (dto.Status == (int)SOApproveDBStatus.WaitingApprove)
            {
                //Reject数据重新Waiting for Approve
                if (dto.IsRejectData)
                {
                    RejectItemCache.Add(dto);
                }
                WaitingApprovalItemCache.Add(dto);
            }
            //已经Archived的数据需要同时更新Azure Table和CosmosDB
            else if (dto.Status == (int)SOApproveDBStatus.Archived)
            {
                //只有ManualRule且数据Archived才更新AzureTable数据
                if (dto.IsManualRule)
                {
                    WaitingApprovalItemCache.Add(dto);
                }
                if (dto.ExplorerStatus == (int)RMRecordStatus.Destroyed)
                {
                    CosmosDBItemCache.Add(dto);
                }
            }
            else
            {
                logger.Warn("Wrong SOApproveDBStatus when OperationForBusinessLayer. SOApproveDBStatus:{0}.", dto.Status);
                return;
            }

            if (WaitingApprovalItemCache.Count > ExternalUtil.TransferDataCount)
            {
                var tempEntities = WaitingApprovalItemCache.Take(ExternalUtil.TransferDataCount).ToList();
                try
                {
                    using (new AgentPerformanceScope("OnPremiseSP.AddOnpremiseSPManualDataToAzureTable", $"OnPremiseSP.AddOnpremiseSPManualDataToAzureTable.Count:{tempEntities.Count}", true))
                    {
                        List<Guid> failedGuidsForAzureTable = JobContext.Current.ApiClient.AddOnpremiseSPManualDataToAzureTable(tempEntities);
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while updating azure table. Error:{0}.", e.ToString());
                }

            }

            if (CosmosDBItemCache.Count > ExternalUtil.TransferDataCount)
            {

                var tempEntities = CosmosDBItemCache.Take(ExternalUtil.TransferDataCount).ToList();
                try
                {
                    using (new AgentPerformanceScope("OnPremiseSP.OnPremiseSPUpdateRecordsInExplorer", $"OnPremiseSP.OnPremiseSPUpdateRecordsInExplorer.Count:{tempEntities.Count}", true))
                    {
                        List<Guid> failedGuidsForCosmosDB = JobContext.Current.ApiClient.OnPremiseSPUpdateRecordsInExplorer(tempEntities);
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while updating cosmosDB. Error:{0}.", e.ToString());
                }

            }

            if (RejectItemCache.Count > ExternalUtil.TransferDataCount)
            {
                var tempEntities = RejectItemCache.Take(ExternalUtil.TransferDataCount).ToList();
                try
                {
                    using (new AgentPerformanceScope("OnPremiseSP.AddRejectItemsToStaticTableForOnPremiseSP", $"OnPremiseSP.AddRejectItemsToStaticTableForOnPremiseSP.Count:{tempEntities.Count}", true))
                    {
                        JobContext.Current.ApiClient.AddRejectItemsToStaticTableForOnPremiseSP(tempEntities);
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while updating azure table. Error:{0}.", e.ToString());
                }

            }
        }

        public void FinalOperationForBusinessLayer()
        {
            var tempEntities = WaitingApprovalItemCache.TakeAll().ToList();
            if (tempEntities.Count > 0)
            {
                try
                {
                    using (new AgentPerformanceScope("OnPremiseSP.AddOnpremiseSPManualDataToAzureTable", $"OnPremiseSP.AddOnpremiseSPManualDataToAzureTable.Count:{tempEntities.Count}", true))
                    {
                        List<Guid> failedGuidsForAzureTable = JobContext.Current.ApiClient.AddOnpremiseSPManualDataToAzureTable(tempEntities);
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while updating azure table. Error:{0}.", e.ToString());
                }
            }



            var tempEntities1 = CosmosDBItemCache.TakeAll().ToList();
            if (tempEntities1.Count > 0)
            {
                try
                {
                    using (new AgentPerformanceScope("OnPremiseSP.OnPremiseSPUpdateRecordsInExplorer", $"OnPremiseSP.OnPremiseSPUpdateRecordsInExplorer.Count:{tempEntities1.Count}", true))
                    {
                        List<Guid> failedGuidsForAzureTable = JobContext.Current.ApiClient.OnPremiseSPUpdateRecordsInExplorer(tempEntities1);
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while updating cosmosDB. Error:{0}.", e.ToString());
                }
            }



            var tempEntities2 = RejectItemCache.TakeAll().ToList();
            try
            {
                using (new AgentPerformanceScope("OnPremiseSP.AddRejectItemsToStaticTableForOnPremiseSP", $"OnPremiseSP.AddRejectItemsToStaticTableForOnPremiseSP.Count:{tempEntities2.Count}", true))
                {
                    JobContext.Current.ApiClient.AddRejectItemsToStaticTableForOnPremiseSP(tempEntities2);
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while updating azure table. Error:{0}.", e.ToString());
            }

        }

        private OnPremiseSPAzureTableEntityDto CreateAzureEntityDto(IAveListItem item, Rule rule)
        {
            var itemParentFolder = item.ParentList.GetFolder(item.DirPath().Substring(0, item.DirPath().LastIndexOf('/')));
            var recId = AvePoint.RA.SharePoint.ExplorerSync.Utils.IDGenerator.GetRecordId(item.ParentList.ParentWeb.Site.ID, item.UniqueId);
            var relatedRecords = GetRelatedRecordsInfo(item);
            OnPremiseSPAzureTableEntityDto dto = new OnPremiseSPAzureTableEntityDto()
            {
                Id = recId,
                NodeID = item.UniqueId,
                ParentID = itemParentFolder.UniqueId,
                RuleID = new Guid(rule.Id),
                RuleAction = GetRuleAction(rule),
                IsManualRule = rule.IsManualApproval,
                JobID = JobContext.Current.JobId,
                ScopeID = new Guid(CurrentNode.ID),
                ScopePath = ScopePath,
                Status = (int)SOApproveDBStatus.Approved,
                ExplorerStatus = (int)RMRecordStatus.Active,
                MovedToApprovalTable = false,//set a default value and will reset later.
                UIVersion = 0, //current does not support backup and restore so it does not need set UIVersion.
                ArchiveLevel = GetArchiveLevel(rule),
                CacheNodeType = (int)CacheNodeType.Item,//Current only support Item Level rule.
                JsonMeta = string.Empty,
                SourceFlag = (int)AvePoint.GCommon.Contract.StorageOptimization.Object.RecordFlag.OnPremSP,
                SortTicks = string.Empty, // will process in API Web.
                HasRelatedDocument = string.IsNullOrEmpty(relatedRecords) ? 0 : 1,//Current does not support Related.
                DeleteRelatedRecords = (int)rule.RelatedRecordOption,//Current does not support Related.
                RelatedRecordInfo = relatedRecords,//Current does not support Related.
                LastModifiedTime = Convert.ToDateTime(item["Modified"]).Ticks,
                LeafName = item.Name,
                //Level = 0, No need this property now.
                ExpireTime = DateTime.UtcNow,
                LibRowID = item.ID,
                ListId = item.ParentList.ID,
                //NodeType = 0, No need this property now.
                Path = item.DirPath(),
                Property = string.Empty,
                //SPNodeLevel = 0,No need this property now.
                //ScanItemID = 0,No need this property now.
                ScanTime = DateTime.UtcNow,
                SiteUrl = CurrentSiteColTreeNode.FullPath,
                SiteId = item.ParentList.ParentWeb.Site.ID.ToString(),
                RegistedSiteId = CurrentSiteColTreeNode.ID,
                WebId = item.ParentList.ParentWeb.ID,
                Metadata = GetMetaData(item),
                ArchivedTime = DateTime.UtcNow,
                SiteGroupId = new Guid(CurrentSiteColTreeNode.Parent.ID),
                //KeepDataStatus = 0, No need this property now.
                SiteTitle = CurrentSiteColTreeNode.Name,
            };
            return dto;
        }

        private string GetRelatedRecordsInfo(IAveListItem item)
        {
            if (item != null && item.FieldValues != null && item.FieldValues.ContainsKey(RelatedColumnInternalName) && item[RelatedColumnInternalName] != null)
            {
                var sourceUrlValue = item[RelatedColumnInternalName].ToString();
                return sourceUrlValue;
            }
            return string.Empty;
        }

        public string GetMetaData(IAveListItem item)
        {
            Hashtable columns = GetItemColumns(item);
            if (columns != null && columns.Count > 0)
            {
                XmlDocument doc = new XmlDocument();
                XmlElement xe = doc.CreateElement("MetaData");
                foreach (var column in columns.Keys)
                {
                    XmlElement colXe = doc.CreateElement("Column");
                    colXe.SetAttribute("Name", column.ToString());
                    string value = columns[column].ToString();
                    if (value.Contains(delimiter))
                    {
                        string[] values = value.Split(delimiter);
                        colXe.SetAttribute("Value", values[0].ToString());
                        colXe.SetAttribute("ExtendValue", values[1].ToString());
                    }
                    else
                    {
                        colXe.SetAttribute("Value", columns[column].ToString());
                    }
                    xe.AppendChild(colXe);
                }
                return xe.OuterXml;
            }
            return null;
        }

        /// <summary>
        /// 1.获取Item相关Column Value，可以通过Display Name获取，也可以通过Internal Name获取
        /// 2.先通过Display Name获取，如果Display Name获取不到则通过Internal Name获取
        /// 3.通过不同Name获取，返回不同Name的Key+value
        /// 4.RA Job need get BCS Column by BCSColumnID Default.
        /// </summary>
        public Hashtable GetItemColumns(IAveListItem item)
        {
            List<string> fieldNames = new List<string>() { "Content Type", "Created", "Author", "Modified", "Editor" };
            using (var performance = new AgentPerformanceScope("ArchiverScan.GetItemColumns", addToStatistics: true))
            {
                Hashtable columnCollectionOfDisplayName = new Hashtable(StringComparer.OrdinalIgnoreCase);
                if (item != null)
                {
                    #region get RA BCSColumn
                    try
                    {
                        IAveField field = null;
                        if (!string.IsNullOrEmpty(BCSColumnName))
                        {
                            field = item.Fields.GetField(BCSColumnName);
                        }
                        //如果为空，就取BCS Column
                        if (field == null)
                        {
                            string BCSColumnID = "20f84bba906045b4af568ee102a52dcb";
                            field = item.Fields.GetFieldById(new Guid(BCSColumnID), false);
                        }
                        if (field.Type == AveFieldType.Invalid)
                        {
                            var fileObj = item[field.ID];
                            if (fileObj.GetType() != typeof(string))
                            {
                                var dic = ((Dictionary<string, object>)item[field.ID]);
                                var termName = dic["Label"].ToString();
                                var termId = new Guid(dic["TermGuid"].ToString());
                                columnCollectionOfDisplayName[BCSColumnName] = field.GetFieldValueAsText(item[field.ID]) + delimiter.ToString() + termName + "|" + termId;
                            }
                            else
                            {
                                columnCollectionOfDisplayName[BCSColumnName] = field.GetFieldValueAsText(item[field.ID]) + delimiter.ToString() + item[field.ID];
                            }
                        }
                        else
                        {
                            logger.Info("BCSColumnID exist but column type is not Invalid.Field Type:{0}.", field.Type.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Info("Can not get RA BCS Column property when get item columns.Message:{0}.", ex.Message);
                    }
                    #endregion
                    foreach (var fieldName in fieldNames)
                    {
                        bool isGetColumnByInternalName = false;
                        IAveField field = null;
                        try
                        {
                            if (fieldName.Equals("Content Type", StringComparison.OrdinalIgnoreCase) || fieldName.Equals("ContentType", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    columnCollectionOfDisplayName[fieldName.ToLower(CultureInfo.CurrentCulture)] = item.ContentType.Name;
                                }
                                catch (Exception ex)
                                {
                                    logger.Info("Can not get content type property when get item columns.Message:{0}.", ex.Message);
                                }
                                continue;
                            }
                            field = item.Fields[fieldName];
                        }
                        catch (Exception e)
                        {
                            try
                            {
                                field = item.Fields.GetFieldByInternalName(fieldName);
                                isGetColumnByInternalName = true;
                            }
                            catch (Exception ex)
                            {
                                logger.Info("Can not get field by internal name when get item columns.Message:{0}.", ex.Message);
                                columnCollectionOfDisplayName[fieldName.ToLower(CultureInfo.CurrentCulture)] = string.Empty;
                                continue;
                            }
                        }
                        try
                        {
                            string fieldTitle = field.Title.ToLower(CultureInfo.CurrentCulture);//RA Need Lower
                            string fieldInternalName = field.InternalName.ToLower(CultureInfo.InvariantCulture);
                            if (field.Hidden)
                            {
                                logger.Info("Current field is hidden, field id:{0}.", field?.ID);
                                continue;
                            }
                            if (item[field.ID] == null)
                            {
                                if (field.Type != AveFieldType.Number && field.Type != AveFieldType.DateTime && field.Type != AveFieldType.Boolean && field.Type != AveFieldType.Integer)
                                {//text match * need this.        
                                    columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = string.Empty;
                                }
                                continue;
                            }
                            switch (field.Type)
                            {
                                //在rule判断时，会判断数据类型。
                                case AveFieldType.Boolean:
                                case AveFieldType.Number:
                                    columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = item[field.ID];
                                    break;
                                case AveFieldType.Counter:
                                    columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = Convert.ToDouble(item[field.ID]);
                                    break;
                                case AveFieldType.DateTime:
                                    columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = ToUniversalTimeWithTimeZone((DateTime)item[field.ID], item.Web);
                                    break;
                                case AveFieldType.User:
                                    var value = item[field.ID];
                                    var stringVlue = value as string;
                                    if (stringVlue != null)
                                    {
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = stringVlue.Substring(stringVlue.IndexOf('#') + 1);
                                    }
                                    else if (value is IEnumerable)
                                    {
                                        StringBuilder users = new StringBuilder();
                                        foreach (var userinfo in (value as IEnumerable))
                                        {
                                            var user = userinfo.ToString();
                                            users.Append(user.Substring(user.IndexOf('#') + 1));
                                            users.Append(';');
                                        }
                                        users.Length = Math.Max(0, users.Length - 1);
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = users.ToString();
                                    }
                                    else
                                    {
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = value;
                                    }
                                    break;
                                case AveFieldType.Lookup:
                                    var lookupValue = item[field.ID];
                                    var realValue = lookupValue as IAveFieldLookupValue;
                                    if (realValue != null)
                                    {
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = realValue.LookupValue;
                                    }
                                    else if (string.Equals(field.TypeAsString, "Lookup", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(field.TypeAsString, "LookupMulti", StringComparison.OrdinalIgnoreCase))
                                    {
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = field.GetFieldValueAsText(lookupValue);
                                    }
                                    else
                                    {
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = lookupValue;
                                    }
                                    break;
                                case AveFieldType.Invalid:
                                    if (string.Equals(field.TypeAsString, "TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(field.TypeAsString, "TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                                    {
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = field.GetFieldValueAsText(item[field.ID]) + delimiter.ToString() + item[field.ID].ToString();
                                    }
                                    else
                                    {
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = item[field.ID];
                                    }
                                    break;
                                case AveFieldType.ModStat:
                                    columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = field.GetFieldValueAsText(item[field.ID]);
                                    break;
                                default:
                                    columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = field.GetFieldValueAsText(item[field.ID]);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Info(string.Format("Get the metadata of item error.Field id:{0}.Exception:{1}", field?.ID, ex.Message));
                        }
                    }
                }
                return columnCollectionOfDisplayName;
            }
        }

        private DateTime ToUniversalTimeWithTimeZone(DateTime datetime, IAveWeb web)
        {
            if (datetime.Kind != DateTimeKind.Utc)
            {
                datetime = web.RegionalSettings.TimeZone.LocalTimeToUTC(datetime);
            }
            return datetime;
        }

        private int GetRuleAction(Rule currentRule)
        {
            int action = 0;
            //if (config.VaultRulesCollection.ContainsKey(currentRule.Id))
            //{
            //    action = RuleAction.ExportOnly;
            //}
            if (currentRule.MoveToRecordCenterAndDelareSetting != null &&
                                currentRule.MoveToRecordCenterAndDelareSetting.OperateDataMode == OperatingSharePointDataMode.MoveToRecordCenterAndDelare)
            {
                action = (int)RuleAction.MoveAndDeclare;
            }
            else if (currentRule.KeepDataOption.Equals((int)KeepDataOption.Delete) ||
                currentRule.KeepDataOption.Equals((int)KeepDataOption.LinkDocument) ||
                currentRule.KeepDataOption.Equals((int)KeepDataOption.Remove) ||
                (currentRule.KeepDataOption & (int)KeepDataOption.NotBackup) == (int)KeepDataOption.NotBackup)
            {
                action = (int)RuleAction.ArchiveAndRemove;
            }
            else if ((currentRule.KeepDataOption & (int)KeepDataOption.Keep) == (int)KeepDataOption.Keep)
            {
                action = (int)RuleAction.ArchiveAndKeep;
            }
            return action;
        }

        public int GetArchiveLevel(Rule rule)
        {
            int ArchiveLevel = -1;
            switch (rule.PolicyLevel)
            {
                case GCommon.Contract.CommonFilter.PolicyLevel.SiteCollection:
                    ArchiveLevel = (int)OnPremiseSPNodeLevel.SiteCollection;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.Site:
                    ArchiveLevel = (int)OnPremiseSPNodeLevel.Web;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.Library:
                case GCommon.Contract.CommonFilter.PolicyLevel.List:
                    ArchiveLevel = (int)OnPremiseSPNodeLevel.List;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.Folder:
                    ArchiveLevel = (int)OnPremiseSPNodeLevel.Folder;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.Item:
                    //如果节点级别是ItemVersion 6 或者Attachment 6 ，并且符合了Item rule 表示符合parent rule
                    ArchiveLevel = (int)OnPremiseSPNodeLevel.Item;
                    break;
                //case GCommon.Contract.CommonFilter.PolicyLevel.Newsfeed:
                //    ArchiveLevel = (int)OnPremiseSPNodeLevel.Item;
                //    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.ItemVersion:
                    ArchiveLevel = (int)OnPremiseSPNodeLevel.ItemVersion;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.Document:
                    //如果节点级别是Document version 2 ，并且符合了Document rule ，表示符合parent rule
                    ArchiveLevel = (int)OnPremiseSPNodeLevel.Document;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion:
                    ArchiveLevel = (int)OnPremiseSPNodeLevel.DocumentVersion;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.Attachment:
                    ArchiveLevel = (int)OnPremiseSPNodeLevel.Attachment;
                    break;
                default:
                    break;
            }
            return ArchiveLevel;
        }

        public void SendReport(string name, string fullPath, Rule rule, JobDetailsStatus status, long fileSize, bool isManualScan, string comment = "")
        {
            JMOnPremiseSPEnforceRuleActionJobDetails detail = new JMOnPremiseSPEnforceRuleActionJobDetails()
            {
                ObjectName = name,
                Size = ExternalUtil.ConvertToFormatSize(fileSize),
                FinishTime = GetFinishTime(DateTime.UtcNow),
                Action = GetActionString(rule, isManualScan),
                SourceLocation = fullPath,
                RuleName = rule == null ? string.Empty : rule.Name,
                Status = status,
                Type = "RM_OnPremiseSP_EnforceRuleAction_Item",
                AgentName = AvePoint.GCommon.Utility.OSInformation.HostName,
                Comment = comment
            };
            JobDetailService.Commit(detail);
        }
        public void SendRelatedRecordReport(string name, string fullPath, Rule rule, JobDetailsStatus status, long fileSize, bool isManualScan, string comment = "")
        {
            JMOnPremiseSPEnforceRuleActionJobDetails detail = new JMOnPremiseSPEnforceRuleActionJobDetails()
            {
                ObjectName = name,
                Size = ExternalUtil.ConvertToFormatSize(fileSize),
                FinishTime = GetFinishTime(DateTime.UtcNow),
                Action = GetActionString(rule, isManualScan),
                SourceLocation = fullPath,
                RuleName = rule == null ? string.Empty : rule.Name,
                Status = status,
                Type = "RM_OnPremiseSP_EnforceRuleAction_Item",
                AgentName = AvePoint.GCommon.Utility.OSInformation.HostName,
                Comment = comment
            };
            JobDetailService.Commit(detail);
        }
        private string GetActionString(Rule currentRule, bool isManualScan)
        {
            string actionString = string.Empty;
            if (currentRule == null)
            {
                actionString = string.Empty;
            }
            //else if (isManualScan)
            //{
            //    actionString = "RM_OnPremiseSP_EnforceRuleAction_Manual";
            //}
            else if (currentRule.spMoveOption != null &&
                                currentRule.spMoveOption.MoveSetting != null)
            {
                actionString = "RM_OnPremiseSP_EnforceRuleAction_Move";
            }
            else if ((currentRule.KeepDataOption & (int)KeepDataOption.LinkDocument) == (int)KeepDataOption.LinkDocument)
            {
                actionString = "RM_OnPremiseSP_EnforceRuleAction_LeaveStub";
            }
            else if ((currentRule.KeepDataOption & (int)KeepDataOption.Keep) == (int)KeepDataOption.Keep)
            {
                actionString = "RM_OnPremiseSP_EnforceRuleAction_TagAndDeclare";
            }
            else if ((currentRule.KeepDataOption & (int)KeepDataOption.Delete) == (int)KeepDataOption.Delete
                 ||
                (currentRule.KeepDataOption & (int)KeepDataOption.Remove) == (int)KeepDataOption.Remove)
            {
                actionString = "RM_OnPremiseSP_EnforceRuleAction_Remove";
            }

            return actionString;
        }

        private bool IsOnedrive(string siteUrl)
        {
            var reg = new Regex(@"https://([^/]+?)-my\.(sharepoint[^/]*)(/.*)?");
            var matches = reg.Match(siteUrl);
            if (matches.Success)
            {
                logger.Info($"Current site is onedrive site. Url:[{siteUrl.LogBase64()}]");
            }
            return matches.Success;
        }

        public void RunMultiThreadsProcessItem(IAveListItemCollection items, int itemsPerTask, CancellationTokenSource cts, string columnName, Dictionary<Guid, OnPremiseSPListCacheDto> azureTableRecords, Dictionary<Guid, OnPremiseSPListCacheDto> exploreDBRecords)
        {
            AveTenantTasks.RunParallel(items, itemsPerTask, cts, item =>
            {
                ProcessItem(item, columnName, azureTableRecords, exploreDBRecords);
            });
        }
        public void RunMultiThreadsProcessItem(List<IAveListItem> items, int itemsPerTask, CancellationTokenSource cts, string columnName, Dictionary<Guid, OnPremiseSPListCacheDto> azureTableRecords, Dictionary<Guid, OnPremiseSPListCacheDto> exploreDBRecords)
        {
            AveTenantTasks.RunParallel(items, itemsPerTask, cts, item =>
            {
                ProcessItem(item, columnName, azureTableRecords, exploreDBRecords);
            });
        }
        #region to do merge common method?
        /// <summary>
        /// 获取每次最多可以操作多少条记录。
        /// *以后需要让Wrapper在AgentCommonObjectModelCommon.dll的IAveSite中提供一个获取MaxItemsPerThrottledOperation的方式*
        /// </summary>
        /// <param name="discoverSite">IAveSite</param>
        /// <returns>MaxItemsPerThrottledOperation: 每次最多可以操作多少条记录</returns>
        protected int GetMaxItemsPerThrottledOperation(IAveSite aveSite)
        {
            int maxItemsPer = 2000; //5000;  //SPO默认值为5000 并且不能修改， 某些Library 5000分页查询依然会超出Throttle， 限制到2000   from CI
            try
            {
                var dataCacheType = aveSite.GetType().GetProperty("DataCache");
                var dataCacheObj = dataCacheType.GetValue(aveSite);
                BindingFlags InstanceBindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var propertiesCacheProp = dataCacheObj.GetType().GetProperty("PropertiesCache", InstanceBindFlags);
                var propertiesCacheObj = propertiesCacheProp.GetValue(dataCacheObj);
                var propertiesDic = (propertiesCacheObj as AveDictionary<string, object>);
                object maxItemsPerObj;
                if (propertiesDic.TryGetValue("MaxItemsPerThrottledOperation", out maxItemsPerObj))
                {
                    maxItemsPer = Convert.ToInt32(maxItemsPerObj);
                    logger.Info($"GetMaxItemsPerThrottledOperation succeed. Count:[{maxItemsPer}]");
                    if (maxItemsPer > 2000)
                    {
                        logger.Info("MaxItemsPerThrottledOperation is > 2000, limit it to 2000");
                        maxItemsPer = 2000;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("GetMaxItemsPerThrottledOperation by siteCollection NodeItem faild, Error message:", ex.ToString());
            }
            return maxItemsPer;
        }
        public List<Guid> GetTermIds(IAveTaxonomyField taxonomyField)
        {
            List<Guid> subTermIds;
            Guid anchordGuid;
            string anchordId = taxonomyField.GetProperty("AnchorId");
            if (!string.IsNullOrEmpty(anchordId) && anchordId != "00000000-0000-0000-0000-000000000000")
            {
                anchordGuid = new Guid(anchordId);
                var iTermId = AllTerms.Where(t => t.UniqueId == anchordGuid).Select(t => t.Id).FirstOrDefault();
                if (iTermId > 0)
                {
                    string partPath = "/" + iTermId.ToString() + "/";
                    return (from a in AllTermSetMemberships
                            join b in AllTerms on a.TermId equals b.Id
                            where a.IsRemoved == false && a.Path.Contains(partPath)
                            select b.UniqueId).ToList();
                }
                //subTermIds = TermDao.GetAllSubTermUniqueIdsByTermId(anchordGuid);//
            }
            else
            {
                var iTermSetId = AllTermSets.Where(t => t.UniqueId == taxonomyField.TermSetId).Select(t => t.Id).FirstOrDefault();
                if (iTermSetId > 0)
                {
                    return AllTerms.Where(t => t.IsRemoved == false && t.TermSetId == iTermSetId).Select(t => t.UniqueId).ToList();
                }
                //subTermIds = TermDao.GetAllSubTermUniqueIdsByTermSetId();
            }

            return new List<Guid>();
        }
        protected IAveTaxonomyField GetTaxonomyField(IAveFieldCollection fields, string rmFieldTitle)
        {
            //var field = fields.GetField(rmFieldTitle);
            var field = fields.AsQueryable().Where(f => f.Title.Equals(rmFieldTitle, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (field == null)
            {
                field = fields.AsQueryable().Where(f => f.InternalName.Equals("RevIMBCS", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            }
            return field as IAveTaxonomyField;
        }
        protected void InitWssIdsForTerms(IAveSite site)
        {
            using (new AgentPerformanceScope($"Init WssIds:{site.Url}"))
            {
                try
                {
                    IAveList taxonomyList = site.RootWeb.Lists.GetByTitle("TaxonomyHiddenList");
                    IAveListItemCollection termItems = taxonomyList.Items;
                    if (null == TermWssidMappingsOfSite)
                    {
                        TermWssidMappingsOfSite = new Dictionary<Guid, int>();
                    }
                    foreach (var termItem in termItems)
                    {
                        if (!TermWssidMappingsOfSite.ContainsKey(new Guid(termItem["IdForTerm"].ToString())))
                        {
                            TermWssidMappingsOfSite.Add(new Guid(termItem["IdForTerm"].ToString()), int.Parse(termItem["ID"].ToString()));
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("Init Term Ids error {0}", e.ToString());
                }
            }
        }
        protected string GetWssIDForTerm(IAveSite site, Guid termId)
        {
            try
            {
                string result = "-1";
                IAveList taxonomyList = site.RootWeb.Lists.GetByTitle("TaxonomyHiddenList");

                AveCamlQuery camlQueryForTerm = new AveCamlQuery();
                camlQueryForTerm.ViewXml = @"
<View>
    <Query>
        <Where>
            <Eq>
                <FieldRef Name='IdForTerm' />
                <Value Type='Text'>" + termId + @"</Value>
            </Eq>
        </Where>
    </Query>       
</View>";

                IAveListItemCollection termItems = taxonomyList.GetItems(camlQueryForTerm);
                foreach (var termItem in termItems)
                {
                    string taxId = termItem["IdForTerm"].ToString();
                    if (taxId.Equals(termId.ToString()))
                    {
                        return termItem["ID"].ToString();
                    }
                }
                return result;
            }
            catch (Exception e1)
            {
                logger.Warn("Get Term Id error {0}", e1.ToString());
                return "-1";
            }
        }
        public virtual bool GetWssidOfTerm(IAveList list, IAveTaxonomyField taxonomyField, Guid termId, out int wssid)
        {
            bool result = false;
            lock (LockObj)
            {
                if (null == TermWssidMappingsOfSite)
                {
                    TermWssidMappingsOfSite = new Dictionary<Guid, int>();
                }
                if (!TermWssidMappingsOfSite.TryGetValue(termId, out wssid))
                {
                    var taxonomyFieldValue = taxonomyField.TaxonomyFieldValue;
                    try
                    {
                        wssid = int.Parse(GetWssIDForTerm(list.ParentWeb.Site, termId));
                        if (wssid > 0)
                        {
                            result = true;
                            TermWssidMappingsOfSite.Add(termId, wssid);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Get TermId And WssId Mapping failed! Term id: {0}. Error message: {1}.", termId, ex.ToString());
                    }
                }
                else if (wssid != 0)
                {
                    return true;
                }
            }
            return result;
        }
        /// <summary>
        /// Replace get field value by internal name
        /// </summary>
        /// <param name="item"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldInternalName"></param>
        /// <param name="termId"></param>
        /// <param name="termName"></param>
        /// <returns></returns>
        protected bool GetSingleTaxonomyFieldValue(IAveListItem item, string fieldInternalName, out Guid termId, out string termName)
        {
            bool result = true;
            termName = string.Empty;
            termId = new Guid();
            try
            {
                if (item.FieldValues.ContainsKey(fieldInternalName))
                {
                    var valueString = item[fieldInternalName].ToString();
                    var values = valueString.Split('|');
                    termId = new Guid(values[1]);
                    termName = values[0];
                    //如果term以full path形式显示会包含“:”，Content Due和Term Usage Report要求只显示Name，不显示路径
                    if (termName.Contains(":"))
                    {
                        termName = termName.Substring(termName.LastIndexOf(":") + 1);
                    }
                }
            }
            catch (Exception ex)
            {
                //REC-4997 Get RevIMBCS Field Value Error
                if (item.ParentList.GetItemById(item.ID).FieldValues.ContainsKey(fieldInternalName))
                {
                    var valueString = item.ParentList.GetItemById(item.ID)[fieldInternalName].ToString();
                    var values = valueString.Split('|');
                    termId = new Guid(values[1]);
                    termName = values[0];
                    //如果term以full path形式显示会包含“:”，Content Due和Term Usage Report要求只显示Name，不显示路径
                    if (termName.Contains(":"))
                    {
                        termName = termName.Substring(termName.LastIndexOf(":") + 1);
                    }
                    return result;
                }
                logger.Warn("Get single taxonomy field value failed! Item url: {0}, fieldName: {1}, error message: {2}.", item.Url.LogBase64(), fieldInternalName.LogBase64(), ex.ToString());
                result = false;
            }
            return result;
        }
        #endregion
        #region SPAction Job Detail
        protected virtual void AddFailedDetail(object objInfo, string comment, string ruleName = "", string action = "")
        {
            try
            {
                string objectName = string.Empty;
                string fullPath = string.Empty;
                if (objInfo is AveDiscoverItem)
                {
                    var item = objInfo as AveDiscoverItem;
                    objectName = item.LeafName;
                    fullPath = item.FullUrl;
                }
                else if (objInfo is AveDiscoverFolder)
                {
                    var folder = objInfo as AveDiscoverFolder;
                    objectName = folder.LeafName;
                    fullPath = folder.FullUrl;
                }
                else if (objInfo is AveDiscoverList)
                {
                    var list = objInfo as AveDiscoverList;
                    objectName = list.Name;
                    fullPath = list.RootFolderUrl;
                }
                else if (objInfo is AveDiscoverWeb)
                {
                    var web = objInfo as AveDiscoverWeb;
                    objectName = web.Name;
                    fullPath = web.FullUrl;
                }
                else if (objInfo is AveDiscoverSite)
                {
                    var site = objInfo as AveDiscoverSite;
                    objectName = site.GetRootWeb().Name;
                    //to do
                    fullPath = site.GetRootWeb().FullUrl;
                }
                else if (objInfo is IAveListItem)
                {
                    var item = objInfo as IAveListItem;
                    objectName = item.Name;
                    fullPath = item.Url;
                }

                //finalDetails.Add(new JMCollectionDataJobDetails() { ObjectName = objectName, FullPath = fullPath, Comment = comment, Status = JobDetailsStatus.Failed });
                SendReport(objectName, fullPath, null, JobDetailsStatus.Failed, 0, false, comment);
            }
            catch (Exception e)
            {
                logger.Warn($"Add job exception report failed {e.ToString()}");
            }
        }
        #endregion
        public void DisposeSPObj(object objInfo)
        {
            try
            {
                if (objInfo is AveDiscoverItem)
                {
                    var itemDiscoverObj = objInfo as AveDiscoverItem;
                    itemDiscoverObj.Dispose();
                }
                else if (objInfo is AveDiscoverFolder)
                {
                    var folderDiscoverObj = objInfo as AveDiscoverFolder;
                    folderDiscoverObj.Dispose();
                }
                else if (objInfo is AveDiscoverList)
                {
                    var listDiscoverObj = objInfo as AveDiscoverList;
                    listDiscoverObj.Dispose();
                }
                else if (objInfo is AveDiscoverWeb)
                {
                    var webDiscoverObj = objInfo as AveDiscoverWeb;
                    webDiscoverObj.Dispose();
                }
                else if (objInfo is AveDiscoverSite)
                {
                    var siteDiscoverObj = objInfo as AveDiscoverSite;
                    siteDiscoverObj.Dispose();
                }
                else if (objInfo is IAveWeb)
                {
                    var webObj = objInfo as IAveWeb;
                    webObj.Dispose();
                }

                else if (objInfo is IAveSite)
                {
                    var siteObj = objInfo as IAveSite;
                    siteObj.Dispose();
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Error in Dispose sp object {0}", ex.ToString());
            }
        }
        protected bool CheckIsDesignList(string listInfo)
        {
            bool isDesignList = false;
            try
            {
                if (this.DesignLists.Contains(listInfo))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                logger.Info($"Init Design List error {e.ToString()}.");
                return isDesignList;
            }
            return isDesignList;
        }
        public bool IsSystemList(AveDiscoverList list)
        {
            try
            {
                if (list.Title == "{System Folder}")
                {
                    logger.Info($"SystemFolder list: [{list.Title}], list type: [{list.Type}].");
                    return true;
                }
                var listInfo = list.RootFolderUrl.Substring(list.RootFolderUrl.LastIndexOf('/') + 1) + list.ServerTemplate;
                if (CheckIsDesignList(listInfo))
                {
                    logger.Info("design list : {0}, listInfo: {1}.", list.Title, listInfo);
                    return true;
                }
                if (list.Hidden.HasValue && list.Hidden.Value)
                {
                    logger.Info("hidden list : {0}, listInfo: {1}.", list.Title, listInfo);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.Warn("get list system setting error {0}", ex.ToString());
                return false;
            }
        }
        public bool IsFileExtentionInExculdeList(List<string> excludeFileExtention, IAveListItem item)
        {
            var extention = item.Name.Substring(item.Name.LastIndexOf('.') + 1);
            if (excludeFileExtention.Contains(extention.ToLowerInvariant()))
            {
                return true;
            }
            return false;
        }
        public bool IsLibraryInExcludeList(List<string> excludeList, IAveList list)
        {
            var listTitle = list.Title.ToLowerInvariant();
            if (excludeList.Contains(listTitle))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// web fullpath list serverel url..
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool IsBreakInheritNode(string url)
        {
            //如果传的是ServerRelativeURL，则拼接Full URL
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }
            else if (!url.StartsWith(PartSiteUrl))
            {
                url = PartSiteUrl + url;
            }
            string sha1Url = RAEncodeUtil.EncryptBySHA1(url);
            if (BreakTreeNodeUrls != null && BreakTreeNodeUrls.Contains(sha1Url))
            {
                return true;
            }
            return false;
        }

        public void UndeclaredItem(IAveListItem item, Rule resultRule, long itemSize)
        {
            string itemFullPath = WebUtil.MakeFullUrl(item.ParentList.ParentWeb.Url, item.Url);
            if (item.IsRecord())
            {
                actionUtility.UndeclareItem(item);
            }
        }
        /// <summary>
        /// Real delete item ,check label setting first.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="LabelInfos"></param>
        public void DeleteItem(IAveListItem item, Rule resultRule, long itemSize)
        {
            string itemFullPath = WebUtil.MakeFullUrl(item.ParentList.ParentWeb.Url, item.Url);
            if (item.IsRecord())
            {
                actionUtility.UndeclareItem(item);
            }
            item.Delete();
        }
        public Tuple<long,string> DeleteRelatedItem(string itemUrl, Guid webId,Guid listId, Guid docId,bool needToDelete,Rule resultRule)
        {
            try
            {
                logger.Info($"start delete related data,id:{docId},webid:{webId} ");
                var site = ObjectModelFactoryForRelated.CreateSite();
                var web = site.OpenWeb(webId);
                var item = web.GetListItem(itemUrl, listId, docId);
                if (item == null)
                {
                    logger.Info($"no need to delete related data,id:{docId} ,it has not exist anymore");
                    throw new Exception(RELATEDFILENOTEXIST);
                }
                long size = GetItemSize(item);
                string url = item.FullPath();
                LinkDocumentForStubRule(resultRule, item);
                if (needToDelete)
                {
                    logger.Info($"need to delete related data,id:{docId} ");
                    string itemFullPath = WebUtil.MakeFullUrl(item.ParentList.ParentWeb.Url, item.Url);
                    if (item.IsRecord())
                    {
                        actionUtility.UndeclareItem(item);
                    }
                    item.Delete();
                }
                return new Tuple<long, string>(size, url);
            }
            catch (Exception e)
            {
                logger.Error($"delete related data failed,error:{e}");
                throw;
            }
        }
        public bool ValidateItemCanDelete(IAveListItem item, AveComplianceTagInfo labelInfo, ref DateTime labelCalTime)
        {
            bool result = false;
            try
            {
                DateTime labelExpireTime = DateTime.MinValue;
                switch (labelInfo.TagRetentionBasedOn)
                {
                    case "CreationAgeInDays":
                        labelCalTime = DateTime.Parse(item["Created"].ToString()).ToUniversalTime();
                        labelExpireTime = DateTime.Parse(item["Created"].ToString()).ToUniversalTime().AddDays(labelInfo.TagDuration);
                        break;
                    case "TaggedAgeInDays":
                        labelCalTime = DateTime.Parse(item["_ComplianceTagWrittenTime"].ToString()).ToUniversalTime();
                        labelExpireTime = DateTime.Parse(item["_ComplianceTagWrittenTime"].ToString()).ToUniversalTime().AddDays(labelInfo.TagDuration);
                        break;
                    case "ModificationAgeInDays":
                        labelCalTime = DateTime.Parse(item["Modified"].ToString()).ToUniversalTime();
                        labelExpireTime = DateTime.Parse(item["Modified"].ToString()).ToUniversalTime().AddDays(labelInfo.TagDuration);
                        break;
                    default:
                        logger.Info($"Current label retention base on type not support {labelInfo.TagName.LogBase64()} : {labelInfo.TagRetentionBasedOn}");
                        return false;
                }

                if (DateTime.UtcNow > labelExpireTime)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
                logger.Info($"[{item.ID}] Calculte the label [{labelCalTime}] [{labelInfo.TagRetentionBasedOn}] : [{labelInfo.TagDuration}] == delay retention about 1day [{result}]");
            }
            catch (Exception e)
            {
                logger.Warn($"{item.Url.LogBase64()}  Init label validation failed result {result}  {e.ToString()}");
            }
            return result;
        }

        private string GetExceptionMessage(Exception e)
        {
            string comment = e.Message;
            if (e is System.Reflection.TargetInvocationException)
            {
                System.Reflection.TargetInvocationException te = e as System.Reflection.TargetInvocationException;
                if (te.InnerException != null)
                {
                    comment = te.InnerException.Message;
                }
            }
            return comment;
        }

        private string GetFinishTime(DateTime time)
        {
            if (TimeSettingModel != null)
            {
                return TimeSettingUtil.ConvertTiksToDateTime(TimeSettingModel, time.Ticks, TimeFormat).SimplifyFormatTime;
            }
            else
            {
                TimeZoneInfo localZone = TimeZoneInfo.Local;
                DateTime currentDate = TimeSettingUtil.ConvertTimeFromUtc(time.Ticks, localZone, false);
                return currentDate.ToString(DEFAULT_TIME_FORMAT);
            }
        }

        #region Leave Stub logic
        private void LinkDocument(IAveFile file, Rule resultRule)
        {
            using (var performance = new AgentPerformanceScope("ArchiverDeletion.LinkDocument", addToStatistics: true))
            {
                string folderPath = string.Empty;
                string filePath = string.Empty;
                string desUrl = string.Empty;
                StubFileContent = GetFileContent(file.ParentFolder.ParentWeb.LanguageCulture);
                folderPath = Path.Combine(AveEnv.AgentJobFolder, JobContext.Current.JobId);
                filePath = Path.Combine(folderPath, Guid.NewGuid().ToString() + ".dat");
                desUrl = this.GetDestUrlByFile(file);
                logger.Info("Current file is LinkDocument.FileUrl:{0}.", file.UniqueId);
                try
                {
                    #region init temp file
                    try
                    {
                        lock (mLock)
                        {
                            if (!Directory.Exists(folderPath))
                            {
                                Directory.CreateDirectory(folderPath);
                                logger.Info("Create Folder : {0}.", folderPath.LogBase64());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Directory.Exists(folderPath))
                        {
                            logger.Info("the folder is exist, folderPath: {0}.", folderPath.LogBase64());
                        }
                        else
                        {
                            logger.Error("Can not create temp folder : {0}. Reason: {1}.", folderPath.LogBase64(), ex.ToString());
                            throw;
                        }
                    }
                    #endregion


                    using (var performance1 = new AgentPerformanceScope("ArchiverDeletion.LinkDocument.ExportDocument", addToStatistics: true))
                    {
                        using (OnPremSPLeaveStubFileSender fileSender = new OnPremSPLeaveStubFileSender(filePath))
                        {
                            using (IAveBackupStream exportStream = new WrapperBackupStreamV1(new FileSendWrapper(fileSender)))
                            {
                                OnPremSPLeaveStubDocExport exportor = null;
                                exportor = new OnPremSPLeaveStubDocExport(onPremSPLeaveStubWrapperBackupCache.GetCurrentAveBackupFolder(file.ParentFolder), file, StubFileContent);
                                exportor.ExportSPFile(exportStream);
                            }
                        }
                    }
                    this.LeaveDocumentLinkFile(file, desUrl, filePath, resultRule);
                }
                catch (Exception e)
                {
                    logger.Warn("Some errors occur when export file, file unique Id {0}, error detail {1}.", file?.UniqueId, e.ToString());
                    throw;
                }

            }
        }

        private string GetDestUrlByFile(IAveFile file)
        {
            using (var performance = new AgentPerformanceScope("ArchiverDeletion.GetDestUrlByFile", addToStatistics: true))
            {
                string desUrl = string.Empty;
                string[] destUrls = file.ServerRelativeUrl.Split('/');
                string[] webdestUrls = file.Web.Site.Url.Split('/');
                for (int i = 0; i < 3; i++)
                {
                    desUrl += webdestUrls[i] + "/";
                }
                for (int i = 1; i < destUrls.Length - 1; i++)
                {
                    desUrl += destUrls[i] + "/";
                }
                return desUrl;
            }
        }

        private void LeaveDocumentLinkFile(IAveFile file, string desUrl, string filePath, Rule resultRule)
        {
            using (var performance = new AgentPerformanceScope("ArchiverDeletion.LeaveDocumentLinkFile", addToStatistics: true))
            {
                string newLeafName = file.Name + ".aspx";
                IAveFile newfile = null;
                try
                {
                    using (var performance1 = new AgentPerformanceScope("ArchiverDeletion.LeaveDocumentLinkFile.Restore", addToStatistics: true))
                    {
                        using (OnPremSPLeaveStubFileReceiver fileReceiver = new OnPremSPLeaveStubFileReceiver(filePath))
                        {
                            using (var importStream = new WrapperRestoreStreamV1(new FileReceiverWrapper(fileReceiver)))
                            {
                                string listUrl = onPremSPLeaveStubWrapperIAveObjectCache.StubIAveList.ParentWeb.Url + "/" + onPremSPLeaveStubWrapperIAveObjectCache.StubIAveList.RootFolder.Url;
                                string subFolderUrl = desUrl.Substring(listUrl.Length).Trim('/');
                                Wrapper.Restore.AveSPFolder aveSPFolder = null;
                                aveSPFolder = onPremSPLeaveStubWrapperRestoreCache.GetStubRestoreAveCurrentFolder(subFolderUrl, file.ParentFolder.UniqueId);
                                using (OnPremSPLeaveStubDocImport importor = new OnPremSPLeaveStubDocImport(aveSPFolder, null, newLeafName, desUrl))
                                {
                                    importor.ImportAveSPDoc(importStream);
                                }
                            }
                        }
                    }
                    newfile = GetCreateLinkFile(desUrl, newLeafName);
                    IAveListItem newItem = newfile.Item;
                    //REC-2432 Host Header Site Collection通过IAveFile GetFile(string serverRelativeUrl);方式获取不到IAveListItem对象.
                    if (newItem == null)
                    {
                        logger.Info("Current IAveListItem is null and will ReGet IAveListItem by List GetItemByUniqueId.");
                        newItem = onPremSPLeaveStubWrapperIAveObjectCache.StubIAveList.GetItemByUniqueId(newfile.UniqueId);
                        logger.Info("ReGet IAveListItem successful by List GetItemByUniqueId. IAveListItem is null:{0}.", newItem == null);
                    }
                    try
                    {
                        if (resultRule != null && resultRule.DeclareLinkFile)
                        {
                            try
                            {
                                if (newItem.File != null && newItem.File.CheckedOutByUser != null)
                                {
                                    newItem.File.CheckIn("");
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Warn($"Error occurred while undocheckout for link file. File:{file?.UniqueId} Error:{e.ToString()}");
                            }
                            actionUtility.DeclareItem(newItem, newItem.Url);
                        }
                    }
                    catch (Exception exc)
                    {
                        logger.Warn("Declare Item has some error, detail: {0}.", exc.ToString());
                        throw;
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("Some error occur when create leave a stub item ,file unique Id {0}, error detail: {1}.", file?.UniqueId, e.ToString());
                    newfile = GetCreateLinkFile(desUrl, newLeafName);
                    if (newfile != null && newfile.Exists)
                    {
                        var exItem = onPremSPLeaveStubWrapperIAveObjectCache.StubIAveList.GetItemByUniqueId(newfile.UniqueId);
                        if (exItem.IsRecord())
                        {
                            logger.Info("Current wrong stub file is declare file and need undeclare.FileName:{0}.", newfile.UniqueId);
                            actionUtility.UndeclareItem(exItem);
                            exItem = onPremSPLeaveStubWrapperIAveObjectCache.StubIAveList.GetItemByUniqueId(newfile.UniqueId);
                            logger.Info("Current wrong stub file is declare file and undeclare success.FileName:{0}.", newfile.UniqueId);
                        }
                        exItem.Delete();
                        logger.Info("Current wrong stub file is delete success.FileName:{0}.", newfile.UniqueId);
                    }
                    else
                    {
                        logger.Info("Current wrong stub file object is null and skip delete.FileName:{0}.", newLeafName.LogBase64());
                    }
                    throw new RALeaveStubException(e.Message, e);
                }
                finally
                {
                    logger.Info("End to Restore.SourceFileUrl:{0}.DesListUrl:{1}.", file.UniqueId, desUrl.LogBase64());
                    DeleteTempFile(new List<string>() { filePath });
                }
            }
        }

        private IAveFile GetCreateLinkFile(string desContainerUrl, string newLeafName)
        {
            using (var performance = new AgentPerformanceScope("ArchiverDeletion.GetCreateLinkFile", addToStatistics: true))
            {
                IAveFile linkFile = onPremSPLeaveStubWrapperIAveObjectCache.StubIAveWeb.GetFile(desContainerUrl + newLeafName);
                if (linkFile.UniqueId == Guid.Empty || linkFile.Item == null)
                {
                    logger.Info("File UniqueId is Guid empty and reGet file.");
                    try
                    {
                        //Office 365 Root Site Collection need send serverRelativeUrl.RECO-1278
                        linkFile = onPremSPLeaveStubWrapperIAveObjectCache.StubIAveWeb.GetFile(System.Web.HttpUtility.UrlDecode(new Uri(desContainerUrl + newLeafName).AbsolutePath));
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Can't reGet file UniqueId.Message:{0}.", ex.ToString());
                    }
                }
                return linkFile;
            }
        }

        private void DeleteTempFile(List<string> files)
        {
            foreach (string fileFullPath in files)
            {
                try
                {
                    File.Delete(fileFullPath);
                    logger.Info("Delete Temp file Successful: {0}", fileFullPath.LogBase64());
                }
                catch (Exception ex)
                {
                    logger.Warn("Error in Delete Temp File: {0}, Error: {1}", fileFullPath.LogBase64(), ex.ToString());
                }
            }
        }

        private byte[] GetFileContent(CultureInfo cultureInfo)
        {
            byte[] content = null;
            logger.Info("Current culture is:{0}.", cultureInfo.Name.LogBase64());
            string stubPath = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"Config\AgentCommonRecordsDisposalStub.aspx");
            using (FileStream stream = new FileStream(stubPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                content = new byte[stream.Length];
                stream.Read(content, 0, (int)stream.Length);
            }
            return content;
        }
        #endregion
    }
}
