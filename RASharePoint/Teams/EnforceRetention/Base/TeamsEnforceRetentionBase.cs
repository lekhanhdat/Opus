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
using System.Linq;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.RA.SharePoint.EnforceRetention;
using AvePoint.RA.SharePoint.EnforceRetention.Cache;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.SPObjDiscover;
using AvePoint.RA.SharePoint.Teams.EnforceRetention.Cache;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;

namespace AvePoint.RA.SharePoint.Teams.EnforceRetention.Base
{
    public class TeamsEnforceRetentionBase : RMEnforceRetentionBase
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(TeamsEnforceRetentionBase));
        public TeamsEnforceRetentionBase(AveDiscoverSite discoverSite, SPTreeNodeDto treeNode, JobContext jobContext) : base(discoverSite, treeNode, jobContext)
        {
        }

        public override void InnerProcessAveItem(IAveListItem aveItem, ref Guid recordId, ref string itemName, ref string itemUrl)
        {
            var siteId = aveItem.ParentList.ParentWeb.Site.ID;
            var nodeId = aveItem.UniqueId;
            recordId = IDGenerator.GetRecordId(siteId, nodeId);
            JobContext.ReportManager.Increase();
            itemName = aveItem?.GetObjectName();
            itemUrl = aveItem.FullPath();
            logger.Info($"Process item:siteId {siteId} Node Id{nodeId} rowId {aveItem.ID}");
            Guid termId;
            if (TeamsRetentionDataCache.Instance.GetProcessedItem(aveItem.UniqueId))
            {
                logger.Info($"Item already processed, item url:siteId {siteId} Node Id{nodeId} rowId {aveItem.ID}");
                return;
            }

            TeamsRetentionDataCache.Instance.AddProcessedItem(aveItem.UniqueId);
            using (CheckJobStopScope stopScope = new CheckJobStopScope())
            {
                var obj = aveItem.FieldValues.ContainsKey(TeamsRetentionDataCache.Instance.BCSColumnInternalName) ? aveItem.FieldValues[TeamsRetentionDataCache.Instance.BCSColumnInternalName] : null;
                if (obj != null)
                {
                    var columnVal = obj.ToString();
                    if (columnVal.Split('|').Length > 1)
                    {
                        var termIdStr = obj.ToString().Split('|')[1];
                        termId = Guid.Parse(termIdStr);
                        TermSettingsInfo termInfo = GetTermInfo(termId);
                        if (termInfo != null)
                        {
                            Guid tempRecordId = recordId;
                            WaitSPOExecuteAction(() =>
                            {
                                if ((termInfo.EnforceRetention & (int)EnforceRetentionType.Teams) == (int)EnforceRetentionType.Teams)
                                {
                                    ApplyComplianceTag(aveItem, tempRecordId);
                                }
                                else
                                {
                                    RemoveComplianceTag(aveItem, tempRecordId);
                                }
                            });

                        }
                    }
                    else
                    {
                        logger.Info($"invalid term format:{columnVal}, {itemUrl}");
                    }
                }
                else
                {
                    logger.Info($"item does not have bcs column,{TeamsRetentionDataCache.Instance.BCSColumnInternalName}, {itemUrl}");
                }
            }
        }

        protected override List<CAMLManager> GetCAMLManager()
        {
            var changedTermIds = TeamsRetentionDataCache.Instance.TermRetentionMapping.Keys.ToList();
            var removeLabelTermIds = TeamsRetentionDataCache.Instance.TermRetentionMapping.Where(t => (t.Value.EnforceRetention & (int)EnforceRetentionType.Teams) != (int)EnforceRetentionType.Teams).ToDictionary(t => t.Key, o => o.Value);
            if (removeLabelTermIds.Count > 0)
            {
                logger.Info("int CAML query include remove label action.");
                return CAMLManagerUtil.BuildCAMLMangager(DiscoverSite.SiteID, changedTermIds, TeamsRetentionDataCache.Instance.BCSColumnInternalName);
            }
            else
            {
                logger.Info("int CAML query for apply label.");
                return CAMLManagerUtil.BuildCAMLMangagerForRetention(DiscoverSite.SiteID, changedTermIds, TeamsRetentionDataCache.Instance.LabelStateInfo.CurrentLabel?.Name, TeamsRetentionDataCache.Instance.BCSColumnInternalName);
            }

        }

        protected override TermSettingsInfo GetTermInfo(Guid termId)
        {
            TermSettingsInfo result = null;

            if (!TeamsRetentionDataCache.Instance.TermRetentionMapping.TryGetValue(termId, out result))
            {
                var tempTerm = TermDao.GetParentInhertSetting(termId);
                if (tempTerm != null)
                {
                    result = new TermSettingsInfo() { EnforceRetention = tempTerm.EnforceRetention };
                    TeamsRetentionDataCache.Instance.AddTermRetentionObj(termId, result);
                }
                else
                {
                    logger.Warn($"item term not exist in db:{termId}");
                }
            }
            return result;
        }

        protected override bool HasBCSColumn(IAveList list)
        {
            bool result = true;
            try
            {
                if (!list.Fields.ContainsFieldWithInternalName(TeamsRetentionDataCache.Instance.BCSColumnInternalName))
                {
                    if (TeamsRetentionDataCache.Instance.BCSColumnInternalName != RcordsBuiltInColumn.ITEM_BCS_NAME)
                    {
                        //existing column reset internal name
                        var bcsColumn = list.Fields.GetFieldById(TeamsRetentionDataCache.Instance.BCSColumnID, false);
                        if (bcsColumn != null)
                        {
                            TeamsRetentionDataCache.Instance.BCSColumnInternalName = bcsColumn.InternalName;
                            logger.Info($"reset list bcs column, list:{list.RootFolder?.ServerRelativeUrl}, column name:{TeamsRetentionDataCache.Instance.BCSColumnInternalName}");
                        }
                        else
                        {
                            result = false;
                        }
                    }
                    else
                    {
                        result = false;
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error($"Get list bcs column error:{ex.ToString()}");
            }



            return result;
        }

        protected override void ApplyComplianceTag(IAveListItem item, Guid recordId)
        {
            using (var performance = new PerformanceScope("RMTeamsEnforceRetentionProcesser.ApplyLabel", addToStatistics: true))
            {
                var processingLabelName = TeamsRetentionDataCache.Instance.LabelStateInfo.CurrentLabel.Name;
                var previousLabelNames = TeamsRetentionDataCache.Instance.LabelStateInfo.PreviousLabelNames;
                AveComplianceTagInfo tagInfo = null;
                var itemUrl = item.FullPath();
                var currentLabel = item.GetComplianceTagName().ToLower();
                if (IsCurrentLabelLocked(item, itemUrl, currentLabel, true))
                {
                    return;
                }
                var needApplyLabel = string.IsNullOrEmpty(currentLabel) || previousLabelNames.Count > 0 && previousLabelNames.Contains(currentLabel) && !currentLabel.Equals(processingLabelName, StringComparison.OrdinalIgnoreCase);
                //only overwrite tag of retention setting label
                logger.Info($"ApplyComplianceTag:RowId {item.ID} processingLabelName:{processingLabelName}, currentLabel:{currentLabel}.");
                if (needApplyLabel)
                {
                    if (TeamsRetentionDataCache.Instance.SPSiteRetentionLables.TryGetValue(processingLabelName, out tagInfo))
                    {
                        using (var performance1 = new PerformanceScope("SP.RMEnforceRetentionProcesser.ApplyComplianceTag", addToStatistics: true))
                        {
                            //item.SetComplianceTag(tagInfo.TagName, tagInfo.BlockDelete, tagInfo.BlockEdit, tagInfo.IsEventTag, tagInfo.SuperLock);
                            item.SetComplianceTagOnBulkItems(tagInfo.TagName);
                        }

                        needUpdateLabelState = true;
                        logger.Info($"add item label:{processingLabelName}, Item RowId:{item.ID}");
                        JobContext.HasSuccessNode = true;
                        using (var performance2 = new PerformanceScope("RMEnforceRetentionProcesser.SendReport", addToStatistics: true))
                        {
                            JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                            {
                                ObjectName = item.GetObjectName(),
                                SourceURL = itemUrl,
                                Action = "RM_EXO_EnforceRetention_TagLabel",
                                Status = JobDetailsStatus.Successful,
                            });
                            JobContext.HasSuccessNode = true;
                        }
                    }
                    else
                    {
                        logger.Error($"SPLabel cannot be found:{processingLabelName}");
                        JobContext.HasErrorNode = true;
                        JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                        {
                            ObjectName = item.GetObjectName(),
                            SourceURL = itemUrl,
                            Status = JobDetailsStatus.Failed,
                            Action = "RM_EXO_EnforceRetention_TagLabel",
                            Comment = $"RM_JS_JM_EnforceRetention_LabelNotFound|I18NSplit|{processingLabelName}",
                        });
                        //throw new Exception($"Label cannot be found, label name:{processingLabelName}");
                    }
                }
                else
                {
                    logger.Info($"skip item:Row Id {item.ID}, compliance tag:{processingLabelName} already exist.");
                    if (!previousLabelNames.Contains(currentLabel))
                    {
                        JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                        {
                            ObjectName = item.GetObjectName(),
                            SourceURL = itemUrl,
                            Status = JobDetailsStatus.Skipped,
                            Action = "RM_EXO_EnforceRetention_TagLabel",
                            Comment = $"RM_JS_JM_EnforceRetention_LabelAlreadyExist|I18NSplit|{processingLabelName}",
                        });
                        JobContext.HasSuccessNode = true;
                    }
                }
            }
        }

        public override async Task ProcessListAsync(AveDiscoverList discoverList, Guid webId)
        {
            string listPath = string.Empty;
            try
            {
                using (var performance = new PerformanceScope("RMEnforceRetentionProcesser.ProcessList", $"RMEnforceRetentionProcesser.ProcessList Path:[{discoverList?.RootFolderUrl}]", true))
                {
                    logger.Info($"Process list:{discoverList?.RootFolderUrl}");
                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        JobContext.ReportManager.Increase();
                        ArgumentCheck.CheckNotNull(discoverList);
                        if (discoverList.ChangeType == Wrapper.Common.ChangeType.Delete)
                        {
                            logger.Info("skip removed list object {0}", discoverList?.ListId);
                            return;
                        }
                        if (discoverList.Name.Equals("{System Folder}"))
                        {
                            logger.Info("Skip the system list {0}", string.IsNullOrEmpty(discoverList?.RootFolderUrl) ? discoverList?.Name : discoverList?.RootFolderUrl);
                            return;
                        }
                        var list = discoverList.GetListObject();
                        if (CheckIsDesignList(discoverList))
                        {
                            logger.Info("Skip the design list {0}", string.IsNullOrEmpty(discoverList?.RootFolderUrl) ? discoverList?.Name : discoverList?.RootFolderUrl);
                            return;
                        }
                        if (!HasBCSColumn(list))
                        {
                            logger.Warn($"list does not have bcs column, list:{discoverList?.RootFolderUrl}, column name:{TeamsRetentionDataCache.Instance.BCSColumnInternalName}");
                            return;
                        }
                        listPath = WebUtil.MakeFullUrl(list.ParentWeb.Url, list.RootFolder.Url);
                        ProcessFailedItems(list);

                        switch (mDiscoverType)
                        {
                            case SPDiscoverType.Full:
                                await ProcessItemsForFullJobAsync(list);
                                break;
                            case SPDiscoverType.CAMLSearch:
                                ProcessItemsForSearchDiscover(list);
                                break;
                            case SPDiscoverType.Incremental:
                            default:
                                await ProcessItemsForIncrementalJobAsync(list, discoverList, webId);
                                break;
                        }
                    }
                }

            }
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (LabelNotExistException ex)
            {
                throw new LabelNotExistException(ex.Message);
            }
            catch (Exception e)
            {
                logger.Error($"error occurred while Process list:{discoverList?.RootFolderUrl}, ERROR:{e.ToString()}");
                JobContext.HasErrorNode = true;
                JobContext.NodeLevelError = true;
                JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                {
                    ObjectName = discoverList?.Title,
                    SourceURL = listPath,
                    Status = JobDetailsStatus.Failed,
                    Comment = GetExceptionMessage(e),
                });
            }

        }

        protected override async Task UpdateLabelStatusAsync()
        {
            var label = TeamsRetentionDataCache.Instance.LabelStateInfo.CurrentLabel;
            var dbLabel = LabelDao.GetLabel((int)RMRetentionSourceType.Teams, (int)RMRetentionLabelStatus.JobProcessing);
            LabelDao.RemoveOldFaildLabel((int)RMRetentionSourceType.Teams);
            if (dbLabel == null)
            {
                var tempLabel = new RMEXOLabel();
                tempLabel.LabelName = label.Name;
                tempLabel.Status = (int)RMRetentionLabelStatus.JobProcessing;
                tempLabel.Type = (int)RMRetentionSourceType.Teams;
                tempLabel.LabelId = label.LabelId;
                tempLabel.SavedTime = DateTime.UtcNow.Ticks;
                LabelDao.Create(tempLabel);
            }
            else
            {
                dbLabel.LabelName = label.Name;
                dbLabel.LabelId = label.LabelId;
                dbLabel.SavedTime = DateTime.UtcNow.Ticks;
                await LabelDao.UpdateAsync(dbLabel);
            }
        }

        protected override async Task CheckLabelExistAndThrowExceptionAsync()
        {
            var processingLabelName = TeamsRetentionDataCache.Instance.LabelStateInfo.CurrentLabel.Name;
            if (!TeamsRetentionDataCache.Instance.SPSiteRetentionLables.TryGetValue(processingLabelName, out AveComplianceTagInfo tagInfo))
            {
                logger.Warn($"label not exist:{processingLabelName}");
                await JobContext.MonitorExcetionAsync(Contract.Monitor.MonitorExceptionType.LabelNotFound);
                throw new LabelNotExistException($"The label cannot be found, label name: {processingLabelName}");
            }
        }

        protected override void RemoveComplianceTag(IAveListItem item, Guid recordId)
        {
            using (var performance = new PerformanceScope("RMEnforceRetentionProcesser.RemoveLabel", addToStatistics: true))
            {
                var processingLabelName = TeamsRetentionDataCache.Instance.LabelStateInfo.CurrentLabel.Name;
                var previousLabelNames = TeamsRetentionDataCache.Instance.LabelStateInfo.PreviousLabelNames;
                var itemUrl = item.FullPath();
                var currentLabel = item.GetComplianceTagName().ToLower();
                if (IsCurrentLabelLocked(item, itemUrl, currentLabel))
                {
                    return;
                }
                var needRemoveLabel = !string.IsNullOrEmpty(currentLabel) && previousLabelNames.Contains(currentLabel);
                logger.Info($"RemoveComplianceTag:RowId {item.ID} processingLabelName:{processingLabelName}, currentLabel:{currentLabel}.");
                //only remove tag of retention setting label
                if (needRemoveLabel)
                {
                    using (var performance1 = new PerformanceScope("RMEnforceRetentionProcesser.RemoveComplianceTag", addToStatistics: true))
                    {
                        //item.SetComplianceTag(null, false, false, false, false);
                        item.SetComplianceTagOnBulkItems(string.Empty);
                    }
                    logger.Info($"remove item label:{currentLabel}, ItemRowId:{item.ID}");
                    needUpdateLabelState = true;
                    JobContext.HasSuccessNode = true;
                    using (var performance2 = new PerformanceScope("RMEnforceRetentionProcesser.SendReport", addToStatistics: true))
                    {
                        JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                        {
                            ObjectName = item.GetObjectName(),
                            SourceURL = itemUrl,
                            Action = "RM_EXO_EnforceRetention_RemoveLabel",
                            Status = JobDetailsStatus.Successful,
                        });
                        JobContext.HasSuccessNode = true;
                    }
                }
                else
                {
                    logger.Info($"skip item:RowId {item.ID}, compliance tag:current:{currentLabel}.");
                    if (!previousLabelNames.Contains(currentLabel))
                    {
                        JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                        {
                            ObjectName = item.GetObjectName(),
                            SourceURL = itemUrl,
                            Action = "RM_EXO_EnforceRetention_RemoveLabel",
                            Status = JobDetailsStatus.Skipped,
                            Comment = $"RM_JS_JM_EnforceRetention_LabelNoNeedRemove|I18NSplit|{currentLabel}"
                        });
                        JobContext.HasSuccessNode = true;
                    }
                }
            }
        }
    }
}
