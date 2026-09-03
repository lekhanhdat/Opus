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

using AvePoint.GCommon;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.EnforceRetention;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.EnforceRetention.Cache
{
    public class RetentionDataCache : IDisposable
    {
        private RALogger logger = RALogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly static object locker = new object();
        private readonly static object termRetentionLocker = new object();

        static RetentionDataCache _instance;

        public Dictionary<Guid, TermSettingsInfo> TermRetentionMapping { get; private set; }

        public Dictionary<string, AveComplianceTagInfo> SPSiteRetentionLables { get; private set; }

        public string BCSColumnInternalName { get; set; }
        public Guid BCSColumnID { get; set; }

        public List<string> DesignLists { get; private set; }
        public LabelStateInfo LabelStateInfo { get; private set; } = new LabelStateInfo();

        public static RetentionDataCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (locker)
                    {
                        if (_instance == null)
                        {
                            _instance = new RetentionDataCache();
                            _instance.Init();
                        }
                    }
                }
                return _instance;
            }
        }
        private RMRetentionSourceType _sourceType = RMRetentionSourceType.SharePoint;
        public RetentionDataCache()
        {
            Init();
        }
        public RetentionDataCache(RMRetentionSourceType sourceType)
        {
            _sourceType = sourceType;
            Init();
        }
        private void Init()
        {
            CacheLabelSetting();
            SetDesignList();
        }

        private List<Guid> mProcessedItems = new List<Guid>();
        public bool GetProcessedItem(Guid itemId)
        {
            return mProcessedItems.Contains(itemId);
        }
        public void AddProcessedItem(Guid itemId)
        {
            lock (locker)
            {
                if (!mProcessedItems.Contains(itemId))
                {
                    mProcessedItems.Add(itemId);
                }
            }


        }

        public void CacheTermChange(long startTime)
        {
            logger.Info("cache term retention setting.");
            IRMChangeClassificationDao TermChangeDao = new RMChangeClassificationDao();
            ITermDao TermDao = new TermDao();
            var tIds = TermChangeDao.GetAllChange(startTime, (int)TermChangeType.Retention);
            if (tIds.Count > 0)
            {
                TermRetentionMapping = TermDao.GetRetetionTermDic(tIds);
            }
            else
            {
                TermRetentionMapping = new Dictionary<Guid, TermSettingsInfo>();
            }
            logger.Info($"cache term retention setting success, startTime:{startTime}, total count:{TermRetentionMapping.Count}.");

        }

        public void CacheSPLabelInfo(IAveSite aveSite)
        {
            var availableTags = aveSite.GetAvailableTagsForSite();
            string tagNames = string.Empty;
            availableTags.ForEach(tag => tagNames += "TagName:" + tag.TagName + ":displayName:" + tag.DisplayName);
            logger.Info($"all available tags in site {aveSite.Url} :{tagNames}");
            try
            {
                SPSiteRetentionLables = availableTags.ToDictionary(t => t.TagName, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception e)
            {
                logger.Info($"init label exception retry {e}");
                SPSiteRetentionLables = availableTags.ToDictionary(t => t.TagName);
            }
        }

        private void CacheLabelSetting()
        {
            logger.Info("cache label retention setting.");
            IRMEXOLabelDao RMLabelDao = new RMEXOLabelDao();
            var labels = RMLabelDao.GetLabelByType((int)_sourceType);
            RetentionLabel currentLabel = null;
            List<string> prevLabels = new List<string>();
            if (labels.Any(l => l.Status == (int)RMRetentionLabelStatus.Previous))
            {
                prevLabels = labels.Where(l => l.Status == (int)RMRetentionLabelStatus.Previous).Select(l => l.LabelName.ToLower()).Distinct().ToList();
            }
            // 优先取中间状态,Job正在处理的label
            if (labels.Any(l => l.Status == (int)RMRetentionLabelStatus.JobProcessing))
            {
                currentLabel = labels.Where(l => l.Status == (int)RMRetentionLabelStatus.JobProcessing).Select(l =>
                new RetentionLabel()
                {
                    ID = l.Id,
                    LabelId = l.LabelId,
                    Name = l.LabelName
                }).First();
            }
            // 没有Job正在处理的Label获取GUI设置的Label
            else if (labels.Any(l => l.Status == (int)RMRetentionLabelStatus.FromGUI))
            {
                currentLabel = labels.Where(l => l.Status == (int)RMRetentionLabelStatus.FromGUI).Select(l =>
                new RetentionLabel()
                {
                    ID = l.Id,
                    LabelId = l.LabelId,
                    Name = l.LabelName
                }).First();
            }
            
            LabelStateInfo = new LabelStateInfo()
            {
                PreviousLabelNames = prevLabels,
                CurrentLabel = currentLabel
            };
            logger.Info($"cache label retention setting, PreviousLable:{string.Join(",", prevLabels)}, ProcessLabel:{currentLabel?.Name}.");
        }

        private void SetDesignList()
        {
            logger.Info("begin to cache design list.");
            DesignLists = WebUtil.GetDesignLists();
            logger.Info("cache design list  success, total count:{0}.", DesignLists.Count);
        }

        public void AddTermRetentionObj(Guid termId, TermSettingsInfo info)
        {
            lock (termRetentionLocker)
            {
                if (!TermRetentionMapping.ContainsKey(termId))
                {
                    TermRetentionMapping.Add(termId, info);
                }
            }
        }

        public async System.Threading.Tasks.Task AddLabelHistoryAsync()
        {
            try
            {
                IRMEXOLabelDao RMLabelDao = new RMEXOLabelDao();
                var tempLabelName = this.LabelStateInfo.CurrentLabel.Name;
                var previousUsedLabels = RMLabelDao.GetLabelByStatusAndType((int)Contract.TaxonomyModel.RMRetentionLabelStatus.Previous, (int)_sourceType);

                if (previousUsedLabels != null && previousUsedLabels.Count > 0 && (previousUsedLabels.Where(l => l.LabelName.Equals(tempLabelName, StringComparison.OrdinalIgnoreCase)).ToList().Count > 0))
                {
                    //update saved time
                    var tempLabel = previousUsedLabels.Where(l => l.LabelName.Equals(tempLabelName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    tempLabel.SavedTime = DateTime.UtcNow.Ticks;
                    await RMLabelDao.UpdateAsync(tempLabel);
                }
                else
                {
                    //create new record
                    RMEXOLabel newLabel = new RMEXOLabel()
                    {
                        LabelName = tempLabelName,
                        Status = (int)Contract.TaxonomyModel.RMRetentionLabelStatus.Previous,
                        Type = (int)_sourceType,
                        SavedTime = DateTime.UtcNow.Ticks
                    };
                    RMLabelDao.Create(newLabel);
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while add label, error:{0}", e.ToString());
            }
        }

        public void ClearTermRetentionSetting()
        {
            if (TermRetentionMapping != null && TermRetentionMapping.Count > 0)
            {
                TermRetentionMapping.Clear();
            }
        }

        public void Dispose()
        {
            if (mProcessedItems != null && mProcessedItems.Count > 0)
            {
                mProcessedItems.Clear();
            }
            if (TermRetentionMapping != null && TermRetentionMapping.Count > 0)
            {
                TermRetentionMapping.Clear();
            }
            BCSColumnInternalName = string.Empty;
        }
    }
}
