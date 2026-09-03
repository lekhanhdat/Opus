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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.EnforceRetention.Cache;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.EnforceRetention
{
    public class SPOLabelUtility
    {
        protected AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(SPOLabelUtility));

        private IRMEXOLabelDao _labelDao;
        public IRMEXOLabelDao LabelDao
        {
            get { return _labelDao ?? (IRMEXOLabelDao)PlatformWindsorManager.GetService(typeof(IRMEXOLabelDao)); }
            set { _labelDao = value; }
        }
        private ITermDao mTermDao;
        public ITermDao TermDao
        {
            get
            {
                if (mTermDao == null)
                {
                    mTermDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
                }
                return mTermDao;
            }
        }

        private IRMChangeClassificationDao mChangeClassificationDao;
        public IRMChangeClassificationDao ChangeClassificationDao
        {
            get
            {
                if (mChangeClassificationDao == null)
                {
                    mChangeClassificationDao = (IRMChangeClassificationDao)PlatformWindsorManager.GetService(typeof(IRMChangeClassificationDao));
                }
                return mChangeClassificationDao;
            }
        }

        protected RetentionDataCache mRetentionCache = null;
        protected bool mAddToStatistics = false;
        private List<Guid> mAddedTermIds = new List<Guid>();
        private readonly object mChangedTermObj = new object();
        public bool LabelApplied = false;
        public SPOLabelUtility(bool addToStatistics = false)
        {
            mAddToStatistics = addToStatistics;
            Init();
        }

        protected virtual void Init()
        {
            mRetentionCache = new RetentionDataCache();
            mRetentionCache.CacheTermChange(DateTime.UtcNow.Ticks);
        }

        public void CacheSPLabel(IAveSite site)
        {
            mRetentionCache.CacheSPLabelInfo(site);
        }

        #region apply label
        public virtual bool UpdateLabel(IAveListItem aveItem, Guid termId, Guid recordId, Guid previousTermId)
        {
            if (aveItem.FileSystemObjectType == AveFileSystemObjectType.Folder)
            {
                logger.Info($"Skip folder. Path:[{aveItem.FullPath()}]");
                return false;
            }
            bool labelNotExist = false;
            if (termId != Guid.Empty)
            {
                //term id改变时才操作label
                try
                {
                    TermSettingsInfo termInfo = GetTermInfo(termId);
                    if (termInfo != null)
                    {
                        if ((termInfo.EnforceRetention & (int)EnforceRetentionType.SharePoint) == (int)EnforceRetentionType.SharePoint)
                        {
                            labelNotExist = ApplyComplianceTag(aveItem, recordId, termInfo, termId, previousTermId);
                        }
                        else
                        {
                            if (previousTermId != termId)
                            {
                                RemoveComplianceTag(aveItem);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while updating retention label. Item url:{0} Error:{1}", aveItem.FullPath(), e.ToString());
                }
            }
            else
            {
                //term id改变时才操作label
                if (previousTermId != Guid.Empty)
                {
                    RemoveComplianceTag(aveItem);
                }
            }
            return labelNotExist;
        }

        protected bool ApplyComplianceTag(IAveListItem item, Guid recordId, TermSettingsInfo termInfo, Guid termId, Guid previousTermId)
        {
            bool labelNotExist = false;
            using (var performance = new PerformanceScope("RMExplorerUtility.ApplyLabel", addToStatistics: mAddToStatistics))
            {
                var processingLabelName = mRetentionCache.LabelStateInfo.CurrentLabel.Name;
                AveComplianceTagInfo tagInfo = null;
                var itemUrl = item.FullPath();
                var currentLabel = item.GetComplianceTagName();

                //bool needApplyLabel = (!string.IsNullOrEmpty(previousLabelName) && currentLabel == previousLabelName && currentLabel != processingLabelName);


                logger.Info($"ApplyComplianceTag:RowId {item.ID} .currentLabel:{currentLabel}. processing lable:{processingLabelName}");
                if (NeedApplyLabel(item, termId, previousTermId))
                {
                    if (mRetentionCache.SPSiteRetentionLables.TryGetValue(processingLabelName, out tagInfo))
                    {
                        using (var performance1 = new PerformanceScope("RMExplorerUtility.ApplyComplianceTag", addToStatistics: mAddToStatistics))
                        {
                            //item.SetComplianceTag(tagInfo.TagName, tagInfo.BlockDelete, tagInfo.BlockEdit, tagInfo.IsEventTag, tagInfo.SuperLock);
                            item.SetComplianceTagOnBulkItems(tagInfo.TagName);
                        }
                        logger.Info($"add item label:{processingLabelName}, Item RowId:{item.ID}");
                        LabelApplied = true;
                    }
                    else
                    {
                        labelNotExist = true;
                        logger.Error($"SPLabel cannot be found:{processingLabelName}");
                        UpdateLabelRelatedTermChangeTime(termId);
                        //AddFaildLabel(recordId);
                        //throw new Exception($"Label cannot be found, label name:{processingLabelName}");
                    }
                }
                else
                {
                    logger.Info($"skip item:Row Id {item.ID}, compliance tag:{processingLabelName} already exist.");
                }
            }
            return labelNotExist;
        }

        private void UpdateLabelRelatedTermChangeTime(Guid termId)
        {
            if (!mAddedTermIds.Contains(termId))
            {
                ChangeClassificationDao.AddChange(new List<Guid> { termId }, (int)TermChangeType.Retention);
                lock (mChangedTermObj)
                {
                    if (!mAddedTermIds.Contains(termId))
                    {
                        mAddedTermIds.Add(termId);
                    }
                }
            }
        }

        //以下下情况会给数据打Label
        //1.数据在cosmos db中没有记录，并且数据没有Label
        //2.数据在cosmos db中有记录，但是db中的term id和当前term id不一致
        private bool NeedApplyLabel(IAveListItem item, Guid termId, Guid previousTermId)
        {
            bool applyLabel = false;
            var processingLabelName = mRetentionCache.LabelStateInfo.CurrentLabel.Name;
            var previousLabelNames = mRetentionCache.LabelStateInfo.PreviousLabelNames;
            var currentLabel = item.GetComplianceTagName().ToLower();

            if (previousTermId != termId && (!item.ExistComplianceTag()
               || (previousLabelNames.Count > 0 && previousLabelNames.Contains(currentLabel) && !currentLabel.Equals(processingLabelName, StringComparison.OrdinalIgnoreCase))))
            {
                applyLabel = true;
            }

            return applyLabel;
        }

        protected void RemoveComplianceTag(IAveListItem item)
        {
            using (var performance = new PerformanceScope("RMExplorerUtility.RemoveLabel", addToStatistics: mAddToStatistics))
            {
                try
                {
                    if (item.ExistComplianceTag())
                    {
                        var previousLabelNames = mRetentionCache.LabelStateInfo.PreviousLabelNames;
                        var currentLabel = item.GetComplianceTagName().ToLower();
                        var itemUrl = item.FullPath();
                        var needRemoveLabel = previousLabelNames.Contains(currentLabel);
                        logger.Info($"RemoveComplianceTag:RowId {item.ID}.currentLabel:{currentLabel}.");
                        //only remove tag of retention setting label
                        if (needRemoveLabel)
                        {
                            using (var performance1 = new PerformanceScope("RMExplorerUtility.RemoveComplianceTag", addToStatistics: mAddToStatistics))
                            {
                                //item.SetComplianceTag(null, false, false, false, false);
                                item.SetComplianceTagOnBulkItems(string.Empty);
                            }
                            logger.Info($"remove item label:{currentLabel}, ItemRowId:{item.ID}");
                        }
                        else
                        {
                            logger.Info($"skip item:RowId {item.ID}, compliance tag:current:{currentLabel}.");
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Info($"An error occurred while removing label:RowId {item.ID}.error:{e.ToString()}.");
                }
            }
        }


        protected virtual TermSettingsInfo GetTermInfo(Guid termId)
        {
            TermSettingsInfo result = null;

            if (!mRetentionCache.TermRetentionMapping.TryGetValue(termId, out result))
            {
                var tempTerm = TermDao.GetParentInhertSetting(termId);
                if (tempTerm != null)
                {
                    result = new TermSettingsInfo() { EnforceRetention = tempTerm.EnforceRetention, SPRetentionLabel = tempTerm.SPRetentionLabel };
                    mRetentionCache.AddTermRetentionObj(termId, result);
                }
                else
                {
                    logger.Warn($"item term not exist in db:{termId}");
                    //throw new Exception($"term cannot be found, termId:{termId}");
                }
            }
            return result;
        }

        public async System.Threading.Tasks.Task AddLabelHistoryAsync()
        {
            if (mRetentionCache != null)
            {
                await mRetentionCache.AddLabelHistoryAsync();
            }
        }

        #endregion

    }
}
