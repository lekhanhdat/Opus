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


#region using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using System.Configuration;
using System.Threading;
using System.IO;
using AvePoint.GCommon.FileTransfer;
using AvePoint.Wrapper.Common.Common.Utility;
using AvePoint.GCommon.Utility.I18N;

namespace AvePoint.Item.Restore
{
    #endregion

    class AveItemMultiThreadRestore : AveItemRestore
    {
        private const int maxCacheCount = 20;
        private BlockingQueue<object> cacheQueue;
        CacheQueueProducer cacheQueueProducer;
        CacheQueueConsumer cacheQueueConsumer;

        public AveItemMultiThreadRestore()
        {
            this.cacheQueue = new BlockingQueue<object>(maxCacheCount);
        }

        public override void Init()
        {
            base.Init();
            this.cacheQueueConsumer = new CacheQueueConsumer(this.cacheQueue, Convert.ToInt32("3"), new RestoreItemMethod(base.RestoreItem));//ConfigurationManager.AppSettings["restoreThreadCount"]:3
            SetInitRestoreStreamMethod();
            this.cacheQueueProducer = new CacheQueueProducer(this.cacheQueueConsumer, this.cacheQueue, FileReceiver);
            this.cacheQueueConsumer.StartRestoreFromCache();
        }

        private void SetInitRestoreStreamMethod()
        {
            if (Config.EventCategory == EventCategorys.DocAveAgentService.StorageOptimization_SP2010_Archiver_Restore)
            {
                cacheQueueConsumer.InitRestoreStream = GetRestoreStreamV1;
            }
            else
            {
                cacheQueueConsumer.InitRestoreStream = GetRestoreStreamV2;
            }
        }

        private IAveRestoreStream GetRestoreStreamV1(IInputStreamWrapper stream)
        {
            return new WrapperRestoreStreamV1(stream);
        }
        private IAveRestoreStream GetRestoreStreamV2(IInputStreamWrapper stream)
        {
            return new WrapperRestoreStreamV2(stream);
        }

        protected override void IsItemHasDepedenciesList()
        {
            bool isItemHasDependenciesList = false;
            if (AveList != null && AveList.ListInfo != null
                && (AveList.ListInfo.BaseTemplate == (int)AveListTemplateType.DiscussionBoard
                || AveList.ListInfo.BaseTemplate == (int)AveListTemplateType.TasksWithTimelineAndHierarchy))  //discussion list ,(AOSBR-1667) Task list,有sub item，多线程无法保持源端结构
            {
                isItemHasDependenciesList = true;
            }
            cacheQueueConsumer.SwitchThreadMode((isItemHasDependenciesList || !WrapperConfiguration.WrapperConfigurationForBPOS.IsMultiThreadRestore) ? ThreadMode.SingleThread : ThreadMode.MultiThread);
        }

        protected override void WaitForItems(bool isEndOfJob)
        {
            cacheQueueProducer.PutNonItemLevelSignalToCacheQueue(isEndOfJob);
        }

        public override void RestoreItem(RestoreContentDto aveItemDto)
        {
            try
            {
                cacheQueueProducer.ProduceCacheObj(aveItemDto);
            }
            catch (InvalidDataException e)
            {
                AveRestoreReportDto reportDto = new AveRestoreReportDto();
                reportDto.Type = aveItemDto.Type.ToString();
                reportDto.Title = aveItemDto.Name;
                reportDto.SourcePath = aveItemDto.SrcUrl;
                reportDto.Status = RestoreStatus.Failed;
                reportDto.PathMD5 = aveItemDto.ItemPathMd5;
                reportDto.ErrorMessage = "RM_JM_RestoreFailed_DataLostError";
                this.AddReport(reportDto);
            }
        }

        protected override void AddReport(AveRestoreReportDto reportDto)
        {
            lock (Report)
            {
                base.AddReport(reportDto);
            }
        }

        public override void Dispose()
        {
            if (this.cacheQueueConsumer != null)
            {
                this.cacheQueueConsumer.WaitForAll();
            }
            if (this.cacheQueue != null)
            {
                this.cacheQueue.Close();
            }
            if (this.cacheQueueProducer != null)
            {
                this.cacheQueueProducer.Close();
            }
            if (this.cacheQueueConsumer != null)
            {
                this.cacheQueueConsumer.Close();
            }
            base.Dispose();
        }
    }
}
