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
//using AvePoint.Adonis.Records.Object.ActionOnly;
//using AvePoint.Adonis.StorageOptimization.Common.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.SharePoint.ActionOnly.Base;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Discover;
//using AvePoint.RA.SharePoint.EnforceRetention.Common;
using AvePoint.Wrapper.Common;
//using AvePoint.Wrapper.Contract;
using AvePoint.Wrapper.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text.RegularExpressions;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.SharePoint.Extension;
using System.Reflection;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Common.Global.Utils;

namespace AvePoint.RA.SharePoint.ActionOnly.SPActionOnly
{
    public abstract class BaseSPActionProcessor : BaseActionProcessor
    {
        private static RALogger logger = RALogger.GetInstance(typeof(BaseSPActionProcessor));
        protected ITermDao TermDao = new TermDao();
        protected SharePointSettingUtility SPUtility = new SharePointSettingUtility();
        //protected AveDiscoveryOMFactory discoverFactory = AveDiscoveryOMFactory.CreateDiscoveryOMFactory();
        public DAUtil DAUtil { get; private set; }
        public Dictionary<Guid, RMRuleItemCollection> TermAndRulesMapping { get; private set; }
        public Dictionary<Guid, int> TermWssidMappingsOfSite;
        public Dictionary<Guid, IAveTimeZone> TimeZones = new Dictionary<Guid, IAveTimeZone>();
        protected string BCSColumnName;
        protected AveObjectModelFactory ObjectModelFactory;
        protected AveBPOSAccountInfo bposInfo;
        protected SPTreeNodeDto CurrentSiteColTreeNode;
        protected SPTreeNodeDto CurrentNode;
        protected ActionUtility actionUtility;
        protected List<string> DesignLists = new List<string>();
        protected ConfigSiteSetting ConfigSiteSetting = null;
        protected List<AveComplianceTagInfo> TagInfos = new List<AveComplianceTagInfo>();
        protected ActiveWindow ActiveWindow;
        public BaseSPActionProcessor() : base()
        {
            WrapperConfiguration.WrapperConfigurationForBPOS.IncludeVersionForPerformance = false;
            DAUtil = new DAUtil();
            TermAndRulesMapping = DAUtil.GetTermAndRuleMappings(DateTime.UtcNow, AllRecordsRule, false);//Init Term Rule Settings
        }
        public BaseSPActionProcessor(SPTreeNodeDto CurrentTreeNode, List<Rule> recordsRule) : base(recordsRule)
        {
            WrapperConfiguration.WrapperConfigurationForBPOS.IncludeVersionForPerformance = false;
            DAUtil = new DAUtil();
            TermAndRulesMapping = DAUtil.GetTermAndRuleMappings(DateTime.UtcNow, AllRecordsRule, false);//Init Term Rule Settings
            CurrentNode = CurrentTreeNode;
            CurrentSiteColTreeNode = GetSiteCollectionNode(CurrentTreeNode);
            BCSColumnName = SPUtility.GetMedataColumn(new Guid(CurrentSiteColTreeNode.Parent.SPObjectId));
            GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(CurrentSiteColTreeNode.FullPath);
            bposInfo = PoolUserUtil.GetBPOSInfoAsync(remoteSiteCollection).Result;
            //bposInfo = PoolUserUtil.GetAveBPOSAccountInfo(CurrentSiteColTreeNode.NodeExtension.BposInfo, CurrentSiteColTreeNode.FullPath);//SPUtility.GetBPOSInfo(CurrentSiteColTreeNode);
            ObjectModelFactory = MultiAppUtil.CreateAveObjectModelFactory(CurrentSiteColTreeNode.FullPath, bposInfo, AveContextKind.ClientObjectModel);
            actionUtility = ActionUtility.GetInstance(ObjectModelFactory);
            DesignLists = WebUtil.GetDesignLists(JobContext.IsCSDTenant);
            ConfigSiteSetting = (new ConfigSiteUtil(bposInfo, CurrentSiteColTreeNode.FullPath)).GetConfigData();
            using (var aveSite = ObjectModelFactory.CreateSite(CurrentSiteColTreeNode.FullPath))
            {
                InitTagInfos(aveSite);
                InitRecordsFeature(aveSite);
            }
            ActiveWindow = new ActiveWindow();
            ActiveWindow.Init();
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
        protected void InitTagInfos(IAveSite site)
        {
            try
            {
                TagInfos = site.GetAvailableTagsForSite();
            }
            catch (Exception e)
            {
                logger.Info($"Init Complicance lable tag infos failed {e.ToString()}");
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
        public virtual void ProcessSiteCollection(SPTreeNodeDto SiteNode)
        {
            ReportManager.Increase();
        }
        public virtual void ProcessSite(IAveWeb site)
        {
            ReportManager.Increase();
        }
        public virtual void ProcessSite(AveDiscoverWeb site)
        {
            ReportManager.Increase();
        }
        public virtual void ProcessList(AveDiscoverList list)
        {
            ReportManager.Increase();
        }
        public virtual void ProcessList(IAveList list)
        {
            ReportManager.Increase();
        }
        public virtual void ProcessFolder(AveDiscoverFolder folder)
        {
            ReportManager.Increase();
        }
        public virtual void ProcessFolder(IAveFolder folder)
        {
            ReportManager.Increase();
        }

        public bool NeedSkip(IAveListItem item, string bcsInternalName, ref string skipReason)
        {
            var taxValue = (string)item[bcsInternalName];
            var termId = new Guid(taxValue.Substring(taxValue.LastIndexOf('|') + 1));
            if (ConfigSiteSetting.ExcludedFileTypeDefaultTerm.ID.Equals(termId))
            {
                if (IsFileExtentionInExculdeList(ConfigSiteSetting.ExcludeFileExtentions, item))
                {
                    skipReason = "RM_JS_DAM_JobDetail_WhiteFileWithDefaultTerm";
                }
                else
                {
                    skipReason = "RM_JS_DAM_JobDetail_UseWhiteFileTerm";
                }
                return true;
            }
            else if (ConfigSiteSetting.ModifiedBasedTermIds.Contains(termId))
            {
                skipReason = "RM_JS_DAM_JobDetail_UseModifiedBasedTerm";
                return true;
            }
            return false;
        }

        private T DeepCopy<T>(object rc)
        {
            string xml = SerializerHelper.SerializeByDataContractSerializer(rc);
            return SerializerHelper.DeserializeByDataContractSerializer<T>(xml);
        }

        public virtual void ProcessItem(IAveListItem item, string BCSColumnInternalName, bool checkRule = false)
        {
            Guid termId;
            string termName;
            RMRuleItemCollection rules;
            Rule resultRule = null;
            try
            {
                while (ActiveWindow.IsEnabled && !ActiveWindow.IsCurrentTimeInActiveWindow())
                {
                    logger.Warn($"Pause this job, because current time is NOT in active window. StartTime:[{ActiveWindow.StartTimeSpan}] EndTime:[{ActiveWindow.EndTimeSpan}] TimeZone:[{ActiveWindow.TimeZone.Id}]");
                    Thread.Sleep(3000);
                }
                logger.Info("Process item {0}", item.Url);
                string itemFullPath = WebUtil.MakeFullUrl(item.ParentList.ParentWeb.Url, item.Url);
                if (item.Folder != null)
                {
                    logger.Info($"Skip folder content type {item.Url}");
                    return;
                }
                string skipReason = string.Empty;
                if (NeedSkip(item, BCSColumnInternalName, ref skipReason))
                {
                    logger.Info($"Skip documents in exclude file extensions list. Item Url:[{item.Url}]");
                    SendReport(item.Name, itemFullPath, KeepDataOption.DeclareRecord.ToString(), "", JobDetailsStatus.Skipped, skipReason);
                    return;
                }
                if (!GetSingleTaxonomyFieldValue(item, BCSColumnInternalName, out termId, out termName))
                {
                    logger.Warn("can't get sigle item value {0}", item.Url);
                    return;
                }
                if (TermAndRulesMapping.TryGetValue(termId, out rules))
                {
                    string jobDetailComment = string.Empty;
                    if (!checkRule
                        //&& JobMessage.DiscoverType == 1
                        && !rules.HasUnCamlQueryableCondition
                        && rules.Rules.Count == 1
                        && rules.Rules[0].RuleFilters.Any(r => r.RuleType != ArchiverFilterRuleType.LastAccessedTime))
                    {
                        var ruleId = rules.Rules[0].RuleId;
                        resultRule = rules.CommonRules.Rules.Where(t => t.Value.Id.Equals(ruleId)).FirstOrDefault().Value;
                    }
                    else
                    {
                        RuleManagement ruleManagement = new RuleManagement(DeepCopy<RuleCollection>(rules.CommonRules));
                        resultRule = ruleManagement.CheckItemCriteria(item.UniqueId, item, true);
                    }
                    if (resultRule == null)
                    {
                        logger.Info($"Item {item.Url} not fit rule");
                        return;
                    }
                    logger.Info($"Start do action {resultRule.Name} : {resultRule.KeepDataOption}");
                    bool undeclared = false;
                    if ((resultRule.KeepDataOption & (int)KeepDataOption.TagContent) == (int)KeepDataOption.TagContent)
                    {
                        //add for manual declare and rule for add tag
                        if (item.IsBlockDeleteOnlyRecord())
                        {
                            logger.Info("This kind of records no need undeclared {0}", item.Url);
                        }
                        else if (item.IsRecord())
                        {
                            actionUtility.UndeclareItem(item);
                            undeclared = true;
                        }
                        item = item.ParentList.ParentWeb.GetListItem(item.Url, item.ParentList.ID, item.UniqueId);
                        actionUtility.CreateTagContent(item, resultRule.TagContentInfo);
                    }
                    if (undeclared || (resultRule.KeepDataOption & (int)KeepDataOption.DeclareRecord) == (int)KeepDataOption.DeclareRecord)
                    {
                        if (item.IsRecord())
                        {
                            logger.Info($"item is already being declared {item.Url}");
                            SendReport(item.Name, itemFullPath, KeepDataOption.DeclareRecord.ToString(), resultRule.Name, JobDetailsStatus.Skipped, "RM_UI_Detail_IsDeclared");
                            return;
                        }
                        if (!IsOnedrive(item.ParentList.ParentWeb.Site.Url))
                        {
                            logger.Info($"Check and update csd setting. ItemUrl:[{item.Url}]");
                            //1. Item以下情况Skip， Deletion Date为空，Retention Label为空，BCS Class对应的Rule为空， Job Details里要提示是Skip的原因
                            //2. 根据EventDate是否有值，判断使用CreationRetentionPeriod还是CSD_EventRetentionPeriod做Deletion/Label计算，并比较
                            //   现有的DeletionDate和新计算的DeletionDate/Label是否一致，不一致的话，Update成新计算的DeletionDate/Label
                            string declareSkipReason = string.Empty;
                            if (NeedSkip(item, ref declareSkipReason))
                            {
                                logger.Info($"Skip Item. Skip reason:[{declareSkipReason}] ItemUrl:[{item.Url}]");
                                SendReport(item.Name, itemFullPath, KeepDataOption.DeclareRecord.ToString(), resultRule.Name, JobDetailsStatus.Skipped, declareSkipReason);
                                return;
                            }
                            if (item.File != null)
                            {
                                logger.Info($"Delete file versions. ItemUrl:[{item.Url}]");
                                item.File.Versions.DeleteAll();
                                jobDetailComment += "The document's history versions have been deleted.";
                            }
                            ResetDeletionDateAndRetentionLabel(item, ref jobDetailComment);
                        }
                        try
                        {
                            actionUtility.DeclareItem(item, item.Url, undeclared);
                        }
                        catch (InvalidOperationException ex)
                        {
                            logger.Warn("In Place Records Management Feature is not installed in this farm, cannot declare document as a record:{0}", ex.ToString());
                        }
                    }
                    else if ((resultRule.KeepDataOption & (int)KeepDataOption.Remove) == (int)KeepDataOption.Remove || resultRule.KeepDataOption == 0)
                    {
                        logger.Info("Start tag validation logic.");
                        DeleteItem(item, TagInfos, resultRule);
                        return;
                    }
                    else if ((resultRule.KeepDataOption & (int)KeepDataOption.UndeclaredRecord) == (int)KeepDataOption.UndeclaredRecord)
                    {
                        logger.Info("Start tag validation logic.");
                        UndeclaredItem(item, TagInfos, resultRule);
                        return;
                    }
                    SendReport(item.Name, itemFullPath, KeepDataOption.DeclareRecord.ToString(), resultRule.Name, JobDetailsStatus.Successful, jobDetailComment);
                }
                logger.Info("Process item {0} success", item.Url);
            }
            catch (Exception e)
            {
                logger.Warn($"Process Item failed {e.ToString()}");
                string exMsg = GetExceptionMessage(e);
                AddFailedDetail(item, exMsg);
                JobHasErrorNode = true;
            }
            finally
            {
                ReportManager.Increase();
            }
        }

        public void SendReport(string name, string fullPath, string type, string ruleName, JobDetailsStatus status, string comment = "")
        {
            var detail = new JMActionOnlyJobDetails();
            detail.ObjectName = name;
            detail.RuleName = ruleName;
            detail.Status = status;
            detail.Comment = comment;
            detail.Url = fullPath;
            ReportManager.SendJobDetail(detail);
        }

        public static bool IsOnedrive(string siteUrl)
        {
            var reg = new Regex(@"https://([^/]+?)-my\.(sharepoint[^/]*)(/.*)?");
            var matches = reg.Match(siteUrl);
            if (matches.Success)
            {
                logger.Info($"Current site is onedrive site. Url:[{siteUrl}]");
            }
            return matches.Success;
        }
        private string GetLabel4UnLock(IAveListItem item, CSDRuleObject csdRule)
        {
            if (HasEventDate(item))
            {
                return csdRule.EventRetentionSetting.RetentionLabel;
            }
            else if (HasReclassDateFromModified2Creation(item))
            {
                return csdRule.RetentionLabel4ReclassModified2Creation;
            }
            else
            {
                return csdRule.CreationRetentionSetting.RetentionLabel;
            }
        }

        private void SetRetentionLabel(IAveListItem item)
        {
            var taxValue = (string)item[CSDFieldName.BCSColumn];
            var termId = new Guid(taxValue.Substring(taxValue.LastIndexOf('|') + 1));
            var csdRule = ConfigSiteSetting.CSDRules[termId];
            RetentionSetting rs;
            bool isSetEvent = HasEventDate(item);
            rs = isSetEvent ? csdRule.EventRetentionSetting : csdRule.CreationRetentionSetting;
            if (rs == null)
            {
                logger.Info("Retention label is null.");
                return;
            }
            var label = GetLabel4UnLock(item, csdRule);
            logger.Info($"Reset retention label. ItemUrl:[{item.Url}] Label:[{label}]");
            if (isSetEvent)
            {
                item.SetComplianceTag(string.Empty, false, false, false, false);
                var complianceWrittenDate = new DateTime(Convert.ToDateTime(item.FieldValues[CSDFieldName.EventDate]).Ticks, DateTimeKind.Utc);
                item.SetComplianceTag(label, false, false, complianceWrittenDate);
            }
            else
            {
                item.SetComplianceTag(label, false, false, false, false);
            }
        }
        private bool HasDateOfM2CColumn(IAveListItem item)
        {
            return item.Properties.Contains(CSDFieldName.ReclassDateOfModified2Creation) && item.Fields.ContainsField(CSDFieldName.ReclassDateOfModified2Creation);
        }

        private bool HasReclassDateFromModified2Creation(IAveListItem item)
        {
            return HasDateOfM2CColumn(item)
                && !string.IsNullOrEmpty(item[CSDFieldName.ReclassDateOfModified2Creation]?.ToString());
        }

        private bool HasEventDate(IAveListItem item)
        {
            return !string.IsNullOrEmpty(item[CSDFieldName.EventDate]?.ToString());
        }

        private bool HasLockedLabel(CSDRuleObject csdRule)
        {
            return !string.IsNullOrEmpty(csdRule.RetentionLabelForLockedDoc);
        }

        private string GetLabelForLock(IAveListItem item, CSDRuleObject csdRule)
        {
            if (HasLockedLabel(csdRule))
            {
                return csdRule.RetentionLabelForLockedDoc;
            }
            else if (HasEventDate(item))
            {
                return csdRule.EventRetentionSetting.RetentionLabel;
            }
            else if (HasReclassDateFromModified2Creation(item))
            {
                if (string.IsNullOrEmpty(csdRule.RetentionLabel4ReclassModified2Creation))
                {
                    throw new Exception("RM_JS_JMD_NoRetentionLabel4ReclassModified2Creation");
                }
                return csdRule.RetentionLabel4ReclassModified2Creation;
            }
            else
            {
                return csdRule.CreationRetentionSetting.RetentionLabel;
            }
        }

        private void ResetDeletionDateAndRetentionLabel(IAveListItem item, ref string jobDetailComment)
        {
            logger.Info($"Started to check and update deletionDate and retentionLabel. Item url:[{item.Url}]");
            int csdActionResult = (int)CSDActionResult.None;
            DateTime baseTime;
            RetentionSetting rs;
            var taxValue = (string)item[CSDFieldName.BCSColumn];
            var termId = new Guid(taxValue.Substring(taxValue.LastIndexOf('|') + 1));
            var hasReclassDateOfM2C = HasReclassDateFromModified2Creation(item);
            var csdRule = ConfigSiteSetting.CSDRules[termId];
            if (HasEventDate(item))
            {
                baseTime = new DateTime(Convert.ToDateTime(item.FieldValues[CSDFieldName.EventDate]).Ticks, DateTimeKind.Utc);
                rs = csdRule.EventRetentionSetting;
            }
            else
            {
                baseTime = hasReclassDateOfM2C
                    ? new DateTime(Convert.ToDateTime(item.FieldValues[CSDFieldName.ReclassDateOfModified2Creation]).Ticks, DateTimeKind.Utc)
                    : new DateTime(Convert.ToDateTime(item.FieldValues[CSDFieldName.Created]).Ticks, DateTimeKind.Utc);
                rs = csdRule.CreationRetentionSetting;
                //这种情况可能是后来修改了csd rule，我们认为当前的deletion date和retention label是对的，不需要检查。
                if (rs == null)
                {
                    logger.Info("No need to reset deletion date and retention label.");
                    return;
                }
            }
            var calculatedDate = csdRule.CalculateDeletionDate(baseTime, rs);
            DateTime curDeletionDate = DateTime.MinValue;
            if (item.FieldValues[CSDFieldName.DeletionDate] != null)
            {
                curDeletionDate = new DateTime(Convert.ToDateTime(item.FieldValues[CSDFieldName.DeletionDate]).Ticks, DateTimeKind.Utc);
            }
            logger.Info($"Current deletion Date:[{curDeletionDate.ToString("yyyy-MM-dd HH:mm:ss")}] Calculated deletion date:[{calculatedDate.ToString("yyyy-MM-dd HH:mm:ss")}]");
            //reset deletionDate
            if (!(curDeletionDate.AddMinutes(1) > calculatedDate && calculatedDate > curDeletionDate.AddMinutes(-1)))
            {
                logger.Info($"Reset detetionDate. ItemUrl:[{item.Url}]");
                item[CSDFieldName.DeletionDate] = calculatedDate;
                item.SystemUpdateForRecords();
                csdActionResult |= (int)CSDAction.UpdateDeletionDate;
            }

            if (!DataCenterUtil.Is21V())
            {
                bool useRetentionLabelForLockedDoc = HasLockedLabel(csdRule);
                var calculatedLabel = GetLabelForLock(item, csdRule);
                var curLabel = string.Empty;
                if (item[CSDFieldName.RetentionLabel] != null)
                {
                    curLabel = item[CSDFieldName.RetentionLabel].ToString();
                }
                logger.Info($"Current retention label:[{curLabel}] Calculated retention label:[{calculatedLabel}]");
                //reset label
                if (!curLabel.Equals(calculatedLabel, StringComparison.OrdinalIgnoreCase))
                {
                    logger.Info($"Reset retention label. ItemUrl:[{item.Url}]");
                    item.SetComplianceTag(calculatedLabel, useRetentionLabelForLockedDoc, false, false, false);
                    //item.SetComplianceTagOnBulkItems(calculatedLabel);
                    csdActionResult |= (int)CSDAction.UpdateRetentionLabel;
                }
            }
            switch ((CSDActionResult)csdActionResult)
            {
                case CSDActionResult.UpdatedDeletionDate:
                    jobDetailComment += "Successfully declared the document as a record and reset the document's deletion date.";
                    break;
                case CSDActionResult.UpdatedRetentionLabel:
                    jobDetailComment += "Successfully declared the document as a record and reset the document's retention label.";
                    break;
                case CSDActionResult.UpdatedBoth:
                    jobDetailComment += "Successfully declared the document as a record and reset the document's retention label and deletion date.";
                    break;
                case CSDActionResult.None:
                default:
                    jobDetailComment += "";
                    break;
            }
            logger.Info($"Finish checking and updating deletionDate and retentionLabel. Item url:[{item.Url}]");
        }

        private bool NeedSkip(IAveListItem item, ref string skipReason)
        {
            if (item[CSDFieldName.DeletionDate] == null || string.IsNullOrEmpty(item[CSDFieldName.DeletionDate].ToString()))
            {
                skipReason = "RM_JS_DAM_JobDetail_NoDeletionDate";
                return true;
            }
            if (!DataCenterUtil.Is21V())
            {
                if (item[CSDFieldName.RetentionLabel] == null || string.IsNullOrEmpty(item[CSDFieldName.RetentionLabel].ToString()))
                {
                    skipReason = "RM_JS_DAM_JobDetail_NoRetentionLabel";
                    return true;
                }
            }
            var taxValue = (string)item[CSDFieldName.BCSColumn];
            var termId = new Guid(taxValue.Substring(taxValue.LastIndexOf('|') + 1));
            if (!ConfigSiteSetting.CSDRules.ContainsKey(termId))
            {
                skipReason = "RM_JS_DAM_JobDetail_CSDClassInvalid";
                return true;
            }
            return false;
        }
        public void RunMultiThreadsProcessItem(IAveListItemCollection items, int itemsPerTask, CancellationTokenSource cts, string columnName, bool checkRule)
        {
            AveTenantTasks.RunParallel(items, itemsPerTask, cts, item =>
            {
                ProcessItem(item, columnName, checkRule);
            });
        }
        public void RunMultiThreadsProcessItem(List<IAveListItem> items, int itemsPerTask, CancellationTokenSource cts, string columnName)
        {
            AveTenantTasks.RunParallel(items, itemsPerTask, cts, item =>
            {
                ProcessItem(item, columnName);
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
                subTermIds = TermDao.GetAllSubTermUniqueIdsByTermId(anchordGuid);//
            }
            else
            {
                subTermIds = TermDao.GetAllSubTermUniqueIdsByTermSetId(taxonomyField.TermSetId);
            }

            return subTermIds;
        }
        protected IAveTaxonomyField GetTaxonomyField(IAveFieldCollection fields, string rmFieldTitle)
        {
            return fields.GetRecordTaxonomyField(rmFieldTitle, true);
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
                logger.Warn("Get single taxonomy field value failed! Item url: {0}, fieldName: {1}, error message: {2}.", item.Url, fieldInternalName, ex.ToString());
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
                SendReport(objectName, fullPath, action, ruleName, JobDetailsStatus.Failed, comment);
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
                logger.Info($"Init Design List error {e.ToString()}");
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
                    logger.Info($"SystemFolder list: [{list.Title}], list type: [{list.Type}]");
                    return true;
                }
                var listInfo = list.RootFolderUrl.Substring(list.RootFolderUrl.LastIndexOf('/') + 1) + list.ServerTemplate;
                if (CheckIsDesignList(listInfo))
                {
                    logger.Info("design list : {0}, listInfo: {1}", list.RootFolderUrl, listInfo);
                    return true;
                }
                if (list.Hidden.HasValue && list.Hidden.Value)
                {
                    logger.Info("hidden list : {0}, listInfo: {1}", list.RootFolderUrl, listInfo);
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
        public bool IsInExcludeNodeList(Guid id)
        {
            if (ExcludeNodes
                .Where(t => t.NodeLevel != NodeLevel.SiteCollection
                    && new Guid(t.NodeId).Equals(id)
                    && new Guid(t.SiteId).Equals(new Guid(CurrentSiteColTreeNode.ID)))
                .FirstOrDefault() != null)
            {
                logger.Info($"Node has unique disposal setting: {id}");
                return true;
            }
            else
            {
                return false;
            }
        }
        public void UndeclaredItem(IAveListItem item, List<AveComplianceTagInfo> LabelInfos, Rule resultRule)
        {
            string itemFullPath = WebUtil.MakeFullUrl(item.ParentList.ParentWeb.Url, item.Url);
            if (DataCenterUtil.Is21V())
            {
                if (!item.IsRecordOnly())
                {
                    var itemRetentionMessage = "RM_JS_DAM_JobDetail_ItemIsUndecalred";
                    SendReport(item.Name, itemFullPath, KeepDataOption.UndeclaredRecord.ToString(), resultRule.Name, JobDetailsStatus.Skipped, itemRetentionMessage);
                    return;
                }
                if (item.IsRecord())
                {
                    actionUtility.UndeclareItem(item);
                    var itemRetentionMessage = string.Format("The in-place record is undeclared.");
                    SendReport(item.Name, itemFullPath, KeepDataOption.UndeclaredRecord.ToString(), resultRule.Name, JobDetailsStatus.Successful, itemRetentionMessage);
                }
            }
            else
            {
                string labelName = item.GetComplianceTagName();
                if (!string.IsNullOrEmpty(labelName))
                {
                    var label = TagInfos.Where(t => t.TagName.Equals(labelName)).FirstOrDefault();
                    if (label == null)
                    {
                        throw new Exception(string.Format("Its applied label cannot be found. Label: {0}.", labelName));
                    }
                    if (label.IsEventTag)
                    {
                        logger.Info($"Event tag not supported {labelName}");
                        throw new Exception(string.Format("The retention settings of the applied label are invalid. Label: {0}.", labelName));
                    }
                    if (label.ReviewerEmail != null)
                    {
                        logger.Info($"Action need review {labelName}");
                        throw new Exception(string.Format("The retention settings of the applied label are invalid. Label: {0}.", labelName));
                    }
                    if (!item.IsRecordOnly())
                    {
                        var itemRetentionMessage = "RM_JS_DAM_JobDetail_ItemIsUndecalred";
                        SendReport(item.Name, itemFullPath, KeepDataOption.UndeclaredRecord.ToString(), resultRule.Name, JobDetailsStatus.Skipped, itemRetentionMessage);
                        return;
                    }

                    if (item.IsRecord())
                    {
                        actionUtility.UndeclareItem(item);
                        SetRetentionLabel(item);
                        var itemRetentionMessage = string.Format("The in-place record is undeclared. Label: {0}.", labelName);
                        SendReport(item.Name, itemFullPath, KeepDataOption.UndeclaredRecord.ToString(), resultRule.Name, JobDetailsStatus.Successful, itemRetentionMessage);
                    }

                    //Since we change the option of 'Retain Label' to be 'Forever' in bundle 14, we should not check AutoDelete any more.
                    //if (label.HasRetentionAction && label.AutoDelete)
                    //{
                    //    //validate the logic .
                    //    //var labelCalTime = DateTime.MinValue;
                    //    //if (ValidateItemCanDelete(item, label, ref labelCalTime))
                    //    //{
                    //    if (item.IsRecord())
                    //    {
                    //        actionUtility.UndeclareItem(item);
                    //        SetRetentionLabel(item);
                    //        var itemRetentionMessage = string.Format("The in-place record is undeclared. Label: {0}.", labelName);
                    //        SendReport(item.Name, itemFullPath, KeepDataOption.UndeclaredRecord.ToString(), resultRule.Name, JobDetailsStatus.Successful, itemRetentionMessage);
                    //    }

                    //    //}
                    //    //else
                    //    //{
                    //    //    var itemRetentionMessage = string.Format("Its deletion date does not meet the configured rule. Label: {0}.", labelName);
                    //    //    SendReport(item.Name, itemFullPath, KeepDataOption.UndeclaredRecord.ToString(), resultRule.Name, JobDetailsStatus.Skipped, itemRetentionMessage);
                    //    //}
                    //}
                    //else
                    //{
                    //    var itemRetentionMessage = string.Format("The retention settings of the applied label are invalid. Label: {0}.", labelName);
                    //    logger.Info($"[{itemFullPath}] Label setting Has Retention [{label.HasRetentionAction}] Auto Delete [{label.AutoDelete}] Skip the delete option due to invalid label setting");
                    //    SendReport(item.Name, itemFullPath, KeepDataOption.UndeclaredRecord.ToString(), resultRule.Name, JobDetailsStatus.Failed, itemRetentionMessage);
                    //    JobHasErrorNode = true;
                    //}
                }
                else
                {
                    logger.Info($"{itemFullPath} item have no lable applied");
                    //if (item.IsRecord())
                    //{
                    //    actionUtility.UndeclareItem(item);
                    //}
                    //item.Delete();
                    var itemRetentionMessage = "RM_JS_DAM_JobDetail_NoLabel";
                    SendReport(item.Name, itemFullPath, KeepDataOption.UndeclaredRecord.ToString(), resultRule.Name, JobDetailsStatus.Skipped, itemRetentionMessage);
                }
            }
        }
        /// <summary>
        /// Real delete item ,check label setting first.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="LabelInfos"></param>
        public void DeleteItem(IAveListItem item, List<AveComplianceTagInfo> LabelInfos, Rule resultRule)
        {
            string itemFullPath = WebUtil.MakeFullUrl(item.ParentList.ParentWeb.Url, item.Url);
            if (DataCenterUtil.Is21V())
            {
                if (item.IsRecord())
                {
                    actionUtility.UndeclareItem(item);
                }
                item.Delete();
                var itemRetentionMessage = $"item {itemFullPath} is removed.";
                SendReport(item.Name, itemFullPath, KeepDataOption.Delete.ToString(), resultRule.Name, JobDetailsStatus.Successful, itemRetentionMessage);
            }
            else
            {
                string labelName = item.GetComplianceTagName();
                if (!string.IsNullOrEmpty(labelName))
                {
                    var label = TagInfos.Where(t => t.TagName.Equals(labelName)).FirstOrDefault();
                    if (label == null)
                    {
                        throw new Exception($"Can't get label setting {labelName}");
                    }
                    if (label.IsEventTag)
                    {
                        logger.Info($"Event tag not supported {labelName}");
                        throw new Exception($"Event tag not supported {labelName}");
                    }
                    if (label.ReviewerEmail != null)
                    {
                        logger.Info($"Action need review {labelName}");
                        throw new Exception($"Action need review");
                    }
                    if (label.HasRetentionAction && label.AutoDelete)
                    {
                        //validate the logic .
                        //var labelCalTime = DateTime.MinValue;
                        //if (ValidateItemCanDelete(item, label, ref labelCalTime))
                        //{
                        if (item.IsRecord())
                        {
                            actionUtility.UndeclareItem(item);
                        }
                        //item.SetComplianceTag(null, false, false, false, false);
                        item.SetComplianceTagOnBulkItems(string.Empty);
                        item.Delete();
                        var itemRetentionMessage = $"item {itemFullPath} is removed label name {labelName} base on {label.TagRetentionBasedOn} duration {label.TagDuration} ";
                        SendReport(item.Name, itemFullPath, KeepDataOption.Delete.ToString(), resultRule.Name, JobDetailsStatus.Successful, itemRetentionMessage);
                        //}
                        //else
                        //{
                        //    var itemRetentionMessage = $"item {itemFullPath} is removed label name {labelName} time {labelCalTime} base on {label.TagRetentionBasedOn} duration {label.TagDuration} ";
                        //    SendReport(item.Name, itemFullPath, KeepDataOption.Delete.ToString(), resultRule.Name, JobDetailsStatus.Skipped, itemRetentionMessage);
                        //}
                    }
                    else
                    {
                        var itemRetentionMessage = $"item {itemFullPath} is removed label name {labelName} time label has retention {label.HasRetentionAction} AutoDelete {label.AutoDelete} base on {label.TagRetentionBasedOn} duration {label.TagDuration} ";
                        logger.Info($"{itemFullPath}Label setting Has Retention {label.HasRetentionAction} Auto Delete {label.AutoDelete} Skip the delete option due to invalid label setting");
                        SendReport(item.Name, itemFullPath, KeepDataOption.Delete.ToString(), resultRule.Name, JobDetailsStatus.Skipped, itemRetentionMessage);
                    }
                }
                else
                {
                    logger.Info($"{itemFullPath} item have no lable applied");
                    //if (DeclareSettingUtils.IsRecord(item))
                    //{
                    //    actionUtility.UndeclareItem(item);
                    //}
                    //item.Delete();
                    var itemRetentionMessage = "No labels applied";
                    SendReport(item.Name, itemFullPath, KeepDataOption.Delete.ToString(), resultRule.Name, JobDetailsStatus.Skipped, itemRetentionMessage);
                }
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
                        logger.Info($"Current label retention base on type not support {labelInfo.TagName} : {labelInfo.TagRetentionBasedOn}");
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
                logger.Info($"[{item.Url}] Calculte the label [{labelCalTime}] [{labelInfo.TagRetentionBasedOn}] : [{labelInfo.TagDuration}] == delay retention about 1day [{result}]");
            }
            catch (Exception e)
            {
                logger.Warn($"{item.Url }  Init label validation failed result {result}  {e.ToString()}");
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
    }

    #region no use
    //internal class MultiActionOnly
    //{
    //    private AveLogger mLog = null;
    //    private Queue nodeQueue = new Queue();
    //    private string nodeId = string.Empty;

    //    public MultiActionOnly()
    //    {
    //        ThreadPool.SetMaxThreads(50, 50);
    //    }

    //    public void DoAction(object obj, BaseActionProcessor cuprocessor)
    //    {
    //        if (nodeQueue.Count > 50)
    //        {
    //            BeginMultiActionOnlyAction();
    //        }
    //        nodeQueue.Enqueue(new MultiThreadItemEntity() { CurrentObj = obj, processor = cuprocessor });
    //    }

    //    public void BeginMultiActionOnlyAction()
    //    {
    //        Queue items = nodeQueue;
    //        if (items.Count > 0)
    //        {
    //            mLog.Debug($"Item count {items.Count}");
    //            List<MultiThreadItemEntity> CacheItems = new List<MultiThreadItemEntity>();
    //            int taskCount = items.Count;

    //            while (items.Count > 0)
    //            {
    //                MultiThreadItemEntity itemBackup = (items.Dequeue() as MultiThreadItemEntity);

    //                CacheItems.Add(itemBackup);
    //            }
    //            RunAndWaitTasks(CacheItems, new CancellationTokenSource(), m =>
    //            {
    //                if (m.CurrentObj is IAveListItem)
    //                {
    //                    var proObj = m.processor as BaseSPActionProcessor;
    //                    proObj.ProcessItem(m.CurrentObj as IAveListItem, m.BCSColumnName);
    //                }
    //            });

    //        }

    //    }
    //    public void RunAndWaitTasks<TSource>(IEnumerable<TSource> items, CancellationTokenSource cts, Action<TSource> action)
    //    {
    //        var taskCount = items.Count();
    //        mLog.Info($"Enter tag multi tasks, taskCount: {taskCount}");

    //        var tasks = new System.Threading.Tasks.Task[taskCount];
    //        for (var i = 0; i < taskCount; i++)
    //        {
    //            var k = i;
    //            tasks[i] = System.Threading.Tasks.Task.Factory.StartNew(() =>
    //            {
    //                try
    //                {
    //                    action(items.ElementAt(k));
    //                }
    //                catch (Exception e)
    //                {
    //                    cts.Cancel();
    //                    mLog.Error($"An error occurred while executing the task. error : {e.ToString()}");
    //                }
    //            },
    //            cts.Token);
    //        }
    //        try
    //        {
    //            System.Threading.Tasks.Task.WaitAll(tasks, taskCount * 1000 * 60 * 30, cts.Token);
    //        }
    //        catch (Exception e)
    //        {
    //            mLog.Warn($"An error occurred while wait all tasks to complete. error : {e.ToString()}");
    //        }

    //    }
    //}
    //internal class MultiThreadItemEntity
    //{
    //    public BaseActionProcessor processor { get; set; }
    //    public object CurrentObj { get; set; }
    //    public string BCSColumnName { get; set; }
    //}
    #endregion
}
